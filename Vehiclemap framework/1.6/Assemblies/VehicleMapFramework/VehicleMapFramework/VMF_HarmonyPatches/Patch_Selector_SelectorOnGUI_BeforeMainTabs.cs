// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.VMF_HarmonyPatches.Patch_Selector_SelectorOnGUI_BeforeMainTabs
// Assembly: VehicleMapFramework, Version=1.6.562.0, Culture=neutral, PublicKeyToken=null
// MVID: 10A61882-945F-4CFC-9B06-CA8EEF5ADB36
// Assembly location: D:\Programas\steamapps\workshop\content\294100\3426502333\1.6\Assemblies\VehicleMapFramework.dll

using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using System;
using UnityEngine;
using Vehicles.World;
using Verse;

#nullable disable
namespace VehicleMapFramework.VMF_HarmonyPatches;

[HarmonyPatch(typeof (Selector), "SelectorOnGUI_BeforeMainTabs")]
[PatchLevel(Level.Safe)]
public static class Patch_Selector_SelectorOnGUI_BeforeMainTabs
{
  private const float SphereRadius = 100f;
  private const float Magnification = 0.048828125f;

  public static void Postfix(Selector __instance)
  {
    VehiclePawnWithMap vehicle;
    if (Event.current.type != null || Event.current.button != 1 || !Event.current.shift || !Find.CurrentMap.IsVehicleMapOf(out vehicle) || !(((Thing) vehicle).ParentHolder is VehicleCaravan parentHolder) || !((Caravan) parentHolder).IsPlayerControlled || !GenCollection.Empty<Pawn>(__instance.SelectedPawns))
      return;
    Rect rect = new Rect(Vector2.zero, Patch_Map_MapUpdate.MeshSize);
    if (!((Rect) ref rect).Contains(Vector2Utility.ToVector2(UI.MouseMapPosition())))
      return;
    float altitude = Patch_Selector_SelectorOnGUI_BeforeMainTabs.RootSizeToAltitude();
    Patch_Map_MapUpdate.JumpTo(((WorldObject) parentHolder).DrawPos, altitude);
    ((Component) Find.WorldCamera).transform.Translate(Vector2.op_Implicit(Patch_Selector_SelectorOnGUI_BeforeMainTabs.ScreenOffset()));
    Find.WorldSelector.ClearSelection();
    Find.WorldSelector.Select((WorldObject) parentHolder, false);
    Find.WorldSelector.WorldSelectorOnGUI();
    Event.current.Use();
  }

  private static Vector2 ScreenOffset()
  {
    return Vector2.op_Multiply(Vector2.op_Subtraction(Vector2Utility.ToVector2(((Component) Find.Camera).transform.position), Vector2.op_Division(Patch_Map_MapUpdate.MeshSize, 2f)), 25f / 512f);
  }

  private static float RootSizeToAltitude()
  {
    return (float) ((double) Find.CameraDriver.RootSize * (25.0 / 512.0) / (double) Mathf.Tan((float) ((double) Find.WorldCamera.fieldOfView * 0.5 * (Math.PI / 180.0))) + 100.0);
  }
}
