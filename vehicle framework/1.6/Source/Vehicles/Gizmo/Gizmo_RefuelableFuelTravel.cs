// Decompiled with JetBrains decompiler
// Type: Vehicles.Rendering.Gizmo_RefuelableFuelTravel
// Assembly: Vehicles, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 81BFDC99-D8AD-42E1-8470-2F73E7836B4A
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\Vehicles.dll

using RimWorld;
using SmashTools;
using System;
using System.Text;
using UnityEngine;
using Verse;
using Verse.Sound;
using Verse.Steam;

#nullable disable
namespace Vehicles.Rendering;

[StaticConstructorOnStartup]
public class Gizmo_RefuelableFuelTravel : Gizmo_Slider
{
  private const float FuelIconSize = 24f;
  private static readonly StringBuilder TooltipBuilder = new StringBuilder();
  private readonly CompFueledTravel refuelable;
  private readonly bool showVehicleLabel;
  private float fuelAvailable;
  private bool refuelFromInventoryDisabled;
  private string refuelFromInventoryDisabledReason;

  public Gizmo_RefuelableFuelTravel(CompFueledTravel refuelable, bool showVehicleLabel)
  {
    this.refuelable = refuelable;
    this.showVehicleLabel = showVehicleLabel;
    ((Gizmo) this).Order = -100f;
  }

  protected virtual float Target
  {
    get => this.refuelable.TargetFuelPercent;
    set => this.refuelable.TargetFuelPercent = value;
  }

  protected virtual float ValuePercent => this.refuelable.FuelPercent;

  protected virtual string Title
  {
    get
    {
      return !this.showVehicleLabel ? this.refuelable.Props.GizmoLabel : ((Entity) this.refuelable.Vehicle).LabelCap;
    }
  }

  protected virtual bool IsDraggable => !this.refuelable.Props.ElectricPowered;

  protected virtual string BarLabel
  {
    get
    {
      return $"{GenText.ToStringDecimalIfSmall(this.refuelable.Fuel)} / {GenText.ToStringDecimalIfSmall(this.refuelable.FuelCapacity)}";
    }
  }

  private KeyBindingDef KeyBindDef
  {
    get
    {
      return this.refuelable.Props.ElectricPowered ? KeyBindingDefOf.Command_TogglePower : KeyBindingDefOf.Command_ItemForbid;
    }
  }

  protected virtual bool DraggingBar { get; set; }

  protected virtual string GetTooltip()
  {
    return $"{this.refuelable.TargetFuelLevel:F0} {((Def) this.refuelable.Props.fuelType).LabelCap}";
  }

  private void UpdateDisableStatus()
  {
    this.refuelFromInventoryDisabled = false;
    this.refuelFromInventoryDisabledReason = (string) null;
    this.fuelAvailable = 0.0f;
    if (Mathf.Approximately(this.refuelable.FuelPercent, 1f))
    {
      this.refuelFromInventoryDisabled = true;
      this.refuelFromInventoryDisabledReason = TaggedString.op_Implicit(TranslatorFormattedStringExtensions.Translate("VF_VehicleFullyFueled", NamedArgument.op_Implicit(((Entity) this.refuelable.Vehicle).LabelCap)));
    }
    else
    {
      this.fuelAvailable = FuelInVehicle(this.refuelable.Vehicle);
      if ((double) this.fuelAvailable == 0.0)
      {
        this.refuelFromInventoryDisabled = true;
        this.refuelFromInventoryDisabledReason = TaggedString.op_Implicit(TranslatorFormattedStringExtensions.Translate("VF_NoFuelInVehicle", NamedArgument.op_Implicit(((Entity) this.refuelable.Vehicle).LabelCap)));
      }
      if (!this.refuelable.Vehicle.InAerialVehicle())
        return;
      this.refuelFromInventoryDisabled = true;
      this.refuelFromInventoryDisabledReason = TaggedString.op_Implicit(TranslatorFormattedStringExtensions.Translate("VF_CantRefuelWhileFlying", NamedArgument.op_Implicit(((Entity) this.refuelable.Vehicle).LabelCap)));
    }

    static float FuelInVehicle(VehiclePawn vehicle)
    {
      float num = 0.0f;
      foreach (Thing thing in CompFueledTravel.AllFuelFromInventory(vehicle))
        num += (float) thing.stackCount;
      return num;
    }
  }

  public virtual GizmoResult GizmoOnGUI(Vector2 topLeft, float maxWidth, GizmoRenderParms parms)
  {
    if (SteamDeck.IsSteamDeckInNonKeyboardMode)
      return base.GizmoOnGUI(topLeft, maxWidth, parms);
    this.UpdateDisableStatus();
    KeyCode mainKey = this.KeyBindDef.MainKey;
    if (mainKey != null && !GizmoGridDrawer.drawnHotKeys.Contains(mainKey) && this.KeyBindDef.KeyDownEvent)
    {
      this.ToggleSwitch();
      Event.current.Use();
    }
    return base.GizmoOnGUI(topLeft, maxWidth, parms);
  }

  protected virtual void DrawHeader(Rect headerRect, ref bool mouseOverElement)
  {
    ref Rect local1 = ref headerRect;
    ((Rect) ref local1).xMax = ((Rect) ref local1).xMax - 24f;
    Rect rect1;
    // ISSUE: explicit constructor call
    ((Rect) ref rect1).\u002Ector(((Rect) ref headerRect).xMax, ((Rect) ref headerRect).y, 24f, 24f);
    bool electricPowered = this.refuelable.Props.ElectricPowered;
    Texture texture = electricPowered ? (Texture) TexData.FlickerIcon : (Texture) ((BuildableDef) ThingDefOf.Chemfuel).uiIcon;
    GUI.DrawTexture(rect1, texture);
    Rect rect2;
    // ISSUE: explicit constructor call
    ((Rect) ref rect2).\u002Ector(((Rect) ref rect1).center.x, ((Rect) ref rect1).y, ((Rect) ref rect1).width / 2f, ((Rect) ref rect1).height / 2f);
    bool flag = electricPowered ? this.refuelable.Charging : this.refuelable.allowAutoRefuel;
    GUI.DrawTexture(rect2, flag ? (Texture) Widgets.CheckboxOnTex : (Texture) Widgets.CheckboxOffTex);
    if (Widgets.ButtonInvisible(rect1, true))
      this.ToggleAutoRefuel();
    if (Mouse.IsOver(rect1))
    {
      Widgets.DrawHighlight(rect1);
      TooltipHandler.TipRegion(rect1, electricPowered ? new Func<string>(this.PowerNetTip) : new Func<string>(this.RefuelTip), electricPowered ? "PowerNetTip".GetHashCode() : "RefuelTip".GetHashCode());
      mouseOverElement = true;
    }
    if (!electricPowered)
    {
      ref Rect local2 = ref rect1;
      ((Rect) ref local2).x = ((Rect) ref local2).x - (((Rect) ref rect1).width + 4f);
      Rect rect3 = GenUI.ExpandedBy(rect1, 3f);
      ref Rect local3 = ref rect3;
      ((Rect) ref local3).y = ((Rect) ref local3).y + 2f;
      GenUI.DrawTextureWithMaterial(rect3, (Texture) VehicleTex.RefuelFromCargo, this.refuelFromInventoryDisabled ? TexUI.GrayscaleGUI : (Material) null, new Rect());
      if (Widgets.ButtonInvisible(rect1, true))
      {
        if (this.refuelFromInventoryDisabled)
        {
          Messages.Message(this.refuelFromInventoryDisabledReason, MessageTypeDefOf.RejectInput, true);
          return;
        }
        int num = Mathf.FloorToInt(Mathf.Min(this.fuelAvailable, this.refuelable.FuelCapacity - this.refuelable.Fuel));
        if (num < 1)
          num = 1;
        Find.WindowStack.Add((Window) new Dialog_Slider((Func<int, string>) (count => TaggedString.op_Implicit(TranslatorFormattedStringExtensions.Translate("VF_RefuelFromInventoryCount", NamedArgument.op_Implicit(count), NamedArgument.op_Implicit(((Def) this.refuelable.Props.fuelType).label)))), 1, num, new Action<int>(this.refuelable.ConsumeFuelFromInventory), int.MinValue, 1f));
      }
      if (Mouse.IsOver(rect1))
      {
        Widgets.DrawHighlight(rect1);
        TooltipHandler.TipRegion(rect1, new Func<string>(this.RefuelFromInventoryTip), "RefuelFromInventoryTip".GetHashCode());
        mouseOverElement = true;
      }
    }
    base.DrawHeader(headerRect, ref mouseOverElement);
  }

  private void ToggleSwitch()
  {
    if (this.refuelable.Props.ElectricPowered)
      this.ToggleCharging();
    else
      this.ToggleAutoRefuel();
  }

  private void ToggleAutoRefuel()
  {
    this.refuelable.allowAutoRefuel = !this.refuelable.allowAutoRefuel;
    if (this.refuelable.allowAutoRefuel)
      SoundStarter.PlayOneShotOnCamera(SoundDefOf.Tick_High, (Map) null);
    else
      SoundStarter.PlayOneShotOnCamera(SoundDefOf.Tick_Low, (Map) null);
  }

  private void ToggleCharging()
  {
    if (!this.refuelable.Charging)
    {
      if (this.refuelable.TryConnectPower())
        SoundStarter.PlayOneShotOnCamera(SoundDefOf.Tick_High, (Map) null);
      else
        SoundStarter.PlayOneShotOnCamera(SoundDefOf.ClickReject, (Map) null);
    }
    else
    {
      this.refuelable.DisconnectPower();
      SoundStarter.PlayOneShotOnCamera(SoundDefOf.Tick_Low, (Map) null);
    }
  }

  private string PowerNetTip()
  {
    using (new ClearStringOnDispose(Gizmo_RefuelableFuelTravel.TooltipBuilder))
    {
      Gizmo_RefuelableFuelTravel.TooltipBuilder.AppendLine(TaggedString.op_Implicit(Translator.Translate("VF_ElectricFlick")));
      Gizmo_RefuelableFuelTravel.TooltipBuilder.AppendLine();
      Gizmo_RefuelableFuelTravel.TooltipBuilder.AppendLine();
      Gizmo_RefuelableFuelTravel.TooltipBuilder.AppendLine(TaggedString.op_Implicit(TranslatorFormattedStringExtensions.Translate("VF_ElectricFlickDesc", NamedArgument.op_Implicit(GenText.UncapitalizeFirst(GenText.ToStringYesNo(this.refuelable.Charging))))));
      Gizmo_RefuelableFuelTravel.TooltipBuilder.AppendLine();
      Gizmo_RefuelableFuelTravel.TooltipBuilder.AppendLine();
      Gizmo_RefuelableFuelTravel.TooltipBuilder.AppendLine($"{Translator.Translate("HotKeyTip")}: {GenText.ToStringReadable(KeyPrefs.KeyPrefsData.GetBoundKeyCode(KeyBindingDefOf.Command_TogglePower, (KeyPrefs.BindingSlot) 0))}");
      return Gizmo_RefuelableFuelTravel.TooltipBuilder.ToString();
    }
  }

  private string RefuelTip()
  {
    using (new ClearStringOnDispose(Gizmo_RefuelableFuelTravel.TooltipBuilder))
    {
      Gizmo_RefuelableFuelTravel.TooltipBuilder.AppendLine(TaggedString.op_Implicit(Translator.Translate("CommandToggleAllowAutoRefuel")));
      Gizmo_RefuelableFuelTravel.TooltipBuilder.AppendLine();
      Gizmo_RefuelableFuelTravel.TooltipBuilder.AppendLine();
      StringBuilder tooltipBuilder = Gizmo_RefuelableFuelTravel.TooltipBuilder;
      TaggedString taggedString = TranslatorFormattedStringExtensions.Translate("CommandToggleAllowAutoRefuelDesc", NamedArgument.op_Implicit(ColoredText.Colorize(this.refuelable.TargetFuelLevel.ToString("F0"), ColoredText.TipSectionTitleColor)), NamedArgumentUtility.Named((object) GenText.UncapitalizeFirst(this.refuelable.allowAutoRefuel ? Translator.TranslateSimple("On") : Translator.TranslateSimple("Off")), "ONOFF"));
      string str = ((TaggedString) ref taggedString).Resolve();
      tooltipBuilder.AppendLine(str);
      Gizmo_RefuelableFuelTravel.TooltipBuilder.AppendLine();
      Gizmo_RefuelableFuelTravel.TooltipBuilder.AppendLine();
      Gizmo_RefuelableFuelTravel.TooltipBuilder.AppendLine($"{Translator.Translate("HotKeyTip")}: {GenText.ToStringReadable(KeyPrefs.KeyPrefsData.GetBoundKeyCode(KeyBindingDefOf.Command_ItemForbid, (KeyPrefs.BindingSlot) 0))}");
      return Gizmo_RefuelableFuelTravel.TooltipBuilder.ToString();
    }
  }

  private string RefuelFromInventoryTip()
  {
    using (new ClearStringOnDispose(Gizmo_RefuelableFuelTravel.TooltipBuilder))
    {
      Gizmo_RefuelableFuelTravel.TooltipBuilder.AppendLine(TaggedString.op_Implicit(TranslatorFormattedStringExtensions.Translate("VF_RefuelFromInventoryDesc", NamedArgument.op_Implicit(((Entity) this.refuelable.Vehicle).LabelCap))));
      return Gizmo_RefuelableFuelTravel.TooltipBuilder.ToString();
    }
  }
}
