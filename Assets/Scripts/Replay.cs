using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Replay : MonoBehaviour
{
    public int startingNumberOfRewinds = 5;
    public TextMeshProUGUI rewindsLeft;
    private int rewinds;
    // Start is called before the first frame update
    void Start()
    {
        rewinds = PlayerPrefs.GetInt("rewinds left", startingNumberOfRewinds);
        rewindsLeft.text = rewinds.ToString("You have 0 rewinds left");
    }

    public void Rewind()
    {

        //TODO: Nothing is currently stopping rewinds from happening, just counting
        if(rewinds > 0)
        {
            rewinds--;
            PlayerPrefs.SetInt("rewinds left", rewinds);
            Debug.Log(rewinds + " rewinds remaining!");
        }

    }
}
