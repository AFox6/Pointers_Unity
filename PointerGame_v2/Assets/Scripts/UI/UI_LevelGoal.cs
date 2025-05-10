using System.Collections;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class UI_LevelGoal : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI progressText;
    private int currProgress;
    private int totalProgress;

    //for multiple goals
    [SerializeField] private GameObject gemImagePrefab;
    private GameObject goalTabsParent;
    [SerializeField] private float menuSpeed;
    public bool goalsOpen {get; private set;}
    
    private Button goalOpenButton;
    private Vector2 originalSize;

    private Color[] goalColors;


    public void SetupLevelGoalUI(int totalProgressAmount, Color[] colors){
        currProgress = 0;
        totalProgress = totalProgressAmount;
        goalColors = colors;

        if(totalProgress > 1){
            // Debug.Log("More than one goal, creating goal parent tab");

            //Note: uncomment to create drop-down menu when clicking on level goal to see all required gems
            // CreateNewParent();
        }
        
        UpdateProgressText();
    }

    public void UpdateProgressText(){
        progressText.text = currProgress + " / " + totalProgress;
    }

    public void UpdateGoalProgress(int amount, Color c)
    {
        currProgress += amount;
        UpdateProgressText();
        CheckGoalColors(c);
    }

    //check if goal colors match, then destroy child obj
    private void CheckGoalColors(Color c)
    {
        c.a = 1;
        if (totalProgress > 1 && goalTabsParent != null)
        {
            for (int i = 0; i < goalTabsParent.transform.childCount; i++)
            {
                if (goalTabsParent.transform.GetChild(i).GetComponent<Image>().color == c)
                {
                    Destroy(goalTabsParent.transform.GetChild(i).gameObject);
                    return;
                }
            }
        }
    }

    public void ResetLevelProgress(){
        currProgress = 0;
        UpdateProgressText();
    }

    //creates new goal parent for goal tab
    private void CreateNewParent(){
        //button
        goalOpenButton = gameObject.AddComponent<Button>();
        goalOpenButton.onClick.AddListener(OpenGoals);
        goalOpenButton.transition = Selectable.Transition.None;

        //goal parent
        goalTabsParent = new GameObject("GoalTabsParent");
        RectTransform goalTabRT = goalTabsParent.GetOrAddComponent<RectTransform>();
        goalTabRT.SetParent(transform);
        goalTabRT.localScale = Vector3.one;
        goalTabRT.anchoredPosition = GetComponent<RectTransform>().anchoredPosition;
        goalTabRT.position = GetComponent<RectTransform>().position;
        goalTabRT.pivot = GetComponent<RectTransform>().pivot;

        originalSize = goalTabsParent.GetComponent<RectTransform>().sizeDelta;

        //grid layout
        GridLayoutGroup group = goalTabsParent.AddComponent<GridLayoutGroup>();
        group.cellSize = new Vector2(90, 90);
        group.startCorner = GridLayoutGroup.Corner.UpperLeft;
        group.childAlignment = TextAnchor.UpperLeft;
        group.spacing = new Vector2(0, 10);
        

        for(int i = 0; i < totalProgress; i++){
            GameObject newGoal = Instantiate(gemImagePrefab, goalTabsParent.transform);
            newGoal.GetComponent<Image>().color = goalColors[i];
        }

        goalsOpen = false;
        goalTabsParent.SetActive(false);
    }

    //opens goals menu
    public void OpenGoals(){
        //if completion menu active, do nothing
        if(UIManager.instance.completionMenu.isActiveAndEnabled){
            return;
        }
        
        goalTabsParent.SetActive(!goalTabsParent.activeSelf);
        goalsOpen = !goalsOpen;

        if(goalsOpen){
            progressText.enabled = false;
            StartCoroutine(SlideAnimationCoroutine());
        }
        else{
            StopAllCoroutines();
            ResetIcons();
        }
    }

    //slides icons out from menu
    IEnumerator SlideAnimationCoroutine(){
        RectTransform rt = goalTabsParent.GetComponent<RectTransform>();

        int child = 0;
        rt.sizeDelta = new Vector2(100, 0);

        while(rt.sizeDelta.y < originalSize.y * goalTabsParent.transform.childCount){
            rt.sizeDelta += new Vector2(0, menuSpeed * 100 * Time.deltaTime);

            if (rt.sizeDelta.y > (child + 1) * 100 && child < goalTabsParent.transform.childCount) // Check if the current width can fit the child
            {
                goalTabsParent.transform.GetChild(child).gameObject.SetActive(true); 
                child++;
            }

            yield return null;
        }

        rt.sizeDelta = originalSize;
    }

    private void ResetIcons(){
        for (int i = 0; i < goalTabsParent.transform.childCount; i++)
        {
            goalTabsParent.transform.GetChild(i).GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
            goalTabsParent.transform.GetChild(i).gameObject.SetActive(false);
        }

        goalTabsParent.GetComponent<RectTransform>().sizeDelta = new Vector2(0, 100);
        goalTabsParent.SetActive(false);

        progressText.enabled = true;
    }

    //destroys goal parent tab
    public void DestroyParentTab(){
        Destroy(goalOpenButton);
        goalOpenButton = null;

        if(goalTabsParent != null && goalTabsParent.transform.childCount > 0){
            foreach(GameObject child in goalTabsParent.transform.GetChild(0)){
                Destroy(child);
            }
            Destroy(goalTabsParent);
        }

        goalTabsParent = null;
        originalSize = Vector2.zero;
        goalsOpen = false;
    }
}
