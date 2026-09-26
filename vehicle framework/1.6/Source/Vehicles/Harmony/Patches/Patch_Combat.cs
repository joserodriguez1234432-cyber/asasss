// Decompiled with JetBrains decompiler
// Type: Vehicles.Patch_Combat
// Assembly: Vehicles, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 81BFDC99-D8AD-42E1-8470-2F73E7836B4A
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\Vehicles.dll

using HarmonyLib;
using RimWorld;
using SmashTools;
using SmashTools.Patching;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using UnityEngine;
using Verse;

#nullable disable
namespace Vehicles;

internal class Patch_Combat : IPatchCategory
{
  PatchSequence IPatchCategory.PatchAt => PatchSequence.Async;

  void IPatchCategory.PatchMethods()
  {
    HarmonyPatcher.Patch((MethodBase) AccessTools.PropertyGetter(typeof (Projectile), "StartingTicksToImpact"), postfix: new HarmonyMethod(typeof (Patch_Combat), "StartingTicksFromTurret", (System.Type[]) null));
    HarmonyPatcher.Patch((MethodBase) AccessTools.Method(typeof (Projectile), "CanHit", (System.Type[]) null, (System.Type[]) null), new HarmonyMethod(typeof (Patch_Combat), "TurretHitFlags", (System.Type[]) null));
    HarmonyPatcher.Patch((MethodBase) AccessTools.Method(typeof (Projectile_Explosive), "Impact", (System.Type[]) null, (System.Type[]) null), new HarmonyMethod(typeof (Patch_Combat), "ImpactExplosiveProjectiles", (System.Type[]) null));
    HarmonyPatcher.Patch((MethodBase) AccessTools.Method(typeof (Projectile), "ImpactSomething", (System.Type[]) null, (System.Type[]) null), transpiler: new HarmonyMethod(typeof (Patch_Combat), "VehicleProjectileChanceToHit", (System.Type[]) null));
    HarmonyPatcher.Patch((MethodBase) AccessTools.Method(typeof (Thing), "Destroy", (System.Type[]) null, (System.Type[]) null), new HarmonyMethod(typeof (Patch_Combat), "ProjectileMapToWorld", (System.Type[]) null));
    HarmonyPatcher.Patch((MethodBase) AccessTools.Method(typeof (Projectile), "CheckForFreeIntercept", (System.Type[]) null, (System.Type[]) null), transpiler: new HarmonyMethod(typeof (Patch_Combat), "VehicleProjectileInterceptor", (System.Type[]) null));
    HarmonyPatcher.Patch((MethodBase) AccessTools.Method(typeof (Explosion), "AffectCell", (System.Type[]) null, (System.Type[]) null), new HarmonyMethod(typeof (Patch_Combat), "AffectVehicleInCell", (System.Type[]) null));
    HarmonyPatcher.Patch((MethodBase) AccessTools.Method(typeof (DamageWorker), "ExplosionDamageThing", (System.Type[]) null, (System.Type[]) null), postfix: new HarmonyMethod(typeof (Patch_Combat), "VehicleMultipleExplosionInstances", (System.Type[]) null), transpiler: new HarmonyMethod(typeof (Patch_Combat), "VehicleExplosionDamageTranspiler", (System.Type[]) null));
  }

  private static void StartingTicksFromTurret(
    Projectile __instance,
    ref float __result,
    Vector3 ___origin,
    Vector3 ___destination)
  {
    CompTurretProjectileProperties comp = ThingCompUtility.TryGetComp<CompTurretProjectileProperties>((Thing) __instance);
    if (comp == null)
      return;
    Vector3 vector3 = Vector3.op_Subtraction(___origin, ___destination);
    float num = ((Vector3) ref vector3).magnitude / (comp.speed / 100f);
    if ((double) num <= 0.0)
      num = 1f / 1000f;
    __result = num;
  }

  private static bool TurretHitFlags(
    Thing thing,
    Projectile __instance,
    Thing ___launcher,
    ref bool __result)
  {
    CompTurretProjectileProperties comp = ThingCompUtility.TryGetComp<CompTurretProjectileProperties>((Thing) __instance);
    if (comp == null)
      return true;
    if (!thing.Spawned)
    {
      __result = false;
      return false;
    }
    if (thing == ___launcher)
    {
      __result = false;
      return false;
    }
    bool flag1 = false;
    CellRect cellRect = GenAdj.OccupiedRect(thing);
    foreach (IntVec3 intVec3 in cellRect)
    {
      bool flag2 = false;
      foreach (Thing thing1 in GridsUtility.GetThingList(intVec3, ((Thing) __instance).Map))
      {
        if (thing1 != thing && (comp.hitflags != null && (double) thing1.def.fillPercent >= (double) comp.hitflags.minFillPercent || comp.hitflags == null && thing1.def.Fillage == 2) && (double) ((BuildableDef) thing1.def).Altitude >= (double) ((BuildableDef) thing.def).Altitude)
        {
          flag2 = true;
          break;
        }
      }
      if (!flag2)
      {
        flag1 = true;
        break;
      }
    }
    if (!flag1)
    {
      __result = false;
      return false;
    }
    ProjectileHitFlags hitFlags = __instance.HitFlags;
    if (LocalTargetInfo.op_Equality(LocalTargetInfo.op_Implicit(thing), __instance.intendedTarget) && (hitFlags & 1) != null)
    {
      __result = true;
      return false;
    }
    if (LocalTargetInfo.op_Inequality(LocalTargetInfo.op_Implicit(thing), __instance.intendedTarget))
    {
      if (thing is Pawn pawn)
      {
        if ((hitFlags & 2) != null)
        {
          __result = true;
          return false;
        }
        CustomHitFlags hitflags = comp.hitflags;
        if (hitflags != null && hitflags.hitThroughPawns && !pawn.Dead && !pawn.Downed)
          thing.TakeDamage(new DamageInfo(DamageDefOf.Blunt, comp.speed * 2f, 0.0f, -1f, (Thing) __instance, (BodyPartRecord) null, (ThingDef) null, (DamageInfo.SourceCategory) 0, (Thing) null, true, true, (QualityCategory) 2, true, false));
      }
      else if ((hitFlags & 4) != null)
      {
        __result = true;
        return false;
      }
    }
    CustomHitFlags hitflags1 = comp.hitflags;
    if (hitflags1 != null && (double) hitflags1.minFillPercent > 0.0)
    {
      ref bool local = ref __result;
      double fillPercent = (double) thing.def.fillPercent;
      float? minFillPercent = comp.hitflags?.minFillPercent;
      double valueOrDefault = (double) minFillPercent.GetValueOrDefault();
      int num = fillPercent >= valueOrDefault & minFillPercent.HasValue ? 1 : 0;
      local = num != 0;
      return false;
    }
    ref bool local1 = ref __result;
    int num1;
    if (LocalTargetInfo.op_Equality(LocalTargetInfo.op_Implicit(thing), __instance.intendedTarget))
    {
      double fillPercent = (double) thing.def.fillPercent;
      float? minFillPercent = comp.hitflags?.minFillPercent;
      double valueOrDefault = (double) minFillPercent.GetValueOrDefault();
      num1 = fillPercent >= valueOrDefault & minFillPercent.HasValue ? 1 : 0;
    }
    else
      num1 = 0;
    local1 = num1 != 0;
    return false;
  }

  private static bool ImpactExplosiveProjectiles(
    Thing hitThing,
    Projectile __instance,
    Thing ___launcher)
  {
    if (hitThing is VehiclePawn vehiclePawn)
      vehiclePawn.statHandler.RegisterImpacter(___launcher, ((Thing) __instance).Position);
    if (VehicleMod.settings.main.reduceExplosionsOnWater && ((Def) ((Thing) __instance).def).GetModExtension<ReduceExplosionOnWater>() != null)
    {
      TerrainDef terrainDef = ((Thing) __instance).Map.terrainGrid.TerrainAt(((Thing) __instance).Position);
      if (((Thing) __instance).def.projectile.explosionDelay == 0 && terrainDef.IsWater && !GridsUtility.GetThingList(((Thing) __instance).Position, ((Thing) __instance).Map).NotNullAndAny<Thing>((Predicate<Thing>) (x => x is VehiclePawn)))
      {
        DamageHelper.Explode(__instance);
        return false;
      }
    }
    return true;
  }

  private static IEnumerable<CodeInstruction> VehicleProjectileChanceToHit(
    IEnumerable<CodeInstruction> instructions)
  {
    List<CodeInstruction> instructionList = instructions.ToList<CodeInstruction>();
    for (int i = 0; i < instructionList.Count; ++i)
    {
      CodeInstruction instruction = instructionList[i];
      if (instruction.opcode == OpCodes.Stloc_S && instruction.operand is LocalBuilder operand && operand.LocalIndex == 8)
      {
        yield return instruction;
        instruction = instructionList[++i];
        yield return instruction;
        instruction = instructionList[++i];
        yield return new CodeInstruction(OpCodes.Call, (object) AccessTools.Method(typeof (Patch_Combat), "VehiclePawnFillageInterceptReroute", (System.Type[]) null, (System.Type[]) null));
      }
      yield return instruction;
      instruction = (CodeInstruction) null;
    }
  }

  private static void ProjectileMapToWorld(Thing __instance)
  {
    if (!(__instance is Projectile projectile))
      return;
    ((ThingWithComps) projectile).GetComp<CompProjectileExitMap>()?.LeaveMap();
  }

  private static IEnumerable<CodeInstruction> VehicleProjectileInterceptor(
    IEnumerable<CodeInstruction> instructions)
  {
    List<CodeInstruction> instructionList = instructions.ToList<CodeInstruction>();
    for (int i = 0; i < instructionList.Count; ++i)
    {
      CodeInstruction instruction = instructionList[i];
      if (instruction.opcode == OpCodes.Stloc_S && instruction.operand is LocalBuilder operand && operand.LocalIndex == 10)
      {
        yield return instruction;
        instruction = instructionList[++i];
        yield return instruction;
        instruction = instructionList[++i];
        yield return new CodeInstruction(OpCodes.Call, (object) AccessTools.Method(typeof (Patch_Combat), "VehiclePawnFillageInterceptReroute", (System.Type[]) null, (System.Type[]) null));
      }
      yield return instruction;
      instruction = (CodeInstruction) null;
    }
  }

  private static bool AffectVehicleInCell(Explosion __instance, IntVec3 c)
  {
    VehiclePawn vehiclePawn = ((Thing) __instance).Map.GetDetachedMapComponent<VehiclePositionManager>().ClaimedBy(c);
    if (vehiclePawn == null)
      return true;
    CellRect cellRect = GenAdj.OccupiedRect((Thing) vehiclePawn);
    return ((CellRect) ref cellRect).EdgeCells.Contains<IntVec3>(c);
  }

  private static void VehicleMultipleExplosionInstances(
    Thing t,
    ref List<Thing> damagedThings,
    List<Thing> ignoredThings)
  {
    if (!(t is VehiclePawn vehiclePawn) || ignoredThings != null && ignoredThings.Contains(t))
      return;
    damagedThings.Remove((Thing) vehiclePawn);
  }

  private static IEnumerable<CodeInstruction> VehicleExplosionDamageTranspiler(
    IEnumerable<CodeInstruction> instructions)
  {
    List<CodeInstruction> instructionList = instructions.ToList<CodeInstruction>();
    MethodInfo takeDamageMethod = AccessTools.Method(typeof (Thing), "TakeDamage", (System.Type[]) null, (System.Type[]) null);
    for (int i = 0; i < instructionList.Count; ++i)
    {
      CodeInstruction codeInstruction = instructionList[i];
      if (CodeInstructionExtensions.Calls(codeInstruction, takeDamageMethod))
      {
        yield return new CodeInstruction(OpCodes.Pop, (object) null);
        yield return new CodeInstruction(OpCodes.Pop, (object) null);
        yield return new CodeInstruction(OpCodes.Ldloc_1, (object) null);
        yield return new CodeInstruction(OpCodes.Ldarg_2, (object) null);
        yield return new CodeInstruction(OpCodes.Ldarg_S, (object) 5);
        yield return new CodeInstruction(OpCodes.Call, (object) AccessTools.Method(typeof (Patch_Combat), "TakeDamageReroute", (System.Type[]) null, (System.Type[]) null));
        codeInstruction = instructionList[++i];
      }
      yield return codeInstruction;
    }
  }

  private static DamageWorker.DamageResult TakeDamageReroute(
    DamageInfo dinfo,
    Thing thing,
    IntVec3 cell)
  {
    DamageWorker.DamageResult result;
    return thing is VehiclePawn vehiclePawn && vehiclePawn.TryTakeDamage(dinfo, cell, out result) ? result : thing.TakeDamage(dinfo);
  }

  private static Pawn VehiclePawnFillageInterceptReroute(Pawn pawn)
  {
    return !(pawn is VehiclePawn) ? pawn : (Pawn) null;
  }
}
