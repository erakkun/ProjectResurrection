using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;

[CustomEditor(typeof(CharacterNPC))]

public class CharacterNPCEditor : Editor
{
    CharacterNPC t;
    SerializedObject GetTarget;
    SerializedProperty actionList;
    SerializedProperty pathActionList;
    SerializedProperty parameters;
    SerializedProperty attackActions;
    SerializedProperty ownAttacks;
    int ListSize;

    void OnEnable()
    {
        t = (CharacterNPC)target;
        GetTarget = new SerializedObject(t);
        SerializedProperty actionRef = GetTarget.FindProperty("mainAction");
        actionList = actionRef.FindPropertyRelative("actions");
        pathActionList = GetTarget.FindProperty("pathFindActions");
        parameters = GetTarget.FindProperty("parameters");
        attackActions = GetTarget.FindProperty("attackActions");
        ownAttacks = GetTarget.FindProperty("characterAttacks");
    }

    string[] GetOwnAttacks()
    {
        List<string> ret = new List<string>();

        CharacterAttacks attacks = t.gameObject.GetComponent<CharacterAttacks>();
        if(attacks)
        {
            foreach (AttackSet attack in attacks.possibleAttacks)
            {
                ret.Add(attack.gameObject.name);
            }
        }

        return ret.ToArray();
    }

    public override void OnInspectorGUI()
    {
        GetTarget.Update();

        EditorGUILayout.Space();

        SerializedProperty cannotMoveAgainst = GetTarget.FindProperty("cannotMoveAgainst");
        SerializedProperty npcState = GetTarget.FindProperty("npcState");
        SerializedProperty npcMood = GetTarget.FindProperty("npcMood");
        SerializedProperty staticPath = GetTarget.FindProperty("staticPath");

        SerializedProperty showParameters = GetTarget.FindProperty("showParameters");
        SerializedProperty showActions = GetTarget.FindProperty("showActions");
        SerializedProperty showAttacks = GetTarget.FindProperty("showAttacks");

        EditorGUILayout.PropertyField(cannotMoveAgainst);
        EditorGUILayout.PropertyField(npcState);

        EditorGUILayout.Space();

        if (GUILayout.Button("Parameters"))
        {
            showParameters.boolValue = !showParameters.boolValue;
        }

        if (showParameters.boolValue)
        {
            EditorGUILayout.Space();
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            bool hasAggro = false;
            for (int a = 0; a < parameters.arraySize; a++)
            {
                EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);

                SerializedProperty param = parameters.GetArrayElementAtIndex(a);
                EditorGUILayout.PropertyField(param);
                if (param.intValue == (int)CharacterNPC.ParameterType.HasAggro)
                {
                    hasAggro = true;
                }

                if (GUILayout.Button("Remove", GUILayout.MaxWidth(80)))
                {
                    parameters.DeleteArrayElementAtIndex(a);
                }

                EditorGUILayout.EndHorizontal();
            }

            if (GUILayout.Button("Add New Parameter"))
            {
                parameters.InsertArrayElementAtIndex(parameters.arraySize);
            }

            if (hasAggro)
            {
                EditorGUILayout.Space();

                EditorGUILayout.LabelField("Aggro details", EditorStyles.boldLabel);

                SerializedProperty aggroLookDistance = GetTarget.FindProperty("aggroLookDistance");
                SerializedProperty aggroActDistance = GetTarget.FindProperty("aggroActDistance");
                SerializedProperty aggroSpeedMultiplier = GetTarget.FindProperty("aggroSpeedMultiplier");

                EditorGUILayout.PropertyField(aggroLookDistance);
                EditorGUILayout.PropertyField(aggroActDistance);
                EditorGUILayout.PropertyField(aggroSpeedMultiplier);

                EditorGUILayout.Space();
            }

            EditorGUILayout.EndVertical();
            EditorGUILayout.Space();
        }

        if (GUILayout.Button("Attack behaviour"))
        {
            showAttacks.boolValue = !showAttacks.boolValue;
        }

        if (showAttacks.boolValue)
        {
            EditorGUILayout.Space();
            //EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            for (int a = 0; a < attackActions.arraySize; a++)
            {
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);

                SerializedProperty attackActionRef = attackActions.GetArrayElementAtIndex(a);
                SerializedProperty aa_eventName = attackActionRef.FindPropertyRelative("eventName");
                SerializedProperty aa_uses = attackActionRef.FindPropertyRelative("usesAmount");
                SerializedProperty aa_actions = attackActionRef.FindPropertyRelative("actions");

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.PropertyField(aa_eventName);
                if (GUILayout.Button("Remove"))
                {
                    attackActions.DeleteArrayElementAtIndex(a);
                    break;
                }
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.PropertyField(aa_uses);

                for (int b = 0; b < aa_actions.arraySize; b++)
                {
                    EditorGUILayout.BeginVertical(EditorStyles.helpBox);

                    SerializedProperty aa_actionRef = aa_actions.GetArrayElementAtIndex(b);

                    SerializedProperty aa_type = aa_actionRef.FindPropertyRelative("type");
                    SerializedProperty aa_checkType = aa_actionRef.FindPropertyRelative("checkType");
                    SerializedProperty aa_stat = aa_actionRef.FindPropertyRelative("stat");
                    SerializedProperty aa_statType = aa_actionRef.FindPropertyRelative("statType");
                    SerializedProperty aa_statValue = aa_actionRef.FindPropertyRelative("statValue");
                    SerializedProperty aa_player = aa_actionRef.FindPropertyRelative("player");
                    SerializedProperty aa_playerValue = aa_actionRef.FindPropertyRelative("playerValue");
                    SerializedProperty aa_activateThis = aa_actionRef.FindPropertyRelative("activateThis");
                    SerializedProperty aa_customCooldown = aa_actionRef.FindPropertyRelative("customCooldown");
                    SerializedProperty aa_usesAmount = aa_actionRef.FindPropertyRelative("usesAmount");

                    EditorGUILayout.PropertyField(aa_type);

                    if(aa_type.intValue == (int)CharacterNPC.ActionType.Check)
                    {
                        EditorGUILayout.PropertyField(aa_checkType);

                        if (aa_checkType.intValue == (int)CharacterNPC.ActionCheckType.WhenStatIs)
                        {
                            EditorGUILayout.PropertyField(aa_stat);
                            EditorGUILayout.PropertyField(aa_statType);
                            EditorGUILayout.PropertyField(aa_statValue);
                        }
                        if (aa_checkType.intValue == (int)CharacterNPC.ActionCheckType.WhenPlayerIs)
                        {
                            EditorGUILayout.PropertyField(aa_player);
                            EditorGUILayout.PropertyField(aa_playerValue);
                        }
                    }
                    if (aa_type.intValue == (int)CharacterNPC.ActionType.Execute)
                    {
                        string[] attacks = GetOwnAttacks();
                        aa_activateThis.intValue = EditorGUILayout.Popup("Activate this", aa_activateThis.intValue, attacks);
                        EditorGUILayout.PropertyField(aa_customCooldown);
                    }
                    if (aa_type.intValue == (int)CharacterNPC.ActionType.ReduceUses)
                    {
                        EditorGUILayout.PropertyField(aa_usesAmount);
                    }

                    if (GUILayout.Button("Remove Statement", GUILayout.MaxWidth(150), GUILayout.MaxHeight(20)))
                    {
                        aa_actions.DeleteArrayElementAtIndex(b);
                    }

                    EditorGUILayout.EndVertical();
                }

                if (GUILayout.Button("Add New Statement"))
                {
                    aa_actions.InsertArrayElementAtIndex(aa_actions.arraySize);
                }

                EditorGUILayout.EndVertical();
            }

            EditorGUILayout.Space();

            if (GUILayout.Button("Add New Behaviour"))
            {
                attackActions.InsertArrayElementAtIndex(attackActions.arraySize);
            }

            //EditorGUILayout.EndVertical();
            EditorGUILayout.Space();
        }

        if (GUILayout.Button("Actions"))
        {
            showActions.boolValue = !showActions.boolValue;
        }

        if (showActions.boolValue)
        {
            EditorGUILayout.Space();
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.PropertyField(staticPath);

            for (int a = 0; a < actionList.arraySize; a++)
            {
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);

                SerializedProperty actionRef = actionList.GetArrayElementAtIndex(a);
                SerializedProperty a_type = actionRef.FindPropertyRelative("type");
                SerializedProperty a_randomizationTime = actionRef.FindPropertyRelative("randomizationTime");
                SerializedProperty a_randomizationTarget = actionRef.FindPropertyRelative("randomizationTarget");
                SerializedProperty a_min = actionRef.FindPropertyRelative("min");
                SerializedProperty a_max = actionRef.FindPropertyRelative("max");
                SerializedProperty a_speed = actionRef.FindPropertyRelative("speed");
                SerializedProperty a_time = actionRef.FindPropertyRelative("time");
                SerializedProperty a_target = actionRef.FindPropertyRelative("target");
                SerializedProperty a_realTarget = actionRef.FindPropertyRelative("realTarget");
                SerializedProperty a_target2 = actionRef.FindPropertyRelative("target2");
                SerializedProperty a_roamDistance = actionRef.FindPropertyRelative("roamDistance");

                EditorGUILayout.PropertyField(a_type);

                if (a_type.intValue == (int)CharacterNPC.MovementPointType.Idle)
                {
                    EditorGUILayout.PropertyField(a_randomizationTime);

                    if (a_randomizationTime.intValue == (int)CharacterNPC.MovementPointRandom.Constant)
                    {
                        EditorGUILayout.PropertyField(a_time);
                    }
                    if (a_randomizationTime.intValue == (int)CharacterNPC.MovementPointRandom.Random)
                    {
                        EditorGUILayout.PropertyField(a_min);
                        EditorGUILayout.PropertyField(a_max);
                    }
                }
                if (a_type.intValue == (int)CharacterNPC.MovementPointType.MoveTo)
                {
                    EditorGUILayout.PropertyField(a_randomizationTarget);
                    if (a_randomizationTarget.intValue == (int)CharacterNPC.MovementPointRandom.Constant)
                    {
                        EditorGUILayout.PropertyField(a_target);
                    }
                    if (a_randomizationTarget.intValue == (int)CharacterNPC.MovementPointRandom.Random)
                    {
                        a_target.vector2Value = EditorGUILayout.Vector2Field("X range", a_target.vector2Value);
                        a_target2.vector2Value = EditorGUILayout.Vector2Field("Y range", a_target2.vector2Value);
                    }
                    //EditorGUILayout.PropertyField(a_realTarget);

                    EditorGUILayout.PropertyField(a_speed);
                }
                if (a_type.intValue == (int)CharacterNPC.MovementPointType.LookAt)
                {
                    EditorGUILayout.PropertyField(a_randomizationTarget);
                    if (a_randomizationTarget.intValue == (int)CharacterNPC.MovementPointRandom.Constant)
                    {
                        EditorGUILayout.PropertyField(a_target);
                    }
                    if (a_randomizationTarget.intValue == (int)CharacterNPC.MovementPointRandom.Random)
                    {
                        a_target.vector2Value = EditorGUILayout.Vector2Field("X range", a_target.vector2Value);
                        a_target2.vector2Value = EditorGUILayout.Vector2Field("Y range", a_target2.vector2Value);
                    }

                    EditorGUILayout.PropertyField(a_randomizationTime);
                    if (a_randomizationTime.intValue == (int)CharacterNPC.MovementPointRandom.Constant)
                    {
                        EditorGUILayout.PropertyField(a_time);
                    }
                    if (a_randomizationTime.intValue == (int)CharacterNPC.MovementPointRandom.Random)
                    {
                        EditorGUILayout.PropertyField(a_min);
                        EditorGUILayout.PropertyField(a_max);
                    }
                }

                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("Remove", GUILayout.MaxWidth(150), GUILayout.MaxHeight(20)))
                {
                    actionList.DeleteArrayElementAtIndex(a);
                }

                if (a > 0)
                {
                    if (GUILayout.Button("Move Up", GUILayout.MaxWidth(80), GUILayout.MaxHeight(20)))
                    {
                        actionList.MoveArrayElement(a, a - 1);
                        GUI.FocusControl("");
                    }
                }
                if (a < actionList.arraySize - 1)
                {
                    if (GUILayout.Button("Move Down", GUILayout.MaxWidth(80), GUILayout.MaxHeight(20)))
                    {
                        actionList.MoveArrayElement(a, a + 1);
                        GUI.FocusControl("");
                    }
                }
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.EndVertical();
            }

            if (GUILayout.Button("Add New Action"))
            {
                actionList.InsertArrayElementAtIndex(actionList.arraySize);
            }

            EditorGUILayout.EndVertical();
            EditorGUILayout.Space();
        }

        GetTarget.ApplyModifiedProperties();
    }
}
