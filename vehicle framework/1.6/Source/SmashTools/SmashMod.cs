// Decompiled with JetBrains decompiler
// Type: SmashTools.SmashMod
// Assembly: SmashTools, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 3B8999A7-6EA8-49BB-AA7F-3E2E898B5249
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\SmashTools.dll

using System;
using System.IO;
using Verse;

#nullable disable
namespace SmashTools;

public class SmashMod : Mod
{
  public static SmashSettings settings;
  public static SmashMod mod;

  public SmashMod(ModContentPack modContentPack)
    : base(modContentPack)
  {
    SmashMod.mod = this;
    SmashMod.settings = new SmashSettings();
  }

  public static void Serialize()
  {
    Scribe.saver.InitSaving(SmashSettings.FullPath, "SmashSettings");
    try
    {
      Scribe_Deep.Look<SmashSettings>(ref SmashMod.settings, "SmashSettings", Array.Empty<object>());
    }
    finally
    {
      Scribe.saver.FinalizeSaving();
    }
  }

  public static void LoadFromSettings()
  {
    if (!File.Exists(SmashSettings.FullPath))
      return;
    Scribe.loader.InitLoading(SmashSettings.FullPath);
    try
    {
      Scribe_Deep.Look<SmashSettings>(ref SmashMod.settings, "SmashSettings", Array.Empty<object>());
    }
    finally
    {
      Scribe.loader.FinalizeLoading();
    }
  }
}
