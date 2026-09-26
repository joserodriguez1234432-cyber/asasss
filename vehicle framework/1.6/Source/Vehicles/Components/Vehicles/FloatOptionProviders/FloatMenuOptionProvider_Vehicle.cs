// Decompiled with JetBrains decompiler
// Type: Vehicles.FloatMenuOptionProvider_Vehicle
// Assembly: Vehicles, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 81BFDC99-D8AD-42E1-8470-2F73E7836B4A
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\Vehicles.dll

using JetBrains.Annotations;
using RimWorld;
using Verse;

#nullable disable
namespace Vehicles;

[PublicAPI]
public abstract class FloatMenuOptionProvider_Vehicle : FloatMenuOptionProvider
{
  protected virtual bool Drafted => true;

  protected virtual bool Undrafted => false;

  protected virtual bool Multiselect => true;

  protected virtual bool MechanoidCanDo => false;

  protected virtual bool IgnoreFogged => false;

  public virtual bool SelectedPawnValid(Pawn pawn, FloatMenuContext context)
  {
    return pawn is VehiclePawn vehicle && this.SelectedVehicleValid(vehicle, context);
  }

  protected virtual bool SelectedVehicleValid(VehiclePawn vehicle, FloatMenuContext context)
  {
    return true;
  }
}
