// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.VMF_HarmonyPatches.Patch_DamageWatcher_Notify_DamageTaken
// Assembly: VehicleMapFramework, Version=1.6.562.0, Culture=neutral, PublicKeyToken=null
// MVID: 10A61882-945F-4CFC-9B06-CA8EEF5ADB36
// Assembly location: D:\Programas\steamapps\workshop\content\294100\3426502333\1.6\Assemblies\VehicleMapFramework.dll

using HarmonyLib;
using RimWorld;
using Verse;

#nullable disable
namespace VehicleMapFramework.VMF_HarmonyPatches;

[HarmonyPatch(typeof (DamageWatcher), "Notify_DamageTaken")]
[PatchLevel(Level.Safe)]
public static class Patch_DamageWatcher_Notify_DamageTaken
{
  private static bool working;

  public static void Postfix(Thing damagee, float amount)
  {
    if (Patch_DamageWatcher_Notify_DamageTaken.working || damagee.Faction != Faction.OfPlayer)
      return;
    Map groundMap = VehicleMapUtility.get_GroundMap(damagee);
    if (damagee.Map == VehicleMapUtility.get_GroundMap(damagee))
      return;
    Patch_DamageWatcher_Notify_DamageTaken.working = true;
    groundMap.damageWatcher.Notify_DamageTaken(damagee, amount);
    Patch_DamageWatcher_Notify_DamageTaken.working = false;
  }
}
