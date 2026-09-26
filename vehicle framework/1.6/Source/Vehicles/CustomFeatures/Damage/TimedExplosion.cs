// Decompiled with JetBrains decompiler
// Type: Vehicles.TimedExplosion
// Assembly: Vehicles, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 81BFDC99-D8AD-42E1-8470-2F73E7836B4A
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\Vehicles.dll

using HarmonyLib;
using JetBrains.Annotations;
using RimWorld;
using SmashTools.Rendering;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using UnityEngine;
using Verse;
using Verse.AI;
using Verse.Sound;

#nullable disable
namespace Vehicles;

[PublicAPI]
[StaticConstructorOnStartup]
public class TimedExplosion : IExposable, IParallelRenderer
{
  private const float WickerFlickerTicks = 3f;
  private const int ExplosionNotificationInterval = 60;
  private static readonly int PawnNotifyCellCount = GenRadial.NumCellsInRadius(4.5f);
  private static readonly Material WickMaterialA = MaterialPool.MatFrom("Things/Special/BurningWickA", ShaderDatabase.MetaOverlay);
  private static readonly Material WickMaterialB = MaterialPool.MatFrom("Things/Special/BurningWickB", ShaderDatabase.MetaOverlay);
  private static readonly float WickAltitude = Altitudes.AltitudeFor((AltitudeLayer) 39);
  private static readonly MethodInfo Notify_DangerousExploderAboutToExplode = AccessTools.Method(typeof (Pawn_MindState), nameof (Notify_DangerousExploderAboutToExplode), (System.Type[]) null, (System.Type[]) null);
  private int ticksLeft;
  public VehiclePawn vehicle;
  private TimedExplosion.PreRenderResults results;
  private TimedExplosion.Data data;
  private readonly DrawOffsets drawOffsets;
  private Sustainer wickSoundSustainer;
  private readonly List<Pawn> pawnsNotifiedOfExplosion = new List<Pawn>();
  public Action<TimedExplosion> explosionCallback;

  bool IParallelRenderer.IsDirty { get; set; }

  public bool Active { get; private set; }

  public TimedExplosion(VehiclePawn vehicle, TimedExplosion.Data data, DrawOffsets drawOffsets = null)
  {
    this.vehicle = vehicle;
    this.drawOffsets = drawOffsets;
    this.data = data;
    this.ticksLeft = data.wickTicks;
    this.Start();
  }

  private IntVec3 AdjustedCell
  {
    get
    {
      IntVec2 intVec2 = this.data.cell.MirrorRotatedBy(((Thing) this.vehicle).Rotation, ((BuildableDef) this.vehicle.VehicleDef).Size);
      IntVec3 adjustedCell;
      ((IntVec3) ref adjustedCell).\u002Ector(intVec2.x + ((Thing) this.vehicle).Position.x, 0, intVec2.z + ((Thing) this.vehicle).Position.z);
      return adjustedCell;
    }
  }

  public void DynamicDrawPhaseAt(DrawPhase phase, in TransformData transformData, bool forceDraw = false)
  {
    switch ((int) phase)
    {
      case 0:
        break;
      case 1:
        this.results = this.ParallelGetPreRenderResults(ref transformData, forceDraw);
        break;
      case 2:
        if (!this.results.valid)
          this.results = this.ParallelGetPreRenderResults(ref transformData, forceDraw);
        this.Draw();
        this.results = new TimedExplosion.PreRenderResults();
        break;
      default:
        throw new NotImplementedException();
    }
  }

  private TimedExplosion.PreRenderResults ParallelGetPreRenderResults(
    [RequiresLocation, In] ref TransformData transformData,
    bool forceDraw = false)
  {
    TimedExplosion.PreRenderResults preRenderResults = new TimedExplosion.PreRenderResults()
    {
      valid = true,
      draw = true,
      material = (double) (((Thing) this.vehicle).thingIDNumber + Find.TickManager.TicksGame) % 6.0 < 3.0 ? TimedExplosion.WickMaterialA : TimedExplosion.WickMaterialB
    };
    Vector3 position = transformData.position;
    position.y = TimedExplosion.WickAltitude;
    Vector3 vector3_1 = position;
    if (this.drawOffsets != null)
    {
      Vector3 vector3_2 = this.drawOffsets.OffsetFor(transformData.orientation);
      vector3_1 = Vector3.op_Addition(vector3_1, vector3_2);
    }
    else
    {
      IntVec2 intVec2 = this.data.cell.MirrorRotatedBy((Rot4) transformData.orientation, ((BuildableDef) this.vehicle.VehicleDef).Size);
      IntVec3 toIntVec3 = ((IntVec2) ref intVec2).ToIntVec3;
      Vector3 vector3_3 = Vector3Utility.RotatedBy(((IntVec3) ref toIntVec3).ToVector3(), transformData.orientation.AsRotationAngle);
      // ISSUE: explicit constructor call
      ((Vector3) ref vector3_1).\u002Ector(vector3_1.x + vector3_3.x, vector3_1.y, vector3_1.z + vector3_3.z);
    }
    preRenderResults.matrix = Matrix4x4.TRS(vector3_1, Quaternion.identity, Vector3.one);
    return preRenderResults;
  }

  private void Draw()
  {
    if (!this.Active || !this.results.draw)
      return;
    Graphics.DrawMesh(MeshPool.plane20, this.results.matrix, this.results.material, 0);
  }

  private void Start()
  {
    if (this.Active)
      return;
    this.Active = true;
    this.StartWickSustainer();
    this.NotifyPawnsOfExplosion();
  }

  private void End()
  {
    this.Active = false;
    this.wickSoundSustainer?.End();
    foreach (Pawn pawn in this.pawnsNotifiedOfExplosion)
    {
      if (pawn.mindState != null && pawn.mindState.knownExploder == this.vehicle)
        pawn.mindState.knownExploder = (Thing) null;
    }
  }

  public bool Tick()
  {
    if (!this.Active || !((Thing) this.vehicle).Spawned)
      return false;
    if (this.ticksLeft % 60 == 0)
      this.NotifyPawnsOfExplosion();
    this.UpdateWick();
    if (this.ticksLeft <= 0)
    {
      this.Explode();
      return false;
    }
    --this.ticksLeft;
    return true;
  }

  private void NotifyPawnsOfExplosion()
  {
    if (!this.data.notifyNearbyPawns)
      return;
    for (int index = 0; index < TimedExplosion.PawnNotifyCellCount; ++index)
    {
      IntVec3 intVec3 = IntVec3.op_Addition(this.AdjustedCell, GenRadial.RadialPattern[index]);
      if (GenGrid.InBounds(intVec3, ((Thing) this.vehicle).MapHeld))
      {
        foreach (Thing thing in GridsUtility.GetThingList(intVec3, ((Thing) this.vehicle).MapHeld))
        {
          if (thing is Pawn pawn && CanNotifyPawn(pawn))
          {
            Room room = RegionAndRoomQuery.GetRoom((Thing) pawn, (RegionType) 15);
            if (room == null || room.CellCount == 1 || room == RegionAndRoomQuery.GetRoom((Thing) this.vehicle, (RegionType) 15) && GenSight.LineOfSightToThing(((Thing) pawn).Position, (Thing) this.vehicle, ((Thing) this.vehicle).MapHeld, true, (Func<IntVec3, bool>) null))
            {
              this.pawnsNotifiedOfExplosion.Add(pawn);
              TimedExplosion.Notify_DangerousExploderAboutToExplode.Invoke((object) pawn.mindState, new object[1]
              {
                (object) this.vehicle
              });
            }
          }
        }
      }
    }

    bool CanNotifyPawn(Pawn pawn)
    {
      return pawn.RaceProps.intelligence >= 2 && this.data.damageDef.ExternalViolenceFor((Thing) pawn);
    }
  }

  private void UpdateWick()
  {
    if (this.wickSoundSustainer == null)
      this.StartWickSustainer();
    else
      this.wickSoundSustainer.Maintain();
  }

  private void StartWickSustainer()
  {
    SoundInfo soundInfo = SoundInfo.InMap(TargetInfo.op_Implicit((Thing) this.vehicle), (MaintenanceType) 1);
    this.wickSoundSustainer = SoundStarter.TrySpawnSustainer(SoundDefOf.HissSmall, soundInfo);
  }

  private void Explode()
  {
    this.End();
    GenExplosion.DoExplosion(this.AdjustedCell, ((Thing) this.vehicle).Map, (float) this.data.radius, this.data.damageDef, (Thing) this.vehicle, this.data.damageAmount, this.data.armorPenetration, (SoundDef) null, (ThingDef) null, (ThingDef) null, (Thing) null, (ThingDef) null, 0.0f, 1, new GasType?(), new float?(), (int) byte.MaxValue, false, (ThingDef) null, 0.0f, 1, 0.0f, false, new float?(), (List<Thing>) null, new FloatRange?(), true, 1f, 0.0f, true, (ThingDef) null, 1f, (SimpleCurve) null, (List<IntVec3>) null, (ThingDef) null, (ThingDef) null);
    Action<TimedExplosion> explosionCallback = this.explosionCallback;
    if (explosionCallback == null)
      return;
    explosionCallback(this);
  }

  public void ExposeData()
  {
    Scribe_References.Look<VehiclePawn>(ref this.vehicle, "vehicle", false);
    Scribe_Deep.Look<TimedExplosion.Data>(ref this.data, "data", Array.Empty<object>());
    Scribe_Values.Look<int>(ref this.ticksLeft, "ticksLeft", 0, false);
  }

  void IParallelRenderer.DynamicDrawPhaseAt(
    DrawPhase phase,
    in TransformData transformData,
    bool forceDraw = false)
  {
    this.DynamicDrawPhaseAt(phase, in transformData, forceDraw);
  }

  private struct PreRenderResults
  {
    public bool valid;
    public bool draw;
    public Material material;
    public Matrix4x4 matrix;
  }

  public class Data : IExposable
  {
    public IntVec2 cell;
    public int wickTicks;
    public int radius;
    public DamageDef damageDef;
    public int damageAmount;
    public float armorPenetration;
    public bool notifyNearbyPawns;

    public Data(
      IntVec2 cell,
      int wickTicks,
      int radius,
      DamageDef damageDef,
      int damageAmount,
      float armorPenetration = -1f,
      bool notifyNearbyPawns = true)
    {
      this.cell = cell;
      this.wickTicks = wickTicks;
      this.radius = radius;
      this.damageDef = damageDef;
      this.damageAmount = damageAmount;
      this.armorPenetration = armorPenetration;
      this.notifyNearbyPawns = notifyNearbyPawns;
      if ((double) this.armorPenetration >= 0.0)
        return;
      this.armorPenetration = damageDef.defaultArmorPenetration;
    }

    public void ExposeData()
    {
      Scribe_Values.Look<IntVec2>(ref this.cell, "cell", new IntVec2(), false);
      Scribe_Values.Look<int>(ref this.wickTicks, "wickTicks", 0, false);
      Scribe_Values.Look<int>(ref this.radius, "radius", 0, false);
      Scribe_Defs.Look<DamageDef>(ref this.damageDef, "damageDef");
      Scribe_Values.Look<int>(ref this.damageAmount, "damageAmount", 0, false);
      Scribe_Values.Look<float>(ref this.armorPenetration, "armorPenetration", -1f, false);
      Scribe_Values.Look<bool>(ref this.notifyNearbyPawns, "notifyNearbyPawns", false, false);
    }
  }
}
