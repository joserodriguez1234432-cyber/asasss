// Decompiled with JetBrains decompiler
// Type: SmashTools.SmashSettings
// Assembly: SmashTools, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 3B8999A7-6EA8-49BB-AA7F-3E2E898B5249
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\SmashTools.dll

using System.Collections.Generic;
using System.IO;
using Verse;

#nullable disable
namespace SmashTools;

public class SmashSettings : IExposable
{
  public static string startupAction;
  public static QuickStartOption quickStartOption = QuickStartOption.None;
  public static string quickStartFile;
  public static HashSet<string> profileAssemblies = new HashSet<string>();

  public static string FullPath => Path.Combine(GenFilePaths.ConfigFolderPath, "SmashTools.xml");

  public void ExposeData()
  {
  }
}
