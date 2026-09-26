// Decompiled with JetBrains decompiler
// Type: Vehicles.Dialog_NodeSettings
// Assembly: Vehicles, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 81BFDC99-D8AD-42E1-8470-2F73E7836B4A
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\Vehicles.dll

using UnityEngine;
using Verse;

#nullable disable
namespace Vehicles;

public class Dialog_NodeSettings : Window
{
  private const float ModSettingsDefaultWidth = 900f;
  private const float ModSettingsDefaultHeight = 700f;
  private Vector2 windowSize = new Vector2(10f, 10f);
  private Vector2 drawLoc = new Vector2(0.0f, 0.0f);

  public Dialog_NodeSettings(VehicleDef def, UpgradeNode node, Vector2 origin)
    : base((IWindowDrawing) null)
  {
    this.VehicleDef = def;
    this.UpgradeNode = node;
    this.closeOnClickedOutside = true;
    this.onlyOneOfTypeAllowed = true;
    this.closeOnCancel = true;
    this.doCloseX = true;
    this.windowSize = new Vector2(400f, (float) (3 * 100 + 50));
    this.drawLoc = origin;
    this.Lister = new Listing_Settings(SettingsPage.Upgrades, (GameFont) 0);
  }

  public VehicleDef VehicleDef { get; set; }

  public UpgradeNode UpgradeNode { get; set; }

  public Listing_Settings Lister { get; set; }

  public virtual Vector2 InitialSize => this.windowSize;

  public virtual void PostClose()
  {
    base.PostClose();
    VehicleMod.selectedNode = (UpgradeNode) null;
  }

  protected virtual void SetInitialSizeAndPosition()
  {
    this.windowRect = new Rect(this.drawLoc.x + (float) (((double) UI.screenWidth - 900.0) / 2.0), this.drawLoc.y + (float) (((double) UI.screenHeight - 700.0) / 2.0), base.InitialSize.x, base.InitialSize.y);
    this.windowRect = GenUI.Rounded(this.windowRect);
  }

  public virtual void DoWindowContents(Rect inRect)
  {
    this.Lister.Begin(inRect, 2);
    ((Listing) this.Lister).End();
  }
}
