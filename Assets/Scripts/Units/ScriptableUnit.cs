using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName ="New Unit",menuName ="Scriptable Unit")]
public class ScriptableUnit : ScriptableObject
{
    public Faction Faction;
    public BaseUnit UnitPrefab;
}

public enum Faction {
    Red = 0,
    Blue = 1,
    Yellow = 2,
    Purple = 3,
    Green = 4,
    Orange = 5
}