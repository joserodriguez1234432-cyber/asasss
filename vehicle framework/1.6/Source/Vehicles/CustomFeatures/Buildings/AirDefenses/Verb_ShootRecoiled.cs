// Decompiled with JetBrains decompiler
// Type: Vehicles.Verb_ShootRecoiled
// Assembly: Vehicles, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 81BFDC99-D8AD-42E1-8470-2F73E7836B4A
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\Vehicles.dll

using SmashTools;
using Verse;

#nullable disable
namespace Vehicles;

public class Verb_ShootRecoiled : Verb_ShootRealistic
{
  protected override bool TryCastShot()
  {
    if (!base.TryCastShot())
      return false;
    if (((Verb) this).caster is Building_RecoiledTurret caster)
      caster.Notify_Recoiled();
    else
      SmashLog.Error($"Unable to produce recoil to {((Entity) ((Verb) this).caster).Label} of type <type>{((Verb) this).caster.GetType()}</type>. Type should be <type>Building_RecoiledTurret</type>");
    return true;
  }
}
