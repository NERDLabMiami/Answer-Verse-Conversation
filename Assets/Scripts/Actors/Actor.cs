using System.Collections.Generic;
using UnityEngine;

namespace VNEngine
{
    public enum Pose { Unspecified, Straight, Side }

    [System.Serializable]
    public class ActorExpression
    {
        public string name;
        public Pose pose = Pose.Unspecified;
        public Sprite sprite;
    }

    [System.Serializable]
    public class ActorOutfit
    {
        public string outfit_name = "Default";
        public List<ActorExpression> expressions = new List<ActorExpression>();
    }

    // Pure character data: identity, display name, and the set of useable expression images,
    // grouped by outfit/style so a designer picking an expression can't accidentally also switch outfits.
    // The on-stage runtime representation lives on ActorInstance, which references this asset.
    [CreateAssetMenu(fileName = "New Actor", menuName = "VNEngine/Actor")]
    public class Actor : ScriptableObject
    {
        public string actor_name;       // Stable lookup key - used to reference this actor in dialogue (CSV/Stat sources) and for save features
        public string display_name;     // Optional. Human-facing name shown in the speaker panel - falls back to actor_name when empty.
        public List<ActorOutfit> outfits = new List<ActorOutfit>();
        public int default_outfit_index = 0;
        public int default_expression_index = 0;   // indexes into outfits[default_outfit_index].expressions

        // The name to print in the speaker panel: display_name if set, otherwise falls back to actor_name
        public string Get_Display_Name()
        {
            return string.IsNullOrEmpty(display_name) ? actor_name : display_name;
        }

        public ActorOutfit Get_Default_Outfit()
        {
            if (outfits.Count == 0)
                return null;

            int index = Mathf.Clamp(default_outfit_index, 0, outfits.Count - 1);
            return outfits[index];
        }

        // Resolves an outfit by name, falling back to the default outfit for an empty/unmatched name -
        // this is what lets ChangeActorImageNode leave outfit_name blank to mean "whatever's active."
        public ActorOutfit Get_Outfit(string outfit_name)
        {
            if (string.IsNullOrEmpty(outfit_name))
                return Get_Default_Outfit();

            ActorOutfit match = outfits.Find(o => o.outfit_name == outfit_name);
            return match != null ? match : Get_Default_Outfit();
        }

        public Sprite Get_Default_Sprite()
        {
            ActorOutfit outfit = Get_Default_Outfit();
            if (outfit == null || outfit.expressions.Count == 0)
                return null;

            int index = Mathf.Clamp(default_expression_index, 0, outfit.expressions.Count - 1);
            return outfit.expressions[index].sprite;
        }

        // Primary sprite lookup. Falls back gracefully when the requested pose isn't available for this
        // expression (most expressions only have one pose, or none) rather than failing outright.
        public Sprite Get_Sprite(string outfit_name, string expression_name, Pose pose)
        {
            ActorOutfit outfit = Get_Outfit(outfit_name);
            if (outfit == null)
                return null;

            List<ActorExpression> candidates = outfit.expressions.FindAll(e => e.name == expression_name);
            if (candidates.Count == 0)
                return null;

            ActorExpression exact = candidates.Find(e => e.pose == pose);
            if (exact != null)
                return exact.sprite;

            ActorExpression unspecified = candidates.Find(e => e.pose == Pose.Unspecified);
            if (unspecified != null)
                return unspecified.sprite;

            return candidates[0].sprite;
        }

        // Whether this expression has distinct art for both poses within the given outfit - used by the
        // node editor to decide whether a pose toggle is meaningful to show at all.
        public bool Expression_Has_Both_Poses(string outfit_name, string expression_name)
        {
            ActorOutfit outfit = Get_Outfit(outfit_name);
            if (outfit == null)
                return false;

            bool has_straight = outfit.expressions.Exists(e => e.name == expression_name && e.pose == Pose.Straight);
            bool has_side = outfit.expressions.Exists(e => e.name == expression_name && e.pose == Pose.Side);
            return has_straight && has_side;
        }

        // Distinct expression names available within an outfit, for the node editor's expression popup.
        public List<string> Get_Expression_Names(string outfit_name)
        {
            List<string> names = new List<string>();

            ActorOutfit outfit = Get_Outfit(outfit_name);
            if (outfit == null)
                return names;

            foreach (ActorExpression expression in outfit.expressions)
            {
                if (!names.Contains(expression.name))
                    names.Add(expression.name);
            }

            return names;
        }
    }
}
