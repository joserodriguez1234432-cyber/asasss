// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.Reactor_Sink
// Assembly: VehicleMapFramework, Version=1.6.562.0, Culture=neutral, PublicKeyToken=null
// MVID: 10A61882-945F-4CFC-9B06-CA8EEF5ADB36
// Assembly location: D:\Programas\steamapps\workshop\content\294100\3426502333\1.6\Assemblies\VehicleMapFramework.dll

using JetBrains.Annotations;
using LudeonTK;
using RimWorld;
using SmashTools;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Vehicles;
using Verse;
using Verse.Sound;

#nullable disable
namespace VehicleMapFramework;

[UsedImplicitly]
public class Reactor_Sink : Reactor, ITweakFields
{
  [TweakField]
  [SliderValues(MinValue = 0.0f, MaxValue = 1f, Increment = 0.01f, RoundDecimalPlaces = 2)]
  public float chance = 1f;
  [TweakField]
  [SliderValues(MinValue = 0.0f, MaxValue = 1f, Increment = 0.05f, RoundDecimalPlaces = 2)]
  [LoadAlias("maxHealth")]
  public float healthPercent = 1f;
  [TweakField]
  public float speed = 5f;
  [TweakField]
  public FloatRange angle = new FloatRange(-5f, 5f);
  [TweakField]
  public FloatRange rotationRate = FloatRange.Zero;
  public string overlayId;
  public string resetKey;
  public Color overlayColor;
  public SimpleCurve colorOverlayAlphaCurve;

  string ITweakFields.Category => "";

  string ITweakFields.Label => nameof (Reactor_Sink);

  public virtual void Hit(
    VehiclePawn vehicle,
    VehicleComponent component,
    ref DamageInfo dinfo,
    VehicleComponent.Penetration penetration)
  {
    if (!((Thing) vehicle).Spawned || (double) component.Health <= 0.0 || (double) component.HealthPercent > (double) this.healthPercent || !Rand.Chance(this.chance))
      return;
    this.SpawnMote(vehicle);
    this.ResetUpgrades(vehicle);
  }

  private void SpawnMote(VehiclePawn vehicle)
  {
    if (!(vehicle is VehiclePawnWithMap vehiclePawnWithMap))
      return;
    CompUpgradeTree compUpgradeTree = vehicle.CompUpgradeTree;
    if (compUpgradeTree == null)
      return;
    ExpansionUpgrade expansionUpgrade = (ExpansionUpgrade) null;
    foreach (UpgradeNode node in compUpgradeTree.Props.def.nodes)
    {
      if (node.key == this.resetKey && compUpgradeTree.NodeUnlocked(node))
      {
        expansionUpgrade = node.upgrades.OfType<ExpansionUpgrade>().FirstOrDefault<ExpansionUpgrade>();
        if (expansionUpgrade != null)
          break;
      }
    }
    Vector3 drawPos = ((Thing) vehicle).DrawPos;
    List<GraphicOverlay> overlaysListForReading = vehicle.DrawTracker.overlayRenderer.AllOverlaysListForReading;
    CellRect? nullable = new CellRect?();
    if (expansionUpgrade != null)
    {
      foreach (CellRect expandArea in expansionUpgrade.expandAreas)
      {
        CellRect cellRect;
        if (!nullable.HasValue)
        {
          cellRect = ((CellRect) ref expandArea).MovedBy(IntVec2.One);
        }
        else
        {
          CellRect valueOrDefault = nullable.GetValueOrDefault();
          cellRect = ((CellRect) ref valueOrDefault).Encapsulate(((CellRect) ref expandArea).MovedBy(IntVec2.One));
        }
        nullable = new CellRect?(cellRect);
      }
    }
    nullable.GetValueOrDefault();
    if (!nullable.HasValue)
      nullable = new CellRect?(CellRect.Empty);
    Rot4 rot = vehicle.FullRotation.RotForVehicleDraw();
    for (int index = overlaysListForReading.Count - 1; index >= 0; --index)
    {
      GraphicOverlay graphicOverlay = overlaysListForReading[index];
      if (graphicOverlay.data.identifier == this.overlayId)
      {
        MoteThrownSinker moteThrownSinker = (MoteThrownSinker) ThingMaker.MakeThing(VMF_DefOf.VMF_MoteSink, (ThingDef) null);
        Vector2 drawSize = graphicOverlay.Graphic.drawSize;
        Vector2 vector2 = ((Rot4) ref rot).IsHorizontal ? Vector2Utility.Rotated(drawSize) : drawSize;
        (int, int) valueTuple = (Mathf.CeilToInt(vector2.x * 256f), Mathf.CeilToInt(vector2.y * 256f));
        Texture vehicleMapTexture = VehicleMapUIRenderer.GetOverlayWithVehicleMapTexture(vehiclePawnWithMap, graphicOverlay, rot, valueTuple, nullable.Value);
        moteThrownSinker.SetParameters(vehicleMapTexture, Quaternion.AngleAxis(-VehicleMapUtility.get_ExtraAngle(vehicle), Vector3.up), Vector3Utility.WithY(Vector2Utility.ToVector3(vector2), 1f), valueTuple, vehiclePawnWithMap, graphicOverlay, this.overlayColor, this.colorOverlayAlphaCurve);
        moteThrownSinker.SetVelocity(((FloatRange) ref this.angle).RandomInRange, this.speed);
        ((Mote) moteThrownSinker).exactPosition = Vector3.op_Addition(drawPos, Vector3Utility.RotatedBy(graphicOverlay.Graphic.DrawOffset(rot), VehicleMapUtility.get_ExtraAngle(vehicle)));
        GenSpawn.Spawn((Thing) moteThrownSinker, IntVec3Utility.ToIntVec3(((Mote) moteThrownSinker).exactPosition), ((Thing) vehicle).Map, (WipeMode) 0);
      }
    }
  }

  private void ResetUpgrades(VehiclePawn vehicle)
  {
    CompUpgradeTree compUpgradeTree = vehicle.CompUpgradeTree;
    if (compUpgradeTree == null)
      return;
    foreach (UpgradeNode node in compUpgradeTree.Props.def.nodes)
    {
      if (node.key == this.resetKey && compUpgradeTree.NodeUnlocked(node))
      {
        List<ThingDefCountClass> ingredients = node.ingredients;
        node.ingredients = new List<ThingDefCountClass>();
        compUpgradeTree.ResetUnlock(node);
        node.ingredients = ingredients;
      }
    }
  }

  void ITweakFields.OnFieldChanged()
  {
  }

  [DebugAction("Vehicle Map Framework", "Sink component", false, false, false, false, false, 0, false)]
  private static void SinkComponent(Pawn pawn)
  {
    if (!(pawn is VehiclePawn vehicle))
    {
      SoundStarter.PlayOneShotOnCamera(SoundDefOf.ClickReject, (Map) null);
    }
    else
    {
      foreach (VehicleComponent component in vehicle.statHandler.components)
      {
        if (!GenList.NullOrEmpty<Reactor>((IList<Reactor>) component.props.reactors))
        {
          Reactor_Sink reactorSink = component.props.reactors.OfType<Reactor_Sink>().FirstOrDefault<Reactor_Sink>();
          if (reactorSink != null)
          {
            reactorSink.SpawnMote(vehicle);
            reactorSink.ResetUpgrades(vehicle);
            break;
          }
        }
      }
    }
  }
}
