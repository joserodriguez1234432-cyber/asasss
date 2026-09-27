using System;
using System.Linq;
using SmashTools;
using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;
using Vehicles;
using VehicleRaidFramework;

namespace VehicleRaid
{
    public enum FlightType
    {
        Hover,
        Airplane,
        Gravship
    }

    public enum HoverState
    {
        Grounded,
        TakingOff,
        Hovering,
        Landing,
        Crashing
    }

    public class CompVehicleHover : VehicleComp
    {
        public HoverState State = HoverState.Grounded;
        public int ticksInState = 0;
        public float currentAltitude = 0f;
        public float bobbingOffset = 0f;

        public FlightType FlightType => Props.flightType;
        public bool IsGravshipEntity => FlightType == FlightType.Gravship || (Vehicle != null && (VehicleRaidFramework.CrewManager.IsGravshipVehicle(Vehicle) || VRF_VehicleMapCompat.IsVehicleWithMap(Vehicle) || (Vehicle.def != null && Vehicle.def.defName.IndexOf("grav", System.StringComparison.OrdinalIgnoreCase) >= 0)));


        public Vector2 realPos;
        public Vector2 targetPos;
        public bool hasTarget = false;
        public float flyAngle = 0f;
        public float currentFlyAngle = 0f;
        public float moveSpeed = 0f;

        private LocalTargetInfo facingTarget = LocalTargetInfo.Invalid;
        private bool isFacingTarget = false;
        
        public LocalTargetInfo attackTarget = LocalTargetInfo.Invalid;
        public bool isAttackingTarget = false;
        private int turretUpdateTicks = 0;

        public Thing facingTargetNPC = null;
        public bool isFacingTargetNPC = false;

        private int ticksWithoutPilot = 0;
        private int ticksWithoutFuel = 0;
        private float crashStartAltitude = 0f;
        private const int CrashDelayTicks = 180;
        private const int CrashFuelDelayTicks = 240;
        private const int CrashDurationTicks = 90;

        private Vector2 takeoffOrigin;
        private Vector2 landingOrigin;
        private Vector2 landingApproachTarget;
        private bool hasLandingApproach = false;
        private int landingApproachTicks = 0;
        private const int MaxLandingApproachTicks = 900;
        private bool engineNotificationSent = false;
        private bool fuelTankNotificationSent = false;
        private Rot4 pendingLandingRot = Rot4.North;

        public CompProperties_VehicleHover Props => (CompProperties_VehicleHover)props;

        public float EffectiveHoverMoveSpeed
        {
            get
            {
                int currentTick = Find.TickManager?.TicksGame ?? -1;
                if (currentTick >= 0 && lastSpeedCheckTick == currentTick)
                    return effectiveHoverMoveSpeed;

                if (Vehicle != null && (VehicleRaidFramework.CrewManager.IsGravshipVehicle(Vehicle) || Props.flightType == FlightType.Gravship))
                {
                    float gSpeed = VehicleRaidFramework.VehicleMapFramework.VRF_GravshipSpeedUtility.CalculateGravshipSpeed(Vehicle);
                    effectiveHoverMoveSpeed = gSpeed > 0f ? gSpeed : Props.hoverMoveSpeed;
                }
                else
                {
                    effectiveHoverMoveSpeed = Props.hoverMoveSpeed;
                }

                lastSpeedCheckTick = currentTick;
                return effectiveHoverMoveSpeed;
            }
        }


        public bool IsAirborne => State == HoverState.Hovering || State == HoverState.TakingOff || State == HoverState.Landing || State == HoverState.Crashing;

        private Command_Action takeoffCommand;
        private Command_Action landCommand;
        private Command_Action faceTargetCommand;
        private Command_Action attackTargetCommand;
        private Command_Action cancelTargetCommand;
        private Command_Action jumpCommand;

        private HoverVehicleProjectile dummyProjectile;
        private CompFueledTravel fuelComp;
        private CompVehicleTurrets turretsComp;
        private bool engineBroken;
        private bool fuelTankBroken;
        private int lastDamageStateCheckTick = -1;
        private int lastPilotCheckTick = -1;
        private bool pilotAvailable;
        private int lastSpeedCheckTick = -1;
        private float effectiveHoverMoveSpeed;
        // A retreat can be requested by a Lord, the hover transport manager, or a
        // job giver.  Keep its edge cell on the vehicle so all of those callers
        // steer toward the same exit instead of competing for a new random one.
        private IntVec3 lockedNpcExitCell = IntVec3.Invalid;

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
            if (!respawningAfterLoad)
            {
                realPos = new Vector2(Vehicle.Position.x + 0.5f, Vehicle.Position.z + 0.5f);
                targetPos = realPos;
            }
            CacheVehicleComps();
            RefreshCriticalDamageState();
            HoverVehicleRegistry.Register(Vehicle);
        }

        public override void PostDeSpawn(Map map, DestroyMode mode = DestroyMode.Vanish)
        {
            HoverVehicleRegistry.Deregister(Vehicle, map);
            base.PostDeSpawn(map, mode);
        }

        public override void PostPostApplyDamage(DamageInfo dinfo, float totalDamageDealt)
        {
            base.PostPostApplyDamage(dinfo, totalDamageDealt);
            RefreshCriticalDamageState();
        }

        public bool HasPilot()
        {
            int currentTick = Find.TickManager?.TicksGame ?? -1;
            if (currentTick >= 0 && lastPilotCheckTick == currentTick)
                return pilotAvailable;

            pilotAvailable = CalculateHasPilot();
            lastPilotCheckTick = currentTick;
            return pilotAvailable;
        }

        private bool CalculateHasPilot()
        {
            if (Vehicle == null) return false;

            if (VehicleMod.settings?.debug?.debugDraftAnyVehicle == true ||
                (Vehicle.MovementPermissions & VehiclePermissions.Autonomous) != VehiclePermissions.None)
            {
                return true;
            }

            // For gravships (VehiclePawnWithMap from Vehicle Map Framework)
            // VMF converts the pilot console into a vehicle seat with role 'pilot' and HandlingType.Movement
            if (CrewManager.IsGravshipVehicle(Vehicle))
            {
                if (!Vehicle.HasEnoughOperators)
                {
                    return false;
                }

                if (Vehicle.handlers != null)
                {
                    bool hasActivePilot = false;
                    foreach (var handler in Vehicle.handlers)
                    {
                        if (handler?.role == null) continue;
                        if ((handler.role.HandlingTypes & HandlingType.Movement) != 0 || handler.role.key == "pilot")
                        {
                            if (handler.RoleFulfilled)
                            {
                                foreach (var thing in handler.thingOwner)
                                {
                                    if (thing is Pawn p && !p.Dead && !p.Downed)
                                    {
                                        hasActivePilot = true;
                                        break;
                                    }
                                }
                            }
                        }
                        if (hasActivePilot) break;
                    }
                    if (!hasActivePilot) return false;
                }
                else
                {
                    return false;
                }

                return true;
            }

            if (!Vehicle.HasEnoughOperators) return false;
            return CrewManager.HasOperationalDriver(Vehicle);
        }

        private void CacheVehicleComps()
        {
            if (Vehicle == null) return;
            fuelComp = Vehicle.GetComp<CompFueledTravel>();
            turretsComp = Vehicle.GetComp<CompVehicleTurrets>();
        }

        private void RefreshCriticalDamageState()
        {
            if (Vehicle == null || CrewManager.IsGravshipVehicle(Vehicle))
            {
                engineBroken = false;
                fuelTankBroken = false;
                return;
            }

            engineBroken = false;
            fuelTankBroken = false;
            if (Vehicle.statHandler?.components == null) return;

            foreach (VehicleComponent part in Vehicle.statHandler.components)
            {
                if (part.Health > 0 || part.props == null) continue;
                if (part.props.key == "Engine" || (part.props.tags != null && part.props.tags.Contains("engine")))
                    engineBroken = true;
                else if (part.props.key == "Chemtank" || part.props.key == "FuelTank" ||
                         (part.props.tags != null && part.props.tags.Contains("fuel_tank")))
                    fuelTankBroken = true;

                if (engineBroken && fuelTankBroken) break;
            }

            lastDamageStateCheckTick = Find.TickManager?.TicksGame ?? -1;
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look(ref State, "hoverState", HoverState.Grounded);
            Scribe_Values.Look(ref ticksInState, "ticksInState", 0);
            Scribe_Values.Look(ref currentAltitude, "currentAltitude", 0f);
            Scribe_Values.Look(ref realPos, "realPos");
            Scribe_Values.Look(ref targetPos, "targetPos");
            Scribe_Values.Look(ref hasTarget, "hasTarget");
            Scribe_Values.Look(ref flyAngle, "flyAngle");
            Scribe_Values.Look(ref currentFlyAngle, "currentFlyAngle", 0f);
            Scribe_Values.Look(ref isFacingTarget, "isFacingTarget", false);
            Scribe_TargetInfo.Look(ref facingTarget, "facingTarget");
            Scribe_Values.Look(ref isAttackingTarget, "isAttackingTarget", false);
            Scribe_TargetInfo.Look(ref attackTarget, "attackTarget");
            Scribe_Values.Look(ref turretUpdateTicks, "turretUpdateTicks", 0);
            Scribe_Values.Look(ref ticksWithoutPilot, "ticksWithoutPilot", 0);
            Scribe_Values.Look(ref ticksWithoutFuel, "ticksWithoutFuel", 0);
            Scribe_Values.Look(ref crashStartAltitude, "crashStartAltitude", 0f);
            Scribe_Values.Look(ref takeoffOrigin, "takeoffOrigin");
            Scribe_Values.Look(ref landingOrigin, "landingOrigin");
            Scribe_Values.Look(ref landingApproachTarget, "landingApproachTarget");
            Scribe_Values.Look(ref hasLandingApproach, "hasLandingApproach", false);
            Scribe_Values.Look(ref engineNotificationSent, "engineNotificationSent", false);
            Scribe_Values.Look(ref fuelTankNotificationSent, "fuelTankNotificationSent", false);
            Scribe_Values.Look(ref pendingLandingRot, "pendingLandingRot", Rot4.North);
            Scribe_Values.Look(ref lockedNpcExitCell, "lockedNpcExitCell");
            Scribe_References.Look(ref dummyProjectile, "dummyProjectile");
        }

        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            if (this.Vehicle.Faction != Faction.OfPlayer && !DebugSettings.godMode) yield break;
            if (!this.Vehicle.Spawned || this.Vehicle.Map == null) yield break;

            if (takeoffCommand == null)
            {
                takeoffCommand = new Vehicles.Command_ActionHighlighter
                {
                    defaultLabel = "VRF_HoverTakeoff".Translate(),
                    defaultDesc = "VRF_HoverTakeoffDesc".Translate(),
                    icon = ContentFinder<Texture2D>.Get("UI/Commands/LaunchShip"),
                    action = StartTakeoff,
                    mouseOver = DrawTakeoffRunway
                };
            }

            if (landCommand == null)
            {
                landCommand = new Command_Action
                {
                    defaultLabel = "VRF_HoverLand".Translate(),
                    defaultDesc = "VRF_HoverLandDesc".Translate(),
                    icon = ContentFinder<Texture2D>.Get("UI/Designators/Cancel"),
                    action = StartLanding
                };
            }

            if (faceTargetCommand == null)
            {
                faceTargetCommand = new Command_Action
                {
                    defaultLabel = "VRF_HoverFaceTarget".Translate(),
                    defaultDesc = "VRF_HoverFaceTargetDesc".Translate(),
                    icon = ContentFinder<Texture2D>.Get("UI/Commands/Attack"),
                    action = StartFaceTarget
                };
            }

            if (attackTargetCommand == null)
            {
                attackTargetCommand = new Command_Action
                {
                    defaultLabel = "VRF_HoverAttackTarget".Translate(),
                    defaultDesc = "VRF_HoverAttackTargetDesc".Translate(),
                    icon = ContentFinder<Texture2D>.Get("UI/Commands/Attack"),
                    action = StartAttackTarget,
                    hotKey = KeyBindingDefOf.Misc1
                };
            }

            if (cancelTargetCommand == null)
            {
                cancelTargetCommand = new Command_Action
                {
                    defaultLabel = "VRF_HoverCancelTarget".Translate(),
                    defaultDesc = "VRF_HoverCancelTargetDesc".Translate(),
                    icon = ContentFinder<Texture2D>.Get("UI/Designators/Cancel"),
                    action = CancelTarget
                };
            }

            if (jumpCommand == null)
            {
                jumpCommand = new Command_Action
                {
                    defaultLabel = "VRF_HoverJump".Translate(),
                    defaultDesc = "VRF_HoverJumpDesc".Translate(),
                    icon = ContentFinder<Texture2D>.Get("UI/Commands/Jump"),
                    action = delegate
                    {
                        List<FloatMenuOption> list = new List<FloatMenuOption>();
                        foreach (Pawn p in Vehicle.AllPawnsAboard)
                        {
                            Pawn pawn = p;
                            list.Add(new FloatMenuOption(pawn.LabelShort, delegate
                            {
                                Vehicle.DisembarkPawn(pawn);
                            }));
                        }
                        if (list.Count > 0)
                        {
                            Find.WindowStack.Add(new FloatMenu(list));
                        }
                    }
                };
            }

            if (State == HoverState.Grounded || State == HoverState.Landing)
            {
                takeoffCommand.Disabled = false;
                takeoffCommand.disabledReason = null;
                
                var launcher = Vehicle.GetComp<Vehicles.CompVehicleLauncher>();
                
                if (!Vehicle.Drafted)
                    takeoffCommand.Disable("VRF_HoverEngineOff".Translate());
                else if (!HasPilot() || Vehicle.PawnCountToOperateLeft > 0)
                    takeoffCommand.Disable("VF_NotEnoughToOperate".Translate());
                else if (Ext_Vehicles.IsRoofed(Vehicle.Position, Vehicle.Map))
                    takeoffCommand.Disable("CommandLaunchGroupFailUnderRoof".Translate());
                else if (FlightType == FlightType.Airplane && launcher != null && launcher.launchProtocol != null && launcher.launchProtocol.LaunchRestricted)
                    takeoffCommand.Disable(launcher.launchProtocol.FailLaunchMessage);
                
                yield return takeoffCommand;
            }
            else if (State == HoverState.Hovering || State == HoverState.TakingOff)
            {
                landCommand.Disabled = false;
                landCommand.disabledReason = null;

                if (FlightType == FlightType.Airplane)
                {
                    if (hasLandingApproach)
                    {
                        landCommand.defaultLabel = "VRF_HoverCancelLanding".Translate();
                        landCommand.defaultDesc = "VRF_HoverCancelLandingDesc".Translate();
                        landCommand.action = () => { hasLandingApproach = false; };
                    }
                    else
                    {
                        landCommand.defaultLabel = "VRF_HoverLand".Translate();
                        landCommand.defaultDesc = "VRF_HoverAirplaneLandDesc".Translate();
                        landCommand.action = StartLanding;
                    }
                }
                else
                {
                    landCommand.defaultLabel = "VRF_HoverLand".Translate();
                    landCommand.defaultDesc = "VRF_HoverLandDesc".Translate();
                    landCommand.action = StartLanding;
                    IntVec3 landPos = new IntVec3(Mathf.RoundToInt(realPos.x - 0.5f), 0, Mathf.RoundToInt(realPos.y - 0.5f));
                    if (landPos.InBounds(Vehicle.Map) && Ext_Vehicles.IsRoofed(landPos, Vehicle.Map))
                        landCommand.Disable("CommandLaunchGroupFailUnderRoof".Translate());
                }

                yield return landCommand;

                if (isAttackingTarget || isFacingTarget)
                {
                    yield return cancelTargetCommand;
                }
                else
                {
                    faceTargetCommand.Disabled = !HasPilot();
                    if (!HasPilot())
                        faceTargetCommand.disabledReason = "VRF_HoverNeedsPilot".Translate();
                    yield return faceTargetCommand;

                    attackTargetCommand.Disabled = !HasPilot();
                    if (!HasPilot())
                        attackTargetCommand.disabledReason = "VRF_HoverNeedsPilot".Translate();
                    yield return attackTargetCommand;
                }

                yield return jumpCommand;
            }
        }

        private void StartFaceTarget()
        {
            TargetingParameters parms = new TargetingParameters
            {
                canTargetPawns = true,
                canTargetBuildings = true,
                canTargetLocations = true,
                canTargetAnimals = true,
                canTargetHumans = true,
                canTargetMechs = true
            };

            Find.Targeter.BeginTargeting(parms, target =>
            {
                facingTarget = target;
                isFacingTarget = true;
            }, null, null, null, null, null, true);
        }

        private void StartAttackTarget()
        {
            TargetingParameters parms = new TargetingParameters
            {
                canTargetPawns = true,
                canTargetBuildings = true,
                canTargetLocations = false,
                canTargetAnimals = true,
                canTargetHumans = true,
                canTargetMechs = true,
                validator = (TargetInfo targ) =>
                {
                    if (!targ.HasThing) return false;
                    Thing thing = targ.Thing;
                    if (thing == Vehicle) return false;
                    if (thing.Faction == Vehicle.Faction) return false;
                    if (thing is Pawn pawn && pawn.Dead) return false;
                    return true;
                }
            };

            Find.Targeter.BeginTargeting(parms, target =>
            {
                attackTarget = target;
                isAttackingTarget = true;
                facingTarget = target;
                isFacingTarget = true;
                turretUpdateTicks = 0;
            }, null, null, null, null, null, true);
        }

        private void CancelTarget()
        {
            if (isAttackingTarget)
            {
                ClearTurretTargets();
            }
            isAttackingTarget = false;
            attackTarget = LocalTargetInfo.Invalid;
            isFacingTarget = false;
            facingTarget = LocalTargetInfo.Invalid;
            isFacingTargetNPC = false;
            facingTargetNPC = null;
            hasLandingApproach = false;
        }

        private void AssignTurretTargets()
        {
            if (!attackTarget.IsValid || !attackTarget.HasThing) return;
            
            var turretComp = Vehicle.CompVehicleTurrets;
            if (turretComp == null || turretComp.turrets == null) return;
            
            foreach (var turret in turretComp.turrets)
            {
                if (turret == null) continue;
                if (!turret.IsManned) continue;
                if (!turret.InRange(attackTarget)) continue;
                
                if (turret.targetInfo != attackTarget)
                {
                    turret.SetTarget(attackTarget);
                }
            }
        }

        private void ClearTurretTargets()
        {
            if (!attackTarget.IsValid) return;
            
            var turretComp = Vehicle.CompVehicleTurrets;
            if (turretComp == null || turretComp.turrets == null) return;
            
            foreach (var turret in turretComp.turrets)
            {
                if (turret == null) continue;
                if (turret.targetInfo == attackTarget)
                {
                    turret.SetTarget(LocalTargetInfo.Invalid);
                }
            }
        }

        public void SetTarget(Vector3 worldPos)
        {
            if (Vehicle.Map != null)
            {
                float margin = MapEdgeMargin;
                worldPos.x = Mathf.Clamp(worldPos.x, margin, Vehicle.Map.Size.x - margin);
                worldPos.z = Mathf.Clamp(worldPos.z, margin, Vehicle.Map.Size.z - margin);
            }
            targetPos = new Vector2(worldPos.x, worldPos.z);
            hasTarget = true;
        }

        /// <summary>
        /// Drives an airborne NPC hover vehicle to one stable map exit and removes it
        /// once the hover-safe edge margin is reached.  This is deliberately shared
        /// by all retreat callers: exit spots may fall back to a random cell.
        /// </summary>
        public bool TryExitMapForNPC()
        {
            if (Vehicle == null || !Vehicle.Spawned || Vehicle.Map == null || State != HoverState.Hovering)
                return false;

            isAttackingTarget = false;
            attackTarget = LocalTargetInfo.Invalid;
            isFacingTargetNPC = false;
            facingTargetNPC = null;

            if (!lockedNpcExitCell.IsValid)
            {
                if (!VehicleRaidFramework.VehicleTrafficManager.TryFindExitCell(Vehicle, out lockedNpcExitCell))
                    return false;
            }

            SetTarget(lockedNpcExitCell.ToVector3Shifted());

            Vector2 exitPos = new Vector2(lockedNpcExitCell.x + 0.5f, lockedNpcExitCell.z + 0.5f);
            float distanceToExit = Vector2.Distance(realPos, exitPos);

            // SetTarget and ClampRealPosToMap keep the vehicle's footprint inside
            // the map.  Include that mandatory margin, otherwise some vehicle sizes
            // can never satisfy the old strict edge-distance test.
            float exitThreshold = Mathf.Max(
                Mathf.Max(Vehicle.def.size.x, Vehicle.def.size.z) / 2f + 2f,
                MapEdgeMargin + 0.51f);

            if (distanceToExit > exitThreshold)
                return false;

            Rot4 edge = CellRect.WholeMap(Vehicle.Map).GetClosestEdge(Vehicle.Position);
            lockedNpcExitCell = IntVec3.Invalid;
            Vehicle.ExitMap(false, edge);
            return true;
        }

        public void ActivateHoverNPC()
        {
            if (State != HoverState.Grounded) return;

            ticksInState = Props.maxTicks;
            State = HoverState.Hovering;
            currentAltitude = (FlightType == FlightType.Airplane) ? Props.hoverAltitude : Props.hoverBobAmount;
            bobbingOffset = currentAltitude;
            realPos = new Vector2(Vehicle.Position.x + 0.5f, Vehicle.Position.z + 0.5f);
            targetPos = realPos;
            takeoffOrigin = realPos;
            if (FlightType == FlightType.Airplane)
            {
                flyAngle = Vehicle.Rotation.AsAngle;
                currentFlyAngle = flyAngle;
            }
            UpdatePropellerSpeed();
            Vehicle.jobs?.StopAll();
            SpawnDummyProjectile();
        }

        private void StartTakeoff()
        {
            if (State != HoverState.Grounded && State != HoverState.Landing) return;

            Vehicle.vehiclePather.StopDead();
            Vehicle.jobs?.StopAll();

            if (State == HoverState.Grounded)
            {
                ticksInState = 0;
                realPos = new Vector2(Vehicle.Position.x + 0.5f, Vehicle.Position.z + 0.5f);
                targetPos = realPos;
                
                if (FlightType == FlightType.Airplane)
                {
                    flyAngle = Vehicle.Rotation.AsAngle;
                    currentFlyAngle = flyAngle;
                    takeoffOrigin = realPos;
                }
            }
            else
            {
                if (FlightType == FlightType.Airplane)
                {
                    int landMaxTicks = Props.landingMaxTicks > 0 ? Props.landingMaxTicks : Props.maxTicks;
                    float tLanding = Mathf.Clamp01((float)ticksInState / landMaxTicks);
                    float tTakeoff = 1f - tLanding;
                    ticksInState = Mathf.RoundToInt(tTakeoff * Props.maxTicks);
                    takeoffOrigin = realPos - new Vector2(
                        Mathf.Sin(flyAngle * Mathf.Deg2Rad),
                        Mathf.Cos(flyAngle * Mathf.Deg2Rad)) * (Props.xPositionCurve != null ? Props.xPositionCurve.Evaluate(tTakeoff) : 0f);
                }
                else
                    ticksInState = Mathf.Max(0, Props.maxTicks - ticksInState);
            }

            State = HoverState.TakingOff;
            UpdatePropellerSpeed();
            SpawnDummyProjectile();
        }

        private void DrawTakeoffRunway()
        {
            if (!Vehicle.Spawned || FlightType != FlightType.Airplane) return;
            var launcher = Vehicle.GetComp<Vehicles.CompVehicleLauncher>();
            if (launcher != null && launcher.launchProtocol != null && launcher.launchProtocol.LaunchProperties.restriction != null)
            {
                launcher.launchProtocol.LaunchProperties.restriction.DrawRestrictionsTargeter(Vehicle, Vehicle.Map, Vehicle.Position, Vehicle.Rotation);
            }
        }

        private void StartLanding()
        {
            if (State != HoverState.Hovering && State != HoverState.TakingOff) return;

            if (FlightType == FlightType.Airplane)
            {
                StartLandingTargeting();
                return;
            }

            CancelTarget();
            hasTarget = false;
            moveSpeed = 0f;

            SnapRotationToCardinal();

            if (State == HoverState.Hovering)
                ticksInState = 0;
            else
                ticksInState = Mathf.Max(0, Props.maxTicks - ticksInState);

            State = HoverState.Landing;
        }

        private void StartLandingTargeting()
        {
            var launcher = Vehicle.GetComp<Vehicles.CompVehicleLauncher>();
            LandingTargeter.Instance.BeginTargeting(
                Vehicle,
                Vehicle.Map,
                (target, rot) => ExecuteAirplaneLanding(target.Cell, rot),
                null,
                null,
                null,
                null,
                true,
                false
            );
        }

        private void ExecuteAirplaneLanding(IntVec3 landCell, Rot4 rot)
        {
            Vehicle.vehiclePather.StopDead();
            Vehicle.jobs?.StopAll();
            CancelTarget();
            hasTarget = false;
            moveSpeed = 0f;

            flyAngle = rot.AsAngle;
            currentFlyAngle = flyAngle;

            landingOrigin = new Vector2(landCell.x + 0.5f, landCell.z + 0.5f);

            float initialForward = Props.landingForwardCurve != null ? Props.landingForwardCurve.Evaluate(0f) : 0f;
            float rad = flyAngle * Mathf.Deg2Rad;
            Vector2 forward = new Vector2(Mathf.Sin(rad), Mathf.Cos(rad));
            landingApproachTarget = landingOrigin - forward * initialForward;

            pendingLandingRot = rot;
            hasLandingApproach = true;
            landingApproachTicks = 0;

            ApplyRotationFromAngle(flyAngle);
        }

        private void SnapRotationToCardinal()
        {
            float a = ((currentFlyAngle % 360f) + 360f) % 360f;

            Rot4 rot4;
            if (a >= 315f || a < 45f)
                rot4 = Rot4.North;
            else if (a >= 45f && a < 135f)
                rot4 = Rot4.East;
            else if (a >= 135f && a < 225f)
                rot4 = Rot4.South;
            else
                rot4 = Rot4.West;

            flyAngle = currentFlyAngle;
            Vehicle.Angle = 0f;
            Vehicle.Transform.rotation = 0f;
            Vehicle.FullRotation = (Rot8)rot4;
            Vehicle.Rotation = rot4;
        }

        public float lastTransformRotation = 0f;

        public override void CompTick()
        {
            base.CompTick();

            if (!Vehicle.Spawned || Vehicle.Map == null) return;

            // Damage callbacks keep this current immediately; the interval also catches repairs
            // performed by other mods without repeatedly scanning every vehicle component.
            if (lastDamageStateCheckTick < 0 || Vehicle.IsHashIntervalTick(120))
                RefreshCriticalDamageState();

            float currentRot = Vehicle.Transform.rotation;
            float deltaRot = currentRot - lastTransformRotation;
            lastTransformRotation = currentRot;

            if (deltaRot != 0f && IsAirborne)
            {
                if (turretsComp == null) CacheVehicleComps();
                if (turretsComp != null && turretsComp.turrets != null)
                {
                    foreach (var turret in turretsComp.turrets)
                    {
                        if (!turret.TargetLocked)
                        {
                            turret.TurretRotationTargeted += deltaRot;
                            turret.TurretRotation += deltaRot;
                        }
                    }
                }
            }

            if (IsAirborne)
            {
                ClampRealPosToMap();
                if (dummyProjectile == null || !dummyProjectile.Spawned)
                {
                    if (State != HoverState.Crashing)
                        SpawnDummyProjectile();
                }
            }

            if (State == HoverState.Grounded)
            {
                if (Vehicle.IsHashIntervalTick(250))
                {
                    if (!engineBroken) engineNotificationSent = false;
                    if (!fuelTankBroken) fuelTankNotificationSent = false;
                }

                if (Vehicle.Faction != null &&
                    !Vehicle.Faction.IsPlayer &&
                    !CrewManager.IsGravshipVehicle(Vehicle) &&
                    Vehicle.IsHashIntervalTick(180))
                {
                    if (Patch_VehicleNPCOnOff.ShouldVehicleBeOn(Vehicle))
                        ActivateHoverNPC();
                }
            }

            if (IsAirborne && State != HoverState.Crashing)
            {
                // Gravships manage their own flight systems — no crash-on-damage or crash-on-fuel-loss
                bool isGravship = CrewManager.IsGravshipVehicle(Vehicle);

                if (engineBroken && !engineNotificationSent && Vehicle.Faction == Faction.OfPlayer)
                {
                    Messages.Message("VRF_HoverEngineBroken".Translate(Vehicle.LabelShort), Vehicle, MessageTypeDefOf.NegativeEvent);
                    engineNotificationSent = true;
                }
                if (fuelTankBroken && !fuelTankNotificationSent && Vehicle.Faction == Faction.OfPlayer)
                {
                    Messages.Message("VRF_HoverFuelTankBroken".Translate(Vehicle.LabelShort), Vehicle, MessageTypeDefOf.NegativeEvent);
                    fuelTankNotificationSent = true;
                }

                if (!isGravship && (!Vehicle.HasEnoughOperators || engineBroken))
                {
                    ticksWithoutPilot++;
                    if (ticksWithoutPilot % 30 == 0 && Vehicle.Spawned)
                        FleckMaker.ThrowSmoke(new Vector3(realPos.x, 0f, realPos.y), Vehicle.Map, 1f);
                    if (ticksWithoutPilot >= CrashDelayTicks)
                    {
                        StartCrashing();
                        return;
                    }
                }
                else
                {
                    ticksWithoutPilot = 0;
                }

                if (fuelComp == null) CacheVehicleComps();
                if (!isGravship && fuelComp != null && fuelComp.Fuel <= 0)
                {
                    ticksWithoutFuel++;
                    
                    if (ticksWithoutFuel == 1)
                    {
                        TryEmergencyRefuel();
                    }

                    if (ticksWithoutFuel % 30 == 0 && Vehicle.Spawned)
                        FleckMaker.ThrowSmoke(new Vector3(realPos.x, 0f, realPos.y), Vehicle.Map, 1f);
                    if (ticksWithoutFuel >= CrashFuelDelayTicks)
                    {
                        StartCrashing();
                        return;
                    }
                }
                else
                {
                    ticksWithoutFuel = 0;
                }
            }

            if (State == HoverState.TakingOff)
            {
                ticksInState++;
                UpdateAltitude();
                TickMotes();
                if (IsGravshipEntity) TickGravshipThrusters();

                if (ticksInState >= Props.maxTicks)
                {
                    ticksInState = Props.maxTicks;
                    State = HoverState.Hovering;
                }
                UpdatePropellerSpeed();
            }
            else if (State == HoverState.Landing)
            {
                ticksInState++;
                UpdateAltitude();
                TickMotes();
                UpdatePropellerSpeed();
                if (IsGravshipEntity) TickGravshipThrusters();

                int landMaxTicks = (FlightType == FlightType.Airplane && Props.landingMaxTicks > 0)
                    ? Props.landingMaxTicks
                    : Props.maxTicks;

                if (ticksInState >= landMaxTicks)
                {
                    ticksInState = 0;
                    currentAltitude = 0f;
                    State = HoverState.Grounded;
                    DespawnDummyProjectile();
                    Vehicle.Angle = 0f;
                    Vehicle.Transform.rotation = 0f;
                    SnapPositionToGrid();
                    realPos = new Vector2(Vehicle.Position.x + 0.5f, Vehicle.Position.z + 0.5f);
                    Vehicle.Notify_Teleported(false, true);
                    UpdatePropellerSpeed();
                }
            }
            else if (State == HoverState.Hovering)
            {
                if (!HasPilot())
                {
                    if (isFacingTarget || isAttackingTarget || isFacingTargetNPC)
                    {
                        CancelTarget();
                        isFacingTargetNPC = false;
                        facingTargetNPC = null;
                    }
                    hasTarget = false;
                    moveSpeed = 0f;
                }

                if (FlightType == FlightType.Hover || FlightType == FlightType.Gravship)
                {
                    bobbingOffset = Props.hoverBobAmount * Mathf.Sin(Find.TickManager.TicksGame * Props.hoverBobSpeed * Mathf.PI / 60f);
                    currentAltitude = bobbingOffset;
                }
                else
                {
                    currentAltitude = Props.hoverAltitude;
                }

                UpdatePropellerSpeed();
                TickFacingTarget();
                TickHoverMovement();
                TickAttackTarget();
                if (IsGravshipEntity) TickGravshipThrusters();
            }
            else if (State == HoverState.Crashing)
            {
                ticksInState++;
                float t = Mathf.Clamp01((float)ticksInState / CrashDurationTicks);
                Vehicle.Angle = Mathf.Lerp(0f, 45f, t);

                float speed = EffectiveHoverMoveSpeed * 0.4f / 60f;
                float rad = currentFlyAngle * Mathf.Deg2Rad;
                realPos += new Vector2(Mathf.Sin(rad), Mathf.Cos(rad)) * speed * (1f - t * 0.5f);
                ClampRealPosToMap();
                UpdateGridPosition();

                currentAltitude = Mathf.Lerp(crashStartAltitude, -0.3f, t);

                if (ticksInState % 4 == 0 && Vehicle.Spawned)
                {
                    FleckMaker.ThrowSmoke(new Vector3(realPos.x, 0f, realPos.y), Vehicle.Map, 2f);
                    if (t > 0.5f)
                        FleckMaker.ThrowMicroSparks(new Vector3(realPos.x, 0f, realPos.y), Vehicle.Map);
                }

                if (ticksInState >= CrashDurationTicks)
                    CrashImpact();
            }

            if (IsAirborne && Vehicle.Spawned && Vehicle.Map != null)
            {
                ClampRealPosToMap();
                IntVec3 pos = Vehicle.Position;
                int margin = Mathf.CeilToInt(MapEdgeMargin);
                if (pos.x < margin || pos.z < margin || pos.x >= Vehicle.Map.Size.x - margin || pos.z >= Vehicle.Map.Size.z - margin)
                {
                    int sx = Mathf.Clamp(pos.x, margin, Vehicle.Map.Size.x - margin - 1);
                    int sz = Mathf.Clamp(pos.z, margin, Vehicle.Map.Size.z - margin - 1);
                    Vehicle.Position = new IntVec3(sx, 0, sz);
                    realPos = new Vector2(sx + 0.5f, sz + 0.5f);
                }
            }
        }

        private void TickAttackTarget()
        {
            if (!isAttackingTarget) return;

            if (attackTarget.HasThing && (attackTarget.Thing == null || attackTarget.Thing.Destroyed || !attackTarget.Thing.Spawned))
            {
                CancelTarget();
                return;
            }

            if (FlightType == FlightType.Airplane && attackTarget.HasThing)
            {
                Thing enemy = attackTarget.Thing;

                if (Vehicle.Map != null)
                {
                    RoofDef enemyRoof = Vehicle.Map.roofGrid.RoofAt(enemy.Position);
                    if (enemyRoof != null && HoverRoofUtil.IsBlockingRoof(enemyRoof))
                        return;
                }

                if (Vehicle.IsHashIntervalTick(15))
                {
                    bool readyToFire = HoverNPC_AirplaneCombatPlanner.IsReadyToFire(Vehicle, this, enemy);

                    if (!readyToFire)
                    {
                        Vector3 engagePoint = HoverNPC_AirplaneCombatPlanner.CalcEngagementPoint(Vehicle, this, enemy);
                        if (engagePoint != Vector3.zero)
                            SetTarget(engagePoint);
                    }
                    else
                    {
                        AssignTurretTargets();
                    }
                }
                return;
            }

            if (!Vehicle.IsHashIntervalTick(15)) return;

            AssignTurretTargets();
        }

        private void TickFacingTarget()
        {
            if (!HasPilot())
            {
                if (isFacingTarget || isAttackingTarget || isFacingTargetNPC)
                {
                    CancelTarget();
                    isFacingTargetNPC = false;
                    facingTargetNPC = null;
                }
                return;
            }

            if (isFacingTargetNPC)
            {
                if (facingTargetNPC == null || facingTargetNPC.Destroyed || !facingTargetNPC.Spawned)
                {
                    isFacingTargetNPC = false;
                    facingTargetNPC = null;
                }
                else
                {
                    Vector3 targetPos3 = facingTargetNPC.DrawPos;
                    float npcDx = targetPos3.x - realPos.x;
                    float npcDz = targetPos3.z - realPos.y;
                    if (Mathf.Abs(npcDx) >= 0.1f || Mathf.Abs(npcDz) >= 0.1f)
                    {
                        flyAngle = Mathf.Atan2(npcDx, npcDz) * Mathf.Rad2Deg;
                        RotateTowardsAngle(flyAngle);
                    }
                }
                return;
            }

            if (!isFacingTarget) return;

            Vector3 targetWorldPos;

            if (facingTarget.HasThing)
            {
                if (facingTarget.Thing == null || facingTarget.Thing.Destroyed || !facingTarget.Thing.Spawned)
                {
                    isFacingTarget = false;
                    facingTarget = LocalTargetInfo.Invalid;
                    if (isAttackingTarget)
                    {
                        CancelTarget();
                    }
                    return;
                }
                targetWorldPos = facingTarget.Thing.DrawPos;
            }
            else
            {
                targetWorldPos = facingTarget.Cell.ToVector3Shifted();
            }

            float dx = targetWorldPos.x - realPos.x;
            float dz = targetWorldPos.z - realPos.y;

            if (Mathf.Abs(dx) < 0.1f && Mathf.Abs(dz) < 0.1f) return;

            float angle = Mathf.Atan2(dx, dz) * Mathf.Rad2Deg;
            flyAngle = angle;
            RotateTowardsAngle(flyAngle);
        }

        private void TickHoverMovement()
        {
            if (!HasPilot())
            {
                hasTarget = false;
                moveSpeed = 0f;
                return;
            }

            float speed = EffectiveHoverMoveSpeed / 60f;
            moveSpeed = speed;

            if (FlightType == FlightType.Airplane)
            {
                if (hasLandingApproach)
                {
                    landingApproachTicks++;
                    if (landingApproachTicks >= MaxLandingApproachTicks)
                    {
                        hasLandingApproach = false;
                        landingApproachTicks = 0;
                        if (Vehicle.Faction == Faction.OfPlayer)
                        {
                            Messages.Message("VRF_HoverLandingFailed".Translate(Vehicle.LabelShort), Vehicle, MessageTypeDefOf.RejectInput);
                        }
                    }
                    else
                    {
                    Vector2 diff = landingApproachTarget - realPos;
                    float approachDist = diff.magnitude;

                    float radPending = pendingLandingRot.AsAngle * Mathf.Deg2Rad;
                    Vector2 runwayForward = new Vector2(Mathf.Sin(radPending), Mathf.Cos(radPending));
                    float longDist = Vector2.Dot(diff, runwayForward);
                    Vector2 runwayRight = new Vector2(runwayForward.y, -runwayForward.x);
                    float latDist = Vector2.Dot(diff, runwayRight);

                    float angleDiffApproach = Mathf.Abs(Mathf.DeltaAngle(currentFlyAngle, pendingLandingRot.AsAngle));
                    bool isAligned = Mathf.Abs(latDist) < 3f && angleDiffApproach < 30f;

                    if (isAligned)
                    {
                        float approachAngle = Mathf.Atan2(diff.x, diff.y) * Mathf.Rad2Deg;
                        RotateTowardsAngle(approachAngle);
                    }
                    else
                    {
                        float pushBack = Mathf.Max(Mathf.Abs(latDist) * 2.5f, 5f);
                        if (longDist < pushBack)
                        {
                            pushBack += (pushBack - longDist) + 20f;
                        }

                        Vector2 dynamicWaypoint = landingApproachTarget - runwayForward * pushBack;
                        Vector2 steerDiff = dynamicWaypoint - realPos;
                        float approachAngle = Mathf.Atan2(steerDiff.x, steerDiff.y) * Mathf.Rad2Deg;
                        RotateTowardsAngle(approachAngle);
                    }

                    float rad = currentFlyAngle * Mathf.Deg2Rad;
                    realPos += new Vector2(Mathf.Sin(rad), Mathf.Cos(rad)) * speed;
                    ClampRealPosToMap();
                    UpdateGridPosition();

                    float angleDiff = Mathf.Abs(Mathf.DeltaAngle(currentFlyAngle, pendingLandingRot.AsAngle));
                    if (approachDist < speed * 3f && angleDiff < 20f)
                    {
                        hasLandingApproach = false;
                        realPos = landingApproachTarget;

                        flyAngle = pendingLandingRot.AsAngle;
                        currentFlyAngle = flyAngle;
                        Vehicle.Angle = 0f;
                        Vehicle.Transform.rotation = 0f;
                        Vehicle.FullRotation = (Rot8)pendingLandingRot;
                        Vehicle.Rotation = pendingLandingRot;

                        ticksInState = 0;
                        State = HoverState.Landing;
                    }
                    }
                    return;
                }

                if (hasTarget)
                {
                    Vector2 diff = targetPos - realPos;
                    if (diff.magnitude > 0.5f)
                    {
                        float targetAngle = Mathf.Atan2(diff.x, diff.y) * Mathf.Rad2Deg;
                        RotateTowardsAngle(targetAngle);
                    }
                    else
                    {
                        hasTarget = false;
                    }
                }

                float rad2 = currentFlyAngle * Mathf.Deg2Rad;
                realPos += new Vector2(Mathf.Sin(rad2), Mathf.Cos(rad2)) * speed;
                ClampRealPosToMap();
                UpdateGridPosition();
                return;
            }

            if (!hasTarget) return;

            Vector2 hoverDiff = targetPos - realPos;
            float dist = hoverDiff.magnitude;

            if (dist < 0.1f)
            {
                hasTarget = false;
                moveSpeed = 0f;
                SnapPositionToGrid();
                return;
            }

            if (!isFacingTarget)
            {
                flyAngle = Mathf.Atan2(hoverDiff.x, hoverDiff.y) * Mathf.Rad2Deg;
                RotateTowardsAngle(flyAngle);
            }

            if (dist <= speed)
            {
                realPos = targetPos;
                ClampRealPosToMap();
                hasTarget = false;
                moveSpeed = 0f;
                SnapPositionToGrid();
            }
            else
            {
                Vector2 dir = hoverDiff.normalized;
                realPos += dir * speed;
                ClampRealPosToMap();
                UpdateGridPosition();
            }
        }

        private void RotateTowardsAngle(float targetAngle)
        {
            float maxDelta = Props.hoverRotationSpeed / 60f;
            currentFlyAngle = MoveTowardsAngle(currentFlyAngle, targetAngle, maxDelta);
            ApplyRotationFromAngle(currentFlyAngle);
        }

        private static float MoveTowardsAngle(float current, float target, float maxDelta)
        {
            float delta = Mathf.DeltaAngle(current, target);
            if (Mathf.Abs(delta) <= maxDelta)
                return target;
            return current + Mathf.Sign(delta) * maxDelta;
        }

        private void ApplyRotationFromAngle(float angle)
        {
            float a = ((angle % 360f) + 360f) % 360f;

            Rot4 cardinal;
            float visualOffset;

            if (a >= 315f || a < 45f)
            {
                cardinal = Rot4.North;
                visualOffset = a < 180f ? a : a - 360f;
            }
            else if (a >= 45f && a < 135f)
            {
                cardinal = Rot4.East;
                visualOffset = a - 90f;
            }
            else if (a >= 135f && a < 225f)
            {
                cardinal = Rot4.South;
                visualOffset = a - 180f;
            }
            else
            {
                cardinal = Rot4.West;
                visualOffset = a - 270f;
            }

            // Gravships y vehiculos normales usan rotacion estandar (visualOffset directo sin invertir).

            Rot8 rot8 = (Rot8)cardinal;
            if (Vehicle.FullRotation != rot8)
            {
                Vehicle.FullRotation = rot8;
                Vehicle.Rotation = cardinal;
            }

            Vehicle.Angle = 0f;
            Vehicle.Transform.rotation = visualOffset;
        }

        private void SnapPositionToGrid()
        {
            if (!Vehicle.Spawned || Vehicle.Map == null) return;
            IntVec3 newPos = new IntVec3(Mathf.RoundToInt(realPos.x - 0.5f), 0, Mathf.RoundToInt(realPos.y - 0.5f));
            newPos.x = Mathf.Clamp(newPos.x, 0, Vehicle.Map.Size.x - 1);
            newPos.z = Mathf.Clamp(newPos.z, 0, Vehicle.Map.Size.z - 1);
            if (newPos != Vehicle.Position)
                Vehicle.Position = newPos;
        }

        private void UpdateGridPosition()
        {
            if (!Vehicle.Spawned || Vehicle.Map == null) return;
            ClampRealPosToMap();
            int margin = Mathf.CeilToInt(MapEdgeMargin);
            int x = Mathf.Clamp((int)realPos.x, margin, Vehicle.Map.Size.x - margin - 1);
            int z = Mathf.Clamp((int)realPos.y, margin, Vehicle.Map.Size.z - margin - 1);
            IntVec3 newPos = new IntVec3(x, 0, z);
            if (newPos != Vehicle.Position)
                Vehicle.Position = newPos;
        }

        private void UpdateAltitude()
        {
            if (FlightType == FlightType.Airplane)
            {
                UpdateAltitudeAirplane();
                return;
            }

            float t = Mathf.Clamp01((float)ticksInState / Props.maxTicks);
            if (State == HoverState.Landing)
                t = 1f - t;
            float targetBob = Props.hoverBobAmount * Mathf.Sin(Find.TickManager.TicksGame * Props.hoverBobSpeed * Mathf.PI / 60f);
            currentAltitude = Mathf.Lerp(0f, targetBob, t);
            bobbingOffset = currentAltitude;
        }

        private void UpdateAltitudeAirplane()
        {
            if (State == HoverState.TakingOff)
            {
                float t = Mathf.Clamp01((float)ticksInState / Props.maxTicks);

                float forwardDist = Props.xPositionCurve != null ? Props.xPositionCurve.Evaluate(t) : 0f;
                float altitude    = Props.zPositionCurve  != null ? Props.zPositionCurve.Evaluate(t)  : 0f;
                float pitch       = Props.rotationCurve   != null ? Props.rotationCurve.Evaluate(t)   : 0f;

                float rad = flyAngle * Mathf.Deg2Rad;
                Vector2 forward = new Vector2(Mathf.Sin(rad), Mathf.Cos(rad));
                realPos = takeoffOrigin + forward * forwardDist;
                ClampRealPosToMap();
                UpdateGridPosition();

                currentAltitude = altitude;
                bobbingOffset   = currentAltitude;

                ApplyRotationFromAngle(flyAngle);
                
                int flipSign = (Vehicle.Rotation == Rot4.West) ? -1 : 1;
                if (Vehicle.Rotation == Rot4.North || Vehicle.Rotation == Rot4.South) flipSign = 0;
                Vehicle.Transform.rotation += pitch * flipSign;
            }
            else if (State == HoverState.Landing)
            {
                int landMaxTicks = Props.landingMaxTicks > 0 ? Props.landingMaxTicks : Props.maxTicks;
                float t = Mathf.Clamp01((float)ticksInState / landMaxTicks);

                float forwardDist = Props.landingForwardCurve   != null ? Props.landingForwardCurve.Evaluate(t)   : 0f;
                float altitude    = Props.landingAltitudeCurve  != null ? Props.landingAltitudeCurve.Evaluate(t)  : 0f;
                float pitch       = Props.landingRotationCurve  != null ? Props.landingRotationCurve.Evaluate(t)  : 0f;

                float rad = flyAngle * Mathf.Deg2Rad;
                Vector2 forward = new Vector2(Mathf.Sin(rad), Mathf.Cos(rad));
                realPos = landingOrigin - forward * forwardDist;
                ClampRealPosToMap();
                UpdateGridPosition();

                currentAltitude = altitude;
                bobbingOffset   = currentAltitude;

                ApplyRotationFromAngle(flyAngle);
                
                int flipSign = (Vehicle.Rotation == Rot4.West) ? -1 : 1;
                if (Vehicle.Rotation == Rot4.North || Vehicle.Rotation == Rot4.South) flipSign = 0;
                Vehicle.Transform.rotation += pitch * flipSign;
            }
        }

        private float MapEdgeMargin
        {
            get
            {
                if (Vehicle == null || Vehicle.def == null) return 2.5f;
                int maxDim = Mathf.Max(Vehicle.def.size.x, Vehicle.def.size.z);
                return (maxDim / 2f) + 1.5f;
            }
        }

        private void ClampRealPosToMap()
        {
            if (Vehicle.Map == null) return;
            float margin = MapEdgeMargin;
            float minX = margin;
            float maxX = Vehicle.Map.Size.x - margin;
            float minZ = margin;
            float maxZ = Vehicle.Map.Size.z - margin;

            bool hitEdge = realPos.x < minX || realPos.x > maxX || realPos.y < minZ || realPos.y > maxZ;

            realPos.x = Mathf.Clamp(realPos.x, minX, maxX);
            realPos.y = Mathf.Clamp(realPos.y, minZ, maxZ);

            if (hitEdge && State == HoverState.Hovering && FlightType == FlightType.Airplane)
            {
                Vector2 center = new Vector2(Vehicle.Map.Size.x * 0.5f, Vehicle.Map.Size.z * 0.5f);
                Vector2 toCenter = (center - realPos).normalized;
                float turnAngle = Mathf.Atan2(toCenter.x, toCenter.y) * Mathf.Rad2Deg;
                flyAngle = turnAngle;
                currentFlyAngle = turnAngle;
                ApplyRotationFromAngle(currentFlyAngle);
            }
        }

        private void UpdatePropellerSpeed()
        {
            if (Props.angularVelocityPropeller == null || Vehicle.DrawTracker?.overlayRenderer == null) return;

            float t;
            if (State == HoverState.Hovering)
                t = 1f;
            else if (State == HoverState.Grounded)
                t = 0f;
            else
            {
                t = Mathf.Clamp01((float)ticksInState / Props.maxTicksPropeller);
                if (State == HoverState.Landing) t = 1f - t;
            }

            Vehicle.DrawTracker.overlayRenderer.SetAcceleration(Props.angularVelocityPropeller.Evaluate(t));
        }

        private void TickMotes()
        {
            if (!VehicleMod.settings.main.aerialVehicleEffects) return;

            float t = Mathf.Clamp01((float)ticksInState / Props.maxTicks);
            if (State == HoverState.Landing) t = 1f - t;

            if (Props.fleckDataVertical != null) TryThrowFleck(Props.fleckDataVertical, t);
            if (Props.fleckDataPropeller != null) TryThrowFleck(Props.fleckDataPropeller, t);
        }

        private void TryEmergencyRefuel()
        {
            if (!Vehicle.AllPawnsAboard.Any()) return;
            
            if (fuelComp == null) CacheVehicleComps();
            CompFueledTravel comp = fuelComp;
            if (comp == null || comp.Props.ElectricPowered) return;

            float needed = comp.FuelCapacity - comp.Fuel;
            if (needed <= 0) return;

            var fuelThings = CompFueledTravel.AllFuelFromInventory(Vehicle).ToList();
            int availableFuel = 0;
            foreach (var t in fuelThings) availableFuel += t.stackCount;

            if (availableFuel > 0)
            {
                int toConsume = Mathf.Min(availableFuel, Mathf.CeilToInt(needed));
                
                comp.Refuel((float)toConsume);
                
                int remainingToConsume = toConsume;
                foreach (Thing fuelThing in fuelThings)
                {
                    int take = Mathf.Min(remainingToConsume, fuelThing.stackCount);
                    fuelThing.SplitOff(take).Destroy(DestroyMode.Vanish);
                    remainingToConsume -= take;
                    if (remainingToConsume <= 0) break;
                }

                Patch_VehicleNPCOnOff.UpdateVehiclePower(Vehicle);
            }
        }

        public void StartCrashing()
        {
            State = HoverState.Crashing;
            ticksInState = 0;
            crashStartAltitude = currentAltitude;
            CancelTarget();
            hasTarget = false;
            moveSpeed = 0f;
            for (int i = Vehicle.AllPawnsAboard.Count - 1; i >= 0; i--)
                Vehicle.DisembarkPawn(Vehicle.AllPawnsAboard[i]);
        }

        private void CrashImpact()
        {
            if (!Vehicle.Spawned) return;

            Map map = Vehicle.Map;
            IntVec3 pos = Vehicle.Position;

            for (int i = 0; i < 5; i++)
                Vehicle.TakeDamage(new DamageInfo(DamageDefOf.Bomb, 30f));

            GenExplosion.DoExplosion(pos, map, 2.5f, DamageDefOf.Bomb, null, 15);
            GenExplosion.DoExplosion(pos, map, 1.5f, DamageDefOf.Flame, null);

            for (int i = 0; i < 8; i++)
            {
                IntVec3 cell = pos + GenRadial.RadialPattern[i];
                if (cell.InBounds(map))
                    FleckMaker.ThrowDustPuff(cell.ToVector3Shifted(), map, 2.5f);
            }

            State = HoverState.Grounded;
            currentAltitude = 0f;
            ticksInState = 0;
            ticksWithoutPilot = 0;
            ticksWithoutFuel = 0;
            Vehicle.Angle = 0f;
            Vehicle.Transform.rotation = 0f;
            Vehicle.ignition.Drafted = false;
            SnapPositionToGrid();
            realPos = new Vector2(Vehicle.Position.x + 0.5f, Vehicle.Position.z + 0.5f);
            Vehicle.Notify_Teleported(false, true);
            DespawnDummyProjectile();
        }

        private void SpawnDummyProjectile()
        {
            if (dummyProjectile != null && dummyProjectile.Spawned) return;
            if (!Vehicle.Spawned || Vehicle.Map == null) return;

            ThingDef projDef = VRF_ThingDefOf.VRF_HoverVehicleDummyProjectile;
            if (projDef == null)
            {
                Log.ErrorOnce("[VRF] VRF_HoverVehicleDummyProjectile ThingDef not found.", 0x7F3A01);
                return;
            }

            HoverVehicleProjectile proj = (HoverVehicleProjectile)ThingMaker.MakeThing(projDef);
            proj.vehicle = Vehicle;

            typeof(Verse.Projectile).GetField("launcher", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                ?.SetValue(proj, Vehicle);

            GenSpawn.Spawn(proj, Vehicle.Position, Vehicle.Map, WipeMode.Vanish);
            dummyProjectile = proj;
        }

        private void DespawnDummyProjectile()
        {
            if (dummyProjectile != null)
            {
                if (dummyProjectile.Spawned)
                    dummyProjectile.CleanDestroy();
                dummyProjectile = null;
            }
        }

        private void TryThrowFleck(FleckData fleckData, float t)
        {
            float frequency = fleckData.frequency != null ? fleckData.frequency.Evaluate(t) : 0f;
            if (frequency <= 0) return;

            float particlesToSpawn = frequency / 60f;
            int count = Mathf.FloorToInt(particlesToSpawn);
            if (Rand.Value < particlesToSpawn - count) count++;

            for (int i = 0; i < count; i++)
            {
                float size = fleckData.size != null ? fleckData.size.Evaluate(t) : 1f;
                float? airTime = fleckData.airTime?.Evaluate(t);
                float? speed = fleckData.speed?.Evaluate(t);
                float? rotationRate = fleckData.rotationRate?.Evaluate(t);
                float angle = fleckData.angle.RandomInRange;

                Vector3 pos = new Vector3(realPos.x, 0f, realPos.y);
                if (fleckData.drawOffset != null)
                    pos = pos.PointFromAngle(fleckData.drawOffset.Evaluate(t), angle);

                pos += fleckData.originOffset;

                if (fleckData.originOffsetRange != null)
                {
                    Vector3 from = fleckData.originOffsetRange.from;
                    Vector3 to = fleckData.originOffsetRange.to;
                    pos += new Vector3(Rand.Range(from.x, to.x), Rand.Range(from.y, to.y), Rand.Range(from.z, to.z));
                }
                pos.y = Altitudes.AltitudeFor(fleckData.def.altitudeLayer);

                LaunchProtocol.ThrowFleck(fleckData.def, pos, Vehicle.Map, size, airTime, angle, speed, rotationRate);
            }
        }

                private static readonly int ShaderPropertyColor2 = Shader.PropertyToID("_Color2");

        public void DrawGravshipThrusters()
        {
            if (!IsGravshipEntity) return;
            if (State == HoverState.Grounded) return;
            if (Vehicle == null || !Vehicle.Spawned || Vehicle.Map == null) return;

            // Siempre activo en modo hover, con ligera oscilacion visual natural
            float thrustFactor = UnityEngine.Random.Range(0.9f, 1.1f);

            Map interiorMap = VRF_VehicleMapCompat.GetInteriorVehicleMap(Vehicle);
            if (interiorMap != null && interiorMap.listerThings != null)
            {
                var things = interiorMap.listerThings.AllThings;
                for (int i = 0; i < things.Count; i++)
                {
                    Thing t = things[i];
                    if (t is Building b && !b.Destroyed)
                    {
                        var thrusterComp = b.TryGetComp<CompGravshipThruster>();
                        if (thrusterComp != null)
                        {
                            DrawSingleGravshipThruster(Vehicle, b, thrusterComp, thrustFactor);
                        }
                    }
                }
            }
            else
            {
                DrawFallbackGravshipFlames(thrustFactor);
            }
        }

        
        private static MaterialPropertyBlock s_ThrusterFlameBlock;
        private static readonly Dictionary<Shader, Material> s_ThrusterMaterialCache = new Dictionary<Shader, Material>();

        private static Material GetOrCreateThrusterMaterial(Shader shader)
        {
            if (shader == null) shader = ShaderTypeDefOf.MoteGlow.Shader;
            if (!s_ThrusterMaterialCache.TryGetValue(shader, out Material mat))
            {
                mat = MaterialPool.MatFrom(new MaterialRequest(shader) { renderQueue = 3201 });
                s_ThrusterMaterialCache[shader] = mat;
            }
            return mat;
        }

        private static MaterialPropertyBlock ThrusterFlameBlock
        {
            get
            {
                if (s_ThrusterFlameBlock == null)
                    s_ThrusterFlameBlock = new MaterialPropertyBlock();
                return s_ThrusterFlameBlock;
            }
        }

        private void DrawSingleGravshipThruster(VehiclePawn gravVehicle, Building b, CompGravshipThruster thrusterComp, float thrustFactor)
        {
            CompProperties_GravshipThruster props = thrusterComp.Props;
            if (props == null) return;

            float baseScale = (float)b.def.size.x * props.flameSize;
            float scale = baseScale * thrustFactor;

            Rot4 thrusterRot = b.Rotation;
            Quaternion thrusterQuat = thrusterRot.AsQuat;
            Vector3 flameOffset = Vector3.zero;
            if (props.flameOffsetsPerDirection != null && props.flameOffsetsPerDirection.Count > thrusterRot.AsInt)
            {
                flameOffset = thrusterQuat * props.flameOffsetsPerDirection[thrusterRot.AsInt];
            }

            Vector3 thrusterWorldFacing = Quaternion.AngleAxis(currentFlyAngle, Vector3.up) * thrusterRot.FacingCell.ToVector3();
            if (props.flameOffsetsPerDirection != null && props.flameOffsetsPerDirection.Count > thrusterRot.AsInt)
            {
                flameOffset = Quaternion.AngleAxis(currentFlyAngle + thrusterRot.AsAngle, Vector3.up) * props.flameOffsetsPerDirection[thrusterRot.AsInt];
            }
            // En VMF, Patch_GenThing_TrueCenter intercepta b.DrawPos y ya devuelve la coordenada exacta del mundo base.
            // NO se debe volver a proyectar con ToBaseMapCoord ni sumarle realPos/pivote, porque duplicaba la coordenada mandando el fuego a 100+ celdas.
            Vector3 worldFlamePos = b.DrawPos - thrusterWorldFacing * ((float)b.def.size.z * 0.5f + scale * 0.5f) + flameOffset;
            worldFlamePos.y = AltitudeLayer.MetaOverlays.AltitudeFor() + 0.05f;

            Shader shader = props.FlameShaderType?.Shader ?? ShaderTypeDefOf.MoteGlow.Shader;
            Material mat = GetOrCreateThrusterMaterial(shader);

            MaterialPropertyBlock block = ThrusterFlameBlock;
            block.Clear();
            Color flameColor = Color.white;
            flameColor.a = UnityEngine.Random.Range(0.85f, 1.0f);
            block.SetColor(ShaderPropertyColor2, flameColor);

            if (props.flameShaderParameters != null)
            {
                for (int i = 0; i < props.flameShaderParameters.Count; i++)
                {
                    props.flameShaderParameters[i].Apply(block);
                }
            }

            float vehicleAngle = currentFlyAngle;
            Quaternion flameRot = Quaternion.AngleAxis(vehicleAngle + thrusterRot.AsAngle, Vector3.up);

            GenDraw.DrawQuad(mat, worldFlamePos, flameRot, scale, block);
        }

        private void DrawFallbackGravshipFlames(float thrustFactor)
        {
            Vector3 forward = Quaternion.Euler(0f, currentFlyAngle, 0f) * Vector3.forward;
            Vector3 right = Quaternion.Euler(0f, currentFlyAngle, 0f) * Vector3.right;
            Vector3 shipCenter = new Vector3(realPos.x, AltitudeLayer.MetaOverlays.AltitudeFor() + 0.05f, realPos.y + currentAltitude);

            float rearOffset = Mathf.Max(1.2f, Vehicle.def.Size.z * 0.48f);
            float halfWidth = Mathf.Max(0.5f, Vehicle.def.Size.x * 0.35f);
            Vector3 rearCenter = shipCenter - forward * rearOffset;

            float speed = EffectiveHoverMoveSpeed;
            int thrusterCount = Mathf.Clamp(Mathf.RoundToInt(speed / 0.1f), 1, 6);
            float scale = Mathf.Max(2.5f, Vehicle.def.Size.x * 0.5f) * thrustFactor;

            Shader shader = ShaderTypeDefOf.MoteGlow.Shader;
            Material mat = GetOrCreateThrusterMaterial(shader);

            MaterialPropertyBlock block = ThrusterFlameBlock;
            block.Clear();
            Color flameColor = Color.white;
            flameColor.a = UnityEngine.Random.Range(0.85f, 1.0f);
            block.SetColor(ShaderPropertyColor2, flameColor);

            Quaternion flameRot = Quaternion.Euler(0f, currentFlyAngle, 0f);

            for (int i = 0; i < thrusterCount; i++)
            {
                float offsetFactor = thrusterCount == 1 ? 0f : Mathf.Lerp(-1f, 1f, (float)i / (thrusterCount - 1));
                Vector3 nozzlePos = rearCenter + right * (offsetFactor * halfWidth) - forward * (scale * 0.45f);
                GenDraw.DrawQuad(mat, nozzlePos, flameRot, scale, block);
            }
        }

        private void TickGravshipThrusters()
        {
            if (!IsGravshipEntity) return;
            if (State == HoverState.Grounded) return;
            if (Vehicle == null || !Vehicle.Spawned || Vehicle.Map == null) return;

            // Emit particles and sound during hover, movement, takeoff and landing
            if ((Find.TickManager.TicksGame + Vehicle.thingIDNumber) % 2 == 0)
            {
                FleckDef exhaustFleck = FleckDefOf.GravshipThrusterExhaust ?? DefDatabase<FleckDef>.GetNamedSilentFail("GravshipThrusterExhaust") ?? FleckDefOf.Smoke;

                Map interiorMap = VRF_VehicleMapCompat.GetInteriorVehicleMap(Vehicle);
                if (interiorMap != null && interiorMap.listerThings != null)
                {
                    var things = interiorMap.listerThings.AllThings;
                    for (int i = 0; i < things.Count; i++)
                    {
                        Thing t = things[i];
                        if (t is Building b && !b.Destroyed && b.TryGetComp<CompGravshipThruster>() is CompGravshipThruster thrusterComp)
                        {
                            Rot4 rot = b.Rotation;
                            Vector3 thrusterWorldFacing = Quaternion.AngleAxis(currentFlyAngle, Vector3.up) * rot.FacingCell.ToVector3();
                            Vector3 exhaustDir = -thrusterWorldFacing;
                            // b.DrawPos ya está en coordenadas de mundo por el parche de VMF
                            Vector3 worldPos = b.DrawPos - thrusterWorldFacing * ((float)b.def.size.z * 0.5f + 0.5f);
                            worldPos.y = Altitudes.AltitudeFor(AltitudeLayer.MoteOverhead);

                            FleckCreationData fleckData = new FleckCreationData
                            {
                                def = exhaustFleck,
                                spawnPosition = worldPos + UnityEngine.Random.insideUnitSphere * 0.2f,
                                scale = UnityEngine.Random.Range(1.3f, 2.2f),
                                velocity = exhaustDir * UnityEngine.Random.Range(0.25f, 0.6f) + UnityEngine.Random.insideUnitSphere * 0.15f,
                                rotationRate = UnityEngine.Random.Range(-30f, 30f),
                                ageTicksOverride = -1
                            };
                            Vehicle.Map.flecks.CreateFleck(fleckData);

                            if (Rand.Chance(0.2f))
                            {
                                FleckMaker.ThrowFireGlow(worldPos, Vehicle.Map, UnityEngine.Random.Range(0.8f, 1.3f));
                            }
                        }
                    }
                }
                else
                {
                    Vector3 shipCenter = new Vector3(realPos.x, Altitudes.AltitudeFor(AltitudeLayer.MoteOverhead), realPos.y);
                    float rearOffset = Mathf.Max(1f, Vehicle.def.Size.z * 0.45f);
                    float halfWidth = Mathf.Max(0.5f, Vehicle.def.Size.x * 0.35f);
                    Vector3 forward = Quaternion.Euler(0f, currentFlyAngle, 0f) * Vector3.forward;
                    Vector3 right = Quaternion.Euler(0f, currentFlyAngle, 0f) * Vector3.right;
                    Vector3 rearCenter = shipCenter - forward * rearOffset;
                    Vector3 exhaustDir = -forward;

                    float speed = EffectiveHoverMoveSpeed;
                    int thrusterCount = Mathf.Clamp(Mathf.RoundToInt(speed / 0.1f), 1, 6);

                    for (int i = 0; i < thrusterCount; i++)
                    {
                        float offsetFactor = thrusterCount == 1 ? 0f : Mathf.Lerp(-1f, 1f, (float)i / (thrusterCount - 1));
                        Vector3 emitterPos = rearCenter + right * (offsetFactor * halfWidth);

                        FleckCreationData fleckData = new FleckCreationData
                        {
                            def = exhaustFleck,
                            spawnPosition = emitterPos + UnityEngine.Random.insideUnitSphere * 0.15f,
                            scale = UnityEngine.Random.Range(1.2f, 2.0f),
                            velocity = exhaustDir * UnityEngine.Random.Range(0.25f, 0.6f) + UnityEngine.Random.insideUnitSphere * 0.2f,
                            rotationRate = UnityEngine.Random.Range(-35f, 35f),
                            ageTicksOverride = -1
                        };
                        Vehicle.Map.flecks.CreateFleck(fleckData);

                        if (Rand.Chance(0.2f))
                        {
                            FleckMaker.ThrowFireGlow(emitterPos, Vehicle.Map, UnityEngine.Random.Range(0.8f, 1.4f));
                        }
                    }
                }
            }
        }
    }
}
