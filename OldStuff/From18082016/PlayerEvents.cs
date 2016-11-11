using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PlayerEvents : MonoBehaviour
{
    [System.Serializable]
    public class Event
    {
        public string tag;
        public bool done = false;
    }

    public List<Event> events = new List<Event>();

    public bool EventDone(string tag)
    {
        foreach(Event eve in events)
        {
            if(eve.tag == tag)
            {
                return eve.done;
            }
        }

        return false;
    }

    public bool EventDone(int i)
    {
        for (int a = 0; a < events.Count; a++)
        {
            if (a == i)
            {
                return events[a].done;
            }
        }

        return false;
    }

    public void SetEventDone(int i, bool tru)
    {
        for (int a = 0; a < events.Count; a++)
        {
            if (a == i)
            {
                events[a].done = tru;
            }
        }
    }

	void Start ()
    {
	
	}
	
	void Update ()
    {
	
	}
}
