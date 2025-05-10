using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UI_Terminal : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler
{
    [Header("Button Components")]
    [SerializeField] private Sprite sourceImage;
    [SerializeField] private Sprite highlightedImage;
    [SerializeField] private Sprite selectedImage;

    [Header("Input Field Components")]
    [SerializeField] private GameObject textParent;

    private Image targetGFX;
    private TMP_InputField inputField;
    public bool terminalOpen; //{get; private set;}

    private void Start(){
        targetGFX = GetComponent<Image>();
        inputField = GetComponent<TMP_InputField>();

        textParent.SetActive(false);
    }

    public void OpenTerminal(){
        //if completion menu active, do nothing
        if(UIManager.instance.completionMenu.isActiveAndEnabled){
            return;
        }

        //open terminal
        if(!terminalOpen){
            targetGFX.sprite = selectedImage;
            terminalOpen = true;
            textParent.SetActive(true);
        }
        //close terminal
        else{
            inputField.text = "";
            terminalOpen = false;
            targetGFX.sprite = sourceImage;
            textParent.SetActive(false);
        }
        targetGFX.SetNativeSize();
    }

    //mouse click
    public void OnPointerDown(PointerEventData eventData)
    {
        OpenTerminal();
    }

    //mouse hovering over
    public void OnPointerEnter(PointerEventData eventData)
    {
        //highlight if terminal not open
        if(!terminalOpen){
            targetGFX.sprite = highlightedImage;
        }
    }

    //mouse not hovering over
    public void OnPointerExit(PointerEventData eventData)
    {
        //switch back to original image
        if(!terminalOpen){
            targetGFX.sprite = sourceImage;
        }
    }

    //logic for when user enters line or clicks out of terminal 
    public void LineEntered(){
        if(terminalOpen){
            //if enter pressed
            if(Input.GetKeyDown(KeyCode.Return)){
                string stmt = inputField.text.ToString();
                inputField.text = "";
                inputField.ActivateInputField();
                // Debug.Log("Enter key pressed! User entered: " + text);
                RobotInterpreter interpretor = new RobotInterpreter();
                interpretor.InterpretStatement(stmt);
                // RobotCompiler.InterpretStatement(stmt);
            }
            //if somewhere else clicked not in terminal && mousepos in screen
            else if(Input.mousePosition.x >= 0 && Input.mousePosition.x <= Screen.width &&
                Input.mousePosition.y >= 0 && Input.mousePosition.y <= Screen.height && 
                ConnectionManager.instance.SelectingNode() && Input.GetKey(KeyCode.Mouse1)){
                // Debug.Log("Enter key NOT pressed!");
                inputField.text += ConnectionManager.instance.currSelection.name;
                inputField.caretPosition = inputField.text.Length; //moves caret to last position

                inputField.ActivateInputField();
            }
        }
    }
}
