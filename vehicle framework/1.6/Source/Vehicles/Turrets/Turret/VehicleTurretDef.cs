// Decompiled with JetBrains decompiler
// Type: Vehicles.VehicleTurretDef
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
using Verse;
using Verse.AI;

#nullable disable
namespace Vehicles;

[PublicAPI]
public class VehicleTurretDef : Def, ITweakFields
{
  public TurretType turretType;
  public List<AnimationProperties> motes;
  [CanBeNull]
  public ThingFilter ammunition;
  [TweakField(SettingsType = UISettingsType.IntegerBox)]
  public int magazineCapacity = 1;
  [TweakField(SettingsType = UISettingsType.FloatBox)]
  public float chargePerAmmoCount = 1f;
  public bool genericAmmo;
  public TurretCooldownProperties cooldown;
  [TweakField(SubCategory = "Turret Recoil")]
  public RecoilProperties recoil;
  [TweakField(SubCategory = "Vehicle Recoil")]
  public RecoilProperties vehicleRecoil;
  [TweakField]
  public GraphicDataRGB graphicData;
  [TweakField(SubCategory = "Layered Graphics")]
  public List<VehicleTurretRenderData> graphics;
  public string gizmoDescription;
  [TweakField(SettingsType = UISettingsType.FloatBox)]
  public float gizmoIconScale = 1f;
  public string gizmoIconTexPath;
  public bool matchParentColor = true;
  [TweakField(SubCategory = "Fire Modes")]
  public List<FireMode> fireModes = new List<FireMode>();
  [TweakField(SettingsType = UISettingsType.Checkbox)]
  public bool autoSnapTargeting;
  [TweakField(SettingsType = UISettingsType.FloatBox)]
  public float rotationSpeed = 1f;
  [TweakField(SettingsType = UISettingsType.FloatBox)]
  public float rotationDelta;
  [TweakField(SettingsType = UISettingsType.FloatBox)]
  [NumericBoxValues(MinValue = 0.0f, MaxValue = 9999f)]
  public float maxRange;
  [TweakField(SettingsType = UISettingsType.FloatBox)]
  [NumericBoxValues(MinValue = 0.0f, MaxValue = 9999f)]
  public float minRange;
  [TweakField(SettingsType = UISettingsType.FloatBox)]
  public float reloadTimer = 5f;
  public LinearCurve reloadTimerMultiplierPerCrewCount;
  [TweakField(SettingsType = UISettingsType.FloatBox)]
  public float warmUpTimer = 3f;
  [TweakField(SettingsType = UISettingsType.FloatBox)]
  public float autoRefuelProportion = 2f;
  [TweakField(SettingsType = UISettingsType.Checkbox)]
  public bool empDisables;
  public SoundDef shotSound;
  public SoundDef reloadSound;
  public TargetScanFlags targetScanFlags;
  public ThingDef projectile;
  public CustomHitFlags attachProjectileFlag;
  [TweakField(SettingsType = UISettingsType.FloatBox)]
  public float projectileOffset;
  [TweakField(SettingsType = UISettingsType.FloatBox)]
  public float projectileSpeed = -1f;
  public List<float> projectileShifting = new List<float>();
  public System.Type restrictionType;

  string ITweakFields.Label => nameof (VehicleTurretDef);

  string ITweakFields.Category => string.Empty;

  public virtual IEnumerable<VehicleStatDrawEntry> SpecialDisplayStats(int displayOrder)
  {
    VehicleTurretDef vehicleTurretDef = this;
    yield return new VehicleStatDrawEntry(TaggedString.op_Implicit(vehicleTurretDef.LabelCap), displayOrder, TaggedString.op_Implicit(Translator.Translate("Description")), string.Empty, vehicleTurretDef.description, 99999, hyperlinks: Dialog_InfoCard.DefsToHyperlinks((IEnumerable<DefHyperlink>) vehicleTurretDef.descriptionHyperlinks));
    yield return new VehicleStatDrawEntry(TaggedString.op_Implicit(vehicleTurretDef.LabelCap), displayOrder, TaggedString.op_Implicit(Translator.Translate("VF_Rotatable")), GenText.ToStringYesNo(vehicleTurretDef.turretType == TurretType.Rotatable), TaggedString.op_Implicit(Translator.Translate("VF_RotatableTooltip")), 9000);
    string valueString1 = vehicleTurretDef.magazineCapacity <= 0 ? "∞" : vehicleTurretDef.magazineCapacity.ToString();
    yield return new VehicleStatDrawEntry(TaggedString.op_Implicit(vehicleTurretDef.LabelCap), displayOrder, TaggedString.op_Implicit(Translator.Translate("VF_MagazineCapacity")), valueString1, TaggedString.op_Implicit(Translator.Translate("VF_MagazineCapacityTooltip")), 6000);
    if ((double) vehicleTurretDef.minRange > 0.0)
      yield return new VehicleStatDrawEntry(TaggedString.op_Implicit(vehicleTurretDef.LabelCap), displayOrder, TaggedString.op_Implicit(Translator.Translate("VF_MinRange")), vehicleTurretDef.minRange.ToString("F0"), TaggedString.op_Implicit(Translator.Translate("VF_MinRangeTooltip")), 5010);
    float num = (double) vehicleTurretDef.maxRange <= 0.0 ? 9999f : vehicleTurretDef.maxRange;
    yield return new VehicleStatDrawEntry(TaggedString.op_Implicit(vehicleTurretDef.LabelCap), displayOrder, TaggedString.op_Implicit(Translator.Translate("VF_MaxRange")), num.ToString("F0"), TaggedString.op_Implicit(Translator.Translate("VF_MaxRangeTooltip")), 5000);
    yield return new VehicleStatDrawEntry(TaggedString.op_Implicit(vehicleTurretDef.LabelCap), displayOrder, TaggedString.op_Implicit(Translator.Translate("VF_WarmupTime")), TaggedString.op_Implicit(TranslatorFormattedStringExtensions.Translate("VF_WarmupTimeValue", NamedArgument.op_Implicit(GenText.ToStringByStyle(vehicleTurretDef.warmUpTimer, (ToStringStyle) 1, (ToStringNumberSense) 1)))), TaggedString.op_Implicit(Translator.Translate("VF_WarmupTimeTooltip")), 4010);
    yield return new VehicleStatDrawEntry(TaggedString.op_Implicit(vehicleTurretDef.LabelCap), displayOrder, TaggedString.op_Implicit(Translator.Translate("VF_ReloadTime")), TaggedString.op_Implicit(TranslatorFormattedStringExtensions.Translate("VF_ReloadTimeValue", NamedArgument.op_Implicit(GenText.ToStringByStyle(vehicleTurretDef.reloadTimer, (ToStringStyle) 1, (ToStringNumberSense) 1)))), TaggedString.op_Implicit(Translator.Translate("VF_ReloadTimeTooltip")), 4000);
    string valueString2 = TaggedString.op_Implicit(vehicleTurretDef.autoSnapTargeting ? Translator.Translate("VF_Instant") : TranslatorFormattedStringExtensions.Translate("VF_RotationSpeedValue", NamedArgument.op_Implicit(Mathf.RoundToInt(vehicleTurretDef.rotationSpeed * 60f))));
    yield return new VehicleStatDrawEntry(TaggedString.op_Implicit(vehicleTurretDef.LabelCap), displayOrder, TaggedString.op_Implicit(Translator.Translate("VF_RotationSpeed")), valueString2, TaggedString.op_Implicit(Translator.Translate("VF_RotationSpeedTooltip")), 3000);
    StringBuilder stringBuilder = new StringBuilder();
    foreach (FireMode fireMode in vehicleTurretDef.fireModes)
    {
      stringBuilder.AppendLine();
      stringBuilder.AppendLine(fireMode.label);
      int roundsPerMinute = ((IntRange) ref fireMode.shotsPerBurst).TrueMax > 1 ? fireMode.RoundsPerMinute : Mathf.RoundToInt((float) (60.0 / ((double) vehicleTurretDef.warmUpTimer + (double) vehicleTurretDef.reloadTimer)));
      stringBuilder.AppendLine($"    {Translator.Translate("VF_RateOfFire")}: {TranslatorFormattedStringExtensions.Translate("VF_RateOfFireValue", NamedArgument.op_Implicit(VehicleTurretDef.RoundsPerMinuteRounded(roundsPerMinute)))}");
      if (((IntRange) ref fireMode.ticksBetweenBursts).TrueMax > fireMode.ticksBetweenShots)
      {
        string str = fireMode.shotsPerBurst.min == fireMode.shotsPerBurst.max ? fireMode.shotsPerBurst.min.ToString() : fireMode.shotsPerBurst.ToString();
        stringBuilder.AppendLine($"    {Translator.Translate("VF_ShotsPerBurst")}: {str}");
      }
      stringBuilder.AppendLine($"    {Translator.Translate("VF_ShotGroup")}: {TranslatorFormattedStringExtensions.Translate("VF_ShotGroupValue", NamedArgument.op_Implicit(fireMode.forcedMissRadius))}");
    }
    yield return new VehicleStatDrawEntry(TaggedString.op_Implicit(vehicleTurretDef.LabelCap), displayOrder, TaggedString.op_Implicit(Translator.Translate("VF_FireModes")), string.Empty, $"{Translator.Translate("VF_FireModesTooltip")}{Environment.NewLine}{stringBuilder}", 99998);
  }

  [UsedImplicitly]
  public static int RoundsPerMinuteRounded(int roundsPerMinute)
  {
    return roundsPerMinute >= 100 ? (roundsPerMinute < 1000 ? roundsPerMinute.RoundTo(10) : roundsPerMinute.RoundTo(50)) : (roundsPerMinute < 25 ? roundsPerMinute : roundsPerMinute.RoundTo(5));
  }

  public void OnFieldChanged()
  {
  }

  public virtual void ResolveReferences()
  {
    base.ResolveReferences();
    this.ammunition?.ResolveReferences();
    this.ValidateTargetScanFlags();
  }

  public virtual void PostDefDatabase()
  {
  }

  public virtual void PostLoad()
  {
    ((Editable) this).PostLoad();
    LongEventHandler.ExecuteWhenFinished((Action) (() =>
    {
      if (this.graphicData == null)
        return;
      VehicleTurretDef.FixInvalidGraphicDataFields(this.graphicData);
      if (GenList.NullOrEmpty<VehicleTurretRenderData>((IList<VehicleTurretRenderData>) this.graphics))
        return;
      foreach (VehicleTurretRenderData graphic in this.graphics)
        VehicleTurretDef.FixInvalidGraphicDataFields(graphic.graphicData);
    }));
  }

  private void ValidateTargetScanFlags()
  {
    if (this.targetScanFlags != null || this.projectile?.projectile == null)
      return;
    this.targetScanFlags = this.projectile.projectile.flyOverhead ? (TargetScanFlags) (this.targetScanFlags | 512 /*0x0200*/) : (TargetScanFlags) (this.targetScanFlags | 3);
    if (!this.projectile.projectile.ai_IsIncendiary)
      return;
    this.targetScanFlags = (TargetScanFlags) (this.targetScanFlags | 16 /*0x10*/);
  }

  private static void FixInvalidGraphicDataFields(GraphicDataRGB graphicData)
  {
    if (graphicData == null)
      return;
    if (graphicData.shaderType == null)
      graphicData.shaderType = ShaderTypeDefOf.Cutout;
    else if (!VehicleMod.settings.main.useCustomShaders)
      graphicData.shaderType = graphicData.shaderType.Shader.SupportsRGBMaskTex(true) ? ShaderTypeDefOf.CutoutComplex : graphicData.shaderType;
    graphicData.RecacheLayerOffsets();
  }

  public virtual IEnumerable<string> ConfigErrors()
  {
    foreach (string configError in base.ConfigErrors())
      yield return configError;
    if (this.motes.NotNullAndAny<AnimationProperties>((Predicate<AnimationProperties>) (m => m.moteDef == null || m.animationType == AnimationWrapperType.Off)))
      yield return "Invalid fields in <field>motes</field>. <field>moteDef</field> cannot be null and <field>animationType</field> cannot be \"Off\"".ConvertRichText();
    if (this.graphicData == null && GenText.NullOrEmpty(this.gizmoIconTexPath))
      yield return "Null graphicData and no gizmoIconTexPath, this turret has no way to be rendered in gizmos.";
    if (GenList.NullOrEmpty<FireMode>((IList<FireMode>) this.fireModes) || GenCollection.Any<FireMode>(this.fireModes, (Predicate<FireMode>) (f => !f.IsValid)))
      yield return "Empty or Invalid <field>fireModes</field> list. Must include at least 1 entry with non-negative numbers.".ConvertRichText();
    if (this.ammunition == null && this.projectile == null)
      yield return "Must include either <field>ammunition</field> or a default <field>projectile</field>.".ConvertRichText();
    if (this.ammunition == null)
    {
      if (this.genericAmmo)
        yield return "Turret has no <field>ammunition</field> field, but has been flagged as using <field>genericAmmo</field>. This makes no sense.";
      if (!Mathf.Approximately(this.chargePerAmmoCount, 1f))
        yield return "Turret has no <field>ammunition</field> field, but has been assigned <field>chargePerAmmoCount</field>. This makes no sense.";
    }
    if ((double) this.chargePerAmmoCount <= 0.0)
      yield return "<field>chargePerAmmoCount</field> must be greater than 0.".ConvertRichText();
    if (this.ammunition != null)
    {
      if (!Ext_Mods.HasActiveMod("CETeam.CombatExtended") && !this.genericAmmo && !this.ammunition.AllowedThingDefs.Any<ThingDef>((Func<ThingDef, bool>) (c => c.projectile != null || c.projectileWhenLoaded != null)))
        yield return "Non-generic ammo must be a <type>ThingDef</type> with projectile properties.".ConvertRichText();
      if (this.ammunition.AllowedDefCount == 0)
        yield return "<field>ammunition</field> is non-null but no defs are available to use as ammo. Either omit the field entirely or specify valid <type>ThingDefs</type> to use as ammo.".ConvertRichText();
    }
    if (this.genericAmmo)
    {
      if (this.projectile == null)
        yield return "Generic ammo must include a default projectile so the turret knows what to shoot.".ConvertRichText();
      if (this.ammunition != null && this.ammunition.AllowedDefCount != 1)
        yield return "Generic ammo turrets will only use the first <type>ThingDef</type> in <field>ammunition</field>. Consider removing all other entries but the first.".ConvertRichText();
    }
    if (GenCollection.Any<FireMode>(this.fireModes, (Predicate<FireMode>) (f => f.ticksBetweenShots > ((IntRange) ref f.ticksBetweenBursts).TrueMin)))
      yield return "Setting <field>ticksBetweenBursts</field> with a lower tick count than <field>ticksBetweenShots</field> will produce odd shooting behavior. Please set to either the same amount (fully automatic) or greater than.".ConvertRichText();
  }

  public Vector2 ScaleDrawRatio(VehicleDef vehicleDef, Vector2 size)
  {
    Vector2 drawSize = this.graphicData.drawSize;
    Vector2 vector2 = Vector2.op_Division(drawSize, vehicleDef.graphicData.drawSize);
    float num1 = size.x * vehicleDef.uiIconScale * vector2.x;
    float num2 = size.y * vehicleDef.uiIconScale * vector2.y;
    if ((double) num1 < (double) num2)
      num2 = num1 * (drawSize.y / drawSize.x);
    else
      num1 = num2 * (drawSize.x / drawSize.y);
    return new Vector2(num1, num2);
  }
}
