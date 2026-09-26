// Decompiled with JetBrains decompiler
// Type: Vehicles.World.VehicleCaravan
// Assembly: Vehicles, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 81BFDC99-D8AD-42E1-8470-2F73E7836B4A
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\Vehicles.dll

using JetBrains.Annotations;
using RimWorld;
using RimWorld.Planet;
using SmashTools;
using SmashTools.Targeting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;
using Verse.Sound;

#nullable disable
namespace Vehicles.World;

[PublicAPI]
[StaticConstructorOnStartup]
public class VehicleCaravan : 
  Caravan,
  IVehicleWorldObject,
  IThingHolder,
  ITargeterSource<GlobalTargetInfo, ArrivalOption>,
  ILauncher
{
  private const int RepairMothballTicks = 300;
  private static readonly Dictionary<VehicleDef, int> VehicleCounts = new Dictionary<VehicleDef, int>();
  private static readonly Color PlayerCaravanColor = new Color(1f, 0.863f, 0.33f);
  private static readonly MaterialPropertyBlock PropertyBlock = new MaterialPropertyBlock();
  private static readonly Dictionary<ThingDef, Material> Materials = new Dictionary<ThingDef, Material>();
  private static readonly StringBuilder ExplanationBuilder = new StringBuilder();
  public VehicleCaravan_PathFollower vehiclePather;
  public VehicleCaravanTweener vehicleTweener;
  private VehiclePawn leadVehicle;
  private bool initialized;
  private bool repairing;
  private List<Pawn> allPawns = new List<Pawn>();
  private List<VehiclePawn> vehicles = new List<VehiclePawn>();
  private List<Pawn> dismountedPawns = new List<Pawn>();

  public VehicleCaravan()
  {
    this.vehiclePather = new VehicleCaravan_PathFollower(this);
    this.vehicleTweener = new VehicleCaravanTweener(this);
  }

  private bool RecachePawnLists { get; set; } = true;

  public float ConstructionAverage { get; private set; }

  public bool VehiclesNeedRepairs { get; private set; }

  public virtual Vector3 DrawPos => this.vehicleTweener.TweenedPos;

  bool ITargeterSource<GlobalTargetInfo, ArrivalOption>.TargeterValid
  {
    get
    {
      if (((WorldObject) this).Destroyed)
        return false;
      VehiclePawn leadVehicle = this.LeadVehicle;
      return leadVehicle != null && !((Thing) leadVehicle).Spawned && !((Thing) leadVehicle).Destroyed;
    }
  }

  public Vector3 Origin
  {
    get
    {
      PlanetTile tile = ((WorldObject) this).Tile;
      return !((PlanetTile) ref tile).Valid ? ((WorldObject) this).DrawPos : Find.WorldGrid.GetTileCenter(((WorldObject) this).Tile);
    }
  }

  public bool CanDismount => true;

  public bool AerialVehicle
  {
    get
    {
      VehiclePawn vehiclePawn = this.vehicles.FirstOrDefault<VehiclePawn>();
      return vehiclePawn != null && vehiclePawn.VehicleDef.type == VehicleType.Air;
    }
  }

  public IEnumerable<VehiclePawn> Vehicles => (IEnumerable<VehiclePawn>) this.vehicles;

  public IEnumerable<Pawn> DismountedPawns => (IEnumerable<Pawn>) this.dismountedPawns;

  public List<VehiclePawn> VehiclesListForReading => this.vehicles;

  public List<Pawn> DismountedPawnsListForReading => this.dismountedPawns;

  public List<Pawn> AllPawnsAndVehiclePassengers => this.allPawns;

  public bool Repairing
  {
    get => !this.vehiclePather.Moving && this.repairing;
    set => this.repairing = value;
  }

  public VehiclePawn LeadVehicle
  {
    get
    {
      if (this.leadVehicle == null)
        this.leadVehicle = GenCollection.FirstOrDefault<Pawn>(this.PawnsListForReading, (Predicate<Pawn>) (pawn => pawn is VehiclePawn)) as VehiclePawn;
      return this.leadVehicle;
    }
  }

  public virtual Material Material
  {
    get
    {
      VehicleDef vehicleDef = this.LeadVehicle?.VehicleDef;
      if (vehicleDef == null)
        return (Material) null;
      if (!VehicleCaravan.Materials.ContainsKey((ThingDef) vehicleDef))
      {
        Color color = Color.white;
        if (((WorldObject) this).Faction.IsPlayer)
          color = VehicleCaravan.PlayerCaravanColor;
        else if (((WorldObject) this).Faction != null)
          color = ((WorldObject) this).Faction.Color;
        Material material = MaterialPool.MatFrom(VehicleTex.CachedTextureIcons[vehicleDef], ShaderDatabase.WorldOverlayTransparentLit, color, 3550);
        VehicleCaravan.Materials.Add((ThingDef) vehicleDef, material);
      }
      return VehicleCaravan.Materials[(ThingDef) vehicleDef];
    }
  }

  public bool OutOfFuel
  {
    get
    {
      foreach (VehiclePawn vehicle in this.vehicles)
      {
        CompFueledTravel compFueledTravel = vehicle.CompFueledTravel;
        if (compFueledTravel != null && (double) compFueledTravel.Fuel <= 0.0)
          return true;
      }
      return false;
    }
  }

  public bool VehicleCantMove
  {
    get
    {
      return this.VehiclesListForReading.Exists(new Predicate<VehiclePawn>(Immobile));

      static bool Immobile(VehiclePawn vehicle) => !vehicle.CanMoveFinal;
    }
  }

  public int TicksPerMove => VehicleCaravanTicksPerMoveUtility.GetTicksPerMove((Caravan) this);

  public string TicksPerMoveExplanation
  {
    get
    {
      using (new ClearStringOnDispose(VehicleCaravan.ExplanationBuilder))
      {
        VehicleCaravanTicksPerMoveUtility.GetTicksPerMove((Caravan) this, VehicleCaravan.ExplanationBuilder);
        return VehicleCaravan.ExplanationBuilder.ToString();
      }
    }
  }

  public virtual void Draw()
  {
    float averageTileSize = Find.WorldGrid.AverageTileSize;
    float num1 = ExpandableWorldObjectsUtility.TransitionPct((WorldObject) this);
    if (((WorldObject) this).def.expandingIcon && (double) num1 > 0.0)
    {
      Color color = ((WorldObject) this).Material.color;
      float num2 = 1f - num1;
      VehicleCaravan.PropertyBlock.SetColor(ShaderPropertyIDs.Color, new Color(color.r, color.g, color.b, color.a * num2));
      WorldHelper.DrawQuadTangentialToPlanet(((WorldObject) this).DrawPos, 0.7f * averageTileSize, 0.015f, ((WorldObject) this).Material, propertyBlock: VehicleCaravan.PropertyBlock);
    }
    else
      WorldHelper.DrawQuadTangentialToPlanet(((WorldObject) this).DrawPos, 0.7f * averageTileSize, 0.015f, ((WorldObject) this).Material);
  }

  public virtual IEnumerable<Gizmo> GetGizmos()
  {
    VehicleCaravan vehicleCaravan = this;
    // ISSUE: reference to a compiler-generated method
    IEnumerator<Gizmo> enumerator = vehicleCaravan.\u003C\u003En__0().GetEnumerator();
    while (enumerator.MoveNext())
    {
      Gizmo current = enumerator.Current;
      if (!(current is Command command) || Object.op_Implicit((Object) command.icon))
        yield return current;
    }
    enumerator = (IEnumerator<Gizmo>) null;
    if (vehicleCaravan.IsPlayerControlled)
    {
      if (vehicleCaravan.AerialVehicle)
      {
        VehiclePawn vehiclePawn = vehicleCaravan.vehicles.FirstOrDefault<VehiclePawn>();
        Command_Action commandAction = new Command_Action();
        ((Command) commandAction).defaultLabel = TaggedString.op_Implicit(Translator.Translate("CommandLaunchGroup"));
        ((Command) commandAction).defaultDesc = TaggedString.op_Implicit(Translator.Translate("CommandLaunchGroupDesc"));
        ((Command) commandAction).icon = (Texture) TexData.LaunchCommandTex;
        ((Gizmo) commandAction).alsoClickIfOtherInGroupClicked = false;
        commandAction.action = new Action(vehicleCaravan.LaunchAerialVehicle);
        Command_Action gizmo = commandAction;
        string disableReason;
        if (!vehiclePawn.CompVehicleLauncher.CanLaunchWithCargoCapacity(out disableReason))
        {
          ((Gizmo) gizmo).Disabled = true;
          ((Gizmo) gizmo).disabledReason = disableReason;
        }
        yield return (Gizmo) gizmo;
      }
      if (vehicleCaravan.vehiclePather.Moving)
      {
        Command_Toggle gizmo = new Command_Toggle();
        ((Command) gizmo).defaultLabel = TaggedString.op_Implicit(Translator.Translate("CommandPauseCaravan"));
        ((Command) gizmo).defaultDesc = TaggedString.op_Implicit(TranslatorFormattedStringExtensions.Translate("CommandToggleCaravanPauseDesc", NamedArgument.op_Implicit(2f.ToString("0.#")), NamedArgument.op_Implicit(GenText.ToStringPercent(0.3f))));
        ((Command) gizmo).icon = (Texture) TexCommand.PauseCaravan;
        ((Command) gizmo).hotKey = KeyBindingDefOf.Misc1;
        // ISSUE: reference to a compiler-generated method
        gizmo.isActive = new Func<bool>(vehicleCaravan.\u003CGetGizmos\u003Eb__63_0);
        // ISSUE: reference to a compiler-generated method
        gizmo.toggleAction = new Action(vehicleCaravan.\u003CGetGizmos\u003Eb__63_1);
        yield return (Gizmo) gizmo;
      }
      else
      {
        if (vehicleCaravan.VehiclesNeedRepairs)
        {
          Command_Toggle gizmo = new Command_Toggle();
          ((Command) gizmo).defaultLabel = TaggedString.op_Implicit(Translator.Translate("VF_ToggleRepairVehicle"));
          ((Command) gizmo).defaultDesc = TaggedString.op_Implicit(Translator.Translate("VF_ToggleRepairVehicleDesc"));
          ((Command) gizmo).icon = (Texture) VehicleTex.RepairVehicles;
          ((Command) gizmo).hotKey = KeyBindingDefOf.Misc2;
          // ISSUE: reference to a compiler-generated method
          gizmo.isActive = new Func<bool>(vehicleCaravan.\u003CGetGizmos\u003Eb__63_2);
          // ISSUE: reference to a compiler-generated method
          gizmo.toggleAction = new Action(vehicleCaravan.\u003CGetGizmos\u003Eb__63_3);
          yield return (Gizmo) gizmo;
        }
        Command_Action commandAction = new Command_Action();
        ((Command) commandAction).icon = (Texture) VehicleTex.StashVehicle;
        ((Command) commandAction).defaultLabel = TaggedString.op_Implicit(Translator.Translate("VF_CommandDisembark"));
        ((Command) commandAction).defaultDesc = TaggedString.op_Implicit(Translator.Translate("VF_CommandDisembarkDesc"));
        // ISSUE: reference to a compiler-generated method
        commandAction.action = new Action(vehicleCaravan.\u003CGetGizmos\u003Eb__63_4);
        Command_Action gizmo1 = commandAction;
        if (Find.World.Impassable(((WorldObject) vehicleCaravan).Tile))
          ((Gizmo) gizmo1).Disable(TaggedString.op_Implicit(Translator.Translate("VF_CommandDisembarkImpassableBiome")));
        yield return (Gizmo) gizmo1;
      }
    }
    foreach (ThingWithComps thingWithComps in vehicleCaravan.VehiclesListForReading)
    {
      foreach (ThingComp allComp in thingWithComps.AllComps)
      {
        if (allComp is VehicleComp vehicleComp)
        {
          enumerator = vehicleComp.CompCaravanGizmos().GetEnumerator();
          while (enumerator.MoveNext())
            yield return enumerator.Current;
          enumerator = (IEnumerator<Gizmo>) null;
        }
      }
    }
    if (DebugSettings.ShowDevGizmos)
    {
      Command_Action gizmo2 = new Command_Action();
      ((Command) gizmo2).defaultLabel = "Down Random Pawn";
      // ISSUE: reference to a compiler-generated method
      gizmo2.action = new Action(vehicleCaravan.\u003CGetGizmos\u003Eb__63_5);
      yield return (Gizmo) gizmo2;
      Command_Action gizmo3 = new Command_Action();
      ((Command) gizmo3).defaultLabel = "Kill Random Pawn";
      // ISSUE: reference to a compiler-generated method
      gizmo3.action = new Action(vehicleCaravan.\u003CGetGizmos\u003Eb__63_7);
      yield return (Gizmo) gizmo3;
      Command_Action gizmo4 = new Command_Action();
      ((Command) gizmo4).defaultLabel = "Vehicle Dev: Teleport to destination";
      // ISSUE: reference to a compiler-generated method
      gizmo4.action = new Action(vehicleCaravan.\u003CGetGizmos\u003Eb__63_9);
      yield return (Gizmo) gizmo4;
      Command_Action gizmo5 = new Command_Action();
      ((Command) gizmo5).defaultLabel = "Repair all Vehicles";
      // ISSUE: reference to a compiler-generated method
      gizmo5.action = new Action(vehicleCaravan.\u003CGetGizmos\u003Eb__63_10);
      yield return (Gizmo) gizmo5;
    }
  }

  public void LaunchAerialVehicle()
  {
    if (!this.AerialVehicle)
    {
      Trace.Fail("Trying to launch aerial vehicle with caravan not registered as an aerial vehicle.");
      SoundStarter.PlayOneShotOnCamera(SoundDefOf.ClickReject, (Map) null);
    }
    else
    {
      VehiclePawn vehicle = this.vehicles[0];
      using (new VehicleCaravan.RecacheDisabler(this))
        RoleHelper.DistributeAll(in this.vehicles, this.DismountedPawns.ToList<Pawn>());
      this.RecacheVehicles();
      if (((ThingOwner) this.pawns).Count > 1)
      {
        // ISSUE: method pointer
        Find.WindowStack.Add((Window) new Dialog_Confirm(TaggedString.op_Implicit(Translator.Translate("VF_PawnsLeftBehindConfirm")), TaggedString.op_Implicit(Translator.Translate("VF_PawnsLeftBehindConfirmDesc")), new Action((object) this, __methodptr(\u003CLaunchAerialVehicle\u003Eg__LaunchAction\u007C64_0))));
      }
      else
        LaunchAction();
    }

    void LaunchAction()
    {
      CameraJumper.TryJump(CameraJumper.GetWorldTarget(GlobalTargetInfo.op_Implicit((WorldObject) this)), (CameraJumper.MovementMode) 0);
      Find.WorldSelector.ClearSelection();
      new WorldTargeter<ArrivalOption>((ITargeterSource<GlobalTargetInfo, ArrivalOption>) this, this.LeadVehicle.CompFueledTravel != null ? (ITargeterUpdate<GlobalTargetInfo>) new FuelTargetUpdater(this.LeadVehicle, (ILauncher) this) : (ITargeterUpdate<GlobalTargetInfo>) null)
      {
        TargetTexture = TexData.TargeterMouseAttachment
      }.Start();
    }
  }

  TargetValidation ITargeterSource<GlobalTargetInfo, ArrivalOption>.CanTarget(
    GlobalTargetInfo target)
  {
    return this.LeadVehicle.CompVehicleLauncher.CanTarget(target);
  }

  TargeterResult ITargeterSource<GlobalTargetInfo, ArrivalOption>.Select(GlobalTargetInfo target)
  {
    return this.LeadVehicle.CompVehicleLauncher.Select(target);
  }

  void ITargeterSource<GlobalTargetInfo, ArrivalOption>.OnTargetingFinished(
    TargetData<GlobalTargetInfo> targetData,
    ArrivalOption arrivalOption)
  {
    if (arrivalOption.continueWith != null)
      arrivalOption.continueWith(targetData);
    else
      this.Launch(targetData, arrivalOption.arrivalAction);
  }

  public void Launch(TargetData<GlobalTargetInfo> targetData, IArrivalAction arrivalAction)
  {
    this.LeadVehicle.GetOrMakeAerialVehicle().OrderFlyToTiles(targetData.targets.Select<GlobalTargetInfo, FlightNode>((Func<GlobalTargetInfo, FlightNode>) (target => new FlightNode(target))).ToList<FlightNode>(), arrivalAction);
  }

  IEnumerable<ArrivalOption> ILauncher.OptionsAt(GlobalTargetInfo target)
  {
    return this.LeadVehicle.CompVehicleLauncher.OptionsAt(target);
  }

  public void Notify_PawnAdded(Pawn pawn) => this.RecacheVehiclesOrConvertCaravan();

  public void Notify_PawnRemoved(Pawn pawn) => this.RecacheVehiclesOrConvertCaravan();

  public void Notify_VehicleTeleported()
  {
    this.vehicleTweener.ResetTweenedPosToRoot();
    this.vehiclePather.Notify_Teleported_Int();
  }

  public virtual void Notify_Merged(List<Caravan> group)
  {
    base.Notify_Merged(group);
    this.RecacheVehiclesOrConvertCaravan();
  }

  public virtual void Notify_MemberDied(Pawn member)
  {
    if (!((WorldObject) this).Spawned)
      Log.Error("Caravan member died in an unspawned caravan. Unspawned caravans shouldn't be kept for more than a single frame.");
    if (!this.PawnsListForReading.NotNullAndAny<Pawn>((Predicate<Pawn>) (x => x is VehiclePawn vehiclePawn && !x.Dead && vehiclePawn.AllPawnsAboard.NotNullAndAny<Pawn>((Predicate<Pawn>) (pawn => pawn != member && this.IsOwner(pawn))))))
    {
      this.RemovePawn(member);
      if (((WorldObject) this).Faction == Faction.OfPlayer)
      {
        LetterStack letterStack = Find.LetterStack;
        TaggedString taggedString1 = Translator.Translate("LetterLabelAllCaravanColonistsDied");
        TaggedString taggedString2 = TranslatorFormattedStringExtensions.Translate("LetterAllCaravanColonistsDied", NamedArgument.op_Implicit(this.Name));
        TaggedString taggedString3 = ((TaggedString) ref taggedString2).CapitalizeFirst();
        LetterDef negativeEvent = LetterDefOf.NegativeEvent;
        LookTargets lookTargets = LookTargets.op_Implicit(new GlobalTargetInfo(((WorldObject) this).Tile));
        letterStack.ReceiveLetter(taggedString1, taggedString3, negativeEvent, lookTargets, (Faction) null, (Quest) null, (List<ThingDef>) null, (string) null, 0, true);
      }
      ((ThingOwner) this.pawns).Clear();
      ((WorldObject) this).Destroy();
    }
    else
    {
      member.Strip(true);
      this.RemovePawn(member);
    }
    this.RecacheStatAverages();
  }

  public void RecacheVehicles()
  {
    if (!this.RecachePawnLists)
      return;
    this.allPawns.Clear();
    this.vehicles.Clear();
    this.dismountedPawns.Clear();
    foreach (Pawn pawn1 in this.pawns.InnerListForReading)
    {
      this.allPawns.Add(pawn1);
      if (pawn1 is VehiclePawn vehiclePawn)
      {
        this.vehicles.Add(vehiclePawn);
        foreach (Pawn pawn2 in vehiclePawn.AllPawnsAboard)
          this.allPawns.Add(pawn2);
        foreach (Thing thing in vehiclePawn.inventory.innerContainer)
        {
          if (thing is Pawn pawn3)
            this.allPawns.Add(pawn3);
        }
      }
      else
        this.dismountedPawns.Add(pawn1);
    }
    this.leadVehicle = (VehiclePawn) null;
  }

  public void RecacheVehiclesOrConvertCaravan()
  {
    if (!this.RecachePawnLists)
      return;
    this.RecacheVehicles();
    this.ValidateCaravanType();
    this.RecacheStatAverages();
  }

  public void RecacheStatAverages()
  {
    float num1 = 0.0f;
    int num2 = 0;
    foreach (Pawn pawn in this.PawnsListForReading)
    {
      if (pawn.IsColonistPlayerControlled || pawn.IsColonyMechPlayerControlled)
      {
        num1 += StatExtension.GetStatValue((Thing) pawn, StatDefOf.ConstructionSpeed, true, -1);
        ++num2;
      }
    }
    float num3 = 1f;
    if (num2 > 0)
      num3 = num1 / (float) num2;
    this.ConstructionAverage = num3;
    this.VehiclesNeedRepairs = GenCollection.Any<VehiclePawn>(this.vehicles, (Predicate<VehiclePawn>) (vehicle => GenCollection.Any<VehicleComponent>(vehicle.statHandler.ComponentsPrioritized, (Predicate<VehicleComponent>) (c => (double) c.HealthPercent < 1.0))));
  }

  public virtual void PostInit()
  {
    this.initialized = true;
    this.RecacheVehiclesOrConvertCaravan();
  }

  private void ValidateCaravanType()
  {
    if (!this.initialized || !GenList.NullOrEmpty<VehiclePawn>((IList<VehiclePawn>) this.vehicles))
      return;
    Debug.Message($"VehicleCaravan {this} has no more vehicles. Converting to normal caravan. Pawns={string.Join(", ", this.pawns.InnerListForReading.Select<Pawn, string>((Func<Pawn, string>) (pawn => ((Entity) pawn).Label)))}");
    List<Pawn> list = this.pawns.InnerListForReading.ToList<Pawn>();
    if (list.Count > 0)
    {
      using (new VehicleCaravan.RecacheDisabler(this))
      {
        this.RemoveAllPawns();
        CaravanMaker.MakeCaravan((IEnumerable<Pawn>) list, ((WorldObject) this).Faction, ((WorldObject) this).Tile, true);
      }
    }
    if (((WorldObject) this).Destroyed)
      return;
    ((WorldObject) this).Destroy();
  }

  public virtual void Destroy()
  {
    if (((WorldObject) this).Destroyed)
      return;
    this.RecacheVehicles();
    using (new VehicleCaravan.RecacheDisabler(this))
    {
      foreach (VehiclePawn vehicle in this.vehicles)
      {
        int count = vehicle.AllPawnsAboard.Count;
        while (--count >= 0)
        {
          Pawn pawn = vehicle.AllPawnsAboard[count];
          vehicle.RemovePawn(pawn);
          if (!WorldPawnsUtility.IsWorldPawn(pawn) && !(pawn is VehiclePawn))
            Find.WorldPawns.PassToWorld(pawn, (PawnDiscardDecideMode) 0);
        }
        if (!((Thing) vehicle).Destroyed)
          ((Thing) vehicle).Destroy((DestroyMode) 0);
      }
      ((WorldObject) this).Destroy();
    }
  }

  public virtual string GetInspectString()
  {
    StringBuilder stringBuilder = new StringBuilder();
    int num1 = 0;
    int num2 = 0;
    int num3 = 0;
    int num4 = 0;
    int num5 = 0;
    int num6 = 0 + 1;
    foreach (Pawn pawn in this.PawnsListForReading)
    {
      if (pawn is VehiclePawn)
        ++num6;
      if (pawn.IsColonist)
        ++num1;
      if (pawn.RaceProps.Animal)
        ++num2;
      if (pawn.IsPrisoner)
        ++num3;
      if (pawn.Downed)
        ++num4;
      if (pawn.InMentalState)
        ++num5;
    }
    if (num6 >= 1)
    {
      VehicleCaravan.VehicleCounts.Clear();
      VehicleDef vehicleDef1;
      int num7;
      foreach (VehiclePawn vehiclePawn in this.VehiclesListForReading)
      {
        if (!VehicleCaravan.VehicleCounts.TryAdd(vehiclePawn.VehicleDef, 1))
        {
          Dictionary<VehicleDef, int> vehicleCounts = VehicleCaravan.VehicleCounts;
          vehicleDef1 = vehiclePawn.VehicleDef;
          num7 = vehicleCounts[vehicleDef1]++;
        }
      }
      foreach (KeyValuePair<VehicleDef, int> vehicleCount in VehicleCaravan.VehicleCounts)
      {
        vehicleCount.Deconstruct(ref vehicleDef1, ref num7);
        VehicleDef vehicleDef2 = vehicleDef1;
        int num8 = num7;
        stringBuilder.Append($"{num8} {((Def) vehicleDef2).LabelCap}, ");
      }
      VehicleCaravan.VehicleCounts.Clear();
    }
    stringBuilder.Append(TaggedString.op_Implicit(TranslatorFormattedStringExtensions.Translate("CaravanColonistsCount", NamedArgument.op_Implicit(num1), NamedArgument.op_Implicit(num1 != 1 ? Faction.OfPlayer.def.pawnsPlural : Faction.OfPlayer.def.pawnSingular))));
    if (num2 == 1)
      stringBuilder.Append(TaggedString.op_Implicit(TaggedString.op_Addition(", ", Translator.Translate("CaravanAnimal"))));
    else if (num2 > 1)
      stringBuilder.Append(TaggedString.op_Implicit(TaggedString.op_Addition(", ", TranslatorFormattedStringExtensions.Translate("CaravanAnimalsCount", NamedArgument.op_Implicit(num2)))));
    if (num3 == 1)
      stringBuilder.Append(TaggedString.op_Implicit(TaggedString.op_Addition(", ", Translator.Translate("CaravanPrisoner"))));
    else if (num3 > 1)
      stringBuilder.Append(TaggedString.op_Implicit(TaggedString.op_Addition(", ", TranslatorFormattedStringExtensions.Translate("CaravanPrisonersCount", NamedArgument.op_Implicit(num3)))));
    stringBuilder.AppendLine();
    if (num5 > 0)
      stringBuilder.Append(TaggedString.op_Implicit(TranslatorFormattedStringExtensions.Translate("CaravanPawnsInMentalState", NamedArgument.op_Implicit(num5))));
    if (num4 > 0)
    {
      if (num5 > 0)
        stringBuilder.Append(", ");
      stringBuilder.Append(TaggedString.op_Implicit(TranslatorFormattedStringExtensions.Translate("CaravanPawnsDowned", NamedArgument.op_Implicit(num4))));
    }
    VehiclePawn vehiclePawn1 = (VehiclePawn) null;
    string str1 = (string) null;
    foreach (VehiclePawn vehiclePawn2 in this.VehiclesListForReading)
    {
      if (vehiclePawn1 == null)
      {
        if (!vehiclePawn2.CanMove)
        {
          vehiclePawn1 = vehiclePawn2;
          str1 = TaggedString.op_Implicit(TranslatorFormattedStringExtensions.Translate("VF_VehicleUnableToMove", NamedArgument.op_Implicit((Thing) vehiclePawn2)));
        }
        else if (!vehiclePawn2.HasEnoughOperators)
        {
          vehiclePawn1 = vehiclePawn2;
          str1 = TaggedString.op_Implicit(Translator.Translate("VF_NotEnoughToOperate"));
        }
      }
      foreach (ThingComp allComp in ((ThingWithComps) vehiclePawn2).AllComps)
      {
        if (allComp is VehicleComp vehicleComp)
          vehicleComp.CompCaravanInspectString(stringBuilder);
      }
    }
    if (num5 > 0 || num4 > 0)
      stringBuilder.AppendLine();
    if (this.vehiclePather.Moving)
    {
      if (this.vehiclePather.ArrivalAction != null)
        stringBuilder.Append(this.vehiclePather.ArrivalAction.ReportString);
      else if (this.HasBoat())
        stringBuilder.Append(TaggedString.op_Implicit(Translator.Translate("VF_Sailing")));
      else
        stringBuilder.Append(TaggedString.op_Implicit(Translator.Translate("CaravanTraveling")));
    }
    else
    {
      Settlement settlement = CaravanVisitUtility.SettlementVisitedNow((Caravan) this);
      stringBuilder.Append(TaggedString.op_Implicit(settlement != null ? TranslatorFormattedStringExtensions.Translate("CaravanVisiting", NamedArgument.op_Implicit(((WorldObject) settlement).Label)) : Translator.Translate("CaravanWaiting")));
    }
    if (this.vehiclePather.Moving)
    {
      float num9 = (float) VehicleCaravanPathingHelper.EstimatedTicksToArrive(this, true) / 60000f;
      stringBuilder.AppendLine();
      stringBuilder.Append(TaggedString.op_Implicit(TranslatorFormattedStringExtensions.Translate("CaravanEstimatedTimeToDestination", NamedArgument.op_Implicit(num9.ToString("0.#")))));
    }
    if (vehiclePawn1 != null)
    {
      stringBuilder.AppendLine();
      stringBuilder.Append(str1);
    }
    else if (this.AllOwnersDowned)
    {
      stringBuilder.AppendLine();
      stringBuilder.Append(TaggedString.op_Implicit(Translator.Translate("AllCaravanMembersDowned")));
    }
    else if (this.AllOwnersHaveMentalBreak)
    {
      stringBuilder.AppendLine();
      stringBuilder.Append(TaggedString.op_Implicit(Translator.Translate("AllCaravanMembersMentalBreak")));
    }
    else if (this.ImmobilizedByMass)
    {
      stringBuilder.AppendLine();
      stringBuilder.Append(TaggedString.op_Implicit(Translator.Translate("CaravanImmobilizedByMass")));
    }
    string str2;
    if (this.needs.AnyPawnOutOfFood(ref str2))
    {
      stringBuilder.AppendLine();
      stringBuilder.Append(TaggedString.op_Implicit(Translator.Translate("CaravanOutOfFood")));
      if (!GenText.NullOrEmpty(str2))
      {
        stringBuilder.Append(" ");
        stringBuilder.Append(str2);
        stringBuilder.Append(".");
      }
    }
    if (!this.vehiclePather.MovingNow)
    {
      int usedBedCount = this.beds.GetUsedBedCount();
      stringBuilder.AppendLine();
      stringBuilder.Append(CaravanBedUtility.AppendUsingBedsLabel(TaggedString.op_Implicit(Translator.Translate("CaravanResting")), usedBedCount));
    }
    else
    {
      string inspectStringLine1 = this.carryTracker.GetInspectStringLine();
      if (!GenText.NullOrEmpty(inspectStringLine1))
      {
        stringBuilder.AppendLine();
        stringBuilder.Append(inspectStringLine1);
      }
      string inspectStringLine2 = this.beds.GetInBedForMedicalReasonsInspectStringLine();
      if (!GenText.NullOrEmpty(inspectStringLine2))
      {
        stringBuilder.AppendLine();
        stringBuilder.Append(inspectStringLine2);
      }
    }
    return stringBuilder.ToString();
  }

  public virtual void DrawExtraSelectionOverlays()
  {
    if (this.IsPlayerControlled && this.vehiclePather.curPath != null)
      this.vehiclePather.curPath.DrawPath((Caravan) this);
    this.gotoMote.RenderMote();
  }

  protected virtual void Tick()
  {
    ((WorldObject) this).Tick();
    this.vehiclePather.PatherTick();
    if (this.vehiclePather.MovingNow)
    {
      foreach (VehiclePawn vehicle in this.vehicles)
        vehicle.CompFueledTravel?.ConsumeFuelWorld();
    }
    else
    {
      int ticksGame = Find.TickManager.TicksGame;
      if (ticksGame % 300 == 0)
        this.RecacheStatAverages();
      if (!this.VehiclesNeedRepairs || !this.Repairing || ticksGame % 300 != 0)
        return;
      this.RepairAllVehicles();
    }
  }

  public void RepairAllVehicles()
  {
    this.LearnSkill(SkillDefOf.Construction, 300);
    foreach (VehiclePawn vehicle in this.vehicles)
    {
      VehicleComponent vehicleComponent = GenCollection.FirstOrDefault<VehicleComponent>(vehicle.statHandler.ComponentsPrioritized, (Predicate<VehicleComponent>) (c => (double) c.HealthPercent < 1.0));
      if (vehicleComponent != null)
      {
        vehicleComponent.HealComponent((float) ((double) vehicle.GetStatValue(VehicleStatDefOf.RepairRate) * 300.0 / 60.0));
        break;
      }
    }
  }

  private void LearnSkill(SkillDef skillDef, int mothballTicks)
  {
    foreach (Pawn pawn in this.PawnsListForReading)
    {
      if (pawn.IsColonistPlayerControlled || pawn.IsColonyMechPlayerControlled)
      {
        pawn.skills?.Learn(skillDef, 0.08f * (float) mothballTicks, false, false);
        pawn.records.Increment(RecordDefOf.ThingsRepaired);
      }
    }
  }

  public virtual void PostRemove()
  {
    base.PostRemove();
    this.vehiclePather.StopDead();
  }

  public virtual void SpawnSetup()
  {
    base.SpawnSetup();
    this.RecacheVehicles();
    this.vehicleTweener.ResetTweenedPosToRoot();
    foreach (VehiclePawn vehicle in this.vehicles)
      vehicle.RegisterEvents();
  }

  public virtual void ExposeData()
  {
    base.ExposeData();
    Scribe_Deep.Look<VehicleCaravan_PathFollower>(ref this.vehiclePather, "vehiclePather", new object[1]
    {
      (object) this
    });
    Scribe_Values.Look<bool>(ref this.repairing, "repairing", false, false);
    if (Scribe.mode != 4)
      return;
    this.initialized = true;
  }

  public readonly struct RecacheDisabler : IDisposable
  {
    private readonly bool oldValue;
    private readonly VehicleCaravan caravan;

    public RecacheDisabler(VehicleCaravan caravan)
    {
      this.caravan = caravan;
      this.oldValue = caravan.RecachePawnLists;
      caravan.RecachePawnLists = false;
    }

    void IDisposable.Dispose() => this.caravan.RecachePawnLists = this.oldValue;
  }
}
