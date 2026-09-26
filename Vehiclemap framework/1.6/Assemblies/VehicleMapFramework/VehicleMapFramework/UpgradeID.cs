// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.UpgradeID
// Assembly: VehicleMapFramework, Version=1.6.562.0, Culture=neutral, PublicKeyToken=null
// MVID: 10A61882-945F-4CFC-9B06-CA8EEF5ADB36
// Assembly location: D:\Programas\steamapps\workshop\content\294100\3426502333\1.6\Assemblies\VehicleMapFramework.dll

using System;
using System.Collections.Generic;
using Verse;

#nullable disable
namespace VehicleMapFramework;

public class UpgradeID : IExposable
{
  public string editKey;
  public int id;
  public string key;
  public List<string> turretIds;

  public UpgradeID()
  {
  }

  public UpgradeID(string key, string editKey, List<string> turretIds, int id)
  {
    this.key = key;
    this.editKey = editKey;
    this.turretIds = turretIds;
    this.id = id;
  }

  public void ExposeData()
  {
    Scribe_Values.Look<string>(ref this.key, "key", (string) null, false);
    Scribe_Values.Look<string>(ref this.editKey, "editKey", (string) null, false);
    Scribe_Collections.Look<string>(ref this.turretIds, "turretIds", (LookMode) 1, Array.Empty<object>());
    Scribe_Values.Look<int>(ref this.id, "id", 0, false);
  }
}
