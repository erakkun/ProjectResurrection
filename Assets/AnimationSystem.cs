using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class AnimationPiece
{
    public string subtag = "Default";
    public AnimationComponent prefab;
    AnimationComponent reference;

    public AnimationPiece()
    {
        subtag = "Default";
    }

    public void Init(GameObject target)
    {
        reference = target.AddComponent<AnimationComponent>();
        reference.Copy(prefab);
        reference.Init(target);
    }

    public AnimationComponent Reference()
    {
        return reference;
    }
}

[System.Serializable]
public class AnimationInfo
{
    public string tag = "";
    public GameObject targetPicture;
    public List<AnimationPiece> animation = new List<AnimationPiece>();
    

    public void Init()
    {
        foreach(AnimationPiece anim in animation)
        {
            anim.Init(targetPicture);
        }
    }

    public AnimationComponent Get(string tag = "Default")
    {
        foreach (AnimationPiece anim in animation)
        {
            if(anim.subtag == tag)
            {
                return anim.Reference();
            }
        }

        return null;
    }
}

public class AnimationSystem : MonoBehaviour
{
    public List<AnimationInfo> animations = new List<AnimationInfo>();

    void Start()
    {

    }

    void Update()
    {

    }

    public void Init()
    {
        foreach(AnimationInfo animation in animations)
        {
            animation.Init();
        }
    }

    public AnimationComponent GetAnimation(string tag, string subtag = "Default")
    {
        foreach (AnimationInfo animation in animations)
        {
            if(animation.tag == tag && animation.Get(subtag))
            {
                return animation.Get(subtag);
            }
        }

        return null;
    }

    public void Play(string tag, int freezeAt = -1, string subtag = "Default")
    {
        AnimationComponent animation = GetAnimation(tag, subtag);
        if(animation != null)
        {
            animation.Play(freezeAt);
        }
    }

    public bool IsPlaying(string tag, string subtag = "Default")
    {
        AnimationComponent animation = GetAnimation(tag, subtag);
        if (animation != null)
        {
            return animation.IsPlaying();
        }

        return false;
    }

    public void Stop(string tag, bool reset = false, bool changeframe = true, string subtag = "Default")
    {
        AnimationComponent animation = GetAnimation(tag, subtag);
        if (animation != null)
        {
            animation.Stop(reset, changeframe);
        }
    }

    public void StopAll(bool reset = false)
    {
        foreach (AnimationInfo animation in animations)
        {
            foreach(AnimationPiece anim in animation.animation)
            {
                if(anim.Reference())
                {
                    anim.Reference().Stop(reset);
                }
            }
        }
    }

    public void Handle()
    {
        bool block = false;
        foreach (AnimationInfo animation in animations)
        {
            foreach(AnimationPiece animPiece in animation.animation)
            {
                AnimationComponent anim = animPiece.Reference();
                if (anim && (!anim.blockedByOthers || anim.blockedByOthers && !block))
                {
                    bool valueForBlock = anim.Handle();
                    if (anim.blocksOthers)
                    {
                        block = valueForBlock;
                    }
                }
            }
        }
    }
}
