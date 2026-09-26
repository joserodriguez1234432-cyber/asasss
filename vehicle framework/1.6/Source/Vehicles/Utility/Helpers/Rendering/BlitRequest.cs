// Decompiled with JetBrains decompiler
// Type: Vehicles.Rendering.BlitRequest
// Assembly: Vehicles, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 81BFDC99-D8AD-42E1-8470-2F73E7836B4A
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\Vehicles.dll

using JetBrains.Annotations;
using SmashTools;
using System.Collections.Generic;
using Verse;

#nullable disable
namespace Vehicles.Rendering;

[UsedImplicitly]
public struct BlitRequest
{
  public readonly VehicleDef vehicleDef;
  public Rot8 rot;
  public PatternData patternData;
  public List<IBlitTarget> blitTargets;
  public bool iconFrame;

  public BlitRequest(Rot8 rot, PatternData patternData)
  {
    this.vehicleDef = (VehicleDef) null;
    this.iconFrame = false;
    this.blitTargets = new List<IBlitTarget>();
    this.rot = rot;
    this.patternData = patternData;
    if (VehicleMod.settings.main.useCustomShaders)
      return;
    patternData.patternDef = PatternDefOf.Default;
  }

  public BlitRequest(VehicleDef vehicleDef)
    : this(vehicleDef.drawProperties.displayRotation, GenCollection.TryGetValue<string, PatternData>((IReadOnlyDictionary<string, PatternData>) VehicleMod.settings.vehicles.defaultGraphics, ((Def) vehicleDef).defName, (PatternData) vehicleDef.graphicData))
  {
    this.vehicleDef = vehicleDef;
  }

  public BlitRequest(VehiclePawn vehicle)
    : this(vehicle.VehicleDef.drawProperties.displayRotation, vehicle.patternData)
  {
    this.vehicleDef = vehicle.VehicleDef;
  }

  public static BlitRequest For(VehiclePawn vehicle)
  {
    VehicleDef vehicleDef = vehicle.VehicleDef;
    BlitRequest blitRequest = new BlitRequest(vehicle);
    blitRequest.blitTargets.Add((IBlitTarget) vehicleDef);
    CompVehicleTurrets cachedComp = vehicle.GetCachedComp<CompVehicleTurrets>();
    if (cachedComp != null && !cachedComp.Turrets.NullOrEmpty<VehicleTurret>())
    {
      foreach (VehicleTurret turret in (IEnumerable<VehicleTurret>) cachedComp.Turrets)
      {
        if (!turret.NoGraphic)
          blitRequest.blitTargets.Add((IBlitTarget) turret);
      }
    }
    if (!GenList.NullOrEmpty<GraphicOverlay>((IList<GraphicOverlay>) vehicle.DrawTracker.overlayRenderer.AllOverlaysListForReading))
      blitRequest.blitTargets.AddRange((IEnumerable<IBlitTarget>) vehicle.DrawTracker.overlayRenderer.AllOverlaysListForReading);
    return blitRequest;
  }

  public static BlitRequest For(VehicleDef vehicleDef)
  {
    BlitRequest blitRequest = new BlitRequest(vehicleDef);
    blitRequest.blitTargets.Add((IBlitTarget) vehicleDef);
    CompProperties_VehicleTurrets sortedCompProperties = vehicleDef.GetSortedCompProperties<CompProperties_VehicleTurrets>();
    if (sortedCompProperties != null)
    {
      foreach (VehicleTurret turret in sortedCompProperties.turrets)
      {
        if (!turret.NoGraphic)
          blitRequest.blitTargets.Add((IBlitTarget) turret);
      }
    }
    if (!GenList.NullOrEmpty<GraphicOverlay>((IList<GraphicOverlay>) vehicleDef.drawProperties.overlays))
      blitRequest.blitTargets.AddRange((IEnumerable<IBlitTarget>) vehicleDef.drawProperties.overlays);
    return blitRequest;
  }
}
