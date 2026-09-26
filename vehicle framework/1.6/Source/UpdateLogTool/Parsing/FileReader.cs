// Decompiled with JetBrains decompiler
// Type: UpdateLogTool.FileReader
// Assembly: UpdateLogTool, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: C0B0D1B9-DF61-4E60-8ED1-86C03A6FDDFD
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\UpdateLogTool.dll

using JetBrains.Annotations;
using RimWorld;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml;
using UnityEngine;
using UnityEngine.Networking;
using Verse;

#nullable disable
namespace UpdateLogTool;

[PublicAPI]
public static class FileReader
{
  public const string UpdateLogFolder = "Updates";
  public const string UpdateLogOldFolder = "Previous";
  public const string UpdateLogFileName = "UpdateLog.xml";
  public const string UpdateLogImageFolder = "Images";
  public const string UpdateLogGifFolder = "Gifs";

  public static string UpdateLogDirectory(ModContentPack mod, string folderName)
  {
    return Path.Combine(mod.RootDir, folderName, "Updates");
  }

  public static string UpdateLogOldDirectory(ModContentPack mod, string folderName)
  {
    return Path.Combine(mod.RootDir, folderName, "Updates", "Previous");
  }

  public static string UpdateImagesDirectory(ModContentPack mod, string folderName)
  {
    return Path.Combine(mod.RootDir, folderName, "Updates", "Images");
  }

  public static string UpdateImagesDirectory(UpdateLog log)
  {
    return FileReader.UpdateImagesDirectory(log.Mod, log.CurrentFolder);
  }

  public static string UpdateGifDirectory(ModContentPack mod, string folderName)
  {
    return Path.Combine(mod.RootDir, folderName, "Updates", "Gifs");
  }

  public static string UpdateGifDirectory(UpdateLog log)
  {
    return FileReader.UpdateImagesDirectory(log.Mod, log.CurrentFolder);
  }

  public static UpdateLog LoadUpdateLog(ModContentPack mod)
  {
    try
    {
      List<string> stringList = FileReader.ModFoldersForVersion(mod);
      if (!GenList.NullOrEmpty<string>((IList<string>) stringList))
      {
        foreach (string str in stringList)
        {
          if (File.Exists(Path.Combine(FileReader.UpdateLogDirectory(mod, str), "UpdateLog.xml")))
            return new UpdateLog(mod, str);
        }
      }
    }
    catch (Exception ex)
    {
      Log.Error($"Exception thrown while attempting to read in UpdateLog data for {mod.Name}.\nException=\"{ex}\"");
    }
    return (UpdateLog) null;
  }

  public static List<UpdateLog> ReadPreviousFiles(this ModContentPack mod)
  {
    List<UpdateLog> updateLogList = new List<UpdateLog>();
    try
    {
      List<string> stringList = FileReader.ModFoldersForVersion(mod);
      if (GenList.NullOrEmpty<string>((IList<string>) stringList))
        return updateLogList;
      foreach (string str in stringList)
      {
        if (Directory.Exists(FileReader.UpdateLogDirectory(mod, str)))
        {
          if (File.Exists(Path.Combine(FileReader.UpdateLogDirectory(mod, str), "UpdateLog.xml")))
            updateLogList.Add(new UpdateLog(mod, str));
          if (Directory.Exists(FileReader.UpdateLogOldDirectory(mod, str)))
          {
            foreach (string enumerateFile in Directory.EnumerateFiles(FileReader.UpdateLogOldDirectory(mod, str), "*.xml"))
            {
              if (File.Exists(enumerateFile))
                updateLogList.Add(new UpdateLog(mod, str, enumerateFile, false));
            }
          }
        }
      }
    }
    catch (Exception ex)
    {
      Log.Error($"Exception thrown while attempting to read in UpdateLog data for {mod.Name}.\nException=\"{ex}\"");
    }
    return updateLogList;
  }

  public static List<string> ModFoldersForVersion(ModContentPack mod)
  {
    ModMetaData modWithIdentifier = ModLister.GetActiveModWithIdentifier(mod.PackageId, false);
    if (modWithIdentifier == null)
    {
      Log.Warning("Unable to load folders for " + mod.PackageId);
      return (List<string>) null;
    }
    if (modWithIdentifier.loadFolders != null && modWithIdentifier.loadFolders.DefinedVersions().Count > 0)
    {
      List<LoadFolder> source = modWithIdentifier.LoadFoldersForVersion(VersionControl.CurrentVersionStringWithoutBuild);
      if (!GenList.NullOrEmpty<LoadFolder>((IList<LoadFolder>) source))
        return source.Select<LoadFolder, string>((Func<LoadFolder, string>) (lf => lf.folderName)).ToList<string>();
    }
    int major = VersionControl.CurrentVersion.Major;
    int num = VersionControl.CurrentVersion.Minor;
    List<LoadFolder> source1;
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
        List<LoadFolder> source2 = modWithIdentifier.LoadFoldersForVersion("default");
        return source2 != null ? source2.Select<LoadFolder, string>((Func<LoadFolder, string>) (lf => lf.folderName)).ToList<string>() : FileReader.DefaultFoldersForVersion(mod).ToList<string>();
      }
      source1 = modWithIdentifier.LoadFoldersForVersion($"{major}.{num}");
    }
    while (GenList.NullOrEmpty<LoadFolder>((IList<LoadFolder>) source1));
    return source1.Select<LoadFolder, string>((Func<LoadFolder, string>) (lf => lf.folderName)).ToList<string>();
  }

  public static IEnumerable<string> DefaultFoldersForVersion(ModContentPack mod)
  {
    ModMetaData modMetaData = mod.ModMetaData;
    string rootDir = mod.RootDir;
    string path1 = Path.Combine(rootDir, VersionControl.CurrentVersionStringWithoutBuild);
    if (Directory.Exists(path1))
    {
      yield return path1;
    }
    else
    {
      Version version1 = new Version(0, 0);
      foreach (FileSystemInfo directory in modMetaData.RootDir.GetDirectories())
      {
        Version version2;
        if (VersionControl.TryParseVersionString(directory.Name, ref version2) && version2 > version1)
          version1 = version2;
      }
      if (version1.Major > 0)
        yield return Path.Combine(rootDir, version1.ToString());
    }
    string path2 = Path.Combine(rootDir, ModContentPack.CommonFolderName);
    yield return Directory.Exists(path2) ? path2 : rootDir;
  }

  public static UpdateLog.UpdateLogData ParseUpdateData(string filePath)
  {
    string xml = File.ReadAllText(filePath);
    UpdateLog.UpdateLogData updateData = new UpdateLog.UpdateLogData();
    try
    {
      XmlDocument xmlDocument = new XmlDocument();
      xmlDocument.LoadXml(xml);
      foreach (XmlNode childNode in xmlDocument.DocumentElement.ChildNodes)
      {
        string name = childNode.Name;
        if (name != null)
        {
          switch (name.Length)
          {
            case 6:
              switch (name[0])
              {
                case 'i':
                  if (name == "images")
                  {
                    updateData.images = FileReader.ImageListFromXml(childNode);
                    continue;
                  }
                  break;
                case 'u':
                  if (name == "update")
                  {
                    bool result;
                    updateData.update = bool.TryParse(childNode.InnerText, out result) & result;
                    continue;
                  }
                  break;
              }
              break;
            case 7:
              if (name == "testing")
              {
                bool result;
                updateData.testing = bool.TryParse(childNode.InnerText, out result) & result;
                continue;
              }
              break;
            case 8:
              switch (name[0])
              {
                case '#':
                  if (name == "#comment")
                    continue;
                  break;
                case 'u':
                  if (name == "updateOn")
                  {
                    updateData.updateOn = (UpdateFor) Enum.Parse(typeof (UpdateFor), childNode.InnerText);
                    continue;
                  }
                  break;
              }
              break;
            case 11:
              switch (name[0])
              {
                case 'd':
                  if (name == "description")
                  {
                    updateData.description = childNode.InnerText;
                    continue;
                  }
                  break;
                case 'l':
                  if (name == "leftIconBar")
                  {
                    updateData.leftIconBar = FileReader.ListFromXml(childNode);
                    continue;
                  }
                  break;
              }
              break;
            case 12:
              if (name == "rightIconBar")
              {
                updateData.rightIconBar = FileReader.ListFromXml(childNode);
                continue;
              }
              break;
            case 14:
              switch (name[0])
              {
                case 'a':
                  if (name == "actionOnUpdate")
                  {
                    updateData.actionOnUpdate = childNode.InnerText;
                    continue;
                  }
                  break;
                case 'c':
                  if (name == "currentVersion")
                  {
                    updateData.currentVersion = childNode.InnerText;
                    continue;
                  }
                  break;
              }
              break;
          }
        }
        Log.Error($"Failed to find {childNode.Name} in manual parsing.");
      }
    }
    catch (Exception ex)
    {
      Log.Error($"Exception loading file at {filePath}. Loading defaults instead. Exception={ex}");
    }
    return updateData;
  }

  private static List<UpdateLog.UpdateLogData.HyperlinkedIcon> ListFromXml(XmlNode listRootNode)
  {
    List<UpdateLog.UpdateLogData.HyperlinkedIcon> hyperlinkedIconList = new List<UpdateLog.UpdateLogData.HyperlinkedIcon>();
    try
    {
      foreach (XmlNode childNode in listRootNode.ChildNodes)
      {
        try
        {
          hyperlinkedIconList.Add(DirectXmlToObject.ObjectFromXml<UpdateLog.UpdateLogData.HyperlinkedIcon>(childNode, true));
        }
        catch (Exception ex)
        {
          Log.Error($"Exception loading list element from XML. Ex={ex}\nXml={listRootNode.OuterXml}");
        }
      }
    }
    catch (Exception ex)
    {
      Log.Error($"Exception loading list element from XML. Ex={ex.Message}\nXml={listRootNode.OuterXml}");
    }
    return hyperlinkedIconList;
  }

  private static List<UpdateLog.UpdateLogData.UploadedImages> ImageListFromXml(XmlNode listRootNode)
  {
    List<UpdateLog.UpdateLogData.UploadedImages> uploadedImagesList = new List<UpdateLog.UpdateLogData.UploadedImages>();
    try
    {
      foreach (XmlNode childNode in listRootNode.ChildNodes)
      {
        try
        {
          uploadedImagesList.Add(DirectXmlToObject.ObjectFromXml<UpdateLog.UpdateLogData.UploadedImages>(childNode, true));
        }
        catch (Exception ex)
        {
          Log.Error($"Exception loading list element from XML. Ex={ex}\nXml={listRootNode.OuterXml}");
        }
      }
    }
    catch (Exception ex)
    {
      Log.Error($"Exception loading list element from XML. Ex={ex}\nXml={listRootNode.OuterXml}");
    }
    return uploadedImagesList;
  }

  public static async Task<Texture2D> GetTextureFromURL(string url)
  {
    UnityWebRequestAsyncOperation operation;
    Texture2D content;
    using (UnityWebRequest webRequest = UnityWebRequestTexture.GetTexture(url))
    {
      operation = webRequest.SendWebRequest();
      while (!((AsyncOperation) operation).isDone)
        await Task.Delay(33);
      content = webRequest.result - 2 > 1 ? DownloadHandlerTexture.GetContent(webRequest) : (Texture2D) null;
    }
    operation = (UnityWebRequestAsyncOperation) null;
    return content;
  }
}
