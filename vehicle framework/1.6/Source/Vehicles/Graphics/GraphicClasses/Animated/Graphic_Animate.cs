// Decompiled with JetBrains decompiler
// Type: Vehicles.Graphic_Animate
// Assembly: Vehicles, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 81BFDC99-D8AD-42E1-8470-2F73E7836B4A
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\Vehicles.dll

using JetBrains.Annotations;
using SmashTools;
using UnityEngine;
using Verse;

#nullable disable
namespace Vehicles;

[PublicAPI]
public class Graphic_Animate : Graphic_Collection
{
  public int AnimationCount
  {
    get
    {
      Graphic[] subGraphics = this.subGraphics;
      return subGraphics == null ? 0 : subGraphics.Length;
    }
  }

  public virtual Material MatSingle => this.subGraphics[0].MatSingle;

  public virtual Graphic GetColoredVersion(Shader newShader, Color newColor, Color newColorTwo)
  {
    return GraphicDatabase.Get<Graphic_Animate>(((Graphic) this).path, newShader, ((Graphic) this).drawSize, newColor, newColorTwo, ((Graphic) this).data, (string) null);
  }

  public void DrawWorkerAnimated(Thing thing, int index, float extraRotation, bool rotatePoints = false)
  {
    Mesh mesh = ((Graphic) this).MeshAt(thing.Rotation);
    Quaternion quaternion = ((Graphic) this).QuatFromRot(thing.Rotation);
    if ((double) extraRotation != 0.0)
      quaternion = Quaternion.op_Multiply(quaternion, Quaternion.Euler(Vector3.op_Multiply(Vector3.up, extraRotation)));
    Vector3 point = ((Graphic) this).DrawOffset(thing.Rotation);
    if (rotatePoints && (double) extraRotation != 0.0)
      point = Ext_Math.RotatePoint(point, Vector3.zero, extraRotation);
    Vector3 vector3 = Vector3.op_Addition(thing.DrawPos, point);
    Material matSingle = this.SubGraphicForIndex(index).MatSingle;
    ((Graphic) this).DrawMeshInt(mesh, vector3, quaternion, matSingle);
    ((Graphic) ((Graphic) this).ShadowGraphic)?.DrawWorker(vector3, thing.Rotation, (ThingDef) null, (Thing) null, extraRotation);
  }

  public void DrawWorkerAnimated(
    Vector3 loc,
    Rot4 rot,
    int index,
    float extraRotation,
    bool rotatePoints = false)
  {
    Mesh mesh = ((Graphic) this).MeshAt(rot);
    Vector3 vector3_1 = loc;
    Quaternion quaternion = ((Graphic) this).QuatFromRot(rot);
    if ((double) extraRotation != 0.0)
      quaternion = Quaternion.op_Multiply(quaternion, Quaternion.Euler(Vector3.op_Multiply(Vector3.up, extraRotation)));
    Vector3 point = ((Graphic) this).DrawOffset(rot);
    if (rotatePoints && (double) extraRotation != 0.0)
      point = Ext_Math.RotatePoint(point, Vector3.zero, extraRotation);
    Vector3 vector3_2 = Vector3.op_Addition(vector3_1, point);
    Material matSingle = this.SubGraphicForIndex(index).MatSingle;
    ((Graphic) this).DrawMeshInt(mesh, vector3_2, quaternion, matSingle);
    if (((Graphic) this).ShadowGraphic == null)
      return;
    ((Graphic) ((Graphic) this).ShadowGraphic).DrawWorker(vector3_2, rot, (ThingDef) null, (Thing) null, extraRotation);
  }

  public virtual Material MatAt(Rot4 rot, Thing thing = null)
  {
    return thing == null ? ((Graphic) this).MatSingle : ((Graphic) this).MatSingleFor(thing);
  }

  public virtual Material MatAt(Rot4 rot, int index) => this.SubGraphicForIndex(index).MatSingle;

  public Graphic SubGraphicForIndex(int index)
  {
    index %= this.subGraphics.Length;
    return this.subGraphics[index];
  }
}
