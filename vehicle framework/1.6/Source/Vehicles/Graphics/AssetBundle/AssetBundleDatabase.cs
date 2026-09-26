// Decompiled with JetBrains decompiler
// Type: Vehicles.AssetBundleDatabase
// Assembly: Vehicles, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 81BFDC99-D8AD-42E1-8470-2F73E7836B4A
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\Vehicles.dll

using SmashTools;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using UnityEngine;
using Verse;

#nullable disable
namespace Vehicles;

public static class AssetBundleDatabase
{
  private static readonly Dictionary<string, string> bundleBuildVersions = new Dictionary<string, string>()
  {
    {
      "1.3",
      "2019.4.30f1"
    },
    {
      "1.4",
      "2019.4.30f1"
    },
    {
      "1.5",
      "2019.4.30f1"
    },
    {
      "1.6",
      "2022.3.35f1"
    }
  };
  private static readonly Dictionary<string, Object> assetLookup = new Dictionary<string, Object>();
  private static readonly List<string> loadFoldersChecked = new List<string>();
  private static readonly List<AssetBundle> vehicleAssets = new List<AssetBundle>();

  private static bool IsLoaded { get; }

  static AssetBundleDatabase()
  {
    AssetBundleDatabase.vehicleAssets = VehicleMod.mod.Content.assetBundles.loadedAssetBundles.ToList<AssetBundle>();
    AssetBundleDatabase.IsLoaded = true;
  }

  private static string PlatformFolder
  {
    get
    {
      if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        return "StandaloneWindows64";
      if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        return "StandaloneLinux64";
      if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        return "StandaloneOSX";
      Log.Warning(RuntimeInformation.OSDescription + " is not currently supported for RGBShaders. Disabling custom shaders.");
      VehicleMod.settings.main.useCustomShaders = false;
      return (string) null;
    }
  }

  public static T LoadAsset<T>(string path) where T : Object
  {
    Object object1;
    if (AssetBundleDatabase.assetLookup.TryGetValue(path, out object1))
      return (T) object1;
    foreach (AssetBundle vehicleAsset in AssetBundleDatabase.vehicleAssets)
    {
      Object object2 = vehicleAsset.LoadAsset(path);
      if (Object.op_Inequality(object2, (Object) null))
      {
        if (!(object2 is T obj))
        {
          SmashLog.Error($"Asset has loaded successfully from path=<text>\"{path}\"</text> but is not of type <type>{typeof (T)}</type>. Actual type is <type>{object2.GetType()}</type>.");
          return default (T);
        }
        AssetBundleDatabase.assetLookup.Add(path, object2);
        return obj;
      }
    }
    SmashLog.Error($"Unable to locate asset at path=\"{path}\".");
    return default (T);
  }

  public static bool SupportsRGBMaskTex(this Shader shader, bool ignoreSettings = false)
  {
    if (!VehicleMod.settings.main.useCustomShaders && !ignoreSettings)
      return false;
    return Object.op_Equality((Object) shader, (Object) VehicleShaderTypeDefOf.CutoutComplexPattern.Shader) || Object.op_Equality((Object) shader, (Object) VehicleShaderTypeDefOf.CutoutComplexSkin.Shader) || Object.op_Equality((Object) shader, (Object) VehicleShaderTypeDefOf.CutoutComplexRGB.Shader);
  }
}
