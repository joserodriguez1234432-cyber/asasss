// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.VMF_HarmonyPatches.Patch_TransporterUtility_GetTransportersInGroup
// Assembly: VehicleMapFramework, Version=1.6.562.0, Culture=neutral, PublicKeyToken=null
// MVID: 10A61882-945F-4CFC-9B06-CA8EEF5ADB36
// Assembly location: D:\Programas\steamapps\workshop\content\294100\3426502333\1.6\Assemblies\VehicleMapFramework.dll

using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using Verse;

#nullable disable
namespace VehicleMapFramework.VMF_HarmonyPatches;

[HarmonyPatch(typeof (TransporterUtility), "GetTransportersInGroup")]
[PatchLevel(Level.Safe)]
public static class Patch_TransporterUtility_GetTransportersInGroup
{
  public static void Postfix(int transportersGroup, Map map, List<CompTransporter> outTransporters)
  {
    if (transportersGroup < 0)
      return;
    outTransporters.AddRange((IEnumerable<CompTransporter>) VehiclePawnWithMapCache.AllVehiclesOn(map.BaseMap()).SelectMany<VehiclePawnWithMap, CompBuildableContainer>((Func<VehiclePawnWithMap, IEnumerable<CompBuildableContainer>>) (vehicle => (IEnumerable<CompBuildableContainer>) vehicle.ContainerComps)).Where<CompBuildableContainer>((Func<CompBuildableContainer, bool>) (compTransporter => compTransporter.groupID == transportersGroup)));
  }
}
