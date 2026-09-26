// Decompiled with JetBrains decompiler
// Type: Vehicles.VehicleGenerationRequest
// Assembly: Vehicles, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 81BFDC99-D8AD-42E1-8470-2F73E7836B4A
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\Vehicles.dll

using JetBrains.Annotations;
using RimWorld;
using System;
using UnityEngine;
using Verse;

#nullable disable
namespace Vehicles;

[PublicAPI]
public struct VehicleGenerationRequest
{
  public readonly VehicleDef vehicleDef;
  public Faction faction;
  public bool cleanSlate;
  public bool randomizeColors;
  public Color colorOne;
  public Color colorTwo;
  public Color colorThree;
  public float tiling;
  public Vector2 displacement;

  public VehicleGenerationRequest(VehicleDef vehicleDef)
  {
    this.faction = (Faction) null;
    this.randomizeColors = false;
    this.colorOne = new Color();
    this.colorTwo = new Color();
    this.colorThree = new Color();
    this.tiling = 0.0f;
    this.displacement = new Vector2();
    // ISSUE: reference to a compiler-generated field
    this.\u003CVehicleDef\u003Ek__BackingField = (VehicleDef) null;
    // ISSUE: reference to a compiler-generated field
    this.\u003CFaction\u003Ek__BackingField = (Faction) null;
    // ISSUE: reference to a compiler-generated field
    this.\u003CRandomizeMask\u003Ek__BackingField = false;
    // ISSUE: reference to a compiler-generated field
    this.\u003CUpgrades\u003Ek__BackingField = 0;
    // ISSUE: reference to a compiler-generated field
    this.\u003CCleanSlate\u003Ek__BackingField = false;
    // ISSUE: reference to a compiler-generated field
    this.\u003CColorOne\u003Ek__BackingField = new Color();
    // ISSUE: reference to a compiler-generated field
    this.\u003CColorTwo\u003Ek__BackingField = new Color();
    // ISSUE: reference to a compiler-generated field
    this.\u003CColorThree\u003Ek__BackingField = new Color();
    // ISSUE: reference to a compiler-generated field
    this.\u003CTiling\u003Ek__BackingField = 0.0f;
    // ISSUE: reference to a compiler-generated field
    this.\u003CDisplacement\u003Ek__BackingField = new Vector2();
    this.cleanSlate = true;
    this.vehicleDef = vehicleDef;
  }

  public VehicleGenerationRequest(VehicleDef vehicleDef, Faction faction)
    : this(vehicleDef)
  {
    this.faction = faction;
  }

  public VehicleGenerationRequest(
    VehicleDef vehicleDef,
    Faction faction,
    bool randomizeColors = false,
    bool randomizeMask = false,
    bool cleanSlate = true)
    : this(vehicleDef, faction)
  {
    this.randomizeColors = randomizeColors;
    this.randomizeColors |= randomizeMask;
    this.cleanSlate = cleanSlate;
    this.AssignForBackCompatibility();
  }

  public VehicleGenerationRequest(
    VehicleDef vehicleDef,
    Faction faction,
    Color colorOne,
    Color colorTwo,
    Color colorThree,
    float tiling,
    Vector2 displacement)
    : this(vehicleDef, faction)
  {
    this.colorOne = colorOne;
    this.colorTwo = colorTwo;
    this.colorThree = colorThree;
    this.tiling = tiling;
    this.displacement = displacement;
  }

  [Obsolete("Deprecated - Use constructor instead.", true)]
  public VehicleDef VehicleDef { get; set; }

  [Obsolete("Deprecated - Use constructor instead.")]
  public Faction Faction { get; set; }

  [Obsolete("Deprecated - Use field or constructor instead.")]
  public bool RandomizeMask { get; set; }

  [Obsolete("Deprecated - Use field or constructor instead.")]
  public int Upgrades { get; set; }

  [Obsolete("Deprecated - Use field or constructor instead.")]
  public bool CleanSlate { get; set; }

  [Obsolete("Deprecated - Use field or constructor instead.")]
  public Color ColorOne { get; set; }

  [Obsolete("Deprecated - Use field or constructor instead.")]
  public Color ColorTwo { get; set; }

  [Obsolete("Deprecated - Use field or constructor instead.")]
  public Color ColorThree { get; set; }

  [Obsolete("Deprecated - Use field or constructor instead.")]
  public float Tiling { get; set; }

  [Obsolete("Deprecated - Use field or constructor instead.")]
  public Vector2 Displacement { get; set; }

  private void AssignForBackCompatibility()
  {
    if (this.faction == null)
      this.faction = this.Faction;
    this.randomizeColors |= this.RandomizeMask;
    this.colorOne = Color.op_Inequality(this.ColorOne, Color.clear) ? this.ColorOne : this.colorOne;
    this.colorTwo = Color.op_Inequality(this.ColorTwo, Color.clear) ? this.ColorTwo : this.colorTwo;
    this.colorThree = Color.op_Inequality(this.ColorThree, Color.clear) ? this.ColorThree : this.colorThree;
    this.tiling = (double) this.Tiling != 0.0 ? this.Tiling : 0.0f;
    this.displacement = Vector2.op_Inequality(this.Displacement, Vector2.zero) ? this.Displacement : Vector2.zero;
  }

  public static (Color colorOne, Color colorTwo, Color colorThree) GetCompletelyRandomColors()
  {
    float num1 = Rand.Range(0.25f, 0.75f);
    float num2 = Rand.Range(0.25f, 0.75f);
    float num3 = Rand.Range(0.25f, 0.75f);
    Color color1;
    // ISSUE: explicit constructor call
    ((Color) ref color1).\u002Ector(num1, num2, num3, 1f);
    float num4 = Rand.Range(0.25f, 0.75f);
    float num5 = Rand.Range(0.25f, 0.75f);
    float num6 = Rand.Range(0.25f, 0.75f);
    Color color2;
    // ISSUE: explicit constructor call
    ((Color) ref color2).\u002Ector(num4, num5, num6, 1f);
    float num7 = Rand.Range(0.25f, 0.75f);
    float num8 = Rand.Range(0.25f, 0.75f);
    float num9 = Rand.Range(0.25f, 0.75f);
    Color color3;
    // ISSUE: explicit constructor call
    ((Color) ref color3).\u002Ector(num7, num8, num9, 1f);
    return (color1, color2, color3);
  }
}
