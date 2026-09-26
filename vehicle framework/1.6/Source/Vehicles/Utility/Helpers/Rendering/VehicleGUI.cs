// Decompiled with JetBrains decompiler
// Type: Vehicles.Rendering.VehicleGui
// Assembly: Vehicles, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 81BFDC99-D8AD-42E1-8470-2F73E7836B4A
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\Vehicles.dll

using RimWorld;
using SmashTools;
using SmashTools.Rendering;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using UnityEngine;
using Verse;
using Verse.Sound;

#nullable disable
namespace Vehicles.Rendering;

public static class VehicleGui
{
  private const float OversampleFactor = 2f;
  private const float IdlerTimeExpiry = 10f;
  private static readonly List<VehicleTurret> AllTurrets = new List<VehicleTurret>();
  private static readonly List<GraphicOverlay> AllOverlays = new List<GraphicOverlay>();

  private static (int width, int height) GetOptimalTextureSize(
    Rect rect,
    in BlitRequest request,
    float oversampleFactor)
  {
    (int, int) valueTuple = (0, 0);
    foreach (IBlitTarget blitTarget in request.blitTargets)
    {
      (int width, int height) tuple = blitTarget.TextureSize(in request);
      if (tuple.width * tuple.height > valueTuple.Item1 * valueTuple.Item2)
        valueTuple = tuple;
    }
    return (Mathf.Min(Mathf.RoundToInt(((Rect) ref rect).width * oversampleFactor), valueTuple.Item1), Mathf.Min(Mathf.RoundToInt(((Rect) ref rect).height * oversampleFactor), valueTuple.Item2));
  }

  public static RenderTexture CreateRenderTexture(
    Rect rect,
    in BlitRequest request,
    float oversampleFactor = 2f)
  {
    (int width, int height) optimalTextureSize = VehicleGui.GetOptimalTextureSize(rect, in request, oversampleFactor);
    return RenderTextureUtil.CreateRenderTexture(optimalTextureSize.width, optimalTextureSize.height);
  }

  public static RenderTextureBuffer CreateRenderTextureBuffer(
    Rect rect,
    in BlitRequest request,
    float oversampleFactor = 2f)
  {
    (int width, int height) = VehicleGui.GetOptimalTextureSize(rect, in request, oversampleFactor);
    return new RenderTextureBuffer(RenderTextureUtil.CreateRenderTexture(width, height), RenderTextureUtil.CreateRenderTexture(width, height));
  }

  private static void AddRenderData([RequiresLocation, In] ref SmashTools.Rendering.RenderData renderData)
  {
    if (RenderTextureDrawer.InUse)
    {
      RenderTextureDrawer.Add(renderData);
    }
    else
    {
      if (!TextureDrawer.InUse)
        throw new InvalidOperationException();
      TextureDrawer.Add(renderData);
    }
  }

  private static VehicleGui.BlitData GetBlitData(
    Rect rect,
    VehicleDef vehicleDef,
    PatternData patternData = null,
    Rot8? rot = null)
  {
    Vector2 vector2_1 = vehicleDef.ScaleDrawRatio(((Rect) ref rect).size);
    Rot8 rot1 = rot ?? vehicleDef.drawProperties.displayRotation;
    bool flag = rot1.IsHorizontal || rot1.IsDiagonal;
    Vector2 vector2_2 = Vector2.op_Implicit(vehicleDef.drawProperties.DisplayOffsetForRot((Rot4) rot1));
    float num1 = vector2_1.x;
    float num2 = vector2_1.y;
    if (flag)
    {
      num1 = vector2_1.y;
      num2 = vector2_1.x;
    }
    float num3 = (float) (((double) ((Rect) ref rect).width - (double) num1) / 2.0 + (double) vector2_2.x * (double) ((Rect) ref rect).width);
    float num4 = (float) (((double) ((Rect) ref rect).height - (double) num2) / 2.0 + (double) vector2_2.y * (double) ((Rect) ref rect).height);
    Rect rect1;
    // ISSUE: explicit constructor call
    ((Rect) ref rect1).\u002Ector(((Rect) ref rect).x + num3, ((Rect) ref rect).y + num4, num1, num2);
    Graphic_Vehicle graphic = vehicleDef.graphicData.Graphic as Graphic_Vehicle;
    PatternData patternData1 = patternData ?? GenCollection.TryGetValue<string, PatternData>((IReadOnlyDictionary<string, PatternData>) VehicleMod.settings.vehicles.defaultGraphics, ((Def) vehicleDef).defName, (PatternData) vehicleDef.graphicData);
    if (!VehicleMod.settings.main.useCustomShaders)
      patternData1.patternDef = PatternDefOf.Default;
    Texture2D mainTex = graphic.TexAt(rot1);
    Material material = (Material) null;
    if (graphic.Shader.SupportsRGBMaskTex())
    {
      material = RGBMaterialPool.GetUi((IMaterialCacheTarget) vehicleDef, (Rot4) rot1);
      RGBMaterialPool.SetProperties((IMaterialCacheTarget) vehicleDef, patternData1, new Func<Rot8, Texture2D>(((Graphic_Rgb) graphic).TexAt), new Func<Rot8, Texture2D>(((Graphic_Rgb) graphic).MaskAt));
    }
    return new VehicleGui.BlitData(rect1, mainTex, material, rot1, patternData1);
  }

  public static void Blit(
    RenderTexture renderTexture,
    Rect rect,
    in BlitRequest request,
    float iconScale = 1f,
    bool forceCentering = false)
  {
    RenderTextureDrawer.Open(renderTexture);
    try
    {
      foreach (IBlitTarget blitTarget in request.blitTargets)
      {
        foreach (SmashTools.Rendering.RenderData renderData in blitTarget.GetRenderData(rect, request))
          RenderTextureDrawer.Add(renderData);
      }
      RenderTextureDrawer.Draw(rect, iconScale, forceCentering);
    }
    finally
    {
      RenderTextureDrawer.Close();
    }
  }

  public static void DrawVehicleOnGUI(
    Rect rect,
    in BlitRequest request,
    float iconScale = 1f,
    bool forceCentering = false)
  {
    if (Event.current.type != 7)
      return;
    TextureDrawer.Open();
    try
    {
      foreach (IBlitTarget blitTarget in request.blitTargets)
      {
        foreach (SmashTools.Rendering.RenderData renderData in blitTarget.GetRenderData(rect, request))
          TextureDrawer.Add(renderData);
      }
      TextureDrawer.Draw(rect, iconScale, forceCentering);
    }
    finally
    {
      TextureDrawer.Close();
      VehicleGui.AllTurrets.Clear();
      VehicleGui.AllOverlays.Clear();
    }
  }

  public static void DrawVehicleDefOnGUI(
    Rect rect,
    VehicleDef vehicleDef,
    PatternData patternData = null,
    Rot8? rot = null)
  {
    if (Event.current.type != 7)
      return;
    VehicleGui.BlitData blitData = VehicleGui.GetBlitData(rect, vehicleDef, patternData, rot);
    TextureDrawer.Open();
    try
    {
      TextureDrawer.Add(new SmashTools.Rendering.RenderData(blitData.rect, (Texture) blitData.mainTex, blitData.material, vehicleDef.PropertyBlock, 0.0f, 0.0f));
      CompProperties_VehicleTurrets sortedCompProperties = vehicleDef.GetSortedCompProperties<CompProperties_VehicleTurrets>();
      if (sortedCompProperties != null)
      {
        VehicleGui.AllTurrets.AddRange((IEnumerable<VehicleTurret>) sortedCompProperties.turrets);
        VehicleGui.AddAllTurretSettingsGUIProperties(rect, vehicleDef, blitData.rot, (IEnumerable<VehicleTurret>) VehicleGui.AllTurrets, blitData.patternData);
      }
      if (!GenList.NullOrEmpty<GraphicOverlay>((IList<GraphicOverlay>) vehicleDef.drawProperties.overlays))
      {
        VehicleGui.AllOverlays.AddRange((IEnumerable<GraphicOverlay>) vehicleDef.drawProperties.overlays);
        VehicleGui.AddAllOverlaySettingsGUIProperties(rect, vehicleDef, blitData.rot, (IEnumerable<GraphicOverlay>) VehicleGui.AllOverlays, blitData.patternData);
      }
      TextureDrawer.Draw(rect);
    }
    finally
    {
      TextureDrawer.Close();
      VehicleGui.AllTurrets.Clear();
      VehicleGui.AllOverlays.Clear();
    }
  }

  private static void AddAllOverlaySettingsGUIProperties(
    Rect rect,
    VehicleDef vehicleDef,
    Rot8 rot,
    IEnumerable<GraphicOverlay> graphicOverlays,
    PatternData patternData)
  {
    foreach (GraphicOverlay graphicOverlay in graphicOverlays)
    {
      if (graphicOverlay.data.renderUI)
        VehicleGui.AddOverlaySettingsGUIProperties(rect, vehicleDef, rot, graphicOverlay, patternData);
    }
  }

  private static void AddOverlaySettingsGUIProperties(
    Rect rect,
    VehicleDef vehicleDef,
    Rot8 rot,
    GraphicOverlay graphicOverlay,
    PatternData patternData)
  {
    Rect rect1 = VehicleGraphics.OverlayRect(rect, vehicleDef, graphicOverlay, rot);
    Graphic graphic = graphicOverlay.Graphic;
    bool flag = graphic.Shader.SupportsRGBMaskTex();
    Material ui = flag ? RGBMaterialPool.GetUi((IMaterialCacheTarget) graphicOverlay, (Rot4) rot) : (Material) null;
    Texture2D mainTexture = graphic.MatAt((Rot4) rot, (Thing) null).mainTexture as Texture2D;
    if (flag)
    {
      if (graphic is Graphic_Rgb graphicRgb)
        RGBMaterialPool.SetProperties((IMaterialCacheTarget) graphicOverlay, patternData, new Func<Rot8, Texture2D>(graphicRgb.TexAt), new Func<Rot8, Texture2D>(graphicRgb.MaskAt));
      else
        RGBMaterialPool.SetProperties((IMaterialCacheTarget) graphicOverlay, patternData, (Func<Rot8, Texture2D>) (forRot => graphic.MatAt((Rot4) forRot, (Thing) null).mainTexture as Texture2D), (Func<Rot8, Texture2D>) (forRot => MaterialUtility.GetMaskTexture(graphic.MatAt((Rot4) forRot, (Thing) null))));
    }
    SmashTools.Rendering.RenderData renderData = new SmashTools.Rendering.RenderData(rect1, (Texture) mainTexture, ui, vehicleDef.PropertyBlock, graphicOverlay.data.graphicData.DrawOffsetFull(rot).y, graphicOverlay.data.rotation);
    VehicleGui.AddRenderData(ref renderData);
  }

  private static void AddAllTurretSettingsGUIProperties(
    Rect rect,
    VehicleDef vehicleDef,
    Rot8 rot,
    IEnumerable<VehicleTurret> turrets,
    PatternData patternData)
  {
    foreach (VehicleTurret turret in turrets)
    {
      if (!turret.NoGraphic)
        VehicleGui.AddTurretSettingsGUIProperties(rect, vehicleDef, turret, rot, patternData);
      if (!GenList.NullOrEmpty<VehicleTurret.TurretDrawData>((IList<VehicleTurret.TurretDrawData>) turret.TurretGraphics))
      {
        foreach (VehicleTurret.TurretDrawData turretGraphic in turret.TurretGraphics)
        {
          Rect rect1 = VehicleGraphics.TurretRect(rect, vehicleDef, turret, rot);
          Graphic_Turret graphic = turretGraphic.graphic;
          bool flag = graphic.Shader.SupportsRGBMaskTex();
          Material ui = flag ? RGBMaterialPool.GetUi((IMaterialCacheTarget) turretGraphic, Rot4.North) : (Material) null;
          if (flag && turret.def.matchParentColor)
            RGBMaterialPool.SetProperties((IMaterialCacheTarget) turretGraphic, patternData, new Func<Rot8, Texture2D>(((Graphic_Rgb) graphic).TexAt), new Func<Rot8, Texture2D>(((Graphic_Rgb) graphic).MaskAt));
          SmashTools.Rendering.RenderData renderData = new SmashTools.Rendering.RenderData(rect1, (Texture) graphic.TexAt(Rot8.North), ui, turretGraphic.PropertyBlock, turretGraphic.graphicData.drawOffset.y, turret.defaultAngleRotated + rot.AsAngle);
          VehicleGui.AddRenderData(ref renderData);
        }
      }
    }
  }

  private static void AddTurretSettingsGUIProperties(
    Rect rect,
    VehicleDef vehicleDef,
    VehicleTurret turret,
    Rot8 rot,
    PatternData patternData,
    float iconScale = 1f)
  {
    if (turret.NoGraphic)
    {
      Log.Warning("Attempting to fetch GUI properties for VehicleTurret with no graphic.");
    }
    else
    {
      Rect rect1 = VehicleGraphics.TurretRect(rect, vehicleDef, turret, rot, iconScale);
      Graphic_Turret graphic = turret.Graphic;
      bool flag = turret.Graphic.Shader.SupportsRGBMaskTex();
      Material ui = flag ? RGBMaterialPool.GetUi((IMaterialCacheTarget) turret, Rot4.North) : (Material) null;
      if (flag && turret.def.matchParentColor)
        RGBMaterialPool.SetProperties((IMaterialCacheTarget) turret, patternData, new Func<Rot8, Texture2D>(((Graphic_Rgb) graphic).TexAt), new Func<Rot8, Texture2D>(((Graphic_Rgb) graphic).MaskAt));
      SmashTools.Rendering.RenderData renderData = new SmashTools.Rendering.RenderData(rect1, (Texture) turret.Texture, ui, turret.PropertyBlock, turret.GraphicData.drawOffset.y, turret.defaultAngleRotated + rot.AsAngle);
      VehicleGui.AddRenderData(ref renderData);
    }
  }

  public static GizmoResult GizmoOnGUIWithMaterial(
    Command command,
    Rect rect,
    GizmoRenderParms parms,
    VehicleBuildDef buildDef)
  {
    bool flag1 = false;
    bool flag2 = false;
    VehicleDef thingToSpawn = buildDef.thingToSpawn;
    TextBlock textBlock1;
    // ISSUE: explicit constructor call
    ((TextBlock) ref textBlock1).\u002Ector((GameFont) 0, Color.white);
    try
    {
      if (Mouse.IsOver(rect))
      {
        flag1 = true;
        if (!((Gizmo) command).Disabled)
          GUI.color = GenUI.MouseoverColor;
      }
      MouseoverSounds.DoRegion(rect, SoundDefOf.Mouseover_Command);
      if (parms.highLight)
        Widgets.DrawStrongHighlight(GenUI.ExpandedBy(rect, 12f), new Color?());
      if (parms.lowLight)
        GUI.color = Command.LowLightBgColor;
      Material grayscaleGui = ((Gizmo) command).Disabled ? TexUI.GrayscaleGUI : (Material) null;
      GenUI.DrawTextureWithMaterial(rect, (Texture) command.BGTexture, grayscaleGui, new Rect());
      GUI.color = Color.white;
      Rect rect1 = GenUI.ContractedBy(rect, 1f);
      Widgets.BeginGroup(rect1);
      Rect rect2 = GenUI.AtZero(rect1);
      Rect rect3 = rect2;
      PatternData patternData = new PatternData((GraphicDataRGB) GenCollection.TryGetValue<string, PatternData>((IReadOnlyDictionary<string, PatternData>) VehicleMod.settings.vehicles.defaultGraphics, ((Def) thingToSpawn).defName, (PatternData) thingToSpawn.graphicData));
      if (((Gizmo) command).Disabled)
      {
        patternData.color = thingToSpawn.graphicData.color.SubtractNoAlpha(0.1f, 0.1f, 0.1f);
        patternData.colorTwo = thingToSpawn.graphicData.colorTwo.SubtractNoAlpha(0.1f, 0.1f, 0.1f);
        patternData.colorThree = thingToSpawn.graphicData.colorThree.SubtractNoAlpha(0.1f, 0.1f, 0.1f);
      }
      if (!((Gizmo) command).Disabled || parms.lowLight)
      {
        GUI.color = command.IconDrawColor;
      }
      else
      {
        GUI.color = GenColor.SaturationChanged(command.IconDrawColor, 0.0f);
        patternData.color = GenColor.SaturationChanged(thingToSpawn.graphicData.color, 0.0f);
        patternData.colorTwo = GenColor.SaturationChanged(thingToSpawn.graphicData.colorTwo, 0.0f);
        patternData.colorThree = GenColor.SaturationChanged(thingToSpawn.graphicData.colorThree, 0.0f);
      }
      if (parms.lowLight)
      {
        GUI.color = ColorExtension.ToTransparent(GUI.color, 0.6f);
        patternData.color = ColorExtension.ToTransparent(patternData.color, 0.6f);
        patternData.colorTwo = ColorExtension.ToTransparent(patternData.colorTwo, 0.6f);
        patternData.colorThree = ColorExtension.ToTransparent(patternData.colorThree, 0.6f);
      }
      BlitRequest request = BlitRequest.For(thingToSpawn);
      VehicleGui.DrawVehicleOnGUI(rect3, in request);
      GUI.color = Color.white;
      if (command.hotKey != null)
      {
        KeyCode mainKey = command.hotKey.MainKey;
        if (mainKey != null && !GizmoGridDrawer.drawnHotKeys.Contains(mainKey))
        {
          Vector2 vector2;
          // ISSUE: explicit constructor call
          ((Vector2) ref vector2).\u002Ector(5f, 3f);
          Widgets.Label(new Rect(((Rect) ref rect2).x + vector2.x, ((Rect) ref rect2).y + vector2.y, ((Rect) ref rect2).width - 10f, 18f), GenText.ToStringReadable(mainKey));
          GizmoGridDrawer.drawnHotKeys.Add(mainKey);
          if (command.hotKey.KeyDownEvent)
          {
            flag2 = true;
            Event.current.Use();
          }
        }
      }
      if (Widgets.ButtonInvisible(rect2, true))
        flag2 = true;
      Widgets.EndGroup();
      string topRightLabel = command.TopRightLabel;
      if (!GenText.NullOrEmpty(topRightLabel))
      {
        Vector2 vector2 = Text.CalcSize(topRightLabel);
        Rect rect4;
        // ISSUE: explicit constructor call
        ((Rect) ref rect4).\u002Ector((float) ((double) ((Rect) ref rect).xMax - (double) vector2.x - 2.0), ((Rect) ref rect).y + 3f, vector2.x, vector2.y);
        Rect rect5 = rect4;
        ref Rect local1 = ref rect4;
        ((Rect) ref local1).x = ((Rect) ref local1).x - 2f;
        ref Rect local2 = ref rect4;
        ((Rect) ref local2).width = ((Rect) ref local2).width + 3f;
        TextBlock textBlock2;
        // ISSUE: explicit constructor call
        ((TextBlock) ref textBlock2).\u002Ector((TextAnchor) 2, Color.white);
        try
        {
          GUI.DrawTexture(rect4, (Texture) TexUI.GrayTextBG);
          Widgets.Label(rect5, topRightLabel);
        }
        finally
        {
          textBlock2.Dispose();
        }
      }
      string str = TaggedString.op_Implicit(((Def) buildDef).LabelCap);
      if (!GenText.NullOrEmpty(str))
      {
        float num = Text.CalcHeight(str, ((Rect) ref rect).width);
        Rect rect6;
        // ISSUE: explicit constructor call
        ((Rect) ref rect6).\u002Ector(((Rect) ref rect).x, (float) ((double) ((Rect) ref rect).yMax - (double) num + 12.0), ((Rect) ref rect).width, num);
        GUI.DrawTexture(rect6, (Texture) TexUI.GrayTextBG);
        TextBlock textBlock3;
        // ISSUE: explicit constructor call
        ((TextBlock) ref textBlock3).\u002Ector((TextAnchor) 1, Color.white);
        try
        {
          Widgets.Label(rect6, str);
        }
        finally
        {
          textBlock3.Dispose();
        }
      }
      GUI.color = Color.white;
      if (Mouse.IsOver(rect))
      {
        TipSignal tipSignal = TipSignal.op_Implicit(command.Desc);
        if (((Gizmo) command).Disabled && !GenText.NullOrEmpty(((Gizmo) command).disabledReason))
        {
          ref string local = ref tipSignal.text;
          local = TaggedString.op_Implicit(TaggedString.op_Addition(local, TaggedString.op_Addition(TaggedString.op_Addition(TaggedString.op_Addition("\n\n", Translator.Translate("DisabledCommand")), ": "), ((Gizmo) command).disabledReason)));
        }
        TooltipHandler.TipRegion(rect, tipSignal);
      }
      if (!GenText.NullOrEmpty(command.HighlightTag) && (Find.WindowStack.FloatMenu == null || !((Rect) ref ((Window) Find.WindowStack.FloatMenu).windowRect).Overlaps(rect)))
        UIHighlighter.HighlightOpportunity(rect, command.HighlightTag);
      Text.Font = (GameFont) 1;
      if (!flag2)
        return new GizmoResult(flag1 ? (GizmoState) 1 : (GizmoState) 0, (Event) null);
      if (((Gizmo) command).Disabled)
      {
        if (!GenText.NullOrEmpty(((Gizmo) command).disabledReason))
          Messages.Message(((Gizmo) command).disabledReason, MessageTypeDefOf.RejectInput, false);
        return new GizmoResult((GizmoState) 1, (Event) null);
      }
      GizmoResult gizmoResult;
      if (Event.current.button == 1)
      {
        // ISSUE: explicit constructor call
        ((GizmoResult) ref gizmoResult).\u002Ector((GizmoState) 3, Event.current);
      }
      else
      {
        if (!TutorSystem.AllowAction(EventPack.op_Implicit(command.TutorTagSelect)))
          return new GizmoResult((GizmoState) 1, (Event) null);
        // ISSUE: explicit constructor call
        ((GizmoResult) ref gizmoResult).\u002Ector((GizmoState) 2, Event.current);
        TutorSystem.Notify_Event(EventPack.op_Implicit(command.TutorTagSelect));
      }
      return gizmoResult;
    }
    finally
    {
      textBlock1.Dispose();
    }
  }

  private readonly struct BlitData(
    Rect rect,
    Texture2D mainTex,
    Material material,
    Rot8 rot,
    PatternData patternData)
  {
    public readonly Rect rect = rect;
    public readonly Texture2D mainTex = mainTex;
    public readonly Material material = material;
    public readonly Rot8 rot = rot;
    public readonly PatternData patternData = patternData;
  }
}
