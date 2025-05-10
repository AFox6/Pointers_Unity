using UnityEngine;
using System.Collections.Generic;
using System.Collections;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class Room : MonoBehaviour
{
    [Header("References")]
    //references
    [SerializeField] private Wall wall;
    [SerializeField] private GameObject roofPrefab;
    [SerializeField] private GameObject doorPrefab; //can be node or gate

    [Header("Roof Settings")]
    //roof
    [SerializeField] private LayerMask whatIsNode;
    [SerializeField] private float fadeDuration = 0.25f;
    [SerializeField] private float delayBetweenFades = 0.1f;

    [Header("Requirements")]
    //requirements
    [SerializeField] private TimeGem.Shape entryRequirement;
    [SerializeField] private TimeGem.Shape exitRequirement;

    [Header("Properties")]
    //dimensions/locality
    [SerializeField][Range(3, 15)] private int width = 3;
    [SerializeField][Range(3, 15)] private int height = 3;
    [SerializeField] private bool spawnAtTransform = true;
    [HideInInspector] [SerializeField] private Vector2 spawnLocation = Vector2.zero;

    private List<Node> objectsInRoom = new List<Node>();
    // private List<GameObject> roofObjs = new List<GameObject>();
    private GameObject[,] roofObjs;

    public bool entryRequirementMet {get; private set;}
    public bool exitRequirementMet {get; private set;}


    void Start()
    {
        InitializeRoom();
    }


    void Update()
    {
        
    }

    //sets up entry/exit parameters
    private void SetupParameters(Vector2 spawnPos, TimeGem.Shape requirement, string name, bool entrance){
        Door newDoor = Instantiate(doorPrefab, spawnPos, Quaternion.identity, transform).GetComponent<Door>();
        newDoor.name = name;
        newDoor.roomStatus = entrance ? Node.RoomStatus.Entrance : Node.RoomStatus.Exit;
        newDoor.SetRequirement(requirement);
    }

    //initialises rooms based on values set in editor
    private void InitializeRoom(){
        roofObjs = new GameObject[width, height];
        if(spawnAtTransform){
            spawnLocation = transform.position;
        }

        for (int w = 0; w < width; w++)
        {
            for (int h = 0; h < height; h++)
            {
                //placing entrance
                if(w == 0 && h == height / 2){
                    Vector2 position = new Vector2(spawnLocation.x + w, spawnLocation.y + h);
                    SetupParameters(position, entryRequirement, "Entry_Node", true);
                }
                //placing exit
                else if(w == width - 1 && h == height / 2){
                    Vector2 position = new Vector2(spawnLocation.x + w, spawnLocation.y + h);
                    SetupParameters(position, exitRequirement, "Exit_Node", false);
                }

                //placing walls
                else if (w == 0 || w == width - 1 || h == 0 || h == height - 1)
                {
                    Vector2 position = new Vector2(spawnLocation.x + w, spawnLocation.y + h);
                    GameObject newWall = Instantiate(wall.wallPrefab, position, Quaternion.identity, transform);
                    newWall.name = $"Wall_{w}_{h}";
                    Sprite sprite = wall.GetSpriteByLocation(w, h, width, height);
                    newWall.GetComponentInChildren<SpriteRenderer>().sprite = sprite;
                }

                //scanning for objects within the room
                if(w > 0 && w < width - 1 && h > 0 && h < height - 1){
                    RaycastHit2D hit = Physics2D.Raycast(spawnLocation + new Vector2(w, h), Vector2.zero, 20, whatIsNode);
                    Node currSelection = hit ? hit.collider.gameObject.GetComponent<Node>() : null;

                    if(currSelection != null){
                        // Debug.Log("found: " + currSelection);
                        // currSelection.GetComponent<HighlightFX>().ChangeColor(Color.magenta); //changes color
                        objectsInRoom.Add(currSelection);
                        currSelection.roomStatus = Node.RoomStatus.InRoom;
                    }

                    //adding roof
                    GameObject roof = Instantiate(roofPrefab, spawnLocation + new Vector2(w, h), Quaternion.identity, transform);
                    roof.GetComponent<SpriteRenderer>().color = LevelManager.instance.GetBGColor();
                    // roofObjs.Add(roof);
                    roofObjs[w, h] = roof;
                }
            }
        }
    }

    public void ParameterSatisfied(TimeGem.Shape shape){
        if(shape == entryRequirement && !ConnectionManager.instance.inRoom){
            entryRequirementMet = true;
            OpenRoofAnimation();
            ConnectionManager.instance.inRoom = true;
        }
        else if(shape == exitRequirement && ConnectionManager.instance.inRoom){
            exitRequirementMet = true;
            CloseRoof();
            ConnectionManager.instance.inRoom = false;
        }
    }

    public void OpenRoofAnimation(){
        StartCoroutine(OpenRoofCoroutine());
    }

    private IEnumerator OpenRoofCoroutine(){
        // Fade objects along the diagonal (top-left to bottom-right)
        for (int d = 0; d < width + height - 1; d++)
        {
            for (int w = d; w >= 0; w--)
            {
                int h = d - w;

                // Check if the coordinates are within bounds of the grid
                if (w > 0 && w < width - 1 && h > 0 && h < height - 1)
                {
                    StartCoroutine(FadeCoroutine(roofObjs[w, h], roofObjs[w, h].GetComponent<SpriteRenderer>().color));
                }
            }

            yield return new WaitForSeconds(delayBetweenFades);
        }
    }

    private IEnumerator FadeCoroutine(GameObject roof, Color c){
        float elapsedTime = 0;
        float startAlpha = c.a;
        
        while(elapsedTime < fadeDuration){
            c.a = Mathf.Lerp(startAlpha, 0f, elapsedTime / fadeDuration);
            roof.GetComponent<SpriteRenderer>().color = c;
            yield return null;
            elapsedTime += Time.deltaTime;
        }
        roof.SetActive(false);
    }

    public void CloseRoof(){
        foreach(GameObject roof in roofObjs){
            if(roof != null){
                roof.SetActive(true);
                //reset alpha back to normal
                Color c = roof.GetComponent<SpriteRenderer>().color;
                c.a = 1f;
                roof.GetComponent<SpriteRenderer>().color = c;
            }
        }
    }

    public void ResetRoom(){
        entryRequirementMet = false;
        exitRequirementMet = false;
        StopAllCoroutines();
        CloseRoof();
    }

    //preview room in editor
    private void OnDrawGizmos()
    {
        //room outline
        float x = Mathf.Floor(width / 2) - (width % 2 == 0 ? 0.5f : 0); //need to add offset if even
        float y = Mathf.Floor(height / 2) - (height % 2 == 0 ? 0.5f : 0); //need to add offset if even
        Vector2 spawnPos = (spawnAtTransform ? transform.position : spawnLocation) + new Vector2(x, y);
        //outer wall
        Gizmos.DrawWireCube(spawnPos, new Vector2(width, height));
        //inner wall
        Gizmos.DrawWireCube(spawnPos, new Vector2(width - 2, height - 2));

        //door 
        Vector2 doorLoc = (spawnAtTransform ? transform.position : spawnLocation) + new Vector2(0, height / 2);
        Gizmos.DrawWireCube(doorLoc, Vector2.one);

        //exit
        Vector2 exitLoc = (spawnAtTransform ? transform.position : spawnLocation) + new Vector2(width - 1, height / 2);
        Gizmos.DrawWireCube(exitLoc, Vector2.one);
    }

    //used to visualise room in editor
    #region Editor
#if UNITY_EDITOR
    [CustomEditor(typeof(Room))]
    [CanEditMultipleObjects]
    public class Room_Editor : Editor
    {
        public override void OnInspectorGUI()
        {

            Room script = (Room)target;
            
            base.OnInspectorGUI();
            
            if(!script.spawnAtTransform){
                script.spawnLocation = EditorGUILayout.Vector2Field("Spawn Position", script.spawnLocation);
            }
        }
    }
#endif
    #endregion
}
