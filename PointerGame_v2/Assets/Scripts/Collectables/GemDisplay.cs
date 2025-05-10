using UnityEngine;

public class GemDisplay : MonoBehaviour
{
    private SpriteRenderer sr => GetComponent<SpriteRenderer>();
    public bool isDisplayingGem {get; private set;} = false;

    public void UpdateDisplay(TimeGem _gem){
        if(_gem != null && _gem.image != null){
            sr.enabled = true;
            sr.sprite = _gem.image;
            sr.color = _gem.color;

            isDisplayingGem = true;
        }
        else{
            sr.enabled = false;
            isDisplayingGem = false;
        }
    }
}
