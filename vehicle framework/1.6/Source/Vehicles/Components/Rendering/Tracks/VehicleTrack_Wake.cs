// Decompiled with JetBrains decompiler
// Type: Vehicles.VehicleTrack_Wake
// Assembly: Vehicles, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 81BFDC99-D8AD-42E1-8470-2F73E7836B4A
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\Vehicles.dll

using RimWorld;
using UnityEngine;
using Verse;

#nullable disable
namespace Vehicles;

public class VehicleTrack_Wake : VehicleTrack
{
  private const float DefaultSizeSplash = 10f;
  private const float DefaultSizePassiveSplash = 2f;
  private const float WakeDistanceInterval = 0.399424046f;
  public float size = 1f;
  public float speed = 1.6f;

  public override void TryPlaceTrack(VehiclePawn vehicle, ref Vector3 lastTrackPlacePos)
  {
    if ((double) GenGeo.MagnitudeHorizontalSquared(Vector3.op_Subtraction(vehicle.DrawTracker.DrawPos, lastTrackPlacePos)) > 0.39942404627799988)
    {
      Vector3 drawPos = vehicle.DrawTracker.DrawPos;
      if (!GenGrid.InBounds(IntVec3Utility.ToIntVec3(drawPos), ((Thing) vehicle).Map) || vehicle.beached)
        return;
      FleckMaker.WaterSplash(drawPos, ((Thing) vehicle).Map, 10f * this.size, this.speed);
      lastTrackPlacePos = drawPos;
    }
    else
    {
      if (!VehicleMod.settings.main.passiveWaterWaves || Find.TickManager.TicksGame % 360 != 0)
        return;
      float num = Mathf.PingPong((float) (Find.TickManager.TicksGame / 10), vehicle.VehicleDef.graphicData.drawSize.y / 4f);
      FleckMaker.WaterSplash(Vector3.op_Subtraction(vehicle.DrawTracker.DrawPos, new Vector3(0.0f, 0.0f, num)), ((Thing) vehicle).Map, 2f * this.size, this.speed);
    }
  }
}
