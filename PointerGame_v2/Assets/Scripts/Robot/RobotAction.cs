using System;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

[Serializable]
public class RobotAction
{
    //base info
    [Header("Base Info")]
    public Transform location;
    public RobotActionType actionType;

    //action - speak
    [Header("Speak")] 
    [SerializeField, TextArea] public string dialogue;

    //secondary node used by:
    // - move gem
    // - modify connection
    // - break connection
    // - add connection
    [Header("Second Node")]
    public Node secondaryNode;


    public RobotAction(){
        
    }

    public RobotAction(Transform _loc, RobotActionType _at, Node _n){
        location = _loc;
        actionType = _at;
        secondaryNode = _n;
    }

    //for speaking actions
    public RobotAction(Transform _loc, RobotActionType _at, string _dialogue){
        location = _loc;
        actionType = _at;
        dialogue = _dialogue;
    }

    public RobotAction(Transform _loc, RobotActionType _at, string _dialogue, Node _n){
        location = _loc;
        actionType = _at;
        dialogue = _dialogue;
        secondaryNode = _n;
    }
}
