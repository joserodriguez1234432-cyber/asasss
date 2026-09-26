// Decompiled with JetBrains decompiler
// Type: Vehicles.ProjectileSkyfaller
// Assembly: Vehicles, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 81BFDC99-D8AD-42E1-8470-2F73E7836B4A
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\Vehicles.dll

using UnityEngine;
using Verse;

#nullable disable
namespace Vehicles;

[StaticConstructorOnStartup]
public class ProjectileSkyfaller : Thing
{
  public int ticksToImpact;
  public Thing caster;
  public ThingDef projectileDef;
  public Vector3 origin;
  public Vector3 destination;
  public bool reverseDraw;
  public float speedTilesPerTick = 1f;

  public virtual Vector3 DrawPos => this.ExactPosition;

  protected virtual void DrawAt(Vector3 drawLoc, bool flip = false)
  {
    Graphics.DrawMesh(MeshPool.GridPlane(this.projectileDef.graphicData.drawSize), drawLoc, this.ExactRotation, ((BuildableDef) this.projectileDef).DrawMatSingle, 0);
  }

  public virtual Vector3 ExactPosition
  {
    get
    {
      return Vector3.op_Addition(Vector3.op_Addition(this.origin, Vector3.op_Multiply(Vector3.op_Subtraction(this.destination, this.origin), Mathf.Clamp01((float) (1.0 - (double) this.ticksToImpact / (double) this.ProjectileTicksToImpact)))), Vector3.op_Multiply(Vector3.up, ((BuildableDef) this.def).Altitude));
    }
  }

  public virtual Quaternion ExactRotation
  {
    get
    {
      return Quaternion.LookRotation(Vector3Utility.Yto0(this.reverseDraw ? Vector3.op_Subtraction(this.destination, this.origin) : Vector3.op_Subtraction(this.origin, this.destination)));
    }
  }

  protected float ProjectileTicksToImpact
  {
    get
    {
      Vector3 vector3 = Vector3.op_Subtraction(this.origin, this.destination);
      float projectileTicksToImpact = ((Vector3) ref vector3).magnitude / this.speedTilesPerTick;
      if ((double) projectileTicksToImpact <= 0.0)
        projectileTicksToImpact = 1f / 1000f;
      return projectileTicksToImpact;
    }
  }

  protected virtual void Tick()
  {
    --this.ticksToImpact;
    if (this.ticksToImpact > 0)
      return;
    this.Impact();
  }

  protected virtual void Impact()
  {
    if (GenGrid.InBounds(this.Position, this.Map))
    {
      Projectile projectile1 = (Projectile) GenSpawn.Spawn(this.projectileDef, this.Position, this.Map, (WipeMode) 0);
      Projectile projectile2 = projectile1;
      Thing caster1 = this.caster;
      IntVec3 intVec3 = IntVec3Utility.ToIntVec3(this.destination);
      Vector3 vector3Shifted = ((IntVec3) ref intVec3).ToVector3Shifted();
      LocalTargetInfo localTargetInfo1 = LocalTargetInfo.op_Implicit(IntVec3Utility.ToIntVec3(this.destination));
      LocalTargetInfo localTargetInfo2 = LocalTargetInfo.op_Implicit(IntVec3Utility.ToIntVec3(this.destination));
      ProjectileHitFlags hitFlags = projectile1.HitFlags;
      Thing caster2 = this.caster;
      projectile2.Launch(caster1, vector3Shifted, localTargetInfo1, localTargetInfo2, hitFlags, false, caster2, (ThingDef) null);
    }
    this.Destroy((DestroyMode) 0);
  }

  public virtual void ExposeData()
  {
    base.ExposeData();
    Scribe_Values.Look<Vector3>(ref this.origin, "origin", new Vector3(), false);
    Scribe_Values.Look<Vector3>(ref this.destination, "destination", new Vector3(), false);
    Scribe_Values.Look<bool>(ref this.reverseDraw, "reverseDraw", false, false);
    Scribe_Values.Look<int>(ref this.ticksToImpact, "ticksToImpact", 0, false);
    Scribe_Values.Look<float>(ref this.speedTilesPerTick, "speedTilesPerTick", 0.0f, false);
    Scribe_References.Look<Thing>(ref this.caster, "caster", false);
    Scribe_Defs.Look<ThingDef>(ref this.projectileDef, "projectileDef");
  }

  public virtual void SpawnSetup(Map map, bool respawningAfterLoad)
  {
    base.SpawnSetup(map, respawningAfterLoad);
    if (respawningAfterLoad)
      return;
    this.ticksToImpact = Mathf.CeilToInt(this.ProjectileTicksToImpact);
  }
}
