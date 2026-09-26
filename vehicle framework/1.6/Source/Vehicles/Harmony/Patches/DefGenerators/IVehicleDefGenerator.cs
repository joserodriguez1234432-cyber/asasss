// Decompiled with JetBrains decompiler
// Type: Vehicles.IVehicleDefGenerator`1
// Assembly: Vehicles, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 81BFDC99-D8AD-42E1-8470-2F73E7836B4A
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\Vehicles.dll

using JetBrains.Annotations;
using Verse;

#nullable disable
namespace Vehicles;

[PublicAPI]
public interface IVehicleDefGenerator<T> where T : Def
{
  [Pure]
  bool TryGenerateImpliedDef(VehicleDef vehicleDef, out T impliedDef, bool hotReload);
}
