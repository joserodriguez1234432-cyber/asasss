// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.VMF_HarmonyPatches.Patch_HaulAIUtility_PawnCanAutomaticallyHaul
// Assembly: VehicleMapFramework, Version=1.6.562.0, Culture=neutral, PublicKeyToken=null
// MVID: 10A61882-945F-4CFC-9B06-CA8EEF5ADB36
// Assembly location: D:\Programas\steamapps\workshop\content\294100\3426502333\1.6\Assemblies\VehicleMapFramework.dll

using HarmonyLib;
using Verse;
using Verse.AI;

#nullable disable
namespace VehicleMapFramework.VMF_HarmonyPatches;

[HarmonyPatch(typeof (HaulAIUtility), "PawnCanAutomaticallyHaul")]
[PatchLevel(Level.Sensitive)]
public static class Patch_HaulAIUtility_PawnCanAutomaticallyHaul
{
  public static void Prefix(Pawn p, Thing t, ref VirtualTeleporter? __state)
  {
    if (((Thing) p).Map == t.Map)
      return;
    __state = new VirtualTeleporter?(new VirtualTeleporter((Thing) p, t.Map));
  }

  public static void Finalizer(VirtualTeleporter? __state) => __state?.Dispose();
}
