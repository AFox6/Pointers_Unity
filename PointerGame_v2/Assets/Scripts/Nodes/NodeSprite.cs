using UnityEngine;

[CreateAssetMenu(fileName = "NodeSprite", menuName = "Scriptable Objects/NodeSprite")]
public class NodeSprite : ScriptableObject
{
    [Header("Sprites")]
    public Sprite metal;
    public Sprite copper;
    public Sprite wood;
    public Sprite forest;
    public Sprite iceBrick;
    public Sprite redBrick;
    public Sprite whiteBrick;
    public Sprite gold;


    public Sprite GetSpriteByLevelType(LevelManager.LevelType levelType){
        switch(levelType){
            case LevelManager.LevelType.Metal:
                return metal;
            case LevelManager.LevelType.Copper:
                return copper;
            case LevelManager.LevelType.Wood:
                return wood;
            case LevelManager.LevelType.Forest:
                return forest;
            case LevelManager.LevelType.Ice_Brick:
                return iceBrick;
            case LevelManager.LevelType.Red_Brick:
                return redBrick;
            case LevelManager.LevelType.White_Brick:
                return whiteBrick;
            case LevelManager.LevelType.Gold:
                return gold;
            default:
                return metal;
        }
    }
}
