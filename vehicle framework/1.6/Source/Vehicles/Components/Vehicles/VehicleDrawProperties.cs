// Decompiled with JetBrains decompiler
// Type: Vehicles.VehicleDrawProperties
// Assembly: Vehicles, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 81BFDC99-D8AD-42E1-8470-2F73E7836B4A
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\Vehicles.dll

using JetBrains.Annotations;
using SmashTools;
using SmashTools.Animations;
using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;

#nullable disable
namespace Vehicles;

[PublicAPI]
[HeaderTitle(Label = "VehicleDrawProperties")]
public class VehicleDrawProperties
{
  public Rot8 displayRotation = Rot8.East;
  [TweakField(SettingsType = UISettingsType.FloatBox)]
  public Vector2 displayOffset = Vector2.zero;
  [TweakField(SettingsType = UISettingsType.FloatBox)]
  public Vector2? displayOffsetNorth;
  [TweakField(SettingsType = UISettingsType.FloatBox)]
  public Vector2? displayOffsetEast;
  [TweakField(SettingsType = UISettingsType.FloatBox)]
  public Vector2? displayOffsetSouth;
  [TweakField(SettingsType = UISettingsType.FloatBox)]
  public Vector2? displayOffsetWest;
  public string loadCargoTexPath = string.Empty;
  public string cancelCargoTexPath = string.Empty;
  public List<GraphicDataOverlay> graphicOverlays = new List<GraphicDataOverlay>();
  public AnimationController controller;
  [Unsaved(false)]
  public readonly List<GraphicOverlay> overlays = new List<GraphicOverlay>();

  public void PostDefDatabase(VehicleDef vehicleDef)
  {
    LongEventHandler.ExecuteWhenFinished((Action) (() =>
    {
      foreach (GraphicDataOverlay graphicOverlay1 in this.graphicOverlays)
      {
        GraphicOverlay graphicOverlay2 = GraphicOverlay.Create(graphicOverlay1, vehicleDef);
        graphicOverlay2.data.graphicData.RecacheLayerOffsets();
        this.overlays.Add(graphicOverlay2);
      }
    }));
  }

  public Vector3 DisplayOffsetForRot(Rot4 rot)
  {
    switch (((Rot4) ref rot).AsInt)
    {
      case 0:
        Vector2? displayOffsetNorth = this.displayOffsetNorth;
        return (displayOffsetNorth.HasValue ? new Vector3?(Vector2.op_Implicit(displayOffsetNorth.GetValueOrDefault())) : new Vector3?()) ?? Vector2.op_Implicit(this.displayOffset);
      case 1:
        Vector2? displayOffsetEast = this.displayOffsetEast;
        return (displayOffsetEast.HasValue ? new Vector3?(Vector2.op_Implicit(displayOffsetEast.GetValueOrDefault())) : new Vector3?()) ?? Vector2.op_Implicit(this.displayOffset);
      case 2:
        Vector2? displayOffsetSouth = this.displayOffsetSouth;
        return (displayOffsetSouth.HasValue ? new Vector3?(Vector2.op_Implicit(displayOffsetSouth.GetValueOrDefault())) : new Vector3?()) ?? Vector2.op_Implicit(this.displayOffset);
      case 3:
        Vector2? displayOffsetWest = this.displayOffsetWest;
        return (displayOffsetWest.HasValue ? new Vector3?(Vector2.op_Implicit(displayOffsetWest.GetValueOrDefault())) : new Vector3?()) ?? Vector2.op_Implicit(this.displayOffset);
      default:
        return Vector2.op_Implicit(this.displayOffset);
    }
  }
}
