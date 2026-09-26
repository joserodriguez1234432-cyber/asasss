// Decompiled with JetBrains decompiler
// Type: Vehicles.CompUpgradeTree
// Assembly: Vehicles, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 81BFDC99-D8AD-42E1-8470-2F73E7836B4A
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\Vehicles.dll

using RimWorld;
using SmashTools;
using SmashTools.Performance;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;
using Verse.Sound;

#nullable disable
namespace Vehicles;

[StaticConstructorOnStartup]
public class CompUpgradeTree : VehicleComp, IRefundable
{
  private static readonly Material UnderfieldMat = MaterialPool.MatFrom("Things/Building/BuildingFrame/Underfield", ShaderDatabase.Transparent);
  private static readonly Texture2D CornerTex = ContentFinder<Texture2D>.Get("Things/Building/BuildingFrame/Corner", true);
  private static readonly Texture2D TileTex = ContentFinder<Texture2D>.Get("Things/Building/BuildingFrame/Tile", true);
  private static readonly Texture2D CancelIcon = ContentFinder<Texture2D>.Get("UI/Designators/Cancel", true);
  private static readonly Color FrameColor = new Color(0.6f, 0.6f, 0.6f);
  private Material cachedCornerMat;
  private Material cachedTileMat;
  private HashSet<string> upgrades = new HashSet<string>();
  private string nodeUnlocking;
  public UpgradeInProgress upgrade;
  public ThingOwner<Thing> upgradeContainer = new ThingOwner<Thing>();

  private Dictionary<string, List<UpgradeState>> States { get; } = new Dictionary<string, List<UpgradeState>>();

  public CompProperties_UpgradeTree Props => (CompProperties_UpgradeTree) this.props;

  public bool Upgrading => this.NodeUnlocking != (UpgradeNode) null;

  public UpgradeNode NodeUnlocking => this.upgrade?.node;

  private float PercentComplete
  {
    get
    {
      return !this.Upgrading || (double) this.NodeUnlocking.work <= 0.0 ? 0.0f : (float) (1.0 - (double) this.upgrade.WorkLeft / (double) this.NodeUnlocking.work);
    }
  }

  private Material CornerMat
  {
    get
    {
      if (!Object.op_Implicit((Object) this.cachedCornerMat))
        this.cachedCornerMat = MaterialPool.MatFrom(CompUpgradeTree.CornerTex, ShaderDatabase.MetaOverlay, CompUpgradeTree.FrameColor);
      return this.cachedCornerMat;
    }
  }

  private Material TileMat
  {
    get
    {
      if (!Object.op_Implicit((Object) this.cachedTileMat))
        this.cachedTileMat = MaterialPool.MatFrom(CompUpgradeTree.TileTex, ShaderDatabase.MetaOverlay, CompUpgradeTree.FrameColor);
      return this.cachedTileMat;
    }
  }

  public bool StoredCostSatisfied
  {
    get
    {
      if (this.NodeUnlocking == (UpgradeNode) null)
        return false;
      foreach (ThingDefCountClass ingredient in this.NodeUnlocking.ingredients)
      {
        if (((ThingOwner) this.upgradeContainer).TotalStackCountOfDef(ingredient.thingDef) < ingredient.count)
          return false;
      }
      return true;
    }
  }

  IEnumerable<(ThingDef thingDef, float count)> IRefundable.Refunds
  {
    get
    {
      if (!GenList.NullOrEmpty<Thing>((IList<Thing>) this.upgradeContainer))
      {
        List<Thing>.Enumerator enumerator = this.upgradeContainer.GetEnumerator();
        while (enumerator.MoveNext())
        {
          Thing current = enumerator.Current;
          yield return (current.def, (float) current.stackCount);
        }
        enumerator = new List<Thing>.Enumerator();
      }
      if (!GenCollection.NullOrEmpty<string>(this.upgrades))
      {
        foreach (string upgrade in this.upgrades)
        {
          UpgradeNode node = this.Props.def.GetNode(upgrade);
          if (node != (UpgradeNode) null && !GenList.NullOrEmpty<ThingDefCountClass>((IList<ThingDefCountClass>) node.ingredients))
          {
            List<ThingDefCountClass>.Enumerator enumerator = node.ingredients.GetEnumerator();
            while (enumerator.MoveNext())
            {
              ThingDefCountClass current = enumerator.Current;
              yield return (current.thingDef, (float) current.count);
            }
            enumerator = new List<ThingDefCountClass>.Enumerator();
          }
        }
      }
    }
  }

  public bool NodeUnlocked(UpgradeNode node) => this.upgrades.Contains(node.key);

  public UpgradeNode RootNode(UpgradeNode child)
  {
    UpgradeNode upgradeNode = child;
    while (!GenList.NullOrEmpty<string>((IList<string>) upgradeNode.prerequisiteNodes))
      upgradeNode = this.Props.def.GetNode(upgradeNode.prerequisiteNodes.First<string>());
    return upgradeNode;
  }

  public bool PrerequisitesMet(UpgradeNode node)
  {
    if (GenList.NullOrEmpty<string>((IList<string>) node.prerequisiteNodes))
      return true;
    foreach (string prerequisiteNode in node.prerequisiteNodes)
    {
      if (!this.upgrades.Contains(prerequisiteNode))
        return false;
    }
    return true;
  }

  public bool Disabled(UpgradeNode node)
  {
    if (!GenText.NullOrEmpty(node.disableIfUpgradeNodeEnabled) && this.upgrades.Contains(node.disableIfUpgradeNodeEnabled))
      return true;
    return !GenList.NullOrEmpty<string>((IList<string>) node.disableIfUpgradeNodesEnabled) && GenCollection.Any<string>(node.disableIfUpgradeNodesEnabled, (Predicate<string>) (key => this.upgrades.Contains(key)));
  }

  public bool LastNodeUnlocked(UpgradeNode node)
  {
    return !this.Vehicle.CompUpgradeTree.Props.def.nodes.FindAll((Predicate<UpgradeNode>) (x => x.prerequisiteNodes.Contains(node.key))).NotNullAndAny<UpgradeNode>((Predicate<UpgradeNode>) (preReqNode => this.Vehicle.CompUpgradeTree.NodeUnlocked(preReqNode)));
  }

  public void ResetUnlock(UpgradeNode node)
  {
    if ((object) node == null || !this.upgrades.Contains(node.key))
      return;
    if (!this.upgrades.Remove(node.key))
    {
      Trace.Fail($"Unable to reset {node.key}. Upgrade doesn't exist.");
    }
    else
    {
      if (!GenList.NullOrEmpty<Upgrade>((IList<Upgrade>) node.upgrades))
      {
        for (int index = node.upgrades.Count - 1; index >= 0; --index)
        {
          Upgrade upgrade = node.upgrades[index];
          try
          {
            upgrade.Refund(this.Vehicle);
          }
          catch (Exception ex)
          {
            Log.Error($"{"[VehicleFramework]"} Unable to reset {((object) this).GetType()} to {((Entity) this.Vehicle).LabelShort}. \nException: {ex}");
          }
        }
      }
      this.RefundIngredients(node);
      node.RemoveOverlays(this.Vehicle);
      SoundDef resetSound = node.resetSound;
      if (resetSound != null)
        SoundStarter.PlayOneShot(resetSound, SoundInfo.op_Implicit(new TargetInfo(((Thing) this.Vehicle).Position, ((Thing) this.Vehicle).Map, false)));
      this.Vehicle.EventRegistry[VehicleEventDefOf.UpgradeRefundCompleted].ExecuteEvents();
    }
  }

  private void RefundIngredients(UpgradeNode node)
  {
    if (GenList.NullOrEmpty<ThingDefCountClass>((IList<ThingDefCountClass>) node.ingredients))
      return;
    ThingOwner<Thing> container = new ThingOwner<Thing>();
    foreach (ThingDefCountClass ingredient in node.ingredients)
    {
      Thing thing = ThingMaker.MakeThing(ingredient.thingDef, (ThingDef) null);
      float num = Ext_Vehicles.RefundMaterialCount(this.Vehicle.VehicleDef, (DestroyMode) 4);
      thing.stackCount = GenMath.RoundRandom((float) ingredient.count * num);
      ((ThingOwner) container).TryAdd(thing, true);
    }
    ((ThingOwner) container).TryDropAllOutsideVehicle(((Thing) this.Vehicle).Map, GenAdj.OccupiedRect((Thing) this.Vehicle), (DestroyMode) 7);
  }

  public void FinishUnlock(UpgradeNode node)
  {
    UpgradeInProgress upgrade1 = this.upgrade;
    if (upgrade1 != null && upgrade1.Removal)
    {
      this.ResetUnlock(node);
    }
    else
    {
      if (!GenList.NullOrEmpty<string>((IList<string>) node.replaces))
      {
        foreach (string replace in node.replaces)
          this.ResetUnlock(this.Props.def.GetNode(replace));
      }
      if (!GenList.NullOrEmpty<Upgrade>((IList<Upgrade>) node.upgrades))
      {
        foreach (Upgrade upgrade2 in node.upgrades)
        {
          try
          {
            upgrade2.Unlock(this.Vehicle, false);
          }
          catch (Exception ex)
          {
            Log.Error($"{"[VehicleFramework]"} Unable to unlock {((object) this).GetType()} for {((Entity) this.Vehicle).LabelShort}. \nException: {ex}");
          }
        }
      }
      node.AddOverlays(this.Vehicle);
      SoundDef unlockSound = node.unlockSound;
      if (unlockSound != null)
        SoundStarter.PlayOneShot(unlockSound, SoundInfo.op_Implicit(new TargetInfo(((Thing) this.Vehicle).Position, ((Thing) this.Vehicle).Map, false)));
      this.upgrades.Add(node.key);
      ((ThingOwner) this.upgradeContainer).ClearAndDestroyContents((DestroyMode) 0);
      this.Vehicle.EventRegistry[VehicleEventDefOf.UpgradeCompleted].ExecuteEvents();
    }
  }

  public void ClearUpgrade()
  {
    ((Thing) this.Vehicle).Map.GetCachedMapComponent<VehicleReservationManager>().ClearReservedFor(this.Vehicle);
    ((ThingOwner) this.upgradeContainer).TryDropAll(((Thing) this.Vehicle).Position, ((Thing) this.Vehicle).Map, (ThingPlaceMode) 1, (Action<Thing, int>) null, (Predicate<IntVec3>) null, true);
    this.upgrade = (UpgradeInProgress) null;
    this.Vehicle.EventRegistry[VehicleEventDefOf.UpgradeCanceled].ExecuteEvents();
  }

  public void StartUnlock(UpgradeNode node)
  {
    this.upgrade = new UpgradeInProgress(this.Vehicle, node, false);
    ((ThingOwner) this.upgradeContainer).TryDropAll(((Thing) this.Vehicle).Position, ((Thing) this.Vehicle).Map, (ThingPlaceMode) 1, (Action<Thing, int>) null, (Predicate<IntVec3>) null, true);
    this.Vehicle.EventRegistry[VehicleEventDefOf.UpgradeEnqueued].ExecuteEvents();
  }

  public void RemoveUnlock(UpgradeNode node)
  {
    this.upgrade = new UpgradeInProgress(this.Vehicle, node, true);
    ((ThingOwner) this.upgradeContainer).TryDropAll(((Thing) this.Vehicle).Position, ((Thing) this.Vehicle).Map, (ThingPlaceMode) 1, (Action<Thing, int>) null, (Predicate<IntVec3>) null, true);
    this.Vehicle.EventRegistry[VehicleEventDefOf.UpgradeRefundEnqueued].ExecuteEvents();
  }

  private void PrepVehicleForWork()
  {
    this.Vehicle.ignition.Drafted = false;
    this.Vehicle.Angle = 0.0f;
    if (this.Vehicle.AllPawnsAboard.Count <= 0)
      return;
    this.Vehicle.DisembarkAll();
  }

  internal void ReactivateComps()
  {
    if (GenCollection.NullOrEmpty<string>(this.upgrades))
      return;
    foreach (string upgrade1 in this.upgrades)
    {
      UpgradeNode node = this.Props.def.GetNode(upgrade1);
      if (!(node == (UpgradeNode) null) && !GenList.NullOrEmpty<Upgrade>((IList<Upgrade>) node.upgrades))
      {
        foreach (Upgrade upgrade2 in node.upgrades)
        {
          if (upgrade2 is CompUpgrade compUpgrade)
          {
            try
            {
              compUpgrade.Unlock(this.Vehicle, true);
            }
            catch (Exception ex)
            {
              Log.Error($"{"[VehicleFramework]"} Unable to unlock {((object) this).GetType()} post-load for {((Entity) this.Vehicle).LabelShort}. \nException: {ex}");
            }
          }
        }
      }
    }
  }

  internal void ReloadUnlocks()
  {
    if (GenCollection.NullOrEmpty<string>(this.upgrades))
      return;
    foreach (string upgrade1 in this.upgrades)
    {
      UpgradeNode node = this.Props.def.GetNode(upgrade1);
      if (!(node == (UpgradeNode) null))
      {
        node.AddOverlays(this.Vehicle);
        if (!GenList.NullOrEmpty<Upgrade>((IList<Upgrade>) node.upgrades))
        {
          foreach (Upgrade upgrade2 in node.upgrades)
          {
            if (upgrade2.UnlockOnLoad)
            {
              try
              {
                upgrade2.Unlock(this.Vehicle, true);
              }
              catch (Exception ex)
              {
                Log.Error($"{"[VehicleFramework]"} Unable to unlock {((object) this).GetType()} post-load for {((Entity) this.Vehicle).LabelShort}. \nException: {ex}");
              }
            }
          }
        }
      }
    }
  }

  public void AddToContainer(Thing thing, int count)
  {
    ((ThingOwner) this.upgradeContainer).TryAddOrTransfer(thing, count, true);
    this.ValidateListers();
  }

  public void AddSettings(UpgradeState state)
  {
    if (!this.States.ContainsKey(state.key))
      this.States[state.key] = new List<UpgradeState>();
    this.States[state.key].Add(state);
  }

  public bool TryGetStates(string key, out List<UpgradeState> outList)
  {
    return this.States.TryGetValue(key, out outList);
  }

  public void RemoveSettings(UpgradeState state)
  {
    List<UpgradeState> upgradeStateList;
    if (!this.States.TryGetValue(state.key, out upgradeStateList))
      Log.Error($"Unable to locate {state.key} in state cache.");
    else
      upgradeStateList.Remove(state);
  }

  public virtual void PostDraw()
  {
    base.PostDraw();
    if (!this.Upgrading)
      return;
    Vector3 drawPos = ((Thing) this.Vehicle).DrawPos;
    Vector3 vector3_1;
    // ISSUE: explicit constructor call
    ((Vector3) ref vector3_1).\u002Ector((float) ((BuildableDef) this.Vehicle.VehicleDef).Size.x * 1.15f, 1f, (float) ((BuildableDef) this.Vehicle.VehicleDef).Size.z * 1.15f);
    Vector3 vector3_2 = drawPos;
    Rot4 rotation = ((Thing) this.Vehicle).Rotation;
    Quaternion asQuat1 = ((Rot4) ref rotation).AsQuat;
    Vector3 vector3_3 = vector3_1;
    Matrix4x4 matrix4x4_1 = Matrix4x4.TRS(vector3_2, asQuat1, vector3_3);
    Graphics.DrawMesh(MeshPool.plane10, matrix4x4_1, CompUpgradeTree.UnderfieldMat, 0);
    for (int index = 0; index < 4; ++index)
    {
      float num = (float) Mathf.Min(((Thing) this.Vehicle).RotatedSize.x, ((Thing) this.Vehicle).RotatedSize.z) * 0.38f;
      IntVec3 intVec3_1;
      switch (index)
      {
        case 0:
          intVec3_1 = new IntVec3(-1, 0, -1);
          break;
        case 1:
          intVec3_1 = new IntVec3(-1, 0, 1);
          break;
        case 2:
          intVec3_1 = new IntVec3(1, 0, 1);
          break;
        case 3:
          intVec3_1 = new IntVec3(1, 0, -1);
          break;
        default:
          throw new InvalidOperationException("Rot4");
      }
      IntVec3 intVec3_2 = intVec3_1;
      Vector3 vector3_4 = new Vector3();
      vector3_4.x = (float) intVec3_2.x * (float) ((double) ((Thing) this.Vehicle).RotatedSize.x / 2.0 - (double) num / 2.0);
      vector3_4.z = (float) intVec3_2.z * (float) ((double) ((Thing) this.Vehicle).RotatedSize.z / 2.0 - (double) num / 2.0);
      Vector3 vector3_5;
      // ISSUE: explicit constructor call
      ((Vector3) ref vector3_5).\u002Ector(num, 1f, num);
      Matrix4x4 matrix4x4_2 = new Matrix4x4();
      ref Matrix4x4 local = ref matrix4x4_2;
      Vector3 vector3_6 = Vector3.op_Addition(Vector3.op_Addition(drawPos, Vector3.op_Multiply(Vector3.up, 0.03f)), vector3_4);
      Rot4 rot4 = new Rot4(index);
      Quaternion asQuat2 = ((Rot4) ref rot4).AsQuat;
      Vector3 vector3_7 = vector3_5;
      ((Matrix4x4) ref local).SetTRS(vector3_6, asQuat2, vector3_7);
      Graphics.DrawMesh(MeshPool.plane10, matrix4x4_2, this.CornerMat, 0);
    }
    int num1 = Mathf.CeilToInt((float) ((double) this.PercentComplete * (double) ((Thing) this.Vehicle).RotatedSize.x * (double) ((Thing) this.Vehicle).RotatedSize.z * 4.0));
    IntVec2 intVec2_1 = IntVec2.op_Multiply(((Thing) this.Vehicle).RotatedSize, 2);
    for (int index = 0; index < num1; ++index)
    {
      IntVec2 intVec2_2 = new IntVec2()
      {
        z = index / intVec2_1.x
      };
      intVec2_2.x = index - intVec2_2.z * intVec2_1.x;
      Vector3 vector3_8 = Vector3.op_Addition(new Vector3((float) intVec2_2.x * 0.5f, 0.0f, (float) intVec2_2.z * 0.5f), drawPos);
      vector3_8.x -= (float) ((double) ((Thing) this.Vehicle).RotatedSize.x * 0.5 - 0.25);
      vector3_8.z -= (float) ((double) ((Thing) this.Vehicle).RotatedSize.z * 0.5 - 0.25);
      Vector3 vector3_9;
      // ISSUE: explicit constructor call
      ((Vector3) ref vector3_9).\u002Ector(0.5f, 1f, 0.5f);
      Matrix4x4 matrix4x4_3 = new Matrix4x4();
      ((Matrix4x4) ref matrix4x4_3).SetTRS(Vector3.op_Addition(vector3_8, Vector3.op_Multiply(Vector3.up, 0.02f)), Quaternion.identity, vector3_9);
      Graphics.DrawMesh(MeshPool.plane10, matrix4x4_3, this.TileMat, 0);
    }
  }

  public override AcceptanceReport CanDraft()
  {
    return this.Upgrading ? AcceptanceReport.op_Implicit(TranslatorFormattedStringExtensions.Translate("VF_DisabledByVehicleUpgrading", NamedArgument.op_Implicit(((Entity) this.Vehicle).LabelCap))) : AcceptanceReport.op_Implicit(true);
  }

  public virtual void CompTickRare()
  {
    base.CompTickRare();
    if (!((Thing) this.Vehicle).Spawned)
      return;
    this.ValidateListers();
  }

  private void ValidateListers()
  {
    if (!((Thing) this.Vehicle).Spawned)
      return;
    if (this.Upgrading)
    {
      if (this.upgrade.Removal || this.StoredCostSatisfied)
      {
        ((Thing) this.Vehicle).Map.GetCachedMapComponent<VehicleReservationManager>().RegisterLister(this.Vehicle, "Upgrade");
        ((Thing) this.Vehicle).Map.GetCachedMapComponent<VehicleReservationManager>().RemoveLister(this.Vehicle, "LoadUpgradeMaterials");
      }
      else
      {
        ((Thing) this.Vehicle).Map.GetCachedMapComponent<VehicleReservationManager>().RegisterLister(this.Vehicle, "LoadUpgradeMaterials");
        ((Thing) this.Vehicle).Map.GetCachedMapComponent<VehicleReservationManager>().RemoveLister(this.Vehicle, "Upgrade");
      }
    }
    else
    {
      ((Thing) this.Vehicle).Map.GetCachedMapComponent<VehicleReservationManager>().RemoveLister(this.Vehicle, "Upgrade");
      ((Thing) this.Vehicle).Map.GetCachedMapComponent<VehicleReservationManager>().RemoveLister(this.Vehicle, "LoadUpgradeMaterials");
    }
  }

  public override void EventRegistration()
  {
    base.EventRegistration();
    this.Vehicle.AddEvent<VehicleEventDef>(VehicleEventDefOf.UpgradeEnqueued, new Action(this.PrepVehicleForWork), new Action(this.ValidateListers));
    this.Vehicle.AddEvent<VehicleEventDef>(VehicleEventDefOf.UpgradeRefundEnqueued, new Action(this.PrepVehicleForWork), new Action(this.ValidateListers));
    this.Vehicle.AddEvent<VehicleEventDef>(VehicleEventDefOf.UpgradeCanceled, new Action(this.ValidateListers));
    this.Vehicle.AddEvent<VehicleEventDef>(VehicleEventDefOf.UpgradeCompleted, new Action(this.ValidateListers));
    this.Vehicle.AddEvent<VehicleEventDef>(VehicleEventDefOf.UpgradeRefundCompleted, new Action(this.ValidateListers));
  }

  public virtual IEnumerable<Gizmo> CompGetGizmosExtra()
  {
    CompUpgradeTree compUpgradeTree = this;
    if (compUpgradeTree.Upgrading)
    {
      Command_Action commandAction = new Command_Action();
      ((Command) commandAction).defaultLabel = TaggedString.op_Implicit(Translator.Translate("DesignatorCancel"));
      ((Command) commandAction).defaultDesc = TaggedString.op_Implicit(TranslatorFormattedStringExtensions.Translate("VF_CancelUpgrade", NamedArgument.op_Implicit(((Entity) compUpgradeTree.Vehicle).LabelCap)));
      ((Command) commandAction).icon = (Texture) CompUpgradeTree.CancelIcon;
      ((Command) commandAction).activateSound = SoundDefOf.Tick_Low;
      ((Command) commandAction).hotKey = KeyBindingDefOf.Designator_Cancel;
      commandAction.action = new Action(compUpgradeTree.ClearUpgrade);
      yield return (Gizmo) commandAction;
    }
  }

  public virtual string CompInspectStringExtra()
  {
    if (this.upgrade == null)
      return (string) null;
    if (this.StoredCostSatisfied)
      return $"{Translator.Translate("WorkLeft")}: {GenText.ToStringWorkAmount(this.Vehicle.CompUpgradeTree.upgrade.WorkLeft)}";
    StringBuilder stringBuilder;
    using (GlobalObjectPool.Get(out stringBuilder))
    {
      foreach (ThingDefCountClass ingredient in this.NodeUnlocking.ingredients)
      {
        int num = ((ThingOwner) this.upgradeContainer).TotalStackCountOfDef(ingredient.thingDef);
        if (num > 0)
          GenText.AppendWithComma(stringBuilder, $"{num}x {((Def) ingredient.thingDef).LabelCap}");
      }
      return stringBuilder.ToString();
    }
  }

  public virtual void PostExposeData()
  {
    base.PostExposeData();
    Scribe_Collections.Look<string>(ref this.upgrades, "upgrades", (LookMode) 1);
    Scribe_Values.Look<string>(ref this.nodeUnlocking, "nodeUnlocking", (string) null, false);
    Scribe_Deep.Look<UpgradeInProgress>(ref this.upgrade, "upgrade", Array.Empty<object>());
    Scribe_Deep.Look<ThingOwner<Thing>>(ref this.upgradeContainer, "upgradeContainer", Array.Empty<object>());
    if (this.upgrades == null)
      this.upgrades = new HashSet<string>();
    if (this.upgradeContainer != null)
      return;
    this.upgradeContainer = new ThingOwner<Thing>();
  }
}
