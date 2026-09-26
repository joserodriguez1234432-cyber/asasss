// Decompiled with JetBrains decompiler
// Type: Vehicles.DropShip
// Assembly: Vehicles, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 81BFDC99-D8AD-42E1-8470-2F73E7836B4A
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\Vehicles.dll

using JetBrains.Annotations;
using RimWorld;
using SmashTools;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Text;
using Verse;
using Verse.Sound;

#nullable enable
namespace Vehicles;

[PublicAPI]
public class DropShip : IExposable
{
  private static readonly FloatRange DropAngleRange = new FloatRange(170f, 190f);
  private static readonly FloatRange ExtraAngleVariance = new FloatRange(-5f, 5f);
  private 
  #nullable disable
  Map map;
  private Faction faction;
  private SoundDef flyoverSoundDef;
  private DropShip.Properties properties;
  private DropZone dropZone;
  private List<IAirDroppable> airDroppables = new List<IAirDroppable>();
  private Sustainer sustainer;

  public DropShip(Map map, DropZone dropZone, DropShip.Properties properties)
  {
    this.map = map;
    this.dropZone = dropZone;
    this.properties = properties;
  }

  private float DropAngle { get; set; }

  public IntVec3 Origin => this.dropZone.from;

  public Faction Faction
  {
    get => this.faction;
    init => this.faction = value;
  }

  public SoundDef FlyoverSoundDef
  {
    get => this.flyoverSoundDef;
    init => this.flyoverSoundDef = value;
  }

  public void Add(IAirDroppable airDroppable) => this.airDroppables.Add(airDroppable);

  public void OnSpawned()
  {
    if (this.FlyoverSoundDef == null)
      return;
    if (this.FlyoverSoundDef.sustain)
      this.sustainer = SoundStarter.TrySpawnSustainer(this.FlyoverSoundDef, SoundInfo.InMap(new TargetInfo(this.Origin, this.map, false), (MaintenanceType) 1));
    else
      SoundStarter.PlayOneShotOnCamera(this.FlyoverSoundDef, this.map);
  }

  private void RecalculateDropAngle()
  {
    FloatRange dropAngleRange = DropShip.DropAngleRange;
    this.DropAngle = ((FloatRange) ref dropAngleRange).RandomInRange;
  }

  public void Tick()
  {
    this.sustainer?.Maintain();
    if (this.properties.lifetime-- <= 0)
    {
      this.DropAll();
      this.Destroy();
    }
    if (this.properties.delayDropByTicks-- > 0 || this.properties.ticksToNextDrop-- > 0)
      return;
    this.TryDropSingle();
    this.properties.ticksToNextDrop = this.properties.ticksBetweenDrops;
  }

  private void TryDropSingle()
  {
    if (this.airDroppables.Count == 0)
      return;
    int index = this.dropZone.dropPoints.Count - this.airDroppables.Count;
    IAirDroppable airDroppable = GenCollection.Pop<IAirDroppable>(this.airDroppables);
    IntVec3 dropPoint = this.dropZone.dropPoints[index];
    if (!DropShip.TryDropNear(airDroppable, this.map, dropPoint, this.DropAngle))
      airDroppable.OnFailureToDrop(this.map, dropPoint);
    else
      airDroppable.OnDropped(this.map, dropPoint);
  }

  private void DropAll()
  {
    while (this.airDroppables.Count > 0)
      this.TryDropSingle();
  }

  private void Destroy() => this.map.GetCachedMapComponent<AirdropManager>().Remove(this);

  void IExposable.ExposeData()
  {
    Scribe_References.Look<Map>(ref this.map, "map", false);
    Scribe_References.Look<Faction>(ref this.faction, "faction", false);
    Scribe_Defs.Look<SoundDef>(ref this.flyoverSoundDef, "flyoverSoundDef");
    Scribe_Deep.Look<DropShip.Properties>(ref this.properties, "properties", Array.Empty<object>());
    Scribe_Deep.Look<DropZone>(ref this.dropZone, "dropZone", Array.Empty<object>());
    Scribe_Collections.Look<IAirDroppable>(ref this.airDroppables, "airDroppables", (LookMode) 2, Array.Empty<object>());
    if (Scribe.mode != 4)
      return;
    this.RecalculateDropAngle();
  }

  private static bool TryDropNear(
    IAirDroppable airDroppable,
    Map map,
    IntVec3 dropCenter,
    float angle)
  {
    IntVec3 intVec3;
    if (!DropCellFinder.TryFindDropSpotNear(dropCenter, map, ref intVec3, false, true, 9, true, new IntVec2?(), false) || !((IntVec3) ref intVec3).IsValid || !GenGrid.InBounds(intVec3, map))
      return false;
    IAirDroppable airDroppable1 = airDroppable;
    double num = (double) angle;
    FloatRange extraAngleVariance = DropShip.ExtraAngleVariance;
    double randomInRange = (double) ((FloatRange) ref extraAngleVariance).RandomInRange;
    double angle1 = num + randomInRange;
    GenSpawn.Spawn((Thing) AirdropSkyfallerMaker.MakeAirdrop(airDroppable1, (float) angle1), intVec3, map, (WipeMode) 0);
    return true;
  }

  public record Properties() : IExposable
  {
    public required int lifetime;
    public int ticksBetweenDrops;
    public int delayDropByTicks;
    public int ticksToNextDrop;

    void IExposable.ExposeData()
    {
      Scribe_Values.Look<int>(ref this.lifetime, "lifetime", 0, false);
      Scribe_Values.Look<int>(ref this.ticksBetweenDrops, "ticksBetweenDrops", 0, false);
      Scribe_Values.Look<int>(ref this.delayDropByTicks, "delayDropByTicks", 0, false);
      Scribe_Values.Look<int>(ref this.ticksToNextDrop, "ticksToNextDrop", 0, false);
    }

    [CompilerGenerated]
    protected virtual bool PrintMembers(
    #nullable enable
    StringBuilder builder)
    {
      RuntimeHelpers.EnsureSufficientExecutionStack();
      builder.Append("lifetime = ");
      builder.Append(this.lifetime.ToString());
      builder.Append(", ticksBetweenDrops = ");
      builder.Append(this.ticksBetweenDrops.ToString());
      builder.Append(", delayDropByTicks = ");
      builder.Append(this.delayDropByTicks.ToString());
      builder.Append(", ticksToNextDrop = ");
      builder.Append(this.ticksToNextDrop.ToString());
      return true;
    }

    [CompilerGenerated]
    public override int GetHashCode()
    {
      return (((EqualityComparer<Type>.Default.GetHashCode(this.EqualityContract) * -1521134295 + EqualityComparer<int>.Default.GetHashCode(this.lifetime)) * -1521134295 + EqualityComparer<int>.Default.GetHashCode(this.ticksBetweenDrops)) * -1521134295 + EqualityComparer<int>.Default.GetHashCode(this.delayDropByTicks)) * -1521134295 + EqualityComparer<int>.Default.GetHashCode(this.ticksToNextDrop);
    }

    [CompilerGenerated]
    public virtual bool Equals(DropShip.Properties? other)
    {
      if ((object) this == (object) other)
        return true;
      return (object) other != null && this.EqualityContract == other.EqualityContract && EqualityComparer<int>.Default.Equals(this.lifetime, other.lifetime) && EqualityComparer<int>.Default.Equals(this.ticksBetweenDrops, other.ticksBetweenDrops) && EqualityComparer<int>.Default.Equals(this.delayDropByTicks, other.delayDropByTicks) && EqualityComparer<int>.Default.Equals(this.ticksToNextDrop, other.ticksToNextDrop);
    }

    [CompilerGenerated]
    [SetsRequiredMembers]
    protected Properties(DropShip.Properties original)
    {
      this.lifetime = original.lifetime;
      this.ticksBetweenDrops = original.ticksBetweenDrops;
      this.delayDropByTicks = original.delayDropByTicks;
      this.ticksToNextDrop = original.ticksToNextDrop;
    }
  }
}
