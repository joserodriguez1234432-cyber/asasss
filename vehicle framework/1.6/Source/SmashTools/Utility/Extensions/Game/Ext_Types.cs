// Decompiled with JetBrains decompiler
// Type: SmashTools.Ext_Types
// Assembly: SmashTools, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 3B8999A7-6EA8-49BB-AA7F-3E2E898B5249
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\SmashTools.dll

using Verse;

#nullable disable
namespace SmashTools;

public static class Ext_Types
{
  public static void Deconstruct<Item1, Item2>(
    this Pair<Item1, Item2> pair,
    out Item1 item1,
    out Item2 item2)
  {
    item1 = pair.First;
    item2 = pair.Second;
  }
}
