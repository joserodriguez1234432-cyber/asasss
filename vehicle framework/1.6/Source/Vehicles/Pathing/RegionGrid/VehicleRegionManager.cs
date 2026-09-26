// Decompiled with JetBrains decompiler
// Type: Vehicles.VehicleGridManager
// Assembly: Vehicles, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 81BFDC99-D8AD-42E1-8470-2F73E7836B4A
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\Vehicles.dll

#nullable disable
namespace Vehicles;

public abstract class VehicleGridManager
{
  protected readonly VehiclePathingSystem mapping;
  protected internal VehicleDef createdFor;

  protected VehicleGridManager(VehiclePathingSystem mapping, VehicleDef createdFor)
  {
    this.mapping = mapping;
    this.createdFor = createdFor;
  }

  public VehicleDef CreatedFor => this.createdFor;

  public virtual void PostInit()
  {
  }
}
