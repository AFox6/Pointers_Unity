using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class Laser : Node
{
    public Connection laserConnection {get; private set;}
    [SerializeField] private Transform laserPointerTransform;
    [SerializeField] private NodeSprite pointerSprites;

    [HideInInspector] 
    [SerializeField] private Node pointTo;

    protected override void Start()
    {
        base.Start();

        SpriteRenderer pointerSR = laserPointerTransform.GetComponent<SpriteRenderer>();
        //setting sprites
        pointerSR.sprite = pointerSprites.GetSpriteByLevelType(LevelManager.instance.levelType);

        //if point to isn't equal to null, set laser pointer to point to's position
        if(pointTo != null){
            // Debug.Log("pointing to: " + pointTo.name);
            ConnectionManager.instance.DrawConnection(this, pointTo, ConnectionManager.ConnectionType.Laser);
            Vector2 lookDir = pointTo.transform.position - transform.position;
            laserPointerTransform.up = -lookDir;
        }
    }
    
    //adds laser connection to laser
    public void AddLaserConnection(Connection _laserConnection){
        laserConnection = _laserConnection;   
    }

    //removes laser's connection and destroys it
    public void RemoveLaserConnection(){
        if(laserConnection != null){
            Destroy(laserConnection.gameObject);
            laserConnection = null;
        }
    }

    //points to specific node, used if laser is not updatable
    public void PointTo(Node _pointTo) => pointTo = _pointTo;

    //changes laser's shape but not display
    public override void ChangeGemType(TimeGem _gem)
    {
        gem = _gem;
    }

    public override void UpdateDisplay()
    {
        //Do nothing
        if(roomStatus == RoomStatus.Entrance || roomStatus == RoomStatus.Exit){
            base.UpdateDisplay();
        }
    }

    //updates connection at laser, broken into specific cases
    public override void UpdateConnection(TimeGem _gem)
    {
        // Debug.Log("updating connection at " + gameObject.name);

        if(HasLaserConnection()){ 
            // Debug.Log("hi");
            //case: laser = &display, *laser = mine
            if(!HasConnection()){
                // Debug.Log("case: laser = &display, *laser = mine");
                laserConnection.endNode.ChangeGemType(_gem);

                TimeGem resetShape = ScriptableObject.CreateInstance<TimeGem>();
                resetShape.name = "None";
                resetShape.image = null;

                ChangeGemType(resetShape);
                return;
            }

            Laser endNodeLaser = connection.endNode.GetComponent<Laser>();
            
           
            //case: laser = &mine, node = *laser 
            if(endNodeLaser == null){
                // Debug.Log("case: laser = &mine, node = *laser ");
                gem = laserConnection.endNode.gem;
                // connection.endNode.ChangeGemType(laserConnection.endNode.gem);
                connection.endNode.UpdateConnection(laserConnection.endNode.gem);
            }
            //case: laser = &mine, laser2 = &mine2, *laser2 = *laser
            else if(endNodeLaser != null && endNodeLaser.HasLaserConnection()){
                // Debug.Log("case: laser = &mine, laser2 = &mine2, *laser2 = *laser");
                endNodeLaser.UpdateLaserConnection(laserConnection.endNode.gem);
            }
        }
        else{
            if(!HasConnection()){
                return;
            }

            Laser endNodeLaser = connection.endNode.GetComponent<Laser>();

            //case: laser = &node, laser2 = &node2, laser2 = laser
            if(endNodeLaser != null && endNodeLaser.HasLaserConnection()){
                // Debug.Log("case: laser = &node, laser2 = &node2, laser2 = laser");
                ConnectionManager.instance.DrawConnection(this, endNodeLaser.laserConnection.endNode, ConnectionManager.ConnectionType.Laser);
            }
        }
    }

    //updates the laser connection (different than regular)
    public void UpdateLaserConnection(TimeGem _gem){
        if(_gem == null || laserConnection == null){
            return;
        }

        gem = _gem;

        //if can update endnode, update
        if(laserConnection.endNode != null && laserConnection.endNode.isUpdatable){            
            laserConnection.endNode.ChangeGemType(_gem);
            laserConnection.endNode.UpdateConnection(_gem);
        }
    }

    public bool HasLaserConnection() => laserConnection != null;

    //laser pointer follows mouse position
    public void FollowMouse(){
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector2 lookDir = mousePos - transform.position;
        laserPointerTransform.up = lookDir;
    }

    //resets laser completely
    public override void ResetNode()
    {        
        base.ResetNode();

        if(isUpdatable){
            RemoveLaserConnection();
        }
    }


    #region Editor
#if UNITY_EDITOR
    [CustomEditor(typeof(Laser))]
    [CanEditMultipleObjects]
    public class Laser_Editor : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            Laser script = (Laser)target;

            EditorGUI.BeginChangeCheck();

            if(!script.isUpdatable){
                EditorGUILayout.Space();
                EditorGUILayout.LabelField("Static Info");
                script.PointTo(EditorGUILayout.ObjectField("Point To ", script.pointTo, typeof(Node), true) as Node);
                // EditorUtility.SetDirty(script);
            }

            if(EditorGUI.EndChangeCheck() && script.pointTo != null){
                EditorUtility.SetDirty(script);
            }
        }
    }
#endif
    #endregion

}
