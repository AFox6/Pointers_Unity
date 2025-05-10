using UnityEngine;

public class UIManager : MonoBehaviour
{
    public static UIManager instance;
    public GameObject uiGroup;
    public UI_Terminal terminal;
    public UI_PauseMenu pauseMenu;
    public UI_LevelGoal levelGoal;
    public UI_DialogueSystem dialogueSystem;
    public UI_CompletionMenu completionMenu;

    public bool inPauseMenu;
    
    void Awake(){
        if(instance != null){
            Destroy(instance.gameObject);
        }
        else{
            instance = this;
        }
    }

    public void OpenUI(){
        // Debug.Log("Opening UI");
        uiGroup.SetActive(true);
    }

    public void CloseUI(){
        // Debug.Log("Closing UI");
        uiGroup.SetActive(false);
        completionMenu.CloseCompletionMenu();
        inPauseMenu = false;
    }

    public void Reset()
    {
        if(terminal != null && terminal.isActiveAndEnabled){
            terminal.terminalOpen = true;
            terminal.OpenTerminal();
        }
        levelGoal.ResetLevelProgress();
    }
}
