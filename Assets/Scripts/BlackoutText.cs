using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using VNEngine;

public class BlackoutText : MonoBehaviour
{
    public string blackoutStart = "[BLACKOUT]";
    public string blackoutEnd = "[/BLACKOUT]";
    public char blackoutCharacter = '-';
    public static string[] profaneWords = { "shit", "shitting", "shittin'", "fuck", "fucking", "fucked", "asshole" };


    public bool filterProfanity = false;

    private DialogueNode[] dialogueNodes;


    // Start is called before the first frame update
    void Start()
    {
        dialogueNodes = GetComponentsInChildren<DialogueNode>();
        for(int i = 0; i < dialogueNodes.Length; i++)
        {
            if(dialogueNodes[i].text.Contains(blackoutStart))
            {
                
                dialogueNodes[i].text = Censor(dialogueNodes[i].text, blackoutStart, blackoutEnd, blackoutCharacter);
            }

            if (filterProfanity)
            {
                dialogueNodes[i].text = FilterProfanity(dialogueNodes[i].text);
            }


        }


    }

    public static string Censor(string input, string startCensorString, string endCensorString, char censorOutput)
    {
        string result = input;

        int startIndex = input.IndexOf(startCensorString);
        int endIndex = input.IndexOf(endCensorString);

        while (startIndex != -1 && endIndex != -1 && endIndex > startIndex)
        {
            string textToReplace = input.Substring(startIndex + startCensorString.Length, endIndex - startIndex - endCensorString.Length);
            string replacement = new string(censorOutput, textToReplace.Length);
            result = result.Substring(0, startIndex) + replacement + result.Substring(endIndex + endCensorString.Length);

            startIndex = result.IndexOf(startCensorString);
            endIndex = result.IndexOf(endCensorString);
        }

        return result;
    }


    public static string FilterProfanity(string inputText)
    {
        string filteredText = inputText;

        foreach (string profaneWord in profaneWords)
        {
            // Replace exact matches of the profane word
            filteredText = Regex.Replace(filteredText, $@"\b{Regex.Escape(profaneWord)}\b", "****", RegexOptions.IgnoreCase);

            // Add more custom logic for pattern-based filtering if needed
            // For example: filteredText = Regex.Replace(filteredText, profaneWord, "****", RegexOptions.IgnoreCase);
        }

        return filteredText;
    }


}

