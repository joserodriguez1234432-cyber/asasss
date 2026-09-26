// Decompiled with JetBrains decompiler
// Type: Vehicles.TranslationHelper
// Assembly: Vehicles, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 81BFDC99-D8AD-42E1-8470-2F73E7836B4A
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\Vehicles.dll

using System;
using Verse;

#nullable disable
namespace Vehicles;

public static class TranslationHelper
{
  public static string Translate(this VehicleComponent.VehiclePartDepth depth)
  {
    TaggedString taggedString;
    switch (depth)
    {
      case VehicleComponent.VehiclePartDepth.Undefined:
        taggedString = Translator.Translate("VF_Depth_Undefined");
        break;
      case VehicleComponent.VehiclePartDepth.External:
        taggedString = Translator.Translate("VF_Depth_External");
        break;
      case VehicleComponent.VehiclePartDepth.Internal:
        taggedString = Translator.Translate("VF_Depth_Internal");
        break;
      default:
        throw new NotImplementedException();
    }
    return TaggedString.op_Implicit(taggedString);
  }
}
