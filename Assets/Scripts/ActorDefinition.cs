using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System.Text;
using System.Text.RegularExpressions;
using System.Globalization;

public static class ActorKey
{
    // Convert any human text into a stable slug like "ji-ah" or "hair-long".
    public static string Canon(string input)
    {
        if (string.IsNullOrWhiteSpace(input)) return "";

        // 1) lower + trim
        string s = input.Trim().ToLowerInvariant();

        // 2) remove diacritics (Bréanna -> Breanna)
        s = s.Normalize(NormalizationForm.FormD);
        var sb = new StringBuilder(s.Length);
        foreach (var ch in s)
        {
            var cat = CharUnicodeInfo.GetUnicodeCategory(ch);
            if (cat != UnicodeCategory.NonSpacingMark) sb.Append(ch);
        }
        s = sb.ToString();

        // 3) replace any run of non [a-z0-9] with a single '-'
        s = Regex.Replace(s, @"[^a-z0-9]+", "-");

        // 4) collapse and trim hyphens
        s = Regex.Replace(s, @"-+", "-").Trim('-');

        return s;
    }

    // True if already canonical (helps in OnValidate warnings)
    public static bool IsCanonical(string s) => s == Canon(s);

    // Ensure uniqueness against an existing set: adds -2, -3, ...
    public static string MakeUnique(string baseKey, ISet<string> taken)
    {
        var key = Canon(baseKey);
        if (!taken.Contains(key)) return key;

        int i = 2;
        string candidate;
        do { candidate = $"{key}-{i++}"; }
        while (taken.Contains(candidate));
        return candidate;
    }
}

[CreateAssetMenu(menuName="AnswerVerse/Actor Definition")]
public class ActorDefinition : ScriptableObject
{
    [Header("Identity")]
    public string displayName;                 // Writer-facing
    public string key;                         // Canonical slug (engine identity), e.g., "ji-ah"
    public List<string> aliases;               // Optional: back-compat/localization aliases
    public string prefabName;                  // Resources/Actors/<prefabName>

    [Header("Variants (strict)")]
    public List<ActorVariantDef> actorVariants = new() {
        new ActorVariantDef { key = "default", displayName = "Default" }
    };
    public string defaultVariantKey = "default";

    [Header("Expressions (full body)")]
    public List<BodyExpression> bodyExpressions = new();

    [Header("Expressions (portraits)")]
    public List<PortraitExpression> portraitExpressions = new();

    [Header("Audio Routing (optional)")]
    public int drumIndex;

    // --- Validation / authoring guidance ---
    [Header("Validation")]
    [Tooltip("Each variant should minimally include these base poses at Closed/Center (and portrait).")]
    public List<string> requiredBaselines = new() { "neutral" };

    void OnValidate()
    {
        // Canonicalize identity keys
        key = string.IsNullOrWhiteSpace(key) ? ActorKey.Canon(string.IsNullOrWhiteSpace(displayName) ? name : displayName) : ActorKey.Canon(key);
        if (string.IsNullOrWhiteSpace(prefabName)) prefabName = key;
        if (aliases != null) for (int i=0;i<aliases.Count;i++) aliases[i] = ActorKey.Canon(aliases[i]);

        // Build variant key set and ensure uniqueness
        var vset = new HashSet<string>();
        foreach (var v in actorVariants)
        {
            v.key = ActorKey.Canon(string.IsNullOrWhiteSpace(v.key) ? "default" : v.key);
            if (!vset.Add(v.key)) Debug.LogError($"{name}: duplicate variant key '{v.key}'");
        }
        defaultVariantKey = ActorKey.Canon(string.IsNullOrWhiteSpace(defaultVariantKey) ? "default" : defaultVariantKey);
        if (!vset.Contains(defaultVariantKey)) Debug.LogError($"{name}: defaultVariantKey '{defaultVariantKey}' not in actorVariants");

        // Validate expressions: variant must exist; keys canonicalized
        foreach (var e in bodyExpressions)
        {
            e.baseKey    = ActorKey.Canon(e.baseKey);
            e.variantKey = ActorKey.Canon(e.variantKey);
            if (!vset.Contains(e.variantKey))
                Debug.LogError($"{name}: body '{e.baseKey}' has invalid variant '{e.variantKey}'");
        }
        foreach (var e in portraitExpressions)
        {
            e.baseKey    = ActorKey.Canon(e.baseKey);
            e.variantKey = ActorKey.Canon(e.variantKey);
            if (!vset.Contains(e.variantKey))
                Debug.LogError($"{name}: portrait '{e.baseKey}' has invalid variant '{e.variantKey}'");
        }

        // Baseline coverage warnings (strict within variant)
        foreach (var v in vset)
        {
            foreach (var baseK in requiredBaselines)
            {
                bool hasBody = bodyExpressions.Any(e => e.variantKey==v && e.baseKey==baseK && e.mouth==Mouth.Closed && e.gaze==Gaze.Center && e.hands==Hands.Default);
                bool hasFace = portraitExpressions.Any(e => e.variantKey==v && e.baseKey==baseK && e.mouth==Mouth.Closed && e.gaze==Gaze.Center);
                if (!hasBody) Debug.LogWarning($"{name}: missing BODY baseline '{baseK}/Closed/Center/Default' for variant '{v}'");
                if (!hasFace) Debug.LogWarning($"{name}: missing PORTRAIT baseline '{baseK}/Closed/Center' for variant '{v}'");
            }
        }
    }

    // -------- Helper queries for editor UIs (populate-by-variant) --------

    public IReadOnlyList<string> GetBaseKeysForVariant(string variantKey, bool portrait = false)
    {
        variantKey = ActorKey.Canon(variantKey);
        return (portrait ? portraitExpressions.Select(e => (e.variantKey, e.baseKey))
                         : bodyExpressions    .Select(e => (e.variantKey, e.baseKey)))
               .Where(t => t.variantKey == variantKey)
               .Select(t => t.baseKey)
               .Distinct()
               .OrderBy(s => s)
               .ToList();
    }

    public IReadOnlyList<Mouth> GetMouths(string variantKey, string baseKey, bool portrait = false)
    {
        variantKey = ActorKey.Canon(variantKey); baseKey = ActorKey.Canon(baseKey);
        return (portrait ? portraitExpressions.Where(e => e.variantKey==variantKey && e.baseKey==baseKey).Select(e => e.mouth)
                         : bodyExpressions    .Where(e => e.variantKey==variantKey && e.baseKey==baseKey).Select(e => e.mouth))
               .Distinct().OrderBy(x => (int)x).ToList();
    }

    public IReadOnlyList<Gaze> GetGazes(string variantKey, string baseKey, bool portrait = false)
    {
        variantKey = ActorKey.Canon(variantKey); baseKey = ActorKey.Canon(baseKey);
        return (portrait ? portraitExpressions.Where(e => e.variantKey==variantKey && e.baseKey==baseKey).Select(e => e.gaze)
                         : bodyExpressions    .Where(e => e.variantKey==variantKey && e.baseKey==baseKey).Select(e => e.gaze))
               .Distinct().OrderBy(x => (int)x).ToList();
    }

    public IReadOnlyList<Hands> GetHands(string variantKey, string baseKey)
    {
        variantKey = ActorKey.Canon(variantKey); baseKey = ActorKey.Canon(baseKey);
        return bodyExpressions.Where(e => e.variantKey==variantKey && e.baseKey==baseKey)
               .Select(e => e.hands).Distinct().OrderBy(x => (int)x).ToList();
    }
}

// ---- Typed pieces ----

[Serializable]
public class ActorVariantDef
{
    public string key;         // canonical: "default", "hair-long", ...
    public string displayName; // writer-facing
}

public enum Mouth { Closed, Talk }

// NOTE: switch from Straight/Side to a 3-way gaze so you can target “angry-left” cleanly.
public enum Gaze  { Center, Left, Right }

public enum Hands { Default, PoseA, PoseB }

[Serializable]
public class BodyExpression
{
    public string baseKey;     // "sad", "angry", "neutral" ...
    public string variantKey;  // MUST match an ActorVariantDef.key
    public Mouth mouth;        // Closed/Talk
    public Gaze gaze;          // Center/Left/Right
    public Hands hands;        // Default/PoseA/PoseB
    public Sprite sprite;
}

[Serializable]
public class PortraitExpression
{
    public string baseKey;
    public string variantKey;  // MUST match an ActorVariantDef.key
    public Mouth mouth;
    public Gaze gaze;          // Center/Left/Right (portraits typically omit Hands)
    public Sprite sprite;
}
