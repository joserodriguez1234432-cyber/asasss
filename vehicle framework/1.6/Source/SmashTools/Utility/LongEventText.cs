// Decompiled with JetBrains decompiler
// Type: SmashTools.LongEventText
// Assembly: SmashTools, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 3B8999A7-6EA8-49BB-AA7F-3E2E898B5249
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\SmashTools.dll

using System;
using Verse;

#nullable disable
namespace SmashTools;

public readonly struct LongEventText : IDisposable
{
  private readonly string text;

  public LongEventText() => this.text = Ext_LongEventHandler.GetLongEventText();

  void IDisposable.Dispose() => LongEventHandler.SetCurrentEventText(this.text);
}
