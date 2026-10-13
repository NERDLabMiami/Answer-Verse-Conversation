using UnityEngine;
using UnityEditor;

namespace VNEngine
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(ExitActorNode))]
    public class ExitActorNodeEditor : Editor
    {
        private SerializedProperty m_actor_name_from;
        private SerializedProperty m_actor_name;
        private SerializedProperty m_actor_data;
        private SerializedProperty m_slide_out;
        private SerializedProperty m_fade_out;
        private SerializedProperty m_fade_out_time;

        private void OnEnable()
        {
            m_actor_name_from = serializedObject.FindProperty("actor_name_from");
            m_actor_name = serializedObject.FindProperty("actor_name");
            m_actor_data = serializedObject.FindProperty("actor_data");
            m_slide_out = serializedObject.FindProperty("slide_out");
            m_fade_out = serializedObject.FindProperty("fade_out");
            m_fade_out_time = serializedObject.FindProperty("fade_out_time");
        }

        override public void OnInspectorGUI()
        {
            serializedObject.Update();

            var node = target as ExitActorNode;

            VNNodeEditorGUI.DrawCategoryBanner(VNNodeEditorGUI.NodeCategory.Actor);

            VNNodeEditorGUI.DrawActorSourceField(
                m_actor_name_from, m_actor_data, m_actor_name,
                node.actor_name_from, node.actor_name,
                popupTooltip: "Direct Reference: drag the Actor asset below. CSV Key / String Stat: resolved to an Actor by matching its actor_name at runtime, for a dynamically-determined actor to remove.",
                objectFieldLabel: "Actor",
                objectFieldTooltip: "Drag the Actor asset (already on scene via 'EnterActorNode') to remove from the scene.",
                statFieldLabel: "Actor Name",
                statFieldTooltip: "CSV key / Stat name used to look up which actor to remove at runtime.",
                legacyWarningExtra: "Run Pipeline > Writing > Migrate Actor References, or drag the matching Actor asset in above.");

            EditorGUILayout.PropertyField(m_slide_out, new GUIContent("Slide Out", "If true, have the character slide out before being destroyed."));
            EditorGUILayout.PropertyField(m_fade_out, new GUIContent("Fade Out", "If true, have the character fade out before being destroyed."));
            if (node.fade_out)
                EditorGUILayout.PropertyField(m_fade_out_time, new GUIContent("Fade Out Time", "How many seconds does it take to fade out?"));

            serializedObject.ApplyModifiedProperties();
        }
    }
}
