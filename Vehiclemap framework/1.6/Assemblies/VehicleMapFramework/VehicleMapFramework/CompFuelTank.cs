// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.CompFuelTank
// Assembly: VehicleMapFramework, Version=1.6.562.0, Culture=neutral, PublicKeyToken=null
// MVID: 10A61882-945F-4CFC-9B06-CA8EEF5ADB36
// Assembly location: D:\Programas\steamapps\workshop\content\294100\3426502333\1.6\Assemblies\VehicleMapFramework.dll

using RimWorld;
using SmashTools;
using System;
using System.Collections.Generic;
using UnityEngine;
using Vehicles;
using Verse;

#nullable disable
namespace VehicleMapFramework;

[StaticConstructorOnStartup]
public class CompFuelTank : CompRefuelable
{
  private static readonly Vector3 DrawOffset = new Vector3(0.0015f, 0.1f, -5f / 16f);
  private static readonly Vector2 BarSize = new Vector2(0.15f, 0.18f);
  private static readonly Material UnfilledMat = BaseContent.ClearMat;
  private Material FilledMat;

  public VehiclePawnWithMap Vehicle
  {
    get
    {
      VehiclePawnWithMap vehicle;
      return !((Thing) ((ThingComp) this).parent).IsOnVehicleMapOf(out vehicle) ? (VehiclePawnWithMap) null : vehicle;
    }
  }

  public virtual void PostSpawnSetup(bool respawningAfterLoad)
  {
    LongEventHandler.ExecuteWhenFinished((Action) (() =>
    {
      VehiclePawnWithMap vehicle;
      if (!((Thing) ((ThingComp) this).parent).IsOnVehicleMapOf(out vehicle))
        return;
      vehicle.FuelTankComps.Add(this);
      if (ModCompat.VGE && ((Def) ((Thing) vehicle).def).HasModExtension<VehicleMapProps_Gravship>())
        this.FilledMat = SolidColorMaterials.SimpleSolidColorMaterial(new Color(0.3f, 0.2f, 0.5f), false);
      else
        this.FilledMat = SolidColorMaterials.SimpleSolidColorMaterial(new Color(0.4f, 0.25f, 0.1f), false);
    }));
  }

  public virtual void PostDeSpawn(Map map, DestroyMode mode = 0)
  {
    VehiclePawnWithMap vehicle;
    if (!map.IsVehicleMapOf(out vehicle))
      return;
    vehicle.FuelTankComps.Remove(this);
  }

  public virtual void PostDraw()
  {
    VehiclePawnWithMap vehicle;
    if (!((Thing) ((ThingComp) this).parent).IsOnVehicleMapOf(out vehicle) || vehicle.CompFueledTravel == null)
      return;
    Rot4 rot4 = this.Vehicle.FullRotation.RotForVehicleDraw();
    if (!((Rot4) ref rot4).IsHorizontal)
      rot4 = ((Rot4) ref rot4).Opposite;
    if (GridsUtility.GetFirstThing(IntVec3.op_Addition(((Thing) ((ThingComp) this).parent).Position, ((Rot4) ref rot4).FacingCell), ((Thing) ((ThingComp) this).parent).Map, ((Thing) ((ThingComp) this).parent).def) != null)
      return;
    GenDraw.FillableBarRequest fillableBarRequest = new GenDraw.FillableBarRequest()
    {
      center = Vector3.op_Addition(Vector3.op_Addition(((Thing) ((ThingComp) this).parent).DrawPos, Vector3Utility.RotatedBy(CompFuelTank.DrawOffset, -vehicle.Angle + vehicle.Transform.rotation)), Vector3.op_Multiply(Vector3.down, 0.015f)),
      size = CompFuelTank.BarSize,
      fillPercent = vehicle.CompFueledTravel.FuelPercent,
      filledMat = this.FilledMat,
      unfilledMat = CompFuelTank.UnfilledMat,
      margin = 0.03f,
      rotation = Rot8.FromAngle(Mathf.Repeat(-vehicle.Angle, 360f)).AsRot4Force()
    };
    Rot8Utility.Rotate(ref fillableBarRequest.rotation, (RotationDirection) 1);
    GenDraw.DrawFillableBar(fillableBarRequest);
  }

  public virtual IEnumerable<Gizmo> CompGetGizmosExtra()
  {
    CompFuelTank compFuelTank = this;
    VehiclePawnWithMap vehicle;
    if (((Thing) ((ThingComp) compFuelTank).parent).IsOnVehicleMapOf(out vehicle))
    {
      CompGravshipFacilityPossibly facilityPossibly;
      if (!ThingCompUtility.TryGetComp<CompGravshipFacilityPossibly>(((ThingComp) compFuelTank).parent, ref facilityPossibly) || GenCollection.Empty<Thing>(((CompFacility) facilityPossibly).LinkedBuildings))
      {
        CompFueledTravel compFueledTravel = vehicle.CompFueledTravel;
        if (compFueledTravel != null)
        {
          foreach (Gizmo gizmo in ((ThingComp) compFueledTravel).CompGetGizmosExtra())
            yield return gizmo;
        }
      }
      else
      {
        // ISSUE: reference to a compiler-generated method
        foreach (Gizmo gizmo in compFuelTank.\u003C\u003En__0())
          yield return gizmo;
      }
    }
  }

  public virtual string CompInspectStringExtra()
  {
    VehiclePawnWithMap vehicle;
    if (((Thing) ((ThingComp) this).parent).IsOnVehicleMapOf(out vehicle))
    {
      CompFueledTravel compFueledTravel = vehicle.CompFueledTravel;
      if (compFueledTravel != null && !((Def) ((Thing) vehicle).def).HasModExtension<VehicleMapProps_Gravship>())
        return $"{Translator.TranslateSimple("Fuel")}: {GenText.ToStringDecimalIfSmall(compFueledTravel.Fuel)} / {GenText.ToStringDecimalIfSmall(compFueledTravel.FuelCapacity)}";
    }
    return base.CompInspectStringExtra();
  }
}
