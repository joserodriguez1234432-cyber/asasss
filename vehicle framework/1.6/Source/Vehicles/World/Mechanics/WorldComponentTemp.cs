// Decompiled with JetBrains decompiler
// Type: Vehicles.WorldComponentTemp
// Assembly: Vehicles, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 81BFDC99-D8AD-42E1-8470-2F73E7836B4A
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\Vehicles.dll

using RimWorld.Planet;

#nullable disable
namespace Vehicles;

public abstract class WorldComponentTemp
{
  public WorldComponentTemp(World world)
  {
  }

  public abstract void WorldComponentUpdate();

  public abstract void WorldComponentTick();

  public abstract void FinalizeInit();

  public virtual void ExposeData()
  {
  }
}
