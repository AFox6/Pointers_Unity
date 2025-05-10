using UnityEngine;
using System;

public class Node : MonoBehaviour
{
    public enum RoomStatus{
        NotInRoom = 0,
        InRoom = 1,
        Entrance = 2,
        Exit = 3,
    }
    
    #region Components
    public SpriteRenderer sr {get; private set;}
    public GemDisplay gemDisplay {get; protected set;}
    public UI_NodeStatus status {get; private set;}
    #endregion


    [Header("Node Info")]
    [SerializeField] protected NodeSprite sprites;
    public bool isUpdatable;
    public bool isProtected;
    public RoomStatus roomStatus = RoomStatus.NotInRoom;
    public Connection connection {get; private set;}
    public TimeGem gem; //{get; private set;}
    protected TimeGem originalGem;

    protected virtual void Start()
    {
        sr = GetComponentInChildren<SpriteRenderer>();
        gemDisplay = GetComponentInChildren<GemDisplay>();
        status = GetComponentInChildren<UI_NodeStatus>();

        //setting sprites
        sr.sprite = sprites.GetSpriteByLevelType(LevelManager.instance.levelType);

        if(gem != null){
            gemDisplay.UpdateDisplay(gem);
        }

        originalGem = gem;

        status.SetupMenu(name, gem); 
    }

    protected virtual void Update()
    {
        
    }

    //adds connection to node
    public virtual void AddConnection(Connection _connection){
        connection = _connection;
    }

    //removes and destroys connection at node
    public virtual void RemoveConnection(){
        if(connection != null){
            Destroy(connection.gameObject);
            connection = null;
        }
    }

    //changes node's shape and updates display
    public virtual void ChangeGemType(TimeGem _gem){
        gem = _gem;
    }

    //updates display to show gem, if it has one
    public virtual void UpdateDisplay(){
        if(gemDisplay == null) gemDisplay = GetComponentInChildren<GemDisplay>();
        gemDisplay.UpdateDisplay(gem);
        status.UpdateStatusMenu(gem);
    }

    //updates connection at node
    public virtual void UpdateConnection(TimeGem _gem){
        if(connection == null){
            return;
        }

        //if end node laser, ignore isUpdatable
        if(connection.endNode != null && 
            (connection.endNode.GetComponent<Laser>() != null || connection.endNode.isUpdatable)){
            // Debug.Log("updating connection at: " + connection.endNode.name);
            // connection.endNode.ChangeGemType(_gem);
            connection.endNode.UpdateConnection(_gem);
        }
    }

    public virtual void OnMouseOver(){
        if(!status.statusMenuOpen){
            status.OpenStatusMenu();
        }
    }

    public virtual void OnMouseExit(){
        //if status open, close it
        if(status.statusMenuOpen){
            status.OpenStatusMenu();
        }
    }

    public virtual bool HasConnection() => connection != null;

    public virtual void ResetGem() => gem = originalGem;

    //resets node -- called by connection manager
    public virtual void ResetNode(){
        // Debug.Log("Resetting node: " + gameObject.name + ", gem: " + originalGem);
        RemoveConnection();
        ResetGem();
        UpdateDisplay();

        HighlightFX.currSelection = null;

        //if in between room, reset room
        if(roomStatus == RoomStatus.Entrance){
            GetComponentInParent<Room>().ResetRoom();
        }
    }
}
