// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.VMF_HarmonyPatches.Patch_FloatMenuContext_Constructor
// Assembly: VehicleMapFramework, Version=1.6.562.0, Culture=neutral, PublicKeyToken=null
// MVID: 10A61882-945F-4CFC-9B06-CA8EEF5ADB36
// Assembly location: D:\Programas\steamapps\workshop\content\294100\3426502333\1.6\Assemblies\VehicleMapFramework.dll

using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using Verse;

#nullable disable
namespace VehicleMapFramework.VMF_HarmonyPatches;

[HarmonyPatch]
public static class Patch_FloatMenuContext_Constructor
{
  [PatchLevel(Level.Safe)]
  public static void Prefix(List<Pawn> selectedPawns, ref Vector3 clickPosition, ref Map map)
  {
    VehiclePawnWithMap vehicle;
    if (selectedPawns.All<Pawn>((Func<Pawn, bool>) (p => p is VehiclePawnWithMap)) || !clickPosition.TryGetVehicleMap(Find.CurrentMap, out vehicle, VehicleMapFlag.None))
      return;
    Vector3 vector3 = clickPosition;
    IntVec3 positionHeld = ((Thing) vehicle).PositionHeld;
    Vector3 vector3Shifted = ((IntVec3) ref positionHeld).ToVector3Shifted();
    if (Vector3.op_Equality(vector3, vector3Shifted))
      return;
    GenUIOnVehicle.vehicleForSelector = vehicle;
    clickPosition = clickPosition.ToVehicleMapCoord(vehicle);
    map = vehicle.CurrentLevel;
  }

  [PatchLevel(Level.Safe)]
  public static void Finalizer(FloatMenuContext __instance)
  {
    Pawn firstSelectedPawn;
    if (!__instance.IsMultiselect && (firstSelectedPawn = __instance.FirstSelectedPawn) != null)
      TargetMapUtility.set_TargetInfo((Thing) firstSelectedPawn, new TargetInfo(__instance.ClickedCell, __instance.map, false));
    GenUIOnVehicle.vehicleForSelector = (VehiclePawnWithMap) null;
  }

  [PatchLevel(Level.Cautious)]
  public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
  {
    MethodInfo method1 = GenUI.ThingsUnderMouse.Method;
    // ISSUE: reference to a compiler-generated field
    // ISSUE: reference to a compiler-generated field
    MethodInfo method2 = (Patch_FloatMenuContext_Constructor.\u003C\u003EO.\u003C1\u003E__ThingsUnderMouse ?? (Patch_FloatMenuContext_Constructor.\u003C\u003EO.\u003C1\u003E__ThingsUnderMouse = new Func<Vector3, float, TargetingParameters, ITargetingSource, List<Thing>>(GenUIOnVehicle.ThingsUnderMouse))).Method;
    return (IEnumerable<CodeInstruction>) instructions.MethodReplacer(method1, method2);
  }
}
