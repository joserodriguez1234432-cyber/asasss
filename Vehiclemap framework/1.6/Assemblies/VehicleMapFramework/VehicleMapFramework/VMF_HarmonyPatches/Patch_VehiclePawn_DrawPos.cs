// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.VMF_HarmonyPatches.Patch_VehiclePawn_DrawPos
// Assembly: VehicleMapFramework, Version=1.6.562.0, Culture=neutral, PublicKeyToken=null
// MVID: 10A61882-945F-4CFC-9B06-CA8EEF5ADB36
// Assembly location: D:\Programas\steamapps\workshop\content\294100\3426502333\1.6\Assemblies\VehicleMapFramework.dll

using HarmonyLib;
using UnityEngine;
using Vehicles;
using Verse;
using Verse.AI;

#nullable disable
namespace VehicleMapFramework.VMF_HarmonyPatches;

[HarmonyPatch]
[PatchLevel(Level.Safe)]
public static class Patch_VehiclePawn_DrawPos
{
  public static bool Prefix(VehiclePawn ___vehicle, ref Vector3 __result, out bool __state)
  {
    __state = !((Thing) ___vehicle).TryGetDrawPos(ref __result);
    return __state;
  }

  public static void Postfix(VehiclePawn ___vehicle, ref Vector3 __result, bool __state)
  {
    if (!__state)
      return;
    __result = Vector3.op_Addition(__result, ((Pawn) ___vehicle).jobs?.curDriver is JobDriverBodyOffset curDriver ? ((JobDriver) curDriver).ForcedBodyOffset : Vector3.zero);
  }
}
