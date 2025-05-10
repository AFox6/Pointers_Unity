using System.Collections;
using System.Security.Cryptography;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelLoader : MonoBehaviour
{
    public static string[] sceneList = {"EntryPointScene", "MainMenu", "T_1", "1_1", "1_2", "T_2", "1_3", "1_4", "1_5", "T_3", "1_6", "1_7", "T_4", "1_8", "1_9"};
    public static string entryPointScene {get; private set;} = sceneList[0]; //"EntryPointScene";
    public static string mainMenuScene {get; private set;} = sceneList[1]; //"MainMenu";
    public static string activeLevelName {get; private set;} = "";
    public static string prevLevelName {get; private set;} = "";

    [SerializeField] private bool loadAtStart;

    private void Start()
    {
        if(loadAtStart){
            // activeLevelName = gameObject.scene.name;
            LoadScene();
        }
    }

    private static void ResetLevelState()
    {
        //resolve previous scene
        if (activeLevelName == mainMenuScene)
        {
            UIManager.instance.CloseUI();
        }
        else
        {
            UIManager.instance.OpenUI();
        }

        CameraManager.instance.RemoveFollowTarget();
        
        ConnectionManager.instance.ResetConnections();

        UIManager.instance.levelGoal.DestroyParentTab();
        UIManager.instance.completionMenu.CloseCompletionMenu();
    }

    private IEnumerator LoadCoroutine(){
        prevLevelName = activeLevelName;
        activeLevelName = gameObject.name;

        ResetLevelState();

        // Debug.Log("prev level = " + prevLevelName);
        // Debug.Log("Active level = " + activeLevelName);

        if (prevLevelName != "" && prevLevelName != entryPointScene)
        {
            Debug.Log("unloading: " + prevLevelName);
            yield return SceneManager.UnloadSceneAsync(prevLevelName);
            Debug.Log("finished unloading: " + prevLevelName);
        }

        Debug.Log("loading: " + gameObject.name);
        yield return SceneManager.LoadSceneAsync(activeLevelName, LoadSceneMode.Additive);
        Debug.Log("finished loading: " + gameObject.name);
    }

    public void LoadScene()
    {
        prevLevelName = activeLevelName;
        activeLevelName = gameObject.name;

        ResetLevelState();

        if (prevLevelName != "" && prevLevelName != entryPointScene)
        {
            // Debug.Log("unloading: " + prevLevelName);
            SceneManager.UnloadSceneAsync(prevLevelName);
            // Debug.Log("finished unloading: " + prevLevelName);
        }

        // Debug.Log("loading: " + gameObject.name);
        SceneManager.LoadSceneAsync(activeLevelName, LoadSceneMode.Additive);
        // Debug.Log("finished loading: " + gameObject.name);

        // Debug.Log("prev level = " + prevLevelName);
        // Debug.Log("Active level = " + activeLevelName);

        // StartCoroutine(LoadCoroutine());
    }

}
