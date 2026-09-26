// Decompiled with JetBrains decompiler
// Type: SmashTools.DetachedMapComponent
// Assembly: SmashTools, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 3B8999A7-6EA8-49BB-AA7F-3E2E898B5249
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\SmashTools.dll

using JetBrains.Annotations;
using Verse;

#nullable disable
namespace SmashTools;

[PublicAPI]
public abstract class DetachedMapComponent
{
  protected readonly Map map;

  protected DetachedMapComponent(Map map) => this.map = map;

  protected virtual void PreMapRemoval()
  {
  }
}
