// Decompiled with JetBrains decompiler
// Type: SmashTools.Xml.FilePaths
// Assembly: SmashTools, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 3B8999A7-6EA8-49BB-AA7F-3E2E898B5249
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\SmashTools.dll

using RimWorld;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Verse;

#nullable disable
namespace SmashTools.Xml;

[Obsolete("Do not use, will be removed in 1.7 and is currently non-functional", true)]
public static class FilePaths
{
  public static List<string> ModFoldersForVersion(ModContentPack mod)
  {
    ModMetaData modWithIdentifier = ModLister.GetModWithIdentifier(mod.PackageId, false);
    List<LoadFolder> loadFolderList1 = new List<LoadFolder>();
    if (modWithIdentifier?.loadFolders != null && modWithIdentifier.loadFolders.DefinedVersions().Count > 0)
    {
      List<LoadFolder> loadFolderList2 = modWithIdentifier.LoadFoldersForVersion(VersionControl.CurrentVersionStringWithoutBuild);
      if (!loadFolderList2.NullOrEmpty<LoadFolder>())
        return loadFolderList2.Select<LoadFolder, string>((Func<LoadFolder, string>) (lf => lf.folderName)).ToList<string>();
    }
    loadFolderList1 = new List<LoadFolder>();
    int major = VersionControl.CurrentVersion.Major;
    int num = VersionControl.CurrentVersion.Minor;
    List<LoadFolder> loadFolderList3;
    do
    {
      if (num == 0)
      {
        --major;
        num = 9;
      }
      else
        --num;
      if (major < 1)
      {
        List<LoadFolder> source = modWithIdentifier.LoadFoldersForVersion("default");
        return source != null ? source.Select<LoadFolder, string>((Func<LoadFolder, string>) (lf => lf.folderName)).ToList<string>() : FilePaths.DefaultFoldersForVersion(mod).ToList<string>();
      }
      loadFolderList3 = modWithIdentifier.LoadFoldersForVersion($"{major.ToString()}.{num.ToString()}");
    }
    while (loadFolderList3.NullOrEmpty<LoadFolder>());
    return loadFolderList3.Select<LoadFolder, string>((Func<LoadFolder, string>) (lf => lf.folderName)).ToList<string>();
  }

  public static IEnumerable<string> DefaultFoldersForVersion(ModContentPack mod)
  {
    ModMetaData modWithIdentifier = ModLister.GetModWithIdentifier(mod.PackageId, false);
    string rootDir = mod.RootDir;
    string path1 = Path.Combine(rootDir, VersionControl.CurrentVersionStringWithoutBuild);
    if (Directory.Exists(path1))
    {
      yield return path1;
    }
    else
    {
      Version version1 = new Version(0, 0);
      foreach (FileSystemInfo directory in modWithIdentifier.RootDir.GetDirectories())
      {
        Version version2;
        if (VersionControl.TryParseVersionString(directory.Name, ref version2) && version2 > version1)
          version1 = version2;
      }
      if (version1.Major > 0)
        yield return Path.Combine(rootDir, version1.ToString());
    }
    string path2 = Path.Combine(rootDir, ModContentPack.CommonFolderName);
    if (Directory.Exists(path2))
      yield return path2;
    yield return rootDir;
  }
}
