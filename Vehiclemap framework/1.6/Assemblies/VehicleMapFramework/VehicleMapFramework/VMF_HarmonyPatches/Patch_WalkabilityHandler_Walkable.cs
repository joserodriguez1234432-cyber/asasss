// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.VMF_HarmonyPatches.Patch_WalkabilityHandler_Walkable
// Assembly: VehicleMapFramework, Version=1.6.562.0, Culture=neutral, PublicKeyToken=null
// MVID: 10A61882-945F-4CFC-9B06-CA8EEF5ADB36
// Assembly location: D:\Programas\steamapps\workshop\content\294100\3426502333\1.6\Assemblies\VehicleMapFramework.dll

using HarmonyLib;
using Verse;

#nullable disable
namespace VehicleMapFramework.VMF_HarmonyPatches;

[HarmonyPatchCategory("VMF_Patches_SmarterConstruction")]
[HarmonyPatch]
[PatchLevel(Level.Safe)]
public static class Patch_WalkabilityHandler_Walkable
{
  public static void Postfix(IntVec3 loc, Map ___map, ref bool __result)
  {
    if (__result)
      return;
    bool flag = GenGrid.InBounds(loc, ___map);
    if ((!flag || !(GridsUtility.GetEdifice(loc, ___map) is VehicleStructure)) && flag)
      return;
    __result = true;
  }
}
