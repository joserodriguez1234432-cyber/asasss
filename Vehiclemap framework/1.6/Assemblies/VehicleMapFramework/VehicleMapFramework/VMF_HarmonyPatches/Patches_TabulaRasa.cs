// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.VMF_HarmonyPatches.Patches_TabulaRasa
// Assembly: VehicleMapFramework, Version=1.6.562.0, Culture=neutral, PublicKeyToken=null
// MVID: 10A61882-945F-4CFC-9B06-CA8EEF5ADB36
// Assembly location: D:\Programas\steamapps\workshop\content\294100\3426502333\1.6\Assemblies\VehicleMapFramework.dll

#nullable disable
namespace VehicleMapFramework.VMF_HarmonyPatches;

[StaticConstructorOnStartupPriority(200)]
internal static class Patches_TabulaRasa
{
  static Patches_TabulaRasa()
  {
    if (!ModCompat.TabulaRasa)
      return;
    VMF_Harmony.PatchCategory("VMF_Patches_TabulaRasa");
    Patch_Projectile_CheckForFreeInterceptBetween.Postfixes.Add(new Patch_Projectile_CheckForFreeInterceptBetween.Postfix(Patch_Patch_Projectile_CheckForFreeInterceptBetween_Postfix.PostfixPatch));
  }
}
