// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.CrossMapForbidUtility
// Assembly: VehicleMapFramework, Version=1.6.562.0, Culture=neutral, PublicKeyToken=null
// MVID: 10A61882-945F-4CFC-9B06-CA8EEF5ADB36
// Assembly location: D:\Programas\steamapps\workshop\content\294100\3426502333\1.6\Assemblies\VehicleMapFramework.dll

using RimWorld;
using System;
using System.Runtime.CompilerServices;
using Verse;

#nullable disable
namespace VehicleMapFramework;

public static class CrossMapForbidUtility
{
  public static bool IsForbidden(this IntVec3 c, Pawn pawn, Thing thing)
  {
    Map map1 = thing?.MapHeld ?? TargetMapUtility.get_TargetMapOrPawnMap(pawn);
    if (map1 == null || map1 == ((Thing) pawn).Map)
      return ForbidUtility.IsForbidden(c, pawn);
    Pawn pawn1 = pawn;
    Map map2 = map1;
    bool flag = pawn != null && CrossMapReachabilityUtility.get_DepartMap(pawn) == null;
    IntVec3? c1 = new IntVec3?();
    int num = flag ? 1 : 0;
    using (new VirtualTeleporter((Thing) pawn1, map2, c1, num != 0))
      return ForbidUtility.IsForbidden(c, pawn);
  }

  public static bool IsForbidden(this IntVec3 c, Pawn pawn, Map map)
  {
    if (map == ((Thing) pawn).Map)
      return ForbidUtility.IsForbidden(c, pawn);
    Pawn pawn1 = pawn;
    Map map1 = map;
    bool flag = pawn != null && CrossMapReachabilityUtility.get_DepartMap(pawn) == null;
    IntVec3? c1 = new IntVec3?();
    int num = flag ? 1 : 0;
    using (new VirtualTeleporter((Thing) pawn1, map1, c1, num != 0))
      return ForbidUtility.IsForbidden(c, pawn);
  }

  [SpecialName]
  public sealed class \u003CG\u003E\u0024EBA7D8B16BF040CED4E3DDA13865FC4E
  {
    [ExtensionMarker("<M>$CDF12A6F7EBB5A4C3C551878406C614D")]
    public bool IsForbidden(Pawn pawn, Thing thing) => throw new NotSupportedException();

    [ExtensionMarker("<M>$CDF12A6F7EBB5A4C3C551878406C614D")]
    public bool IsForbidden(Pawn pawn, Map map) => throw new NotSupportedException();

    [SpecialName]
    public static class \u003CM\u003E\u0024CDF12A6F7EBB5A4C3C551878406C614D
    {
      [CompilerGenerated]
      [SpecialName]
      public static void \u003CExtension\u003E\u0024(IntVec3 c)
      {
      }
    }
  }
}
