// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.VMF_HarmonyPatches.Patch_ItemGraphicWorker_DrawOffsetForItem
// Assembly: VehicleMapFramework, Version=1.6.562.0, Culture=neutral, PublicKeyToken=null
// MVID: 10A61882-945F-4CFC-9B06-CA8EEF5ADB36
// Assembly location: D:\Programas\steamapps\workshop\content\294100\3426502333\1.6\Assemblies\VehicleMapFramework.dll

using HarmonyLib;
using RimWorld;
using SmashTools;
using System.Collections.Generic;
using UnityEngine;
using Verse;

#nullable disable
namespace VehicleMapFramework.VMF_HarmonyPatches;

[HarmonyPatchCategory("VMF_Patches_AdaptiveStorageFramework")]
[HarmonyPatch]
public static class Patch_ItemGraphicWorker_DrawOffsetForItem
{
  private static readonly AccessTools.FieldRef<object, int> stackBehaviour = AccessTools.FieldRefAccess<int>("AdaptiveStorage.ItemGraphic:stackBehaviour");
  private static readonly AccessTools.FieldRef<object, object> graphic = AccessTools.FieldRefAccess<object>("AdaptiveStorage.ItemGraphicWorker:<graphic>P");

  [PatchLevel(Level.Safe)]
  public static void Postfix(
    object __instance,
    Building_Storage building,
    Thing item,
    ref Vector3 __result)
  {
    if (!item.IsOnVehicleMapOf(out VehiclePawnWithMap _) || building == null || Patch_ItemGraphicWorker_DrawOffsetForItem.stackBehaviour.Invoke(Patch_ItemGraphicWorker_DrawOffsetForItem.graphic.Invoke(__instance)) == 1)
      return;
    Rot4 rotForPrint = VehicleSectionLayerManager.RotForPrint;
    float asAngle = ((Rot4) ref rotForPrint).AsAngle;
    Vector3 drawPos = ((Thing) building).DrawPos;
    Vector3 vector3;
    if (Rot4.op_Equality(VehicleSectionLayerManager.RotForPrint, Rot4.East))
    {
      IntVec3 position = item.Position;
      vector3 = Vector3Utility.RotatedBy(Vector3.op_Subtraction(((IntVec3) ref position).ToVector3Shifted(), drawPos), asAngle);
    }
    else if (Rot4.op_Equality(VehicleSectionLayerManager.RotForPrint, Rot4.West))
    {
      IntVec3 position = item.Position;
      vector3 = Vector3.op_Subtraction(((IntVec3) ref position).ToVector3Shifted(), drawPos);
    }
    else
      vector3 = Vector3.zero;
    Rot4 rot4 = ((Thing) building).RotationForPrint();
    if (!((Rot4) ref rot4).IsHorizontal)
    {
      if (Rot4.op_Equality(VehicleSectionLayerManager.RotForPrint, Rot4.East))
        vector3 = Vector3Utility.RotatedBy(vector3, -asAngle);
      if (Rot4.op_Equality(VehicleSectionLayerManager.RotForPrint, Rot4.West))
        vector3 = Vector3Utility.RotatedBy(vector3, asAngle);
    }
    __result = Ext_Math.RotatePoint(__result, vector3, asAngle);
  }

  [PatchLevel(Level.Cautious)]
  public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
  {
    return (IEnumerable<CodeInstruction>) instructions.MethodReplacer(MethodInfoCache.CachedMethodInfo.g_Thing_Rotation, MethodInfoCache.CachedMethodInfo.m_RotationForPrint);
  }
}
