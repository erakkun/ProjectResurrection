using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class AnimationSystem : MonoBehaviour
{
    [System.Serializable]
    public class Frame
    {
        public Sprite sprite;
		public bool changeSortOrder = false;
		public bool changeFlip = false;

        public int sortOrder = 0;
        public bool flipX = false;
		public bool flipY = false;
    }

    [System.Serializable]
    public class Animation
    {
        public string tag = "";
        public List<Frame> frames = new List<Frame>();
        public float speed;
        public bool flipX = false;
        public bool flipY = false;

        public Frame getFrame(int index)
        {
			if(frames.Count > 0 && index >= 0 && index < frames.Count-1)
			{
	            return frames[index];
			}

			return false;
        }
    }

    public SpriteRenderer target;
    private int index = 0;
	private float duration = 0;
    private float speed = 1;
    
    private string play = "";
    private bool playing = false;
    private string prefix = "";
	private bool initialized = false;

    [System.Serializable]
    public class Group
    {
        public string tag = "";
        public List<Animation> animations = new List<Animation>();

		public Animation getAnimation(string tag)
		{
			foreach(Animation animation in animations)
            {
                if(animation.tag == tag)
                {
					return animation;
				}
			}

			return null;
		}
    }

    public List<Group> groups = new List<Group>();

	private Group getGroup(string tag)
	{
        foreach(Group group in groups)
        {
            if(group.tag == tag)
            {
				return group;
			}
		}

		return null;
	}

	void OnStart()
	{
		initialized = false;

		if(target)
		{
			initialized = true;
		}

		if(!initialized)
		{
			Debug.Log("AnimationSystem on " + gameObject.name + " not setup properly.");
		}
	}

	private void handle(Frame frame, Animation animation)
	{
		if(!initialized)
		{
			return;
		}

		target.flipX = animation.flipX;
		target.flipY = animation.flipY;

		if(frame.changeFlip)
		{
		    if(frame.flipX)
		    {
				target.flipX = (animation.flipX) ? false : frame.flipX;
		    }
		    else if(frame.flipY)
		    {
				target.flipY = (animation.flipY) ? false : frame.flipY;
		    }
		}

        if(frame.changeSortOrder)
        {
            target.sortingOrder = frame.sortOrder;
        }

		target.sprite = frame.sprite;

		if (duration < animation.speed)
        {
            duration += Time.deltaTime * speed;
        }
        else
        {
            duration = 0;
            if(index < animation.frames.Count-1)
            {
                index += 1;
            }
            else
            {
                index = 0;
                playing = false;
            }
        }
	}

    public void run(string prefix)
    {
		if(!initialized)
		{
			return;
		}

        this.prefix = prefix;

		Group group = getGroup(this.prefix);
		if(group != null)
		{
			Animation animation = group.getAnimation();
			if(animation != null)
			{
				Frame frame = animation.getFrame(index);
				if(frame != null)
				{
                    handle(frame, animation);
				}
			}
		}
    }

    void OnGUI()
    {
        GUI.Label(new Rect(Screen.width - 150, Screen.height - 100, 150, 100), prefix + "\n" + index + "\n" + duration);
    }

	public void play(string play)
    {
        if(play != this.play)
        {
            index = 0;
            duration = 99;
        }
        this.play = play;
    }

    public void setSpeed(float speed)
    {
        this.speed = speed;
    }

    public void stop()
    {
        play = "";
        index = 0;
        duration = 0;
        target.sprite = null;
    }
}
