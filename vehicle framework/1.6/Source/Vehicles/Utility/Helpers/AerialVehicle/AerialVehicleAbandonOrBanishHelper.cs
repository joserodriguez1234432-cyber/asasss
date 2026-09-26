// Decompiled with JetBrains decompiler
// Type: Vehicles.World.AerialVehicleAbandonOrBanishHelper
// Assembly: Vehicles, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 81BFDC99-D8AD-42E1-8470-2F73E7836B4A
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\Vehicles.dll

using RimWorld;
using RimWorld.Planet;
using System;
using System.Text;
using Verse;

#nullable disable
namespace Vehicles.World;

public static class AerialVehicleAbandonOrBanishHelper
{
  public static void TryAbandonOrBanishViaInterface(
    Thing thing,
    AerialVehicleInFlight aerialVehicle)
  {
    Pawn pawn = thing as Pawn;
    if (pawn == null)
      Find.WindowStack.Add((Window) Dialog_MessageBox.CreateConfirmation(TranslatorFormattedStringExtensions.Translate("ConfirmAbandonItemDialog", NamedArgument.op_Implicit(((Entity) thing).Label)), (Action) (() =>
      {
        Pawn ownerOf = AerialVehicleAbandonOrBanishHelper.GetOwnerOf(aerialVehicle, thing);
        if (ownerOf == null)
        {
          Log.Error($"Could not find owner of {thing}");
        }
        else
        {
          thing.Notify_AbandonedAtTile(aerialVehicle.Tile);
          ((ThingOwner) ownerOf.inventory.innerContainer).Remove(thing);
          thing.Destroy((DestroyMode) 0);
        }
      }), true, (string) null, (WindowLayer) 1));
    else if (!GenCollection.Any<Pawn>(aerialVehicle.Vehicle.AllCapablePawns, (Predicate<Pawn>) (innerPawn => innerPawn != pawn && !WildManUtility.NonHumanlikeOrWildMan(innerPawn))))
      Messages.Message(TaggedString.op_Implicit(Translator.Translate("MessageCantBanishLastColonist")), LookTargets.op_Implicit((WorldObject) aerialVehicle), MessageTypeDefOf.RejectInput, false);
    else
      PawnBanishUtility.ShowBanishPawnConfirmationDialog(pawn, (Action) null);
  }

  public static void TryAbandonOrBanishViaInterface(
    TransferableImmutable transferable,
    AerialVehicleInFlight aerialVehicle)
  {
    if (((Transferable) transferable).AnyThing is Pawn anyThing)
      AerialVehicleAbandonOrBanishHelper.TryAbandonOrBanishViaInterface((Thing) anyThing, aerialVehicle);
    else
      Find.WindowStack.Add((Window) Dialog_MessageBox.CreateConfirmation(TranslatorFormattedStringExtensions.Translate("ConfirmAbandonItemDialog", NamedArgument.op_Implicit(transferable.LabelWithTotalStackCount)), (Action) (() =>
      {
        for (int index = 0; index < transferable.things.Count; ++index)
        {
          Thing thing = transferable.things[index];
          Pawn ownerOf = AerialVehicleAbandonOrBanishHelper.GetOwnerOf(aerialVehicle, thing);
          if (ownerOf == null)
          {
            Log.Error("Could not find owner of " + thing?.ToString());
            break;
          }
          thing.Notify_AbandonedAtTile(aerialVehicle.Tile);
          ((ThingOwner) ownerOf.inventory.innerContainer).Remove(thing);
          thing.Destroy((DestroyMode) 0);
        }
      }), true, (string) null, (WindowLayer) 1));
  }

  public static void TryAbandonSpecificCountViaInterface(
    Thing thing,
    AerialVehicleInFlight aerialVehicle)
  {
    Find.WindowStack.Add((Window) new Dialog_Slider(TaggedString.op_Implicit(TranslatorFormattedStringExtensions.Translate("AbandonSliderText", NamedArgument.op_Implicit(thing.LabelNoCount))), 1, thing.stackCount, (Action<int>) (x =>
    {
      Pawn ownerOf = AerialVehicleAbandonOrBanishHelper.GetOwnerOf(aerialVehicle, thing);
      if (ownerOf == null)
        Log.Error($"Could not find owner of {thing}");
      else if (x >= thing.stackCount)
      {
        thing.Notify_AbandonedAtTile(aerialVehicle.Tile);
        ((ThingOwner) ownerOf.inventory.innerContainer).Remove(thing);
        thing.Destroy((DestroyMode) 0);
      }
      else
      {
        Thing thing1 = thing.SplitOff(x);
        thing1.Notify_AbandonedAtTile(aerialVehicle.Tile);
        thing1.Destroy((DestroyMode) 0);
      }
    }), int.MinValue, 1f));
  }

  public static void TryAbandonSpecificCountViaInterface(
    TransferableImmutable transferable,
    AerialVehicleInFlight aerialVehicle)
  {
    Find.WindowStack.Add((Window) new Dialog_Slider(TaggedString.op_Implicit(TranslatorFormattedStringExtensions.Translate("AbandonSliderText", NamedArgument.op_Implicit(((Transferable) transferable).Label))), 1, transferable.TotalStackCount, (Action<int>) (x =>
    {
      int num = x;
      for (int index = 0; index < transferable.things.Count && num > 0; ++index)
      {
        Thing thing1 = transferable.things[index];
        Pawn ownerOf = AerialVehicleAbandonOrBanishHelper.GetOwnerOf(aerialVehicle, thing1);
        if (ownerOf == null)
        {
          Log.Error("Could not find owner of " + thing1?.ToString());
          break;
        }
        if (num >= thing1.stackCount)
        {
          num -= thing1.stackCount;
          thing1.Notify_AbandonedAtTile(aerialVehicle.Tile);
          ((ThingOwner) ownerOf.inventory.innerContainer).Remove(thing1);
          thing1.Destroy((DestroyMode) 0);
        }
        else
        {
          Thing thing2 = thing1.SplitOff(num);
          thing2.Notify_AbandonedAtTile(aerialVehicle.Tile);
          thing2.Destroy((DestroyMode) 0);
          num = 0;
        }
      }
    }), int.MinValue, 1f));
  }

  public static string GetAbandonOrBanishButtonTooltip(Thing thing, bool abandonSpecificCount)
  {
    return thing is Pawn pawn ? PawnBanishUtility.GetBanishButtonTip(pawn) : AerialVehicleAbandonOrBanishHelper.GetAbandonItemButtonTooltip(thing.stackCount, abandonSpecificCount);
  }

  public static string GetAbandonOrBanishButtonTooltip(
    TransferableImmutable transferable,
    bool abandonSpecificCount)
  {
    return ((Transferable) transferable).AnyThing is Pawn anyThing ? PawnBanishUtility.GetBanishButtonTip(anyThing) : AerialVehicleAbandonOrBanishHelper.GetAbandonItemButtonTooltip(transferable.TotalStackCount, abandonSpecificCount);
  }

  private static string GetAbandonItemButtonTooltip(
    int currentStackCount,
    bool abandonSpecificCount)
  {
    StringBuilder stringBuilder = new StringBuilder();
    if (currentStackCount == 1)
      stringBuilder.AppendLine(TaggedString.op_Implicit(Translator.Translate("AbandonTip")));
    else if (abandonSpecificCount)
      stringBuilder.AppendLine(TaggedString.op_Implicit(Translator.Translate("AbandonSpecificCountTip")));
    else
      stringBuilder.AppendLine(TaggedString.op_Implicit(Translator.Translate("AbandonAllTip")));
    stringBuilder.AppendLine();
    stringBuilder.Append(TaggedString.op_Implicit(Translator.Translate("AbandonItemTipExtraText")));
    return stringBuilder.ToString();
  }

  public static Pawn GetOwnerOf(AerialVehicleInFlight aerialVehicle, Thing item)
  {
    IThingHolder parentHolder1 = item.ParentHolder;
    if (parentHolder1 is Pawn_InventoryTracker)
    {
      Pawn parentHolder2 = (Pawn) parentHolder1.ParentHolder;
      if (parentHolder2 == aerialVehicle.Vehicle || aerialVehicle.Vehicle.AllPawnsAboard.Contains(parentHolder2))
        return parentHolder2;
    }
    return (Pawn) null;
  }
}
