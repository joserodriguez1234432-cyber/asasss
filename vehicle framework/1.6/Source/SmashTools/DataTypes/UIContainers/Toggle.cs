// Decompiled with JetBrains decompiler
// Type: SmashTools.Toggle
// Assembly: SmashTools, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 3B8999A7-6EA8-49BB-AA7F-3E2E898B5249
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\SmashTools.dll

using System;

#nullable disable
namespace SmashTools;

public class Toggle
{
  private readonly Func<bool> get_State;
  private readonly Action<bool> set_State;
  private readonly Action<bool> onToggle;

  public Toggle(
    string id,
    Func<bool> stateGetter = null,
    Action<bool> stateSetter = null,
    Action<bool> onToggle = null)
  {
    this.Id = id;
    this.DisplayName = id;
    this.Category = string.Empty;
    this.get_State = stateGetter;
    this.set_State = stateSetter;
    this.onToggle = onToggle;
  }

  public Toggle(
    string id,
    string category,
    Func<bool> stateGetter = null,
    Action<bool> stateSetter = null,
    Action<bool> onToggle = null)
  {
    this.Id = id;
    this.DisplayName = id;
    this.Category = category;
    this.get_State = stateGetter;
    this.set_State = stateSetter;
    this.onToggle = onToggle;
  }

  public Toggle(
    string id,
    string name,
    string category,
    Func<bool> stateGetter = null,
    Action<bool> stateSetter = null,
    Action<bool> onToggle = null)
  {
    this.Id = id;
    this.DisplayName = name;
    this.Category = category;
    this.get_State = stateGetter;
    this.set_State = stateSetter;
    this.onToggle = onToggle;
  }

  public string Id { get; private set; }

  public string DisplayName { get; private set; }

  public string Category { get; private set; }

  public bool Disabled { get; set; }

  public bool Active
  {
    get
    {
      Func<bool> getState = this.get_State;
      return getState != null && getState();
    }
    set
    {
      if (this.Active == value)
        return;
      Action<bool> setState = this.set_State;
      if (setState != null)
        setState(value);
      Action<bool> onToggle = this.onToggle;
      if (onToggle == null)
        return;
      onToggle(value);
    }
  }
}
