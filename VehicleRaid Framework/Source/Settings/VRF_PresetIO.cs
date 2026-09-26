using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Serialization;
using UnityEngine;
using Verse;
using RimWorld;
using Vehicles;
using VehicleRaid;

namespace VehicleRaidFramework
{
    public static class VRF_PresetIO
    {
        private const string PresetFolder = "NaturalRaidPresets";

        public static string ExportPath => Path.Combine(
            GenFilePaths.ConfigFolderPath, "VehicleRaidFramework", "Presets");

        public static List<string> FindAllPresetFiles()
        {
            var files = new List<string>();

            foreach (ModContentPack mod in LoadedModManager.RunningMods)
            {
                string folder = Path.Combine(mod.RootDir, PresetFolder);
                if (Directory.Exists(folder))
                {
                    foreach (string f in Directory.GetFiles(folder, "*.xml"))
                        files.Add(f);
                }
            }

            if (Directory.Exists(ExportPath))
            {
                foreach (string f in Directory.GetFiles(ExportPath, "*.xml"))
                    files.Add(f);
            }

            return files;
        }

        public static void AutoLoadAllPresets(VRF_ModSettings settings)
        {
            var claimed = new HashSet<string>();

            bool any = false;
            foreach (ModContentPack mod in LoadedModManager.RunningMods)
            {
                string folder = Path.Combine(mod.RootDir, PresetFolder);
                if (!Directory.Exists(folder)) continue;

                foreach (string f in Directory.GetFiles(folder, "*.xml"))
                {
                    try
                    {
                        XmlDocument doc = new XmlDocument();
                        doc.Load(f);
                        XmlElement root = doc.DocumentElement;
                        if (root == null || root.Name != "VRF_NaturalRaidPreset") continue;

                        foreach (XmlElement factionEl in root.SelectNodes("FactionConfig"))
                        {
                            string factionName = factionEl.GetAttribute("faction");
                            if (string.IsNullOrEmpty(factionName)) continue;

                            var factionConfig = settings.GetOrCreateFactionConfig(factionName);

                            foreach (XmlElement vehicleEl in factionEl.SelectNodes("VehicleEntry"))
                            {
                                string kindName = vehicleEl.GetAttribute("kind");
                                if (string.IsNullOrEmpty(kindName)) continue;

                                string claimKey = factionName + "::" + kindName;

                                bool.TryParse(vehicleEl.GetAttribute("enabled"), out bool enabled);

                                if (claimed.Contains(claimKey)) continue;

                                var vehicleEntry = factionConfig.GetOrCreate(kindName);

                                vehicleEntry.enabled = enabled;
                                if (float.TryParse(vehicleEl.GetAttribute("combatPower"), out float cp))
                                    vehicleEntry.combatPowerOverride = cp;
                                if (float.TryParse(vehicleEl.GetAttribute("minRaidPoints"), out float mrp))
                                    vehicleEntry.minRaidPoints = mrp;
                                if (float.TryParse(vehicleEl.GetAttribute("fuelPercent"), out float fp))
                                    vehicleEntry.fuelPercent = fp;
                                if (bool.TryParse(vehicleEl.GetAttribute("allowDropPod"), out bool adp))
                                    vehicleEntry.allowDropPod = adp;

                                foreach (XmlElement turretEl in vehicleEl.SelectNodes("TurretAmmo"))
                                {
                                    string turretKey = turretEl.GetAttribute("turret");
                                    if (string.IsNullOrEmpty(turretKey)) continue;
                                    if (float.TryParse(turretEl.GetAttribute("ammoPercent"), out float ap))
                                        vehicleEntry.GetOrCreateTurretAmmo(turretKey).ammoPercent = ap;
                                }

                                if (enabled)
                                    claimed.Add(claimKey);

                                any = true;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Log.Warning($"[VRF] Failed to auto-load preset '{f}': {ex.Message}");
                    }
                }
            }

            if (any)
                VRF_Mod.Instance.WriteSettings();
        }

        public static void Export(VRF_ModSettings settings, string fileName)
        {
            try
            {
                if (!Directory.Exists(ExportPath))
                    Directory.CreateDirectory(ExportPath);

                string fullPath = Path.Combine(ExportPath, fileName + ".xml");

                XmlWriterSettings xmlSettings = new XmlWriterSettings
                {
                    Indent = true,
                    IndentChars = "  ",
                    NewLineChars = "\n"
                };

                using (XmlWriter writer = XmlWriter.Create(fullPath, xmlSettings))
                {
                    writer.WriteStartDocument();
                    writer.WriteStartElement("VRF_NaturalRaidPreset");

                    foreach (var factionConfig in settings.factionConfigs)
                    {
                        if (factionConfig.vehicleEntries.Count == 0) continue;

                        writer.WriteStartElement("FactionConfig");
                        writer.WriteAttributeString("faction", factionConfig.factionDefName);

                        foreach (var vehicleEntry in factionConfig.vehicleEntries)
                        {
                            writer.WriteStartElement("VehicleEntry");
                            writer.WriteAttributeString("kind", vehicleEntry.vehicleKindDefName);
                            writer.WriteAttributeString("enabled", vehicleEntry.enabled.ToString().ToLower());
                            writer.WriteAttributeString("combatPower", vehicleEntry.combatPowerOverride.ToString("F0"));
                            writer.WriteAttributeString("minRaidPoints", vehicleEntry.minRaidPoints.ToString("F0"));
                            writer.WriteAttributeString("fuelPercent", vehicleEntry.fuelPercent.ToString("F1"));
                            writer.WriteAttributeString("allowDropPod", vehicleEntry.allowDropPod.ToString().ToLower());

                            foreach (var turretAmmo in vehicleEntry.turretAmmo)
                            {
                                writer.WriteStartElement("TurretAmmo");
                                writer.WriteAttributeString("turret", turretAmmo.turretKey);
                                writer.WriteAttributeString("ammoPercent", turretAmmo.ammoPercent.ToString("F1"));
                                writer.WriteEndElement();
                            }

                            writer.WriteEndElement();
                        }

                        writer.WriteEndElement(); 
                    }

                    if (!settings.hoverConfigs.NullOrEmpty())
                    {
                        foreach (var hcfg in settings.hoverConfigs)
                        {
                            writer.WriteStartElement("HoverConfig");
                            writer.WriteAttributeString("vehicleDef",           hcfg.vehicleDefName);
                            writer.WriteAttributeString("maxTicks",             hcfg.maxTicks.ToString());
                            writer.WriteAttributeString("maxTicksVertical",     hcfg.maxTicksVertical.ToString());
                            writer.WriteAttributeString("maxTicksPropeller",    hcfg.maxTicksPropeller.ToString());
                            writer.WriteAttributeString("hoverAltitude",        hcfg.hoverAltitude.ToString("F3"));
                            writer.WriteAttributeString("hoverShadowOffset",    hcfg.hoverShadowOffset.ToString("F3"));
                            writer.WriteAttributeString("hoverBobAmount",       hcfg.hoverBobAmount.ToString("F4"));
                            writer.WriteAttributeString("hoverBobSpeed",        hcfg.hoverBobSpeed.ToString("F3"));
                            writer.WriteAttributeString("hoverMoveSpeed",       hcfg.hoverMoveSpeed.ToString("F3"));
                            writer.WriteAttributeString("angVelPropeller",      hcfg.angularVelocityPropeller.ToString("F2"));
                            writer.WriteAttributeString("baseMoveSpeed",        hcfg.baseMoveSpeed.ToString("F3"));
                            writer.WriteEndElement();
                        }
                    }

                    writer.WriteEndElement();
                }

                Messages.Message("VRF_Preset_Exported".Translate(fullPath), MessageTypeDefOf.PositiveEvent, false);
            }
            catch (Exception ex)
            {
                Log.Error($"[VRF] Failed to export preset: {ex}");
                Messages.Message("VRF_Preset_ExportFailed".Translate(), MessageTypeDefOf.RejectInput, false);
            }
        }

        public static bool Import(string filePath, VRF_ModSettings settings)
        {
            try
            {
                XmlDocument doc = new XmlDocument();
                doc.Load(filePath);

                XmlElement root = doc.DocumentElement;
                if (root == null || root.Name != "VRF_NaturalRaidPreset")
                {
                    Messages.Message("VRF_Preset_InvalidFile".Translate(), MessageTypeDefOf.RejectInput, false);
                    return false;
                }

                foreach (XmlElement factionEl in root.SelectNodes("FactionConfig"))
                {
                    string factionName = factionEl.GetAttribute("faction");
                    if (string.IsNullOrEmpty(factionName)) continue;

                    var factionConfig = settings.GetOrCreateFactionConfig(factionName);

                    foreach (XmlElement vehicleEl in factionEl.SelectNodes("VehicleEntry"))
                    {
                        string kindName = vehicleEl.GetAttribute("kind");
                        if (string.IsNullOrEmpty(kindName)) continue;

                        var vehicleEntry = factionConfig.GetOrCreate(kindName);

                        if (bool.TryParse(vehicleEl.GetAttribute("enabled"), out bool enabled))
                            vehicleEntry.enabled = enabled;
                        if (float.TryParse(vehicleEl.GetAttribute("combatPower"), out float cp))
                            vehicleEntry.combatPowerOverride = cp;
                        if (float.TryParse(vehicleEl.GetAttribute("minRaidPoints"), out float mrp))
                            vehicleEntry.minRaidPoints = mrp;
                        if (float.TryParse(vehicleEl.GetAttribute("fuelPercent"), out float fp))
                            vehicleEntry.fuelPercent = fp;
                        if (bool.TryParse(vehicleEl.GetAttribute("allowDropPod"), out bool adp))
                            vehicleEntry.allowDropPod = adp;

                        foreach (XmlElement turretEl in vehicleEl.SelectNodes("TurretAmmo"))
                        {
                            string turretKey = turretEl.GetAttribute("turret");
                            if (string.IsNullOrEmpty(turretKey)) continue;
                            if (float.TryParse(turretEl.GetAttribute("ammoPercent"), out float ap))
                                vehicleEntry.GetOrCreateTurretAmmo(turretKey).ammoPercent = ap;
                        }
                    }
                }

                foreach (XmlElement hoverEl in root.SelectNodes("HoverConfig"))
                {
                    string vDefName = hoverEl.GetAttribute("vehicleDef");
                    if (string.IsNullOrEmpty(vDefName)) continue;
                    var hcfg = settings.GetOrCreateHoverConfig(vDefName);
                    if (int.TryParse(hoverEl.GetAttribute("maxTicks"),          out int mt))  hcfg.maxTicks                 = mt;
                    if (int.TryParse(hoverEl.GetAttribute("maxTicksVertical"),   out int mtv)) hcfg.maxTicksVertical         = mtv;
                    if (int.TryParse(hoverEl.GetAttribute("maxTicksPropeller"),  out int mtp)) hcfg.maxTicksPropeller        = mtp;
                    if (float.TryParse(hoverEl.GetAttribute("hoverAltitude"),    out float ha))  hcfg.hoverAltitude            = ha;
                    if (float.TryParse(hoverEl.GetAttribute("hoverShadowOffset"),out float hs))  hcfg.hoverShadowOffset        = hs;
                    if (float.TryParse(hoverEl.GetAttribute("hoverBobAmount"),   out float hba)) hcfg.hoverBobAmount           = hba;
                    if (float.TryParse(hoverEl.GetAttribute("hoverBobSpeed"),    out float hbs)) hcfg.hoverBobSpeed            = hbs;
                    if (float.TryParse(hoverEl.GetAttribute("hoverMoveSpeed"),   out float hms)) hcfg.hoverMoveSpeed           = hms;
                    if (float.TryParse(hoverEl.GetAttribute("angVelPropeller"),  out float avp)) hcfg.angularVelocityPropeller = avp;
                    if (float.TryParse(hoverEl.GetAttribute("baseMoveSpeed"),    out float bms)) hcfg.baseMoveSpeed             = bms;

                    VehicleDef vDef = DefDatabase<VehicleDef>.GetNamedSilentFail(vDefName);
                    if (vDef != null)
                    {
                        var props = vDef.comps?.OfType<CompProperties_VehicleHover>().FirstOrDefault();
                        if (props != null)
                        {
                            props.maxTicks          = hcfg.maxTicks;
                            props.maxTicksVertical  = hcfg.maxTicksVertical;
                            props.maxTicksPropeller = hcfg.maxTicksPropeller;
                            props.hoverAltitude     = hcfg.hoverAltitude;
                            props.hoverShadowOffset = hcfg.hoverShadowOffset;
                            props.hoverBobAmount    = hcfg.hoverBobAmount;
                            props.hoverBobSpeed     = hcfg.hoverBobSpeed;
                            props.hoverMoveSpeed    = hcfg.hoverMoveSpeed;
                        }
                    }
                }

                VRF_Mod.Instance.WriteSettings();
                Messages.Message("VRF_Preset_Imported".Translate(Path.GetFileNameWithoutExtension(filePath)),
                    MessageTypeDefOf.PositiveEvent, false);
                return true;
            }
            catch (Exception ex)
            {
                Log.Error($"[VRF] Failed to import preset: {ex}");
                Messages.Message("VRF_Preset_ImportFailed".Translate(), MessageTypeDefOf.RejectInput, false);
                return false;
            }
        }

        public static void ExportHoverAsPatch(VRF_HoverConfig cfg, string vehicleLabel)
        {
            try
            {
                string patchesPath = Path.Combine(
                    LoadedModManager.RunningMods
                        .FirstOrDefault(m => m.assemblies.loadedAssemblies
                            .Any(a => a.GetName().Name == "VehicleRaidFramework"))
                        ?.RootDir ?? GenFilePaths.ConfigFolderPath,
                    "Patches");

                if (!Directory.Exists(patchesPath))
                    Directory.CreateDirectory(patchesPath);

                string safeName = string.Concat((cfg.vehicleDefName ?? "Unknown").Split(Path.GetInvalidFileNameChars()));
                string fullPath = Path.Combine(patchesPath, $"HoverConfig_{safeName}.xml");

                VehicleDef vDef = DefDatabase<VehicleDef>.GetNamedSilentFail(cfg.vehicleDefName);
                string mayRequireId = vDef?.modContentPack?.PackageId;

                var xmlSettings = new XmlWriterSettings { Indent = true, IndentChars = "\t", NewLineChars = "\n" };

                string mayRequireNormalized = mayRequireId?.ToLowerInvariant()
                    .Replace("_steam", "").Replace("_local", "");

                using (XmlWriter w = XmlWriter.Create(fullPath, xmlSettings))
                {
                    w.WriteStartDocument();
                    w.WriteStartElement("Patch");

                    w.WriteStartElement("Operation");
                    w.WriteAttributeString("Class", "PatchOperationFindMod");
                    w.WriteStartElement("mods");
                    string modName = vDef?.modContentPack?.Name ?? cfg.vehicleDefName;
                    w.WriteElementString("li", modName);
                    w.WriteEndElement();
                    w.WriteStartElement("match");
                    w.WriteAttributeString("Class", "PatchOperationSequence");
                    w.WriteStartElement("operations");

                    if (cfg.baseMoveSpeed > 0f)
                    {
                        w.WriteStartElement("li");
                        w.WriteAttributeString("Class", "PatchOperationConditional");
                        w.WriteElementString("xpath", $"//Vehicles.VehicleDef[defName=\"{cfg.vehicleDefName}\"]/vehicleStats/MoveSpeed");
                        w.WriteStartElement("match");
                        w.WriteAttributeString("Class", "PatchOperationReplace");
                        w.WriteElementString("xpath", $"//Vehicles.VehicleDef[defName=\"{cfg.vehicleDefName}\"]/vehicleStats/MoveSpeed");
                        w.WriteStartElement("value");
                        w.WriteElementString("MoveSpeed", cfg.baseMoveSpeed.ToString("F2"));
                        w.WriteEndElement();
                        w.WriteEndElement(); 
                        w.WriteEndElement(); 
                    }

                    w.WriteStartElement("li");
                    w.WriteAttributeString("Class", "PatchOperationConditional");
                    w.WriteElementString("xpath", $"//Vehicles.VehicleDef[defName=\"{cfg.vehicleDefName}\"]/comps");
                    w.WriteStartElement("match");
                    w.WriteAttributeString("Class", "PatchOperationAdd");
                    w.WriteElementString("xpath", $"//Vehicles.VehicleDef[defName=\"{cfg.vehicleDefName}\"]/comps");
                    w.WriteStartElement("value");
                    WriteHoverCompXml(w, cfg);
                    w.WriteEndElement(); 
                    w.WriteEndElement(); 
                    w.WriteStartElement("nomatch");
                    w.WriteAttributeString("Class", "PatchOperationAdd");
                    w.WriteElementString("xpath", $"//Vehicles.VehicleDef[defName=\"{cfg.vehicleDefName}\"]");
                    w.WriteStartElement("value");
                    w.WriteStartElement("comps");
                    WriteHoverCompXml(w, cfg);
                    w.WriteEndElement(); 
                    w.WriteEndElement(); 
                    w.WriteEndElement(); 
                    w.WriteEndElement(); 

                    w.WriteEndElement(); 
                    w.WriteEndElement(); 
                    w.WriteEndElement(); 

                    w.WriteEndElement(); 
                    w.WriteEndDocument();
                }

                Messages.Message("VRF_Hover_PatchExported".Translate(fullPath), MessageTypeDefOf.PositiveEvent, false);
            }
            catch (Exception ex)
            {
                Log.Error($"[VRF] Failed to export hover patch: {ex}");
                Messages.Message("VRF_Hover_PatchExportFailed".Translate(), MessageTypeDefOf.RejectInput, false);
            }
        }

        private static void WriteHoverCompXml(XmlWriter w, VRF_HoverConfig cfg)
        {
            bool isAirplane = cfg.flightType == "Airplane";

            w.WriteStartElement("li");
            w.WriteAttributeString("Class", "VehicleRaid.CompProperties_VehicleHover");

            if (isAirplane)
                w.WriteElementString("flightType", "Airplane");

            w.WriteElementString("maxTicks",          cfg.maxTicks.ToString());
            w.WriteElementString("maxTicksVertical",  cfg.maxTicksVertical.ToString());
            w.WriteElementString("maxTicksPropeller", cfg.maxTicksPropeller.ToString());
            w.WriteElementString("hoverAltitude",     cfg.hoverAltitude.ToString("F2"));
            w.WriteElementString("hoverMoveSpeed",    cfg.hoverMoveSpeed.ToString("F2"));
            w.WriteElementString("hoverShadowOffset", cfg.hoverShadowOffset.ToString("F2"));

            if (isAirplane)
            {
                w.WriteElementString("hoverRotationSpeed", cfg.hoverRotationSpeed.ToString("F1"));

                w.WriteStartElement("shadowAlphaPropellerCurve");
                w.WriteAttributeString("Class", "SmashTools.BezierCurve");
                w.WriteStartElement("points");
                w.WriteElementString("li", "(0, 0)");
                w.WriteElementString("li", "(0.2, 0.4)");
                w.WriteElementString("li", "(0.5, 0.6)");
                w.WriteElementString("li", "(1, 0.6)");
                w.WriteEndElement();
                w.WriteEndElement();

                w.WriteStartElement("xPositionCurve");
                w.WriteAttributeString("Class", "SmashTools.BezierCurve");
                w.WriteStartElement("points");
                w.WriteElementString("li", "(0, 0)");
                w.WriteElementString("li", "(0.4, 6)");
                w.WriteElementString("li", "(0.7, 16)");
                w.WriteElementString("li", "(1, 30)");
                w.WriteEndElement();
                w.WriteEndElement();

                w.WriteStartElement("zPositionCurve");
                w.WriteAttributeString("Class", "SmashTools.BezierCurve");
                w.WriteStartElement("points");
                w.WriteElementString("li", "(0, 0)");
                w.WriteElementString("li", "(0.6, 0)");
                w.WriteElementString("li", "(0.8, 0.2)");
                w.WriteElementString("li", "(1, 0.5)");
                w.WriteEndElement();
                w.WriteEndElement();

                w.WriteStartElement("rotationCurve");
                w.WriteAttributeString("Class", "SmashTools.BezierCurve");
                w.WriteStartElement("points");
                w.WriteElementString("li", "(0, 0)");
                w.WriteElementString("li", "(0.7, 0)");
                w.WriteElementString("li", "(0.9, -15)");
                w.WriteElementString("li", "(1, 0)");
                w.WriteEndElement();
                w.WriteEndElement();

                w.WriteElementString("runwayClearCells", "30");
                w.WriteElementString("landingMaxTicks", "600");

                w.WriteStartElement("landingForwardCurve");
                w.WriteAttributeString("Class", "SmashTools.BezierCurve");
                w.WriteStartElement("points");
                w.WriteElementString("li", "(0, 30)");
                w.WriteElementString("li", "(0.4, 20)");
                w.WriteElementString("li", "(0.7, 8)");
                w.WriteElementString("li", "(1, 0)");
                w.WriteEndElement();
                w.WriteEndElement();

                w.WriteStartElement("landingAltitudeCurve");
                w.WriteAttributeString("Class", "SmashTools.BezierCurve");
                w.WriteStartElement("points");
                w.WriteElementString("li", "(0, 0.5)");
                w.WriteElementString("li", "(0.3, 0.2)");
                w.WriteElementString("li", "(0.4, 0)");
                w.WriteElementString("li", "(1, 0)");
                w.WriteEndElement();
                w.WriteEndElement();

                w.WriteStartElement("landingRotationCurve");
                w.WriteAttributeString("Class", "SmashTools.BezierCurve");
                w.WriteStartElement("points");
                w.WriteElementString("li", "(0, 0)");
                w.WriteElementString("li", "(0.2, -10)");
                w.WriteElementString("li", "(0.4, 0)");
                w.WriteElementString("li", "(1, 0)");
                w.WriteEndElement();
                w.WriteEndElement();
            }
            else
            {
                w.WriteElementString("hoverBobAmount", cfg.hoverBobAmount.ToString("F3"));
                w.WriteElementString("hoverBobSpeed",  cfg.hoverBobSpeed.ToString("F2"));

                w.WriteStartElement("rotationCurve");
                w.WriteAttributeString("Class", "SmashTools.BezierCurve");
                w.WriteStartElement("points");
                w.WriteElementString("li", "(0, 0)");
                w.WriteElementString("li", "(0.3, 2)");
                w.WriteElementString("li", "(0.5, 3)");
                w.WriteElementString("li", "(0.7, 2)");
                w.WriteElementString("li", "(1, 0)");
                w.WriteEndElement();
                w.WriteEndElement();

                w.WriteStartElement("rotationVerticalCurve");
                w.WriteAttributeString("Class", "SmashTools.BezierCurve");
                w.WriteStartElement("points");
                w.WriteElementString("li", "(0, 0)");
                w.WriteElementString("li", "(0.2, 3)");
                w.WriteElementString("li", "(0.4, 5)");
                w.WriteElementString("li", "(0.6, 4)");
                w.WriteElementString("li", "(0.8, 2)");
                w.WriteElementString("li", "(1, 0)");
                w.WriteEndElement();
                w.WriteEndElement();

                w.WriteStartElement("angularVelocityPropeller");
                w.WriteAttributeString("Class", "SmashTools.BezierCurve");
                w.WriteStartElement("points");
                w.WriteElementString("li", "0, 0");
                w.WriteElementString("li", "(0.3, 0)");
                w.WriteElementString("li", $"(0.5, {cfg.angularVelocityPropeller * 0.5f:F0})");
                w.WriteElementString("li", $"(1, {cfg.angularVelocityPropeller:F0})");
                w.WriteEndElement();
                w.WriteEndElement();

                w.WriteStartElement("zPositionVerticalCurve");
                w.WriteAttributeString("Class", "SmashTools.BezierCurve");
                w.WriteStartElement("points");
                w.WriteElementString("li", "(0, 0)");
                w.WriteElementString("li", "(0.15, 0)");
                w.WriteElementString("li", $"(0.5, {cfg.hoverAltitude * 0.75f:F1})");
                w.WriteElementString("li", $"(0.85, {cfg.hoverAltitude:F1})");
                w.WriteElementString("li", $"(1, {cfg.hoverAltitude:F1})");
                w.WriteEndElement();
                w.WriteEndElement();

                w.WriteStartElement("shadowAlphaPropellerCurve");
                w.WriteAttributeString("Class", "SmashTools.BezierCurve");
                w.WriteStartElement("points");
                w.WriteElementString("li", "(0, 1)");
                w.WriteElementString("li", "(0.3, 0.8)");
                w.WriteElementString("li", "(0.7, 0.5)");
                w.WriteElementString("li", "(1, 0.3)");
                w.WriteEndElement();
                w.WriteEndElement();

                w.WriteStartElement("fleckDataVertical");
                w.WriteElementString("def", "DustPuff");
                w.WriteStartElement("airTime");
                w.WriteStartElement("points");
                w.WriteElementString("li", "(1, 0.5)");
                w.WriteEndElement();
                w.WriteEndElement();
                w.WriteStartElement("frequency");
                w.WriteAttributeString("Class", "SmashTools.BezierCurve");
                w.WriteStartElement("points");
                w.WriteElementString("li", "(0, 160)");
                w.WriteElementString("li", "(0.25, 160)");
                w.WriteElementString("li", "(0.5, 160)");
                w.WriteElementString("li", "(0.75, 130)");
                w.WriteElementString("li", "(0.75, 0)");
                w.WriteEndElement();
                w.WriteEndElement();
                w.WriteStartElement("size");
                w.WriteAttributeString("Class", "SmashTools.BezierCurve");
                w.WriteStartElement("points");
                w.WriteElementString("li", "(0, 2.5)");
                w.WriteElementString("li", "(0.25, 1.64)");
                w.WriteElementString("li", "(0.75, 1.56)");
                w.WriteElementString("li", "(1, 1)");
                w.WriteEndElement();
                w.WriteEndElement();
                w.WriteStartElement("speed");
                w.WriteStartElement("points");
                w.WriteElementString("li", "(0, 6)");
                w.WriteElementString("li", "(1, 3)");
                w.WriteEndElement();
                w.WriteEndElement();
                w.WriteEndElement();
            }

            w.WriteEndElement();
        }

        public static string GetPresetDisplayName(string filePath)
        {
            return Path.GetFileNameWithoutExtension(filePath);
        }

        public static void ApplyHoverConfigsToVehicleDefs(VRF_ModSettings settings)
        {
            if (settings.hoverConfigs.NullOrEmpty()) return;
            foreach (var hcfg in settings.hoverConfigs)
            {
                if (!hcfg.enabled) continue;
                VehicleDef vDef = DefDatabase<VehicleDef>.GetNamedSilentFail(hcfg.vehicleDefName);
                if (vDef == null) continue;
                var props = vDef.comps?.OfType<CompProperties_VehicleHover>().FirstOrDefault();
                if (props == null) continue;
                if (System.Enum.TryParse<FlightType>(hcfg.flightType, out var ft))
                    props.flightType        = ft;
                props.maxTicks              = hcfg.maxTicks;
                props.maxTicksVertical      = hcfg.maxTicksVertical;
                props.maxTicksPropeller     = hcfg.maxTicksPropeller;
                props.hoverAltitude         = hcfg.hoverAltitude;
                props.hoverShadowOffset     = hcfg.hoverShadowOffset;
                props.hoverBobAmount        = hcfg.hoverBobAmount;
                props.hoverBobSpeed         = hcfg.hoverBobSpeed;
                props.hoverMoveSpeed        = hcfg.hoverMoveSpeed;
                props.hoverRotationSpeed    = hcfg.hoverRotationSpeed;

                if (hcfg.baseMoveSpeed > 0f)
                {
                    if (!VehicleMod.settings.vehicles.vehicleStats.ContainsKey(vDef.defName))
                        VehicleMod.settings.vehicles.vehicleStats[vDef.defName] = new System.Collections.Generic.Dictionary<string, float>();
                    VehicleMod.settings.vehicles.vehicleStats[vDef.defName][VehicleStatDefOf.MoveSpeed.defName] = hcfg.baseMoveSpeed;
                    VehicleStatDefOf.MoveSpeed.Worker.ClearCachedBaseValues(vDef);
                    vDef.RecacheMovementPermissions();
                }
            }
        }
    }
}