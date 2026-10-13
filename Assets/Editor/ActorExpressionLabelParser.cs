using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

namespace VNEngine
{
    // Shared heuristic for turning a legacy flat expression/filename label (which historically mashed
    // together emotion, pose, and outfit/style with no consistent convention - "Angry Side", "Braid Happy",
    // "Straight Angry", "Ah Angry Left", "MASTER Side"...) into a best-guess split of outfit/emotion/pose.
    // Used by ActorAssetBuilder when onboarding new characters going forward.
    //
    // This is a best guess only - every caller must let a human confirm/correct the result before
    // committing it to an Actor asset, never write it through unreviewed.
    public static class ActorExpressionLabelParser
    {
        public struct ParsedLabel
        {
            public string suggested_outfit;   // "" when no outfit token was found (i.e. the "Default" outfit)
            public string suggested_emotion;
            public Pose suggested_pose;
            public bool suggested_skip;       // true for likely-junk entries (e.g. "MASTER") - human should confirm deletion
            public string skip_reason;        // why it was flagged, when suggested_skip (or just a caution note)
        }

        private static readonly Dictionary<string, Pose> PoseTokens = new Dictionary<string, Pose>(System.StringComparer.OrdinalIgnoreCase)
        {
            { "straight", Pose.Straight },
            { "stright", Pose.Straight },   // known typo (e.g. Ji-ah's "Resting Talk Hand Stright")
            { "side", Pose.Side },
            { "left", Pose.Side },          // Ji-ah/Leilani use "Left" for the same pose concept as "Side"
        };

        // Connector words that appear alongside an outfit token ("With Sunglasses") but aren't themselves
        // part of the outfit name or the emotion label.
        private static readonly HashSet<string> StopWords = new HashSet<string>(System.StringComparer.OrdinalIgnoreCase)
        {
            "with"
        };

        private static readonly HashSet<string> OutfitTokens = new HashSet<string>(System.StringComparer.OrdinalIgnoreCase)
        {
            "braid", "variant", "sunglasses"
        };

        private static readonly Dictionary<string, string> EmotionSynonyms = new Dictionary<string, string>(System.StringComparer.OrdinalIgnoreCase)
        {
            { "sacred", "Scared" },
        };

        // Tokens that are almost certainly not a real expression (a leftover base-rig image, etc.) -
        // flagged for the human to skip/delete rather than guessed into an outfit or emotion.
        private static readonly HashSet<string> JunkTokens = new HashSet<string>(System.StringComparer.OrdinalIgnoreCase)
        {
            "master"
        };

        // Splits raw text into tokens on '-', '_', ' ' - the same convention ActorAssetBuilder has always
        // used when deriving labels from filenames.
        public static string[] Tokenize(string text)
        {
            return text.Split(new[] { '-', '_', ' ' }, System.StringSplitOptions.RemoveEmptyEntries);
        }

        // Fuzzy-matches a candidate token against a character's name - handles filename/label prefixes
        // that don't exactly match (e.g. "Breeana-*" for the "Breanna" folder, "Ah " for "Ji-ah").
        public static bool Is_Fuzzy_Match(string candidate, string character_name)
        {
            string a = Regex.Replace(candidate, "[^a-zA-Z]", "").ToLowerInvariant();
            string b = Regex.Replace(character_name, "[^a-zA-Z]", "").ToLowerInvariant();
            if (string.IsNullOrEmpty(a) || string.IsNullOrEmpty(b))
                return false;

            if (a.Contains(b) || b.Contains(a))
                return true;

            int distance = Levenshtein(a, b);
            int threshold = Mathf.Max(2, b.Length / 3);
            return distance <= threshold;
        }

        private static int Levenshtein(string a, string b)
        {
            int[,] d = new int[a.Length + 1, b.Length + 1];
            for (int i = 0; i <= a.Length; i++) d[i, 0] = i;
            for (int j = 0; j <= b.Length; j++) d[0, j] = j;

            for (int i = 1; i <= a.Length; i++)
            {
                for (int j = 1; j <= b.Length; j++)
                {
                    int cost = a[i - 1] == b[j - 1] ? 0 : 1;
                    int deletion = d[i - 1, j] + 1;
                    int insertion = d[i, j - 1] + 1;
                    int substitution = d[i - 1, j - 1] + cost;
                    d[i, j] = Mathf.Min(deletion, Mathf.Min(insertion, substitution));
                }
            }
            return d[a.Length, b.Length];
        }

        // Parses one legacy flat label (already stripped of any filename extension / character-name
        // prefix) into a best-guess outfit/emotion/pose split. Token classification is position-independent
        // (handles Sophia's pose-first ordering, e.g. "Straight Angry", the same as everyone else's
        // pose-last ordering) and never authoritative - callers must let a human confirm/correct before
        // committing the result.
        public static ParsedLabel Parse(string raw_label)
        {
            string[] rawTokens = Tokenize(raw_label);

            var emotionTokens = new List<string>();
            string outfit = "";
            Pose pose = Pose.Unspecified;
            bool skip = false;
            var notes = new List<string>();

            foreach (string token in rawTokens)
            {
                if (StopWords.Contains(token))
                    continue;

                if (JunkTokens.Contains(token))
                {
                    skip = true;
                    notes.Add("Looks like junk (\"" + token + "\")");
                    continue;
                }

                bool is_numeric = token.Length > 0;
                foreach (char c in token)
                {
                    if (!char.IsDigit(c)) { is_numeric = false; break; }
                }
                if (is_numeric)
                {
                    // Ambiguous - could be a legit alt-take, an outfit variant, or a duplicate. Flag for a
                    // human decision rather than guessing; keep it in the emotion label in the meantime.
                    notes.Add("Numbered variant (\"" + token + "\") - confirm this isn't a duplicate");
                    emotionTokens.Add(token);
                    continue;
                }

                if (PoseTokens.TryGetValue(token, out Pose matchedPose))
                {
                    pose = matchedPose;
                    continue;
                }

                if (OutfitTokens.Contains(token))
                {
                    outfit = TitleCase(token);
                    continue;
                }

                emotionTokens.Add(EmotionSynonyms.TryGetValue(token, out string synonym) ? synonym : TitleCase(token));
            }

            return new ParsedLabel
            {
                suggested_outfit = outfit,
                suggested_emotion = string.Join(" ", emotionTokens),
                suggested_pose = pose,
                suggested_skip = skip,
                skip_reason = string.Join("; ", notes)
            };
        }

        private static string TitleCase(string token)
        {
            if (string.IsNullOrEmpty(token)) return token;
            return char.ToUpperInvariant(token[0]) + (token.Length > 1 ? token.Substring(1) : "");
        }
    }
}
