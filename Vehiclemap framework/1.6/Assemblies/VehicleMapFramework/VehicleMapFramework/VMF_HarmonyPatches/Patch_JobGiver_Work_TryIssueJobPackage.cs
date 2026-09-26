// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.VMF_HarmonyPatches.Patch_JobGiver_Work_TryIssueJobPackage
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
using System.Runtime.CompilerServices;
using UnityEngine;
using Verse;
using Verse.AI;

#nullable disable
namespace VehicleMapFramework.VMF_HarmonyPatches;

[HarmonyAfter(new string[] {"SmarterConstruction"})]
[HarmonyPatch(typeof (JobGiver_Work), "TryIssueJobPackage")]
[PatchLevel(Level.Sensitive)]
public static class Patch_JobGiver_Work_TryIssueJobPackage
{
  private static readonly List<Thing> tmpThings = new List<Thing>();

  public static IEnumerable<CodeInstruction> Transpiler(
    IEnumerable<CodeInstruction> instructions,
    ILGenerator generator,
    MethodBase original)
  {
    CodeMatcher codeMatcher = new CodeMatcher(instructions, generator);
    codeMatcher.MatchStartForward(new CodeMatch[1]
    {
      new CodeMatch((Func<CodeInstruction, bool>) (c => c.opcode == OpCodes.Isinst && c.operand.Equals((object) typeof (WorkGiver_Scanner))), (string) null)
    });
    LocalBuilder localBuilder;
    codeMatcher.DeclareLocal(typeof (WorkGiver_Scanner), ref localBuilder);
    codeMatcher.InsertAfterAndAdvance(new CodeInstruction[2]
    {
      new CodeInstruction(OpCodes.Dup, (object) null),
      new CodeInstruction(OpCodes.Stloc_S, (object) localBuilder)
    });
    object local = (object) null;
    CodeMatch codeMatch = new CodeMatch((Func<CodeInstruction, bool>) (c => c.opcode == OpCodes.Stloc_S && ((LocalVariableInfo) c.operand).LocalType == typeof (IEnumerable<Thing>) && c.operand != local), (string) null);
    // ISSUE: reference to a compiler-generated field
    // ISSUE: reference to a compiler-generated field
    CodeInstruction[] codeInstructionArray = new CodeInstruction[3]
    {
      CodeInstruction.LoadArgument(1, false),
      new CodeInstruction(OpCodes.Ldloc_S, (object) localBuilder),
      PatchHelper.get_CallInstruction((Patch_JobGiver_Work_TryIssueJobPackage.\u003C\u003EO.\u003C0\u003E__AddSearchSet ?? (Patch_JobGiver_Work_TryIssueJobPackage.\u003C\u003EO.\u003C0\u003E__AddSearchSet = new Func<List<Thing>, Pawn, WorkGiver_Scanner, IEnumerable<Thing>>(Patch_JobGiver_Work_TryIssueJobPackage.AddSearchSet))).Method)
    };
    codeMatcher.MatchStartForward(new CodeMatch[1]
    {
      codeMatch
    });
    local = codeMatcher.Operand;
    codeMatcher.MatchStartForward(new CodeMatch[1]
    {
      codeMatch
    });
    local = codeMatcher.Operand;
    codeMatcher.InsertAndAdvance(codeInstructionArray);
    codeMatcher.MatchStartForward(new CodeMatch[1]
    {
      codeMatch
    });
    codeMatcher.InsertAndAdvance(codeInstructionArray);
    codeMatcher.MatchStartForward(new CodeMatch[1]
    {
      new CodeMatch((Func<CodeInstruction, bool>) (c => c.opcode == OpCodes.Stloc_S && ((LocalVariableInfo) c.operand).LocalType == typeof (IEnumerable<IntVec3>)), (string) null)
    });
    IList<LocalVariableInfo> localVariables = original.GetMethodBody()?.LocalVariables;
    int num1 = GenCollection.FirstIndexOf<LocalVariableInfo>((IEnumerable<LocalVariableInfo>) localVariables, (Func<LocalVariableInfo, bool>) (l =>
    {
      Type localType = l.LocalType;
      return ((object) localType != null ? localType.GetCustomAttribute<CompilerGeneratedAttribute>() : (CompilerGeneratedAttribute) null) != null;
    }));
    int num2 = GenCollection.FirstIndexOf<LocalVariableInfo>((IEnumerable<LocalVariableInfo>) localVariables, (Func<LocalVariableInfo, bool>) (l =>
    {
      Type localType = l.LocalType;
      return ((object) localType != null ? localType.GetCustomAttribute<CompilerGeneratedAttribute>() : (CompilerGeneratedAttribute) null) != null && AccessToolsExtensions.IsStruct(l.LocalType);
    }));
    // ISSUE: reference to a compiler-generated field
    // ISSUE: reference to a compiler-generated field
    codeMatcher.InsertAfterAndAdvance(new CodeInstruction[4]
    {
      new CodeInstruction(OpCodes.Ldloc_S, (object) localBuilder),
      CodeInstruction.LoadLocal(num1, true),
      CodeInstruction.LoadLocal(num2, true),
      PatchHelper.get_CallInstruction((Patch_JobGiver_Work_TryIssueJobPackage.\u003C\u003EO.\u003C1\u003E__ScanCellsAcrossMaps ?? (Patch_JobGiver_Work_TryIssueJobPackage.\u003C\u003EO.\u003C1\u003E__ScanCellsAcrossMaps = new \u003C\u003EA\u007B00000048\u007D<WorkGiver_Scanner, Patch_JobGiver_Work_TryIssueJobPackage.InnerClass, Patch_JobGiver_Work_TryIssueJobPackage.InnerStruct>(Patch_JobGiver_Work_TryIssueJobPackage.ScanCellsAcrossMaps))).Method)
    });
    MethodInfo methodInfo1 = AccessTools.PropertyGetter(typeof (TargetInfo), "Cell");
    codeMatcher.MatchStartForward(new CodeMatch[1]
    {
      CodeMatch.Calls(methodInfo1)
    });
    codeMatcher.RemoveInstruction();
    MethodInfo methodInfo2 = AccessTools.Method(typeof (WorkGiver_Scanner), "JobOnCell", (Type[]) null, (Type[]) null);
    codeMatcher.MatchStartForward(new CodeMatch[1]
    {
      CodeMatch.Calls(methodInfo2)
    });
    codeMatcher.SetInstruction(PatchHelper.get_CallInstruction(Patch_JobGiver_Work_TryIssueJobPackage.JobOnCellMap.Method));
    MethodInfo method1 = GenClosest.ClosestThing_Global.Method;
    MethodInfo method2 = GenClosestCrossMap.ClosestThing_Global.Method;
    MethodInfo method3 = GenClosest.ClosestThing_Global_Reachable.Method;
    MethodInfo method4 = GenClosestCrossMap.ClosestThing_Global_Reachable.Method;
    MethodInfo methodInfo3 = AccessTools.Method(typeof (WorkGiver), "NonScanJob", (Type[]) null, (Type[]) null);
    // ISSUE: reference to a compiler-generated field
    // ISSUE: reference to a compiler-generated field
    MethodInfo method5 = (Patch_JobGiver_Work_TryIssueJobPackage.\u003C\u003EO.\u003C7\u003E__NonScanJobAll ?? (Patch_JobGiver_Work_TryIssueJobPackage.\u003C\u003EO.\u003C7\u003E__NonScanJobAll = new Func<WorkGiver, Pawn, Job>(Patch_JobGiver_Work_TryIssueJobPackage.NonScanJobAll))).Method;
    MethodInfo methodInfo4 = AccessTools.Method(typeof (WorkGiver_Scanner), "PotentialWorkThingsGlobal", (Type[]) null, (Type[]) null);
    // ISSUE: reference to a compiler-generated field
    // ISSUE: reference to a compiler-generated field
    MethodInfo method6 = (Patch_JobGiver_Work_TryIssueJobPackage.\u003C\u003EO.\u003C8\u003E__PotentialWorkThingsGlobalAll ?? (Patch_JobGiver_Work_TryIssueJobPackage.\u003C\u003EO.\u003C8\u003E__PotentialWorkThingsGlobalAll = new Func<WorkGiver_Scanner, Pawn, IEnumerable<Thing>>(Patch_JobGiver_Work_TryIssueJobPackage.PotentialWorkThingsGlobalAll))).Method;
    MethodInfo methodInfo5 = AccessTools.Method(typeof (WorkGiver_Scanner), "JobOnThing", (Type[]) null, (Type[]) null);
    MethodInfo method7 = Patch_JobGiver_Work_TryIssueJobPackage.JobOnThingMap.Method;
    return (IEnumerable<CodeInstruction>) codeMatcher.InstructionEnumeration().MethodReplacer(((MethodBase) method1, (MethodBase) method2), ((MethodBase) method3, (MethodBase) method4), ((MethodBase) methodInfo3, (MethodBase) method5), ((MethodBase) methodInfo4, (MethodBase) method6), ((MethodBase) methodInfo5, (MethodBase) method7));
  }

  internal static IEnumerable<Thing> AddSearchSet(
    List<Thing> list,
    Pawn pawn,
    WorkGiver_Scanner scanner)
  {
    List<Thing>.Enumerator enumerator;
    if (list != null)
    {
      enumerator = list.GetEnumerator();
      while (enumerator.MoveNext())
        yield return enumerator.Current;
      enumerator = new List<Thing>.Enumerator();
    }
    if (!JobAcrossMapsUtility.NoNeedVirtualMapTransfer(((Thing) pawn).Map, (Map) null, ((WorkGiver) scanner).def))
    {
      ThingRequest req = scanner.PotentialWorkThingRequest;
      foreach (Map mapAndVehicleMap in ((Thing) pawn).Map.BaseMapAndVehicleMaps(false))
      {
        enumerator = mapAndVehicleMap.listerThings.ThingsMatching(req).GetEnumerator();
        while (enumerator.MoveNext())
          yield return enumerator.Current;
        enumerator = new List<Thing>.Enumerator();
      }
    }
  }

  internal static Job NonScanJobAll(this WorkGiver workGiver, Pawn pawn)
  {
    // ISSUE: object of a compiler-generated type is created
    // ISSUE: variable of a compiler-generated type
    Patch_JobGiver_Work_TryIssueJobPackage.\u003C\u003Ec__DisplayClass3_0 cDisplayClass30 = new Patch_JobGiver_Work_TryIssueJobPackage.\u003C\u003Ec__DisplayClass3_0();
    Job nextJob = workGiver.NonScanJob(pawn);
    if (nextJob != null || !JobAcrossMapsUtility.WorkGiverClassesNonScanAll.Contains(workGiver.def.giverClass))
      return nextJob;
    Map map = ((Thing) pawn).Map;
    Region region = GridsUtility.GetRegion(((Thing) pawn).Position, map, (RegionType) 14);
    if (region == null)
      return (Job) null;
    // ISSUE: reference to a compiler-generated field
    cDisplayClass30.traverseParms = TraverseParms.For(pawn, (Danger) 3, (TraverseMode) 0, false, false, false, true);
    foreach (Map mapAndVehicleMap in ((Thing) pawn).Map.BaseMapAndVehicleMaps(false))
    {
      // ISSUE: object of a compiler-generated type is created
      // ISSUE: variable of a compiler-generated type
      Patch_JobGiver_Work_TryIssueJobPackage.\u003C\u003Ec__DisplayClass3_1 cDisplayClass31 = new Patch_JobGiver_Work_TryIssueJobPackage.\u003C\u003Ec__DisplayClass3_1();
      // ISSUE: reference to a compiler-generated field
      cDisplayClass31.map2 = mapAndVehicleMap;
      // ISSUE: reference to a compiler-generated field
      cDisplayClass31.region2 = (Region) null;
      // ISSUE: reference to a compiler-generated field
      // ISSUE: reference to a compiler-generated field
      // ISSUE: method pointer
      // ISSUE: method pointer
      RegionTraverserAcrossMaps.BreadthFirstTraverse(region, cDisplayClass30.\u003C\u003E9__0 ?? (cDisplayClass30.\u003C\u003E9__0 = new RegionEntryPredicate((object) cDisplayClass30, __methodptr(\u003CNonScanJobAll\u003Eb__0))), new RegionProcessor((object) cDisplayClass31, __methodptr(\u003CNonScanJobAll\u003Eb__1)));
      // ISSUE: reference to a compiler-generated field
      // ISSUE: reference to a compiler-generated field
      if (cDisplayClass31.region2 != null && cDisplayClass31.region2.valid)
      {
        // ISSUE: reference to a compiler-generated field
        // ISSUE: reference to a compiler-generated field
        using (new VirtualTeleporter((Thing) pawn, cDisplayClass31.map2, new IntVec3?(cDisplayClass31.region2.RandomCell)))
          nextJob = workGiver.NonScanJob(pawn);
        TargetInfo exitSpot;
        TargetInfo enterSpot;
        List<TraverseSpots> spotsQueue;
        // ISSUE: reference to a compiler-generated field
        if (nextJob != null && pawn.CanReach(nextJob.targetA, (PathEndMode) 2, (Danger) 3, false, false, (TraverseMode) 0, cDisplayClass31.map2, out exitSpot, out enterSpot, out spotsQueue))
          return JobAcrossMapsUtility.GotoDestMapJob(pawn, new TargetInfo?(exitSpot), new TargetInfo?(enterSpot), spotsQueue, nextJob);
      }
    }
    return (Job) null;
  }

  internal static IEnumerable<Thing> PotentialWorkThingsGlobalAll(
    this WorkGiver_Scanner scanner,
    Pawn pawn)
  {
    if (JobAcrossMapsUtility.NoNeedVirtualMapTransfer(((Thing) pawn).Map, (Map) null, ((WorkGiver) scanner).def))
      return scanner.PotentialWorkThingsGlobal(pawn);
    Map map = ((Thing) pawn).Map;
    CrossMapReachabilityUtility.set_DepartMap(pawn, map);
    IntVec3 position = ((Thing) pawn).Position;
    try
    {
      Patch_JobGiver_Work_TryIssueJobPackage.tmpThings.Clear();
      bool flag = true;
      foreach (Map mapAndVehicleMap in ((Thing) pawn).Map.BaseMapAndVehicleMaps(true))
      {
        ((Thing) pawn).VirtualMapTransfer(mapAndVehicleMap);
        IEnumerable<Thing> source = scanner.PotentialWorkThingsGlobal(pawn);
        IEnumerable<Thing> collection = source != null ? source.Where<Thing>((Func<Thing, bool>) (t => t != null)) : (IEnumerable<Thing>) null;
        if (collection != null)
        {
          Patch_JobGiver_Work_TryIssueJobPackage.tmpThings.AddRange(collection);
          flag = false;
        }
      }
      return flag ? (IEnumerable<Thing>) null : Patch_JobGiver_Work_TryIssueJobPackage.tmpThings.Distinct<Thing>();
    }
    finally
    {
      ((Thing) pawn).VirtualMapTransfer(map, position);
      pawn.RemoveDepartMap();
    }
  }

  internal static Job JobOnThingMap(
    this WorkGiver_Scanner scanner,
    Pawn pawn,
    Thing t,
    bool forced = false)
  {
    Map mapHeld = t.MapHeld;
    if (JobAcrossMapsUtility.NoNeedVirtualMapTransfer(((Thing) pawn).Map, mapHeld, ((WorkGiver) scanner).def))
      return scanner.JobOnThing(pawn, t, forced);
    using (new VirtualTeleporter((Thing) pawn, mapHeld, setDepartMap: true))
    {
      Job job = scanner.JobOnThing(pawn, t, forced);
      TargetInfo exitSpot;
      TargetInfo enterSpot;
      List<TraverseSpots> spotsQueue;
      if (job != null && JobAcrossMapsUtility.NeedWrapGotoDestMapJob(scanner, job) && pawn.CanReach(LocalTargetInfo.op_Implicit(t), scanner.PathEndMode, scanner.MaxPathDanger(pawn), false, false, (TraverseMode) 0, t.MapHeld, out exitSpot, out enterSpot, out spotsQueue))
        job = JobAcrossMapsUtility.GotoDestMapJob(pawn, new TargetInfo?(exitSpot), new TargetInfo?(enterSpot), spotsQueue, job);
      return job;
    }
  }

  internal static Job JobOnCellMap(
    this WorkGiver_Scanner scanner,
    Pawn pawn,
    in TargetInfo target,
    bool forced = false)
  {
    Map map1 = ((Thing) pawn).Map;
    TargetInfo targetInfo1 = target;
    Map map2 = ((TargetInfo) ref targetInfo1).Map;
    Map map3 = map2;
    if (map1 == map3)
    {
      WorkGiver_Scanner workGiverScanner = scanner;
      Pawn pawn1 = pawn;
      TargetInfo targetInfo2 = target;
      IntVec3 cell = ((TargetInfo) ref targetInfo2).Cell;
      int num = forced ? 1 : 0;
      return workGiverScanner.JobOnCell(pawn1, cell, num != 0);
    }
    TargetMapUtility.set_TargetInfo((Thing) pawn, target);
    try
    {
      TargetInfo targetInfo3 = target;
      Building edifice = GridsUtility.GetEdifice(((TargetInfo) ref targetInfo3).Cell, map2);
      LocalTargetInfo localTargetInfo;
      if (edifice == null)
      {
        TargetInfo targetInfo4 = target;
        localTargetInfo = LocalTargetInfo.op_Implicit(((TargetInfo) ref targetInfo4).Cell);
      }
      else
        localTargetInfo = LocalTargetInfo.op_Implicit((Thing) edifice);
      LocalTargetInfo dest3 = localTargetInfo;
      TargetInfo exitSpot;
      TargetInfo enterSpot;
      List<TraverseSpots> spotsQueue;
      if (!pawn.CanReach(dest3, scanner.PathEndMode, scanner.MaxPathDanger(pawn), false, false, (TraverseMode) 0, map2, out exitSpot, out enterSpot, out spotsQueue))
        return (Job) null;
      TargetInfo targetInfo5 = target;
      IntVec3 intVec3 = CellFinder.StandableCellNear(((TargetInfo) ref targetInfo5).Cell, map2, 1.5f, (Predicate<IntVec3>) null);
      if (!((IntVec3) ref intVec3).IsValid)
      {
        TargetInfo targetInfo6 = target;
        intVec3 = ((TargetInfo) ref targetInfo6).Cell;
      }
      using (new VirtualTeleporter((Thing) pawn, map2, new IntVec3?(intVec3)))
      {
        WorkGiver_Scanner workGiverScanner = scanner;
        Pawn pawn2 = pawn;
        TargetInfo targetInfo7 = target;
        IntVec3 cell = ((TargetInfo) ref targetInfo7).Cell;
        int num = forced ? 1 : 0;
        Job nextJob = workGiverScanner.JobOnCell(pawn2, cell, num != 0);
        if (!JobAcrossMapsUtility.NoNeedWrapGotoDestMapJob(scanner))
          nextJob = JobAcrossMapsUtility.GotoDestMapJob(pawn, new TargetInfo?(exitSpot), new TargetInfo?(enterSpot), spotsQueue, nextJob);
        return nextJob;
      }
    }
    finally
    {
      ((Thing) pawn).RemoveTargetInfo();
    }
  }

  internal static void ScanCellsAcrossMaps(
    this WorkGiver_Scanner scanner,
    ref Patch_JobGiver_Work_TryIssueJobPackage.InnerClass innerClass,
    ref Patch_JobGiver_Work_TryIssueJobPackage.InnerStruct innerStruct)
  {
    Pawn pawn = innerClass.pawn;
    IntVec3 positionOnBaseMap = VehicleMapUtility.get_PositionOnBaseMap((Thing) pawn);
    Map map1;
    CrossMapReachabilityUtility.set_DepartMap(pawn, map1 = ((Thing) pawn).Map);
    Map map2 = map1;
    try
    {
      foreach (Map mapAndVehicleMap in ((Thing) pawn).Map.BaseMapAndVehicleMaps(false))
      {
        ((Thing) pawn).VirtualMapTransfer(mapAndVehicleMap);
        VehiclePawnWithMap vehicle;
        IntVec3 intVec3_1 = mapAndVehicleMap.IsVehicleMapOf(out vehicle) ? positionOnBaseMap.ToVehicleMapCoord(vehicle) : positionOnBaseMap;
        foreach (IntVec3 intVec3_2 in scanner.PotentialWorkCellsGlobal(pawn))
        {
          bool flag = false;
          IntVec3 intVec3_3 = IntVec3.op_Subtraction(intVec3_2, intVec3_1);
          float horizontalSquared = (float) ((IntVec3) ref intVec3_3).LengthHorizontalSquared;
          float num = 0.0f;
          if (innerStruct.prioritized)
          {
            if (!ForbidUtility.IsForbidden(intVec3_2, pawn) && scanner.HasJobOnCell(pawn, intVec3_2, false))
            {
              num = scanner.GetPriority(pawn, intVec3_2);
              if ((double) num > (double) innerStruct.bestPriority || Mathf.Approximately(num, innerStruct.bestPriority) && (double) horizontalSquared < (double) innerStruct.closestDistSquared)
                flag = true;
            }
          }
          else if ((double) horizontalSquared < (double) innerStruct.closestDistSquared && !ForbidUtility.IsForbidden(intVec3_2, pawn) && scanner.HasJobOnCell(pawn, intVec3_2, false))
            flag = true;
          if (flag)
          {
            innerClass.bestTargetOfLastPriority = new TargetInfo(intVec3_2, mapAndVehicleMap, false);
            innerClass.scannerWhoProvidedTarget = scanner;
            innerStruct.closestDistSquared = horizontalSquared;
            innerStruct.bestPriority = num;
          }
        }
      }
    }
    finally
    {
      ((Thing) pawn).VirtualMapTransfer(map2);
      pawn.RemoveDepartMap();
    }
  }

  public struct InnerStruct
  {
    public IntVec3 pawnPosition;
    public bool prioritized;
    public bool allowUnreachable;
    public Danger maxPathDanger;
    public float bestPriority;
    public float closestDistSquared;
  }

  public class InnerClass
  {
    public Pawn pawn;
    public TargetInfo bestTargetOfLastPriority;
    public WorkGiver_Scanner scannerWhoProvidedTarget;
  }

  [SpecialName]
  public sealed class \u003CG\u003E\u00245E54C0D029DAD96524148E97E8282E51
  {
    [ExtensionMarker("<M>$F7FEADF431A0BCB54325C9EA3C6B3A2D")]
    internal IEnumerable<Thing> PotentialWorkThingsGlobalAll(Pawn pawn)
    {
      throw new NotSupportedException();
    }

    [ExtensionMarker("<M>$F7FEADF431A0BCB54325C9EA3C6B3A2D")]
    internal Job JobOnThingMap(Pawn pawn, Thing t, bool forced = false)
    {
      throw new NotSupportedException();
    }

    [ExtensionMarker("<M>$F7FEADF431A0BCB54325C9EA3C6B3A2D")]
    internal Job JobOnCellMap(Pawn pawn, in TargetInfo target, bool forced = false)
    {
      throw new NotSupportedException();
    }

    [ExtensionMarker("<M>$F7FEADF431A0BCB54325C9EA3C6B3A2D")]
    internal void ScanCellsAcrossMaps(
      ref Patch_JobGiver_Work_TryIssueJobPackage.InnerClass innerClass,
      ref Patch_JobGiver_Work_TryIssueJobPackage.InnerStruct innerStruct)
    {
      throw new NotSupportedException();
    }

    [SpecialName]
    public static class \u003CM\u003E\u0024F7FEADF431A0BCB54325C9EA3C6B3A2D
    {
      [CompilerGenerated]
      [SpecialName]
      internal static void \u003CExtension\u003E\u0024(WorkGiver_Scanner scanner)
      {
      }
    }
  }
}
