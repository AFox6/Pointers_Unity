using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

public class TutorialManager : MonoBehaviour
{
    [Serializable]
    public class TutorialAction{
        public string text;
        public string loc;
    }

    [Serializable]
    public class TutorialInfo{
        public TutorialAction[] actions;
    }

    [SerializeField] private string toReadPath;
    public List<TutorialAction> actions {get; private set;}

    public void ReadJson()
    {
        //read json
        string tutorialData = File.ReadAllText(toReadPath);
        // Debug.Log(tutorialData);
        //convert to tutorial info obj -- note: names must EXACTLY match
        TutorialInfo tInfo =  JsonUtility.FromJson<TutorialInfo>(tutorialData);
        // TutorialInfo tInfo = JsonUtility.FromJson<TutorialInfo>(tutorialData);
        
        actions = new List<TutorialAction>();

        //add actions to dialogue
        foreach (var action in tInfo.actions)
        {
            // Debug.Log(action.text);
            actions.Add(action);
        }
    }
}
