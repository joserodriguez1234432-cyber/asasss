// Decompiled with JetBrains decompiler
// Type: Vehicles.VehiclesModSettings
// Assembly: Vehicles, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 81BFDC99-D8AD-42E1-8470-2F73E7836B4A
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\Vehicles.dll

using RimWorld;
using RimWorld.Planet;
using System;
using System.Linq;
using Verse;

#nullable disable
namespace Vehicles;

public class VehiclesModSettings : ModSettings
{
  public bool showAllCargoItems;
  public bool promptVehicleCollision;
  public SectionMain main = new SectionMain();
  public SectionVehicles vehicles = new SectionVehicles();
  public SectionUpgrades upgrades = new SectionUpgrades();
  public SectionDebug debug = new SectionDebug();
  public ColorStorage colorStorage = new ColorStorage();

  public virtual void ExposeData()
  {
    try
    {
      Scribe_Deep.Look<SectionMain>(ref this.main, "main", Array.Empty<object>());
      Scribe_Deep.Look<SectionVehicles>(ref this.vehicles, "vehicles", Array.Empty<object>());
      Scribe_Deep.Look<SectionDebug>(ref this.debug, "debug", Array.Empty<object>());
      Scribe_Values.Look<bool>(ref this.showAllCargoItems, "showAllCargoItems", false, false);
      Scribe_Deep.Look<ColorStorage>(ref this.colorStorage, "colorStorage", Array.Empty<object>());
      Scribe_Values.Look<bool>(ref this.promptVehicleCollision, "promptVehicleCollision", true, false);
    }
    catch (Exception ex)
    {
      Log.Error($"Exception thrown while trying to load mod settings. Deleting the Vehicles config file might fix this.\nException={ex}\nInnerException={ex.InnerException}");
    }
  }

  public static void Open()
  {
    Find.WindowStack.Add((Window) new Dialog_ModSettings((Mod) VehicleMod.mod));
  }

  public static void TryPromptPlayer(
    string labelKey,
    string descriptionKey,
    Action onConfirm,
    Action onDeny)
  {
  }

  public static void OpenWithContext(SettingsSection section = null)
  {
    VehiclesModSettings.Open();
    if (section != null)
    {
      VehicleMod.CurrentSection = section;
    }
    else
    {
      if (WorldRendererUtility.WorldRendered || Find.CurrentMap == null || !(Find.Selector.SelectedObjects.FirstOrDefault<object>() is VehiclePawn vehiclePawn))
        return;
      VehicleMod.CurrentSection = (SettingsSection) VehicleMod.settings.vehicles;
      VehicleMod.SelectVehicle(vehiclePawn.VehicleDef);
    }
  }
}
