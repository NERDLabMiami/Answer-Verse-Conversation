// ButtonModifier.cs
using UnityEngine;

[CreateAssetMenu(menuName="AnswerVerse/Button Modifier")]
public class ButtonModifier : ScriptableObject
{
    public string id;               // e.g., "charisma_check" or "sarcastic"
    public Sprite icon;             // UI icon
    public string vfxTag;           // hook for VFX system
    public string requiredTraitKey; // optional: e.g., "Charisma"
    public bool launchMiniGame;     // future hook
    public string miniGameId;       // to route into your flow
    public Color iconTint = Color.white;
}