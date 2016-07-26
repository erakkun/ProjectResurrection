using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(VendorHandler))]
[RequireComponent(typeof(GiftingHandler))]

public class DialogHandler : MonoBehaviour
{
    VendorHandler vendorHandler;
    GiftingHandler giftingHandler;

    CharacterPlayer charPlayer;
    CharacterNPC charNpc;
    DialogInformation currentInfo;

    string chosenKeyword;
    string chosenEvent;
    int index = 0;
    int chosenControl = 0;
    bool alreadymoving = false;

    string textToBeShown;
    string promptYes;
    string promptNo;

    public enum DialogState {None, KeywordPick, ShowDialog};
    DialogState state = DialogState.None;
    int dialogState = -1;

	void Start ()
    {
        vendorHandler = GetComponent<VendorHandler>();
        giftingHandler = GetComponent<GiftingHandler>();
	}
	
    public void Activate(CharacterPlayer self, CharacterNPC receiver)
    {
        charPlayer = self;
        charNpc = receiver;

        charNpc.LookAt(charPlayer.transform.position);

        currentInfo = charNpc.dialogInformation;

        charPlayer.canAction = false;
        charPlayer.canMove = false;
        charPlayer.canAttack = false;
        charPlayer.canRotate = false;

        charNpc.canAction = false;
        charNpc.canMove = false;
        charNpc.canAttack = false;
        charNpc.canRotate = false;

        state = DialogState.KeywordPick;
        dialogState = -1;
        chosenEvent = "DefaultEvent";
        chosenControl = 0;
    }

    public void Deactivate()
    {
        charPlayer.canAction = true;
        charPlayer.canMove = true;
        charPlayer.canAttack = true;
        charPlayer.canRotate = true;

        charNpc.canAction = true;
        charNpc.canMove = true;
        charNpc.canAttack = true;
        charNpc.canRotate = true;

        charPlayer = null;
        charNpc = null;
        currentInfo = null;

        state = DialogState.None;
        dialogState = -1;
    }

    void Update()
    {
        float left_y = Input.GetAxisRaw("Vertical");

        if (state == DialogState.KeywordPick)
        {
            if(!alreadymoving)
            { 
                if(left_y > 0)
                {
                    chosenControl -= 1;
                    alreadymoving = true;
                }
                if(left_y < 0)
                {
                    chosenControl += 1;
                    alreadymoving = true;
                }

                if (chosenControl < 0)
                {
                    chosenControl = charPlayer.playerKeyword.keywords.Count - 1;
                }
                if (chosenControl > charPlayer.playerKeyword.keywords.Count - 1)
                {
                    chosenControl = 0;
                }
            }
            else if(left_y == 0)
            {
                alreadymoving = false;
            }

            if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.JoystickButton2))
            {
                Deactivate();
            }
        }
        if(state == DialogState.ShowDialog)
        {
            if(dialogState == -1)
            {
                DialogInformation.Response response = currentInfo.GetResponse(chosenKeyword);
                if (response != null)
                {
                    DialogInformation.DialogEvent eve = response.GetEvent(chosenEvent);
                    if (eve != null)
                    {
                        if(eve.eventType == DialogInformation.EventType.EventBranch)
                        {
                            bool found = false;
                            foreach(DialogInformation.EventBranch branch in eve.eventBranches)
                            {
                                if (charPlayer.playerEvents.EventDone(eve.eventName))
                                {
                                    chosenEvent = response.events[branch.jumpTarget].eventTag;
                                    index = 0;
                                    dialogState = -1;
                                    found = true;
                                    break;
                                }
                            }

                            if(!found)
                            {
                                chosenEvent = response.events[eve.ifNoBranchFound].eventTag;
                                index = 0;
                                dialogState = -1;
                            }
                        }
                        /*if (eve.requirement == DialogInformation.ResponseRequirement.Event)
                        {
                            if(eventHandler.EventDone(eve.eventName))
                            {
                                chosenEvent = response.events[eve.jumpIfNotMet].eventTag;
                                index = 0;
                                dialogState = -1;
                            }
                            else
                            {
                                if(eve.requirementElse)
                                {
                                    chosenEvent = response.events[eve.jumpIfMet].eventTag;
                                    index = 0;
                                    dialogState = -1;
                                }
                                else
                                {
                                    index = 0;
                                    dialogState = 0;
                                }
                            }
                        }*/
                        else if (eve.requirement == DialogInformation.ResponseRequirement.Item)
                        {
                            //find if the item is in inventory here

                            index = 0;
                            dialogState = 0;
                        }
                        else if (eve.requirement == DialogInformation.ResponseRequirement.Affection)
                        {
                            //check the affection here

                            index = 0;
                            dialogState = 0;
                        }
                        else if (eve.requirement == DialogInformation.ResponseRequirement.Stat)
                        {
                            bool tru = false;

                            if (eve.statRequirement == CharacterStats.StatType.Attack)
                            {
                                if (charPlayer.characterStats.attack.GetRealValue() >= eve.statRequirementValue)
                                {
                                    tru = true;
                                }
                            }
                            else if (eve.statRequirement == CharacterStats.StatType.Confidence)
                            {
                                if (charPlayer.characterStats.confidence >= eve.statRequirementValue)
                                {
                                    tru = true;
                                }
                            }
                            else if (eve.statRequirement == CharacterStats.StatType.DefenseFire)
                            {
                                if (charPlayer.characterStats.defense.fire.GetRealValue() >= eve.statRequirementValue)
                                {
                                    tru = true;
                                }
                            }
                            else if (eve.statRequirement == CharacterStats.StatType.DefenseIce)
                            {
                                if (charPlayer.characterStats.defense.ice.GetRealValue() >= eve.statRequirementValue)
                                {
                                    tru = true;
                                }
                            }
                            else if (eve.statRequirement == CharacterStats.StatType.DefensePhysical)
                            {
                                if (charPlayer.characterStats.defense.physical.GetRealValue() >= eve.statRequirementValue)
                                {
                                    tru = true;
                                }
                            }
                            else if (eve.statRequirement == CharacterStats.StatType.DefenseRanged)
                            {
                                if (charPlayer.characterStats.defense.ranged.GetRealValue() >= eve.statRequirementValue)
                                {
                                    tru = true;
                                }
                            }
                            else if (eve.statRequirement == CharacterStats.StatType.ElementalBoost)
                            {
                                if (charPlayer.characterStats.elementalBoost.GetRealValue() >= eve.statRequirementValue)
                                {
                                    tru = true;
                                }
                            }
                            else if (eve.statRequirement == CharacterStats.StatType.Health)
                            {
                                if (charPlayer.characterStats.health.maxValue >= eve.statRequirementValue)
                                {
                                    tru = true;
                                }
                            }
                            else if (eve.statRequirement == CharacterStats.StatType.Stamina)
                            {
                                if (charPlayer.characterStats.stamina.maxValue >= eve.statRequirementValue)
                                {
                                    tru = true;
                                }
                            }
                            else if (eve.statRequirement == CharacterStats.StatType.Intelligence)
                            {
                                if (charPlayer.characterStats.intelligence >= eve.statRequirementValue)
                                {
                                    tru = true;
                                }
                            }
                            else if (eve.statRequirement == CharacterStats.StatType.PainEndurance)
                            {
                                if (charPlayer.characterStats.painEndurance >= eve.statRequirementValue)
                                {
                                    tru = true;
                                }
                            }

                            if (tru)
                            {
                                chosenEvent = response.events[eve.jumpIfNotMet].eventTag;
                                index = 0;
                                dialogState = -1;
                            }
                            else
                            {
                                if(eve.requirementElse)
                                {
                                    chosenEvent = response.events[eve.jumpIfMet].eventTag;
                                    index = 0;
                                    dialogState = -1;
                                }
                                else
                                {
                                    index = 0;
                                    dialogState = 0;
                                }   
                            }
                        }
                        else
                        {
                            index = 0;
                            dialogState = 0;
                        }
                    }
                    else
                    {
                        Deactivate();
                    }
                }
                else
                {
                    chosenKeyword = "Default";
                    chosenEvent = "DefaultEvent";

                    response = currentInfo.GetResponse(chosenKeyword);
                    if(response != null)
                    {
                        DialogInformation.DialogEvent eve = response.GetEvent(chosenEvent);
                        if (eve != null)
                        {
                            index = 0;
                            dialogState = -1;
                        }
                        else
                        {
                            Deactivate();
                        }
                    }
                    else
                    {
                        Deactivate();
                    }
                }
            }
            if(dialogState == 0)
            {
                DialogInformation.Response response = currentInfo.GetResponse(chosenKeyword);
                if(response != null)
                {
                    DialogInformation.DialogEvent dialogEvent = response.GetEvent(chosenEvent);
                    if (dialogEvent != null)
                    {
                        if (dialogEvent.lines.Count > 0 && index < dialogEvent.lines.Count)
                        {
                            DialogInformation.DialogLine currentLine = dialogEvent.lines[index];

                            if (currentLine.type == DialogInformation.LineType.Default)
                            {
                                textToBeShown = currentLine.line;
                                if (currentLine.isPrompt)
                                {
                                    dialogState = 3;
                                    chosenControl = 0;
                                    promptYes = currentLine.positiveOption;
                                    promptNo = currentLine.negativeOption;
                                    dialogEvent.seen = true;
                                }
                                else
                                {
                                    dialogState = 1;
                                }
                            }
                            if (currentLine.type == DialogInformation.LineType.BreakIfSeen)
                            {
                                if (dialogEvent.seen)
                                {
                                    Deactivate();
                                }
                                else
                                {
                                    dialogState = 2;
                                    dialogEvent.seen = true;
                                }
                            }
                            if (currentLine.type == DialogInformation.LineType.JumpToIfSeen)
                            {
                                if (dialogEvent.seen)
                                {
                                    chosenEvent = response.events[currentLine.whenSeen].eventTag;
                                    index = 0;
                                    dialogState = -1;
                                }
                                else
                                {
                                    dialogState = 2;
                                }
                            }
                            if (currentLine.type == DialogInformation.LineType.SpecialAction)
                            {
                                if(currentLine.action == DialogInformation.SpecialAction.Gifting || currentLine.action == DialogInformation.SpecialAction.Vendor)
                                {
                                    InformationBase vendor = currentInfo.gameObject.GetComponent<InformationBase>();
                                    if (vendor)
                                    {
                                        if (currentLine.action == DialogInformation.SpecialAction.Vendor)
                                        {
                                            Debug.Log("vendor");
                                            dialogState = 7;
                                            vendorHandler.Activate();
                                            vendorHandler.vendor = (VendorInformation)vendor;
                                        }
                                        if (currentLine.action == DialogInformation.SpecialAction.Gifting)
                                        {
                                            Debug.Log("gifting");
                                            dialogState = 8;
                                            giftingHandler.Activate();
                                            giftingHandler.vendor = (GiftingInformation)vendor;
                                        }
                                    }
                                }
                            }
                        }
                        else
                        {
                            if(index == dialogEvent.lines.Count)
                            {
                                dialogState = 6;
                            }
                            else
                            {
                                Deactivate();
                            }
                        }
                    }
                    else
                    {
                        Deactivate();
                    }
                }
            }
            else if(dialogState == 1)
            {
                if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.JoystickButton1))
                {
                    dialogState = 2;
                }
            }
            else if (dialogState == 2)
            {
                dialogState = 0;
                index += 1;
                textToBeShown = "";
            }
            else if (dialogState == 3)
            {
                if (!alreadymoving)
                {
                    if (left_y > 0)
                    {
                        chosenControl -= 1;
                        alreadymoving = true;
                    }
                    if (left_y < 0)
                    {
                        chosenControl += 1;
                        alreadymoving = true;
                    }

                    if (chosenControl < 0)
                    {
                        chosenControl = charPlayer.playerKeyword.keywords.Count - 1;
                    }
                    if (chosenControl > charPlayer.playerKeyword.keywords.Count - 1)
                    {
                        chosenControl = 0;
                    }
                }
                else if (left_y == 0)
                {
                    alreadymoving = false;
                }
            }
            else if (dialogState == 4 || dialogState == 5)
            {
                DialogInformation.Response response = currentInfo.GetResponse(chosenKeyword);
                if (response != null)
                {
                    DialogInformation.DialogEvent dialogEvent = response.GetEvent(chosenEvent);
                    if (dialogEvent != null)
                    {
                        if (dialogEvent.lines.Count > 0 && index < dialogEvent.lines.Count)
                        {
                            DialogInformation.DialogLine currentLine = dialogEvent.lines[index];

                            if (dialogState == 4)
                            {
                                chosenEvent = response.events[currentLine.whenPositive].eventTag;
                            }
                            if (dialogState == 5)
                            {
                                chosenEvent = response.events[currentLine.whenNegative].eventTag;
                            }
                            index = 0;
                            dialogState = -1;
                        }
                    }
                    else
                    {
                        Deactivate();
                    }
                }
            }
            else if (dialogState == 6)
            {
                DialogInformation.Response response = currentInfo.GetResponse(chosenKeyword);
                if (response != null)
                {
                    DialogInformation.DialogEvent dialogEvent = response.GetEvent(chosenEvent);
                    if (dialogEvent != null)
                    {
                        foreach(DialogInformation.Reward reward in dialogEvent.rewards)
                        {
                            if (reward.type == DialogInformation.RewardType.Event)
                            {
                                charPlayer.playerEvents.SetEventDone(reward.eventName, reward.setDone);
                            }
                            if (reward.type == DialogInformation.RewardType.Item)
                            {
                                //item additions here
                            }
                            if (reward.type == DialogInformation.RewardType.Affection)
                            {
                                //affection add here
                            }
                            if (reward.type == DialogInformation.RewardType.ChangeState)
                            {
                                //state change here
                            }
                            if (reward.type == DialogInformation.RewardType.UnlockCharInfo)
                            {
                                //unlock char info here
                            }
                            if (reward.type == DialogInformation.RewardType.Keyword)
                            {
                                bool found = false;
                                foreach(string word in charPlayer.playerKeyword.keywords)
                                {
                                    if(word == reward.keyword)
                                    {
                                        found = true;
                                    }
                                }
                                if(!found)
                                {
                                    charPlayer.playerKeyword.keywords.Add(reward.keyword);
                                }
                            }
                        }
                    }
                }

                Deactivate();
            }
            else if(dialogState == 7)
            {
                if(!vendorHandler.IsActivated())
                {
                    chosenEvent = "TradeOver";
                    index = 0;
                    dialogState = -1;
                }
            }
            else if (dialogState == 8)
            {
                if (!giftingHandler.IsActivated())
                {
                    chosenEvent = "OfferOverHappy";
                    index = 0;
                    dialogState = -1;
                }
            }
        }
    }

    void OnGUI()
    {
        if(state == DialogState.KeywordPick)
        {
            Rect rect = new Rect(5, 5, 150, 25);

            GUI.FocusControl(charPlayer.playerKeyword.keywords[chosenControl]);

            foreach (string word in charPlayer.playerKeyword.keywords)
            {
                GUI.SetNextControlName(word);
                if (GUI.Button(rect, word))
                {
                    chosenKeyword = word;
                    state = DialogState.ShowDialog;
                    dialogState = -1;
                }

                rect.y += rect.height;
            }
        }
        if(state == DialogState.ShowDialog)
        {
            GUI.Label(new Rect(0, Screen.height-100, Screen.width, 100), textToBeShown);

            if(dialogState == 3)
            {
                string[] words = new string[2] { promptYes, promptNo };
                GUI.FocusControl(words[chosenControl]);

                GUI.SetNextControlName(promptYes);
                if (GUI.Button(new Rect(Screen.width - 200, Screen.height - 100, 200, 20), promptYes))
                {
                    dialogState = 4;
                }
                GUI.SetNextControlName(promptNo);
                if (GUI.Button(new Rect(Screen.width - 200, Screen.height - 80, 200, 20), promptNo))
                {
                    dialogState = 5;
                }
            }
        }
    }
}
