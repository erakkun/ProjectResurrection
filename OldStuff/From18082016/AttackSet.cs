using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class AttackSet : MonoBehaviour
{
    public enum AttackType {PHYSICAL, RANGED, BOOST};
    public enum ElementType {NONE, FIRE, ICE};

    [System.Serializable]
    public class Action
    {
        [System.Serializable]
        public class ActionTarget
        {
            public GameObject hitObject;
            public Vector2 position = Vector2.zero;
            public Vector2 movement = Vector2.zero;
            public float lifeTime = 0.1f;
            public float powerMultiplier = 1f;

            public ActionTarget(ActionTarget t)
            {
                this.hitObject = t.hitObject;
                this.position = new Vector2(t.position.x, t.position.y);
                this.movement = new Vector2(t.movement.x, t.movement.y);
                this.lifeTime = t.lifeTime;
                this.powerMultiplier = t.powerMultiplier;
            }
        }

        [Header("Targets")]
        public List<ActionTarget> target = new List<ActionTarget>();

        [Header("Delay")]
        public float delayBefore = 0.1f;
        public float delayAfter = 0.1f;

        public Action(Action t)
        {
            this.target.Clear();
            foreach(ActionTarget tt in t.target)
            {
                this.target.Add(new ActionTarget(tt));
            }
            this.delayAfter = t.delayAfter;
            this.delayBefore = t.delayBefore;
        }
    }

    [Header("Attack information")]
    public string attackName = "";
    [TextArea(3, 10)]
    public string description = "";
    public int requiredIntelligence;
    public float basicPower;
    public float heavyness;
    public AttackType attackType;
    public ElementType element;

    [Header("Action information")]
    public List<Action> actions = new List<Action>();
    public float cooldown = 0f;
    public float animationCooldown = 0f;
    public bool blockMovement = true;
    public bool bullet = false;
    public bool useLifeTime = true;
    public bool destroyOnContact = true;
    public bool characterFollows = false;
    int index = 0;

    bool attacking = false;

    public void Copy(AttackSet b)
    {
        this.actions.Clear();
        foreach (Action tt in b.actions)
        {
            this.actions.Add(new Action(tt));
        }
        this.cooldown = b.cooldown;
        this.blockMovement = b.blockMovement;
        this.bullet = b.bullet;
        this.useLifeTime = b.useLifeTime;
        this.requiredIntelligence = b.requiredIntelligence;
        this.basicPower = b.basicPower;
        this.attackType = b.attackType;
        this.element = b.element;
        this.name = b.name;
        this.attackName = this.name;
        this.description = b.description;
        this.heavyness = b.heavyness;
        this.characterFollows = b.characterFollows;
        this.destroyOnContact = b.destroyOnContact;
    }

    public bool Attack(CharacterStats stats, CharacterMaster attacker, AnimationSystem anim, string direction)
    {
        if(attacking)
        {
            return false;
        }

        StartCoroutine(WaitForEnd(stats, attacker, anim, direction));

        return true;
    }

    public bool IsAttacking()
    {
        return attacking;
    }

    IEnumerator WaitForEnd(CharacterStats stats, CharacterMaster attacker, AnimationSystem anim, string direction)
    {
        attacking = true;
        index = 0;

        if (blockMovement)
        {
            attacker.canMove = false;
        }
        attacker.canRotate = false;
        attacker.canAction = false;

        anim.StopAll(true);
        anim.Play(this.attackName, -1, direction);

        while (index < actions.Count)
        {
            Action action = actions[index];
            yield return new WaitForSeconds(action.delayBefore);
            foreach(Action.ActionTarget target in action.target)
            {
                GameObject obj = null;
                if (target.hitObject)
                {
                    obj = Instantiate(target.hitObject, transform.position, transform.rotation) as GameObject;
                }
                else
                {
                    Debug.Log("HitObject prefab was no assigned.");
                }

                if(obj)
                {
                    obj.transform.parent = transform;
                    obj.transform.localPosition = target.position;

                    //obj.transform.position = new Vector3(Mathf.Round(obj.transform.position.x), Mathf.Round(obj.transform.position.y), Mathf.Round(obj.transform.position.z));

                    Hit hit = obj.GetComponent<Hit>();
                    if(hit)
                    {
                        hit.sender = transform.gameObject;
                        hit.destroyIn = target.lifeTime;
                        hit.movement = target.movement;
                        hit.useLifeTime = useLifeTime;

                        float power = basicPower;

                        Debug.Log(power + " + " + stats.attack.GetRealValue());
                        power += stats.attack.GetRealValue();

                        power *= target.powerMultiplier;

                        hit.power = power;
                        Debug.Log("Hit with " + power + " power!");

                        if (bullet)
                        {
                            obj.transform.parent = null;
                        }
                        hit.destroyOnContact = destroyOnContact;
                    }
                    else
                    {
                        Debug.Log("Couldn't find Hit component.");
                    }
                }
                else
                {
                    Debug.Log("Failed to instantiate obj.");
                }
            }
            yield return new WaitForSeconds(action.delayAfter);

            index += 1;
        }

        yield return new WaitForSeconds(animationCooldown);
        while(anim.IsPlaying(this.attackName, direction))
        {
            yield return null;
        }

        if (blockMovement)
        {
            attacker.canMove = true;
        }
        attacker.canRotate = true;
        attacker.canAction = true;

        yield return new WaitForSeconds(cooldown);

        attacking = false;
    }
}
