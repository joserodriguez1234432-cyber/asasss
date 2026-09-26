// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.VMF_HarmonyPatches.Patch_BeamManipulatorUtility_Cooldown
// Assembly: VehicleMapFramework, Version=1.6.562.0, Culture=neutral, PublicKeyToken=null
// MVID: 10A61882-945F-4CFC-9B06-CA8EEF5ADB36
// Assembly location: D:\Programas\steamapps\workshop\content\294100\3426502333\1.6\Assemblies\VehicleMapFramework.dll

using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Reflection;
using Verse;

#nullable disable
namespace VehicleMapFramework.VMF_HarmonyPatches;

[HarmonyPatchCategory("VMF_Patches_ManipulatorBeamEmitter")]
[HarmonyPatch]
[PatchLevel(Level.Safe)]
public static class Patch_BeamManipulatorUtility_Cooldown
{
  private static IEnumerable<MethodBase> TargetMethods()
  {
    Type type = GenTypes.GetTypeInAnyAssembly("ManipulatorBeam.BeamManipulatorUtility", "ManipulatorBeam");
    yield return (MethodBase) AccessTools.Method(type, "IsStorageRetryCoolingDown", (Type[]) null, (Type[]) null);
    yield return (MethodBase) AccessTools.Method(type, "IsSourceUnavailableCoolingDown", (Type[]) null, (Type[]) null);
    yield return (MethodBase) AccessTools.Method(type, "MarkStorageRetryCooldown", (Type[]) null, (Type[]) null);
    yield return (MethodBase) AccessTools.Method(type, "ClearStorageRetryCooldown", (Type[]) null, (Type[]) null);
    yield return (MethodBase) AccessTools.Method(type, "MarkSourceUnavailableCooldown", (Type[]) null, (Type[]) null);
  }

  public static void Prefix(ref Map map, Thing thing) => map = thing.MapHeld ?? map;
}
