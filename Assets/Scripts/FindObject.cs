using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VNEngine;

public class FindObject : MonoBehaviour
{
    public GameObject[] foundObjects;
    public GameObject startingLocation;
    public NodeSearch search;
    // Start is called before the first frame update

    //TODO: this is obsolete but still used in Linguistic Gaps
    public void Found()
    {
        search.Finish_Node();
        
        //outcome.gameObject.SetActive(false);
        ResetSearch();
        gameObject.SetActive(false);

    }

    public void ResetSearch()
    {
        for(int i = 0; i < GetComponentsInChildren<VNEngine.ClickToStartConversation>().Length; i++)
        {
            GetComponentsInChildren<VNEngine.ClickToStartConversation>()[i].gameObject.SetActive(false);
        } 
        startingLocation.SetActive(true);
    }

}
