// Decompiled with JetBrains decompiler
// Type: Vehicles.Compatibility.ConditionalVehiclePatch
// Assembly: Vehicles, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 81BFDC99-D8AD-42E1-8470-2F73E7836B4A
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\Vehicles.dll

using SmashTools.Patching;
using Verse;

#nullable disable
namespace Vehicles.Compatibility;

public abstract class ConditionalVehiclePatch : IConditionalPatch
{
  string IConditionalPatch.SourceId => "SmashPhil.VehicleFramework";

  public abstract string PackageId { get; }

  public abstract PatchSequence PatchAt { get; }

  public abstract void PatchAll(ModMetaData mod);
}
