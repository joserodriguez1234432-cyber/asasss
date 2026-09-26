// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.VehicleMapBlitter
// Assembly: VehicleMapFramework, Version=1.6.562.0, Culture=neutral, PublicKeyToken=null
// MVID: 10A61882-945F-4CFC-9B06-CA8EEF5ADB36
// Assembly location: D:\Programas\steamapps\workshop\content\294100\3426502333\1.6\Assemblies\VehicleMapFramework.dll

using SmashTools;
using SmashTools.Rendering;
using System;
using System.Collections.Generic;
using UnityEngine;
using Vehicles;
using Vehicles.Rendering;
using Verse;

#nullable disable
namespace VehicleMapFramework;

[StaticConstructorOnStartup]
public class VehicleMapBlitter(VehiclePawnWithMap vehicle) : IBlitTarget
{
  private static Material defaultMat;

  static VehicleMapBlitter()
  {
    LongEventHandler.ExecuteWhenFinished((Action) (() => VehicleMapBlitter.defaultMat = new Material(ShaderDatabase.CutoutComplexUI)));
  }

  (int width, int height) IBlitTarget.TextureSize(in BlitRequest request)
  {
    if (vehicle.VehicleMap == null)
      return (0, 0);
    IntVec3 mapSize = vehicle.MapSize;
    int num = Mathf.Max(mapSize.x, mapSize.z);
    return (num * 64 /*0x40*/, num * 64 /*0x40*/);
  }

  IEnumerable<RenderData> IBlitTarget.GetRenderData(Rect rect, BlitRequest request)
  {
    // ISSUE: reference to a compiler-generated field
    int num = this.\u003C\u003E1__state;
    VehicleMapBlitter vehicleMapBlitter = this;
    if (num != 0)
    {
      if (num != 1)
        return false;
      // ISSUE: reference to a compiler-generated field
      this.\u003C\u003E1__state = -1;
      return false;
    }
    // ISSUE: reference to a compiler-generated field
    this.\u003C\u003E1__state = -1;
    (int, int) tuple = ((IBlitTarget) vehicleMapBlitter).TextureSize(ref request);
    Texture vehicleMapTexture = VehicleMapUIRenderer.GetVehicleMapTexture(vehicle, request.rot.RotForVehicleDraw(), (tuple.Item1, tuple.Item2));
    VehicleMapBlitter.defaultMat.mainTexture = vehicleMapTexture;
    // ISSUE: reference to a compiler-generated field
    this.\u003C\u003E2__current = new RenderData(vehicleMapBlitter.GetRenderRect(rect, request), vehicleMapTexture, VehicleMapBlitter.defaultMat, (MaterialPropertyBlock) null, 0.1f, 0.0f);
    // ISSUE: reference to a compiler-generated field
    this.\u003C\u003E1__state = 1;
    return true;
  }

  public Rect GetRenderRect(Rect parentRect, BlitRequest request, bool fitToValidRect = false)
  {
    Vector2 vector2_1 = vehicle.VehicleDef.ScaleDrawRatio(((Rect) ref parentRect).size);
    float num1 = Mathf.Max(vector2_1.x, vector2_1.y) / Mathf.Max(((GraphicData) vehicle.VehicleDef.graphicData).drawSize.x, ((GraphicData) vehicle.VehicleDef.graphicData).drawSize.y);
    CellRect cellRect;
    IntVec2 intVec2_1;
    if (!fitToValidRect)
    {
      IntVec3 mapSize = vehicle.MapSize;
      intVec2_1 = ((IntVec3) ref mapSize).ToIntVec2;
    }
    else
    {
      CellRect validMapRect = vehicle.ValidMapRect;
      cellRect = ((CellRect) ref validMapRect).ExpandedBy(1);
      intVec2_1 = ((CellRect) ref cellRect).Size;
    }
    IntVec2 intVec2_2 = intVec2_1;
    float num2 = (float) Mathf.Max(intVec2_2.x, intVec2_2.z);
    Vector2 vector2_2;
    // ISSUE: explicit constructor call
    ((Vector2) ref vector2_2).\u002Ector(num2 * num1, num2 * num1);
    int num3 = ((Rot8) ref request.rot).IsHorizontal ? 1 : (((Rot8) ref request.rot).IsDiagonal ? 1 : 0);
    GraphicDataRGB graphicData = vehicle.VehicleDef.graphicData;
    Vector2 vector2_3;
    // ISSUE: explicit constructor call
    ((Vector2) ref vector2_3).\u002Ector(((GraphicData) graphicData).drawSize.x, ((GraphicData) graphicData).drawSize.y);
    if (num3 != 0)
    {
      ref float local1 = ref vector2_3.x;
      ref float local2 = ref vector2_3.y;
      float y = vector2_3.y;
      float x = vector2_3.x;
      local1 = y;
      double num4 = (double) x;
      local2 = (float) num4;
    }
    Vector2 vector2_4;
    // ISSUE: explicit constructor call
    ((Vector2) ref vector2_4).\u002Ector(vector2_1.x / vector2_3.x, vector2_1.y / vector2_3.y);
    Vector3 vector3_1 = ((GraphicData) graphicData).DrawOffsetForRot(Rot8.op_Implicit(request.rot));
    Vector2 vector2_5;
    // ISSUE: explicit constructor call
    ((Vector2) ref vector2_5).\u002Ector(vector3_1.x * vector2_4.x, -vector3_1.z * vector2_4.y);
    Vector3 vector3_2 = vehicle.VehicleDef.drawProperties.DisplayOffsetForRot(Rot8.op_Implicit(request.rot));
    Vector2 vector2_6;
    // ISSUE: explicit constructor call
    ((Vector2) ref vector2_6).\u002Ector(((Rect) ref parentRect).center.x + vector3_2.x * ((Rect) ref parentRect).width, ((Rect) ref parentRect).center.y + vector3_2.y * ((Rect) ref parentRect).height);
    Vector3 vector3_3 = VehicleMapUtility.OffsetFor(vehicle, request.rot);
    if (fitToValidRect)
    {
      Vector3 vector3_4 = vector3_3;
      cellRect = vehicle.ValidMapRect;
      Vector3 centerVector3_1 = ((CellRect) ref cellRect).CenterVector3;
      cellRect = vehicle.MapRect;
      Vector3 centerVector3_2 = ((CellRect) ref cellRect).CenterVector3;
      Vector3 vector3_5 = Vector3Utility.RotatedBy(Vector3.op_Subtraction(centerVector3_1, centerVector3_2), Rot8.op_Implicit(request.rot));
      vector3_3 = Vector3.op_Addition(vector3_4, vector3_5);
    }
    Vector2 vector2_7;
    // ISSUE: explicit constructor call
    ((Vector2) ref vector2_7).\u002Ector(vector2_6.x + vector3_3.x * num1 + vector2_5.x, vector2_6.y + -vector3_3.z * num1 + vector2_5.y);
    Rect renderRect;
    // ISSUE: explicit constructor call
    ((Rect) ref renderRect).\u002Ector(Vector2.zero, vector2_2);
    ((Rect) ref renderRect).center = vector2_7;
    return renderRect;
  }
}
