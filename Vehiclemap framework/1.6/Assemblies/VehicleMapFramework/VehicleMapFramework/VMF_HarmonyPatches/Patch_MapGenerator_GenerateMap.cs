// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.VMF_HarmonyPatches.Patch_MapGenerator_GenerateMap
// Assembly: VehicleMapFramework, Version=1.6.562.0, Culture=neutral, PublicKeyToken=null
// MVID: 10A61882-945F-4CFC-9B06-CA8EEF5ADB36
// Assembly location: D:\Programas\steamapps\workshop\content\294100\3426502333\1.6\Assemblies\VehicleMapFramework.dll

using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using Verse;

#nullable disable
namespace VehicleMapFramework.VMF_HarmonyPatches;

[HarmonyPatchCategory("VMF_Patches_AsAboveSoBelow")]
[HarmonyAfter(new string[] {"astryl.asabovesobelow"})]
[HarmonyPatch(typeof (MapGenerator), "GenerateMap")]
[PatchLevel(Level.Mandatory)]
public static class Patch_MapGenerator_GenerateMap
{
  public static void Prefix(ref IntVec3 mapSize, MapParent parent)
  {
    if (!(parent is MapParent_Vehicle))
      return;
    Faction faction = ((WorldObject) parent).Faction;
    if (faction == null || !faction.IsPlayer)
      return;
    object obj = ModCompat.AsAboveSoBelow.PendingLayout.Invoke((object[]) null);
    int num1 = ModCompat.AsAboveSoBelow.UpperLevels();
    if (num1 < 1)
      return;
    int num2 = num1 + 1;
    ModCompat.AsAboveSoBelow.bandCount.SetValue(obj, (object) num2);
    ModCompat.AsAboveSoBelow.bandHeight.SetValue(obj, (object) mapSize.z);
    ModCompat.AsAboveSoBelow.pending.SetValue((object) null, obj);
    mapSize = new IntVec3(mapSize.x, mapSize.y, num2 * ModCompat.AsAboveSoBelow.SlotFor(mapSize.z));
  }
}
