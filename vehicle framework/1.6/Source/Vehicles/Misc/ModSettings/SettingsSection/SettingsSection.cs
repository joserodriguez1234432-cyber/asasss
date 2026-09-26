// Decompiled with JetBrains decompiler
// Type: Vehicles.SettingsSection
// Assembly: Vehicles, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 81BFDC99-D8AD-42E1-8470-2F73E7836B4A
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\Vehicles.dll

using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;
using Verse.Sound;

#nullable disable
namespace Vehicles;

public abstract class SettingsSection : IExposable
{
  protected static Listing_Standard listingStandard = new Listing_Standard();
  protected static Listing_Settings listingSplit = new Listing_Settings();

  public virtual IEnumerable<FloatMenuOption> ResetOptions
  {
    get
    {
      SettingsSection settingsSection = this;
      yield return new FloatMenuOption(TaggedString.op_Implicit(Translator.Translate("VF_DevMode_ResetPage")), new Action(settingsSection.ResetSettings), (MenuOptionPriority) 4, (Action<Rect>) null, (Thing) null, 0.0f, (Func<Rect, bool>) null, (WorldObject) null, true, 0);
      yield return new FloatMenuOption(TaggedString.op_Implicit(Translator.Translate("VF_DevMode_ResetAll")), SettingsSection.\u003C\u003EO.\u003C0\u003E__ResetAllSettings ?? (SettingsSection.\u003C\u003EO.\u003C0\u003E__ResetAllSettings = new Action(VehicleMod.ResetAllSettings)), (MenuOptionPriority) 4, (Action<Rect>) null, (Thing) null, 0.0f, (Func<Rect, bool>) null, (WorldObject) null, true, 0);
    }
  }

  public virtual Rect ButtonRect(Rect rect)
  {
    return new Rect(((Rect) ref rect).x + 2.5f, ((Rect) ref rect).y - 2.5f, ((Rect) ref rect).width, ((Rect) ref rect).height);
  }

  public virtual void ResetSettings()
  {
    SoundStarter.PlayOneShotOnCamera(SoundDefOf.Click, (Map) null);
  }

  public virtual void OnClose()
  {
  }

  public virtual void OnOpen()
  {
  }

  public virtual void Update()
  {
  }

  public abstract void OnGUI(Rect rect);

  public virtual void Initialize()
  {
  }

  public virtual void ExposeData()
  {
  }

  public virtual void PostDefDatabase()
  {
  }

  public virtual void VehicleSelected()
  {
  }
}
