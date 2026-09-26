// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.VMF_HarmonyPatches.Patch_CompLaserADS_ShootGroundTarget
// Assembly: VehicleMapFramework, Version=1.6.562.0, Culture=neutral, PublicKeyToken=null
// MVID: 10A61882-945F-4CFC-9B06-CA8EEF5ADB36
// Assembly location: D:\Programas\steamapps\workshop\content\294100\3426502333\1.6\Assemblies\VehicleMapFramework.dll

using HarmonyLib;
using Verse;

#nullable disable
namespace VehicleMapFramework.VMF_HarmonyPatches;

[HarmonyPatchCategory("VMF_Patches_SRALib")]
[HarmonyPatch]
[PatchLevel(Level.Safe)]
public static class Patch_CompLaserADS_ShootGroundTarget
{
  public static void Prefix(ThingWithComps ___parent, Thing target, ref VirtualTeleporter? __state)
  {
    Map map = target.Map;
    if (map == null || map == ((Thing) ___parent).Map)
      return;
    __state = new VirtualTeleporter?(new VirtualTeleporter((Thing) ___parent, map));
  }

  public static void Finalizer(VirtualTeleporter? __state) => __state?.Dispose();
}
