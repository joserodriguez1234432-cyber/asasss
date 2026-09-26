// Decompiled with JetBrains decompiler
// Type: Vehicles.PawnOverlayRenderer
// Assembly: Vehicles, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 81BFDC99-D8AD-42E1-8470-2F73E7836B4A
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\Vehicles.dll

using JetBrains.Annotations;
using SmashTools;
using System;
using UnityEngine;
using Verse;

#nullable disable
namespace Vehicles;

[UsedImplicitly]
public class PawnOverlayRenderer
{
  private Listing_SplitColumns listing = new Listing_SplitColumns();
  [TweakField(SettingsType = UISettingsType.Checkbox)]
  public bool showBody = true;
  [TweakField(SettingsType = UISettingsType.ToggleLabel)]
  public Rot4 north = (Rot4) Rot8.North;
  [TweakField(SettingsType = UISettingsType.ToggleLabel)]
  public Rot4 east = (Rot4) Rot8.East;
  [TweakField(SettingsType = UISettingsType.ToggleLabel)]
  public Rot4 south = (Rot4) Rot8.South;
  [TweakField(SettingsType = UISettingsType.ToggleLabel)]
  public Rot4 west = (Rot4) Rot8.West;
  [TweakField(SettingsType = UISettingsType.ToggleLabel)]
  public Rot4 northEast = Rot4.North;
  [TweakField(SettingsType = UISettingsType.ToggleLabel)]
  public Rot4 southEast = Rot4.South;
  [TweakField(SettingsType = UISettingsType.ToggleLabel)]
  public Rot4 southWest = Rot4.South;
  [TweakField(SettingsType = UISettingsType.ToggleLabel)]
  public Rot4 northWest = Rot4.North;
  [TweakField(SettingsType = UISettingsType.IntegerBox)]
  public int layer = 1;
  [TweakField(SettingsType = UISettingsType.IntegerBox)]
  public int? layerNorth;
  [TweakField(SettingsType = UISettingsType.IntegerBox)]
  public int? layerEast;
  [TweakField(SettingsType = UISettingsType.IntegerBox)]
  public int? layerSouth;
  [TweakField(SettingsType = UISettingsType.IntegerBox)]
  public int? layerWest;
  [TweakField(SettingsType = UISettingsType.IntegerBox)]
  public int? layerNorthEast;
  [TweakField(SettingsType = UISettingsType.IntegerBox)]
  public int? layerSouthEast;
  [TweakField(SettingsType = UISettingsType.IntegerBox)]
  public int? layerSouthWest;
  [TweakField(SettingsType = UISettingsType.IntegerBox)]
  public int? layerNorthWest;
  [TweakField(SettingsType = UISettingsType.FloatBox)]
  public Vector3 drawOffset = Vector3.zero;
  [TweakField(SettingsType = UISettingsType.FloatBox)]
  public Vector3? drawOffsetNorth;
  [TweakField(SettingsType = UISettingsType.FloatBox)]
  public Vector3? drawOffsetEast;
  [TweakField(SettingsType = UISettingsType.FloatBox)]
  public Vector3? drawOffsetSouth;
  [TweakField(SettingsType = UISettingsType.FloatBox)]
  public Vector3? drawOffsetWest;
  [TweakField(SettingsType = UISettingsType.FloatBox)]
  public Vector3? drawOffsetNorthEast;
  [TweakField(SettingsType = UISettingsType.FloatBox)]
  public Vector3? drawOffsetSouthEast;
  [TweakField(SettingsType = UISettingsType.FloatBox)]
  public Vector3? drawOffsetSouthWest;
  [TweakField(SettingsType = UISettingsType.FloatBox)]
  public Vector3? drawOffsetNorthWest;
  [TweakField(SettingsType = UISettingsType.SliderFloat)]
  [SliderValues(MinValue = 0.0f, MaxValue = 360f, RoundDecimalPlaces = 0, Increment = 1f)]
  public float angle;
  [TweakField(SettingsType = UISettingsType.SliderFloat)]
  [SliderValues(MinValue = 0.0f, MaxValue = 360f, RoundDecimalPlaces = 0, Increment = 1f)]
  public float? angleNorth;
  [TweakField(SettingsType = UISettingsType.SliderFloat)]
  [SliderValues(MinValue = 0.0f, MaxValue = 360f, RoundDecimalPlaces = 0, Increment = 1f)]
  public float? angleEast;
  [TweakField(SettingsType = UISettingsType.SliderFloat)]
  [SliderValues(MinValue = 0.0f, MaxValue = 360f, RoundDecimalPlaces = 0, Increment = 1f)]
  public float? angleSouth;
  [TweakField(SettingsType = UISettingsType.SliderFloat)]
  [SliderValues(MinValue = 0.0f, MaxValue = 360f, RoundDecimalPlaces = 0, Increment = 1f)]
  public float? angleWest;
  [TweakField(SettingsType = UISettingsType.SliderFloat)]
  [SliderValues(MinValue = 0.0f, MaxValue = 360f, RoundDecimalPlaces = 0, Increment = 1f)]
  public float? angleNorthEast;
  [TweakField(SettingsType = UISettingsType.SliderFloat)]
  [SliderValues(MinValue = 0.0f, MaxValue = 360f, RoundDecimalPlaces = 0, Increment = 1f)]
  public float? angleSouthEast;
  [TweakField(SettingsType = UISettingsType.SliderFloat)]
  [SliderValues(MinValue = 0.0f, MaxValue = 360f, RoundDecimalPlaces = 0, Increment = 1f)]
  public float? angleSouthWest;
  [TweakField(SettingsType = UISettingsType.SliderFloat)]
  [SliderValues(MinValue = 0.0f, MaxValue = 360f, RoundDecimalPlaces = 0, Increment = 1f)]
  public float? angleNorthWest;

  public Rot4 RotFor(Rot8 rot)
  {
    switch (rot.AsInt)
    {
      case 0:
        return this.north;
      case 1:
        return this.east;
      case 2:
        return this.south;
      case 3:
        return this.west;
      case 4:
        return this.northEast;
      case 5:
        return this.southEast;
      case 6:
        return this.southWest;
      case 7:
        return this.northWest;
      default:
        throw new NotImplementedException();
    }
  }

  public float AngleFor(Rot8 rot)
  {
    switch (rot.AsInt)
    {
      case 0:
        float? angleNorth1 = this.angleNorth;
        double num1;
        if (!angleNorth1.HasValue)
        {
          float? angleSouth = this.angleSouth;
          float num2 = 180f;
          num1 = (angleSouth.HasValue ? (double) new float?(angleSouth.GetValueOrDefault() + num2) : (double) new float?()) ?? (double) this.angle;
        }
        else
          num1 = (double) angleNorth1.GetValueOrDefault();
        return (float) num1;
      case 1:
        float? angleEast1 = this.angleEast;
        double num3;
        if (!angleEast1.HasValue)
        {
          float? angleWest = this.angleWest;
          num3 = (angleWest.HasValue ? (double) new float?(-angleWest.GetValueOrDefault()) : (double) new float?()) ?? (double) this.angle;
        }
        else
          num3 = (double) angleEast1.GetValueOrDefault();
        return (float) num3;
      case 2:
        float? angleSouth1 = this.angleSouth;
        double num4;
        if (!angleSouth1.HasValue)
        {
          float? angleNorth2 = this.angleNorth;
          num4 = (angleNorth2.HasValue ? (double) new float?(-angleNorth2.GetValueOrDefault()) : (double) new float?()) ?? (double) this.angle;
        }
        else
          num4 = (double) angleSouth1.GetValueOrDefault();
        return (float) num4;
      case 3:
        float? angleWest1 = this.angleWest;
        double num5;
        if (!angleWest1.HasValue)
        {
          float? angleEast2 = this.angleEast;
          num5 = (angleEast2.HasValue ? (double) new float?(-angleEast2.GetValueOrDefault()) : (double) new float?()) ?? (double) this.angle;
        }
        else
          num5 = (double) angleWest1.GetValueOrDefault();
        return (float) num5;
      case 4:
        float? angleNorthEast1 = this.angleNorthEast;
        double num6;
        if (!angleNorthEast1.HasValue)
        {
          float? angleNorthWest = this.angleNorthWest;
          num6 = (angleNorthWest.HasValue ? (double) new float?(-angleNorthWest.GetValueOrDefault()) : (double) new float?()) ?? (double) this.angle + 45.0;
        }
        else
          num6 = (double) angleNorthEast1.GetValueOrDefault();
        return (float) num6;
      case 5:
        float? angleSouthEast1 = this.angleSouthEast;
        double num7;
        if (!angleSouthEast1.HasValue)
        {
          float? angleSouthWest = this.angleSouthWest;
          num7 = (angleSouthWest.HasValue ? (double) new float?(-angleSouthWest.GetValueOrDefault()) : (double) new float?()) ?? (double) this.angle - 45.0;
        }
        else
          num7 = (double) angleSouthEast1.GetValueOrDefault();
        return (float) num7;
      case 6:
        float? angleSouthWest1 = this.angleSouthWest;
        double num8;
        if (!angleSouthWest1.HasValue)
        {
          float? angleSouthEast2 = this.angleSouthEast;
          num8 = (angleSouthEast2.HasValue ? (double) new float?(-angleSouthEast2.GetValueOrDefault()) : (double) new float?()) ?? (double) this.angle + 45.0;
        }
        else
          num8 = (double) angleSouthWest1.GetValueOrDefault();
        return (float) num8;
      case 7:
        float? angleNorthWest1 = this.angleNorthWest;
        double num9;
        if (!angleNorthWest1.HasValue)
        {
          float? angleNorthEast2 = this.angleNorthEast;
          num9 = (angleNorthEast2.HasValue ? (double) new float?(-angleNorthEast2.GetValueOrDefault()) : (double) new float?()) ?? (double) this.angle - 45.0;
        }
        else
          num9 = (double) angleNorthWest1.GetValueOrDefault();
        return (float) num9;
      default:
        throw new NotImplementedException();
    }
  }

  public float LayerFor(Rot8 rot)
  {
    int num;
    switch (rot.AsInt)
    {
      case 0:
        num = this.layerNorth ?? this.layerSouth ?? this.layer;
        break;
      case 1:
        num = this.layerEast ?? this.layerWest ?? this.layer;
        break;
      case 2:
        num = this.layerSouth ?? this.layerNorth ?? this.layer;
        break;
      case 3:
        num = this.layerWest ?? this.layerEast ?? this.layer;
        break;
      case 4:
        num = this.layerNorthEast ?? this.layerNorthWest ?? this.layerNorth ?? this.layer;
        break;
      case 5:
        num = this.layerSouthEast ?? this.layerSouthWest ?? this.layerSouth ?? this.layer;
        break;
      case 6:
        num = this.layerSouthWest ?? this.layerSouthEast ?? this.layerSouth ?? this.layer;
        break;
      case 7:
        num = this.layerNorthWest ?? this.layerNorthEast ?? this.layerNorth ?? this.layer;
        break;
      default:
        throw new NotImplementedException();
    }
    return (float) num * 0.00365853682f;
  }

  public Vector3 DrawOffsetFor(Rot8 rot)
  {
    Vector3 vector3_1;
    switch (rot.AsInt)
    {
      case 0:
        Vector3? drawOffsetNorth = this.drawOffsetNorth;
        Vector3 vector3_2;
        if (!drawOffsetNorth.HasValue)
        {
          ref Vector3? local = ref this.drawOffsetSouth;
          vector3_2 = local.HasValue ? local.GetValueOrDefault().MirrorVertical() : this.drawOffset;
        }
        else
          vector3_2 = drawOffsetNorth.GetValueOrDefault();
        vector3_1 = vector3_2;
        break;
      case 1:
        Vector3? drawOffsetEast = this.drawOffsetEast;
        Vector3 vector3_3;
        if (!drawOffsetEast.HasValue)
        {
          ref Vector3? local = ref this.drawOffsetWest;
          vector3_3 = local.HasValue ? local.GetValueOrDefault().MirrorHorizontal() : Vector3Utility.RotatedBy(this.drawOffset, rot.AsAngle);
        }
        else
          vector3_3 = drawOffsetEast.GetValueOrDefault();
        vector3_1 = vector3_3;
        break;
      case 2:
        Vector3? drawOffsetSouth = this.drawOffsetSouth;
        Vector3 vector3_4;
        if (!drawOffsetSouth.HasValue)
        {
          ref Vector3? local = ref this.drawOffsetNorth;
          vector3_4 = local.HasValue ? local.GetValueOrDefault().MirrorVertical() : Vector3Utility.RotatedBy(this.drawOffset, rot.AsAngle);
        }
        else
          vector3_4 = drawOffsetSouth.GetValueOrDefault();
        vector3_1 = vector3_4;
        break;
      case 3:
        Vector3? drawOffsetWest = this.drawOffsetWest;
        Vector3 vector3_5;
        if (!drawOffsetWest.HasValue)
        {
          ref Vector3? local = ref this.drawOffsetEast;
          vector3_5 = local.HasValue ? local.GetValueOrDefault().MirrorHorizontal() : Vector3Utility.RotatedBy(this.drawOffset, rot.AsAngle);
        }
        else
          vector3_5 = drawOffsetWest.GetValueOrDefault();
        vector3_1 = vector3_5;
        break;
      case 4:
        Vector3? drawOffsetNorthEast = this.drawOffsetNorthEast;
        Vector3 vector3_6;
        if (!drawOffsetNorthEast.HasValue)
        {
          ref Vector3? local1 = ref this.drawOffsetNorthWest;
          if (!local1.HasValue)
          {
            ref Vector3? local2 = ref this.drawOffsetNorth;
            vector3_6 = local2.HasValue ? Vector3Utility.RotatedBy(local2.GetValueOrDefault(), 45f) : Vector3Utility.RotatedBy(this.drawOffset, rot.AsAngle);
          }
          else
            vector3_6 = local1.GetValueOrDefault().MirrorHorizontal();
        }
        else
          vector3_6 = drawOffsetNorthEast.GetValueOrDefault();
        vector3_1 = vector3_6;
        break;
      case 5:
        Vector3? drawOffsetSouthEast = this.drawOffsetSouthEast;
        Vector3 vector3_7;
        if (!drawOffsetSouthEast.HasValue)
        {
          ref Vector3? local3 = ref this.drawOffsetSouthWest;
          if (!local3.HasValue)
          {
            ref Vector3? local4 = ref this.drawOffsetSouth;
            vector3_7 = local4.HasValue ? Vector3Utility.RotatedBy(local4.GetValueOrDefault(), -45f) : Vector3Utility.RotatedBy(this.drawOffset, rot.AsAngle);
          }
          else
            vector3_7 = local3.GetValueOrDefault().MirrorHorizontal();
        }
        else
          vector3_7 = drawOffsetSouthEast.GetValueOrDefault();
        vector3_1 = vector3_7;
        break;
      case 6:
        Vector3? drawOffsetSouthWest = this.drawOffsetSouthWest;
        Vector3 vector3_8;
        if (!drawOffsetSouthWest.HasValue)
        {
          ref Vector3? local5 = ref this.drawOffsetSouthEast;
          if (!local5.HasValue)
          {
            ref Vector3? local6 = ref this.drawOffsetSouth;
            vector3_8 = local6.HasValue ? Vector3Utility.RotatedBy(local6.GetValueOrDefault(), 45f) : Vector3Utility.RotatedBy(this.drawOffset, rot.AsAngle);
          }
          else
            vector3_8 = local5.GetValueOrDefault().MirrorHorizontal();
        }
        else
          vector3_8 = drawOffsetSouthWest.GetValueOrDefault();
        vector3_1 = vector3_8;
        break;
      case 7:
        Vector3? drawOffsetNorthWest = this.drawOffsetNorthWest;
        Vector3 vector3_9;
        if (!drawOffsetNorthWest.HasValue)
        {
          ref Vector3? local7 = ref this.drawOffsetNorthEast;
          if (!local7.HasValue)
          {
            ref Vector3? local8 = ref this.drawOffsetNorth;
            vector3_9 = local8.HasValue ? Vector3Utility.RotatedBy(local8.GetValueOrDefault(), -45f) : Vector3Utility.RotatedBy(this.drawOffset, rot.AsAngle);
          }
          else
            vector3_9 = local7.GetValueOrDefault().MirrorHorizontal();
        }
        else
          vector3_9 = drawOffsetNorthWest.GetValueOrDefault();
        vector3_1 = vector3_9;
        break;
      default:
        throw new NotImplementedException();
    }
    Vector3 vector3_10 = vector3_1;
    vector3_10.y += this.LayerFor(rot);
    return vector3_10;
  }

  public void RenderEditor(Rect rect)
  {
    this.listing.Begin(rect, 2);
    if (this.layerNorth.HasValue)
    {
      int num = this.layerNorth.Value;
      this.listing.SliderLabeled("Layer North", ref num, string.Empty, string.Empty, string.Empty, -5, 5);
      this.layerNorth = new int?(num);
    }
    if (this.layerEast.HasValue)
    {
      int num = this.layerEast.Value;
      this.listing.SliderLabeled("Layer East", ref num, string.Empty, string.Empty, string.Empty, -5, 5);
      this.layerEast = new int?(num);
    }
    if (this.layerSouth.HasValue)
    {
      int num = this.layerSouth.Value;
      this.listing.SliderLabeled("Layer South", ref num, string.Empty, string.Empty, string.Empty, -5, 5);
      this.layerSouth = new int?(num);
    }
    if (this.layerWest.HasValue)
    {
      int num = this.layerWest.Value;
      this.listing.SliderLabeled("Layer West", ref num, string.Empty, string.Empty, string.Empty, -5, 5);
      this.layerWest = new int?(num);
    }
    this.listing.NextRow();
    if (this.drawOffsetNorth.HasValue)
    {
      this.drawOffsetNorth = new Vector3?(this.listing.Vector3Box("Offset North", this.drawOffsetNorth.Value, string.Empty));
      this.listing.NextRow();
    }
    if (this.drawOffsetEast.HasValue)
    {
      this.drawOffsetEast = new Vector3?(this.listing.Vector3Box("Offset East", this.drawOffsetEast.Value, string.Empty));
      this.listing.NextRow();
    }
    if (this.drawOffsetSouth.HasValue)
    {
      this.drawOffsetSouth = new Vector3?(this.listing.Vector3Box("Offset South", this.drawOffsetSouth.Value, string.Empty));
      this.listing.NextRow();
    }
    if (this.drawOffsetWest.HasValue)
    {
      this.drawOffsetWest = new Vector3?(this.listing.Vector3Box("Offset West", this.drawOffsetWest.Value, string.Empty));
      this.listing.NextRow();
    }
    ((Listing) this.listing).End();
  }
}
