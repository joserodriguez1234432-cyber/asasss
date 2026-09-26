// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.PawnOverlayRendererUtility
// Assembly: VehicleMapFramework, Version=1.6.562.0, Culture=neutral, PublicKeyToken=null
// MVID: 10A61882-945F-4CFC-9B06-CA8EEF5ADB36
// Assembly location: D:\Programas\steamapps\workshop\content\294100\3426502333\1.6\Assemblies\VehicleMapFramework.dll

using RimWorld;
using SmashTools;
using UnityEngine;
using Vehicles;
using Verse;

#nullable disable
namespace VehicleMapFramework;

public static class PawnOverlayRendererUtility
{
  public static void SetDrawOffsets(
    this PawnOverlayRenderer renderer,
    VehiclePawnWithMap vehicle,
    VehicleRoleBuildable role)
  {
    ThingWithComps parent = role.upgradeComp.parent;
    int num = VehicleSectionLayerManager.CacheMode ? 1 : 0;
    VehicleSectionLayerManager.CacheMode = true;
    Vector3 original = GenThing.TrueCenter(((Thing) parent).Position, ((Thing) parent).Rotation, ((BuildableDef) ((Thing) parent).def).Size, 0.0f);
    VehicleSectionLayerManager.CacheMode = num != 0;
    Vector3 cachedDrawPos = vehicle.cachedDrawPos;
    Rot4 rotation = ((Thing) parent).Rotation;
    Rot8 rot = Rot8.op_Implicit(rotation);
    GraphicDataRGB graphicData = vehicle.VehicleDef.graphicData;
    PawnOverlayRenderer sourceRenderer = role.sourceRenderer;
    renderer.drawOffset = Vector3.op_Addition(Vector3.op_Addition(Vector3.op_Subtraction(original.ToBaseMapCoord(vehicle, Rot8.North), cachedDrawPos), ((GraphicData) graphicData).DrawOffsetForRot(Rot4.North)), sourceRenderer.drawOffset);
    renderer.drawOffsetNorth = new Vector3?(Vector3.op_Addition(Vector3.op_Addition(Vector3.op_Subtraction(original.ToBaseMapCoord(vehicle, Rot8.North), cachedDrawPos), ((GraphicData) graphicData).DrawOffsetForRot(Rot4.North)), sourceRenderer.DrawOffsetFor(Rot8.op_Implicit(new Rot4(((Rot4) ref rotation).AsInt)))));
    renderer.drawOffsetSouth = new Vector3?(Vector3.op_Addition(Vector3.op_Addition(Vector3.op_Subtraction(original.ToBaseMapCoord(vehicle, Rot8.South), cachedDrawPos), ((GraphicData) graphicData).DrawOffsetForRot(Rot4.South)), sourceRenderer.DrawOffsetFor(Rot8.op_Implicit(new Rot4(2 + ((Rot4) ref rotation).AsInt)))));
    renderer.drawOffsetEast = new Vector3?(Vector3.op_Addition(Vector3.op_Addition(Vector3.op_Subtraction(original.ToBaseMapCoord(vehicle, Rot8.East), cachedDrawPos), ((GraphicData) graphicData).DrawOffsetForRot(Rot4.East)), sourceRenderer.DrawOffsetFor(Rot8.op_Implicit(new Rot4(1 + ((Rot4) ref rotation).AsInt)))));
    renderer.drawOffsetWest = new Vector3?(Vector3.op_Addition(Vector3.op_Addition(Vector3.op_Subtraction(original.ToBaseMapCoord(vehicle, Rot8.West), cachedDrawPos), ((GraphicData) graphicData).DrawOffsetForRot(Rot4.West)), sourceRenderer.DrawOffsetFor(Rot8.op_Implicit(new Rot4(3 + ((Rot4) ref rotation).AsInt)))));
    renderer.drawOffsetNorthEast = new Vector3?(Vector3.op_Addition(Vector3.op_Addition(Vector3.op_Subtraction(original.ToBaseMapCoord(vehicle, Rot8.NorthEast), cachedDrawPos), Vector3Utility.RotatedBy(((GraphicData) graphicData).DrawOffsetForRot(Rot4.North), 45f)), ((Rot4) ref rotation).IsHorizontal ? Vector3Utility.RotatedBy(sourceRenderer.DrawOffsetFor(Rot8.op_Implicit(rotation)), 45f) : sourceRenderer.DrawOffsetFor(rot.Rotated(Rot8.NorthEast))));
    renderer.drawOffsetNorthWest = new Vector3?(Vector3.op_Addition(Vector3.op_Addition(Vector3.op_Subtraction(original.ToBaseMapCoord(vehicle, Rot8.NorthWest), cachedDrawPos), Vector3Utility.RotatedBy(((GraphicData) graphicData).DrawOffsetForRot(Rot4.North), -45f)), ((Rot4) ref rotation).IsHorizontal ? Vector3Utility.RotatedBy(sourceRenderer.DrawOffsetFor(Rot8.op_Implicit(rotation)), -45f) : sourceRenderer.DrawOffsetFor(rot.Rotated(Rot8.NorthWest))));
    renderer.drawOffsetSouthEast = new Vector3?(Vector3.op_Addition(Vector3.op_Addition(Vector3.op_Subtraction(original.ToBaseMapCoord(vehicle, Rot8.SouthEast), cachedDrawPos), Vector3Utility.RotatedBy(((GraphicData) graphicData).DrawOffsetForRot(Rot4.South), -45f)), ((Rot4) ref rotation).IsHorizontal ? Vector3Utility.RotatedBy(sourceRenderer.DrawOffsetFor(Rot8.op_Implicit(((Rot4) ref rotation).Opposite)), -45f) : sourceRenderer.DrawOffsetFor(rot.Rotated(Rot8.SouthEast))));
    renderer.drawOffsetSouthWest = new Vector3?(Vector3.op_Addition(Vector3.op_Addition(Vector3.op_Subtraction(original.ToBaseMapCoord(vehicle, Rot8.SouthWest), cachedDrawPos), Vector3Utility.RotatedBy(((GraphicData) graphicData).DrawOffsetForRot(Rot4.South), 45f)), ((Rot4) ref rotation).IsHorizontal ? Vector3Utility.RotatedBy(sourceRenderer.DrawOffsetFor(Rot8.op_Implicit(((Rot4) ref rotation).Opposite)), 45f) : sourceRenderer.DrawOffsetFor(rot.Rotated(Rot8.SouthWest))));
  }
}
