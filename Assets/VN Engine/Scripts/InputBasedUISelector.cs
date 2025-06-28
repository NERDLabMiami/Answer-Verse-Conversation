using UnityEngine;
using UnityEngine.EventSystems;

public class InputBasedUISelector : MonoBehaviour
{
    public GameObject firstSelected;

    private bool lastInputWasMouse = true;

    void Update()
    {
        if (Input.GetAxis("Mouse X") != 0 || Input.GetAxis("Mouse Y") != 0)
        {
            if (!lastInputWasMouse)
                SwitchToMouseMode();
        }
        else if (Input.GetButtonDown("Submit") || Input.GetAxis("Horizontal") != 0 || Input.GetAxis("Vertical") != 0)
        {
            if (lastInputWasMouse)
                SwitchToGamepadMode();
        }
    }

    void SwitchToMouseMode()
    {
        lastInputWasMouse = true;
        EventSystem.current.SetSelectedGameObject(null);
    }

    void SwitchToGamepadMode()
    {
        lastInputWasMouse = false;
        EventSystem.current.SetSelectedGameObject(firstSelected);
    }
}