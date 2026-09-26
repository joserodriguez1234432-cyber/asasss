// Decompiled with JetBrains decompiler
// Type: Vehicles.GeneratorVehiclePawnKindDef
// Assembly: Vehicles, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 81BFDC99-D8AD-42E1-8470-2F73E7836B4A
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\Vehicles.dll

using System.Collections.Generic;
using Verse;

#nullable disable
namespace Vehicles;

internal class GeneratorVehiclePawnKindDef : IVehicleDefGenerator<PawnKindDef>
{
  public bool TryGenerateImpliedDef(VehicleDef vehicleDef, out PawnKindDef kindDef, bool hotReload)
  {
    kindDef = vehicleDef.kindDef;
    if (kindDef != null)
      return false;
    string str = ((Def) vehicleDef).defName + "_PawnKind";
    kindDef = !hotReload ? new PawnKindDef() : DefDatabase<PawnKindDef>.GetNamed(str, false) ?? new PawnKindDef();
    ((Def) kindDef).defName = str;
    ((Def) kindDef).modContentPack = ((Def) vehicleDef).modContentPack;
    ((Def) kindDef).label = ((Def) vehicleDef).label;
    ((Def) kindDef).description = ((Def) vehicleDef).description;
    kindDef.combatPower = vehicleDef.combatPower;
    kindDef.race = (ThingDef) vehicleDef;
    kindDef.ignoresPainShock = true;
    kindDef.lifeStages = new List<PawnKindLifeStage>(1)
    {
      new PawnKindLifeStage()
      {
        bodyGraphicData = (GraphicData) vehicleDef.graphicData
      }
    };
    kindDef.canBeSapper = true;
    vehicleDef.kindDef = kindDef;
    return true;
  }
}
