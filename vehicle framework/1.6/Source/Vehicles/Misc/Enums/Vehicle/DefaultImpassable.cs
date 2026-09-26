// Decompiled with JetBrains decompiler
// Type: Vehicles.DefaultImpassable
// Assembly: Vehicles, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 81BFDC99-D8AD-42E1-8470-2F73E7836B4A
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\Vehicles.dll

using System;

#nullable disable
namespace Vehicles;

[Flags]
public enum DefaultImpassable
{
  None = 0,
  Terrain = 1,
  Things = 2,
  Biomes = 4,
  Rivers = 8,
  Hilliness = 16, // 0x00000010
  Roads = 32, // 0x00000020
}
