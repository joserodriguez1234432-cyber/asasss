// Decompiled with JetBrains decompiler
// Type: SmashTools.Dialog_Graph
// Assembly: SmashTools, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 3B8999A7-6EA8-49BB-AA7F-3E2E898B5249
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\SmashTools.dll

using System.Collections.Generic;
using UnityEngine;
using Verse;

#nullable disable
namespace SmashTools;

public class Dialog_Graph : Window
{
  private List<CurvePoint> plotPoints;

  public Dialog_Graph(
    Graph.Function function,
    FloatRange range,
    List<CurvePoint> plotPoints = null,
    bool vectorEvaluation = false)
    : base((IWindowDrawing) null)
  {
    this.Function = function;
    this.XRange = range;
    this.YRange = range;
    this.VectorEvaluation = vectorEvaluation;
    this.plotPoints = plotPoints;
  }

  public Dialog_Graph(
    Graph.Function function,
    FloatRange xRange,
    FloatRange yRange,
    List<CurvePoint> plotPoints = null,
    bool vectorEvaluation = false)
    : base((IWindowDrawing) null)
  {
    this.Function = function;
    this.XRange = xRange;
    this.YRange = yRange;
    this.VectorEvaluation = vectorEvaluation;
    this.plotPoints = plotPoints;
  }

  public FloatRange XRange { get; protected set; }

  public FloatRange YRange { get; protected set; }

  public Graph.Function Function { get; protected set; }

  public bool VectorEvaluation { get; protected set; }

  public virtual List<CurvePoint> CurvePoints => this.plotPoints;

  protected virtual bool Editable => false;

  protected virtual bool DrawCoordLabels => true;

  protected virtual float Progress => 0.0f;

  public virtual Vector2 InitialSize
  {
    get
    {
      float num = (float) Mathf.Min(UI.screenWidth, UI.screenHeight);
      return new Vector2(num, num);
    }
  }

  public virtual void DoWindowContents(Rect inRect) => this.DrawGraph(inRect);

  protected virtual void DrawGraph(Rect rect)
  {
    Widgets.DrawMenuSection(rect);
    Graph.DrawGraph(GenUI.ContractedBy(rect, 45f), this.Function, this.XRange, this.YRange, this.CurvePoints, this.Progress, !this.VectorEvaluation, this.Editable, this.DrawCoordLabels);
  }
}
