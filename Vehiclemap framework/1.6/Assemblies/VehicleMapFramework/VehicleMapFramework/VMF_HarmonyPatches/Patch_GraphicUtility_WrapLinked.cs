// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.VMF_HarmonyPatches.Patch_GraphicUtility_WrapLinked
// Assembly: VehicleMapFramework, Version=1.6.562.0, Culture=neutral, PublicKeyToken=null
// MVID: 10A61882-945F-4CFC-9B06-CA8EEF5ADB36
// Assembly location: D:\Programas\steamapps\workshop\content\294100\3426502333\1.6\Assemblies\VehicleMapFramework.dll

using HarmonyLib;
using Verse;

#nullable disable
namespace VehicleMapFramework.VMF_HarmonyPatches;

[HarmonyPatchCategory("VehicleMapFramework.EarlyPatches")]
[HarmonyPatch(typeof (GraphicUtility), "WrapLinked")]
[PatchLevel(Level.Mandatory)]
public static class Patch_GraphicUtility_WrapLinked
{
  public static bool Prefix(
    Graphic subGraphic,
    LinkDrawerType linkDrawerType,
    ref Graphic_Linked __result)
  {
    if (linkDrawerType != 56)
      return true;
    __result = (Graphic_Linked) new Graphic_LinkedCornerOverlaySingle(subGraphic);
    return false;
  }
}
