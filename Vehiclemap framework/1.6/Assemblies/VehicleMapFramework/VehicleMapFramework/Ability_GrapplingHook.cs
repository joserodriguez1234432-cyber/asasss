// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.Ability_GrapplingHook
// Assembly: VehicleMapFramework, Version=1.6.562.0, Culture=neutral, PublicKeyToken=null
// MVID: 10A61882-945F-4CFC-9B06-CA8EEF5ADB36
// Assembly location: D:\Programas\steamapps\workshop\content\294100\3426502333\1.6\Assemblies\VehicleMapFramework.dll

using RimWorld;
using System;
using Verse;

#nullable disable
namespace VehicleMapFramework;

public class Ability_GrapplingHook : Ability_MapTraverse
{
  public Ability_GrapplingHook()
  {
  }

  public Ability_GrapplingHook(Pawn pawn)
    : base(pawn)
  {
  }

  public Ability_GrapplingHook(Pawn pawn, Precept sourcePrecept)
    : base(pawn, sourcePrecept)
  {
  }

  public Ability_GrapplingHook(Pawn pawn, AbilityDef def)
    : base(pawn, def)
  {
  }

  public Ability_GrapplingHook(Pawn pawn, Precept sourcePrecept, AbilityDef def)
    : base(pawn, sourcePrecept, def)
  {
  }

  public virtual AcceptanceReport CanCast
  {
    get
    {
      AcceptanceReport canCast = base.CanCast;
      if (!((AcceptanceReport) ref canCast).Accepted || !(this.verb is Verb_LaunchZipline verb))
        return canCast;
      return verb.ziplineEnd == null ? AcceptanceReport.WasAccepted : AcceptanceReport.op_Implicit(Translator.Translate("VMF_GrapplingHookAlreadyLaunched"));
    }
  }

  public virtual void OnHit(ZiplineEnd ziplineEnd)
  {
    if (TargetMapUtility.get_TargetMap((Thing) this.pawn) != ((Thing) ziplineEnd).Map)
      TargetMapUtility.set_TargetMap((Thing) this.pawn, ((Thing) ziplineEnd).Map);
    if (!JumpUtility.DoJump(this.pawn, LocalTargetInfo.op_Implicit((Thing) ziplineEnd), (CompApparelReloadable) null, this.verb.verbProps, (Ability) this, LocalTargetInfo.op_Implicit((Thing) ziplineEnd), VMF_DefOf.VMF_GrapplingHookFlyer) || !(((Thing) this.pawn).ParentHolder is PawnFlyer_PersistentJob parentHolder))
      return;
    parentHolder.OnLanded += new Action(this.OnLanded);
  }

  protected virtual void OnLanded()
  {
    if (!(this.verb is Verb_LaunchZipline verb))
      return;
    verb.ziplineEnd = (Thing) null;
  }
}
