using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using VNEngine;

[System.Serializable]
public enum Relationship { FRIEND, BFF };
[System.Serializable]
public enum Character { NONE, LEILANI, DEEPAK, SOFIA, BREANNA, MATTHEW, BEAU, ERIC, JIAH, CHARLI, JOSE, BRAD }

/*
[System.Serializable]
public struct FriendRelationship
{
    public string friend;
    public Relationship relationship;
}
*/

[System.Serializable]
public class Friend { 

    public Relationship relationship;
    public string characterName;
    public string scene;
    //image might be obsolete
    public Image status;
    public Sprite[] statusTypes;

    private int relationshipLevel;

    // Start is called before the first frame update
    // Start is called before the first frame update
    void Start()
    {
        // Load relationship status from StatsManager
        relationshipLevel = (int)StatsManager.Get_Numbered_Stat(characterName + "_relationship_level");
        status.sprite = statusTypes[relationshipLevel];

        // Load friendship status
        if (StatsManager.Get_Boolean_Stat(characterName + "_is_friend"))
        {
            Debug.Log(characterName + " is already a contact.");
        }
        else
        {
            Debug.Log(characterName + " is not a contact yet.");
        }
    }

    // Add this friend to the player's contact list
    public void AddToContacts()
    {
        StatsManager.Set_Boolean_Stat(characterName + "_is_friend", true);
        Debug.Log(characterName + " added to contacts.");
    }

    // Update the relationship level based on player's interaction
    public void UpdateRelationship(Relationship newStatus)
    {
        relationship = newStatus;
        int newStatusIndex = (int)newStatus;
        StatsManager.Set_Numbered_Stat(characterName + "_relationship_level", newStatusIndex); // Save new level
        status.sprite = statusTypes[newStatusIndex];  // Update visual icon
    }

    // Set the current scene for the friend
    public void SetCurrentScene(string currentScene)
    {
        scene = currentScene;
        StatsManager.Set_String_Stat(characterName + "_last_scene", scene);
    }

    // Load the last scene the friend appeared in
    public string GetLastScene()
    {
        return StatsManager.Get_String_Stat(characterName + "_last_scene");
    }

}
