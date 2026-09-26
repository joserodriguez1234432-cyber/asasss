// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.DebugDrawHelper
// Assembly: VehicleMapFramework, Version=1.6.562.0, Culture=neutral, PublicKeyToken=null
// MVID: 10A61882-945F-4CFC-9B06-CA8EEF5ADB36
// Assembly location: D:\Programas\steamapps\workshop\content\294100\3426502333\1.6\Assemblies\VehicleMapFramework.dll

using HarmonyLib;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Vehicles;
using Verse;

#nullable disable
namespace VehicleMapFramework;

internal static class DebugDrawHelper
{
  private static readonly AccessTools.FieldRef<DebugCellDrawer, IList> debugCells = AccessTools.FieldRefAccess<DebugCellDrawer, IList>(nameof (debugCells));
  private static readonly Type t_DebugCell = GenTypes.GetTypeInAnyAssembly("Verse.DebugCell", "Verse");
  private static readonly Dictionary<string, DebugDrawHelper.DebugCellField> _fields = new Dictionary<string, DebugDrawHelper.DebugCellField>();
  private static int lastCameraUpdateFrame = -1;
  private static Bounds bounds;

  public static void DebugDraw(DebugCellDrawer drawer, Map map)
  {
    IList list = DebugDrawHelper.debugCells.Invoke(drawer);
    for (int index = 0; index < list.Count; ++index)
      DebugDrawHelper.Draw(list[index], map);
  }

  public static void DebugOnGUI(DebugCellDrawer drawer, Map map)
  {
    if (Find.CameraDriver.CurrentZoom != null)
      return;
    IList list = DebugDrawHelper.debugCells.Invoke(drawer);
    if (list.Count == 0)
      return;
    Text.Font = (GameFont) 0;
    Text.Anchor = (TextAnchor) 4;
    GUI.color = new Color(1f, 1f, 1f, 0.5f);
    for (int index = 0; index < list.Count; ++index)
      DebugDrawHelper.OnGUI(list[index], map);
    GUI.color = Color.white;
    Text.Anchor = (TextAnchor) 0;
  }

  private static void Draw(object debugCell, Map map)
  {
    IntVec3 fieldValue1 = debugCell.GetFieldValue<IntVec3>("c");
    Material fieldValue2 = debugCell.GetFieldValue<Material>("customMat");
    if (fieldValue2 != null)
      DebugDrawHelper.RenderCell(fieldValue1, fieldValue2, map);
    else
      DebugDrawHelper.RenderCell(fieldValue1, debugCell.GetFieldValue<float>("colorPct"), map);
  }

  private static void OnGUI(object debugCell, Map map)
  {
    string fieldValue1 = debugCell.GetFieldValue<string>("displayString");
    if (fieldValue1 == null)
      return;
    IntVec3 fieldValue2 = debugCell.GetFieldValue<IntVec3>("c");
    Vector3 original = ((IntVec3) ref fieldValue2).ToVector3Shifted();
    VehiclePawnWithMap vehicle;
    if (map.IsNonFocusedVehicleMapOf(out vehicle))
      original = original.ToBaseMapCoord(vehicle);
    Vector2 uiPosition = UI.MapToUIPosition(original);
    Rect rect1;
    // ISSUE: explicit constructor call
    ((Rect) ref rect1).\u002Ector(uiPosition.x - 20f, uiPosition.y - 20f, 40f, 40f);
    Rect rect2 = new Rect(0.0f, 0.0f, (float) UI.screenWidth, (float) UI.screenHeight);
    if (!((Rect) ref rect2).Overlaps(rect1))
      return;
    Widgets.Label(rect1, fieldValue1);
  }

  private static void InitFrame()
  {
    if (Time.frameCount == DebugDrawHelper.lastCameraUpdateFrame)
      return;
    CellRect currentViewRect = Find.CameraDriver.CurrentViewRect;
    DebugDrawHelper.bounds = ((CellRect) ref currentViewRect).ToBounds();
    DebugDrawHelper.lastCameraUpdateFrame = Time.frameCount;
  }

  private static Material MatFromColorPct(float colorPct, bool transparent)
  {
    return DebugMatsSpectrum.Mat(GenMath.PositiveMod(Mathf.RoundToInt(colorPct * 100f), 100), transparent);
  }

  public static void RenderCell(IntVec3 c, float colorPct, Map map)
  {
    DebugDrawHelper.RenderCell(c, DebugDrawHelper.MatFromColorPct(colorPct, true), map);
  }

  public static void RenderCell(IntVec3 c, Material mat, Map map)
  {
    DebugDrawHelper.InitFrame();
    Vector3 original = ((IntVec3) ref c).ToVector3Shifted();
    VehiclePawnWithMap vehicle;
    if (map.IsNonFocusedVehicleMapOf(out vehicle))
      original = original.ToBaseMapCoord(vehicle);
    if (!((Bounds) ref DebugDrawHelper.bounds).Contains(Vector3Utility.Yto0(original)))
      return;
    Graphics.DrawMesh(MeshPool.plane10, Vector3Utility.SetToAltitude(original, (AltitudeLayer) 39), Quaternion.AngleAxis(VehicleMapUtility.get_ExtraAngle((VehiclePawn) vehicle), Vector3.up), mat, 0);
  }

  private static T GetFieldValue<T>(this object debugCell, string name)
  {
    DebugDrawHelper.DebugCellField debugCellField;
    if (!DebugDrawHelper._fields.TryGetValue(name, out debugCellField))
      DebugDrawHelper._fields[name] = debugCellField = DebugDrawHelper.DebugCellField.Create<T>(name);
    return ((DebugDrawHelper.DebugCellField<T>) debugCellField).Accessor.Invoke(debugCell);
  }

  private abstract class DebugCellField
  {
    public static DebugDrawHelper.DebugCellField Create<T>(string name)
    {
      return (DebugDrawHelper.DebugCellField) new DebugDrawHelper.DebugCellField<T>(name);
    }
  }

  private class DebugCellField<T>(string name) : DebugDrawHelper.DebugCellField
  {
    public readonly AccessTools.FieldRef<object, T> Accessor = AccessTools.FieldRefAccess<T>(DebugDrawHelper.t_DebugCell, name);
  }
}
