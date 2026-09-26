// Decompiled with JetBrains decompiler
// Type: Vehicles.JobDriver_IdleVehicle
// Assembly: Vehicles, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 81BFDC99-D8AD-42E1-8470-2F73E7836B4A
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\Vehicles.dll

using JetBrains.Annotations;
using RimWorld;
using SmashTools;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Vehicles.Compatibility;
using Verse;
using Verse.AI;

#nullable disable
namespace Vehicles;

[PublicAPI]
public class JobDriver_IdleVehicle : JobDriver
{
  private const int FishingTicksMin = 2000;
  private const int FishingTicksMax = 6000;
  private const int FishingTicksOdyssey = 6800;
  private static readonly IntRange MoteIntervalRange = new IntRange(240 /*0xF0*/, 280);

  protected VehiclePawn Vehicle
  {
    get
    {
      LocalTargetInfo targetA = this.TargetA;
      return ((LocalTargetInfo) ref targetA).Thing as VehiclePawn;
    }
  }

  private int TicksToFish
  {
    get
    {
      FishingProperties fishingProperties = this.Vehicle.VehicleDef.fishingProperties;
      int num1 = (int?) fishingProperties?.animalSkillOverride ?? this.Vehicle.AverageSkillOfCapablePawns(SkillDefOf.Animals);
      float num2 = ModsConfig.OdysseyActive ? (float) Mathf.RoundToInt(6000f / StatExtension.GetStatValue((Thing) this.pawn, StatDefOf.FishingSpeed, true, -1)) : Mathf.Lerp((float) num1, 2000f, 6000f);
      if (fishingProperties != null)
        num2 *= fishingProperties.fishingTicksModifier;
      return Mathf.CeilToInt(num2);
    }
  }

  public virtual void Notify_PatherFailed() => this.EndJobWith((JobCondition) 4);

  public virtual bool TryMakePreToilReservations(bool errorOnFailed) => true;

  private static Vector3 GetRandomFishingPosition(VehiclePawn vehicle, out float angle)
  {
    FishingProperties fishingProperties = vehicle.VehicleDef.fishingProperties;
    List<IntVec2> fishingCells = fishingProperties?.fishingCells;
    IntVec2 intVec2;
    if (fishingCells != null && fishingCells.Count > 0)
    {
      intVec2 = GenCollection.RandomElement<IntVec2>((IEnumerable<IntVec2>) fishingProperties.fishingCells);
    }
    else
    {
      int num = Mathf.FloorToInt((float) ((BuildableDef) vehicle.VehicleDef).Size.x / 2f) + 1;
      // ISSUE: explicit constructor call
      ((IntVec2) ref intVec2).\u002Ector(Rand.Bool ? num : -num, 0);
    }
    float num1 = (float) ((BuildableDef) vehicle.VehicleDef).Size.x / 2f;
    float num2 = (float) ((BuildableDef) vehicle.VehicleDef).Size.z / 2f;
    ref float local = ref angle;
    Rot8 fullRotation = vehicle.FullRotation;
    double asAngle1 = (double) fullRotation.AsAngle;
    local = (float) asAngle1;
    if ((double) Mathf.Abs(intVec2.z) > (double) num2 && intVec2.z < 0)
      angle *= -1f;
    if ((double) Mathf.Abs(intVec2.x) > (double) num1)
      angle += 90f * Mathf.Sign((float) intVec2.x);
    angle = angle.ClampAngle();
    Vector2 vector2Shifted = ((IntVec2) ref intVec2).ToVector2Shifted();
    fullRotation = vehicle.FullRotation;
    double asAngle2 = (double) fullRotation.AsAngle;
    Vector2 vector2 = vector2Shifted.RotatePointClockwise((float) asAngle2);
    return Vector3.op_Addition(((Thing) vehicle).DrawPos, Vector2Utility.ToVector3(vector2));
  }

  protected virtual IEnumerable<Toil> MakeNewToils()
  {
    // ISSUE: reference to a compiler-generated field
    int num = this.\u003C\u003E1__state;
    JobDriver_IdleVehicle driverIdleVehicle = this;
    if (num != 0)
    {
      if (num != 1)
        return false;
      // ISSUE: reference to a compiler-generated field
      this.\u003C\u003E1__state = -1;
      return false;
    }
    // ISSUE: reference to a compiler-generated field
    this.\u003C\u003E1__state = -1;
    ToilFailConditions.FailOnDestroyedOrNull<JobDriver_IdleVehicle>(driverIdleVehicle, (TargetIndex) 1);
    ToilFailConditions.FailOn<JobDriver_IdleVehicle>(driverIdleVehicle, (Func<bool>) (() => !((Thing) this.Vehicle).Spawned));
    int ticksTillFish = int.MaxValue;
    IntRange moteIntervalRange = JobDriver_IdleVehicle.MoteIntervalRange;
    int ticksToThrowMote = ((IntRange) ref moteIntervalRange).RandomInRange;
    // ISSUE: reference to a compiler-generated field
    this.\u003C\u003E2__current = new Toil()
    {
      initAction = (Action) (() =>
      {
        if (((Thing) this.Vehicle).IsBoat())
          ticksTillFish = this.TicksToFish;
        this.Map.pawnDestinationReservationManager.Reserve((Pawn) this.Vehicle, this.job, ((Thing) this.Vehicle).Position);
        this.Vehicle.vehiclePather.StopDead();
      }),
      tickAction = new Action(TickFishAction),
      defaultCompleteMode = (ToilCompleteMode) 5
    };
    // ISSUE: reference to a compiler-generated field
    this.\u003C\u003E1__state = 1;
    return true;

    void TickFishAction()
    {
      if (!this.Vehicle.IsFishing)
        return;
      --ticksTillFish;
      --ticksToThrowMote;
      if (ticksToThrowMote <= 0)
      {
        IntRange moteIntervalRange = JobDriver_IdleVehicle.MoteIntervalRange;
        ticksToThrowMote = ((IntRange) ref moteIntervalRange).RandomInRange;
        float angle;
        Vector3 randomFishingPosition = JobDriver_IdleVehicle.GetRandomFishingPosition(this.Vehicle, out angle);
        randomFishingPosition.y = Altitudes.AltitudeFor((AltitudeLayer) 27);
        MoteThrown mote = (MoteThrown) ThingMaker.MakeThing(ThingDefOf_VehicleMotes.MoteFishingNet, (ThingDef) null);
        ((Mote) mote).exactPosition = randomFishingPosition;
        ((Mote) mote).Scale = 1f;
        ((Mote) mote).rotationRate = Rand.Range(2f, 6f) * (Rand.Bool ? 1f : -1f);
        mote.SetVelocity(angle, 0.55f);
        MoteGenerator.ThrowMote(IntVec3Utility.ToIntVec3(randomFishingPosition), ((Thing) this.Vehicle).Map, mote);
      }
      foreach (Pawn pawn in this.Vehicle.AllPawnsAboard)
        pawn.skills?.Learn(SkillDefOf.Animals, VehicleMod.FishingSkillValue, false, false);
      if (ticksTillFish > 0)
        return;
      ticksTillFish = this.TicksToFish;
      IntVec3 position = ((Thing) this.Vehicle).Position;
      bool rare;
      List<Thing> catchesFor = FishingCompatibility.GetCatchesFor(this.Vehicle, position, ((Thing) this.Vehicle).Map.Biome, out rare);
      if (catchesFor.Count == 0)
        return;
      if (rare)
      {
        ((Thing) this.Vehicle).Map.waterBodyTracker.lastRareCatchTick = Find.TickManager.TicksGame;
        Find.LetterStack.ReceiveLetter(Translator.Translate("LetterLabelRareCatch"), TaggedString.op_Implicit($"{TranslatorFormattedStringExtensions.Translate("LetterTextRareCatch", NamedArgumentUtility.Named((object) this.pawn, "PAWN"))}:\n{GenText.ToLineList(catchesFor.Select<Thing, string>((Func<Thing, string>) (thing => ((Entity) thing).LabelCap)), "  - ", false)}"), LetterDefOf.PositiveEvent, LookTargets.op_Implicit(catchesFor), (Faction) null, (Quest) null, (List<ThingDef>) null, (string) null, 0, true);
      }
      else if (ModsConfig.OdysseyActive)
      {
        int num = catchesFor.Sum<Thing>((Func<Thing, int>) (thing => thing.stackCount));
        ((Thing) this.Vehicle).Map.waterBodyTracker.Notify_Fished(position, (float) num);
        Find.HistoryEventsManager.RecordEvent(new HistoryEvent(HistoryEventDefOf.SlaughteredFish, NamedArgumentUtility.Named((object) this.pawn, HistoryEventArgsNames.Doer)), true);
      }
      foreach (Thing thing in catchesFor)
        this.Vehicle.AddOrTransfer(thing);
    }
  }
}
