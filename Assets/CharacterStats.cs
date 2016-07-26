using UnityEngine;
using System.Collections;

public class CharacterStats : MonoBehaviour
{
    public enum StatType { Health, Attack, ElementalBoost, DefensePhysical, DefenseRanged, DefenseFire, DefenseIce, Stamina, Intelligence, Confidence, PainEndurance, AuraColor };

    [System.Serializable]
    public class BaseStat
    {
        [Header("Basic")]
        public int level;
        public int scale;
        public int baseValue;
    }

    [System.Serializable]
    public class CommonStat : BaseStat
    {
        [Header("Values")]
        public float currentValue;
        public float maxValue;

        public void UpdateValues()
        {
            maxValue = baseValue + scale * level;
        }
    }

    [System.Serializable]
    public class StatDetail : BaseStat
    {
        [Header("Detail")]
        public float originalValue;
        public float bonusValue;
        
        public float GetRealValue()
        {
            return originalValue + bonusValue;
        }

        public void UpdateValues()
        {
            originalValue = baseValue + scale * level;
        }
    }

    [System.Serializable]
    public class BattleStat
    {
        public StatDetail physical;
        public StatDetail ranged;
        public StatDetail fire;
        public StatDetail ice;

        public void UpdateValues()
        {
            physical.UpdateValues();
            ranged.UpdateValues();
            fire.UpdateValues();
            ice.UpdateValues();
        }
    }


    [System.Serializable]
    public class Health : CommonStat
    {

    }

    [System.Serializable]
    public class Stamina : CommonStat
    {
        public float recoverySpeed;
        public float endurance;
    }

    [Header("HP/ST")]
    public Health health = new Health();
    public Stamina stamina = new Stamina();

    [Header("ATK/DEF")]
    public StatDetail attack = new StatDetail();
    public StatDetail elementalBoost = new StatDetail();
    public BattleStat defense = new BattleStat();

    [Header("Other")]
    public float intelligence;
    public float confidence;
    public float painEndurance;
    public Color auraColor;

    bool cooldown = false;
    float cooldownValue = 0;

    public void InflictDamage(float points)
    {
        health.currentValue -= points;
    }

    public bool IsDead()
    {
        return health.currentValue <= 0;
    }

    public bool IsSmartEnough(float limit)
    {
        return intelligence >= limit;
    }

    public bool HasStamina()
    {
        return stamina.currentValue > 0;
    }

    public bool CanDash()
    {
        return !cooldown;
    }

    public void DashStamina()
    {
        float limit = 100 - stamina.endurance;
        if (stamina.currentValue > 0)
        {
            stamina.currentValue -= limit;
            cooldown = true;
            if(stamina.currentValue <= 0)
            {
                cooldownValue = -1;
                stamina.currentValue = 0;
            }
            else
            {
                cooldownValue = 20;
            }
        }
    }

    void Start()
    {
        health.level = Random.Range(1, 100);
        health.UpdateValues();
        health.currentValue = health.maxValue;

        stamina.level = Random.Range(1, 100);
        stamina.UpdateValues();
        stamina.currentValue = stamina.maxValue;
        stamina.recoverySpeed = Random.Range(1, 100);
        stamina.endurance = Random.Range(1, 100);

        attack.level = Random.Range(1, 100);
        attack.UpdateValues();

        defense.fire.level = Random.Range(1, 100);
        defense.ice.level = Random.Range(1, 100);
        defense.physical.level = Random.Range(1, 100);
        defense.ranged.level = Random.Range(1, 100);
        defense.UpdateValues();

        intelligence = Random.Range(1, 100);
        confidence = Random.Range(1, 100);
        painEndurance = Random.Range(1, 100);
    }

    void Update()
    {
        if(cooldown)
        {
            if(cooldownValue == -1)
            {
                if(stamina.currentValue < stamina.maxValue)
                {
                    stamina.currentValue += Time.deltaTime * stamina.recoverySpeed;
                }
                else
                {
                    cooldownValue = 0;
                    cooldown = false;
                }
            }
            else
            {
                if (cooldownValue > 0)
                {
                    cooldownValue -= stamina.recoverySpeed * Time.deltaTime;
                }
                else
                {
                    cooldownValue = 0;
                    cooldown = false;
                }
            }
        }
        else if (stamina.currentValue < stamina.maxValue)
        {
            stamina.currentValue += Time.deltaTime * stamina.recoverySpeed;
        }
    }

}
