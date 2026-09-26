// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.VMF_HarmonyPatches.Patch_PaintUtility_FindNearbyDyes
// Assembly: VehicleMapFramework, Version=1.6.562.0, Culture=neutral, PublicKeyToken=null
// MVID: 10A61882-945F-4CFC-9B06-CA8EEF5ADB36
// Assembly location: D:\Programas\steamapps\workshop\content\294100\3426502333\1.6\Assemblies\VehicleMapFramework.dll

using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using Verse;
using Verse.AI;

#nullable disable
namespace VehicleMapFramework.VMF_HarmonyPatches;

[HarmonyPatch]
[PatchLevel(Level.Sensitive)]
public static class Patch_PaintUtility_FindNearbyDyes
{
  private static readonly List<Thing> tmpList = new List<Thing>();

  private static IEnumerable<MethodBase> TargetMethods()
  {
    yield return (MethodBase) AccessTools.Method(typeof (PaintUtility), "FindNearbyDyes", (Type[]) null, (Type[]) null);
    yield return (MethodBase) AccessTools.Method(typeof (WorkGiver_PaintBuilding), "ShouldPaintThing", (Type[]) null, (Type[]) null);
    yield return (MethodBase) AccessTools.Method(typeof (WorkGiver_PaintFloor), "ShouldPaintCell", (Type[]) null, (Type[]) null);
  }

  public static IEnumerable<CodeInstruction> Transpiler(
    IEnumerable<CodeInstruction> instructions,
    MethodBase original)
  {
    ParameterInfo[] parameters = original.GetParameters();
    int num1 = !original.IsStatic ? 1 : 0;
    int num2 = GenCollection.FirstIndexOf<ParameterInfo>((IEnumerable<ParameterInfo>) parameters, (Func<ParameterInfo, bool>) (p => p.ParameterType == typeof (Pawn))) + num1;
    int num3 = GenCollection.FirstIndexOf<ParameterInfo>((IEnumerable<ParameterInfo>) parameters, (Func<ParameterInfo, bool>) (p => p.ParameterType == typeof (bool))) + num1;
    // ISSUE: reference to a compiler-generated field
    // ISSUE: reference to a compiler-generated field
    return new CodeMatcher(instructions, (ILGenerator) null).MatchStartForward(new CodeMatch[1]
    {
      CodeMatch.Calls(AccessTools.Method(typeof (ListerThings), "ThingsOfDef", (Type[]) null, (Type[]) null))
    }).InsertAfter(new CodeInstruction[3]
    {
      CodeInstruction.LoadArgument(num2, false),
      CodeInstruction.LoadArgument(num3, false),
      PatchHelper.get_CallInstruction((Patch_PaintUtility_FindNearbyDyes.\u003C\u003EO.\u003C0\u003E__AddThingList ?? (Patch_PaintUtility_FindNearbyDyes.\u003C\u003EO.\u003C0\u003E__AddThingList = new Func<List<Thing>, Pawn, bool, List<Thing>>(Patch_PaintUtility_FindNearbyDyes.AddThingList))).Method)
    }).InstructionEnumeration();
  }

  private static List<Thing> AddThingList(List<Thing> list, Pawn pawn, bool forced)
  {
    Map map = ((Thing) pawn).Map;
    Patch_PaintUtility_FindNearbyDyes.tmpList.Clear();
    Patch_PaintUtility_FindNearbyDyes.tmpList.AddRange((IEnumerable<Thing>) list);
    Patch_PaintUtility_FindNearbyDyes.tmpList.AddRange(map.BaseMapAndVehicleMaps(false).SelectMany<Map, Thing>((Func<Map, IEnumerable<Thing>>) (m => (IEnumerable<Thing>) m.listerThings.ThingsOfDef(ThingDefOf.Dye))).Where<Thing>((Func<Thing, bool>) (t => !ForbidUtility.IsForbidden(t, pawn) && ReservationUtility.CanReserveAndReach(pawn, LocalTargetInfo.op_Implicit(t), (PathEndMode) 3, (Danger) 3, 1, -1, (ReservationLayerDef) null, forced))));
    return Patch_PaintUtility_FindNearbyDyes.tmpList;
  }
}
