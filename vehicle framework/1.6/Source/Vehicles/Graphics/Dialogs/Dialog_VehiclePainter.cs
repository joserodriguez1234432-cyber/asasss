// Decompiled with JetBrains decompiler
// Type: Vehicles.Dialog_VehiclePainter
// Assembly: Vehicles, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 81BFDC99-D8AD-42E1-8470-2F73E7836B4A
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\Vehicles.dll

using RimWorld;
using SmashTools;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Vehicles.Rendering;
using Verse;
using Verse.Sound;

#nullable disable
namespace Vehicles;

[StaticConstructorOnStartup]
public class Dialog_VehiclePainter : Window
{
  private const float ButtonWidth = 90f;
  private const float ButtonHeight = 30f;
  private const float SliderHeight = 40f;
  private const float IconButtonSize = 48f;
  private const float SwitchSize = 60f;
  private const float SamplePadding = 2f;
  private const float FramePadding = 1f;
  private const int GridDimensionColumns = 2;
  private const int GridDimensionRows = 2;
  private const int SampleCount = 4;
  private int pageNumber;
  private int pageCount;
  private static PatternDef selectedPattern;
  private Rot8 displayRotation;
  private readonly ColorPicker colorPicker = new ColorPicker();
  private float hue;
  private float saturation;
  private float value;
  private string hex;
  private Color currentColorOne;
  private Color currentColorTwo;
  private Color currentColorThree;
  private Dialog_VehiclePainter.ColorIndex colorSelected;
  private float additionalTiling = 1f;
  private float displacementX;
  private float displacementY;
  private bool draggingDisplacement;
  private float initialDragDifferenceX;
  private float initialDragDifferenceY;
  private bool showPatterns = true;
  private bool mouseOver;

  private Dialog_VehiclePainter(VehicleDef vehicleDef)
    : base((IWindowDrawing) null)
  {
    this.VehicleDef = vehicleDef;
  }

  private Dialog_VehiclePainter(VehiclePawn vehicle)
    : this(vehicle.VehicleDef)
  {
    this.Vehicle = vehicle;
  }

  private VehiclePawn Vehicle { get; }

  private VehicleDef VehicleDef { get; }

  private PatternData PatternData { get; init; }

  private List<PatternDef> AvailablePatterns { get; set; }

  private Dialog_VehiclePainter.SaveColor OnSave { get; init; }

  private bool IsClosing { get; set; }

  private int CurrentSelectedPalette { get; set; }

  public virtual Vector2 InitialSize => new Vector2(900f, 540f);

  private Rot8 DisplayRotation
  {
    get => this.displayRotation;
    set => this.displayRotation = value;
  }

  private Color CurrentColor
  {
    get
    {
      switch (this.colorSelected)
      {
        case Dialog_VehiclePainter.ColorIndex.One:
          return this.currentColorOne;
        case Dialog_VehiclePainter.ColorIndex.Two:
          return this.currentColorTwo;
        case Dialog_VehiclePainter.ColorIndex.Three:
          return this.currentColorThree;
        default:
          throw new NotImplementedException("ColorIndex");
      }
    }
  }

  private static string ColorToHex(Color col) => ColorUtility.ToHtmlStringRGB(col);

  private static bool HexToColor(string hexColor, out Color color)
  {
    return ColorUtility.TryParseHtmlString("#" + hexColor, ref color);
  }

  public static void OpenColorPicker(VehiclePawn vehicle, Dialog_VehiclePainter.SaveColor onSave)
  {
    Dialog_VehiclePainter.Open(new Dialog_VehiclePainter(vehicle)
    {
      OnSave = onSave,
      PatternData = new PatternData(vehicle)
    });
  }

  public static void OpenColorPicker(VehicleDef vehicleDef, Dialog_VehiclePainter.SaveColor onSave)
  {
    Dialog_VehiclePainter.Open(new Dialog_VehiclePainter(vehicleDef)
    {
      OnSave = onSave,
      PatternData = new PatternData((GraphicDataRGB) GenCollection.TryGetValue<string, PatternData>((IReadOnlyDictionary<string, PatternData>) VehicleMod.settings.vehicles.defaultGraphics, ((Def) vehicleDef).defName, (PatternData) vehicleDef.graphicData))
    });
  }

  private static void Open(Dialog_VehiclePainter colorPickerDialog)
  {
    colorPickerDialog.Init();
    Find.WindowStack.Add((Window) colorPickerDialog);
  }

  private void Init()
  {
    this.additionalTiling = this.PatternData.tiles;
    this.displacementX = this.PatternData.displacement.x;
    this.displacementY = this.PatternData.displacement.y;
    this.SetColors(this.PatternData.color, this.PatternData.colorTwo, this.PatternData.colorThree);
    this.CurrentSelectedPalette = -1;
    this.doCloseX = true;
    this.forcePause = true;
    this.absorbInputAroundWindow = true;
    this.DisplayRotation = this.VehicleDef.drawProperties.displayRotation;
    this.RecacheAvailablePatterns();
  }

  private void UpdateHSV(Color color)
  {
    Color.RGBToHSV(color, ref this.hue, ref this.saturation, ref this.value);
    this.hex = Dialog_VehiclePainter.ColorToHex(color);
  }

  private void SetColor(Color color) => this.SetColor(color, this.colorSelected);

  private void SetColor(Color color, Dialog_VehiclePainter.ColorIndex index)
  {
    switch (index)
    {
      case Dialog_VehiclePainter.ColorIndex.One:
        this.currentColorOne = color;
        break;
      case Dialog_VehiclePainter.ColorIndex.Two:
        this.currentColorTwo = color;
        break;
      case Dialog_VehiclePainter.ColorIndex.Three:
        this.currentColorThree = color;
        break;
      default:
        throw new NotImplementedException("ColorIndex");
    }
    this.UpdateHSV(this.CurrentColor);
  }

  private void SetColors(Color color1, Color color2, Color color3)
  {
    this.SetColor(color1, Dialog_VehiclePainter.ColorIndex.One);
    this.SetColor(color2, Dialog_VehiclePainter.ColorIndex.Two);
    this.SetColor(color3, Dialog_VehiclePainter.ColorIndex.Three);
  }

  private void SetColor(string hex)
  {
    Color color;
    if (!Dialog_VehiclePainter.HexToColor(hex, out color))
      return;
    this.SetColor(color);
  }

  private void SetColor(float h, float s, float v) => this.SetColor(Color.HSVToRGB(h, s, v));

  private void RecacheAvailablePatterns()
  {
    this.AvailablePatterns = !this.showPatterns ? DefDatabase<SkinDef>.AllDefsListForReading.Where<SkinDef>((Func<SkinDef, bool>) (patternDef => patternDef.ValidFor(this.VehicleDef))).Cast<PatternDef>().ToList<PatternDef>() : DefDatabase<PatternDef>.AllDefsListForReading.Where<PatternDef>((Func<PatternDef, bool>) (patternDef => !(patternDef is SkinDef) && patternDef.ValidFor(this.VehicleDef))).ToList<PatternDef>();
    this.pageCount = Mathf.CeilToInt((float) this.AvailablePatterns.Count / 4f);
    this.pageNumber = 1;
    Dialog_VehiclePainter.selectedPattern = this.AvailablePatterns.Contains(this.PatternData.patternDef) ? this.PatternData.patternDef : this.AvailablePatterns.FirstOrDefault<PatternDef>();
    if (Dialog_VehiclePainter.selectedPattern != null)
      return;
    Dialog_VehiclePainter.selectedPattern = PatternDefOf.Default;
  }

  public virtual void PostClose()
  {
    base.PostClose();
    this.IsClosing = true;
    CursorSettings.Reset();
  }

  public virtual void PostOpen()
  {
    base.PostOpen();
    this.hex = Dialog_VehiclePainter.ColorToHex(this.CurrentColor);
  }

  public virtual void DoWindowContents(Rect inRect)
  {
    if (this.IsClosing)
      return;
    TextBlock textBlock;
    // ISSUE: explicit constructor call
    ((TextBlock) ref textBlock).\u002Ector((GameFont) 1);
    try
    {
      if (Widgets.ButtonText(new Rect(0.0f, 0.0f, 135f, 30f), TaggedString.op_Implicit(Translator.Translate("VF_ResetColorPalettes")), true, true, true, new TextAnchor?()))
        Find.WindowStack.Add((Window) Dialog_MessageBox.CreateConfirmation(Translator.Translate("VF_ResetColorPalettesConfirmation"), new Action(VehicleMod.settings.colorStorage.ResetPalettes), false, (string) null, (WindowLayer) 1));
      if (this.VehicleDef.graphicData.drawRotated && this.VehicleDef.graphicData.Graphic is Graphic_Vehicle graphic)
      {
        Rect rect;
        // ISSUE: explicit constructor call
        ((Rect) ref rect).\u002Ector((float) ((double) ((Rect) ref inRect).width / 3.0 - 10.0 - 30.0), 0.0f, 30f, 30f);
        Widgets.DrawHighlightIfMouseover(rect);
        TooltipHandler.TipRegionByKey(rect, "VF_RotateVehicleRendering");
        Widgets.DrawTextureFitted(rect, (Texture) VehicleTex.Rotate, 1f, 1f);
        if (Widgets.ButtonInvisible(rect, true))
        {
          SoundStarter.PlayOneShotOnCamera(SoundDefOf.Click, (Map) null);
          List<Rot8> list = graphic.RotationsRenderableByUI.ToList<Rot8>();
          for (int index = 0; index < 4; ++index)
          {
            this.DisplayRotation = this.DisplayRotation.Rotated((RotationDirection) 1, false);
            if (list.Contains(this.DisplayRotation))
              break;
          }
        }
      }
      float num1 = (float) ((double) ((Rect) ref inRect).height / 2.0 + 60.0);
      Rect colorContainerRect = new Rect(((Rect) ref inRect).width / 1.5f, 15f, ((Rect) ref inRect).width / 3f, num1);
      this.DrawColorSelection(colorContainerRect);
      Rect paintRect = new Rect((float) ((double) ((Rect) ref inRect).width / 3.0 - 5.0), ((Rect) ref colorContainerRect).y, ((Rect) ref inRect).width / 3f, num1);
      this.DrawPaintSelection(paintRect);
      float num2 = (float) ((double) ((Rect) ref inRect).width * 2.0 / 3.0 + 5.0);
      float num3 = (float) ((double) ((Rect) ref inRect).height - (double) num1 - 20.0);
      this.DrawColorPalette(new Rect((float) ((double) ((Rect) ref inRect).width / 3.0 - 5.0), ((Rect) ref inRect).height - num3, num2, num3));
      Rect rect1 = new Rect(0.0f, ((Rect) ref paintRect).y + (float) ((double) ((Rect) ref paintRect).height / 2.0 - ((double) ((Rect) ref paintRect).width - 15.0) / 2.0), ((Rect) ref paintRect).width - 15f, ((Rect) ref paintRect).width - 15f);
      this.HandleDisplacementDrag(rect1);
      PatternData patternData = new PatternData(this.currentColorOne, this.currentColorTwo, this.currentColorThree, Dialog_VehiclePainter.selectedPattern, new Vector2(this.displacementX, this.displacementY), this.additionalTiling);
      BlitRequest blitRequest;
      if (this.Vehicle == null)
        blitRequest = BlitRequest.For(this.VehicleDef) with
        {
          patternData = patternData,
          rot = this.DisplayRotation
        };
      else
        blitRequest = BlitRequest.For(this.Vehicle) with
        {
          patternData = patternData,
          rot = this.DisplayRotation
        };
      BlitRequest request = blitRequest;
      VehicleGui.DrawVehicleOnGUI(rect1, in request);
      if (!Dialog_VehiclePainter.selectedPattern.properties.dynamicTiling)
        GUIState.Disable();
      Rect rect2 = new Rect(0.0f, ((Rect) ref inRect).height - 120f, 270f, 40f);
      UIElements.SliderLabeled(rect2, TaggedString.op_Implicit(Translator.Translate("VF_PatternZoom")), TaggedString.op_Implicit(Translator.Translate("VF_PatternZoomTooltip")), string.Empty, ref this.additionalTiling, 0.01f, 2f);
      Rect rect3 = new Rect(rect2);
      ((Rect) ref rect3).y = ((Rect) ref rect2).y + ((Rect) ref rect2).height;
      ((Rect) ref rect3).width = (float) ((double) ((Rect) ref rect2).width / 2.0 * 0.949999988079071);
      Rect rect4 = rect3;
      Rect rect5 = new Rect(rect4);
      ((Rect) ref rect5).x = ((Rect) ref rect4).x + (float) ((double) ((Rect) ref rect2).width / 2.0 * 1.0499999523162842);
      Rect rect6 = rect5;
      UIElements.SliderLabeled(rect4, TaggedString.op_Implicit(Translator.Translate("VF_PatternDisplacementX")), TaggedString.op_Implicit(Translator.Translate("VF_PatternDisplacementXTooltip")), string.Empty, ref this.displacementX, -1.5f, 1.5f);
      UIElements.SliderLabeled(rect6, TaggedString.op_Implicit(Translator.Translate("VF_PatternDisplacementY")), TaggedString.op_Implicit(Translator.Translate("VF_PatternDisplacementYTooltip")), string.Empty, ref this.displacementY, -1.5f, 1.5f);
      GUIState.Enable();
      this.DoBottomButtons(new Rect(0.0f, ((Rect) ref inRect).height - 40f, 90f, 40f));
    }
    finally
    {
      textBlock.Dispose();
    }
  }

  private void HandleDisplacementDrag(Rect rect)
  {
    if (Dialog_VehiclePainter.selectedPattern.properties.dynamicTiling && Mouse.IsOver(rect))
    {
      if (!this.mouseOver)
      {
        this.mouseOver = true;
        CursorSettings.SetCursor(CursorSettings.Type.OpenHand);
      }
      if (Input.GetMouseButtonDown(0) && !this.draggingDisplacement)
      {
        this.draggingDisplacement = true;
        this.initialDragDifferenceX = (float) ((double) Mathf.InverseLerp(0.0f, ((Rect) ref rect).width, Event.current.mousePosition.x - ((Rect) ref rect).x) * 2.0 - 1.0) - this.displacementX;
        this.initialDragDifferenceY = (float) ((double) Mathf.InverseLerp(((Rect) ref rect).height, 0.0f, Event.current.mousePosition.y - ((Rect) ref rect).y) * 2.0 - 1.0) - this.displacementY;
        CursorSettings.SetCursor(CursorSettings.Type.CloseHand);
      }
      if (this.draggingDisplacement && Event.current.isMouse)
      {
        float displacementX = this.displacementX;
        float displacementY = this.displacementY;
        this.displacementX = ((float) ((double) Mathf.InverseLerp(0.0f, ((Rect) ref rect).width, Event.current.mousePosition.x - ((Rect) ref rect).x) * 2.0 - 1.0) - this.initialDragDifferenceX).Clamp(-1.5f, 1.5f);
        this.displacementY = ((float) ((double) Mathf.InverseLerp(((Rect) ref rect).height, 0.0f, Event.current.mousePosition.y - ((Rect) ref rect).y) * 2.0 - 1.0) - this.initialDragDifferenceY).Clamp(-1.5f, 1.5f);
        if (!Mathf.Approximately(this.displacementX, displacementX) || !Mathf.Approximately(this.displacementY, displacementY))
          SoundStarter.PlayOneShotOnCamera(SoundDefOf.DragSlider, (Map) null);
      }
      if (!Input.GetMouseButtonUp(0))
        return;
      this.draggingDisplacement = false;
      CursorSettings.SetCursor(CursorSettings.Type.OpenHand);
    }
    else
    {
      if (!this.mouseOver)
        return;
      this.mouseOver = false;
      this.draggingDisplacement = false;
      CursorSettings.Reset();
    }
  }

  private void DrawPaintSelection(Rect paintRect)
  {
    Widgets.DrawMenuSection(paintRect);
    string label1 = TaggedString.op_Implicit(Translator.Translate("VF_Patterns"));
    string label2 = TaggedString.op_Implicit(Translator.Translate("VF_Skins"));
    float y = Text.CalcSize(label1).y;
    Rect rect1;
    // ISSUE: explicit constructor call
    ((Rect) ref rect1).\u002Ector(((Rect) ref paintRect).x, ((Rect) ref paintRect).y, (float) ((double) ((Rect) ref paintRect).width / 2.0 - 24.0), y);
    Color textColor1 = this.showPatterns ? Color.white : Color.gray;
    Color textColor2 = !this.showPatterns ? Color.white : Color.gray;
    UIElements.DrawLabel(rect1, label1, Color.clear, textColor1, (GameFont) 1, (TextAnchor) 5);
    Rect rect2;
    // ISSUE: explicit constructor call
    ((Rect) ref rect2).\u002Ector(((Rect) ref rect1).xMax, (float) ((double) ((Rect) ref rect1).y - 12.0 - 2.0), 48f, 48f);
    bool flag = Mouse.IsOver(rect2);
    if (Widgets.ButtonImage(rect2, this.showPatterns ? VehicleTex.SwitchLeft : VehicleTex.SwitchRight, true, (string) null))
    {
      this.showPatterns = !this.showPatterns;
      this.RecacheAvailablePatterns();
      SoundStarter.PlayOneShotOnCamera(SoundDefOf.Click, (Map) null);
    }
    ((Rect) ref rect1).x = ((Rect) ref rect2).xMax;
    UIElements.DrawLabel(rect1, label2, Color.clear, textColor2, (GameFont) 1);
    Rect rect3 = paintRect;
    ((Rect) ref rect3).yMin = ((Rect) ref rect1).yMax;
    ((Rect) ref rect3).yMax = ((Rect) ref paintRect).yMax - 20f;
    Rect rect4 = GenUI.ContractedBy(rect3, 10f);
    float num1 = Mathf.Min(((Rect) ref rect4).width, ((Rect) ref rect4).height) / 2f;
    Rect rect5;
    // ISSUE: explicit constructor call
    ((Rect) ref rect5).\u002Ector(0.0f, 0.0f, num1, num1);
    Rect rect6;
    // ISSUE: explicit constructor call
    ((Rect) ref rect6).\u002Ector(((Rect) ref paintRect).x + 5f, (float) ((double) ((Rect) ref paintRect).y + (double) ((Rect) ref paintRect).height - 30.0), ((Rect) ref paintRect).width - 10f, 20f);
    if (this.pageCount > 1)
      UIHelper.DrawPagination(rect6, ref this.pageNumber, this.pageCount);
    Rect square = rect4.ToSquare();
    int num2 = (this.pageNumber - 1) * 4;
    int num3 = (this.pageNumber * 2 * 2).Clamp(0, this.AvailablePatterns.Count);
    int num4 = 0;
    int index = num2;
    while (index < num3)
    {
      PatternDef availablePattern = this.AvailablePatterns[index];
      ((Rect) ref rect5).x = ((Rect) ref square).x + (float) (num4 % 2) * num1;
      ((Rect) ref rect5).y = ((Rect) ref square).y + (float) Mathf.FloorToInt((float) num4 / 2f) * num1;
      PatternData patternData = new PatternData(this.currentColorOne, this.currentColorTwo, this.currentColorThree, availablePattern, new Vector2(this.displacementX, this.displacementY), this.additionalTiling);
      BlitRequest blitRequest;
      if (this.Vehicle == null)
        blitRequest = BlitRequest.For(this.VehicleDef) with
        {
          patternData = patternData,
          rot = this.DisplayRotation
        };
      else
        blitRequest = BlitRequest.For(this.Vehicle) with
        {
          patternData = patternData,
          rot = this.DisplayRotation
        };
      BlitRequest request = blitRequest;
      VehicleGui.DrawVehicleOnGUI(rect5, in request);
      Rect rect7;
      // ISSUE: explicit constructor call
      ((Rect) ref rect7).\u002Ector(((Rect) ref rect5).x, ((Rect) ref rect5).y, num1, num1);
      if (!flag)
      {
        TooltipHandler.TipRegion(rect7, TipSignal.op_Implicit(availablePattern.LabelCap));
        if (Widgets.ButtonInvisible(rect7, true))
        {
          Dialog_VehiclePainter.selectedPattern = availablePattern;
          SoundStarter.PlayOneShotOnCamera(SoundDefOf.Click, (Map) null);
        }
      }
      ++index;
      ++num4;
    }
  }

  private void DrawColorSelection(Rect colorContainerRect)
  {
    Rect fullRect;
    // ISSUE: explicit constructor call
    ((Rect) ref fullRect).\u002Ector(colorContainerRect);
    ref Rect local = ref fullRect;
    ((Rect) ref local).x = ((Rect) ref local).x + 5f;
    ((Rect) ref fullRect).y = 35f;
    ((Rect) ref fullRect).height = ((Rect) ref colorContainerRect).height - 60f;
    Widgets.DrawMenuSection(colorContainerRect);
    string label1 = Translator.Translate("VF_ColorOne").ToString();
    string label2 = Translator.Translate("VF_ColorTwo").ToString();
    string label3 = Translator.Translate("VF_ColorThree").ToString();
    float y = Text.CalcSize(label1).y;
    Rect rect1 = this.colorPicker.Draw(fullRect, ref this.hue, ref this.saturation, ref this.value, new ColorPicker.SetColor(this.SetColor));
    Rect rect2;
    // ISSUE: explicit constructor call
    ((Rect) ref rect2).\u002Ector(((Rect) ref rect1).x, y - 2f, ((Rect) ref rect1).width, y);
    Rect rect3;
    // ISSUE: explicit constructor call
    ((Rect) ref rect3).\u002Ector(rect2);
    ((Rect) ref rect3).width = ((Rect) ref rect2).width / 3f;
    Rect rect4 = rect3;
    // ISSUE: explicit constructor call
    ((Rect) ref rect3).\u002Ector(rect2);
    ((Rect) ref rect3).x = ((Rect) ref rect2).x + ((Rect) ref rect2).width / 3f;
    ((Rect) ref rect3).width = ((Rect) ref rect2).width / 3f;
    Rect rect5 = rect3;
    // ISSUE: explicit constructor call
    ((Rect) ref rect3).\u002Ector(rect2);
    ((Rect) ref rect3).x = ((Rect) ref rect2).x + (float) ((double) ((Rect) ref rect2).width / 3.0 * 2.0);
    ((Rect) ref rect3).width = ((Rect) ref rect2).width / 3f;
    Rect rect6 = rect3;
    Rect rect7;
    // ISSUE: explicit constructor call
    ((Rect) ref rect7).\u002Ector(((Rect) ref colorContainerRect).x + 11f, 20f, 21.818182f, 21.818182f);
    if (Widgets.ButtonImage(rect7, VehicleTex.SwapColors, true, (string) null))
    {
      this.SetColors(this.currentColorTwo, this.currentColorThree, this.currentColorOne);
      SoundStarter.PlayOneShotOnCamera(SoundDefOf.Click, (Map) null);
    }
    TooltipHandler.TipRegion(rect7, TipSignal.op_Implicit(Translator.Translate("VF_SwapColors")));
    Color textColor1 = this.colorSelected == Dialog_VehiclePainter.ColorIndex.One ? Color.white : Color.gray;
    Color textColor2 = this.colorSelected == Dialog_VehiclePainter.ColorIndex.Two ? Color.white : Color.gray;
    Color textColor3 = this.colorSelected == Dialog_VehiclePainter.ColorIndex.Three ? Color.white : Color.gray;
    if (Mouse.IsOver(rect4) && this.colorSelected != Dialog_VehiclePainter.ColorIndex.One)
      textColor1 = GenUI.MouseoverColor;
    else if (Mouse.IsOver(rect5) && this.colorSelected != Dialog_VehiclePainter.ColorIndex.Two)
      textColor2 = GenUI.MouseoverColor;
    else if (Mouse.IsOver(rect6) && this.colorSelected != Dialog_VehiclePainter.ColorIndex.Three)
      textColor3 = GenUI.MouseoverColor;
    UIElements.DrawLabel(rect4, label1, Color.clear, textColor1, (GameFont) 1);
    UIElements.DrawLabel(rect5, label2, Color.clear, textColor2, (GameFont) 1, (TextAnchor) 4);
    UIElements.DrawLabel(rect6, label3, Color.clear, textColor3, (GameFont) 1, (TextAnchor) 5);
    if (this.colorSelected != Dialog_VehiclePainter.ColorIndex.One && Widgets.ButtonInvisible(rect4, true))
    {
      this.colorSelected = Dialog_VehiclePainter.ColorIndex.One;
      this.SetColor(this.currentColorOne);
      SoundStarter.PlayOneShotOnCamera(SoundDefOf.Click, (Map) null);
    }
    if (this.colorSelected != Dialog_VehiclePainter.ColorIndex.Two && Widgets.ButtonInvisible(rect5, true))
    {
      this.colorSelected = Dialog_VehiclePainter.ColorIndex.Two;
      this.SetColor(this.currentColorTwo);
      SoundStarter.PlayOneShotOnCamera(SoundDefOf.Click, (Map) null);
    }
    if (this.colorSelected != Dialog_VehiclePainter.ColorIndex.Three && Widgets.ButtonInvisible(rect6, true))
    {
      this.colorSelected = Dialog_VehiclePainter.ColorIndex.Three;
      this.SetColor(this.currentColorThree);
      SoundStarter.PlayOneShotOnCamera(SoundDefOf.Click, (Map) null);
    }
    Rect rect8;
    // ISSUE: explicit constructor call
    ((Rect) ref rect8).\u002Ector(((Rect) ref fullRect).x, (float) ((double) ((Rect) ref fullRect).y + (double) ((Rect) ref fullRect).height + 5.0), ((Rect) ref fullRect).width / 2f, 20f);
    string str1 = UIElements.HexField(TaggedString.op_Implicit(Translator.Translate("VF_ColorPickerHex")), rect8, this.hex);
    if (this.hex != str1)
    {
      this.hex = str1;
      this.SetColor(this.hex);
    }
    string str2 = TaggedString.op_Implicit(Translator.Translate("VF_SaveColorPalette"));
    ((Rect) ref rect8).width = Text.CalcSize(str2).x + 20f;
    ((Rect) ref rect8).x = (float) ((double) ((Rect) ref colorContainerRect).x + (double) ((Rect) ref colorContainerRect).width - (double) ((Rect) ref rect8).width - 5.0);
    if (!Widgets.ButtonText(rect8, str2, true, true, true, new TextAnchor?()))
      return;
    int currentSelectedPalette = this.CurrentSelectedPalette;
    if (currentSelectedPalette >= 0 && currentSelectedPalette < 20)
    {
      VehicleMod.settings.colorStorage.AddPalette(this.currentColorOne, this.currentColorTwo, this.currentColorThree, this.CurrentSelectedPalette);
      SoundStarter.PlayOneShotOnCamera(SoundDefOf.Click, (Map) null);
    }
    else
      Messages.Message(TaggedString.op_Implicit(Translator.Translate("VF_MustSelectColorPalette")), MessageTypeDefOf.RejectInput, true);
  }

  private void DrawColorPalette(Rect rect)
  {
    List<(Color, Color, Color)> colorPalette = VehicleMod.settings.colorStorage.colorPalette;
    Widgets.DrawMenuSection(rect);
    rect = GenUI.ContractedBy(rect, 5f);
    float num = (float) (((double) ((Rect) ref rect).width - (20.0 / (double) ColorStorage.PaletteRowCount - 1.0) * 5.0) / 15.0);
    Rect rect1;
    // ISSUE: explicit constructor call
    ((Rect) ref rect1).\u002Ector(((Rect) ref rect).x, ((Rect) ref rect).y, num, num);
    for (int index = 0; index < 20; ++index)
    {
      if (index % (20 / ColorStorage.PaletteRowCount) == 0 && index != 0)
      {
        ref Rect local = ref rect1;
        ((Rect) ref local).y = ((Rect) ref local).y + (num + 5f);
        ((Rect) ref rect1).x = ((Rect) ref rect).x;
      }
      Rect rect2;
      // ISSUE: explicit constructor call
      ((Rect) ref rect2).\u002Ector(((Rect) ref rect1).x, ((Rect) ref rect1).y, ((Rect) ref rect1).width * 3f, ((Rect) ref rect1).height);
      if (Widgets.ButtonInvisible(rect2, true))
      {
        if (this.CurrentSelectedPalette == index)
        {
          this.CurrentSelectedPalette = -1;
        }
        else
        {
          this.CurrentSelectedPalette = index;
          this.SetColors(colorPalette[index].Item1, colorPalette[index].Item2, colorPalette[index].Item3);
        }
      }
      if (this.CurrentSelectedPalette == index)
        Widgets.DrawBoxSolid(GenUI.ExpandedBy(rect2, 1.5f), Color.white);
      Widgets.DrawBoxSolid(rect1, colorPalette[index].Item1);
      ref Rect local1 = ref rect1;
      ((Rect) ref local1).x = ((Rect) ref local1).x + num;
      Widgets.DrawBoxSolid(rect1, colorPalette[index].Item2);
      ref Rect local2 = ref rect1;
      ((Rect) ref local2).x = ((Rect) ref local2).x + num;
      Widgets.DrawBoxSolid(rect1, colorPalette[index].Item3);
      ref Rect local3 = ref rect1;
      ((Rect) ref local3).x = ((Rect) ref local3).x + (num + 5f);
    }
  }

  private void DoBottomButtons(Rect buttonRect)
  {
    if (Widgets.ButtonText(buttonRect, TaggedString.op_Implicit(Translator.Translate("VF_ApplyButton")), true, true, true, new TextAnchor?()))
    {
      this.OnSave(this.currentColorOne, this.currentColorTwo, this.currentColorThree, Dialog_VehiclePainter.selectedPattern, new Vector2(this.displacementX, this.displacementY), this.additionalTiling);
      this.Close(true);
    }
    ref Rect local1 = ref buttonRect;
    ((Rect) ref local1).x = ((Rect) ref local1).x + 90f;
    if (Widgets.ButtonText(buttonRect, TaggedString.op_Implicit(Translator.Translate("CancelButton")), true, true, true, new TextAnchor?()))
      this.Close(true);
    ref Rect local2 = ref buttonRect;
    ((Rect) ref local2).x = ((Rect) ref local2).x + 90f;
    if (!Widgets.ButtonText(buttonRect, TaggedString.op_Implicit(Translator.Translate("ResetButton")), true, true, true, new TextAnchor?()))
      return;
    SoundStarter.PlayOneShotOnCamera(SoundDefOf.Click, (Map) null);
    Dialog_VehiclePainter.selectedPattern = this.PatternData.patternDef;
    this.additionalTiling = this.PatternData.tiles;
    this.displacementX = this.PatternData.displacement.x;
    this.displacementY = this.PatternData.displacement.y;
    if (this.CurrentSelectedPalette >= 0)
    {
      (Color, Color, Color) tuple = VehicleMod.settings.colorStorage.colorPalette[this.CurrentSelectedPalette];
      this.SetColors(tuple.Item1, tuple.Item2, tuple.Item3);
    }
    else
      this.SetColors(this.PatternData.color, this.PatternData.colorTwo, this.PatternData.colorThree);
  }

  public delegate void SaveColor(
    Color r,
    Color g,
    Color b,
    PatternDef pattern,
    Vector2 displacement,
    float tiles);

  private enum ColorIndex
  {
    One,
    Two,
    Three,
  }
}
