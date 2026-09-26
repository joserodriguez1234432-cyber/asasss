// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.TargetMapManager
// Assembly: VehicleMapFramework, Version=1.6.562.0, Culture=neutral, PublicKeyToken=null
// MVID: 10A61882-945F-4CFC-9B06-CA8EEF5ADB36
// Assembly location: D:\Programas\steamapps\workshop\content\294100\3426502333\1.6\Assemblies\VehicleMapFramework.dll

using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Verse;

#nullable disable
namespace VehicleMapFramework;

public class TargetMapManager(World world) : WorldComponent(world)
{
  private Dictionary<Thing, TargetInfo> tmpTargetInfoDic = new Dictionary<Thing, TargetInfo>();
  private List<Thing> tmpKeys = new List<Thing>();
  private List<TargetInfo> tmpValues = new List<TargetInfo>();

  public ConditionalWeakTable<Thing, StrongBox<TargetInfo>> TargetInfoTable { get; } = new ConditionalWeakTable<Thing, StrongBox<TargetInfo>>();

  internal StrongBox<TargetInfo> GetOrCreateTargetInfo(Thing thing)
  {
    return thing == null ? (StrongBox<TargetInfo>) null : this.TargetInfoTable.GetValue(thing, (ConditionalWeakTable<Thing, StrongBox<TargetInfo>>.CreateValueCallback) (_ => new StrongBox<TargetInfo>()
    {
      Value = TargetInfo.Invalid
    }));
  }

  public virtual void FinalizeInit(bool fromLoad) => TargetMapUtility.manager = this;

  public virtual void WorldComponentTick()
  {
    if (!GenTicks.IsTickInterval(10800))
      return;
    if (this.tmpKeys == null)
      this.tmpKeys = new List<Thing>();
    foreach (KeyValuePair<Thing, StrongBox<TargetInfo>> keyValuePair in ((IEnumerable<KeyValuePair<Thing, StrongBox<TargetInfo>>>) this.TargetInfoTable).Where<KeyValuePair<Thing, StrongBox<TargetInfo>>>((Func<KeyValuePair<Thing, StrongBox<TargetInfo>>, bool>) (pair =>
    {
      StrongBox<TargetInfo> strongBox = pair.Value;
      if (strongBox != null)
      {
        TargetInfo targetInfo = strongBox.Value;
        if (((TargetInfo) ref targetInfo).IsValid)
          goto label_4;
      }
      bool flag;
      if (pair.Key != null)
      {
        flag = true;
        goto label_5;
      }
label_4:
      flag = false;
label_5:
      return flag;
    })))
      this.tmpKeys.Add(keyValuePair.Key);
    foreach (Thing tmpKey in this.tmpKeys)
    {
      if (tmpKey != null)
        this.TargetInfoTable.Remove(tmpKey);
    }
    this.tmpKeys.Clear();
  }

  public virtual void ExposeData()
  {
    switch ((int) Scribe.mode)
    {
      case 1:
        Dictionary<Thing, TargetInfo> dictionary = ((IEnumerable<KeyValuePair<Thing, StrongBox<TargetInfo>>>) this.TargetInfoTable).Select<KeyValuePair<Thing, StrongBox<TargetInfo>>, (Thing, TargetInfo)>((Func<KeyValuePair<Thing, StrongBox<TargetInfo>>, (Thing, TargetInfo)>) (pair =>
        {
          Thing key = pair.Key;
          StrongBox<TargetInfo> strongBox = pair.Value;
          TargetInfo targetInfo = strongBox != null ? strongBox.Value : TargetInfo.Invalid;
          return (key, targetInfo);
        })).Where<(Thing, TargetInfo)>((Func<(Thing, TargetInfo), bool>) (tuple =>
        {
          if (tuple.Key != null)
          {
            TargetInfo targetInfo = tuple.Item2;
            if (((TargetInfo) ref targetInfo).IsValid)
              return ((TargetInfo) ref targetInfo).Map != null;
          }
          return false;
        })).ToDictionary<(Thing, TargetInfo), Thing, TargetInfo>((Func<(Thing, TargetInfo), Thing>) (pair => pair.Key), (Func<(Thing, TargetInfo), TargetInfo>) (pair => pair.Item2));
        Scribe_Collections.Look<Thing, TargetInfo>(ref dictionary, "TargetInfo", (LookMode) 3, (LookMode) 6, ref this.tmpKeys, ref this.tmpValues, false, false, false);
        this.tmpTargetInfoDic = (Dictionary<Thing, TargetInfo>) null;
        break;
      case 2:
      case 3:
        if (this.tmpTargetInfoDic == null)
          this.tmpTargetInfoDic = new Dictionary<Thing, TargetInfo>();
        if (this.tmpKeys == null)
          this.tmpKeys = new List<Thing>();
        if (this.tmpValues == null)
          this.tmpValues = new List<TargetInfo>();
        Scribe_Collections.Look<Thing, TargetInfo>(ref this.tmpTargetInfoDic, "TargetInfo", (LookMode) 3, (LookMode) 6, ref this.tmpKeys, ref this.tmpValues, false, false, false);
        break;
      case 4:
        if (this.tmpTargetInfoDic == null)
          break;
        foreach (KeyValuePair<Thing, TargetInfo> keyValuePair in this.tmpTargetInfoDic)
        {
          if (keyValuePair.Key != null)
            this.TargetInfoTable.Add(keyValuePair.Key, new StrongBox<TargetInfo>(keyValuePair.Value));
        }
        this.tmpTargetInfoDic = (Dictionary<Thing, TargetInfo>) null;
        break;
    }
  }
}
