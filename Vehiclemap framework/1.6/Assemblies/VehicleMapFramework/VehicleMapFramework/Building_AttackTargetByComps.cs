// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.Building_AttackTargetByComps
// Assembly: VehicleMapFramework, Version=1.6.562.0, Culture=neutral, PublicKeyToken=null
// MVID: 10A61882-945F-4CFC-9B06-CA8EEF5ADB36
// Assembly location: D:\Programas\steamapps\workshop\content\294100\3426502333\1.6\Assemblies\VehicleMapFramework.dll

using System;
using System.Linq;
using Verse;
using Verse.AI;

#nullable disable
namespace VehicleMapFramework;

public class Building_AttackTargetByComps : Building, IAttackTarget, ILoadReferenceable
{
  Thing IAttackTarget.Thing => (Thing) this;

  LocalTargetInfo IAttackTarget.TargetCurrentlyAimingAt => LocalTargetInfo.Invalid;

  float IAttackTarget.TargetPriorityFactor
  {
    get
    {
      return ((ThingWithComps) this).AllComps.OfType<IAttackTarget>().Select<IAttackTarget, float>((Func<IAttackTarget, float>) (a => a.TargetPriorityFactor)).Aggregate<float>((Func<float, float, float>) ((a, b) => a * b));
    }
  }

  bool IAttackTarget.ThreatDisabled(IAttackTargetSearcher disabledFor)
  {
    return ((ThingWithComps) this).AllComps.OfType<IAttackTarget>().Any<IAttackTarget>((Func<IAttackTarget, bool>) (attackTarget => attackTarget.ThreatDisabled(disabledFor)));
  }
}
