// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.VMF_HarmonyPatches.Patch_GenHostility_AnyHostileActiveThreatTo
// Assembly: VehicleMapFramework, Version=1.6.562.0, Culture=neutral, PublicKeyToken=null
// MVID: 10A61882-945F-4CFC-9B06-CA8EEF5ADB36
// Assembly location: D:\Programas\steamapps\workshop\content\294100\3426502333\1.6\Assemblies\VehicleMapFramework.dll

using HarmonyLib;
using RimWorld;
using Verse;
using Verse.AI;

#nullable disable
namespace VehicleMapFramework.VMF_HarmonyPatches;

[HarmonyPatch(typeof (GenHostility), "AnyHostileActiveThreatTo")]
[HarmonyPatch]
[PatchLevel(Level.Safe)]
public static class Patch_GenHostility_AnyHostileActiveThreatTo
{
  public static void Postfix(
    Map map,
    Faction faction,
    ref IAttackTarget threat,
    bool countDormantPawnsAsHostile,
    bool canBeFogged,
    ref bool __result)
  {
    if (__result)
      return;
    foreach (Map mapAndVehicleMap in map.BaseMapAndVehicleMaps(false))
    {
      foreach (IAttackTarget iattackTarget in mapAndVehicleMap.attackTargetsCache.TargetsHostileToFaction(faction))
      {
        if (GenHostility.IsActiveThreatTo(iattackTarget, faction, true, canBeFogged))
        {
          threat = iattackTarget;
          __result = true;
          return;
        }
        if (countDormantPawnsAsHostile && GenHostility.HostileTo(iattackTarget.Thing, faction) && (canBeFogged || !GridsUtility.Fogged(iattackTarget.Thing)) && !iattackTarget.ThreatDisabled((IAttackTargetSearcher) null) && iattackTarget.Thing is Pawn thing)
        {
          CompCanBeDormant comp = ((ThingWithComps) thing).GetComp<CompCanBeDormant>();
          if (comp != null && !comp.Awake)
          {
            threat = iattackTarget;
            __result = true;
            return;
          }
        }
      }
    }
  }
}
