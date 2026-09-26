// Decompiled with JetBrains decompiler
// Type: Vehicles.RiverDefOf
// Assembly: Vehicles, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 81BFDC99-D8AD-42E1-8470-2F73E7836B4A
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\Vehicles.dll

using RimWorld;

#nullable disable
namespace Vehicles;

[DefOf]
public static class RiverDefOf
{
  public static RiverDef Creek;
  public static RiverDef River;
  public static RiverDef LargeRiver;
  public static RiverDef HugeRiver;

  static RiverDefOf() => DefOfHelper.EnsureInitializedInCtor(typeof (RiverDefOf));
}
