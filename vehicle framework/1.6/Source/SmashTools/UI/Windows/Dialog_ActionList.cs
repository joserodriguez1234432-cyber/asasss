// Decompiled with JetBrains decompiler
// Type: SmashTools.Dialog_ActionList
// Assembly: SmashTools, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 3B8999A7-6EA8-49BB-AA7F-3E2E898B5249
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\SmashTools.dll

using System;
using System.Collections.Generic;
using Verse;

#nullable disable
namespace SmashTools;

public class Dialog_ActionList(string label, List<Toggle> toggles, Action postClose = null) : 
  Dialog_ToggleMenu(label, toggles, postClose)
{
  protected override void DrawToggle(Toggle toggle)
  {
    if (toggle.Disabled)
      GUIState.Disable();
    if (((Listing) this.lister).ClickableLabel(toggle.DisplayName))
    {
      toggle.Active = true;
      this.Close(true);
    }
    GUIState.Enable();
  }
}
