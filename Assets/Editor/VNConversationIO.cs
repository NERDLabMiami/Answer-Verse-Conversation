// VNConversationIO.cs
// Editor utility: clean JSON export + patch/match import for VNEngine conversations.

#if UNITY_EDITOR
using System;
using System.Collections;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;
// --- Minimal JSON writer that skips nulls and empty lists/dicts (editor-only) ---
static class MiniJson
{
    public static string Write(object obj, int indent = 0)
    {
        var sb = new System.Text.StringBuilder();
        WriteValue(sb, obj, indent, true);
        return sb.ToString();
    }

    static void WriteValue(System.Text.StringBuilder sb, object v, int indent, bool topLevel)
    {
        switch (v)
        {
            case null:
                sb.Append("null");
                return;
            case string s:
                sb.Append('"').Append(Escape(s)).Append('"');
                return;
            case bool b:
                sb.Append(b ? "true" : "false");
                return;
            case float f:
                sb.Append(f.ToString(System.Globalization.CultureInfo.InvariantCulture));
                return;
            case double d:
                sb.Append(d.ToString(System.Globalization.CultureInfo.InvariantCulture));
                return;
            case int i:
                sb.Append(i);
                return;
            case long l:
                sb.Append(l);
                return;
            case IEnumerable<string> sa:
                {
                    var list = sa.ToList();
                    if (list.Count == 0) { sb.Append("[]"); return; }
                    sb.Append('[');
                    for (int x = 0; x < list.Count; x++)
                    {
                        if (x > 0) sb.Append(", ");
                        sb.Append('"').Append(Escape(list[x] ?? "")).Append('"');
                    }
                    sb.Append(']');
                    return;
                }
            case IEnumerable<object> oa:
                {
                    var list = oa.ToList();
                    if (list.Count == 0) { sb.Append("[]"); return; }
                    sb.Append('[');
                    for (int x = 0; x < list.Count; x++)
                    {
                        if (x > 0) sb.Append(", ");
                        WriteValue(sb, list[x], indent + 2, false);
                    }
                    sb.Append(']');
                    return;
                }
            case Dictionary<string, object> dict:
                {
                    // Skip empties at top-level values
                    var nonNull = dict.Where(kv => !IsNullish(kv.Value)).ToList();
                    sb.Append('{');
                    if (nonNull.Count > 0)
                    {
                        int i2 = 0;
                        foreach (var kv in nonNull)
                        {
                            if (i2++ > 0) sb.Append(", ");
                            sb.Append('"').Append(Escape(kv.Key)).Append("\": ");
                            WriteValue(sb, kv.Value, indent + 2, false);
                        }
                    }
                    sb.Append('}');
                    return;
                }
            default:
                // numbers or other simple types
                if (v is float || v is double || v is int || v is long) { WriteValue(sb, v.ToString(), indent, topLevel); return; }
                // fallback: ToString as string
                sb.Append('"').Append(Escape(v.ToString())).Append('"');
                return;
        }
    }

    static string Escape(string s) => s
        .Replace("\\", "\\\\")
        .Replace("\"", "\\\"")
        .Replace("\n", "\\n")
        .Replace("\r", "\\r")
        .Replace("\t", "\\t");

    static bool IsNullish(object v)
    {
        if (v == null) return true;
        if (v is string s) return false; // keep empty strings as-is (human-editable)
        if (v is IList<object> list) return list.Count == 0;
        if (v is IEnumerable<string> sa) return !sa.Any();
        if (v is Dictionary<string, object> d) return d.Count == 0;
        return false;
    }
    
}

namespace VNEngine.EditorTools
{
    // ---- Stable Node ID ------------------------------------------------------
    [DisallowMultipleComponent]
    public class NodeExportTag : MonoBehaviour
    {
        [SerializeField] private string _id;
        public string Id => _id;

        private void OnValidate()
        {
            if (string.IsNullOrEmpty(_id))
                _id = GUID.Generate().ToString();
        }

        [ContextMenu("Regenerate Export ID (Danger)")]
        private void Regenerate() => _id = GUID.Generate().ToString();
    }

    // ==== LEGACY STRUCTS (back-compat import) =================================
    [Serializable] public class ExportedConversation
    {
        public string conversationName;
        public List<ExportedNode> nodes = new List<ExportedNode>();
        public string exportedAt;
        public string exporterVersion = "1.0";
    }
    [Serializable] public class ExportedNode
    {
        public string id;
        public string type;
        public string goName;
        public int order;
        public List<FieldEntry> fields = new List<FieldEntry>();
    }

    // FieldEntry shape is defined by VNEngine; mirror it exactly:
    [Serializable] public class FieldEntry
    {
        public string name;
        public string kind;        // Primitive, Enum, String, Asset, Array, Other
        public string type;        // AssemblyQualifiedName
        public string value;
        public string assetGuid;
        public string assetType;   // AssemblyQualifiedName for asset type
        public string[] stringArray;
    }

    // ==== CLEAN STRUCTS (new export/import) ===================================
    [Serializable] class ConversationExport
    {
        public string conversationName;
        public string exportedAt;
        public List<NodeRecord> nodes = new();
        public string exporterVersion = "clean-1.1";
    }

    [Serializable] class NodeRecord
    {
        public string id;
        public string type;
        public string goName;
        public int order;

        // One of these will be non-null, depending on `type`
        public DialoguePayload dialogue;
        public ChoicePayload choice;
        public ChangeActorImagePayload changeActorImage;
        public SetBackgroundPayload setBackground;
        public EnterActorPayload enterActor;
        public ShowChoicePayload showChoice;
    }

    // ---- Payloads ------------------------------------------------------------

    [Serializable] class ChoicePayload
    {
        public bool localize;
        public List<ChoiceItem> choices = new();
    }
    [Serializable] class ChoiceItem
    {
        public string id;            // nodeGuid#index
        public string key;           // Button_Text[i]
        public string text;          // resolved or literal
        public string disabledText;  // Disabled_Text[i]
    }

    [Serializable] class DialoguePayload
    {
        public string actorKey;
        public string actorKeySource;   // Dialogue_Source enum name
        public string speakerTitle;     // textbox_title
        public string dialogueKey;      // localized_key (if used)
        public string dialogueKeySource;// dialogue_from enum name
        public string text;             // literal or resolved
        public bool bringToFront;
        public bool darkenOthers;
    }

    [Serializable] class ChangeActorImagePayload
    {
        public string actorName;
        public string expressionName; // name of the expression (emotion only, no pose/outfit) to switch to
        public string outfitName;     // "" = whatever outfit is currently active/default
        public string pose;           // Pose enum name ("Unspecified"/"Straight"/"Side")
        public bool fadeIn;
        public bool lightenActor;
        public bool bringToFront;
    }

    [Serializable] class SetBackgroundPayload
    {
        public string imageGuid;
        public string imageFile;
        public bool setForeground;
        public bool fadeIn;
        public bool fadeOut;
        public string fadeStyle;   // enum name of SetBackground.Fade_Style
        public float fadeDuration;
    }

    [Serializable] class EnterActorPayload
    {
        public string actorName;
        public string actorNameSource; // Dialogue_Source enum name
        public string entranceType;    // Entrance_Type enum name
        public float fadeInTime;
        public string destination;     // Actor_Positions enum name
    }

    [Serializable] class ShowChoicePayload
    {
        public bool hideDialogueUI;
        public List<ShowChoiceItem> choices = new();
    }
    [Serializable] class ShowChoiceItem
    {
        public string id;                  // nodeGuid#index
        public string text;
        public string nextConversationName;// resolved to a ConversationManager by name on import; null => continue current
        public List<ShowChoiceRequirement> requirements = new();
        public string label, category, variant;
        public bool enableLogging;
        // buttonModifier (an asset reference) is intentionally not round-tripped via CSV —
        // there's no simple textual key for it.
    }
    [Serializable] class ShowChoiceRequirement
    {
        public string traitKey;
        public string compare; // NumberCompare enum name
        public float value;
    }

    // ==== WINDOW ==============================================================
    // AnswerVerse VNEngine's writing pipeline. Lets a writer round-trip a conversation's
    // node tree to/from CSV: export one conversation or a whole folder of them for editing
    // in a spreadsheet, then re-import to patch the scene's nodes to match. NodeExportTag
    // (below) gives each exported node a stable ID so patch imports match nodes correctly
    // even after reordering.
    public class VNConversationIOWindow : EditorWindow
    {
        [MenuItem("VNEngine/Writing/Conversation IO")]
        private static void Open() => GetWindow<VNConversationIOWindow>(true, "Conversation IO");

        [SerializeField] private GameObject conversationRoot;
        [SerializeField] private TextAsset jsonToImport;
        [SerializeField] private bool removeMissingOnImport = false;

        Vector2 _scroll;

   // 2) Update OnGUI() to show the new button when a set root is selected
void OnGUI()
{
    _scroll = EditorGUILayout.BeginScrollView(_scroll);
    EditorGUILayout.Space();
    EditorGUILayout.LabelField("VNEngine Conversation IO", EditorStyles.boldLabel);
    EditorGUILayout.HelpBox(
        "Select either:\n• A single Conversation root (has ConversationManager + child Nodes), or\n• A parent whose direct children are ConversationManager objects.",
        MessageType.Info);

    conversationRoot =
        (GameObject)EditorGUILayout.ObjectField("Selected Root", conversationRoot, typeof(GameObject), true);
    removeMissingOnImport = EditorGUILayout.ToggleLeft("Remove scene nodes not present in JSON (destructive)", removeMissingOnImport);

    bool isSingle = IsValidConversationRoot(conversationRoot);
    bool isSet    = IsValidConversationSetRoot(conversationRoot);

    using (new EditorGUI.DisabledScope(!(isSingle || isSet)))
    {
        EditorGUILayout.Space();

        // Single-conversation tools (existing)
        using (new EditorGUI.DisabledScope(!isSingle))
        {
            if (GUILayout.Button("Export Unified CSV (1 sheet)…", GUILayout.Height(24)))
            {
                if (!IsValidConversationRoot(conversationRoot)) return;
                var path = EditorUtility.SaveFilePanel("Export Unified CSV", Application.dataPath, conversationRoot.name + "_nodes", "csv");
                if (!string.IsNullOrEmpty(path))
                    ExportUnifiedCsv(conversationRoot, path);
            }

            if (GUILayout.Button("Import Unified CSV (Patch)…", GUILayout.Height(24)))
            {
                if (!IsValidConversationRoot(conversationRoot)) return;
                var path = EditorUtility.OpenFilePanel("Import Unified CSV", Application.dataPath, "csv");
                if (!string.IsNullOrEmpty(path))
                {
                    Undo.RegisterFullObjectHierarchyUndo(conversationRoot, "Patch Unified CSV");
                    ImportUnifiedCsv(conversationRoot, path);
                }
            }
        }

        // NEW: Conversation set export
        using (new EditorGUI.DisabledScope(!isSet))
        {
            if (GUILayout.Button("Export All Child Conversations (CSV Folder)…", GUILayout.Height(24)))
            {
                var folder = EditorUtility.SaveFolderPanel("Export Conversations CSVs", Application.dataPath, conversationRoot.name + "_CSVs");
                if (!string.IsNullOrEmpty(folder))
                {
                    ExportAllChildConversationsCsv(conversationRoot, folder);
                }
            }
        }

        EditorGUILayout.Space();
        if (GUILayout.Button("Remove Export Tags Under Selected Root (Release Cleanup)", GUILayout.Height(24)))
        {
            int removed = RemoveExportTags(conversationRoot);
            if (removed > 0)
                UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(conversationRoot.scene);
            Debug.Log($"[VNConversationIO] Removed {removed} NodeExportTag(s) under '{conversationRoot.name}'.");
        }
    }

    EditorGUILayout.EndScrollView();
}

// 1) Add these helpers near IsValidConversationRoot(...)
        static bool IsValidConversationSetRoot(GameObject go)
        {
            if (!go) return false;
            // "Set" = at least one ConversationManager somewhere in the subtree, at any depth.
            // Conversations in this project are nested inside purely organizational folder
            // GameObjects (e.g. Conversations > Example - Trait Gating > Example 1 - Groundhog Day
            // Continued > Example 1 - Groundhog Day Awareness), not always as direct children.
            return go.GetComponentsInChildren<VNEngine.ConversationManager>(true)
                     .Any(cm => cm.gameObject != go);
        }

        static bool HasAnyNodeChild(Transform t)
        {
            // must have at least one direct child Node (or deeper)
            return t.GetComponentsInChildren<VNEngine.Node>(true).Length > 0;
        }

        static void ExportAllChildConversationsCsv(GameObject setRoot, string folderPath)
        {
            if (string.IsNullOrEmpty(folderPath)) return;
            if (!System.IO.Directory.Exists(folderPath))
                System.IO.Directory.CreateDirectory(folderPath);

            // Walk the whole subtree rather than only direct children - conversations can sit
            // several folder levels below setRoot (see IsValidConversationSetRoot above).
            var conversationManagers = setRoot
                .GetComponentsInChildren<VNEngine.ConversationManager>(true)
                .OrderBy(cm => cm.transform.GetSiblingIndex())
                .ToList();

            int exported = 0, skipped = 0;
            var usedNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var cm in conversationManagers)
            {
                var child = cm.transform;

                // Avoid exporting empty shells
                if (!HasAnyNodeChild(child))
                {
                    skipped++;
                    continue;
                }

                // Reuse the existing per-conversation exporter so we keep behavior consistent
                var safeName = child.name.Replace(Path.DirectorySeparatorChar, '_').Replace(Path.AltDirectorySeparatorChar, '_');
                var fileBase = safeName;
                int suffix = 1;
                while (!usedNames.Add(fileBase))
                    fileBase = safeName + "_" + (++suffix); // guard against same-named conversations in different folders

                var outPath = Path.Combine(folderPath, fileBase + "_nodes.csv");

                try
                {
                    ExportUnifiedCsv(child.gameObject, outPath);
                    exported++;
                }
                catch (Exception ex)
                {
                    Debug.LogError($"Failed to export CSV for {child.name}: {ex.Message}", child.gameObject);
                }
            }

            Debug.Log($"Conversation set export complete. Exported: {exported}, Skipped (no nodes): {skipped} → {folderPath}");
            EditorUtility.RevealInFinder(folderPath);
        }

        static bool IsValidConversationRoot(GameObject go)
{
    if (!go) return false;
    // Must be a real conversation root: has a ConversationManager
    // and at least one direct child Node.
    return go.GetComponent<VNEngine.ConversationManager>() != null
           && go.transform.Cast<Transform>().Any(t => t.GetComponent<VNEngine.Node>() != null);
}

        // ==== PAYLOAD BUILDERS ================================================
        static ShowChoicePayload BuildShowChoicePayload(VNEngine.ShowChoiceNode n, string nodeGuid)
        {
            var payload = new ShowChoicePayload
            {
                hideDialogueUI = GetBool(n, "hideDialogueUI")
            };

            for (int i = 0; i < n.choices.Count; i++)
            {
                var c = n.choices[i];
                var item = new ShowChoiceItem
                {
                    id = $"{nodeGuid}#{i}",
                    text = c.text,
                    nextConversationName = c.nextConversation ? c.nextConversation.name : null,
                    label = c.label,
                    category = c.category,
                    variant = c.variant,
                    enableLogging = c.enableLogging
                };
                foreach (var r in c.requirements)
                {
                    item.requirements.Add(new ShowChoiceRequirement
                    {
                        traitKey = r.ResolveKey(),
                        compare = Enum.GetName(typeof(NumberCompare), r.compare),
                        value = r.value
                    });
                }
                payload.choices.Add(item);
            }

            return payload;
        }

        static DialoguePayload BuildDialoguePayload(VNEngine.DialogueNode dn)
        {
            string actorName = GetString(dn, "actor");
            string actorSource = GetEnumName(dn, "actor_name_from");
            string speaker = GetString(dn, "textbox_title");
            string key = GetString(dn, "localized_key");
            string keySource = GetEnumName(dn, "dialogue_from");
            string text = GetString(dn, "text");

            bool bringFront = GetBool(dn, "bring_speaker_to_front");
            bool darken = GetBool(dn, "darken_all_other_characters");

            return new DialoguePayload
            {
                actorKey = actorName,
                actorKeySource = actorSource,
                speakerTitle = speaker,
                dialogueKey = key,
                dialogueKeySource = keySource,
                text = text,
                bringToFront = bringFront,
                darkenOthers = darken
            };
        }

        static ChoicePayload BuildChoicePayload(VNEngine.ChoiceNode cn, string nodeGuid)
        {
            var payload = new ChoicePayload
            {
                localize = GetBool(cn, "Localize_Choice_Text")
            };

            int n = GetInt(cn, "Number_Of_Choices");
            var keys = GetStringArray(cn, "Button_Text");
            var disabled = GetStringArray(cn, "Disabled_Text");

            for (int i = 0; i < n; i++)
            {
                string k = (keys != null && i < keys.Length) ? keys[i] : "";
                string d = (disabled != null && i < disabled.Length) ? disabled[i] : "";
                string resolved = k;

                if (payload.localize && !string.IsNullOrEmpty(k) && VNEngine.VNSceneManager.scene_manager != null)
                    resolved = VNEngine.VNSceneManager.scene_manager.Get_Localized_Dialogue_Entry(k);

                payload.choices.Add(new ChoiceItem
                {
                    id = $"{nodeGuid}#{i}",
                    key = k,
                    text = resolved,
                    disabledText = d
                });
            }

            return payload;
        }

        static ChangeActorImagePayload BuildChangeActorImagePayload(VNEngine.ChangeActorImageNode n)
        {
            return new ChangeActorImagePayload
            {
                actorName = GetString(n, "actor_name"),
                expressionName = GetString(n, "expression_name"),
                outfitName = GetString(n, "outfit_name"),
                pose = GetEnumName(n, "pose"),
                fadeIn = GetBool(n, "fade_in_new_image"),
                lightenActor = GetBool(n, "lighten_actor"),
                bringToFront = GetBool(n, "bring_actor_to_front")
            };
        }

        static SetBackgroundPayload BuildSetBackgroundPayload(object n /*SetBackground*/)
        {
            var (guid, file) = AssetGuidAndFile(n, "sprite");
            return new SetBackgroundPayload
            {
                imageGuid = guid,
                imageFile = file,
                setForeground = GetBool(n, "set_foreground"),
                fadeIn = GetBool(n, "fade_in"),
                fadeOut = GetBool(n, "fade_out"),
                fadeStyle = GetEnumName(n, "fade_style"),
                fadeDuration = GetFloat(n, "fade_duration")
            };
        }

        static EnterActorPayload BuildEnterActorPayload(VNEngine.EnterActorNode n)
        {
            return new EnterActorPayload
            {
                actorName = GetString(n, "actor_name"),
                actorNameSource = GetEnumName(n, "actor_name_from"),
                entranceType = GetEnumName(n, "entrance_type"),
                fadeInTime = GetFloat(n, "fade_in_time"),
                destination = GetEnumName(n, "destination")
            };
        }

        // ==== EXPORT ==========================================================
        static void EnsureIds(GameObject root)
        {
            foreach (Transform child in root.transform)
            {
                if (!child.GetComponent<VNEngine.Node>()) continue;
                if (!child.GetComponent<NodeExportTag>()) child.gameObject.AddComponent<NodeExportTag>();
            }
        }

        // ==== RELEASE CLEANUP =================================================
        // NodeExportTag is declared in this Editor-only file, so it doesn't exist in player builds -
        // any GameObject still carrying one when a scene ships shows up as a "missing script" warning
        // at runtime. Strip them before cutting a release build; running the writing tools again
        // afterward just re-adds fresh tags (with new IDs) as needed, so this is safe to run repeatedly.
        static int RemoveExportTags(GameObject root)
        {
            var tags = root.GetComponentsInChildren<NodeExportTag>(true);
            foreach (var tag in tags)
                Undo.DestroyObjectImmediate(tag);
            return tags.Length;
        }

        [MenuItem("VNEngine/Writing/Remove Export Tags From All Open Scenes (Release Cleanup)")]
        private static void RemoveExportTagsFromAllOpenScenes()
        {
            int sceneCount = UnityEngine.SceneManagement.SceneManager.sceneCount;
            int totalRemoved = 0;

            for (int i = 0; i < sceneCount; i++)
            {
                var scene = UnityEngine.SceneManagement.SceneManager.GetSceneAt(i);
                if (!scene.isLoaded) continue;

                int removedInScene = 0;
                foreach (var rootGo in scene.GetRootGameObjects())
                    removedInScene += RemoveExportTags(rootGo);

                if (removedInScene > 0)
                {
                    UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(scene);
                    totalRemoved += removedInScene;
                    Debug.Log($"[VNConversationIO] Removed {removedInScene} NodeExportTag(s) from scene '{scene.name}'.");
                }
            }

            if (totalRemoved == 0)
                Debug.Log("[VNConversationIO] No NodeExportTag components found in any open scene.");
            else
                EditorUtility.DisplayDialog("Remove Export Tags",
                    $"Removed {totalRemoved} NodeExportTag component(s) across the open scene(s).\n\nRemember to save before building.",
                    "OK");
        }


 
// --- CSV helpers ---

    static string CsvEscape(string s)
{
    if (s == null) return "";
    bool needs = s.IndexOfAny(new[] { ',', '"', '\n', '\r' }) >= 0;
    if (!needs) return s;
    return "\"" + s.Replace("\"", "\"\"") + "\"";
}

    const char ChoiceSep = '|';

    static bool Has(string s) => !string.IsNullOrWhiteSpace(s);

    static string[] SplitChoices(string s)
{   
    if (!Has(s)) return Array.Empty<string>();
    return s.Split(new[] { ChoiceSep }, StringSplitOptions.None)
        .Select(x => x.Trim())
        .Where(x => x.Length > 0)
        .ToArray();
}


// Split/join with '|'
    static string JoinPipe(IEnumerable<string> arr) => arr == null ? "" : string.Join("|", arr.Select(x => x ?? ""));
    static string[] SplitPipe(string s) => string.IsNullOrEmpty(s) ? Array.Empty<string>() : s.Split('|');

// ShowChoiceNode requirement cell: multiple "traitKey:compare:value" triples per choice, joined with ';'
    static string JoinRequirements(ShowChoiceItem item)
        => string.Join(";", item.requirements.Select(r => $"{r.traitKey}:{r.compare}:{r.value.ToString(System.Globalization.CultureInfo.InvariantCulture)}"));

    static List<VNEngine.FlexibleTraitRequirement> ParseRequirements(string cell)
    {
        var list = new List<VNEngine.FlexibleTraitRequirement>();
        if (string.IsNullOrWhiteSpace(cell)) return list;
        foreach (var triple in cell.Split(';'))
        {
            if (string.IsNullOrWhiteSpace(triple)) continue;
            var parts = triple.Split(':');
            if (parts.Length < 3) continue;
            if (!float.TryParse(parts[2], System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out var val))
                continue;
            if (!Enum.TryParse<VNEngine.NumberCompare>(parts[1], out var compare))
                continue;
            list.Add(new VNEngine.FlexibleTraitRequirement { traitKey = parts[0], compare = compare, value = val });
        }
        return list;
    }

// Minimal CSV line parser (handles quotes)
    static List<string> ParseCsvLine(string line)
    {
    var cells = new List<string>();
    if (string.IsNullOrEmpty(line)) return cells;
    int i = 0, n = line.Length;
    while (i < n)
    {
        if (line[i] == '"')
        {
            i++;
            var sb = new System.Text.StringBuilder();
            while (i < n)
            {
                if (line[i] == '"' && i + 1 < n && line[i + 1] == '"') { sb.Append('"'); i += 2; }
                else if (line[i] == '"') { i++; break; }
                else { sb.Append(line[i++]); }
            }
            if (i < n && line[i] == ',') i++;
            cells.Add(sb.ToString());
        }
        else
        {
            int start = i;
            while (i < n && line[i] != ',') i++;
            cells.Add(line.Substring(start, i - start));
            if (i < n && line[i] == ',') i++;
        }
    }
    return cells;
}
// ---------- CHOICES: one-row export helper ----------
    static (bool localize, string keysPipe, string textsPipe, string disabledPipe)
    ExportRow_ChoiceNode(VNEngine.ChoiceNode cn, string nodeGuid)
{
    // Build runtime-ish payload to get resolved text if localization is on,
    // but for the CSV we intentionally mirror keys <-> texts 1:1.
    var payload = BuildChoicePayload(cn, nodeGuid);

    var keys     = GetStringArray(cn, "Button_Text")   ?? Array.Empty<string>();
    var disabled = GetStringArray(cn, "Disabled_Text") ?? Array.Empty<string>();

    int n = Mathf.Max(0, GetInt(cn, "Number_Of_Choices"));

    // truncate or pad to exactly n so export reflects the active choices only
    if (keys.Length > n)      keys = keys.Take(n).ToArray();
    else if (keys.Length < n) Array.Resize(ref keys, n);

    if (disabled.Length > n)      disabled = disabled.Take(n).ToArray();
    else if (disabled.Length < n) Array.Resize(ref disabled, n);

    // mirror: keys == texts in CSV (import will keep them mirrored)
    var keysPipe  = JoinPipe(keys.Select(s => s ?? ""));
    var textsPipe = keysPipe;
    var disPipe   = JoinPipe(disabled.Select(s => s ?? ""));

    return (payload.localize, keysPipe, textsPipe, disPipe);}
    
    static bool TryParseBool(string s, out bool value)
{
    value = false;
    if (string.IsNullOrWhiteSpace(s)) return false;
    if (bool.TryParse(s, out value)) return true;
    if (s == "1") { value = true;  return true; }
    if (s == "0") { value = false; return true; }
    return false;
}

    static void ExportUnifiedCsv(GameObject root, string outPath)
{
    EnsureIds(root);

    // NEW: lock export to the same ConversationManager as 'root'
    var rootCM = root.GetComponent<VNEngine.ConversationManager>();

    var ordered = root
        .GetComponentsInChildren<VNEngine.Node>(true)     // any depth
        .Where(n => n && n.GetComponentInParent<VNEngine.ConversationManager>() == rootCM)
        .OrderBy(n => n.transform.GetSiblingIndex())
        .Select(n => n.transform)
        .ToList();

    var rows = new List<string>(ordered.Count + 1);
    rows.Add(string.Join(",", kUnifiedHeaders)); // header

    for (int i = 0; i < ordered.Count; i++)
    {
        var t = ordered[i];
        var node = t.GetComponent<VNEngine.Node>();
        if (!node) continue;

        var tag = t.GetComponent<NodeExportTag>();
        var id  = tag ? tag.Id : GUID.Generate().ToString();
        var typeName = node.GetType().Name;

        // create an empty row map
        var row = new Dictionary<string,string>(StringComparer.Ordinal);

        // shared columns
        Put(row, "id", id);
        Put(row, "order", i.ToString());
        Put(row, "type", typeName);
        Put(row, "goName", t.name);

        // per-type payload
        switch (typeName)
        {
            case "DialogueNode":
            {
                var p = BuildDialoguePayload((VNEngine.DialogueNode)node);
                Put(row, "actorKey",         p.actorKey);
//                Put(row, "actorKeySource",   p.actorKeySource);
                Put(row, "speakerTitle",     p.speakerTitle);
//                Put(row, "dialogueKey",      p.dialogueKey);
//                Put(row, "dialogueKeySource",p.dialogueKeySource);
                Put(row, "text",             p.text);
//                Put(row, "bringToFront",     p.bringToFront);
//                Put(row, "darkenOthers",     p.darkenOthers);
                break;
            }
            case "ChoiceNode":
            {
                var cn = (VNEngine.ChoiceNode)node;

                // Use the existing helper to build CSV-friendly values
                var (localize, keysPipe, textsPipe, disabledPipe) = ExportRow_ChoiceNode(cn, id);

                // Write the columns expected by the unified CSV header
                Put(row, "choice_keys",     keysPipe);
                Put(row, "choice_texts",    textsPipe);
                Put(row, "choice_disabled", disabledPipe);
                break;
            }
            case "ChangeActorImageNode":
            {
                var p = BuildChangeActorImagePayload((VNEngine.ChangeActorImageNode)node);
                Put(row, "change_actorName",       p.actorName);
                Put(row, "change_expressionName",  p.expressionName);
                Put(row, "change_outfitName",      p.outfitName);
                Put(row, "change_pose",            p.pose);
//                Put(row, "change_fadeIn",        p.fadeIn);
//                Put(row, "change_lightenActor",  p.lightenActor);
///                Put(row, "change_bringToFront",  p.bringToFront);
                break;
            }
            case "SetBackground":
            {
                var p = BuildSetBackgroundPayload(node);
                Put(row, "bg_imageGuid",     p.imageGuid);
                Put(row, "bg_imageFile",     p.imageFile);
                Put(row, "bg_setForeground", p.setForeground);
                Put(row, "bg_fadeIn",        p.fadeIn);
                Put(row, "bg_fadeOut",       p.fadeOut);
                Put(row, "bg_fadeStyle",     p.fadeStyle);
                Put(row, "bg_fadeDuration",  p.fadeDuration.ToString(System.Globalization.CultureInfo.InvariantCulture));
                break;
            }
            case "EnterActorNode":
            {
                var p = BuildEnterActorPayload((VNEngine.EnterActorNode)node);
                Put(row, "enter_actorName",        p.actorName);
//                Put(row, "enter_actorNameSource",  p.actorNameSource);
//                Put(row, "enter_entranceType",     p.entranceType);
//                Put(row, "enter_fadeInTime",       p.fadeInTime.ToString(System.Globalization.CultureInfo.InvariantCulture));
//                Put(row, "enter_destination",      p.destination);
                break;
            }
            case "ShowChoiceNode":
            {
                var p = BuildShowChoicePayload((VNEngine.ShowChoiceNode)node, id);
                Put(row, "showchoice_texts",         JoinPipe(p.choices.Select(c => c.text ?? "")));
                Put(row, "showchoice_nextConv",      JoinPipe(p.choices.Select(c => c.nextConversationName ?? "")));
                Put(row, "showchoice_requirements",  JoinPipe(p.choices.Select(JoinRequirements)));
                Put(row, "showchoice_labels",        JoinPipe(p.choices.Select(c => c.label ?? "")));
                Put(row, "showchoice_categories",    JoinPipe(p.choices.Select(c => c.category ?? "")));
                Put(row, "showchoice_variants",      JoinPipe(p.choices.Select(c => c.variant ?? "")));
                Put(row, "showchoice_enableLogging", JoinPipe(p.choices.Select(c => c.enableLogging ? "true" : "false")));
                break;
            }
        }

        // finalize row into CSV
        rows.Add(RowToCsvLine(row, kUnifiedHeaders));
    }

    File.WriteAllLines(outPath, rows);
    Debug.Log($"Exported unified CSV with {rows.Count-1} node row(s) → {outPath}");
    EditorUtility.RevealInFinder(outPath);
}

    static void ImportUnifiedCsv(GameObject root, string path)
{
    if (!File.Exists(path)) { Debug.LogWarning("CSV not found: " + path); return; }
    var lines = File.ReadAllLines(path).ToList();
    if (lines.Count <= 1) { Debug.Log("CSV has no data rows."); return; }

    // Build id -> (Transform, Node)
    var map = new Dictionary<string,(Transform t, VNEngine.Node node)>();
    foreach (var tag in root.GetComponentsInChildren<NodeExportTag>(true))
    {
        var n = tag.GetComponent<VNEngine.Node>();
        if (n) map[tag.Id] = (tag.transform, n);
    }
    var header = ParseCsvLine(lines[0]);
    int Col(string name) => header.FindIndex(h => string.Equals(h, name, StringComparison.OrdinalIgnoreCase));
    string Cell(List<string> row, int idx) => (idx >= 0 && idx < row.Count) ? row[idx] : "";

    // required cols
    int c_id   = Col("id");
    int c_type = Col("type");
    int c_order= Col("order"); // optional

    // early out only if the REQUIRED columns are missing
    if (c_id < 0 || c_type < 0)
    {
        Debug.LogError("CSV is missing required columns: id/type");
        return;
    }

    // dialogue
    int d_actorKey = Col("actorKey");
    int d_speaker  = Col("speakerTitle");
    int d_text     = Col("text");

    // choices
    int ch_keys = Col("choice_keys");
    int ch_texts= Col("choice_texts");
    int ch_dis  = Col("choice_disabled");

    // change image
    int ca_name = Col("change_actorName");
    int ca_expression = Col("change_expressionName");
    int ca_outfit = Col("change_outfitName");
    int ca_pose = Col("change_pose");

    // backgrounds
    int bg_guid = Col("bg_imageGuid"), bg_file = Col("bg_imageFile");
    int bg_setForeground = Col("bg_setForeground");
    int bg_fadeIn = Col("bg_fadeIn"), bg_fadeOut = Col("bg_fadeOut");
    int bg_fadeStyle = Col("bg_fadeStyle"), bg_fadeDuration = Col("bg_fadeDuration");

    // enter actor
    int en_actor=Col("enter_actorName");

    // show choice
    int sc_texts=Col("showchoice_texts"), sc_nextConv=Col("showchoice_nextConv"), sc_reqs=Col("showchoice_requirements");
    int sc_labels=Col("showchoice_labels"), sc_categories=Col("showchoice_categories"), sc_variants=Col("showchoice_variants");
    int sc_enableLogging=Col("showchoice_enableLogging");

    Undo.RegisterFullObjectHierarchyUndo(root, "Import VN CSV");

    for (int i = 1; i < lines.Count; i++)
    {
        var line = ParseCsvLine(lines[i]);
        if (line.Count == 0) continue;

        var id      = Cell(line, c_id);
        var rowType = Cell(line, c_type);
        if (string.IsNullOrWhiteSpace(id) || string.IsNullOrWhiteSpace(rowType)) continue;
        if (!map.TryGetValue(id, out var tuple)) continue;

        var (t, node) = tuple;
        var typeName  = node.GetType().Name;

        // Only modify when types match
        if (!string.Equals(typeName, rowType, StringComparison.Ordinal))
            continue;

        // Sibling order (optional)
        var orderStr = Cell(line, c_order);
        if (!string.IsNullOrEmpty(orderStr) && int.TryParse(orderStr, out var ord))
        {
            t.SetSiblingIndex(Mathf.Clamp(ord, 0, root.transform.childCount - 1));
            EditorUtility.SetDirty(t);
        }

        switch (typeName)
        {
            case "DialogueNode":
            {
                var dn = (VNEngine.DialogueNode)node;
                var actor   = Cell(line, d_actorKey);
                var speaker = Cell(line, d_speaker);
                var text    = Cell(line, d_text);

                if (!string.IsNullOrEmpty(actor))   Set(dn,"actor",actor);
                if (!string.IsNullOrEmpty(speaker)) Set(dn,"textbox_title",speaker);
                if (!string.IsNullOrEmpty(text))    Set(dn,"text",text);

                EditorUtility.SetDirty(dn);
                break;
            }
            case "ChoiceNode":
            {
                var cn = (VNEngine.ChoiceNode)node;

                // read from the CURRENT ROW, not re-parsing a single cell
                string keysJoined    = Cell(line, ch_keys);
                string textsJoined   = Cell(line, ch_texts);
                string disabledJoin  = Cell(line, ch_dis);

                // treat "none"/"null" as empty too
                bool IsBlank(string s) =>
                    string.IsNullOrWhiteSpace(s) || string.Equals(s, "none", StringComparison.OrdinalIgnoreCase) || string.Equals(s, "null", StringComparison.OrdinalIgnoreCase);

                // mirror one into the other if only one is present
                bool hasKeys = !IsBlank(keysJoined);
                bool hasText = !IsBlank(textsJoined);
                if (hasKeys && !hasText) textsJoined = keysJoined;
                else if (!hasKeys && hasText) keysJoined = textsJoined;

                if (hasKeys || hasText)
                {
                    var keys     = IsBlank(keysJoined)   ? Array.Empty<string>() : keysJoined.Split('|');
                    var texts    = IsBlank(textsJoined)  ? Array.Empty<string>() : textsJoined.Split('|');
                    var disabled = IsBlank(disabledJoin) ? Array.Empty<string>() : disabledJoin.Split('|');

                    // keep length consistent (use keys/texts length as driver)
                    int n = Math.Max(keys.Length, texts.Length);
                    if (n == 0) break;

                    Array.Resize(ref keys, n);
                    Array.Resize(ref texts, n);
                    Array.Resize(ref disabled, n);

                    Set(cn, "Number_Of_Choices", n);
                    SetArray(cn, "Button_Text",   keys);     // keys shown on buttons (or localization keys)
                    SetArray(cn, "Disabled_Text", disabled); // optional disabled text

                    EditorUtility.SetDirty(cn);
                }
                break;
            }

            case "ChangeActorImageNode":
            {
                var n = (VNEngine.ChangeActorImageNode)node;
                var name = Cell(line, ca_name);
                var expression = Cell(line, ca_expression);
                var outfit = Cell(line, ca_outfit);
                var pose = Cell(line, ca_pose);

                if (!string.IsNullOrEmpty(name)) Set(n, "actor_name", name);
                if (!string.IsNullOrEmpty(expression))
                {
                    Set(n, "expression_name", expression);
                    EditorUtility.SetDirty(n);
                }
                if (!string.IsNullOrEmpty(outfit))
                {
                    Set(n, "outfit_name", outfit);
                    EditorUtility.SetDirty(n);
                }
                if (!string.IsNullOrEmpty(pose))
                {
                    SetEnum(n, "pose", pose);
                    EditorUtility.SetDirty(n);
                }
                break;
            }

            case "SetBackground":
            {
                var comp = node;
                var guid = Cell(line, bg_guid);
                var file = Cell(line, bg_file);

                bool set =
                    TrySetAssetByGuidSafe(comp, "sprite", guid) ||
                    TrySetAssetByNameSafe(comp, "sprite", file);

                if (TryParseBool(Cell(line, bg_setForeground), out var sf)) { Set(comp, "set_foreground", sf); set = true; }
                if (TryParseBool(Cell(line, bg_fadeIn), out var fi)) { Set(comp, "fade_in", fi); set = true; }
                if (TryParseBool(Cell(line, bg_fadeOut), out var fo)) { Set(comp, "fade_out", fo); set = true; }

                var fadeStyleStr = Cell(line, bg_fadeStyle);
                if (!string.IsNullOrEmpty(fadeStyleStr)) { SetEnum(comp, "fade_style", fadeStyleStr); set = true; }

                var fadeDurationStr = Cell(line, bg_fadeDuration);
                if (!string.IsNullOrEmpty(fadeDurationStr)
                    && float.TryParse(fadeDurationStr, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out var fd))
                {
                    Set(comp, "fade_duration", fd);
                    set = true;
                }

                if (set) EditorUtility.SetDirty(comp);
                break;
            }

            case "EnterActorNode":
            {
                var n = (VNEngine.EnterActorNode)node;
                var actor = Cell(line, en_actor);
                if (!string.IsNullOrEmpty(actor)) Set(n,"actor_name", actor);
                EditorUtility.SetDirty(n);
                break;
            }
            case "ShowChoiceNode":
            {
                var n = (VNEngine.ShowChoiceNode)node;
                var texts      = SplitPipe(Cell(line, sc_texts));
                var nextConv   = SplitPipe(Cell(line, sc_nextConv));
                var reqs       = SplitPipe(Cell(line, sc_reqs));
                var labels     = SplitPipe(Cell(line, sc_labels));
                var categories = SplitPipe(Cell(line, sc_categories));
                var variants   = SplitPipe(Cell(line, sc_variants));
                var logging    = SplitPipe(Cell(line, sc_enableLogging));

                if (texts.Length > 0)
                {
                    var allConvs = Resources.FindObjectsOfTypeAll<VNEngine.ConversationManager>();
                    var newChoices = new List<VNEngine.ShowChoiceNode.Choice>();
                    for (int ci = 0; ci < texts.Length; ci++)
                    {
                        var c = new VNEngine.ShowChoiceNode.Choice { text = texts[ci] };

                        var convName = ci < nextConv.Length ? nextConv[ci] : "";
                        if (!string.IsNullOrEmpty(convName))
                            c.nextConversation = allConvs.FirstOrDefault(x => x.name == convName);

                        if (ci < reqs.Length) c.requirements = ParseRequirements(reqs[ci]);
                        if (ci < labels.Length) c.label = labels[ci];
                        if (ci < categories.Length) c.category = categories[ci];
                        if (ci < variants.Length) c.variant = variants[ci];
                        if (ci < logging.Length) TryParseBool(logging[ci], out c.enableLogging);

                        newChoices.Add(c);
                    }
                    n.choices = newChoices;
                    EditorUtility.SetDirty(n);
                }
                break;
            }
        }
    }

    UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(root.scene);
    Debug.Log("Unified CSV import complete.");
}
    // Case‑insensitive column lookup
    static int Col(IReadOnlyList<string> header, string name)
{
    if (header == null || string.IsNullOrEmpty(name)) return -1;
    for (int i = 0; i < header.Count; i++)
        if (string.Equals(header[i], name, StringComparison.OrdinalIgnoreCase))
            return i;
    return -1;
}
    static bool TrySetAssetByGuidSafe(UnityEngine.Object target, string fieldName, string guid)
{
    if (string.IsNullOrWhiteSpace(guid)) return false;
    if (string.Equals(guid, "none", StringComparison.OrdinalIgnoreCase) ||
        string.Equals(guid, "null", StringComparison.OrdinalIgnoreCase)) return false;

#if UNITY_EDITOR
    var path = UnityEditor.AssetDatabase.GUIDToAssetPath(guid);
    if (string.IsNullOrEmpty(path)) return false;
    var asset = UnityEditor.AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(path);
    if (!asset) return false;
    Set(target, fieldName, asset);
    return true;
#else
    return false;
#endif
}

    static bool TrySetAssetByNameSafe(UnityEngine.Object target, string fieldName, string file)
{
    if (string.IsNullOrWhiteSpace(file)) return false;
    if (string.Equals(file, "none", StringComparison.OrdinalIgnoreCase) ||
        string.Equals(file, "null", StringComparison.OrdinalIgnoreCase)) return false;

#if UNITY_EDITOR
    var guids = UnityEditor.AssetDatabase.FindAssets(System.IO.Path.GetFileNameWithoutExtension(file));
    foreach (var g in guids)
    {
        var p = UnityEditor.AssetDatabase.GUIDToAssetPath(g);
        var asset = UnityEditor.AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(p);
        if (asset && string.Equals(System.IO.Path.GetFileName(p), file, StringComparison.OrdinalIgnoreCase))
        {
            Set(target, fieldName, asset);
            return true;
        }
    }
#endif
    return false;
}

        static void ApplyTypedPayload(VNEngine.Node node, NodeRecord rec)
        {
            switch (rec.type)
            {
                case "DialogueNode":
                    if (rec.dialogue != null)
                        ApplyDialoguePayload((VNEngine.DialogueNode)node, rec.dialogue);
                    break;

                case "ChoiceNode":
                    if (rec.choice != null)
                        ApplyChoicePayload((VNEngine.ChoiceNode)node, rec.choice);
                    break;

                case "ChangeActorImageNode":
                    if (rec.changeActorImage != null)
                        ApplyChangeActorImagePayload((VNEngine.ChangeActorImageNode)node, rec.changeActorImage);
                    break;

                case "SetBackground":
                    if (rec.setBackground != null)
                        ApplySetBackgroundPayload(node, rec.setBackground);
                    break;

                case "EnterActorNode":
                    if (rec.enterActor != null)
                        ApplyEnterActorPayload((VNEngine.EnterActorNode)node, rec.enterActor);
                    break;
            }
        }
        static void ApplyDialoguePayload(VNEngine.DialogueNode dn, DialoguePayload p)
        {
            Set(dn, "actor", p.actorKey);
            SetEnum(dn, "actor_name_from", p.actorKeySource);
            Set(dn, "textbox_title", p.speakerTitle);
            Set(dn, "localized_key", p.dialogueKey);
            SetEnum(dn, "dialogue_from", p.dialogueKeySource);
            Set(dn, "text", p.text);
            Set(dn, "bring_speaker_to_front", p.bringToFront);
            Set(dn, "darken_all_other_characters", p.darkenOthers);
        }
        static void ApplyChoicePayload(VNEngine.ChoiceNode cn, ChoicePayload p)
        {
            Set(cn, "Localize_Choice_Text", p.localize);
            int n = p.choices?.Count ?? 0;
            Set(cn, "Number_Of_Choices", n);

            var keys = GetStringArray(cn, "Button_Text") ?? new string[Mathf.Max(6, n)];
            var disabled = GetStringArray(cn, "Disabled_Text") ?? new string[Mathf.Max(6, n)];

            if (keys.Length < n) Array.Resize(ref keys, n);
            if (disabled.Length < n) Array.Resize(ref disabled, n);

            for (int i = 0; i < n; i++)
            {
                keys[i] = p.choices[i]?.key ?? "";
                disabled[i] = p.choices[i]?.disabledText ?? "";
            }

            SetArray(cn, "Button_Text", keys);
            SetArray(cn, "Disabled_Text", disabled);
        }
        static void ApplyChangeActorImagePayload(VNEngine.ChangeActorImageNode n, ChangeActorImagePayload p)
        {
            Set(n, "actor_name", p.actorName);
            Set(n, "expression_name", p.expressionName);
            Set(n, "outfit_name", p.outfitName);
            if (!string.IsNullOrEmpty(p.pose))
                SetEnum(n, "pose", p.pose);
        }
        static void ApplySetBackgroundPayload(object node /*SetBackground*/, SetBackgroundPayload p)
        {
            Set(node, "set_foreground", p.setForeground);
            Set(node, "fade_in", p.fadeIn);
            Set(node, "fade_out", p.fadeOut);

            if (!string.IsNullOrEmpty(p.fadeStyle))
                SetEnum(node, "fade_style", p.fadeStyle);
            if (p.fadeDuration > 0f)
                Set(node, "fade_duration", p.fadeDuration);

            if (!string.IsNullOrEmpty(p.imageGuid))
            {
                var path = AssetDatabase.GUIDToAssetPath(p.imageGuid);
                var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
                if (sprite != null) SetObject(node, "sprite", sprite);
            }
        }
        static void ApplyEnterActorPayload(VNEngine.EnterActorNode n, EnterActorPayload p)
        {
            Set(n, "actor_name", p.actorName);
            SetEnum(n, "actor_name_from", p.actorNameSource);
            SetEnum(n, "entrance_type", p.entranceType);
            Set(n, "fade_in_time", p.fadeInTime);
            SetEnum(n, "destination", p.destination);
        }

        // ==== REFLECTION HELPERS (get) ========================================
        static bool TryGetField(Type t, string name, out FieldInfo fi)
        {
            const BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
            while (t != null && t != typeof(object))
            {
                fi = t.GetField(name, flags);
                if (fi != null) return true;
                t = t.BaseType;
            }

            fi = null;
            return false;
        }

        static string GetString(object o, string f) =>
            TryGetField(o.GetType(), f, out var fi) && fi.FieldType == typeof(string) ? (string)fi.GetValue(o) : "";

        static bool GetBool(object o, string f) =>
            TryGetField(o.GetType(), f, out var fi) && fi.FieldType == typeof(bool) ? (bool)fi.GetValue(o) : false;

        static int GetInt(object o, string f) => TryGetField(o.GetType(), f, out var fi) && fi.FieldType == typeof(int)
            ? (int)fi.GetValue(o)
            : 0;

        static float GetFloat(object o, string f) =>
            TryGetField(o.GetType(), f, out var fi) && fi.FieldType == typeof(float) ? (float)fi.GetValue(o) : 0f;

        static string[] GetStringArray(object o, string f)
            => TryGetField(o.GetType(), f, out var fi) && fi.FieldType == typeof(string[])
                ? (string[])fi.GetValue(o)
                : null;

        static string GetEnumName(object o, string f)
        {
            if (!TryGetField(o.GetType(), f, out var fi) || !fi.FieldType.IsEnum) return "";
            var val = fi.GetValue(o);
            return Enum.GetName(fi.FieldType, val) ?? "";
        }

        // ==== REFLECTION HELPERS (set) ========================================
        static void Set(object o, string f, string v)
        {
            if (TryGetField(o.GetType(), f, out var fi) && fi.FieldType == typeof(string)) fi.SetValue(o, v);
        }

        static void Set(object o, string f, bool v)
        {
            if (TryGetField(o.GetType(), f, out var fi) && fi.FieldType == typeof(bool)) fi.SetValue(o, v);
        }

        static void Set(object o, string f, int v)
        {
            if (TryGetField(o.GetType(), f, out var fi) && fi.FieldType == typeof(int)) fi.SetValue(o, v);
        }

        static void Set(object o, string f, float v)
        {
            if (TryGetField(o.GetType(), f, out var fi) && fi.FieldType == typeof(float)) fi.SetValue(o, v);
        }

        static void SetArray(object o, string f, Array arr)
        {
            if (TryGetField(o.GetType(), f, out var fi) && fi.FieldType.IsArray) fi.SetValue(o, arr);
        }

        static void SetEnum(object o, string f, string enumName)
        {
            if (string.IsNullOrEmpty(enumName)) return;
            if (TryGetField(o.GetType(), f, out var fi) && fi.FieldType.IsEnum)
            {
                try
                {
                    fi.SetValue(o, Enum.Parse(fi.FieldType, enumName));
                }
                catch
                {
                }
            }
        }

        static void SetObject(object o, string f, UnityEngine.Object obj)
        {
            if (TryGetField(o.GetType(), f, out var fi) && typeof(UnityEngine.Object).IsAssignableFrom(fi.FieldType))
                fi.SetValue(o, obj);
        }

   
        static (string guid, string file) AssetGuidAndFile(object target, string fieldName)
        {
            if (!TryGetField(target.GetType(), fieldName, out var fi)) return (null, null);
            if (!typeof(UnityEngine.Object).IsAssignableFrom(fi.FieldType)) return (null, null);

            var obj = fi.GetValue(target) as UnityEngine.Object;
            if (!obj) return (null, null);

            string path = AssetDatabase.GetAssetPath(obj);
            string guid = string.IsNullOrEmpty(path) ? null : AssetDatabase.AssetPathToGUID(path);
            string file = string.IsNullOrEmpty(path) ? null : Path.GetFileNameWithoutExtension(path);
            return (guid, file);
        }
// Column order used by the unified CSV
        static readonly string[] kUnifiedHeaders = new[]{
            "id","order","type","goName",
            "actorKey","speakerTitle","text",
            "choice_keys","choice_texts","choice_disabled",   // <-- add these two
            "change_actorName","change_expressionName","change_outfitName","change_pose",
            "bg_imageGuid","bg_imageFile",
            "bg_setForeground","bg_fadeIn","bg_fadeOut","bg_fadeStyle","bg_fadeDuration",
            "enter_actorName",
            "showchoice_texts","showchoice_nextConv","showchoice_requirements",
            "showchoice_labels","showchoice_categories","showchoice_variants","showchoice_enableLogging"
        };
        
// safe put
        static void Put(Dictionary<string,string> row, string key, string value)
        {
            if (row == null) return;
            row[key] = value ?? "";
        }
        static void Put(Dictionary<string,string> row, string key, bool value) => row[key] = value ? "true" : "false";

// build a CSV line from a row map + headers
        static string RowToCsvLine(Dictionary<string,string> row, IReadOnlyList<string> headers)
        {
            var cells = new string[headers.Count];
            for (int i = 0; i < headers.Count; i++)
            {
                row.TryGetValue(headers[i], out var val);
                cells[i] = CsvEscape(val ?? "");
            }
            return string.Join(",", cells);
        }
        
    }
}
#endif
