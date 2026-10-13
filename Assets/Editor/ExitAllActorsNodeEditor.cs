using UnityEngine;
using UnityEditor;

namespace VNEngine
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(ExitAllActorsNode))]
    public class ExitAllActorsNodeEditor : Editor
    {
        private SerializedProperty m_slide_out;
        private SerializedProperty m_fade_out;
        private SerializedProperty m_wait_for_actors_to_exit;

        private void OnEnable()
        {
            m_slide_out = serializedObject.FindProperty("slide_out");
            m_fade_out = serializedObject.FindProperty("fade_out");
            m_wait_for_actors_to_exit = serializedObject.FindProperty("wait_for_actors_to_exit");
        }

        override public void OnInspectorGUI()
        {
            serializedObject.Update();

            VNNodeEditorGUI.DrawCategoryBanner(VNNodeEditorGUI.NodeCategory.Actor);

            EditorGUILayout.PropertyField(m_slide_out, new GUIContent("Slide Out", "If true, have all characters slide out before being destroyed."));
            EditorGUILayout.PropertyField(m_fade_out, new GUIContent("Fade Out", "If true, have all characters fade out before being destroyed."));
            EditorGUILayout.PropertyField(m_wait_for_actors_to_exit, new GUIContent("Wait For Actors To Exit", "If true, this node won't finish until all actors have left the scene."));

            serializedObject.ApplyModifiedProperties();
        }
    }
}
