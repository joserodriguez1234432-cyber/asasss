// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.VMF_HarmonyPatches.Patch_Projectile_CheckForFreeInterceptBetween
// Assembly: VehicleMapFramework, Version=1.6.562.0, Culture=neutral, PublicKeyToken=null
// MVID: 10A61882-945F-4CFC-9B06-CA8EEF5ADB36
// Assembly location: D:\Programas\steamapps\workshop\content\294100\3426502333\1.6\Assemblies\VehicleMapFramework.dll

using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using UnityEngine;
using Verse;

#nullable disable
namespace VehicleMapFramework.VMF_HarmonyPatches;

[HarmonyPatch(typeof (Projectile), "CheckForFreeInterceptBetween")]
[PatchLevel(Level.Sensitive)]
[StaticConstructorOnStartup]
public static class Patch_Projectile_CheckForFreeInterceptBetween
{
  private static readonly Action<Projectile, Thing, bool> Impact = AccessTools.MethodDelegate<Action<Projectile, Thing, bool>>(AccessTools.Method(typeof (Projectile), nameof (Impact), (Type[]) null, (Type[]) null), (object) null, true, (Type[]) null);

  public static List<Patch_Projectile_CheckForFreeInterceptBetween.Prefix> Prefixes { get; } = new List<Patch_Projectile_CheckForFreeInterceptBetween.Prefix>();

  public static List<Patch_Projectile_CheckForFreeInterceptBetween.Postfix> Postfixes { get; } = new List<Patch_Projectile_CheckForFreeInterceptBetween.Postfix>(1)
  {
    new Patch_Projectile_CheckForFreeInterceptBetween.Postfix(Patch_Projectile_CheckForFreeInterceptBetween.VanillaIntercept)
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
      CodeMatch.Calls(MethodInfoCache.CachedMethodInfo.m_ToIntVec3)
    }).CreateLabel(ref label).Insert(new CodeInstruction[7]
    {
      CodeInstruction.LoadArgument(0, false),
      CodeInstruction.LoadArgument(1, false),
      CodeInstruction.LoadArgument(2, false),
      PatchHelper.get_CallInstruction((Patch_Projectile_CheckForFreeInterceptBetween.\u003C\u003EO.\u003C0\u003E__CheckInterceptCrossMap ?? (Patch_Projectile_CheckForFreeInterceptBetween.\u003C\u003EO.\u003C0\u003E__CheckInterceptCrossMap = new Func<Projectile, Vector3, Vector3, bool>(Patch_Projectile_CheckForFreeInterceptBetween.CheckInterceptCrossMap))).Method),
      new CodeInstruction(OpCodes.Brfalse_S, (object) label),
      new CodeInstruction(OpCodes.Ldc_I4_1, (object) null),
      new CodeInstruction(OpCodes.Ret, (object) null)
    }).InstructionEnumeration();
  }

  private static bool CheckInterceptCrossMap(
    Projectile instance,
    Vector3 lastExactPos,
    Vector3 newExactPos)
  {
    if (!((Thing) instance).Spawned)
      return false;
    ReadOnlySpan<VehiclePawnWithMap> readOnlySpan = VehiclePawnWithMapCache.AllVehiclesOnAsReadOnlySpan(((Thing) instance).Map);
    for (int index1 = 0; index1 < readOnlySpan.Length; ++index1)
    {
      VehiclePawnWithMap vehiclePawnWithMap = readOnlySpan[index1];
      TargetMapUtility.set_TargetMap((Thing) instance, vehiclePawnWithMap.VehicleMap);
      try
      {
        for (int index2 = 0; index2 < Patch_Projectile_CheckForFreeInterceptBetween.Prefixes.Count; ++index2)
        {
          bool __result = false;
          if (!Patch_Projectile_CheckForFreeInterceptBetween.Prefixes[index2](instance, lastExactPos, newExactPos, ref __result) & __result)
            return true;
        }
        for (int index3 = 0; index3 < Patch_Projectile_CheckForFreeInterceptBetween.Postfixes.Count; ++index3)
        {
          bool __result = false;
          Patch_Projectile_CheckForFreeInterceptBetween.Postfixes[index3](instance, ref __result, lastExactPos, newExactPos);
          if (__result)
          {
            Patch_Projectile_CheckForFreeInterceptBetween.Impact(instance, (Thing) null, true);
            return true;
          }
        }
      }
      finally
      {
        ((Thing) instance).RemoveTargetInfo();
      }
    }
    return false;
  }

  private static void VanillaIntercept(
    Projectile instance,
    ref bool __result,
    Vector3 lastExactPos,
    Vector3 newExactPos)
  {
    List<Thing> thingList = TargetMapUtility.get_TargetMapOrThingMap((Thing) instance).listerThings.ThingsInGroup((ThingRequestGroup) 54);
    for (int index = 0; index < thingList.Count; ++index)
    {
      if (Patch_CompProjectileInterceptor_CheckIntercept.CheckIntercept(ThingCompUtility.TryGetComp<CompProjectileInterceptor>(thingList[index]), instance, lastExactPos, newExactPos))
      {
        __result = true;
        break;
      }
    }
  }

  public delegate void Postfix(
    Projectile __instance,
    ref bool __result,
    Vector3 lastExactPos,
    Vector3 newExactPos);

  public delegate bool Prefix(
    Projectile __instance,
    Vector3 lastExactPos,
    Vector3 newExactPos,
    ref bool __result);
}
