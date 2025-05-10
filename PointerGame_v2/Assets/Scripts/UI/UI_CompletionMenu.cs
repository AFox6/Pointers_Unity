using UnityEngine;
using UnityEngine.SceneManagement;

public class UI_CompletionMenu : MonoBehaviour
{
    [SerializeField] private GameObject nextLevelLoader;

    [SerializeField] private GameObject mainMenuLoader;

    public void OpenCompletionMenu(){
        gameObject.SetActive(true);

        //if there is a valid next level to load, show next level loader
        int activeSceneIdx = SceneManager.GetSceneByName(LevelLoader.activeLevelName).buildIndex;
        if(activeSceneIdx + 1 >= LevelLoader.sceneList.Length){
            nextLevelLoader.SetActive(false);
        }
        else{
            nextLevelLoader.name = LevelLoader.sceneList[activeSceneIdx + 1];
            nextLevelLoader.SetActive(true);
        }

        UIManager.instance.inPauseMenu = true;
        
        UIManager.instance.pauseMenu.CloseMenu(); //close pause menu if open
    }
    public void CloseCompletionMenu(){
        gameObject.SetActive(false);
        UIManager.instance.inPauseMenu = false;
    }
}
