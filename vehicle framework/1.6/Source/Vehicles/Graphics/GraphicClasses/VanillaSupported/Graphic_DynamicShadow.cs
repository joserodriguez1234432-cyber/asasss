// Decompiled with JetBrains decompiler
// Type: Vehicles.Graphic_DynamicShadow
// Assembly: Vehicles, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 81BFDC99-D8AD-42E1-8470-2F73E7836B4A
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\Vehicles.dll

using System;
using UnityEngine;
using Verse;

#nullable disable
namespace Vehicles;

public class Graphic_DynamicShadow : Graphic
{
  private readonly ShadowData shadowData;
  private Mesh shadowMesh;

  public Graphic_DynamicShadow(Texture2D texture, ShadowData shadowData)
  {
    this.shadowData = shadowData;
    this.shadowMesh = shadowData != null ? DynamicShadows.GetShadowMesh(texture, shadowData) : throw new ArgumentNullException(nameof (shadowData));
  }

  public virtual void DrawWorker(
    Vector3 loc,
    Rot4 rot,
    ThingDef thingDef,
    Thing thing,
    float extraRotation)
  {
    if (!Object.op_Inequality((Object) this.shadowMesh, (Object) null) || this.shadowData == null || Find.CurrentMap != null && GenGrid.InBounds(IntVec3Utility.ToIntVec3(loc), Find.CurrentMap) && Find.CurrentMap.roofGrid.Roofed(IntVec3Utility.ToIntVec3(loc)) || !DebugViewSettings.drawShadows)
      return;
    Vector3 vector3 = Vector3.op_Addition(loc, this.shadowData.offset);
    vector3.y = Altitudes.AltitudeFor((AltitudeLayer) 13);
    Graphics.DrawMesh(this.shadowMesh, vector3, ((Rot4) ref rot).AsQuat, MatBases.SunShadowFade, 0);
  }

  public virtual void Print(SectionLayer layer, Thing thing, float extraRotation)
  {
  }

  public virtual string ToString() => $"Graphic_DynamicShadow({this.shadowData})";
}
