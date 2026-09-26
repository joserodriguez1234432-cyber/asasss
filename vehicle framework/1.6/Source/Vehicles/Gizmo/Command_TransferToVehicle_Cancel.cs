// Decompiled with JetBrains decompiler
// Type: Vehicles.Command_TransferToVehicle_Cancel
// Assembly: Vehicles, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 81BFDC99-D8AD-42E1-8470-2F73E7836B4A
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\Vehicles.dll

using RimWorld;
using UnityEngine;
using Verse;

#nullable disable
namespace Vehicles;

public class Command_TransferToVehicle_Cancel : Command_Action
{
  public static readonly Command_TransferToVehicle_Cancel Command;

  private Command_TransferToVehicle_Cancel()
  {
    ((Verse.Command) this).defaultLabel = TaggedString.op_Implicit(Translator.Translate("VF_TransferToVehicle_Cancel"));
    ((Verse.Command) this).defaultDesc = TaggedString.op_Implicit(Translator.Translate("VF_TransferToVehicle_Cancel_Desc"));
    ((Verse.Command) this).icon = (Texture) VehicleTex.CancelPackCargoIcon[2];
    // ISSUE: reference to a compiler-generated field
    // ISSUE: reference to a compiler-generated field
    this.action = Command_TransferToVehicle_Cancel.\u003C\u003EO.\u003C0\u003E__Action ?? (Command_TransferToVehicle_Cancel.\u003C\u003EO.\u003C0\u003E__Action = new System.Action(Command_TransferToVehicle_Cancel.Action));
    ((Verse.Command) this).hotKey = KeyBindingDefOf.Cancel;
  }

  private static void Action()
  {
    foreach (Thing transferableThing in Command_TransferToVehicle_Order.GetSelectedTransferableThings())
      transferableThing.CancelTransferToAnyVehicle();
  }

  static Command_TransferToVehicle_Cancel()
  {
    Command_TransferToVehicle_Cancel transferToVehicleCancel = new Command_TransferToVehicle_Cancel();
    ((Gizmo) transferToVehicleCancel).Order = 1001f;
    Command_TransferToVehicle_Cancel.Command = transferToVehicleCancel;
  }
}
