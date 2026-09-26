// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.VehiclePawnWithMap
// Assembly: VehicleMapFramework, Version=1.6.562.0, Culture=neutral, PublicKeyToken=null
// MVID: 10A61882-945F-4CFC-9B06-CA8EEF5ADB36
// Assembly location: D:\Programas\steamapps\workshop\content\294100\3426502333\1.6\Assemblies\VehicleMapFramework.dll

using HarmonyLib;
using JetBrains.Annotations;
using LudeonTK;
using RimWorld;
using RimWorld.Planet;
using SmashTools;
using SmashTools.Rendering;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using VehicleMapFramework.VMF_HarmonyPatches;
using Vehicles;
using Vehicles.Rendering;
using Vehicles.World;
using Verse;
using Verse.AI;
using Verse.AI.Group;

#nullable disable
namespace VehicleMapFramework;

[StaticConstructorOnStartup]
public class VehiclePawnWithMap : 
  VehiclePawn,
  IEventManager<MapVehicleEventDef>,
  IAttackTarget,
  ILoadReferenceable
{
  private bool generatingVehicleMap;
  private Map interiorMap;
  private VehicleMapFollower mapFollower;
  public Vector3 cachedDrawPos;
  public Vector3 cachedExactPos;
  private bool allowEnter = true;
  private bool allowExit = true;
  public bool impassableCellsDirty = true;
  public bool mapEdgeCellsDirty = true;
  public bool walkableCellsDirty = true;
  public bool enterPositionsDirty = true;
  private int cellDesignationsDirtyTick;
  private int vehicleCaravanOrStashedVehicleCachedTick;
  internal bool resizeRequest;
  private readonly List<CompVehicleEnterSpot> tmpEnterComps = new List<CompVehicleEnterSpot>();
  private static readonly Material ClipMat = SolidColorMaterials.NewSolidColorMaterial(new Color(0.3f, 0.1f, 0.1f, 0.5f), ShaderDatabase.MetaOverlay);
  private static readonly CachedTexture iconIncreasePriority = new CachedTexture("VehicleMapFramework/UI/IncreasePriority");
  private static readonly CachedTexture iconDecreasePriority = new CachedTexture("VehicleMapFramework/UI/DecreasePriority");
  private static readonly CachedTexture iconAllowEnter = new CachedTexture("VehicleMapFramework/UI/AllowEnter");
  private static readonly CachedTexture iconAllowExit = new CachedTexture("VehicleMapFramework/UI/AllowExit");
  private static readonly CachedTexture iconEye = new CachedTexture("VehicleMapFramework/UI/Eye");
  private static readonly Type t_SectionLayer_Zones = GenTypes.GetTypeInAnyAssembly("Verse.SectionLayer_Zones", "Verse");
  private static readonly FastInvokeHandler DirtyCellDesignationsCache = MethodInvoker.GetHandler(AccessTools.Method(typeof (DesignationManager), nameof (DirtyCellDesignationsCache), (Type[]) null, (Type[]) null), false);
  private static readonly List<DesignationDef> cellDesignations = DefDatabase<DesignationDef>.AllDefs.Where<DesignationDef>((Func<DesignationDef, bool>) (d => d.targetType == 1)).ToList<DesignationDef>();
  internal static readonly AccessTools.FieldRef<MapDrawer, Section[,]> sections = AccessTools.FieldRefAccess<MapDrawer, Section[,]>(nameof (sections));
  [CompilerGenerated]
  private List<IntVec3> \u003CCachedMapEdgeCells\u003Ek__BackingField = new List<IntVec3>();
  [CompilerGenerated]
  private Dictionary<IntVec3, District> \u003CCachedWalkableMapEdgeCells\u003Ek__BackingField = new Dictionary<IntVec3, District>();
  [CompilerGenerated]
  private HashSet<District> \u003CCachedEdgeDistricts\u003Ek__BackingField = new HashSet<District>();
  [CompilerGenerated]
  private List<IntVec3?> \u003CCachedEnterPositions\u003Ek__BackingField = new List<IntVec3?>();
  private FetchedComp<CompNpcVehicleMap> _compNpcVehicleMap;
  private FetchedComp<CompDelayedKill> _compDelayedKill;
  private FetchedComp<CompVehicleDrawOffset> _compVehicleDrawOffset;
  private FetchedComp<VehicleComp> _compVehicleHover;

  EventManager<MapVehicleEventDef> IEventManager<MapVehicleEventDef>.EventRegistry { get; set; }

  public EventManager<MapVehicleEventDef> MapVehicleEventManager
  {
    get => ((IEventManager<MapVehicleEventDef>) this).EventRegistry;
  }

  public VehicleMapProps VehicleMapProps
  {
    get
    {
      return this.\u003CVehicleMapProps\u003Ek__BackingField ?? (this.\u003CVehicleMapProps\u003Ek__BackingField = ((Def) ((Thing) this).def).GetModExtension<VehicleMapProps>());
    }
  }

  public Map VehicleMap
  {
    get
    {
      if (this.interiorMap == null)
        this.GenerateVehicleMap(((Thing) this).Map);
      return this.interiorMap;
    }
  }

  public IntVec3 MapSize { get; private set; }

  public CellRect MapRect => new CellRect(0, 0, this.MapSize.x, this.MapSize.z);

  public Map CurrentLevel
  {
    get
    {
      return this.\u003CCurrentLevel\u003Ek__BackingField ?? (this.\u003CCurrentLevel\u003Ek__BackingField = this.interiorMap);
    }
    set => this.\u003CCurrentLevel\u003Ek__BackingField = value;
  }

  public VehicleMapBlitter VehicleMapBlitter
  {
    get
    {
      return this.\u003CVehicleMapBlitter\u003Ek__BackingField ?? (this.\u003CVehicleMapBlitter\u003Ek__BackingField = new VehicleMapBlitter(this));
    }
  }

  public Command_SelectVehicleMap VehicleMapGizmo
  {
    get
    {
      return this.\u003CVehicleMapGizmo\u003Ek__BackingField ?? (this.\u003CVehicleMapGizmo\u003Ek__BackingField = this.GetVehicleMapGizmo());
    }
  }

  [UsedImplicitly]
  public bool AllowEnter => this.allowEnter;

  [UsedImplicitly]
  public bool AllowExit => this.allowExit;

  [CanBeNull]
  public WorldObject VehicleCaravanOrStashedVehicle
  {
    get
    {
      int ticksGame = GenTicks.TicksGame;
      if (GenTicks.TicksGame == this.vehicleCaravanOrStashedVehicleCachedTick)
        return this.\u003CVehicleCaravanOrStashedVehicle\u003Ek__BackingField;
      this.vehicleCaravanOrStashedVehicleCachedTick = ticksGame;
      this.\u003CVehicleCaravanOrStashedVehicle\u003Ek__BackingField = (WorldObject) ((object) (((Thing) this).ParentHolder as VehicleCaravan) ?? (object) Find.World.GetComponent<VehicleWorldObjectsHolder>().StashedVehicleObject((VehiclePawn) this));
      return this.\u003CVehicleCaravanOrStashedVehicle\u003Ek__BackingField;
    }
  }

  public BoolGrid ImpassableCellGrid
  {
    get
    {
      if (this.impassableCellsDirty)
      {
        this.impassableCellsDirty = false;
        if (this.\u003CImpassableCellGrid\u003Ek__BackingField == null)
          this.\u003CImpassableCellGrid\u003Ek__BackingField = new BoolGrid(this.interiorMap);
        this.\u003CImpassableCellGrid\u003Ek__BackingField.Clear();
        int x = this.MapSize.x;
        int z = this.MapSize.z;
        TerrainGrid terrainGrid = this.interiorMap.terrainGrid;
        for (int index1 = 0; index1 < x; ++index1)
        {
          for (int index2 = 0; index2 < z; ++index2)
          {
            TerrainDef terrainDef = terrainGrid.TerrainAt(index2 * x + index1);
            if (terrainDef != null && ((BuildableDef) terrainDef).passability == 2)
              this.\u003CImpassableCellGrid\u003Ek__BackingField[new IntVec3(index1, 0, index2)] = true;
          }
        }
        List<Thing> thingList = this.interiorMap.listerThings.ThingsOfDef(VMF_DefOf.VMF_VehicleStructureFilled);
        for (int index = 0; index < thingList.Count; ++index)
          this.\u003CImpassableCellGrid\u003Ek__BackingField[thingList[index].Position] = true;
        ModCompat.AsAboveSoBelow.CopyBoolGrid(this, this.\u003CImpassableCellGrid\u003Ek__BackingField);
      }
      return this.\u003CImpassableCellGrid\u003Ek__BackingField;
    }
  }

  public BoolGrid EmptyStructureGrid
  {
    get
    {
      if (this.\u003CEmptyStructureGrid\u003Ek__BackingField != null)
        return this.\u003CEmptyStructureGrid\u003Ek__BackingField;
      this.\u003CEmptyStructureGrid\u003Ek__BackingField = new BoolGrid(this.interiorMap);
      VehicleMapProps vehicleMapProps = this.VehicleMapProps;
      if (vehicleMapProps != null)
      {
        foreach (IntVec2 emptyStructureCell in vehicleMapProps.EmptyStructureCells)
          this.\u003CEmptyStructureGrid\u003Ek__BackingField[((IntVec2) ref emptyStructureCell).ToIntVec3] = true;
        ModCompat.AsAboveSoBelow.CopyBoolGrid(this, this.\u003CEmptyStructureGrid\u003Ek__BackingField);
      }
      return this.\u003CEmptyStructureGrid\u003Ek__BackingField;
    }
  }

  public BoolGrid ExpandableGrid
  {
    get
    {
      if (this.\u003CExpandableGrid\u003Ek__BackingField != null)
        return this.\u003CExpandableGrid\u003Ek__BackingField;
      this.\u003CExpandableGrid\u003Ek__BackingField = new BoolGrid(this.interiorMap);
      VehicleMapProps vehicleMapProps = this.VehicleMapProps;
      if (vehicleMapProps != null)
      {
        foreach (IntVec2 expandableCell in vehicleMapProps.ExpandableCells)
          this.\u003CExpandableGrid\u003Ek__BackingField[((IntVec2) ref expandableCell).ToIntVec3] = true;
        ModCompat.AsAboveSoBelow.CopyBoolGrid(this, this.\u003CExpandableGrid\u003Ek__BackingField);
      }
      return this.\u003CExpandableGrid\u003Ek__BackingField;
    }
  }

  public BoolGrid OutOfBoundsGrid
  {
    get
    {
      if (this.\u003COutOfBoundsGrid\u003Ek__BackingField != null)
        return this.\u003COutOfBoundsGrid\u003Ek__BackingField;
      this.\u003COutOfBoundsGrid\u003Ek__BackingField = new BoolGrid(this.interiorMap);
      VehicleMapProps vehicleMapProps = this.VehicleMapProps;
      if (vehicleMapProps != null)
      {
        foreach (IntVec2 outOfBoundsCell in vehicleMapProps.OutOfBoundsCells)
          this.\u003COutOfBoundsGrid\u003Ek__BackingField[((IntVec2) ref outOfBoundsCell).ToIntVec3] = true;
        ModCompat.AsAboveSoBelow.CopyBoolGrid(this, this.\u003COutOfBoundsGrid\u003Ek__BackingField);
      }
      return this.\u003COutOfBoundsGrid\u003Ek__BackingField;
    }
  }

  public CellRect ValidMapRect
  {
    get
    {
      List<IntVec3> cachedMapEdgeCells = this.CachedMapEdgeCells;
      return this.\u003CValidMapRect\u003Ek__BackingField;
    }
    private set => this.\u003CValidMapRect\u003Ek__BackingField = value;
  }

  public List<IntVec3> CachedMapEdgeCells
  {
    get
    {
      if (this.mapEdgeCellsDirty)
      {
        this.mapEdgeCellsDirty = false;
        this.walkableCellsDirty = true;
        this.enterPositionsDirty = true;
        this.\u003CCachedMapEdgeCells\u003Ek__BackingField.Clear();
        IntVec3 mapSize = this.MapSize;
        CellRect cellRect;
        ((CellRect) ref cellRect).\u002Ector(1, 1, mapSize.x - 1, mapSize.z - 1);
        BoolGrid impassableCellGrid = this.ImpassableCellGrid;
        for (int index = 0; index < 4; ++index)
        {
          Rot4 rot;
          ((Rot4) ref rot).\u002Ector(index);
          Rot4 opposite = ((Rot4) ref rot).Opposite;
          IntVec3 facingCell = ((Rot4) ref opposite).FacingCell;
          foreach (IntVec3 intVec3_1 in cellRect.EdgeRectClockwise(rot))
          {
            IntVec3 intVec3_2 = intVec3_1;
            while (((CellRect) ref cellRect).Contains(intVec3_2) && impassableCellGrid[intVec3_2])
              intVec3_2 = IntVec3.op_Addition(intVec3_2, facingCell);
            if (((CellRect) ref cellRect).Contains(intVec3_2) && !impassableCellGrid[intVec3_2])
              GenCollection.AddUnique<IntVec3>(this.\u003CCachedMapEdgeCells\u003Ek__BackingField, intVec3_2);
          }
        }
        this.ValidMapRect = CellRect.FromCellList((IEnumerable<IntVec3>) this.\u003CCachedMapEdgeCells\u003Ek__BackingField);
      }
      return this.\u003CCachedMapEdgeCells\u003Ek__BackingField;
    }
  }

  public Dictionary<IntVec3, District> CachedWalkableMapEdgeCells
  {
    get
    {
      if (this.walkableCellsDirty)
      {
        List<IntVec3> cachedMapEdgeCells = this.CachedMapEdgeCells;
        this.walkableCellsDirty = false;
        CrossMapReachabilityCache.ClearCacheFor(this.interiorMap);
        this.\u003CCachedWalkableMapEdgeCells\u003Ek__BackingField.Clear();
        this.CachedEdgeDistricts.Clear();
        for (int index = 0; index < cachedMapEdgeCells.Count; ++index)
        {
          IntVec3 key = cachedMapEdgeCells[index];
          if (GenGrid.Walkable(key, this.interiorMap))
          {
            District district = RegionAndRoomQuery.DistirctAtFast(key, this.interiorMap, (RegionType) 14);
            this.\u003CCachedWalkableMapEdgeCells\u003Ek__BackingField[key] = district;
            this.CachedEdgeDistricts.Add(district);
          }
        }
      }
      return this.\u003CCachedWalkableMapEdgeCells\u003Ek__BackingField;
    }
  }

  public HashSet<District> CachedEdgeDistricts
  {
    get
    {
      Dictionary<IntVec3, District> walkableMapEdgeCells = this.CachedWalkableMapEdgeCells;
      return this.\u003CCachedEdgeDistricts\u003Ek__BackingField;
    }
  }

  private List<IntVec3?> CachedEnterPositions
  {
    get
    {
      if (this.enterPositionsDirty)
      {
        List<IntVec3> cachedMapEdgeCells = this.CachedMapEdgeCells;
        this.enterPositionsDirty = false;
        CrossMapReachabilityCache.ClearCacheFor(this.interiorMap);
        this.\u003CCachedEnterPositions\u003Ek__BackingField.Clear();
        for (int index = 0; index < cachedMapEdgeCells.Count; ++index)
          this.\u003CCachedEnterPositions\u003Ek__BackingField.Add(new IntVec3?());
      }
      return this.\u003CCachedEnterPositions\u003Ek__BackingField;
    }
  }

  public IntVec3 GetCachedEnterPosition(int index)
  {
    List<IntVec3?> cachedEnterPositions = this.CachedEnterPositions;
    IntVec3? nullable1 = cachedEnterPositions[index];
    if (nullable1.HasValue)
      return nullable1.Value;
    IntVec3 cachedMapEdgeCell = this.CachedMapEdgeCells[index];
    List<IntVec3?> nullableList = cachedEnterPositions;
    int index1 = index;
    IntVec3? nullable2;
    ref IntVec3? local = ref nullable2;
    IntVec3 intVec3_1 = CrossMapReachabilityUtility.EnterVehiclePosition(new TargetInfo(cachedMapEdgeCell, this.interiorMap, false));
    IntVec3 intVec3_2 = ((IntVec3) ref intVec3_1).IsValid ? intVec3_1 : IntVec3.Invalid;
    local = new IntVec3?(intVec3_2);
    IntVec3? nullable3 = nullable2;
    nullableList[index1] = nullable3;
    return nullable2.Value;
  }

  private void WalkableCellsDirtyIfNeeded(Building building)
  {
    if (!((Thing) building).def.AffectsReachability)
      return;
    CellRect cellRect = GenAdj.OccupiedRect((Thing) building);
    foreach (IntVec3 intVec3 in cellRect)
    {
      if (this.CachedMapEdgeCells.Contains(intVec3))
      {
        this.walkableCellsDirty = true;
        this.enterPositionsDirty = true;
        CrossMapReachabilityCache.ClearCacheFor(this.interiorMap);
        break;
      }
    }
  }

  private void WalkableCellsDirtyIfNeeded(IntVec3 c)
  {
    if (!this.CachedMapEdgeCells.Contains(c))
      return;
    this.walkableCellsDirty = true;
    this.enterPositionsDirty = true;
    CrossMapReachabilityCache.ClearCacheFor(this.interiorMap);
  }

  public CompNpcVehicleMap CompNpcVehicleMap
  {
    get
    {
      return (this._compNpcVehicleMap ?? (this._compNpcVehicleMap = new FetchedComp<CompNpcVehicleMap>((ThingWithComps) this))).Value;
    }
  }

  public CompDelayedKill CompDelayedKill
  {
    get
    {
      return (this._compDelayedKill ?? (this._compDelayedKill = new FetchedComp<CompDelayedKill>((ThingWithComps) this))).Value;
    }
  }

  public CompVehicleDrawOffset CompVehicleDrawOffset
  {
    get
    {
      return (this._compVehicleDrawOffset ?? (this._compVehicleDrawOffset = new FetchedComp<CompVehicleDrawOffset>((ThingWithComps) this))).Value;
    }
  }

  protected VehicleComp CompVehicleHover
  {
    get
    {
      return (this._compVehicleHover ?? (this._compVehicleHover = new FetchedComp<VehicleComp>((ThingWithComps) this, ModCompat.VehicleRaidFramework.CompVehicleHover))).Value;
    }
  }

  protected bool IsAirborne
  {
    get
    {
      return ModCompat.CompatBase<ModCompat.VehicleRaidFramework>.Active && this.CompVehicleHover != null && ModCompat.VehicleRaidFramework.State.Invoke(this.CompVehicleHover) > 0;
    }
  }

  public MapComponent InterceptorMapComponent
  {
    get
    {
      return this.\u003CInterceptorMapComponent\u003Ek__BackingField ?? (this.\u003CInterceptorMapComponent\u003Ek__BackingField = this.interiorMap.GetComponent(ModCompat.DefenseGrid.InterceptorMapComponent));
    }
  }

  public List<CompVehicleEnterSpot> EnterComps { get; } = new List<CompVehicleEnterSpot>();

  public List<CompFuelTank> FuelTankComps { get; } = new List<CompFuelTank>();

  public List<CompMapExpander> MapExpanderComps { get; } = new List<CompMapExpander>();

  public List<CompBuildableContainer> ContainerComps { get; } = new List<CompBuildableContainer>();

  public virtual Vector3 DrawPos
  {
    get
    {
      if (((Thing) this).Spawned && Find.CurrentMap != this.CurrentLevel)
        return base.DrawPos;
      Vector3 cachedDrawPos = this.cachedDrawPos;
      CompVehicleDrawOffset vehicleDrawOffset = this.CompVehicleDrawOffset;
      Vector3 vector3 = vehicleDrawOffset != null ? vehicleDrawOffset.DrawOffsetFull(this.FullRotation) : Vector3.zero;
      return Vector3.op_Subtraction(cachedDrawPos, vector3);
    }
  }

  public virtual Vector2 DrawSize
  {
    get
    {
      if (!UniqueVehicleUtility.get_IsUniqueVehicle(this.VehicleDef))
        return ((Thing) this).DrawSize;
      IntVec2 size = ((BuildableDef) this.VehicleDef).Size;
      return ((IntVec2) ref size).ToVector2();
    }
  }

  float IAttackTarget.TargetPriorityFactor => 0.15f;

  public virtual IEnumerable<Gizmo> GetGizmos()
  {
    VehiclePawnWithMap vehiclePawnWithMap = this;
    // ISSUE: reference to a compiler-generated method
    foreach (Gizmo gizmo in vehiclePawnWithMap.\u003C\u003En__0())
      yield return gizmo;
    if (((Thing) vehiclePawnWithMap).Faction == Faction.OfPlayer || DebugSettings.ShowDevGizmos)
    {
      Command_Action gizmo1 = new Command_Action();
      // ISSUE: reference to a compiler-generated method
      gizmo1.action = new Action(vehiclePawnWithMap.\u003CGetGizmos\u003Eb__125_0);
      ((Command) gizmo1).defaultLabel = TaggedString.op_Implicit(Translator.Translate("VMF_IncreasePriority"));
      ((Command) gizmo1).defaultDesc = TaggedString.op_Implicit(Translator.Translate("VMF_IncreasePriorityDesc"));
      ((Command) gizmo1).icon = (Texture) VehiclePawnWithMap.iconIncreasePriority.Texture;
      yield return (Gizmo) gizmo1;
      Command_Action gizmo2 = new Command_Action();
      // ISSUE: reference to a compiler-generated method
      gizmo2.action = new Action(vehiclePawnWithMap.\u003CGetGizmos\u003Eb__125_2);
      ((Command) gizmo2).defaultLabel = TaggedString.op_Implicit(Translator.Translate("VMF_DecreasePriority"));
      ((Command) gizmo2).defaultDesc = TaggedString.op_Implicit(Translator.Translate("VMF_DecreasePriorityDesc"));
      ((Command) gizmo2).icon = (Texture) VehiclePawnWithMap.iconDecreasePriority.Texture;
      yield return (Gizmo) gizmo2;
      Command_Toggle gizmo3 = new Command_Toggle();
      // ISSUE: reference to a compiler-generated method
      gizmo3.isActive = new Func<bool>(vehiclePawnWithMap.\u003CGetGizmos\u003Eb__125_4);
      // ISSUE: reference to a compiler-generated method
      gizmo3.toggleAction = new Action(vehiclePawnWithMap.\u003CGetGizmos\u003Eb__125_5);
      ((Command) gizmo3).defaultLabel = TaggedString.op_Implicit(Translator.Translate("VMF_AllowEnter"));
      ((Command) gizmo3).defaultDesc = TaggedString.op_Implicit(Translator.Translate("VMF_AllowEnterDesc"));
      ((Command) gizmo3).icon = (Texture) VehiclePawnWithMap.iconAllowEnter.Texture;
      yield return (Gizmo) gizmo3;
      Command_Toggle gizmo4 = new Command_Toggle();
      // ISSUE: reference to a compiler-generated method
      gizmo4.isActive = new Func<bool>(vehiclePawnWithMap.\u003CGetGizmos\u003Eb__125_6);
      // ISSUE: reference to a compiler-generated method
      gizmo4.toggleAction = new Action(vehiclePawnWithMap.\u003CGetGizmos\u003Eb__125_7);
      ((Command) gizmo4).defaultLabel = TaggedString.op_Implicit(Translator.Translate("VMF_AllowsGetOff"));
      ((Command) gizmo4).defaultDesc = TaggedString.op_Implicit(Translator.Translate("VMF_AllowsGetOffDesc"));
      ((Command) gizmo4).icon = (Texture) VehiclePawnWithMap.iconAllowExit.Texture;
      yield return (Gizmo) gizmo4;
      yield return (Gizmo) vehiclePawnWithMap.VehicleMapGizmo;
      if (DebugSettings.ShowDevGizmos)
      {
        yield return (Gizmo) new Command_FocusVehicleMap();
        Command_Action gizmo5 = new Command_Action();
        ((Command) gizmo5).defaultLabel = "Flash CachedMapEdgeCells";
        ((Gizmo) gizmo5).Order = 5001f;
        // ISSUE: reference to a compiler-generated method
        gizmo5.action = new Action(vehiclePawnWithMap.\u003CGetGizmos\u003Eb__125_8);
        yield return (Gizmo) gizmo5;
        Command_Action gizmo6 = new Command_Action();
        ((Command) gizmo6).defaultLabel = "Flash CachedWalkableMapEdgeCells";
        ((Gizmo) gizmo6).Order = 5002f;
        // ISSUE: reference to a compiler-generated method
        gizmo6.action = new Action(vehiclePawnWithMap.\u003CGetGizmos\u003Eb__125_9);
        yield return (Gizmo) gizmo6;
        Command_Action gizmo7 = new Command_Action();
        ((Command) gizmo7).defaultLabel = "Flash CachedEnterPositions";
        ((Gizmo) gizmo7).Order = 5003f;
        // ISSUE: reference to a compiler-generated method
        gizmo7.action = new Action(vehiclePawnWithMap.\u003CGetGizmos\u003Eb__125_10);
        yield return (Gizmo) gizmo7;
        Command_Action gizmo8 = new Command_Action();
        ((Command) gizmo8).defaultLabel = "Flash ValidMapRect";
        ((Gizmo) gizmo8).Order = 5004f;
        // ISSUE: reference to a compiler-generated method
        gizmo8.action = new Action(vehiclePawnWithMap.\u003CGetGizmos\u003Eb__125_11);
        yield return (Gizmo) gizmo8;
        if (UniqueVehicleUtility.get_IsUniqueVehicle(vehiclePawnWithMap.VehicleDef))
        {
          Command_Toggle gizmo9 = new Command_Toggle();
          ((Command) gizmo9).defaultLabel = "Debug draw: bridge cells";
          ((Gizmo) gizmo9).Order = 5005f;
          gizmo9.isActive = (Func<bool>) (() => CompMapExpander.debugDraw);
          gizmo9.toggleAction = (Action) (() => CompMapExpander.debugDraw = !CompMapExpander.debugDraw);
          yield return (Gizmo) gizmo9;
        }
      }
    }
  }

  private Command_SelectVehicleMap GetVehicleMapGizmo()
  {
    Command_SelectVehicleMap vehicleMapGizmo = new Command_SelectVehicleMap(this);
    vehicleMapGizmo.portrait = new VehiclePortrait(new VehiclePortrait.Config()
    {
      expiryTime = 5f
    });
    vehicleMapGizmo.toggleAction = (Action) (() =>
    {
      if (Find.CurrentMap == this.interiorMap && ((Thing) this).Spawned)
      {
        Current.Game.CurrentMap = ((Thing) this).Map;
      }
      else
      {
        Patch_Game_CurrentMap.ForceSet = true;
        Current.Game.CurrentMap = this.interiorMap;
      }
    });
    vehicleMapGizmo.isActive = (Func<bool>) (() => Find.CurrentMap != this.interiorMap || !((Thing) this).Spawned);
    ((Command) vehicleMapGizmo).defaultLabel = ((WorldObject) this.interiorMap.Parent).LabelCap;
    vehicleMapGizmo.miniIcon = (Texture) VehiclePawnWithMap.iconEye.Texture;
    vehicleMapGizmo.miniIconSize = 28f;
    return vehicleMapGizmo;
  }

  public List<CompVehicleEnterSpot> GetSortedEnterComps(
    IntVec3 cell,
    CompVehicleEnterSpot.Kind kind = CompVehicleEnterSpot.Kind.All)
  {
    this.tmpEnterComps.Clear();
    if (GenCollection.Empty<CompVehicleEnterSpot>(this.EnterComps))
      return this.tmpEnterComps;
    for (int index = 0; index < this.EnterComps.Count; ++index)
    {
      CompVehicleEnterSpot enterComp = this.EnterComps[index];
      switch (kind)
      {
        case CompVehicleEnterSpot.Kind.RampOnly:
          if (!enterComp.Props.allowPassingVehicle)
            break;
          goto default;
        case CompVehicleEnterSpot.Kind.GroundAccessOnly:
          if (!enterComp.Props.canAccessToGround)
            break;
          goto default;
        case CompVehicleEnterSpot.Kind.DirectAccessOnly:
          if (!enterComp.Props.canAccessVehicleToVehicle)
            break;
          goto default;
        default:
          if (GenGrid.Walkable(((Thing) enterComp.parent).Position, this.interiorMap))
          {
            this.tmpEnterComps.Add(enterComp);
            break;
          }
          break;
      }
    }
    GenCollection.SortBy<CompVehicleEnterSpot, int>(this.tmpEnterComps, (Func<CompVehicleEnterSpot, int>) (c => IntVec3Utility.DistanceToSquared(((Thing) c.parent).Position, cell)));
    return this.tmpEnterComps;
  }

  private void GenerateVehicleMap(Map sourceMap)
  {
    if (((Thing) this).Destroyed)
    {
      VMF_Log.Error("Tried to generate vehicle map for destroyed vehicle.");
    }
    else
    {
      this.generatingVehicleMap = true;
      if (MapGenerator.mapBeingGenerated != null)
      {
        if (this.generatingVehicleMap)
          return;
        LongEventHandler.ExecuteWhenFinished((Action) (() => this.GenerateVehicleMap(sourceMap)));
      }
      else
      {
        try
        {
          VehicleMapProps vehicleMapProps = this.VehicleMapProps;
          if (vehicleMapProps != null)
          {
            MapParent_Vehicle mapParentVehicle = (MapParent_Vehicle) WorldObjectMaker.MakeWorldObject(VMF_DefOf.VMF_VehicleMap);
            mapParentVehicle.mapGenerator = VMF_DefOf.VMF_VehicleMapGenerator;
            mapParentVehicle.vehicle = this;
            ((WorldObject) mapParentVehicle).Tile = PlanetTile.op_Implicit(0);
            ((WorldObject) mapParentVehicle).SetFaction(((Thing) this).Faction);
            IntVec3 intVec3;
            // ISSUE: explicit constructor call
            ((IntVec3) ref intVec3).\u002Ector(vehicleMapProps.size.x, 1, vehicleMapProps.size.z);
            intVec3.x += 2;
            intVec3.z += 2;
            this.MapSize = intVec3;
            mapParentVehicle.sourceMap = sourceMap;
            this.interiorMap = MapGenerator.GenerateMap(intVec3, (MapParent) mapParentVehicle, ((MapParent) mapParentVehicle).MapGeneratorDef, ((MapParent) mapParentVehicle).ExtraGenStepDefs, (Action<Map>) null, true, false);
            if (VehicleMapFramework.VehicleMapFramework.settings.drawPlanet)
            {
              Vector2 meshSize = Patch_Map_MapUpdate.MeshSize;
              RememberedCameraPos rememberedCameraPos = this.interiorMap.rememberedCameraPos;
              if (rememberedCameraPos != null)
                rememberedCameraPos.rootPos = new Vector3(meshSize.x / 2f, 0.0f, meshSize.y / 2f);
            }
            Find.World.pocketMaps.Add((PocketMapParent) mapParentVehicle);
            this.SpawnStructures(IntVec3.Zero);
          }
          this.interiorMap.events.BuildingSpawned += new Action<Building>(this.WalkableCellsDirtyIfNeeded);
          this.interiorMap.events.PathCostRecalculate += new Action<IntVec3>(this.WalkableCellsDirtyIfNeeded);
          if (this.CurrentLevel == null)
          {
            Map interiorMap;
            this.CurrentLevel = interiorMap = this.interiorMap;
          }
          if (!Find.World.worldObjects.Contains((WorldObject) this.interiorMap.Parent))
            Find.World.worldObjects.Add((WorldObject) this.interiorMap.Parent);
          if (sourceMap == null)
            return;
          this.interiorMap.skyManager = sourceMap.skyManager;
          this.interiorMap.weatherDecider = sourceMap.weatherDecider;
          this.interiorMap.weatherManager = sourceMap.weatherManager;
        }
        catch (Exception ex)
        {
          VMF_Log.Error($"Error while generating vehicle map.\n{ex}");
        }
        finally
        {
          this.generatingVehicleMap = false;
        }
      }
    }
  }

  internal void SpawnStructures(IntVec3 origin)
  {
    VehicleMapProps vehicleMapProps = this.VehicleMapProps;
    if (vehicleMapProps == null)
      return;
    foreach (IntVec2 filledStructureCell in vehicleMapProps.FilledStructureCells)
      GenSpawn.Spawn(VMF_DefOf.VMF_VehicleStructureFilled, IntVec3.op_Addition(origin, ((IntVec2) ref filledStructureCell).ToIntVec3), this.interiorMap, (WipeMode) 0).SetFaction(((Thing) this).Faction, (Pawn) null);
    foreach (IntVec2 emptyStructureCell in vehicleMapProps.EmptyStructureCells)
      this.interiorMap.terrainGrid.SetTerrain(IntVec3.op_Addition(origin, ((IntVec2) ref emptyStructureCell).ToIntVec3), VMF_DefOf.VMF_ImpassableFloor);
    foreach (IntVec2 expandableCell in vehicleMapProps.ExpandableCells)
      this.interiorMap.terrainGrid.SetTerrain(IntVec3.op_Addition(origin, ((IntVec2) ref expandableCell).ToIntVec3), VMF_DefOf.VMF_ImpassableFloor);
    foreach (IntVec2 outOfBoundsCell in vehicleMapProps.OutOfBoundsCells)
      this.interiorMap.terrainGrid.SetTerrain(IntVec3.op_Addition(origin, ((IntVec2) ref outOfBoundsCell).ToIntVec3), VMF_DefOf.VMF_ImpassableFloor);
  }

  internal void RemoveVehicleMap()
  {
    LongEventHandler.ExecuteWhenFinished((Action) (() =>
    {
      if (Find.Maps.Contains(this.interiorMap))
      {
        PocketMapParent pocketMapParent = this.interiorMap.PocketMapParent;
        if (pocketMapParent != null)
        {
          pocketMapParent.sourceMap = (Map) null;
          Find.World.pocketMaps.Remove(pocketMapParent);
          Find.World.renderer.wantedMode = (WorldRenderMode) 0;
        }
        Current.Game.DeinitAndRemoveMap(this.interiorMap, false);
      }
      this.interiorMap = (Map) null;
      if (!VehicleMapFramework.VehicleMapFramework.settings.dynamicUnpatchEnabled || GenCollection.Any<Map>(Find.Maps, (Predicate<Map>) (m => VehicleMapUtility.get_IsVehicleMap(m))))
        return;
      VMF_Harmony.DynamicPatchAll(VehicleMapFramework.VehicleMapFramework.settings.dynamicPatchLevel);
    }));
  }

  public virtual void SpawnSetup(Map map, bool respawningAfterLoad)
  {
    if (this.interiorMap == null)
      this.GenerateVehicleMap(map);
    else if (respawningAfterLoad)
    {
      this.interiorMap.events.BuildingSpawned += new Action<Building>(this.WalkableCellsDirtyIfNeeded);
      this.interiorMap.events.PathCostRecalculate += new Action<IntVec3>(this.WalkableCellsDirtyIfNeeded);
      List<IntVec3> cachedMapEdgeCells = this.CachedMapEdgeCells;
      if (this.VehicleMapProps is VehicleMapProps_Unique vehicleMapProps && vehicleMapProps.baseDef != null)
        FrameDelay.DelayOne<object>((Action<object>) (_ => LongEventHandler.ExecuteWhenFinished((Action) (() => this.ResizeNow(false)))), (object) null);
    }
    if (this.interiorMap != null)
    {
      PocketMapParent pocketMapParent = this.interiorMap.PocketMapParent;
      if (pocketMapParent != null)
        pocketMapParent.sourceMap = map;
      if (!Find.World.worldObjects.Contains((WorldObject) this.interiorMap.Parent))
        Find.World.worldObjects.Add((WorldObject) this.interiorMap.Parent);
      if (this.VehicleMapProps is VehicleMapProps_Gravship)
      {
        Building_GravEngine gravEngineNewTemp = GravshipUtility.GetPlayerGravEngine_NewTemp(this.interiorMap);
        if (gravEngineNewTemp != null)
        {
          LaunchInfo launchInfo = gravEngineNewTemp.launchInfo;
          if (launchInfo != null && launchInfo.doNegativeOutcome)
          {
            foreach (Pawn pawn in this.handlers.OfType<VehicleRoleHandlerBuildable>().SelectMany<VehicleRoleHandlerBuildable, Pawn>((Func<VehicleRoleHandlerBuildable, IEnumerable<Pawn>>) (h => (IEnumerable<Pawn>) h.thingOwner)).ToList<Pawn>())
              this.DisembarkPawn(pawn);
            Gravship gravship = GravshipUtility.GenerateGravship(gravEngineNewTemp);
            Building_GravEngine buildingGravEngine = GravshipVehicleUtility.PlaceGravship((WorldComponent_GravshipController) null, gravship, gravship.originalPosition, this.interiorMap);
            GenCollection.RandomElementByWeight<LandingOutcomeDef>((IEnumerable<LandingOutcomeDef>) DefDatabase<LandingOutcomeDef>.AllDefsListForReading, (Func<LandingOutcomeDef, float>) (d => d.weight)).Worker.ApplyOutcome(gravship);
            gravEngineNewTemp.launchInfo = (LaunchInfo) null;
          }
        }
      }
    }
    base.SpawnSetup(map, respawningAfterLoad);
    this.RegisterEvents();
    this.RecacheDrawPos(((Thing) this).DrawPos);
    VehiclePawnWithMapCache.RegisterVehicle(this);
    this.mapFollower = new VehicleMapFollower(this);
    this.mapFollower.RegisterVehicle();
    if (this.interiorMap != null)
    {
      this.interiorMap.skyManager = map.skyManager;
      this.interiorMap.weatherDecider = map.weatherDecider;
      this.interiorMap.weatherManager = map.weatherManager;
      if (Find.CurrentMap == this.interiorMap)
        Current.Game.CurrentMap = map;
      CollectionExtensions.Do<VehiclePawn>(this.interiorMap.mapPawns.AllPawns.OfType<VehiclePawn>(), (Action<VehiclePawn>) (v => v.Transform.rotation = 0.0f));
    }
    this.SetTile();
    this.Transform.rotation = 0.0f;
    this.enterPositionsDirty = true;
  }

  protected virtual void Tick()
  {
    this.Resize();
    if (((Thing) this).Spawned)
    {
      this.RecacheDrawPos(((Thing) this).DrawPos);
      CompDelayedKill compDelayedKill = this.CompDelayedKill;
      if (compDelayedKill != null && compDelayedKill.KillStarted)
      {
        ((ThingComp) this.CompDelayedKill).CompTick();
        return;
      }
      this.mapFollower?.MapFollowerTick();
    }
    else if (Gen.IsHashIntervalTick((Thing) this, 30))
      this.SetTile();
    base.Tick();
  }

  protected virtual void TickInterval(int delta)
  {
    if (((Thing) this).Spawned)
    {
      CompDelayedKill compDelayedKill = this.CompDelayedKill;
      if (compDelayedKill != null && compDelayedKill.KillStarted)
        return;
    }
    base.TickInterval(delta);
  }

  private void SetTile()
  {
    if (((Thing) this).Spawned)
    {
      Map interiorMap = this.interiorMap;
      if (interiorMap == null)
        return;
      ((WorldObject) interiorMap.Parent).Tile = ((Thing) this).Map.Tile;
    }
    else
    {
      WorldObject worldObject = GetWorldObject((IThingHolder) this);
      AerialVehicleInFlight aerialVehicleInFlight = worldObject as AerialVehicleInFlight;
      if (aerialVehicleInFlight == null)
      {
        if (worldObject == null || worldObject is MapParent_Vehicle)
          return;
        Map interiorMap = this.interiorMap;
        if (interiorMap == null)
          return;
        ((WorldObject) interiorMap.Parent).Tile = worldObject.Tile;
      }
      else
        Task.Run((Action) (() =>
        {
          Map interiorMap = this.interiorMap;
          if (interiorMap == null)
            return;
          ((WorldObject) interiorMap.Parent).Tile = PlanetTile.op_Implicit(WorldHelper.GetNearestTile(((WorldObject) aerialVehicleInFlight).DrawPos));
        }));
    }

    static WorldObject GetWorldObject(IThingHolder holder)
    {
      for (; holder != null; holder = holder.ParentHolder)
      {
        if (holder is WorldObject worldObject1)
          return worldObject1;
      }
      return (WorldObject) null;
    }
  }

  public virtual void Notify_MyMapRemoved()
  {
    ((ThingWithComps) this).Notify_MyMapRemoved();
    ((Thing) this).Destroy((DestroyMode) 0);
  }

  public virtual void Notify_AbandonedAtTile(PlanetTile tile)
  {
    ((ThingWithComps) this).Notify_AbandonedAtTile(tile);
    ((Thing) this).Destroy((DestroyMode) 0);
  }

  public virtual void Notify_LeftBehind()
  {
    ((Pawn) this).Notify_LeftBehind();
    ((Thing) this).Destroy((DestroyMode) 0);
  }

  public virtual void Kill(DamageInfo? dinfo, DestroyMode destroyMode = 2, bool spawnWreckage = false)
  {
    if (((Thing) this).Spawned)
    {
      CompDelayedKill compDelayedKill = this.CompDelayedKill;
      if (compDelayedKill != null && !compDelayedKill.KillOnTick)
      {
        if (this.CompDelayedKill.KillStarted)
          return;
        Thing thing;
        if (!dinfo.HasValue)
        {
          thing = (Thing) null;
        }
        else
        {
          DamageInfo valueOrDefault = dinfo.GetValueOrDefault();
          thing = ((DamageInfo) ref valueOrDefault).Instigator;
        }
        if (thing is Pawn pawn)
          RecordsUtility.Notify_PawnKilled((Pawn) this, pawn);
        this.CompDelayedKill.StartKillTimer(destroyMode, spawnWreckage);
        return;
      }
    }
    base.Kill(dinfo, destroyMode, spawnWreckage);
  }

  public virtual void Destroy(DestroyMode mode = 0)
  {
    Map map = ((Thing) this).Map;
    if (((Thing) this).Spawned)
      this.DisembarkAll();
    if (this.interiorMap == null)
    {
      base.Destroy(mode);
    }
    else
    {
      if (map != null)
      {
        StringBuilder stringBuilder = new StringBuilder();
        bool flag1 = false;
        List<Thing> allThings = this.interiorMap.listerThings.AllThings;
        for (int index = allThings.Count - 1; index >= 0; --index)
        {
          if (index < allThings.Count)
          {
            Thing thing1 = allThings[index];
            if (mode != null && thing1 != null && !thing1.Destroyed)
            {
              IntVec3 positionOnBaseMap = VehicleMapUtility.get_PositionOnBaseMap(thing1);
              if (thing1.def.category == 3)
              {
                if (!thing1.def.destroyable)
                {
                  Thing.allowDestroyNonDestroyable = true;
                  thing1.Destroy((DestroyMode) 0);
                  Thing.allowDestroyNonDestroyable = false;
                }
                else
                  thing1.Destroy((DestroyMode) 0);
                if (GenGrid.Walkable(positionOnBaseMap, map) && GridsUtility.GetItemCount(positionOnBaseMap, map) < GridsUtility.GetMaxItemsAllowedInCell(positionOnBaseMap, map))
                {
                  IntVec3 position = thing1.Position;
                  thing1.Position = positionOnBaseMap;
                  GenLeaving.DoLeavingsFor(thing1, map, (DestroyMode) 4, (List<Thing>) null);
                  thing1.Position = position;
                }
              }
              else
              {
                bool flag2;
                switch (thing1)
                {
                  case Explosion _:
                  case Projectile _:
                  case Fire _:
                    flag2 = true;
                    break;
                  default:
                    flag2 = false;
                    break;
                }
                if (!flag2)
                {
                  IntVec3 intVec3_1 = IntVec3Utility.ToIntVec3(thing1.DrawPos);
                  if (!GenGrid.InBounds(intVec3_1, map))
                    intVec3_1 = ((IntVec3) ref intVec3_1).ClampInsideMap(map);
                  ((Entity) thing1).DeSpawn((DestroyMode) 0);
                  TerrainDef terrain1 = GridsUtility.GetTerrain(intVec3_1, map);
                  if (terrain1.IsWater && thing1 is Filth)
                    thing1.Destroy((DestroyMode) 0);
                  else if (thing1 is Pawn pawn && (terrain1 == TerrainDefOf.WaterDeep || terrain1 == TerrainDefOf.WaterOceanDeep) && HealthHelper.AttemptToDrown(pawn))
                  {
                    flag1 = true;
                    stringBuilder.AppendLine(((Entity) pawn).LabelCap);
                  }
                  else
                    FrameDelay.DelayOne<(Thing, IntVec3, Map)>((Action<(Thing, IntVec3, Map)>) (state =>
                    {
                      if (!GenPlace.TryPlaceThing(state.thing, state.cell, state.map, (ThingPlaceMode) 1, (Action<Thing, int>) null, (Predicate<IntVec3>) null, new Rot4?(), 1))
                      {
                        IntVec3 intVec3_2;
                        CellFinder.TryFindRandomCellNear(state.cell, state.map, 50, (Predicate<IntVec3>) (c => GenPlace.TryPlaceThing(state.thing, c, state.map, (ThingPlaceMode) 1, (Action<Thing, int>) null, (Predicate<IntVec3>) null, new Rot4?(), 1)), ref intVec3_2, -1);
                      }
                      if (state.thing is Pawn thing5)
                      {
                        Pawn_CarryTracker carryTracker = thing5.carryTracker;
                        if (carryTracker != null && carryTracker.CarriedThing != null)
                        {
                          Thing thing4;
                          thing5.carryTracker.TryDropCarriedThing(((Thing) thing5).Position, (ThingPlaceMode) 1, ref thing4, (Action<Thing, int>) null);
                        }
                      }
                      if (!(state.thing is VehiclePawn thing6))
                        return;
                      TerrainDef terrain2 = GridsUtility.GetTerrain(((Thing) thing6).Position, state.map);
                      if (terrain2 == null || !terrain2.IsWater || Ext_Vehicles.DrivableRectOnCell(thing6, ((Thing) thing6).Position, (Ext_Vehicles.DestinationHitboxReq) 0))
                        return;
                      thing6.DisembarkAll();
                      ((Thing) thing6).Destroy((DestroyMode) 0);
                    }), (thing1, intVec3_1, map));
                }
              }
            }
          }
        }
        if (flag1)
        {
          string str = TaggedString.op_Implicit(TranslatorFormattedStringExtensions.Translate("VF_BoatSunkWithPawnsDesc", NamedArgument.op_Implicit(((Entity) this).LabelShort), NamedArgument.op_Implicit(stringBuilder.ToString())));
          Find.LetterStack.ReceiveLetter(Translator.Translate("VF_BoatSunk"), TaggedString.op_Implicit(str), LetterDefOf.NegativeEvent, LookTargets.op_Implicit(new TargetInfo(((Thing) this).Position, map, false)), (Faction) null, (Quest) null, (List<ThingDef>) null, (string) null, 0, true);
        }
      }
      base.Destroy(mode);
      this.RemoveVehicleMap();
      if (!((Def) this.VehicleDef).HasModExtension<VehicleMapProps_Unique>())
        return;
      UniqueVehicleUtility.ReleaseUniqueVehicleDef(this.VehicleDef);
    }
  }

  public virtual void DeSpawn(DestroyMode mode = 0)
  {
    this.interiorMap.PocketMapParent.sourceMap = (Map) null;
    VehiclePawnWithMapCache.DeRegisterVehicle(this);
    this.mapFollower.DeRegisterVehicle();
    if (mode < 2)
    {
      this.interiorMap.skyManager = new SkyManager(this.interiorMap);
      this.interiorMap.skyManager.ForceSetCurSkyGlow(((Thing) this).Map.skyManager.CurSkyGlow);
      this.interiorMap.weatherManager = new WeatherManager(this.interiorMap)
      {
        curWeather = ((Thing) this).Map.weatherManager.curWeather,
        lastWeather = ((Thing) this).Map.weatherManager.lastWeather,
        prevSkyTargetLerp = ((Thing) this).Map.weatherManager.prevSkyTargetLerp,
        currSkyTargetLerp = ((Thing) this).Map.weatherManager.currSkyTargetLerp,
        curWeatherAge = ((Thing) this).Map.weatherManager.curWeatherAge
      };
      this.interiorMap.weatherDecider = new WeatherDecider(this.interiorMap);
      foreach (Pawn allPawn in this.interiorMap.mapPawns.AllPawns)
      {
        Lord lord;
        if (LordUtility.TryGetLord(allPawn, ref lord) && lord.Map != this.interiorMap)
          lord.Notify_PawnLost(allPawn, (PawnLostCondition) 6, new DamageInfo?());
      }
    }
    foreach (object obj in ((IEnumerable<object>) this.interiorMap.listerThings.AllThings).Intersect<object>((IEnumerable<object>) Find.Selector.SelectedObjects))
      Find.Selector.Deselect(obj);
    foreach (object obj in ((IEnumerable<object>) this.interiorMap.zoneManager.AllZones).Intersect<object>((IEnumerable<object>) Find.Selector.SelectedObjects))
      Find.Selector.Deselect(obj);
    CrossMapHaulDestinationManager cachedMapComponent = ComponentCache.GetCachedMapComponent<CrossMapHaulDestinationManager>(((Thing) this).Map);
    foreach (IHaulDestination allHaulDestination in this.interiorMap.haulDestinationManager.AllHaulDestinations)
      cachedMapComponent.RemoveHaulDestination(allHaulDestination);
    CrossMapReachabilityCache.ClearCacheFor(this.interiorMap);
    base.DeSpawn(mode);
  }

  public virtual void DrawAt(in Vector3 drawLoc, Rot8 rot, float rotation)
  {
    if (!((Thing) this).Spawned)
    {
      Map interiorMap1 = this.interiorMap;
      if (interiorMap1 != null)
        CollectionExtensions.DoIf<VehiclePawn>((IEnumerable<VehiclePawn>) ComponentCache.GetDetachedMapComponent<VehiclePositionManager>(interiorMap1).AllClaimants, (Func<VehiclePawn, bool>) (v =>
        {
          GraphicData graphicData = ((Thing) v).def.graphicData;
          return graphicData != null && graphicData.drawRotated;
        }), (Action<VehiclePawn>) (v => v.Transform.rotation = rotation.FlipAngle(v)));
      if (!Mathf.Approximately(this.Transform.rotation, rotation))
      {
        this.Transform.rotation = rotation;
        this.CellDesignationsDirty();
      }
      Map interiorMap2 = this.interiorMap;
      if (interiorMap2 != null)
        interiorMap2.rememberedCameraPos.rootPos = drawLoc;
    }
    Vector3 vector3_1 = Vector3Utility.WithYOffset(drawLoc, -3.658537f);
    CompVehicleDrawOffset vehicleDrawOffset = this.CompVehicleDrawOffset;
    Vector3 vector3_2 = vehicleDrawOffset != null ? vehicleDrawOffset.DrawOffsetFull(this.FullRotation) : Vector3.zero;
    Vector3 drawLoc1 = Vector3.op_Addition(vector3_1, vector3_2);
    this.RecacheDrawPos(drawLoc1);
    this.DrawTracker.DynamicDrawPhaseAt((DrawPhase) 2, ref drawLoc1, rot, this.Transform.rotation.FlipAngle((VehiclePawn) this));
    this.DrawVehicleMap();
  }

  public virtual void DynamicDrawPhaseAt(DrawPhase phase, Vector3 drawLoc, bool flip = false)
  {
    Vector3 vector3_1 = drawLoc;
    CompVehicleDrawOffset vehicleDrawOffset = this.CompVehicleDrawOffset;
    Vector3 vector3_2 = vehicleDrawOffset != null ? vehicleDrawOffset.DrawOffsetFull(this.FullRotation) : Vector3.zero;
    Vector3 drawLoc1 = Vector3.op_Addition(vector3_1, vector3_2);
    base.DynamicDrawPhaseAt(phase, drawLoc1, flip);
    if (phase != 2)
      return;
    this.RecacheDrawPos(drawLoc1);
    VehiclePathFollower vehiclePather = this.vehiclePather;
    if ((vehiclePather != null ? (vehiclePather.Moving ? 1 : 0) : 0) != 0)
      this.CellDesignationsDirty();
    this.DrawVehicleMap();
  }

  public void RecacheDrawPos(Vector3 drawLoc)
  {
    if (!UnityData.IsInMainThread)
      return;
    TransformData transformData;
    // ISSUE: explicit constructor call
    ((TransformData) ref transformData).\u002Ector(Vector3.op_Addition(drawLoc, this.Transform.position), this.FullRotation, this.Transform.rotation.FlipAngle((VehiclePawn) this));
    PreRenderResults? preRenderResults = ((Graphic_Rgb) this.VehicleGraphic)?.ParallelGetPreRenderResults(ref transformData, false, (Thing) this, 0.0f);
    this.cachedDrawPos = preRenderResults.HasValue ? preRenderResults.GetValueOrDefault().position : drawLoc;
    if (((Thing) this).Spawned && Find.CurrentMap == this.CurrentLevel)
      this.cachedExactPos = Vector3.op_Subtraction(Vector3.op_Addition(this.cachedDrawPos, base.DrawPos), drawLoc);
    else
      this.cachedExactPos = this.cachedDrawPos;
  }

  private void CellDesignationsDirty()
  {
    if (this.cellDesignationsDirtyTick == GenTicks.TicksGame)
      return;
    this.cellDesignationsDirtyTick = GenTicks.TicksGame;
    foreach (DesignationDef cellDesignation in VehiclePawnWithMap.cellDesignations)
      VehiclePawnWithMap.DirtyCellDesignationsCache.Invoke((object) this.CurrentLevel.designationManager, SingleParam.Get((object) cellDesignation));
  }

  protected virtual void DrawVehicleMap()
  {
    Map currentLevel1 = this.CurrentLevel;
    if (currentLevel1 == null)
      return;
    FrameDelay.DelayOne<VehiclePawnWithMap>((Action<VehiclePawnWithMap>) (vehicle =>
    {
      try
      {
        Map currentLevel2 = vehicle.CurrentLevel;
        VehicleSectionLayerManager.CacheMode = true;
        ComponentCache.GetCachedMapComponent<VehicleSectionLayerManager>(currentLevel2)?.UpdateAllSection();
        currentLevel2.mapDrawer.MapMeshDrawerUpdate_First();
      }
      finally
      {
        VehicleSectionLayerManager.CacheMode = false;
      }
    }), this);
    Vector3 original;
    if (!ModCompat.CompatBase<ModCompat.AsAboveSoBelow>.Active)
    {
      original = Vector3.zero;
    }
    else
    {
      CellRect cellRect = ModCompat.AsAboveSoBelow.RectOfBand(currentLevel1, ModCompat.AsAboveSoBelow.CurrentBand(currentLevel1));
      IntVec3 min = ((CellRect) ref cellRect).Min;
      original = Vector3.op_UnaryNegation(((IntVec3) ref min).ToVector3());
    }
    this.DrawVehicleMapMesh(original.ToBaseMapCoord(this), currentLevel1);
    DynamicDrawManagerOnVehicle.DrawDynamicThings(currentLevel1);
    this.DrawClippers();
    currentLevel1.designationManager.DrawDesignations();
    currentLevel1.overlayDrawer.DrawAllOverlays();
    currentLevel1.temporaryThingDrawer.Draw();
    currentLevel1.flecks.FleckManagerDraw();
    using (new Command_FocusVehicleMap.FocusVehicle(this))
    {
      currentLevel1.roofGrid.RoofGridUpdate();
      currentLevel1.mapTemperature.TemperatureUpdate();
      MapComponentUtility.MapComponentOnDraw(currentLevel1);
      CompMapExpander.DebugDraw(this.MapExpanderComps);
    }
    DebugDrawHelper.DebugDraw(currentLevel1.debugDrawer, currentLevel1);
  }

  internal void DrawVehicleMapMesh(Vector3 drawPos, Map map)
  {
    MapDrawer mapDrawer = map.mapDrawer;
    VehicleSectionLayerManager cachedMapComponent = ComponentCache.GetCachedMapComponent<VehicleSectionLayerManager>(map);
    if (cachedMapComponent == null)
      return;
    bool flag = false;
    CellRect cellRect = ModCompat.CompatBase<ModCompat.AsAboveSoBelow>.Active ? ModCompat.AsAboveSoBelow.RectOfBand(map, ModCompat.AsAboveSoBelow.CurrentBand(map)) : new CellRect();
    Section[,] sectionArray = VehiclePawnWithMap.sections.Invoke(mapDrawer);
    int upperBound1 = sectionArray.GetUpperBound(0);
    int upperBound2 = sectionArray.GetUpperBound(1);
    for (int lowerBound1 = sectionArray.GetLowerBound(0); lowerBound1 <= upperBound1; ++lowerBound1)
    {
      for (int lowerBound2 = sectionArray.GetLowerBound(1); lowerBound2 <= upperBound2; ++lowerBound2)
      {
        Section section = sectionArray[lowerBound1, lowerBound2];
        if (!flag && (section.dirtyFlags & (MapMeshFlagDef.op_Implicit(MapMeshFlagDefOf.Things) | MapMeshFlagDef.op_Implicit(MapMeshFlagDefOf.Terrain))) > 0UL)
        {
          VehicleMapUIRenderer.SetDirty(this);
          this.VehicleMapGizmo.portrait.MarkDirty();
          flag = true;
        }
        if (!ModCompat.CompatBase<ModCompat.AsAboveSoBelow>.Active || ((CellRect) ref cellRect).Overlaps(section.CellRect))
          this.DrawSection(section, drawPos, cachedMapComponent);
      }
    }
  }

  protected virtual void DrawSection(
    Section section,
    Vector3 drawPos,
    VehicleSectionLayerManager component)
  {
    Rot8 fullRotation = this.FullRotation;
    ((SectionLayer_TerrainOnVehicle) component.GetLayer(section, typeof (SectionLayer_TerrainOnVehicle), new Rot8())).DrawLayer(drawPos);
    ((SectionLayer_SnowOnVehicle) component.GetLayer(section, typeof (SectionLayer_SnowOnVehicle), new Rot8())).DrawLayer(Vector3Utility.WithYOffset(drawPos, 0.1f));
    float fullAngle = VehicleMapUtility.get_FullAngle((VehiclePawn) this);
    VehicleSectionLayerManager.DrawLayer(component.GetLayer(section, typeof (SectionLayer_ThingsGeneral), fullRotation), drawPos, fullAngle);
    VehicleSectionLayerManager.DrawLayer(component, section, typeof (SectionLayer_BuildingsDamage), drawPos, fullRotation, fullAngle);
    VehicleSectionLayerManager.DrawLayer(component, section, typeof (SectionLayer_IndoorMask), Vector3Utility.Yto0(drawPos), fullRotation, fullAngle);
    VehicleSectionLayerManager.DrawLayer(component, section, typeof (SectionLayer_EdgeShadows), drawPos, fullRotation, fullAngle);
    ((SectionLayer_SunShadowsOnVehicle) component.GetLayer(section, typeof (SectionLayer_SunShadowsOnVehicle), fullRotation)).DrawLayer(drawPos, this.Transform.rotation - this.Angle);
    ((SectionLayer_LightingOnVehicle) component.GetLayer(section, typeof (SectionLayer_LightingOnVehicle), new Rot8())).DrawLayer(drawPos);
    if (OverlayDrawHandler.ShouldDrawPowerGrid)
      VehicleSectionLayerManager.DrawLayer(component.GetLayer(section, typeof (SectionLayer_ThingsPowerGrid), fullRotation), Vector3Utility.Yto0(drawPos), fullAngle);
    if (OverlayDrawHandler.ShouldDrawZones)
      VehicleSectionLayerManager.DrawLayer(component, section, VehiclePawnWithMap.t_SectionLayer_Zones, drawPos, fullRotation, fullAngle);
    if (ModsConfig.OdysseyActive)
    {
      ((SectionLayer_SubstructurePropsOnVehicle) component.GetLayer(section, typeof (SectionLayer_SubstructurePropsOnVehicle), new Rot8()))?.DrawLayer(fullRotation, drawPos, this.Transform.rotation);
      ((SectionLayer_GravshipHullOnVehicle) component.GetLayer(section, typeof (SectionLayer_GravshipHullOnVehicle), new Rot8()))?.DrawLayer(fullRotation, drawPos, this.Transform.rotation);
    }
    this.DrawModLayers(section, drawPos, component);
  }

  protected virtual void DrawModLayers(
    Section section,
    Vector3 drawPos,
    VehicleSectionLayerManager component)
  {
    float fullAngle = VehicleMapUtility.get_FullAngle((VehiclePawn) this);
    Rot8 fullRotation = this.FullRotation;
    foreach (ModCompat.CompatBase classesForDrawLayer in VehicleSectionLayerManager.CompatClassesForDrawLayers)
      classesForDrawLayer.DrawSectionLayers(component, section, drawPos, fullRotation, fullAngle);
  }

  private void DrawClippers()
  {
    if (Command_FocusVehicleMap.FocusLockedVehicle == this || Command_FocusVehicleMap.FocusedVehicle == this)
    {
      Material clipMat = VehiclePawnWithMap.ClipMat;
      Quaternion fullAngleQuat = VehicleMapUtility.get_FullAngleQuat((VehiclePawn) this);
      IntVec3 mapSize = this.MapSize;
      Vector3 one;
      // ISSUE: explicit constructor call
      ((Vector3) ref one).\u002Ector(500f, 1f, (float) mapSize.z);
      Matrix4x4 matrix4x4_1 = new Matrix4x4();
      ((Matrix4x4) ref matrix4x4_1).SetTRS(ToBaseMapCoord(new Vector3(-250f, 0.0f, (float) mapSize.z / 2f), this), fullAngleQuat, one);
      Graphics.DrawMesh(MeshPool.plane10, matrix4x4_1, clipMat, 0);
      Matrix4x4 matrix4x4_2 = new Matrix4x4();
      ((Matrix4x4) ref matrix4x4_2).SetTRS(ToBaseMapCoord(new Vector3((float) mapSize.x + 250f, 0.0f, (float) mapSize.z / 2f), this), fullAngleQuat, one);
      Graphics.DrawMesh(MeshPool.plane10, matrix4x4_2, clipMat, 0);
      // ISSUE: explicit constructor call
      ((Vector3) ref one).\u002Ector(1000f, 1f, 500f);
      Matrix4x4 matrix4x4_3 = new Matrix4x4();
      ((Matrix4x4) ref matrix4x4_3).SetTRS(ToBaseMapCoord(new Vector3((float) mapSize.x / 2f, 0.0f, (float) mapSize.z + 250f), this), fullAngleQuat, one);
      Graphics.DrawMesh(MeshPool.plane10, matrix4x4_3, clipMat, 0);
      Matrix4x4 matrix4x4_4 = new Matrix4x4();
      ((Matrix4x4) ref matrix4x4_4).SetTRS(ToBaseMapCoord(new Vector3((float) mapSize.x / 2f, 0.0f, -250f), this), fullAngleQuat, one);
      Graphics.DrawMesh(MeshPool.plane10, matrix4x4_4, clipMat, 0);
      one = Vector3.one;
      bool flag = Find.DesignatorManager.SelectedDesignator is Designator_Build selectedDesignator && ((Designator_Place) selectedDesignator).PlacingDef is ThingDef placingDef && placingDef.HasComp<CompMapExpander>();
      BoolGrid impassableCellGrid = this.ImpassableCellGrid;
      CellRect mapRect = this.MapRect;
      foreach (IntVec3 intVec3 in mapRect)
      {
        if (impassableCellGrid[intVec3] && (!flag || !this.ExpandableGrid[intVec3]))
        {
          ((Matrix4x4) ref matrix4x4_4).SetTRS(ToBaseMapCoord(((IntVec3) ref intVec3).ToVector3Shifted(), this), fullAngleQuat, one);
          Graphics.DrawMesh(MeshPool.plane10, matrix4x4_4, clipMat, 0);
        }
      }
    }
    Map currentMap = Find.CurrentMap;
    if (currentMap != this.CurrentLevel && currentMap != this.interiorMap || !WorldRendererUtility.DrawingMap || !VehicleMapFramework.VehicleMapFramework.settings.drawPlanet)
      return;
    Material clipMat1 = MapEdgeClipDrawer.ClipMat;
    Vector2 meshSize = Patch_Map_MapUpdate.MeshSize;
    Vector3 vector3;
    // ISSUE: explicit constructor call
    ((Vector3) ref vector3).\u002Ector(500f, 1f, meshSize.y);
    Matrix4x4 matrix4x4_5 = new Matrix4x4();
    ((Matrix4x4) ref matrix4x4_5).SetTRS(new Vector3(-250f, 0.0f, meshSize.y / 2f), Quaternion.identity, vector3);
    Graphics.DrawMesh(MeshPool.plane10, matrix4x4_5, clipMat1, 0);
    Matrix4x4 matrix4x4_6 = new Matrix4x4();
    ((Matrix4x4) ref matrix4x4_6).SetTRS(new Vector3(meshSize.x + 250f, 0.0f, meshSize.y / 2f), Quaternion.identity, vector3);
    Graphics.DrawMesh(MeshPool.plane10, matrix4x4_6, clipMat1, 0);
    // ISSUE: explicit constructor call
    ((Vector3) ref vector3).\u002Ector(1000f, 1f, 500f);
    Matrix4x4 matrix4x4_7 = new Matrix4x4();
    ((Matrix4x4) ref matrix4x4_7).SetTRS(new Vector3(meshSize.x / 2f, 0.0f, meshSize.y + 250f), Quaternion.identity, vector3);
    Graphics.DrawMesh(MeshPool.plane10, matrix4x4_7, clipMat1, 0);
    Matrix4x4 matrix4x4_8 = new Matrix4x4();
    ((Matrix4x4) ref matrix4x4_8).SetTRS(new Vector3(meshSize.x / 2f, 0.0f, -250f), Quaternion.identity, vector3);
    Graphics.DrawMesh(MeshPool.plane10, matrix4x4_8, clipMat1, 0);

    static Vector3 ToBaseMapCoord(Vector3 original, VehiclePawnWithMap vehicle)
    {
      Vector3 cachedDrawPos = vehicle.cachedDrawPos;
      IntVec3 mapSize = vehicle.MapSize;
      Vector3 vector3;
      // ISSUE: explicit constructor call
      ((Vector3) ref vector3).\u002Ector((float) mapSize.x / 2f, 0.0f, (float) mapSize.z / 2f);
      return Vector3.op_Addition(Vector3.op_Addition(Vector3Utility.RotatedBy(Vector3.op_Subtraction(original.YOffset(), vector3), VehicleMapUtility.get_FullAngle((VehiclePawn) vehicle)), cachedDrawPos), VehicleMapUtility.OffsetFor(vehicle));
    }
  }

  public virtual void DrawGUIOverlay()
  {
    base.DrawGUIOverlay();
    Map currentLevel = this.CurrentLevel;
    DebugDrawHelper.DebugOnGUI(currentLevel.debugDrawer, currentLevel);
  }

  public virtual string GetInspectString()
  {
    if ((double) VehicleMapFramework.VehicleMapFramework.settings.weightFactor == 0.0)
      return (string) null;
    string inspectString = ((Pawn) this).GetInspectString();
    float statValue = this.GetStatValue(VMF_DefOf.MaximumPayload);
    return $"{inspectString}{$"\n{((Def) VMF_DefOf.MaximumPayload).LabelCap}:"} {GenText.ToStringEnsureThreshold(VehicleMapUtility.VehicleMapMass(this) * VehicleMapFramework.VehicleMapFramework.settings.weightFactor, 2f, 0)} /{$" {GenText.ToStringEnsureThreshold(statValue, 2f, 0)} {Translator.Translate("kg")}"}";
  }

  public virtual bool AllowEnterFor(Pawn pawn)
  {
    return (this.AllowEnter || (pawn != null ? (GenHostility.HostileTo((Thing) pawn, Faction.OfPlayer) ? 1 : 0) : 1) != 0 || pawn.Drafted) && !this.IsAirborne;
  }

  public virtual bool AllowExitFor(Pawn pawn)
  {
    return (this.AllowExit || (pawn != null ? (GenHostility.HostileTo((Thing) pawn, Faction.OfPlayer) ? 1 : 0) : 1) != 0 || pawn.Drafted) && !this.IsAirborne;
  }

  public virtual void ExposeData()
  {
    base.ExposeData();
    Scribe_References.Look<Map>(ref this.interiorMap, "interiorMap", false);
    Scribe_Values.Look<bool>(ref this.allowEnter, "allowEnter", false, false);
    Scribe_Values.Look<bool>(ref this.allowExit, "allowExit", false, false);
    VehicleMapProps vehicleMapProps = this.VehicleMapProps;
    if (vehicleMapProps != null)
    {
      IntVec3 intVec3;
      // ISSUE: explicit constructor call
      ((IntVec3) ref intVec3).\u002Ector(vehicleMapProps.size.x + 2, 1, vehicleMapProps.size.z + 2);
      this.MapSize = intVec3;
    }
    else
      this.MapSize = this.interiorMap.Size;
  }

  protected virtual void PostLoad()
  {
    VMF_Harmony.DynamicPatchAll(Level.All);
    base.PostLoad();
    this.RegisterEvents();
    this.CompVehicleTurrets?.RevalidateTurrets();
    this.ResetRenderStatus();
    if (!UniqueVehicleUtility.get_IsUniqueVehicle(this.VehicleDef))
      return;
    this.ResizeNow();
  }

  public virtual void PostMake()
  {
    ((Pawn) this).PostMake();
    if (!(this.VehicleMapProps is VehicleMapProps_Unique vehicleMapProps) || vehicleMapProps.baseDef != null)
      return;
    ((Thing) this).def = (ThingDef) UniqueVehicleUtility.ClaimUniqueVehicleDef(this.VehicleDef);
  }

  public virtual void PostGenerationSetup()
  {
    VMF_Harmony.DynamicPatchAll(Level.All);
    base.PostGenerationSetup();
    this.RegisterEvents();
  }

  public void RegisterEvents()
  {
    EventManager<MapVehicleEventDef> vehicleEventManager = this.MapVehicleEventManager;
    if (vehicleEventManager != null && Ext_EventManager.Initialized<MapVehicleEventDef>(vehicleEventManager))
      return;
    Ext_EventManager.FillEventsDef<MapVehicleEventDef>((IEventManager<MapVehicleEventDef>) this);
    Ext_EventManager.AddEvent<MapVehicleEventDef>((IEventManager<MapVehicleEventDef>) this, VMF_DefOf.EnterNextCell, (Action) (() =>
    {
      this.enterPositionsDirty = true;
      CrossMapReachabilityCache.ClearCacheFor(this.VehicleMap);
    }), Array.Empty<Action>());
    if (!ModCompat.CompatBase<ModCompat.DefenseGrid>.Active)
      return;
    Ext_EventManager.AddEvent<MapVehicleEventDef>((IEventManager<MapVehicleEventDef>) this, VMF_DefOf.EnterNextCell, (Action) (() =>
    {
      if (this.InterceptorMapComponent == null)
        return;
      foreach (object obj in (IEnumerable) ModCompat.DefenseGrid.grids.Invoke(this.InterceptorMapComponent))
        ModCompat.DefenseGrid.RepaintGrid.Invoke((object) this.InterceptorMapComponent, SingleParam.Get(obj));
    }), Array.Empty<Action>());
    Ext_EventManager.AddEvent<VehicleEventDef>((IEventManager<VehicleEventDef>) this, VehicleEventDefOf.Spawned, (Action) (() => FrameDelay.DelayOne<MapComponent>((Action<MapComponent>) (component =>
    {
      if (component == null)
        return;
      foreach (object obj in (IEnumerable) ModCompat.DefenseGrid.grids.Invoke(component))
        ModCompat.DefenseGrid.RepaintGrid.Invoke((object) component, SingleParam.Get(obj));
    }), this.InterceptorMapComponent)), Array.Empty<Action>());
    Ext_EventManager.AddEvent<VehicleEventDef>((IEventManager<VehicleEventDef>) this, VehicleEventDefOf.Despawned, (Action) (() =>
    {
      if (this.InterceptorMapComponent == null)
        return;
      foreach (object obj in (IEnumerable) ModCompat.DefenseGrid.grids.Invoke(this.InterceptorMapComponent))
        ModCompat.DefenseGrid.UnpaintGrid.Invoke((object) this.InterceptorMapComponent, SingleParam.Get(obj));
    }), Array.Empty<Action>());
  }

  public void Resize()
  {
    if (!this.resizeRequest)
      return;
    this.resizeRequest = false;
    this.ResizeNow();
  }

  [DebugAction("Vehicle Map Framework", "Set Transform.Rotation", false, false, false, false, false, 0, false)]
  private static void SetTransformRotation(Pawn pawn)
  {
    VehiclePawn vehicle = pawn as VehiclePawn;
    if (vehicle == null)
      Messages.Message("The selected pawn is not a vehicle.", MessageTypeDefOf.RejectInput, false);
    else
      DebugTools.curTool = new DebugTool($"{pawn}: to...", (Action) (() =>
      {
        double num1 = (double) Ext_Math.RotateAngle(Vector3Utility.ToAngleFlat(Vector3.op_Subtraction(UI.MouseMapPosition(), ((Thing) vehicle).DrawPos)), 90f);
        Rot8 fullRotation = vehicle.FullRotation;
        double asAngle = (double) ((Rot8) ref fullRotation).AsAngle;
        float num2 = (float) (num1 - asAngle);
        vehicle.Transform.rotation = num2;
        Messages.Message($"Set {pawn}'s Transform.rotation to {num2:F1}", MessageTypeDefOf.NeutralEvent, false);
      }), (Action) null);
  }

  [DebugAction("Vehicle Map Framework", "Reset Transform.Rotation", false, false, false, false, false, 0, false)]
  private static void ResetTransformRotation(Pawn pawn)
  {
    if (!(pawn is VehiclePawn vehiclePawn))
      Messages.Message("The selected pawn is not a vehicle.", MessageTypeDefOf.RejectInput, false);
    else
      vehiclePawn.Transform.rotation = 0.0f;
  }
}
