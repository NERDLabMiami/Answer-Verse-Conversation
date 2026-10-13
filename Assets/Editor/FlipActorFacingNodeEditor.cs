using UnityEngine;
using UnityEditor;

namespace VNEngine
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(FlipActorFacingNode))]
    public class FlipActorFacingNodeEditor : Editor
    {
        private SerializedProperty m_actor_name_from;
        private SerializedProperty m_actor_name;
        private SerializedProperty m_actor_data;

        private void OnEnable()
        {
            m_actor_name_from = serializedObject.FindProperty("actor_name_from");
            m_actor_name = serializedObject.FindProperty("actor_name");
            m_actor_data = serializedObject.FindProperty("actor_data");
        }

        override public void OnInspectorGUI()
        {
            serializedObject.Update();

            var node = target as FlipActorFacingNode;

            VNNodeEditorGUI.DrawCategoryBanner(VNNodeEditorGUI.NodeCategory.Actor);

            VNNodeEditorGUI.DrawActorSourceField(
                m_actor_name_from, m_actor_data, m_actor_name,
                node.actor_name_from, node.actor_name,
                popupTooltip: "Direct Reference: drag the Actor asset below. CSV Key / String Stat: resolved to an Actor by matching its actor_name at runtime, for a dynamically-determined target.",
                objectFieldLabel: "Actor",
                objectFieldTooltip: "Drag the Actor asset (already on scene via 'EnterActorNode') to flip.",
                statFieldLabel: "Actor Name",
                statFieldTooltip: "CSV key / Stat name used to look up which actor to flip at runtime.");

            serializedObject.ApplyModifiedProperties();
        }
    }
}
