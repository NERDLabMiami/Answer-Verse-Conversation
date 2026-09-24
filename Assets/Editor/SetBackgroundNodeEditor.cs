using UnityEditor;
using UnityEngine;

namespace VNEngine
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(SetBackground))]
    public class SetBackgroundNodeEditor : Editor
    {
        private SerializedProperty sprite;
        private SerializedProperty set_foreground;
        private SerializedProperty fade_in;
        private SerializedProperty fade_out;
        private SerializedProperty fade_style;
        private SerializedProperty fade_duration;

        private void OnEnable()
        {
            sprite = serializedObject.FindProperty("sprite");
            set_foreground = serializedObject.FindProperty("set_foreground");
            fade_in = serializedObject.FindProperty("fade_in");
            fade_out = serializedObject.FindProperty("fade_out");
            fade_style = serializedObject.FindProperty("fade_style");
            fade_duration = serializedObject.FindProperty("fade_duration");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            var node = target as SetBackground;

            VNNodeEditorGUI.DrawCategoryBanner(VNNodeEditorGUI.NodeCategory.Background);

            EditorGUILayout.PropertyField(sprite, new GUIContent("Image", "The sprite to display."));
            if (sprite.objectReferenceValue == null)
                EditorGUILayout.HelpBox("No image assigned - the background will be blank.", MessageType.Warning);
            EditorGUILayout.PropertyField(set_foreground, new GUIContent("Set Foreground", "Foreground sits in front of Actors. Unchecked = sets the Background (behind Actors)."));

            EditorGUILayout.Space();
            EditorGUILayout.PropertyField(fade_in, new GUIContent("Fade In", "Reveal the new image gradually instead of switching instantly."));
            EditorGUILayout.PropertyField(fade_out, new GUIContent("Fade Out", "Hide the current image gradually instead of switching instantly."));

            if (node.fade_in && node.fade_out)
                EditorGUILayout.HelpBox("Fades out the current image, swaps in the new sprite while hidden, then fades the new image back in.", MessageType.Info);
            else if (!node.fade_in && !node.fade_out)
                EditorGUILayout.HelpBox("The image switches instantly, with no animation.", MessageType.Info);

            if (node.fade_in || node.fade_out)
            {
                EditorGUILayout.PropertyField(fade_style, new GUIContent("Fade Style",
                    "Fade to Black: dips through solid black/white (briefly hides everything else behind it) — good for full scene changes.\nFade Alpha: fades transparency only, letting whatever is behind show through — better for foreground overlays or subtle transitions."));
                EditorGUILayout.PropertyField(fade_duration, new GUIContent("Fade Duration (seconds)", "How long the fade takes."));
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
}
