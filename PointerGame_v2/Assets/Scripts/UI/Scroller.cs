using UnityEngine;
using UnityEngine.UI;

public class Scroller : MonoBehaviour
{
    [SerializeField] private RawImage img;
    [SerializeField] private float xScrollSpeed, yScrollSpeed;

    void Update()
    {
        img.uvRect = new Rect(img.uvRect.position + new Vector2(xScrollSpeed, yScrollSpeed) * Time.deltaTime, img.uvRect.size);
    }
}
