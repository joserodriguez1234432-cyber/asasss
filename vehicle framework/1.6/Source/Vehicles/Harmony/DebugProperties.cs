// Decompiled with JetBrains decompiler
// Type: Vehicles.DebugProperties
// Assembly: Vehicles, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 81BFDC99-D8AD-42E1-8470-2F73E7836B4A
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\Vehicles.dll

using SmashTools;

#nullable disable
namespace Vehicles;

internal static class DebugProperties
{
  internal static readonly bool Debug = false;
  internal static readonly bool DrawPaths = false;
  internal static readonly bool DrawAllRegions = false;
  private static readonly (string defName, DebugRegionType regionType) RegionDebugging = ("VF_TestMarshal", DebugRegionType.Regions | DebugRegionType.Links);

  internal static void Init()
  {
    Trace.IsFalse(DebugProperties.Debug);
    typeof (DebugProperties).SetStaticFieldsDefault();
  }
}
