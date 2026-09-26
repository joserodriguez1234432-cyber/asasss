// Decompiled with JetBrains decompiler
// Type: Vehicles.Rendering.RenderConditions
// Assembly: Vehicles, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 81BFDC99-D8AD-42E1-8470-2F73E7836B4A
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\Vehicles.dll

using System;

#nullable disable
namespace Vehicles.Rendering;

[Flags]
public enum RenderConditions
{
  None = 0,
  CurrentMap = 1,
  OnScreen = 2,
  Vanilla = OnScreen | CurrentMap, // 0x00000003
}
