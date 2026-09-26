// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.VMF_HarmonyPatches.Patch_Job_Clone
// Assembly: VehicleMapFramework, Version=1.6.562.0, Culture=neutral, PublicKeyToken=null
// MVID: 10A61882-945F-4CFC-9B06-CA8EEF5ADB36
// Assembly location: D:\Programas\steamapps\workshop\content\294100\3426502333\1.6\Assemblies\VehicleMapFramework.dll

using HarmonyLib;
using Verse.AI;

#nullable disable
namespace VehicleMapFramework.VMF_HarmonyPatches;

[HarmonyPatchCategory("VMF_Patches_CallTradeShips")]
[HarmonyPatch(typeof (Job), "Clone")]
[PatchLevel(Level.Safe)]
public static class Patch_Job_Clone
{
  public static bool Prefix(Job __instance, ref Job __result)
  {
    if (!(__instance.GetType() == ModCompat.CallTradeShips.Job_CallTradeShip))
      return true;
    Job instance = (Job) AccessToolsExtensions.CreateInstance(ModCompat.CallTradeShips.Job_CallTradeShip);
    __result = __instance.Clone();
    ModCompat.CallTradeShips.TraderKindDef.Invoke(__result) = ModCompat.CallTradeShips.TraderKindDef.Invoke(__instance);
    ModCompat.CallTradeShips.TraderKind.Invoke(__result) = ModCompat.CallTradeShips.TraderKind.Invoke(__instance);
    return false;
  }
}
