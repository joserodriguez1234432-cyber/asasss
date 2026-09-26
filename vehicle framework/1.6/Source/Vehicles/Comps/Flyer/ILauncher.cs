// Decompiled with JetBrains decompiler
// Type: Vehicles.ILauncher
// Assembly: Vehicles, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 81BFDC99-D8AD-42E1-8470-2F73E7836B4A
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\Vehicles.dll

using JetBrains.Annotations;
using RimWorld.Planet;
using SmashTools.Targeting;
using System.Collections.Generic;
using UnityEngine;
using Vehicles.World;

#nullable disable
namespace Vehicles;

[PublicAPI]
public interface ILauncher
{
  PlanetTile Tile { get; }

  Vector3 Origin { get; }

  void Launch(TargetData<GlobalTargetInfo> targetData, IArrivalAction arrivalAction);

  IEnumerable<ArrivalOption> OptionsAt(GlobalTargetInfo target);
}
