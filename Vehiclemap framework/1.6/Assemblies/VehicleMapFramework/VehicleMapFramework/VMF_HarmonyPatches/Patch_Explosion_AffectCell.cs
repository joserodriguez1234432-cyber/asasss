// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.VMF_HarmonyPatches.Patch_Explosion_AffectCell
// Assembly: VehicleMapFramework, Version=1.6.562.0, Culture=neutral, PublicKeyToken=null
// MVID: 10A61882-945F-4CFC-9B06-CA8EEF5ADB36
// Assembly location: D:\Programas\steamapps\workshop\content\294100\3426502333\1.6\Assemblies\VehicleMapFramework.dll

using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using Verse;

#nullable disable
namespace VehicleMapFramework.VMF_HarmonyPatches;

[HarmonyPatch(typeof (Explosion), "AffectCell")]
public static class Patch_Explosion_AffectCell
{
  [HarmonyPriority(400)]
  [PatchLevel(Level.Safe)]
  public static void Postfix(Explosion __instance, IntVec3 c)
  {
    VehiclePawnWithMap vehicle;
    if (!c.TryGetVehicleMap(((Thing) __instance).Map, out vehicle))
      return;
    IntVec3 vehicleMapCoord = c.ToVehicleMapCoord(vehicle);
    if (!GenGrid.InBounds(vehicleMapCoord, vehicle.VehicleMap))
      return;
    using (new VirtualTeleporter((Thing) __instance, vehicle.VehicleMap, new IntVec3?(((Thing) __instance).Position.ToVehicleMapCoord(vehicle))))
      Patch_Explosion_AffectCell.AffectCell(__instance, vehicleMapCoord, c);
  }

  [HarmonyReversePatch]
  [HarmonyPriority(500)]
  [PatchLevel(Level.Mandatory)]
  [MethodImpl(MethodImplOptions.NoInlining)]
  public static void AffectCell(Explosion instance, IntVec3 c2, IntVec3 c)
  {
    Transpiler((IEnumerable<CodeInstruction>) null);
    throw new NotImplementedException();

    static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
    {
      return (IEnumerable<CodeInstruction>) new CodeMatcher(instructions, (ILGenerator) null).MatchStartForward(new CodeMatch[2]
      {
        new CodeMatch(new OpCode?(OpCodes.Ldarg_1), (object) null, (string) null),
        CodeMatch.Calls(AccessTools.Method(typeof (Explosion), "ShouldCellBeAffectedOnlyByDamage", (Type[]) null, (Type[]) null))
      }).Repeat((Action<CodeMatcher>) (matcher => matcher.SetOpcodeAndAdvance(OpCodes.Ldarg_2)), (Action<string>) null).InstructionEnumeration().MethodReplacer((MethodInfoCache.CachedMethodInfo.g_Thing_Position, MethodInfoCache.CachedMethodInfo.m_PositionOnTargetMap), (MethodInfoCache.CachedMethodInfo.g_Thing_Map, MethodInfoCache.CachedMethodInfo.m_TargetMapOrThingMap));
    }
  }
}
