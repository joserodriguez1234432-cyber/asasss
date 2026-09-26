// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.VMF_HarmonyPatches.Patch_BeautyDrawer_DrawBeautyAroundMouse
// Assembly: VehicleMapFramework, Version=1.6.562.0, Culture=neutral, PublicKeyToken=null
// MVID: 10A61882-945F-4CFC-9B06-CA8EEF5ADB36
// Assembly location: D:\Programas\steamapps\workshop\content\294100\3426502333\1.6\Assemblies\VehicleMapFramework.dll

using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using Verse;

#nullable disable
namespace VehicleMapFramework.VMF_HarmonyPatches;

[HarmonyPatch(typeof (BeautyDrawer), "DrawBeautyAroundMouse")]
public static class Patch_BeautyDrawer_DrawBeautyAroundMouse
{
  [PatchLevel(Level.Safe)]
  public static void Prefix(
    ref (sbyte, Command_FocusVehicleMap.FocusVehicle)? __state)
  {
    Patch_MouseoverReadout_MouseoverReadoutOnGUI.PrefixCommon(ref __state);
  }

  [PatchLevel(Level.Cautious)]
  public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
  {
    // ISSUE: reference to a compiler-generated field
    // ISSUE: reference to a compiler-generated field
    MethodInfo method1 = (Patch_BeautyDrawer_DrawBeautyAroundMouse.\u003C\u003EO.\u003C0\u003E__LabelDrawPosFor ?? (Patch_BeautyDrawer_DrawBeautyAroundMouse.\u003C\u003EO.\u003C0\u003E__LabelDrawPosFor = new Func<IntVec3, Vector2>(GenMapUI.LabelDrawPosFor))).Method;
    // ISSUE: reference to a compiler-generated field
    // ISSUE: reference to a compiler-generated field
    MethodInfo method2 = (Patch_BeautyDrawer_DrawBeautyAroundMouse.\u003C\u003EO.\u003C1\u003E__LabelDrawPosForOffset ?? (Patch_BeautyDrawer_DrawBeautyAroundMouse.\u003C\u003EO.\u003C1\u003E__LabelDrawPosForOffset = new Func<IntVec3, Vector2>(Patch_BeautyDrawer_DrawBeautyAroundMouse.LabelDrawPosForOffset))).Method;
    return (IEnumerable<CodeInstruction>) instructions.MethodReplacer(method1, method2);
  }

  private static Vector2 LabelDrawPosForOffset(IntVec3 center)
  {
    Vector2 vector2 = Vector2.op_Implicit(Vector3.op_Division(Find.Camera.WorldToScreenPoint(((IntVec3) ref center).ToVector3ShiftedWithAltitude((AltitudeLayer) 39).ToBaseMapCoord()), Prefs.UIScale));
    vector2.y = (float) UI.screenHeight - vector2.y;
    --vector2.y;
    return vector2;
  }

  [PatchLevel(Level.Safe)]
  public static void Finalizer(
    (sbyte, Command_FocusVehicleMap.FocusVehicle)? __state)
  {
    if (!__state.HasValue)
      return;
    Current.Game.currentMapIndex = __state.Value.Item1;
    __state.Value.Item2.Dispose();
  }
}
