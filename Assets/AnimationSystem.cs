using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class AnimationSystem : MonoBehaviour
{
    [System.Serializable]
    public class Frame
    {
        public Sprite sprite;
    }

    [System.Serializable]
    public class Animation
    {
        public string tag = "";
        public List<Frame> frames = new List<Frame>();
        public float speed;
        public bool flipX = false;
        public bool flipY = false;
        public int sortOrder = 0;
        public bool takeOver = false;

        public Frame getFrame(int index)
        {
            return frames[index];
        }
    }

    public SpriteRenderer target;
    int index = 0;
    float duration = 0;
    
    string play = "";
    bool playing = false;
    string prfix = "";
    bool takeover = false;

    [System.Serializable]
    public class Group
    {
        public string tag = "";
        public List<Animation> animations = new List<Animation>();
    }

    public List<Group> groups = new List<Group>();

    public void Play(string p)
    {
        if(takeover)
        {
            return;
        }
        if(p != play)
        {
            index = 0;
            duration = 0;
        }
        play = p;
    }

    public void Stop()
    {
        play = "";
        index = 0;
        duration = 0;
        target.sprite = null;
    }

    public void Run(string prefix)
    {
        prfix = prefix;
        foreach(Group g in groups)
        {
            if(g.tag == prefix)
            {
                foreach(Animation a in g.animations)
                {
                    if(a.tag == play)
                    {
                        Frame frame = a.getFrame(index);
                        target.flipX = a.flipX;
                        target.flipY = a.flipY;
                        target.sprite = frame.sprite;
                        target.sortingOrder = a.sortOrder;
                        takeover = a.takeOver;
                        if (duration < a.speed)
                        {
                            duration += Time.deltaTime;
                        }
                        else
                        {
                            duration = 0;
                            if(index < a.frames.Count-1)
                            {
                                index += 1;
                            }
                            else
                            {
                                index = 0;
                                playing = false;
                                if(a.takeOver && takeover)
                                {
                                    takeover = false;
                                }
                            }
                        }
                        break;
                    }
                }
                break;
            }
        }
    }

    void OnGUI()
    {
        GUI.Label(new Rect(Screen.width - 100, Screen.height - 100, 100, 100), prfix + "\n" + index + "\n" + duration);
    }

}
