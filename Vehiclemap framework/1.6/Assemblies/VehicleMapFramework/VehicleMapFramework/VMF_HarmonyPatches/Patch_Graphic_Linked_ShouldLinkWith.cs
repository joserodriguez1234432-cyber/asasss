// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.VMF_HarmonyPatches.Patch_Graphic_Linked_ShouldLinkWith
// Assembly: VehicleMapFramework, Version=1.6.562.0, Culture=neutral, PublicKeyToken=null
// MVID: 10A61882-945F-4CFC-9B06-CA8EEF5ADB36
// Assembly location: D:\Programas\steamapps\workshop\content\294100\3426502333\1.6\Assemblies\VehicleMapFramework.dll

using HarmonyLib;
using Verse;

#nullable disable
namespace VehicleMapFramework.VMF_HarmonyPatches;

[HarmonyPatch(typeof (Graphic_Linked), "ShouldLinkWith")]
public static class Patch_Graphic_Linked_ShouldLinkWith
{
  [PatchLevel(Level.Mandatory)]
  [HarmonyReversePatch]
  [HarmonyPriority(400)]
  public static bool ShouldLinkWith(Graphic_Linked instance, IntVec3 c, Thing parent)
  {
    if (!parent.Spawned)
      return false;
    return !GenGrid.InBounds(c, parent.Map) ? (parent.def.graphicData.linkFlags & 1) > 0 : (parent.Map.linkGrid.LinkFlagsAt(c) & parent.def.graphicData.linkFlags) > 0;
  }

  [PatchLevel(Level.Safe)]
  [HarmonyPriority(200)]
  public static void Prefix(ref IntVec3 c, Thing parent)
  {
    if (!Rot4.op_Inequality(VehicleSectionLayerManager.RotForPrint, Rot4.North))
      return;
    IntVec3 intVec3 = IntVec3Utility.RotatedBy(IntVec3.op_Subtraction(c, parent.Position), VehicleSectionLayerManager.RotForPrintCounter);
    c = IntVec3.op_Addition(intVec3, parent.Position);
  }
}
