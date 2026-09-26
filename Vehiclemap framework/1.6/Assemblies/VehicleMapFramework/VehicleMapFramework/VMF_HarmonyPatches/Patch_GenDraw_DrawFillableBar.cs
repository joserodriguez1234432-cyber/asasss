// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.VMF_HarmonyPatches.Patch_GenDraw_DrawFillableBar
// Assembly: VehicleMapFramework, Version=1.6.562.0, Culture=neutral, PublicKeyToken=null
// MVID: 10A61882-945F-4CFC-9B06-CA8EEF5ADB36
// Assembly location: D:\Programas\steamapps\workshop\content\294100\3426502333\1.6\Assemblies\VehicleMapFramework.dll

using HarmonyLib;
using SmashTools;
using UnityEngine;
using Verse;

#nullable disable
namespace VehicleMapFramework.VMF_HarmonyPatches;

[HarmonyPatch(typeof (GenDraw), "DrawFillableBar")]
[PatchLevel(Level.Safe)]
public static class Patch_GenDraw_DrawFillableBar
{
  public static bool Prefix(GenDraw.FillableBarRequest r)
  {
    VehiclePawnWithMap vehicle = (VehiclePawnWithMap) null;
    if (((Rot4) ref r.rotation).AsInt < 4 && !Find.CurrentMap.IsNonFocusedVehicleMapOf(out vehicle))
      return true;
    float num1 = vehicle != null ? vehicle.Transform.rotation : 0.0f;
    Rot8 rot;
    // ISSUE: explicit constructor call
    ((Rot8) ref rot).\u002Ector(((Rot4) ref r.rotation).AsInt);
    Rot8 opposite = ((Rot8) ref rot).Opposite;
    float num2 = ((Rot8) ref opposite).AsAngle + num1;
    Vector2 vector2 = Vector2Utility.RotatedBy(r.preRotationOffset, num2);
    ref Vector3 local = ref r.center;
    local = Vector3.op_Addition(local, new Vector3(vector2.x, 0.0f, vector2.y));
    Vector3 vector3_1;
    // ISSUE: explicit constructor call
    ((Vector3) ref vector3_1).\u002Ector(r.size.x + r.margin, 1f, r.size.y + r.margin);
    Matrix4x4 matrix4x4_1 = new Matrix4x4();
    Quaternion quaternion = Quaternion.op_Multiply(rot.AsQuat(), Quaternion.AngleAxis(num1, Vector3.up));
    ((Matrix4x4) ref matrix4x4_1).SetTRS(r.center, quaternion, vector3_1);
    Graphics.DrawMesh(MeshPool.plane10, matrix4x4_1, r.unfilledMat, 0);
    if ((double) r.fillPercent > 1.0 / 1000.0)
    {
      // ISSUE: explicit constructor call
      ((Vector3) ref vector3_1).\u002Ector(r.size.x * r.fillPercent, 1f, r.size.y);
      Matrix4x4 matrix4x4_2 = new Matrix4x4();
      Vector3 vector3_2 = Vector3.op_Addition(Vector3.op_Addition(r.center, Vector3.op_Multiply(Vector3.up, 0.01f)), Vector3Utility.RotatedBy(new Vector3((float) (-(double) r.size.x * 0.5 + 0.5 * (double) r.size.x * (double) r.fillPercent), 0.0f, 0.0f), num2));
      ((Matrix4x4) ref matrix4x4_2).SetTRS(vector3_2, quaternion, vector3_1);
      Graphics.DrawMesh(MeshPool.plane10, matrix4x4_2, r.filledMat, 0);
    }
    return false;
  }
}
