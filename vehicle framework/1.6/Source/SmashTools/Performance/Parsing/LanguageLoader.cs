// Decompiled with JetBrains decompiler
// Type: SmashTools.Performance.LanguageLoader
// Assembly: SmashTools, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 3B8999A7-6EA8-49BB-AA7F-3E2E898B5249
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\SmashTools.dll

using RimWorld.IO;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Verse;

#nullable disable
namespace SmashTools.Performance;

internal static class LanguageLoader
{
  public static void LoadMetaData(ref LoadedLanguage language)
  {
  }

  private static void LoadDirectory([RequiresLocation, In] ref LoadedLanguage language, string filePath)
  {
    foreach (VirtualDirectory directory in AbstractFilesystem.GetDirectories(filePath, "*", SearchOption.TopDirectoryOnly, false))
    {
      if (directory.Name == language.folderName || directory.Name == language.LegacyFolderName)
      {
        language.info = DirectXmlLoader.ItemFromXmlFile<LanguageInfo>(directory, "LanguageInfo.xml", false);
        if (language.info.friendlyNameNative.NullOrEmpty<char>() && directory.FileExists("FriendlyName.txt"))
          language.info.friendlyNameNative = directory.ReadAllText("FriendlyName.txt");
        if (language.info.friendlyNameNative.NullOrEmpty<char>())
          language.info.friendlyNameNative = language.folderName;
        if (!language.info.friendlyNameEnglish.NullOrEmpty<char>())
          break;
        language.info.friendlyNameEnglish = language.folderName;
        break;
      }
    }
  }
}
