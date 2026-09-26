// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.VMF_HarmonyPatches.Patch_GenSpawn_Spawn_Rimatomics
// Assembly: VehicleMapFramework, Version=1.6.562.0, Culture=neutral, PublicKeyToken=null
// MVID: 10A61882-945F-4CFC-9B06-CA8EEF5ADB36
// Assembly location: D:\Programas\steamapps\workshop\content\294100\3426502333\1.6\Assemblies\VehicleMapFramework.dll

using HarmonyLib;
using System;
using Verse;

#nullable disable
namespace VehicleMapFramework.VMF_HarmonyPatches;

[HarmonyPatchCategory("VMF_Patches_Rimatomics")]
[HarmonyPatch(typeof (GenSpawn), "Spawn", new Type[] {typeof (Thing), typeof (IntVec3), typeof (Map), typeof (Rot4), typeof (WipeMode), typeof (bool), typeof (bool)})]
[PatchLevel(Level.Safe)]
public static class Patch_GenSpawn_Spawn_Rimatomics
{
  public static void Prefix(Thing newThing, ref Map map)
  {
    if (!GenTypes.SameOrSubclassOf(newThing?.def?.thingClass, ModCompat.Rimatomics.BaseMissile))
      return;
    map = map.BaseMap();
  }
}
