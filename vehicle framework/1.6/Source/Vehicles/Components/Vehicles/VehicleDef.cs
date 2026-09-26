// Decompiled with JetBrains decompiler
// Type: Vehicles.VehicleDef
// Assembly: Vehicles, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 81BFDC99-D8AD-42E1-8470-2F73E7836B4A
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\Vehicles.dll

using JetBrains.Annotations;
using RimWorld;
using SmashTools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Vehicles.Compatibility;
using Vehicles.Rendering;
using Verse;

#nullable disable
namespace Vehicles;

[PublicAPI]
[VehicleSettingsClass]
public class VehicleDef : 
  ThingDef,
  IDefIndex<VehicleDef>,
  IMaterialCacheTarget,
  ITweakFields,
  IBlitTarget
{
  private static readonly int NullShaderErrorCode = "NullShaderVehicleDef".GetHashCode();
  [PostToSettings]
  public VehicleEnabled.For enabled = VehicleEnabled.For.Everyone;
  [PostToSettings(Label = "VF_Nameable", Translate = true, Tooltip = "VF_NameableTooltip", UISettingsType = UISettingsType.Checkbox)]
  public bool nameable = true;
  [DisableSetting]
  [PostToSettings(Label = "VF_CombatPower", Translate = true, Tooltip = "VF_CombatPowerTooltip", UISettingsType = UISettingsType.FloatBox)]
  [NumericBoxValues(MinValue = 0.0f, MaxValue = 3.40282347E+38f)]
  public float combatPower = 100f;
  public List<VehicleStatModifier> vehicleStats = new List<VehicleStatModifier>();
  [PostToSettings(Label = "VF_CanCaravan", Translate = true, Tooltip = "VF_CanCaravanTooltip", UISettingsType = UISettingsType.Checkbox)]
  public bool canCaravan = true;
  public VehicleCategory vehicleCategory;
  [LoadAlias("vehicleType")]
  public VehicleType type = VehicleType.Land;
  [PostToSettings(Label = "VF_NavigationType", Translate = true, Tooltip = "VF_NavigationTypeTooltip", UISettingsType = UISettingsType.SliderEnum)]
  public NavigationCategory navigationCategory = NavigationCategory.Opportunistic;
  public VehicleBuildDef buildDef;
  [TweakField(SubCategory = "GraphicData")]
  public GraphicDataRGB graphicData;
  [TweakField]
  [PostToSettings(Label = "VF_Properties", Translate = true, ParentHolder = true)]
  public VehicleProperties properties;
  public VehicleNPCProperties npcProperties;
  [TweakField]
  public VehicleDrawProperties drawProperties;
  [MayRequireAnyOf("ludeon.rimworld.odyssey,VanillaExpanded.VCEF")]
  public FishingProperties fishingProperties;
  public List<StatCache.EventLister> statEvents;
  public List<VehicleSoundEventEntry<VehicleEventDef>> soundOneShotsOnEvent = new List<VehicleSoundEventEntry<VehicleEventDef>>();
  public List<VehicleSustainerEventEntry<VehicleEventDef>> soundSustainersOnEvent;
  public SimpleDictionary<VehicleEventDef, List<DynamicDelegate<VehiclePawn>>> events;
  public List<System.Type> designatorTypes = new List<System.Type>();
  [NoTranslate]
  public string draftLabel;
  public SoundDef soundBuilt;
  public PawnKindDef kindDef;
  public List<VehicleComponentProperties> components;
  [Unsaved(false)]
  private readonly SelfOrderingList<CompProperties> cachedComps = new SelfOrderingList<CompProperties>();
  [Unsaved(false)]
  private Texture2D resolvedLoadCargoTexture;
  [Unsaved(false)]
  private Texture2D resolvedCancelCargoTexture;

  public VehiclePermissions MovementPermissions { get; private set; }

  public int DefIndex { get; set; }

  public int SizePadding { get; private set; }

  public MaterialPropertyBlock PropertyBlock { get; private set; }

  public VehicleFleshTypeDef BodyType => this.kindDef.RaceProps.FleshType as VehicleFleshTypeDef;

  public CompProperties_FueledTravel CompPropsFueledTravel { get; private set; }

  public CompProperties_VehicleLauncher CompPropsVehicleLauncher { get; private set; }

  public CompProperties_VehicleTurrets CompPropsVehicleTurrets { get; private set; }

  public CompProperties_UpgradeTree CompPropsUpgradeTree { get; private set; }

  public Texture2D LoadCargoIcon
  {
    get
    {
      if (!Object.op_Implicit((Object) this.resolvedLoadCargoTexture))
      {
        this.resolvedLoadCargoTexture = ContentFinder<Texture2D>.Get(this.drawProperties.loadCargoTexPath, false);
        if (!Object.op_Implicit((Object) this.resolvedLoadCargoTexture))
          this.resolvedLoadCargoTexture = VehicleTex.PackCargoIcon[(int) this.type];
        Trace.IsTrue(Object.op_Implicit((Object) this.resolvedLoadCargoTexture), "Unable to load LoadCargo icon.");
      }
      return this.resolvedLoadCargoTexture;
    }
  }

  public Texture2D CancelCargoIcon
  {
    get
    {
      if (!Object.op_Implicit((Object) this.resolvedCancelCargoTexture))
      {
        this.resolvedCancelCargoTexture = ContentFinder<Texture2D>.Get(this.drawProperties.cancelCargoTexPath, false);
        if (!Object.op_Implicit((Object) this.resolvedCancelCargoTexture))
          this.resolvedCancelCargoTexture = VehicleTex.CancelPackCargoIcon[(int) this.type];
        Trace.IsTrue(Object.op_Implicit((Object) this.resolvedCancelCargoTexture), "Unable to load CancelCargo icon.");
      }
      return this.resolvedCancelCargoTexture;
    }
  }

  public int MaterialCount => 4;

  public PatternDef PatternDef => PatternDefOf.Default;

  public string Name => ((Def) this).defName;

  string ITweakFields.Label => ((Def) this).defName;

  string ITweakFields.Category => ((Def) this).defName;

  public bool CanDisableEMPSetting
  {
    get
    {
      if (!GenList.NullOrEmpty<VehicleComponentProperties>((IList<VehicleComponentProperties>) this.components))
      {
        foreach (VehicleComponentProperties component in this.components)
        {
          if (component.empSeverity > VehicleEMPSeverity.None)
            return false;
        }
      }
      return true;
    }
  }

  public void OnFieldChanged()
  {
  }

  public virtual void ResolveReferences()
  {
    this.CacheCompProperties();
    if (this.CompPropsUpgradeTree != null)
      this.inspectorTabs?.Add(typeof (ITab_Vehicle_Upgrades));
    base.ResolveReferences();
    if (!GenList.NullOrEmpty<VehicleComponentProperties>((IList<VehicleComponentProperties>) this.components))
    {
      foreach (VehicleComponentProperties component in this.components)
        component.ResolveReferences(this);
    }
    if (this.designatorTypes == null)
      this.designatorTypes = new List<System.Type>();
    if (this.drawProperties == null)
      this.drawProperties = new VehicleDrawProperties();
    if (this.properties == null)
      this.properties = new VehicleProperties();
    this.properties.ResolveReferences(this);
    if (this.npcProperties == null)
      this.npcProperties = new VehicleNPCProperties();
    this.SizePadding = Mathf.Clamp(Mathf.CeilToInt((float) Mathf.Min(this.size.x, this.size.z) / 2f) - 1, 0, 100);
    if (GenDictionary.NullOrEmpty<string, PatternData>(VehicleMod.settings.vehicles.defaultGraphics))
      VehicleMod.settings.vehicles.defaultGraphics = new Dictionary<string, PatternData>();
    if (GenText.NullOrEmpty(this.draftLabel))
      this.draftLabel = TaggedString.op_Implicit(Translator.Translate("VF_draftLabel"));
    if (GenList.NullOrEmpty<CompProperties>((IList<CompProperties>) this.comps))
      return;
    this.cachedComps.AddRange((IEnumerable<CompProperties>) this.comps);
  }

  public virtual void PostLoad()
  {
    base.graphicData = (GraphicData) this.graphicData;
    LongEventHandler.ExecuteWhenFinished(new Action(this.VerifyGraphicData));
    base.PostLoad();
  }

  private void VerifyGraphicData()
  {
    this.PropertyBlock = new MaterialPropertyBlock();
    if (this.graphicData == null)
      return;
    GraphicDataRGB graphicData = this.graphicData;
    if (graphicData.shaderType == null)
      graphicData.shaderType = ShaderTypeDefOf.Cutout;
    if (!VehicleMod.settings.main.useCustomShaders)
      this.graphicData.shaderType = this.graphicData.shaderType.Shader.SupportsRGBMaskTex(true) ? ShaderTypeDefOf.CutoutComplex : this.graphicData.shaderType;
    if (this.graphicData.shaderType.Shader.SupportsRGBMaskTex())
    {
      RGBMaterialPool.CacheMaterialsFor((IMaterialCacheTarget) this);
      this.graphicData.Init((IMaterialCacheTarget) this);
      PatternData patternData = GenCollection.TryGetValue<string, PatternData>((IReadOnlyDictionary<string, PatternData>) VehicleMod.settings.vehicles.defaultGraphics, ((Def) this).defName, new PatternData(this.graphicData));
      patternData.ExposeDataPostDefDatabase();
      RGBMaterialPool.SetProperties((IMaterialCacheTarget) this, patternData, new Func<Rot8, Texture2D>(this.graphicData.Graphic.TexAt), new Func<Rot8, Texture2D>(this.graphicData.Graphic.MaskAt));
    }
    else
    {
      Graphic_Rgb graphic = this.graphicData.Graphic;
    }
  }

  public void RecacheMovementPermissions()
  {
    this.MovementPermissions = VehiclePermissions.Mobile | VehiclePermissions.Autonomous;
    if (Mathf.Approximately(this.GetStatValueAbstract(VehicleStatDefOf.MoveSpeed), 0.0f))
      this.MovementPermissions &= ~VehiclePermissions.Mobile;
    foreach (VehicleRole role in this.properties.roles)
    {
      if ((role.HandlingTypes & HandlingType.Movement) != HandlingType.None)
      {
        this.MovementPermissions &= ~VehiclePermissions.Autonomous;
        break;
      }
    }
  }

  public void PostDefDatabase()
  {
    this.properties.PostDefDatabase(this);
    this.drawProperties.PostDefDatabase(this);
    if (this.graphicData != null)
    {
      GraphicDataRGB graphicData = this.graphicData;
      if (graphicData.pattern == null)
        graphicData.pattern = PatternDefOf.Default;
    }
    this.RecacheMovementPermissions();
  }

  private void CacheCompProperties()
  {
    this.CompPropsFueledTravel = this.GetCompProperties<CompProperties_FueledTravel>();
    this.CompPropsVehicleLauncher = this.GetCompProperties<CompProperties_VehicleLauncher>();
    this.CompPropsVehicleTurrets = this.GetCompProperties<CompProperties_VehicleTurrets>();
    this.CompPropsUpgradeTree = this.GetCompProperties<CompProperties_UpgradeTree>();
  }

  protected virtual void ResolveIcon()
  {
    if (((BuildableDef) this).graphic == null || ((BuildableDef) this).graphic == BaseContent.BadGraphic)
      return;
    Material material = this.graphicData.Graphic.Shader.SupportsRGBMaskTex() ? RGBMaterialPool.Get((IMaterialCacheTarget) this, (Rot8) ((BuildableDef) this).defaultPlacingRot) : ((BuildableDef) this).graphic.MatAt(((BuildableDef) this).defaultPlacingRot, (Thing) null);
    ((BuildableDef) this).uiIcon = (Texture2D) material.mainTexture;
    ((BuildableDef) this).uiIconColor = material.color;
  }

  public virtual IEnumerable<string> ConfigErrors()
  {
    VehicleDef vehicleDef = this;
    // ISSUE: reference to a compiler-generated method
    IEnumerator<string> enumerator = vehicleDef.\u003C\u003En__0().GetEnumerator();
    while (enumerator.MoveNext())
    {
      string current = enumerator.Current;
      if (vehicleDef.Fillage != 2 || !(current == "fillPercent is 1.00 but is not edifice") && !(current == "gives full cover but is not a building."))
        yield return current;
    }
    enumerator = (IEnumerator<string>) null;
    if (vehicleDef.drawerType != 1)
      yield return $"{vehicleDef.drawerType} is not valid for vehicle rendering. <field>drawerType</type> must be DrawerType.RealtimeOnly";
    enumerator = vehicleDef.properties.ConfigErrors(vehicleDef).GetEnumerator();
    while (enumerator.MoveNext())
      yield return enumerator.Current;
    enumerator = (IEnumerator<string>) null;
    if (!GenList.NullOrEmpty<StatModifier>((IList<StatModifier>) ((BuildableDef) vehicleDef).statBases))
    {
      foreach (StatModifier statBase in ((BuildableDef) vehicleDef).statBases)
      {
        if (statBase.stat == StatDefOf.Mass)
          Log.Error("Vehicles must define Mass in vehicleStats and not statBases.");
        if (statBase.stat == StatDefOf.MoveSpeed)
          Log.Error("Vehicles must define MoveSpeed in vehicleStats and not statBases.");
      }
    }
    if (vehicleDef.graphicData == null)
      yield return "<field>graphicData</field> must be specified in order to properly render the vehicle.".ConvertRichText();
    if (GenList.NullOrEmpty<VehicleComponentProperties>((IList<VehicleComponentProperties>) vehicleDef.components))
      yield return "<field>components</field> must include at least 1 VehicleComponent".ConvertRichText();
    if (!GenList.NullOrEmpty<VehicleComponentProperties>((IList<VehicleComponentProperties>) vehicleDef.components))
    {
      if (vehicleDef.components.GroupBy<VehicleComponentProperties, string>((Func<VehicleComponentProperties, string>) (component => component.key)).Any<IGrouping<string, VehicleComponentProperties>>((Func<IGrouping<string, VehicleComponentProperties>, bool>) (group => group.Count<VehicleComponentProperties>() > 1)))
        yield return "<field>components</field> must not contain duplicate keys".ConvertRichText();
      foreach (VehicleComponentProperties component in vehicleDef.components)
      {
        enumerator = component.ConfigErrors().GetEnumerator();
        while (enumerator.MoveNext())
          yield return enumerator.Current;
        enumerator = (IEnumerator<string>) null;
      }
    }
  }

  public Vector2 ScaleDrawRatio(Vector2 size)
  {
    float num1 = size.x * this.uiIconScale;
    float num2 = size.y * this.uiIconScale;
    Vector2 drawSize = this.graphicData.drawSize;
    if ((double) num1 < (double) num2)
      num2 = num1 * (drawSize.y / drawSize.x);
    else
      num1 = num2 * (drawSize.x / drawSize.y);
    return new Vector2(num1, num2);
  }

  public Vector2 ScaleDrawRatio(GraphicData graphicData, Rot4 rot, Vector2 size, float iconScale = 1f)
  {
    Vector2 drawSize = graphicData.drawSize;
    if (((Rot4) ref rot).IsHorizontal)
    {
      ref float local1 = ref drawSize.x;
      ref float local2 = ref drawSize.y;
      float y = drawSize.y;
      float x = drawSize.x;
      local1 = y;
      double num = (double) x;
      local2 = (float) num;
    }
    Vector2 vector2 = Vector2.op_Division(drawSize, this.graphicData.drawSize);
    float num1 = size.x * this.uiIconScale * vector2.x * iconScale;
    float num2 = size.y * this.uiIconScale * vector2.y * iconScale;
    if ((double) num1 < (double) num2)
      num2 = num1 * (drawSize.y / drawSize.x);
    else
      num1 = num2 * (drawSize.x / drawSize.y);
    return new Vector2(num1, num2);
  }

  public VehicleRole GetRole(string key)
  {
    if (!GenList.NullOrEmpty<VehicleRole>((IList<VehicleRole>) this.properties.roles))
    {
      foreach (VehicleRole role in this.properties.roles)
      {
        if (role.key == key)
          return role;
      }
    }
    return (VehicleRole) null;
  }

  public VehicleRole CreateRole(string key)
  {
    VehicleRole role1 = this.GetRole(key);
    if (role1 != null)
      return new VehicleRole(role1);
    CompProperties_UpgradeTree compProperties = this.GetCompProperties<CompProperties_UpgradeTree>();
    if (compProperties != null)
    {
      foreach (UpgradeNode node in compProperties.def.nodes)
      {
        if (!GenList.NullOrEmpty<Upgrade>((IList<Upgrade>) node.upgrades))
        {
          foreach (Upgrade upgrade in node.upgrades)
          {
            if (upgrade is VehicleUpgrade vehicleUpgrade && !GenList.NullOrEmpty<VehicleUpgrade.RoleUpgrade>((IList<VehicleUpgrade.RoleUpgrade>) vehicleUpgrade.roles))
            {
              foreach (VehicleUpgrade.RoleUpgrade role2 in vehicleUpgrade.roles)
              {
                if (role2.key == key && GenText.NullOrEmpty(role2.editKey))
                  return VehicleUpgrade.RoleUpgrade.RoleFromUpgrade(role2);
              }
            }
          }
        }
      }
      Log.Error($"Unable to create role {key}. Matching VehicleRole not found in VehicleDef ({((Def) this).defName}) or UpgradeTreeDef ({compProperties.def.defName})");
      return (VehicleRole) null;
    }
    Log.Error($"Unable to create role {key}. Matching VehicleRole not found in VehicleDef ({((Def) this).defName}).");
    return (VehicleRole) null;
  }

  public virtual IEnumerable<VehicleStatDrawEntry> SpecialDisplayStats(VehiclePawn vehicle = null)
  {
    VehicleDef vehicleDef = this;
    foreach (VehicleStatDrawEntry specialDisplayStat in vehicleDef.buildDef.SpecialDisplayStats())
      yield return specialDisplayStat;
    VehiclePawn vehicle1 = vehicle;
    (int, int, string) valueTuple = vehicle1 != null ? vehicle1.GetTotalHealth() : vehicleDef.GetTotalHealth();
    StatCategoryDef vehicleBasicsImportant1 = VehicleStatCategoryDefOf.VehicleBasicsImportant;
    TaggedString taggedString1 = Translator.Translate("HitPointsBasic");
    string label1 = TaggedString.op_Implicit(((TaggedString) ref taggedString1).CapitalizeFirst());
    string valueString1 = $"{valueTuple.Item1} / {valueTuple.Item2}";
    string reportText1 = $"{Translator.Translate("Stat_HitPoints_Desc")}{Environment.NewLine}{Environment.NewLine}{valueTuple.Item3}";
    yield return new VehicleStatDrawEntry(vehicleBasicsImportant1, label1, valueString1, reportText1, 99998);
    StringBuilder stringBuilder = new StringBuilder();
    int num1 = 0;
    VehiclePawn vehiclePawn1 = vehicle;
    List<VehicleRole> vehicleRoleList = (vehiclePawn1 != null ? vehiclePawn1.handlers.Select<VehicleRoleHandler, VehicleRole>((Func<VehicleRoleHandler, VehicleRole>) (h => h.role)).ToList<VehicleRole>() : (List<VehicleRole>) null) ?? vehicleDef.properties.roles;
    if (!GenList.NullOrEmpty<VehicleRole>((IList<VehicleRole>) vehicleDef.properties.roles))
    {
      stringBuilder.AppendLine();
      foreach (VehicleRole vehicleRole in vehicleRoleList)
      {
        num1 += vehicleRole.Slots;
        stringBuilder.AppendLine($"x{vehicleRole.Slots}  {vehicleRole.label}");
      }
    }
    StatCategoryDef vehicleBasicsImportant2 = VehicleStatCategoryDefOf.VehicleBasicsImportant;
    TaggedString taggedString2 = Translator.Translate("VF_CrewCount");
    string label2 = TaggedString.op_Implicit(((TaggedString) ref taggedString2).CapitalizeFirst());
    string valueString2 = num1.ToString();
    string reportText2 = $"{Translator.Translate("VF_CrewCountDesc")}{Environment.NewLine}{stringBuilder}";
    yield return new VehicleStatDrawEntry(vehicleBasicsImportant2, label2, valueString2, reportText2, 99995);
    if (vehicleDef.CompPropsFueledTravel != null)
    {
      ThingDef fuelType = vehicleDef.CompPropsFueledTravel.fuelType;
      StatCategoryDef vehicleRefuelable1 = VehicleStatCategoryDefOf.VehicleRefuelable;
      TaggedString taggedString3 = Translator.Translate("VF_FuelType");
      string label3 = TaggedString.op_Implicit(((TaggedString) ref taggedString3).CapitalizeFirst());
      string valueString3 = TaggedString.op_Implicit(((Def) fuelType).LabelCap);
      string reportText3 = TaggedString.op_Implicit(Translator.Translate("VF_FuelTypeDesc"));
      yield return new VehicleStatDrawEntry(vehicleRefuelable1, label3, valueString3, reportText3, 4500);
      VehiclePawn vehiclePawn2 = vehicle;
      float num2 = vehiclePawn2 != null ? vehiclePawn2.CompFueledTravel.FuelEfficiency : vehicleDef.CompPropsFueledTravel.fuelConsumptionRate;
      StatCategoryDef vehicleRefuelable2 = VehicleStatCategoryDefOf.VehicleRefuelable;
      TaggedString taggedString4 = Translator.Translate("VF_FuelConsumptionRate");
      string label4 = TaggedString.op_Implicit(((TaggedString) ref taggedString4).CapitalizeFirst());
      string valueString4 = num2.ToString("0.##");
      string reportText4 = TaggedString.op_Implicit(Translator.Translate("VF_FuelConsumptionRateTooltip"));
      yield return new VehicleStatDrawEntry(vehicleRefuelable2, label4, valueString4, reportText4, 4501);
      VehiclePawn vehiclePawn3 = vehicle;
      float num3 = vehiclePawn3 != null ? vehiclePawn3.CompFueledTravel.FuelCapacity : (float) vehicleDef.CompPropsFueledTravel.fuelCapacity;
      StatCategoryDef vehicleRefuelable3 = VehicleStatCategoryDefOf.VehicleRefuelable;
      TaggedString taggedString5 = Translator.Translate("VF_FuelCapacity");
      string label5 = TaggedString.op_Implicit(((TaggedString) ref taggedString5).CapitalizeFirst());
      string stringByStyle = GenText.ToStringByStyle(num3, (ToStringStyle) 0, (ToStringNumberSense) 1);
      string reportText5 = TaggedString.op_Implicit(Translator.Translate("VF_FuelCapacityTooltip"));
      yield return new VehicleStatDrawEntry(vehicleRefuelable3, label5, stringByStyle, reportText5, 4502);
    }
    if (vehicleDef.CompPropsVehicleTurrets != null)
    {
      List<VehicleTurret> turrets = (List<VehicleTurret>) vehicle?.CompVehicleTurrets.Turrets ?? vehicleDef.CompPropsVehicleTurrets.turrets;
      if (!GenList.NullOrEmpty<VehicleTurret>((IList<VehicleTurret>) turrets))
      {
        for (int i = 0; i < turrets.Count; ++i)
        {
          foreach (VehicleStatDrawEntry specialDisplayStat in turrets[i].def.SpecialDisplayStats(VehicleStatCategoryDefOf.VehicleTurrets.displayOrder + i))
            yield return specialDisplayStat;
        }
      }
      turrets = (List<VehicleTurret>) null;
    }
    if (vehicleDef.CompPropsVehicleLauncher != null && (ModsConfig.OdysseyActive || Ext_Mods.HasActiveMod("kentington.saveourship2") || Ext_Mods.HasActiveMod("sindre0830.rimnauts2") || Ext_Mods.HasActiveMod("sindre0830.universum")))
    {
      VehiclePawn vehiclePawn4 = vehicle;
      bool flag = vehiclePawn4 != null ? vehiclePawn4.CompVehicleLauncher.SpaceFlight : vehicleDef.CompPropsVehicleLauncher.spaceFlight;
      StatCategoryDef vehicleRefuelable = VehicleStatCategoryDefOf.VehicleRefuelable;
      TaggedString taggedString6 = Translator.Translate("VF_SpaceFlight");
      string label6 = TaggedString.op_Implicit(((TaggedString) ref taggedString6).CapitalizeFirst());
      string stringYesNo = GenText.ToStringYesNo(flag);
      string reportText6 = TaggedString.op_Implicit(Translator.Translate("VF_SpaceFlightTooltip"));
      yield return new VehicleStatDrawEntry(vehicleRefuelable, label6, stringYesNo, reportText6, 1000);
    }
    if ((double) vehicleDef.fillPercent > 0.0)
      yield return new VehicleStatDrawEntry(StatCategoryDefOf.Building, TaggedString.op_Implicit(Translator.Translate("CoverEffectiveness")), GenText.ToStringPercent(CoverUtility.BaseBlockChance((ThingDef) vehicleDef)), TaggedString.op_Implicit(Translator.Translate("CoverEffectivenessExplanation")), 2000);
    yield return new VehicleStatDrawEntry(VehicleStatCategoryDefOf.VehicleBasics, TaggedString.op_Implicit(Translator.Translate("VF_Upgradeable")), GenText.ToStringYesNo(vehicleDef.CompPropsUpgradeTree != null), TaggedString.op_Implicit(Translator.Translate("VF_UpgradeableDesc")), 6001);
    if (VehicleMod.settings.main.useCustomShaders)
      yield return new VehicleStatDrawEntry(VehicleStatCategoryDefOf.VehicleBasics, TaggedString.op_Implicit(Translator.Translate("Stat_Building_Paintable")), GenText.ToStringYesNo(vehicleDef.graphicData.shaderType.Shader.SupportsRGBMaskTex()), TaggedString.op_Implicit(Translator.Translate("VF_PaintableDesc")), 6000);
    ModContentPack modContentPack = ((Def) vehicleDef).modContentPack;
    if (modContentPack != null && !modContentPack.IsCoreMod)
      yield return new VehicleStatDrawEntry(StatCategoryDefOf.Source, TaggedString.op_Implicit(Translator.Translate("Stat_Source_Label")), ((Def) vehicleDef).modContentPack.Name, $"{(((Def) vehicleDef).modContentPack.IsOfficialMod ? Translator.Translate("Stat_Source_OfficialExpansionReport") : Translator.Translate("Stat_Source_ModReport"))}: {((Def) vehicleDef).modContentPack.Name}", 90000);
  }

  public IEnumerable<VehicleStatDef> StatCategoryDefs()
  {
    VehicleDef vehicleDef = this;
    yield return VehicleStatDefOf.BodyIntegrity;
    List<VehicleStatModifier>.Enumerator enumerator1 = vehicleDef.vehicleStats.GetEnumerator();
    while (enumerator1.MoveNext())
    {
      VehicleStatModifier current = enumerator1.Current;
      if (current.statDef != VehicleStatDefOf.MoveSpeed || !vehicleDef.properties.roles.NotNullAndAny<VehicleRole>((Predicate<VehicleRole>) (role => (role.HandlingTypes & HandlingType.Movement) != 0)))
        yield return current.statDef;
    }
    enumerator1 = new List<VehicleStatModifier>.Enumerator();
    foreach (CompProperties comp in vehicleDef.comps)
    {
      if (comp is VehicleCompProperties vehicleCompProperties)
      {
        IEnumerator<VehicleStatDef> enumerator2 = vehicleCompProperties.StatCategoryDefs().GetEnumerator();
        while (enumerator2.MoveNext())
          yield return enumerator2.Current;
        enumerator2 = (IEnumerator<VehicleStatDef>) null;
      }
    }
  }

  public T GetSortedCompProperties<T>() where T : CompProperties
  {
    for (int index = 0; index < this.cachedComps.Count; ++index)
    {
      if (this.cachedComps[index] is T cachedComp)
      {
        this.cachedComps.Touch(index);
        return cachedComp;
      }
    }
    return default (T);
  }

  (int width, int height) IBlitTarget.TextureSize(in BlitRequest request)
  {
    Texture2D texture2D = (this.graphicData.Graphic as Graphic_Vehicle).TexAt(request.rot);
    return !Object.op_Inequality((Object) texture2D, (Object) null) ? (0, 0) : (((Texture) texture2D).width, ((Texture) texture2D).height);
  }

  IEnumerable<SmashTools.Rendering.RenderData> IBlitTarget.GetRenderData(
    Rect rect,
    BlitRequest request)
  {
    VehicleDef target = this;
    Vector2 vector2_1 = target.ScaleDrawRatio(((Rect) ref rect).size);
    bool flag = request.rot.IsHorizontal || request.rot.IsDiagonal;
    Vector2 vector2_2 = Vector2.op_Implicit(target.drawProperties.DisplayOffsetForRot((Rot4) request.rot));
    float num1 = vector2_1.x;
    float num2 = vector2_1.y;
    if (flag)
    {
      num1 = vector2_1.y;
      num2 = vector2_1.x;
    }
    float num3 = (float) (((double) ((Rect) ref rect).width - (double) num1) / 2.0 + (double) vector2_2.x * (double) ((Rect) ref rect).width);
    float num4 = (float) (((double) ((Rect) ref rect).height - (double) num2) / 2.0 + (double) vector2_2.y * (double) ((Rect) ref rect).height);
    Rect rect1;
    // ISSUE: explicit constructor call
    ((Rect) ref rect1).\u002Ector(((Rect) ref rect).x + num3, ((Rect) ref rect).y + num4, num1, num2);
    Graphic_Vehicle graphic = target.graphicData.Graphic as Graphic_Vehicle;
    Texture2D mainTex = graphic.TexAt(request.rot);
    Material material = (Material) null;
    Shader shader = graphic.Shader;
    if (!Object.op_Implicit((Object) shader) && RGBMaterialPool.TargetCached((IMaterialCacheTarget) target))
    {
      Log.ErrorOnce($"Null shader for {target} when requesting blit data. Defaulting to pattern shader.", VehicleDef.NullShaderErrorCode);
      shader = request.patternData.patternDef?.ShaderTypeDef?.Shader;
    }
    if (shader.SupportsRGBMaskTex())
    {
      RGBMaterialPool.SetProperties((IMaterialCacheTarget) target, request.patternData, new Func<Rot8, Texture2D>(((Graphic_Rgb) graphic).TexAt), new Func<Rot8, Texture2D>(((Graphic_Rgb) graphic).MaskAt));
      material = RGBMaterialPool.GetUi((IMaterialCacheTarget) target, (Rot4) request.rot);
    }
    // ISSUE: explicit non-virtual call
    yield return new SmashTools.Rendering.RenderData(rect1, (Texture) mainTex, material, __nonvirtual (target.PropertyBlock), 0.0f, 0.0f);
  }
}
