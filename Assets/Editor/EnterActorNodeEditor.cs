using UnityEngine;
using UnityEditor;

namespace VNEngine
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(EnterActorNode))]
    public class EnterActorNodeEditor : Editor
    {
        float normal_label_width;
        private bool show_advanced = false;
        private SerializedProperty m_actorname;
        private SerializedProperty m_actor_data;
        private SerializedProperty m_actor_name_from;
        private SerializedProperty m_entrancetype;
        private SerializedProperty m_fadeintime;
        private SerializedProperty m_destination;
        private SerializedProperty m_customposition;

        private void OnEnable()
        {
            m_actorname = serializedObject.FindProperty("actor_name");
            m_actor_data = serializedObject.FindProperty("actor_data");
            m_actor_name_from = serializedObject.FindProperty("actor_name_from");
            m_entrancetype = serializedObject.FindProperty("entrance_type");
            m_fadeintime = serializedObject.FindProperty("fade_in_time");
            m_destination = serializedObject.FindProperty("destination");
            m_customposition = serializedObject.FindProperty("custom_position");
        }

        override public void OnInspectorGUI()
        {
            serializedObject.Update();

            var node = target as EnterActorNode;
            normal_label_width = EditorGUIUtility.labelWidth;

            VNNodeEditorGUI.DrawCategoryBanner(VNNodeEditorGUI.NodeCategory.Actor);

            VNNodeEditorGUI.DrawActorSourceField(
                m_actor_name_from, m_actor_data, m_actorname,
                node.actor_name_from, node.actor_name,
                popupTooltip: "Direct Reference: drag the Actor asset below. CSV Key / String Stat: resolved to an Actor by matching its actor_name at runtime, for a dynamically-determined actor to bring on scene.",
                objectFieldLabel: "Actor",
                objectFieldTooltip: "Drag the Actor asset (from Resources/Actors) to bring onto the scene.",
                statFieldLabel: "Actor Name",
                statFieldTooltip: "CSV key / Stat name used to look up which actor to bring on scene at runtime.",
                legacyWarningExtra: "Run Pipeline > Writing > Migrate Actor References, or drag the matching Actor asset in above.");

            EditorGUILayout.PropertyField(m_entrancetype, new GUIContent("Entrance Type", "How should the actor enter?"));

            EditorGUILayout.PropertyField(m_destination, new GUIContent("Destination", "What side should the actor appear on?"));
            switch (node.destination)
            {
                case Actor_Positions.CUSTOM:
                    EditorGUILayout.PropertyField(m_customposition, new GUIContent("Custom Position", "What transform should this actor be anchored to?"));
                    break;
                default:
                    EditorGUILayout.HelpBox("The Actor will automatically be placed, and when new Actors appear on the same DESTINATION, Actors will automatically shift over to give the most room.", MessageType.Info);
                    break;
            }

            if (node.entrance_type == Entrance_Type.Fade_In)
            {
                EditorGUILayout.Space();
                show_advanced = EditorGUILayout.Foldout(show_advanced, "Advanced", true);
                if (show_advanced)
                {
                    EditorGUI.indentLevel++;
                    EditorGUILayout.PropertyField(m_fadeintime, new GUIContent("Fade in Time", "How many seconds does it take to fade in?"));
                    EditorGUI.indentLevel--;
                }
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
}