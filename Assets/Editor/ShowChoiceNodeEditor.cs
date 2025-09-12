#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System.Linq;
using System.Collections.Generic;
using VNEngine;

[CustomEditor(typeof(ShowChoiceNode))]
public class ShowChoiceNodeEditor : Editor
{
    SerializedProperty choicesProp;
    SerializedProperty hideDialogueUIProp;
    SerializedProperty logOnShowProp, logOnSelectProp, includeTraitSnapshotProp;
    SerializedProperty traitRegistryProp;          // optional if you add a registry later

    private ShowChoiceNode node;
    void OnEnable()
    {
       
        choicesProp               = serializedObject.FindProperty("choices");
        hideDialogueUIProp        = serializedObject.FindProperty("hideDialogueUI");
        logOnShowProp             = serializedObject.FindProperty("logOnShow");
        logOnSelectProp           = serializedObject.FindProperty("logOnSelect");
        includeTraitSnapshotProp  = serializedObject.FindProperty("includeTraitSnapshot");
        traitRegistryProp         = serializedObject.FindProperty("traitRegistry");
        // --- MIGRATION: fill traitKey from legacy enum (once) ---
       MigrateTraitKeys();
    }
    void MigrateTraitKeys()
    {
        if (choicesProp == null) return;

        // Source of truth for built-ins: enum order
        var builtInKeys = GateTraitsNode.AllTraitKeys().ToArray(); // ["Humor","Charisma","Empathy","Grades"]

        for (int i = 0; i < choicesProp.arraySize; i++)
        {
            var ch = choicesProp.GetArrayElementAtIndex(i);
            var reqsProp = ch.FindPropertyRelative("requirements");
            if (reqsProp == null) continue;

            for (int j = 0; j < reqsProp.arraySize; j++)
            {
                var r = reqsProp.GetArrayElementAtIndex(j);

                // new string path
                var keyProp = r.FindPropertyRelative("traitKey");
                if (keyProp != null && string.IsNullOrEmpty(keyProp.stringValue))
                {
                    // legacy enum path (if present)
                    var enumProp = r.FindPropertyRelative("enumTrait"); // type: VNEngine.Trait
                    if (enumProp != null)
                    {
                        int idx = Mathf.Clamp(enumProp.enumValueIndex, 0, builtInKeys.Length - 1);
                        keyProp.stringValue = builtInKeys.Length > 0 ? builtInKeys[idx] : string.Empty;
                    }
                }
            }
        }
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();


        // Choices list
        EditorGUILayout.Space(6);
        EditorGUILayout.LabelField("Choices", EditorStyles.boldLabel);

        bool anyChoiceLogging = false;

        if (choicesProp != null)
        {
            for (int i = 0; i < choicesProp.arraySize; i++)
            {
                var ch = choicesProp.GetArrayElementAtIndex(i);
                using (new EditorGUILayout.VerticalScope("box"))
                {
                    EditorGUILayout.BeginHorizontal();
                    ch.isExpanded = EditorGUILayout.Foldout(ch.isExpanded, $"Choice {i}", true);
                    if (GUILayout.Button("X", GUILayout.Width(22))) { choicesProp.DeleteArrayElementAtIndex(i); break; }
                    EditorGUILayout.EndHorizontal();

                    if (!ch.isExpanded) continue;

                    // ---- fields on Choice ----
                    EditorGUI.indentLevel++;
                    EditorGUILayout.PropertyField(ch.FindPropertyRelative("text"), new GUIContent("Text"));
                    EditorGUILayout.PropertyField(ch.FindPropertyRelative("nextConversation"), new GUIContent("Jump To"));

                    // Button modifier (ScriptableObject). If you haven't added it yet, comment this line out.
                    var modifierProp = ch.FindPropertyRelative("modifier");
                    if (modifierProp != null)
                        EditorGUILayout.PropertyField(modifierProp, new GUIContent("Button Modifier"));

                    // Requirements (FlexibleTraitRequirement list expected)
                    var reqsProp = ch.FindPropertyRelative("requirements");
                    DrawRequirements(reqsProp);

                    // Per-choice logging toggle + labels
                    var enableLoggingProp = ch.FindPropertyRelative("enableLogging");
                    if (enableLoggingProp != null)
                    {
                        EditorGUILayout.PropertyField(enableLoggingProp, new GUIContent("Enable Logging"));
                        if (enableLoggingProp.boolValue)
                        {
                            anyChoiceLogging = true;
                            EditorGUILayout.PropertyField(ch.FindPropertyRelative("label"));
                            EditorGUILayout.PropertyField(ch.FindPropertyRelative("category"));
                            EditorGUILayout.PropertyField(ch.FindPropertyRelative("variant"));
                        }
                    }

                    EditorGUI.indentLevel--;
                }
            }

            // Add button
            if (GUILayout.Button("Add Choice"))
            {
                int i = choicesProp.arraySize;
                choicesProp.InsertArrayElementAtIndex(i);
                var ch = choicesProp.GetArrayElementAtIndex(i);

                // Reset fields on the new choice
                var textProp = ch.FindPropertyRelative("text");
                if (textProp != null) textProp.stringValue = string.Empty;

                var nextConvProp = ch.FindPropertyRelative("nextConversation");
                if (nextConvProp != null) nextConvProp.objectReferenceValue = null;

                var reqsProp = ch.FindPropertyRelative("requirements");
                if (reqsProp != null) reqsProp.arraySize = 0;  // clear list

                var enableLoggingProp = ch.FindPropertyRelative("enableLogging");
                if (enableLoggingProp != null) enableLoggingProp.boolValue = false;

                var labelProp = ch.FindPropertyRelative("label");
                if (labelProp != null) labelProp.stringValue = string.Empty;

                var categoryProp = ch.FindPropertyRelative("category");
                if (categoryProp != null) categoryProp.stringValue = string.Empty;

                var variantProp = ch.FindPropertyRelative("variant");
                if (variantProp != null) variantProp.stringValue = string.Empty;

                var modifierProp = ch.FindPropertyRelative("modifier");
                if (modifierProp != null) modifierProp.objectReferenceValue = null;

                // If you have any other serialized fields on Choice, reset them here too.
            }

        }
        EditorGUILayout.PropertyField(traitRegistryProp, new GUIContent("Trait Registry"));

        if (traitRegistryProp.objectReferenceValue == null)
        {
            EditorGUILayout.HelpBox("No TraitRegistry assigned. Falling back to legacy enum list.", MessageType.Warning);
#if UNITY_EDITOR
            if (GUILayout.Button("Create Default TraitRegistry in Assets/Resources"))
            {
                var reg = ScriptableObject.CreateInstance<TraitRegistry>();
                // add your 4 built-ins
                reg.numberedTraitKeys = new List<string>{ "Humor","Charisma","Empathy","Grades" };
                System.IO.Directory.CreateDirectory("Assets/Resources");
                var path = "Assets/Resources/TraitRegistry.asset";
                UnityEditor.AssetDatabase.CreateAsset(reg, path);
                UnityEditor.AssetDatabase.SaveAssets();
                traitRegistryProp.objectReferenceValue = reg;
            }
#endif
        }
        EditorGUILayout.PropertyField(hideDialogueUIProp);

        // Node-level analytics (only if any choice has logging enabled)
        if (anyChoiceLogging)
        {
            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("Open Game Data", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(logOnShowProp,            new GUIContent("Log On Show"));
            EditorGUILayout.PropertyField(logOnSelectProp,          new GUIContent("Log On Select"));
            EditorGUILayout.PropertyField(includeTraitSnapshotProp, new GUIContent("Include Trait Snapshot"));

        }

        serializedObject.ApplyModifiedProperties();
    }

    // --- helpers --------------------------------------------------------------

    static string[] BuiltInTraitKeys()
    {
        // Fallback: enum-based keys from GateTraitsNode (Humor, Charisma, Empathy, Grades)
        return GateTraitsNode.AllTraitKeys().ToArray();
    }

void DrawRequirements(SerializedProperty reqsProp)
{
    if (reqsProp == null) return;

    // Prefer registry; fall back to enum keys
    var reg = GetRegistry(); // your helper that reads node.traitRegistry or Resources
    string[] keys = (reg != null && reg.numberedTraitKeys != null && reg.numberedTraitKeys.Count > 0)
        ? reg.numberedTraitKeys.ToArray()
        : GateTraitsNode.AllTraitKeys().ToArray();

    EditorGUILayout.LabelField("Requirements (All must pass)", EditorStyles.miniBoldLabel);

    for (int j = 0; j < reqsProp.arraySize; j++)
    {
        var r = reqsProp.GetArrayElementAtIndex(j);
        using (new EditorGUILayout.HorizontalScope())
        {
            var keyProp  = r.FindPropertyRelative("traitKey");
            var compProp = r.FindPropertyRelative("compare");
            var valProp  = r.FindPropertyRelative("value");

            if (keyProp == null) {
                EditorGUILayout.HelpBox("This node uses the legacy requirement type. Convert to FlexibleTraitRequirement.", MessageType.Info);
                break;
            }

            int sel = Mathf.Max(0, System.Array.IndexOf(keys, keyProp.stringValue));
            int newSel = EditorGUILayout.Popup(sel < 0 ? 0 : sel, keys, GUILayout.MaxWidth(200));
            keyProp.stringValue = (newSel >= 0 && newSel < keys.Length) ? keys[newSel] : keyProp.stringValue;

            if (compProp != null) EditorGUILayout.PropertyField(compProp, GUIContent.none, GUILayout.MaxWidth(140));
            if (valProp  != null) EditorGUILayout.PropertyField(valProp,  GUIContent.none, GUILayout.MaxWidth(80));

            if (GUILayout.Button("-", GUILayout.Width(22)))
                reqsProp.DeleteArrayElementAtIndex(j);
        }
    }

    if (GUILayout.Button("Add Requirement", GUILayout.MaxWidth(160)))
    {
        int j = reqsProp.arraySize;
        reqsProp.InsertArrayElementAtIndex(j);
        var r = reqsProp.GetArrayElementAtIndex(j);

        var keyProp  = r.FindPropertyRelative("traitKey");
        var compProp = r.FindPropertyRelative("compare");
        var valProp  = r.FindPropertyRelative("value");

        if (keyProp  != null) keyProp.stringValue = keys.Length > 0 ? keys[0] : string.Empty;
        if (compProp != null) compProp.enumValueIndex = (int)NumberCompare.GreaterOrEqual;
        if (valProp  != null) valProp.floatValue = 1f;

        // keep legacy enum hidden but valid
        var enumProp = r.FindPropertyRelative("enumTrait");
        if (enumProp != null) enumProp.enumValueIndex = 0;
    }
}
    TraitRegistry GetRegistry()
{
    // prefer per-node override, else Resources/TraitRegistry
    var regObj = traitRegistryProp != null ? traitRegistryProp.objectReferenceValue as TraitRegistry : null;
    if (regObj != null) return regObj;
    return TraitRegistry.Load();
}
}
#endif
