// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.VMF_HarmonyPatches.Patch_GenConstruct_GetWallAttachedTo
// Assembly: VehicleMapFramework, Version=1.6.562.0, Culture=neutral, PublicKeyToken=null
// MVID: 10A61882-945F-4CFC-9B06-CA8EEF5ADB36
// Assembly location: D:\Programas\steamapps\workshop\content\294100\3426502333\1.6\Assemblies\VehicleMapFramework.dll

using HarmonyLib;
using RimWorld;
using System;
using System.Linq;
using Verse;

#nullable disable
namespace VehicleMapFramework.VMF_HarmonyPatches;

[HarmonyPatch(typeof (GenConstruct), "GetWallAttachedTo", new Type[] {typeof (Thing)})]
[PatchLevel(Level.Mandatory)]
public static class Patch_GenConstruct_GetWallAttachedTo
{
  public static void Postfix(Thing thing, ref Thing __result)
  {
    if (__result != null || ((BuildableDef) thing.def).PlaceWorkers.All<PlaceWorker>((Func<PlaceWorker, bool>) (p => !(p is PlaceWorker_AttachedWallMultiCell))) || (GenConstruct.BuiltDefOf(thing.def) is ThingDef thingDef ? thingDef.building : (BuildingProperties) null) == null || !thingDef.building.isAttachment)
      return;
    Rot4 rotation = thing.Rotation;
    CellRect cellRect = GenAdj.OccupiedRect(thing);
    __result = GenConstruct.GetWallAttachedTo(((CellRect) ref cellRect).GetCenterCellOnEdge(rotation), rotation, thing.Map);
    if (__result != null || ((CellRect) ref cellRect).GetSideLength(thing.Rotation) % 2 == 1)
      return;
    __result = GenConstruct.GetWallAttachedTo(((CellRect) ref cellRect).GetCenterCellOnEdge(rotation, -1), rotation, thing.Map);
  }
}
