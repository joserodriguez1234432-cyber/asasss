// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.CompAllowDangerTerrains
// Assembly: VehicleMapFramework, Version=1.6.562.0, Culture=neutral, PublicKeyToken=null
// MVID: 10A61882-945F-4CFC-9B06-CA8EEF5ADB36
// Assembly location: D:\Programas\steamapps\workshop\content\294100\3426502333\1.6\Assemblies\VehicleMapFramework.dll

using SmashTools;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Verse;

#nullable disable
namespace VehicleMapFramework;

[StaticConstructorOnStartup]
public class CompAllowDangerTerrains : ThingComp
{
  public static readonly ConditionalWeakTable<Pawn, List<TerrainDef>> AllowedTerrains = new ConditionalWeakTable<Pawn, List<TerrainDef>>();

  static CompAllowDangerTerrains()
  {
    GameEvent.OnGameDisposed += new Action(CompAllowDangerTerrains.AllowedTerrains.Clear);
  }

  public CompProperties_AllowDangerTerrains Props
  {
    get => (CompProperties_AllowDangerTerrains) this.props;
  }

  public virtual void Notify_Equipped(Pawn pawn)
  {
    CompAllowDangerTerrains.AllowedTerrains.AddOrUpdate(pawn, this.Props.allowedDangerTerrains);
  }

  public virtual void Notify_Unequipped(Pawn pawn)
  {
    CompAllowDangerTerrains.AllowedTerrains.Remove(pawn);
  }
}
