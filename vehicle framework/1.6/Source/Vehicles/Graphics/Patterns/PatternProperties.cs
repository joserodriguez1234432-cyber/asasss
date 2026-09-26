// Decompiled with JetBrains decompiler
// Type: Vehicles.PatternProperties
// Assembly: Vehicles, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 81BFDC99-D8AD-42E1-8470-2F73E7836B4A
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\Vehicles.dll

using System.Collections.Generic;
using UnityEngine;
using Verse;

#nullable disable
namespace Vehicles;

public class PatternProperties
{
  public Dictionary<string, float> tiles = new Dictionary<string, float>();
  public bool equalize = true;
  public bool dynamicTiling = true;
  public Color? colorOne;
  public Color? colorTwo;
  public Color? colorThree;

  public bool IsDefault { get; internal set; }

  public override int GetHashCode()
  {
    return Gen.HashCombine<Color?>(Gen.HashCombine<Color?>(Gen.HashCombine<Color?>(Gen.HashCombine<bool>(Gen.HashCombine<Dictionary<string, float>>(0, this.tiles), this.equalize), this.colorOne), this.colorTwo), this.colorThree);
  }

  public override string ToString()
  {
    return $"Tiles:{this.tiles} Equalize:{this.equalize} ColorOne:{Gen.ToStringSafe<Color?>(this.colorOne) ?? "Null"} ColorTwo:{Gen.ToStringSafe<Color?>(this.colorTwo) ?? "Null"} ColorThree:{Gen.ToStringSafe<Color?>(this.colorThree) ?? "Null"}";
  }
}
