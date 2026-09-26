// Decompiled with JetBrains decompiler
// Type: SmashTools.Animations.AnimationCondition
// Assembly: SmashTools, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 3B8999A7-6EA8-49BB-AA7F-3E2E898B5249
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\SmashTools.dll

using RimWorld;
using RimWorld.Planet;
using SmashTools.Xml;
using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;
using Verse.Sound;

#nullable disable
namespace SmashTools.Animations;

public class AnimationCondition : IXmlExport
{
  private const float UIDropdownPadding = 5f;
  private string def;
  private ComparisonType comparison = ComparisonType.Equal;
  private float value;
  [Unsaved(false)]
  private AnimationParameterDef paramDef;
  [Unsaved(false)]
  private string inputBuffer;
  [Unsaved(false)]
  private AnimationParameter parameter;

  public AnimationTransition Transition { get; internal set; }

  public AnimationParameterDef Def
  {
    get => this.paramDef;
    private set
    {
      if (value == this.paramDef)
        return;
      this.paramDef = value;
      this.def = this.paramDef.defName;
    }
  }

  public AnimationParameter Parameter
  {
    get
    {
      if (this.parameter == null)
        this.ResolveParameter();
      return this.parameter;
    }
    internal set
    {
      if (this.parameter == value)
        return;
      this.parameter = value;
      if (this.parameter == null)
        return;
      this.Def = this.parameter.def;
    }
  }

  public bool ConditionMet(float value)
  {
    switch (this.Parameter.Type)
    {
      case AnimationParameter.ParamType.Float:
        return this.comparison != ComparisonType.GreaterThan ? (double) value < (double) this.value : (double) value > (double) this.value;
      case AnimationParameter.ParamType.Int:
        switch (this.comparison)
        {
          case ComparisonType.LessThan:
            return (double) value < (double) this.value;
          case ComparisonType.Equal:
            return (double) value == (double) this.value;
          case ComparisonType.GreaterThan:
            return (double) value > (double) this.value;
          case ComparisonType.NotEqual:
            return (double) value != (double) this.value;
          default:
            throw new NotSupportedException($"Integer comparision type: {this.comparison}");
        }
      case AnimationParameter.ParamType.Bool:
      case AnimationParameter.ParamType.Trigger:
        return (double) value == (double) this.value;
      default:
        throw new NotImplementedException(this.Parameter.Type.ToString());
    }
  }

  public void DrawConditionInput(Rect rect)
  {
    Rect rect1;
    // ISSUE: explicit constructor call
    ((Rect) ref rect1).\u002Ector(((Rect) ref rect).x, ((Rect) ref rect).y, 24f, 24f);
    ref Rect local = ref rect;
    ((Rect) ref local).xMin = ((Rect) ref local).xMin + ((Rect) ref rect1).width;
    if (Event.current != null && Event.current.type == null && Event.current.button == 1 && Mouse.IsOver(rect))
    {
      Event.current.Use();
      Find.WindowStack.Add((Window) new FloatMenu(new List<FloatMenuOption>(1)
      {
        new FloatMenuOption(TaggedString.op_Implicit(Translator.Translate("Delete")), (Action) (() => this.Transition.conditions.Remove(this)), (MenuOptionPriority) 4, (Action<Rect>) null, (Thing) null, 0.0f, (Func<Rect, bool>) null, (WorldObject) null, true, 0)
      }));
    }
    List<AnimationParameter> parameters = this.Transition.FromState.Layer.Controller.parameters;
    if (this.Parameter.Type == AnimationParameter.ParamType.Float)
    {
      this.FieldFloat(rect);
    }
    else
    {
      if (this.Parameter.Type == AnimationParameter.ParamType.Int)
        return;
      if (this.Parameter.Type == AnimationParameter.ParamType.Bool)
      {
        this.FieldBool(rect);
      }
      else
      {
        int type = (int) this.Parameter.Type;
      }
    }
  }

  private void FieldFloat(Rect rect)
  {
    List<AnimationParameter> parameters = this.Transition.FromState.Layer.Controller.parameters;
    Rect[] rectArray = rect.SplitVertically(new float[3]
    {
      0.4f,
      0.3f,
      0.3f
    }, 5f);
    Rect rect1 = rectArray[0];
    Rect rect2 = rectArray[1];
    Rect rect3 = rectArray[2];
    if (AnimationEditor.Dropdown(rect1, this.Parameter?.Name ?? string.Empty, (string) null))
    {
      if (!parameters.NullOrEmpty<AnimationParameter>())
      {
        List<FloatMenuOption> floatMenuOptionList = new List<FloatMenuOption>();
        foreach (AnimationParameter animationParameter in parameters)
        {
          AnimationParameter parameter = animationParameter;
          floatMenuOptionList.Add(new FloatMenuOption(parameter.Name, (Action) (() => this.Parameter = parameter), (MenuOptionPriority) 4, (Action<Rect>) null, (Thing) null, 0.0f, (Func<Rect, bool>) null, (WorldObject) null, true, 0));
        }
        Find.WindowStack.Add((Window) new FloatMenu(floatMenuOptionList));
      }
      else
        SoundStarter.PlayOneShotOnCamera(SoundDefOf.ClickReject, (Map) null);
    }
    if (AnimationEditor.Dropdown(rect2, this.comparison.ToString() ?? string.Empty, (string) null))
      Find.WindowStack.Add((Window) new FloatMenu(new List<FloatMenuOption>()
      {
        new FloatMenuOption(ComparisonType.LessThan.ToString(), (Action) (() => this.comparison = ComparisonType.LessThan), (MenuOptionPriority) 4, (Action<Rect>) null, (Thing) null, 0.0f, (Func<Rect, bool>) null, (WorldObject) null, true, 0),
        new FloatMenuOption(ComparisonType.GreaterThan.ToString(), (Action) (() => this.comparison = ComparisonType.GreaterThan), (MenuOptionPriority) 4, (Action<Rect>) null, (Thing) null, 0.0f, (Func<Rect, bool>) null, (WorldObject) null, true, 0)
      }));
    Widgets.TextFieldNumeric<float>(rect1, ref this.value, ref this.inputBuffer, float.MinValue, float.MaxValue);
  }

  private void FieldInt(Rect rect)
  {
  }

  private void FieldBool(Rect rect)
  {
    List<AnimationParameterDef> defsListForReading = DefDatabase<AnimationParameterDef>.AllDefsListForReading;
    Rect rect1;
    // ISSUE: explicit constructor call
    ((Rect) ref rect1).\u002Ector(((Rect) ref rect).xMax - 24f, ((Rect) ref rect).y, 24f, 24f);
    Rect rect2;
    // ISSUE: explicit constructor call
    ((Rect) ref rect2).\u002Ector(rect);
    ((Rect) ref rect2).width = (float) ((double) ((Rect) ref rect).width - (double) ((Rect) ref rect1).width - 5.0);
    Rect rect3 = rect2;
    string label = this.Parameter?.Name;
    if (label == null)
    {
      AnimationParameterDef def = this.Def;
      label = TaggedString.op_Implicit(def != null ? def.LabelCap : TaggedString.op_Implicit("NULL"));
    }
    if (AnimationEditor.Dropdown(rect3, label, (string) null))
    {
      if (!defsListForReading.NullOrEmpty<AnimationParameterDef>())
      {
        List<FloatMenuOption> floatMenuOptionList = new List<FloatMenuOption>();
        foreach (AnimationParameterDef animationParameterDef in defsListForReading)
        {
          AnimationParameterDef paramDef = animationParameterDef;
          floatMenuOptionList.Add(new FloatMenuOption(TaggedString.op_Implicit(paramDef.LabelCap), (Action) (() => this.Parameter = new AnimationParameter(paramDef)), (MenuOptionPriority) 4, (Action<Rect>) null, (Thing) null, 0.0f, (Func<Rect, bool>) null, (WorldObject) null, true, 0));
        }
        Find.WindowStack.Add((Window) new FloatMenu(floatMenuOptionList));
      }
      else
        SoundStarter.PlayOneShotOnCamera(SoundDefOf.ClickReject, (Map) null);
    }
    bool flag = (double) this.value != 0.0;
    UIElements.CheckboxButton(rect1, ref flag);
    this.value = (float) (flag ? 1 : 0);
  }

  public void ResolveParameter()
  {
    if (this.Def != null || this.def.NullOrEmpty<char>())
      return;
    this.paramDef = DefDatabase<AnimationParameterDef>.GetNamed(this.def, true);
    this.Parameter = new AnimationParameter(this.Def);
  }

  internal void ResolveReferences() => this.ResolveParameter();

  public void Export()
  {
    XmlExporter.WriteObject<string>("def", this.def);
    XmlExporter.WriteObject<ComparisonType>("comparison", this.comparison);
    XmlExporter.WriteObject<float>("value", this.value);
  }
}
