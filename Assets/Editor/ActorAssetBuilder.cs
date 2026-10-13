using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace VNEngine
{
    // Bulk-populates each character's Actor ScriptableObject expression list from their sprite folder
    // (Assets/Characters/<Name>/). Best-effort: sprite folder naming is inconsistent across the roster
    // (different separators, typos, cross-character contamination) so every generated asset should be
    // treated as a starting point for designer review, not a final answer - the report this prints
    // flags the cases most likely to need a manual pass.
    public static class ActorAssetBuilder
    {
        private const string CharactersRoot = "Assets/Characters";
        private const string ActorsRoot = "Assets/Resources/Actors";

        private static readonly string[] KnownExpressionWords =
            { "neutral", "happy", "sad", "angry", "confused", "surprised", "scared", "resting", "default", "smile", "laugh", "talk" };

        private class CharacterReport
        {
            public string character_name;
            public int expression_count;
            public List<string> skipped_files = new List<string>();
            public List<string> flagged_labels = new List<string>();
            public bool default_cross_referenced;
            public bool already_migrated;   // asset already has structured outfit/pose data - left untouched
        }

        [MenuItem("VNEngine/Writing/Build Actor Assets From Character Folders")]
        private static void Run()
        {
            if (!AssetDatabase.IsValidFolder(CharactersRoot))
            {
                Debug.LogError("No folder found at " + CharactersRoot);
                return;
            }

            // actor_name -> default sprite guid, read straight from the old prefabs' root Image
            // (distinguished from the FadingImage child by m_PreserveAspect: 1, confirmed true on every
            // root Image and false on every FadingImage across all 11 existing prefabs)
            Dictionary<string, string> old_default_sprite_guid_by_name = Build_Old_Prefab_Default_Sprite_Lookup();

            var reports = new List<CharacterReport>();

            foreach (string folder in Directory.GetDirectories(CharactersRoot))
            {
                string character_folder_name = Path.GetFileName(folder);
                reports.Add(Build_Actor_Asset_For_Folder(folder, character_folder_name, old_default_sprite_guid_by_name));
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Print_Report(reports);
        }

        private static Dictionary<string, string> Build_Old_Prefab_Default_Sprite_Lookup()
        {
            var result = new Dictionary<string, string>();
            if (!Directory.Exists(ActorsRoot))
                return result;

            foreach (string prefab_path in Directory.GetFiles(ActorsRoot, "*.prefab"))
            {
                string text = File.ReadAllText(prefab_path);
                string[] blocks = Regex.Split(text, "(?=\\n--- !u!)");

                string actor_name = null;
                string default_sprite_guid = null;

                foreach (string block in blocks)
                {
                    if (actor_name == null)
                    {
                        Match name_match = Regex.Match(block, @"(?<![A-Za-z_])actor_name:\s*(.*)");
                        if (name_match.Success)
                            actor_name = name_match.Groups[1].Value.Trim();
                    }

                    if (block.Contains("m_PreserveAspect: 1"))
                    {
                        Match sprite_match = Regex.Match(block, @"m_Sprite:\s*\{fileID:\s*\d+,\s*guid:\s*([0-9a-fA-F]+)");
                        if (sprite_match.Success)
                            default_sprite_guid = sprite_match.Groups[1].Value;
                    }
                }

                if (!string.IsNullOrEmpty(actor_name))
                    result[actor_name] = default_sprite_guid; // may be null if the prefab had no sprite assigned
            }

            return result;
        }

        private static CharacterReport Build_Actor_Asset_For_Folder(string folder_path, string character_folder_name,
            Dictionary<string, string> old_default_sprite_guid_by_name)
        {
            var report = new CharacterReport { character_name = character_folder_name };

            string asset_path = ActorsRoot + "/" + character_folder_name + ".asset";
            Actor data = AssetDatabase.LoadAssetAtPath<Actor>(asset_path);
            bool is_new = data == null;
            if (is_new)
                data = ScriptableObject.CreateInstance<Actor>();

            // Never clobber a character that's already been through outfit/pose review (via the migration
            // tool, or hand-edited) - only populate brand-new characters this tool hasn't touched yet.
            if (!is_new && data.outfits != null && data.outfits.Count > 0)
            {
                report.already_migrated = true;
                return report;
            }

            data.actor_name = character_folder_name;
            var outfits_by_name = new Dictionary<string, ActorOutfit>();

            old_default_sprite_guid_by_name.TryGetValue(character_folder_name, out string default_sprite_guid);
            string default_outfit_name = null;
            int default_index = 0;
            bool found_default = false;

            foreach (string file_path in Directory.GetFiles(folder_path))
            {
                string asset_relative_path = file_path.Replace('\\', '/');

                // .meta companions are an expected, uninteresting sidecar of every asset - skip them
                // silently rather than reporting each one as "not a .png" noise
                if (asset_relative_path.EndsWith(".meta", System.StringComparison.OrdinalIgnoreCase))
                    continue;

                string ext = Path.GetExtension(asset_relative_path);
                string file_name_no_ext = Path.GetFileNameWithoutExtension(asset_relative_path);
                string display_file_name = Path.GetFileName(asset_relative_path);

                if (!ext.Equals(".png", System.StringComparison.OrdinalIgnoreCase))
                {
                    report.skipped_files.Add(display_file_name + " (not a .png)");
                    continue;
                }

                ActorExpressionLabelParser.ParsedLabel? parsed = Derive_Expression_Label(file_name_no_ext, character_folder_name);
                if (parsed == null)
                {
                    report.skipped_files.Add(display_file_name + " (couldn't match this character's name - likely belongs to another character)");
                    continue;
                }

                Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(asset_relative_path);
                if (sprite == null)
                {
                    report.skipped_files.Add(display_file_name + " (not imported as a Sprite)");
                    continue;
                }

                ActorExpressionLabelParser.ParsedLabel p = parsed.Value;

                if (!string.IsNullOrEmpty(p.skip_reason))
                    report.flagged_labels.Add(display_file_name + " -> \"" + p.suggested_emotion + "\" (" + p.skip_reason + ")");
                else if (Looks_Suspicious(p.suggested_emotion))
                    report.flagged_labels.Add(display_file_name + " -> \"" + p.suggested_emotion + "\"");

                string outfit_key = string.IsNullOrEmpty(p.suggested_outfit) ? "Default" : p.suggested_outfit;
                if (!outfits_by_name.TryGetValue(outfit_key, out ActorOutfit outfit))
                {
                    outfit = new ActorOutfit { outfit_name = outfit_key };
                    outfits_by_name[outfit_key] = outfit;
                }

                if (!found_default && default_sprite_guid != null)
                {
                    string guid = AssetDatabase.AssetPathToGUID(asset_relative_path);
                    if (guid == default_sprite_guid)
                    {
                        default_outfit_name = outfit_key;
                        default_index = outfit.expressions.Count;
                        found_default = true;
                    }
                }

                outfit.expressions.Add(new ActorExpression { name = p.suggested_emotion, pose = p.suggested_pose, sprite = sprite });
            }

            data.outfits = new List<ActorOutfit>(outfits_by_name.Values);
            data.outfits.Sort((x, y) =>
            {
                if (x.outfit_name == "Default") return -1;
                if (y.outfit_name == "Default") return 1;
                return string.Compare(x.outfit_name, y.outfit_name, System.StringComparison.OrdinalIgnoreCase);
            });

            data.default_outfit_index = found_default ? data.outfits.FindIndex(o => o.outfit_name == default_outfit_name) : 0;
            data.default_expression_index = found_default ? default_index : 0;
            report.default_cross_referenced = found_default;

            int total_expressions = 0;
            foreach (ActorOutfit o in data.outfits)
                total_expressions += o.expressions.Count;
            report.expression_count = total_expressions;

            if (is_new)
                AssetDatabase.CreateAsset(data, asset_path);
            else
                EditorUtility.SetDirty(data);

            return report;
        }

        // Derives a best-guess outfit/emotion/pose split from a filename by fuzzy-stripping the leading
        // character-name token - handles filename prefixes that don't exactly match the folder name
        // (e.g. "Breeana-*" for the "Breanna" folder, "Sofia-*" for "Sophia", "DrPurnell_-_*" for
        // "Purnell") - then hands the remainder to the shared ActorExpressionLabelParser (also used by the
        // one-time outfit/pose migration tool, so filename-derived and migrated labels stay consistent).
        // Returns null when the filename's leading token doesn't plausibly belong to this character.
        private static ActorExpressionLabelParser.ParsedLabel? Derive_Expression_Label(string file_name_no_ext, string character_folder_name)
        {
            int boundary = 0;
            while (boundary < file_name_no_ext.Length && char.IsLetterOrDigit(file_name_no_ext[boundary]))
                boundary++;

            string candidate_prefix = file_name_no_ext.Substring(0, boundary);

            int remainder_start = boundary;
            while (remainder_start < file_name_no_ext.Length && !char.IsLetterOrDigit(file_name_no_ext[remainder_start]))
                remainder_start++;

            if (!ActorExpressionLabelParser.Is_Fuzzy_Match(candidate_prefix, character_folder_name))
                return null;

            string remainder = file_name_no_ext.Substring(remainder_start);
            if (string.IsNullOrWhiteSpace(remainder))
                remainder = "Default";

            return ActorExpressionLabelParser.Parse(remainder);
        }

        // Flags labels that don't contain a recognizable expression word - catches junk tokens like
        // "MASTER", talk-cycle numbers, or near-duplicate variants that need a human glance.
        private static bool Looks_Suspicious(string label)
        {
            string lower = label.ToLowerInvariant();
            foreach (string word in KnownExpressionWords)
            {
                if (lower.Contains(word))
                    return false;
            }
            return true;
        }

        private static void Print_Report(List<CharacterReport> reports)
        {
            var sb = new StringBuilder();
            sb.AppendLine("Actor Asset Builder - " + reports.Count + " character folder(s) processed.");
            sb.AppendLine();

            foreach (CharacterReport r in reports)
            {
                sb.AppendLine("== " + r.character_name + " ==");

                if (r.already_migrated)
                {
                    sb.AppendLine("  Already has structured outfit/pose data - skipped (delete its outfits to force a re-scan).");
                    sb.AppendLine();
                    continue;
                }

                sb.AppendLine("  " + r.expression_count + " expression(s) added. Default " +
                    (r.default_cross_referenced ? "cross-referenced from the old prefab." : "NOT found - defaulted to the first expression, please check."));

                if (r.skipped_files.Count > 0)
                {
                    sb.AppendLine("  Skipped " + r.skipped_files.Count + " file(s):");
                    foreach (string s in r.skipped_files) sb.AppendLine("    - " + s);
                }
                if (r.flagged_labels.Count > 0)
                {
                    sb.AppendLine("  Flagged " + r.flagged_labels.Count + " label(s) for review (no recognizable expression word):");
                    foreach (string s in r.flagged_labels) sb.AppendLine("    - " + s);
                }
                sb.AppendLine();
            }

            Debug.Log("[ActorAssetBuilder]\n" + sb);
            EditorUtility.DisplayDialog("Actor Asset Builder",
                "Done - see Console for the full per-character report.\n\n" + reports.Count + " character folder(s) processed.",
                "OK");
        }
    }
}
