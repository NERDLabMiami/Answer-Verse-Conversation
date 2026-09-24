using UnityEditor;
using UnityEngine;

namespace VNEngine
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(EndSceneSetupNode))]
    public class EndSceneSetupNodeEditor : Editor
    {
        private SerializedProperty fade_duration;

        private void OnEnable()
        {
            fade_duration = serializedObject.FindProperty("fade_duration");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            VNNodeEditorGUI.DrawCategoryBanner(VNNodeEditorGUI.NodeCategory.Background);

            EditorGUILayout.HelpBox("Fades out the black curtain raised by a Begin Scene Setup node, revealing everything set up underneath in one smooth fade.", MessageType.Info);
            EditorGUILayout.PropertyField(fade_duration, new GUIContent("Fade Duration (seconds)", "How long the reveal fade takes."));

            serializedObject.ApplyModifiedProperties();
        }
    }
}
