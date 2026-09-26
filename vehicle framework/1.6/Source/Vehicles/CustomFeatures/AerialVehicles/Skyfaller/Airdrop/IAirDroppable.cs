// Decompiled with JetBrains decompiler
// Type: Vehicles.IAirDroppable
// Assembly: Vehicles, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 81BFDC99-D8AD-42E1-8470-2F73E7836B4A
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\Vehicles.dll

using JetBrains.Annotations;
using Verse;

#nullable disable
namespace Vehicles;

[PublicAPI]
public interface IAirDroppable : IExposable
{
  Thing Thing { get; }

  ThingDef SkyfallerDef { get; }

  void OnDropped(Map map, IntVec3 pos);

  void OnFailureToDrop(Map map, IntVec3 simPos);
}
