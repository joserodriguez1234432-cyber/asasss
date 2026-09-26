// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.VMF_HarmonyPatches.Patches_RealFogOfWar
// Assembly: VehicleMapFramework, Version=1.6.562.0, Culture=neutral, PublicKeyToken=null
// MVID: 10A61882-945F-4CFC-9B06-CA8EEF5ADB36
// Assembly location: D:\Programas\steamapps\workshop\content\294100\3426502333\1.6\Assemblies\VehicleMapFramework.dll

#nullable disable
namespace VehicleMapFramework.VMF_HarmonyPatches;

[StaticConstructorOnStartupPriority(200)]
internal class Patches_RealFogOfWar
{
  static Patches_RealFogOfWar()
  {
    if (!ModCompat.RealFogOfWar)
      return;
    VMF_Harmony.PatchCategory("VMF_Patches_RealFogOfWar");
  }
}
