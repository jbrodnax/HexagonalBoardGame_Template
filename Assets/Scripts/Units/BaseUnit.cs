using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class BaseUnit : MonoBehaviour
{
   // BEGIN ATTRIBUTES
   [SerializeField] public float Health;
   [SerializeField] [Range(0f, 1f)] public float Armor;
   [SerializeField] [Range(0f, 1f)] public float MagicResist;
   [SerializeField] public float DistancePerTurn;
   [SerializeField] public AbilityBinding[] AbilitiesConfig;
   [SerializeField] public Animator animator;
   // END ATTRIBUTES

   // Tile that this unit currently occupies
   private Tile occupiedTile;
   public Tile OccupiedTile
   {
      get {return occupiedTile;} set {occupiedTile = value;}
   }

   private BaseUnit targetUnit;
   public BaseUnit TargetUnit
   {
      get {return targetUnit;} set {targetUnit = value;}
   }

   private Tile targetTile;
   public Tile TargetTile
   {
      get {return targetTile;} set {targetTile = value;}
   }
   // The Faction that this unit is associated with
   public Faction Faction;

   // Name of the unit
   public string UnitName;

   // Team the unit is apart of
   public Team UnitTeam;

   private BaseAbility selectedAbility;
   public BaseAbility SelectedAbility
   {
      get {return selectedAbility;} set {selectedAbility = value;}
   }

   public (AbilityBinding,bool) SelectedBinding;

   public UnitState unitState;

   public virtual void Init(){
      unitState = UnitState.Alive;
   }

   public void SetTargetTile(Tile tile){
      if (TargetTile != null){
         TargetTile.Highlight(TileEvent.UnHighlight);
      }
      TargetTile = tile;
      TargetTile.Highlight(TileEvent.Highlight);
   }

   public void SetTargetUnit(BaseUnit unit){
      // TKTK - implement rendering changes for target here (deselect previous unit)
      TargetUnit = unit;
      SetTargetTile(TargetUnit.OccupiedTile);
   }

   public void ClearTargets(){
      if (TargetTile != null){
         TargetTile.Highlight(TileEvent.UnHighlight);
      }
      OccupiedTile.Highlight(TileEvent.UnHighlight);
      TargetTile = null;

      if (TargetUnit != null){
         // TKTK - unhighlight the unit once implemented
         //TargetUnit.OccupiedTile.Highlight(TileEvent.UnHighlight);
      }
      TargetUnit = null;
   }

   /*
    * Sets the unit's selected ability.
    * Saves the keybinding for the selection
    * Deselects any previously selected ability.
    * Instantiates and Init()'s the ability
   */
   public bool SelectAbility(AbilityBinding binding){
      Debug.Log("Selecting ability");
      var prefab = binding.abilityPrefab;

      if (selectedAbility != null){
         DeselectAbility();
      }

      BaseAbility ability = Instantiate(prefab);
      selectedAbility = ability;
      SelectedBinding = (binding, true);
      switch(ability.AbilityType){
         case AbilityType.SingleTarget:
            
            break;
         case AbilityType.AOE:
            var aoeAbility = (Nova)ability;
            aoeAbility.Init(0f, OccupiedTile, this);
            break;
         case AbilityType.Cleave:
            // TKTK
            break;
         case AbilityType.Projectile:
            // TKTK
            break;
         case AbilityType.Movement:
            var moveAbility = (Move)ability;
            moveAbility.Init(OccupiedTile, DistancePerTurn, this);
            break;
         default:
            Debug.LogError("SelectAbility received an ability with invalid type.");
            return false;
      }
      return true;
   }

   public void DeselectAbility(){
      if (selectedAbility != null){
         SelectedBinding.Item2 = false;
         selectedAbility.CleanUp();
         selectedAbility = null;
      }
      return;
   }

   /*
    * Attempt to execute (trigger) the ability.
    * Cleans up the ability if successfully triggered.
    * TKTK - trigger animator associated with the specific class
   */
   public bool TriggerAbility(){
      Debug.Log("Triggering ability");
      if (SelectedAbility == null)
         return false;

      var ret = SelectedAbility.Trigger();
      if (ret)
         SelectedAbility.CleanUp();
      
      return ret;
   }

   public void ChangeState(UnitState newState){
      unitState = newState;

      switch(newState){
         case UnitState.Alive:
            break;
         case UnitState.Stunned:
            break;
         case UnitState.Asleep:
            break;
         case UnitState.Dead:
            UnitManager.Instance.UnitDeath(this);
            break;
         default:
            throw new ArgumentOutOfRangeException(nameof(newState), newState, null);
      }
   }

   // Call this directly from ability if it bypasses damage reduction
   public float ApplyDamage(float damage){
      Health -= damage;

      Debug.Log($"{UnitName} hit for {damage} damage!");
   
      if (Health <= 0)
         ChangeState(UnitState.Dead);

      return damage;
   }

   public float ApplyPhysicalDamage(float damage, float pen = 0){
      damage -= (damage * Armor);
      // TKTK - implement pen calculation

      return ApplyDamage(damage);
   }

   public float ApplyMagicDamage(float damage, float pen = 0){
      damage -= (damage * MagicResist);
      // TKTK - implement pen calculation

      return ApplyDamage(damage);
   }
}

[Serializable]
public struct AbilityBinding {
   public BaseAbility abilityPrefab;
   public KeyCode binding;
}

public enum UnitState{
   Alive = 0,
   Stunned = 1,
   Asleep = 2,
   Dead = 3
}

public enum ClassType {
    Rogue = 0,
    Warrior = 1
}