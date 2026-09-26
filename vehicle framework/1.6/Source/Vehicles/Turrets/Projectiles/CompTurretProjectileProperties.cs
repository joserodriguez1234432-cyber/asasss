// Decompiled with JetBrains decompiler
// Type: Vehicles.CompTurretProjectileProperties
// Assembly: Vehicles, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 81BFDC99-D8AD-42E1-8470-2F73E7836B4A
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\Vehicles.dll

using Verse;

#nullable disable
namespace Vehicles;

public class CompTurretProjectileProperties : ThingComp
{
  public float speed = -1f;
  public CustomHitFlags hitflags;

  public CompTurretProjectileProperties(ThingWithComps parent) => this.parent = parent;

  public virtual void PostExposeData()
  {
    base.PostExposeData();
    Scribe_Values.Look<float>(ref this.speed, "speed", 0.0f, false);
    Scribe_Defs.Look<CustomHitFlags>(ref this.hitflags, "hitflags");
  }
}
