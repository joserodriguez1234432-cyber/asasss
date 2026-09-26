// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.VMF_HarmonyPatches.VFVersionalPatchAttribute
// Assembly: VehicleMapFramework, Version=1.6.562.0, Culture=neutral, PublicKeyToken=null
// MVID: 10A61882-945F-4CFC-9B06-CA8EEF5ADB36
// Assembly location: D:\Programas\steamapps\workshop\content\294100\3426502333\1.6\Assemblies\VehicleMapFramework.dll

using SmashTools;
using System;
using System.Text.RegularExpressions;
using Vehicles;

#nullable disable
namespace VehicleMapFramework.VMF_HarmonyPatches;

[AttributeUsage(AttributeTargets.Class)]
internal class VFVersionalPatchAttribute : Attribute
{
  internal const string LatestRelease = "1.6.2144";
  internal const string CurrentDevBranch = "1.6.2361";

  public VFVersionalPatchAttribute(string version, ComparisonType comparison = 2)
  {
    this.TargetVersion = Version.Parse(version);
    bool flag;
    switch ((int) comparison)
    {
      case 0:
        flag = VFVersionalPatchAttribute.CurrentVersion < this.TargetVersion;
        break;
      case 1:
        flag = VFVersionalPatchAttribute.CurrentVersion <= this.TargetVersion;
        break;
      case 2:
        flag = VFVersionalPatchAttribute.CurrentVersion == this.TargetVersion;
        break;
      case 3:
        flag = VFVersionalPatchAttribute.CurrentVersion > this.TargetVersion;
        break;
      case 4:
        flag = VFVersionalPatchAttribute.CurrentVersion >= this.TargetVersion;
        break;
      case 5:
        flag = VFVersionalPatchAttribute.CurrentVersion != this.TargetVersion;
        break;
      default:
        flag = false;
        break;
    }
    this.Available = flag;
  }

  public bool Available { get; }

  private Version TargetVersion { get; }

  private static Version CurrentVersion { get; } = Version.Parse(Regex.Replace(VehicleMod.metaData?.ModVersion ?? "1.6.2144", "[^\\d.]", ""));
}
