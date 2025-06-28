using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using OGD;
using System.Text;

public class Logging : MonoBehaviour
{
    public string appId;
    public int appVersion;
    public int clientLogVersion;
    public bool debugMode = true;

    private OGDLog m_Logger;
    public static Logging Instance { get; private set; }


    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            // Read appId from steam_appid.txt
            string appIdPath = Application.dataPath + "/../steam_appid.txt";
            if (System.IO.File.Exists(appIdPath))
            {
                appId = System.IO.File.ReadAllText(appIdPath).Trim();
            }
            else
            {
                Debug.LogError("steam_appid.txt not found!");
            }
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
            return;
        }
    }

    private IEnumerator Start()
    {
        m_Logger = new OGDLog(appId, appVersion);
        m_Logger.SetUserId("default");
        m_Logger.SetDebug(debugMode);

        while (!m_Logger.IsReady())
            yield return null;

        Debug.LogFormat("current session id: {0}", m_Logger.GetSessionId().ToString("X16"));

        m_Logger.GameState("{\"platform\":16}");

        var sb = new StringBuilder();
        sb.Append("{\"platform\":16}");
        m_Logger.GameState(sb);

    }


    public void LogPlayerChoice(string choice, int choiceIndex, Dictionary<int, string> order, List<int> randomizedOrder)
    {
        if(m_Logger.IsReady()) {
            m_Logger.BeginEvent("playerChoice");
            m_Logger.EventParam("choice", choice);
            m_Logger.EventParam("choiceIndex", choiceIndex); 
            m_Logger.EventParamJson("order", JsonUtility.ToJson(order));
            m_Logger.EventParamJson("random_order", JsonUtility.ToJson(randomizedOrder));
            m_Logger.SubmitEvent();
        }
        else
        {
            Debug.Log("OGD Logger Not Ready...");
        }
    }

}
