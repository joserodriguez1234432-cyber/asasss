// Decompiled with JetBrains decompiler
// Type: UpdateLogTool.UpdateHandler
// Assembly: UpdateLogTool, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: C0B0D1B9-DF61-4E60-8ED1-86C03A6FDDFD
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\UpdateLogTool.dll

using System;
using System.Collections.Generic;
using System.Linq;
using Verse;

#nullable disable
namespace UpdateLogTool;

public static class UpdateHandler
{
  public static readonly HashSet<UpdateLog> modUpdates = new HashSet<UpdateLog>();
  public static readonly HashSet<UpdateLog> updateList = new HashSet<UpdateLog>();

  public static void LoadUpdateLog(ModContentPack mod)
  {
    UpdateLog updateLog = FileReader.LoadUpdateLog(mod);
    if (updateLog == (UpdateLog) null)
    {
      Log.Error("Unable to load update log from " + mod.Name);
    }
    else
    {
      UpdateHandler.modUpdates.Add(updateLog);
      SegmentParser.ParseAndCreateSegments();
      SegmentParser.GenerateRegexText();
    }
  }

  public static UpdateLog UpdateLogData(ModContentPack mod)
  {
    return UpdateHandler.modUpdates.FirstOrDefault<UpdateLog>((Func<UpdateLog, bool>) (m => m.Mod == mod));
  }

  public static void CheckUpdates(UpdateFor updating)
  {
    UpdateHandler.updateList.Clear();
    foreach (UpdateLog modUpdate in UpdateHandler.modUpdates)
    {
      if (modUpdate.UpdateData.updateOn == updating)
      {
        if (modUpdate.UpdateData.update)
        {
          modUpdate.NotifyModUpdated();
          if (!GenText.NullOrEmpty(modUpdate.UpdateData.description))
            UpdateHandler.updateList.Add(modUpdate);
        }
        modUpdate.SaveUpdateStatus();
      }
    }
    if (!GenCollection.Any<UpdateLog>(UpdateHandler.updateList))
      return;
    Find.WindowStack.Add((Window) new Dialog_NewUpdate(UpdateHandler.updateList));
  }
}
