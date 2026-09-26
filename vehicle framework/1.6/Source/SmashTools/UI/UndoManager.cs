// Decompiled with JetBrains decompiler
// Type: SmashTools.UndoManager
// Assembly: SmashTools, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 3B8999A7-6EA8-49BB-AA7F-3E2E898B5249
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\SmashTools.dll

using System;
using System.Collections.Generic;

#nullable disable
namespace SmashTools;

public class UndoManager
{
  private readonly Stack<UndoManager.UndoItem> undoStack = new Stack<UndoManager.UndoItem>();

  public bool Disabled { get; internal set; }

  public void StartOperation(Action action, Action undo)
  {
    this.undoStack.Push(new UndoManager.UndoItem(action, undo));
  }

  public void UndoOperation() => this.undoStack.Pop().undo();

  public void Clear() => this.undoStack.Clear();

  private struct UndoItem(Action action, Action undo)
  {
    public Action action = action;
    public Action undo = undo;
  }
}
