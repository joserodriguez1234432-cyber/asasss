// Decompiled with JetBrains decompiler
// Type: Vehicles.Editor.PathPreview
// Assembly: Vehicles, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 81BFDC99-D8AD-42E1-8470-2F73E7836B4A
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\Vehicles.dll

using RimWorld;
using SmashTools;
using SmashTools.Targeting;
using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using Verse;
using Verse.AI;
using Verse.Sound;

#nullable disable
namespace Vehicles.Editor;

internal class PathPreview : ITargeter, IDisposable
{
  private readonly Texture2D mouseAttachment = ContentFinder<Texture2D>.Get("UI/Overlays/WaypointMouseAttachment", true);
  private readonly VehiclePawn vehicle;
  private readonly VehiclePathFinder pathFinder;
  private readonly VehicleReachability reachability;
  private readonly TraverseParms parms;
  private IntVec3 start = IntVec3.Invalid;
  private IntVec3 dest = IntVec3.Invalid;
  private VehiclePath path;

  public PathPreview(VehiclePawn vehicle)
  {
    this.vehicle = vehicle;
    VehiclePathingSystem.VehiclePathData vehiclePathData = ((Thing) vehicle).Map.GetCachedMapComponent<VehiclePathingSystem>()[vehicle.VehicleDef];
    this.pathFinder = vehiclePathData.VehiclePathFinder;
    this.reachability = vehiclePathData.VehicleReachability;
    this.parms = TraverseParms.For((Pawn) vehicle, (Danger) 3, (TraverseMode) 4, false, true, false, true);
  }

  void ITargeter.OnStart()
  {
  }

  void ITargeter.OnStop() => this.Dispose();

  void ITargeter.OnGUI()
  {
    if (KeyBindingDefOf.Cancel.KeyDownEvent)
    {
      this.Stop();
      Event.current.Use();
    }
    else
    {
      GenUI.DrawMouseAttachment(this.mouseAttachment);
      IntVec3 intVec3 = UI.MouseCell();
      if (!GenGrid.InBounds(intVec3, ((Thing) this.vehicle).Map))
        return;
      Event current = Event.current;
      if (current == null || current.type != null)
        return;
      switch (Event.current.button)
      {
        case 0:
          if (!((IntVec3) ref this.start).IsValid)
            this.start = intVec3;
          else if (!this.reachability.CanReachVehicle(this.start, LocalTargetInfo.op_Implicit(intVec3), (PathEndMode) 1, this.parms))
          {
            SoundStarter.PlayOneShotOnCamera(SoundDefOf.ClickReject, (Map) null);
          }
          else
          {
            this.dest = intVec3;
            this.CalculatePath();
            SoundStarter.PlayOneShotOnCamera(SoundDefOf.Checkbox_TurnedOn, (Map) null);
          }
          Event.current.Use();
          break;
        case 1:
          this.path?.Dispose();
          this.path = (VehiclePath) null;
          if (((IntVec3) ref this.dest).IsValid)
            this.dest = IntVec3.Invalid;
          else
            this.start = IntVec3.Invalid;
          Event.current.Use();
          SoundStarter.PlayOneShotOnCamera(SoundDefOf.Checkbox_TurnedOff, (Map) null);
          break;
      }
    }
  }

  void ITargeter.Update()
  {
    if (((IntVec3) ref this.start).IsValid)
      GenDraw.DrawRadiusRing(this.start, 1f);
    if (((IntVec3) ref this.dest).IsValid)
      GenDraw.DrawRadiusRing(this.dest, 1f);
    if (this.path == null)
      return;
    this.path.DrawPath(this.vehicle);
    if (GenTicks.TicksGame % 15 != 0)
      return;
    foreach (IntVec3 node in (IEnumerable<IntVec3>) this.path.Nodes)
      ((Thing) this.vehicle).Map.debugDrawer.FlashCell(node, 0.0f, (string) null, 15);
  }

  private void CalculatePath()
  {
    this.path = this.pathFinder.FindPath(this.start, LocalTargetInfo.op_Implicit(this.dest), this.parms, CancellationToken.None, (PathEndMode) 1);
  }

  public void Dispose()
  {
    this.path?.Dispose();
    this.path = (VehiclePath) null;
  }
}
