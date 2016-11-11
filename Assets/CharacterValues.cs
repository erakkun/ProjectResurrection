using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[ExecuteInEditMode]

[System.Serializable]
public class CharacterValues : MonoBehaviour
{
    [System.Serializable]
    public class Point
    {
        public int value;
        public int bonus;

        public int GetRealValue()
        {
            return value + bonus;
        }
    }

    [System.Serializable]
    public class Points
    {
        public Point vitality;
        public Point endurance;
        public Point strength;
        public Point control;
        public Point resistance;
        public Point intelligence;

        public int pointRandomMin;
        public int pointRandomMax;

        public void Randomize()
        {
            vitality.value = Random.Range(pointRandomMin, pointRandomMax);
            endurance.value = Random.Range(pointRandomMin, pointRandomMax);
            strength.value = Random.Range(pointRandomMin, pointRandomMax);
            control.value = Random.Range(pointRandomMin, pointRandomMax);
            resistance.value = Random.Range(pointRandomMin, pointRandomMax);
            intelligence.value = Random.Range(pointRandomMin, pointRandomMax);
        }
    }

    [System.Serializable]
    public class Value
    {
        public bool useCurrentMax = false;
        public bool useLevels = true;

        public float currentValue;
        public int maxValue;
        public int value;
        public int bonus;

        public int baseValue;
        public int step;

        public Point reference = null;

        public int GetRealValue()
        {
            value = baseValue + bonus;
            if(reference != null)
            {
                value += step * reference.GetRealValue();
            }

            maxValue = value;

            return value;
        }
    }

    [System.Serializable]
    public class Values
    {
        public Value health;
        public Value stamina;
        public Value painEndurance;
        public Value staminaRecovery;
        public Value defensePhysical;
        public Value defensePlasma;
        public Value defenseFire;
        public Value defenseIce;
        public Value defenseAuraColor;
        public Value defensePierce;
        public Value defenseLeech;
        public Value defenseVirus;

        public void Calculate()
        {
            health.currentValue = health.GetRealValue();
            stamina.currentValue = stamina.GetRealValue();
            /*staminaRecovery.GetRealValue();
            painEndurance.GetRealValue();*/
            defenseAuraColor.GetRealValue();
            defenseFire.GetRealValue();
            defenseIce.GetRealValue();
            defenseLeech.GetRealValue();
            defensePhysical.GetRealValue();
            defensePierce.GetRealValue();
            defensePlasma.GetRealValue();
            defenseVirus.GetRealValue();
        }
    }

    public Points points;
    public Values values;

    void Start()
    {
        values.Calculate();
    }

    public void DefaultValues()
    {
        values.health.baseValue = 100;
        values.health.step = 100;

        values.stamina.baseValue = 70;
        values.stamina.step = 5;

        values.painEndurance.value = 20;
        values.staminaRecovery.value = 5;

        values.defensePhysical.baseValue = 10;
        values.defensePhysical.step = 1;

        values.defensePlasma.baseValue = 10;
        values.defensePlasma.step = 1;

        values.defenseFire.baseValue = 10;
        values.defenseFire.step = 1;

        values.defenseIce.baseValue = 10;
        values.defenseIce.step = 1;

        values.defenseAuraColor.baseValue = 10;
        values.defenseAuraColor.step = 1;

        values.defensePierce.baseValue = 10;
        values.defensePierce.step = 1;

        values.defenseLeech.baseValue = 10;
        values.defenseLeech.step = 1;

        values.defenseVirus.baseValue = 10;
        values.defenseVirus.step = 1;
    }

    public void Recalibrate()
    {
        values.health.reference = points.vitality;
        values.health.useCurrentMax = true;

        values.stamina.reference = points.endurance;
        values.stamina.useCurrentMax = true;

        values.painEndurance.useLevels = false;
        values.staminaRecovery.useLevels = false;

        values.defensePhysical.reference = points.resistance;
        values.defensePlasma.reference = points.resistance;
        values.defenseFire.reference = points.resistance;
        values.defenseIce.reference = points.resistance;
        values.defenseAuraColor.reference = points.resistance;
        values.defensePierce.reference = points.resistance;
        values.defenseLeech.reference = points.resistance;
        values.defenseVirus.reference = points.resistance;
    }

}
