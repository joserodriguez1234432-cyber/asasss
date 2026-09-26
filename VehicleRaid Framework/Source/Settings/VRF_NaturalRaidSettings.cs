using System.Collections.Generic;
using UnityEngine;
using Verse;
using Vehicles;

namespace VehicleRaidFramework
{
    public enum VRF_PaintMode
    {
        Default,
        Fixed,
        Faction
    }

    public enum VRF_SpawnContext
    {
        Raid,
        Settlement,
        Outpost,
        Ambush,
        Gravship
    }

    public enum VRF_NaturalRaidBehavior
    {
        ImmediateAssault,
        HoldThenAssault,
    }

    public class VRF_GravshipRaidEntry : IExposable
    {
        public string presetName;
        public bool enabled = false;
        public float weight = 1f;
        public float combatPower = 0f;
        public float minRaidPoints = 0f;
        public float maxRaidPoints = 0f;
        public bool hoverMode = false;
        public List<string> allowedRaidStrategies = new List<string>();

        public VRF_GravshipRaidEntry() { }
        public VRF_GravshipRaidEntry(string name) { presetName = name; }

        public void ExposeData()
        {
            Scribe_Values.Look(ref presetName,    "presetName");
            Scribe_Values.Look(ref enabled,       "enabled",       false);
            Scribe_Values.Look(ref weight,        "weight",        1f);
            Scribe_Values.Look(ref combatPower,   "combatPower",   0f);
            Scribe_Values.Look(ref minRaidPoints, "minRaidPoints", 0f);
            Scribe_Values.Look(ref maxRaidPoints, "maxRaidPoints", 0f);
            Scribe_Values.Look(ref hoverMode,     "hoverMode",     false);
            Scribe_Collections.Look(ref allowedRaidStrategies, "allowedRaidStrategies", LookMode.Value);
            if (allowedRaidStrategies == null) allowedRaidStrategies = new List<string>();
        }
    }

    public class VRF_PaintConfig : IExposable
    {
        public VRF_PaintMode mode = VRF_PaintMode.Default;
        public Color colorOne   = Color.white;
        public Color colorTwo   = Color.white;
        public Color colorThree = Color.white;
        public string patternDefName = null;

        public PatternDef ResolvedPattern =>
            patternDefName != null
                ? DefDatabase<PatternDef>.GetNamedSilentFail(patternDefName) ?? PatternDefOf.Default
                : null;

        public void ExposeData()
        {
            Scribe_Values.Look(ref mode,           "mode",           VRF_PaintMode.Default);
            Scribe_Values.Look(ref colorOne,        "colorOne",       Color.white);
            Scribe_Values.Look(ref colorTwo,        "colorTwo",       Color.white);
            Scribe_Values.Look(ref colorThree,      "colorThree",     Color.white);
            Scribe_Values.Look(ref patternDefName,  "patternDefName", null);
        }
    }

    public class VRF_HoverConfig : IExposable
    {
        public string vehicleDefName;
        public bool  enabled                  = false;
        public string flightType              = "Hover";
        public float baseMoveSpeed            = 0f;
        public int   maxTicks                 = 600;
        public int   maxTicksVertical         = 400;
        public int   maxTicksPropeller        = 800;
        public float hoverAltitude            = 4f;
        public float hoverShadowOffset        = 1.5f;
        public float hoverBobAmount           = 0.22f;
        public float hoverBobSpeed            = 2.0f;
        public float hoverMoveSpeed           = 4.5f;
        public float hoverRotationSpeed       = 60f;
        public float angularVelocityPropeller = 59f;

        public VRF_HoverConfig() { }
        public VRF_HoverConfig(string defName) { vehicleDefName = defName; }

        public void ExposeData()
        {
            Scribe_Values.Look(ref vehicleDefName,           "vehicleDefName");
            Scribe_Values.Look(ref enabled,                  "enabled",                  false);
            Scribe_Values.Look(ref flightType,               "flightType",               "Hover");
            Scribe_Values.Look(ref baseMoveSpeed,            "baseMoveSpeed",            0f);
            Scribe_Values.Look(ref maxTicks,                 "maxTicks",                  600);
            Scribe_Values.Look(ref maxTicksVertical,         "maxTicksVertical",          400);
            Scribe_Values.Look(ref maxTicksPropeller,        "maxTicksPropeller",         800);
            Scribe_Values.Look(ref hoverAltitude,            "hoverAltitude",             4f);
            Scribe_Values.Look(ref hoverShadowOffset,        "hoverShadowOffset",         1.5f);
            Scribe_Values.Look(ref hoverBobAmount,           "hoverBobAmount",            0.22f);
            Scribe_Values.Look(ref hoverBobSpeed,            "hoverBobSpeed",             2.0f);
            Scribe_Values.Look(ref hoverMoveSpeed,           "hoverMoveSpeed",            4.5f);
            Scribe_Values.Look(ref hoverRotationSpeed,       "hoverRotationSpeed",        60f);
            Scribe_Values.Look(ref angularVelocityPropeller, "angularVelocityPropeller",  59f);
        }
    }

    public class VRF_UpgradeLoadout : IExposable
    {
        public List<string> nodeKeys = new List<string>();
        public List<VRF_TurretAmmoEntry> upgradeAmmo = new List<VRF_TurretAmmoEntry>();

        public VRF_UpgradeLoadout() { }

        public VRF_TurretAmmoEntry GetOrCreateUpgradeAmmo(string turretKey)
        {
            var t = upgradeAmmo.Find(e => e.turretKey == turretKey);
            if (t == null)
            {
                t = new VRF_TurretAmmoEntry(turretKey);
                upgradeAmmo.Add(t);
            }
            return t;
        }

        public void ExposeData()
        {
            Scribe_Collections.Look(ref nodeKeys,     "nodeKeys",     LookMode.Value);
            Scribe_Collections.Look(ref upgradeAmmo,  "upgradeAmmo",  LookMode.Deep);
            if (nodeKeys    == null) nodeKeys    = new List<string>();
            if (upgradeAmmo == null) upgradeAmmo = new List<VRF_TurretAmmoEntry>();
        }
    }

    public class VRF_TurretAmmoEntry : IExposable
    {
        public string turretKey;
        public float ammoPercent = 50f;

        public VRF_TurretAmmoEntry() { }

        public VRF_TurretAmmoEntry(string key)
        {
            turretKey = key;
        }

        public void ExposeData()
        {
            Scribe_Values.Look(ref turretKey, "turretKey");
            Scribe_Values.Look(ref ammoPercent, "ammoPercent", 50f);
        }
    }

    public class VRF_NaturalRaidVehicleEntry : IExposable
    {
        public string vehicleKindDefName;
        public bool   enabled              = false;
        public float  combatPowerOverride  = 0f;
        public float  minRaidPoints        = 0f;
        public float  fuelPercent          = 100f;
        public List<VRF_TurretAmmoEntry> turretAmmo = new List<VRF_TurretAmmoEntry>();
        public bool   helicopterMode       = false;
        public float  helicopterMoveSpeed  = 8.5f;
        public string airVehicleType       = "Helicopter"; 
        public float  maxRaidPoints        = 0f;
        public VRF_PaintConfig paintConfig = null;
        public List<VRF_UpgradeLoadout> upgradeLoadouts = new List<VRF_UpgradeLoadout>();
        public bool   forceSpawn           = false;
        public List<string> allowedRaidStrategies = new List<string>();  
        public bool   isSiegeDrop          = false;
        public bool   allowDropPod         = true;
        public string gravshipPresetName   = null;

        public VRF_NaturalRaidVehicleEntry() { }

        public VRF_NaturalRaidVehicleEntry(string defName)
        {
            vehicleKindDefName = defName;
        }

        public VRF_TurretAmmoEntry GetOrCreateTurretAmmo(string turretKey)
        {
            var t = turretAmmo.Find(e => e.turretKey == turretKey);
            if (t == null)
            {
                t = new VRF_TurretAmmoEntry(turretKey);
                turretAmmo.Add(t);
            }
            return t;
        }

        public void ExposeData()
        {
            Scribe_Values.Look(ref vehicleKindDefName,  "vehicleKindDefName");
            Scribe_Values.Look(ref enabled,             "enabled",             false);
            Scribe_Values.Look(ref combatPowerOverride, "combatPowerOverride", 0f);
            Scribe_Values.Look(ref minRaidPoints,       "minRaidPoints",       0f);
            Scribe_Values.Look(ref fuelPercent,         "fuelPercent",         100f);
            Scribe_Collections.Look(ref turretAmmo,     "turretAmmo",          LookMode.Deep);
            if (turretAmmo == null) turretAmmo = new List<VRF_TurretAmmoEntry>();
            Scribe_Values.Look(ref helicopterMode,      "helicopterMode",      false);
            Scribe_Values.Look(ref helicopterMoveSpeed, "helicopterMoveSpeed", 8.5f);
            Scribe_Values.Look(ref airVehicleType,      "airVehicleType",      "Helicopter");
            Scribe_Values.Look(ref maxRaidPoints,       "maxRaidPoints",       0f);
            Scribe_Deep.Look(ref paintConfig,           "paintConfig");
            Scribe_Collections.Look(ref upgradeLoadouts,"upgradeLoadouts",     LookMode.Deep);
            if (upgradeLoadouts == null) upgradeLoadouts = new List<VRF_UpgradeLoadout>();
            Scribe_Values.Look(ref forceSpawn,          "forceSpawn",          false);
            Scribe_Collections.Look(ref allowedRaidStrategies, "allowedRaidStrategies", LookMode.Value);
            if (allowedRaidStrategies == null) allowedRaidStrategies = new List<string>();
            Scribe_Values.Look(ref isSiegeDrop, "isSiegeDrop", false);
            Scribe_Values.Look(ref allowDropPod, "allowDropPod", true);
            Scribe_Values.Look(ref gravshipPresetName, "gravshipPresetName", null);
        }
    }

    public class VRF_NaturalRaidFactionConfig : IExposable
    {
        public string factionDefName;
        public List<VRF_NaturalRaidVehicleEntry> vehicleEntries           = new List<VRF_NaturalRaidVehicleEntry>();
        public List<VRF_NaturalRaidVehicleEntry> settlementVehicleEntries = new List<VRF_NaturalRaidVehicleEntry>();
        public List<VRF_NaturalRaidVehicleEntry> outpostVehicleEntries    = new List<VRF_NaturalRaidVehicleEntry>();
        public List<VRF_NaturalRaidVehicleEntry> ambushVehicleEntries     = new List<VRF_NaturalRaidVehicleEntry>();
        public List<VRF_GravshipRaidEntry> gravshipEntries                = new List<VRF_GravshipRaidEntry>();
        public float settlementBudget = 600f;
        public float outpostBudget    = 600f;
        public float ambushBudget     = 600f;

        public VRF_NaturalRaidBehavior raidBehavior = VRF_NaturalRaidBehavior.ImmediateAssault;
        public int holdTicks = 3000;

        public VRF_NaturalRaidFactionConfig() { }
        public VRF_NaturalRaidFactionConfig(string defName) { factionDefName = defName; }

        public VRF_GravshipRaidEntry GetOrCreateGravshipEntry(string presetName)
        {
            if (gravshipEntries == null) gravshipEntries = new List<VRF_GravshipRaidEntry>();
            var entry = gravshipEntries.Find(e => e.presetName == presetName);
            if (entry == null)
            {
                entry = new VRF_GravshipRaidEntry(presetName);
                gravshipEntries.Add(entry);
            }
            return entry;
        }

        public List<VRF_NaturalRaidVehicleEntry> GetEntriesForContext(VRF_SpawnContext ctx)
        {
            switch (ctx)
            {
                case VRF_SpawnContext.Settlement: return settlementVehicleEntries;
                case VRF_SpawnContext.Outpost:    return outpostVehicleEntries;
                case VRF_SpawnContext.Ambush:     return ambushVehicleEntries;
                default:                          return vehicleEntries;
            }
        }

        public VRF_NaturalRaidVehicleEntry GetOrCreate(string kindDefName)
        {
            return GetOrCreateForContext(kindDefName, VRF_SpawnContext.Raid);
        }

        public VRF_NaturalRaidVehicleEntry GetOrCreateForContext(string kindDefName, VRF_SpawnContext ctx)
        {
            var list  = GetEntriesForContext(ctx);
            var entry = list.Find(e => e.vehicleKindDefName == kindDefName);
            if (entry == null)
            {
                entry = new VRF_NaturalRaidVehicleEntry(kindDefName);
                list.Add(entry);
            }
            return entry;
        }

        public void ExposeData()
        {
            Scribe_Values.Look(ref factionDefName,   "factionDefName");
            Scribe_Collections.Look(ref vehicleEntries,           "vehicleEntries",           LookMode.Deep);
            Scribe_Collections.Look(ref settlementVehicleEntries, "settlementVehicleEntries", LookMode.Deep);
            Scribe_Collections.Look(ref outpostVehicleEntries,    "outpostVehicleEntries",    LookMode.Deep);
            Scribe_Collections.Look(ref ambushVehicleEntries,     "ambushVehicleEntries",     LookMode.Deep);
            Scribe_Collections.Look(ref gravshipEntries,          "gravshipEntries",          LookMode.Deep);
            if (vehicleEntries           == null) vehicleEntries           = new List<VRF_NaturalRaidVehicleEntry>();
            if (settlementVehicleEntries == null) settlementVehicleEntries = new List<VRF_NaturalRaidVehicleEntry>();
            if (outpostVehicleEntries    == null) outpostVehicleEntries    = new List<VRF_NaturalRaidVehicleEntry>();
            if (ambushVehicleEntries     == null) ambushVehicleEntries     = new List<VRF_NaturalRaidVehicleEntry>();
            if (gravshipEntries          == null) gravshipEntries          = new List<VRF_GravshipRaidEntry>();
            Scribe_Values.Look(ref settlementBudget, "settlementBudget", 600f);
            Scribe_Values.Look(ref outpostBudget,    "outpostBudget",    600f);
            Scribe_Values.Look(ref ambushBudget,     "ambushBudget",     600f);
            Scribe_Values.Look(ref raidBehavior,     "raidBehavior",     VRF_NaturalRaidBehavior.ImmediateAssault);
            Scribe_Values.Look(ref holdTicks,        "holdTicks",        3000);
        }
    }

    public class VRF_ModSettings : ModSettings
    {
        public Dictionary<string, float> thrusterSpeedSettings = new Dictionary<string, float>();

        public List<VRF_NaturalRaidFactionConfig> factionConfigs = new List<VRF_NaturalRaidFactionConfig>();
        public bool autoLoadPresets = true;
        public List<VRF_HoverConfig> hoverConfigs = new List<VRF_HoverConfig>();

        public float WeightHP        = 1.0f;
        public float WeightArmor     = 60f;
        public float WeightDPS       = 18f;
        public float WeightRange     = 0.8f;
        public float WeightSpeed     = 12f;
        public float WeightDrivers   = 20f;
        public float WeightGunners   = 15f;
        public float WeightPassengers = 8f;
        public float WeightSize      = 6f;
        public float AirMultiplier   = 1.25f;

        public float AutoFillAmmoFraction = 0.5f;

        public bool AutoFillTransportVehicles = true;

        public bool AutoFillSiegeDropVehicles = true;

        public bool SiegeDropOnlyOnDropRaids = true;

        public bool allowGlobalDropPodRaids = true;

        public float VehiclePointsFraction = 0.5f;

        // Vehicle Map Framework vehicles each maintain a complete interior map.  Keep the
        // default deliberately low so one large raid cannot create a costly number of maps.
        public int MaxVehicleMapSpawnsPerRaid = 4;

        public List<string> globalExcludedRaidStrategies = new List<string>();

        public bool VerboseAILogging = false;

        public VRF_NaturalRaidFactionConfig GetOrCreateFactionConfig(string factionDefName)
        {
            var config = factionConfigs.Find(c => c.factionDefName == factionDefName);
            if (config == null)
            {
                config = new VRF_NaturalRaidFactionConfig(factionDefName);
                factionConfigs.Add(config);
            }
            return config;
        }

        public VRF_NaturalRaidFactionConfig GetFactionConfig(string factionDefName)
        {
            return factionConfigs.Find(c => c.factionDefName == factionDefName);
        }

        public VRF_HoverConfig GetOrCreateHoverConfig(string vehicleDefName)
        {
            var cfg = hoverConfigs.Find(h => h.vehicleDefName == vehicleDefName);
            if (cfg == null)
            {
                cfg = new VRF_HoverConfig(vehicleDefName);
                hoverConfigs.Add(cfg);
            }
            return cfg;
        }

        public VRF_HoverConfig GetHoverConfig(string vehicleDefName)
        {
            return hoverConfigs.Find(h => h.vehicleDefName == vehicleDefName);
        }

        public void ResetAllSettings()
        {
            factionConfigs.Clear();
            globalExcludedRaidStrategies.Clear();
        }

        public void ResetEstimatorSettings()
        {
            WeightHP        = 1.0f;
            WeightArmor     = 60f;
            WeightDPS       = 18f;
            WeightRange     = 0.8f;
            WeightSpeed     = 12f;
            WeightDrivers   = 20f;
            WeightGunners   = 15f;
            WeightPassengers = 8f;
            WeightSize      = 6f;
            AirMultiplier   = 1.25f;
            AutoFillTransportVehicles = true;
            AutoFillSiegeDropVehicles = true;
            SiegeDropOnlyOnDropRaids  = true;
            MaxVehicleMapSpawnsPerRaid = 4;
        }

        public override void ExposeData()
        {
            Scribe_Collections.Look(ref factionConfigs, "factionConfigs", LookMode.Deep);
            if (factionConfigs == null) factionConfigs = new List<VRF_NaturalRaidFactionConfig>();
            Scribe_Values.Look(ref autoLoadPresets, "autoLoadPresets", true);
            Scribe_Collections.Look(ref hoverConfigs, "hoverConfigs", LookMode.Deep);
            if (hoverConfigs == null) hoverConfigs = new List<VRF_HoverConfig>();

            Scribe_Values.Look(ref WeightHP,        "WeightHP",        1.0f);
            Scribe_Values.Look(ref WeightArmor,     "WeightArmor",     60f);
            Scribe_Values.Look(ref WeightDPS,       "WeightDPS",       18f);
            Scribe_Values.Look(ref WeightRange,     "WeightRange",     0.8f);
            Scribe_Values.Look(ref WeightSpeed,     "WeightSpeed",     12f);
            Scribe_Values.Look(ref WeightDrivers,   "WeightDrivers",   20f);
            Scribe_Values.Look(ref WeightGunners,   "WeightGunners",   15f);
            Scribe_Values.Look(ref WeightPassengers,"WeightPassengers",8f);
            Scribe_Values.Look(ref WeightSize,      "WeightSize",      6f);
            Scribe_Values.Look(ref AirMultiplier,   "AirMultiplier",   1.25f);
            Scribe_Values.Look(ref AutoFillAmmoFraction, "AutoFillAmmoFraction", 0.5f);
            Scribe_Values.Look(ref AutoFillTransportVehicles, "AutoFillTransportVehicles", true);
            Scribe_Values.Look(ref AutoFillSiegeDropVehicles, "AutoFillSiegeDropVehicles", true);
            Scribe_Values.Look(ref SiegeDropOnlyOnDropRaids, "SiegeDropOnlyOnDropRaids", true);
            Scribe_Values.Look(ref allowGlobalDropPodRaids, "allowGlobalDropPodRaids", true);
            Scribe_Values.Look(ref VehiclePointsFraction, "VehiclePointsFraction", 0.5f);
            Scribe_Values.Look(ref MaxVehicleMapSpawnsPerRaid, "MaxVehicleMapSpawnsPerRaid", 4);
            MaxVehicleMapSpawnsPerRaid = Mathf.Clamp(MaxVehicleMapSpawnsPerRaid, 0, 100);
            Scribe_Collections.Look(ref globalExcludedRaidStrategies, "globalExcludedRaidStrategies", LookMode.Value);
            if (globalExcludedRaidStrategies == null) globalExcludedRaidStrategies = new List<string>();
            Scribe_Values.Look(ref VerboseAILogging, "VerboseAILogging", false);
        }
    }
}
