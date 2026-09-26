// Decompiled with JetBrains decompiler
// Type: SmashTools.Animations.Dialog_CameraView
// Assembly: SmashTools, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 3B8999A7-6EA8-49BB-AA7F-3E2E898B5249
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\SmashTools.dll

using System;
using UnityEngine;
using Verse;

#nullable disable
namespace SmashTools.Animations;

public class Dialog_CameraView : Window
{
  private const float WindowMargin = 15f;
  private const float LeftWindowWidth = 160f;
  private const float WindowRatio = 1.5333333f;
  private const float LeftWindowMinHeight = 250f;
  private const float MinWidth = 413.3333f;
  private const float DefaultViewSize = 300f;
  private const float ResizerBtnSize = 24f;
  private readonly Func<bool> disabled;
  private readonly Func<Vector3> position;
  private Listing_SplitColumns lister = new Listing_SplitColumns();
  private Vector2 windowPosition;
  private Vector2 windowSize;
  private bool resizing;
  private bool needsResizing;
  private Rect startingWindowRect;
  private Rect resizeLaterRect;

  public Dialog_CameraView(
    Func<bool> disabled,
    Func<Vector3> position,
    Vector2 bottomRight,
    float size = 1f)
    : base((IWindowDrawing) null)
  {
    this.disabled = disabled;
    this.position = position;
    this.windowSize = new Vector2((float) (160.0 + 300.0 * (double) size), 300f * size);
    this.windowPosition = new Vector2(bottomRight.x - this.windowSize.x, bottomRight.y - this.windowSize.y);
    this.SetWindowProperties();
  }

  public virtual Vector2 InitialSize => this.windowSize;

  protected virtual float Margin => 0.0f;

  private void SetWindowProperties()
  {
    this.closeOnClickedOutside = false;
    this.draggable = true;
    this.absorbInputAroundWindow = false;
    this.preventCameraMotion = false;
    this.doCloseX = true;
    this.layer = (WindowLayer) 3;
  }

  protected virtual void SetInitialSizeAndPosition()
  {
    this.windowRect = GenUI.Rounded(new Rect(this.windowPosition, this.windowSize));
  }

  public virtual void WindowUpdate()
  {
    base.WindowUpdate();
    if (this.disabled())
      return;
    if (CameraView.animationSettings.drawCellGrid)
      CameraView.DrawMapGridInView();
    CameraView.Update(this.position());
  }

  public virtual void WindowOnGUI()
  {
    if (this.needsResizing)
    {
      this.needsResizing = false;
      this.windowRect = this.resizeLaterRect;
    }
    base.WindowOnGUI();
  }

  public virtual void DoWindowContents(Rect inRect)
  {
    Widgets.DrawWindowBackground(inRect);
    this.DoResizerButton(this.windowRect);
    this.DrawPreviewWindow(GenUI.ContractedBy(inRect, 15f), this.disabled());
  }

  private void DrawPreviewWindow(Rect rect, bool disabled)
  {
    string header = "Preview Window Disabled";
    bool flag = false;
    if (!disabled)
    {
      try
      {
        float height = ((Rect) ref rect).height;
        Rect rect1;
        // ISSUE: explicit constructor call
        ((Rect) ref rect1).\u002Ector(((Rect) ref rect).x + ((Rect) ref rect).width - height, ((Rect) ref rect).y, height, height);
        disabled = !CameraView.RenderAt(rect1);
        if (!disabled)
        {
          using (new TextBlock(Color.white))
          {
            Rect rect2 = GenUI.ContractedBy(new Rect(((Rect) ref rect).x, ((Rect) ref rect).y, (float) ((double) ((Rect) ref rect).width - (double) height - 1.0), height), 5f);
            this.lister.columnGap = 0.0f;
            this.lister.Begin(rect2, 1);
            this.lister.Header("Settings", (GameFont) 2, (TextAnchor) 4);
            this.lister.CheckboxLabeled("Draw Cell Grid", ref CameraView.animationSettings.drawCellGrid, "Draw lines along edges of the map's cells.", string.Empty, false);
            ((Listing) this.lister).End();
            if (Mouse.IsOver(rect1))
              CameraView.HandleZoom();
          }
        }
      }
      catch (Exception ex)
      {
        disabled = true;
        flag = true;
        header = ex.ToString();
        Log.ErrorOnce($"Exception thrown in CameraView. Exception = {ex}", "CameraView_GraphEditor".GetHashCode());
      }
    }
    if (!disabled)
      return;
    UIElements.Header(rect, header, flag ? new Color(0.0f, 0.0f, 0.0f, 0.75f) : ListingExtension.BannerColor, (GameFont) 2, (TextAnchor) 4);
  }

  private void DoResizerButton(Rect winRect)
  {
    Vector2 mousePosition = Event.current.mousePosition;
    Rect rect;
    // ISSUE: explicit constructor call
    ((Rect) ref rect).\u002Ector(((Rect) ref winRect).width - 24f, ((Rect) ref winRect).height - 24f, 24f, 24f);
    if (Event.current.type == null && Mouse.IsOver(rect))
    {
      this.resizing = true;
      this.startingWindowRect = new Rect(mousePosition.x, mousePosition.y, ((Rect) ref winRect).width, ((Rect) ref winRect).height);
    }
    if (this.resizing)
    {
      float num = Mathf.Max(mousePosition.x - ((Rect) ref this.startingWindowRect).x, mousePosition.y - ((Rect) ref this.startingWindowRect).y);
      ((Rect) ref winRect).width = ((Rect) ref this.startingWindowRect).width + num;
      ((Rect) ref winRect).height = ((Rect) ref this.startingWindowRect).height + num;
      if ((double) ((Rect) ref winRect).width < 413.33331298828125 || (double) ((Rect) ref winRect).height < 250.0)
      {
        ((Rect) ref winRect).width = 413.3333f;
        ((Rect) ref winRect).height = 250f;
      }
      if ((double) ((Rect) ref winRect).xMax > (double) UI.screenWidth || (double) ((Rect) ref winRect).yMax > (double) UI.screenHeight)
        return;
      if (Event.current.type == 1)
        this.resizing = false;
      if (Rect.op_Inequality(winRect, this.resizeLaterRect))
      {
        this.needsResizing = true;
        this.resizeLaterRect = winRect;
      }
    }
    Widgets.ButtonImage(rect, TexUI.WinExpandWidget, true, (string) null);
  }
}
