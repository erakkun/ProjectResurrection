using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CharacterAttacks : MonoBehaviour
{
    public List<AttackSet> possibleAttacks = new List<AttackSet>();
    List<AttackSet> attackSet = new List<AttackSet>();
    AttackSet currentAttack = null;
    public GameObject attackSetObj;

    void Start ()
    {
        Init();
    }
	
	void Update ()
    {
	
	}

    public void Init()
    {
        if(!attackSetObj)
        {
            return;
        }

        foreach (AttackSet set in attackSetObj.GetComponents<AttackSet>())
        {
            Destroy(set);
        }
        attackSet.Clear();

        foreach (AttackSet set in possibleAttacks)
        {
            if (set)
            {
                AttackSet s = attackSetObj.AddComponent<AttackSet>();
                s.Copy(set);
                attackSet.Add(s);
            }
        }
    }

    public void Rotate(Quaternion newRotation)
    {
        attackSetObj.transform.rotation = newRotation;
    }

    public bool Attack(CharacterStats stats, CharacterMaster attacker, AnimationSystem anim, string direction)
    {
        return currentAttack.Attack(stats, attacker, anim, direction);
    }

    public bool IsAttacking()
    {
        bool attacking = false;
        foreach (AttackSet set in attackSet)
        {
            if(set.IsAttacking())
            {
                attacking = true;
                break;
            }
        }

        return attacking;
    }

    public void ChangeAttack(int i)
    {
        if (i >= possibleAttacks.Count)
        {
            Debug.Log("Trying to reach outside of the attacklist.");
            return;
        }
        if (possibleAttacks[i] == null)
        {
            Debug.Log("Attacklist element has nothing in it.");
            currentAttack = null;
            return;
        }

        currentAttack = attackSet[i];
    }

    public AttackSet GetCurrent()
    {
        return currentAttack;
    }
}
