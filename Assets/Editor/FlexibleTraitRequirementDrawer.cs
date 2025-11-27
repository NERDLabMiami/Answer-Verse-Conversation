#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.Linq;
using VNEngine;   // so it can see FlexibleTraitRequirement and GateTraitsNode

[CustomPropertyDrawer(typeof(FlexibleTraitRequirement))]
public class FlexibleTraitRequirementDrawer : PropertyDrawer
{
    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        // Single line
        return EditorGUIUtility.singleLineHeight;
    }

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);

        var traitKeyProp = property.FindPropertyRelative("traitKey");
        var compareProp  = property.FindPropertyRelative("compare");
        var valueProp    = property.FindPropertyRelative("value");

        // Load registry
        TraitRegistry registry = TraitRegistry.Load();
        string[] options;

        if (registry != null && registry.numberedTraitKeys != null && registry.numberedTraitKeys.Count > 0)
        {
            options = registry.numberedTraitKeys.ToArray();
        }
        else
        {
            // Fallback to enum-based keys if no registry is found
            options = GateTraitsNode.AllTraitKeys().ToArray();
        }

        if (options.Length == 0)
        {
            EditorGUI.HelpBox(position,
                "No traits found. Add keys to TraitRegistry.numberedTraitKeys.",
                MessageType.Warning);
            EditorGUI.EndProperty();
            return;
        }

        // Layout: [TraitDropdown] [CompareDropdown] [ValueFloat]
        float totalWidth = position.width;
        float traitWidth = totalWidth * 0.45f;
        float compareWidth = totalWidth * 0.25f;
        float valueWidth = totalWidth * 0.30f;
        float padding = 2f;

        Rect traitRect = new Rect(position.x, position.y, traitWidth, position.height);
        Rect compareRect = new Rect(traitRect.xMax + padding, position.y, compareWidth, position.height);
        Rect valueRect = new Rect(compareRect.xMax + padding, position.y, valueWidth, position.height);

        // Current index
        int currentIndex = Mathf.Max(0, System.Array.IndexOf(options, traitKeyProp.stringValue));
        if (currentIndex >= options.Length) currentIndex = 0;

        EditorGUI.BeginChangeCheck();
        int newIndex = EditorGUI.Popup(traitRect, label.text, currentIndex, options);
        if (EditorGUI.EndChangeCheck())
        {
            traitKeyProp.stringValue = options[newIndex];
        }

        // Compare enum
        EditorGUI.PropertyField(compareRect, compareProp, GUIContent.none);
        // Value
        EditorGUI.PropertyField(valueRect, valueProp, GUIContent.none);

        EditorGUI.EndProperty();
    }
}
#endif
