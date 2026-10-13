# Answer Verse Conversation Template
A conversation template for that can be used to develop content for the Answer Verse. The Answer Verse features a cast of characters from a university that can be used to construct branching narratives around the campus. 

## Audio

Audio plays through plain Unity `AudioSource`s by default - no extra setup needed. To use
FMOD instead, add `ANSWERVERSE_FMOD` to *Project Settings > Player > Scripting Define
Symbols* for your target platform(s); `AudioManager` (`Assets/Scripts/AudioManager.cs`)
switches its music/ambience/SFX playback to FMOD accordingly. Voice-over always plays
through an `AudioSource`, in both modes.

## Editor Tools

Writing/maintenance tools (CSV conversation round-trip, Actor asset builder, etc.) and a
`GameObject > VNEngine` Hierarchy menu for adding nodes to a conversation without dragging
prefabs in by hand both live under one `VNEngine` menu - see `Assets/Editor/README.md`.

- [AnswerVerse Website](https://answerverse.nerdlab.miami)
- [Template Tutorial](https://drive.google.com/file/d/1S0fkniUf7r8XGC6F9-YJUdBV_0fFhJxs/view?usp=sharing) by Justin Jacobson
- [Answer Campus: First Semester](https://nerdlab.itch.io/answer-campus-first-semester) - Currently in Development (Release scheduled for Fall 2026)
- [Wiki](https://answerverse.nerdlab.miami/wiki/doku.php?id=start)
