#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using VNEngine;

[CustomEditor(typeof(OpenGameDataNode))]
public class OpenGameDataNodeEditor : Editor
{
    // Common
    SerializedProperty studyId, notes, includeConversationContext, includePlayTime, statsCapture;

    // Assignment
    SerializedProperty assignCondition, conditionGroup, assignmentMode, conditions, writeConditionToStat /* hidden */, conditionStatKeyOverride /* hidden */;

    // Milestone
    SerializedProperty markerId;

    void OnEnable()
    {
        studyId = serializedObject.FindProperty("studyId");
        notes = serializedObject.FindProperty("notes");
        includeConversationContext = serializedObject.FindProperty("includeConversationContext");
        includePlayTime = serializedObject.FindProperty("includePlayTime");
        statsCapture = serializedObject.FindProperty("statsCapture");

        assignCondition = serializedObject.FindProperty("assignCondition");
        conditionGroup = serializedObject.FindProperty("conditionGroup");
        assignmentMode = serializedObject.FindProperty("assignmentMode");
        conditions = serializedObject.FindProperty("conditions");
        writeConditionToStat = serializedObject.FindProperty("writeConditionToStat");
        conditionStatKeyOverride = serializedObject.FindProperty("conditionStatKeyOverride");

        markerId = serializedObject.FindProperty("markerId");

        // Force global defaults the moment the component is selected
        serializedObject.Update();
        if (writeConditionToStat != null) writeConditionToStat.boolValue = true;
        serializedObject.ApplyModifiedProperties();
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        // HEADER: Common
        EditorGUILayout.PropertyField(studyId, new GUIContent("Study ID"));
        EditorGUILayout.PropertyField(notes, new GUIContent("Notes"));
        EditorGUILayout.PropertyField(includeConversationContext, new GUIContent("Include Conversation Context"));
        using (new EditorGUI.DisabledScope(!includeConversationContext.boolValue))
        {
            EditorGUILayout.PropertyField(includePlayTime, new GUIContent("Include Play Time"));
        }
        EditorGUILayout.PropertyField(statsCapture, new GUIContent("Stats Snapshot"));

        EditorGUILayout.Space(8);
        // HEADER: Condition Assignment
        EditorGUILayout.LabelField("Condition Assignment", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(assignCondition, new GUIContent("Enable Assignment"));
        if (assignCondition.boolValue)
        {
            EditorGUILayout.PropertyField(conditionGroup, new GUIContent("Condition Group (e.g., 'arm', 'phase2')"));
            EditorGUILayout.PropertyField(assignmentMode, new GUIContent("Assignment Mode"));

            // Force writeConditionToStat = true (hidden)
            if (writeConditionToStat != null) writeConditionToStat.boolValue = true;

            // Hide the override completely (less confusing)
            // (Do nothing with conditionStatKeyOverride)

            // Conditions list UI
            DrawConditionsList();

            // Validation
            ValidateConditionsBlock();
        }

        EditorGUILayout.Space(8);
        // HEADER: Milestone
        EditorGUILayout.LabelField("Milestone Marker", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(markerId, new GUIContent("Marker ID", "e.g., 'conversation_end', 'survey_complete', 'arm_end'"));

        serializedObject.ApplyModifiedProperties();
    }

    void DrawConditionsList()
    {
        using (new EditorGUILayout.VerticalScope("box"))
        {
            // Quick actions
            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("Add Condition"))
                {
                    AddConditionRow("A", 1f);
                }
                if (GUILayout.Button("Add A/B/C"))
                {
                    conditions.arraySize = 0;
                    AddConditionRow("A", 1f);
                    AddConditionRow("B", 1f);
                    AddConditionRow("C", 1f);
                }
                if (GUILayout.Button("Equalize Weights"))
                {
                    for (int i = 0; i < conditions.arraySize; i++)
                    {
                        var row = conditions.GetArrayElementAtIndex(i);
                        var weight = row.FindPropertyRelative("weight");
                        var enabled = row.FindPropertyRelative("enabled");
                        if (enabled != null && enabled.boolValue && weight != null)
                            weight.floatValue = 1f;
                    }
                }
            }

            if (conditions.arraySize == 0)
            {
                EditorGUILayout.HelpBox("No conditions yet. Add at least one (A/B/C).", MessageType.Info);
            }

            // Rows
            for (int i = 0; i < conditions.arraySize; i++)
            {
                var row = conditions.GetArrayElementAtIndex(i);
                using (new EditorGUILayout.VerticalScope("box"))
                {
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField($"Condition {i}", EditorStyles.miniBoldLabel);
                    if (GUILayout.Button("X", GUILayout.Width(22)))
                    {
                        conditions.DeleteArrayElementAtIndex(i);
                        break;
                    }
                    EditorGUILayout.EndHorizontal();

                    // Fields
                    var conditionId = row.FindPropertyRelative("conditionId");
                    var weight = row.FindPropertyRelative("weight");
                    var nextConversation = row.FindPropertyRelative("nextConversation");
                    var startOnAssign = row.FindPropertyRelative("startOnAssign");
                    var enabled = row.FindPropertyRelative("enabled");

                    EditorGUILayout.PropertyField(enabled, new GUIContent("Enabled"));

                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.PropertyField(conditionId, GUIContent.none, GUILayout.MinWidth(100));
                    if ((AssignmentMode)assignmentMode.enumValueIndex == AssignmentMode.WeightedRandom)
                    {
                        EditorGUILayout.PropertyField(weight, new GUIContent("Weight"), GUILayout.MaxWidth(220));
                    }
                    else
                    {
                        using (new EditorGUI.DisabledScope(true))
                        {
                            EditorGUILayout.FloatField(new GUIContent("Weight"), 1f, GUILayout.MaxWidth(220));
                        }
                        if (weight != null) weight.floatValue = 1f; // keep consistent
                    }
                    EditorGUILayout.EndHorizontal();

                    EditorGUILayout.PropertyField(nextConversation, new GUIContent("Next Conversation"));

                    // Force and hide StartOnAssign = true
                    if (startOnAssign != null) startOnAssign.boolValue = true;
                    using (new EditorGUI.DisabledScope(true))
                    {
                        EditorGUILayout.Toggle(new GUIContent("Start On Assign (forced)"), true);
                    }
                }
            }
        }
    }

    void AddConditionRow(string id, float w)
    {
        int i = conditions.arraySize;
        conditions.InsertArrayElementAtIndex(i);
        var row = conditions.GetArrayElementAtIndex(i);
        row.FindPropertyRelative("conditionId").stringValue = id;
        row.FindPropertyRelative("weight").floatValue = w;
        row.FindPropertyRelative("nextConversation").objectReferenceValue = null;
        var startOnAssign = row.FindPropertyRelative("startOnAssign");
        if (startOnAssign != null) startOnAssign.boolValue = true; // force on
        var enabled = row.FindPropertyRelative("enabled");
        if (enabled != null) enabled.boolValue = true;
    }

    void ValidateConditionsBlock()
    {
        bool anyEnabled = false;
        for (int i = 0; i < conditions.arraySize; i++)
        {
            var row = conditions.GetArrayElementAtIndex(i);
            var enabled = row.FindPropertyRelative("enabled");
            if (enabled != null && enabled.boolValue) { anyEnabled = true; break; }
        }
        if (!anyEnabled)
        {
            EditorGUILayout.HelpBox("Assignment is enabled, but no conditions are enabled. Nothing will be assigned.", MessageType.Warning);
        }

        // Duplicate ID check (nice to have)
        var seen = new System.Collections.Generic.HashSet<string>();
        for (int i = 0; i < conditions.arraySize; i++)
        {
            var id = conditions.GetArrayElementAtIndex(i).FindPropertyRelative("conditionId").stringValue ?? "";
            if (string.IsNullOrEmpty(id)) continue;
            if (!seen.Add(id))
            {
                EditorGUILayout.HelpBox($"Duplicate conditionId '{id}'. Consider making them unique.", MessageType.Warning);
                break;
            }
        }
    }
}
#endif
