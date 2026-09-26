// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.TargetMapUtility
// Assembly: VehicleMapFramework, Version=1.6.562.0, Culture=neutral, PublicKeyToken=null
// MVID: 10A61882-945F-4CFC-9B06-CA8EEF5ADB36
// Assembly location: D:\Programas\steamapps\workshop\content\294100\3426502333\1.6\Assemblies\VehicleMapFramework.dll

using LudeonTK;
using RimWorld.Planet;
using System;
using System.Runtime.CompilerServices;
using Verse;
using Verse.AI;

#nullable disable
namespace VehicleMapFramework;

public static class TargetMapUtility
{
  public static TargetMapManager manager;
  internal static bool logging;

  public static TargetInfo get_TargetInfo(Thing thing)
  {
    StrongBox<TargetInfo> strongBox;
    bool? nullable = TargetMapUtility.manager?.TargetInfoTable.TryGetValue(thing, out strongBox);
    return !nullable.HasValue || !nullable.GetValueOrDefault() ? TargetInfo.Invalid : strongBox.Value;
  }

  public static void set_TargetInfo(Thing thing, TargetInfo value)
  {
    if (TargetMapUtility.logging)
      VMF_Log.Message($"Set TargetInfo ({value}) to {thing}");
    TargetMapManager manager = TargetMapUtility.manager;
    if (manager == null)
      return;
    StrongBox<TargetInfo> targetInfo = manager.GetOrCreateTargetInfo(thing);
    if (targetInfo == null)
      return;
    targetInfo.Value = value;
  }

  public static Map get_TargetMap(Thing thing)
  {
    StrongBox<TargetInfo> strongBox;
    bool? nullable = TargetMapUtility.manager?.TargetInfoTable.TryGetValue(thing, out strongBox);
    return !nullable.HasValue || !nullable.GetValueOrDefault() ? (Map) null : ((TargetInfo) ref strongBox.Value).Map;
  }

  public static void set_TargetMap(Thing thing, Map value)
  {
    if (TargetMapUtility.logging)
      VMF_Log.Message($"Set TargetMap ({value}) to {thing}");
    TargetMapManager manager = TargetMapUtility.manager;
    if (manager == null)
      return;
    StrongBox<TargetInfo> targetInfo = manager.GetOrCreateTargetInfo(thing);
    if (targetInfo == null)
      return;
    targetInfo.Value = new TargetInfo(IntVec3.Invalid, value, false);
  }

  public static void RemoveTargetInfo(this Thing thing)
  {
    StrongBox<TargetInfo> strongBox;
    if (thing == null || TargetMapUtility.manager == null || !TargetMapUtility.manager.TargetInfoTable.TryGetValue(thing, out strongBox))
      return;
    if (TargetMapUtility.logging)
      VMF_Log.Message($"Remove TargetInfo ({strongBox.Value}) from {thing}");
    strongBox.Value = TargetInfo.Invalid;
  }

  public static bool TryGetTargetInfo(this Thing thing, out TargetInfo target)
  {
    target = TargetInfo.Invalid;
    if (thing == null || TargetMapUtility.manager == null)
      return false;
    StrongBox<TargetInfo> strongBox;
    int num = TargetMapUtility.manager.TargetInfoTable.TryGetValue(thing, out strongBox) ? 1 : 0;
    if (num != 0)
      target = strongBox.Value;
    return num != 0 && ((TargetInfo) ref target).IsValid;
  }

  public static bool IsTargeting(
    this Thing thing,
    LocalTargetInfo localTarget,
    out TargetInfo target)
  {
    target = TargetInfo.Invalid;
    return thing != null && thing.TryGetTargetInfo(out target) && LocalTargetInfo.op_Equality(TargetInfo.op_Explicit(target), localTarget);
  }

  public static bool TryGetTargetMap(this Thing thing, out Map map)
  {
    map = (Map) null;
    if (thing == null)
      return false;
    ConditionalWeakTable<Thing, StrongBox<TargetInfo>> targetInfoTable = TargetMapUtility.manager?.TargetInfoTable;
    if (targetInfoTable == null)
      return false;
    StrongBox<TargetInfo> strongBox;
    int num = targetInfoTable.TryGetValue(thing, out strongBox) ? 1 : 0;
    if (num != 0)
      map = ((TargetInfo) ref strongBox.Value).Map;
    return num != 0 && map != null;
  }

  public static Map get_TargetMapOrThingMap(Thing thing)
  {
    return TargetMapUtility.get_TargetMap(thing) ?? thing.Map;
  }

  public static IntVec3 get_PositionOnTargetMap(Thing thing)
  {
    Map map;
    if (!thing.TryGetTargetMap(out map) || map == thing.Map)
      return thing.Position;
    IntVec3 original = VehicleMapUtility.get_PositionOnBaseMap(thing);
    VehiclePawnWithMap vehicle;
    if (map.IsNonFocusedVehicleMapOf(out vehicle))
      original = original.ToVehicleMapCoord(vehicle);
    return original;
  }

  public static Map get_TargetMapOrPawnMap(Pawn pawn)
  {
    Map targetMap = TargetMapUtility.get_TargetMap((Thing) pawn);
    if (targetMap != null)
      return targetMap;
    Job curJob = pawn.CurJob;
    return (curJob != null ? ((GlobalTargetInfo) ref curJob.globalTarget).Map : (Map) null) ?? ((Thing) pawn).Map;
  }

  public static IntVec3 TargetCellOnBaseMap(ref this LocalTargetInfo targ, Thing thing)
  {
    if (((LocalTargetInfo) ref targ).HasThing)
      return VehicleMapUtility.get_PositionOnBaseMap(((LocalTargetInfo) ref targ).Thing);
    Map map;
    return !thing.TryGetTargetMap(out map) ? ((LocalTargetInfo) ref targ).Cell : ((LocalTargetInfo) ref targ).Cell.ToBaseMapCoord(map);
  }

  public static Map TargetMapOrMap(Map map, Thing thing)
  {
    return TargetMapUtility.get_TargetMap(thing) ?? map;
  }

  [DebugAction("Vehicle Map Framework", "Toggle Logging TargetMapManager", false, false, false, false, false, 0, false)]
  private static void ToggleLogging() => TargetMapUtility.logging = !TargetMapUtility.logging;

  [SpecialName]
  public sealed class \u003CG\u003E\u00249EA7376D1A13FE36E57A42DEEFA9C1DE
  {
    [ExtensionMarker("<M>$DFCC9EE29BE0341F99924D3A409FC2EE")]
    public TargetInfo TargetInfo
    {
      [ExtensionMarker("<M>$DFCC9EE29BE0341F99924D3A409FC2EE")] get
      {
        throw new NotSupportedException();
      }
      [ExtensionMarker("<M>$DFCC9EE29BE0341F99924D3A409FC2EE")] set
      {
        throw new NotSupportedException();
      }
    }

    [ExtensionMarker("<M>$DFCC9EE29BE0341F99924D3A409FC2EE")]
    public Map TargetMap
    {
      [ExtensionMarker("<M>$DFCC9EE29BE0341F99924D3A409FC2EE")] get
      {
        throw new NotSupportedException();
      }
      [ExtensionMarker("<M>$DFCC9EE29BE0341F99924D3A409FC2EE")] set
      {
        throw new NotSupportedException();
      }
    }

    [ExtensionMarker("<M>$DFCC9EE29BE0341F99924D3A409FC2EE")]
    public void RemoveTargetInfo() => throw new NotSupportedException();

    [ExtensionMarker("<M>$DFCC9EE29BE0341F99924D3A409FC2EE")]
    public bool TryGetTargetInfo(out TargetInfo target) => throw new NotSupportedException();

    [ExtensionMarker("<M>$DFCC9EE29BE0341F99924D3A409FC2EE")]
    public bool IsTargeting(LocalTargetInfo localTarget, out TargetInfo target)
    {
      throw new NotSupportedException();
    }

    [ExtensionMarker("<M>$DFCC9EE29BE0341F99924D3A409FC2EE")]
    public bool TryGetTargetMap(out Map map) => throw new NotSupportedException();

    [ExtensionMarker("<M>$DFCC9EE29BE0341F99924D3A409FC2EE")]
    public Map TargetMapOrThingMap
    {
      [ExtensionMarker("<M>$DFCC9EE29BE0341F99924D3A409FC2EE")] get
      {
        throw new NotSupportedException();
      }
    }

    [ExtensionMarker("<M>$DFCC9EE29BE0341F99924D3A409FC2EE")]
    public IntVec3 PositionOnTargetMap
    {
      [ExtensionMarker("<M>$DFCC9EE29BE0341F99924D3A409FC2EE")] get
      {
        throw new NotSupportedException();
      }
    }

    [SpecialName]
    public static class \u003CM\u003E\u0024DFCC9EE29BE0341F99924D3A409FC2EE
    {
      [CompilerGenerated]
      [SpecialName]
      public static void \u003CExtension\u003E\u0024(Thing thing)
      {
      }
    }
  }

  [SpecialName]
  public sealed class \u003CG\u003E\u00242F59771236D7C1AA57ADCD68358D448A
  {
    [ExtensionMarker("<M>$E15D13A036DCD41255EBD6266F8556E9")]
    public Map TargetMapOrPawnMap
    {
      [ExtensionMarker("<M>$E15D13A036DCD41255EBD6266F8556E9")] get
      {
        throw new NotSupportedException();
      }
    }

    [SpecialName]
    public static class \u003CM\u003E\u0024E15D13A036DCD41255EBD6266F8556E9
    {
      [CompilerGenerated]
      [SpecialName]
      public static void \u003CExtension\u003E\u0024(Pawn pawn)
      {
      }
    }
  }
}
