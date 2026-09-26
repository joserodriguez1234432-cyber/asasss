// Decompiled with JetBrains decompiler
// Type: Vehicles.Ext_Toils
// Assembly: Vehicles, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 81BFDC99-D8AD-42E1-8470-2F73E7836B4A
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\Vehicles.dll

using SmashTools;
using System;
using Verse;
using Verse.AI;

#nullable disable
namespace Vehicles;

public static class Ext_Toils
{
  public static T FailOnMoving<T>(this T jobEndable, TargetIndex index) where T : IJobEndable
  {
    ref T local = ref jobEndable;
    if ((object) default (T) == null)
    {
      T obj = local;
      local = ref obj;
    }
    Func<JobCondition> func = (Func<JobCondition>) (() =>
    {
      LocalTargetInfo target = jobEndable.GetActor().jobs.curJob.GetTarget(index);
      if (!(((LocalTargetInfo) ref target).Thing is VehiclePawn thing2))
      {
        Trace.Fail("Null vehicle");
        return (JobCondition) 64 /*0x40*/;
      }
      return !thing2.vehiclePather.Moving ? (JobCondition) 1 : (JobCondition) 16 /*0x10*/;
    });
    local.AddEndCondition(func);
    return jobEndable;
  }
}
