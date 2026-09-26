// Decompiled with JetBrains decompiler
// Type: Vehicles.VehicleFleshTypeDef
// Assembly: Vehicles, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 81BFDC99-D8AD-42E1-8470-2F73E7836B4A
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\Vehicles.dll

using RimWorld;
using Verse;

#nullable disable
namespace Vehicles;

public class VehicleFleshTypeDef : FleshTypeDef
{
  public EffecterDef nonPenetrationEffect;
  public EffecterDef diminishedEffect;
  public EffecterDef deflectionEffect;
  public EffecterDef deflectionEffectBullet;
  public EffecterDef electrifiedEffect;
  public SoundDef soundImpactDeflect;
  public SoundDef soundImpactDiminished;
  public SoundDef soundImpactNonPenetrated;
  public SoundDef soundImpactPenetrated;

  public virtual void ResolveReferences()
  {
    ((Def) this).ResolveReferences();
    if (this.nonPenetrationEffect == null)
      this.nonPenetrationEffect = EffecterDefOf.DamageDiminished_Metal;
    if (this.diminishedEffect == null)
      this.diminishedEffect = EffecterDefOf.DamageDiminished_Metal;
    if (this.deflectionEffect == null)
      this.deflectionEffect = EffecterDefOf.Deflect_Metal;
    if (this.deflectionEffectBullet != null)
      return;
    this.deflectionEffectBullet = EffecterDefOf.Deflect_Metal_Bullet;
  }
}
