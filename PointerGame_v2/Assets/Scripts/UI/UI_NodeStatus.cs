using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UI_NodeStatus : MonoBehaviour
{
    [SerializeField] private GameObject bggroup;
    [SerializeField] private TextMeshProUGUI tmp;
    [SerializeField] private Image gemImg;

    public bool statusMenuOpen {get; private set;} = false;

    public void SetupMenu(string name, TimeGem g){
        tmp.text = name;
        gemImg.color = g != null ? g.color : Color.clear; //if has gem, show color, else null
    }
    public void UpdateStatusMenu(TimeGem g){
        //if has gem, show color, else null
        if(g != null){
            gemImg.color = g.color;
        }
        else{
            gemImg.color = Color.clear;
        }
    }
    public void OpenStatusMenu(){
        statusMenuOpen = !statusMenuOpen;
        bggroup.SetActive(statusMenuOpen);
    }
}
