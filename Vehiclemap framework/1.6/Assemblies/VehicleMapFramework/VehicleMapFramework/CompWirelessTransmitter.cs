// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.CompWirelessTransmitter
// Assembly: VehicleMapFramework, Version=1.6.562.0, Culture=neutral, PublicKeyToken=null
// MVID: 10A61882-945F-4CFC-9B06-CA8EEF5ADB36
// Assembly location: D:\Programas\steamapps\workshop\content\294100\3426502333\1.6\Assemblies\VehicleMapFramework.dll

using RimWorld;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using Verse;

#nullable disable
namespace VehicleMapFramework;

public class CompWirelessTransmitter : CompPowerNetLink, IThingGlower
{
  protected const float PushMin = 0.0f;
  protected const float PushMax = 5000f;
  private static readonly CachedTexture PowerLowerTex = new CachedTexture("VehicleMapFramework/UI/PowerLower");
  private static readonly CachedTexture PowerRaiseTex = new CachedTexture("VehicleMapFramework/UI/PowerRaise");
  private static readonly CachedTexture PowerResetTex = new CachedTexture("VehicleMapFramework/UI/PowerReset");
  private static readonly CachedTexture Push = new CachedTexture("VehicleMapFramework/UI/Push");
  private static readonly CachedTexture Draw = new CachedTexture("VehicleMapFramework/UI/Draw");
  private static readonly CachedTexture Transmit = new CachedTexture("VehicleMapFramework/UI/Transmit");
  private readonly Color DrawColor;
  private readonly Color PushColor;
  private readonly Color TransmitColor;
  protected float maxPushSetting;
  protected CompPowerNetLink.PowerTransferMode mode;

  public CompProperties_WirelessTransmitter Props
  {
    get => (CompProperties_WirelessTransmitter) ((ThingComp) this).props;
  }

  protected override float Radius => this.Props.radius;

  protected override float MaxPowerPush => this.maxPushSetting;

  protected override float PowerLossFactor => this.Props.powerLossFactor;

  protected override CompPowerNetLink.PowerTransferMode Mode => this.mode;

  bool IThingGlower.ShouldBeLitNow() => (double) this.PowerOutput != 0.0;

  protected override bool TryFindConnection(out CompPowerNetLink linkTo)
  {
    int num = GenRadial.NumCellsInRadius(this.Radius);
    IntVec3 position = ((Thing) ((ThingComp) this).parent).Position;
    for (int index = 0; index < num; ++index)
    {
      foreach (Thing thingListAcrossMap in IntVec3.op_Addition(position, GenRadial.RadialPattern[index]).GetThingListAcrossMaps(((Thing) ((ThingComp) this).parent).Map))
      {
        CompPowerNetLink other;
        if (thingListAcrossMap.Map != ((Thing) ((ThingComp) this).parent).Map && ThingCompUtility.TryGetComp<CompPowerNetLink>(thingListAcrossMap, ref other) && this.CanLinkTo(other))
        {
          linkTo = other;
          return true;
        }
      }
    }
    linkTo = (CompPowerNetLink) null;
    return false;
  }

  public virtual IEnumerable<Gizmo> CompGetGizmosExtra()
  {
    CompWirelessTransmitter wirelessTransmitter = this;
    // ISSUE: reference to a compiler-generated method
    foreach (Gizmo gizmo in wirelessTransmitter.\u003C\u003En__0())
      yield return gizmo;
    Command_Action commandAction1 = new Command_Action();
    // ISSUE: reference to a compiler-generated method
    commandAction1.action = new Action(wirelessTransmitter.\u003CCompGetGizmosExtra\u003Eb__25_0);
    Command_Action commandAction2 = commandAction1;
    TaggedString taggedString1;
    switch (wirelessTransmitter.mode)
    {
      case CompPowerNetLink.PowerTransferMode.Push:
        taggedString1 = Translator.Translate("VMF_PowerPush");
        break;
      case CompPowerNetLink.PowerTransferMode.Draw:
        taggedString1 = Translator.Translate("VMF_PowerDraw");
        break;
      default:
        taggedString1 = Translator.Translate("VMF_PowerTransmit");
        break;
    }
    ((Command) commandAction2).defaultLabel = TaggedString.op_Implicit(taggedString1);
    Command_Action commandAction3 = commandAction1;
    TaggedString taggedString2;
    switch (wirelessTransmitter.mode)
    {
      case CompPowerNetLink.PowerTransferMode.Push:
        taggedString2 = Translator.Translate("VMF_PowerPushDesc");
        break;
      case CompPowerNetLink.PowerTransferMode.Draw:
        taggedString2 = Translator.Translate("VMF_PowerDrawDesc");
        break;
      default:
        taggedString2 = Translator.Translate("VMF_PowerTransmitDesc");
        break;
    }
    ((Command) commandAction3).defaultDesc = TaggedString.op_Implicit(taggedString2);
    Command_Action commandAction4 = commandAction1;
    Texture2D texture;
    switch (wirelessTransmitter.mode)
    {
      case CompPowerNetLink.PowerTransferMode.Push:
        texture = CompWirelessTransmitter.Push.Texture;
        break;
      case CompPowerNetLink.PowerTransferMode.Draw:
        texture = CompWirelessTransmitter.Draw.Texture;
        break;
      default:
        texture = CompWirelessTransmitter.Transmit.Texture;
        break;
    }
    ((Command) commandAction4).icon = (Texture) texture;
    yield return (Gizmo) commandAction1;
    if (wirelessTransmitter.mode != CompPowerNetLink.PowerTransferMode.Draw)
    {
      Command_Action commandAction5 = new Command_Action();
      // ISSUE: reference to a compiler-generated method
      commandAction5.action = new Action(wirelessTransmitter.\u003CCompGetGizmosExtra\u003Eb__25_1);
      ((Command) commandAction5).defaultLabel = "-1000W";
      ((Command) commandAction5).defaultDesc = TaggedString.op_Implicit(Translator.Translate("VMF_LowerPowerDesc"));
      ((Command) commandAction5).hotKey = KeyBindingDefOf.Misc5;
      ((Command) commandAction5).icon = (Texture) CompWirelessTransmitter.PowerLowerTex.Texture;
      yield return (Gizmo) commandAction5;
      Command_Action commandAction6 = new Command_Action();
      // ISSUE: reference to a compiler-generated method
      commandAction6.action = new Action(wirelessTransmitter.\u003CCompGetGizmosExtra\u003Eb__25_2);
      ((Command) commandAction6).defaultLabel = "-100W";
      ((Command) commandAction6).defaultDesc = TaggedString.op_Implicit(Translator.Translate("VMF_LowerPowerDesc"));
      ((Command) commandAction6).hotKey = KeyBindingDefOf.Misc4;
      ((Command) commandAction6).icon = (Texture) CompWirelessTransmitter.PowerLowerTex.Texture;
      yield return (Gizmo) commandAction6;
      Command_Action commandAction7 = new Command_Action();
      // ISSUE: reference to a compiler-generated method
      commandAction7.action = new Action(wirelessTransmitter.\u003CCompGetGizmosExtra\u003Eb__25_3);
      ((Command) commandAction7).defaultLabel = TaggedString.op_Implicit(Translator.Translate("VMF_ResetPower"));
      ((Command) commandAction7).defaultDesc = TaggedString.op_Implicit(Translator.Translate("VMF_ResetPowerDesc"));
      ((Command) commandAction7).hotKey = KeyBindingDefOf.Misc1;
      ((Command) commandAction7).icon = (Texture) CompWirelessTransmitter.PowerResetTex.Texture;
      yield return (Gizmo) commandAction7;
      Command_Action commandAction8 = new Command_Action();
      // ISSUE: reference to a compiler-generated method
      commandAction8.action = new Action(wirelessTransmitter.\u003CCompGetGizmosExtra\u003Eb__25_4);
      ((Command) commandAction8).defaultLabel = "+100W";
      ((Command) commandAction8).defaultDesc = TaggedString.op_Implicit(Translator.Translate("VMF_RaisePowerDesc"));
      ((Command) commandAction8).hotKey = KeyBindingDefOf.Misc2;
      ((Command) commandAction8).icon = (Texture) CompWirelessTransmitter.PowerRaiseTex.Texture;
      yield return (Gizmo) commandAction8;
      Command_Action commandAction9 = new Command_Action();
      // ISSUE: reference to a compiler-generated method
      commandAction9.action = new Action(wirelessTransmitter.\u003CCompGetGizmosExtra\u003Eb__25_5);
      ((Command) commandAction9).defaultLabel = "+1000W";
      ((Command) commandAction9).defaultDesc = TaggedString.op_Implicit(Translator.Translate("VMF_RaisePowerDesc"));
      ((Command) commandAction9).hotKey = KeyBindingDefOf.Misc3;
      ((Command) commandAction9).icon = (Texture) CompWirelessTransmitter.PowerRaiseTex.Texture;
      yield return (Gizmo) commandAction9;
    }
  }

  public virtual void PostDraw()
  {
    ((ThingComp) this).PostDraw();
    if (!this.Connected)
      return;
    Graphic graphic = this.Props.lightGraphic?.Graphic;
    if (graphic == null)
      return;
    VehiclePawnWithMap vehicle;
    ((double) this.PowerOutput != 0.0 ? graphic : graphic.GetColoredVersion(graphic.Shader, GenColor.WithAlpha(graphic.Color, 0.5f), graphic.ColorTwo)).Draw(Vector3Utility.WithYOffset(((Thing) ((ThingComp) this).parent).DrawPos, (float) (0.03658536821603775 / (VehicleMapUtility.get_IsOnNonFocusedVehicleMap((Thing) ((ThingComp) this).parent) ? 10.0 : 1.0))), ((Thing) ((ThingComp) this).parent).Rotation, (Thing) ((ThingComp) this).parent, ((Thing) ((ThingComp) this).parent).IsOnNonFocusedVehicleMapOf(out vehicle) ? -vehicle.Angle : 0.0f);
  }

  public virtual void PostDrawExtraSelectionOverlays()
  {
    ((ThingComp) this).PostDrawExtraSelectionOverlays();
    GenDraw.DrawRadiusRing(((Thing) ((ThingComp) this).parent).Position, this.Props.radius);
  }

  public override void PostExposeData()
  {
    base.PostExposeData();
    Scribe_Values.Look<CompPowerNetLink.PowerTransferMode>(ref this.mode, "mode", CompPowerNetLink.PowerTransferMode.Transmit, false);
    Scribe_Values.Look<float>(ref this.maxPushSetting, "maxPushSetting", 500f, false);
  }

  public virtual string CompInspectStringExtra()
  {
    return base.CompInspectStringExtra() + "\n" + $"{Translator.Translate("VMF_PowerTransferSetting")}: {this.maxPushSetting} W";
  }

  [Conditional("DEV")]
  public void SetMode(CompPowerNetLink.PowerTransferMode _mode) => this.mode = _mode;

  [Conditional("DEV")]
  public void SetMaxPush(float max) => this.maxPushSetting = max;

  public CompWirelessTransmitter()
  {
    ColorInt colorInt1 = new ColorInt(47, 207, 0);
    this.DrawColor = ((ColorInt) ref colorInt1).ToColor;
    ColorInt colorInt2 = new ColorInt(215, 90, 0);
    this.PushColor = ((ColorInt) ref colorInt2).ToColor;
    ColorInt colorInt3 = new ColorInt(0, 198, 208 /*0xD0*/);
    this.TransmitColor = ((ColorInt) ref colorInt3).ToColor;
    this.maxPushSetting = 500f;
    this.mode = CompPowerNetLink.PowerTransferMode.Transmit;
    // ISSUE: explicit constructor call
    base.\u002Ector();
  }
}
