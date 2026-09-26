// Decompiled with JetBrains decompiler
// Type: Vehicles.ICustomSettingsDrawer
// Assembly: Vehicles, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 81BFDC99-D8AD-42E1-8470-2F73E7836B4A
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\Vehicles.dll

using System.Reflection;

#nullable disable
namespace Vehicles;

public interface ICustomSettingsDrawer
{
  void DrawSetting(
    Listing_Settings lister,
    VehicleDef vehicleDef,
    FieldInfo field,
    string label,
    string tooltip,
    string disabledTooltip,
    bool locked,
    bool translate);
}
