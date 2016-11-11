using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;

[CustomEditor(typeof(DialogInformation))]

public class DialogInformationEditor : Editor
{
    DialogInformation t;
    SerializedObject GetTarget;
    SerializedProperty KeywordList;
    int ListSize;

    void OnEnable()
    {
        t = (DialogInformation)target;
        GetTarget = new SerializedObject(t);
        KeywordList = GetTarget.FindProperty("responses"); // Find the List in our script and create a refrence of it
    }

    string[] GetGameEvents()
    {
        List<string> ret = new List<string>();

        PlayerEvents[] eventHandlers = (PlayerEvents[])Resources.FindObjectsOfTypeAll(typeof(PlayerEvents));
        if(eventHandlers.Length > 0)
        {
            foreach(PlayerEvents.Event st in eventHandlers[0].events)
            {
                ret.Add(st.tag);
            }
        }

        return ret.ToArray();
    }

    string[] GetEventsHere(string keyword, string except = "")
    {
        List<string> ret = new List<string>();

        for (int i = 0; i < KeywordList.arraySize; i++)
        {
            SerializedProperty KeywordRef = KeywordList.GetArrayElementAtIndex(i);
            SerializedProperty k_keyword = KeywordRef.FindPropertyRelative("keyword");
            if(k_keyword.stringValue == keyword)
            {
                SerializedProperty EventList = KeywordRef.FindPropertyRelative("events");
                for(int a = 0; a < EventList.arraySize; a++)
                {
                    SerializedProperty EventRef = EventList.GetArrayElementAtIndex(a);
                    SerializedProperty e_eventTag = EventRef.FindPropertyRelative("eventTag");

                    if(e_eventTag.stringValue != except || e_eventTag.stringValue != "")
                    {
                        ret.Add(e_eventTag.stringValue);
                    }
                }
            }
        }

        return ret.ToArray();
    }

    public override void OnInspectorGUI()
    {
        GetTarget.Update();

        for (int i = 0; i < KeywordList.arraySize; i++)
        {
            SerializedProperty KeywordRef = KeywordList.GetArrayElementAtIndex(i);
            SerializedProperty k_keyword = KeywordRef.FindPropertyRelative("keyword");
            SerializedProperty k_events = KeywordRef.FindPropertyRelative("events");
            SerializedProperty k_folded = KeywordRef.FindPropertyRelative("folded");

            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button(k_keyword.stringValue))
            {
                k_folded.boolValue = !k_folded.boolValue;
            }

            EditorGUILayout.EndHorizontal();

            //k_folded.boolValue = EditorGUILayout.Foldout(k_folded.boolValue, k_keyword.stringValue);

            if (k_folded.boolValue)
            {
                EditorGUILayout.PropertyField(k_keyword);

                for (int a = 0; a < k_events.arraySize; a++)
                {
                    SerializedProperty EventRef = k_events.GetArrayElementAtIndex(a);
                    SerializedProperty e_eventTag = EventRef.FindPropertyRelative("eventTag");
                    SerializedProperty e_folded = EventRef.FindPropertyRelative("folded");

                    e_folded.boolValue = EditorGUILayout.Foldout(e_folded.boolValue, e_eventTag.stringValue);
                    if (e_folded.boolValue)
                    {
                        EditorGUILayout.BeginVertical(EditorStyles.helpBox);

                        SerializedProperty e_eventType = EventRef.FindPropertyRelative("eventType");
                        SerializedProperty e_requirement = EventRef.FindPropertyRelative("requirement");
                        SerializedProperty e_statRequirement = EventRef.FindPropertyRelative("statRequirement");
                        SerializedProperty e_statRequirementValue = EventRef.FindPropertyRelative("statRequirementValue");
                        SerializedProperty e_jumpIfNotMet = EventRef.FindPropertyRelative("jumpIfNotMet");
                        SerializedProperty e_requirementElse = EventRef.FindPropertyRelative("requirementElse");
                        SerializedProperty e_jumpIfMet = EventRef.FindPropertyRelative("jumpIfMet");
                        SerializedProperty e_requiredAffection = EventRef.FindPropertyRelative("requiredAffection");

                        SerializedProperty e_itemId = EventRef.FindPropertyRelative("itemId");
                        //SerializedProperty e_eventName = EventRef.FindPropertyRelative("eventName");

                        SerializedProperty e_lines = EventRef.FindPropertyRelative("lines");
                        SerializedProperty e_rewards = EventRef.FindPropertyRelative("rewards");

                        SerializedProperty e_eventBranches = EventRef.FindPropertyRelative("eventBranches");
                        SerializedProperty e_ifNoBranchFound = EventRef.FindPropertyRelative("ifNoBranchFound");

                        EditorGUILayout.PropertyField(e_eventTag);
                        EditorGUILayout.PropertyField(e_eventType);

                        EditorGUILayout.Space();

                        if (e_eventType.intValue == (int)DialogInformation.EventType.EventBranch)
                        {
                            for (int b = 0; b < e_eventBranches.arraySize; b++)
                            {
                                EditorGUILayout.BeginVertical(EditorStyles.helpBox);

                                SerializedProperty EventBranchRef = e_eventBranches.GetArrayElementAtIndex(b);
                                SerializedProperty eb_eventName = EventBranchRef.FindPropertyRelative("eventName");
                                SerializedProperty eb_jumpTarget = EventBranchRef.FindPropertyRelative("jumpTarget");

                                string[] gameEvents = GetGameEvents();
                                eb_eventName.intValue = EditorGUILayout.Popup("Event", eb_eventName.intValue, gameEvents);

                                string[] eventsHere = GetEventsHere(k_keyword.stringValue, e_eventTag.stringValue);
                                eb_jumpTarget.intValue = EditorGUILayout.Popup("Target", eb_jumpTarget.intValue, eventsHere);

                                EditorGUILayout.BeginHorizontal();
                                if (GUILayout.Button("Remove This Branch", GUILayout.MaxWidth(130), GUILayout.MaxHeight(20)))
                                {
                                    e_eventBranches.DeleteArrayElementAtIndex(b);
                                }
                                if (b > 0)
                                {
                                    if (GUILayout.Button("Move Up", GUILayout.MaxWidth(80), GUILayout.MaxHeight(20)))
                                    {
                                        e_eventBranches.MoveArrayElement(b, b - 1);
                                        GUI.FocusControl("");
                                    }
                                }
                                if (b < e_eventBranches.arraySize - 1)
                                {
                                    if (GUILayout.Button("Move Down", GUILayout.MaxWidth(80), GUILayout.MaxHeight(20)))
                                    {
                                        e_eventBranches.MoveArrayElement(b, b + 1);
                                        GUI.FocusControl("");
                                    }
                                }
                                EditorGUILayout.EndHorizontal();

                                EditorGUILayout.EndVertical();
                            }

                            string[] eventsHereOver = GetEventsHere(k_keyword.stringValue, e_eventTag.stringValue);
                            e_ifNoBranchFound.intValue = EditorGUILayout.Popup("If no branch", e_ifNoBranchFound.intValue, eventsHereOver);

                            if (GUILayout.Button("Add New Branch"))
                            {
                                e_eventBranches.InsertArrayElementAtIndex(e_eventBranches.arraySize);
                            }
                        }
                        if (e_eventType.intValue == (int)DialogInformation.EventType.Normal)
                        {
                            EditorGUILayout.LabelField("Requirements", EditorStyles.boldLabel);

                            EditorGUILayout.PropertyField(e_requirement);
                            /*if (e_requirement.intValue != (int)DialogInformation.ResponseRequirement.None)
                            {
                                EditorGUILayout.PropertyField(e_requirementType);
                            }*/
                            if (e_requirement.intValue == (int)DialogInformation.ResponseRequirement.Stat)
                            {
                                EditorGUILayout.PropertyField(e_statRequirement);
                                EditorGUILayout.PropertyField(e_statRequirementValue);
                                EditorGUILayout.PropertyField(e_requirementElse);

                                EditorGUILayout.LabelField("When met", EditorStyles.boldLabel);
                                string[] keywordsHere = GetEventsHere(k_keyword.stringValue, e_eventTag.stringValue);
                                e_jumpIfNotMet.intValue = EditorGUILayout.Popup("Event", e_jumpIfNotMet.intValue, keywordsHere);

                                if (e_requirementElse.boolValue)
                                {
                                    EditorGUILayout.LabelField("When not met", EditorStyles.boldLabel);
                                    keywordsHere = GetEventsHere(k_keyword.stringValue, e_eventTag.stringValue);
                                    e_jumpIfMet.intValue = EditorGUILayout.Popup("Event", e_jumpIfMet.intValue, keywordsHere);
                                }
                            }
                            if (e_requirement.intValue == (int)DialogInformation.ResponseRequirement.Item)
                            {
                                EditorGUILayout.PropertyField(e_itemId);
                                EditorGUILayout.PropertyField(e_requirementElse);

                                EditorGUILayout.LabelField("When met", EditorStyles.boldLabel);
                                string[] keywordsHere = GetEventsHere(k_keyword.stringValue, e_eventTag.stringValue);
                                e_jumpIfNotMet.intValue = EditorGUILayout.Popup("Event", e_jumpIfNotMet.intValue, keywordsHere);

                                if (e_requirementElse.boolValue)
                                {
                                    EditorGUILayout.LabelField("When not met", EditorStyles.boldLabel);
                                    e_jumpIfMet.intValue = EditorGUILayout.Popup("Event", e_jumpIfMet.intValue, keywordsHere);
                                }
                            }
                            if (e_requirement.intValue == (int)DialogInformation.ResponseRequirement.Affection)
                            {
                                EditorGUILayout.PropertyField(e_requiredAffection);
                                EditorGUILayout.PropertyField(e_requirementElse);

                                EditorGUILayout.LabelField("When met", EditorStyles.boldLabel);
                                string[] keywordsHere = GetEventsHere(k_keyword.stringValue, e_eventTag.stringValue);
                                e_jumpIfNotMet.intValue = EditorGUILayout.Popup("Event", e_jumpIfNotMet.intValue, keywordsHere);

                                if (e_requirementElse.boolValue)
                                {
                                    EditorGUILayout.LabelField("When not met", EditorStyles.boldLabel);
                                    e_jumpIfMet.intValue = EditorGUILayout.Popup("Event", e_jumpIfMet.intValue, keywordsHere);
                                }
                            }
                            /*if (e_requirement.intValue == (int)DialogInformation.ResponseRequirement.Event)
                            {
                                string[] gameEvents = GetGameEvents();
                                e_eventName.intValue = EditorGUILayout.Popup("Event", e_eventName.intValue, gameEvents);
                                EditorGUILayout.PropertyField(e_requirementElse);

                                EditorGUILayout.LabelField("When met", EditorStyles.boldLabel);
                                string[] keywordsHere = GetEventsHere(k_keyword.stringValue, e_eventTag.stringValue);
                                e_jumpIfNotMet.intValue = EditorGUILayout.Popup("Event", e_jumpIfNotMet.intValue, keywordsHere);

                                if (e_requirementElse.boolValue)
                                {
                                    EditorGUILayout.LabelField("When not met", EditorStyles.boldLabel);
                                    e_jumpIfMet.intValue = EditorGUILayout.Popup("Event", e_jumpIfMet.intValue, keywordsHere);
                                }
                            }*/

                            EditorGUILayout.Space();
                            EditorGUILayout.Separator();
                            EditorGUILayout.Space();

                            //EditorGUILayout.PropertyField(seen);

                            EditorGUILayout.LabelField("Lines", EditorStyles.boldLabel);

                            for (int b = 0; b < e_lines.arraySize; b++)
                            {
                                EditorGUILayout.BeginVertical(EditorStyles.helpBox);

                                SerializedProperty LineRef = e_lines.GetArrayElementAtIndex(b);
                                SerializedProperty type = LineRef.FindPropertyRelative("type");
                                SerializedProperty line = LineRef.FindPropertyRelative("line");
                                SerializedProperty emotion = LineRef.FindPropertyRelative("emotion");
                                SerializedProperty isPrompt = LineRef.FindPropertyRelative("isPrompt");
                                SerializedProperty positiveOption = LineRef.FindPropertyRelative("positiveOption");
                                SerializedProperty negativeOption = LineRef.FindPropertyRelative("negativeOption");
                                SerializedProperty whenPositive = LineRef.FindPropertyRelative("whenPositive");
                                SerializedProperty whenNegative = LineRef.FindPropertyRelative("whenNegative");
                                SerializedProperty whenSeen = LineRef.FindPropertyRelative("whenSeen");
                                SerializedProperty action = LineRef.FindPropertyRelative("action");

                                EditorGUILayout.PropertyField(type);

                                if (type.intValue == (int)DialogInformation.LineType.Default)
                                {
                                    line.stringValue = EditorGUILayout.TextArea(line.stringValue);
                                    EditorGUILayout.PropertyField(emotion);
                                    EditorGUILayout.PropertyField(isPrompt);
                                    if (isPrompt.boolValue)
                                    {
                                        EditorGUILayout.PropertyField(positiveOption);
                                        EditorGUILayout.PropertyField(negativeOption);

                                        string[] keywordsHere = GetEventsHere(k_keyword.stringValue, e_eventTag.stringValue);
                                        whenPositive.intValue = EditorGUILayout.Popup("When Positive", whenPositive.intValue, keywordsHere);
                                        whenNegative.intValue = EditorGUILayout.Popup("When Negative", whenNegative.intValue, keywordsHere);
                                    }
                                }
                                if (type.intValue == (int)DialogInformation.LineType.JumpToIfSeen)
                                {
                                    string[] keywordsHere = GetEventsHere(k_keyword.stringValue, e_eventTag.stringValue);
                                    whenSeen.intValue = EditorGUILayout.Popup("Jump to", whenSeen.intValue, keywordsHere);
                                }
                                if (type.intValue == (int)DialogInformation.LineType.SpecialAction)
                                {
                                    EditorGUILayout.PropertyField(action);
                                }

                                EditorGUILayout.BeginHorizontal();
                                if (GUILayout.Button("Remove This Line", GUILayout.MaxWidth(130), GUILayout.MaxHeight(20)))
                                {
                                    e_lines.DeleteArrayElementAtIndex(b);
                                }
                                if (b > 0)
                                {
                                    if (GUILayout.Button("Move Up", GUILayout.MaxWidth(80), GUILayout.MaxHeight(20)))
                                    {
                                        e_lines.MoveArrayElement(b, b - 1);
                                        GUI.FocusControl("");
                                    }
                                }
                                if (b < e_lines.arraySize - 1)
                                {
                                    if (GUILayout.Button("Move Down", GUILayout.MaxWidth(80), GUILayout.MaxHeight(20)))
                                    {
                                        e_lines.MoveArrayElement(b, b + 1);
                                        GUI.FocusControl("");
                                    }
                                }
                                EditorGUILayout.EndHorizontal();

                                EditorGUILayout.EndVertical();
                            }

                            if (GUILayout.Button("Add New Line"))
                            {
                                e_lines.InsertArrayElementAtIndex(e_lines.arraySize);
                            }

                            EditorGUILayout.LabelField("Rewards", EditorStyles.boldLabel);

                            for (int b = 0; b < e_rewards.arraySize; b++)
                            {
                                EditorGUILayout.BeginVertical(EditorStyles.helpBox);

                                SerializedProperty RewardRef = e_rewards.GetArrayElementAtIndex(b);
                                SerializedProperty rewardType = RewardRef.FindPropertyRelative("type");
                                SerializedProperty rewardEventName = RewardRef.FindPropertyRelative("eventName");
                                SerializedProperty rewardSetDone = RewardRef.FindPropertyRelative("setDone");
                                SerializedProperty rewardItemId = RewardRef.FindPropertyRelative("itemId");
                                SerializedProperty rewardItemCount = RewardRef.FindPropertyRelative("itemCount");
                                SerializedProperty rewardKeyword = RewardRef.FindPropertyRelative("keyword");
                                SerializedProperty rewardAffection = RewardRef.FindPropertyRelative("affectionPlus");

                                EditorGUILayout.PropertyField(rewardType);

                                if (rewardType.intValue == (int)DialogInformation.RewardType.Keyword)
                                {
                                    EditorGUILayout.PropertyField(rewardKeyword);
                                }
                                if (rewardType.intValue == (int)DialogInformation.RewardType.Affection)
                                {
                                    EditorGUILayout.PropertyField(rewardAffection);
                                }
                                if (rewardType.intValue == (int)DialogInformation.RewardType.Item)
                                {
                                    EditorGUILayout.PropertyField(rewardItemId);
                                    EditorGUILayout.PropertyField(rewardItemCount);
                                }
                                if (rewardType.intValue == (int)DialogInformation.RewardType.Event)
                                {
                                    string[] gameEvents = GetGameEvents();
                                    rewardEventName.intValue = EditorGUILayout.Popup("Event", rewardEventName.intValue, gameEvents);
                                    EditorGUILayout.PropertyField(rewardSetDone);
                                }
                                if (rewardType.intValue == (int)DialogInformation.RewardType.UnlockCharInfo)
                                {

                                }
                                if (rewardType.intValue == (int)DialogInformation.RewardType.ChangeState)
                                {

                                }

                                if (GUILayout.Button("Remove This Reward", GUILayout.MaxWidth(150), GUILayout.MaxHeight(20)))
                                {
                                    e_rewards.DeleteArrayElementAtIndex(b);
                                }

                                EditorGUILayout.EndVertical();
                            }

                            if (GUILayout.Button("Add New Reward"))
                            {
                                e_rewards.InsertArrayElementAtIndex(e_rewards.arraySize);
                            }
                        }

                        EditorGUILayout.EndVertical();

                        if (GUILayout.Button("Remove This Event"))
                        {
                            k_events.DeleteArrayElementAtIndex(a);
                        }
                    }
                }

                EditorGUILayout.Space();
                EditorGUILayout.Separator();

                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("Add New Event"))
                {
                    t.responses[i].events.Add(new DialogInformation.DialogEvent());
                }

                if (GUILayout.Button("Remove This Response"))
                {
                    KeywordList.DeleteArrayElementAtIndex(i);
                }
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.Space();
                EditorGUILayout.Separator();
            }
        }

        EditorGUILayout.Space();
        EditorGUILayout.Separator();

        if (GUILayout.Button("Add New Response"))
        {
            t.responses.Add(new DialogInformation.Response());
        }

        //Apply the changes to our list
        GetTarget.ApplyModifiedProperties();
    }

}
