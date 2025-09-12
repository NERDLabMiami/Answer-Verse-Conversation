// OpenGameDataNode.cs
using UnityEngine;
using System.Collections.Generic;
using VNEngine;

namespace VNEngine
{
    public enum StatsCaptureMode { None, TraitsOnly, AllState }
    public enum AssignmentMode  { Random, WeightedRandom }

    [AddComponentMenu("VN Engine/Analytics/Open Game Data Event")]
    public class OpenGameDataNode : Node
    {
        public string studyId = "";                   // optional study/stimulus id
        [TextArea] public string notes = "";          // optional free-text tag
        public bool includeConversationContext = true;
        public bool includePlayTime = true;
        public StatsCaptureMode statsCapture = StatsCaptureMode.None;

        public bool assignCondition = false;

        [Tooltip("Logical bucket for this assignment (e.g., 'arm', 'phase2'). Also used as default stat key.")]
        public string conditionGroup = "arm";

        public AssignmentMode assignmentMode = AssignmentMode.Random;

        [System.Serializable]
        public class ConditionEntry
        {
            public string conditionId = "A";                 // e.g., A/B/C or X/Y/Z
            public float weight = 1f;                        // used if WeightedRandom
            public ConversationManager nextConversation;     // optional jump target
            public bool startOnAssign = true;                // jump immediately after logging
            public bool enabled = true;                      // allow temporarily disabling
        }
        public List<ConditionEntry> conditions = new();

        [Tooltip("Also write the chosen condition to StatsManager.string_stats.")]
        public bool writeConditionToStat = true;

        [Tooltip("If empty, defaults to UPPERCASE(conditionGroup).")]
        public string conditionStatKeyOverride = "";         // e.g., "ARM" or "PHASE2"

        public string markerId = "";                         // e.g., "conversation_end", "survey_complete"

        public override void Run_Node()
        {
            // 1) --- LOG ---
            var didPick = false;
            ConditionEntry picked = null;
            int pickedIndex = -1;

            if (assignCondition)
            {
                pickedIndex = PickConditionIndex();
                if (pickedIndex >= 0)
                {
                    picked = conditions[pickedIndex];
                    didPick = true;
                }
                else
                {
                    Debug.LogWarning($"{name}: assignCondition enabled but no valid entries found.");
                }
            }

            if (Logging.Instance != null)
            {
                // Event name reflects what happened
                string eventName = didPick ? "condition_assigned" : "milestone";
                if (!didPick && string.IsNullOrEmpty(markerId)) eventName = "milestone"; // fallback

                Logging.Instance.BeginCustom(eventName);

                if (!string.IsNullOrEmpty(studyId)) Logging.Instance.Param("study", studyId);
                if (!string.IsNullOrEmpty(notes))   Logging.Instance.Param("notes", notes);

                // Assignment payload
                if (didPick)
                {
                    string groupKey = string.IsNullOrEmpty(conditionGroup) ? "group" : conditionGroup;
                    Logging.Instance.Param("condition_group", groupKey);
                    Logging.Instance.Param("condition", picked.conditionId);
                    Logging.Instance.Param("assignment_mode", assignmentMode.ToString());
                    Logging.Instance.Param("condition_index", pickedIndex);
                    Logging.Instance.Param("condition_weight", picked.weight);

                    if (picked.nextConversation != null)
                        Logging.Instance.Param("next_conversation", picked.nextConversation.name);
                }

                // Milestone marker (can be used with or without assignment)
                if (!string.IsNullOrEmpty(markerId))
                    Logging.Instance.Param("marker", markerId);

                // Context JSON
                if (includeConversationContext)
                {
                    var cm = GetComponentInParent<ConversationManager>();
                    var ctx = new Dictionary<string, object> {
                        { "conversation", cm ? cm.name : "" },
                        { "node", name },
                        { "nodeIndex", cm ? cm.cur_node : 0 }
                    };
                    if (includePlayTime && VNSceneManager.scene_manager != null)
                        ctx["play_time"] = VNSceneManager.scene_manager.play_time;

                    Logging.Instance.ParamJson("context", WriteJson(ctx));
                }

                // Optional stats snapshot
                var stats = BuildStats(statsCapture);
                if (stats != null && stats.Count > 0)
                    Logging.Instance.ParamJson("stats", WriteJson(stats));

                Logging.Instance.SubmitCustom();
            }

            // 2) --- STATE WRITE (optional) ---
            if (didPick && writeConditionToStat)
            {
                string statKey = !string.IsNullOrEmpty(conditionStatKeyOverride)
                               ? conditionStatKeyOverride
                               : (string.IsNullOrEmpty(conditionGroup) ? "CONDITION" : conditionGroup.ToUpperInvariant());

                // Store as string stat (so other nodes/gates/UI can read it later)
                StatsManager.Set_String_Stat(statKey, picked.conditionId);
            }

            // 3) --- FLOW (optional jump) ---
            if (didPick && picked.startOnAssign && picked.nextConversation != null)
            {
                var host = GetComponentInParent<ConversationManager>();
                if (host) host.Finish_Conversation();
                picked.nextConversation.Start_Conversation();
                go_to_next_node = false; // stop this chain; we jumped
                return;
            }

            // Otherwise, continue
            base.Finish_Node();
        }

        // ----------------- helpers -----------------

        int PickConditionIndex()
        {
            // Build list of enabled entries with positive weight (for weighted mode)
            var idxs = new List<int>();
            float totalW = 0f;

            for (int i = 0; i < conditions.Count; i++)
            {
                var c = conditions[i];
                if (c == null || !c.enabled) continue;
                if (assignmentMode == AssignmentMode.WeightedRandom && c.weight <= 0f) continue;
                idxs.Add(i);
                totalW += Mathf.Max(0f, c.weight);
            }

            if (idxs.Count == 0) return -1;

            if (assignmentMode == AssignmentMode.Random)
            {
                return idxs[Random.Range(0, idxs.Count)];
            }

            // WeightedRandom
            if (totalW <= 0f) return idxs[Random.Range(0, idxs.Count)]; // fallback uniform
            float roll = Random.value * totalW;
            float acc = 0f;
            foreach (int i in idxs)
            {
                acc += Mathf.Max(0f, conditions[i].weight);
                if (roll <= acc) return i;
            }
            return idxs[idxs.Count - 1]; // safety
        }

        private static Dictionary<string, object> BuildStats(StatsCaptureMode mode)
        {
            if (mode == StatsCaptureMode.None) return null;

            var result = new Dictionary<string, object>();

            if (mode == StatsCaptureMode.TraitsOnly && StatsManager.numbered_stats.Count > 0)
            {
                var n = new Dictionary<string, object>();
                foreach (var kv in StatsManager.numbered_stats) n[kv.Key] = kv.Value;
                result["numbered"] = n;
                return result;
            }

            if (StatsManager.numbered_stats.Count > 0)
            {
                var n = new Dictionary<string, object>();
                foreach (var kv in StatsManager.numbered_stats) n[kv.Key] = kv.Value;
                result["numbered"] = n;
            }
            if (StatsManager.boolean_stats.Count > 0)
            {
                var b = new Dictionary<string, object>();
                foreach (var kv in StatsManager.boolean_stats) b[kv.Key] = kv.Value;
                result["boolean"] = b;
            }
            if (StatsManager.string_stats.Count > 0)
            {
                var s = new Dictionary<string, object>();
                foreach (var kv in StatsManager.string_stats) s[kv.Key] = kv.Value;
                result["string"] = s;
            }
            return result;
        }

        private static string WriteJson(object obj)
        {
            var sb = new System.Text.StringBuilder();
            void W(object o)
            {
                switch (o)
                {
                    case null: sb.Append("null"); break;
                    case string s: sb.Append('"').Append(s.Replace("\\","\\\\").Replace("\"","\\\"").Replace("\n","\\n").Replace("\r","\\r").Replace("\t","\\t")).Append('"'); break;
                    case bool b: sb.Append(b ? "true" : "false"); break;
                    case int i: sb.Append(i); break;
                    case float f: sb.Append(f.ToString(System.Globalization.CultureInfo.InvariantCulture)); break;
                    case double d: sb.Append(d.ToString(System.Globalization.CultureInfo.InvariantCulture)); break;
                    case IDictionary<string, object> dict:
                        sb.Append('{'); bool first = true;
                        foreach (var kv in dict) { if(!first) sb.Append(','); first=false; W(kv.Key); sb.Append(':'); W(kv.Value); }
                        sb.Append('}'); break;
                    case System.Collections.IEnumerable seq:
                        sb.Append('['); bool firstArr = true;
                        foreach (var v in seq) { if(!firstArr) sb.Append(','); firstArr=false; W(v); }
                        sb.Append(']'); break;
                    default: sb.Append('"').Append(o.ToString()).Append('"'); break;
                }
            }
            W(obj);
            return sb.ToString();
        }
        
#if UNITY_EDITOR
        void OnValidate()
        {
            writeConditionToStat = true; // always true
            if (conditions != null)
                foreach (var c in conditions)
                    if (c != null) c.startOnAssign = true; // always true
        }
#endif
    }
    
}
