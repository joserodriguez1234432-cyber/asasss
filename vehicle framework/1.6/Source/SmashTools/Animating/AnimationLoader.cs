// Decompiled with JetBrains decompiler
// Type: SmashTools.Animations.AnimationLoader
// Assembly: SmashTools, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 3B8999A7-6EA8-49BB-AA7F-3E2E898B5249
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\SmashTools.dll

using RimWorld;
using SmashTools.Xml;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using Verse;

#nullable disable
namespace SmashTools.Animations;

public static class AnimationLoader
{
  private const string AnimationFolderName = "Animations";
  private const string AnimationFolder = "Animations/";
  private static readonly Dictionary<Type, string> FileExtensions = new Dictionary<Type, string>()
  {
    {
      typeof (AnimationClip),
      ".rwa"
    },
    {
      typeof (AnimationController),
      ".ctlr"
    }
  };
  private static readonly bool LoadedAll;

  static AnimationLoader()
  {
    ParseHelper.Parsers<AnimationClip>.Register(new Func<string, AnimationClip>(AnimationLoader.ParseAnimationFileByGuid<AnimationClip>));
    ParseHelper.Parsers<AnimationController>.Register(new Func<string, AnimationController>(AnimationLoader.ParseAnimationFileByPath<AnimationController>));
    AnimationLoader.LoadAll();
  }

  private static void LoadAll()
  {
    foreach (ModContentPack mod in LoadedModManager.RunningModsListForReading)
    {
      AnimationLoader.LoadAnimationFilePaths<AnimationClip>(mod);
      AnimationLoader.LoadAnimationFilePaths<AnimationController>(mod);
    }
  }

  internal static void ResolveAllReferences()
  {
    AnimationLoader.Cache<AnimationClip>.ResolveReferences();
    AnimationLoader.Cache<AnimationController>.ResolveReferences();
  }

  private static void LoadAnimationFilePaths<T>(ModContentPack mod) where T : IAnimationFile, new()
  {
    // ISSUE: reference to a compiler-generated field
    // ISSUE: reference to a compiler-generated field
    foreach (KeyValuePair<string, FileInfo> keyValuePair in ModContentPack.GetAllFilesForMod(mod, "Animations/", AnimationLoader.\u003CLoadAnimationFilePaths\u003EO__7_0<T>.\u003C0\u003E__IsAcceptableExtension ?? (AnimationLoader.\u003CLoadAnimationFilePaths\u003EO__7_0<T>.\u003C0\u003E__IsAcceptableExtension = new Func<string, bool>(AnimationLoader.IsAcceptableExtension<T>)), (List<string>) null))
    {
      string str1;
      FileInfo fileInfo;
      keyValuePair.Deconstruct(ref str1, ref fileInfo);
      string str2 = str1;
      T file = AnimationLoader.LoadFile<T>(fileInfo.FullName);
      string path = str2;
      if (Path.HasExtension(path))
        path = Path.GetFileNameWithoutExtension(path);
      AnimationLoader.Cache<T>.Add(path, file);
    }
  }

  private static bool IsAcceptableExtension<T>(string ext)
  {
    string str;
    return AnimationLoader.FileExtensions.TryGetValue(typeof (T), out str) && ext == str;
  }

  private static T ParseAnimationFileByPath<T>(string filePath) where T : IAnimationFile, new()
  {
    return AnimationLoader.LoadFile<T>(filePath);
  }

  private static T ParseAnimationFileByGuid<T>(string guidStr) where T : IAnimationFile, new()
  {
    Guid result;
    T file;
    if (Guid.TryParse(guidStr, out result) && AnimationLoader.Cache<T>.Get(result, out file))
      return file;
    Log.Error("Unable to load animation file " + guidStr);
    return default (T);
  }

  public static T LoadFile<T>(string filePath) where T : IAnimationFile, new()
  {
    T file;
    if (AnimationLoader.Cache<T>.Get(filePath, out file))
      return file;
    if (!File.Exists(filePath))
    {
      Log.Error($"Unable to load file at \"{filePath}\". File not found.");
      return default (T);
    }
    T obj1 = AnimationLoader.LoadFileFromXml<T>(filePath);
    if ((object) obj1 == null)
    {
      Log.Error($"Unable to load animation file at \"{filePath}\".");
      return default (T);
    }
    obj1.FilePath = filePath;
    ref T local = ref obj1;
    if ((object) default (T) == null)
    {
      T obj2 = local;
      local = ref obj2;
    }
    string withoutExtension = Path.GetFileNameWithoutExtension(filePath);
    local.FileName = withoutExtension;
    if (AnimationLoader.LoadedAll)
      obj1.ResolveReferences();
    return obj1;
  }

  private static T LoadFileFromXml<T>(string filePath) where T : IAnimationFile, new()
  {
    if (!File.Exists(filePath))
      return default (T);
    XmlDocument xmlDocument = new XmlDocument();
    xmlDocument.LoadXml(File.ReadAllText(filePath));
    return DirectXmlToObject.ObjectFromXml<T>((XmlNode) xmlDocument.DocumentElement, true);
  }

  public static bool Save<T>(T file) where T : IAnimationFile, new()
  {
    if ((object) file == null)
      return false;
    if (file.FilePath == null || !File.Exists(file.FilePath))
    {
      AnimationLoader.SaveAs<T>(file);
      return false;
    }
    AnimationLoader.ExportXml<T>(file);
    return true;
  }

  public static void SaveAs<T>(T file) where T : IAnimationFile, new()
  {
    if ((object) file == null)
      return;
    Find.WindowStack.Add((Window) new Dialog_FilePicker((TaggedString.op_Implicit(Translator.Translate("Save")), (Action<DirectoryInfo>) (dir => AnimationLoader.ExportXmlToDirectory<T>(file, dir)))));
  }

  private static void ExportXmlToDirectory<T>(T file, DirectoryInfo directory) where T : IAnimationFile, new()
  {
    ref T local = ref file;
    if ((object) default (T) == null)
    {
      T obj = local;
      local = ref obj;
    }
    string str = Path.Combine(directory.FullName, file.FileNameWithExtension);
    local.FilePath = str;
    AnimationLoader.ExportXml<T>(file);
  }

  private static void ExportXml<T>(T file) where T : IAnimationFile, new()
  {
    bool flag = true;
    try
    {
      XmlExporter.StartDocument(file.FilePath);
      XmlExporter.WriteElement(file.GetType().Name, (IXmlExport) file);
    }
    catch (IOException ex)
    {
      flag = false;
      Log.Error($"Unable to export animation data.\nException = {ex}");
      Messages.Message($"Failed to save {file.FileName}.", MessageTypeDefOf.RejectInput, true);
    }
    finally
    {
      XmlExporter.Close();
    }
    if (!flag)
      return;
    Messages.Message($"{file.FileName} successfully saved at {file.FilePath}", MessageTypeDefOf.TaskCompletion, true);
  }

  public static string GetAvailableName(IEnumerable<string> takenNames, string defaultName)
  {
    string availableName = defaultName;
    for (int index = 0; index < 100; ++index)
    {
      bool flag = true;
      foreach (string takenName in takenNames)
      {
        if (takenName == availableName)
        {
          flag = false;
          break;
        }
      }
      if (flag)
        return availableName;
      availableName = $"{defaultName} {index}";
    }
    return $"{defaultName} {Rand.Range(100000, 999999)}";
  }

  internal static class Cache<T> where T : IAnimationFile
  {
    private static readonly Dictionary<string, T> Files = new Dictionary<string, T>();
    private static readonly Dictionary<Guid, T> FilesByGuid = new Dictionary<Guid, T>();

    public static int Count => AnimationLoader.Cache<T>.Files.Count;

    public static List<T> GetAll() => AnimationLoader.Cache<T>.Files.Values.ToList<T>();

    public static void Add(string path, T file)
    {
      AnimationLoader.Cache<T>.Files[path] = file;
      AnimationLoader.Cache<T>.FilesByGuid[file.Guid] = file;
    }

    public static bool Get(string path, out T file)
    {
      return AnimationLoader.Cache<T>.Files.TryGetValue(path, out file);
    }

    public static bool Get(Guid guid, out T file)
    {
      return AnimationLoader.Cache<T>.FilesByGuid.TryGetValue(guid, out file);
    }

    public static void ResolveReferences()
    {
      foreach (T obj in AnimationLoader.Cache<T>.Files.Values)
        obj.ResolveReferences();
    }
  }
}
