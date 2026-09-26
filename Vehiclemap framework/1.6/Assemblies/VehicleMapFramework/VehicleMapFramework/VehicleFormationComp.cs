// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.VehicleFormationComp
// Assembly: VehicleMapFramework, Version=1.6.562.0, Culture=neutral, PublicKeyToken=null
// MVID: 10A61882-945F-4CFC-9B06-CA8EEF5ADB36
// Assembly location: D:\Programas\steamapps\workshop\content\294100\3426502333\1.6\Assemblies\VehicleMapFramework.dll

using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Vehicles;
using Verse;

#nullable disable
namespace VehicleMapFramework;

public class VehicleFormationComp : WorldObjectComp
{
  private Dictionary<VehiclePawn, VehicleFormationComp.DrawData> drawPositions = new Dictionary<VehiclePawn, VehicleFormationComp.DrawData>();
  private List<VehiclePawn> keysWorkingList;
  private List<VehicleFormationComp.DrawData> valuesWorkingList;

  public Dictionary<VehiclePawn, VehicleFormationComp.DrawData> DrawPositions => this.drawPositions;

  public virtual void Initialize(WorldObjectCompProperties _props)
  {
    base.Initialize(_props);
    FrameDelay.DelayOne<VehicleFormationComp>((Action<VehicleFormationComp>) (state => state.RecalculateVehiclePositions()), this);
  }

  public void RecalculateVehiclePositions()
  {
    this.drawPositions.Clear();
    foreach (VehiclePawn vehicle1 in VehicleCaravanHelper.get_Vehicles(this.parent))
    {
      if (vehicle1 is VehiclePawnWithMap vehicle2)
      {
        VehicleDef vehicleDef = vehicle1.VehicleDef;
        if (vehicleDef != null && UniqueVehicleUtility.get_IsUniqueVehicle(vehicleDef))
          vehicle2.ResizeNow();
      }
      this.FindVehiclePosition(vehicle1);
    }
    this.CenteredDrawPositions();
  }

  public void FindVehiclePosition(VehiclePawn vehicle)
  {
    if (this.drawPositions.ContainsKey(vehicle))
      return;
    CellRect cellRect1 = CellRect.CenteredOn(IntVec3.Zero, ((BuildableDef) vehicle.VehicleDef).Size);
    int num = GenRadial.NumCellsInRadius(ModCompat.CombatExtended ? 119f : GenRadial.MaxRadialPatternRadius - 0.1f);
    for (int index = 0; index < num; ++index)
    {
      CellRect cellRect2 = ((CellRect) ref cellRect1).MovedBy(GenRadial.RadialPattern[index]);
      bool flag = true;
      foreach (KeyValuePair<VehiclePawn, VehicleFormationComp.DrawData> drawPosition in this.drawPositions)
      {
        if (((CellRect) ref cellRect2).Overlaps(((CellRect) ref drawPosition.Value.cellRect).ExpandedBy(1)))
        {
          flag = false;
          break;
        }
      }
      if (flag)
      {
        this.drawPositions[vehicle] = new VehicleFormationComp.DrawData(cellRect2, Vector3Utility.SetToAltitude(((CellRect) ref cellRect2).CenterVector3, (AltitudeLayer) 20));
        return;
      }
    }
    VMF_Log.Error($"Could not find draw position for {((Pawn) vehicle).Name}.");
    this.drawPositions[vehicle] = new VehicleFormationComp.DrawData(CellRect.Empty, Vector3Utility.WithY(Vector3.zero, Altitudes.AltitudeFor((AltitudeLayer) 20)));
  }

  public void CenteredDrawPositions()
  {
    if (GenDictionary.NullOrEmpty<VehiclePawn, VehicleFormationComp.DrawData>(this.drawPositions))
      return;
    Dictionary<VehiclePawn, VehicleFormationComp.DrawData>.ValueCollection values = this.drawPositions.Values;
    Vector3 vector3;
    // ISSUE: explicit constructor call
    ((Vector3) ref vector3).\u002Ector(values.Average<VehicleFormationComp.DrawData>((Func<VehicleFormationComp.DrawData, float>) (p => ((CellRect) ref p.cellRect).CenterVector3.x)), 0.0f, values.Average<VehicleFormationComp.DrawData>((Func<VehicleFormationComp.DrawData, float>) (p => ((CellRect) ref p.cellRect).CenterVector3.z)));
    foreach (VehiclePawn key in this.drawPositions.Keys.ToArray<VehiclePawn>())
    {
      VehicleFormationComp.DrawData drawPosition = this.drawPositions[key];
      this.drawPositions[key] = drawPosition with
      {
        position = Vector3.op_Subtraction(Vector3Utility.SetToAltitude(((CellRect) ref drawPosition.cellRect).CenterVector3, (AltitudeLayer) 20), vector3)
      };
    }
    foreach (VehiclePawnWithMap key in VehicleCaravanHelper.get_Vehicles(this.parent).OfType<VehiclePawnWithMap>())
      key.RecacheDrawPos(this.drawPositions[(VehiclePawn) key].position);
  }

  public virtual void PostExposeData()
  {
    Scribe_Collections.Look<VehiclePawn, VehicleFormationComp.DrawData>(ref this.drawPositions, "drawPositions", (LookMode) 3, (LookMode) 2, ref this.keysWorkingList, ref this.valuesWorkingList, false, false, false);
    if (Scribe.mode != 4)
      return;
    if (this.drawPositions == null)
      this.drawPositions = new Dictionary<VehiclePawn, VehicleFormationComp.DrawData>();
    if (this.drawPositions.Count != 0)
      return;
    this.RecalculateVehiclePositions();
  }

  public struct DrawData(CellRect cellRect, Vector3 position) : IExposable
  {
    public CellRect cellRect = cellRect;
    public Vector3 position = position;

    void IExposable.ExposeData()
    {
      Scribe_Values.Look<CellRect>(ref this.cellRect, "cellRect", new CellRect(), false);
      Scribe_Values.Look<Vector3>(ref this.position, "position", new Vector3(), false);
    }
  }
}
