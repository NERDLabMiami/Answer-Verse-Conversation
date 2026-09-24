# Nodes

All node types - original VNEngine nodes and AnswerVerse-authored ones alike - live here
together under the `VNEngine` namespace, with the same base class (`Node.cs`) and no folder
split between "stock" and "custom." Copy `NodeTemplate.cs` when adding a new node type.

Two naming conventions coexist here, for historical reasons:

- **`<Feature>Node`** (e.g. `DialogueNode`, `ChoiceNode`, `GateTraitsNode`) - the convention
  most nodes use, including newer additions.
- **`Node<Feature>`** (e.g. `NodeTask`, `NodeContact`, `NodeMessage`, `NodeCheckpoint`) - an older
  batch of nodes tied to the game-simulation systems (`StatsManager`, `TaskManager`,
  `MessageManager`, etc.), built by copying `NodeTemplate.cs` before the naming settled.

These are **not** being renamed as part of any cleanup pass - they're wired into existing
game-simulation content, and renaming them is a separate decision from simplifying the
template itself.
