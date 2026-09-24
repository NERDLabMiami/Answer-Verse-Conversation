using UnityEngine;
using UnityEditor;

namespace VNEngine
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(MoveActorInwardsNode))]
    public class MoveActorInwardsNodeEditor : Editor
    {
        private SerializedProperty m_actor_name_from;
        private SerializedProperty m_actor_name;
        private SerializedProperty m_actor_data;
        private SerializedProperty m_wait_until_actor_has_stopped;

        private void OnEnable()
        {
            m_actor_name_from = serializedObject.FindProperty("actor_name_from");
            m_actor_name = serializedObject.FindProperty("actor_name");
            m_actor_data = serializedObject.FindProperty("actor_data");
            m_wait_until_actor_has_stopped = serializedObject.FindProperty("wait_until_actor_has_stopped");
        }

        override public void OnInspectorGUI()
        {
            serializedObject.Update();

            var node = target as MoveActorInwardsNode;

            VNNodeEditorGUI.DrawCategoryBanner(VNNodeEditorGUI.NodeCategory.Actor);

            VNNodeEditorGUI.DrawActorSourceField(
                m_actor_name_from, m_actor_data, m_actor_name,
                node.actor_name_from, node.actor_name,
                popupTooltip: "Direct Reference: drag the Actor asset below. CSV Key / String Stat: resolved to an Actor by matching its actor_name at runtime, for a dynamically-determined target.",
                objectFieldLabel: "Actor",
                objectFieldTooltip: "Drag the Actor asset (already on scene via 'EnterActorNode') to move inwards.",
                statFieldLabel: "Actor Name",
                statFieldTooltip: "CSV key / Stat name used to look up which actor to move at runtime.");

            EditorGUILayout.PropertyField(m_wait_until_actor_has_stopped, new GUIContent("Wait until stopped", "If true, this node doesn't finish until the actor has finished moving."));

            serializedObject.ApplyModifiedProperties();
        }
    }
}
