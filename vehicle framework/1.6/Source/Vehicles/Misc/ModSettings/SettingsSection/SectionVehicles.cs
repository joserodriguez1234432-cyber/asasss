// Decompiled with JetBrains decompiler
// Type: Vehicles.SectionVehicles
// Assembly: Vehicles, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 81BFDC99-D8AD-42E1-8470-2F73E7836B4A
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\Vehicles.dll

using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using SmashTools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using Vehicles.Rendering;
using Verse;
using Verse.Sound;

#nullable disable
namespace Vehicles;

public class SectionVehicles : SettingsSection
{
  private const int DefPropertyColumns = 2;
  private const int CompPropertyColumns = 3;
  private const float SmallIconSize = 24f;
  private static readonly FieldInfo EnabledField = AccessTools.Field(typeof (VehicleDef), "enabled");
  private static DesignationCategoryDef structureDesignationDef;
  public Dictionary<string, Dictionary<SaveableField, SavedField<object>>> fieldSettings = new Dictionary<string, Dictionary<SaveableField, SavedField<object>>>();
  public Dictionary<string, Dictionary<SaveableField, object>> defaultValues = new Dictionary<string, Dictionary<SaveableField, object>>();
  public Dictionary<string, Dictionary<string, float>> vehicleStats = new Dictionary<string, Dictionary<string, float>>();
  public Dictionary<string, PatternData> defaultGraphics = new Dictionary<string, PatternData>();
  private readonly Dictionary<VehicleDef, Rot8> directionFacing = new Dictionary<VehicleDef, Rot8>();
  private Rot8 currentVehicleFacing;
  private float propertyHeight;

  public override IEnumerable<FloatMenuOption> ResetOptions
  {
    get
    {
      SectionVehicles sectionVehicles = this;
      if (VehicleMod.selectedDef != null)
        yield return new FloatMenuOption(TaggedString.op_Implicit(TranslatorFormattedStringExtensions.Translate("VF_DevMode_ResetVehicle", NamedArgument.op_Implicit(((Def) VehicleMod.selectedDef).LabelCap))), new Action(sectionVehicles.\u003Cget_ResetOptions\u003Eb__13_0), (MenuOptionPriority) 4, (Action<Rect>) null, (Thing) null, 0.0f, (Func<Rect, bool>) null, (WorldObject) null, true, 0);
      yield return new FloatMenuOption(TaggedString.op_Implicit(Translator.Translate("VF_DevMode_ResetAllVehicles")), new Action(((SettingsSection) sectionVehicles).ResetSettings), (MenuOptionPriority) 4, (Action<Rect>) null, (Thing) null, 0.0f, (Func<Rect, bool>) null, (WorldObject) null, true, 0);
      yield return new FloatMenuOption(TaggedString.op_Implicit(Translator.Translate("VF_DevMode_ResetAll")), SectionVehicles.\u003C\u003EO.\u003C0\u003E__ResetAllSettings ?? (SectionVehicles.\u003C\u003EO.\u003C0\u003E__ResetAllSettings = new Action(VehicleMod.ResetAllSettings)), (MenuOptionPriority) 4, (Action<Rect>) null, (Thing) null, 0.0f, (Func<Rect, bool>) null, (WorldObject) null, true, 0);
    }
  }

  public override void OnOpen()
  {
    Listing_Settings listingSettings = new Listing_Settings();
    listingSettings.maxOneColumn = true;
    listingSettings.shiftRectScrollbar = true;
    SettingsSection.listingSplit = listingSettings;
  }

  public override void Initialize()
  {
    if (this.fieldSettings == null)
      this.fieldSettings = new Dictionary<string, Dictionary<SaveableField, SavedField<object>>>();
    if (this.vehicleStats == null)
      this.vehicleStats = new Dictionary<string, Dictionary<string, float>>();
    if (this.defaultGraphics != null)
      return;
    this.defaultGraphics = new Dictionary<string, PatternData>();
  }

  public override void ResetSettings()
  {
    base.ResetSettings();
    VehicleMod.CachedFields.Clear();
    VehicleMod.PopulateCachedFields();
    this.fieldSettings.Clear();
    this.vehicleStats.Clear();
    this.defaultGraphics.Clear();
    if (!VehicleMod.ModifiableSettings)
      return;
    foreach (VehicleDef def in DefDatabase<VehicleDef>.AllDefsListForReading)
      SettingsCustomizableFields.PopulateSaveableFields(def, true);
  }

  public override void PostDefDatabase()
  {
    foreach (PatternData patternData in this.defaultGraphics.Values)
      patternData.ExposeDataPostDefDatabase();
    SectionVehicles.structureDesignationDef = DefDatabase<DesignationCategoryDef>.GetNamed("Structure", true);
  }

  public override void ExposeData()
  {
    Scribe_NestedCollections.Look<string, SaveableField, SavedField<object>>(ref this.fieldSettings, "fieldSettings", (LookMode) 1, (LookMode) 2, (LookMode) 0);
    Scribe_NestedCollections.Look<string, string, float>(ref this.vehicleStats, "vehicleStats", (LookMode) 1, (LookMode) 1, (LookMode) 1);
    Scribe_Collections.Look<string, PatternData>(ref this.defaultGraphics, "defaultGraphics", (LookMode) 1, (LookMode) 2);
  }

  public override void OnGUI(Rect rect)
  {
    this.DrawVehicleOptions(rect);
    SectionDrawer.DrawVehicleList(rect, (Func<bool, string>) (isValid => !isValid ? Translator.Translate("VF_SettingsDisabledTooltip").ToString() : string.Empty), (Predicate<VehicleDef>) (vehicleDef => !VehicleMod.SettingsDisabledFor.Contains(((Def) vehicleDef).defName)));
  }

  private void RecalculateHeight()
  {
    float num1 = 5f;
    foreach (List<FieldInfo> source in VehicleMod.VehicleCompFields.Values)
    {
      PostToSettingsAttribute settingsAttribute;
      if (!GenList.NullOrEmpty<FieldInfo>((IList<FieldInfo>) source) && !source.All<FieldInfo>((Func<FieldInfo, bool>) (f => GenAttribute.TryGetAttribute<PostToSettingsAttribute>((MemberInfo) f, ref settingsAttribute) && settingsAttribute.VehicleType != VehicleType.Universal && settingsAttribute.VehicleType != VehicleMod.selectedDef.type)))
      {
        int num2 = Mathf.CeilToInt((float) source.Count / 3f);
        num1 += (float) (50 + num2 * 16 /*0x10*/);
      }
    }
    this.propertyHeight = num1;
  }

  public override void VehicleSelected()
  {
    this.currentVehicleFacing = VehicleMod.selectedDef.drawProperties.displayRotation;
    this.RecalculateHeight();
  }

  private void DrawVehicleOptions(Rect menuRect)
  {
    Rect rect1 = GenUI.ContractedBy(menuRect, 10f);
    ref Rect local1 = ref rect1;
    ((Rect) ref local1).width = ((Rect) ref local1).width / 4f;
    ((Rect) ref rect1).height = ((Rect) ref rect1).width;
    ref Rect local2 = ref rect1;
    ((Rect) ref local2).x = ((Rect) ref local2).x + ((Rect) ref rect1).width;
    Rect rect2 = GenUI.ContractedBy(menuRect, 10f);
    ref Rect local3 = ref rect2;
    ((Rect) ref local3).x = ((Rect) ref local3).x + (((Rect) ref rect1).width - 1f);
    ref Rect local4 = ref rect2;
    ((Rect) ref local4).width = ((Rect) ref local4).width - ((Rect) ref rect1).width;
    Widgets.DrawBoxSolid(rect2, Color.grey);
    Widgets.DrawBoxSolid(GenUI.ContractedBy(rect2, 1f), ListingExtension.MenuSectionBGFillColor);
    Rect rect3 = rect2;
    ((Rect) ref rect3).height = Text.LineHeightOf((GameFont) 2);
    Rect rect4 = GenUI.ContractedBy(rect3, 1f);
    VehicleDef selectedDef = VehicleMod.selectedDef;
    string header1 = $"{(selectedDef != null ? ((Def) selectedDef).LabelCap : TaggedString.op_Implicit(string.Empty))}";
    Color bannerColor = ListingExtension.BannerColor;
    UIElements.Header(rect4, header1, bannerColor, (GameFont) 2, (TextAnchor) 4);
    if (VehicleMod.selectedDef == null)
      return;
    try
    {
      Rect rect5 = GenUI.ContractedBy(menuRect, 10f);
      ref Rect local5 = ref rect5;
      ((Rect) ref local5).width = ((Rect) ref local5).width / 5f;
      ((Rect) ref rect5).height = ((Rect) ref rect5).width;
      ref Rect local6 = ref rect5;
      ((Rect) ref local6).x = ((Rect) ref local6).x + ((Rect) ref menuRect).width / 4f;
      ref Rect local7 = ref rect5;
      ((Rect) ref local7).y = ((Rect) ref local7).y + 35f;
      this.DoShowcaseButtons(rect5);
      BlitRequest request = BlitRequest.For(VehicleMod.selectedDef) with
      {
        rot = GenCollection.TryGetValue<VehicleDef, Rot8>((IReadOnlyDictionary<VehicleDef, Rot8>) this.directionFacing, VehicleMod.selectedDef, this.currentVehicleFacing)
      };
      VehicleGui.DrawVehicleOnGUI(rect5, in request);
      Rect rect6 = GenUI.ContractedBy(menuRect, 10f);
      ref Rect local8 = ref rect6;
      ((Rect) ref local8).x = ((Rect) ref local8).x + (float) ((double) ((Rect) ref rect6).width / 4.0 + 5.0);
      this.EnableButton(rect6);
      Rect rect7 = GenUI.ContractedBy(menuRect, 10f);
      ref Rect local9 = ref rect7;
      ((Rect) ref local9).x = ((Rect) ref local9).x + (float) ((double) ((Rect) ref rect1).width * 2.0 - 10.0);
      ((Rect) ref rect7).y = ((Rect) ref rect5).y;
      ref Rect local10 = ref rect7;
      ((Rect) ref local10).width = ((Rect) ref local10).width - ((Rect) ref rect1).width * 2f;
      ((Rect) ref rect7).height = ((Rect) ref rect5).height;
      SettingsSection.listingSplit.Begin(rect7, 2);
      foreach (FieldInfo vehicleDefField in VehicleMod.vehicleDefFields)
      {
        PostToSettingsAttribute settingsAttribute;
        if (GenAttribute.TryGetAttribute<PostToSettingsAttribute>((MemberInfo) vehicleDefField, ref settingsAttribute))
          settingsAttribute.DrawLister(SettingsSection.listingSplit, VehicleMod.selectedDef, vehicleDefField);
      }
      SettingsSection.listingSplit.Shift();
      if (Widgets.ButtonText(SettingsSection.listingSplit.GetSplitRect(24f), TaggedString.op_Implicit(Translator.Translate("VF_VehicleStats")), true, true, true, new TextAnchor?()))
      {
        SoundStarter.PlayOneShotOnCamera(SoundDefOf.Click, (Map) null);
        Find.WindowStack.Add((Window) new Dialog_StatSettings(VehicleMod.selectedDef));
      }
      ((Listing) SettingsSection.listingSplit).End();
      float num = ((Rect) ref menuRect).height * 0.4f;
      Rect rect8;
      // ISSUE: explicit constructor call
      ((Rect) ref rect8).\u002Ector(((Rect) ref rect2).x + 1f, ((Rect) ref menuRect).y + num, ((Rect) ref rect2).width - 2f, (float) ((double) ((Rect) ref menuRect).height - (double) num - 10.0));
      Rect viewRect;
      // ISSUE: explicit constructor call
      ((Rect) ref viewRect).\u002Ector(((Rect) ref rect8).x, ((Rect) ref rect8).y, ((Rect) ref rect8).width - 20f, this.propertyHeight);
      UIElements.DrawLineHorizontalGrey(((Rect) ref rect8).x, ((Rect) ref rect8).y - 1f, ((Rect) ref rect8).width);
      SettingsSection.listingSplit.BeginScrollView(rect8, ref SectionDrawer.saveableFieldsScrollPosition, ref viewRect, 3);
      foreach (KeyValuePair<System.Type, List<FieldInfo>> vehicleCompField in VehicleMod.VehicleCompFields)
      {
        System.Type type1;
        List<FieldInfo> fieldInfoList;
        vehicleCompField.Deconstruct(ref type1, ref fieldInfoList);
        System.Type type2 = type1;
        List<FieldInfo> source = fieldInfoList;
        PostToSettingsAttribute settingsAttribute1;
        if (!GenList.NullOrEmpty<FieldInfo>((IList<FieldInfo>) source) && !source.All<FieldInfo>((Func<FieldInfo, bool>) (f => GenAttribute.TryGetAttribute<PostToSettingsAttribute>((MemberInfo) f, ref settingsAttribute1) && settingsAttribute1.VehicleType != VehicleType.Universal && settingsAttribute1.VehicleType != VehicleMod.selectedDef.type)))
        {
          string header2 = string.Empty;
          HeaderTitleAttribute headerTitleAttribute;
          if (GenAttribute.TryGetAttribute<HeaderTitleAttribute>((MemberInfo) type2, ref headerTitleAttribute))
            header2 = headerTitleAttribute.Translate ? Translator.Translate(headerTitleAttribute.Label).ToString() : headerTitleAttribute.Label;
          SettingsSection.listingSplit.Header(header2, ListingExtension.BannerColor, (GameFont) 1, (TextAnchor) 4, 24f);
          foreach (FieldInfo field in source)
          {
            PostToSettingsAttribute settingsAttribute2;
            if (GenAttribute.TryGetAttribute<PostToSettingsAttribute>((MemberInfo) field, ref settingsAttribute2))
              settingsAttribute2.DrawLister(SettingsSection.listingSplit, VehicleMod.selectedDef, field);
          }
        }
      }
      SettingsSection.listingSplit.EndScrollView(ref viewRect);
    }
    catch (Exception ex)
    {
      Log.Error($"Exception thrown while trying to select {((Def) VehicleMod.selectedDef).defName}. Disabling vehicle to preserve mod settings.\nException={ex}");
      VehicleMod.SettingsDisabledFor.Add(((Def) VehicleMod.selectedDef).defName);
      VehicleMod.selectedDef = (VehicleDef) null;
      VehicleMod.selectedPatterns.Clear();
      VehicleMod.selectedDefUpgradeComp = (CompProperties_UpgradeTree) null;
      VehicleMod.selectedNode = (UpgradeNode) null;
    }
  }

  private void DoShowcaseButtons(Rect iconRect)
  {
    Rect rect;
    // ISSUE: explicit constructor call
    ((Rect) ref rect).\u002Ector(((Rect) ref iconRect).x + ((Rect) ref iconRect).width, ((Rect) ref iconRect).y, 24f, 24f);
    if (VehicleMod.selectedDef.graphicData.drawRotated && VehicleMod.selectedDef.graphicData.Graphic is Graphic_Vehicle graphic)
    {
      if (SectionVehicles.VehicleShowcaseButton(rect, VehicleTex.Rotate))
      {
        List<Rot8> list = graphic.RotationsRenderableByUI.ToList<Rot8>();
        for (int index = 0; index < 4; ++index)
        {
          this.currentVehicleFacing = this.currentVehicleFacing.Rotated((RotationDirection) 1, false);
          if (list.Contains(this.currentVehicleFacing))
            break;
        }
      }
      ref Rect local = ref rect;
      ((Rect) ref local).y = ((Rect) ref local).y + 24f;
    }
    if (VehicleMod.selectedPatterns.Count <= 1 || !VehicleMod.settings.main.useCustomShaders || !VehicleMod.selectedDef.graphicData.shaderType.Shader.SupportsRGBMaskTex())
      return;
    if (SectionVehicles.VehicleShowcaseButton(rect, VehicleTex.Recolor, "VF_RecolorDefaultMaskTooltip"))
      Dialog_VehiclePainter.OpenColorPicker(VehicleMod.selectedDef, new Dialog_VehiclePainter.SaveColor(this.SetDefaultColor));
    ref Rect local1 = ref rect;
    ((Rect) ref local1).y = ((Rect) ref local1).y + 24f;
  }

  private void SetDefaultColor(
    Color colorOne,
    Color colorTwo,
    Color colorThree,
    PatternDef pattern,
    Vector2 displacement,
    float tiles)
  {
    this.defaultGraphics[((Def) VehicleMod.selectedDef).defName] = new PatternData(colorOne, colorTwo, colorThree, pattern, displacement, tiles);
  }

  private static bool VehicleShowcaseButton(Rect rect, Texture2D icon, string tooltipKey = null)
  {
    Widgets.DrawHighlightIfMouseover(rect);
    Widgets.DrawTextureFitted(rect, (Texture) icon, 1f, 1f);
    if (!GenText.NullOrEmpty(tooltipKey))
      TooltipHandler.TipRegionByKey(rect, tooltipKey);
    if (!Widgets.ButtonInvisible(rect, true))
      return false;
    SoundStarter.PlayOneShotOnCamera(SoundDefOf.Click, (Map) null);
    return true;
  }

  private void EnableButton(Rect rect)
  {
    if (VehicleMod.selectedDef == null)
    {
      Log.Error("SelectedDef is null while trying to create Enable button for VehicleDef.");
    }
    else
    {
      TextBlock textBlock;
      // ISSUE: explicit constructor call
      ((TextBlock) ref textBlock).\u002Ector((GameFont) 2);
      try
      {
        SaveableField key = new SaveableField((Def) VehicleMod.selectedDef, SectionVehicles.EnabledField);
        VehicleEnabled.For for1 = VehicleEnabled.For.Everyone;
        SavedField<object> savedField;
        if (this.fieldSettings[((Def) VehicleMod.selectedDef).defName].TryGetValue(key, out savedField))
          for1 = (VehicleEnabled.For) savedField.EndValue;
        (string str, Color color) = VehicleEnabled.GetStatus(for1);
        Vector2 vector2 = Text.CalcSize(str);
        Rect rect1 = new Rect(((Rect) ref rect).x, ((Rect) ref rect).y, vector2.x, vector2.y);
        TooltipHandler.TipRegion(rect1, TipSignal.op_Implicit(Translator.Translate("VF_EnableButtonTooltip")));
        Color mouseOver = new Color(color.r + 0.25f, color.g + 0.25f, color.b + 0.25f);
        if (!UIElements.ClickableLabel(rect1, str, mouseOver, color, (GameFont) 2, clickColor: new Color?(new Color(color.r - 0.15f, color.g - 0.15f, color.b - 0.15f))))
          return;
        VehicleEnabled.For for2 = for1.Next();
        this.fieldSettings[((Def) VehicleMod.selectedDef).defName][key] = new SavedField<object>((object) for2);
        if (for2 == (VehicleEnabled.For) SectionVehicles.EnabledField.GetValue((object) VehicleMod.selectedDef))
          this.fieldSettings[((Def) VehicleMod.selectedDef).defName].Remove(key);
        bool flag = for2 == VehicleEnabled.For.Player || for2 == VehicleEnabled.For.Everyone;
        if (Current.ProgramState == 2)
          Current.Game.Rules.SetAllowBuilding((ThingDef) VehicleMod.selectedDef.buildDef, flag);
        GizmoHelper.DesignatorsChanged(((BuildableDef) VehicleMod.selectedDef).designationCategory ?? SectionVehicles.structureDesignationDef);
      }
      finally
      {
        textBlock.Dispose();
      }
    }
  }
}
