// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.CompZipline
// Assembly: VehicleMapFramework, Version=1.6.562.0, Culture=neutral, PublicKeyToken=null
// MVID: 10A61882-945F-4CFC-9B06-CA8EEF5ADB36
// Assembly location: D:\Programas\steamapps\workshop\content\294100\3426502333\1.6\Assemblies\VehicleMapFramework.dll

using RimWorld;
using System.Linq;
using UnityEngine;
using Verse;

#nullable disable
namespace VehicleMapFramework;

public class CompZipline : CompVehicleEnterSpot
{
  public CompProperties_Zipline Props => (CompProperties_Zipline) this.props;

  public Verb_LaunchZipline LaunchVerb
  {
    get
    {
      if (this.\u003CLaunchVerb\u003Ek__BackingField == null)
      {
        switch (this.parent)
        {
          case Building_Turret buildingTurret:
            this.\u003CLaunchVerb\u003Ek__BackingField = buildingTurret.AttackVerb as Verb_LaunchZipline;
            break;
          case ZiplineEnd ziplineEnd:
            this.\u003CLaunchVerb\u003Ek__BackingField = ziplineEnd.launchVerb;
            break;
          case Pawn pawn:
            this.\u003CLaunchVerb\u003Ek__BackingField = pawn.VerbTracker.AllVerbs.OfType<Verb_LaunchZipline>().FirstOrDefault<Verb_LaunchZipline>();
            break;
          default:
            if (((Thing) this.parent).def.IsWeapon)
            {
              this.\u003CLaunchVerb\u003Ek__BackingField = ThingCompUtility.TryGetComp<CompEquippable>((Thing) this.parent)?.PrimaryVerb as Verb_LaunchZipline;
              break;
            }
            break;
        }
      }
      return this.\u003CLaunchVerb\u003Ek__BackingField;
    }
  }

  public Thing Pair
  {
    get => !this.IsZiplineEnd ? this.LaunchVerb?.ziplineEnd : ((Verb) this.LaunchVerb)?.caster;
  }

  public bool IsZiplineEnd { get; private set; }

  protected override bool Available
  {
    get
    {
      Thing pair = this.Pair;
      return pair != null && pair.Spawned;
    }
  }

  public override bool ShouldOffsetOnEdge => false;

  protected override TargetInfo AccessSpot
  {
    get
    {
      Thing pair = this.Pair;
      return pair == null ? TargetInfo.Invalid : TargetInfo.op_Implicit(pair);
    }
  }

  public override float MovePerTick(Pawn pawn)
  {
    return (this.IsZiplineEnd ? 0.5f : 1f) / pawn.TicksPerMoveCardinal;
  }

  public override void PostSpawnSetup(bool respawningAfterLoad)
  {
    base.PostSpawnSetup(respawningAfterLoad);
    this.IsZiplineEnd = this.parent is ZiplineEnd;
  }

  public virtual void PostDraw()
  {
    if (this.IsZiplineEnd)
      return;
    Thing ziplineEnd1 = this.LaunchVerb?.ziplineEnd;
    Thing thing = ziplineEnd1;
    if (!(thing is IZiplineEnd ziplineEnd2))
    {
      if (thing != null || this.Props.standbyGraphic == null)
        return;
      Graphics.DrawMesh(MeshPool.plane10, ((Thing) this.parent).DrawPos, Quaternion.AngleAxis((this.parent is Building_TurretGun parent ? parent.Top?.CurRotation : new float?()).GetValueOrDefault(), Vector3.up), this.Props.standbyGraphic.Graphic.MatSingleFor((Thing) this.parent), 0);
    }
    else
      ziplineEnd2.DrawZipline(ziplineEnd1.DrawPos);
  }
}
