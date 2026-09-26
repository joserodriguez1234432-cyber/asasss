// Decompiled with JetBrains decompiler
// Type: Vehicles.VehicleEnabled
// Assembly: Vehicles, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 81BFDC99-D8AD-42E1-8470-2F73E7836B4A
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\Vehicles.dll

using System;
using UnityEngine;
using Verse;

#nullable disable
namespace Vehicles;

public static class VehicleEnabled
{
  public static VehicleEnabled.For Next(this VehicleEnabled.For current)
  {
    switch (current)
    {
      case VehicleEnabled.For.None:
        return VehicleEnabled.For.Player;
      case VehicleEnabled.For.Player:
        return VehicleEnabled.For.Raiders;
      case VehicleEnabled.For.Raiders:
        return VehicleEnabled.For.Everyone;
      case VehicleEnabled.For.Everyone:
        return VehicleEnabled.For.None;
      default:
        throw new NotImplementedException();
    }
  }

  public static (string text, Color color) GetStatus(VehicleEnabled.For status)
  {
    (string, Color) status1;
    switch (status)
    {
      case VehicleEnabled.For.None:
        status1 = (TaggedString.op_Implicit(Translator.Translate("VF_VehicleDisabled")), Color.red);
        break;
      case VehicleEnabled.For.Player:
        status1 = (TaggedString.op_Implicit(Translator.Translate("VF_VehiclePlayerOnly")), new Color(0.1f, 0.85f, 0.85f));
        break;
      case VehicleEnabled.For.Raiders:
        status1 = (TaggedString.op_Implicit(Translator.Translate("VF_VehicleRaiderOnly")), new Color(0.9f, 0.53f, 0.1f));
        break;
      case VehicleEnabled.For.Everyone:
        status1 = (TaggedString.op_Implicit(Translator.Translate("VF_VehicleEnabled")), Color.green);
        break;
      default:
        status1 = ("[Err] Uncaught Status", Color.red);
        break;
    }
    return status1;
  }

  [Flags]
  public enum For
  {
    None = 0,
    Player = 1,
    Raiders = 2,
    Everyone = Raiders | Player, // 0x00000003
  }
}
