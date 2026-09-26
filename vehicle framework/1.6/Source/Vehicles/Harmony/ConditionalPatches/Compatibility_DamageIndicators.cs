// Decompiled with JetBrains decompiler
// Type: Vehicles.Compatibility.Compatibility_DamageIndicators
// Assembly: Vehicles, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 81BFDC99-D8AD-42E1-8470-2F73E7836B4A
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\Vehicles.dll

using HarmonyLib;
using SmashTools.Patching;
using System;
using UnityEngine;
using Verse;

#nullable disable
namespace Vehicles.Compatibility;

internal class Compatibility_DamageIndicators : ConditionalVehiclePatch
{
  public static Action<float, Map, Vector3, string> throwDamageMote;

  public static bool ModLoaded { get; private set; }

  public override string PackageId => "CaesarV6.DamageIndicators";

  public override PatchSequence PatchAt => PatchSequence.Async;

  public override void PatchAll(ModMetaData mod)
  {
    Compatibility_DamageIndicators.ModLoaded = true;
    Compatibility_DamageIndicators.throwDamageMote = (Action<float, Map, Vector3, string>) Delegate.CreateDelegate(typeof (Action<float, Map, Vector3, string>), AccessTools.Method(GenTypes.GetTypeInAnyAssembly("DamageMotes.DamageMotes_Patch", (string) null), "ThrowDamageMote", (System.Type[]) null, (System.Type[]) null), true);
  }
}
