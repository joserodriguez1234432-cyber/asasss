// Decompiled with JetBrains decompiler
// Type: SmashTools.Dialog_ToggleMenu
// Assembly: SmashTools, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 3B8999A7-6EA8-49BB-AA7F-3E2E898B5249
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\SmashTools.dll

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

#nullable disable
namespace SmashTools;

public class Dialog_ToggleMenu : Window
{
  protected readonly Listing_Standard lister;
  private readonly string label;
  private readonly List<Toggle> toggles;
  private readonly Action postClose;

  public Dialog_ToggleMenu(string label, List<Toggle> toggles, Action postClose = null)
    : base((IWindowDrawing) null)
  {
    this.label = label;
    this.toggles = toggles.OrderBy<Toggle, string>((Func<Toggle, string>) (rb => rb.Category)).ToList<Toggle>();
    this.postClose = postClose;
    this.doCloseX = true;
    this.onlyOneOfTypeAllowed = true;
    this.absorbInputAroundWindow = true;
    Listing_Standard listingStandard = new Listing_Standard((GameFont) 1);
    ((Listing) listingStandard).ColumnWidth = 300f;
    this.lister = listingStandard;
  }

  public virtual Vector2 InitialSize
  {
    get => new Vector2((float) UI.screenWidth, (float) UI.screenHeight);
  }

  public virtual void PostClose()
  {
    Action postClose = this.postClose;
    if (postClose == null)
      return;
    postClose();
  }

  protected virtual void DrawToggle(Toggle toggle)
  {
    bool active = toggle.Active;
    if (toggle.Disabled)
      GUIState.Disable();
    this.lister.CheckboxLabeled(toggle.DisplayName, ref active, (string) null, 0.0f, 1f);
    GUIState.Enable();
    toggle.Active = active;
  }

  public virtual void DoWindowContents(Rect rect)
  {
    TextBlock textBlock;
    // ISSUE: explicit constructor call
    ((TextBlock) ref textBlock).\u002Ector((GameFont) 2, (TextAnchor) 1);
    try
    {
      Widgets.Label(rect, this.label);
      ref Rect local = ref rect;
      ((Rect) ref local).yMin = ((Rect) ref local).yMin + 30f;
    }
    finally
    {
      textBlock.Dispose();
    }
    ((Listing) this.lister).Begin(rect);
    string header = string.Empty;
    foreach (Toggle toggle in this.toggles)
    {
      if (toggle.Category != header)
      {
        header = toggle.Category;
        ((Listing) this.lister).Header(header, ListingExtension.BannerColor, (GameFont) 2, (TextAnchor) 4);
      }
      this.DrawToggle(toggle);
    }
    ((Listing) this.lister).End();
  }
}
