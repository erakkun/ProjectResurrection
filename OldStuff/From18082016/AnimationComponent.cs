using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class AnimationComponent : MonoBehaviour
{
    public List<Sprite> frames = new List<Sprite>();
    public float animationSpeed = 0.5f;
    public bool endwithSameFrame = false;
    public bool blockedByOthers = false;
    public bool blocksOthers = false;

    SpriteRenderer rend;

    public int frame = 0;
    public float interval = 0;

    bool initialized = false;
    bool playing = false;

    public bool loop = true;

    public bool IsPlaying()
    {
        return playing;
    }

    public void Copy(AnimationComponent b)
    {
        this.frames.Clear();
        foreach (Sprite sprite in b.frames)
        {
            this.frames.Add(sprite);
        }
        this.animationSpeed = b.animationSpeed;
        this.endwithSameFrame = b.endwithSameFrame;
        this.loop = b.loop;
    }

    public void Init(GameObject obj)
    {
        rend = obj.GetComponent<SpriteRenderer>();
        if(!rend)
        {
            Debug.Log("Failed to initialize SpriteRenderer");
            return;
        }

        initialized = true;
    }

    public void Play(int freezeAt = -1)
    {
        if (!initialized)
        {
            Debug.Log("AnimationSystem not initialized");
            return;
        }

        if (!playing)
        {
            if (freezeAt == -1)
            {
                frame = 0;
                playing = true;
            }
            else
            {
                frame = freezeAt;
                playing = false;
            }

            interval = animationSpeed;

            UpdateFrame();
        }
    }

    public void Stop(bool reset = false, bool changeframe = true)
    {
        if (!initialized)
        {
            Debug.Log("AnimationSystem not initialized");
            return;
        }

        playing = false;
        if (reset)
        {
            frame = 0;
        }
        if (changeframe)
        {
            UpdateFrame();
        }
    }

    void UpdateFrame()
    {
        if (!initialized)
        {
            Debug.Log("AnimationSystem not initialized");
            return;
        }

        if (frame < 0 || frame >= frames.Count)
        {
            Debug.Log("Frame goes out of boundaries");
            return;
        }

        rend.sprite = frames[frame];
    }

    public bool Handle()
    {
        if (initialized && playing)
        {
            if (interval > 0)
            {
                interval -= Time.deltaTime;
            }
            else
            {
                interval = animationSpeed;
                if (frame < frames.Count - 1)
                {
                    frame += 1;
                    UpdateFrame();
                }
                else
                {
                    if (loop)
                    {
                        frame = 0;
                        UpdateFrame();
                    }
                    else
                    {
                        Stop(endwithSameFrame);
                    }
                }

                return true;
            }
        }

        return false;
    }
}
