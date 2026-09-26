// Decompiled with JetBrains decompiler
// Type: SmashTools.SubWindow
// Assembly: SmashTools, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 3B8999A7-6EA8-49BB-AA7F-3E2E898B5249
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\SmashTools.dll

using Verse;

#nullable disable
namespace SmashTools;

public abstract class SubWindow : Window, IWindowEventListener
{
  private readonly Window parent;

  protected SubWindow(Window parent)
    : base((IWindowDrawing) null)
  {
    this.parent = parent;
  }

  public void RegisterEvents()
  {
    this.Register(this.parent, new WindowEvents.OnEvent(this.OnParentClosed), WindowEvents.Event.Closed);
  }

  public void DeregisterEvents() => this.Deregister();

  private void OnParentClosed() => this.Close(true);

  public virtual void PostClose()
  {
    base.PostClose();
    this.DeregisterEvents();
  }
}
