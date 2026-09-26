// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.CompPowerNetLink
// Assembly: VehicleMapFramework, Version=1.6.562.0, Culture=neutral, PublicKeyToken=null
// MVID: 10A61882-945F-4CFC-9B06-CA8EEF5ADB36
// Assembly location: D:\Programas\steamapps\workshop\content\294100\3426502333\1.6\Assemblies\VehicleMapFramework.dll

using RimWorld;
using System;
using System.Diagnostics;
using System.Linq;
using UnityEngine;
using Verse;

#nullable disable
namespace VehicleMapFramework;

public abstract class CompPowerNetLink : CompPowerTrader
{
  private const float BatteryDischargingWatts = 5f;
  private ThingWithComps linkedTo;

  public bool Connected => this.LinkedTo != null && this.LinkedComp != null;

  protected abstract float Radius { get; }

  protected abstract float MaxPowerPush { get; }

  protected abstract float PowerLossFactor { get; }

  protected ThingWithComps LinkedTo
  {
    get => this.linkedTo;
    set
    {
      this.linkedTo = value;
      ThingWithComps linkedTo = this.linkedTo;
      this.LinkedComp = linkedTo != null ? ThingCompUtility.TryGetComp<CompPowerNetLink>((Thing) linkedTo) : (CompPowerNetLink) null;
    }
  }

  public CompPowerNetLink LinkedComp { get; private set; }

  protected abstract CompPowerNetLink.PowerTransferMode Mode { get; }

  public virtual int UpdateRateIntervalTicks => !this.Connected ? 180 : 60;

  public virtual void CompTick()
  {
    if (!((Thing) ((ThingComp) this).parent).Spawned)
      return;
    ((ThingComp) this).CompTick();
    int thingIdNumber1 = ((Thing) ((ThingComp) this).parent).thingIDNumber;
    int? thingIdNumber2 = ((Thing) this.LinkedTo)?.thingIDNumber;
    int valueOrDefault = thingIdNumber2.GetValueOrDefault();
    if (!GenTicks.IsTickInterval(thingIdNumber1 < valueOrDefault & thingIdNumber2.HasValue ? this.UpdateRateIntervalTicks / 2 : 0, this.UpdateRateIntervalTicks))
      return;
    if (this.Connected)
    {
      if (VehicleMapUtility.get_BaseMapOrCaravan((Thing) ((ThingComp) this).parent) != VehicleMapUtility.get_BaseMapOrCaravan((Thing) this.LinkedTo) || (double) GenGeo.MagnitudeHorizontalSquared(Vector3.op_Subtraction(((Thing) this.LinkedTo).DrawPos, ((Thing) ((ThingComp) this).parent).DrawPos)) > (double) this.Radius * (double) this.Radius)
      {
        this.Disconnect();
      }
      else
      {
        float powerOutput = this.PowerOutput;
        switch (this.Mode)
        {
          case CompPowerNetLink.PowerTransferMode.Push:
            this.PowerOutput = -CompPowerNetLink.PushAmount(this, this.LinkedComp) - ((CompPower) this).Props.PowerConsumption;
            break;
          case CompPowerNetLink.PowerTransferMode.Draw:
            this.LinkedComp.PowerOutput = -CompPowerNetLink.PushAmount(this.LinkedComp, this) - ((CompPower) this.LinkedComp).Props.PowerConsumption;
            break;
          case CompPowerNetLink.PowerTransferMode.Transmit:
            float num = CompPowerNetLink.TransmitAmount(this, this.LinkedComp);
            if ((double) num <= 0.0)
            {
              if ((double) num >= 0.0)
              {
                if ((double) num == 0.0)
                {
                  this.PowerOutput = -((CompPower) this).Props.PowerConsumption;
                  this.LinkedComp.PowerOutput = -((CompPower) this.LinkedComp).Props.PowerConsumption;
                  break;
                }
                break;
              }
              this.LinkedComp.PowerOutput = num - ((CompPower) this.LinkedComp).Props.PowerConsumption;
              break;
            }
            this.PowerOutput = -num - ((CompPower) this).Props.PowerConsumption;
            break;
          default:
            throw new ArgumentOutOfRangeException();
        }
        if ((double) this.PowerOutput < 0.0 && this.PowerOn)
          this.LinkedComp.PowerOutput = (-this.PowerOutput - ((CompPower) this).Props.PowerConsumption) * this.PowerLossFactor - ((CompPower) this.LinkedComp).Props.PowerConsumption;
        if (((double) powerOutput != 0.0 || (double) this.PowerOutput == 0.0) && ((double) this.PowerOutput != 0.0 || (double) powerOutput == 0.0))
          return;
        ((ThingComp) this).parent.BroadcastCompSignal("PowerTurnedOn");
      }
    }
    else
    {
      CompPowerNetLink linkTo;
      if (!this.TryFindConnection(out linkTo))
        return;
      this.Connect(linkTo);
    }
  }

  public virtual bool CanLinkTo(CompPowerNetLink other)
  {
    bool flag = !this.Connected && !other.Connected;
    if (flag)
    {
      CompPowerNetLink.PowerTransferMode mode = other.Mode;
      CompPowerNetLink.PowerTransferMode powerTransferMode;
      switch (this.Mode)
      {
        case CompPowerNetLink.PowerTransferMode.Push:
          powerTransferMode = CompPowerNetLink.PowerTransferMode.Draw;
          break;
        case CompPowerNetLink.PowerTransferMode.Draw:
          powerTransferMode = CompPowerNetLink.PowerTransferMode.Push;
          break;
        default:
          powerTransferMode = CompPowerNetLink.PowerTransferMode.Transmit;
          break;
      }
      flag = mode == powerTransferMode;
    }
    return flag;
  }

  protected abstract bool TryFindConnection(out CompPowerNetLink linkTo);

  public virtual void Connect(CompPowerNetLink other)
  {
    this.LinkedTo = ((ThingComp) other).parent;
    other.LinkedTo = ((ThingComp) this).parent;
  }

  public virtual void Disconnect()
  {
    CompPowerNetLink linkedComp1 = this.LinkedComp;
    if (linkedComp1 != null)
      linkedComp1.PowerOutput = 0.0f;
    CompPowerNetLink linkedComp2 = this.LinkedComp;
    if (linkedComp2 != null)
      linkedComp2.LinkedTo = (ThingWithComps) null;
    this.PowerOutput = 0.0f;
    this.LinkedTo = (ThingWithComps) null;
    ((ThingComp) this).parent.BroadcastCompSignal("PowerTurnedOff");
  }

  protected static float PushAmount(CompPowerNetLink pusher, CompPowerNetLink drawer)
  {
    float num1 = (float) ((CompPower) drawer).PowerNet.batteryComps.Count * 5f;
    double num2 = (double) CompPowerNetLink.PowerNetNeeds(((CompPower) drawer).PowerNet, (CompPowerTrader) drawer) + (double) CompPowerNetLink.PowerNetBatteryAccepts(((CompPower) drawer).PowerNet) / (double) CompPower.WattsToWattDaysPerTick;
    float num3 = Mathf.Max(0.0f, (float) (-(double) CompPowerNetLink.PowerNetNeeds(((CompPower) pusher).PowerNet, (CompPowerTrader) pusher) + (double) ((CompPower) pusher).PowerNet.CurrentStoredEnergy() / (double) CompPower.WattsToWattDaysPerTick));
    double powerLossFactor = (double) pusher.PowerLossFactor;
    return Mathf.Clamp(Mathf.Clamp((float) (num2 / powerLossFactor + 1.0000000116860974E-07), num1, pusher.MaxPowerPush), 0.0f, num3);
  }

  protected static float TransmitAmount(CompPowerNetLink me, CompPowerNetLink other)
  {
    PowerNet powerNet1 = ((CompPower) me).PowerNet;
    PowerNet powerNet2 = ((CompPower) other).PowerNet;
    if (powerNet1 == null || powerNet2 == null)
      return 0.0f;
    float num1 = -CompPowerNetLink.PowerNetNeeds(powerNet1, (CompPowerTrader) me);
    float num2 = powerNet1.CurrentStoredEnergy();
    bool flag1 = GenCollection.Any<CompPowerBattery>(powerNet1.batteryComps, (Predicate<CompPowerBattery>) (b => (double) b.AmountCanAccept != 0.0));
    float num3 = -CompPowerNetLink.PowerNetNeeds(powerNet2, (CompPowerTrader) other);
    float num4 = powerNet2.CurrentStoredEnergy();
    bool flag2 = GenCollection.Any<CompPowerBattery>(powerNet2.batteryComps, (Predicate<CompPowerBattery>) (b => (double) b.AmountCanAccept != 0.0));
    float num5 = num2 - num4;
    float num6 = num1;
    if ((double) num6 <= 0.0)
    {
      if ((double) num6 <= 0.0)
      {
        if ((double) num3 > 0.0)
        {
          float num7 = -num1 / other.PowerLossFactor;
          return (double) num3 <= (double) num7 ? -Mathf.Min(num7, other.MaxPowerPush) : -Mathf.Min((double) num5 > 0.0 ? (flag1 ? num3 : num7) : (flag2 ? num7 : num3), other.MaxPowerPush);
        }
        if ((double) num3 <= 0.0)
        {
          float num8 = -num3 / me.PowerLossFactor;
          float num9 = -num1 / other.PowerLossFactor;
          if ((double) num5 > 0.0)
            return Mathf.Clamp(num8, 0.0f, me.MaxPowerPush);
          return (double) num5 >= 0.0 ? Mathf.Max(Mathf.Min(num8, me.MaxPowerPush), Mathf.Min(num9, other.MaxPowerPush)) : -Mathf.Clamp(num9, 0.0f, other.MaxPowerPush);
        }
      }
    }
    else
    {
      if ((double) num3 <= 0.0)
      {
        float num10 = -num3 / me.PowerLossFactor;
        return (double) num1 <= (double) num10 ? Mathf.Min(num10, me.MaxPowerPush) : Mathf.Min((double) num5 > 0.0 ? (flag2 ? num1 : num10) : (flag1 ? num10 : num1), me.MaxPowerPush);
      }
      if ((double) num3 > 0.0)
        return (double) num5 <= 0.0 ? (!flag1 ? Mathf.Clamp(num1, 0.0f, me.MaxPowerPush) : -Mathf.Clamp(num3, 0.0f, other.MaxPowerPush)) : (!flag2 ? -Mathf.Clamp(num3, 0.0f, other.MaxPowerPush) : Mathf.Clamp(num1, 0.0f, me.MaxPowerPush));
    }
    return 0.0f;
  }

  private static float PowerNetNeeds(PowerNet powerNet, CompPowerTrader ignore = null)
  {
    float num = 0.0f;
    foreach (CompPowerTrader powerComp in powerNet.powerComps)
    {
      if (powerComp.PowerOn || FlickUtility.WantsToBeOn((Thing) ((ThingComp) powerComp).parent) && !BreakdownableUtility.IsBrokenDown((Thing) ((ThingComp) powerComp).parent))
      {
        if (powerComp == ignore)
          num += ((CompPower) powerComp).Props.PowerConsumption;
        else
          num -= powerComp.PowerOutput;
      }
    }
    return num;
  }

  private static float PowerNetBatteryAccepts(PowerNet powerNet)
  {
    return powerNet.batteryComps.Sum<CompPowerBattery>((Func<CompPowerBattery, float>) (b => b.AmountCanAccept));
  }

  public virtual void PostExposeData()
  {
    base.PostExposeData();
    Scribe_References.Look<ThingWithComps>(ref this.linkedTo, "linkedTo", false);
    this.LinkedTo = this.linkedTo;
  }

  [Conditional("DEBUG")]
  protected void DebugMessage(string message)
  {
    if ((double) GenGeo.MagnitudeHorizontalSquared(Vector3.op_Subtraction(((Thing) ((ThingComp) this).parent).DrawPos, UI.MouseMapPosition())) >= 1.5)
      return;
    Log.Message(message);
  }

  public enum PowerTransferMode
  {
    Push,
    Draw,
    Transmit,
  }
}
