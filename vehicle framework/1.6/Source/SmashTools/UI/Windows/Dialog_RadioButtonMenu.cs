// Decompiled with JetBrains decompiler
// Type: SmashTools.Dialog_RadioButtonMenu
// Assembly: SmashTools, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 3B8999A7-6EA8-49BB-AA7F-3E2E898B5249
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\SmashTools.dll

using System;
using System.Collections.Generic;

#nullable disable
namespace SmashTools;

public class Dialog_RadioButtonMenu(string label, List<Toggle> toggles, Action postClose = null) : 
  Dialog_ToggleMenu(label, toggles, postClose)
{
  protected override void DrawToggle(Toggle toggle)
  {
    if (toggle.Disabled)
      GUIState.Disable();
    bool flag = this.lister.RadioButton(toggle.DisplayName, toggle.Active, 0.0f, (string) null, new float?());
    GUIState.Enable();
    toggle.Active = flag;
  }
}
