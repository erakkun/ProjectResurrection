using UnityEngine;
using System.Collections;

public class VendorBase : MonoBehaviour
{
    bool activated = false;

    public bool IsActivated()
    {
        return activated;
    }

    public virtual void Activate()
    {
        WhenActivating();

        activated = true;
    }

    protected virtual void WhenActivating()
    {

    }

    public virtual void Deactivate()
    {
        WhenDeactivating();

        activated = false;
    }

    protected virtual void WhenDeactivating()
    {

    }

    void OnGUI()
    {
        if (activated)
        {
            GUI.Box(new Rect(50, 50, Screen.width - 100, Screen.height - 100), "");
            if (GUI.Button(new Rect(Screen.width - 150, Screen.height - 170, 40, 25), "Back"))
            {
                Deactivate();
            }
        }
    }
}
