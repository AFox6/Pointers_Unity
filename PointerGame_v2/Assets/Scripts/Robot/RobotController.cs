using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public enum RobotActionType{
    None,
    Speak,
    SpeakClickable,
    MoveGem,
    PointLaser,
    BreakConnection,
    TransferToTransferConnection,
    DerefToDerefConnection,
    TransferToDerefConnection,
    DerefToTransferConnection,
}

public class RobotController : MonoBehaviour
{
    //components
    public Animator anim {get; private set;}
    public GemDisplay gemDisplay {get; private set;}

    private UI_DialogueSystem dialogueSystem;
    
    [SerializeField] private float moveSpeed;
    [SerializeField] private float cooldownTime;
    [SerializeField] private float timeBetweenActions;
    // public List<RobotAction> startingActions; //debug
    [TextArea, SerializeField] private string affirmations; 
    private Queue<RobotAction> actionsQueue = new Queue<RobotAction>();
    private bool inAction;
    private bool isBeingNudged;

    void Start()
    {
        anim = GetComponentInChildren<Animator>();
        gemDisplay = GetComponentInChildren<GemDisplay>();
        dialogueSystem = GetComponentInChildren<UI_DialogueSystem>();

        dialogueSystem.gameObject.SetActive(false);

        FaceCamera();

        // AddActionsToQueue(startingActions);
    }

    void Update()
    {
        if(actionsQueue.Count > 0 && !inAction && !isBeingNudged){
            StartCoroutine(DoNextActionCoroutine());
        }
    }

    //adds starting actions to queue, ignores duplicate actions
    public void AddActionsToQueue(List<RobotAction> actionsToAdd){
        if(actionsToAdd == null){
            return;
        }

        if(actionsToAdd.Count > 0){
            foreach (var action in actionsToAdd)
            {
                if(!actionsQueue.Contains(action)){
                    actionsQueue.Enqueue(action);
                }
            }
        }
    }

    //resets robot's actions
    public void ResetRobot(){
        StopAllCoroutines();
        actionsQueue.Clear();
        gemDisplay.UpdateDisplay(null);
        inAction = false;
        dialogueSystem.CloseTextBox();
        FaceCamera();
    }

    //when robot is clicked
    public void OnMouseDown()
    {
        StartCoroutine(NudgePosition());
    }

    public void FaceCamera(){
        //look at cam
        anim.SetFloat("xVelocity", 0);
        anim.SetFloat("yVelocity", -1);
    }

    #region Generic Actions
    //does next action in queue, then dequeues it
    IEnumerator DoNextActionCoroutine(){
        // Debug.Log("performing next action");

        inAction = true;

        RobotAction toDo = actionsQueue.Dequeue();

        //if loc is null, set it to be curr position
        if(toDo.location == null) toDo.location = transform;

        yield return MoveToPosCoroutine(toDo.location.position);
        
        yield return DoAction(toDo);

        yield return new WaitForSeconds(timeBetweenActions);

        inAction = false;
    }

    //move to position
    IEnumerator MoveToPosCoroutine(Vector2 _pos){
        anim.SetFloat("xVelocity", _pos.x);
        anim.SetFloat("yVelocity", _pos.y);

        if(Mathf.Abs(_pos.x) > 1){
            anim.gameObject.transform.rotation = Quaternion.Euler(0, 0, _pos.y < 0 ? 20 : -20);
        }


        while(Vector2.Distance(transform.position, _pos) > 0.1){
            transform.position = Vector2.MoveTowards(transform.position, _pos, moveSpeed * Time.deltaTime);
            yield return null;
        }

        anim.gameObject.transform.rotation = Quaternion.Euler(0, 0, 0);

        transform.position = _pos;
    }

    //nudges position to the right
    IEnumerator NudgePosition(){
        isBeingNudged = true;
        yield return MoveToPosCoroutine(transform.position + Vector3.right);
        isBeingNudged = false;
    }

    //does action
    IEnumerator DoAction(RobotAction _action){
        RobotActionType actionType = _action.actionType;
        bool addConnectionAction = false;
        bool nudgeAfterAction = true;
        ConnectionManager.ConnectionType cStartType = ConnectionManager.ConnectionType.Transfer;
        ConnectionManager.ConnectionType cEndType = ConnectionManager.ConnectionType.Transfer;

        switch(actionType){
            case RobotActionType.Speak:
                // Debug.Log("SPEAK");
                nudgeAfterAction = false;
                yield return OpenDialogue(_action.dialogue);
                break;
            case RobotActionType.SpeakClickable:
                nudgeAfterAction = false;
                dialogueSystem.clickableDialogue = true;
                yield return OpenDialogue(_action.dialogue);
                dialogueSystem.clickableDialogue = false;
                break;
            case RobotActionType.MoveGem:
                // Debug.Log("MOVE GEM");
                Node start = _action.location.gameObject.GetComponent<Node>();
                //check to see if from has connection
                if(start.HasConnection()){
                    yield return MoveGem(start, _action.secondaryNode);
                }
                break;
            case RobotActionType.PointLaser:
                // Debug.Log("POINT LASER");
                yield return PointLaserTo(_action.location.gameObject.GetComponent<Laser>(), _action.secondaryNode);
                break;
            
            case RobotActionType.TransferToTransferConnection:
                // Debug.Log("Transfer to Transfer CONNECTION");
                //dont need to assign cStart/cEnd type because initialised to transfer connection
                addConnectionAction = true;
                break;
            case RobotActionType.TransferToDerefConnection:
                // Debug.Log("Transfer to Deref CONNECTION");
                cStartType = ConnectionManager.ConnectionType.Transfer;
                cEndType = ConnectionManager.ConnectionType.TransferDeref;
                addConnectionAction = true;
                break;
            case RobotActionType.DerefToDerefConnection:
                // Debug.Log("Deref to Deref CONNECTION");
                cStartType = ConnectionManager.ConnectionType.TransferDeref;
                cEndType = ConnectionManager.ConnectionType.TransferDeref;
                addConnectionAction = true;
                break;
            case RobotActionType.DerefToTransferConnection:
                // Debug.Log("Deref to Transfer CONNECTION");
                cStartType = ConnectionManager.ConnectionType.TransferDeref;
                cEndType = ConnectionManager.ConnectionType.Transfer;
                addConnectionAction = true;
                break;
            case RobotActionType.BreakConnection:
                // Debug.Log("BREAK CONNECTION");
                yield return BreakConnection(_action.location.gameObject.GetComponent<Node>());
                break;
        }

        if(addConnectionAction){
            yield return AddConnection(_action.location.gameObject.GetComponent<Node>(), _action.secondaryNode, cStartType, cEndType);
        }

        if(nudgeAfterAction){
            yield return NudgePosition();
        }
        
        yield return null;
    }

    IEnumerator CooldownCoroutine(){
        yield return new WaitForSeconds(cooldownTime);
    }

    public void EnqueueAction(RobotAction action) => actionsQueue.Enqueue(action);

    #endregion

    #region Specific Actions

    //coroutine for opening dialogue
    IEnumerator OpenDialogue(string dialogue){
        FaceCamera();

        dialogueSystem.gameObject.SetActive(true);
        dialogueSystem.UpdateDialogue(dialogue);
        
        yield return dialogueSystem.DialogueCoroutine();

        dialogueSystem.gameObject.SetActive(false);
    }

    //coroutine for moving gem from one node to another
    IEnumerator MoveGem(Node from, Node to){
        //move to start
        yield return MoveToPosCoroutine(from.transform.position);

        //cooldown
        yield return CooldownCoroutine();

        //shows robot's gem
        if(from.gem != null){
            gemDisplay.UpdateDisplay(from.gem);
        }

        //move to end
        yield return MoveToPosCoroutine(to.transform.position);
        
        //updates end's display to show gem
        to.ChangeGemType(from.gem);
        to.UpdateDisplay();
        //if end is laser, update laser connection (See: Laser Case 2)
        if(to.GetComponent<Laser>() != null){
            to.GetComponent<Laser>().laserConnection?.endNode.UpdateDisplay();
        }

        from.RemoveConnection();

        //hides robot's gem
        gemDisplay.UpdateDisplay(null);
    }

    //coroutine for pointing laser to another node
    IEnumerator PointLaserTo(Laser laser, Node to){
        //move to laser
        yield return MoveToPosCoroutine(laser.transform.position);

        //draws a laser connection
        yield return ConnectionManager.instance.DrawConnectionWithTime(laser, to, ConnectionManager.ConnectionType.Laser);

        if(laser.HasConnection()){
            yield return MoveGem(laser, laser.connection.endNode);      
        }
    }

    //move to position with a connection
    IEnumerator MoveToPosCoroutineWithConnection(Vector2 _pos, Connection _connection){
        anim.SetFloat("xVelocity", _pos.x);
        anim.SetFloat("yVelocity", _pos.y);

        if(Mathf.Abs(_pos.x) > 1){
            anim.gameObject.transform.rotation = Quaternion.Euler(0, 0, _pos.y < 0 ? 20 : -20);
        }


        while(Vector2.Distance(transform.position, _pos) > 0.1){
            transform.position = Vector2.MoveTowards(transform.position, _pos, moveSpeed * Time.deltaTime);
            _connection.FollowPoint(transform.position);
            yield return null;
        }

        anim.gameObject.transform.rotation = Quaternion.Euler(0, 0, 0);

        transform.position = _pos;
    }

    //adds connection from one node to another
    IEnumerator AddConnection(Node from, Node to, ConnectionManager.ConnectionType startType, ConnectionManager.ConnectionType endType){
        //move to start
        yield return MoveToPosCoroutine(from.transform.position);

        //cooldown
        yield return CooldownCoroutine();

        //starts guided connection
        Connection currConnection = ConnectionManager.instance.StartGuidedConnection(from, startType);

        if(currConnection != null){
            //moves to end
            yield return MoveToPosCoroutineWithConnection(to.transform.position, currConnection);

            //ends guided connection
            if(ConnectionManager.instance.EndGuidedConnection(to, endType)){
                //if end guided connection successful, move gem
                yield return MoveGem(from, to); 
            }
        }
    }

    IEnumerator BreakConnection(Node at){
        //move to start
        yield return MoveToPosCoroutine(at.transform.position);

        //cooldown
        yield return CooldownCoroutine();

        at.RemoveConnection();
    }

    //adds move gem action to queue -- used by connection manager
    public void AddMoveGemToQueue(Transform start, Node to){
        //check to make sure there is a gem to transfer
        // if(start.GetComponent<Node>().gem == null){
        //     return;
        // }

        //add action to robot
        RobotAction moveGem = new RobotAction(
            start,
            RobotActionType.MoveGem,
            null,
            to
        );

        actionsQueue.Enqueue(moveGem);
    }

    //prints error by having robot say it
    public void PrintError(string error)
    {
        RobotAction errWarning = new RobotAction(transform, RobotActionType.Speak, "Error: " + error, null);
        actionsQueue.Enqueue(errWarning);
    }

    //says a random affirmation
    public void SayAffirmation(){
        string[] splitText = affirmations.Split('\n');
        int randNum = UnityEngine.Random.Range(0, splitText.Length - 1);
        // Debug.Log(splitText[randNum]);
        RobotAction affirmation = new RobotAction(transform, RobotActionType.Speak, splitText[randNum]);
        actionsQueue.Enqueue(affirmation);
    }


    #endregion
}
