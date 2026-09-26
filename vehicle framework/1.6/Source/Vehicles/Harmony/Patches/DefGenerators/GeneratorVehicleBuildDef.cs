// Decompiled with JetBrains decompiler
// Type: Vehicles.GeneratorVehicleBuildDef
// Assembly: Vehicles, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 81BFDC99-D8AD-42E1-8470-2F73E7836B4A
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\Vehicles.dll

using RimWorld;
using Verse;

#nullable disable
namespace Vehicles;

internal class GeneratorVehicleBuildDef : IVehicleDefGenerator<VehicleBuildDef>
{
  private const string DefaultDesignationCategoryDefName = "Structure";

  bool IVehicleDefGenerator<VehicleBuildDef>.TryGenerateImpliedDef(
    VehicleDef vehicleDef,
    out VehicleBuildDef impliedBuildDef,
    bool hotReload)
  {
    impliedBuildDef = (VehicleBuildDef) null;
    if (vehicleDef.buildDef != null)
      return false;
    Log.Warning($"[{vehicleDef}] Implied generation for vehicles is incomplete. Please define the VehicleBuildDef separately to avoid improper vehicle generation.");
    string str = ((Def) vehicleDef).defName + "_Blueprint";
    impliedBuildDef = !hotReload ? new VehicleBuildDef() : DefDatabase<VehicleBuildDef>.GetNamed(str, false) ?? new VehicleBuildDef();
    ((Def) impliedBuildDef).defName = str;
    ((Def) impliedBuildDef).label = ((Def) vehicleDef).label;
    ((Def) impliedBuildDef).description = ((Def) vehicleDef).description;
    ((Def) impliedBuildDef).modContentPack = ((Def) vehicleDef).modContentPack;
    impliedBuildDef.thingClass = typeof (VehicleBuilding);
    impliedBuildDef.thingToSpawn = vehicleDef;
    impliedBuildDef.selectable = vehicleDef.selectable;
    ((BuildableDef) impliedBuildDef).altitudeLayer = ((BuildableDef) vehicleDef).altitudeLayer;
    ((BuildableDef) impliedBuildDef).terrainAffordanceNeeded = ((BuildableDef) vehicleDef).terrainAffordanceNeeded;
    ((BuildableDef) impliedBuildDef).constructEffect = ((BuildableDef) vehicleDef).constructEffect ?? EffecterDefOf.ConstructMetal;
    impliedBuildDef.leaveResourcesWhenKilled = vehicleDef.leaveResourcesWhenKilled;
    ((BuildableDef) impliedBuildDef).passability = ((BuildableDef) vehicleDef).passability;
    impliedBuildDef.fillPercent = vehicleDef.fillPercent;
    impliedBuildDef.neverMultiSelect = true;
    ((BuildableDef) impliedBuildDef).designationCategory = ((BuildableDef) vehicleDef).designationCategory ?? DefDatabase<DesignationCategoryDef>.GetNamed("Structure", true);
    ((BuildableDef) impliedBuildDef).clearBuildingArea = true;
    impliedBuildDef.category = (ThingCategory) 3;
    impliedBuildDef.blockWind = vehicleDef.blockWind;
    impliedBuildDef.useHitPoints = true;
    impliedBuildDef.rotatable = vehicleDef.rotatable;
    ((BuildableDef) impliedBuildDef).statBases = ((BuildableDef) vehicleDef).statBases;
    impliedBuildDef.size = vehicleDef.size;
    ((BuildableDef) impliedBuildDef).researchPrerequisites = ((BuildableDef) vehicleDef).researchPrerequisites;
    ((BuildableDef) impliedBuildDef).costList = ((BuildableDef) vehicleDef).costList;
    impliedBuildDef.soundImpactDefault = vehicleDef.soundImpactDefault;
    impliedBuildDef.soundBuilt = vehicleDef.soundBuilt;
    impliedBuildDef.graphicData = new GraphicData();
    VehicleBuildDef vehicleBuildDef = impliedBuildDef;
    BuildingProperties buildingProperties = vehicleDef.building;
    if (buildingProperties == null)
      buildingProperties = new BuildingProperties()
      {
        canPlaceOverImpassablePlant = false,
        paintable = false
      };
    vehicleBuildDef.building = buildingProperties;
    ((BuildableDef) vehicleDef).designationCategory = (DesignationCategoryDef) null;
    impliedBuildDef.graphicData.CopyFrom((GraphicData) vehicleDef.graphicData);
    System.Type type = vehicleDef.graphicData.drawRotated ? typeof (Graphic_Multi) : typeof (Graphic_Single);
    impliedBuildDef.graphicData.graphicClass = type;
    vehicleDef.buildDef = impliedBuildDef;
    return true;
  }
}
