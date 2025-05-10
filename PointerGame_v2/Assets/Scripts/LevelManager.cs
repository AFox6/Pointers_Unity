using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class LevelManager : MonoBehaviour
{   
    public static LevelManager instance;

    public enum LevelType{
        Metal,
        Copper,
        Wood,
        Forest,
        Ice_Brick,
        Red_Brick,
        White_Brick,
        Gold,
    }

    [Header("Environment")]
    public LevelType levelType;
    [SerializeField] private Color backgroundColor;

    [Header("Level Details")]
    
    [Header("Coded Level")]
    public bool codedLevel = false;

    [Header("Level Goals")]
    [SerializeField] private Gate[] gates;
    private List<TimeGem> levelGoals;

    [Header("Robot Details")]
    [SerializeField] private GameObject robotPrefab;
    public RobotController robot {get; private set;}
    [SerializeField] private List<RobotAction> startingActions;
    [SerializeField] private List<RobotAction> milestoneActions;
    [SerializeField] private List<RobotAction> endingActions;


    [Header("Level Hints")]
    [SerializeField] private string[] hints;

    public bool levelLoaded {get; private set;} = false;

    private void Awake()
    {
        if(instance != null){
            Destroy(instance.gameObject);
        }
        else{
            instance = this;
        }
    }

    private void Start()
    {
        if(!levelLoaded){
            SetupLevel();
        }
    }

    public void SetupLevel()
    {
        // Debug.Log("Setting up level");

        CheckForDialogue();

        SetupLevelGoals();

        //set up camera
        CameraManager camManager = CameraManager.instance;
        camManager.SetupCamera();
        camManager.UpdateEnvironment(backgroundColor);

        //set up UI
        UIManager uiManager = UIManager.instance;
        if (levelGoals.Count == 1)
        {
            Color goalColor = levelGoals[0].color;
            goalColor.a = 1f;
            uiManager.levelGoal.GetComponent<Image>().color = goalColor;
        }
        else
        {
            uiManager.levelGoal.GetComponent<Image>().color = Color.white;
        }

        List<Color> goalColors = new List<Color>();
        foreach (var goal in levelGoals)
        {
            //changing alphas to ensure visability
            Color c = goal.color;
            c.a = 1f;
            goalColors.Add(c);
        }

        uiManager.levelGoal.SetupLevelGoalUI(levelGoals.Count, goalColors.ToArray());
        uiManager.dialogueSystem.UpdateDialogue(hints);

        //set up level robot
        robot = Instantiate(robotPrefab, transform).GetComponent<RobotController>();
        robot.AddActionsToQueue(startingActions);

        levelLoaded = true;
    }

    //checks for dialogue
    private void CheckForDialogue(){
        if(GetComponent<TutorialManager>() != null){
            TutorialManager tutorial = GetComponent<TutorialManager>();

            //read json
            tutorial.ReadJson();

            //add actions to starting actions
            foreach (var action in tutorial.actions)
            {
                // Debug.Log(action.loc);
                Transform loc = GameObject.Find(action.loc)?.transform;
                RobotAction tutorialDialogue = new RobotAction(loc, RobotActionType.SpeakClickable, action.text);
                startingActions.Add(tutorialDialogue);
            }
        }
    }

    public void SetupLevelGoals(){
        levelGoals = new List<TimeGem>();

        foreach (var gate in gates)
        {
            gate.SetupGate();
            levelGoals.Add(gate.goalGem);
        }
    }

    private void AddMilestoneActionsToRobot(Transform _loc){
        foreach (var action in milestoneActions)
        {
            if(action.location == _loc){
                robot.AddActionsToQueue(new List<RobotAction>{action});
                milestoneActions.Remove(action);
                return;
            }
        }
    }

    public void CheckLevelProgress(Gate toCheck){
        //say affirmation whenever level is complete
        robot.SayAffirmation();

        // Debug.Log("checking level progress for " + toCheck.gameObject.name);
        // AddMilestoneActionsToRobot(toCheck);

        foreach (var gate in gates)
        {
            if(!gate.goalCompleted){
                return;
            }
        }

        //if all gates completed
        robot.AddActionsToQueue(endingActions);

        UIManager.instance.completionMenu.OpenCompletionMenu();
    }

    //resets all things connection manager doesn't (rooms, arrays, etc)
    public void ResetLevel(){
        robot.transform.position = Vector2.zero;
        robot.ResetRobot();

        ConnectionManager.instance.ResetConnections();
    }

    public Color GetBGColor() => backgroundColor;
}
