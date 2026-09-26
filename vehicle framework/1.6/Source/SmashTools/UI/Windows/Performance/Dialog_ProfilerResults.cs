// Decompiled with JetBrains decompiler
// Type: SmashTools.Performance.Dialog_ProfilerResults
// Assembly: SmashTools, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 3B8999A7-6EA8-49BB-AA7F-3E2E898B5249
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\SmashTools.dll

using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using UnityEngine;
using Verse;

#nullable enable
namespace SmashTools.Performance;

internal sealed class Dialog_ProfilerResults : Window
{
  private const float DefaultWidth = 400f;
  private const float DefaultHeight = 600f;
  private float maxHeight = 600f;
  private Vector2 scrollPosition;
  private readonly 
  #nullable disable
  List<Dialog_ProfilerResults.Entry> entries = new List<Dialog_ProfilerResults.Entry>();

  public Dialog_ProfilerResults()
    : base((IWindowDrawing) null)
  {
    this.draggable = true;
    this.resizeable = true;
    this.focusWhenOpened = false;
    this.onlyOneOfTypeAllowed = true;
    this.doCloseX = true;
    this.absorbInputAroundWindow = false;
    this.preventCameraMotion = false;
  }

  public virtual Vector2 InitialSize => new Vector2(400f, 600f);

  public virtual void PostOpen()
  {
    base.PostOpen();
    Profiler.Enable();
    CoroutineManager.StartCoroutine(new Func<IEnumerator>(this.UpdateResultsRoutine));
  }

  public virtual void PostClose()
  {
    base.PostClose();
    Profiler.Disable();
  }

  public virtual void DoWindowContents(Rect inRect)
  {
    Rect outRect = inRect;
    Rect rect1 = outRect;
    ((Rect) ref rect1).width = ((Rect) ref outRect).width - 16f;
    ((Rect) ref rect1).height = this.maxHeight;
    Rect viewRect = rect1;
    using (new ScrollViewScope(outRect, ref this.scrollPosition, viewRect, false))
    {
      TextBlock textBlock1;
      // ISSUE: explicit constructor call
      ((TextBlock) ref textBlock1).\u002Ector((GameFont) 1);
      try
      {
        float num1 = 0.0f;
        foreach ((string label, Profiler.Summary summary) in this.entries)
        {
          Rect rect2;
          // ISSUE: explicit constructor call
          ((Rect) ref rect2).\u002Ector(((Rect) ref viewRect).x, ((Rect) ref viewRect).y + num1, ((Rect) ref viewRect).width, 22f);
          Rect rect3;
          Rect rect4;
          float num2;
          GenUI.SplitVerticallyWithMargin(rect2, ref rect3, ref rect4, ref num2, 0.0f, new float?(((Rect) ref rect2).width * 0.8f), new float?());
          Widgets.Label(rect3, label);
          TextBlock textBlock2;
          // ISSUE: explicit constructor call
          ((TextBlock) ref textBlock2).\u002Ector((TextAnchor) 2);
          try
          {
            Widgets.Label(rect4, Ext_Profiler.ToMeasurementString(summary.Average));
            num1 += 22f;
          }
          finally
          {
            textBlock2.Dispose();
          }
        }
        this.maxHeight = num1;
      }
      finally
      {
        textBlock1.Dispose();
      }
    }
  }

  private IEnumerator UpdateResultsRoutine()
  {
    Dialog_ProfilerResults dialogProfilerResults = this;
    while (dialogProfilerResults.IsOpen)
    {
      yield return (object) new WaitForSeconds(1f);
      dialogProfilerResults.UpdateResults();
    }
  }

  private void UpdateResults()
  {
    this.entries.Clear();
    using (IEnumerator<KeyValuePair<string, Profiler.Summary>> results = Profiler.GetResults())
    {
      while (results.MoveNext())
      {
        string label;
        Profiler.Summary summary;
        results.Current.Deconstruct(ref label, ref summary);
        this.entries.Add(new Dialog_ProfilerResults.Entry(label, summary));
      }
      this.entries.Sort();
    }
  }

  private record Entry : IComparable<Dialog_ProfilerResults.Entry>
  {
    public readonly string label;
    public readonly Profiler.Summary summary;

    public Entry(string label, Profiler.Summary summary)
    {
      this.label = label;
      this.summary = summary;
      // ISSUE: explicit constructor call
      base.\u002Ector();
    }

    int IComparable<Dialog_ProfilerResults.Entry>.CompareTo(Dialog_ProfilerResults.Entry other)
    {
      return other?.summary == null ? -1 : other.summary.Average.CompareTo(this.summary.Average);
    }

    [CompilerGenerated]
    protected virtual bool PrintMembers(
    #nullable enable
    StringBuilder builder)
    {
      RuntimeHelpers.EnsureSufficientExecutionStack();
      builder.Append("label = ");
      builder.Append((object) this.label);
      builder.Append(", summary = ");
      builder.Append((object) this.summary);
      return true;
    }

    [CompilerGenerated]
    public override int GetHashCode()
    {
      return (EqualityComparer<Type>.Default.GetHashCode(this.EqualityContract) * -1521134295 + EqualityComparer<string>.Default.GetHashCode(this.label)) * -1521134295 + EqualityComparer<Profiler.Summary>.Default.GetHashCode(this.summary);
    }

    [CompilerGenerated]
    public virtual bool Equals(Dialog_ProfilerResults.Entry? other)
    {
      if ((object) this == (object) other)
        return true;
      return (object) other != null && this.EqualityContract == other.EqualityContract && EqualityComparer<string>.Default.Equals(this.label, other.label) && EqualityComparer<Profiler.Summary>.Default.Equals(this.summary, other.summary);
    }

    [CompilerGenerated]
    protected Entry(Dialog_ProfilerResults.Entry original)
    {
      this.label = original.label;
      this.summary = original.summary;
    }

    [CompilerGenerated]
    public void Deconstruct(out 
    #nullable disable
    string label, out Profiler.Summary summary)
    {
      label = this.label;
      summary = this.summary;
    }
  }
}
