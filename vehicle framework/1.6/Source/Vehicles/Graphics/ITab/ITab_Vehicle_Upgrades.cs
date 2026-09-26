// Decompiled with JetBrains decompiler
// Type: Vehicles.ITab_Vehicle_Upgrades
// Assembly: Vehicles, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 81BFDC99-D8AD-42E1-8470-2F73E7836B4A
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\Vehicles.dll

using RimWorld;
using SmashTools;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Vehicles.Rendering;
using Verse;
using Verse.Sound;

#nullable disable
namespace Vehicles;

[StaticConstructorOnStartup]
public class ITab_Vehicle_Upgrades : ITab
{
  public const float UpgradeNodeDim = 40f;
  public const float TopPadding = 20f;
  public const float EdgePadding = 5f;
  public const float SideDisplayedOffset = 40f;
  public const float BottomDisplayedOffset = 20f;
  public const float TotalIconSizeScalar = 6000f;
  private const float ScreenWidth = 880f;
  private const float ScreenHeight = 520f;
  private const float InfoScreenWidth = 375f;
  private const float InfoScreenHeight = 150f;
  private const float InfoPanelRowHeight = 20f;
  private const float InfoPanelMoreDetailButtonSize = 24f;
  private const float InfoPanelArrowPointerSize = 45f;
  private const float OverlayGraphicHeight = 165f;
  public static readonly Vector2 GridSpacing = new Vector2(20f, 20f);
  public static readonly Vector2 GridOrigin = new Vector2(30f, 30f);
  internal static readonly int MaxLinesAcross = Mathf.FloorToInt(880f / ITab_Vehicle_Upgrades.GridSpacing.x) - 2;
  private static readonly int MaxLinesDown = Mathf.FloorToInt(520f / ITab_Vehicle_Upgrades.GridSpacing.y) - 3;
  public static readonly List<string> ReplaceNodes = new List<string>();
  private static readonly Color NotUpgradedColor = new Color(0.0f, 0.0f, 0.0f, 0.3f);
  private static readonly Color UpgradingColor = new Color(1f, 0.75f, 0.0f, 1f);
  private static readonly Color DisabledColor = new Color(0.25f, 0.25f, 0.25f, 1f);
  private static readonly Color DisabledLineColor = new Color(0.3f, 0.3f, 0.3f, 1f);
  private static readonly Color GridLineColor = new Color(0.3f, 0.3f, 0.3f, 1f);
  private static readonly Color EffectColorPositive = new Color(0.1f, 1f, 0.1f);
  private static readonly Color EffectColorNegative = new Color(0.8f, 0.4f, 0.4f);
  private static readonly Color EffectColorNeutral = new Color(0.5f, 0.5f, 0.5f, 0.75f);
  private VehiclePawn openedFor;
  private UpgradeNode selectedNode;
  private UpgradeNode highlightedNode;
  private readonly List<UpgradeTextEntry> textEntries = new List<UpgradeTextEntry>();
  private float textEntryHeight;
  private readonly List<VehicleTurret> renderTurrets = new List<VehicleTurret>();
  private readonly List<string> excludeTurrets = new List<string>();
  private bool showDetails;

  private UpgradeNode InfoNode
  {
    get
    {
      UpgradeNode selectedNode = this.selectedNode;
      return (object) selectedNode != null ? selectedNode : this.highlightedNode;
    }
  }

  private UpgradeNode SelectedNode
  {
    get => this.selectedNode;
    set
    {
      if (!(this.selectedNode != value))
        return;
      this.ClearTurretRenderers(this.selectedNode);
      this.selectedNode = value;
      this.openedFor = this.Vehicle;
      if (!(this.selectedNode != (UpgradeNode) null))
        return;
      this.RecacheTextEntries();
      this.RecacheTurretRenderers();
    }
  }

  public ITab_Vehicle_Upgrades()
  {
    ((InspectTabBase) this).size = new Vector2(880f, 520f);
    ((InspectTabBase) this).labelKey = "VF_TabUpgrades";
  }

  private VehiclePawn Vehicle
  {
    get
    {
      if (this.SelPawn is VehiclePawn selPawn && selPawn.CompUpgradeTree != null)
      {
        if (selPawn != this.openedFor)
        {
          this.openedFor = selPawn;
          this.SelectedNode = (UpgradeNode) null;
        }
        return selPawn;
      }
      ((InspectTabBase) this).CloseTab();
      return (VehiclePawn) null;
    }
  }

  public virtual void OnOpen()
  {
    ((InspectTabBase) this).OnOpen();
    this.SelectedNode = (UpgradeNode) null;
    this.highlightedNode = (UpgradeNode) null;
  }

  protected virtual void CloseTab()
  {
    base.CloseTab();
    this.openedFor = (VehiclePawn) null;
  }

  private void RecacheTextEntries()
  {
    this.textEntries.Clear();
    this.textEntryHeight = 0.0f;
  }

  private void RecacheTurretRenderers()
  {
    if (GenList.NullOrEmpty<Upgrade>((IList<Upgrade>) this.SelectedNode.upgrades))
      return;
    foreach (Upgrade upgrade in this.InfoNode.upgrades)
    {
      if (upgrade is TurretUpgrade turretUpgrade)
      {
        if (!GenList.NullOrEmpty<VehicleTurret>((IList<VehicleTurret>) turretUpgrade.turrets))
        {
          foreach (VehicleTurret turret in turretUpgrade.turrets)
          {
            turret.ResolveGraphics(this.Vehicle.VehicleDef, true);
            this.renderTurrets.Add(turret);
          }
        }
        if (!GenList.NullOrEmpty<string>((IList<string>) turretUpgrade.removeTurrets))
          this.excludeTurrets.AddRange((IEnumerable<string>) turretUpgrade.removeTurrets);
      }
    }
  }

  private void ClearTurretRenderers(UpgradeNode upgradeNode)
  {
    if (upgradeNode != (UpgradeNode) null && !GenList.NullOrEmpty<Upgrade>((IList<Upgrade>) upgradeNode.upgrades))
    {
      foreach (Upgrade upgrade in this.InfoNode.upgrades)
      {
        if (upgrade is TurretUpgrade turretUpgrade && !GenList.NullOrEmpty<VehicleTurret>((IList<VehicleTurret>) turretUpgrade.turrets))
        {
          foreach (VehicleTurret turret in turretUpgrade.turrets)
            turret.OnDestroy();
        }
      }
    }
    this.renderTurrets.Clear();
    this.excludeTurrets.Clear();
  }

  private IEnumerable<UpgradeNode> GetDisablerNodes(UpgradeNode upgradeNode)
  {
    if (!GenText.NullOrEmpty(upgradeNode.disableIfUpgradeNodeEnabled))
    {
      UpgradeNode node = this.Vehicle.CompUpgradeTree.Props.def.GetNode(upgradeNode.disableIfUpgradeNodeEnabled);
      if (node != (UpgradeNode) null)
        yield return node;
    }
    if (!GenList.NullOrEmpty<string>((IList<string>) upgradeNode.disableIfUpgradeNodesEnabled))
    {
      foreach (string key in upgradeNode.disableIfUpgradeNodesEnabled)
      {
        UpgradeNode node = this.Vehicle.CompUpgradeTree.Props.def.GetNode(key);
        if (node != (UpgradeNode) null)
          yield return node;
      }
    }
  }

  private Vector2 GridCoordinateToScreenPos(IntVec2 coord)
  {
    return new Vector2((float) ((double) ITab_Vehicle_Upgrades.GridOrigin.x + (double) ITab_Vehicle_Upgrades.GridSpacing.x * (double) coord.x - (double) ITab_Vehicle_Upgrades.GridSpacing.x / 2.0), (float) ((double) ITab_Vehicle_Upgrades.GridOrigin.y + (double) ITab_Vehicle_Upgrades.GridSpacing.y * (double) coord.z - (double) ITab_Vehicle_Upgrades.GridSpacing.y / 2.0 + 20.0));
  }

  private Vector2 GridCoordinateToScreenPosAdjusted(IntVec2 coord, Vector2 drawSize)
  {
    float num1 = (float) ((double) ITab_Vehicle_Upgrades.GridOrigin.x + (double) ITab_Vehicle_Upgrades.GridSpacing.x * (double) coord.x - (double) ITab_Vehicle_Upgrades.GridSpacing.x / 2.0);
    float num2 = (float) ((double) ITab_Vehicle_Upgrades.GridOrigin.y + (double) ITab_Vehicle_Upgrades.GridSpacing.y * (double) coord.z - (double) ITab_Vehicle_Upgrades.GridSpacing.y / 2.0 + 20.0);
    float num3 = drawSize.x / 2f;
    float num4 = drawSize.y / 2f;
    return new Vector2(num1 - num3, num2 - num4);
  }

  protected virtual void FillTab()
  {
    if (this.Vehicle == null)
      return;
    Rect rect1;
    // ISSUE: explicit constructor call
    ((Rect) ref rect1).\u002Ector(0.0f, 20f, ((InspectTabBase) this).size.x, ((InspectTabBase) this).size.y - 20f);
    Rect rect2 = GenUI.ContractedBy(rect1, 5f);
    if (DebugSettings.ShowDevGizmos)
    {
      string str = TaggedString.op_Implicit(Translator.Translate("VF_DevMode_DebugDrawUpgradeNodeGrid"));
      Vector2 vector2 = Text.CalcSize(str);
      Rect rect3;
      // ISSUE: explicit constructor call
      ((Rect) ref rect3).\u002Ector(((Rect) ref rect1).xMax - 80f - vector2.x, 0.0f, vector2.x + 30f, vector2.y);
      bool debugDrawNodeGrid = VehicleMod.settings.debug.debugDrawNodeGrid;
      Widgets.CheckboxLabeled(rect3, str, ref debugDrawNodeGrid, false, (Texture2D) null, (Texture2D) null, false, false);
      VehicleMod.settings.debug.debugDrawNodeGrid = debugDrawNodeGrid;
    }
    this.highlightedNode = (UpgradeNode) null;
    this.DrawGrid(rect2);
    Rect rect4;
    // ISSUE: explicit constructor call
    ((Rect) ref rect4).\u002Ector(((Rect) ref rect2).x, 5f, ((Rect) ref rect2).width - 16f, 20f);
    Widgets.Label(rect4, ((Entity) this.Vehicle).Label);
    if (VehicleMod.settings.debug.debugDrawNodeGrid)
      return;
    TextBlock textBlock;
    // ISSUE: explicit constructor call
    ((TextBlock) ref textBlock).\u002Ector(ITab_Vehicle_Upgrades.GridLineColor);
    try
    {
      Widgets.DrawLineHorizontal(((Rect) ref rect2).x, ((Rect) ref rect2).y, ((Rect) ref rect2).width);
      Widgets.DrawLineHorizontal(((Rect) ref rect2).x, ((Rect) ref rect2).yMax, ((Rect) ref rect2).width);
      Widgets.DrawLineVertical(((Rect) ref rect2).x, ((Rect) ref rect2).y, ((Rect) ref rect2).height);
      Widgets.DrawLineVertical(((Rect) ref rect2).xMax, ((Rect) ref rect2).y, ((Rect) ref rect2).height);
    }
    finally
    {
      textBlock.Dispose();
    }
  }

  private void DrawButtons(Rect rect)
  {
    if (this.Vehicle.CompUpgradeTree.NodeUnlocking == this.SelectedNode)
    {
      if (!Widgets.ButtonText(rect, TaggedString.op_Implicit(Translator.Translate("CancelButton")), true, true, true, new TextAnchor?()))
        return;
      this.Vehicle.CompUpgradeTree.ClearUpgrade();
      this.SelectedNode = (UpgradeNode) null;
    }
    else if (this.Vehicle.CompUpgradeTree.NodeUnlocked(this.SelectedNode) && this.Vehicle.CompUpgradeTree.LastNodeUnlocked(this.SelectedNode))
    {
      if (!Widgets.ButtonText(rect, TaggedString.op_Implicit(Translator.Translate("VF_RemoveUpgrade")), true, true, true, new TextAnchor?()))
        return;
      SoundStarter.PlayOneShotOnCamera(SoundDefOf.Click, ((Thing) this.Vehicle).Map);
      if (DebugSettings.godMode)
        this.Vehicle.CompUpgradeTree.ResetUnlock(this.SelectedNode);
      else
        this.Vehicle.CompUpgradeTree.RemoveUnlock(this.SelectedNode);
      this.SelectedNode = (UpgradeNode) null;
    }
    else
    {
      if (!Widgets.ButtonText(rect, TaggedString.op_Implicit(Translator.Translate("VF_Upgrade")), true, true, true, new TextAnchor?()) || this.Vehicle.CompUpgradeTree.NodeUnlocked(this.SelectedNode))
        return;
      if (this.Vehicle.CompUpgradeTree.Disabled(this.SelectedNode))
        Messages.Message(TaggedString.op_Implicit(Translator.Translate("VF_DisabledFromOtherNode")), MessageTypeDefOf.RejectInput, false);
      else if (this.Vehicle.CompUpgradeTree.PrerequisitesMet(this.SelectedNode))
      {
        SoundStarter.PlayOneShotOnCamera(SoundDefOf.ExecuteTrade, ((Thing) this.Vehicle).Map);
        if (DebugSettings.godMode)
        {
          this.Vehicle.CompUpgradeTree.FinishUnlock(this.SelectedNode);
          SoundStarter.PlayOneShot(SoundDefOf.Building_Complete, SoundInfo.op_Implicit((Thing) this.Vehicle));
        }
        else
          this.Vehicle.CompUpgradeTree.StartUnlock(this.SelectedNode);
        this.SelectedNode = (UpgradeNode) null;
      }
      else
        Messages.Message(TaggedString.op_Implicit(Translator.Translate("VF_MissingPrerequisiteUpgrade")), MessageTypeDefOf.RejectInput, false);
    }
  }

  private static void DrawBackgroundGridTop()
  {
    for (int index = 0; index <= ITab_Vehicle_Upgrades.MaxLinesAcross; ++index)
    {
      TextBlock textBlock;
      // ISSUE: explicit constructor call
      ((TextBlock) ref textBlock).\u002Ector(ITab_Vehicle_Upgrades.GridLineColor);
      try
      {
        Widgets.DrawLineVertical(ITab_Vehicle_Upgrades.GridSpacing.x + ITab_Vehicle_Upgrades.GridSpacing.x * (float) index, 20f + ITab_Vehicle_Upgrades.GridSpacing.y, (float) ITab_Vehicle_Upgrades.MaxLinesDown * ITab_Vehicle_Upgrades.GridSpacing.y);
        if (index % 5 == 0)
        {
          GUI.color = Color.white;
          Widgets.Label(new Rect((float) ((double) ITab_Vehicle_Upgrades.GridSpacing.x + (double) ITab_Vehicle_Upgrades.GridSpacing.x * (double) index - 5.0), 20f, ITab_Vehicle_Upgrades.GridSpacing.x, ITab_Vehicle_Upgrades.GridSpacing.y), index.ToString());
        }
      }
      finally
      {
        textBlock.Dispose();
      }
    }
  }

  private static void DrawBackgroundGridLeft()
  {
    for (int index = 0; index <= ITab_Vehicle_Upgrades.MaxLinesDown; ++index)
    {
      TextBlock textBlock;
      // ISSUE: explicit constructor call
      ((TextBlock) ref textBlock).\u002Ector(ITab_Vehicle_Upgrades.GridLineColor);
      try
      {
        Widgets.DrawLineHorizontal(ITab_Vehicle_Upgrades.GridSpacing.x, (float) (20.0 + (double) ITab_Vehicle_Upgrades.GridSpacing.y * (double) (index + 1)), (float) ITab_Vehicle_Upgrades.MaxLinesAcross * ITab_Vehicle_Upgrades.GridSpacing.x);
        if (index % 5 == 0)
        {
          GUI.color = Color.white;
          Widgets.Label(new Rect(ITab_Vehicle_Upgrades.GridSpacing.x - 20f, (float) (20.0 + (double) ITab_Vehicle_Upgrades.GridSpacing.y * (double) (index + 1) - (double) ITab_Vehicle_Upgrades.GridSpacing.y / 2.0), ITab_Vehicle_Upgrades.GridSpacing.x, ITab_Vehicle_Upgrades.GridSpacing.y), index.ToString());
        }
      }
      finally
      {
        textBlock.Dispose();
      }
    }
  }

  private void DrawNodeSelection()
  {
    if (this.SelectedNode == (UpgradeNode) null || this.Vehicle.CompUpgradeTree.Upgrading)
      return;
    Vector2 screenPosAdjusted = this.GridCoordinateToScreenPosAdjusted(this.SelectedNode.GridCoordinate, this.SelectedNode.drawSize);
    Rect rect;
    // ISSUE: explicit constructor call
    ((Rect) ref rect).\u002Ector(screenPosAdjusted, this.SelectedNode.drawSize);
    rect = GenUI.ExpandedBy(rect, 2f);
    GUI.DrawTexture(rect, (Texture) BaseContent.WhiteTex);
  }

  private void DrawPrerequisites()
  {
    TextBlock textBlock;
    // ISSUE: explicit constructor call
    ((TextBlock) ref textBlock).\u002Ector(Color.white);
    try
    {
      foreach (UpgradeNode node in this.Vehicle.CompUpgradeTree.Props.def.nodes)
      {
        UpgradeNode upgradeNode = node;
        if (!GenList.NullOrEmpty<string>((IList<string>) upgradeNode.prerequisiteNodes) && !upgradeNode.hidden)
        {
          foreach (UpgradeNode upgradeNode1 in this.Vehicle.CompUpgradeTree.Props.def.nodes.FindAll((Predicate<UpgradeNode>) (prereqNode => !prereqNode.hidden && upgradeNode.prerequisiteNodes.Contains(prereqNode.key))))
          {
            Vector2 screenPos1 = this.GridCoordinateToScreenPos(upgradeNode.GridCoordinate);
            Vector2 screenPos2 = this.GridCoordinateToScreenPos(upgradeNode1.GridCoordinate);
            Color color = this.Vehicle.CompUpgradeTree.NodeUnlocked(upgradeNode) ? Color.white : ITab_Vehicle_Upgrades.DisabledLineColor;
            foreach (UpgradeNode disablerNode in this.GetDisablerNodes(upgradeNode))
            {
              Rect rect;
              // ISSUE: explicit constructor call
              ((Rect) ref rect).\u002Ector(this.GridCoordinateToScreenPosAdjusted(disablerNode.GridCoordinate, disablerNode.drawSize), upgradeNode1.drawSize);
              if (!this.Vehicle.CompUpgradeTree.Upgrading && Mouse.IsOver(rect))
                color = Color.red;
            }
            Widgets.DrawLine(screenPos1, screenPos2, color, 2f);
          }
        }
      }
    }
    finally
    {
      textBlock.Dispose();
    }
  }

  private void DrawUpgradeNodes(Rect rect)
  {
    Rect rect1 = this.InfoNode != (UpgradeNode) null ? this.GetDetailRect(rect) : Rect.zero;
    foreach (UpgradeNode node in this.Vehicle.CompUpgradeTree.Props.def.nodes)
    {
      if (!node.hidden)
      {
        TextBlock textBlock;
        // ISSUE: explicit constructor call
        ((TextBlock) ref textBlock).\u002Ector(Color.white);
        try
        {
          Rect rect2 = new Rect(this.GridCoordinateToScreenPosAdjusted(node.GridCoordinate, node.drawSize), node.drawSize);
          bool colored = false;
          if (this.Vehicle.CompUpgradeTree.Disabled(node) || !this.Vehicle.CompUpgradeTree.PrerequisitesMet(node))
          {
            colored = true;
            GUI.color = ITab_Vehicle_Upgrades.DisabledColor;
          }
          Widgets.DrawTextureFitted(rect2, (Texture) Command.BGTex, 1f, 1f);
          Widgets.DrawTextureFitted(rect2, (Texture) node.UpgradeImage, 1f, 1f);
          this.DrawNodeCondition(rect2, node, colored);
          if (node.displayLabel)
          {
            float x = Text.CalcSize(node.label).x;
            Rect rect3;
            // ISSUE: explicit constructor call
            ((Rect) ref rect3).\u002Ector(((Rect) ref rect2).x - (float) (((double) x - (double) ((Rect) ref rect2).width) / 2.0), ((Rect) ref rect2).y - 20f, 10f * (float) node.label.Length, 25f);
            Widgets.Label(rect3, node.label);
          }
          if (Mouse.IsOver(rect2))
          {
            this.highlightedNode = node;
            if (this.Vehicle.CompUpgradeTree.PrerequisitesMet(node) && !this.Vehicle.CompUpgradeTree.NodeUnlocked(node))
              GUI.DrawTexture(rect2, (Texture) TexUI.HighlightTex);
          }
          if (this.InfoNode != (UpgradeNode) null)
          {
            if (!Mouse.IsOver(rect1))
            {
              if (Widgets.ButtonInvisible(rect2, true))
              {
                if (this.SelectedNode != node)
                {
                  this.SelectedNode = node;
                  SoundStarter.PlayOneShotOnCamera(SoundDefOf.Checkbox_TurnedOn, (Map) null);
                }
                else
                {
                  this.SelectedNode = (UpgradeNode) null;
                  SoundStarter.PlayOneShotOnCamera(SoundDefOf.Checkbox_TurnedOff, (Map) null);
                }
              }
            }
          }
        }
        finally
        {
          textBlock.Dispose();
        }
      }
    }
    if (!(this.InfoNode != (UpgradeNode) null))
      return;
    Widgets.BeginGroup(rect1);
    this.DrawInfoPanel(GenUI.AtZero(rect1));
    Widgets.EndGroup();
  }

  private void DrawGrid(Rect rect)
  {
    if (DebugSettings.ShowDevGizmos && VehicleMod.settings.debug.debugDrawNodeGrid)
    {
      ITab_Vehicle_Upgrades.DrawBackgroundGridTop();
      ITab_Vehicle_Upgrades.DrawBackgroundGridLeft();
    }
    this.DrawNodeSelection();
    this.DrawPrerequisites();
    this.DrawUpgradeNodes(rect);
  }

  private Rect GetDetailRect(Rect rect, float padding = 5f)
  {
    Vector2 screenPosAdjusted = this.GridCoordinateToScreenPosAdjusted(this.InfoNode.GridCoordinate, this.InfoNode.drawSize);
    Rect rect1;
    // ISSUE: explicit constructor call
    ((Rect) ref rect1).\u002Ector(screenPosAdjusted, this.InfoNode.drawSize);
    float num1 = 0.0f;
    if (!GenList.NullOrEmpty<ThingDefCountClass>((IList<ThingDefCountClass>) this.InfoNode.ingredients))
      num1 = (float) this.InfoNode.ingredients.Count * 20f;
    float num2 = (float) (375.0 - (double) padding * 2.0);
    TextBlock textBlock;
    // ISSUE: explicit constructor call
    ((TextBlock) ref textBlock).\u002Ector((GameFont) 2);
    try
    {
      float num3 = Text.CalcHeight(this.InfoNode.label, num2);
      Text.Font = (GameFont) 1;
      float num4 = Text.CalcHeight(this.InfoNode.description, num2);
      float num5 = (float) ((double) num3 + (double) num4 + (double) num1 + 30.0) + padding;
      if (this.SelectedNode != (UpgradeNode) null)
      {
        num5 += 10f;
        if (this.SelectedNode.HasGraphics)
          num5 += 165f;
        if (!GenList.NullOrEmpty<UpgradeTextEntry>((IList<UpgradeTextEntry>) this.textEntries))
          num5 += this.textEntryHeight + 5f;
      }
      float num6 = ((Rect) ref rect1).x + this.InfoNode.drawSize.x + padding;
      if ((double) num6 + (double) num2 > (double) ((Rect) ref rect).xMax - (double) padding)
        num6 = Mathf.Clamp(((Rect) ref rect1).x - num2 - padding, ((Rect) ref rect).x + padding, ((Rect) ref rect).xMax - num2 - padding);
      float num7 = Mathf.Max(150f, num5);
      float num8 = Mathf.Clamp((float) ((double) ((Rect) ref rect1).y + (double) this.InfoNode.drawSize.y / 2.0 - (double) num7 / 2.0), ((Rect) ref rect).y + padding, ((Rect) ref rect).yMax - num7 - padding);
      return new Rect(num6, num8, num2, num7);
    }
    finally
    {
      textBlock.Dispose();
    }
  }

  private void DrawInfoPanel(Rect rect, float padding = 5f)
  {
    Widgets.DrawMenuSection(rect);
    Rect rect1 = GenUI.ContractedBy(rect, padding);
    TextBlock textBlock;
    // ISSUE: explicit constructor call
    ((TextBlock) ref textBlock).\u002Ector((GameFont) 2);
    try
    {
      float num1 = Text.CalcHeight(this.InfoNode.label, ((Rect) ref rect1).width);
      Rect rect2 = new Rect(((Rect) ref rect1).x, ((Rect) ref rect1).y, ((Rect) ref rect1).width, num1);
      Widgets.Label(rect2, this.InfoNode.label);
      Text.Font = (GameFont) 1;
      Rect rect3 = new Rect(((Rect) ref rect1).x, ((Rect) ref rect2).yMax, ((Rect) ref rect1).width, ((Rect) ref rect1).height - ((Rect) ref rect2).height);
      float num2 = this.DrawCostItems(rect3);
      Rect rect4 = rect3;
      ((Rect) ref rect4).height = num2;
      float num3 = Text.CalcHeight(this.InfoNode.description, ((Rect) ref rect1).width);
      Rect rect5 = new Rect(((Rect) ref rect1).x, ((Rect) ref rect3).y + num2, ((Rect) ref rect1).width, num3);
      Widgets.Label(rect5, this.InfoNode.description);
      if (!(this.SelectedNode != (UpgradeNode) null))
        return;
      bool hasGraphics = this.SelectedNode.HasGraphics;
      bool flag = !GenList.NullOrEmpty<Upgrade>((IList<Upgrade>) this.SelectedNode.upgrades);
      if (hasGraphics | flag)
        Widgets.DrawLineHorizontal(((Rect) ref rect).x, ((Rect) ref rect5).yMax + 5f, ((Rect) ref rect).width, UIElements.MenuSectionBgBorderColor);
      Rect rect6;
      // ISSUE: explicit constructor call
      ((Rect) ref rect6).\u002Ector(((Rect) ref rect1).x, ((Rect) ref rect5).yMax + 10f, ((Rect) ref rect1).width, this.textEntryHeight);
      int num4 = flag ? 1 : 0;
      if (hasGraphics)
      {
        Rect rect7;
        // ISSUE: explicit constructor call
        ((Rect) ref rect7).\u002Ector(((Rect) ref rect1).x, ((Rect) ref rect6).yMax + 5f, ((Rect) ref rect1).width, (float) (((double) ((Rect) ref rect1).width - 45.0) / 2.0));
        this.DrawVehicleGraphicComparison(rect7);
      }
      Rect rect8;
      // ISSUE: explicit constructor call
      ((Rect) ref rect8).\u002Ector(((Rect) ref rect1).xMax - 125f, ((Rect) ref rect1).yMax - 30f, 120f, 30f);
      this.DrawButtons(rect8);
      Rect rect9 = rect8;
      ((Rect) ref rect9).width = ((Rect) ref rect1).width;
    }
    finally
    {
      textBlock.Dispose();
    }
  }

  private void DrawVehicleGraphicComparison(Rect rect)
  {
    Rect rect1 = GenUI.ContractedBy(rect, 5f);
    Rect rect2;
    // ISSUE: explicit constructor call
    ((Rect) ref rect2).\u002Ector(((Rect) ref rect1).x, ((Rect) ref rect1).y, ((Rect) ref rect1).height, ((Rect) ref rect1).height);
    float num = 45f;
    Rect rect3;
    // ISSUE: explicit constructor call
    ((Rect) ref rect3).\u002Ector(((Rect) ref rect2).xMax + 5f, (float) ((double) ((Rect) ref rect2).y + (double) ((Rect) ref rect2).height / 2.0 - (double) num / 2.0), num, num);
    Rect rect4;
    // ISSUE: explicit constructor call
    ((Rect) ref rect4).\u002Ector(((Rect) ref rect3).xMax + 5f, ((Rect) ref rect2).y, ((Rect) ref rect2).width, ((Rect) ref rect2).height);
    BlitRequest request1 = BlitRequest.For(this.Vehicle);
    VehicleGui.DrawVehicleOnGUI(rect2, in request1);
    BlitRequest request2 = new BlitRequest(this.Vehicle);
    request2.blitTargets.Add((IBlitTarget) this.Vehicle.VehicleDef);
    List<GraphicOverlay> overlays = this.Vehicle.CompUpgradeTree.Props.TryGetOverlays(this.InfoNode);
    if (!GenList.NullOrEmpty<GraphicOverlay>((IList<GraphicOverlay>) overlays))
      request2.blitTargets.AddRange((IEnumerable<IBlitTarget>) overlays);
    if (!GenList.NullOrEmpty<GraphicOverlay>((IList<GraphicOverlay>) this.Vehicle.DrawTracker.overlayRenderer.AllOverlaysListForReading))
      request2.blitTargets.AddRange((IEnumerable<IBlitTarget>) this.Vehicle.DrawTracker.overlayRenderer.AllOverlaysListForReading);
    if (!GenList.NullOrEmpty<VehicleTurret>((IList<VehicleTurret>) this.renderTurrets))
      request2.blitTargets.AddRange((IEnumerable<IBlitTarget>) this.renderTurrets);
    CompVehicleTurrets cachedComp = this.Vehicle.GetCachedComp<CompVehicleTurrets>();
    if (cachedComp != null && !cachedComp.Turrets.NullOrEmpty<VehicleTurret>())
      request2.blitTargets.AddRange((IEnumerable<IBlitTarget>) cachedComp.Turrets.Where<VehicleTurret>((Func<VehicleTurret, bool>) (turret => this.excludeTurrets == null || !this.excludeTurrets.Contains(turret.key))));
    VehicleGui.DrawVehicleOnGUI(rect4, in request2);
    Widgets.DrawTextureFitted(rect3, (Texture) TexData.TutorArrowRight, 1f, 1f);
  }

  private void DrawSubIconsBar(Rect rect)
  {
    Rect rect1;
    // ISSUE: explicit constructor call
    ((Rect) ref rect1).\u002Ector(((Rect) ref rect).xMax - 24f, ((Rect) ref rect).y, ((Rect) ref rect).width, 24f);
    Color color1 = !this.showDetails ? Color.white : Color.green;
    Color color2 = !this.showDetails ? GenUI.MouseoverColor : new Color(0.0f, 0.5f, 0.0f);
    Rect rect2;
    // ISSUE: explicit constructor call
    ((Rect) ref rect2).\u002Ector(((Rect) ref rect1).x, ((Rect) ref rect1).y, 24f, 24f);
    if (GenList.NullOrEmpty<Upgrade>((IList<Upgrade>) this.InfoNode.upgrades))
      return;
    if (Widgets.ButtonImageFitted(rect2, TexButton.Info, color1, color2))
    {
      this.showDetails = !this.showDetails;
      if (this.showDetails)
        SoundStarter.PlayOneShotOnCamera(SoundDefOf.TabOpen, (Map) null);
      else
        SoundStarter.PlayOneShotOnCamera(SoundDefOf.TabClose, (Map) null);
    }
    ref Rect local = ref rect2;
    ((Rect) ref local).x = ((Rect) ref local).x - ((Rect) ref rect2).width;
  }

  protected virtual void UpdateSize()
  {
    ((InspectTabBase) this).UpdateSize();
    if (Mathf.Approximately(((InspectTabBase) this).size.x, 880f) && Mathf.Approximately(((InspectTabBase) this).size.y, 520f))
      return;
    ((InspectTabBase) this).size = new Vector2(880f, 520f);
  }

  private float DrawCostItems(Rect rect)
  {
    Rect rect1 = rect;
    float num = 0.0f;
    foreach (ThingDefCountClass ingredient in this.InfoNode.ingredients)
    {
      TextBlock textBlock;
      // ISSUE: explicit constructor call
      ((TextBlock) ref textBlock).\u002Ector(Color.white);
      try
      {
        Rect rect2 = new Rect(((Rect) ref rect1).x, ((Rect) ref rect1).y + num, 20f, 20f);
        GUI.DrawTexture(rect2, (Texture) ((BuildableDef) ingredient.thingDef).uiIcon);
        string str = TaggedString.op_Implicit(((Def) ingredient.thingDef).LabelCap);
        if (((Thing) this.Vehicle).Map.resourceCounter.GetCount(ingredient.thingDef) < ingredient.count)
          GUI.color = Color.red;
        Rect rect3 = new Rect(((Rect) ref rect2).x + 26f, ((Rect) ref rect2).y, 50f, 20f);
        Widgets.Label(rect3, $"{ingredient.count}");
        Widgets.Label(new Rect(((Rect) ref rect2).x + 60f, ((Rect) ref rect3).y, (float) ((double) ((Rect) ref rect1).width - (double) ((Rect) ref rect3).width - 25.0), 20f), str);
        num += 20f;
      }
      finally
      {
        textBlock.Dispose();
      }
    }
    return num;
  }

  private void DrawUpgradeList(Rect rect)
  {
    if (this.InfoNode.upgradeExplanation != null)
    {
      Widgets.Label(rect, this.InfoNode.upgradeExplanation);
    }
    else
    {
      if (GenList.NullOrEmpty<Upgrade>((IList<Upgrade>) this.InfoNode.upgrades))
        return;
      TextBlock textBlock1;
      // ISSUE: explicit constructor call
      ((TextBlock) ref textBlock1).\u002Ector((GameFont) 1, (TextAnchor) 3);
      try
      {
        foreach (Upgrade upgrade in this.InfoNode.upgrades)
        {
          foreach (UpgradeTextEntry upgradeTextEntry in upgrade.UpgradeDescription(this.Vehicle))
          {
            TextBlock textBlock2;
            // ISSUE: explicit constructor call
            ((TextBlock) ref textBlock2).\u002Ector(Color.white);
            try
            {
              float num1 = ((Rect) ref rect).width * 0.75f;
              float num2 = Text.CalcHeight(upgradeTextEntry.label, num1);
              float num3 = ((Rect) ref rect).width - num1;
              float num4 = Text.CalcHeight(upgradeTextEntry.description, num3);
              float num5 = Mathf.Max(num2, num4);
              Rect rect1 = new Rect(((Rect) ref rect).x, ((Rect) ref rect).y, num1, num5);
              Widgets.Label(rect1, upgradeTextEntry.label);
              Color color;
              switch (upgradeTextEntry.effectType)
              {
                case UpgradeEffectType.Positive:
                  color = ITab_Vehicle_Upgrades.EffectColorPositive;
                  break;
                case UpgradeEffectType.Negative:
                  color = ITab_Vehicle_Upgrades.EffectColorNegative;
                  break;
                case UpgradeEffectType.Neutral:
                  color = ITab_Vehicle_Upgrades.EffectColorNeutral;
                  break;
                default:
                  color = Color.white;
                  break;
              }
              GUI.color = color;
              Widgets.Label(new Rect(((Rect) ref rect1).xMax, ((Rect) ref rect).y, num3, num5), upgradeTextEntry.description);
              ref Rect local = ref rect;
              ((Rect) ref local).y = ((Rect) ref local).y + num5;
            }
            finally
            {
              textBlock2.Dispose();
            }
          }
        }
      }
      finally
      {
        textBlock1.Dispose();
      }
    }
  }

  private void DrawNodeCondition(Rect rect, UpgradeNode node, bool colored)
  {
    if (DisabledAndMouseOver())
      ITab_Vehicle_Upgrades.DrawBorder(rect, Color.red);
    else if (this.Vehicle.CompUpgradeTree.NodeUnlocking == node)
      ITab_Vehicle_Upgrades.DrawBorder(rect, ITab_Vehicle_Upgrades.UpgradingColor);
    else if (this.Vehicle.CompUpgradeTree.NodeUnlocked(node))
      ITab_Vehicle_Upgrades.DrawBorder(rect, Color.white);
    else if (!colored)
      Widgets.DrawBoxSolid(rect, ITab_Vehicle_Upgrades.NotUpgradedColor);
    else
      ITab_Vehicle_Upgrades.DrawBorder(rect, ITab_Vehicle_Upgrades.NotUpgradedColor);

    bool DisabledAndMouseOver()
    {
      foreach (UpgradeNode disablerNode in this.GetDisablerNodes(node))
      {
        Rect rect;
        // ISSUE: explicit constructor call
        ((Rect) ref rect).\u002Ector(this.GridCoordinateToScreenPosAdjusted(disablerNode.GridCoordinate, disablerNode.drawSize), disablerNode.drawSize);
        if (!this.Vehicle.CompUpgradeTree.Upgrading && Mouse.IsOver(rect))
          return true;
      }
      return false;
    }
  }

  private static void DrawBorder(Rect rect, Color color)
  {
    Vector2 vector2_1;
    // ISSUE: explicit constructor call
    ((Vector2) ref vector2_1).\u002Ector(((Rect) ref rect).x, ((Rect) ref rect).y);
    Vector2 vector2_2;
    // ISSUE: explicit constructor call
    ((Vector2) ref vector2_2).\u002Ector(((Rect) ref rect).xMax, ((Rect) ref rect).y);
    Vector2 vector2_3;
    // ISSUE: explicit constructor call
    ((Vector2) ref vector2_3).\u002Ector(((Rect) ref rect).x, ((Rect) ref rect).yMax);
    Vector2 vector2_4;
    // ISSUE: explicit constructor call
    ((Vector2) ref vector2_4).\u002Ector(((Rect) ref rect).xMax, ((Rect) ref rect).yMax);
    Vector2 vector2_5;
    // ISSUE: explicit constructor call
    ((Vector2) ref vector2_5).\u002Ector(((Rect) ref rect).x, ((Rect) ref rect).y + 2f);
    Vector2 vector2_6;
    // ISSUE: explicit constructor call
    ((Vector2) ref vector2_6).\u002Ector(((Rect) ref rect).x, ((Rect) ref rect).yMax + 2f);
    Vector2 vector2_7;
    // ISSUE: explicit constructor call
    ((Vector2) ref vector2_7).\u002Ector(((Rect) ref rect).xMax, ((Rect) ref rect).y + 2f);
    Vector2 vector2_8;
    // ISSUE: explicit constructor call
    ((Vector2) ref vector2_8).\u002Ector(((Rect) ref rect).xMax, ((Rect) ref rect).yMax + 2f);
    Widgets.DrawLine(vector2_1, vector2_2, color, 1f);
    Widgets.DrawLine(vector2_3, vector2_4, color, 1f);
    Widgets.DrawLine(vector2_5, vector2_6, color, 1f);
    Widgets.DrawLine(vector2_7, vector2_8, color, 1f);
  }
}
