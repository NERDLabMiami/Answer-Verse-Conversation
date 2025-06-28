using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using VNEngine;

public class ClickOutsideToClose : MonoBehaviour
{

    public bool ContinueConversationOnClick = false;
    public GameObject defaultSelectionOnClose;
    void Update()
    {
        if (Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1) || Input.GetMouseButtonDown(2) || Input.GetKey(KeyCode.Escape))
        {
            this.gameObject.SetActive(ClickingSelfOrChild());
            if(ContinueConversationOnClick)
            {
                VNSceneManager.Waiting_till_true = true;
            }
        }
    }
    private bool ClickingSelfOrChild()
    {
        RectTransform[] rectTransforms = GetComponentsInChildren<RectTransform>();
        foreach (RectTransform rectTransform in rectTransforms)
        {
            if (EventSystem.current.currentSelectedGameObject == rectTransform.gameObject)
            {
                return true;
            };
        }
        EventSystem.current.SetSelectedGameObject(defaultSelectionOnClose);
        return false;
    }
}