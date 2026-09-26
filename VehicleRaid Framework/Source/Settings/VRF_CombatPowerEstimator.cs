using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using RimWorld;
using Vehicles;

namespace VehicleRaidFramework
{
    public static class VRF_CombatPowerEstimator
    {
        private static float WeightHP        => VRF_Mod.Settings.WeightHP;
        private static float WeightArmor     => VRF_Mod.Settings.WeightArmor;
        private static float WeightDPS       => VRF_Mod.Settings.WeightDPS;
        private static float WeightRange     => VRF_Mod.Settings.WeightRange;
        private static float WeightSpeed     => VRF_Mod.Settings.WeightSpeed;
        private static float WeightDrivers   => VRF_Mod.Settings.WeightDrivers;
        private static float WeightGunners   => VRF_Mod.Settings.WeightGunners;
        private static float WeightPassengers => VRF_Mod.Settings.WeightPassengers;
        private static float WeightSize      => VRF_Mod.Settings.WeightSize;
        private static float AirMultiplier   => VRF_Mod.Settings.AirMultiplier;

        public static float Estimate(PawnKindDef kind)
        {
            VehicleDef vDef = kind.race as VehicleDef;
            if (vDef == null)
                return Mathf.Max(kind.combatPower, 50f);

            float score = 0f;

            score += ScoreHP(vDef);
            score += ScoreArmor(vDef);
            score += ScoreTurrets(vDef);
            score += ScoreSpeed(vDef);
            score += ScoreCrew(vDef);
            score += ScoreSize(vDef);

            if (vDef.type == VehicleType.Air)
                score *= AirMultiplier;

            score += ScoreLongRangeBonus(vDef);

            score = Mathf.Max(score, 50f);
            return Mathf.Round(score / 25f) * 25f;
        }

        private static float GetMaxTurretRange(VehicleDef vDef)
        {
            var turretProps = vDef.CompPropsVehicleTurrets;
            if (turretProps == null || turretProps.turrets.NullOrEmpty()) return 0f;
            float max = 0f;
            foreach (var turret in turretProps.turrets)
            {
                if (turret?.def == null || turret.parentKey != null) continue;
                float r = turret.def.maxRange > 0f ? turret.def.maxRange : 0f;

                if (r <= 0f)
                {
                    ThingDef proj = turret.def.projectile;
                    if (proj == null && turret.def.ammunition != null)
                        proj = turret.def.ammunition.AllowedThingDefs
                            .FirstOrDefault(d => d.projectile != null || d.projectileWhenLoaded != null);

                    bool isOverhead = proj?.projectile?.flyOverhead == true
                                   || proj?.projectileWhenLoaded?.projectile?.flyOverhead == true;
                    if (isOverhead)
                        r = 999f;
                }

                if (r > max) max = r;
            }
            return max;
        }

        private static float ScoreLongRangeBonus(VehicleDef vDef)
        {
            float maxRange = GetMaxTurretRange(vDef);
            if (maxRange > 250f) return 1100f;
            if (maxRange > 200f) return 800f;
            if (maxRange > 150f) return 500f;
            return 0f;
        }

        private static float ScoreHP(VehicleDef vDef)
        {
            if (vDef.components.NullOrEmpty()) return 0f;
            float total = 0f;
            foreach (var comp in vDef.components)
                total += comp.health;
            return total * WeightHP;
        }

        private static float ScoreArmor(VehicleDef vDef)
        {
            if (vDef.components.NullOrEmpty()) return 0f;
            float totalSharp  = 0f;
            float totalBlunt  = 0f;
            int   count       = 0;

            foreach (var comp in vDef.components)
            {
                if (comp.armor.NullOrEmpty()) continue;
                count++;
                foreach (var mod in comp.armor)
                {
                    if (mod.stat == StatDefOf.ArmorRating_Sharp) totalSharp += mod.value;
                    if (mod.stat == StatDefOf.ArmorRating_Blunt) totalBlunt += mod.value;
                }
            }

            if (count == 0) return 0f;
            float avgSharp = totalSharp / count;
            float avgBlunt = totalBlunt / count;
            return (avgSharp + avgBlunt * 0.6f) * WeightArmor;
        }

        private static float ScoreTurrets(VehicleDef vDef)
        {
            var turretProps = vDef.CompPropsVehicleTurrets;
            if (turretProps == null || turretProps.turrets.NullOrEmpty()) return 0f;

            float total = 0f;
            foreach (var turret in turretProps.turrets)
            {
                if (turret?.def == null) continue;
                if (turret.parentKey != null) continue;

                total += ScoreSingleTurret(turret.def);
            }
            return total;
        }

        private static float ScoreSingleTurret(VehicleTurretDef tDef)
        {
            float dps   = EstimateDPS(tDef);
            float range = tDef.maxRange > 0f ? tDef.maxRange : 25f;

            float rangeMod = Mathf.Clamp(range / 20f, 0.5f, 3f);
            return dps * WeightDPS + range * WeightRange * rangeMod;
        }

        private static float EstimateDPS(VehicleTurretDef tDef)
        {
            if (tDef.fireModes.NullOrEmpty()) return 0f;

            float bestDps = 0f;
            foreach (var fm in tDef.fireModes)
            {
                float dmg = GetProjectileDamage(tDef);
                if (dmg <= 0f) continue;

                int   shotsPerBurst  = fm.shotsPerBurst.max > 0 ? fm.shotsPerBurst.max : 1;
                float burstDamage    = dmg * shotsPerBurst;

                float cycleTime = tDef.warmUpTimer + tDef.reloadTimer;
                if (cycleTime <= 0f) cycleTime = 1f;

                float dps = burstDamage / cycleTime;
                if (dps > bestDps) bestDps = dps;
            }
            return bestDps;
        }

        private static float GetProjectileDamage(VehicleTurretDef tDef)
        {
            ThingDef proj = tDef.projectile;

            if (proj == null && tDef.ammunition != null)
            {
                proj = tDef.ammunition.AllowedThingDefs
                    .FirstOrDefault(d => d.projectile != null || d.projectileWhenLoaded != null);
            }

            if (proj == null) return 10f;

            if (proj.projectile != null)
                return proj.projectile.GetDamageAmount(null);

            if (proj.projectileWhenLoaded != null)
                return proj.projectileWhenLoaded.projectile?.GetDamageAmount(null) ?? 10f;

            return 10f;
        }

        private static float ScoreSpeed(VehicleDef vDef)
        {
            if (vDef.type == VehicleType.Air)
            {
                float flightSpeed = vDef.GetStatValueAbstract(VehicleStatDefOf.FlightSpeed);
                return flightSpeed * WeightSpeed;
            }
            float speed = vDef.GetStatValueAbstract(VehicleStatDefOf.MoveSpeed);
            return speed * WeightSpeed;
        }

        private static float ScoreCrew(VehicleDef vDef)
        {
            if (vDef.properties?.roles.NullOrEmpty() ?? true) return 0f;

            float score = 0f;
            foreach (var role in vDef.properties.roles)
            {
                bool isDriver  = (role.HandlingTypes & HandlingType.Movement) != 0;
                bool isGunner  = (role.HandlingTypes & HandlingType.Turret)   != 0;
                bool isPassenger = role.HandlingTypes == HandlingType.None;

                int slots = role.Slots;
                if (isDriver)  score += slots * WeightDrivers;
                else if (isGunner) score += slots * WeightGunners;
                else if (isPassenger) score += slots * WeightPassengers;
            }
            return score;
        }

        private static float ScoreSize(VehicleDef vDef)
        {
            int cells = vDef.Size.x * vDef.Size.z;
            return cells * WeightSize;
        }

        public static VRF_UpgradeLoadout BuildAutoLoadout(VehicleDef vDef)
        {
            var allNodes = vDef.CompPropsUpgradeTree?.def?.nodes;
            if (allNodes == null || allNodes.Count == 0)
                return null;

            var loadout      = new VRF_UpgradeLoadout();
            var selectedKeys = new HashSet<string>();

            var turretNodes = allNodes
                .Where(n => n.upgrades != null && n.upgrades.OfType<TurretUpgrade>().Any(tu => !tu.turrets.NullOrEmpty()))
                .ToList();

            var rivalGroups = BuildRivalGroups(turretNodes, allNodes);

            foreach (var group in rivalGroups)
            {
                var best = group
                    .OrderByDescending(n => n.upgrades.OfType<TurretUpgrade>().Sum(tu => tu.turrets?.Count ?? 0))
                    .ThenBy(_ => Rand.Value)
                    .First();

                if (ArePrereqsMet(best, selectedKeys, allNodes))
                {
                    selectedKeys.Add(best.key);
                    loadout.nodeKeys.Add(best.key);
                }
            }

            var nonTurretNodes = allNodes
                .Where(n => !turretNodes.Contains(n))
                .OrderBy(_ => Rand.Value)
                .ToList();

            foreach (var node in nonTurretNodes)
            {
                if (selectedKeys.Contains(node.key)) continue;
                if (!ArePrereqsMet(node, selectedKeys, allNodes)) continue;
                if (IsDisabledBy(node, selectedKeys)) continue;

                if (Rand.Bool)
                {
                    selectedKeys.Add(node.key);
                    loadout.nodeKeys.Add(node.key);
                }
            }

            return loadout.nodeKeys.Count > 0 ? loadout : null;
        }

        private static List<List<UpgradeNode>> BuildRivalGroups(
            List<UpgradeNode> turretNodes,
            List<UpgradeNode> allNodes)
        {
            var turretKeyToNodes = new Dictionary<string, List<UpgradeNode>>();
            foreach (var node in turretNodes)
            {
                foreach (var tu in node.upgrades.OfType<TurretUpgrade>())
                {
                    if (tu.turrets.NullOrEmpty()) continue;
                    foreach (var turret in tu.turrets)
                    {
                        if (!turretKeyToNodes.ContainsKey(turret.key))
                            turretKeyToNodes[turret.key] = new List<UpgradeNode>();
                        if (!turretKeyToNodes[turret.key].Contains(node))
                            turretKeyToNodes[turret.key].Add(node);
                    }
                }
            }

            int n = turretNodes.Count;
            int[] parent = Enumerable.Range(0, n).ToArray();

            int Find(int i) { while (parent[i] != i) { parent[i] = parent[parent[i]]; i = parent[i]; } return i; }
            void Union(int a, int b) { parent[Find(a)] = Find(b); }

            foreach (var pair in turretKeyToNodes)
            {
                var rivals = pair.Value;
                for (int i = 1; i < rivals.Count; i++)
                    Union(turretNodes.IndexOf(rivals[0]), turretNodes.IndexOf(rivals[i]));
            }

            for (int i = 0; i < n; i++)
            {
                var node = turretNodes[i];
                if (!string.IsNullOrEmpty(node.disableIfUpgradeNodeEnabled))
                {
                    int j = turretNodes.FindIndex(x => x.key == node.disableIfUpgradeNodeEnabled);
                    if (j >= 0) Union(i, j);
                }
                if (!node.disableIfUpgradeNodesEnabled.NullOrEmpty())
                {
                    foreach (var k in node.disableIfUpgradeNodesEnabled)
                    {
                        int j = turretNodes.FindIndex(x => x.key == k);
                        if (j >= 0) Union(i, j);
                    }
                }
            }

            var groups = new Dictionary<int, List<UpgradeNode>>();
            for (int i = 0; i < n; i++)
            {
                int root = Find(i);
                if (!groups.ContainsKey(root)) groups[root] = new List<UpgradeNode>();
                groups[root].Add(turretNodes[i]);
            }

            return groups.Values.ToList();
        }

        private static bool ArePrereqsMet(UpgradeNode node, HashSet<string> selected, List<UpgradeNode> allNodes)
        {
            if (node.prerequisiteNodes.NullOrEmpty()) return true;
            return node.prerequisiteNodes.All(k => selected.Contains(k));
        }

        private static bool IsDisabledBy(UpgradeNode node, HashSet<string> selected)
        {
            if (!string.IsNullOrEmpty(node.disableIfUpgradeNodeEnabled) &&
                selected.Contains(node.disableIfUpgradeNodeEnabled))
                return true;
            if (!node.disableIfUpgradeNodesEnabled.NullOrEmpty() &&
                node.disableIfUpgradeNodesEnabled.Any(k => selected.Contains(k)))
                return true;
            return false;
        }

        public static string BuildBreakdown(PawnKindDef kind)
        {
            VehicleDef vDef = kind.race as VehicleDef;
            if (vDef == null)
                return "No VehicleDef.";

            float hp        = ScoreHP(vDef);
            float armor     = ScoreArmor(vDef);
            float turrets   = ScoreTurrets(vDef);
            float speed     = ScoreSpeed(vDef);
            float crew      = ScoreCrew(vDef);
            float size      = ScoreSize(vDef);
            float raw       = hp + armor + turrets + speed + crew + size;
            float airMul    = vDef.type == VehicleType.Air ? AirMultiplier : 1f;
            float rangeBonus = ScoreLongRangeBonus(vDef);
            float maxRange  = GetMaxTurretRange(vDef);
            float total     = Mathf.Round(Mathf.Max(raw * airMul + rangeBonus, 50f) / 25f) * 25f;

            return
                $"HP:         {hp:F0}\n" +
                $"Armor:      {armor:F0}\n" +
                $"Turrets:    {turrets:F0}\n" +
                $"Speed:      {speed:F0}\n" +
                $"Crew:       {crew:F0}\n" +
                $"Size:       {size:F0}\n" +
                (vDef.type == VehicleType.Air ? $"Air x{AirMultiplier}: +{(raw * AirMultiplier - raw):F0}\n" : "") +
                (rangeBonus > 0f ? $"LongRange ({maxRange:F0}): +{rangeBonus:F0}\n" : "") +
                $"─────────\n" +
                $"Total:      {total:F0}";
        }
    }
}