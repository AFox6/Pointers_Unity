using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using Unity.VisualScripting;
using UnityEngine;

public class ConnectionManager : MonoBehaviour
{
    public static int MAX_NUM_CONNECTIONS = 16;
    public static KeyCode connectionKey = KeyCode.Mouse0;
    public static KeyCode derefKey = KeyCode.E;//KeyCode.LeftShift;
    public static KeyCode addrKey = KeyCode.F;//KeyCode.LeftControl;

    public static ConnectionManager instance;


    [Header("Connection Info")]
    [SerializeField] private GameObject linePrefab;
    [SerializeField] private LayerMask whatIsNode;

    [Header("Connection Colors")]
    [SerializeField] private Color transferColor;
    [SerializeField] private Color derefColor;
    [SerializeField] private Color laserColor;

    public Node currSelection {get; private set;}
    public Node prevSelection {get; private set;}
    [SerializeField]private Connection currConnection; //serializefield for debug
    private ConnectionType connectionType;
    private bool laserSelected;
    private bool derefConnection;
    private bool automaticConnection;
    public bool inRoom;

    private List<Node> connectedNodes;

    public enum ConnectionType{
        None,
        Transfer,
        TransferDeref,
        Laser,
    }

    private void Awake(){
        if(instance != null){
            Destroy(instance.gameObject);
        }
        else{
            instance = this;
        }
    }

    private void Start(){
        // connectionType = ConnectionType.None;
        connectedNodes = new List<Node>();
    }

    private void Update()
    {
        if(LevelManager.instance != null){
            if(!automaticConnection && !UIManager.instance.inPauseMenu && !UIManager.instance.terminal.terminalOpen && !LevelManager.instance.codedLevel)
            {
                ManualConnection();
            }
        }
    }

    private void ManualConnection()
    {
        if (currConnection != null)
        {
            currConnection.FollowMouse();

            if (laserSelected && connectionType == ConnectionType.Laser)
            {
                currSelection.GetComponent<Laser>().FollowMouse();
            }
        }

        //if selecting node, start connection
        if (Input.GetKeyDown(connectionKey))
        {
            CheckInput();

            if(SelectingNode()){
                CheckAction();            
            }
        }
    }

    #region Input
    //checks input keys and changes connection type based on it
    private void CheckInput()
    {

        //laser connection:
        if(Input.GetKey(addrKey))
        {
            // Debug.Log("Changing connection type to laser");
            connectionType = ConnectionType.Laser;
        }
        //deref key
        else if(Input.GetKey(derefKey)){
            // Debug.Log("Changing connection type to transferderef");
            connectionType = ConnectionType.TransferDeref;
        }
        //transfer:
        else{
            if(connectionType == ConnectionType.Laser){
                //if laser connection active, cannot change connection unless placed or destroyed
                return;
            }

            // Debug.Log("Changing connection type to transfer");
            connectionType = ConnectionType.Transfer;
        }
    }

    //checks action based on connection type and curr selection
    private void CheckAction()
    {
        // Debug.Log("Checking action");
        if(currConnection == null){
            //check correct connection/node is selected and check node's protection (must be accessed by laser)
            if (!CanStartConnection())
            {
                // Debug.Log("cannot start connection");
                currConnection = null;
                connectionType = ConnectionType.None;
                return;
            }

            derefConnection = connectionType == ConnectionType.TransferDeref;

            //start connection
            InitializeConnection();
        }
        else{
            if(!CanEndConnection()){
                // Debug.Log("cannot end connection!");
                Destroy(currConnection.gameObject);
                currConnection = null;
                connectionType = ConnectionType.None;
                return;
            }

            // derefConnection = false; //reset bool

            //end connection
            FinalizeConnection();
        }
    }
    
#endregion

#region Manual Connection
    public void ChangeCurrSelection(Node n){
        prevSelection = currSelection;
        currSelection = n;
        laserSelected = currSelection.GetComponent<Laser>() != null;

        if(currConnection != null){
            if(connectionType == ConnectionType.Transfer){
                prevSelection.RemoveConnection();
            }
            else if(connectionType == ConnectionType.Laser){
                prevSelection.GetComponent<Laser>().RemoveLaserConnection();
            }
        }

        laserSelected = false;
    }

    //checks if selecting node and if so, assigns curr selection to it
    public bool SelectingNode(){
        RaycastHit2D hit = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(Input.mousePosition), Vector2.zero, 10, whatIsNode);

        prevSelection = currSelection;
        currSelection = hit ? hit.collider.GetComponent<Node>() : null;
        laserSelected = currSelection != null ? currSelection.GetComponent<Laser>() != null : false;

        if(currSelection == null){
            //if there is a connection, destroy it
            if(currConnection != null){
                if(connectionType == ConnectionType.Transfer || connectionType == ConnectionType.TransferDeref){
                    prevSelection.RemoveConnection();
                }
                else if(connectionType == ConnectionType.Laser){
                    prevSelection.GetComponent<Laser>().RemoveLaserConnection();
                }

                currConnection = null;
                connectionType = ConnectionType.None;
            }
        }

        return currSelection != null; //WAS hit
    }

    //creates a connection
    private void CreateConnection(){
        currConnection = Instantiate(linePrefab, currSelection.transform).GetComponent<Connection>();
        currConnection.StartConnection(currSelection, GetColor(connectionType));
    }

    //checks whether or not connection can be started
    private bool CanStartConnection()
    {
        bool canStartConnection = false;
        string errmsg = "";

        //checking if curSelection is null
        if(currSelection == null) errmsg = "Must be selecting node";
        // else canStartConnection = true;

        //check room status
        if(!inRoom){
            if(currSelection.roomStatus != Node.RoomStatus.NotInRoom){
                // errmsg = "Cannot start a connection to inside a room when you are outside";
                if(currSelection.roomStatus != Node.RoomStatus.Exit) errmsg = "Cannot start a connection to inside a room when you are outside";
            }
            // else if(prevSelection != null && prevSelection.roomStatus != Node.RoomStatus.Exit) errmsg = "When exiting room, you must start connection from exit";
            if(errmsg != ""){ 
                LevelManager.instance.robot.PrintError(errmsg);
                return false;
            }
        }
        else{
            if(currSelection.roomStatus != Node.RoomStatus.InRoom){
                // errmsg = "Cannot start a connection to outside a room when you are inside";
                if(currSelection.roomStatus != Node.RoomStatus.Entrance) errmsg = "Cannot start a connection to outside a room when you are inside";
            }
            // else if(currSelection.roomStatus != Node.RoomStatus.Entrance) errmsg = "Must start connection from entrance of room when you are inside";
            if(errmsg != ""){ 
                LevelManager.instance.robot.PrintError(errmsg);
                return false;
            }
        }

        if(connectionType == ConnectionType.Laser){
            //case: trying to start a laser connection without a laser
            if(!laserSelected) errmsg = "Trying to start a laser connection but not selecting a laser";
            //trying to start a connection from a non-updatable laser
            else if(!currSelection.GetComponent<Laser>().isUpdatable) errmsg = "Cannot point a non-updatable laser";
            //case: laser = &node
            else canStartConnection = true;
        }
        else if(connectionType == ConnectionType.TransferDeref){
            //case: trying to deref but not selecting laser
            if(!laserSelected) errmsg = "Trying to start a deref connection but not selecting a laser";
            //case: can't deref laser if its not pointed anywhere
            else if(!currSelection.GetComponent<Laser>().HasLaserConnection()) errmsg = "Trying to start a deref connection but laser isn't pointed anywhere";
            //case: *laser = *laser 
            else canStartConnection = true;
        }
        else if(connectionType == ConnectionType.Transfer){
            //case: laser doesn't has laser connection
            if(laserSelected && !currSelection.GetComponent<Laser>().HasLaserConnection()) errmsg = "Laser must be pointed somewhere for a transfer connection";
            //case: transferring from protected node
            else if(!laserSelected && currSelection.isProtected) errmsg = "Trying to transfer from a protected node";
            //case: node = node
            else canStartConnection = true;
        }

        //if cannot start connection, print error
        if(!canStartConnection) LevelManager.instance.robot.PrintError(errmsg);

        return canStartConnection;
    }

    //starts a connection
    public void InitializeConnection()
    {
        // Debug.Log("starting connection");

        if (connectionType == ConnectionType.Laser)
        {
            //if already has laser connection, remove it
            if(currSelection.GetComponent<Laser>().HasLaserConnection()){
                currSelection.GetComponent<Laser>().RemoveLaserConnection();
                currSelection.ResetGem();
            }

            CreateConnection();
            currSelection.GetComponent<Laser>().AddLaserConnection(currConnection);
            UpdateConnectedNodes();
        }
        else if (connectionType == ConnectionType.Transfer || connectionType == ConnectionType.TransferDeref)
        {
            //if already has connection, remove it
            if(currSelection.HasConnection()){
                currSelection.RemoveConnection();
            }

            CreateConnection();
            currSelection.AddConnection(currConnection);
            UpdateConnectedNodes();
        }
    }

    //checks whether or not connection can be made (ended)
    private bool CanEndConnection(){
        bool canEndConnection = false;
        string errmsg = "";

        //case: currSelection != null && currSelection != prevSelection 
        if(currSelection == null) errmsg = "Must be selecting node";
        else if(currSelection == prevSelection) errmsg = "Must be selecting different node to previous";

        //check room status
        if(!inRoom){
            if(currSelection.roomStatus != Node.RoomStatus.NotInRoom){
                // errmsg = "Cannot end a connection to inside a room when you are outside";
                if(currSelection.roomStatus != Node.RoomStatus.Entrance) errmsg = "Cannot end a connection to inside a room when you are outside";
            }
            // else if(currSelection.roomStatus != Node.RoomStatus.Entrance) errmsg = "Must end connection at entrance of room";
            if(errmsg != ""){ 
                LevelManager.instance.robot.PrintError(errmsg);
                return false;
            }
        }
        else{
            if(currSelection.roomStatus != Node.RoomStatus.InRoom){ 
                // errmsg = "Cannot end a connection to outside a room when you are inside";
                if(currSelection.roomStatus != Node.RoomStatus.Exit) errmsg = "Cannot end a connection to outside a room when you are inside";
            }
            // else if(currSelection.roomStatus != Node.RoomStatus.Exit) errmsg = "Must end connection at exit of room when you are inside";
            if(errmsg != ""){ 
                LevelManager.instance.robot.PrintError(errmsg);
                return false;
            }
        }

        if(connectionType == ConnectionType.Laser){
            //case: accessing protected/unupdatable things with a laser connection
            if(prevSelection.GetComponent<Laser>() == null) errmsg = "Previous node must be a laser";
            //cannot do laser = &laser (LC btwn lasers) 
            else if(laserSelected) errmsg = "Cannot make a laser connection to another laser";
            //case: laser = &node
            else canEndConnection = true;
        }
        else if(connectionType == ConnectionType.TransferDeref){
            //laser must be selected in order to deref
            if(!laserSelected) errmsg = "Must select a laser when using a deref connection";
            //case: *laser2 = *laser, laser2 must have laser connection
            else if(!currSelection.GetComponent<Laser>().HasLaserConnection()) errmsg = "Cannot deref to another laser if that laser doesn't have a laser connection";
            //case: *laser = node OR *laser2 = *laser
            else canEndConnection = true;
        }
        else if(connectionType == ConnectionType.Transfer){
            //cannot transfer to protected node
            if(currSelection.isProtected) errmsg = "Cannot transfer to protected node";
            //cannot transfer to node that isn't updatable (e.g. mine = node)
            else if(!currSelection.isUpdatable) errmsg = "Cannot transfer to un-updatable node";
            //cannot deref to transfer connection between lasers
            else if(laserSelected && derefConnection) errmsg = "Cannot make a deref to transfer connection between lasers";
            //case: laser 2 = laser
            else if(laserSelected && !derefConnection && prevSelection.GetComponent<Laser>() == null) errmsg = "Cannot transfer to a laser unless previous selection is a laser";
            //case: node = *laser
            else if(!laserSelected && !derefConnection && prevSelection.GetComponent<Laser>() != null) errmsg = "Cannot transfer directly from a laser. You must deref it first"; 
            //case: node = node
            else canEndConnection = true;
        }

        //if cannot end connection, print error
        if(!canEndConnection) LevelManager.instance.robot.PrintError(errmsg);

        return canEndConnection;
    }

    //ends a connection, returns true upon success
    public void FinalizeConnection(bool addMoveGemToRobotQueue = true)
    {
        // Debug.Log("ending connection");

        currConnection.EndConnection(currSelection, GetColor(connectionType));

        if (connectionType == ConnectionType.Laser)
        {
            prevSelection.GetComponent<Laser>().UpdateLaserConnection(currSelection.gem);
        }
        else if (connectionType == ConnectionType.Transfer || connectionType == ConnectionType.TransferDeref)
        {
            //case: laser = &node, laser 2 = laser
            if(laserSelected && connectionType == ConnectionType.Transfer &&
                prevSelection.GetComponent<Laser>() != null)
            {
                //point laser regardless of whether or not it was previously pointed
                RobotAction pointLaser = new RobotAction(currSelection.transform, RobotActionType.PointLaser, prevSelection.GetComponent<Laser>().laserConnection.endNode);
                LevelManager.instance.robot.EnqueueAction(pointLaser);
                RobotAction breakConnection = new RobotAction(prevSelection.transform, RobotActionType.BreakConnection, currSelection);
                LevelManager.instance.robot.EnqueueAction(breakConnection);
            }
            else{
                // Debug.Log("prevSelection: " + prevSelection.name + ", currSelection: " + currSelection.name);
                prevSelection.UpdateConnection(prevSelection.gem);
                
                //add move gem to robot's queue
                if(addMoveGemToRobotQueue)
                {
                    LevelManager.instance.robot.AddMoveGemToQueue(prevSelection.transform, currSelection);
                }
            }

        }

        UpdateConnectedNodes();

        currConnection = null;
        connectionType = ConnectionType.None;

        // return true;
    }

#endregion

#region Automatic Connection

    //draws a connection with time = 0 (instant)
    public void DrawConnection(Node start, Node end, ConnectionType type){
        StartCoroutine(DrawConnectionWithTime(start, end, type, 0));
    }

    //draws connection with a given time (animation)
    public IEnumerator DrawConnectionWithTime(Node start, Node end, ConnectionType type, float t = 1f){
        automaticConnection = true;

        currSelection = start;
        connectionType = type;

        laserSelected = currSelection.GetComponent<Laser>() != null;

        InitializeConnection();

        //if there is any duration draw, otherwise ignore
        if(t > 0){
            currConnection.GoToPoint(1, end.transform.position, t);
            yield return new WaitForSeconds(t);
        }

        prevSelection = currSelection;
        currSelection = end;

        FinalizeConnection();

        // }


        automaticConnection = false;
    }

    //**MUST BE CALLED WITH EndGuidedConnection()** 
    //starts connection that follows point -- used by robot
    public Connection StartGuidedConnection(Node start, ConnectionType type){
        automaticConnection = true;

        prevSelection = currSelection;
        currSelection = start;
        connectionType = type;
        laserSelected = currSelection != null ? currSelection.GetComponent<Laser>() != null : false;

        if (!CanStartConnection())
        {
            // Debug.Log("cannot start connection");
            currConnection = null;
            connectionType = ConnectionType.None;
            return null;
        }

        derefConnection = connectionType == ConnectionType.TransferDeref;

        InitializeConnection();

        return currConnection;
    }

    //**MUST BE CALLED WITH StartGuidedConnection()** 
    //ends guided connection -- used by robot
    public bool EndGuidedConnection(Node end, ConnectionType type){
        prevSelection = currSelection;
        currSelection = end;
        connectionType = type;
        laserSelected = currSelection != null ? currSelection.GetComponent<Laser>() != null : false;

        if(!CanEndConnection()){
            // Debug.Log("cannot end guided connection!");
            Destroy(currConnection.gameObject);
            currConnection = null;
            connectionType = ConnectionType.None;
            return false;
        }

        derefConnection = false;

        FinalizeConnection(false);
        automaticConnection = false;

        // Debug.Log("successfully ended guided connection!");

        return true;
    }

#endregion

    //updates connected nodes list by adding curr selection
    private void UpdateConnectedNodes()
    {
        if(currSelection == null){
            return;
        }

        if(connectedNodes == null){
            connectedNodes = new List<Node>();
        }

        //add curr selection to connected nodes list
        if (!connectedNodes.Contains(currSelection))
        {
            connectedNodes.Add(currSelection);
        }
    }

    //resets all connections
    public void ResetConnections(){
        if(connectedNodes == null){
            return;
        }

        // Debug.Log("resetting connections");
        //resets each node
        foreach(var node in connectedNodes){
            node.ResetNode();
        }

        //clears list
        connectedNodes.Clear();

        inRoom = false;
    }

    //resets connection state
    // public void ResetConnectionState(){
    //     ResetConnections();

    //     // Debug.Log("destroying connections");
    //     for(int i = 0; i < transform.childCount; i++){
    //         Destroy(transform.GetChild(i).gameObject);
    //     }

    //     inRoom = false;
    // }

    //gets color connection should be based on connection type
    private Color GetColor(ConnectionType type){
        switch(type){
            case ConnectionType.Transfer:
                return transferColor;
            case ConnectionType.TransferDeref:
                return derefColor;
            case ConnectionType.Laser:
                return laserColor;
            default:
                Debug.LogWarning("connection type invalid!");
                return Color.white;
        }
    }
}
