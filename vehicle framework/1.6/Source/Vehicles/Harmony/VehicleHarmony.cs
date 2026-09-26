// Decompiled with JetBrains decompiler
// Type: Vehicles.VehicleHarmony
// Assembly: Vehicles, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 81BFDC99-D8AD-42E1-8470-2F73E7836B4A
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\Vehicles.dll

using HarmonyLib;
using RimWorld;
using SmashTools;
using System;
using System.Collections.Generic;
using System.IO;
using UpdateLogTool;
using Verse;

#nullable disable
namespace Vehicles;

[StaticConstructorOnStartup]
public static class VehicleHarmony
{
  public const string VehiclesUniqueId = "SmashPhil.VehicleFramework";
  public const string VehiclesLabel = "Vehicle Framework";
  internal const string LogLabel = "[VehicleFramework]";
  internal static List<UpdateLog> updates = new List<UpdateLog>();

  internal static string VersionPath
  {
    get => Path.Combine(VehicleMod.metaData.RootDir.FullName, "Version.txt");
  }

  internal static string BuildDatePath
  {
    get => Path.Combine(VehicleMod.metaData.RootDir.FullName, "BuildDate.txt");
  }

  public static List<VehicleDef> AllMoveableVehicleDefs { get; internal set; }

  static VehicleHarmony()
  {
    UpdateHandler.LoadUpdateLog(VehicleMod.content);
    using (new DeepProfilerScope(nameof (VehicleHarmony)))
    {
      Log.Message("[VehicleFramework] version " + VehicleMod.metaData.ModVersion);
      new Action(VehicleHarmony.ResolveAllReferences).InvokeWithLogging();
      new Action(VehicleHarmony.PostDefDatabaseCalls).InvokeWithLogging();
      new Action(VehicleHarmony.RegisterDisplayStats).InvokeWithLogging();
      new Action(VehicleHarmony.RegisterKeyBindingDefs).InvokeWithLogging();
      new Action(VehicleHarmony.FillVehicleLordJobTypes).InvokeWithLogging();
      new Action(VehicleHarmony.ApplyAllDefModExtensions).InvokeWithLogging();
      new Action(PathingHelper.LoadTerrainTagCosts).InvokeWithLogging();
      new Action(PathingHelper.LoadTerrainDefaults).InvokeWithLogging();
      new Action(GridOwners.RecacheMoveableVehicleDefs).InvokeWithLogging();
      new Action(PathingHelper.CacheVehicleRegionEffecters).InvokeWithLogging();
      new Action(LoadedModManager.GetMod<VehicleMod>().InitializeTabs).InvokeWithLogging();
      new Action(((ModSettings) VehicleMod.settings).Write).InvokeWithLogging();
      new Action(VehicleHarmony.RegisterTweakFieldsInEditor).InvokeWithLogging();
      new Action(PatternDef.GenerateMaterials).InvokeWithLogging();
      DebugProperties.Init();
    }
  }

  private static void ResolveAllReferences()
  {
    foreach (Dictionary<SaveableField, SavedField<object>> dictionary in VehicleMod.settings.upgrades.upgradeSettings.Values)
    {
      foreach (SaveableField key in dictionary.Keys)
        key.ResolveReferences();
    }
  }

  private static void PostDefDatabaseCalls()
  {
    VehicleMod.settings.main.PostDefDatabase();
    VehicleMod.settings.vehicles.PostDefDatabase();
    VehicleMod.settings.upgrades.PostDefDatabase();
    VehicleMod.settings.debug.PostDefDatabase();
    foreach (VehicleDef vehicleDef in DefDatabase<VehicleDef>.AllDefsListForReading)
    {
      vehicleDef.PostDefDatabase();
      foreach (CompProperties comp in vehicleDef.comps)
      {
        if (comp is VehicleCompProperties vehicleCompProperties)
          vehicleCompProperties.PostDefDatabase();
      }
    }
    foreach (VehicleTurretDef vehicleTurretDef in DefDatabase<VehicleTurretDef>.AllDefsListForReading)
      vehicleTurretDef.PostDefDatabase();
  }

  private static void RegisterDisplayStats()
  {
    VehicleInfoCard.RegisterStatDef(StatDefOf.Flammability);
  }

  private static void RegisterKeyBindingDefs()
  {
    // ISSUE: reference to a compiler-generated field
    // ISSUE: reference to a compiler-generated field
    MainMenuKeyBindHandler.RegisterKeyBind(KeyBindingDefOf_Vehicles.VF_RestartGame, VehicleHarmony.\u003C\u003EO.\u003C0\u003E__Restart ?? (VehicleHarmony.\u003C\u003EO.\u003C0\u003E__Restart = new Action(GenCommandLine.Restart)));
    MainMenuKeyBindHandler.RegisterKeyBind(KeyBindingDefOf_Vehicles.VF_QuickStartMenu, (Action) (() => { }));
    MainMenuKeyBindHandler.RegisterKeyBind(KeyBindingDefOf_Vehicles.VF_DebugSettings, (Action) (() => VehiclesModSettings.OpenWithContext()));
  }

  private static void FillVehicleLordJobTypes()
  {
    VehicleIncidentSwapper.RegisterLordType(typeof (LordJob_ArmoredAssault));
  }

  public static void ClearModConfig() => Utilities.DeleteConfig((Mod) VehicleMod.mod);

  private static void ApplyAllDefModExtensions()
  {
    PathingHelper.LoadDefModExtensionCosts<ThingDef>((Func<VehicleDef, Dictionary<ThingDef, int>>) (vehicleDef => (Dictionary<ThingDef, int>) vehicleDef.properties.customThingCosts));
    PathingHelper.LoadDefModExtensionCosts<TerrainDef>((Func<VehicleDef, Dictionary<TerrainDef, int>>) (vehicleDef => (Dictionary<TerrainDef, int>) vehicleDef.properties.customTerrainCosts));
    PathingHelper.LoadDefModExtensionCosts<BiomeDef>((Func<VehicleDef, Dictionary<BiomeDef, float>>) (vehicleDef => (Dictionary<BiomeDef, float>) vehicleDef.properties.customBiomeCosts));
    PathingHelper.LoadDefModExtensionCosts<RoadDef>((Func<VehicleDef, Dictionary<RoadDef, float>>) (vehicleDef => (Dictionary<RoadDef, float>) vehicleDef.properties.customRoadCosts));
    PathingHelper.LoadDefModExtensionCosts<RiverDef>((Func<VehicleDef, Dictionary<RiverDef, float>>) (vehicleDef => (Dictionary<RiverDef, float>) vehicleDef.properties.customRiverCosts));
  }

  private static void RegisterTweakFieldsInEditor()
  {
    EditWindow_TweakFields.RegisterField(AccessTools.Field(typeof (Graphic), "data"), string.Empty, string.Empty, UISettingsType.None);
    EditWindow_TweakFields.RegisterField(AccessTools.Field(typeof (GraphicData), "drawOffset"), string.Empty, string.Empty, UISettingsType.FloatBox);
    EditWindow_TweakFields.RegisterField(AccessTools.Field(typeof (GraphicData), "drawOffsetNorth"), string.Empty, string.Empty, UISettingsType.FloatBox);
    EditWindow_TweakFields.RegisterField(AccessTools.Field(typeof (GraphicData), "drawOffsetEast"), string.Empty, string.Empty, UISettingsType.FloatBox);
    EditWindow_TweakFields.RegisterField(AccessTools.Field(typeof (GraphicData), "drawOffsetSouth"), string.Empty, string.Empty, UISettingsType.FloatBox);
    EditWindow_TweakFields.RegisterField(AccessTools.Field(typeof (GraphicData), "drawOffsetWest"), string.Empty, string.Empty, UISettingsType.FloatBox);
    EditWindow_TweakFields.RegisterField(AccessTools.Field(typeof (GraphicDataLayered), "drawOffset"), string.Empty, string.Empty, UISettingsType.FloatBox);
    EditWindow_TweakFields.RegisterField(AccessTools.Field(typeof (GraphicDataLayered), "drawOffsetNorth"), string.Empty, string.Empty, UISettingsType.FloatBox);
    EditWindow_TweakFields.RegisterField(AccessTools.Field(typeof (GraphicDataLayered), "drawOffsetEast"), string.Empty, string.Empty, UISettingsType.FloatBox);
    EditWindow_TweakFields.RegisterField(AccessTools.Field(typeof (GraphicDataLayered), "drawOffsetSouth"), string.Empty, string.Empty, UISettingsType.FloatBox);
    EditWindow_TweakFields.RegisterField(AccessTools.Field(typeof (GraphicDataLayered), "drawOffsetWest"), string.Empty, string.Empty, UISettingsType.FloatBox);
    EditWindow_TweakFields.RegisterField(AccessTools.Field(typeof (GraphicDataRGB), "drawOffset"), string.Empty, string.Empty, UISettingsType.FloatBox);
    EditWindow_TweakFields.RegisterField(AccessTools.Field(typeof (GraphicDataRGB), "drawOffsetNorth"), string.Empty, string.Empty, UISettingsType.FloatBox);
    EditWindow_TweakFields.RegisterField(AccessTools.Field(typeof (GraphicDataRGB), "drawOffsetEast"), string.Empty, string.Empty, UISettingsType.FloatBox);
    EditWindow_TweakFields.RegisterField(AccessTools.Field(typeof (GraphicDataRGB), "drawOffsetSouth"), string.Empty, string.Empty, UISettingsType.FloatBox);
    EditWindow_TweakFields.RegisterField(AccessTools.Field(typeof (GraphicDataRGB), "drawOffsetWest"), string.Empty, string.Empty, UISettingsType.FloatBox);
  }
}
