// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.VMF_HarmonyPatches.Patches_DeepStorage
// Assembly: VehicleMapFramework, Version=1.6.562.0, Culture=neutral, PublicKeyToken=null
// MVID: 10A61882-945F-4CFC-9B06-CA8EEF5ADB36
// Assembly location: D:\Programas\steamapps\workshop\content\294100\3426502333\1.6\Assemblies\VehicleMapFramework.dll

using HarmonyLib;
using RimWorld;
using System;
using System.Reflection;
using Verse;

#nullable disable
namespace VehicleMapFramework.VMF_HarmonyPatches;

[StaticConstructorOnStartupPriority(200)]
internal static class Patches_DeepStorage
{
  static Patches_DeepStorage()
  {
    if (!ModCompat.DeepStorage)
      return;
    MethodInfo method = new Func<IntVec3, Map, Thing, Pawn, Faction, bool>(StoreAcrossMapsUtility.IsGoodStoreCell).Method;
    MethodInfo methodInfo = AccessTools.Method("LWM.DeepStorage.Patch_IsGoodStoreCell:Postfix", (Type[]) null, (Type[]) null);
    VMF_Harmony.Instance.Patch((MethodBase) method, (HarmonyMethod) null, HarmonyMethod.op_Implicit(methodInfo), (HarmonyMethod) null, (HarmonyMethod) null);
  }
}
