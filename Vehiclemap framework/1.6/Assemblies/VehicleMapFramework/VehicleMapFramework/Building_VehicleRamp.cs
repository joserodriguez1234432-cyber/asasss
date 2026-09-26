// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.Building_VehicleRamp
// Assembly: VehicleMapFramework, Version=1.6.562.0, Culture=neutral, PublicKeyToken=null
// MVID: 10A61882-945F-4CFC-9B06-CA8EEF5ADB36
// Assembly location: D:\Programas\steamapps\workshop\content\294100\3426502333\1.6\Assemblies\VehicleMapFramework.dll

using RimWorld;
using SmashTools;
using UnityEngine;
using Verse;

#nullable disable
namespace VehicleMapFramework;

public class Building_VehicleRamp : Building_Door
{
  protected virtual void DrawAt(Vector3 drawLoc, bool flip = false)
  {
    VehiclePawnWithMap vehicle1;
    if (((Thing) this).IsOnVehicleMapOf(out vehicle1) && Color.op_Inequality(((Thing) vehicle1).DrawColor, ((Thing) this).DrawColor))
      ((Thing) this).DrawColor = ((Thing) vehicle1).DrawColor;
    Rot8 rot8 = ((Thing) this).BaseFullRotation();
    VehiclePawnWithMap vehicle2;
    VehicleMapProps modExtension;
    if (ThingCompUtility.HasComp<CompVehicleEnterSpot>((Thing) this) && ((Thing) this).IsOnNonFocusedVehicleMapOf(out vehicle2) && (modExtension = ((Def) vehicle2.VehicleDef).GetModExtension<VehicleMapProps>()) != null)
    {
      Vector3 vector3_1 = drawLoc;
      Rot8 opposite1 = ((Rot8) ref rot8).Opposite;
      Vector3 vector3_2 = Vector2Utility.ToVector3(((Rot8) ref opposite1).AsVector2);
      VehicleMapProps vehicleMapProps = modExtension;
      Rot8 fullRotation = vehicle2.FullRotation;
      Rot4 rotation = ((Thing) this).Rotation;
      Rot4 opposite2 = ((Rot4) ref rotation).Opposite;
      double num = (double) vehicleMapProps.EdgeSpaceValue(fullRotation, opposite2);
      Vector3 vector3_3 = Vector3.op_Multiply(vector3_2, (float) num);
      drawLoc = Vector3.op_Addition(vector3_1, vector3_3);
    }
    Graphic coloredVersion = ((Thing) this).def.building.upperMoverGraphic.Graphic.GetColoredVersion(ShaderDatabase.Cutout, ((Thing) this).DrawColor, ((Thing) this).DrawColorTwo);
    float openPct = this.OpenPct;
    IntVec3 facingCell = ((Rot8) ref rot8).FacingCell;
    Vector3 vector3_4 = ((IntVec3) ref facingCell).ToVector3();
    Vector3 vector3_5 = ((Rot8) ref rot8).IsDiagonal ? Vector3.op_Multiply(vector3_4, 0.353553385f) : Vector3.op_Division(vector3_4, 2f);
    Vector3 vector3_6 = Vector3.op_Subtraction(Vector3.op_Subtraction(drawLoc, vector3_5), Vector3.op_Multiply(vector3_5, openPct));
    vector3_4.x = -vector3_4.x;
    if (((Rot8) ref rot8).IsDiagonal)
      vector3_6 = Vector3.op_Addition(vector3_6, Vector3.op_Multiply((double) vector3_4.z < 0.0 ? vector3_4 : Vector3.op_UnaryNegation(vector3_4), 0.353553385f));
    else if (((Rot8) ref rot8).IsHorizontal)
      vector3_6.z -= 0.5f;
    Vector3 vector3_7 = Rot8.op_Equality(rot8, Rot8.North) || Rot8.op_Equality(rot8, Rot8.South) ? new Vector3(1f, 1f, openPct) : new Vector3(openPct, 1f, 1f);
    Graphics.DrawMesh(coloredVersion.MeshAt(Rot8.op_Implicit(rot8)), Matrix4x4.TRS(vector3_6, Quaternion.AngleAxis(((Rot8) ref rot8).AsRotationAngle + (vehicle1 != null ? vehicle1.Transform.rotation : 0.0f), Vector3.up), vector3_7), coloredVersion.MatAt(Rot8.op_Implicit(rot8), (Thing) this), 0);
  }
}
