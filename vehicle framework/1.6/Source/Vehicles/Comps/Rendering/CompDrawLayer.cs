// Decompiled with JetBrains decompiler
// Type: Vehicles.CompDrawLayer
// Assembly: Vehicles, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 81BFDC99-D8AD-42E1-8470-2F73E7836B4A
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\Vehicles.dll

using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;

#nullable disable
namespace Vehicles;

[Obsolete]
public class CompDrawLayer : ThingComp
{
  private List<Graphic> graphicInt;
  private float extraRotation;

  public CompProperties_DrawLayer Props => this.props as CompProperties_DrawLayer;

  public List<Graphic> GraphicList
  {
    get
    {
      if (GenList.NullOrEmpty<Graphic>((IList<Graphic>) this.graphicInt))
      {
        this.graphicInt = new List<Graphic>();
        foreach (GraphicData graphicData in this.Props.graphicDatas)
          this.graphicInt.Add(graphicData.Graphic.GetColoredVersion(graphicData.Graphic.Shader, graphicData.color, graphicData.colorTwo));
      }
      return this.graphicInt;
    }
  }

  protected float AngleFromRot(Graphic graphic)
  {
    if (!graphic.ShouldDrawRotated)
      return 0.0f;
    Rot4 rotation = ((Thing) this.parent).Rotation;
    float num = ((Rot4) ref rotation).AsAngle + graphic.DrawRotatedExtraAngleOffset;
    if (Rot4.op_Equality(((Thing) this.parent).Rotation, Rot4.West) && graphic.WestFlipped || Rot4.op_Equality(((Thing) this.parent).Rotation, Rot4.East) && graphic.EastFlipped)
      num += 180f;
    return num;
  }

  protected Quaternion QuatFromRot(Graphic graphic)
  {
    float num = this.AngleFromRot(graphic);
    return (double) num == 0.0 ? Quaternion.identity : Quaternion.AngleAxis(num, Vector3.up);
  }

  public virtual void PostDraw()
  {
    foreach (Graphic graphic in this.GraphicList)
    {
      Mesh mesh = graphic.MeshAt(((Thing) this.parent).Rotation);
      Quaternion quaternion = this.QuatFromRot(graphic);
      Vector3 drawPos = ((Thing) this.parent).DrawPos;
      if ((double) this.extraRotation != 0.0)
        quaternion = Quaternion.op_Multiply(quaternion, Quaternion.Euler(Vector3.op_Multiply(Vector3.up, this.extraRotation)));
      Vector3 vector3 = Vector3.op_Addition(drawPos, graphic.DrawOffset(((Thing) this.parent).Rotation));
      Material material = graphic.MatAt(((Thing) this.parent).Rotation, (Thing) null);
      Graphics.DrawMesh(mesh, vector3, quaternion, material, 0);
      if (graphic.ShadowGraphic != null)
        ((Graphic) graphic.ShadowGraphic).DrawWorker(vector3, ((Thing) this.parent).Rotation, ((Thing) this.parent).def, (Thing) null, this.extraRotation);
    }
  }
}
