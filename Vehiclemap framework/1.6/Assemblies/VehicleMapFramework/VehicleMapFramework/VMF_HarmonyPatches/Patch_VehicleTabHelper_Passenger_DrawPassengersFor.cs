// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.VMF_HarmonyPatches.Patch_VehicleTabHelper_Passenger_DrawPassengersFor
// Assembly: VehicleMapFramework, Version=1.6.562.0, Culture=neutral, PublicKeyToken=null
// MVID: 10A61882-945F-4CFC-9B06-CA8EEF5ADB36
// Assembly location: D:\Programas\steamapps\workshop\content\294100\3426502333\1.6\Assemblies\VehicleMapFramework.dll

using HarmonyLib;
using System.Collections.Generic;
using UnityEngine;
using Vehicles;
using Verse;

#nullable disable
namespace VehicleMapFramework.VMF_HarmonyPatches;

[HarmonyPatchCategory("VMF_Patches_VehicleFramework")]
[HarmonyPatch(typeof (VehicleTabHelper_Passenger), "DrawPassengersFor")]
[PatchLevel(Level.Safe)]
public static class Patch_VehicleTabHelper_Passenger_DrawPassengersFor
{
  private const float PawnRowHeight = 50f;

  public static void Postfix(
    ref float curY,
    Rect viewRect,
    Vector2 scrollPos,
    VehiclePawn vehicle,
    ref Pawn moreDetailsForPawn,
    Pawn ___draggedPawn,
    ref IThingHolder ___transferToHolder,
    ref bool ___overDropSpot,
    ref Pawn ___hoveringOverPawn)
  {
    if (!(vehicle is VehiclePawnWithMap vehiclePawnWithMap))
      return;
    List<Pawn> pawnList = Patch_MapPawns_AllPawnsSpawned.AllPawnsSpawned(vehiclePawnWithMap.VehicleMap.mapPawns);
    Rect rect;
    // ISSUE: explicit constructor call
    ((Rect) ref rect).\u002Ector(0.0f, curY, ((Rect) ref viewRect).width - 48f, (float) (25.0 + 50.0 * (double) pawnList.Count));
    if (___draggedPawn != null && Mouse.IsOver(rect) && ((Thing) ___draggedPawn).Map != vehiclePawnWithMap.VehicleMap)
    {
      ___transferToHolder = (IThingHolder) vehiclePawnWithMap.VehicleMap;
      ___overDropSpot = true;
      Widgets.DrawHighlight(rect);
    }
    Widgets.ListSeparator(ref curY, ((Rect) ref viewRect).width, TaggedString.op_Implicit(TaggedString.op_Addition(((Entity) vehiclePawnWithMap).LabelCap, Translator.Translate("VMF_VehicleMap"))));
    foreach (Pawn pawn in pawnList)
    {
      if (VehicleTabHelper_Passenger.DoRow(curY, viewRect, scrollPos, pawn, ref moreDetailsForPawn, true))
        ___hoveringOverPawn = pawn;
      curY += 50f;
    }
  }
}
