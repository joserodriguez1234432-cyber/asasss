// Decompiled with JetBrains decompiler
// Type: SmashTools.QuickStartMenu
// Assembly: SmashTools, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 3B8999A7-6EA8-49BB-AA7F-3E2E898B5249
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\SmashTools.dll

using RimWorld;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using Verse;

#nullable disable
namespace SmashTools;

public class QuickStartMenu : Window
{
  public const float RowSize = 24f;
  private static List<string> presets = new List<string>();
  private static Listing_Standard lister = new Listing_Standard();

  public virtual Vector2 InitialSize => new Vector2(500f, 500f);

  public QuickStartMenu()
    : base((IWindowDrawing) null)
  {
    this.closeOnClickedOutside = true;
    this.closeOnCancel = true;
    this.closeOnAccept = true;
    this.doCloseX = true;
  }

  static QuickStartMenu()
  {
    LongEventHandler.ExecuteWhenFinished(new Action(QuickStartMenu.QuickStart));
  }

  public static void RegisterPreset()
  {
  }

  public virtual void PostClose()
  {
    base.PostClose();
    SmashMod.Serialize();
  }

  public virtual void DoWindowContents(Rect inRect)
  {
    ((Listing) QuickStartMenu.lister).Begin(inRect);
    QuickStartMenu.QuickStartRow("None", QuickStartOption.None);
    QuickStartMenu.QuickStartRow("New Game", QuickStartOption.QuickStart_New);
    ((Listing) QuickStartMenu.lister).End();
  }

  private static Rect QuickStartRow(
    string label,
    QuickStartOption quickStartOption,
    Action onClick = null)
  {
    Rect rect = ((Listing) QuickStartMenu.lister).GetRect(Text.LineHeight, 1f);
    if (QuickStartMenu.lister.ReverseRadioButton(label, SmashSettings.quickStartOption == quickStartOption))
    {
      SmashSettings.quickStartOption = quickStartOption;
      if (onClick != null)
        onClick();
    }
    return rect;
  }

  private static void QuickStart()
  {
    switch (SmashSettings.quickStartOption)
    {
      case QuickStartOption.QuickStart_Load:
        FileInfo fileInfo1 = GenFilePaths.AllSavedGameFiles.FirstOrDefault<FileInfo>((Func<FileInfo, bool>) (fileInfo => Path.GetFileNameWithoutExtension(fileInfo.Name).ToLower() == SmashSettings.quickStartFile));
        if (fileInfo1 == null)
          break;
        GameDataSaveLoader.LoadGame(fileInfo1);
        break;
      case QuickStartOption.QuickStart_New:
        LongEventHandler.QueueLongEvent((Action) (() =>
        {
          Root_Play.SetupForQuickTestPlay();
          PageUtility.InitGameStart();
        }), "GeneratingMap", true, new Action<Exception>(GameAndMapInitExceptionHandlers.ErrorWhileGeneratingMap), true, false, (Action) null);
        break;
    }
  }
}
