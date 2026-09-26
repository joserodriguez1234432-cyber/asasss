// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.Graphic_LinkedCornerOverlaySingle
// Assembly: VehicleMapFramework, Version=1.6.562.0, Culture=neutral, PublicKeyToken=null
// MVID: 10A61882-945F-4CFC-9B06-CA8EEF5ADB36
// Assembly location: D:\Programas\steamapps\workshop\content\294100\3426502333\1.6\Assemblies\VehicleMapFramework.dll

using RimWorld;
using UnityEngine;
using Verse;

#nullable disable
namespace VehicleMapFramework;

public class Graphic_LinkedCornerOverlaySingle : Graphic_Linked
{
  public readonly Graphic_Single overlayGraphic;

  public Graphic_LinkedCornerOverlaySingle(Graphic subGraphic)
    : base(subGraphic)
  {
    this.subGraphic = subGraphic;
    ((Graphic) this).data = subGraphic.data;
    this.overlayGraphic = GraphicDatabase.Get<Graphic_Single>(((Graphic) this).data.cornerOverlayPath, ((Graphic) this).Shader, ((Graphic) this).drawSize, subGraphic.color) as Graphic_Single;
  }

  public virtual void Print(SectionLayer layer, Thing thing, float extraRotation)
  {
    base.Print(layer, thing, extraRotation);
    IntVec3 position = thing.Position;
    if (!this.ShouldLinkWith(IntVec3.op_Addition(position, IntVec3.East), thing) || !this.ShouldLinkWith(IntVec3.op_Addition(position, IntVec3.North), thing) || !this.ShouldLinkWith(IntVec3.op_Addition(position, IntVec3.NorthEast), thing))
      return;
    Material material = ((Graphic) this.overlayGraphic).MatSingleFor(thing);
    Vector2[] vector2Array;
    Color32 color32;
    Graphic.TryGetTextureAtlasReplacementInfo(material, (TextureAtlasGroup) 3, false, false, ref material, ref vector2Array, ref color32);
    Rot4 rotForPrint = VehicleSectionLayerManager.RotForPrint;
    float num = -((Rot4) ref rotForPrint).AsAngle;
    Printer_Plane.PrintPlane((MapDrawLayer) layer, Vector3.op_Addition(GenThing.TrueCenter(thing), VehicleMapUtility.RotateForPrintNegate(new Vector3(0.5f, 0.1f, 0.5f))), Vector2.op_Implicit(Vector3.one), material, num, false, vector2Array, (Color32[]) null, 0.01f, 0.0f);
  }

  public virtual Graphic GetColoredVersion(Shader newShader, Color newColor, Color newColorTwo)
  {
    Graphic_LinkedCornerOverlaySingle coloredVersion = new Graphic_LinkedCornerOverlaySingle(this.subGraphic.GetColoredVersion(newShader, newColor, newColorTwo));
    ((Graphic) coloredVersion).data = ((Graphic) this).data;
    return (Graphic) coloredVersion;
  }
}
