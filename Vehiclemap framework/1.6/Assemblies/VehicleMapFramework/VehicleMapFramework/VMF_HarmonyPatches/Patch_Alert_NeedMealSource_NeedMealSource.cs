// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.VMF_HarmonyPatches.Patch_Alert_NeedMealSource_NeedMealSource
// Assembly: VehicleMapFramework, Version=1.6.562.0, Culture=neutral, PublicKeyToken=null
// MVID: 10A61882-945F-4CFC-9B06-CA8EEF5ADB36
// Assembly location: D:\Programas\steamapps\workshop\content\294100\3426502333\1.6\Assemblies\VehicleMapFramework.dll

using HarmonyLib;
using RimWorld;
using System;
using System.Linq;
using Verse;

#nullable disable
namespace VehicleMapFramework.VMF_HarmonyPatches;

[HarmonyPatch(typeof (Alert_NeedMealSource), "NeedMealSource")]
[PatchLevel(Level.Safe)]
public static class Patch_Alert_NeedMealSource_NeedMealSource
{
  private static readonly FastInvokeHandler NeedMealSource = MethodInvoker.GetHandler(AccessTools.Method(typeof (Alert_NeedMealSource), nameof (NeedMealSource), (Type[]) null, (Type[]) null), false);

  public static void Postfix(Alert_NeedMealSource __instance, Map map, ref bool __result)
  {
    __result &= VehiclePawnWithMapCache.AllVehiclesOn(map).All<VehiclePawnWithMap>((Func<VehiclePawnWithMap, bool>) (v => (bool) Patch_Alert_NeedMealSource_NeedMealSource.NeedMealSource.Invoke((object) __instance, SingleParam.Get((object) v.VehicleMap))));
  }
}
