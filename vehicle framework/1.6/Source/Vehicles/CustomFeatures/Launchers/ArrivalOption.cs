// Decompiled with JetBrains decompiler
// Type: Vehicles.World.ArrivalOption
// Assembly: Vehicles, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 81BFDC99-D8AD-42E1-8470-2F73E7836B4A
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\Vehicles.dll

using JetBrains.Annotations;
using RimWorld;
using RimWorld.Planet;
using SmashTools.Targeting;
using System;
using Verse;

#nullable disable
namespace Vehicles.World;

[PublicAPI]
public class ArrivalOption : ITargetOption
{
  public readonly TaggedString label;
  public readonly IArrivalAction arrivalAction;
  public readonly Action<TargetData<GlobalTargetInfo>> continueWith;

  public ArrivalOption(TaggedString label, IArrivalAction arrivalAction)
  {
    this.label = label;
    this.arrivalAction = arrivalAction;
  }

  public ArrivalOption(TaggedString label, Action<TargetData<GlobalTargetInfo>> continueWith)
  {
    this.label = label;
    this.continueWith = continueWith;
  }

  public FloatMenuAcceptanceReport AcceptanceReport { get; init; } = FloatMenuAcceptanceReport.WasAccepted;

  TaggedString ITargetOption.Label => this.label;
}
