# Notice

The dialogue/conversation engine in this project ("AnswerVerse VNEngine", under the
`VNEngine` namespace in `Assets/Scripts/`) began as the VNEngine Unity asset. Its original
developer no longer maintains it, so this project has fully absorbed and extended it -
custom nodes, editor tooling, and the FMOD-optional audio system all live alongside the
original code with no package boundary between them. The `VNEngine` namespace now covers
all of it uniformly; there's no need to distinguish "stock" from "custom" going forward.

This repository's own code is released under CC0 1.0 (see `LICENSE`). That dedication
covers what NERD Lab Miami has written here; it does not, by itself, change whatever terms
the original VNEngine asset was distributed under before being absorbed into this project.
