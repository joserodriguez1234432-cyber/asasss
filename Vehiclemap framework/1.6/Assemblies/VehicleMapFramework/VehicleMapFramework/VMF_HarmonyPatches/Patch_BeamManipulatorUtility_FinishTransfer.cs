// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.VMF_HarmonyPatches.Patch_BeamManipulatorUtility_FinishTransfer
// Assembly: VehicleMapFramework, Version=1.6.562.0, Culture=neutral, PublicKeyToken=null
// MVID: 10A61882-945F-4CFC-9B06-CA8EEF5ADB36
// Assembly location: D:\Programas\steamapps\workshop\content\294100\3426502333\1.6\Assemblies\VehicleMapFramework.dll

using HarmonyLib;
using Verse;

#nullable disable
namespace VehicleMapFramework.VMF_HarmonyPatches;

[HarmonyPatchCategory("VMF_Patches_ManipulatorBeamEmitter")]
[HarmonyPatch]
[PatchLevel(Level.Safe)]
public static class Patch_BeamManipulatorUtility_FinishTransfer
{
  public static void Prefix(object op, Thing carriedThing, ref VirtualTeleporter? __state)
  {
    Thing thing = ModCompat.ManipulatorBeamEmitter.OperatorThing(op);
    if (thing == null)
      return;
    Map targetMap = TargetMapUtility.get_TargetMap(carriedThing);
    if (targetMap == null || thing.Map == targetMap)
      return;
    __state = new VirtualTeleporter?(new VirtualTeleporter(thing, targetMap));
  }

  public static void Finalizer(VirtualTeleporter? __state) => __state?.Dispose();
}
