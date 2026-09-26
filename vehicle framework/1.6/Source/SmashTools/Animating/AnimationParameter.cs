// Decompiled with JetBrains decompiler
// Type: SmashTools.Animations.AnimationParameter
// Assembly: SmashTools, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 3B8999A7-6EA8-49BB-AA7F-3E2E898B5249
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\SmashTools.dll

using RimWorld;
using SmashTools.Xml;
using System;
using UnityEngine;
using Verse;
using Verse.Sound;

#nullable disable
namespace SmashTools.Animations;

public class AnimationParameter : IXmlExport
{
  private const float ContractedBy = 2f;
  public AnimationParameterDef def;
  private float value;
  private string inputBuffer;

  public AnimationParameter(AnimationParameterDef def) => this.def = def;

  public ushort Id => this.def.shortHash;

  public AnimationParameter.ParamType Type => this.def.type;

  public string Name => TaggedString.op_Implicit(this.def.LabelCap);

  public float Value
  {
    get => this.value;
    internal set => this.value = value;
  }

  public void DrawInput(Rect rect)
  {
    switch (this.def.type)
    {
      case AnimationParameter.ParamType.Float:
        this.DrawFloatInput(rect);
        break;
      case AnimationParameter.ParamType.Int:
        this.DrawIntInput(rect);
        break;
      case AnimationParameter.ParamType.Bool:
        this.DrawBoolInput(rect);
        break;
      case AnimationParameter.ParamType.Trigger:
        this.DrawTriggerInput(rect);
        break;
      default:
        throw new NotImplementedException("ParamType");
    }
  }

  private void DrawFloatInput(Rect rect)
  {
    Widgets.TextFieldNumeric<float>(rect, ref this.value, ref this.inputBuffer, float.MinValue, float.MaxValue);
  }

  private void DrawIntInput(Rect rect)
  {
    Widgets.TextFieldNumeric<float>(rect, ref this.value, ref this.inputBuffer, (float) int.MinValue, (float) int.MaxValue);
  }

  private void DrawBoolInput(Rect rect)
  {
    bool flag = (double) this.value != 0.0;
    Widgets.Checkbox(((Rect) ref rect).position, ref flag, ((Rect) ref rect).height - 4f, false, false, (Texture2D) null, (Texture2D) null);
    this.value = (float) (flag ? 1 : 0);
  }

  private void DrawTriggerInput(Rect rect)
  {
    bool flag = (double) this.value != 0.0;
    Texture2D texture2D = flag ? Widgets.RadioButOnTex : UIData.RadioButOffTex;
    Rect rect1 = GenUI.ContractedBy(new Rect(((Rect) ref rect).x, ((Rect) ref rect).y, ((Rect) ref rect).height, ((Rect) ref rect).height), 2f);
    Color color = GUI.color;
    if (!GUI.enabled)
      GUI.color = Color.gray;
    GUI.DrawTexture(rect1, (Texture) texture2D);
    if (Widgets.ButtonInvisible(rect1, true))
    {
      this.value = (float) (!flag ? 1 : 0);
      SoundStarter.PlayOneShotOnCamera(SoundDefOf.Tick_Tiny, (Map) null);
    }
    GUI.color = color;
  }

  void IXmlExport.Export()
  {
    XmlExporter.WriteObject<AnimationParameterDef>("def", this.def);
    XmlExporter.WriteObject<float>("value", this.value);
  }

  public enum ParamType
  {
    Float,
    Int,
    Bool,
    Trigger,
  }
}
