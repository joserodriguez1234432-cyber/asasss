// Decompiled with JetBrains decompiler
// Type: Vehicles.Dialog_GiveVehicleName
// Assembly: Vehicles, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 81BFDC99-D8AD-42E1-8470-2F73E7836B4A
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\Vehicles.dll

using RimWorld;
using UnityEngine;
using Verse;

#nullable disable
namespace Vehicles;

public class Dialog_GiveVehicleName : Window
{
  private const float MarginSize = 15f;
  private string curName;
  private readonly VehiclePawn vehicle;

  public Dialog_GiveVehicleName(VehiclePawn vehicle)
    : base((IWindowDrawing) null)
  {
    this.forcePause = false;
    this.doCloseX = true;
    this.closeOnClickedOutside = true;
    this.absorbInputAroundWindow = true;
    this.closeOnClickedOutside = true;
    this.closeOnAccept = true;
    this.curName = ((Entity) vehicle).Label;
    this.vehicle = vehicle;
  }

  protected virtual int MaxNameLength => 28;

  private Name CurVehicleName => (Name) new NameSingle(this.curName, false);

  public virtual Vector2 InitialSize => new Vector2(280f, 175f);

  protected virtual AcceptanceReport NameIsValid(string name)
  {
    return name.Length == 0 ? AcceptanceReport.op_Implicit(false) : AcceptanceReport.op_Implicit(true);
  }

  public virtual void OnAcceptKeyPressed() => this.AcceptName();

  public virtual void DoWindowContents(Rect inRect)
  {
    TextBlock textBlock;
    // ISSUE: explicit constructor call
    ((TextBlock) ref textBlock).\u002Ector((GameFont) 2);
    try
    {
      Widgets.Label(new Rect(15f, 15f, 500f, 50f), this.CurVehicleName.ToString().Replace(" '' ", " "));
      Text.Font = (GameFont) 1;
      string str = Widgets.TextField(new Rect(15f, 50f, (float) ((double) ((Rect) ref inRect).width - 15.0 - 15.0), 35f), this.curName);
      if (str.Length < this.MaxNameLength)
        this.curName = str;
      Rect rect = new Rect(15f, (float) ((double) ((Rect) ref inRect).height - 35.0 - 15.0), (float) ((double) ((Rect) ref inRect).width / 2.0 - 15.0), 35f);
      if (Widgets.ButtonText(rect, TaggedString.op_Implicit(Translator.Translate("VF_OkName")), true, true, true, new TextAnchor?()))
        this.AcceptName();
      ref Rect local = ref rect;
      ((Rect) ref local).x = ((Rect) ref local).x + ((Rect) ref rect).width;
      if (!Widgets.ButtonText(rect, TaggedString.op_Implicit(Translator.Translate("VF_RemoveName")), true, true, true, new TextAnchor?()))
        return;
      this.vehicle.Name = (Name) null;
      this.Close(true);
    }
    finally
    {
      textBlock.Dispose();
    }
  }

  private void AcceptName()
  {
    AcceptanceReport acceptanceReport = this.NameIsValid(this.curName);
    if (!((AcceptanceReport) ref acceptanceReport).Accepted)
    {
      if (GenText.NullOrEmpty(((AcceptanceReport) ref acceptanceReport).Reason))
        Messages.Message(TaggedString.op_Implicit(Translator.Translate("VF_InvalidName")), MessageTypeDefOf.RejectInput, false);
      else
        Messages.Message(((AcceptanceReport) ref acceptanceReport).Reason, MessageTypeDefOf.RejectInput, false);
    }
    else
    {
      if (string.IsNullOrEmpty(this.curName))
        this.curName = this.vehicle.Name.ToStringFull;
      this.vehicle.Name = this.CurVehicleName;
      this.Close(true);
    }
  }
}
