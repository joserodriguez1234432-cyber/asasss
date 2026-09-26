// Decompiled with JetBrains decompiler
// Type: Vehicles.Command_TransferToVehicle_Order
// Assembly: Vehicles, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 81BFDC99-D8AD-42E1-8470-2F73E7836B4A
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\Vehicles.dll

using RimWorld;
using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;

#nullable disable
namespace Vehicles;

public class Command_TransferToVehicle_Order : Command_Target
{
  public static readonly Command_TransferToVehicle_Order Command;

  private Command_TransferToVehicle_Order()
  {
    ((Verse.Command) this).defaultLabel = TaggedString.op_Implicit(Translator.Translate("VF_TransferToVehicle_Order"));
    ((Verse.Command) this).defaultDesc = TaggedString.op_Implicit(Translator.Translate("VF_TransferToVehicle_Order_Desc"));
    ((Verse.Command) this).icon = (Texture) VehicleTex.PackCargoIcon[2];
    ((Verse.Command) this).hotKey = KeyBindingDefOf.Misc2;
    // ISSUE: reference to a compiler-generated field
    // ISSUE: reference to a compiler-generated field
    this.action = Command_TransferToVehicle_Order.\u003C\u003EO.\u003C0\u003E__Action ?? (Command_TransferToVehicle_Order.\u003C\u003EO.\u003C0\u003E__Action = new System.Action<LocalTargetInfo>(Command_TransferToVehicle_Order.Action));
    this.targetingParams = TargetingParameters.ForPawns();
    // ISSUE: reference to a compiler-generated field
    // ISSUE: reference to a compiler-generated field
    this.targetingParams.validator = Command_TransferToVehicle_Order.\u003C\u003EO.\u003C1\u003E__IsValidVehicle ?? (Command_TransferToVehicle_Order.\u003C\u003EO.\u003C1\u003E__IsValidVehicle = new Predicate<TargetInfo>(Command_TransferToVehicle_Order.IsValidVehicle));
    // ISSUE: reference to a compiler-generated field
    // ISSUE: reference to a compiler-generated field
    this.onUpdate = Command_TransferToVehicle_Order.\u003C\u003EO.\u003C2\u003E__DrawLineTo ?? (Command_TransferToVehicle_Order.\u003C\u003EO.\u003C2\u003E__DrawLineTo = new System.Action<LocalTargetInfo>(Command_TransferToVehicle_Order.DrawLineTo));
  }

  private static bool IsValidVehicle(TargetInfo target)
  {
    if (!(((TargetInfo) ref target).Thing is VehiclePawn thing))
      return false;
    CompUpgradeTree compUpgradeTree = thing.CompUpgradeTree;
    return (compUpgradeTree == null || !compUpgradeTree.Upgrading) && ((Thing) thing).Faction == Faction.OfPlayer;
  }

  private static bool TransferableObject(object obj, out Thing thing)
  {
    thing = obj as Thing;
    return thing != null && thing.CanBeHauledToVehicle();
  }

  private static void DrawLineTo(LocalTargetInfo target)
  {
    if (!Find.Targeter.IsTargeting)
      return;
    using (List<object>.Enumerator enumerator = Find.Selector.SelectedObjects.GetEnumerator())
    {
      Thing thing1;
      while (enumerator.MoveNext() && Command_TransferToVehicle_Order.TransferableObject(enumerator.Current, out thing1))
      {
        Thing thing2 = ((LocalTargetInfo) ref target).Thing;
        Vector3 vector3 = thing2 != null ? thing2.DrawPos : UI.MouseMapPosition();
        GenDraw.DrawLineBetween(thing1.DrawPos, vector3);
      }
    }
  }

  public static IEnumerable<Thing> GetSelectedTransferableThings()
  {
    foreach (object selectedObject in Find.Selector.SelectedObjects)
    {
      Thing thing;
      if (Command_TransferToVehicle_Order.TransferableObject(selectedObject, out thing))
        yield return thing;
    }
  }

  private static void Action(LocalTargetInfo target)
  {
    if (!(((LocalTargetInfo) ref target).Thing is VehiclePawn thing))
      return;
    Command_TransferToVehicle_Order.GetSelectedTransferableThings().TransferToVehicle(thing);
  }

  static Command_TransferToVehicle_Order()
  {
    Command_TransferToVehicle_Order transferToVehicleOrder = new Command_TransferToVehicle_Order();
    ((Gizmo) transferToVehicleOrder).Order = 1000f;
    Command_TransferToVehicle_Order.Command = transferToVehicleOrder;
  }
}
