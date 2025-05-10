using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class UI_PauseMenu : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private GameObject settingsTabsParent;
    [SerializeField] private float menuSpeed;
    public bool menuOpen {get; private set;}
    
    private Button pauseButton;
    private Vector2 originalSize;

    private void Start()
    {
        pauseButton = GetComponent<Button>();
        originalSize = settingsTabsParent.GetComponent<RectTransform>().sizeDelta;        
        ResetIcons();
        menuOpen = false;
    }

    //opens menu
    public void OpenMenu(){
        //if completion menu active, do nothing
        if(UIManager.instance.completionMenu.isActiveAndEnabled){
            return;
        }

        settingsTabsParent.SetActive(!settingsTabsParent.activeSelf);
        menuOpen = !menuOpen;
        // UIManager.instance.inPauseMenu = settingsOpen;

        if(menuOpen){
            StartCoroutine(SlideAnimationCoroutine());
        }
        else{
            StopAllCoroutines();
            ResetIcons();
        }
    }

    public void CloseMenu(){
        settingsTabsParent.SetActive(false);
        menuOpen = false;
        StopAllCoroutines();
        ResetIcons();
    }

    public void ResetLevel(){
        LevelManager.instance.ResetLevel();
        UIManager.instance.Reset();
    }

    //slides icons out from menu
    IEnumerator SlideAnimationCoroutine(){
        RectTransform rt = settingsTabsParent.GetComponent<RectTransform>();

        int child = 0;
        rt.sizeDelta = new Vector2(0, 100);

        while(rt.sizeDelta.x < originalSize.x){
            rt.sizeDelta += new Vector2(menuSpeed * 100 * Time.deltaTime, 0);

            if (rt.sizeDelta.x > (child + 1) * 100 && child < settingsTabsParent.transform.childCount) // Check if the current width can fit the child
            {
                settingsTabsParent.transform.GetChild(child).gameObject.SetActive(true); 
                child++;
            }

            yield return null;
        }

        rt.sizeDelta = originalSize;
    }

    private void ResetIcons(){
        for (int i = 0; i < settingsTabsParent.transform.childCount; i++)
        {
            settingsTabsParent.transform.GetChild(i).GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
            settingsTabsParent.transform.GetChild(i).gameObject.SetActive(false);
        }

        settingsTabsParent.GetComponent<RectTransform>().sizeDelta = new Vector2(0, 100);

        settingsTabsParent.SetActive(false);
    }
}
