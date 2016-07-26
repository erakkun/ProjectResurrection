using UnityEngine;
using System.Collections;

public class ActionRecognition : MonoBehaviour
{
    public GameObject hintToEnable;
    public CharacterPlayer actioner;
    public DialogHandler dialogHandler;

    GameObject setOn;

    void Update()
    {
        if(!actioner)
        {
            Debug.Log("CharacterMaster is missing.");
            return;
        }
        if(!hintToEnable)
        {
            Debug.Log("HintToEnable is missing.");
            return;
        }
        if(!dialogHandler)
        {
            Debug.Log("DialogHandler is missing.");
            return;
        }

        if (!actioner.canAction && hintToEnable.activeInHierarchy)
        {
            hintToEnable.SetActive(false);
        }
        else if(actioner.canAction && setOn && !hintToEnable.activeInHierarchy)
        {
            hintToEnable.SetActive(true);
        }
    }

    void OnTriggerEnter2D(Collider2D col)
    {
        if(actioner.canAction)
        {
            Debug.Log("Trigger enter");
            setOn = col.gameObject;
            hintToEnable.SetActive(true);
        }
    }

    void OnTriggerExit2D(Collider2D col)
    {
        if (actioner.canAction)
        {
            Debug.Log("Trigger exit");
            setOn = null;
            hintToEnable.SetActive(false);
        }
    }

    public void TriggerAction()
    {
        if(setOn)
        {
            Debug.Log("Ask for action from " + setOn);
            ActionMaster actionMaster = setOn.GetComponent<ActionMaster>();
            if(actionMaster)
            {
                actionMaster.DoOnTrigger();
            }
            else
            {
                Debug.Log("Not found, checking for NPC from " + setOn);
                CharacterNPC npc = setOn.GetComponent<CharacterNPC>();
                if(npc)
                {
                    dialogHandler.Activate(actioner, npc);
                }
                else
                {
                    Debug.Log("No response");
                }
            }
        }
        else
        {
            Debug.Log("There's nothing there");
        }
    }
}
