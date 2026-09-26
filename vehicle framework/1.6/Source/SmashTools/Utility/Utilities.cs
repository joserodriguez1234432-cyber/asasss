// Decompiled with JetBrains decompiler
// Type: SmashTools.Utilities
// Assembly: SmashTools, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 3B8999A7-6EA8-49BB-AA7F-3E2E898B5249
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\SmashTools.dll

using System;
using System.IO;
using Verse;

#nullable disable
namespace SmashTools;

public static class Utilities
{
  public static void DeleteConfig(Mod mod)
  {
    string path = Path.Combine(GenFilePaths.ConfigFolderPath, GenText.SanitizeFilename($"Mod_{mod.Content.FolderName}_{mod.GetType().Name}.xml"));
    if (!File.Exists(path))
      return;
    File.Delete(path);
  }

  internal static void DeleteSettings()
  {
    string fullPath = SmashSettings.FullPath;
    if (!File.Exists(fullPath))
      return;
    File.Delete(fullPath);
  }

  public static void InvokeWithLogging(this Action action)
  {
    try
    {
      action();
    }
    catch (Exception ex)
    {
      Log.Error($"Unable to execute {action.Method.Name} Exception={ex}");
    }
  }

  public delegate void ActionRef<T>(ref T item);

  public delegate void ActionRefP1<T1, in T2>(ref T1 item1, T2 item2);

  public delegate void ActionRefP2<in T1, T2>(T1 item1, ref T2 item2);

  public delegate void ActionRef<T1, T2>(ref T1 item1, ref T2 item2);
}
