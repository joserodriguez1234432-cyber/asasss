// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.UniqueVehicleManager
// Assembly: VehicleMapFramework, Version=1.6.562.0, Culture=neutral, PublicKeyToken=null
// MVID: 10A61882-945F-4CFC-9B06-CA8EEF5ADB36
// Assembly location: D:\Programas\steamapps\workshop\content\294100\3426502333\1.6\Assemblies\VehicleMapFramework.dll

using System;
using System.Collections.Generic;
using System.Linq;
using Vehicles;
using Verse;

#nullable disable
namespace VehicleMapFramework;

public class UniqueVehicleManager : GameComponent
{
  private readonly Game game;
  private Dictionary<VehicleDef, List<string>> claimedDefNames;

  public UniqueVehicleManager(Game game)
  {
    this.game = game;
    this.claimedDefNames = new Dictionary<VehicleDef, List<string>>();
    // ISSUE: explicit constructor call
    base.\u002Ector();
  }

  public static Dictionary<VehicleDef, List<VehicleDef>> PlaceholderDefs { get; } = new Dictionary<VehicleDef, List<VehicleDef>>();

  public VehicleDef ClaimUniqueVehicleDef(VehicleDef parentDef)
  {
    List<VehicleDef> vehicleDefList;
    if (!UniqueVehicleManager.PlaceholderDefs.TryGetValue(parentDef, out vehicleDefList))
    {
      VMF_Log.Error($"Missing PlaceholderDefs for {parentDef}. Using parent def instead.");
      return parentDef;
    }
    List<string> stringList;
    if (!this.claimedDefNames.TryGetValue(parentDef, out stringList))
      stringList = this.claimedDefNames[parentDef] = new List<string>();
    foreach (VehicleDef def in vehicleDefList)
    {
      if (!stringList.Contains(((Def) def).defName))
      {
        stringList.Add(((Def) def).defName);
        ((ThingDef) def).size = ((ThingDef) parentDef).size;
        UniqueVehicleUtility.ReinitializeComponents(def);
        return def;
      }
    }
    VMF_Log.Error($"Failed to claim unique vehicle def for {parentDef}. Using parent def instead.");
    return parentDef;
  }

  public void ReleaseUniqueVehicleDef(VehicleDef def)
  {
    foreach (List<string> stringList in this.claimedDefNames.Values)
      stringList.Remove(((Def) def).defName);
  }

  public int ClaimedCount(VehicleDef vehicleDef)
  {
    List<string> valueOrDefault = CollectionExtensions.GetValueOrDefault<VehicleDef, List<string>>((IReadOnlyDictionary<VehicleDef, List<string>>) this.claimedDefNames, vehicleDef);
    // ISSUE: explicit non-virtual call
    return valueOrDefault == null ? 0 : __nonvirtual (valueOrDefault.Count);
  }

  public virtual void ExposeData()
  {
    HashSet<VehicleMapProps_Gravship> mapPropsGravshipSet = (HashSet<VehicleMapProps_Gravship>) null;
    if (Scribe.mode == 1)
    {
      mapPropsGravshipSet = new HashSet<VehicleMapProps_Gravship>();
      IEnumerable<Pawn> allGravshipVehicles = Find.Maps.SelectMany<Map, Pawn>((Func<Map, IEnumerable<Pawn>>) (m => (IEnumerable<Pawn>) m.mapPawns.AllPawns));
      allGravshipVehicles = allGravshipVehicles.Concat<Pawn>((IEnumerable<Pawn>) Find.WorldPawns.AllPawnsAliveOrDead);
      allGravshipVehicles = allGravshipVehicles.Concat<Pawn>(Find.Maps.SelectMany<Map, Pawn>((Func<Map, IEnumerable<Pawn>>) (m => m.listerThings.AllThings.OfType<VehicleSkyfaller>().Select<VehicleSkyfaller, Pawn>((Func<VehicleSkyfaller, Pawn>) (v => (Pawn) v.vehicle)))));
      List<Pawn> items = new List<Pawn>();
      items.AddRange(allGravshipVehicles);
      // ISSUE: object of a compiler-generated type is created
      allGravshipVehicles = (IEnumerable<Pawn>) new \u003C\u003Ez__ReadOnlyList<Pawn>(items);
      GenCollection.AddRange<VehicleMapProps_Gravship>(mapPropsGravshipSet, DefDatabase<VehicleDef>.AllDefs.Where<VehicleDef>((Func<VehicleDef, bool>) (d => ((Def) d).HasModExtension<VehicleMapProps_Gravship>())).Where<VehicleDef>((Func<VehicleDef, bool>) (d => allGravshipVehicles.Any<Pawn>((Func<Pawn, bool>) (p => ((Thing) p).def == d)))).Select<VehicleDef, VehicleMapProps_Gravship>((Func<VehicleDef, VehicleMapProps_Gravship>) (d => ((Def) d).GetModExtension<VehicleMapProps_Gravship>())));
      foreach (VehicleDef key in UniqueVehicleManager.PlaceholderDefs.Keys)
      {
        List<string> stringList;
        if (this.claimedDefNames.TryGetValue(key, out stringList))
          Scribe_Collections.Look<string>(ref stringList, "claimedDefNames_" + ((Def) key).defName, (LookMode) 1, Array.Empty<object>());
      }
    }
    Scribe_Collections.Look<VehicleMapProps_Gravship>(ref mapPropsGravshipSet, "GravshipVehicleMapProps", (LookMode) 2);
    if (mapPropsGravshipSet == null)
      mapPropsGravshipSet = new HashSet<VehicleMapProps_Gravship>();
    if (Scribe.mode != 2)
      return;
    if (this.claimedDefNames == null)
      this.claimedDefNames = new Dictionary<VehicleDef, List<string>>();
    foreach (VehicleDef key in UniqueVehicleManager.PlaceholderDefs.Keys)
    {
      List<string> stringList = (List<string>) null;
      Scribe_Collections.Look<string>(ref stringList, $"{this.claimedDefNames}_{((Def) key).defName}", (LookMode) 1, Array.Empty<object>());
      if (stringList != null)
        this.claimedDefNames[key] = stringList;
      foreach (VehicleDef def in UniqueVehicleManager.PlaceholderDefs[key])
      {
        ((ThingDef) def).size = ((ThingDef) key).size;
        ((ThingDef) def).uiIconScale = ((ThingDef) key).uiIconScale;
        UniqueVehicleUtility.ReinitializeComponents(def);
      }
    }
    foreach (VehicleMapProps_Gravship props in mapPropsGravshipSet)
      GravshipVehicleUtility.GenerateGravshipVehicleDef(props, this);
  }
}
