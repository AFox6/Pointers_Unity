using UnityEngine;

[CreateAssetMenu(fileName = "Wall", menuName = "Scriptable Objects/Wall")]
public class Wall : ScriptableObject
{
    [Header("Sprites")]
    public Sprite bottomLeftCornerSprite;
    public Sprite bottomWallSprite;
    public Sprite bottomRightCornerSprite;
    public Sprite leftWallSprite;
    public Sprite rightWallSprite;
    public Sprite topLeftCornerSprite;
    public Sprite topWallSprite;
    public Sprite topRightCornerSprite;
    

    [Header("Other Info")]
    public GameObject wallPrefab;


    //get sprite based on w/h
    public Sprite GetSpriteByLocation(int w, int h, int width, int height){

        Sprite sprite;

        if (w == 0)
        {
            //bot left 
            if (h == 0)
            {
                sprite = bottomLeftCornerSprite;
            }
            //top left
            else if (h == height - 1)
            {
                sprite = topLeftCornerSprite;
            }
            //left mid
            else
            {
                sprite = leftWallSprite;
            }
        }
        else if (w == width - 1)
        {
            //bot right
            if (h == 0)
            {
                sprite = bottomRightCornerSprite;
            }
            //top right
            else if (h == height - 1)
            {
                sprite = topRightCornerSprite;
            }
            //right mid
            else
            {
                sprite = rightWallSprite;
            }
        }
        else
        {
            //bot
            if (h == 0)
            {
                sprite = bottomWallSprite;
            }
            //top
            else
            {
                sprite = topWallSprite;
            }
        }

        return sprite;
    }
}
