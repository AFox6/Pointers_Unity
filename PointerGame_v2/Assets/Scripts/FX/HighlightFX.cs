using TMPro;
using UnityEngine;

public class HighlightFX : MonoBehaviour
{
    public static HighlightFX currSelection;

    public readonly static Color protectedColor = Color.cyan;
    public readonly static Color highlightColor = Color.white;
    public readonly static Color nonUpdatableColor = new Color(246, 141, 0); //orange-yellow
    public readonly static Color gateColor = Color.green;

    [SerializeField] private Material highlightMat;
    private GameObject outlineObj;
    private bool selected;

    private void Start()
    {
        SetupHighlight();

        currSelection = null;
        selected = false;
    }

    private void SetupHighlight()
    {
        //setting up highlight obj
        outlineObj = new GameObject("Highlight");
        outlineObj.AddComponent<SpriteRenderer>();

        //setting transform properties
        Transform highlightTransform = outlineObj.transform;
        highlightTransform.parent = transform;
        highlightTransform.localPosition = Vector2.zero;
        highlightTransform.localScale *= 1.15f;

        //getting components
        SpriteRenderer highlightSR = outlineObj.GetComponent<SpriteRenderer>();
        SpriteRenderer sr = GetComponentInChildren<SpriteRenderer>();

        //setting up gfx
        highlightSR.sprite = sr.sprite;
        highlightSR.material = highlightMat;
        GetColor(highlightSR);

        //sorting layer
        highlightSR.sortingLayerName = sr.sortingLayerName;
        highlightSR.sortingOrder = 0;

        outlineObj.SetActive(false);
    }

    //changes color based on type of node
    private void GetColor(SpriteRenderer highlightSR)
    {
        if (GetComponent<Node>() != null)
        {
            if(!GetComponent<Node>().isUpdatable) highlightSR.color = nonUpdatableColor;
            if(GetComponent<Node>().isProtected) highlightSR.color = protectedColor;
        }
        if(GetComponent<Gate>() != null){
            highlightSR.color = gateColor;
        }
    }

    public void Select(bool _select){
        selected = _select;
        outlineObj.SetActive(_select);
    }


    //mouse clicked on gameobj
    public void OnMouseDown()
    {
        if(currSelection == null){
            currSelection = this;
        }

        currSelection.Select(false);
        currSelection = this;
        
        Select(!selected);
    }

    //mouse hovering over gameobj
    public void OnMouseEnter()
    {
        if(!selected){
            outlineObj.SetActive(true);
        }
    }

    //mouse stopped hovering over gameobj
    public void OnMouseExit()
    {
        if(!selected){
            outlineObj.SetActive(false);
        }
    }

    public void ChangeColor(Color c){
        outlineObj.GetComponent<SpriteRenderer>().color = c;
    }
}
