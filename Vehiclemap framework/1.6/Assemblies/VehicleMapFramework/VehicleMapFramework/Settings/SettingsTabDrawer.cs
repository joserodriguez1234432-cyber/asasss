// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.Settings.SettingsTabDrawer
// Assembly: VehicleMapFramework, Version=1.6.562.0, Culture=neutral, PublicKeyToken=null
// MVID: 10A61882-945F-4CFC-9B06-CA8EEF5ADB36
// Assembly location: D:\Programas\steamapps\workshop\content\294100\3426502333\1.6\Assemblies\VehicleMapFramework.dll

using RimWorld;
using UnityEngine;
using Verse;
using Verse.Sound;

#nullable disable
namespace VehicleMapFramework.Settings;

internal abstract class SettingsTabDrawer
{
  private readonly Vector2 ResetButtonSize = new Vector2(150f, 35f);
  protected VehicleMapSettings settings = VehicleMapFramework.VehicleMapFramework.settings;

  public abstract int Index { get; }

  public abstract string Label { get; }

  protected virtual void ResetSettings()
  {
    SoundStarter.PlayOneShotOnCamera(SoundDefOf.Click, (Map) null);
  }

  public virtual void Draw(Rect inRect)
  {
    if (!Widgets.ButtonText(new Rect(((Rect) ref inRect).xMax - this.ResetButtonSize.x, ((Rect) ref inRect).yMax - this.ResetButtonSize.y, this.ResetButtonSize.x, this.ResetButtonSize.y), TaggedString.op_Implicit(Translator.Translate("Default")), true, true, true, new TextAnchor?()))
      return;
    this.ResetSettings();
  }
}
