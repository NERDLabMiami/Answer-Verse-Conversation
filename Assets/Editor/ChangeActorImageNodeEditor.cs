using UnityEngine;
using UnityEditor;

namespace VNEngine
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(ChangeActorImageNode))]
    public class ChangeActorImageNodeEditor : Editor
    {
        private bool show_advanced = false;
        private SerializedProperty m_actor_name_from;
        private SerializedProperty m_actor_name;
        private SerializedProperty m_actor_data;
        private SerializedProperty m_expression_name;
        private SerializedProperty m_outfit_name;
        private SerializedProperty m_pose;
        private SerializedProperty m_fade_in_new_image;
        private SerializedProperty m_lighten_actor;
        private SerializedProperty m_bring_actor_to_front;

        private void OnEnable()
        {
            m_actor_name_from = serializedObject.FindProperty("actor_name_from");
            m_actor_name = serializedObject.FindProperty("actor_name");
            m_actor_data = serializedObject.FindProperty("actor_data");
            m_expression_name = serializedObject.FindProperty("expression_name");
            m_outfit_name = serializedObject.FindProperty("outfit_name");
            m_pose = serializedObject.FindProperty("pose");
            m_fade_in_new_image = serializedObject.FindProperty("fade_in_new_image");
            m_lighten_actor = serializedObject.FindProperty("lighten_actor");
            m_bring_actor_to_front = serializedObject.FindProperty("bring_actor_to_front");
        }

        override public void OnInspectorGUI()
        {
            serializedObject.Update();

            var node = target as ChangeActorImageNode;

            VNNodeEditorGUI.DrawCategoryBanner(VNNodeEditorGUI.NodeCategory.Actor);

            VNNodeEditorGUI.DrawActorSourceField(
                m_actor_name_from, m_actor_data, m_actor_name,
                node.actor_name_from, node.actor_name,
                popupTooltip: "Direct Reference: drag the Actor asset below. CSV Key / String Stat: resolved to an Actor by matching its actor_name at runtime, for a dynamically-determined target.",
                objectFieldLabel: "Actor",
                objectFieldTooltip: "Drag the Actor asset (already on scene via 'EnterActorNode') whose image should change.",
                statFieldLabel: "Actor Name",
                statFieldTooltip: "CSV key / Stat name used to look up which actor's image to change at runtime.",
                legacyWarningExtra: "Run Pipeline > Writing > Migrate Actor References, or drag the matching Actor asset in above.");

            Draw_Expression_And_Pose(node);

            EditorGUILayout.Space();
            show_advanced = EditorGUILayout.Foldout(show_advanced, "Advanced", true);
            if (show_advanced)
            {
                EditorGUI.indentLevel++;
                if (node.actor_data != null)
                    VNNodeEditorGUI.DrawOutfitPicker(node.actor_data, m_outfit_name);
                EditorGUILayout.PropertyField(m_fade_in_new_image, new GUIContent("Fade in new image", "Whether to crossfade from the old image to the new one, instead of switching instantly."));
                EditorGUILayout.PropertyField(m_lighten_actor, new GUIContent("Lighten actor", "Also lightens the actor when the image changes."));
                EditorGUILayout.PropertyField(m_bring_actor_to_front, new GUIContent("Bring actor to front", "Also brings the actor in front of other actors when the image changes."));
                EditorGUI.indentLevel--;
            }

            serializedObject.ApplyModifiedProperties();
        }

        // Lets the designer pick an expression BY NAME rather than dragging a raw Sprite, shows a live
        // thumbnail of the actual resolved image, and (only when meaningful) a Straight/Side pose toggle.
        // The outfit itself is deliberately NOT chosen here - it lives in the Advanced foldout below, since
        // it's a rare, deliberate choice and mixing it into this popup is exactly what used to make it easy
        // to accidentally switch a character's outfit/hairstyle while just meaning to change expression.
        private void Draw_Expression_And_Pose(ChangeActorImageNode node)
        {
            Actor data = node.actor_data;

            if (data == null)
            {
                EditorGUILayout.HelpBox("Assign an Actor above to choose which expression to switch to.", MessageType.Info);
                return;
            }

            string effective_outfit = string.IsNullOrEmpty(node.outfit_name)
                ? (data.Get_Default_Outfit() != null ? data.Get_Default_Outfit().outfit_name : "")
                : node.outfit_name;

            VNNodeEditorGUI.DrawExpressionPicker(data, effective_outfit, m_expression_name);

            Sprite preview_sprite = data.Get_Sprite(effective_outfit, node.expression_name, node.pose);
            VNNodeEditorGUI.DrawSpritePreview(preview_sprite);

            Draw_Pose_Toggle(data, effective_outfit, node.expression_name);
        }

        // Only shows a Straight/Side toggle when the current expression actually has both poses drawn -
        // otherwise the toggle would just be noise, so the single available pose (or "none") is applied
        // silently and shown as an inline label instead.
        private void Draw_Pose_Toggle(Actor data, string effective_outfit, string expression_name)
        {
            ActorOutfit outfit = data.Get_Outfit(effective_outfit);
            if (outfit == null)
                return;

            bool has_straight = outfit.expressions.Exists(e => e.name == expression_name && e.pose == Pose.Straight);
            bool has_side = outfit.expressions.Exists(e => e.name == expression_name && e.pose == Pose.Side);

            if (has_straight && has_side)
            {
                using (new EditorGUILayout.HorizontalScope())
                {
                    EditorGUILayout.LabelField(new GUIContent("Pose", "Which pose variant of this expression to use."), GUILayout.Width(EditorGUIUtility.labelWidth));

                    int toolbar_index = m_pose.enumValueIndex == (int)Pose.Side ? 1 : 0;
                    EditorGUI.BeginChangeCheck();
                    int new_toolbar_index = GUILayout.Toolbar(toolbar_index, new[] { "Straight", "Side" });
                    if (EditorGUI.EndChangeCheck())
                        m_pose.enumValueIndex = new_toolbar_index == 0 ? (int)Pose.Straight : (int)Pose.Side;
                    else if ((Pose)m_pose.enumValueIndex == Pose.Unspecified)
                        m_pose.enumValueIndex = (int)Pose.Straight;
                }
            }
            else
            {
                Pose only_pose = has_straight ? Pose.Straight : (has_side ? Pose.Side : Pose.Unspecified);
                m_pose.enumValueIndex = (int)only_pose;

                if (only_pose != Pose.Unspecified)
                    EditorGUILayout.LabelField("Pose", "(" + only_pose + " only)");
            }
        }
    }
}
