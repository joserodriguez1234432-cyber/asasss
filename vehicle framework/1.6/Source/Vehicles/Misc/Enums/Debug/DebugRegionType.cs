// Decompiled with JetBrains decompiler
// Type: Vehicles.DebugRegionType
// Assembly: Vehicles, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 81BFDC99-D8AD-42E1-8470-2F73E7836B4A
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\Vehicles.dll

using System;

#nullable disable
namespace Vehicles;

[Flags]
public enum DebugRegionType
{
  None = 0,
  Regions = 1,
  Rooms = 2,
  Links = 4,
  Weights = 8,
  PathCosts = 16, // 0x00000010
  References = 32, // 0x00000020
}
