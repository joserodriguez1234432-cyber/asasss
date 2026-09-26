// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.BackCompatibilityConverter_VMF
// Assembly: VehicleMapFramework, Version=1.6.562.0, Culture=neutral, PublicKeyToken=null
// MVID: 10A61882-945F-4CFC-9B06-CA8EEF5ADB36
// Assembly location: D:\Programas\steamapps\workshop\content\294100\3426502333\1.6\Assemblies\VehicleMapFramework.dll

using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Xml;
using Vehicles;
using Verse;

#nullable disable
namespace VehicleMapFramework;

[StaticConstructorOnStartup]
public class BackCompatibilityConverter_VMF : BackCompatibilityConverter
{
  static BackCompatibilityConverter_VMF()
  {
    ((List<BackCompatibilityConverter>) AccessTools.Field(typeof (BackCompatibility), "conversionChain").GetValue((object) null)).Add((BackCompatibilityConverter) new BackCompatibilityConverter_VMF());
  }

  public virtual bool AppliesToVersion(int majorVer, int minorVer) => true;

  public virtual string BackCompatibleDefName(
    Type defType,
    string defName,
    bool forDefInjections = false,
    XmlNode node = null)
  {
    if (defType == typeof (ThingDef))
    {
      if (defName == "VMF_WirelessReceiver")
        return "VMF_WirelessTransmitter";
      VehicleDef vehicleDef;
      if (defName.StartsWith("GravshipVehicle") && (GetClaimedDef(VMF_DefOf.VMF_GravshipVehicleBase, out vehicleDef) || GetClaimedDef(VMF_DefOf.VMF_GravshipVehicleBaseSpace, out vehicleDef)))
        return ((Def) vehicleDef).defName;
    }
    return (string) null;

    bool GetClaimedDef(VehicleDef parentDef, out VehicleDef vehicleDef)
    {
      List<VehicleDef> vehicleDefList;
      if (!UniqueVehicleManager.PlaceholderDefs.TryGetValue(parentDef, out vehicleDefList))
      {
        vehicleDef = (VehicleDef) null;
        return false;
      }
      vehicleDef = GenCollection.FirstOrDefault<VehicleDef>(vehicleDefList, (Predicate<VehicleDef>) (d =>
      {
        VehicleMapProps_Gravship modExtension = ((Def) d).GetModExtension<VehicleMapProps_Gravship>();
        return modExtension != null && modExtension.defName == defName;
      }));
      return vehicleDef != null;
    }
  }

  public virtual Type GetBackCompatibleType(Type baseType, string providedClassName, XmlNode node)
  {
    return baseType == typeof (Thing) && providedClassName == "VehicleMapFramework.ExplosionAcrossMaps" ? typeof (Explosion) : (Type) null;
  }

  public virtual void PostExposeData(object obj)
  {
  }
}
