// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.VMF_HarmonyPatches.Patch_JobGiver_Work_PawnCanUseWorkGiver
// Assembly: VehicleMapFramework, Version=1.6.562.0, Culture=neutral, PublicKeyToken=null
// MVID: 10A61882-945F-4CFC-9B06-CA8EEF5ADB36
// Assembly location: D:\Programas\steamapps\workshop\content\294100\3426502333\1.6\Assemblies\VehicleMapFramework.dll

using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Reflection;
using Verse;

#nullable disable
namespace VehicleMapFramework.VMF_HarmonyPatches;

[HarmonyPatch(typeof (JobGiver_Work), "PawnCanUseWorkGiver")]
[PatchLevel(Level.Sensitive)]
public static class Patch_JobGiver_Work_PawnCanUseWorkGiver
{
  public static readonly HashSet<Type> NoNeedVirtualMapTransferList = new HashSet<Type>();

  public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
  {
    MethodInfo from = AccessTools.Method(typeof (WorkGiver), "ShouldSkip", (Type[]) null, (Type[]) null);
    MethodInfo method = Patch_JobGiver_Work_PawnCanUseWorkGiver.ShouldSkipAll.Method;
    return (IEnumerable<CodeInstruction>) instructions.MethodReplacer(from, method);
  }

  public static bool ShouldSkipAll(this WorkGiver workGiver, Pawn pawn, bool forced = false)
  {
    if (Patch_JobGiver_Work_PawnCanUseWorkGiver.NoNeedVirtualMapTransferList.Contains(workGiver.GetType()))
      return workGiver.ShouldSkip(pawn, forced);
    foreach (Map mapAndVehicleMap in ((Thing) pawn).Map.BaseMapAndVehicleMaps(true))
    {
      using (new VirtualTeleporter((Thing) pawn, mapAndVehicleMap, setDepartMap: true))
      {
        if (!workGiver.ShouldSkip(pawn, forced))
          return false;
      }
    }
    return true;
  }
}
