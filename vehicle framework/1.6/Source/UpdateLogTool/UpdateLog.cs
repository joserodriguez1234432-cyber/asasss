// Decompiled with JetBrains decompiler
// Type: UpdateLogTool.UpdateLog
// Assembly: UpdateLogTool, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: C0B0D1B9-DF61-4E60-8ED1-86C03A6FDDFD
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\UpdateLogTool.dll

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Xml.Linq;
using UnityEngine;
using Verse;

#nullable disable
namespace UpdateLogTool;

public class UpdateLog : IDisposable
{
  public static string[] AllowedImageExtensions = new string[5]
  {
    ".png",
    ".jpg",
    ".jpeg",
    ".psd",
    ".bmp"
  };
  public Dictionary<string, Texture2D> cachedTextures = new Dictionary<string, Texture2D>();
  public Dictionary<string, WebTexture> cachedDownloadedTextures = new Dictionary<string, WebTexture>();

  public ModContentPack Mod { get; private set; }

  public UpdateLog.UpdateLogData UpdateData { get; private set; }

  public string CurrentFolder { get; private set; }

  public bool Disposed { get; private set; }

  public UpdateLog(ModContentPack mod, string loadFolder, string path, bool updateVersion = true)
  {
    this.Mod = mod;
    this.CurrentFolder = loadFolder;
    this.UpdateData = FileReader.ParseUpdateData(path);
    if (updateVersion)
      this.UpdateData.currentVersion = this.UpdateLogVersionFile(this.UpdateData.currentVersion);
    UpdateLog.UpdateLogData updateData1 = this.UpdateData;
    if (updateData1.rightIconBar == null)
      updateData1.rightIconBar = new List<UpdateLog.UpdateLogData.HyperlinkedIcon>();
    UpdateLog.UpdateLogData updateData2 = this.UpdateData;
    if (updateData2.leftIconBar == null)
      updateData2.leftIconBar = new List<UpdateLog.UpdateLogData.HyperlinkedIcon>();
    UpdateLog.UpdateLogData updateData3 = this.UpdateData;
    if (updateData3.images == null)
      updateData3.images = new List<UpdateLog.UpdateLogData.UploadedImages>();
    this.Disposed = true;
  }

  public UpdateLog(ModContentPack mod, string loadFolder, bool updateVersion = true)
  {
    this.Mod = mod;
    this.CurrentFolder = loadFolder;
    this.UpdateData = FileReader.ParseUpdateData(Path.Combine(FileReader.UpdateLogDirectory(this.Mod, this.CurrentFolder), "UpdateLog.xml"));
    if (updateVersion)
      this.UpdateData.currentVersion = this.UpdateLogVersionFile(this.UpdateData.currentVersion);
    if (this.UpdateData.rightIconBar == null)
      this.UpdateData.rightIconBar = new List<UpdateLog.UpdateLogData.HyperlinkedIcon>();
    if (this.UpdateData.leftIconBar == null)
      this.UpdateData.leftIconBar = new List<UpdateLog.UpdateLogData.HyperlinkedIcon>();
    this.Disposed = true;
  }

  public void Open()
  {
    if (!this.Disposed)
      this.Dispose();
    this.CacheImages();
    this.DownloadImages();
    this.Disposed = false;
  }

  public void Dispose()
  {
    foreach (Object @object in this.cachedTextures.Values)
      Object.Destroy(@object);
    foreach (WebTexture webTexture in this.cachedDownloadedTextures.Values)
      webTexture.Dispose();
    this.cachedTextures.Clear();
    this.Disposed = true;
  }

  private void CacheImages()
  {
    if (!Directory.Exists(FileReader.UpdateImagesDirectory(this.Mod, this.CurrentFolder)))
      return;
    foreach (string file in Directory.GetFiles(FileReader.UpdateImagesDirectory(this.Mod, this.CurrentFolder), "*", SearchOption.AllDirectories))
    {
      if (((IEnumerable<string>) UpdateLog.AllowedImageExtensions).Contains<string>(Path.GetExtension(file)))
      {
        try
        {
          byte[] numArray = File.ReadAllBytes(file);
          Texture2D texture2D = new Texture2D(2, 2, (TextureFormat) 1, true);
          ImageConversion.LoadImage(texture2D, numArray);
          ((Object) texture2D).name = ((IEnumerable<string>) file.Split('\\', StringSplitOptions.None)).Last<string>();
          this.cachedTextures.Add(Path.GetFileNameWithoutExtension(((Object) texture2D).name), texture2D);
        }
        catch (Exception ex)
        {
          Log.Error($"Unable to load file {file} into Texture2D. Are you using an unsupported image type? Exception=\"{ex}\"");
        }
      }
    }
  }

  private void DownloadImages()
  {
    if (GenList.NullOrEmpty<UpdateLog.UpdateLogData.UploadedImages>((IList<UpdateLog.UpdateLogData.UploadedImages>) this.UpdateData.images))
      return;
    foreach (UpdateLog.UpdateLogData.UploadedImages image in this.UpdateData.images)
    {
      if (!this.cachedDownloadedTextures.ContainsKey(image.name))
        this.cachedDownloadedTextures[image.name] = new WebTexture();
    }
    this.BeginDownloadingAsync();
  }

  private void BeginDownloadingAsync()
  {
    Task.Run((Func<Task>) (async () =>
    {
      foreach (UpdateLog.UpdateLogData.UploadedImages image1 in this.UpdateData.images)
      {
        UpdateLog.UpdateLogData.UploadedImages image = image1;
        WebTexture webTexture = this.cachedDownloadedTextures[image.name];
        webTexture.Status = DownloadStatus.InProgress;
        try
        {
          Texture2D textureFromUrl = await FileReader.GetTextureFromURL(image.url);
          if (Object.op_Implicit((Object) textureFromUrl))
          {
            webTexture.SetTexture(textureFromUrl);
            webTexture.Status = !Object.op_Implicit((Object) webTexture.texture) ? DownloadStatus.Failed : DownloadStatus.Success;
          }
          else
          {
            webTexture.SetTexture((Texture2D) null);
            webTexture.Status = DownloadStatus.Failed;
          }
        }
        catch (Exception ex)
        {
          Log.Error($"Exception thrown while trying to fetch image from {image.url}.\nException={ex}");
          webTexture.SetTexture((Texture2D) null);
          webTexture.Status = DownloadStatus.Failed;
        }
        webTexture = (WebTexture) null;
        image = new UpdateLog.UpdateLogData.UploadedImages();
      }
    }));
  }

  public string UpdateLogVersionFile(string xmlVersion)
  {
    return File.Exists(Path.Combine(this.Mod.RootDir, "Version.txt")) ? File.ReadAllText(Path.Combine(this.Mod.RootDir, "Version.txt")) : xmlVersion;
  }

  public void NotifyModUpdated()
  {
    this.UpdateData.InvokeActionOnUpdate();
    this.UpdateData.update = false;
  }

  public void SaveUpdateStatus()
  {
    if (this.UpdateData.testing)
      return;
    if (!Directory.Exists(FileReader.UpdateLogDirectory(this.Mod, this.CurrentFolder)))
    {
      Log.Error($"[{this.Mod.Name}] Unable to save UpdateLog info as directory {FileReader.UpdateLogDirectory(this.Mod, this.CurrentFolder)} does not exist.");
    }
    else
    {
      try
      {
        XDocument xdocument = new XDocument(new object[1]
        {
          (object) new XElement((XName) nameof (UpdateLog), new object[12]
          {
            (object) new XComment("Can utilize Version.txt file placed in mod's root directory"),
            (object) new XElement((XName) "currentVersion", (object) this.UpdateData.currentVersion),
            (object) new XComment(string.Join(",", Enum.GetNames(typeof (UpdateFor)))),
            (object) new XElement((XName) "updateOn", (object) this.UpdateData.updateOn),
            (object) new XComment("Full description shown in update page"),
            (object) new XElement((XName) "description", (object) this.UpdateData.description),
            (object) new XComment("Static parameterless method to execute when update log is executed"),
            (object) new XElement((XName) "actionOnUpdate", (object) this.UpdateData.actionOnUpdate),
            (object) new XComment("Show update log on next startup."),
            (object) new XElement((XName) "update", (object) this.UpdateData.update),
            (object) new XComment("Testing mode prevents the update from saving over the UpdateLog file"),
            (object) new XElement((XName) "testing", (object) this.UpdateData.testing)
          })
        });
        if (!GenList.NullOrEmpty<UpdateLog.UpdateLogData.HyperlinkedIcon>((IList<UpdateLog.UpdateLogData.HyperlinkedIcon>) this.UpdateData.rightIconBar))
        {
          xdocument.Element((XName) nameof (UpdateLog)).Add((object) new XComment("Icon bar shown to the right of the mod's name."));
          xdocument.Element((XName) nameof (UpdateLog)).Add((object) new XElement((XName) "rightIconBar"));
          foreach (UpdateLog.UpdateLogData.HyperlinkedIcon hyperlinkedIcon in this.UpdateData.rightIconBar)
            xdocument.Element((XName) nameof (UpdateLog)).Element((XName) "rightIconBar").Add((object) new XElement((XName) "li", new object[3]
            {
              (object) new XElement((XName) "name", (object) hyperlinkedIcon.name),
              (object) new XElement((XName) "icon", (object) hyperlinkedIcon.icon),
              (object) new XElement((XName) "url", (object) hyperlinkedIcon.url)
            }));
        }
        if (!GenList.NullOrEmpty<UpdateLog.UpdateLogData.HyperlinkedIcon>((IList<UpdateLog.UpdateLogData.HyperlinkedIcon>) this.UpdateData.leftIconBar))
        {
          xdocument.Element((XName) nameof (UpdateLog)).Add((object) new XComment("Icon bar shown to the left of the mod's name."));
          xdocument.Element((XName) nameof (UpdateLog)).Add((object) new XElement((XName) "leftIconBar"));
          foreach (UpdateLog.UpdateLogData.HyperlinkedIcon hyperlinkedIcon in this.UpdateData.leftIconBar)
            xdocument.Element((XName) nameof (UpdateLog)).Element((XName) "leftIconBar").Add((object) new XElement((XName) "li", new object[3]
            {
              (object) new XElement((XName) "name", (object) hyperlinkedIcon.name),
              (object) new XElement((XName) "icon", (object) hyperlinkedIcon.icon),
              (object) new XElement((XName) "url", (object) hyperlinkedIcon.url)
            }));
        }
        if (!GenList.NullOrEmpty<UpdateLog.UpdateLogData.UploadedImages>((IList<UpdateLog.UpdateLogData.UploadedImages>) this.UpdateData.images))
        {
          xdocument.Element((XName) nameof (UpdateLog)).Add((object) new XComment("Images to download off the web to reference from the description."));
          xdocument.Element((XName) nameof (UpdateLog)).Add((object) new XElement((XName) "images"));
          foreach (UpdateLog.UpdateLogData.UploadedImages image in this.UpdateData.images)
            xdocument.Element((XName) nameof (UpdateLog)).Element((XName) "images").Add((object) new XElement((XName) "li", new object[2]
            {
              (object) new XElement((XName) "name", (object) image.name),
              (object) new XElement((XName) "url", (object) image.url)
            }));
        }
        xdocument.Save(Path.Combine(FileReader.UpdateLogDirectory(this.Mod, this.CurrentFolder), "UpdateLog.xml"));
      }
      catch (Exception ex)
      {
        Log.Error($"[UpdateLog] Unable to save UpdateLog config info. Exception=\"{ex}\"");
      }
    }
  }

  public static bool operator ==(UpdateLog lhs, UpdateLog rhs)
  {
    if ((object) lhs == null)
      return (object) rhs == null;
    return (object) rhs == null ? (object) lhs == null : lhs.Equals(rhs);
  }

  public static bool operator !=(UpdateLog lhs, UpdateLog rhs) => !(lhs == rhs);

  public override bool Equals(object obj)
  {
    UpdateLog log = obj as UpdateLog;
    return (object) log != null && this.Equals(log);
  }

  public bool Equals(UpdateLog log)
  {
    return this.Mod == log.Mod && this.UpdateData.currentVersion == log.UpdateData.currentVersion;
  }

  public override int GetHashCode()
  {
    return Gen.HashCombine<string>(this.Mod.Name.GetHashCode(), this.UpdateData.currentVersion);
  }

  public class UpdateLogData
  {
    public string currentVersion;
    public UpdateFor updateOn = UpdateFor.GameInit;
    public string description;
    public string actionOnUpdate;
    public List<UpdateLog.UpdateLogData.HyperlinkedIcon> rightIconBar;
    public List<UpdateLog.UpdateLogData.HyperlinkedIcon> leftIconBar;
    public List<UpdateLog.UpdateLogData.UploadedImages> images;
    public bool testing;
    public bool update;

    public void InvokeActionOnUpdate()
    {
      if (GenText.NullOrEmpty(this.actionOnUpdate))
        return;
      try
      {
        string[] strArray = this.actionOnUpdate.Split('.', StringSplitOptions.None);
        GenTypes.GetTypeInAnyAssembly($"{strArray[0]}.{strArray[1]}", strArray[0]).GetMethod(strArray[2], BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic).Invoke((object) null, (object[]) null);
      }
      catch (Exception ex)
      {
        Log.Error($"Unable to invoke method on update. Method could not be found: {this.actionOnUpdate ?? "Null"} Ex={ex}");
      }
    }

    public string EnhancedDescription => this.description;

    public struct HyperlinkedIcon
    {
      public string name;
      public string icon;
      public string url;
    }

    public struct UploadedImages
    {
      public string name;
      public string url;
    }
  }
}
