// Decompiled with JetBrains decompiler
// Type: SmashTools.UndoDisable
// Assembly: SmashTools, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 3B8999A7-6EA8-49BB-AA7F-3E2E898B5249
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\SmashTools.dll

using System;

#nullable disable
namespace SmashTools;

public struct UndoDisable : IDisposable
{
  private UndoManager manager;

  public UndoDisable(UndoManager manager)
  {
    this.manager = manager;
    manager.Disabled = true;
  }

  public void Dispose() => this.manager.Disabled = false;
}
