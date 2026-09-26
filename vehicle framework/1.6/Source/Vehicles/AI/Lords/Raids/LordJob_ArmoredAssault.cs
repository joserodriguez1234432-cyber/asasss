// Decompiled with JetBrains decompiler
// Type: Vehicles.LordJob_ArmoredAssault
// Assembly: Vehicles, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 81BFDC99-D8AD-42E1-8470-2F73E7836B4A
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\Vehicles.dll

using RimWorld;
using System;
using Verse;
using Verse.AI;
using Verse.AI.Group;

#nullable disable
namespace Vehicles;

public class LordJob_ArmoredAssault : LordJob_VehicleNPC
{
  private static readonly IntRange AssaultTimeBeforeGiveUp = new IntRange(26000, 38000);
  private static readonly IntRange SapTimeBeforeGiveUp = new IntRange(33000, 38000);
  private Faction assaulterFaction;
  private LordJob_ArmoredAssault.RaiderPermissions permission = LordJob_ArmoredAssault.RaiderPermissions.All;
  private LordJob_ArmoredAssault.RaiderBehavior behavior;

  public LordJob_ArmoredAssault()
  {
  }

  public LordJob_ArmoredAssault(SpawnedPawnParams parms)
  {
    this.assaulterFaction = parms.spawnerThing.Faction;
    this.permission = LordJob_ArmoredAssault.RaiderPermissions.All;
  }

  public LordJob_ArmoredAssault(
    Faction assaulterFaction,
    LordJob_ArmoredAssault.RaiderPermissions permission)
  {
    this.assaulterFaction = assaulterFaction;
    this.permission = permission;
  }

  public override float MaxVehicleSpeed => 4f;

  public virtual bool GuiltyOnDowned => true;

  public virtual StateGraph CreateGraph()
  {
    StateGraph stateGraph = new StateGraph();
    LordToil rootToil = (LordToil) null;
    LordToil assaultColonyToil = (LordToil) new LordToil_AssaultColonyArmored();
    stateGraph.AddToil(assaultColonyToil);
    LordToil_ExitMap exitMapToil = new LordToil_ExitMap((LocomotionUrgency) 3, false, true);
    ((LordToil) exitMapToil).useAvoidGrid = true;
    stateGraph.AddToil((LordToil) exitMapToil);
    if (this.assaulterFaction.def.humanlikeFaction)
    {
      this.AddTimeoutOrFleeToil(stateGraph, rootToil, assaultColonyToil, (LordToil) exitMapToil);
      this.AddKidnapToil(stateGraph, rootToil, assaultColonyToil);
      this.AddCanStealToil(stateGraph, rootToil, assaultColonyToil);
    }
    Transition transition = new Transition(assaultColonyToil, (LordToil) exitMapToil, false, true);
    if (rootToil != null)
      transition.AddSource(rootToil);
    transition.AddTrigger((Trigger) new Trigger_BecameNonHostileToPlayer());
    transition.AddPreAction((TransitionAction) new TransitionAction_Message(TaggedString.op_Implicit(TranslatorFormattedStringExtensions.Translate("MessageRaidersLeaving", NamedArgument.op_Implicit(GenText.CapitalizeFirst(this.assaulterFaction.def.pawnsPlural)), NamedArgument.op_Implicit(this.assaulterFaction.Name))), (string) null, 1f));
    stateGraph.AddTransition(transition, false);
    return stateGraph;
  }

  private void AddTimeoutOrFleeToil(
    StateGraph stateGraph,
    LordToil rootToil,
    LordToil assaultColonyToil,
    LordToil exitMapToil)
  {
    if (!this.permission.canTimeoutOrFlee)
      return;
    Transition transition1 = new Transition(assaultColonyToil, exitMapToil, false, true);
    if (rootToil != null)
      transition1.AddSource(rootToil);
    transition1.AddPreAction((TransitionAction) new TransitionAction_Message(TaggedString.op_Implicit(TranslatorFormattedStringExtensions.Translate("MessageRaidersGivenUpLeaving", NamedArgument.op_Implicit(GenText.CapitalizeFirst(this.assaulterFaction.def.pawnsPlural)), NamedArgument.op_Implicit(this.assaulterFaction.Name))), (string) null, 1f));
    stateGraph.AddTransition(transition1, false);
    Transition transition2 = new Transition(assaultColonyToil, exitMapToil, false, true);
    if (rootToil != null)
      transition2.AddSource(rootToil);
    FloatRange floatRange = new FloatRange(0.25f, 0.35f);
    float randomInRange = ((FloatRange) ref floatRange).RandomInRange;
    transition2.AddTrigger((Trigger) new Trigger_FractionColonyDamageTaken(randomInRange, 900f));
    transition2.AddPreAction((TransitionAction) new TransitionAction_Message(TaggedString.op_Implicit(TranslatorFormattedStringExtensions.Translate("MessageRaidersSatisfiedLeaving", NamedArgument.op_Implicit(GenText.CapitalizeFirst(this.assaulterFaction.def.pawnsPlural)), NamedArgument.op_Implicit(this.assaulterFaction.Name))), (string) null, 1f));
    stateGraph.AddTransition(transition2, false);
  }

  private void AddKidnapToil(StateGraph stateGraph, LordToil rootToil, LordToil assaultColonyToil)
  {
    if (!this.permission.canKidnap)
      return;
    LordToil startingToil = stateGraph.AttachSubgraph(((LordJob) new LordJob_Kidnap()).CreateGraph()).StartingToil;
    Transition transition = new Transition(assaultColonyToil, startingToil, false, true);
    if (rootToil != null)
      transition.AddSource(rootToil);
    transition.AddPreAction((TransitionAction) new TransitionAction_Message(TaggedString.op_Implicit(TranslatorFormattedStringExtensions.Translate("MessageRaidersKidnapping", NamedArgument.op_Implicit(GenText.CapitalizeFirst(this.assaulterFaction.def.pawnsPlural)), NamedArgument.op_Implicit(this.assaulterFaction.Name))), (string) null, 1f));
    transition.AddTrigger((Trigger) new Trigger_KidnapVictimPresent());
    stateGraph.AddTransition(transition, false);
  }

  private void AddCanStealToil(
    StateGraph stateGraph,
    LordToil rootToil,
    LordToil assaultColonyToil)
  {
    if (!this.permission.canSteal)
      return;
    LordToil startingToil = stateGraph.AttachSubgraph(((LordJob) new LordJob_Steal()).CreateGraph()).StartingToil;
    Transition transition = new Transition(assaultColonyToil, startingToil, false, true);
    if (rootToil != null)
      transition.AddSource(rootToil);
    transition.AddPreAction((TransitionAction) new TransitionAction_Message(TaggedString.op_Implicit(TranslatorFormattedStringExtensions.Translate("MessageRaidersStealing", NamedArgument.op_Implicit(GenText.CapitalizeFirst(this.assaulterFaction.def.pawnsPlural)), NamedArgument.op_Implicit(this.assaulterFaction.Name))), (string) null, 1f));
    transition.AddTrigger((Trigger) new Trigger_HighValueThingsAround());
    stateGraph.AddTransition(transition, false);
  }

  public virtual void ExposeData()
  {
    Scribe_References.Look<Faction>(ref this.assaulterFaction, "assaulterFaction", false);
    Scribe_Values.Look<LordJob_ArmoredAssault.RaiderPermissions>(ref this.permission, "permission", LordJob_ArmoredAssault.RaiderPermissions.All, false);
    Scribe_Values.Look<LordJob_ArmoredAssault.RaiderBehavior>(ref this.behavior, "behavior", LordJob_ArmoredAssault.RaiderBehavior.None, false);
  }

  [Flags]
  public enum RaiderBehavior
  {
    None = 0,
    Sapper = 1,
  }

  public struct RaiderPermissions : IExposable
  {
    public bool canKidnap;
    public bool canTimeoutOrFlee;
    public bool canSteal;
    public bool shouldSabotageVehicles;

    public static LordJob_ArmoredAssault.RaiderPermissions All
    {
      get
      {
        return new LordJob_ArmoredAssault.RaiderPermissions()
        {
          canKidnap = true,
          canTimeoutOrFlee = true,
          canSteal = true,
          shouldSabotageVehicles = true
        };
      }
    }

    void IExposable.ExposeData()
    {
      Scribe_Values.Look<bool>(ref this.canKidnap, "canKidnap", false, false);
      Scribe_Values.Look<bool>(ref this.canTimeoutOrFlee, "canTimeoutOrFlee", false, false);
      Scribe_Values.Look<bool>(ref this.canSteal, "canSteal", false, false);
      Scribe_Values.Look<bool>(ref this.shouldSabotageVehicles, "shouldSabotageVehicles", false, false);
    }
  }
}
