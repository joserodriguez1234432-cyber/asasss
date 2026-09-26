// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.VMF_HarmonyPatches.Patches_UFHeavyIndustries
// Assembly: VehicleMapFramework, Version=1.6.562.0, Culture=neutral, PublicKeyToken=null
// MVID: 10A61882-945F-4CFC-9B06-CA8EEF5ADB36
// Assembly location: D:\Programas\steamapps\workshop\content\294100\3426502333\1.6\Assemblies\VehicleMapFramework.dll

using HarmonyLib;
using RimWorld;
using System;

#nullable disable
namespace VehicleMapFramework.VMF_HarmonyPatches;

[StaticConstructorOnStartupPriority(200)]
internal class Patches_UFHeavyIndustries
{
  static Patches_UFHeavyIndustries()
  {
    if (!ModCompat.UFHeavyIndustries)
      return;
    VMF_Harmony.PatchCategory("VMF_Patches_UFHeavyIndustries");
    try
    {
      Patch_Projectile_CheckForFreeInterceptBetween.Prefixes.Add(new Patch_Projectile_CheckForFreeInterceptBetween.Prefix(Patch_Patch_Projectile_CheckForFreeInterceptBetween_Prefix.PrefixPatch));
      Patch_Bombardment_TryDoExplosion.Prefixes.Add(AccessTools.MethodDelegate<Func<Bombardment, Bombardment.BombardmentProjectile, bool>>(AccessTools.Method("ATFieldGenerator.Patch_Bombardment_TryDoExplosion:Prefix", (Type[]) null, (Type[]) null), (object) null, true, (Type[]) null) ?? throw new NullReferenceException());
    }
    catch (Exception ex)
    {
      VMF_Log.Error($"{ex}");
    }
  }
}
