using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;

[CustomEditor(typeof(CharacterValues))]

public class CharacterValuesEditor : Editor
{
    CharacterValues t;
    SerializedObject GetTarget;

    SerializedProperty pointRandomMin;
    SerializedProperty pointRandomMax;

    SerializedProperty points;
    SerializedProperty values;

    public class SerializedPoint
    {
        public string title;
        public string label;
        public SerializedProperty main;
        public SerializedProperty value;
        public SerializedProperty bonus;

        public SerializedPoint(string n, string l, SerializedProperty reference)
        {
            title = n;
            label = l;
            main = reference.FindPropertyRelative(title);
            value = main.FindPropertyRelative("value");
            bonus = main.FindPropertyRelative("bonus");
        }
    }

    List<SerializedPoint> serializedPoints = new List<SerializedPoint>();

    public class SerializedValue
    {
        public string title;
        public string label;
        public SerializedProperty main;

        public SerializedProperty useCurrentMax;
        public SerializedProperty useLevels;

        public SerializedProperty currentValue;
        public SerializedProperty maxValue;
        public SerializedProperty value;
        public SerializedProperty bonus;
        public SerializedProperty baseValue;
        public SerializedProperty step;

        public SerializedProperty reference;
        public SerializedProperty referenceValue;
        public SerializedProperty referenceBonus;

        public SerializedValue(string n, string l, SerializedProperty reference)
        {
            title = n;
            label = l;
            main = reference.FindPropertyRelative(title);

            useCurrentMax = main.FindPropertyRelative("useCurrentMax");
            useLevels = main.FindPropertyRelative("useLevels");
            currentValue = main.FindPropertyRelative("currentValue");
            maxValue = main.FindPropertyRelative("maxValue");
            value = main.FindPropertyRelative("value");
            bonus = main.FindPropertyRelative("bonus");
            baseValue = main.FindPropertyRelative("baseValue");
            step = main.FindPropertyRelative("step");
            reference = main.FindPropertyRelative("reference");

            referenceValue = reference.FindPropertyRelative("value");
            referenceBonus = reference.FindPropertyRelative("bonus");
        }
    }

    List<SerializedValue> serializedValues = new List<SerializedValue>();

    void OnEnable()
    {
        t = (CharacterValues)target;
        t.Recalibrate();

        GetTarget = new SerializedObject(t);
        points = GetTarget.FindProperty("points");
        values = GetTarget.FindProperty("values");

        pointRandomMin = points.FindPropertyRelative("pointRandomMin");
        pointRandomMax = points.FindPropertyRelative("pointRandomMax");

        serializedPoints.Add(new SerializedPoint("vitality", "Vitality", points));
        serializedPoints.Add(new SerializedPoint("endurance", "Endurance", points));
        serializedPoints.Add(new SerializedPoint("strength", "Strength", points));
        serializedPoints.Add(new SerializedPoint("control", "Control", points));
        serializedPoints.Add(new SerializedPoint("resistance", "Resistance", points));
        serializedPoints.Add(new SerializedPoint("intelligence", "Intelligence", points));

        serializedValues.Add(new SerializedValue("health", "Health", values));
        serializedValues.Add(new SerializedValue("stamina", "Stamina", values));
        serializedValues.Add(new SerializedValue("painEndurance", "Pain Endurance", values));
        serializedValues.Add(new SerializedValue("staminaRecovery", "Stamina Recovery", values));
        serializedValues.Add(new SerializedValue("defensePhysical", "Physical Defense", values));
        serializedValues.Add(new SerializedValue("defensePlasma", "Plasma Defense", values));
        serializedValues.Add(new SerializedValue("defenseFire", "Fire Defense", values));
        serializedValues.Add(new SerializedValue("defenseIce", "Ice Defense", values));
        serializedValues.Add(new SerializedValue("defenseAuraColor", "Aura Color Defense", values));
        serializedValues.Add(new SerializedValue("defensePierce", "Pierce Resistance", values));
        serializedValues.Add(new SerializedValue("defenseLeech", "Leech Resistance", values));
        serializedValues.Add(new SerializedValue("defenseVirus", "Virus Resistance", values));
    }

    public override void OnInspectorGUI()
    {
        GetTarget.Update();

        EditorGUILayout.LabelField("Points", EditorStyles.boldLabel);

        foreach (SerializedPoint point in serializedPoints)
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PropertyField(point.value, new GUIContent(point.label), GUILayout.MaxWidth(200));

            EditorGUILayout.LabelField(" + ", GUILayout.MaxWidth(20));

            point.bonus.intValue = EditorGUILayout.IntField(point.bonus.intValue, GUILayout.MaxWidth(40));

            EditorGUILayout.LabelField(" = " + (point.value.intValue + point.bonus.intValue), GUILayout.MaxWidth(60));

            EditorGUILayout.EndHorizontal();
        }

        EditorGUILayout.BeginHorizontal();

        if (GUILayout.Button("Calculate random", GUILayout.MaxWidth(160)))
        {
            t.points.Randomize();
        }

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.PropertyField(pointRandomMin, GUIContent.none, GUILayout.MaxWidth(60));
        EditorGUILayout.LabelField(" - ", GUILayout.MaxWidth(20));
        EditorGUILayout.PropertyField(pointRandomMax, GUIContent.none, GUILayout.MaxWidth(60));
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.EndHorizontal();
        EditorGUILayout.Space();

        EditorGUILayout.LabelField("Values", EditorStyles.boldLabel);

        foreach (SerializedValue value in serializedValues)
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            EditorGUILayout.LabelField(value.label, GUILayout.MaxWidth(120));

            /*EditorGUILayout.BeginHorizontal();

            if(value.useCurrentMax.boolValue == true)
            {
                EditorGUILayout.PropertyField(value.currentValue, GUIContent.none, GUILayout.MaxWidth(60));
                EditorGUILayout.LabelField(" / ", GUILayout.MaxWidth(20));
                EditorGUILayout.LabelField(value.maxValue.intValue.ToString(), GUILayout.MaxWidth(60));
            }

            EditorGUILayout.EndHorizontal();*/

            EditorGUILayout.Space();

            if(value.useLevels.boolValue == true)
            {
                if(value.useCurrentMax.boolValue == true)
                {
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField("Current", GUILayout.MaxWidth(120));
                    value.currentValue.floatValue = EditorGUILayout.FloatField(value.currentValue.floatValue, GUILayout.MaxWidth(40));
                    EditorGUILayout.EndHorizontal();
                }

                int total = 0;

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Base + Bonus", GUILayout.MaxWidth(120));
                value.baseValue.intValue = EditorGUILayout.IntField(value.baseValue.intValue, GUILayout.MaxWidth(40));
                total += value.baseValue.intValue;
                EditorGUILayout.LabelField("+", GUILayout.MaxWidth(20));
                value.bonus.intValue = EditorGUILayout.IntField(value.bonus.intValue, GUILayout.MaxWidth(40));
                total += value.bonus.intValue;
                EditorGUILayout.LabelField(" = ", GUILayout.MaxWidth(20));
                EditorGUILayout.LabelField((total).ToString(), GUILayout.MaxWidth(40));
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Step * Level", GUILayout.MaxWidth(120));
                value.step.intValue = EditorGUILayout.IntField(value.step.intValue, GUILayout.MaxWidth(40));
                EditorGUILayout.LabelField("*", GUILayout.MaxWidth(20));
                EditorGUILayout.LabelField((value.referenceValue.intValue + value.referenceBonus.intValue).ToString(), GUILayout.MaxWidth(40));
                EditorGUILayout.LabelField(" = ", GUILayout.MaxWidth(20));
                int t = (value.step.intValue * (value.referenceValue.intValue + value.referenceBonus.intValue));
                total += t;
                EditorGUILayout.LabelField(t.ToString(), GUILayout.MaxWidth(40));
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Total", GUILayout.MaxWidth(120));
                EditorGUILayout.LabelField(total.ToString(), GUILayout.MaxWidth(40));
                EditorGUILayout.EndHorizontal();
            }
            else
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.PropertyField(value.value, GUIContent.none);
                EditorGUILayout.EndHorizontal();
            }

            EditorGUILayout.EndVertical();
        }

        if (GUILayout.Button("Default", GUILayout.MaxWidth(160)))
        {
            t.DefaultValues();
        }

        GetTarget.ApplyModifiedProperties();
    }
}
