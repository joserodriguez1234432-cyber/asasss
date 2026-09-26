// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.VMF_HarmonyPatches.Patches_ExosuitFramework
// Assembly: VehicleMapFramework, Version=1.6.562.0, Culture=neutral, PublicKeyToken=null
// MVID: 10A61882-945F-4CFC-9B06-CA8EEF5ADB36
// Assembly location: D:\Programas\steamapps\workshop\content\294100\3426502333\1.6\Assemblies\VehicleMapFramework.dll

#nullable disable
namespace VehicleMapFramework.VMF_HarmonyPatches;

[StaticConstructorOnStartupPriority(200)]
internal static class Patches_ExosuitFramework
{
  static Patches_ExosuitFramework()
  {
    if (!ModCompat.ExosuitFramework)
      return;
    VMF_Harmony.PatchCategory("VMF_Patches_ExosuitFramework");
  }
}
