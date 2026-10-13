using UnityEngine;
using UnityEditor;

namespace VNEngine
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(DialogueNode))]
    public class DialogueNodeEditor : Editor
    {
        float normal_label_width;
        private bool show_advanced = false;
        private SerializedProperty m_darken;
        private SerializedProperty m_front;
        private SerializedProperty m_clear;
        private SerializedProperty m_log;
        private SerializedProperty m_log_category;
        private SerializedProperty m_actor_name_from;
        private SerializedProperty m_actor;
        private SerializedProperty m_actor_data;
        private SerializedProperty m_textbox;
        private SerializedProperty m_speaker_name_matches_actor;
        private SerializedProperty m_speaker_name_source;
        private SerializedProperty m_text_source;
        private SerializedProperty m_localized_key;
        private SerializedProperty m_talking_beep;
        private SerializedProperty m_fetch_localized_stat_value;

        private void OnEnable()
        {
            m_darken = serializedObject.FindProperty("darken_all_other_characters");
            m_front = serializedObject.FindProperty("bring_speaker_to_front");
            m_clear = serializedObject.FindProperty("clear_text_after");
            m_log = serializedObject.FindProperty("log_text");
            m_log_category = serializedObject.FindProperty("log_category");
            m_actor = serializedObject.FindProperty("actor");
            m_actor_data = serializedObject.FindProperty("actor_data");
            m_actor_name_from = serializedObject.FindProperty("actor_name_from");
            m_textbox = serializedObject.FindProperty("textbox_title");
            m_speaker_name_matches_actor = serializedObject.FindProperty("speaker_name_matches_actor");
            m_text_source = serializedObject.FindProperty("dialogue_from");
            m_speaker_name_source = serializedObject.FindProperty("speaker_name_from");
            m_localized_key = serializedObject.FindProperty("localized_key");
            m_talking_beep = serializedObject.FindProperty("talking_beeps");
            m_fetch_localized_stat_value = serializedObject.FindProperty("fetch_localized_stat_value");
        }

        override public void OnInspectorGUI()
        {
            serializedObject.Update();

            var node = target as DialogueNode;
            normal_label_width = EditorGUIUtility.labelWidth;

            VNNodeEditorGUI.DrawCategoryBanner(VNNodeEditorGUI.NodeCategory.Dialogue);

            VNNodeEditorGUI.DrawActorSourceField(
                m_actor_name_from, m_actor_data, m_actor,
                node.actor_name_from, node.actor,
                popupTooltip: "Direct Reference: drag the Actor asset below. CSV Key / String Stat: resolved to an Actor by matching its actor_name at runtime, for a dynamically-determined speaker.",
                objectFieldLabel: "Actor",
                objectFieldTooltip: "WHO IS ON STAGE. Drag the Actor asset that is talking - it must already be brought on scene via 'EnterActorNode'. Controls which character darkens/lightens/moves to the front and where the speaker panel points - does NOT control the printed name text below.",
                statFieldLabel: "Actor",
                statFieldTooltip: "CSV key / Stat name used to look up which actor is speaking at runtime.",
                legacyWarningExtra: "Run Pipeline > Writing > Migrate Actor References, or drag the matching Actor asset in above.");

            bool can_sync_to_actor = node.actor_name_from == Dialogue_Source.Text_From_Editor;
            if (can_sync_to_actor)
            {
                EditorGUILayout.PropertyField(m_speaker_name_matches_actor, new GUIContent("Speaker Name matches Actor", "When on, the printed speaker name automatically follows the Actor above - no separate field to keep in sync. Turn off only for narrator text or a nickname that should differ from the Actor."));
            }

            if (can_sync_to_actor && node.speaker_name_matches_actor)
            {
                string preview = node.actor_data != null ? node.actor_data.Get_Display_Name() : "(no Actor assigned above)";
                EditorGUILayout.LabelField("Speaker name (preview)", preview);
            }
            else
            {
                EditorGUILayout.PropertyField(m_speaker_name_source, new GUIContent("Speaker name from", "Get the name of the speaker directly from the editor or from the UI CSV"));

                string speaker_name_tooltip = "Speaker name";
                if (node.speaker_name_from == Dialogue_Source.Localized_Text_From_CSV)
                {
                    EditorGUILayout.HelpBox("You must assign a Localized_UI_CSV to the UIManager in the DialogueCanvas/SceneManager to use localized speaker names", MessageType.Info);
                    speaker_name_tooltip = "Speaker key";
                }
                else if (node.speaker_name_from == Dialogue_Source.Text_From_String_Stat)
                {
                    EditorGUILayout.HelpBox("Enter the name of the String Stat you wish to use below.\nTo create a Stat, use StatsManager.Set_String_Stat().\nIf there is no Stat of that name present, the name of Stat will be placed in the speaker title area.", MessageType.Info);
                    speaker_name_tooltip = "String Stat name";
                }
                EditorGUILayout.PropertyField(m_textbox, new GUIContent(speaker_name_tooltip, "WHAT TEXT IS PRINTED as the speaker's name above the dialogue. Independent of the 'Actor' field above - leaving this blank will clear the name label rather than reuse the actor's name. Only needs to differ from Actor for narrator text or nicknames."));
                if (node.speaker_name_from == Dialogue_Source.Text_From_String_Stat)
                {
                    EditorGUIUtility.labelWidth = VNNodeEditorGUI.WideLabelWidth;
                    EditorGUILayout.PropertyField(m_fetch_localized_stat_value, new GUIContent("Localize value fetched from Stat", "If true, the value fetched from the specified String Stat will be used as a key to look up a localized version of it from your Localized Dialogue CSV. You'll need to add entries with the possible Stat values into your CSV."));
                    EditorGUIUtility.labelWidth = normal_label_width;
                }
            }


            GUIStyle g = new GUIStyle();
            g.richText = true;
            EditorGUILayout.LabelField("");
            switch (node.dialogue_from)
            {
                case Dialogue_Source.Text_From_Editor:
                    EditorGUILayout.BeginHorizontal();
                    EditorGUIUtility.labelWidth = 300;
                    EditorGUILayout.LabelField("Dialogue  (you can use rich text, ex: <b>Bold</b> becomes");
                    EditorGUIUtility.labelWidth = normal_label_width;
                    EditorGUILayout.LabelField("<b>Bold</b>", g);
                    EditorGUILayout.EndHorizontal();
                    EditorStyles.textField.wordWrap = true;
                    node.text = EditorGUILayout.TextArea(node.text);
                    break;
                case Dialogue_Source.Localized_Text_From_CSV:
                    EditorGUILayout.HelpBox("You must assign a Localized_Dialogue_CSV to the DialogueCanvas/SceneManager to use localized text", MessageType.Info);
                    EditorGUILayout.LabelField("Get text from a CSV you supply. Each row has a key. Enter the key below for the text that you want to retrieve at run-time.");
                    EditorGUILayout.PropertyField(m_localized_key, new GUIContent("Key from CSV", "The key for the row you to use from your Localized Dialogue CSV (located in the VN Scene Manager)"));
                    break;
            }

            EditorGUILayout.Space();
            show_advanced = EditorGUILayout.Foldout(show_advanced, "Advanced", true);
            if (show_advanced)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(m_text_source, new GUIContent("Text comes from ", "Text is either entered using the Unity Editor, or retrieved from a localized CSV at runtime"));
                EditorGUILayout.PropertyField(m_talking_beep, new GUIContent("Talking beeps", "Plays this extremely short audio clip every time a character of text is printed to the text panel"));
                EditorGUILayout.PropertyField(m_darken, new GUIContent("Darken other actors", "Darkens the non-speaking actors on the scene. Same as using 'DarkenActorNode'"));
                EditorGUILayout.PropertyField(m_front, new GUIContent("Bring speaker forward", "Since this is a 2D scene, actors are drawn in a certain order, meaning they can cover one another. This will ensure the speaker will not be covered."));
                EditorGUILayout.PropertyField(m_clear, new GUIContent("Clear text panel after", "Removes all text from the text and speaker panel"));

                EditorGUILayout.PropertyField(m_log, new GUIContent("Record text in Log", "Puts the dialogue text into the Text log, which can be accessed via the 'Log' button in the bottom right hand corner."));
                if (node.log_text)
                {
                    EditorGUILayout.PropertyField(m_log_category, new GUIContent("Log text under category", "Text in the text log is stored in a dictionary accessed by the string key you type here."));
                }
                EditorGUI.indentLevel--;
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
}