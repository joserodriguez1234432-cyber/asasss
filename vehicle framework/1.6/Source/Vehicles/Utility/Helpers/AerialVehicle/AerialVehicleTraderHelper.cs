// Decompiled with JetBrains decompiler
// Type: Vehicles.World.AerialVehicleTraderHelper
// Assembly: Vehicles, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 81BFDC99-D8AD-42E1-8470-2F73E7836B4A
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\Vehicles.dll

using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using Verse;

#nullable disable
namespace Vehicles.World;

public static class AerialVehicleTraderHelper
{
  private static AerialVehicleInFlight aerialVehicle;
  private static readonly List<TransferableUIUtility.ExtraInfo> tmpInfo = new List<TransferableUIUtility.ExtraInfo>();
  private static readonly MethodInfo massUsagePropertyInfo = AccessTools.PropertyGetter(typeof (Dialog_Trade), "MassUsage");
  private static readonly List<Pair<float, Color>> MassColor = new List<Pair<float, Color>>()
  {
    new Pair<float, Color>(0.37f, Color.green),
    new Pair<float, Color>(0.82f, Color.yellow),
    new Pair<float, Color>(1f, new Color(1f, 0.6f, 0.0f))
  };

  public static void SetupAerialVehicleTrade(ref List<Thing> playerCaravanAllPawnsAndItems)
  {
    Pawn playerNegotiator = TradeSession.playerNegotiator;
    AerialVehicleTraderHelper.aerialVehicle = playerNegotiator != null ? playerNegotiator.GetAerialVehicle() : (AerialVehicleInFlight) null;
    if (AerialVehicleTraderHelper.aerialVehicle == null)
      return;
    List<Pawn> allPawnsAboard = AerialVehicleTraderHelper.aerialVehicle.Vehicle.AllPawnsAboard;
    ThingOwner<Thing> innerContainer = AerialVehicleTraderHelper.aerialVehicle.Vehicle.inventory.innerContainer;
    List<Thing> thingList = new List<Thing>(allPawnsAboard.Count + ((ThingOwner) innerContainer).Count);
    foreach (Pawn pawn in allPawnsAboard)
      thingList.Add((Thing) pawn);
    thingList.AddRange((IEnumerable<Thing>) innerContainer);
    playerCaravanAllPawnsAndItems = thingList;
  }

  public static float DrawAerialVehicleInfo(
    Dialog_Trade tradeDialog,
    Rect rect,
    bool lerpMassColor = true)
  {
    if (AerialVehicleTraderHelper.aerialVehicle == null)
      return 0.0f;
    AerialVehicleTraderHelper.tmpInfo.Clear();
    float statValue = AerialVehicleTraderHelper.aerialVehicle.Vehicle.GetStatValue(VehicleStatDefOf.CargoCapacity);
    float massUsage = (float) AerialVehicleTraderHelper.massUsagePropertyInfo.Invoke((object) tradeDialog, Array.Empty<object>());
    TaggedString taggedString = TaggedString.op_Implicit($"{GenText.ToStringEnsureThreshold(massUsage, statValue, 0)} / {statValue:F0} {Translator.Translate("kg")}");
    string massTip = AerialVehicleTraderHelper.GetMassTip(massUsage, statValue);
    AerialVehicleTraderHelper.tmpInfo.Add(new TransferableUIUtility.ExtraInfo(TaggedString.op_Implicit(Translator.Translate("Mass")), TaggedString.op_Implicit(taggedString), AerialVehicleTraderHelper.GetMassColor(massUsage, statValue, lerpMassColor), massTip, -9999f));
    AerialVehicleTraderHelper.tmpInfo.Add(new TransferableUIUtility.ExtraInfo(TaggedString.op_Implicit(VehicleStatDefOf.FlightSpeed.LabelCap), AerialVehicleTraderHelper.GetSpeedLabel(), Color.white, string.Empty, -9999f));
    TransferableUIUtility.DrawExtraInfo(AerialVehicleTraderHelper.tmpInfo, rect);
    AerialVehicleTraderHelper.tmpInfo.Clear();
    return 52f;
  }

  private static string GetMassTip(float massUsage, float massCapacity)
  {
    return TaggedString.op_Implicit(TaggedString.op_Implicit($"{Translator.Translate("MassCarriedSimple")}: {GenText.ToStringEnsureThreshold(massUsage, massCapacity, 2)} {Translator.Translate("kg")} \n {Translator.Translate("MassCapacity")}: {massCapacity.ToString("F2")} {Translator.Translate("kg")}"));
  }

  private static string GetSpeedLabel()
  {
    return VehicleStatDefOf.FlightSpeed.Worker.StatValueFormatted(AerialVehicleTraderHelper.aerialVehicle.Vehicle);
  }

  private static Color GetMassColor(float massUsage, float massCapacity, bool lerpMassColor)
  {
    if ((double) massCapacity == 0.0)
      return Color.white;
    if ((double) massUsage > (double) massCapacity)
      return Color.red;
    return lerpMassColor ? GenUI.LerpColor(AerialVehicleTraderHelper.MassColor, massUsage / massCapacity) : Color.white;
  }
}
