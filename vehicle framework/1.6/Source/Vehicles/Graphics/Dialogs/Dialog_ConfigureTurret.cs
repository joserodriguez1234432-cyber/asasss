// Decompiled with JetBrains decompiler
// Type: Vehicles.Dialog_ConfigureTurret
// Assembly: Vehicles, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 81BFDC99-D8AD-42E1-8470-2F73E7836B4A
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\Vehicles.dll

using RimWorld;
using SmashTools;
using System;
using System.Linq;
using UnityEngine;
using Verse;

#nullable disable
namespace Vehicles;

public class Dialog_ConfigureTurret : Window
{
  private const float RowHeight = 24f;
  private const float RowPadding = 5f;
  private static readonly Vector2 ButtonSize = new Vector2(160f, 40f);
  private readonly QuickSearchWidget filter = new QuickSearchWidget();
  private readonly VehicleTurret turret;
  private Vector2 scrollPosition;

  public Dialog_ConfigureTurret(VehicleTurret turret)
    : base((IWindowDrawing) null)
  {
    this.turret = turret;
    this.resizeable = true;
    this.draggable = true;
  }

  private float ListHeight { get; set; }

  public virtual Vector2 InitialSize => new Vector2(300f, 400f);

  private void RecacheSettings()
  {
    this.ListHeight = 0.0f;
    if (this.turret.def.ammunition == null)
      return;
    this.ListHeight += (float) this.turret.def.ammunition.AllowedThingDefs.Count<ThingDef>() * 29f;
  }

  public virtual void DoWindowContents(Rect inRect)
  {
    TextBlock textBlock;
    // ISSUE: explicit constructor call
    ((TextBlock) ref textBlock).\u002Ector((GameFont) 1, (TextAnchor) 3);
    try
    {
      if (this.turret.def.ammunition == null)
      {
        this.DrawSimpleSlider(inRect);
      }
      else
      {
        Rect rect1 = new Rect(((Rect) ref inRect).position, Window.QuickSearchSize);
        this.filter.OnGUI(rect1, (Action) null, (Action) null);
        Rect rect2 = new Rect(((Rect) ref inRect).x, ((Rect) ref rect1).yMax + 5f, ((Rect) ref inRect).width, Dialog_ConfigureTurret.ButtonSize.y);
        Rect rect3 = inRect;
        ((Rect) ref rect3).yMin = ((Rect) ref rect2).yMax + 5f;
        Rect rect4 = rect3;
        rect3 = rect4;
        ((Rect) ref rect3).height = this.ListHeight;
        ((Rect) ref rect3).width = ((Rect) ref rect4).width - 16f;
        Rect rect5 = rect3;
        Widgets.BeginScrollView(rect4, ref this.scrollPosition, rect5, true);
        rect3 = rect5;
        ((Rect) ref rect3).height = 24f;
        Rect rect6 = rect3;
        foreach (ThingDef allowedThingDef in this.turret.def.ammunition.AllowedThingDefs)
        {
          Rect rect7 = rect6;
          bool flag = this.turret.loadConfig.IsEnabled(allowedThingDef);
          bool enabled = flag;
          rect3 = rect7;
          ((Rect) ref rect3).width = ((Rect) ref rect7).height;
          UIElements.CheckboxButton(rect3, ref enabled);
          if (enabled != flag)
            this.turret.loadConfig.SetEnabled(allowedThingDef, enabled);
          ref Rect local1 = ref rect7;
          ((Rect) ref local1).xMin = ((Rect) ref local1).xMin + 24f;
          rect3 = rect7;
          ((Rect) ref rect3).width = ((Rect) ref rect7).height;
          Rect rect8 = rect3;
          Widgets.ThingIcon(rect8, allowedThingDef, (ThingDef) null, (ThingStyleDef) null, 1f, new Color?(), new int?(), 1f);
          TooltipHandler.TipRegion(rect8, TipSignal.op_Implicit(((Def) allowedThingDef).LabelCap));
          ref Rect local2 = ref rect7;
          ((Rect) ref local2).xMin = ((Rect) ref local2).xMin + ((Rect) ref rect8).width;
          int num = this.turret.loadConfig.Get(allowedThingDef);
          int max = Mathf.RoundToInt(this.turret.vehicle.GetStatValue(VehicleStatDefOf.CargoCapacity) / StatExtension.GetStatValueAbstract((BuildableDef) allowedThingDef, StatDefOf.Mass, (ThingDef) null));
          int count = UIElements.HorizontalSlider(rect7, TaggedString.op_Implicit(TranslatorFormattedStringExtensions.Translate("VF_SetReloadLevel", NamedArgument.op_Implicit(num))), num, 0, max);
          if (!Mathf.Approximately((float) num, (float) count))
            this.turret.loadConfig.Set(allowedThingDef, count);
          ref Rect local3 = ref rect6;
          ((Rect) ref local3).y = ((Rect) ref local3).y + 29f;
        }
        Widgets.EndScrollView();
      }
    }
    finally
    {
      textBlock.Dispose();
    }
  }

  private void DrawSimpleSlider(Rect rect)
  {
  }
}
