// Decompiled with JetBrains decompiler
// Type: Vehicles.Compatibility.Compatibility_DualWield
// Assembly: Vehicles, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 81BFDC99-D8AD-42E1-8470-2F73E7836B4A
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\Vehicles.dll

using HarmonyLib;
using SmashTools.Patching;
using System;
using System.Reflection;
using Verse;

#nullable disable
namespace Vehicles.Compatibility;

internal class Compatibility_DualWield : ConditionalVehiclePatch
{
  public override string PackageId => "Roolo.DualWield";

  public override PatchSequence PatchAt => PatchSequence.Async;

  public override void PatchAll(ModMetaData mod)
  {
    HarmonyPatcher.Patch((MethodBase) AccessTools.Method(typeof (Pawn_RotationTracker), "UpdateRotation", (System.Type[]) null, (System.Type[]) null), finalizer: new HarmonyMethod(typeof (Compatibility_DualWield), "NoRotationCallForVehicles", (System.Type[]) null));
  }

  private static Exception NoRotationCallForVehicles(Pawn ___pawn, Exception __exception)
  {
    return ___pawn is VehiclePawn && __exception != null ? (Exception) null : __exception;
  }
}
