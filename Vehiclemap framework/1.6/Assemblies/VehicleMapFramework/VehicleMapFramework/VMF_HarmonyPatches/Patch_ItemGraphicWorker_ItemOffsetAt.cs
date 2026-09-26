// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.VMF_HarmonyPatches.Patch_ItemGraphicWorker_ItemOffsetAt
// Assembly: VehicleMapFramework, Version=1.6.562.0, Culture=neutral, PublicKeyToken=null
// MVID: 10A61882-945F-4CFC-9B06-CA8EEF5ADB36
// Assembly location: D:\Programas\steamapps\workshop\content\294100\3426502333\1.6\Assemblies\VehicleMapFramework.dll

using HarmonyLib;
using Verse;

#nullable disable
namespace VehicleMapFramework.VMF_HarmonyPatches;

[HarmonyPatchCategory("VMF_Patches_AdaptiveStorageFramework")]
[HarmonyPatch]
[PatchLevel(Level.Safe)]
public static class Patch_ItemGraphicWorker_ItemOffsetAt
{
  public static void Postfix(ref float stackRotation)
  {
    ref float local = ref stackRotation;
    double num1 = (double) stackRotation;
    Rot4 rotForPrint = VehicleSectionLayerManager.RotForPrint;
    double asAngle = (double) ((Rot4) ref rotForPrint).AsAngle;
    double num2 = num1 - asAngle;
    local = (float) num2;
  }
}
