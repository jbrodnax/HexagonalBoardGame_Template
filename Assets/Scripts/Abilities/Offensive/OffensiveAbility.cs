using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class OffensiveAbility : BaseAbility
{
    [SerializeField] public float BaseDamageMin, BaseDamageMax;

    public DamageType DamageType;
    
    public Dictionary<BaseUnit,float> AffectedUnits;

    private float modifier;
    public float Modifier {get {return modifier;} set {modifier = value;}}
    protected float appliedDamage = -1;


    public virtual OffensiveAbility Init(float mod, Tile origin, BaseUnit callerUnit){
        AffectedUnits = new Dictionary<BaseUnit, float>();
        modifier = mod;
        sourceTile = origin;
        Unit = callerUnit;
        rollActualBaseDamage();
        
        return this;
    }
    // Sets the actual amount of damage this ability will cause.
    // Damage modifiers are applied by buffs from the unit that cast the abilit.
    private void rollActualBaseDamage(){
        // TKTK - shift damage based on any applicable buffs
        var newMin = BaseDamageMin + modifier;
        var newMax = BaseDamageMax + modifier;
        appliedDamage = Random.Range(newMin, newMax);
    }

    public float GetBaseDamage(){
        // TKTK - shift actual damage based on applicable buffs
        return appliedDamage;
    }

}

public enum DamageType{
    Physical = 0,
    Magic = 1
}

// TKTK - implement Buff/DeBuff classes
public enum Buffs{
    ArmorPen = 0,
    SpellPen = 1
}