// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.VMF_HarmonyPatches.Patch_SettingsCache_TryGetValue
// Assembly: VehicleMapFramework, Version=1.6.562.0, Culture=neutral, PublicKeyToken=null
// MVID: 10A61882-945F-4CFC-9B06-CA8EEF5ADB36
// Assembly location: D:\Programas\steamapps\workshop\content\294100\3426502333\1.6\Assemblies\VehicleMapFramework.dll

using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Vehicles;
using Verse;

#nullable disable
namespace VehicleMapFramework.VMF_HarmonyPatches;

[HarmonyPatchCategory("VMF_Patches_VehicleFramework")]
[HarmonyPatch]
[PatchLevel(Level.Safe)]
public static class Patch_SettingsCache_TryGetValue
{
  private static IEnumerable<MethodBase> TargetMethods()
  {
    return (IEnumerable<MethodBase>) AccessTools.GetDeclaredMethods(typeof (SettingsCache)).Where<MethodInfo>((Func<MethodInfo, bool>) (m => m.Name == "TryGetValue")).Select<MethodInfo, MethodInfo>((Func<MethodInfo, MethodInfo>) (m =>
    {
      if (!m.IsGenericMethodDefinition)
        return m;
      return m.MakeGenericMethod(typeof (bool));
    }));
  }

  public static void Prefix(ref VehicleDef def)
  {
    VehicleMapProps_Unique modExtension = ((Def) def).GetModExtension<VehicleMapProps_Unique>();
    if (modExtension == null || modExtension.baseDef == null)
      return;
    def = modExtension.baseDef;
  }
}
