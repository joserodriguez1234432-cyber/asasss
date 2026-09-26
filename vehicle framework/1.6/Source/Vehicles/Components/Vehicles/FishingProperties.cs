// Decompiled with JetBrains decompiler
// Type: Vehicles.Compatibility.FishingProperties
// Assembly: Vehicles, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 81BFDC99-D8AD-42E1-8470-2F73E7836B4A
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\Vehicles.dll

using JetBrains.Annotations;
using System.Collections.Generic;
using Verse;

#nullable disable
namespace Vehicles.Compatibility;

[PublicAPI]
public class FishingProperties
{
  public List<IntVec2> fishingCells;
  public float yieldModifier = 1f;
  public float fishingTicksModifier = 1f;
  public int? animalSkillOverride;
}
