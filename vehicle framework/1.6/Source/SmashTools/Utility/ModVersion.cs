// Decompiled with JetBrains decompiler
// Type: SmashTools.ModVersion
// Assembly: SmashTools, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 3B8999A7-6EA8-49BB-AA7F-3E2E898B5249
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\SmashTools.dll

using System;

#nullable disable
namespace SmashTools;

public class ModVersion
{
  public ModVersion(int major, int minor, DateTime buildDate, DateTime startDate)
  {
    this.BuildDate = buildDate;
    int revision = (buildDate.Hour * 360 + buildDate.Minute * 60 + buildDate.Second) / 2;
    int build = Math.Abs((buildDate - startDate).Days);
    this.Version = new Version(major, minor, build, revision);
    this.VersionString = $"{this.Version.Major}.{this.Version.Minor}.{this.Version.Build}";
    this.VersionStringWithRevision = $"{this.Version.Major}.{this.Version.Minor}.{this.Version.Build} rev{this.Version.Revision}";
    this.VersionStringWithoutBuild = $"{this.Version.Major}.{this.Version.Minor}";
  }

  public Version Version { get; private set; }

  public DateTime BuildDate { get; private set; }

  public string VersionString { get; private set; }

  public string VersionStringWithoutBuild { get; private set; }

  public string VersionStringWithRevision { get; private set; }
}
