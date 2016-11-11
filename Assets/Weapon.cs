using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Weapon : MonoBehaviour
{
    public enum Attr { Health, Stamina, PainEndurance, DefensePhysical, DefensePlasma, DefenseFire, DefenseIce, DefenseAuraColor, DefensePierce, DefenseLeech, DefenseVirus };

    [System.Serializable]
    public class AttrBonus
    {
        public Attr what;
        public int amount;
    }

    [System.Serializable]
    public class Plasma
    {
        public int charge;
        public float chargeSpeed;
        public List<AttrBonus> bonuses = new List<AttrBonus>();
    }

    [System.Serializable]
    public class PlasmaSplitVariable
    {
        public int noPlasma;
        public int withPlasma;
    }

    [System.Serializable]
    public class Mode
    {
        public float modeChangeCooldown;

        public PlasmaSplitVariable power;
        public int powerBonus;

        public int GetRealPower(bool plasma)
        {
            int ret = powerBonus;
            if(plasma)
            {
                ret += power.withPlasma;
            }
            else
            {
                ret += power.noPlasma;
            }

            return ret;
        }

        public PlasmaSplitVariable endurance;
        public int enduranceBonus;

        public int GetRealEndurance(bool plasma)
        {
            int ret = enduranceBonus;
            if (plasma)
            {
                ret += endurance.withPlasma;
            }
            else
            {
                ret += endurance.noPlasma;
            }

            return ret;
        }

        public float recoveryTime;
        public int recoveryTimeBonus;

        public float GetRealRecoveryTime()
        {
            return recoveryTime + recoveryTimeBonus;
        }

        public int staminaUse;
        public int staminaUseBonus;

        public int GetRealStaminaUse()
        {
            return staminaUse - staminaUseBonus;
        }

        public int plasmaDrain;
        public int plasmaDrainBonus;

        public int GetRealPlasmaDrain()
        {
            return plasmaDrain - plasmaDrainBonus;
        }
    }

    public string weaponName = "";
    public bool hasPlasma;
    public int endurance;
    public int maxEndurance;
    public int currentMode = 0;
    public int availablePlasmaSlots;
    public List<Plasma> plasmaSlots = new List<Plasma>();
    public List<Mode> modes = new List<Mode>();

    public int GetPlasmaCharge()
    {
        int val = 0;
        foreach(Plasma p in plasmaSlots)
        {
            val += p.charge;
        }

        return val;
    }

    public float GetPlasmaRecoverySpeed()
    {
        float val = 0;
        foreach (Plasma p in plasmaSlots)
        {
            val += p.chargeSpeed;
        }

        return val;
    }

    public Mode GetCurrentMode()
    {
        return modes[currentMode];
    }
}
