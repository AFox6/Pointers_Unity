using System;
using UnityEngine;

[CreateAssetMenu(fileName = "Time Gem", menuName = "Scriptable Objects/Time Gem")]
[Serializable]
public class TimeGem : ScriptableObject
{
    // public new string name;
    public enum Shape{
        Diamond,
        Circle,
        Square,
        Triangle,
        Hexagon,
        Star,
    }

    public string gemName;
    public Sprite image;
    public Color color;
    public Shape shape;
}
