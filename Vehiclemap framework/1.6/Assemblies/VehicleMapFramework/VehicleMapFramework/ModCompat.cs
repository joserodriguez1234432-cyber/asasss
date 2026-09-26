// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.ModCompat
// Assembly: VehicleMapFramework, Version=1.6.562.0, Culture=neutral, PublicKeyToken=null
// MVID: 10A61882-945F-4CFC-9B06-CA8EEF5ADB36
// Assembly location: D:\Programas\steamapps\workshop\content\294100\3426502333\1.6\Assemblies\VehicleMapFramework.dll

using HarmonyLib;
using JetBrains.Annotations;
using RimWorld;
using RimWorld.Planet;
using SmashTools;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading;
using UnityEngine;
using VehicleMapFramework.VMF_HarmonyPatches;
using Vehicles;
using Verse;
using Verse.AI;

#nullable disable
namespace VehicleMapFramework;

[StaticConstructorOnStartup]
public static class ModCompat
{
  public static readonly bool AllowTool = ModCompat.IsModActive("UnlimitedHugs.AllowTool");
  public static readonly bool BillDoorsFramework = ModCompat.IsModActive("3HSTltd.Framework");
  public static readonly bool CombatExtended = ModCompat.IsModActive("CETeam.CombatExtended") || ModCompat.IsModActive("CETeam.CombatExtended_steam");
  public static readonly bool ColonyGroups = ModCompat.IsModActive("DerekBickley.LTOColonyGroupsFinal");
  public static readonly bool DeepStorage = ModCompat.IsModActive("LWM.DeepStorage");
  public static readonly bool Fortified = ModCompat.IsModActive("AOBA.Framework");
  public static readonly bool DrakkenLaserDrill = ModCompat.IsModActive("MYDE.DrakkenLaserDrill") || ModCompat.IsModActive("Mlie.DrakkenLaserDrill");
  public static readonly bool DrillTurret = ModCompat.IsModActive("Mlie.MiningCoDrillTurret");
  public static readonly bool ExosuitFramework = ModCompat.IsModActive("Aoba.Exosuit.Framework");
  public static readonly bool GiantImperialTurret = ModCompat.IsModActive("XMB.Giantimperialcannonturret.MO");
  public static readonly bool Gunplay = ModCompat.IsModActive("automatic.gunplay");
  public static readonly bool HospitalityCasino = ModCompat.IsModActive("Adamas.HospitalityCasino");
  public static readonly bool IRBM = ModCompat.IsModActive("kazepsi.irbm");
  public static readonly bool MuzzleFlash = ModCompat.IsModActive("IssacZhuang.MuzzleFlash");
  public static readonly bool ProjectRimFactory = ModCompat.IsModActive("spdskatr.projectrimfactory");
  public static readonly bool SmarterConstruction = ModCompat.IsModActive("dhultgren.smarterconstruction");
  public static readonly bool TabulaRasa = ModCompat.IsModActive("neronix17.toolbox");
  public static readonly bool VFEArchitect = ModCompat.IsModActive("VanillaExpanded.VFEArchitect");
  public static readonly bool VPsyE = ModCompat.IsModActive("VanillaExpanded.VPsycastsE");
  public static readonly bool VGE = ModCompat.IsModActive("vanillaexpanded.gravship");
  public static readonly bool VQEGenerator = ModCompat.IsModActive("vanillaquestsexpanded.generator");
  public static readonly bool Vivi = ModCompat.IsModActive("gguake.race.vivi");
  public static readonly bool WASDedPawn = ModCompat.IsModActive("addvans.WASDedPawn");
  public static readonly bool WhileYoureUp = ModCompat.IsModActive("CodeOptimist.JobsOfOpportunity") || ModCompat.IsModActive("zsbk.patch16.whileyoureup");
  public static readonly bool WVCWorkModes = ModCompat.IsModActive("wvc.sergkart.biotech.MoreMechanoidsWorkModes");
  public static readonly bool YayosCombat3 = ModCompat.IsModActive("Mlie.YayosCombat3");
  public static readonly bool PickUpAndHaul = ModCompat.IsModActive("Mehni.PickUpAndHaul");
  public static readonly bool TextTool = ModCompat.IsModActive("ferny.TextTool");
  public static readonly bool TraderShips = ModCompat.IsModActive("automatic.traderships");
  public static readonly bool UFHeavyIndustries = ModCompat.IsModActive("KindSeal.LOL");
  public static readonly bool NightmareCore = ModCompat.IsModActive("Nightmare.Core");
  public static readonly bool SmartPistol = ModCompat.IsModActive("rabiosus.smartpistol");
  public static readonly bool RealFogOfWar = ModCompat.IsModActive("Mlie.NWNRealFogOfWar");
  public static readonly bool RimWorldOfMagic = ModCompat.IsModActive("Torann.ARimworldOfMagic");
  public static readonly bool CeleTech = ModCompat.IsModActive("TOT.CeleTech.MKIII");
  public static readonly bool PerspectiveShift = ModCompat.IsModActive("ferny.PerspectiveShift");
  public static readonly bool PauseOtherSettlements = ModCompat.IsModActive("esvn.PauseOtherSettlementsSimulation");
  public static readonly bool CutPlantsBeforeBuilding = ModCompat.IsModActive("Mlie.CutPlantsBeforeBuilding");
  public static readonly bool AnimalCages = ModCompat.IsModActive("zal.animalcages");
  public static readonly bool DoNotHitMe = ModCompat.IsModActive("Og.do.not.hit.me");
  public static readonly bool AutoApparelPickup = ModCompat.IsModActive("Scorpio.AutoApparelPickup");

  static ModCompat()
  {
    if (UnitTestDetector.IsTestingContext)
    {
      ModCompat.CctorException = new ThreadLocal<Exception>();
    }
    else
    {
      foreach (Type innerType in AccessTools.InnerTypes(typeof (ModCompat)))
        RuntimeHelpers.RunClassConstructor(innerType.TypeHandle);
    }
  }

  [UsedImplicitly]
  internal static ThreadLocal<Exception> CctorException { get; private set; }

  internal static bool AnyNull(params object[] args)
  {
    for (int index = 0; index < args.Length; ++index)
    {
      if (args[index] == null)
      {
        ModCompat.LogError(new Exception($"Argument {index} is null."));
        return true;
      }
    }
    return false;
  }

  internal static void LogIncompat(string modName)
  {
    if (UnitTestDetector.IsTestingContext)
      return;
    ModCompat.LogError(new Exception(modName + " compatibility is broken."));
  }

  internal static bool IsModActive(string id)
  {
    return UnitTestDetector.IsTestingContext || ModsConfig.IsActive(id);
  }

  private static void LogError(Exception ex)
  {
    if (UnitTestDetector.IsTestingContext)
      ModCompat.CctorException.Value = ex;
    else
      VMF_Log.Error(ex.Message);
  }

  public abstract class CompatBase
  {
    protected virtual bool ShouldDrawSectionLayers => false;

    public virtual void DrawSectionLayers(
      VehicleSectionLayerManager component,
      Section section,
      Vector3 drawPos,
      Rot8 rot,
      float angle)
    {
    }
  }

  public abstract class CompatBase<T> : ModCompat.CompatBase where T : ModCompat.CompatBase<T>, new()
  {
    public static bool Active { get; protected set; }

    protected static void Initialize(
      string packageId,
      Action initialize,
      [ParamCollection] scoped Span<string> alternativeIds)
    {
      ModCompat.CompatBase<T>.Active = ModCompat.IsModActive(packageId);
      if (!ModCompat.CompatBase<T>.Active && !alternativeIds.IsEmpty)
      {
        Span<string> span = alternativeIds;
        for (int index = 0; index < span.Length; ++index)
        {
          if (ModCompat.IsModActive(span[index]))
          {
            ModCompat.CompatBase<T>.Active = true;
            break;
          }
        }
      }
      if (ModCompat.CompatBase<T>.Active)
      {
        try
        {
          if (initialize != null)
            initialize();
        }
        catch (Exception ex)
        {
          ModCompat.LogError(ex);
          ModCompat.CompatBase<T>.Active = false;
        }
        finally
        {
          if (ModCompat.AnyNull((object) AccessTools.GetDeclaredProperties(typeof (T)).Select<PropertyInfo, object>((Func<PropertyInfo, object>) (f => f.GetValue((object) null)))))
          {
            ModCompat.LogIncompat(typeof (T).Name);
            ModCompat.CompatBase<T>.Active = false;
          }
        }
      }
      T obj = new T();
      if (!ModCompat.CompatBase<T>.Active || !obj.ShouldDrawSectionLayers)
        return;
      VehicleSectionLayerManager.CompatClassesForDrawLayers.Add((ModCompat.CompatBase) obj);
    }
  }

  public static class VehicleFramework
  {
    public const string HarmonyId = "SmashPhil.VehicleFramework";
    public static readonly Dictionary<(VehicleDef, Rot4), Texture2D> CachedVehicleTextures;
    public static readonly AccessTools.FieldRef<CompFueledTravel, CompPower> connectedPower;
    public static readonly AccessTools.StructFieldRef<MapGridOwners.PathConfig, VehicleDef> vehicleDef;

    static VehicleFramework()
    {
      if (UnitTestDetector.IsTestingContext)
        return;
      ModCompat.VehicleFramework.CachedVehicleTextures = AccessTools.StaticFieldRefAccess<Dictionary<(VehicleDef, Rot4), Texture2D>>(typeof (VehicleTex), nameof (CachedVehicleTextures));
      ModCompat.VehicleFramework.connectedPower = AccessTools.FieldRefAccess<CompFueledTravel, CompPower>(nameof (connectedPower));
      ModCompat.VehicleFramework.vehicleDef = AccessTools.StructFieldRefAccess<MapGridOwners.PathConfig, VehicleDef>(nameof (vehicleDef));
      if (!ModCompat.AnyNull((object) ModCompat.VehicleFramework.CachedVehicleTextures, (object) ModCompat.VehicleFramework.connectedPower))
        return;
      ModCompat.LogIncompat("Vehicle Framework");
    }
  }

  public class AdaptiveStorage : ModCompat.CompatBase<ModCompat.AdaptiveStorage>
  {
    public static Type ThingClass { get; private set; }

    public static FastInvokeHandler Renderer { get; private set; }

    public static FastInvokeHandler SetAllPrintDatasDirty { get; private set; }

    private static Dictionary<Type, bool> SameOrSubClassDic { get; set; }

    static AdaptiveStorage()
    {
      ModCompat.CompatBase<ModCompat.AdaptiveStorage>.Initialize("adaptive.storage.framework", (Action) (() =>
      {
        ModCompat.AdaptiveStorage.ThingClass = AccessTools.TypeByName("AdaptiveStorage.ThingClass");
        ModCompat.AdaptiveStorage.Renderer = MethodInvoker.GetHandler(AccessTools.PropertyGetter(ModCompat.AdaptiveStorage.ThingClass, nameof (Renderer)), false);
        ModCompat.AdaptiveStorage.SetAllPrintDatasDirty = MethodInvoker.GetHandler(AccessTools.Method("AdaptiveStorage.StorageRenderer:SetAllPrintDatasDirty", (Type[]) null, (Type[]) null), false);
        ModCompat.AdaptiveStorage.SameOrSubClassDic = new Dictionary<Type, bool>();
      }), new Span<string>());
    }

    public static bool IsAdaptiveStorageClass(Type type)
    {
      bool flag;
      if (!ModCompat.AdaptiveStorage.SameOrSubClassDic.TryGetValue(type, out flag))
        ModCompat.AdaptiveStorage.SameOrSubClassDic[type] = flag = GenTypes.SameOrSubclassOf(type, ModCompat.AdaptiveStorage.ThingClass);
      return flag;
    }
  }

  public class CallTradeShips : ModCompat.CompatBase<ModCompat.CallTradeShips>
  {
    public static Type Job_CallTradeShip { get; private set; }

    public static AccessTools.FieldRef<Job, TraderKindDef> TraderKindDef { get; private set; }

    public static AccessTools.FieldRef<Job, int> TraderKind { get; private set; }

    static CallTradeShips()
    {
      ModCompat.CompatBase<ModCompat.CallTradeShips>.Initialize("calltradeships.kv.rw", (Action) (() =>
      {
        ModCompat.CallTradeShips.Job_CallTradeShip = GenTypes.GetTypeInAnyAssembly("CallTradeShips.Job_CallTradeShip", nameof (CallTradeShips));
        ModCompat.CallTradeShips.TraderKindDef = (AccessTools.FieldRef<Job, TraderKindDef>) AccessTools.FieldRefAccess<TraderKindDef>(ModCompat.CallTradeShips.Job_CallTradeShip, nameof (TraderKindDef));
        ModCompat.CallTradeShips.TraderKind = (AccessTools.FieldRef<Job, int>) AccessTools.FieldRefAccess<int>(ModCompat.CallTradeShips.Job_CallTradeShip, nameof (TraderKind));
      }), new Span<string>());
    }
  }

  public class DubsBadHygiene : ModCompat.CompatBase<ModCompat.DubsBadHygiene>
  {
    public static bool LiteMode { get; private set; }

    public static Type SectionLayer_ThingsSewagePipe { get; private set; }

    public static Type SectionLayer_SewagePipeOverlay { get; private set; }

    public static Type SectionLayer_AirDuctOverlay { get; private set; }

    public static Type SectionLayer_Irrigation { get; private set; }

    public static Type SectionLayer_FertilizerGrid { get; private set; }

    public static Type CompProperties_Pipe { get; private set; }

    public static AccessTools.FieldRef<object, int> CompProperties_Pipe_mode { get; private set; }

    public static AccessTools.FieldRef<object, int> SectionLayer_PipeOverlay_mode { get; private set; }

    static DubsBadHygiene()
    {
      ModCompat.CompatBase<ModCompat.DubsBadHygiene>.Initialize("Dubwise.DubsBadHygiene", (Action) (() =>
      {
        Patch_JobGiver_Work_PawnCanUseWorkGiver.NoNeedVirtualMapTransferList.Add(GenTypes.GetTypeInAnyAssembly("DubsBadHygiene.WorkGiver_PlaceFertilizer", nameof (DubsBadHygiene)));
        ModCompat.DubsBadHygiene.LiteMode = (bool) AccessTools.PropertyGetter("DubsBadHygiene.Settings:LiteMode").Invoke((object) null, (object[]) null);
        if (ModCompat.DubsBadHygiene.LiteMode)
          return;
        ModCompat.DubsBadHygiene.SectionLayer_ThingsSewagePipe = GenTypes.GetTypeInAnyAssembly("DubsBadHygiene.SectionLayer_SewagePipeOverlay", nameof (DubsBadHygiene));
        if (ModCompat.DubsBadHygiene.SectionLayer_ThingsSewagePipe != (Type) null)
          VehicleSectionLayerManager.OrientedSectionLayerTypes.Add(ModCompat.DubsBadHygiene.SectionLayer_ThingsSewagePipe);
        ModCompat.DubsBadHygiene.SectionLayer_SewagePipeOverlay = GenTypes.GetTypeInAnyAssembly("DubsBadHygiene.SectionLayer_SewagePipeOverlay", nameof (DubsBadHygiene));
        ModCompat.DubsBadHygiene.SectionLayer_AirDuctOverlay = GenTypes.GetTypeInAnyAssembly("DubsBadHygiene.SectionLayer_AirDuctOverlay", nameof (DubsBadHygiene));
        ModCompat.DubsBadHygiene.SectionLayer_Irrigation = GenTypes.GetTypeInAnyAssembly("DubsBadHygiene.SectionLayer_Irrigation", nameof (DubsBadHygiene));
        ModCompat.DubsBadHygiene.SectionLayer_FertilizerGrid = GenTypes.GetTypeInAnyAssembly("DubsBadHygiene.SectionLayer_FertilizerGrid", nameof (DubsBadHygiene));
        ModCompat.DubsBadHygiene.CompProperties_Pipe = GenTypes.GetTypeInAnyAssembly("DubsBadHygiene.CompProperties_Pipe", nameof (DubsBadHygiene));
        ModCompat.DubsBadHygiene.CompProperties_Pipe_mode = AccessTools.FieldRefAccess<int>(ModCompat.DubsBadHygiene.CompProperties_Pipe, "mode");
        ModCompat.DubsBadHygiene.SectionLayer_PipeOverlay_mode = AccessTools.FieldRefAccess<int>("DubsBadHygiene.SectionLayer_PipeOverlay:mode");
      }), new Span<string>(new string[1]
      {
        "Dubwise.DubsBadHygiene.Lite"
      }));
    }

    protected override bool ShouldDrawSectionLayers => !ModCompat.DubsBadHygiene.LiteMode;

    public override void DrawSectionLayers(
      VehicleSectionLayerManager component,
      Section section,
      Vector3 drawPos,
      Rot8 rot,
      float angle)
    {
      Designator selectedDesignator = Find.DesignatorManager.SelectedDesignator;
      SectionLayer layer1 = component.GetLayer(section, ModCompat.DubsBadHygiene.SectionLayer_SewagePipeOverlay, new Rot8());
      SectionLayer layer2 = component.GetLayer(section, ModCompat.DubsBadHygiene.SectionLayer_AirDuctOverlay, new Rot8());
      if (selectedDesignator is Designator_Build designatorBuild && ((Designator_Place) designatorBuild).PlacingDef is ThingDef placingDef)
      {
        List<CompProperties> comps = placingDef.comps;
        CompProperties compProperties;
        if ((compProperties = comps.Find((Predicate<CompProperties>) (c =>
        {
          Type compPropertiesPipe = ModCompat.DubsBadHygiene.CompProperties_Pipe;
          return (object) compPropertiesPipe != null && compPropertiesPipe.IsAssignableFrom(c.GetType());
        }))) != null)
        {
          int num = ModCompat.DubsBadHygiene.CompProperties_Pipe_mode.Invoke((object) compProperties);
          if (layer1 != null & ModCompat.DubsBadHygiene.SectionLayer_PipeOverlay_mode.Invoke((object) layer1) == num)
            VehicleSectionLayerManager.DrawLayer(component, section, ModCompat.DubsBadHygiene.SectionLayer_SewagePipeOverlay, Vector3Utility.Yto0(drawPos), rot, angle);
          if (layer2 != null && ModCompat.DubsBadHygiene.SectionLayer_PipeOverlay_mode.Invoke((object) layer2) == num)
            VehicleSectionLayerManager.DrawLayer(component, section, ModCompat.DubsBadHygiene.SectionLayer_AirDuctOverlay, Vector3Utility.Yto0(drawPos), rot, angle);
          if (Time.frameCount % 120 == 0)
          {
            ((MapDrawLayer) component.GetLayer(section, ModCompat.DubsBadHygiene.SectionLayer_SewagePipeOverlay, rot))?.Regenerate();
            ((MapDrawLayer) component.GetLayer(section, ModCompat.DubsBadHygiene.SectionLayer_AirDuctOverlay, rot))?.Regenerate();
          }
        }
      }
      VehicleSectionLayerManager.DrawLayer(component, section, ModCompat.DubsBadHygiene.SectionLayer_Irrigation, Vector3Utility.Yto0(drawPos), rot, angle);
      VehicleSectionLayerManager.DrawLayer(component, section, ModCompat.DubsBadHygiene.SectionLayer_FertilizerGrid, Vector3Utility.Yto0(drawPos), rot, angle);
    }
  }

  public class Rimefeller : ModCompat.CompatBase<ModCompat.Rimefeller>
  {
    public static Type SectionLayer_SewagePipe { get; private set; }

    public static Type SectionLayer_ThingsPipe { get; private set; }

    public static Type XSectionLayer_Napalm { get; private set; }

    public static Type XSectionLayer_OilSpill { get; private set; }

    public static Type CompProperties_Pipe { get; private set; }

    public static AccessTools.FieldRef<object, int> CompProperties_Pipe_mode { get; private set; }

    public static AccessTools.FieldRef<object, int> SectionLayer_PipeOverlay_mode { get; private set; }

    static Rimefeller()
    {
      ModCompat.CompatBase<ModCompat.Rimefeller>.Initialize("Dubwise.Rimefeller", (Action) (() =>
      {
        ModCompat.Rimefeller.SectionLayer_SewagePipe = GenTypes.GetTypeInAnyAssembly("Rimefeller.SectionLayer_SewagePipe", nameof (Rimefeller));
        ModCompat.Rimefeller.SectionLayer_ThingsPipe = GenTypes.GetTypeInAnyAssembly("Rimefeller.SectionLayer_ThingsPipe", nameof (Rimefeller));
        if (ModCompat.Rimefeller.SectionLayer_ThingsPipe != (Type) null)
          VehicleSectionLayerManager.OrientedSectionLayerTypes.Add(ModCompat.Rimefeller.SectionLayer_ThingsPipe);
        ModCompat.Rimefeller.XSectionLayer_Napalm = GenTypes.GetTypeInAnyAssembly("Rimefeller.XSectionLayer_Napalm", nameof (Rimefeller));
        ModCompat.Rimefeller.XSectionLayer_OilSpill = GenTypes.GetTypeInAnyAssembly("Rimefeller.XSectionLayer_OilSpill", nameof (Rimefeller));
        ModCompat.Rimefeller.CompProperties_Pipe = GenTypes.GetTypeInAnyAssembly("Rimefeller.CompProperties_Pipe", nameof (Rimefeller));
        ModCompat.Rimefeller.CompProperties_Pipe_mode = AccessTools.FieldRefAccess<int>(ModCompat.Rimefeller.CompProperties_Pipe, "mode");
        ModCompat.Rimefeller.SectionLayer_PipeOverlay_mode = AccessTools.FieldRefAccess<int>("Rimefeller.SectionLayer_PipeOverlay:mode");
      }), new Span<string>());
    }

    protected override bool ShouldDrawSectionLayers => true;

    public override void DrawSectionLayers(
      VehicleSectionLayerManager component,
      Section section,
      Vector3 drawPos,
      Rot8 rot,
      float angle)
    {
      Designator selectedDesignator = Find.DesignatorManager.SelectedDesignator;
      SectionLayer layer = component.GetLayer(section, ModCompat.Rimefeller.SectionLayer_SewagePipe, rot);
      if (selectedDesignator is Designator_Build designatorBuild && ((Designator_Place) designatorBuild).PlacingDef is ThingDef placingDef)
      {
        List<CompProperties> comps = placingDef.comps;
        CompProperties compProperties;
        if ((compProperties = comps.Find((Predicate<CompProperties>) (c =>
        {
          Type compPropertiesPipe = ModCompat.Rimefeller.CompProperties_Pipe;
          return (object) compPropertiesPipe != null && compPropertiesPipe.IsAssignableFrom(c.GetType());
        }))) != null)
        {
          int num = ModCompat.Rimefeller.CompProperties_Pipe_mode.Invoke((object) compProperties);
          if (layer != null & ModCompat.Rimefeller.SectionLayer_PipeOverlay_mode.Invoke((object) layer) == num)
            VehicleSectionLayerManager.DrawLayer(component, section, ModCompat.Rimefeller.SectionLayer_SewagePipe, Vector3Utility.Yto0(drawPos), rot, angle);
          if (Time.frameCount % 120 == 0)
            ((MapDrawLayer) component.GetLayer(section, ModCompat.Rimefeller.SectionLayer_SewagePipe, rot))?.Regenerate();
        }
      }
      VehicleSectionLayerManager.DrawLayer(component, section, ModCompat.Rimefeller.XSectionLayer_Napalm, drawPos, rot, angle);
      VehicleSectionLayerManager.DrawLayer(component, section, ModCompat.Rimefeller.XSectionLayer_OilSpill, drawPos, rot, angle);
      VehicleSectionLayerManager.DrawLayer(component, section, ModCompat.Rimefeller.SectionLayer_ThingsPipe, drawPos, rot, angle);
    }
  }

  public class DefenseGrid : ModCompat.CompatBase<ModCompat.DefenseGrid>
  {
    public static Type SectionLayer_DefenseGridOverlay { get; private set; }

    public static Type CompDefenseConduit { get; private set; }

    public static Type Designator_DeconstructConduit { get; private set; }

    public static Type InterceptorMapComponent { get; private set; }

    public static AccessTools.FieldRef<MapComponent, IList> grids { get; private set; }

    public static AccessTools.FieldRef<object, MapComponent> mapComponent { get; private set; }

    public static FastInvokeHandler RepaintGrid { get; private set; }

    public static FastInvokeHandler UnpaintGrid { get; private set; }

    static DefenseGrid()
    {
      ModCompat.CompatBase<ModCompat.DefenseGrid>.Initialize("Aelanna.EccentricTech.DefenseGrid", (Action) (() =>
      {
        ModCompat.DefenseGrid.SectionLayer_DefenseGridOverlay = AccessTools.TypeByName("EccentricDefenseGrid.SectionLayer_DefenseGridOverlay");
        ModCompat.DefenseGrid.CompDefenseConduit = AccessTools.TypeByName("EccentricDefenseGrid.CompDefenseConduit");
        ModCompat.DefenseGrid.Designator_DeconstructConduit = AccessTools.TypeByName("EccentricDefenseGrid.Designator_DeconstructConduit");
        ModCompat.DefenseGrid.InterceptorMapComponent = GenTypes.GetTypeInAnyAssembly("EccentricProjectiles.InterceptorMapComponent", "EccentricProjectiles");
        ModCompat.DefenseGrid.grids = (AccessTools.FieldRef<MapComponent, IList>) AccessTools.FieldRefAccess<IList>(ModCompat.DefenseGrid.InterceptorMapComponent, nameof (grids));
        ModCompat.DefenseGrid.mapComponent = AccessTools.FieldRefAccess<MapComponent>(GenTypes.GetTypeInAnyAssembly("EccentricProjectiles.InterceptorGrid", "EccentricProjectiles"), nameof (mapComponent));
        ModCompat.DefenseGrid.RepaintGrid = MethodInvoker.GetHandler(AccessTools.Method(ModCompat.DefenseGrid.InterceptorMapComponent, nameof (RepaintGrid), (Type[]) null, (Type[]) null), false);
        ModCompat.DefenseGrid.UnpaintGrid = MethodInvoker.GetHandler(AccessTools.Method(ModCompat.DefenseGrid.InterceptorMapComponent, nameof (UnpaintGrid), (Type[]) null, (Type[]) null), false);
      }), new Span<string>());
    }

    protected override bool ShouldDrawSectionLayers => true;

    public override void DrawSectionLayers(
      VehicleSectionLayerManager component,
      Section section,
      Vector3 drawPos,
      Rot8 rot,
      float angle)
    {
      Designator selectedDesignator = Find.DesignatorManager.SelectedDesignator;
      if ((!(selectedDesignator is Designator_Build designatorBuild) || !(((Designator_Place) designatorBuild).PlacingDef is ThingDef placingDef) || !placingDef.HasComp(ModCompat.DefenseGrid.CompDefenseConduit)) && !ModCompat.DefenseGrid.Designator_DeconstructConduit.IsInstanceOfType((object) selectedDesignator))
        return;
      VehicleSectionLayerManager.DrawLayer(component, section, ModCompat.DefenseGrid.SectionLayer_DefenseGridOverlay, Vector3Utility.Yto0(drawPos), rot, angle);
    }
  }

  public class MeleeAnimation : ModCompat.CompatBase<ModCompat.MeleeAnimation>
  {
    public static AccessTools.FieldRef<object, Map> AnimRenderer_Map { get; private set; }

    public static AccessTools.FieldRef<object, Matrix4x4> AnimRenderer_RootTransform { get; private set; }

    public static AccessTools.FieldRef<object, Def> AnimRenderer_Def { get; private set; }

    public static AccessTools.FieldRef<Def, IReadOnlyList<object>> AnimRenderer_cellData { get; private set; }

    public static MethodInfo m_GetWorldPosition { get; private set; }

    public static MethodInfo m_GetWorldPositionOffset { get; private set; }

    static MeleeAnimation()
    {
      ModCompat.CompatBase<ModCompat.MeleeAnimation>.Initialize("co.uk.epicguru.meleeanimation", (Action) (() =>
      {
        ModCompat.MeleeAnimation.AnimRenderer_Map = AccessTools.FieldRefAccess<Map>("AM.AnimRenderer:Map");
        ModCompat.MeleeAnimation.AnimRenderer_RootTransform = AccessTools.FieldRefAccess<Matrix4x4>("AM.AnimRenderer:RootTransform");
        ModCompat.MeleeAnimation.AnimRenderer_Def = AccessTools.FieldRefAccess<Def>("AM.AnimRenderer:Def");
        ModCompat.MeleeAnimation.AnimRenderer_cellData = (AccessTools.FieldRef<Def, IReadOnlyList<object>>) AccessTools.FieldRefAccess<IReadOnlyList<object>>("AM.AnimDef:cellData");
        ModCompat.MeleeAnimation.m_GetWorldPosition = AccessTools.Method("AnimPartSnapshot:GetWorldPosition", (Type[]) null, (Type[]) null);
        ModCompat.MeleeAnimation.m_GetWorldPositionOffset = new \u003C\u003EF\u007B00000001\u007D<object, Vector3, Vector3>(Patch_AnimRenderer_DrawPawns.GetWorldPositionOffset).Method;
      }), new Span<string>());
    }
  }

  public class MiscRobots : ModCompat.CompatBase<ModCompat.MiscRobots>
  {
    public static Type X2_AIRobot { get; private set; }

    public static AccessTools.FieldRef<Pawn, Building> rechargeStation { get; private set; }

    static MiscRobots()
    {
      ModCompat.CompatBase<ModCompat.MiscRobots>.Initialize("Haplo.Miscellaneous.Robots", (Action) (() =>
      {
        ModCompat.MiscRobots.X2_AIRobot = GenTypes.GetTypeInAnyAssembly("AIRobot.X2_AIRobot", "AIRobot");
        ModCompat.MiscRobots.rechargeStation = (AccessTools.FieldRef<Pawn, Building>) AccessTools.FieldRefAccess<Building>("AIRobot.X2_AIRobot:rechargeStation");
      }), new Span<string>());
    }
  }

  public class VFECore : ModCompat.CompatBase<ModCompat.VFECore>
  {
    private static Def pipeNet;

    public static Type PipeNetDef { get; private set; }

    public static Type SectionLayer_Resource { get; private set; }

    public static FastInvokeHandler ShouldDraw { get; private set; }

    public static AccessTools.FieldRef<Def> pipeNetDef { get; private set; }

    static VFECore()
    {
      ModCompat.CompatBase<ModCompat.VFECore>.Initialize("OskarPotocki.VanillaFactionsExpanded.Core", (Action) (() =>
      {
        ModCompat.VFECore.PipeNetDef = GenTypes.GetTypeInAnyAssembly("PipeSystem.PipeNetDef", (string) null);
        ModCompat.VFECore.SectionLayer_Resource = GenTypes.GetTypeInAnyAssembly("PipeSystem.SectionLayer_Resource", "PipeSystem");
        ModCompat.VFECore.ShouldDraw = MethodInvoker.GetHandler(AccessTools.PropertyGetter(ModCompat.VFECore.SectionLayer_Resource, nameof (ShouldDraw)), false);
        ModCompat.VFECore.pipeNetDef = AccessTools.StaticFieldRefAccess<Def>(AccessTools.Field(ModCompat.VFECore.SectionLayer_Resource, nameof (pipeNet)));
      }), new Span<string>());
    }

    protected override bool ShouldDrawSectionLayers => true;

    public override void DrawSectionLayers(
      VehicleSectionLayerManager component,
      Section section,
      Vector3 drawPos,
      Rot8 rot,
      float angle)
    {
      SectionLayer layer = component.GetLayer(section, ModCompat.VFECore.SectionLayer_Resource, rot);
      if (layer == null || !(bool) ModCompat.VFECore.ShouldDraw.Invoke((object) layer, Array.Empty<object>()))
        return;
      Def def = ModCompat.VFECore.pipeNetDef.Invoke();
      if (ModCompat.VFECore.pipeNet != def)
      {
        ModCompat.VFECore.pipeNet = def;
        VehiclePawnWithMap vehicle;
        if (component.map.IsVehicleMapOf(out vehicle))
          vehicle.CurrentLevel.mapDrawer.WholeMapChanged(455UL);
      }
      VehicleSectionLayerManager.DrawLayer(layer, drawPos, angle);
    }
  }

  public class VFESecurity : ModCompat.CompatBase<ModCompat.VFESecurity>
  {
    public static AccessTools.FieldRef<object, GlobalTargetInfo> worldTarget { get; private set; }

    public static AccessTools.FieldRef<object, int> worldMapAttackRange { get; private set; }

    static VFESecurity()
    {
      ModCompat.CompatBase<ModCompat.VFESecurity>.Initialize("VanillaExpanded.VFESecurity", (Action) (() =>
      {
        ModCompat.VFESecurity.worldTarget = AccessTools.FieldRefAccess<GlobalTargetInfo>("VFESecurity.CompWorldArtillery:worldTarget");
        ModCompat.VFESecurity.worldMapAttackRange = AccessTools.FieldRefAccess<int>("VFESecurity.CompProperties_WorldArtillery:worldMapAttackRange");
      }), new Span<string>());
    }
  }

  public class VFEFactory : ModCompat.CompatBase<ModCompat.VFEFactory>
  {
    static VFEFactory()
    {
      ModCompat.CompatBase<ModCompat.VFEFactory>.Initialize("VanillaExpanded.VFEFactory", (Action) (() => { }), new Span<string>());
    }
  }

  public class VanillaVehiclesExpanded : ModCompat.CompatBase<ModCompat.VanillaVehiclesExpanded>
  {
    public static AccessTools.FieldRef<CompProperties, float> refuelAmountPerTick { get; private set; }

    static VanillaVehiclesExpanded()
    {
      ModCompat.CompatBase<ModCompat.VanillaVehiclesExpanded>.Initialize("OskarPotocki.VanillaVehiclesExpanded", (Action) (() => ModCompat.VanillaVehiclesExpanded.refuelAmountPerTick = (AccessTools.FieldRef<CompProperties, float>) AccessTools.FieldRefAccess<float>("VanillaVehiclesExpanded.CompProperties_RefuelingPump:refuelAmountPerTick")), new Span<string>());
    }
  }

  public class VanillaTemperatureExpanded : 
    ModCompat.CompatBase<ModCompat.VanillaTemperatureExpanded>
  {
    public static Type ProxyHeatManager { get; private set; }

    public static FastInvokeHandler RemoveComp { get; private set; }

    static VanillaTemperatureExpanded()
    {
      ModCompat.CompatBase<ModCompat.VanillaTemperatureExpanded>.Initialize("VanillaExpanded.Temperature", (Action) (() =>
      {
        ModCompat.VanillaTemperatureExpanded.ProxyHeatManager = GenTypes.GetTypeInAnyAssembly("ProxyHeat.ProxyHeatManager", "ProxyHeat");
        ModCompat.VanillaTemperatureExpanded.RemoveComp = MethodInvoker.GetHandler(AccessTools.Method(ModCompat.VanillaTemperatureExpanded.ProxyHeatManager, nameof (RemoveComp), (Type[]) null, (Type[]) null), false);
      }), new Span<string>());
    }
  }

  public class EnergyShield : ModCompat.CompatBase<ModCompat.EnergyShield>
  {
    public static Type Building_Shield { get; private set; }

    public static bool CECompat { get; private set; }

    static EnergyShield()
    {
      ModCompat.CompatBase<ModCompat.EnergyShield>.Initialize("zhuzi.AdvancedEnergy.Shields", (Action) (() =>
      {
        ModCompat.EnergyShield.Building_Shield = AccessTools.TypeByName("zhuzi.AdvancedEnergy.Shields.Shields.Building_Shield");
        ModCompat.EnergyShield.CECompat = ModCompat.IsModActive("cn.zhuzijun.EnergyShieldCECompat");
      }), new Span<string>());
    }
  }

  public class Aquariums : ModCompat.CompatBase<ModCompat.Aquariums>
  {
    public static FastInvokeHandler CurrentTank { get; private set; }

    static Aquariums()
    {
      ModCompat.CompatBase<ModCompat.Aquariums>.Initialize("Nightmare.Aquariums", (Action) (() => ModCompat.Aquariums.CurrentTank = MethodInvoker.GetHandler(AccessTools.PropertyGetter("Aquariums.AquariumFish:CurrentTank"), false)), new Span<string>());
    }
  }

  public class AsAboveSoBelow : ModCompat.CompatBase<ModCompat.AsAboveSoBelow>
  {
    public const string HarmonyId = "astryl.asabovesobelow";
    public static Func<int, int> SlotFor;
    public static FieldInfo pending;
    public static Func<int> UpperLevels;
    public static ConstructorInfo PendingLayout;
    public static FieldInfo bandCount;
    public static FieldInfo bandHeight;
    public static Func<Map, MapComponent> CompOf;
    public static AccessTools.FieldRef<MapComponent, int> surfaceBand;
    public static Func<MapComponent, bool> Banded;
    public static Func<MapComponent, IntVec3, int> BandOf;
    public static Func<MapComponent, IntVec3, int, IntVec3> Translate;
    public static Func<Map, int> CurrentBand;
    public static Func<Map, int, CellRect> RectOfBand;
    public static ModCompat.AsAboveSoBelow.GetBandRect TryBandRectOf;
    public static FastInvokeHandler TryResolveVisibleBelow;
    public static Func<Pawn, Vector3, Vector3> LocalizeForPawn;
    private static Type SectionLayer_ABBelowV2;

    static AsAboveSoBelow()
    {
      ModCompat.CompatBase<ModCompat.AsAboveSoBelow>.Initialize("astryl.AsAboveSoBelow2", (Action) (() =>
      {
        ModCompat.AsAboveSoBelow.SlotFor = AccessTools.MethodDelegate<Func<int, int>>("AsAboveSoBelow.ABBandMap:SlotFor", (object) null, true, (Type[]) null);
        ModCompat.AsAboveSoBelow.pending = AccessTools.Field("AsAboveSoBelow.ABBandedGeneration:pending");
        ModCompat.AsAboveSoBelow.UpperLevels = AccessTools.MethodDelegate<Func<int>>("AsAboveSoBelow.ABMapSizeLimit:get_UpperLevels", (object) null, true, (Type[]) null);
        Type typeInAnyAssembly = GenTypes.GetTypeInAnyAssembly("AsAboveSoBelow.ABBandedGeneration+PendingLayout", (string) null);
        ModCompat.AsAboveSoBelow.PendingLayout = AccessTools.Constructor(typeInAnyAssembly, (Type[]) null, false);
        ModCompat.AsAboveSoBelow.bandCount = AccessTools.Field(typeInAnyAssembly, nameof (bandCount));
        ModCompat.AsAboveSoBelow.bandHeight = AccessTools.Field(typeInAnyAssembly, nameof (bandHeight));
        ModCompat.AsAboveSoBelow.CompOf = AccessTools.MethodDelegate<Func<Map, MapComponent>>("AsAboveSoBelow.ABBands:CompOf", (object) null, true, (Type[]) null);
        ModCompat.AsAboveSoBelow.surfaceBand = (AccessTools.FieldRef<MapComponent, int>) AccessTools.FieldRefAccess<int>("AsAboveSoBelow.ABBandMap:surfaceBand");
        FastInvokeHandler handler = MethodInvoker.GetHandler(AccessTools.PropertyGetter("AsAboveSoBelow.ABBandMap:Banded"), false);
        ModCompat.AsAboveSoBelow.Banded = (Func<MapComponent, bool>) (component => (bool) handler.Invoke((object) component, Array.Empty<object>()));
        FastInvokeHandler handler2 = MethodInvoker.GetHandler(AccessTools.Method("AsAboveSoBelow.ABBandMap:BandOf", (Type[]) null, (Type[]) null), false);
        ModCompat.AsAboveSoBelow.BandOf = (Func<MapComponent, IntVec3, int>) ((component, c) => (int) handler2.Invoke((object) component, Params<ValueTuple<IntVec3>>.Get((c))));
        FastInvokeHandler handler3 = MethodInvoker.GetHandler(AccessTools.Method("AsAboveSoBelow.ABBandMap:Translate", (Type[]) null, (Type[]) null), false);
        ModCompat.AsAboveSoBelow.Translate = (Func<MapComponent, IntVec3, int, IntVec3>) ((component, c, toBand) => (IntVec3) handler3.Invoke((object) component, Params<(IntVec3, int)>.Get((c, toBand))));
        ModCompat.AsAboveSoBelow.CurrentBand = AccessTools.MethodDelegate<Func<Map, int>>("AsAboveSoBelow.ABBandView:CurrentBand", (object) null, true, (Type[]) null);
        ModCompat.AsAboveSoBelow.RectOfBand = AccessTools.MethodDelegate<Func<Map, int, CellRect>>("AsAboveSoBelow.ABBands:RectOfBand", (object) null, true, (Type[]) null);
        ModCompat.AsAboveSoBelow.TryBandRectOf = AccessTools.MethodDelegate<ModCompat.AsAboveSoBelow.GetBandRect>("AsAboveSoBelow.ABBandSafety:TryBandRectOf", (object) null, true, (Type[]) null);
        ModCompat.AsAboveSoBelow.TryResolveVisibleBelow = MethodInvoker.GetHandler(AccessTools.Method("AsAboveSoBelow.ABBands:TryResolveVisibleBelow", (Type[]) null, (Type[]) null), false);
        ModCompat.AsAboveSoBelow.LocalizeForPawn = AccessTools.MethodDelegate<Func<Pawn, Vector3, Vector3>>("AsAboveSoBelow.ABUIGeometry:LocalizeForPawn", (object) null, true, (Type[]) null);
        ModCompat.AsAboveSoBelow.SectionLayer_ABBelowV2 = GenTypes.GetTypeInAnyAssembly("AsAboveSoBelow.SectionLayer_ABBelowV2", nameof (AsAboveSoBelow));
        VehicleSectionLayerManager.OrientedSectionLayerTypes.Add(ModCompat.AsAboveSoBelow.SectionLayer_ABBelowV2);
      }), new Span<string>());
    }

    public static void CopyBoolGrid(VehiclePawnWithMap vehicle, BoolGrid grid)
    {
      if (!ModCompat.CompatBase<ModCompat.AsAboveSoBelow>.Active)
        return;
      MapComponent mapComponent = ModCompat.AsAboveSoBelow.CompOf(vehicle.VehicleMap);
      if (mapComponent == null || !ModCompat.AsAboveSoBelow.Banded(mapComponent))
        return;
      CellRect mapRect = vehicle.MapRect;
      for (int index = 1; index <= ModCompat.AsAboveSoBelow.UpperLevels(); ++index)
      {
        CellRect cellRect = ModCompat.AsAboveSoBelow.RectOfBand(vehicle.VehicleMap, index);
        IntVec3 min = ((CellRect) ref cellRect).Min;
        foreach (IntVec3 intVec3 in mapRect)
          grid[IntVec3.op_Addition(intVec3, min)] = grid[intVec3];
      }
    }

    protected override bool ShouldDrawSectionLayers => true;

    public override void DrawSectionLayers(
      VehicleSectionLayerManager component,
      Section section,
      Vector3 drawPos,
      Rot8 rot,
      float angle)
    {
      VehicleSectionLayerManager.DrawLayer(component, section, ModCompat.AsAboveSoBelow.SectionLayer_ABBelowV2, Vector3Utility.WithYOffset(drawPos, -0.018292686f), rot, angle);
    }

    public static IntVec3 TranslateToThingBand(IntVec3 c, Thing thing)
    {
      if (!ModCompat.CompatBase<ModCompat.AsAboveSoBelow>.Active)
        return c;
      MapComponent mapComponent = ModCompat.AsAboveSoBelow.CompOf(thing.Map);
      return mapComponent != null && ModCompat.AsAboveSoBelow.Banded(mapComponent) ? ModCompat.AsAboveSoBelow.Translate(mapComponent, c, ModCompat.AsAboveSoBelow.BandOf(mapComponent, thing.Position)) : c;
    }

    public static ModCompat.AsAboveSoBelow.TargetBand? GetTargetBand(Thing thing)
    {
      if (!ModCompat.CompatBase<ModCompat.AsAboveSoBelow>.Active || thing == null)
        return new ModCompat.AsAboveSoBelow.TargetBand?();
      MapComponent comp = ModCompat.AsAboveSoBelow.CompOf(thing.Map);
      if (comp == null)
        return new ModCompat.AsAboveSoBelow.TargetBand?();
      if (!ModCompat.AsAboveSoBelow.Banded(comp))
        return new ModCompat.AsAboveSoBelow.TargetBand?();
      int band = ModCompat.AsAboveSoBelow.BandOf(comp, thing.Position);
      return band == ModCompat.AsAboveSoBelow.surfaceBand.Invoke(comp) ? new ModCompat.AsAboveSoBelow.TargetBand?() : new ModCompat.AsAboveSoBelow.TargetBand?(new ModCompat.AsAboveSoBelow.TargetBand(comp, band));
    }

    public delegate bool GetBandRect(Map map, IntVec3 c, out CellRect band);

    public readonly struct TargetBand(MapComponent comp, int band)
    {
      public readonly MapComponent comp = comp;
      public readonly int band = band;

      public Map Map => this.comp.map;
    }
  }

  public class Rimatomics : ModCompat.CompatBase<ModCompat.Rimatomics>
  {
    public static Type CompProperties_Pipe { get; private set; }

    public static AccessTools.FieldRef<object, int> CompProperties_Pipe_mode { get; private set; }

    public static AccessTools.FieldRef<object, int> SectionLayer_OverlayPipe_mode { get; private set; }

    public static List<Type> SectionLayer_OverlayPipes { get; private set; }

    public static Type Designator_RemovePipe { get; private set; }

    public static AccessTools.FieldRef<Designator, int> Designator_RemovePipe_RemovalMode { get; private set; }

    public static Type SectionLayer_ThingsPipe { get; private set; }

    public static Type BaseMissile { get; private set; }

    static Rimatomics()
    {
      ModCompat.CompatBase<ModCompat.Rimatomics>.Initialize("Dubwise.Rimatomics", (Action) (() =>
      {
        Type typeInAnyAssembly = GenTypes.GetTypeInAnyAssembly("Rimatomics.SectionLayer_OverlayPipe", nameof (Rimatomics));
        ModCompat.Rimatomics.CompProperties_Pipe = GenTypes.GetTypeInAnyAssembly("Rimatomics.CompProperties_Pipe", nameof (Rimatomics));
        ModCompat.Rimatomics.CompProperties_Pipe_mode = AccessTools.FieldRefAccess<int>(ModCompat.Rimatomics.CompProperties_Pipe, "mode");
        ModCompat.Rimatomics.SectionLayer_OverlayPipe_mode = AccessTools.FieldRefAccess<int>(typeInAnyAssembly, "mode");
        ModCompat.Rimatomics.SectionLayer_OverlayPipes = GenTypes.AllSubclassesNonAbstract(typeInAnyAssembly);
        ModCompat.Rimatomics.Designator_RemovePipe = GenTypes.GetTypeInAnyAssembly("Rimatomics.Designator_RemovePipe", nameof (Rimatomics));
        ModCompat.Rimatomics.Designator_RemovePipe_RemovalMode = (AccessTools.FieldRef<Designator, int>) AccessTools.FieldRefAccess<int>(ModCompat.Rimatomics.Designator_RemovePipe, "RemovalMode");
        ModCompat.Rimatomics.SectionLayer_ThingsPipe = GenTypes.GetTypeInAnyAssembly("Rimatomics.SectionLayer_ThingsPipe", nameof (Rimatomics));
        ModCompat.Rimatomics.BaseMissile = GenTypes.GetTypeInAnyAssembly("Rimatomics.BaseMissile", nameof (Rimatomics));
      }), new Span<string>());
    }

    protected override bool ShouldDrawSectionLayers => true;

    public override void DrawSectionLayers(
      VehicleSectionLayerManager component,
      Section section,
      Vector3 drawPos,
      Rot8 rot,
      float angle)
    {
      Designator selectedDesignator = Find.DesignatorManager.SelectedDesignator;
      if (selectedDesignator?.GetType() == ModCompat.Rimatomics.Designator_RemovePipe)
      {
        int num = ModCompat.Rimatomics.Designator_RemovePipe_RemovalMode.Invoke(selectedDesignator);
        foreach (Type layerOverlayPipe in ModCompat.Rimatomics.SectionLayer_OverlayPipes)
        {
          if (num == ModCompat.Rimatomics.SectionLayer_OverlayPipe_mode.Invoke((object) component.GetLayer(section, layerOverlayPipe, rot)))
            VehicleSectionLayerManager.DrawLayer(component, section, layerOverlayPipe, drawPos, rot, angle);
        }
      }
      else if (selectedDesignator is Designator_Build designatorBuild && ((Designator_Place) designatorBuild).PlacingDef is ThingDef placingDef)
      {
        foreach (object obj in placingDef.comps.Where<CompProperties>((Func<CompProperties, bool>) (c => GenTypes.SameOrSubclassOf(c.GetType(), ModCompat.Rimatomics.CompProperties_Pipe))))
        {
          int num = ModCompat.Rimatomics.CompProperties_Pipe_mode.Invoke(obj);
          foreach (Type layerOverlayPipe in ModCompat.Rimatomics.SectionLayer_OverlayPipes)
          {
            if (num == ModCompat.Rimatomics.SectionLayer_OverlayPipe_mode.Invoke((object) component.GetLayer(section, layerOverlayPipe, rot)))
              VehicleSectionLayerManager.DrawLayer(component, section, layerOverlayPipe, drawPos, rot, angle);
          }
        }
      }
      VehicleSectionLayerManager.DrawLayer(component, section, ModCompat.Rimatomics.SectionLayer_ThingsPipe, drawPos, rot, angle);
    }
  }

  public class SmartFarming : ModCompat.CompatBase<ModCompat.SmartFarming>
  {
    private const string SmartFarmingPackageId = "Owlchemist.SmartFarming";
    private const string ReGrowthPackageId = "ReGrowth.BOTR.Core";
    public static readonly bool SmartFarmingActive = ModCompat.IsModActive("Owlchemist.SmartFarming");
    public static readonly bool ReGrowthActive = ModCompat.IsModActive("ReGrowth.BOTR.Core");

    public static Type MapComponent_SmartFarming { get; private set; }

    public static AccessTools.FieldRef<MapComponent, IDictionary> growZoneRegistry { get; private set; }

    public static AccessTools.FieldRef<object, int> priority { get; private set; }

    static SmartFarming()
    {
      ModCompat.CompatBase<ModCompat.SmartFarming>.Initialize("Owlchemist.SmartFarming", (Action) (() =>
      {
        if (ModCompat.SmartFarming.SmartFarmingActive && ModCompat.SmartFarming.ReGrowthActive && !UnitTestDetector.IsTestingContext)
          VMF_Log.Error("When both Smart Farming and ReGrowth 2 are enabled, a patch error will occur. Since these have overlapping functionality, please enable only one of them.");
        Type typeInAnyAssembly;
        if (ModCompat.SmartFarming.SmartFarmingActive)
        {
          ModCompat.SmartFarming.MapComponent_SmartFarming = GenTypes.GetTypeInAnyAssembly("SmartFarming.MapComponent_SmartFarming", nameof (SmartFarming));
          typeInAnyAssembly = GenTypes.GetTypeInAnyAssembly("SmartFarming.ZoneData", nameof (SmartFarming));
        }
        else
        {
          ModCompat.SmartFarming.MapComponent_SmartFarming = GenTypes.GetTypeInAnyAssembly("ReGrowthCore.MapComponent_SmartFarming", "ReGrowthCore");
          typeInAnyAssembly = GenTypes.GetTypeInAnyAssembly("ReGrowthCore.ZoneData", "ReGrowthCore");
        }
        ModCompat.SmartFarming.growZoneRegistry = (AccessTools.FieldRef<MapComponent, IDictionary>) AccessTools.FieldRefAccess<IDictionary>(ModCompat.SmartFarming.MapComponent_SmartFarming, nameof (growZoneRegistry));
        ModCompat.SmartFarming.priority = AccessTools.FieldRefAccess<int>(typeInAnyAssembly, nameof (priority));
      }), new Span<string>(new string[1]
      {
        "ReGrowth.BOTR.Core"
      }));
    }
  }

  public class MultiFloors : ModCompat.CompatBase<ModCompat.MultiFloors>
  {
    private static readonly object MinusOne = (object) -1;

    public static Func<Map, Map> GroundMap { get; private set; }

    public static Func<Map, int> GetLevel { get; private set; }

    public static Action<Map> RevalidateLaunchSiteState { get; private set; }

    public static Type SectionLayer_LowerLevel { get; private set; }

    private static Func<Map, MapComponent> GetCachedLevelMapComp { get; set; }

    private static FastInvokeHandler GetOtherMapVerticallyOutwardFromCache { get; set; }

    static MultiFloors()
    {
      ModCompat.CompatBase<ModCompat.MultiFloors>.Initialize("telardo.MultiFloors", (Action) (() =>
      {
        ModCompat.MultiFloors.GroundMap = AccessTools.MethodDelegate<Func<Map, Map>>("MultiFloors.HarmonyPatches.HarmonyPatch_CallBossGroupOnGround:GetGroundMap", (object) null, true, (Type[]) null);
        ModCompat.MultiFloors.GetLevel = AccessTools.MethodDelegate<Func<Map, int>>("MultiFloors.HarmonyPatches.HarmonyPatch_SortMapInColonistBarByLevel:GetLevel", (object) null, true, (Type[]) null);
        ModCompat.MultiFloors.RevalidateLaunchSiteState = AccessTools.MethodDelegate<Action<Map>>("MultiFloors.HarmonyPatches.HarmonyPatch_OnGravshipLaunch:RevalidateLaunchSiteState", (object) null, true, (Type[]) null);
        ModCompat.MultiFloors.SectionLayer_LowerLevel = GenTypes.GetTypeInAnyAssembly("MultiFloors.Maps.SectionLayer_LowerLevel", "MultiFloors.Maps");
        if ((object) ModCompat.MultiFloors.SectionLayer_LowerLevel != null)
          VehicleSectionLayerManager.OrientedSectionLayerTypes.Add(ModCompat.MultiFloors.SectionLayer_LowerLevel);
        ModCompat.MultiFloors.GetCachedLevelMapComp = AccessTools.MethodDelegate<Func<Map, MapComponent>>(AccessTools.Method(typeof (MapComponentCache<>).MakeGenericType(GenTypes.GetTypeInAnyAssembly("MultiFloors.MF_LevelMapComp", nameof (MultiFloors))), "GetComponent", new Type[1]
        {
          typeof (Map)
        }, (Type[]) null), (object) null, true, (Type[]) null);
        ModCompat.MultiFloors.GetOtherMapVerticallyOutwardFromCache = MethodInvoker.GetHandler(AccessTools.Method("MultiFloors.LevelUtility:GetOtherMapVerticallyOutwardFromCache", (Type[]) null, (Type[]) null), false);
      }), new Span<string>(new string[1]
      {
        "telardo.MultiFloorsDev"
      }));
    }

    public static IEnumerable<Map> GetOtherLevels(Map map)
    {
      return (IEnumerable<Map>) ModCompat.MultiFloors.GetOtherMapVerticallyOutwardFromCache.Invoke((object) null, Params<(object, object, object)>.Get(((object) map, (object) ModCompat.MultiFloors.GetCachedLevelMapComp(map), ModCompat.MultiFloors.MinusOne)));
    }

    protected override bool ShouldDrawSectionLayers => true;

    public override void DrawSectionLayers(
      VehicleSectionLayerManager component,
      Section section,
      Vector3 drawPos,
      Rot8 rot,
      float angle)
    {
      VehiclePawnWithMap vehicle;
      if (!component.map.IsVehicleMapOf(out vehicle) || vehicle.CurrentLevel == vehicle.VehicleMap)
        return;
      VehicleSectionLayerManager.DrawLayer(component, section, ModCompat.MultiFloors.SectionLayer_LowerLevel, drawPos, rot, angle);
    }
  }

  public class SRALib : ModCompat.CompatBase<ModCompat.SRALib>
  {
    public static List<Type> Building_TurretGunHasSpeed;
    public static FastInvokeHandler GetTargetingCount;

    static SRALib()
    {
      ModCompat.CompatBase<ModCompat.SRALib>.Initialize("DiZhuan.SRALib", (Action) (() =>
      {
        ModCompat.SRALib.Building_TurretGunHasSpeed = GenTypes.AllTypes.Where<Type>((Func<Type, bool>) (t => t.Name == nameof (Building_TurretGunHasSpeed))).ToList<Type>();
        ModCompat.SRALib.GetTargetingCount = MethodInvoker.GetHandler(AccessTools.Method("SRA.MapComponent_LaserADSManager:GetTargetingCount", (Type[]) null, (Type[]) null), false);
      }), new Span<string>());
    }
  }

  public class StackGap : ModCompat.CompatBase<ModCompat.StackGap>
  {
    public const string HarmonyId = "Andromeda.StackGap";

    static StackGap()
    {
      ModCompat.CompatBase<ModCompat.StackGap>.Initialize("Andromeda.StackGap", (Action) null, new Span<string>());
    }
  }

  public static class ProgressionEducation
  {
    public const string HarmonyId = "ProgressionEducationMod";
  }

  public class ColonyManagerRedux : ModCompat.CompatBase<ModCompat.ColonyManagerRedux>
  {
    static ColonyManagerRedux()
    {
      ModCompat.CompatBase<ModCompat.ColonyManagerRedux>.Initialize("ilyvion.colonymanagerredux", (Action) (() =>
      {
        Type typeInAnyAssembly = GenTypes.GetTypeInAnyAssembly("ColonyManagerRedux.WorkGiver_Manage", nameof (ColonyManagerRedux));
        if ((object) typeInAnyAssembly != null)
          JobAcrossMapsUtility.WorkGiverClassesNeedWrap.Add(typeInAnyAssembly);
        else
          ModCompat.LogIncompat(nameof (ColonyManagerRedux));
      }), new Span<string>());
    }
  }

  public static class GestaltEngine
  {
    public const string HarmonyId = "GestaltEngine.Mod";
  }

  public class PowerPoles : ModCompat.CompatBase<ModCompat.PowerPoles>
  {
    public static Type Building_LongDistancePower { get; private set; }

    public static Type Building_LongDistanceCabled { get; private set; }

    public static FastInvokeHandler IsLinkedTo { get; private set; }

    public static FastInvokeHandler TryRemoveLink { get; private set; }

    public static FastInvokeHandler GeneratePointsAsync { get; private set; }

    public static AccessTools.FieldRef<float> CableMaxDistance { get; private set; }

    public static AccessTools.FieldRef<Thing, IDictionary> connectionToPoints { get; private set; }

    static PowerPoles()
    {
      ModCompat.CompatBase<ModCompat.PowerPoles>.Initialize("co.uk.epicguru.rimforgepoles", (Action) (() =>
      {
        ModCompat.PowerPoles.Building_LongDistancePower = GenTypes.GetTypeInAnyAssembly("RimForge.Buildings.Building_LongDistancePower", "RimForge.Buildings");
        ModCompat.PowerPoles.Building_LongDistanceCabled = GenTypes.GetTypeInAnyAssembly("RimForge.Buildings.Building_LongDistanceCabled", "RimForge.Buildings");
        ModCompat.PowerPoles.IsLinkedTo = MethodInvoker.GetHandler(AccessTools.Method(ModCompat.PowerPoles.Building_LongDistancePower, nameof (IsLinkedTo), (Type[]) null, (Type[]) null), false);
        ModCompat.PowerPoles.TryRemoveLink = MethodInvoker.GetHandler(AccessTools.Method(ModCompat.PowerPoles.Building_LongDistancePower, nameof (TryRemoveLink), (Type[]) null, (Type[]) null), false);
        ModCompat.PowerPoles.GeneratePointsAsync = MethodInvoker.GetHandler(AccessTools.Method(ModCompat.PowerPoles.Building_LongDistanceCabled, nameof (GeneratePointsAsync), (Type[]) null, (Type[]) null), false);
        ModCompat.PowerPoles.CableMaxDistance = AccessTools.StaticFieldRefAccess<float>(AccessTools.Field("RimForge.PolesSettings.PolesModSettings:CableMaxDistance"));
        ModCompat.PowerPoles.connectionToPoints = AccessTools.FieldRefAccess<Thing, IDictionary>(AccessTools.Field(ModCompat.PowerPoles.Building_LongDistanceCabled, nameof (connectionToPoints)));
      }), new Span<string>());
    }
  }

  public class VehicleRaidFramework : ModCompat.CompatBase<ModCompat.VehicleRaidFramework>
  {
    public const string HarmonyId = "VRF.VehicleRaidFramework";

    public static Type CompVehicleHover { get; private set; }

    public static AccessTools.FieldRef<VehicleComp, int> State { get; private set; }

    static VehicleRaidFramework()
    {
      ModCompat.CompatBase<ModCompat.VehicleRaidFramework>.Initialize("gabrieel1482.raidvehicleframework", (Action) (() =>
      {
        ModCompat.VehicleRaidFramework.CompVehicleHover = GenTypes.GetTypeInAnyAssembly("VehicleRaid.CompVehicleHover", "VehicleRaid");
        ModCompat.VehicleRaidFramework.State = (AccessTools.FieldRef<VehicleComp, int>) AccessTools.FieldRefAccess<int>(ModCompat.VehicleRaidFramework.CompVehicleHover, nameof (State));
      }), new Span<string>());
    }
  }

  public class DefensiveNetwork : ModCompat.CompatBase<ModCompat.DefensiveNetwork>
  {
    public static FastInvokeHandler CountWatchersTargeting;

    static DefensiveNetwork()
    {
      ModCompat.CompatBase<ModCompat.DefensiveNetwork>.Initialize("wuhuansuiyue.defensivenetworkexpanded", (Action) (() => ModCompat.DefensiveNetwork.CountWatchersTargeting = MethodInvoker.GetHandler(AccessTools.Method(GenTypes.GetTypeInAnyAssembly("DNX.WatcherTargetingUtility", "DNX"), nameof (CountWatchersTargeting), (Type[]) null, (Type[]) null), false)), new Span<string>());
    }
  }

  public class ManipulatorBeamEmitter : ModCompat.CompatBase<ModCompat.ManipulatorBeamEmitter>
  {
    public static FastInvokeHandler TryFindBestStorageCellCore;
    public static FastInvokeHandler FillTransferQueue;
    public static AccessTools.FieldRef<object, Thing> thing;
    public static FastInvokeHandler Manipulator;
    public static FastInvokeHandler Pawn;

    static ManipulatorBeamEmitter()
    {
      ModCompat.CompatBase<ModCompat.ManipulatorBeamEmitter>.Initialize("natsuki.manipulatorbeam", (Action) (() =>
      {
        Type typeInAnyAssembly1 = GenTypes.GetTypeInAnyAssembly("ManipulatorBeam.WorkGiver_OperateBeamManipulator", "ManipulatorBeam");
        if ((object) typeInAnyAssembly1 != null)
          JobAcrossMapsUtility.WorkGiverClassesNeedWrap.Add(typeInAnyAssembly1);
        Type typeInAnyAssembly2 = GenTypes.GetTypeInAnyAssembly("ManipulatorBeam.BeamManipulatorUtility", "ManipulatorBeam");
        ModCompat.ManipulatorBeamEmitter.TryFindBestStorageCellCore = MethodInvoker.GetHandler(AccessTools.Method(typeInAnyAssembly2, nameof (TryFindBestStorageCellCore), (Type[]) null, (Type[]) null), true);
        ModCompat.ManipulatorBeamEmitter.FillTransferQueue = MethodInvoker.GetHandler(AccessTools.Method(typeInAnyAssembly2, nameof (FillTransferQueue), (Type[]) null, (Type[]) null), false);
        ModCompat.ManipulatorBeamEmitter.thing = AccessTools.FieldRefAccess<Thing>("ManipulatorBeam.BeamTransfer:thing");
        Type typeInAnyAssembly3 = GenTypes.GetTypeInAnyAssembly("ManipulatorBeam.IBeamOperator", "ManipulatorBeam");
        ModCompat.ManipulatorBeamEmitter.Manipulator = MethodInvoker.GetHandler(AccessTools.PropertyGetter(typeInAnyAssembly3, nameof (Manipulator)), false);
        ModCompat.ManipulatorBeamEmitter.Pawn = MethodInvoker.GetHandler(AccessTools.PropertyGetter(typeInAnyAssembly3, nameof (Pawn)), false);
      }), new Span<string>());
    }

    public static Thing OperatorThing(object op)
    {
      return (Thing) ModCompat.ManipulatorBeamEmitter.Pawn.Invoke(op, Array.Empty<object>()) ?? (Thing) ModCompat.ManipulatorBeamEmitter.Manipulator.Invoke(op, Array.Empty<object>());
    }
  }

  public class HaulersDream : ModCompat.CompatBase<ModCompat.HaulersDream>
  {
    static HaulersDream()
    {
      ModCompat.CompatBase<ModCompat.HaulersDream>.Initialize("giwaffed.HaulersDream", (Action) null, new Span<string>());
    }
  }

  public class AvoidFriendlyFire : ModCompat.CompatBase<ModCompat.AvoidFriendlyFire>
  {
    static AvoidFriendlyFire()
    {
      ModCompat.CompatBase<ModCompat.AvoidFriendlyFire>.Initialize("falconne.AFF", (Action) null, new Span<string>());
    }
  }

  public class DesignationsTooltip : ModCompat.CompatBase<ModCompat.DesignationsTooltip>
  {
    public const string HarmonyId = "com.cheatereater.designationstooltip";
  }
}
