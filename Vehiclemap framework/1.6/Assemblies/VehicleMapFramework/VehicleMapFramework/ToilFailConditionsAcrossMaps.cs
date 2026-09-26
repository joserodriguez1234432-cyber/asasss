// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.ToilFailConditionsAcrossMaps
// Assembly: VehicleMapFramework, Version=1.6.562.0, Culture=neutral, PublicKeyToken=null
// MVID: 10A61882-945F-4CFC-9B06-CA8EEF5ADB36
// Assembly location: D:\Programas\steamapps\workshop\content\294100\3426502333\1.6\Assemblies\VehicleMapFramework.dll

using RimWorld;
using System;
using Verse;
using Verse.AI;

#nullable disable
namespace VehicleMapFramework;

public static class ToilFailConditionsAcrossMaps
{
  public static T FailOnBurningImmobile<T>(this T f, TargetIndex ind, Map map) where T : IJobEndable
  {
    ref T local = ref f;
    if ((object) default (T) == null)
    {
      T obj = local;
      local = ref obj;
    }
    Func<JobCondition> func = (Func<JobCondition>) (() =>
    {
      LocalTargetInfo target = f.GetActor().jobs.curJob.GetTarget(ind);
      return ((LocalTargetInfo) ref target).IsValid && FireUtility.IsBurning(((LocalTargetInfo) ref target).ToTargetInfo(map)) ? (JobCondition) 4 : (JobCondition) 1;
    });
    local.AddEndCondition(func);
    return f;
  }
}
