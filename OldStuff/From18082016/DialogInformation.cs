using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class DialogInformation : MonoBehaviour
{
    public enum ResponseRequirement { None, Stat, Item, Affection };
    public enum EventType { Normal, EventBranch };
    public enum Emotion { Normal, Sad, Happy, Angry, Confused };
    public enum LineType { Default, JumpToIfSeen, BreakIfSeen, SpecialAction };
    public enum JumpType { JumpToKeyword, JumpToLine };
    public enum RewardType { Event, Item, Keyword, UnlockCharInfo, ChangeState, Affection };
    public enum SpecialAction { Vendor, Gifting };

    [System.Serializable]
    public class DialogLine
    {
        public LineType type;

        public string line = "";
        public Emotion emotion;
        public bool isPrompt = false;

        public string positiveOption = "Yes";
        public int whenPositive;

        public string negativeOption = "No";
        public int whenNegative;

        public int whenSeen;

        public SpecialAction action;
    }


    [System.Serializable]
    public class Reward
    {
        public RewardType type;
        public int eventName;
        public bool setDone;
        public int itemId;
        public int itemCount;
        public string keyword;
        public int affectionPlus;
    }

    [System.Serializable]
    public class EventBranch
    {
        public int eventName;
        public int jumpTarget;
    }

    [System.Serializable]
    public class DialogEvent
    {
        public string eventTag = "";

        public EventType eventType;
        public List<EventBranch> eventBranches = new List<EventBranch>();
        public int ifNoBranchFound;

        public ResponseRequirement requirement;
        public CharacterStats.StatType statRequirement;
        public float statRequirementValue;
        public bool requirementElse;
        public int jumpIfMet;
        public int jumpIfNotMet;
        public int requiredAffection;

        public int itemId = -1;
        public int eventName;
        public bool folded = false;
        public bool seen = false;
        public JumpType jumpType;

        public List<DialogLine> lines = new List<DialogLine>();
        public List<Reward> rewards = new List<Reward>();
    }

    [System.Serializable]
    public class Response
    {
        public string keyword = "";
        public string tag = "";
        public bool folded = false;

        public List<DialogEvent> events = new List<DialogEvent>();

        public DialogEvent GetEvent(string t)
        {
            foreach(DialogEvent eve in events)
            {
                if(eve.eventTag == t)
                {
                    return eve;
                }
            }

            return null;
        }
    }

    public List<Response> responses = new List<Response>();

    DialogHandler dialogHandler;

    public Response GetResponse(string keyword)
    {
        foreach(Response response in responses)
        {
            if(response.keyword == keyword)
            {
                return response;
            }
        }

        return null;
    }

    void Start()
    {

    }

    void Update()
    {

    }
}
