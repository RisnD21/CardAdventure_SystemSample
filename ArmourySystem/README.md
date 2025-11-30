# Armoury System

**Equip gear → Build a combat deck → Play skills in battle**

---

## 1. What Problem This Solves

Games with deck-based or skill-based combat need a way to let players **equip gear** and have those choices **directly affect combat**.

This system provides a modular solution that:

* Converts **equipped GearSO** into a **combat deck**
* Lets each card execute its associated **SkillSO**
* Separates UI from gameplay logic
* Supports scalable, data-driven content

---

## 2. System Architecture Overview

```
ArmouryManager
 ├─ Tracks owned gear
 ├─ Equipped gear by slot
 └─ Builds Combat Deck (List<CombatCardSO>)
        ↓
ResourceManager
 └─ Stores combat deck for combat scene
        ↓
DeckPlayer
 ├─ InitDeck()
 ├─ Draw / Play cards
 └─ Sends SkillSO to CombatSystem
        ↓
DeckModel
 ├─ Draw pile
 ├─ Discard pile
 └─ Reshuffle logic
        ↓
CombatSystem
 └─ Executes SkillSO
```

**Core idea:**
**Gear defines the deck → Deck defines available actions → CombatSystem executes skills.**

---

## 3. Flow: Gear → Deck → Combat

### **1) Player equips GearSO**

Each gear item defines a list of `CombatCardSO` entries.
When the loadout changes, the system updates.

### **2) ArmouryManager assembles the Combat Deck**

All cards from equipped gear are combined into a flat list.

### **3) Deck is stored in ResourceManager**

Persistent across scenes.

### **4) DeckPlayer initializes the deck**

Creates a `DeckModel` that manages drawing, discarding, and reshuffling.

### **5) CombatCardSO triggers SkillSO**

Every card references a SkillSO, which is executed during combat.

---

## 4. Folder Structure

```plaintext
ArmourySystem/
 ├─ Core/
 │   └─ ArmouryManager.cs            # Loadout management + deck assembly
 │
 ├─ Shared/
 │   ├─ DeckModel.cs                 # Draw/discard/reshuffle logic
 │   ├─ DeckPlayer.cs                # In-combat deck handler
 │   └─ SkillRunner.cs               # Executes SkillSO
 │
 ├─ SO/
 │   ├─ GearSO.cs                    # Gear defining CombatCard entries
 │   ├─ CombatCardSO.cs              # Playable card
 │   ├─ CardSO.cs                    # Base card data
 │   ├─ SkillSO.cs                   # Combat action
 │   └─ CombatDeck.cs                # Optional deck template
 │
 └─ UI/
     ├─ ArmouryView.cs               # Loadout UI
     ├─ GearSlotView.cs              # Click-to-equip slot
     └─ SelectionPanelView.cs        # Gear selection panel
```

---

## 5. Key Classes

### **ArmouryManager.cs**

Handles:

* owned gear
* equipped gear (Dictionary by slot)
* assembling the combat deck

Deck creation example:

```csharp
public void UpdateCombatDeck()
{
    List<CombatCardSO> combatDeck = new();

    foreach (var gear in currentGears)
    {
        if (gear == null) continue;

        foreach (var entry in gear.normalCards)
            for (int i = 0; i < entry.Count; i++)
                combatDeck.Add(entry.Card);
    }

    ResourceManager.Instance.combatDeck = combatDeck;
}
```

---

### **DeckModel.cs**

A pure model that handles:

* draw pile
* discard pile
* reshuffle
* maintaining card state

No UI logic — clean and testable.

---

### **DeckPlayer.cs**

Provides runtime functionality:

* initializes the deck
* draws cards
* validates and plays cards
* sends SkillSO to the CombatSystem

---

### **SkillRunner.cs**

Responsible for executing the logic defined in a **SkillSO**.

---

### **ScriptableObjects**

#### **GearSO**

* Item metadata
* Equipped slot
* List of combat card entries

#### **CombatCardSO**

* Play cost
* Card visuals
* Targeting rules
* Reference to `SkillSO`

#### **SkillSO**

* Damage / heal / buff / debuff / AoE
* Target validation
* Execution logic

---

## 6. Future Extensions

### **Customizable Gear**

* Sockets & upgrade gems
* Stat-based affixes
* Randomized modifiers

### **Active Abilities**

Support gear-granted **skills with cooldowns**, independent of cards.

### **Passive Abilities**

Integrate a buff/debuff system to support:

* passive stat bonuses
* conditional triggers
* aura effects
