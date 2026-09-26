// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.VMF_HarmonyPatches.Patch_ColonistBarColonistDrawer_DrawGroupFrame
// Assembly: VehicleMapFramework, Version=1.6.562.0, Culture=neutral, PublicKeyToken=null
// MVID: 10A61882-945F-4CFC-9B06-CA8EEF5ADB36
// Assembly location: D:\Programas\steamapps\workshop\content\294100\3426502333\1.6\Assemblies\VehicleMapFramework.dll

using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using UnityEngine;
using Vehicles.Rendering;
using Verse;

#nullable disable
namespace VehicleMapFramework.VMF_HarmonyPatches;

[HarmonyPatch(typeof (ColonistBarColonistDrawer), "DrawGroupFrame")]
[PatchLevel(Level.Safe)]
public static class Patch_ColonistBarColonistDrawer_DrawGroupFrame
{
  public static void Postfix(int group)
  {
    VehicleMapSettings.ShowVehiclesOnColonistBar colonistBarMode = VehicleMapFramework.VehicleMapFramework.settings.colonistBarMode;
    if (colonistBarMode == VehicleMapSettings.ShowVehiclesOnColonistBar.DontShow)
      return;
    ColonistBar colonistBar = Find.ColonistBar;
    Map map = (Map) null;
    foreach (ColonistBar.Entry entry in colonistBar.Entries)
    {
      if (entry.group == group)
      {
        map = entry.map;
        break;
      }
    }
    VehiclePawnWithMap vehicle;
    if (!map.IsVehicleMapOf(out vehicle))
      return;
    Rect rect1 = GroupFrameRect();
    if (colonistBarMode == VehicleMapSettings.ShowVehiclesOnColonistBar.MouseIsOver && !((Rect) ref rect1).Contains(Event.current.mousePosition))
      return;
    Rect rect2 = GenUI.CenteredOnXIn(new Rect(0.0f, ((Rect) ref rect1).yMax - 5f, 50f, 50f), rect1);
    BlitRequest blitRequest = BlitRequest.For(vehicle.VehicleDef);
    ref BlitRequest local = ref blitRequest;
    VehicleGui.DrawVehicleOnGUI(rect2, ref local, 1f, false);

    Rect GroupFrameRect()
    {
      float num1 = 99999f;
      float num2 = 0.0f;
      float num3 = 0.0f;
      List<ColonistBar.Entry> entries = colonistBar.Entries;
      List<Vector2> drawLocs = colonistBar.DrawLocs;
      for (int index = 0; index < entries.Count; ++index)
      {
        if (entries[index].group == group)
        {
          num1 = Mathf.Min(num1, drawLocs[index].x);
          num2 = Mathf.Max(num2, drawLocs[index].x + colonistBar.Size.x);
          num3 = Mathf.Max(num3, drawLocs[index].y + colonistBar.Size.y);
        }
      }
      return GenUI.ContractedBy(new Rect(num1, 0.0f, num2 - num1, num3 - 0.0f), -12f * colonistBar.Scale);
    }
  }
}
