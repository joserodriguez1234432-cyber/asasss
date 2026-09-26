// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.VMF_Widgets
// Assembly: VehicleMapFramework, Version=1.6.562.0, Culture=neutral, PublicKeyToken=null
// MVID: 10A61882-945F-4CFC-9B06-CA8EEF5ADB36
// Assembly location: D:\Programas\steamapps\workshop\content\294100\3426502333\1.6\Assemblies\VehicleMapFramework.dll

using LudeonTK;
using RimWorld;
using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;
using Verse.Sound;

#nullable disable
namespace VehicleMapFramework;

[StaticConstructorOnStartup]
public static class VMF_Widgets
{
  private static readonly Texture2D SliderRailAtlas = ContentFinder<Texture2D>.Get("UI/Buttons/SliderRail", true);
  private static readonly Texture2D SliderHandle = ContentFinder<Texture2D>.Get("UI/Buttons/SliderHandle", true);
  private static readonly Color RangeControlTextColor = new Color(0.6f, 0.6f, 0.6f);
  private static float lastDragSliderSoundTime = -1f;
  private static int sliderDraggingID;
  private static readonly Stack<VMF_Widgets.ZoomPanState> zoomPanStack = new Stack<VMF_Widgets.ZoomPanState>();
  private static readonly Stack<Matrix4x4> matrixStack = new Stack<Matrix4x4>();
  private static int activePanControlID;

  public static Vector2 CurrentScrollPosition
  {
    get
    {
      return VMF_Widgets.zoomPanStack.Count <= 0 ? Vector2.zero : VMF_Widgets.zoomPanStack.Peek().scrollPosition;
    }
  }

  public static float CurrentZoom
  {
    get => VMF_Widgets.zoomPanStack.Count <= 0 ? 1f : VMF_Widgets.zoomPanStack.Peek().zoom;
  }

  public static Vector2 GUIToContent(Vector2 guiPos)
  {
    if (VMF_Widgets.zoomPanStack.Count == 0)
      return guiPos;
    VMF_Widgets.ZoomPanState zoomPanState = VMF_Widgets.zoomPanStack.Peek();
    return Vector2.op_Addition(Vector2.op_Division(guiPos, zoomPanState.zoom), zoomPanState.scrollPosition);
  }

  public static Rect GUIToContent(Rect guiRect)
  {
    if (VMF_Widgets.zoomPanStack.Count == 0)
      return guiRect;
    VMF_Widgets.ZoomPanState zoomPanState = VMF_Widgets.zoomPanStack.Peek();
    return new Rect(((Rect) ref guiRect).x / zoomPanState.zoom + zoomPanState.scrollPosition.x, ((Rect) ref guiRect).y / zoomPanState.zoom + zoomPanState.scrollPosition.y, ((Rect) ref guiRect).width / zoomPanState.zoom, ((Rect) ref guiRect).height / zoomPanState.zoom);
  }

  public static Vector2 ContentToGUI(Vector2 contentPos)
  {
    if (VMF_Widgets.zoomPanStack.Count == 0)
      return contentPos;
    VMF_Widgets.ZoomPanState zoomPanState = VMF_Widgets.zoomPanStack.Peek();
    return Vector2.op_Multiply(Vector2.op_Subtraction(contentPos, zoomPanState.scrollPosition), zoomPanState.zoom);
  }

  public static Rect ContentToGUI(Rect contentRect)
  {
    if (VMF_Widgets.zoomPanStack.Count == 0)
      return contentRect;
    VMF_Widgets.ZoomPanState zoomPanState = VMF_Widgets.zoomPanStack.Peek();
    return new Rect((((Rect) ref contentRect).x - zoomPanState.scrollPosition.x) * zoomPanState.zoom, (((Rect) ref contentRect).y - zoomPanState.scrollPosition.y) * zoomPanState.zoom, ((Rect) ref contentRect).width * zoomPanState.zoom, ((Rect) ref contentRect).height * zoomPanState.zoom);
  }

  public static Vector2 MousePositionContent
  {
    get => VMF_Widgets.GUIToContent(Event.current.mousePosition);
  }

  public static float HorizontalSlider(
    Rect rect,
    float value,
    float min,
    float max,
    bool middleAlignment = false,
    string label = null,
    string leftAlignedLabel = null,
    string rightAlignedLabel = null,
    float roundTo = -1f,
    Color colorFactor = default (Color))
  {
    Color color = GUI.color;
    float num1 = value;
    if (middleAlignment || !GenText.NullOrEmpty(label))
    {
      ref Rect local = ref rect;
      ((Rect) ref local).y = ((Rect) ref local).y + Mathf.Round((float) (((double) ((Rect) ref rect).height - 10.0) / 2.0));
    }
    if (!GenText.NullOrEmpty(label))
    {
      ref Rect local = ref rect;
      ((Rect) ref local).y = ((Rect) ref local).y + 5f;
    }
    int num2 = Gen.HashCombine<float>(Gen.HashCombine<float>(Gen.HashCombine<float>(Gen.HashCombine<float>(UI.GUIToScreenPoint(new Vector2(((Rect) ref rect).x, ((Rect) ref rect).y)).GetHashCode(), ((Rect) ref rect).width), ((Rect) ref rect).height), min), max);
    Rect rect1 = rect;
    ref Rect local1 = ref rect1;
    ((Rect) ref local1).xMin = ((Rect) ref local1).xMin + 6f;
    ref Rect local2 = ref rect1;
    ((Rect) ref local2).xMax = ((Rect) ref local2).xMax - 6f;
    GUI.color = Color.op_Multiply(VMF_Widgets.RangeControlTextColor, colorFactor);
    Rect rect2;
    // ISSUE: explicit constructor call
    ((Rect) ref rect2).\u002Ector(((Rect) ref rect1).x, ((Rect) ref rect1).y + 2f, ((Rect) ref rect1).width, 8f);
    Widgets.DrawAtlas(rect2, VMF_Widgets.SliderRailAtlas);
    GUI.color = colorFactor;
    GUI.DrawTexture(new Rect(Mathf.Clamp((float) ((double) ((Rect) ref rect1).x - 6.0 + (double) ((Rect) ref rect1).width * (double) Mathf.InverseLerp(min, max, num1)), ((Rect) ref rect1).xMin - 6f, ((Rect) ref rect1).xMax - 6f), ((Rect) ref rect2).center.y - 6f, 12f, 12f), (Texture) VMF_Widgets.SliderHandle);
    if (Event.current.type == null && Mouse.IsOver(rect) && VMF_Widgets.sliderDraggingID != num2)
    {
      VMF_Widgets.sliderDraggingID = num2;
      SoundStarter.PlayOneShotOnCamera(SoundDefOf.DragSlider, (Map) null);
      Event.current.Use();
    }
    if (VMF_Widgets.sliderDraggingID == num2 && UnityGUIBugsFixer.MouseDrag(0))
    {
      num1 = Mathf.Clamp((float) (((double) Event.current.mousePosition.x - (double) ((Rect) ref rect1).x) / (double) ((Rect) ref rect1).width * ((double) max - (double) min)) + min, min, max);
      if (Event.current.type == 3)
        Event.current.Use();
    }
    if (!GenText.NullOrEmpty(label) || !GenText.NullOrEmpty(leftAlignedLabel) || !GenText.NullOrEmpty(rightAlignedLabel))
    {
      TextAnchor anchor = Text.Anchor;
      GameFont font = Text.Font;
      Text.Font = (GameFont) 1;
      float num3 = GenText.NullOrEmpty(label) ? 18f : Text.CalcSize(label).y;
      ((Rect) ref rect).y = (float) ((double) ((Rect) ref rect).y - (double) num3 + 3.0);
      if (!GenText.NullOrEmpty(leftAlignedLabel))
      {
        Text.Anchor = (TextAnchor) 0;
        Widgets.Label(rect, leftAlignedLabel);
      }
      if (!GenText.NullOrEmpty(rightAlignedLabel))
      {
        Text.Anchor = (TextAnchor) 2;
        Widgets.Label(rect, rightAlignedLabel);
      }
      if (!GenText.NullOrEmpty(label))
      {
        Text.Anchor = (TextAnchor) 1;
        Widgets.Label(rect, label);
      }
      Text.Anchor = anchor;
      Text.Font = font;
    }
    if ((double) roundTo > 0.0)
      num1 = (float) Mathf.RoundToInt(num1 / roundTo) * roundTo;
    if (!Mathf.Approximately(value, num1) && (double) Time.realtimeSinceStartup > (double) VMF_Widgets.lastDragSliderSoundTime + 0.075000002980232239)
    {
      SoundStarter.PlayOneShotOnCamera(SoundDefOf.DragSlider, (Map) null);
      VMF_Widgets.lastDragSliderSoundTime = Time.realtimeSinceStartup;
    }
    GUI.color = color;
    return num1;
  }

  public static void DrawBoxRotated(
    Rect rect,
    int thickness = 1,
    Texture2D lineTexture = null,
    float rotation = 0.0f)
  {
    Vector2 center = ((Rect) ref rect).center;
    Vector2 vector2_1 = VMF_Widgets.RotatePoint(new Vector2(((Rect) ref rect).x, ((Rect) ref rect).y), center, -rotation);
    Vector2 vector2_2 = VMF_Widgets.RotatePoint(new Vector2(((Rect) ref rect).xMax, ((Rect) ref rect).yMax), center, -rotation);
    if ((double) vector2_1.x > (double) vector2_2.x)
    {
      ref float local1 = ref vector2_1.x;
      ref float local2 = ref vector2_2.x;
      float x1 = vector2_2.x;
      float x2 = vector2_1.x;
      local1 = x1;
      double num = (double) x2;
      local2 = (float) num;
    }
    if ((double) vector2_1.y > (double) vector2_2.y)
    {
      ref float local3 = ref vector2_1.y;
      ref float local4 = ref vector2_2.y;
      float y1 = vector2_2.y;
      float y2 = vector2_1.y;
      local3 = y1;
      double num = (double) y2;
      local4 = (float) num;
    }
    Vector3 vector3 = Vector2.op_Implicit(Vector2.op_Subtraction(vector2_2, vector2_1));
    Matrix4x4 matrix = GUI.matrix;
    UI.RotateAroundPivot(rotation, center);
    GUI.DrawTexture(UIScaling.AdjustRectToUIScaling(new Rect(vector2_1.x, vector2_1.y, (float) thickness, vector3.y)), (Texture) (lineTexture ?? BaseContent.WhiteTex));
    GUI.DrawTexture(UIScaling.AdjustRectToUIScaling(new Rect(vector2_2.x - (float) thickness, vector2_1.y, (float) thickness, vector3.y)), (Texture) (lineTexture ?? BaseContent.WhiteTex));
    GUI.DrawTexture(UIScaling.AdjustRectToUIScaling(new Rect(vector2_1.x + (float) thickness, vector2_1.y, vector3.x - (float) (thickness * 2), (float) thickness)), (Texture) (lineTexture ?? BaseContent.WhiteTex));
    GUI.DrawTexture(UIScaling.AdjustRectToUIScaling(new Rect(vector2_1.x + (float) thickness, vector2_2.y - (float) thickness, vector3.x - (float) (thickness * 2), (float) thickness)), (Texture) (lineTexture ?? BaseContent.WhiteTex));
    GUI.matrix = matrix;
  }

  private static Vector2 RotatePoint(Vector2 point, Vector2 origin, float angle)
  {
    return new Vector2((float) ((double) Mathf.Cos(angle * ((float) Math.PI / 180f)) * ((double) point.x - (double) origin.x) - (double) Mathf.Sin(angle * ((float) Math.PI / 180f)) * ((double) point.y - (double) origin.y)) + origin.x, (float) ((double) Mathf.Sin(angle * ((float) Math.PI / 180f)) * ((double) point.x - (double) origin.x) + (double) Mathf.Cos(angle * ((float) Math.PI / 180f)) * ((double) point.y - (double) origin.y)) + origin.y);
  }

  public static void BeginZoomPanArea(
    Rect outRect,
    ref Vector2 scrollPosition,
    ref float zoom,
    Rect viewRect,
    float minZoom = 0.25f,
    float maxZoom = 1f,
    int panMouseButton = 0,
    int ignoreDragGroup = -1)
  {
    Rect rect = GenUI.ContractedBy(outRect, 5f);
    int controlId = GUIUtility.GetControlID((FocusType) 2);
    Event current = Event.current;
    Vector2 mousePosition = current.mousePosition;
    bool flag1 = ((Rect) ref outRect).Contains(mousePosition);
    bool flag2 = DragAndDropWidget.Dragging || ignoreDragGroup != -1 && DragAndDropWidget.DraggableAt(ignoreDragGroup, Vector2.op_Implicit(VMF_Widgets.GUIToContent(mousePosition))) != null || ReorderableWidget.Dragging || Widgets.Painting || VMF_Widgets.sliderDraggingID != 0;
    if (flag1 && current.type == 6)
    {
      float num1 = (float) (-(double) current.delta.y * 0.5);
      float num2 = Mathf.Clamp(zoom + num1, minZoom, maxZoom);
      Vector2 vector2_1 = Vector2.op_Subtraction(mousePosition, ((Rect) ref outRect).position);
      Vector2 vector2_2 = Vector2.op_Addition(Vector2.op_Division(vector2_1, zoom), scrollPosition);
      scrollPosition = Vector2.op_Subtraction(vector2_2, Vector2.op_Division(vector2_1, num2));
      zoom = num2;
      current.Use();
    }
    if (!flag2)
    {
      switch ((int) current.type)
      {
        case 0:
          if (flag1 && current.button == panMouseButton)
          {
            VMF_Widgets.activePanControlID = controlId;
            GUIUtility.hotControl = controlId;
            break;
          }
          break;
        case 1:
          if (VMF_Widgets.activePanControlID == controlId && current.button == panMouseButton)
          {
            VMF_Widgets.activePanControlID = 0;
            if (GUIUtility.hotControl == controlId)
              GUIUtility.hotControl = 0;
            current.Use();
            break;
          }
          break;
        case 3:
          if (VMF_Widgets.activePanControlID == controlId)
          {
            scrollPosition = Vector2.op_Subtraction(scrollPosition, Vector2.op_Division(current.delta, zoom));
            current.Use();
            break;
          }
          break;
      }
    }
    else if (VMF_Widgets.activePanControlID == controlId)
    {
      VMF_Widgets.activePanControlID = 0;
      if (GUIUtility.hotControl == controlId)
        GUIUtility.hotControl = 0;
    }
    if ((double) ((Rect) ref viewRect).width > 0.0 && (double) ((Rect) ref viewRect).height > 0.0)
    {
      scrollPosition.x = Mathf.Clamp(scrollPosition.x, ((Rect) ref viewRect).xMin - ((Rect) ref rect).width, ((Rect) ref viewRect).xMax);
      scrollPosition.y = Mathf.Clamp(scrollPosition.y, ((Rect) ref viewRect).yMin - ((Rect) ref rect).height, ((Rect) ref viewRect).yMax);
    }
    VMF_Widgets.zoomPanStack.Push(new VMF_Widgets.ZoomPanState()
    {
      scrollPosition = scrollPosition,
      zoom = zoom
    });
    Widgets.BeginGroup(rect);
    UnityGUIBugsFixer.Notify_BeginGroup();
  }

  public static void EndZoomPanArea()
  {
    if (VMF_Widgets.zoomPanStack.Count > 0)
      VMF_Widgets.zoomPanStack.Pop();
    Widgets.EndGroup();
    UnityGUIBugsFixer.Notify_EndGroup();
  }

  private struct ZoomPanState
  {
    public Vector2 scrollPosition;
    public float zoom;
  }
}
