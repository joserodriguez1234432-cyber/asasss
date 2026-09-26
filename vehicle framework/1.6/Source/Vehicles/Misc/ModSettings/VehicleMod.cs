// Decompiled with JetBrains decompiler
// Type: Vehicles.VehicleMod
// Assembly: Vehicles, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 81BFDC99-D8AD-42E1-8470-2F73E7836B4A
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\Vehicles.dll

using JetBrains.Annotations;
using RimWorld;
using SmashTools;
using SmashTools.Performance;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using Vehicles.Config;
using Verse;
using Verse.Sound;

#nullable disable
namespace Vehicles;

[PublicAPI]
[StaticConstructorOnStartup]
public class VehicleMod : Mod
{
  public const float ResetImageSize = 22f;
  internal static readonly ConcurrentDictionary<System.Type, List<FieldInfo>> CachedFields = new ConcurrentDictionary<System.Type, List<FieldInfo>>();
  internal static readonly HashSet<string> SettingsDisabledFor = new HashSet<string>();
  public static VehiclesModSettings settings;
  public static VehicleMod mod;
  public static ModMetaData metaData;
  public static ModContentPack content;
  internal static VehicleDef selectedDef;
  private static SettingsSection currentSection;
  internal string currentKey;
  internal static UpgradeNode selectedNode;
  internal static List<PatternDef> selectedPatterns = new List<PatternDef>();
  internal static CompProperties_UpgradeTree selectedDefUpgradeComp;
  private static List<TabRecord> tabs = new List<TabRecord>();
  internal static List<FieldInfo> vehicleDefFields = new List<FieldInfo>();
  private static Dictionary<System.Type, List<FieldInfo>> vehicleCompFields = new Dictionary<System.Type, List<FieldInfo>>();
  internal readonly FeatureFlags features;

  public VehicleMod(ModContentPack content)
    : base(content)
  {
    VehicleMod.mod = this;
    VehicleMod.settings = this.GetSettings<VehiclesModSettings>();
    VehicleMod.InitializeSections();
    VehiclesModSettings settings = VehicleMod.settings;
    if (settings.colorStorage == null)
      settings.colorStorage = new ColorStorage();
    if (VehicleMod.selectedPatterns == null)
      VehicleMod.selectedPatterns = new List<PatternDef>();
    VehicleMod.CurrentSection = (SettingsSection) VehicleMod.settings.main;
    VehicleMod.content = VehicleMod.mod.Content;
    VehicleMod.metaData = content.ModMetaData;
    this.features = FeatureFlags.InitDefault();
    // ISSUE: reference to a compiler-generated field
    // ISSUE: reference to a compiler-generated field
    GameEvent.OnNewGame += VehicleMod.\u003C\u003EO.\u003C0\u003E__ResetDesignatorStatuses ?? (VehicleMod.\u003C\u003EO.\u003C0\u003E__ResetDesignatorStatuses = new Action(GizmoHelper.ResetDesignatorStatuses));
    // ISSUE: reference to a compiler-generated field
    // ISSUE: reference to a compiler-generated field
    GameEvent.OnLoadGame += VehicleMod.\u003C\u003EO.\u003C0\u003E__ResetDesignatorStatuses ?? (VehicleMod.\u003C\u003EO.\u003C0\u003E__ResetDesignatorStatuses = new Action(GizmoHelper.ResetDesignatorStatuses));
    // ISSUE: reference to a compiler-generated field
    // ISSUE: reference to a compiler-generated field
    GameEvent.OnGenerateImpliedDefs += VehicleMod.\u003C\u003EO.\u003C1\u003E__ImpliedDefGeneratorVehicles ?? (VehicleMod.\u003C\u003EO.\u003C1\u003E__ImpliedDefGeneratorVehicles = new Action<bool>(VehicleMod.ImpliedDefGeneratorVehicles));
  }

  public static bool ModifiableSettings => VehicleMod.settings.main.modifiableSettings;

  public static float FishingSkillValue => VehicleMod.settings.main.fishingSkillIncrease / 100f;

  public static SettingsSection CurrentSection
  {
    get => VehicleMod.currentSection;
    set
    {
      if (VehicleMod.currentSection == value)
        return;
      VehicleMod.currentSection?.OnClose();
      VehicleMod.currentSection = value;
      VehicleMod.currentSection?.OnOpen();
    }
  }

  public static Dictionary<System.Type, List<FieldInfo>> VehicleCompFields
  {
    get
    {
      if (GenDictionary.NullOrEmpty<System.Type, List<FieldInfo>>(VehicleMod.vehicleCompFields))
      {
        VehicleMod.ResetSelectedCachedTypes();
        VehicleMod.vehicleDefFields = GenCollection.TryGetValue<System.Type, List<FieldInfo>>((IReadOnlyDictionary<System.Type, List<FieldInfo>>) VehicleMod.vehicleCompFields, typeof (VehicleDef), new List<FieldInfo>());
        VehicleMod.vehicleCompFields.Remove(typeof (VehicleDef));
        PostToSettingsAttribute settingsAttribute;
        GenCollection.RemoveAll<System.Type, List<FieldInfo>>(VehicleMod.vehicleCompFields, (Predicate<KeyValuePair<System.Type, List<FieldInfo>>>) (d => GenList.NullOrEmpty<FieldInfo>((IList<FieldInfo>) d.Value) || d.Value.All<FieldInfo>((Func<FieldInfo, bool>) (f => GenAttribute.TryGetAttribute<PostToSettingsAttribute>((MemberInfo) f, ref settingsAttribute) && settingsAttribute.UISettingsType == UISettingsType.None))));
        VehicleMod.vehicleCompFields = VehicleMod.vehicleCompFields.OrderByDescending<KeyValuePair<System.Type, List<FieldInfo>>, bool>((Func<KeyValuePair<System.Type, List<FieldInfo>>, bool>) (d => d.Key == typeof (List<VehicleStatModifier>))).ThenByDescending<KeyValuePair<System.Type, List<FieldInfo>>, bool>((Func<KeyValuePair<System.Type, List<FieldInfo>>, bool>) (d => GenTypes.SameOrSubclassOf(d.Key, typeof (VehicleProperties)))).ThenByDescending<KeyValuePair<System.Type, List<FieldInfo>>, bool>((Func<KeyValuePair<System.Type, List<FieldInfo>>, bool>) (d => GenTypes.SameOrSubclassOf(d.Key, typeof (VehicleJobLimitations)))).ThenByDescending<KeyValuePair<System.Type, List<FieldInfo>>, bool>((Func<KeyValuePair<System.Type, List<FieldInfo>>, bool>) (d => d.Key.IsAssignableFrom(typeof (CompProperties)))).ThenByDescending<KeyValuePair<System.Type, List<FieldInfo>>, bool>((Func<KeyValuePair<System.Type, List<FieldInfo>>, bool>) (d => d.Key.IsClass)).ThenByDescending<KeyValuePair<System.Type, List<FieldInfo>>, bool>((Func<KeyValuePair<System.Type, List<FieldInfo>>, bool>) (d => d.Key.IsValueType && !d.Key.IsPrimitive && !d.Key.IsEnum)).ToDictionary<KeyValuePair<System.Type, List<FieldInfo>>, System.Type, List<FieldInfo>>((Func<KeyValuePair<System.Type, List<FieldInfo>>, System.Type>) (d => d.Key), (Func<KeyValuePair<System.Type, List<FieldInfo>>, List<FieldInfo>>) (d => d.Value));
      }
      return VehicleMod.vehicleCompFields;
    }
  }

  public static void SelectVehicle(VehicleDef vehicleDef)
  {
    VehicleMod.selectedDef = vehicleDef;
    VehicleMod.ClearSelectedDefCache();
    VehicleMod.selectedPatterns = DefDatabase<PatternDef>.AllDefsListForReading.Where<PatternDef>((Func<PatternDef, bool>) (d => d.ValidFor(VehicleMod.selectedDef))).ToList<PatternDef>();
    VehicleMod.selectedDefUpgradeComp = vehicleDef.GetSortedCompProperties<CompProperties_UpgradeTree>();
    VehicleMod.CurrentSection.VehicleSelected();
  }

  public static void DeselectVehicle()
  {
    VehicleMod.selectedDef = (VehicleDef) null;
    VehicleMod.selectedPatterns.Clear();
    VehicleMod.selectedDefUpgradeComp = (CompProperties_UpgradeTree) null;
    VehicleMod.selectedNode = (UpgradeNode) null;
  }

  private static void InitializeSections()
  {
    VehiclesModSettings settings1 = VehicleMod.settings;
    if (settings1.main == null)
      settings1.main = new SectionMain();
    VehicleMod.settings.main.Initialize();
    VehiclesModSettings settings2 = VehicleMod.settings;
    if (settings2.vehicles == null)
      settings2.vehicles = new SectionVehicles();
    VehicleMod.settings.vehicles.Initialize();
    VehiclesModSettings settings3 = VehicleMod.settings;
    if (settings3.upgrades == null)
      settings3.upgrades = new SectionUpgrades();
    VehicleMod.settings.upgrades.Initialize();
    VehiclesModSettings settings4 = VehicleMod.settings;
    if (settings4.debug == null)
      settings4.debug = new SectionDebug();
    VehicleMod.settings.debug.Initialize();
  }

  private static void ClearSelectedDefCache()
  {
    VehicleMod.vehicleCompFields.Clear();
    VehicleMod.vehicleDefFields.Clear();
  }

  private static void ResetSelectedCachedTypes()
  {
    if (VehicleMod.selectedDef == null)
      return;
    foreach (FieldInfo postSettingsField in ((object) VehicleMod.selectedDef).GetType().GetPostSettingsFields())
      VehicleMod.IterateTypeFields(typeof (VehicleDef), postSettingsField);
    foreach (CompProperties comp in VehicleMod.selectedDef.comps)
    {
      foreach (FieldInfo postSettingsField in comp.GetType().GetPostSettingsFields())
        VehicleMod.IterateTypeFields(comp.GetType(), postSettingsField);
    }
  }

  private static void IterateTypeFields(System.Type containingType, FieldInfo field)
  {
    PostToSettingsAttribute settingsAttribute;
    if (!GenAttribute.TryGetAttribute<PostToSettingsAttribute>((MemberInfo) field, ref settingsAttribute))
      return;
    if (settingsAttribute.ParentHolder)
    {
      foreach (FieldInfo postSettingsField in field.FieldType.GetPostSettingsFields())
        VehicleMod.IterateTypeFields(field.FieldType, postSettingsField);
    }
    else
    {
      if (!VehicleMod.vehicleCompFields.ContainsKey(containingType))
        VehicleMod.vehicleCompFields.Add(containingType, new List<FieldInfo>());
      VehicleMod.vehicleCompFields[containingType].Add(field);
    }
  }

  internal static void PopulateCachedFields()
  {
    ProfilerBlock profilerBlock;
    // ISSUE: explicit constructor call
    ((ProfilerBlock) ref profilerBlock).\u002Ector("Cache Settings Types");
    try
    {
      // ISSUE: reference to a compiler-generated field
      // ISSUE: reference to a compiler-generated field
      QuickIter.EnumerateAllModTypes(VehicleMod.\u003C\u003EO.\u003C2\u003E__CacheForType ?? (VehicleMod.\u003C\u003EO.\u003C2\u003E__CacheForType = new QuickIter.TypeProcessor(VehicleMod.CacheForType)));
    }
    catch (Exception ex)
    {
      Log.Error($"Exception thrown populating field cache for mod settings. Disable modifiable settings...\n{ex}");
      VehicleMod.settings.main.modifiableSettings = false;
      VehicleMod.CachedFields?.Clear();
    }
    finally
    {
      profilerBlock.Dispose();
    }
  }

  private static void CacheForType(System.Type type)
  {
    if (!GenAttribute.HasAttribute<VehicleSettingsClassAttribute>((MemberInfo) type))
      return;
    List<FieldInfo> list = type.GetPostSettingsFields().ToList<FieldInfo>();
    if (GenList.NullOrEmpty<FieldInfo>((IList<FieldInfo>) list))
      return;
    VehicleMod.CachedFields[type] = list;
  }

  public void InitializeTabs()
  {
    VehicleMod.tabs = new List<TabRecord>(1)
    {
      new TabRecord(TaggedString.op_Implicit(Translator.Translate("VF_MainSettings")), (Action) (() => VehicleMod.CurrentSection = (SettingsSection) VehicleMod.settings.main), (Func<bool>) (() => VehicleMod.CurrentSection == VehicleMod.settings.main))
    };
    if (VehicleMod.ModifiableSettings)
      VehicleMod.tabs.Add(new TabRecord(TaggedString.op_Implicit(Translator.Translate("VF_Vehicles")), (Action) (() =>
      {
        VehicleMod.CurrentSection = (SettingsSection) VehicleMod.settings.vehicles;
        List<VehicleDef> vehicleDefs = SectionDrawer.VehicleDefs;
      }), (Func<bool>) (() => VehicleMod.CurrentSection == VehicleMod.settings.vehicles)));
    VehicleMod.tabs.Add(new TabRecord(TaggedString.op_Implicit(Translator.Translate("VF_DevMode")), (Action) (() => VehicleMod.CurrentSection = (SettingsSection) VehicleMod.settings.debug), (Func<bool>) (() => VehicleMod.CurrentSection == VehicleMod.settings.debug)));
  }

  public virtual void DoSettingsWindowContents(Rect inRect)
  {
    base.DoSettingsWindowContents(inRect);
    Rect rect1 = GenUI.ContractedBy(inRect, 10f);
    ref Rect local1 = ref rect1;
    ((Rect) ref local1).y = ((Rect) ref local1).y + 20f;
    ref Rect local2 = ref rect1;
    ((Rect) ref local2).height = ((Rect) ref local2).height - 20f;
    Widgets.DrawMenuSection(rect1);
    TabDrawer.DrawTabs<TabRecord>(rect1, VehicleMod.tabs, 200f);
    VehicleMod.CurrentSection.OnGUI(rect1);
    Rect rect2;
    // ISSUE: explicit constructor call
    ((Rect) ref rect2).\u002Ector(((Rect) ref rect1).width - 27f, ((Rect) ref rect1).y + 15f, 22f, 22f);
    if (!Widgets.ButtonImage(VehicleMod.CurrentSection.ButtonRect(rect2), VehicleTex.ResetPage, true, (string) null))
      return;
    Find.WindowStack.Add((Window) new FloatMenu(VehicleMod.CurrentSection.ResetOptions.ToList<FloatMenuOption>())
    {
      vanishIfMouseDistant = true
    });
  }

  public virtual string SettingsCategory()
  {
    return TaggedString.op_Implicit(Translator.Translate("VehicleFramework"));
  }

  public static void ResetAllSettings()
  {
    // ISSUE: reference to a compiler-generated field
    // ISSUE: reference to a compiler-generated field
    Find.WindowStack.Add((Window) Dialog_MessageBox.CreateConfirmation(Translator.Translate("VF_DevMode_ResetAllConfirmation"), VehicleMod.\u003C\u003EO.\u003C3\u003E__ResetAllSettingsConfirmed ?? (VehicleMod.\u003C\u003EO.\u003C3\u003E__ResetAllSettingsConfirmed = new Action(VehicleMod.ResetAllSettingsConfirmed)), false, (string) null, (WindowLayer) 1));
  }

  private static void ResetAllSettingsConfirmed()
  {
    SoundStarter.PlayOneShotOnCamera(SoundDefOf.Click, (Map) null);
    VehicleMod.CachedFields.Clear();
    VehicleMod.PopulateCachedFields();
    VehicleMod.settings.main.ResetSettings();
    VehicleMod.settings.vehicles.ResetSettings();
    VehicleMod.settings.upgrades.ResetSettings();
    VehicleMod.settings.debug.ResetSettings();
    if (Current.ProgramState != 2)
      return;
    foreach (Map map in Find.Maps)
      map.GetCachedMapComponent<VehicleReservationManager>().ReleaseAllClaims();
  }

  public virtual void WriteSettings()
  {
    base.WriteSettings();
    VehicleMod.selectedNode = (UpgradeNode) null;
    Find.WindowStack.Windows.FirstOrDefault<Window>((Func<Window, bool>) (w => w is Dialog_NodeSettings))?.Close(true);
  }

  [PublicAPI]
  public static void GenerateImpliedDefs<T, D>(bool hotReload)
    where T : IVehicleDefGenerator<D>, new()
    where D : Def, new()
  {
    T obj = new T();
    foreach (VehicleDef vehicleDef in DefDatabase<VehicleDef>.AllDefsListForReading)
    {
      D impliedDef;
      if (obj.TryGenerateImpliedDef(vehicleDef, out impliedDef, hotReload))
        DefGenerator.AddImpliedDef<D>(impliedDef, hotReload);
    }
  }

  public static bool GenerateImpliedDefs(VehicleDef vehicleDef, bool hotReload)
  {
    bool impliedDefs = true & VehicleMod.TryGenerateImpliedDef<GeneratorVehiclePawnKindDef, PawnKindDef>(vehicleDef, false);
    VehicleMod.TryGenerateImpliedDef<GeneratorVehicleBuildDef, VehicleBuildDef>(vehicleDef, false);
    if (vehicleDef.GetCompProperties<CompProperties_VehicleLauncher>() != null)
      impliedDefs = impliedDefs & VehicleMod.TryGenerateImpliedDef<GeneratorVehicleSkyfallerLeaving, ThingDef>(vehicleDef, false) & VehicleMod.TryGenerateImpliedDef<GeneratorVehicleSkyfallerIncoming, ThingDef>(vehicleDef, false) & VehicleMod.TryGenerateImpliedDef<GeneratorVehicleSkyfallerCrashing, ThingDef>(vehicleDef, false);
    return impliedDefs;
  }

  public static bool TryGenerateImpliedDef<T, D>(VehicleDef vehicleDef, bool hotReload)
    where T : IVehicleDefGenerator<D>, new()
    where D : Def, new()
  {
    return new T().TryGenerateImpliedDef(vehicleDef, out D _, hotReload);
  }

  private static void ImpliedDefGeneratorVehicles(bool hotReload)
  {
    VehicleMod.GenerateImpliedDefs<GeneratorVehiclePawnKindDef, PawnKindDef>(hotReload);
    VehicleMod.GenerateImpliedDefs<GeneratorVehicleBuildDef, VehicleBuildDef>(hotReload);
    VehicleMod.GenerateImpliedDefs<GeneratorVehicleSkyfallerLeaving, ThingDef>(hotReload);
    VehicleMod.GenerateImpliedDefs<GeneratorVehicleSkyfallerIncoming, ThingDef>(hotReload);
    VehicleMod.GenerateImpliedDefs<GeneratorVehicleSkyfallerCrashing, ThingDef>(hotReload);
  }
}
