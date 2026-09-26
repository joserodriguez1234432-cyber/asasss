// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.VehicleMapFlag
// Assembly: VehicleMapFramework, Version=1.6.562.0, Culture=neutral, PublicKeyToken=null
// MVID: 10A61882-945F-4CFC-9B06-CA8EEF5ADB36
// Assembly location: D:\Programas\steamapps\workshop\content\294100\3426502333\1.6\Assemblies\VehicleMapFramework.dll

using System;

#nullable disable
namespace VehicleMapFramework;

[Flags]
public enum VehicleMapFlag
{
  None = 0,
  StructureCells = 1,
  ExpandableCells = 2,
  OutOfBoundsCells = 4,
  All = OutOfBoundsCells | ExpandableCells | StructureCells, // 0x00000007
}
