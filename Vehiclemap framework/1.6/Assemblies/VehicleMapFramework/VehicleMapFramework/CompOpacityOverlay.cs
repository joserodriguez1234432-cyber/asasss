// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.CompOpacityOverlay
// Assembly: VehicleMapFramework, Version=1.6.562.0, Culture=neutral, PublicKeyToken=null
// MVID: 10A61882-945F-4CFC-9B06-CA8EEF5ADB36
// Assembly location: D:\Programas\steamapps\workshop\content\294100\3426502333\1.6\Assemblies\VehicleMapFramework.dll

using System;
using System.Collections.Generic;
using UnityEngine;
using Vehicles;
using Vehicles.Rendering;
using Verse;

#nullable disable
namespace VehicleMapFramework;

public class CompOpacityOverlay : VehicleComp
{
  private float tmpOpacity;

  public CompProperties_OpacityOverlay Props
  {
    get => (CompProperties_OpacityOverlay) ((ThingComp) this).props;
  }

  protected GraphicOverlay Overlay
  {
    get
    {
      if (this.\u003COverlay\u003Ek__BackingField == null)
      {
        VehiclePawn vehicle = this.Vehicle;
        GraphicOverlay graphicOverlay;
        if (vehicle == null)
        {
          graphicOverlay = (GraphicOverlay) null;
        }
        else
        {
          VehicleDrawTracker drawTracker = vehicle.DrawTracker;
          if (drawTracker == null)
          {
            graphicOverlay = (GraphicOverlay) null;
          }
          else
          {
            GraphicOverlayRenderer overlayRenderer = drawTracker.overlayRenderer;
            graphicOverlay = overlayRenderer != null ? GenCollection.FirstOrDefault<GraphicOverlay>(overlayRenderer.AllOverlaysListForReading, (Predicate<GraphicOverlay>) (o => o.data?.identifier == this.Props.identifier)) : (GraphicOverlay) null;
          }
        }
        this.\u003COverlay\u003Ek__BackingField = graphicOverlay;
      }
      return this.\u003COverlay\u003Ek__BackingField;
    }
  }

  public virtual IEnumerable<Gizmo> CompGetGizmosExtra()
  {
    GraphicOverlay overlay = this.Overlay;
    if (overlay != null)
    {
      Texture2D texture2D = ContentFinder<Texture2D>.Get(overlay.Graphic.path + "_east", true);
      Vector2 vector2 = ((Texture) texture2D).height > ((Texture) texture2D).width ? new Vector2((float) ((Texture) texture2D).height / (float) ((Texture) texture2D).height, 1f) : new Vector2(1f, (float) ((Texture) texture2D).height / (float) ((Texture) texture2D).width);
      Command_Action commandAction = new Command_Action();
      ((Command) commandAction).defaultLabel = this.Props.label;
      ((Command) commandAction).icon = (Texture) texture2D;
      ((Command) commandAction).iconProportions = vector2;
      commandAction.action = (Action) (() =>
      {
        Rect rect = new Rect(Vector2.op_Subtraction(UI.MousePositionOnUIInverted, new Vector2(75f, 18f)), new Vector2(150f, 33f));
        Graphic_VehicleOpacity graphic = overlay.Graphic as Graphic_VehicleOpacity;
        if (graphic == null)
          return;
        WindowStack windowStack = Find.WindowStack;
        windowStack.Add((Window) new EphemenalWindow()
        {
          windowRect = rect,
          doWindowFunc = (Action) (() =>
          {
            Widgets.DrawWindowBackground(GenUI.AtZero(rect), GUI.color);
            graphic.Opacity = VMF_Widgets.HorizontalSlider(new Rect(0.0f, 15f, ((Rect) ref rect).width, ((Rect) ref rect).height), graphic.Opacity, 0.0f, 1f, leftAlignedLabel: "0%", rightAlignedLabel: "100%", colorFactor: GUI.color);
          })
        });
      });
      yield return (Gizmo) commandAction;
    }
  }

  public virtual void PostExposeData()
  {
    ((ThingComp) this).PostExposeData();
    if (Scribe.mode == 1)
    {
      if (!(this.Overlay?.Graphic is Graphic_VehicleOpacity graphic))
        return;
      this.tmpOpacity = graphic.Opacity;
      Scribe_Values.Look<float>(ref this.tmpOpacity, this.Props.identifier + "Opacity", 1f, false);
    }
    else if (Scribe.mode == 2)
    {
      Scribe_Values.Look<float>(ref this.tmpOpacity, this.Props.identifier + "Opacity", 1f, false);
    }
    else
    {
      if (Scribe.mode != 4)
        return;
      LongEventHandler.ExecuteWhenFinished((Action) (() =>
      {
        if (!(this.Overlay?.Graphic is Graphic_VehicleOpacity graphic2))
          return;
        graphic2.Opacity = this.tmpOpacity;
      }));
    }
  }
}
