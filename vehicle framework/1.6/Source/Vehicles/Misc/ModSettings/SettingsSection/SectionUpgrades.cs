// Decompiled with JetBrains decompiler
// Type: Vehicles.SectionUpgrades
// Assembly: Vehicles, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 81BFDC99-D8AD-42E1-8470-2F73E7836B4A
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\Vehicles.dll

using RimWorld;
using RimWorld.Planet;
using SmashTools;
using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;
using Verse.Sound;

#nullable disable
namespace Vehicles;

public class SectionUpgrades : SettingsSection
{
  public Dictionary<string, Dictionary<SaveableField, SavedField<object>>> upgradeSettings = new Dictionary<string, Dictionary<SaveableField, SavedField<object>>>();

  public override IEnumerable<FloatMenuOption> ResetOptions
  {
    get
    {
      SectionUpgrades sectionUpgrades = this;
      if (VehicleMod.selectedDef != null)
        yield return new FloatMenuOption(TaggedString.op_Implicit(TranslatorFormattedStringExtensions.Translate("VF_DevMode_ResetVehicle", NamedArgument.op_Implicit(((Def) VehicleMod.selectedDef).LabelCap))), (Action) (() => SettingsCustomizableFields.PopulateSaveableUpgrades(VehicleMod.selectedDef, true)), (MenuOptionPriority) 4, (Action<Rect>) null, (Thing) null, 0.0f, (Func<Rect, bool>) null, (WorldObject) null, true, 0);
      yield return new FloatMenuOption(TaggedString.op_Implicit(Translator.Translate("VF_DevMode_ResetAllVehicles")), new Action(((SettingsSection) sectionUpgrades).ResetSettings), (MenuOptionPriority) 4, (Action<Rect>) null, (Thing) null, 0.0f, (Func<Rect, bool>) null, (WorldObject) null, true, 0);
      yield return new FloatMenuOption(TaggedString.op_Implicit(Translator.Translate("VF_DevMode_ResetAll")), SectionUpgrades.\u003C\u003EO.\u003C0\u003E__ResetAllSettings ?? (SectionUpgrades.\u003C\u003EO.\u003C0\u003E__ResetAllSettings = new Action(VehicleMod.ResetAllSettings)), (MenuOptionPriority) 4, (Action<Rect>) null, (Thing) null, 0.0f, (Func<Rect, bool>) null, (WorldObject) null, true, 0);
    }
  }

  public override void Initialize()
  {
    if (this.upgradeSettings != null)
      return;
    this.upgradeSettings = new Dictionary<string, Dictionary<SaveableField, SavedField<object>>>();
  }

  public override void ResetSettings()
  {
    base.ResetSettings();
    this.upgradeSettings.Clear();
    if (!VehicleMod.ModifiableSettings)
      return;
    foreach (VehicleDef def in DefDatabase<VehicleDef>.AllDefsListForReading)
      SettingsCustomizableFields.PopulateSaveableUpgrades(def, true);
  }

  public override void ExposeData()
  {
    Scribe_NestedCollections.Look<string, SaveableField, SavedField<object>>(ref this.upgradeSettings, "upgradeSettings", (LookMode) 1, (LookMode) 2, (LookMode) 0);
  }

  public override void OnGUI(Rect rect)
  {
    SectionUpgrades.DrawVehicleUpgrades(rect);
    SectionDrawer.DrawVehicleList(rect, (Func<bool, string>) (isValid => !isValid ? Translator.Translate("VF_NonUpgradeableSettingsTooltip").ToString() : string.Empty), (Predicate<VehicleDef>) (vehicleDef => !VehicleMod.SettingsDisabledFor.Contains(((Def) vehicleDef).defName) && vehicleDef.HasComp(typeof (CompUpgradeTree))));
  }

  private static void DrawVehicleUpgrades(Rect menuRect)
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
    SettingsSection.listingStandard = new Listing_Standard();
    ((Listing) SettingsSection.listingStandard).Begin(GenUI.ContractedBy(rect2, 1f));
    Listing_Standard listingStandard = SettingsSection.listingStandard;
    VehicleDef selectedDef = VehicleMod.selectedDef;
    string header = $"{(selectedDef != null ? ((Def) selectedDef).LabelCap : TaggedString.op_Implicit(string.Empty))}";
    Color bannerColor = ListingExtension.BannerColor;
    ((Listing) listingStandard).Header(header, bannerColor, (GameFont) 2, (TextAnchor) 4);
    ((Listing) SettingsSection.listingStandard).End();
    ref Rect local5 = ref rect2;
    ((Rect) ref local5).y = ((Rect) ref local5).y + 5f;
    if (VehicleMod.selectedDef == null)
      return;
    if (VehicleMod.selectedDefUpgradeComp == null)
      return;
    try
    {
      foreach (UpgradeNode node in VehicleMod.selectedDefUpgradeComp.def.nodes)
      {
        UpgradeNode upgradeNode = node;
        if (!GenList.NullOrEmpty<string>((IList<string>) upgradeNode.prerequisiteNodes))
        {
          foreach (UpgradeNode upgradeNode1 in VehicleMod.selectedDefUpgradeComp.def.nodes.FindAll((Predicate<UpgradeNode>) (x => upgradeNode.prerequisiteNodes.Contains(x.key))))
          {
            Vector2 vector2_1;
            // ISSUE: explicit constructor call
            ((Vector2) ref vector2_1).\u002Ector((float) ((double) ((Rect) ref rect2).x + (double) ITab_Vehicle_Upgrades.GridOrigin.x + (double) ITab_Vehicle_Upgrades.GridSpacing.x * (double) upgradeNode1.GridCoordinate.x), (float) ((double) ((Rect) ref rect2).y + (double) ITab_Vehicle_Upgrades.GridOrigin.y + (double) ITab_Vehicle_Upgrades.GridSpacing.y * (double) upgradeNode1.GridCoordinate.z + 40.0));
            Vector2 vector2_2;
            // ISSUE: explicit constructor call
            ((Vector2) ref vector2_2).\u002Ector((float) ((double) ((Rect) ref rect2).x + (double) ITab_Vehicle_Upgrades.GridOrigin.x + (double) ITab_Vehicle_Upgrades.GridSpacing.x * (double) upgradeNode.GridCoordinate.x), (float) ((double) ((Rect) ref rect2).y + (double) ITab_Vehicle_Upgrades.GridOrigin.y + (double) ITab_Vehicle_Upgrades.GridSpacing.y * (double) upgradeNode.GridCoordinate.z + 40.0));
            Color grey = Color.grey;
            Widgets.DrawLine(vector2_1, vector2_2, grey, 2f);
          }
        }
      }
      foreach (UpgradeNode node in VehicleMod.selectedDefUpgradeComp.def.nodes)
      {
        float num1 = 6000f / (float) ((Texture) node.UpgradeImage).width;
        float num2 = 6000f / (float) ((Texture) node.UpgradeImage).height;
        Rect rect3;
        // ISSUE: explicit constructor call
        ((Rect) ref rect3).\u002Ector((float) ((double) ((Rect) ref rect2).x + (double) ITab_Vehicle_Upgrades.GridOrigin.x + (double) ITab_Vehicle_Upgrades.GridSpacing.x * (double) node.GridCoordinate.x - (double) num1 / 2.0), (float) ((double) ((Rect) ref rect2).y + (double) ITab_Vehicle_Upgrades.GridOrigin.y + (double) ITab_Vehicle_Upgrades.GridSpacing.y * (double) node.GridCoordinate.z - (double) num2 / 2.0 + 40.0), num1, num2);
        Widgets.DrawTextureFitted(rect3, (Texture) node.UpgradeImage, 1f, 1f);
        if (node.displayLabel)
        {
          float x = Text.CalcSize(node.label).x;
          Rect rect4;
          // ISSUE: explicit constructor call
          ((Rect) ref rect4).\u002Ector(((Rect) ref rect3).x - (float) (((double) x - (double) ((Rect) ref rect3).width) / 2.0), ((Rect) ref rect3).y - 20f, 10f * (float) node.label.Length, 25f);
          Widgets.Label(rect4, node.label);
        }
        Rect rect5;
        // ISSUE: explicit constructor call
        ((Rect) ref rect5).\u002Ector((float) ((double) ((Rect) ref rect2).x + (double) ITab_Vehicle_Upgrades.GridOrigin.x + (double) ITab_Vehicle_Upgrades.GridSpacing.x * (double) node.GridCoordinate.x - (double) num1 / 2.0), (float) ((double) ((Rect) ref rect2).y + (double) ITab_Vehicle_Upgrades.GridOrigin.y + (double) ITab_Vehicle_Upgrades.GridSpacing.y * (double) node.GridCoordinate.z - (double) num2 / 2.0 + 40.0), num1, num2);
        if (Mouse.IsOver(rect3) || VehicleMod.selectedNode == node)
          GUI.DrawTexture(rect3, (Texture) TexUI.HighlightTex);
        if (Mouse.IsOver(rect3))
          TooltipHandler.TipRegion(rect3, TipSignal.op_Implicit(node.label));
        if (Widgets.ButtonInvisible(rect5, true))
        {
          if (VehicleMod.selectedNode != node)
          {
            VehicleMod.selectedNode = node;
            Find.WindowStack.Add((Window) new Dialog_NodeSettings(VehicleMod.selectedDef, VehicleMod.selectedNode, new Vector2(((Rect) ref rect5).x + num1 * 2f, ((Rect) ref rect5).y + num2 / 2f)));
            SoundStarter.PlayOneShotOnCamera(SoundDefOf.Checkbox_TurnedOn, (Map) null);
          }
          else
          {
            VehicleMod.selectedNode = (UpgradeNode) null;
            SoundStarter.PlayOneShotOnCamera(SoundDefOf.Checkbox_TurnedOff, (Map) null);
          }
        }
      }
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
}
