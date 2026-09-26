// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.VMF_HarmonyPatches.Patch_JobGiver_Work_Validator
// Assembly: VehicleMapFramework, Version=1.6.562.0, Culture=neutral, PublicKeyToken=null
// MVID: 10A61882-945F-4CFC-9B06-CA8EEF5ADB36
// Assembly location: D:\Programas\steamapps\workshop\content\294100\3426502333\1.6\Assemblies\VehicleMapFramework.dll

using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Verse;

#nullable disable
namespace VehicleMapFramework.VMF_HarmonyPatches;

[HarmonyPatch]
[PatchLevel(Level.Sensitive)]
public static class Patch_JobGiver_Work_Validator
{
  private static MethodInfo TargetMethod()
  {
    return AccessTools.InnerTypes(typeof (JobGiver_Work)).SelectMany<Type, MethodInfo>((Func<Type, IEnumerable<MethodInfo>>) (t => (IEnumerable<MethodInfo>) AccessToolsExtensions.GetDeclaredMethods(t))).First<MethodInfo>((Func<MethodInfo, bool>) (m => m.Name.Contains("Validator")));
  }

  public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
  {
    MethodInfo from = AccessTools.Method(typeof (WorkGiver_Scanner), "HasJobOnThing", (Type[]) null, (Type[]) null);
    MethodInfo method = Patch_JobGiver_Work_Validator.HasJobOnThingMap.Method;
    return (IEnumerable<CodeInstruction>) instructions.MethodReplacer(from, method);
  }

  public static bool HasJobOnThingMap(
    this WorkGiver_Scanner scanner,
    Pawn pawn,
    Thing t,
    bool forced = false)
  {
    Map mapHeld = t.MapHeld;
    if (JobAcrossMapsUtility.NoNeedVirtualMapTransfer(((Thing) pawn).Map, mapHeld, ((WorkGiver) scanner).def))
      return scanner.HasJobOnThing(pawn, t, forced);
    using (new VirtualTeleporter((Thing) pawn, mapHeld, setDepartMap: true))
      return scanner.HasJobOnThing(pawn, t, forced);
  }
}
