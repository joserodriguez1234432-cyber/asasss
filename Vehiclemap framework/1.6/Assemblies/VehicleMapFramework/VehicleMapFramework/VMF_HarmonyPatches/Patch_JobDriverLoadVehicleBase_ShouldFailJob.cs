// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.VMF_HarmonyPatches.Patch_JobDriverLoadVehicleBase_ShouldFailJob
// Assembly: VehicleMapFramework, Version=1.6.562.0, Culture=neutral, PublicKeyToken=null
// MVID: 10A61882-945F-4CFC-9B06-CA8EEF5ADB36
// Assembly location: D:\Programas\steamapps\workshop\content\294100\3426502333\1.6\Assemblies\VehicleMapFramework.dll

using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using Vehicles;
using Verse;
using Verse.AI;

#nullable disable
namespace VehicleMapFramework.VMF_HarmonyPatches;

[HarmonyPatchCategory("VMF_Patches_VehicleFramework")]
[HarmonyPatch]
[PatchLevel(Level.Safe)]
public static class Patch_JobDriverLoadVehicleBase_ShouldFailJob
{
  private static readonly Dictionary<Type, Predicate<JobDriverLoadVehicleBase>> ShouldFailJob = new Dictionary<Type, Predicate<JobDriverLoadVehicleBase>>();
  private static bool working;

  private static IEnumerable<MethodBase> TargetMethods()
  {
    MethodInfo g_Map = AccessTools.PropertyGetter(typeof (JobDriver), "Map");
    foreach (MethodBase methodBase in GenTypes.AllSubclasses(typeof (JobDriverLoadVehicleBase)).Select<Type, MethodInfo>((Func<Type, MethodInfo>) (type => AccessTools.DeclaredMethod(type, "ShouldFailJob", (Type[]) null, (Type[]) null))).Where<MethodInfo>((Func<MethodInfo, bool>) (method => (object) method != null && PatchHelper.ReadMethodBodyWrapper((MethodBase) method).Any<KeyValuePair<OpCode, object>>((Func<KeyValuePair<OpCode, object>, bool>) (i => g_Map.Equals(i.Value))))))
      yield return methodBase;
  }

  public static void Postfix(JobDriverLoadVehicleBase __instance, ref bool __result)
  {
    if (Patch_JobDriverLoadVehicleBase_ShouldFailJob.working || !__result)
      return;
    Map map = ((Thing) ((JobDriver) __instance).pawn).Map;
    try
    {
      Patch_JobDriverLoadVehicleBase_ShouldFailJob.working = true;
      Type type = __instance.GetType();
      Predicate<JobDriverLoadVehicleBase> predicate;
      if (!Patch_JobDriverLoadVehicleBase_ShouldFailJob.ShouldFailJob.TryGetValue(type, out predicate))
      {
        MethodInfo methodInfo = AccessTools.DeclaredMethod(type, "ShouldFailJob", (Type[]) null, (Type[]) null);
        predicate = Patch_JobDriverLoadVehicleBase_ShouldFailJob.ShouldFailJob[type] = AccessTools.MethodDelegate<Predicate<JobDriverLoadVehicleBase>>(methodInfo, (object) null, true, (Type[]) null);
      }
      foreach (Map mapAndVehicleMap in map.BaseMapAndVehicleMaps(false))
      {
        ((Thing) ((JobDriver) __instance).pawn).VirtualMapTransfer(mapAndVehicleMap);
        if (!predicate(__instance))
          __result = false;
      }
    }
    finally
    {
      Patch_JobDriverLoadVehicleBase_ShouldFailJob.working = false;
      ((Thing) ((JobDriver) __instance).pawn).VirtualMapTransfer(map);
    }
  }
}
