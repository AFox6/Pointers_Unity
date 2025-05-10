using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class UI_DialogueSystem : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler
{
    public static int MAX_LINES_DIALOGUE = 20;

    [Header("Button Components")]
    [SerializeField] private Sprite sourceImage;
    [SerializeField] private Sprite highlightedImage;
    [SerializeField] private Sprite selectedImage;
    private Image targetGFX;

    [SerializeField] private TextMeshProUGUI textArea;

    [Header("Dialogue Box Options")]
    [SerializeField] private bool autoResize;
    public bool clickableDialogue;

    [HideInInspector, SerializeField] private float showDialogueFor;
    private bool dialogueOpen = false;
    private float minShowtime = 1f;

    private int dialogueIdx = 0;

    private string[] dialogue;

    public bool firstTimeSeen {get; private set;} = true;


    void Start()
    {
        targetGFX = GetComponent<Image>();
    }

    public void OpenTextBox(){
        //open dialogue box
        if(targetGFX == null) targetGFX = GetComponent<Image>();

        targetGFX.sprite = selectedImage;

        if(autoResize){
            targetGFX.SetNativeSize();
        }

        dialogueOpen = true;
        textArea.gameObject.SetActive(true);
        targetGFX.enabled = true;

        ShowText();
    }

    public void CloseTextBox()
    {
        // Debug.Log("Closing text box");
        if(gameObject.activeInHierarchy){
            dialogueOpen = false;

        textArea.text = "";
        targetGFX.sprite = sourceImage;

        if(autoResize){
            targetGFX.SetNativeSize();
        }

        textArea.gameObject.SetActive(false);
        targetGFX.enabled = false;
        }
    }

    public void ShowText(){
        if(dialogue.Length > 0){
            textArea.text = CleanText(dialogue[dialogueIdx]);
        }
    }

    public void UpdateText()
    {
        // Debug.Log("updating text " + dialogueIdx);
        
        dialogueIdx++;

        if (dialogueIdx >= dialogue.Length)
        {
            // Debug.Log("resetting dialogue");
            firstTimeSeen = false;
            dialogueIdx = 0;

            // if(clickableDialogue){
            //     CloseTextBox();
            // }
        }

        ShowText();
    }

    public IEnumerator DialogueCoroutine(){
        OpenTextBox();

        if(!clickableDialogue){
            for(int i = 0; i < dialogue.Length; i++){
                float showDialogueTime = minShowtime + dialogue[i].Length * (showDialogueFor / 100);
                yield return new WaitForSeconds(showDialogueTime);
                UpdateText();
            }
        }
        else{
            while(firstTimeSeen){
                yield return null;
            }
        }

        CloseTextBox();
    }

    //mouse click
    public void OnPointerDown(PointerEventData eventData)
    {
        if(clickableDialogue){
            if(!dialogueOpen){
                OpenTextBox();
            }
            else{
                UpdateText();
            }
        }
    }

    //mouse hovering over
    public void OnPointerEnter(PointerEventData eventData)
    {
        //highlight if terminal not open
        if(!dialogueOpen){
            targetGFX.sprite = highlightedImage;
        }
    }

    //mouse not hovering over
    public void OnPointerExit(PointerEventData eventData)
    {
        //switch back to original image
        if(!dialogueOpen){
            targetGFX.sprite = sourceImage;
        }
        // if(clickableDialogue){
        //     CloseTextBox();
        // }
    }

    public void UpdateDialogue(string[] newDialogue)
    {
        dialogue = newDialogue;
        dialogueIdx = 0;
        firstTimeSeen = true;
    }

    public void UpdateDialogue(string newDialogue, char sep = '\n'){
        string[] sepDialogue = newDialogue.Split(sep);
        UpdateDialogue(sepDialogue);
    }

    public string CleanText(string s)
    {
        s = s.Replace("$connection_start$", "Left click"); //ConnectionManager.connectionKey.ToString()
        s = s.Replace("$connection_end$", "Left click"); //ConnectionManager.connectionKey.ToString()
        s = s.Replace("$deref_connection$", ConnectionManager.derefKey.ToString());
        s = s.Replace("$addr_connection$", ConnectionManager.addrKey.ToString());
        s = s.Replace("$cam_controls$", "WASD");

        // Debug.Log("cleaned string: " + s);
        return s;
    }

    #region Editor
#if UNITY_EDITOR
    [CustomEditor(typeof(UI_DialogueSystem))]
    [CanEditMultipleObjects]
    public class UI_DialogueSystem_Editor : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            UI_DialogueSystem script = (UI_DialogueSystem)target;

            EditorGUI.BeginChangeCheck();

            if(!script.clickableDialogue){
                // EditorGUILayout.Space();
                // EditorGUILayout.LabelField("Dialogue Options");
                script.showDialogueFor = EditorGUILayout.FloatField("Show Dialogue For: ", script.showDialogueFor);
            }

            if(EditorGUI.EndChangeCheck()){
                EditorUtility.SetDirty(script);
            }
        }
    }
#endif
    #endregion
}
