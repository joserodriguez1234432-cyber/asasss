// Decompiled with JetBrains decompiler
// Type: SmashTools.SingleWindow
// Assembly: SmashTools, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 3B8999A7-6EA8-49BB-AA7F-3E2E898B5249
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\SmashTools.dll

using System;
using System.Linq;
using Verse;

#nullable disable
namespace SmashTools;

[Obsolete]
public abstract class SingleWindow : Window
{
  public bool closeOnAnyClickOutside;

  public static SingleWindow CurrentlyOpenedWindow { get; set; }

  public virtual void PreClose()
  {
    base.PreClose();
    SingleWindow.CurrentlyOpenedWindow = (SingleWindow) null;
  }

  public virtual void PreOpen()
  {
    base.PreOpen();
    SingleWindow.CurrentlyOpenedWindow = this;
    foreach (Window window in Find.WindowStack.Windows.ToList<Window>())
    {
      if (window is SingleWindow singleWindow && singleWindow != SingleWindow.CurrentlyOpenedWindow)
        Find.WindowStack.TryRemove(window, false);
    }
  }

  protected SingleWindow()
    : base((IWindowDrawing) null)
  {
  }
}
