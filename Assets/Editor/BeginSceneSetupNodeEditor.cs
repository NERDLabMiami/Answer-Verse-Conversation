using UnityEditor;
using UnityEngine;

namespace VNEngine
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(BeginSceneSetupNode))]
    public class BeginSceneSetupNodeEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            VNNodeEditorGUI.DrawCategoryBanner(VNNodeEditorGUI.NodeCategory.Background);
            EditorGUILayout.HelpBox("Raises an opaque black curtain and hides everything behind it. Any EnterActorNode, ChangeActorImageNode, or SetBackground nodes that follow will apply instantly (their own fade/entrance settings are ignored) until an End Scene Setup node runs.", MessageType.Info);
        }
    }
}
