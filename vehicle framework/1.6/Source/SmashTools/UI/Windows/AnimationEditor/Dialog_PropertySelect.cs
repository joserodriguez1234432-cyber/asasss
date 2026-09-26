// Decompiled with JetBrains decompiler
// Type: SmashTools.Animations.Dialog_PropertySelect
// Assembly: SmashTools, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 3B8999A7-6EA8-49BB-AA7F-3E2E898B5249
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\SmashTools.dll

using RimWorld;
using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;
using Verse.Sound;

#nullable disable
namespace SmashTools.Animations;

public class Dialog_PropertySelect : Window
{
  private const float EntryHeight = 28f;
  private const float SubPropertyPadding = 15f;
  private static readonly Color backgroundColor;
  private static readonly Color backgroundOutlineColor;
  private readonly IAnimator animator;
  private readonly AnimationClip animation;
  private Vector2 scrollPos;
  private Vector2 position;
  private Action<AnimationPropertyParent> propertyAdded;
  private List<string> propertyListOrder = new List<string>();
  private Dictionary<string, List<AnimationPropertyParent>> properties = new Dictionary<string, List<AnimationPropertyParent>>();
  private bool[] expandedContainers;

  public Dialog_PropertySelect(
    IAnimator animator,
    AnimationClip animation,
    Vector2 position,
    Action<AnimationPropertyParent> propertyAdded = null)
    : base((IWindowDrawing) null)
  {
    this.animator = animator;
    this.animation = animation;
    this.position = position;
    this.propertyAdded = propertyAdded;
    this.closeOnClickedOutside = true;
    this.absorbInputAroundWindow = false;
    this.preventCameraMotion = false;
    this.doWindowBackground = false;
    this.layer = (WindowLayer) 3;
  }

  private float WindowHeight { get; set; }

  public virtual Vector2 InitialSize => new Vector2(300f, 350f);

  protected virtual float Margin => 0.0f;

  public virtual void PreOpen()
  {
    HashSet<AnimationPropertyParent> animationPropertyParentSet1;
    if (this.animation.properties != null)
    {
      HashSet<AnimationPropertyParent> animationPropertyParentSet2 = new HashSet<AnimationPropertyParent>();
      foreach (AnimationPropertyParent property in this.animation.properties)
        animationPropertyParentSet2.Add(property);
      animationPropertyParentSet1 = animationPropertyParentSet2;
    }
    else
      animationPropertyParentSet1 = new HashSet<AnimationPropertyParent>();
    HashSet<AnimationPropertyParent> animationPropertyParentSet3 = animationPropertyParentSet1;
    foreach (AnimationPropertyParent animationProperty in AnimationPropertyRegistry.GetAnimationProperties(this.animator))
    {
      if (!animationPropertyParentSet3.Contains(animationProperty))
      {
        string key = animationProperty.Identifier != null ? $"{animationProperty.Type.Name} ({animationProperty.Identifier})" : animationProperty.Type.Name;
        if (!this.properties.ContainsKey(key))
          this.propertyListOrder.Add(key);
        this.properties.AddOrAppend<string, List<AnimationPropertyParent>, AnimationPropertyParent>(key, animationProperty);
      }
    }
    this.expandedContainers = new bool[this.properties.Count];
    base.PreOpen();
  }

  public virtual void Notify_ClickOutsideWindow()
  {
    base.Notify_ClickOutsideWindow();
    this.Close(true);
  }

  private void RecalculateHeight()
  {
    float num = 0.0f;
    for (int index = 0; index < this.properties.Count; ++index)
    {
      string key = this.propertyListOrder[index];
      num += 28f;
      if (this.expandedContainers[index])
        num += (float) this.properties[key].Count * 28f;
    }
    this.WindowHeight = num;
  }

  protected virtual void SetInitialSizeAndPosition()
  {
    if ((double) this.position.x + (double) base.InitialSize.x > (double) UI.screenWidth)
      this.position.x = (float) UI.screenWidth - base.InitialSize.x;
    if ((double) this.position.y + (double) base.InitialSize.y > (double) UI.screenHeight)
      this.position.y = (float) UI.screenHeight - base.InitialSize.y;
    this.windowRect = new Rect(this.position.x, this.position.y, base.InitialSize.x, base.InitialSize.y);
  }

  public virtual void DoWindowContents(Rect inRect)
  {
    Widgets.DrawBoxSolidWithOutline(inRect, Dialog_PropertySelect.backgroundColor, Dialog_PropertySelect.backgroundOutlineColor, 2);
    TextBlock textBlock;
    // ISSUE: explicit constructor call
    ((TextBlock) ref textBlock).\u002Ector(new GameFont?((GameFont) 1), new TextAnchor?((TextAnchor) 3), new bool?(false));
    try
    {
      Rect rect1 = inRect;
      Rect rect2 = new Rect(((Rect) ref rect1).x, ((Rect) ref rect1).y, ((Rect) ref rect1).width - 16f, this.WindowHeight);
      Widgets.BeginScrollView(rect1, ref this.scrollPos, rect2, true);
      Rect rect3 = GenUI.ContractedBy(new Rect(((Rect) ref inRect).x, ((Rect) ref inRect).y, ((Rect) ref inRect).width, 28f), 3f);
      for (int index = 0; index < this.properties.Count; ++index)
      {
        string key = this.propertyListOrder[index];
        bool expandedContainer = this.expandedContainers[index];
        Rect rect4;
        Rect rect5;
        GenUI.SplitVertically(rect3, 28f, ref rect4, ref rect5);
        Widgets.Label(rect5, key);
        if (UIElements.CollapseButton(GenUI.ContractedBy(rect4, 2f), ref expandedContainer))
        {
          this.expandedContainers[index] = expandedContainer;
          SoundStarter.PlayOneShotOnCamera(SoundDefOf.Click, (Map) null);
        }
        ref Rect local1 = ref rect3;
        ((Rect) ref local1).y = ((Rect) ref local1).y + ((Rect) ref rect3).height;
        if (expandedContainer)
        {
          foreach (AnimationPropertyParent container in this.properties[key])
          {
            Rect rect6;
            // ISSUE: explicit constructor call
            ((Rect) ref rect6).\u002Ector(((Rect) ref rect5).x + 15f, ((Rect) ref rect3).y, ((Rect) ref rect5).width - 15f, ((Rect) ref rect5).height);
            if (!this.DrawProperty(rect6, container))
            {
              ref Rect local2 = ref rect3;
              ((Rect) ref local2).y = ((Rect) ref local2).y + ((Rect) ref rect3).height;
            }
            else
              break;
          }
        }
      }
      Widgets.EndScrollView();
    }
    finally
    {
      textBlock.Dispose();
    }
  }

  private bool DrawProperty(Rect rect, AnimationPropertyParent container)
  {
    Widgets.Label(rect, container.Label);
    if (!this.AddPropertyButton(rect))
      return false;
    this.animation.properties.Add(container);
    Action<AnimationPropertyParent> propertyAdded = this.propertyAdded;
    if (propertyAdded != null)
      propertyAdded(container);
    this.Close(true);
    return true;
  }

  private bool AddPropertyButton(Rect rect)
  {
    float height = ((Rect) ref rect).height;
    Rect rect1;
    // ISSUE: explicit constructor call
    ((Rect) ref rect1).\u002Ector(((Rect) ref rect).xMax - height, ((Rect) ref rect).y, height, height);
    return Widgets.ButtonImage(GenUI.ContractedBy(rect1, 2f), TexButton.Plus, true, (string) null);
  }

  static Dialog_PropertySelect()
  {
    ColorInt colorInt1 = new ColorInt(56, 56, 56);
    Dialog_PropertySelect.backgroundColor = ((ColorInt) ref colorInt1).ToColor;
    ColorInt colorInt2 = new ColorInt(74, 74, 74);
    Dialog_PropertySelect.backgroundOutlineColor = ((ColorInt) ref colorInt2).ToColor;
  }
}
