using System.Collections.Generic;
using UnityEngine;

public static class JsonUtilityHack
{
    [System.Serializable]
    private class EntryList
    {
        public Dictionary<string, List<QuestionAnswerPair>> dict;
    }

    public static Dictionary<string, List<QuestionAnswerPair>> ParseDictionary(string rawJson)
    {
        // Surround raw JSON with a wrapper so Unity can parse it
        string wrappedJson = "{\"dict\":" + rawJson + "}";
        EntryList wrapper = JsonUtility.FromJson<EntryList>(wrappedJson);
        return wrapper.dict;
    }
}