using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class Array : MonoBehaviour
{
    public enum ArrayType{
        Mine,
        Laser,
        Display,
    }

    // public static int MAX_SIZE = 25;

    [Header("Refs")]
    [SerializeField] private Mine minePrefab;
    [SerializeField] private Laser laserPrefab;
    [SerializeField] private Node displayPrefab;

    [Header("Array Properties")]
    // [SerializeField] private bool previewInEditor = true;
    [SerializeField][Range(1, 25)] private int size = 1;
    [SerializeField] private ArrayType arrayType;
    [SerializeField] private bool isVertical;
    [SerializeField] private bool isPositive;
    [SerializeField][Range(1, 5)] private int distToStart = 2; //how much space btwn laser and start of array
    [SerializeField] private bool spawnAtTransform = true;
    [HideInInspector][SerializeField] private Vector2 spawnLocation = Vector2.zero;

    public Laser head {get; private set;}
    private Node[] elements;


    void Start()
    {
        InitializeArray();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    //gets node type for array
    private Node GetNodeType(){
        switch(arrayType){
            case ArrayType.Mine:
                return minePrefab;
            case ArrayType.Laser:
                return laserPrefab;
            case ArrayType.Display:
                return displayPrefab;
            default:
                return null;
        }
    }

    //initialises array based on values in editor
    private void InitializeArray(){
        if(spawnAtTransform){
            spawnLocation = transform.position;
        }

        //head laser node
        // head = Instantiate(laserPrefab, spawnLocation + GetOffset(-2), Quaternion.identity, transform).GetComponent<Laser>();
        head = Instantiate(laserPrefab, spawnLocation, Quaternion.identity, transform).GetComponent<Laser>();
        head.name = "head";
        head.isUpdatable = false;

        elements = new Node[size];

        //creating array
        for(int i = 0; i < size; i++)
        { 
            Node newNode = Instantiate(GetNodeType(), spawnLocation + GetOffset(i), Quaternion.identity, transform).GetComponent<Node>();
            newNode.name = GetNodeType().name + i;
            // newNode.gameObject.layer = LayerMask.NameToLayer("Array");
            newNode.isProtected = true;

            elements[i] = newNode;
        }

        head.PointTo(elements[0]);
    }

    //gets offset for next node to spawn in array
    private Vector2 GetOffset(int i)
    {
        Vector2 offset = isVertical
            //if vertical, check if positive or negative
            ? (isPositive ? Vector2.up : Vector2.down) * (i + distToStart)
            //else if horizontal, check if positive or negative
            : (isPositive ? Vector2.right : Vector2.left) * (i + distToStart);
        return offset;
    }

    //preview array in editor
    private void OnDrawGizmos()
    {
        //head
        Vector2 headPos = spawnAtTransform ? transform.position : spawnLocation;
        Gizmos.DrawWireCube(headPos, Vector2.one);
        //point dir
        Gizmos.DrawLine(headPos, headPos + GetOffset(0));

        //body
        Vector2 bodySize = isVertical ? new Vector2(1f, size) : new Vector2(size, 1f);

        //calculating offset if size is even (to reflect in preview how it will be generated in-game)
        float evenOffset = size % 2 == 0 ? 0.5f : 0; //need to add offset if even
        Vector2 totalOffset = isVertical 
            ? new Vector2(0, evenOffset * (isPositive ? 1 : -1))
            : new Vector2(evenOffset * (isPositive ? 1 : -1), 0);

        Vector2 bodyPos = headPos + GetOffset(Mathf.FloorToInt(size / 2)) - totalOffset;
        Gizmos.DrawWireCube(bodyPos, bodySize);
    }

    //used to visualise array in editor
    #region Editor
#if UNITY_EDITOR
    [CustomEditor(typeof(Array))]
    [CanEditMultipleObjects]
    public class Array_Editor : Editor
    {
        public override void OnInspectorGUI()
        {

            Array script = (Array)target;
            
            base.OnInspectorGUI();
            
            if(!script.spawnAtTransform){
                script.spawnLocation = EditorGUILayout.Vector2Field("Spawn Position", script.spawnLocation);
            }
        }
    }
#endif
    #endregion

}
