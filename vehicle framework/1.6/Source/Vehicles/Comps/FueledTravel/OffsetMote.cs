// Decompiled with JetBrains decompiler
// Type: Vehicles.OffsetMote
// Assembly: Vehicles, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 81BFDC99-D8AD-42E1-8470-2F73E7836B4A
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\Vehicles.dll

#nullable disable
namespace Vehicles;

public struct OffsetMote
{
  public bool windAffected;
  public float moteThrownSpeed;
  public float? predeterminedAngleVector;
  public float xOffset;
  public float zOffset;
  public int numTimesSpawned;

  public int NumTimesSpawned => this.numTimesSpawned != 0 ? this.numTimesSpawned : 1;
}
