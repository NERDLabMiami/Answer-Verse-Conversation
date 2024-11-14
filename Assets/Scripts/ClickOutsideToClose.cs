using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using VNEngine;

public class ClickOutsideToClose : MonoBehaviour
{

    public bool ContinueConversationOnClick = false;

    void Update()
    {
        if (Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1) || Input.GetMouseButtonDown(2))
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
        return false;
    }
}