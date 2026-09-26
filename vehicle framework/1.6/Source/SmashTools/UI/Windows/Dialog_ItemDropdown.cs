// Decompiled with JetBrains decompiler
// Type: SmashTools.Dialog_ItemDropdown`1
// Assembly: SmashTools, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 3B8999A7-6EA8-49BB-AA7F-3E2E898B5249
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\SmashTools.dll

using RimWorld;
using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;

#nullable disable
namespace SmashTools;

public abstract class Dialog_ItemDropdown<T> : Window
{
  private const int MaxCountShown = 6;
  private const float EntryHeight = 30f;
  private const float Padding = 2f;
  private const float LabelPadding = 3f;
  private readonly Color highlightColor;
  private readonly float width;
  private readonly Action<T> onItemPicked;
  private readonly Func<T, string> itemName;
  private readonly Func<T, string> itemTooltip;
  private readonly Func<T, bool> isSelected;
  private readonly Dialog_ItemDropdown<T>.CreateItemButton createItem;
  private QuickSearchFilter filter;
  private Vector2 windowSize;
  private Vector2 position;
  private Vector2 scrollPos;
  private float fullHeight;
  private readonly List<T> items;

  public Dialog_ItemDropdown(
    Rect rect,
    List<T> items,
    Action<T> onItemPicked,
    Func<T, string> itemName,
    Func<T, bool> isSelected,
    Func<T, string> itemTooltip = null,
    Dialog_ItemDropdown<T>.CreateItemButton createItem = null)
  {
    ColorInt colorInt = new ColorInt(150, 150, 150, 40);
    this.highlightColor = ((ColorInt) ref colorInt).ToColor;
    this.filter = new QuickSearchFilter();
    // ISSUE: explicit constructor call
    base.\u002Ector((IWindowDrawing) null);
    this.items = items;
    this.width = ((Rect) ref rect).width;
    this.position = ((Rect) ref rect).position;
    this.onItemPicked = onItemPicked;
    this.itemName = itemName;
    this.itemTooltip = itemTooltip;
    this.isSelected = isSelected;
    this.createItem = createItem;
    this.closeOnClickedOutside = true;
    this.absorbInputAroundWindow = false;
    this.preventCameraMotion = false;
    this.doWindowBackground = false;
    this.layer = (WindowLayer) 3;
    this.resizeable = this.ShowSearchBox;
  }

  public virtual Vector2 InitialSize => this.windowSize;

  protected virtual float Margin => 0.0f;

  private bool ShowSearchBox => this.items.Count > 6;

  private float SearchBoxHeight => !this.ShowSearchBox ? 0.0f : 30f;

  private float CreateButtonHeight => this.createItem == null ? 0.0f : 39f;

  private float ResizeableBtnHeight => !this.ShowSearchBox ? 0.0f : 30f;

  public virtual void PreOpen()
  {
    GenCollection.SortBy<T, string>(this.items, (Func<T, string>) (item => this.itemName(item)));
    this.windowSize = this.CalculateWindowSize();
    base.PreOpen();
  }

  public virtual void Notify_ClickOutsideWindow()
  {
    base.Notify_ClickOutsideWindow();
    this.Close(true);
  }

  private Vector2 CalculateWindowSize()
  {
    int num = Mathf.Min(6, this.items.Count);
    this.RecacheHeight();
    return new Vector2(this.width, 30f * (float) num + this.CreateButtonHeight + this.SearchBoxHeight + this.ResizeableBtnHeight);
  }

  private void RecacheHeight()
  {
    int num = 0;
    foreach (T obj in this.items)
    {
      if (!this.ShowSearchBox || this.filter.Matches(this.itemName(obj)))
        ++num;
    }
    this.fullHeight = 30f * (float) num + this.CreateButtonHeight;
  }

  protected virtual void SetInitialSizeAndPosition()
  {
    if ((double) this.position.x + (double) base.InitialSize.x > (double) UI.screenWidth)
      this.position.x = (float) UI.screenWidth - base.InitialSize.x;
    if ((double) this.position.y + (double) base.InitialSize.y > (double) UI.screenHeight)
      this.position.y = (float) UI.screenHeight - base.InitialSize.y;
    this.windowRect = new Rect(this.position.x, this.position.y, base.InitialSize.x, base.InitialSize.y);
  }

  public virtual void DoWindowContents(Rect inRect)
  {
    Widgets.DrawMenuSection(inRect);
    inRect = GenUI.ContractedBy(inRect, 2f);
    TextBlock textBlock;
    // ISSUE: explicit constructor call
    ((TextBlock) ref textBlock).\u002Ector((GameFont) 1);
    try
    {
      Rect rect1 = new Rect(((Rect) ref inRect).x, ((Rect) ref inRect).y, ((Rect) ref inRect).width, 30f);
      if (this.ShowSearchBox)
      {
        string str = Widgets.TextField(GenUI.ContractedBy(rect1, 2f), this.filter.Text);
        if (str != this.filter.Text)
        {
          this.filter.Text = str;
          this.RecacheHeight();
        }
        ref Rect local = ref rect1;
        ((Rect) ref local).y = ((Rect) ref local).y + ((Rect) ref rect1).height;
      }
      Mathf.FloorToInt(((Rect) ref inRect).width / this.width);
      Rect rect2 = new Rect(((Rect) ref inRect).x, ((Rect) ref inRect).y + this.SearchBoxHeight, ((Rect) ref inRect).width, ((Rect) ref inRect).height - this.SearchBoxHeight);
      Rect rect3 = new Rect(((Rect) ref rect2).x, ((Rect) ref rect2).y, ((Rect) ref rect2).width - 16f, this.fullHeight - 4f);
      Widgets.BeginScrollView(rect2, ref this.scrollPos, rect3, true);
      for (int index = 0; index < this.items.Count; ++index)
      {
        T obj = this.items[index];
        if (!this.ShowSearchBox || this.filter.Matches(this.itemName(obj)))
        {
          Rect rect4 = GenUI.ContractedBy(rect1, 3f);
          Rect rect5;
          Rect rect6;
          GenUI.SplitVertically(rect4, 30f, ref rect5, ref rect6);
          Rect rect7 = GenUI.ContractedBy(new Rect(((Rect) ref rect5).x, ((Rect) ref rect5).y, ((Rect) ref rect5).height, ((Rect) ref rect5).height), 3f);
          if (this.isSelected(obj))
            GUI.DrawTexture(rect7, (Texture) Widgets.CheckboxOnTex);
          if (Widgets.ButtonText(rect6, this.itemName(obj), false, true, true, new TextAnchor?()))
          {
            this.onItemPicked(obj);
            this.Close(true);
          }
          if (Mouse.IsOver(rect4))
          {
            Widgets.DrawBoxSolid(rect6, this.highlightColor);
            if (this.itemTooltip != null)
              TooltipHandler.TipRegion(rect6, TipSignal.op_Implicit(this.itemTooltip(obj)));
          }
          ref Rect local = ref rect1;
          ((Rect) ref local).y = ((Rect) ref local).y + ((Rect) ref rect1).height;
        }
      }
      if (this.createItem != null)
      {
        Rect rect8;
        Rect rect9;
        GenUI.SplitVertically(GenUI.ContractedBy(rect1, 3f), 30f, ref rect8, ref rect9);
        if (!this.items.NullOrEmpty<T>())
        {
          UIElements.DrawLineHorizontalGrey(((Rect) ref rect9).x, ((Rect) ref rect9).y, ((Rect) ref rect9).width);
          ref Rect local = ref rect9;
          ((Rect) ref local).y = ((Rect) ref local).y + 4f;
        }
        if (Widgets.ButtonText(rect9, TaggedString.op_Implicit(Translator.Translate(this.createItem.labelKey)), false, true, true, new TextAnchor?()))
        {
          T obj = this.createItem.onClick();
          Action<T> onItemPicked = this.onItemPicked;
          if (onItemPicked != null)
            onItemPicked(obj);
          this.Close(true);
        }
        if (Mouse.IsOver(rect9))
          Widgets.DrawBoxSolid(rect9, this.highlightColor);
      }
      Widgets.EndScrollView();
    }
    finally
    {
      textBlock.Dispose();
    }
  }

  public class CreateItemButton
  {
    public readonly string labelKey;
    public readonly Func<T> onClick;

    public CreateItemButton(string labelKey, Func<T> onClick)
    {
      this.labelKey = labelKey;
      this.onClick = onClick;
    }

    public static implicit operator Dialog_ItemDropdown<T>.CreateItemButton(
      (string labelKey, Func<T> onClick) tuple)
    {
      return new Dialog_ItemDropdown<T>.CreateItemButton(tuple.labelKey, tuple.onClick);
    }
  }
}
