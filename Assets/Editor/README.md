# Editor Tools

Everything here is editor-only (`#if UNITY_EDITOR` / lives under `Assets/Editor`, so it's
excluded from builds). Two kinds of things live in this folder:

- Per-node `[CustomEditor]` Inspectors (`DialogueNodeEditor.cs`, `ChoiceNodeEditor.cs`, etc.),
  sharing GUI helpers factored into `VNNodeEditorGUI.cs` (label widths, the actor-source field,
  trait-requirement list drawer, and the category-color banner shown at the top of each node's
  Inspector).
- Writer/maintenance tooling, registered under one `VNEngine` menu so it's all discoverable in
  the same place instead of scattered across ad hoc menu roots.

## `VNEngine` menu (top menu bar)

- **Writing/Conversation IO** (`VNConversationIO.cs`) - round-trips a conversation's nodes
  to/from CSV so a writer can edit dialogue in a spreadsheet and patch-import the changes back.
  Matches nodes across export/import via a stable per-node GUID (`NodeExportTag`), so reordering
  nodes between export and import doesn't break the patch.
- **Writing/Build Actor Assets From Character Folders** (`ActorAssetBuilder.cs`) - bulk-populates
  each character's `Actor` asset from their sprite folder under `Assets/Characters/`.

## `GameObject > VNEngine` menu (Hierarchy right-click / top GameObject menu)

`VNAddNodeMenu.cs` adds a node to a conversation without dragging a prefab in from
`Assets/Resources/Conversation Pieces/` by hand. Right-click (or select and use the top
`GameObject` menu) a `ConversationManager` GameObject, or any node already parented under one,
and cascade through `VNEngine > <Category>[/<Subcategory>] > <Node>`:

```
VNEngine
├── Actor
│   ├── Entrance
│   │   ├── Enter Actor
│   │   ├── Enter Multiple Actors
│   │   ├── Exit Actor
│   │   └── Exit All Actors
│   ├── Movement
│   │   ├── Actor Forward
│   │   ├── Actor Backward
│   │   ├── Actor Inward
│   │   ├── Actor Outward
│   │   └── Pan Actors
│   ├── Visual
│   │   ├── Brighten Actor
│   │   ├── Brighten All Actors
│   │   ├── Darken All Actors
│   │   ├── Change Actor Image
│   │   └── Flip Actor Face Direction
│   └── Dialogue
│       ├── Dialogue
│       ├── Hide Dialogue Panel
│       └── Think
├── Background
│   ├── Set Background
│   ├── Turn Off Background
│   ├── Turn Off Foreground
│   ├── Fade in from Black
│   └── Fade out to Black
├── Scene & System
│   ├── Branching
│   │   ├── Show Choices
│   │   ├── Show Choices - Timed
│   │   └── Gate Stat Node
│   ├── Begin Scene Setup
│   ├── End Scene Setup
│   ├── Change Conversation
│   ├── Load Scene
│   ├── Save Checkpoint
│   ├── Play Sound Effect
│   ├── Set Music
│   ├── Keyboard Entry
│   ├── Wait
│   └── Time Passes
├── Logging
│   ├── Debug Stat Set
│   └── OpenGameData Event
└── Miscellaneous
    ├── Closer Look
    ├── Text Message
    ├── New Contact
    └── Task
```

Where the node lands depends on what you right-clicked: on the `ConversationManager` itself, it's
appended as the last child; on an existing node, it's inserted immediately after it in sibling
order - which is execution order, since `ConversationManager` runs its children in Hierarchy
sibling order (`Assets/Scripts/Dialogue/ConversationManager.cs`). The menu is only enabled when a
`ConversationManager` can be resolved from the current selection.

Each entry is a real `[MenuItem]` (not a runtime-built popup), so it renders as native cascading
submenus. All ~40 share one validate method (`CanAdd`) since the enable condition - a
`ConversationManager` resolvable from the selection - is identical for every node type.

**Adding a new node type to the menu:** add a `[MenuItem(MenuRoot + "<Category>/<Node Name>", false, 10)]`
method (one line, calling `Create(c, "<Prefab Name>")`) under the right category, and add the
matching `[MenuItem(MenuRoot + "<Category>/<Node Name>", true)]` attribute to the shared `CanAdd`
method. The prefab name must match a `.prefab` file in `Assets/Resources/Conversation Pieces/`
exactly (display names in this menu don't always match the prefab file name, e.g. `KeyboardEntry`).
