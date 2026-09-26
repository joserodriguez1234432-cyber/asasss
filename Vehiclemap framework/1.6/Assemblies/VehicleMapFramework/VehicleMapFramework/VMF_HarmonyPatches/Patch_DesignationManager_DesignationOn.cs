// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.VMF_HarmonyPatches.Patch_DesignationManager_DesignationOn
// Assembly: VehicleMapFramework, Version=1.6.562.0, Culture=neutral, PublicKeyToken=null
// MVID: 10A61882-945F-4CFC-9B06-CA8EEF5ADB36
// Assembly location: D:\Programas\steamapps\workshop\content\294100\3426502333\1.6\Assemblies\VehicleMapFramework.dll

using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Verse;

#nullable disable
namespace VehicleMapFramework.VMF_HarmonyPatches;

[HarmonyPatch]
[PatchLevel(Level.Safe)]
public static class Patch_DesignationManager_DesignationOn
{
  private static IEnumerable<MethodBase> TargetMethods()
  {
    return (IEnumerable<MethodBase>) AccessTools.GetDeclaredMethods(typeof (DesignationManager)).Where<MethodInfo>((Func<MethodInfo, bool>) (m => m.Name == "DesignationOn"));
  }

  public static void Prefix(ref DesignationManager __instance, Thing t)
  {
    Map mapHeld = t.MapHeld;
    if (mapHeld == null || mapHeld == __instance.map)
      return;
    __instance = mapHeld.designationManager;
  }
}
