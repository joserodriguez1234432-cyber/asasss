// Decompiled with JetBrains decompiler
// Type: Vehicles.CompProperties_VehicleTurrets
// Assembly: Vehicles, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 81BFDC99-D8AD-42E1-8470-2F73E7836B4A
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\Vehicles.dll

using SmashTools;
using System;
using System.Collections.Generic;
using System.Linq;
using Verse;

#nullable disable
namespace Vehicles;

[HeaderTitle(Label = "VF_TurretProperties", Translate = true)]
public class CompProperties_VehicleTurrets : VehicleCompProperties
{
  [NumericBoxValues(MinValue = 0.0f)]
  [ActionOnSettingsInput(typeof (CompProperties_VehicleTurrets), "RecacheAllTurrets")]
  public float deployTime;
  public List<VehicleTurret> turrets = new List<VehicleTurret>();
  public SoundDef deployingSustainer;
  public SoundDef deploySound;
  public SoundDef undeploySound;

  public CompProperties_VehicleTurrets() => this.compClass = typeof (CompVehicleTurrets);

  public virtual void ResolveReferences(ThingDef parentDef)
  {
    base.ResolveReferences(parentDef);
    this.ResolveTurrets(parentDef as VehicleDef);
  }

  private void ResolveTurrets(VehicleDef vehicleDef)
  {
    if (GenList.NullOrEmpty<VehicleTurret>((IList<VehicleTurret>) this.turrets))
      return;
    foreach (VehicleTurret turret in this.turrets)
    {
      turret.TurretRotationTargeted = turret.defaultAngleRotated;
      turret.vehicleDef = vehicleDef;
      this.ResolveChildTurrets(turret);
      turret.def.ammunition?.ResolveReferences();
    }
  }

  public override void PostDefDatabase()
  {
    base.PostDefDatabase();
    foreach (VehicleTurret turret in this.turrets)
    {
      turret.ResolveGraphics(turret.vehicleDef, true);
      turret.renderProperties.PostLoad();
    }
  }

  private void ResolveChildTurrets(VehicleTurret turret)
  {
    turret.childTurrets = new List<VehicleTurret>();
    if (string.IsNullOrEmpty(turret.parentKey))
      return;
    foreach (VehicleTurret vehicleTurret in this.turrets.Where<VehicleTurret>((Func<VehicleTurret, bool>) (c => c.key == turret.parentKey)))
    {
      if (turret.parentKey == vehicleTurret.key)
      {
        turret.attachedTo = vehicleTurret;
        if (vehicleTurret.attachedTo == turret || turret == vehicleTurret)
        {
          Log.Error("Recursive turret attachments detected, this is not allowed. Disconnecting turret from parent.");
          turret.attachedTo = (VehicleTurret) null;
        }
        else
          vehicleTurret.childTurrets.Add(turret);
      }
    }
  }

  public virtual IEnumerable<string> ConfigErrors(ThingDef parentDef)
  {
    IEnumerator<string> enumerator = base.ConfigErrors(parentDef).GetEnumerator();
    while (enumerator.MoveNext())
      yield return enumerator.Current;
    enumerator = (IEnumerator<string>) null;
    if (parentDef is VehicleDef vehicleDef)
    {
      foreach (VehicleTurret turret in this.turrets)
      {
        enumerator = turret.ConfigErrors(vehicleDef).GetEnumerator();
        while (enumerator.MoveNext())
          yield return enumerator.Current;
        enumerator = (IEnumerator<string>) null;
      }
    }
    else
      yield return "<field>parentDef</field> must be a <type>VehicleDef</type> in order to implement <type>CompVehicleTurrets</type>.".ConvertRichText();
  }

  private static void RecacheAllTurrets()
  {
    if (GenList.NullOrEmpty<Map>((IList<Map>) Find.Maps))
      return;
    foreach (Map map in Find.Maps)
    {
      foreach (Pawn pawn in (IEnumerable<Pawn>) map.mapPawns.AllPawnsSpawned)
      {
        if (pawn is VehiclePawn vehiclePawn)
          vehiclePawn.CompVehicleTurrets?.RecacheDeployment();
      }
    }
  }
}
