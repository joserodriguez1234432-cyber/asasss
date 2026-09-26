// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.VMF_HarmonyPatches.Patch_Bombardment_TryDoExplosion
// Assembly: VehicleMapFramework, Version=1.6.562.0, Culture=neutral, PublicKeyToken=null
// MVID: 10A61882-945F-4CFC-9B06-CA8EEF5ADB36
// Assembly location: D:\Programas\steamapps\workshop\content\294100\3426502333\1.6\Assemblies\VehicleMapFramework.dll

using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using Verse;

#nullable disable
namespace VehicleMapFramework.VMF_HarmonyPatches;

[HarmonyPatch(typeof (Bombardment), "TryDoExplosion")]
[PatchLevel(Level.Safe)]
public static class Patch_Bombardment_TryDoExplosion
{
  public static List<Func<Bombardment, Bombardment.BombardmentProjectile, bool>> Prefixes { get; } = new List<Func<Bombardment, Bombardment.BombardmentProjectile, bool>>(1)
  {
    new Func<Bombardment, Bombardment.BombardmentProjectile, bool>(Patch_Bombardment_TryDoExplosion.VanillaIntercept)
  };

  public static IEnumerable<CodeInstruction> Transpiler(
    IEnumerable<CodeInstruction> instructions,
    ILGenerator generator)
  {
    Label label;
    // ISSUE: reference to a compiler-generated field
    // ISSUE: reference to a compiler-generated field
    return new CodeMatcher(instructions, generator).MatchStartForward(new CodeMatch[2]
    {
      new CodeMatch(new OpCode?(OpCodes.Ldarg_1), (object) null, (string) null),
      CodeMatch.LoadsField(AccessTools.Field(typeof (Bombardment.BombardmentProjectile), "targetCell"), false)
    }).CreateLabel(ref label).Insert(new CodeInstruction[5]
    {
      CodeInstruction.LoadArgument(0, false),
      CodeInstruction.LoadArgument(1, false),
      PatchHelper.get_CallInstruction((Patch_Bombardment_TryDoExplosion.\u003C\u003EO.\u003C0\u003E__CheckInterceptCrossMap ?? (Patch_Bombardment_TryDoExplosion.\u003C\u003EO.\u003C0\u003E__CheckInterceptCrossMap = new Func<Bombardment, Bombardment.BombardmentProjectile, bool>(Patch_Bombardment_TryDoExplosion.CheckInterceptCrossMap))).Method),
      new CodeInstruction(OpCodes.Brfalse_S, (object) label),
      new CodeInstruction(OpCodes.Ret, (object) null)
    }).InstructionEnumeration();
  }

  private static bool CheckInterceptCrossMap(
    Bombardment __instance,
    Bombardment.BombardmentProjectile proj)
  {
    if (!((Thing) __instance).Spawned)
      return false;
    ReadOnlySpan<VehiclePawnWithMap> readOnlySpan = VehiclePawnWithMapCache.AllVehiclesOnAsReadOnlySpan(((Thing) __instance).Map);
    for (int index1 = 0; index1 < readOnlySpan.Length; ++index1)
    {
      VehiclePawnWithMap vehiclePawnWithMap = readOnlySpan[index1];
      using (new VirtualTeleporter((Thing) __instance, vehiclePawnWithMap.VehicleMap))
      {
        for (int index2 = 0; index2 < Patch_Bombardment_TryDoExplosion.Prefixes.Count; ++index2)
        {
          if (!Patch_Bombardment_TryDoExplosion.Prefixes[index2](__instance, proj))
            return true;
        }
      }
    }
    return false;
  }

  private static bool VanillaIntercept(
    Bombardment __instance,
    Bombardment.BombardmentProjectile proj)
  {
    List<Thing> thingList = ((Thing) __instance).Map.listerThings.ThingsInGroup((ThingRequestGroup) 54);
    for (int index = 0; index < thingList.Count; ++index)
    {
      if (Patch_CompProjectileInterceptor_CheckBombardmentIntercept.CheckBombardmentIntercept(ThingCompUtility.TryGetComp<CompProjectileInterceptor>(thingList[index]), __instance, proj))
        return false;
    }
    return true;
  }
}
