// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.CompVehicleLauncherGravshipVehicle
// Assembly: VehicleMapFramework, Version=1.6.562.0, Culture=neutral, PublicKeyToken=null
// MVID: 10A61882-945F-4CFC-9B06-CA8EEF5ADB36
// Assembly location: D:\Programas\steamapps\workshop\content\294100\3426502333\1.6\Assemblies\VehicleMapFramework.dll

using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using Vehicles;
using Verse;

#nullable disable
namespace VehicleMapFramework;

public class CompVehicleLauncherGravshipVehicle : CompVehicleLauncherWithMap
{
  public override IEnumerable<Gizmo> CompGetGizmosExtra()
  {
    CompVehicleLauncherGravshipVehicle launcherGravshipVehicle1 = this;
    // ISSUE: reference to a compiler-generated method
    foreach (Gizmo gizmo1 in launcherGravshipVehicle1.\u003C\u003En__0())
    {
      Gizmo gizmo = gizmo1;
      CompVehicleLauncherGravshipVehicle launcherGravshipVehicle = launcherGravshipVehicle1;
      yield return gizmo;
      if (gizmo is Command_ActionHighlighter actionHighlighter && !gizmo.Disabled)
      {
        string disableReason;
        Building_GravEngine engine;
        CompPilotConsole console;
        Pawn pilot;
        Pawn copilot;
        if (!launcherGravshipVehicle1.CanLaunchGravship(out disableReason, out engine, out console, out pilot, out copilot))
          ((Gizmo) actionHighlighter).Disable(disableReason);
        else
          ((Command_Action) actionHighlighter).action = (Action) (() => launcherGravshipVehicle.StartChoosingDestination(((VehicleComp) launcherGravshipVehicle).Vehicle, engine, console, pilot, copilot));
        gizmo = (Gizmo) null;
      }
    }
  }

  public bool CanLaunchGravship(
    out string disableReason,
    out Building_GravEngine engine,
    out CompPilotConsole console,
    out Pawn pilot,
    out Pawn copilot)
  {
    disableReason = (string) null;
    engine = (Building_GravEngine) null;
    console = (CompPilotConsole) null;
    pilot = (Pawn) null;
    copilot = (Pawn) null;
    if (!(((VehicleComp) this).Vehicle is VehiclePawnWithMap vehicle) || !((Def) ((Thing) ((VehicleComp) this).Vehicle).def).HasModExtension<VehicleMapProps_Gravship>())
      return false;
    engine = GravshipUtility.GetPlayerGravEngine_NewTemp(vehicle.VehicleMap);
    if (engine == null)
    {
      ref string local = ref disableReason;
      TaggedString taggedString = Translator.Translate("CannotLaunchNoEngine");
      string str = TaggedString.op_Implicit(((TaggedString) ref taggedString).CapitalizeFirst());
      local = str;
      return false;
    }
    PocketMapProperties pocketMapProperties = vehicle.VehicleMap.generatorDef?.pocketMapProperties;
    bool flag = pocketMapProperties != null && pocketMapProperties.canLaunchGravship;
    try
    {
      if (pocketMapProperties != null)
        pocketMapProperties.canLaunchGravship = true;
      AcceptanceReport? report = new AcceptanceReport?();
      if ((console = engine.GravshipComponents.OfType<CompPilotConsole>().FirstOrDefault<CompPilotConsole>((Func<CompPilotConsole, bool>) (c =>
      {
        AcceptanceReport acceptanceReport = (report = new AcceptanceReport?(c.CanUseNow())).Value;
        return ((AcceptanceReport) ref acceptanceReport).Accepted;
      }))) == null)
      {
        ref string local1 = ref disableReason;
        ref AcceptanceReport? local2 = ref report;
        string str;
        if (!local2.HasValue)
        {
          str = (string) null;
        }
        else
        {
          AcceptanceReport valueOrDefault = local2.GetValueOrDefault();
          str = ((AcceptanceReport) ref valueOrDefault).Reason;
        }
        if (str == null)
        {
          TaggedString taggedString = Translator.Translate("PilotConsoleInaccessible");
          str = TaggedString.op_Implicit(((TaggedString) ref taggedString).CapitalizeFirst());
        }
        local1 = str;
        return false;
      }
    }
    finally
    {
      if (pocketMapProperties != null)
        pocketMapProperties.canLaunchGravship = flag;
    }
    ref Pawn local3 = ref pilot;
    List<VehicleRoleHandler> handlers1 = vehicle.handlers;
    Pawn pawn1;
    if (handlers1 == null)
    {
      pawn1 = (Pawn) null;
    }
    else
    {
      VehicleRoleHandler vehicleRoleHandler = GenCollection.FirstOrDefault<VehicleRoleHandler>(handlers1, (Predicate<VehicleRoleHandler>) (h => h.role?.key == nameof (pilot)));
      if (vehicleRoleHandler == null)
      {
        pawn1 = (Pawn) null;
      }
      else
      {
        ThingOwner<Pawn> thingOwner = vehicleRoleHandler.thingOwner;
        if (thingOwner == null)
        {
          pawn1 = (Pawn) null;
        }
        else
        {
          List<Pawn> innerListForReading = thingOwner.InnerListForReading;
          pawn1 = innerListForReading != null ? innerListForReading.FirstOrDefault<Pawn>() : (Pawn) null;
        }
      }
    }
    local3 = pawn1;
    if (pilot == null)
    {
      ref string local4 = ref disableReason;
      TaggedString taggedString = Translator.Translate("VMF_CannotLaunchNoPilot");
      string str = TaggedString.op_Implicit(((TaggedString) ref taggedString).CapitalizeFirst());
      local4 = str;
      return false;
    }
    ref Pawn local5 = ref copilot;
    List<VehicleRoleHandler> handlers2 = vehicle.handlers;
    Pawn pawn2;
    if (handlers2 == null)
    {
      pawn2 = (Pawn) null;
    }
    else
    {
      VehicleRoleHandler vehicleRoleHandler = GenCollection.FirstOrDefault<VehicleRoleHandler>(handlers2, (Predicate<VehicleRoleHandler>) (h => h.role?.key == nameof (copilot)));
      if (vehicleRoleHandler == null)
      {
        pawn2 = (Pawn) null;
      }
      else
      {
        ThingOwner<Pawn> thingOwner = vehicleRoleHandler.thingOwner;
        if (thingOwner == null)
        {
          pawn2 = (Pawn) null;
        }
        else
        {
          List<Pawn> innerListForReading = thingOwner.InnerListForReading;
          pawn2 = innerListForReading != null ? innerListForReading.FirstOrDefault<Pawn>() : (Pawn) null;
        }
      }
    }
    local5 = pawn2;
    return true;
  }

  private void StartChoosingDestination(
    VehiclePawn vehicle,
    Building_GravEngine engine,
    CompPilotConsole console,
    Pawn pilot,
    Pawn copilot)
  {
    if (this.AnyLeftToLoad)
      Find.WindowStack.Add((Window) Dialog_MessageBox.CreateConfirmation(TranslatorFormattedStringExtensions.Translate("ConfirmSendNotCompletelyLoadedPods", NamedArgument.op_Implicit(((Thing) vehicle).LabelCapNoCount)), new Action(OpenDialog), false, (string) null, (WindowLayer) 1));
    else
      OpenDialog();

    void OpenDialog()
    {
      // ISSUE: object of a compiler-generated type is created
      // ISSUE: variable of a compiler-generated type
      CompVehicleLauncherGravshipVehicle.\u003C\u003Ec__DisplayClass2_1 cDisplayClass21 = new CompVehicleLauncherGravshipVehicle.\u003C\u003Ec__DisplayClass2_1();
      // ISSUE: reference to a compiler-generated field
      cDisplayClass21.CS\u0024\u003C\u003E8__locals1 = this;
      // ISSUE: reference to a compiler-generated field
      cDisplayClass21.assignedSeats = new Dictionary<Pawn, VehicleRoleHandler>();
      // ISSUE: reference to a compiler-generated field
      cDisplayClass21.copilotHandler = (VehicleRoleHandler) null;
      foreach (VehicleRoleHandler vehicleRoleHandler in vehicle.handlers.Where<VehicleRoleHandler>((Func<VehicleRoleHandler, bool>) (handler => handler is VehicleRoleHandlerBuildable)))
      {
        if (vehicleRoleHandler.role?.key == nameof (copilot))
        {
          // ISSUE: reference to a compiler-generated field
          cDisplayClass21.copilotHandler = vehicleRoleHandler;
        }
        for (int index = ((ThingOwner) vehicleRoleHandler.thingOwner).Count - 1; index >= 0; --index)
        {
          Pawn key = vehicleRoleHandler.thingOwner[index];
          // ISSUE: reference to a compiler-generated field
          cDisplayClass21.assignedSeats[key] = vehicleRoleHandler;
          vehicle.DisembarkPawn(key);
        }
      }
      // ISSUE: reference to a compiler-generated field
      cDisplayClass21.ritual = (Precept_Ritual) pilot.Ideo.GetPrecept(PreceptDefOf.GravshipLaunch);
      // ISSUE: reference to a compiler-generated field
      List<RitualObligation> activeObligations = cDisplayClass21.ritual.activeObligations;
      // ISSUE: reference to a compiler-generated method
      RitualObligation ritualObligation = activeObligations != null ? GenCollection.FirstOrDefault<RitualObligation>(activeObligations, new Predicate<RitualObligation>(cDisplayClass21.\u003CStartChoosingDestination\u003Eb__1)) : (RitualObligation) null;
      RitualOutcomeEffectDef named = DefDatabase<RitualOutcomeEffectDef>.GetNamed("GravshipLaunch", true);
      // ISSUE: reference to a compiler-generated field
      cDisplayClass21.forcedForRole = new Dictionary<string, Pawn>()
      {
        [nameof (pilot)] = pilot
      };
      if (copilot != null)
      {
        // ISSUE: reference to a compiler-generated field
        cDisplayClass21.forcedForRole[nameof (copilot)] = copilot;
      }
      // ISSUE: reference to a compiler-generated field
      cDisplayClass21.dialog = (Dialog_BeginRitual) null;
      // ISSUE: reference to a compiler-generated field
      // ISSUE: reference to a compiler-generated field
      // ISSUE: reference to a compiler-generated field
      // ISSUE: reference to a compiler-generated method
      // ISSUE: method pointer
      // ISSUE: reference to a compiler-generated field
      cDisplayClass21.dialog = new Dialog_BeginRitual(((Precept) cDisplayClass21.ritual).LabelCap, cDisplayClass21.ritual, TargetInfo.op_Implicit((Thing) ((ThingComp) console).parent), ((Thing) engine).Map, new Dialog_BeginRitual.ActionCallback(cDisplayClass21.\u003CStartChoosingDestination\u003Eb__2), pilot, ritualObligation, new Dialog_BeginRitual.PawnFilter((object) cDisplayClass21, __methodptr(\u003CStartChoosingDestination\u003Eb__3)), (string) null, (List<Pawn>) null, cDisplayClass21.forcedForRole, named, (List<string>) null, (Pawn) null);
      // ISSUE: reference to a compiler-generated field
      Find.WindowStack.Add((Window) cDisplayClass21.dialog);
    }
  }
}
