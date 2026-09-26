// Decompiled with JetBrains decompiler
// Type: Vehicles.Connection
// Assembly: Vehicles, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 81BFDC99-D8AD-42E1-8470-2F73E7836B4A
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\Vehicles.dll

#nullable disable
namespace Vehicles;

public readonly struct Connection(int from, int to, float cost)
{
  public readonly int from = from;
  public readonly int to = to;
  public readonly float cost = cost;
}
