// Decompiled with JetBrains decompiler
// Type: Vehicles.Rendering.Command_CooldownAction
// Assembly: Vehicles, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 81BFDC99-D8AD-42E1-8470-2F73E7836B4A
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\Vehicles.dll

using JetBrains.Annotations;
using RimWorld;
using SmashTools;
using System;
using System.Linq;
using UnityEngine;
using Vehicles.Config;
using Verse;
using Verse.Sound;

#nullable disable
namespace Vehicles.Rendering;

[PublicAPI]
public class Command_CooldownAction : Command_Turret
{
  private const float IdlerTimeExpiry = 5f;
  protected const float TurretGizmoPadding = 5f;
  protected const float GizmoHeight = 75f;
  protected const float ConfigureButtonSize = 20f;
  protected const float SubIconSize = 33.3333321f;
  protected const float CooldownBarWidth = 18f;
  protected const float PaddingBetweenElements = 2f;

  protected override float RecalculateWidth()
  {
    float num = 75f;
    if (this.turret.CanOverheat)
      num += 23f;
    return num + (float) this.turret.SubGizmos.Count<VehicleTurret.SubGizmo>() * 33.3333321f;
  }

  public override void FireTurret(VehicleTurret turret)
  {
    if (turret.ReloadTicks > 0)
      return;
    Vector3 vector3 = turret.TurretLocation.PointFromAngle(turret.MaxRange, turret.TurretRotation);
    turret.SetTarget(LocalTargetInfo.op_Implicit(IntVec3Utility.ToIntVec3(vector3)));
    turret.PushTurretToQueue();
    turret.ResetPrefireTimer();
  }

  public virtual GizmoResult GizmoOnGUI(Vector2 topLeft, float maxWidth, GizmoRenderParms parms)
  {
    TextBlock textBlock;
    // ISSUE: explicit constructor call
    ((TextBlock) ref textBlock).\u002Ector((GameFont) 0);
    try
    {
      Rect rect1 = new Rect(topLeft.x, topLeft.y, ((Gizmo) this).GetWidth(maxWidth), 75f);
      Material grayscaleGui = ((Gizmo) this).disabled ? TexUI.GrayscaleGUI : (Material) null;
      Material cooldownMaterial = this.turret.OnCooldown ? TexUI.GrayscaleGUI : grayscaleGui;
      Widgets.DrawWindowBackground(rect1);
      Rect rect2 = GenUI.ContractedBy(rect1, 5f);
      Rect rect3 = new Rect(((Rect) ref rect2).x, ((Rect) ref rect2).y, ((Rect) ref rect2).height, ((Rect) ref rect2).height);
      bool mouseOver;
      bool fireTurret;
      bool haltTurret;
      float num1 = this.DrawGizmoButton(rect3, cooldownMaterial, out mouseOver, out bool _, out fireTurret, out haltTurret);
      Text.Font = (GameFont) 1;
      Rect rect4 = new Rect((float) ((double) ((Rect) ref rect3).x + (double) num1 + 5.0), ((Rect) ref rect3).y, 18f, ((Rect) ref rect3).height);
      float num2 = this.DrawCooldownBar(rect4);
      float num3 = ((Rect) ref rect3).height / 2f;
      float num4 = ((Rect) ref rect2).width - (float) ((double) num1 + (double) num2 + 5.0);
      Rect rect5 = new Rect(((Rect) ref rect4).x + num2, ((Rect) ref rect3).y, num4, num3);
      VehicleTurret.SubGizmo subGizmo = this.DrawTopBar(rect5, ref mouseOver);
      this.DrawBottomBar(new Rect(((Rect) ref rect5).x, ((Rect) ref rect5).y + num3, ((Rect) ref rect5).width, num3), ref mouseOver);
      if (!GenText.NullOrEmpty(this.LabelCap))
      {
        using (new TextBlock((GameFont) 0))
        {
          float num5 = ((Rect) ref rect3).width + 10f;
          float num6 = Text.CalcHeight(this.LabelCap, num5);
          Rect rect6;
          // ISSUE: explicit constructor call
          ((Rect) ref rect6).\u002Ector(((Rect) ref rect3).x, (float) ((double) ((Rect) ref rect2).yMax - (double) num6 + 12.0), num5, num6);
          GUI.DrawTexture(rect6, (Texture) TexUI.GrayTextBG);
          Text.Anchor = (TextAnchor) 1;
          Widgets.Label(rect6, this.LabelCap);
        }
      }
      if (this.DoTooltip)
      {
        string str = this.Desc;
        if (!GenText.NullOrEmpty(((Gizmo) this).disabledReason))
          str = $"{this.Desc}\n\n{Translator.Translate("DisabledCommand")}: {((Gizmo) this).disabledReason}";
        TooltipHandler.TipRegion(rect3, TipSignal.op_Implicit(str));
      }
      if (!GenText.NullOrEmpty(this.HighlightTag) && (Find.WindowStack.FloatMenu == null || !((Rect) ref ((Window) Find.WindowStack.FloatMenu).windowRect).Overlaps(rect3)))
        UIHighlighter.HighlightOpportunity(rect3, this.HighlightTag);
      KeyBindingDef hotKey = this.hotKey;
      if (hotKey != null && hotKey.MainKey != null && !GizmoGridDrawer.drawnHotKeys.Contains(hotKey.MainKey))
      {
        Rect rect7;
        // ISSUE: explicit constructor call
        ((Rect) ref rect7).\u002Ector(((Rect) ref rect3).x + 5f, ((Rect) ref rect2).y + 5f, ((Rect) ref rect3).width - 10f, 18f);
        Widgets.Label(rect7, GenText.ToStringReadable(hotKey.MainKey));
        GizmoGridDrawer.drawnHotKeys.Add(hotKey.MainKey);
        if (this.hotKey.KeyDownEvent)
        {
          fireTurret = true;
          Event.current.Use();
        }
      }
      if ((object) subGizmo != null && subGizmo.IsValid)
        subGizmo.onClick();
      if (haltTurret)
      {
        if (((Gizmo) this).disabled)
        {
          if (!GenText.NullOrEmpty(((Gizmo) this).disabledReason))
            Messages.Message(((Gizmo) this).disabledReason, MessageTypeDefOf.RejectInput, false);
          return new GizmoResult((GizmoState) 1);
        }
        this.turret.SetTarget(LocalTargetInfo.Invalid);
        return new GizmoResult((GizmoState) 0);
      }
      if (!fireTurret || this.turret.OnCooldown)
        return new GizmoResult(mouseOver ? (GizmoState) 1 : (GizmoState) 0);
      if (((Gizmo) this).disabled)
      {
        if (!GenText.NullOrEmpty(((Gizmo) this).disabledReason))
          Messages.Message(((Gizmo) this).disabledReason, MessageTypeDefOf.RejectInput, false);
        return new GizmoResult((GizmoState) 1);
      }
      if (!TutorSystem.AllowAction(EventPack.op_Implicit(this.TutorTagSelect)))
        return new GizmoResult((GizmoState) 1);
      GizmoResult gizmoResult;
      // ISSUE: explicit constructor call
      ((GizmoResult) ref gizmoResult).\u002Ector((GizmoState) 2, Event.current);
      TutorSystem.Notify_Event(EventPack.op_Implicit(this.TutorTagSelect));
      return gizmoResult;
    }
    finally
    {
      textBlock.Dispose();
    }
  }

  protected virtual float DrawGizmoButton(
    Rect rect,
    Material cooldownMaterial,
    out bool mouseOver,
    out bool ammoLoaded,
    out bool fireTurret,
    out bool haltTurret)
  {
    rect = GenUI.ContractedBy(rect, 2f);
    ammoLoaded = true;
    mouseOver = false;
    fireTurret = false;
    Widgets.BeginGroup(rect);
    Rect rect1 = GenUI.AtZero(rect);
    Rect rect2;
    // ISSUE: explicit constructor call
    ((Rect) ref rect2).\u002Ector(((Rect) ref rect1).xMax - 33.3333321f, ((Rect) ref rect1).y, 33.3333321f, 33.3333321f);
    if ((this.turret.loadedAmmo == null || this.turret.shellCount <= 0) && this.turret.def.ammunition != null)
    {
      ((Gizmo) this).Disable(TaggedString.op_Implicit(Translator.Translate("VF_NoAmmoLoadedTurret")));
      ammoLoaded = false;
    }
    else
    {
      ThingDef projectileDef = this.turret.ProjectileDef;
      if (projectileDef != null)
      {
        ProjectileProperties projectile = projectileDef.projectile;
        if (projectile != null && projectile.flyOverhead && GridsUtility.Roofed(((Thing) this.vehicle).Position, ((Thing) this.vehicle).Map))
        {
          TaggedString taggedString1 = (object) Translator.Translate("CannotFire");
          TaggedString taggedString2 = Translator.Translate("Roofed");
          TaggedString taggedString3 = (object) ((TaggedString) ref taggedString2).CapitalizeFirst();
          ((Gizmo) this).Disable($"{taggedString1}: {taggedString3}");
          goto label_10;
        }
      }
      if (!this.turret.OnCooldown && Mouse.IsOver(rect1) && (!Mouse.IsOver(rect2) || !((LocalTargetInfo) ref this.turret.targetInfo).IsValid))
      {
        if (!((Gizmo) this).disabled)
          GUI.color = GenUI.MouseoverColor;
        mouseOver = true;
        this.turret.GizmoHighlighted = true;
      }
      else
        this.turret.GizmoHighlighted = false;
    }
label_10:
    GenUI.DrawTextureWithMaterial(rect1, (Texture) Command.BGTex, cooldownMaterial, new Rect());
    MouseoverSounds.DoRegion(rect1, SoundDefOf.Mouseover_Command);
    GUI.color = this.IconDrawColor;
    this.DrawVehicleTurret(rect1);
    if (!ammoLoaded)
      Widgets.DrawBoxSolid(rect1, this.DarkGrey);
    if (this.turret.ReloadTicks > 0)
    {
      float fillPercent = (float) this.turret.ReloadTicks / (float) this.turret.MaxTicks;
      UIElements.VerticalFillableBar(rect1, fillPercent, UIData.FillableBarTexture, UIData.ClearBarTexture);
    }
    if (this.DrawSubIcons(rect2, cooldownMaterial, out haltTurret))
    {
      mouseOver = false;
      fireTurret = false;
    }
    else if (ammoLoaded && Widgets.ButtonInvisible(rect1, true))
      fireTurret = true;
    Widgets.EndGroup();
    return ((Rect) ref rect).width;
  }

  private void DrawVehicleTurret(Rect rect)
  {
    TextBlock textBlock;
    // ISSUE: explicit constructor call
    ((TextBlock) ref textBlock).\u002Ector(Color.white);
    try
    {
      Rect rect1 = GenUI.ContractedBy(rect, 2f);
      if (!GenText.NullOrEmpty(this.turret.def.gizmoIconTexPath))
      {
        Widgets.BeginGroup(rect1);
        UIElements.DrawTextureWithMaterialOnGUI(GenUI.ExpandedBy(GenUI.AtZero(rect1), ((Rect) ref rect1).width * (float) ((double) this.turret.def.gizmoIconScale * (double) this.iconDrawScale - 1.0)), (Texture) this.turret.GizmoIcon, (Material) null, 0.0f, new Rect());
        Widgets.EndGroup();
      }
      else
      {
        BlitRequest request = new BlitRequest(this.vehicle)
        {
          rot = Rot8.North,
          iconFrame = true
        };
        request.blitTargets.Add((IBlitTarget) this.turret);
        IntVec2 size = this.vehicle.VehicleDef.size;
        float num = size.x > size.z ? (float) size.x : (float) size.z;
        VehicleGui.DrawVehicleOnGUI(rect1, in request, this.turret.def.gizmoIconScale * this.iconDrawScale, true);
      }
    }
    finally
    {
      textBlock.Dispose();
    }
  }

  protected virtual bool DrawSubIcons(Rect rect, Material material, out bool haltTurret)
  {
    bool flag = false;
    haltTurret = false;
    if (this.turret.OnCooldown)
      GenUI.DrawTextureWithMaterial(rect, (Texture) this.turret.FireIcon, CompFireOverlay.FireGraphic.MatAt(Rot4.North, (Thing) null), new Rect());
    else if (((LocalTargetInfo) ref this.turret.targetInfo).IsValid)
    {
      if (!((Gizmo) this).disabled && Widgets.ButtonInvisible(rect, true))
      {
        SoundStarter.PlayOneShotOnCamera(SoundDefOf.Click, (Map) null);
        haltTurret = true;
      }
      if (!((Gizmo) this).disabled && Mouse.IsOver(rect))
      {
        flag = true;
        GUI.color = GenUI.MouseoverColor;
      }
      GenUI.DrawTextureWithMaterial(rect, (Texture) VehicleTex.HaltIcon, material, new Rect());
    }
    return flag;
  }

  protected virtual float DrawCooldownBar(Rect rect)
  {
    if (!this.turret.CanOverheat)
      return 0.0f;
    float num = this.turret.currentHeatRate / 100f;
    UIElements.VerticalFillableBar(rect, num, TexData.HeatColorPercent(num), BaseContent.BlackTex, true);
    return ((Rect) ref rect).width + 5f;
  }

  protected virtual VehicleTurret.SubGizmo DrawTopBar(Rect rect, ref bool mouseOver)
  {
    VehicleTurret.SubGizmo subGizmo1 = (VehicleTurret.SubGizmo) null;
    Rect rect1;
    // ISSUE: explicit constructor call
    ((Rect) ref rect1).\u002Ector(((Rect) ref rect).x, ((Rect) ref rect).y, ((Rect) ref rect).height, ((Rect) ref rect).height);
    foreach (VehicleTurret.SubGizmo subGizmo2 in this.turret.SubGizmos)
    {
      TextBlock textBlock;
      // ISSUE: explicit constructor call
      ((TextBlock) ref textBlock).\u002Ector(Color.white);
      try
      {
        if (!((Gizmo) this).disabled)
        {
          TooltipHandler.TipRegion(rect1, TipSignal.op_Implicit(subGizmo2.tooltip));
          if (subGizmo2.canClick() && Mouse.IsOver(rect1))
          {
            mouseOver = true;
            GUI.color = GenUI.SubtleMouseoverColor;
            if (Widgets.ButtonInvisible(rect1, true))
              subGizmo1 = subGizmo2;
          }
        }
        subGizmo2.drawGizmo(rect1);
        ref Rect local = ref rect1;
        ((Rect) ref local).x = ((Rect) ref local).x + 33.3333321f;
      }
      finally
      {
        textBlock.Dispose();
      }
    }
    return subGizmo1;
  }

  protected virtual void DrawBottomBar(Rect rect, ref bool mouseOver)
  {
    Widgets.FillableBar(rect, (float) this.turret.shellCount / (float) this.turret.def.magazineCapacity, TexData.FullBarTex, TexData.EmptyBarTex, true);
    TextBlock textBlock;
    // ISSUE: explicit constructor call
    ((TextBlock) ref textBlock).\u002Ector((GameFont) 1, (TextAnchor) 4);
    try
    {
      string str = $"{this.turret.shellCount:F0} / {this.turret.def.magazineCapacity:F0}";
      if (this.turret.def.magazineCapacity <= 0)
      {
        str = "∞";
        Text.Font = (GameFont) 2;
      }
      Widgets.Label(rect, str);
    }
    finally
    {
      textBlock.Dispose();
    }
    if (this.turret.def.ammunition == null)
      return;
    Rect rect1;
    // ISSUE: explicit constructor call
    ((Rect) ref rect1).\u002Ector(((Rect) ref rect).xMax - 20f, ((Rect) ref rect).yMax - 20f, 20f, 20f);
    TooltipHandler.TipRegionByKey(rect1, "VF_SetReloadLevelTooltip");
    if (Mouse.IsOver(rect1))
      mouseOver = true;
    if (!Widgets.ButtonImageFitted(rect1, VehicleTex.Settings))
      return;
    if (FeatureFlags.IsFeatureEnabled("BetterAutoLoadConfig"))
    {
      Find.WindowStack.Add((Window) new Dialog_ConfigureTurret(this.turret));
    }
    else
    {
      ThingDef thingDef = this.turret.loadedAmmo ?? this.turret.def.ammunition.AllowedThingDefs.FirstOrDefault<ThingDef>();
      Find.WindowStack.Add((Window) new Dialog_Slider(new Func<int, string>(TextLabel), 0, Mathf.RoundToInt(this.vehicle.GetStatValue(VehicleStatDefOf.CargoCapacity) / StatExtension.GetStatValueAbstract((BuildableDef) thingDef, StatDefOf.Mass, (ThingDef) null)), (Action<int>) (value => this.vehicle.CompVehicleTurrets.SetQuotaLevel(this.turret, value)), this.vehicle.CompVehicleTurrets.GetQuotaLevel(this.turret), 5f));
    }

    static string TextLabel(int amount)
    {
      return TaggedString.op_Implicit(TranslatorFormattedStringExtensions.Translate("VF_SetReloadLevel", NamedArgument.op_Implicit(amount)));
    }
  }
}
