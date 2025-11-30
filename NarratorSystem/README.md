# Dialogue & Narrative System

**Scene-based rule evaluation → Auto-select narrative → Run conversation → Trigger in-scene actions**

---

## 1. What Problem This Solves

Many story-driven games require a **flexible, extensible, data-driven narrative system**, usually involving:

* When a player enters a scene, the system should **automatically decide** whether to play a narrative sequence.
* Dialogues must support **portraits, background changes, options, audio, and typewriter text**.
* Narrative sequences should trigger **in-scene actions** (give item, remove item, play animation, etc.).
* Once a narrative is played, it should not repeat unless explicitly configured.
* UI must remain thin—no embedded game logic.
* ScriptableObjects should hold **data, not logic**.

This system offers a clean, modular solution:

* **Narrator** manages narrative entry conditions and playback flow
* **RuleDB** determines which NarrativeRule to apply based on SceneID
* **DialogRunner** handles UI, pieces, portraits, options, and transitions
* **ScriptableEvent** provides an event bus for triggering scene actions
* **ConversationScript** provides a purely data-driven dialogue format

---

## 2. System Architecture Overview

```
Narrator (DontDestroyOnLoad)
 ├─ TryPlayForScene(SceneID)
 ├─ EnterStoryMode()
 └─ StartConversation(ScriptId)
        ↓
RuleDB
 ├─ Load rules from Resources/Rules
 ├─ Pick next rule (NeedPlay → Priority)
 └─ MarkPlayed / SetNeedPlay
        ↓
DialogRunner
 ├─ ShowPiece(ConversationPiece)
 ├─ Update dialog text (Typewriter supported)
 ├─ Update Portrait / Background
 ├─ Display Options
 └─ HandleOption(option)
        ↓
ConversationOption
 ├─ targetID → jump to next Piece
 └─ onSelected.Invoke()
        ↓
ScriptableEvent (Event Bus)
 └─ Execute in-scene actions (give item / remove item / print message)
```

**Core idea:**
**Scene → Rule → Conversation → Player Option → ScriptableEvent → Scene Actions**

---

## 3. Flow: Scene Entry → Rule Selection → Dialogue → Event Trigger

### **1) Player enters a scene**

External systems call:

```csharp
Narrator.Instance.TryPlayForScene(sceneID);
```

Narrator will:

* Check if story mode is active
* Query RuleDB for a matching NarrativeRule
* Select the highest-priority rule
* Start the conversation through the DialogRunner

---

### **2) RuleDB selects a NarrativeRule**

`GetNext(SceneID)` performs:

1. Excludes rules where `PlayOnce == true` and already played
2. Prioritizes rules marked `NeedPlay`
3. Orders remaining rules by `Priority` (descending)
4. Returns the best match

After the narrative runs:

```csharp
RuleDB.MarkPlayed(rule);
```

Ensuring one-time events never repeat.

---

### **3) DialogRunner starts the conversation**

Narrator calls:

```csharp
dialogRunner.StartConversation(script);
```

DialogRunner responsibilities:

* Fade in the dialogue panel
* Render text (with Typewriter support via SimplePrinter)
* Update portrait image and optional descriptions
* Update background image if provided
* Instantiate and display options
* Process Piece transitions

---

### **4) Player selects an option**

Each option includes:

```csharp
targetID (string)
onSelected (UnityEvent)
```

Flow:

* If `targetID != ""` → jump to next Piece
* If empty → end the dialogue
* Finally trigger:

```csharp
option.onSelected?.Invoke();
```

This invokes ScriptableEvents → which trigger in-scene actions.

---

### **5) ScriptableEvent → In-scene Actions**

ScriptableEvent acts as a simple event bus:

```
ScriptableEvent
 ├─ Register(listener)
 ├─ UnRegister(listener)
 └─ Raise()
```

Listeners can:

* Print messages
* Give items
* Remove items
* Trigger animations
* Control environment objects
* Anything placed inside the scene as a Triggerable

**Dialogue → Option → ScriptableEvent → Scene Logic**
forms a complete narrative pipeline.

---

## 4. Folder Structure

```plaintext
NarratorSystem/
 ├─ ConversationLayer/
 │   ├─ ConversationOption.cs
 │   ├─ ConversationPiece.cs
 │   ├─ ConversationScript.cs
 │   └─ ConversationScriptDB.cs
 │
 ├─ DialogCore/
 │   ├─ DialogueRunner.cs
 │   └─ Narrator.cs
 │
 ├─ Eventbus/
 │   ├─ ScriptableEvent.cs
 │   ├─ ScriptableEventListener.cs
 │   ├─ ChangeSceneEvent.cs
 │   └─ ChangeSceneEventListener.cs
 │
 ├─ Rules/
 │   ├─ NarrativeRule.cs
 │   └─ RuleDB.cs
 │
 ├─ Triggerable/
 │   ├─ ITriggerable.cs
 │   ├─ PrintMsgOnTrigger.cs
 │   ├─ ReceiveItemOnTrigger.cs
 │   └─ RemoveItemOnTrigger.cs
 │
 └─ Util/
     └─ SimplePrinter.cs
```

---

## 5. Key Components

### **Narrator.cs**

Core logic entry point:

* Singleton, DontDestroyOnLoad
* Ensures ScriptDB / RuleDB are loaded
* `TryPlayForScene()` handles rule evaluation
* `StartConversation()` delegates to DialogRunner
* Manages story mode toggles

Responsible for **when** and **what** narratives play.

---

### **RuleDB.cs**

Narrative rule database:

* Loads all NarrativeRule SOs from `Resources/Rules`
* Creates runtime instances to avoid modifying assets
* Selects rules using:

  * SceneID
  * Priority
  * NeedPlay
  * PlayOnce / HasPlayed
* Provides `MarkPlayed()` and `SetNeedPlay()`

This is the **decision-making engine**.

---

### **DialogRunner.cs**

Handles dialogue UI and playback logic:

* Fade in/out
* Update TextMeshPro dialogue text
* Update portrait, background, portrait description
* Generate option buttons dynamically
* Call `HandleOption()` to process selection
* Integrate SimplePrinter for Typewriter text
* Reset panel after ending dialogue

Clean separation of UI and logic.

---

### **ConversationScript / Piece / Option**

ScriptableObject data format:

* **ConversationScript** – the entire conversation
* **ConversationPiece** – one line or node
* **ConversationOption** – branching options with actions

Fully data-driven.

---

### **ScriptableEvent / ScriptableEventListener**

Event system used for scene actions:

* Raise events globally
* Listeners respond with UnityEvents
* Excellent for loose coupling between dialogue and scene logic

---

### **Triggerable**

Scene-level response components:

* ReceiveItemOnTrigger
* RemoveItemOnTrigger
* PrintMsgOnTrigger

Executed via ScriptableEvents.

---

## 6. ScriptableObjects

### **NarrativeRule**

Defines when a narrative should play:

* `RuleID`
* `ScriptID`
* `SceneID`
* `Priority`
* `NeedPlay`
* `PlayOnce`
* `HasPlayed`

The heart of narrative selection.

---

### **ConversationScript & Piece**

Each **Piece** contains:

* text
* portrait
* portraitDescription
* background
* audio
* options[]
* NextPiece (linear flow)

---

## 7. Future Extensions

Potential enhancements:

### **1. Conditional Dialogue System**

Branch based on flags, stats, inventory, alignment, etc.

### **2. Visual Dialogue Editor**

A graph-based tool for editing conversations and branches.

### **3. Localization / String Tables**

Full multi-language support.

### **4. Cutscene Integration**

Timeline / Camera motion / Character animations.

### **5. Save/Load Integration**

Persist `HasPlayed` and narrative progress.

### **6. Multi-path Narrative Tracking**

Record player choices for long-term consequences.
