using System.Collections.Generic;

public class DeckModel<T> {
    readonly System.Random rng;
    readonly List<T> draw = new();
    readonly List<T> discard = new();
    readonly List<T> hand = new();
    readonly List<T> deck = new();
    public IReadOnlyList<T> Hand => hand;

    public List<T> GetDeck() => deck;

    public DeckModel(IEnumerable<T> start, int seed) {
        draw.AddRange(start);
        deck.AddRange(start);
        Shuffle(draw, seed);
        rng = new System.Random(seed);
    }

    public void ReturnCardToHand(T card)
    {
        hand.Add(card);
        discard.Remove(card);
    }

    public List<T> Draw(int n) {
        List<T> draws = new();
        for (int i = 0; i < n; i++) {
            if (draw.Count == 0) Reshuffle();
            if (draw.Count == 0) break;
            var top = draw[^1];
            draw.RemoveAt(draw.Count - 1);
            draws.Add(top);
            hand.Add(top);
        }
        return draws;
    }

    public void DiscardAllHand() {
        discard.AddRange(hand);
        hand.Clear();
    }

    public void Discard(T card) {
        hand.Remove(card);
        discard.Add(card);
    }

    void Reshuffle() {
        if (discard.Count == 0) return;
        draw.AddRange(discard);
        discard.Clear();
        Shuffle(draw, rng.Next());
    }

    static void Shuffle(List<T> list, int seed) {
        var r = new System.Random(seed);
        for (int i = list.Count - 1; i > 0; i--) {
            int j = r.Next(i + 1);
            (list[i], list[j]) = (list[j], list[i]);
        }
    }
}