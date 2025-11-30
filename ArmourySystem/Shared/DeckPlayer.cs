using System.Collections.Generic;
using UnityEngine;

public class DeckPlayer : MonoBehaviour 
{
    [Header("Map Context")]
    [SerializeField] HexMapPainter painter;

    [Header("Combat Context")]
    [SerializeField] CombatSceneManager CombatSceneManager;
    List<ActorBase> allies = new();
    List<ActorBase> foes = new();
    public static ActorBase CurrentTarget;


    [Header("Deck Info")]
    [SerializeField] CardDomain domain;
    [SerializeField] MapDeckConfigSO initMapDeck;
    [SerializeField] CombatDeckConfigSO initCombatDeck;
    DeckModel<MapCardSO> mapDeck;
    DeckModel<CombatCardSO> combatDeck;
    Stack<IPlayCommand> commands = new();

    public bool CanRewind => commands.Count > 0;

    [Header("Card View Setting")]
    [SerializeField] HandLayoutManager handLayoutManager;

    [SerializeField, Tooltip("需與 PoolManager 所使用的 CardView Prefab 相同")] 
    CardView cardViewPrefab;
    [SerializeField] int drawPerRound = 3;
    List<CardView> cardViews = new();
    
    [Header("Reference")]
    public APTracker apTracker;
    public Transform nodeParent;
    public NodeManager nodeManager;
    public SkillRunner skillRunner;
    [SerializeField] MapSceneManager mapSceneManager;
    
    

    bool isDebugMode = false;

    void Start()
    {
        if (domain == CardDomain.Map)
        {
            InitializeMapDeck();
            PrintDeck(mapDeck);
            drawPerRound = mapSceneManager.GetHandLimit();
        }
        else InitializeCombatDeck();

        SetUpAPTracker();

        NextRound();
    }

    void SetUpAPTracker()
    {
        int apMax;
        int apRegen;
            
        // fetch from rsc manager later
        if(domain == CardDomain.Map)
        {
            apMax = 1;
            apRegen = 1;
        } else
        {
            apMax = 3;
            apRegen = 2;
        }

        apTracker.Init(apRegen,apMax);
        apTracker.Replenish();
    }

    void OnEnable()
    {
        CombatSceneManager.OnAlliesSet += UpdateAllies;
        CombatSceneManager.OnFoesSet += UpdateFoes;
    }

    void OnDisable()
    {
        CombatSceneManager.OnAlliesSet -= UpdateAllies;
        CombatSceneManager.OnFoesSet -= UpdateFoes;
    }

    void InitializeMapDeck()
    {   
        if(ResourceManager.Instance == null) 
        {
            Debug.LogWarning("[Deck Player] ResourceManager can't reach");
        }else if(ResourceManager.Instance.MapDeck == null)
        {
            Debug.LogWarning("[Deck Player] MapDeck is null, use default");
        }else initMapDeck.Pool = ResourceManager.Instance.MapDeck.ToArray();

        mapDeck = new(initMapDeck.Pool, 42);
    }

    void InitializeCombatDeck()
    {
        if(ResourceManager.Instance == null) 
        {
            Debug.LogWarning("[Deck Player] ResourceManager can't reach");
        }else if(ResourceManager.Instance.combatDeck == null)
        {
            Debug.LogWarning("[Deck Player] CombatDeck is null, use default");
        }else initCombatDeck.Pool = ResourceManager.Instance.combatDeck.ToArray();
        
        combatDeck = new(initCombatDeck.Pool, 42);
    }

    void PrintDeck(DeckModel<MapCardSO> model)
    {
        var deck = model.GetDeck();
        if (isDebugMode) Debug.Log($"Printing {domain} Card:");

        foreach(var card in deck) if (isDebugMode) Debug.Log($"{card.title}");
    }

    MapContext GetMapContext(Vector3 position)
    {
        return new MapContext(position, painter);
    }

    CombatContext GetCombatContext()
    {
        return new CombatContext(allies[0], allies, foes, CurrentTarget);
    }

    void UpdateAllies(List<ActorBase> actors)
    {
        
        allies.AddRange(actors);
    }

    void UpdateFoes(List<ActorBase> actors)
    {
        foes.AddRange(actors);
    }

    public bool CanPlay(CardSO card)
    {
        if (domain == CardDomain.Map)
        {
            if (mapSceneManager.CanBindTile) return apTracker.GetAP >= card.cost;
            else return false;
        }
        
        return apTracker.GetAP >= card.cost;
    }

    public void PlayCard(CardView cardView, Vector3 position)
    {
        if(domain == CardDomain.Combat && CurrentTarget == null) return;
        
        if (isDebugMode) Debug.Log("[DeckPlayer] Issuing Command");
        IPlayCommand newCommand;

        if(domain == CardDomain.Map && cardView.card is MapCardSO mapCard)
        {
            MapContext ctx = GetMapContext(position);
            newCommand = new PlayMapCardCommand(mapCard, ctx, this);
            if(!newCommand.TryPlay())
            {
                if (isDebugMode) Debug.Log("[DeckPlayer] Command cancel, aborting command");
                newCommand.Abort();
                return;
            }
            
            apTracker.SubAP(mapCard.cost);
            mapSceneManager.BindTile();

            commands.Push(newCommand);

            mapDeck.Discard(mapCard);
            RemoveCardView(cardView);

            if (isDebugMode) Debug.Log($"[DeckPlayer] Command Complete, commands stack has {commands.Count} commands");
        } else if(cardView.card is CombatCardSO combatCard)
        {
            CombatContext ctx = GetCombatContext();
            newCommand = new PlayCombatCardCommand(combatCard, ctx, this);
            if(!newCommand.TryPlay())
            {
                if (isDebugMode) Debug.Log("[DeckPlayer] Command cancel, aborting command");
                newCommand.Abort();
                return;
            }

            apTracker.SubAP(combatCard.cost);
            
            commands.Push(newCommand);

            combatDeck.Discard(combatCard);
            RemoveCardView(cardView);

            if (isDebugMode) Debug.Log($"[DeckPlayer] Command Complete, commands stack has {commands.Count} commands");
        }
    }

    //限同一回合
    public void Rewind()
    {
        if(!CanRewind) return;
        var commandToAbort = commands.Pop();
        var card = commandToAbort.Abort();
        
        apTracker.AddAP(card.cost);

        if(domain == CardDomain.Map) mapDeck.ReturnCardToHand((MapCardSO) card);
        else combatDeck.ReturnCardToHand((CombatCardSO) card);

        AddCardView(card);
        if (isDebugMode) Debug.Log($"[DeckPlayer] Command abort, commands stack has {commands.Count} commands");
    }

    //抽卡（若牌堆中沒牌就洗牌再抽）同時清空指令
    public void NextRound()
    {
        commands.Clear();
        apTracker.Replenish();

        DiscardHand();

        List<CardSO> newCards = new();

        if(domain == CardDomain.Map) newCards.AddRange(mapDeck.Draw(drawPerRound));
        else newCards.AddRange(combatDeck.Draw(drawPerRound));

        foreach(var card in newCards)
        {
            if (card == null) continue;
            AddCardView(card);
        }

        if(CombatSceneManager) CombatSceneManager.nextRound = true;
    }

    void DiscardHand()
    {
        if (domain == CardDomain.Map) mapDeck.DiscardAllHand();
        else combatDeck.DiscardAllHand();

        List<CardView> cardViewSnapShot = new(cardViews);
        foreach (var cardView in cardViewSnapShot)
        {   
            RemoveCardView(cardView);
        }
    }

    void AddCardView(CardSO card)
    {
        var newCardView = PoolSystem.Instance.GetInstance<CardView>(cardViewPrefab);
        newCardView.SetCard(card, this);
        newCardView.gameObject.SetActive(true);
        cardViews.Add(newCardView);

        handLayoutManager.RegisterCard(newCardView);
    }
    void RemoveCardView(CardView cardView)
    {
        if(cardView == null) 
        {
            Debug.LogError("[DeckPlayer] Attempting to remove null cardView");
            return;
        }
        cardViews.Remove(cardView);
        cardView.gameObject.SetActive(false);

        handLayoutManager.UnregisterCard(cardView);
    }
}