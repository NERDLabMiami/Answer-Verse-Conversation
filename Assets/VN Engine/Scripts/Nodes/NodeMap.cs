using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

namespace VNEngine
{
    // Not used in real code. Merely a template to copy and paste from when creating new nodes.
    public class NodeMap : Node
    {
        public Character character;
        public string locationScene;
        public bool addLocationToMap = true;

        // Called initially when the node is run, put most of your logic here
        public override void Run_Node()
        {
            List<CharacterLocation> characterLocations = PlayerPrefsExtra.GetList<CharacterLocation>("characterLocations", new List<CharacterLocation>());

// Remove *any* character already at this location
            characterLocations.RemoveAll(loc => loc.location == locationScene);

            Debug.Log($"Attempting to add character pin to Map : {character} at {locationScene}");

            if (addLocationToMap)
            {
                Debug.Log($"{character} added to Map");

                characterLocations.Add(new CharacterLocation
                {
                    character = character,
                    location = locationScene
                });
            }
            else
            {
                Debug.Log($"{character} removed from Map (via addLocationToMap = false)");
                // No need to remove here; already removed above
            }

            PlayerPrefsExtra.SetList("characterLocations", characterLocations);

            Finish_Node();
        }


        // What happens when the user clicks on the dialogue text or presses spacebar? Either nothing should happen, or you call Finish_Node to move onto the next node
        public override void Button_Pressed()
        {
            //Finish_Node();
        }


        // Do any necessary cleanup here, like stopping coroutines that could still be running and interfere with future nodes
        public override void Finish_Node()
        {
            StopAllCoroutines();

            base.Finish_Node();
        }
    }
}