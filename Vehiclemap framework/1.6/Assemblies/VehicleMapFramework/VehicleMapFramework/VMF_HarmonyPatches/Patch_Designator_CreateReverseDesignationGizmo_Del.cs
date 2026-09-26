// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.VMF_HarmonyPatches.Patch_Designator_CreateReverseDesignationGizmo_Delegate
// Assembly: VehicleMapFramework, Version=1.6.562.0, Culture=neutral, PublicKeyToken=null
// MVID: 10A61882-945F-4CFC-9B06-CA8EEF5ADB36
// Assembly location: D:\Programas\steamapps\workshop\content\294100\3426502333\1.6\Assemblies\VehicleMapFramework.dll

using HarmonyLib;
using System;
using System.Reflection;
using Verse;

#nullable disable
namespace VehicleMapFramework.VMF_HarmonyPatches;

[HarmonyPatch]
[PatchLevel(Level.Sensitive)]
public static class Patch_Designator_CreateReverseDesignationGizmo_Delegate
{
  private static MethodBase TargetMethod()
  {
    return (MethodBase) AccessTools.FindIncludingInnerTypes<MethodInfo>(typeof (Designator), (Func<Type, MethodInfo>) (t => GenCollection.FirstOrDefault<MethodInfo>(AccessToolsExtensions.GetDeclaredMethods(t), (Predicate<MethodInfo>) (m => m.Name.Contains("<CreateReverseDesignationGizmo>")))));
  }

  public static void Prefix(Thing ___t, ref VehiclePawnWithMap __state)
  {
    VehiclePawnWithMap vehicle;
    ___t.IsOnVehicleMapOf(out vehicle);
    __state = Command_FocusVehicleMap.FocusedVehicle;
    Command_FocusVehicleMap.FocusedVehicle = vehicle;
  }

  public static void Finalizer(VehiclePawnWithMap __state)
  {
    Command_FocusVehicleMap.FocusedVehicle = __state;
  }
}
