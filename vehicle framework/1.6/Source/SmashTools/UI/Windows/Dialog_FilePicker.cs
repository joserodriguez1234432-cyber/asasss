// Decompiled with JetBrains decompiler
// Type: SmashTools.Dialog_FilePicker
// Assembly: SmashTools, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 3B8999A7-6EA8-49BB-AA7F-3E2E898B5249
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\SmashTools.dll

using RimWorld;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Verse;
using Verse.Sound;

#nullable disable
namespace SmashTools;

public class Dialog_FilePicker : Window
{
  private float EntryHeight = 24f;
  private float IndentSpace = 24f;
  private (string text, Action<DirectoryInfo> onClick) confirmBtn;
  private string rootDir;
  private Dialog_FilePicker.DirectoryEntry selectedEntry;
  private float height;
  private Vector2 scrollPos;

  public Dialog_FilePicker(
    (string text, Action<DirectoryInfo> onClick) confirmBtn,
    string rootDir = null)
    : base((IWindowDrawing) null)
  {
    this.confirmBtn = confirmBtn;
    this.rootDir = rootDir;
    this.layer = (WindowLayer) 3;
    this.closeOnAccept = false;
    this.closeOnClickedOutside = false;
    this.doWindowBackground = false;
    this.drawShadow = false;
    this.forcePause = false;
    this.onlyOneOfTypeAllowed = true;
    this.preventCameraMotion = false;
    this.resizeable = true;
  }

  public virtual Vector2 InitialSize => new Vector2(640f, 640f);

  protected virtual float Margin => 0.0f;

  private DirectoryInfo RootDirectory { get; set; }

  private Dialog_FilePicker.DirectoryEntry RootEntry { get; set; }

  public virtual void PreOpen()
  {
    base.PreOpen();
    if (!Directory.Exists(this.rootDir))
      this.rootDir = GenFilePaths.ModsFolderPath;
    this.RootDirectory = new DirectoryInfo(this.rootDir);
    this.RootEntry = new Dialog_FilePicker.DirectoryEntry(this.RootDirectory);
    this.RootEntry.Expanded = true;
  }

  private void GetSubDirectories(Dialog_FilePicker.DirectoryEntry entry)
  {
    entry.Fetched = true;
    try
    {
      foreach (DirectoryInfo directory in entry.directory.GetDirectories("*", SearchOption.TopDirectoryOnly))
      {
        Dialog_FilePicker.DirectoryEntry directoryEntry = new Dialog_FilePicker.DirectoryEntry(directory);
        entry.subDirectories.Add(directoryEntry);
      }
    }
    catch (UnauthorizedAccessException ex)
    {
      entry.Unauthorized = true;
    }
  }

  public virtual void DoWindowContents(Rect inRect)
  {
    TextBlock textBlock;
    // ISSUE: explicit constructor call
    ((TextBlock) ref textBlock).\u002Ector(Color.white);
    try
    {
      Widgets.DrawMenuSection(inRect);
      Rect rect1 = GenUI.ContractedBy(inRect, 5f);
      ref Rect local1 = ref rect1;
      ((Rect) ref local1).yMax = ((Rect) ref local1).yMax - 40f;
      Widgets.DrawLineHorizontal(((Rect) ref rect1).x, ((Rect) ref rect1).yMax, ((Rect) ref rect1).width, UIElements.MenuSectionBgBorderColor);
      Rect rect2 = rect1;
      Rect rect3 = new Rect(((Rect) ref rect2).x, ((Rect) ref rect2).y, ((Rect) ref rect2).width - 16f, this.height);
      Widgets.BeginScrollView(rect2, ref this.scrollPos, rect3, true);
      Rect rect4 = GenUI.AtZero(rect1);
      ((Rect) ref rect4).height = this.EntryHeight;
      this.height = this.DoRow(ref rect4, this.RootEntry);
      Widgets.EndScrollView();
      Rect rect5 = new Rect(((Rect) ref inRect).xMax - 105f, ((Rect) ref inRect).yMax - 40f, 90f, 30f);
      if (Widgets.ButtonText(rect5, TaggedString.op_Implicit(Translator.Translate("Cancel")), true, true, true, new TextAnchor?()))
      {
        SoundStarter.PlayOneShotOnCamera(SoundDefOf.Click, (Map) null);
        this.Close(true);
      }
      ref Rect local2 = ref rect5;
      ((Rect) ref local2).x = ((Rect) ref local2).x - (((Rect) ref rect5).width + 10f);
      if (!Widgets.ButtonText(rect5, this.confirmBtn.text, true, true, true, new TextAnchor?()))
        return;
      if (this.selectedEntry == null)
      {
        SoundStarter.PlayOneShotOnCamera(SoundDefOf.ClickReject, (Map) null);
      }
      else
      {
        this.confirmBtn.onClick(this.selectedEntry.directory);
        this.Close(true);
      }
    }
    finally
    {
      textBlock.Dispose();
    }
  }

  private float DoRow(ref Rect rect, Dialog_FilePicker.DirectoryEntry entry)
  {
    if (!entry.Fetched)
      this.GetSubDirectories(entry);
    bool expanded = entry.Expanded;
    Rect rect1;
    // ISSUE: explicit constructor call
    ((Rect) ref rect1).\u002Ector(((Rect) ref rect).x, ((Rect) ref rect).y, ((Rect) ref rect).height, ((Rect) ref rect).height);
    if (!entry.Unauthorized && entry.subDirectories.Count > 0 && UIElements.CollapseButton(GenUI.ContractedBy(rect1, 2f), ref expanded))
    {
      entry.Expanded = expanded;
      PlayTabSound(entry.Expanded);
    }
    Text.Anchor = (TextAnchor) 3;
    Rect rect2 = rect;
    ((Rect) ref rect2).xMin = ((Rect) ref rect1).xMax;
    string str = !entry.Unauthorized ? entry.directory.Name : entry.directory.Name + " (UNAUTHORIZED)";
    if (Widgets.ButtonText(rect2, str, false, false, true, new TextAnchor?()))
    {
      if (this.selectedEntry == entry)
      {
        entry.Expanded = !entry.Expanded;
        PlayTabSound(entry.Expanded);
      }
      this.selectedEntry = entry;
    }
    if (this.selectedEntry == entry)
      Widgets.DrawBoxSolid(rect, Widgets.HighlightTextBgColor);
    float num = 0.0f;
    if (entry.Expanded)
    {
      ref Rect local1 = ref rect;
      ((Rect) ref local1).xMin = ((Rect) ref local1).xMin + this.IndentSpace;
      foreach (Dialog_FilePicker.DirectoryEntry subDirectory in entry.subDirectories)
      {
        ref Rect local2 = ref rect;
        ((Rect) ref local2).y = ((Rect) ref local2).y + this.EntryHeight;
        num += this.DoRow(ref rect, subDirectory);
      }
      ref Rect local3 = ref rect;
      ((Rect) ref local3).xMin = ((Rect) ref local3).xMin - this.IndentSpace;
    }
    return this.EntryHeight + num;

    static void PlayTabSound(bool open)
    {
      if (open)
        SoundStarter.PlayOneShotOnCamera(SoundDefOf.TabOpen, (Map) null);
      else
        SoundStarter.PlayOneShotOnCamera(SoundDefOf.TabClose, (Map) null);
    }
  }

  private class DirectoryEntry
  {
    public readonly DirectoryInfo directory;
    public readonly List<Dialog_FilePicker.DirectoryEntry> subDirectories = new List<Dialog_FilePicker.DirectoryEntry>();

    public DirectoryEntry(DirectoryInfo directory) => this.directory = directory;

    public bool Expanded { get; set; }

    public bool Unauthorized { get; set; }

    public bool Fetched { get; set; }
  }
}
