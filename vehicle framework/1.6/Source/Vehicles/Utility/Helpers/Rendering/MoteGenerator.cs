// Decompiled with JetBrains decompiler
// Type: Vehicles.MoteGenerator
// Assembly: Vehicles, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 81BFDC99-D8AD-42E1-8470-2F73E7836B4A
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\Vehicles.dll

using JetBrains.Annotations;
using Vehicles.Rendering;
using Verse;

#nullable disable
namespace Vehicles;

[PublicAPI]
public static class MoteGenerator
{
  public static void ThrowMote(
    IntVec3 loc,
    Map map,
    MoteThrown mote,
    MoteGenerator.SaturationPriority priority = MoteGenerator.SaturationPriority.Low,
    RenderConditions renderConditions = RenderConditions.Vanilla)
  {
    switch (priority)
    {
      case MoteGenerator.SaturationPriority.Low:
        if (map.moteCounter.SaturatedLowPriority)
          return;
        break;
      case MoteGenerator.SaturationPriority.Normal:
        if (map.moteCounter.Saturated)
          return;
        break;
      case MoteGenerator.SaturationPriority.High:
        if ((double) map.moteCounter.Saturation > 1.2000000476837158)
          return;
        break;
    }
    if (!RenderHelper.ShouldShow(map, loc, renderConditions))
      return;
    GenSpawn.Spawn((Thing) mote, loc, map, (WipeMode) 0);
  }

  public enum SaturationPriority
  {
    Low,
    Normal,
    High,
    AlwaysShow,
  }
}
