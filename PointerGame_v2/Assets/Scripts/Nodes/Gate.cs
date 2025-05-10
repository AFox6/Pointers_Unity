using UnityEngine;
using UnityEngine.UI;

public class Gate : Node
{
    [Header("Gate Info")]
    [SerializeField] private Color goalColor;
    [SerializeField] private Sprite gemImage;
    public bool goalCompleted {get; private set;} = false;
    private Slider slider;
    public TimeGem goalGem {get; private set;} = null;


    #region Node Overrides 
    protected override void Start()
    {
        base.Start();
        
        SetupGate();
    }

    public override void UpdateConnection(TimeGem _shape)
    {
        //do nothing
    }

    public override void ChangeGemType(TimeGem _gem)
    {
        //do nothing
        if(goalCompleted){
            // Debug.Log("goal already completed");
            return;
        }
        
        //if correct gem
        if(_gem != null && DoColorsMatch(_gem.color, goalColor)){ 
            // Debug.Log("Correct gem passed!");
            gem = _gem;
            goalCompleted = true;
            // gemDisplay.UpdateDisplay(gem);
            slider.value = slider.maxValue;

            LevelManager.instance.CheckLevelProgress(this);
            UIManager.instance.levelGoal.UpdateGoalProgress(1, goalColor);

            if(GetComponent<ParticleFX>() != null) GetComponent<ParticleFX>().Play();
        }
        else{
            //If incorrect gem added, do nothing
            // Debug.Log("incorrect gem added");
        }
    }

    //update gate's display
    public override void UpdateDisplay()
    {
        if(!goalCompleted) return;

        base.UpdateDisplay();
    }

    public override void ResetNode()
    {
        base.ResetNode();

        Destroy(goalGem);

        SetupGate();
    }

    #endregion


    #region Gate
    //setup function
    public void SetupGate()
    {
        goalCompleted = false;

        slider = GetComponentInChildren<Slider>();
        slider.maxValue = 1;//goals.Count;
        slider.value = 0;

        if(goalGem == null){
            SetupGoalGem();
        }

        if (gemDisplay == null) gemDisplay = GetComponentInChildren<GemDisplay>();
        gemDisplay.UpdateDisplay(goalGem);

    }

    //creating new goal gem
    private void SetupGoalGem()
    {
        goalGem = ScriptableObject.CreateInstance<TimeGem>();
        goalGem.image = gemImage;
        goalColor.a = 0.7f;
        goalGem.color = goalColor;
        goalGem.gemName = "GoalGem";
    }

    //setting up goal to be displayed
    public void SetGoal(TimeGem _goal){
        goalColor = _goal.color;
        gemImage = _goal.image;
        SetupGoalGem();

        if(gemDisplay == null) gemDisplay = GetComponentInChildren<GemDisplay>();
        gemDisplay.UpdateDisplay(goalGem);
    }

    //checking if colors match
    private bool DoColorsMatch(Color first, Color second, float delta = 0.1f){
        return (Mathf.Abs(first.r - second.r) < delta) &&
            (Mathf.Abs(first.g - second.g) < delta) &&
            (Mathf.Abs(first.b - second.b) < delta);
    }

    public Color GetGoalColor() => goalColor;

    #endregion

}