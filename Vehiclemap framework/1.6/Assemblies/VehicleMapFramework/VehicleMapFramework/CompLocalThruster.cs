// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.CompLocalThruster
// Assembly: VehicleMapFramework, Version=1.6.562.0, Culture=neutral, PublicKeyToken=null
// MVID: 10A61882-945F-4CFC-9B06-CA8EEF5ADB36
// Assembly location: D:\Programas\steamapps\workshop\content\294100\3426502333\1.6\Assemblies\VehicleMapFramework.dll

using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;

#nullable disable
namespace VehicleMapFramework;

public class CompLocalThruster : ThingComp
{
  public CompGravshipFacility CompGravshipFacility
  {
    get
    {
      if (this.\u003CCompGravshipFacility\u003Ek__BackingField == null)
        this.\u003CCompGravshipFacility\u003Ek__BackingField = this.parent.GetComp<CompGravshipFacility>();
      return this.\u003CCompGravshipFacility\u003Ek__BackingField;
    }
  }

  public virtual IEnumerable<Gizmo> CompGetGizmosExtra()
  {
    CompLocalThruster compLocalThruster = this;
    PlanetTile tile = ((Thing) compLocalThruster.parent).Tile;
    PlanetLayerDef layerDef = ((PlanetTile) ref tile).LayerDef;
    if ((layerDef != null ? (!layerDef.isSpace ? 1 : 0) : 1) == 0)
    {
      VehiclePawnWithMap vehicle;
      if (!((Thing) compLocalThruster.parent).IsOnVehicleMapOf(out vehicle))
      {
        Command_Toggle commandToggle = new Command_Toggle();
        ((Command) commandToggle).defaultLabel = TaggedString.op_Implicit(Translator.Translate("VMF_VehicleMode"));
        ((Command) commandToggle).icon = (Texture) ContentFinder<Texture2D>.Get("VehicleMapFramework/UI/GravshipVehicleMode", true);
        commandToggle.toggleAction = new Action(compLocalThruster.GenerateGravshipVehicle);
        commandToggle.isActive = (Func<bool>) (() => false);
        yield return (Gizmo) commandToggle;
      }
      else if (((Thing) vehicle).Spawned && ((Def) ((Thing) vehicle).def).HasModExtension<VehicleMapProps_Gravship>())
      {
        Command_Toggle commandToggle = new Command_Toggle();
        ((Command) commandToggle).defaultLabel = TaggedString.op_Implicit(Translator.Translate("VMF_VehicleMode"));
        ((Command) commandToggle).icon = (Texture) ContentFinder<Texture2D>.Get("VehicleMapFramework/UI/GravshipVehicleMode", true);
        commandToggle.toggleAction = (Action) (() => this.PlaceGravship(vehicle));
        commandToggle.isActive = (Func<bool>) (() => true);
        yield return (Gizmo) commandToggle;
      }
    }
  }

  public void GenerateGravshipVehicle()
  {
    AcceptanceReport gravshipVehicle = GravshipVehicleUtility.GenerateGravshipVehicle(this.CompGravshipFacility?.engine, VMF_DefOf.VMF_GravshipVehicleBaseSpace, false);
    if (((AcceptanceReport) ref gravshipVehicle).Accepted)
      return;
    Messages.Message(((AcceptanceReport) ref gravshipVehicle).Reason, MessageTypeDefOf.RejectInput, false);
  }

  public void PlaceGravship(VehiclePawnWithMap vehicle)
  {
    AcceptanceReport acceptanceReport = GravshipVehicleUtility.PlaceGravshipVehicle(this.CompGravshipFacility?.engine, vehicle);
    if (((AcceptanceReport) ref acceptanceReport).Accepted)
      return;
    Messages.Message(((AcceptanceReport) ref acceptanceReport).Reason, MessageTypeDefOf.RejectInput, false);
  }
}
