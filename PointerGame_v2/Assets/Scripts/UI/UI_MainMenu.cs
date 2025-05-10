using UnityEngine;

public class UI_MainMenu : MonoBehaviour
{
    [SerializeField] private GameObject levelSelectButton;
    [SerializeField] private GameObject levelsParent;
    // [SerializeField] private GameObject backButton;


    void Start()
    {
        CloseLevelSelect();
        // UIManager.instance.CloseUI();
        // UIManager.instance.OpenUI();
    }


    public void OpenLevelSelect(){
        levelsParent.SetActive(true);
        levelSelectButton.SetActive(false);
    }

    public void CloseLevelSelect(){
        levelsParent.SetActive(false);
        levelSelectButton.SetActive(true);
    }
}
