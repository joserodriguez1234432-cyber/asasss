// Decompiled with JetBrains decompiler
// Type: UpdateLogTool.Dialog_NewUpdate
// Assembly: UpdateLogTool, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: C0B0D1B9-DF61-4E60-8ED1-86C03A6FDDFD
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\UpdateLogTool.dll

using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using Verse.Sound;

#nullable disable
namespace UpdateLogTool;

public class Dialog_NewUpdate : Window
{
  public const float DialogWidth = 700f;
  public const float DialogHeight = 740f;
  public const float Footer = 25f;
  public const float PreviewImageHeight = 200f;
  public const float PaginationButtonHeight = 15f;
  public const int BarIconSize = 25;
  private readonly UpdateLog[] logs;
  private int selectedLogIndex;
  private Vector2 scrollPosition = new Vector2(0.0f, 0.0f);
  private float cachedViewHeight;
  private bool cachedHeightDirty;
  private ModContentPack mod;
  private ModMetaData metaData;
  private UpdateLog log;
  private Listing_Rich lister = new Listing_Rich();
  private List<DescriptionData> segments = new List<DescriptionData>();
  private DescriptionData versionSegment;
  private readonly List<Tuple<string, string, Texture2D>> cachedLeftIconBar = new List<Tuple<string, string, Texture2D>>();
  private readonly List<Tuple<string, string, Texture2D>> cachedRightIconBar = new List<Tuple<string, string, Texture2D>>();

  public Dialog_NewUpdate(HashSet<UpdateLog> logs)
    : base((IWindowDrawing) null)
  {
    if (GenCollection.EnumerableNullOrEmpty<UpdateLog>((IEnumerable<UpdateLog>) logs))
    {
      this.Close(true);
    }
    else
    {
      this.logs = logs.ToArray<UpdateLog>();
      this.selectedLogIndex = 0;
      this.CurrentLog = logs.FirstOrDefault<UpdateLog>();
      this.forcePause = true;
      this.doCloseX = true;
      this.closeOnClickedOutside = true;
      this.absorbInputAroundWindow = true;
    }
  }

  public virtual Vector2 InitialSize => new Vector2(700f, 765f);

  public float DialogHeightFinal => (float) (740.0 - (double) this.Margin / 2.0);

  public UpdateLog CurrentLog
  {
    get => this.log;
    set
    {
      if (this.log == value)
        return;
      if (this.log != (UpdateLog) null)
        this.log.Dispose();
      this.log = value;
      this.mod = this.log.Mod;
      this.metaData = ModLister.GetActiveModWithIdentifier(this.mod.PackageId, false);
      this.segments = EnhancedText.ParseDescriptionData(this.log).ToList<DescriptionData>();
      DescriptionData descriptionData = this.segments.LastOrDefault<DescriptionData>();
      if (descriptionData != null && (GenText.NullOrEmpty(descriptionData.text) || descriptionData.text.Last<char>() != '\n'))
        this.segments.Add(new DescriptionData(Environment.NewLine));
      this.versionSegment = new DescriptionData($"<b>Version {this.CurrentLog.UpdateData.currentVersion}</b>");
      this.log.Open();
      this.RecacheHyperlinks();
      this.cachedHeightDirty = true;
      this.lister.CurrentLog = this.CurrentLog;
    }
  }

  public virtual void PostClose() => this.log.Dispose();

  private void RecacheHyperlinks()
  {
    this.cachedRightIconBar.Clear();
    this.cachedLeftIconBar.Clear();
    if (!GenList.NullOrEmpty<UpdateLog.UpdateLogData.HyperlinkedIcon>((IList<UpdateLog.UpdateLogData.HyperlinkedIcon>) this.CurrentLog.UpdateData.rightIconBar))
    {
      foreach (UpdateLog.UpdateLogData.HyperlinkedIcon hyperlinkedIcon in this.CurrentLog.UpdateData.rightIconBar)
      {
        Texture2D badTex;
        if (!this.CurrentLog.cachedTextures.TryGetValue(hyperlinkedIcon.icon, out badTex))
          badTex = BaseContent.BadTex;
        this.cachedRightIconBar.Add(new Tuple<string, string, Texture2D>(hyperlinkedIcon.name, hyperlinkedIcon.url, badTex));
      }
    }
    if (GenList.NullOrEmpty<UpdateLog.UpdateLogData.HyperlinkedIcon>((IList<UpdateLog.UpdateLogData.HyperlinkedIcon>) this.CurrentLog.UpdateData.leftIconBar))
      return;
    foreach (UpdateLog.UpdateLogData.HyperlinkedIcon hyperlinkedIcon in this.CurrentLog.UpdateData.leftIconBar)
    {
      Texture2D badTex;
      if (!this.CurrentLog.cachedTextures.TryGetValue(hyperlinkedIcon.icon, out badTex))
        badTex = BaseContent.BadTex;
      this.cachedLeftIconBar.Add(new Tuple<string, string, Texture2D>(hyperlinkedIcon.name, hyperlinkedIcon.url, badTex));
    }
  }

  private void RecacheHeight(Rect rect)
  {
    float num1 = 0.0f;
    GameFont font = Text.Font;
    TextAnchor anchor = Text.Anchor;
    Color color = GUI.color;
    foreach (DescriptionData segment in this.segments)
    {
      TaggedSegment tag = segment.tag;
      if (tag != null)
        num1 += (float) tag.HeightOccupied(this.CurrentLog, segment.text);
      else
        num1 += Text.CalcHeight(segment.text, ((Rect) ref rect).width);
    }
    float num2 = num1 + Text.CalcHeight("BottomPadding", ((Rect) ref rect).width);
    GUI.color = color;
    Text.Anchor = anchor;
    Text.Font = font;
    this.cachedViewHeight = num2;
  }

  public virtual void DoWindowContents(Rect inRect)
  {
    if (this.cachedHeightDirty)
    {
      this.RecacheHeight(inRect);
      this.cachedHeightDirty = false;
    }
    TextAnchor anchor = Text.Anchor;
    Text.Anchor = (TextAnchor) 4;
    GameFont font = Text.Font;
    Text.Font = (GameFont) 2;
    Color color = GUI.color;
    Texture2D previewImage = this.metaData.PreviewImage;
    float num = (float) ((previewImage != null ? (double) ((Texture) previewImage).width : 0.0) / (previewImage != null ? (double) ((Texture) previewImage).height : 0.0) * 200.0);
    Rect rect1;
    // ISSUE: explicit constructor call
    ((Rect) ref rect1).\u002Ector(inRect);
    ((Rect) ref rect1).x = (float) (((double) ((Rect) ref inRect).width - (double) num) / 2.0);
    ((Rect) ref rect1).height = 200f;
    ((Rect) ref rect1).width = num;
    Rect rect2 = rect1;
    if (Object.op_Inequality((Object) previewImage, (Object) null))
      GUI.DrawTexture(rect2, (Texture) previewImage);
    // ISSUE: explicit constructor call
    ((Rect) ref rect1).\u002Ector(inRect);
    ((Rect) ref rect1).y = ((Rect) ref rect2).yMax + 5f;
    ((Rect) ref rect1).height = Text.CalcHeight(this.mod.Name, ((Rect) ref inRect).width);
    Rect rect3 = rect1;
    Widgets.Label(rect3, this.mod.Name);
    Widgets.DrawLineHorizontal(0.0f, ((Rect) ref rect3).yMax, ((Rect) ref rect3).width);
    Rect rect4;
    // ISSUE: explicit constructor call
    ((Rect) ref rect4).\u002Ector(((Rect) ref inRect).width - 25f, ((Rect) ref rect3).y, 25f, 25f);
    foreach (Tuple<string, string, Texture2D> tuple in this.cachedRightIconBar)
    {
      if (!GenText.NullOrEmpty(tuple.Item2))
      {
        if (Mouse.IsOver(rect4))
        {
          GUI.color = GenUI.MouseoverColor;
          if (!GenText.NullOrEmpty(tuple.Item1))
            TooltipHandler.TipRegion(rect4, TipSignal.op_Implicit(tuple.Item1));
        }
        if (Widgets.ButtonInvisible(rect4, true))
          Application.OpenURL(tuple.Item2);
      }
      Widgets.DrawTextureFitted(rect4, (Texture) tuple.Item3, 1f, 1f);
      ref Rect local = ref rect4;
      ((Rect) ref local).x = ((Rect) ref local).x - 35f;
      GUI.color = color;
    }
    Rect rect5;
    // ISSUE: explicit constructor call
    ((Rect) ref rect5).\u002Ector(0.0f, ((Rect) ref rect3).y, 25f, 25f);
    foreach (Tuple<string, string, Texture2D> tuple in this.cachedLeftIconBar)
    {
      if (!GenText.NullOrEmpty(tuple.Item2))
      {
        if (Mouse.IsOver(rect5))
        {
          GUI.color = GenUI.MouseoverColor;
          if (!GenText.NullOrEmpty(tuple.Item1))
            TooltipHandler.TipRegion(rect5, TipSignal.op_Implicit(tuple.Item1));
        }
        if (Widgets.ButtonInvisible(rect5, true))
          Application.OpenURL(tuple.Item2);
      }
      Widgets.DrawTextureFitted(rect5, (Texture) tuple.Item3, 1f, 1f);
      ref Rect local = ref rect5;
      ((Rect) ref local).x = ((Rect) ref local).x + 35f;
      GUI.color = color;
    }
    Rect rect6;
    // ISSUE: explicit constructor call
    ((Rect) ref rect6).\u002Ector(((Rect) ref rect3).x, ((Rect) ref rect3).yMax, ((Rect) ref inRect).width, (float) ((double) this.DialogHeightFinal - (double) ((Rect) ref rect3).yMax - 25.0));
    Rect rect7;
    // ISSUE: explicit constructor call
    ((Rect) ref rect7).\u002Ector(((Rect) ref rect6).x, ((Rect) ref rect6).y, ((Rect) ref rect6).width - 20f, Mathf.Max(this.cachedViewHeight, ((Rect) ref rect6).height));
    Widgets.BeginScrollView(rect6, ref this.scrollPosition, rect7, true);
    ((Listing) this.lister).Begin(rect7);
    Text.Anchor = (TextAnchor) 4;
    this.lister.RichText(this.versionSegment);
    Text.Anchor = (TextAnchor) 3;
    foreach (DescriptionData segment in this.segments)
    {
      TaggedSegment tag = segment.tag;
      if (tag != null)
        tag.SegmentAction(this.lister, segment.text);
      else
        this.lister.RichText(segment);
    }
    ((Listing) this.lister).End();
    Widgets.EndScrollView();
    Rect rect8;
    // ISSUE: explicit constructor call
    ((Rect) ref rect8).\u002Ector(((Rect) ref rect6).x, ((Rect) ref rect6).yMax, ((Rect) ref inRect).width, 25f);
    Text.Font = (GameFont) 1;
    if (Dialog_NewUpdate.DrawPagination(rect8, ref this.selectedLogIndex, this.logs.Length))
      this.CurrentLog = this.logs[this.selectedLogIndex];
    Text.Anchor = anchor;
    Text.Font = font;
    GUI.color = color;
  }

  public static bool DrawPagination(Rect rect, ref int pageNumber, int pageCount)
  {
    ++pageNumber;
    GameFont font = Text.Font;
    TextAnchor anchor = Text.Anchor;
    Color color = GUI.color;
    Text.Font = (GameFont) 1;
    Text.Anchor = (TextAnchor) 4;
    string str1 = "Previous";
    string str2 = "Next";
    bool flag = false;
    Rect rect1;
    // ISSUE: explicit constructor call
    ((Rect) ref rect1).\u002Ector(((Rect) ref rect).x, ((Rect) ref rect).y, Text.CalcSize(str1).x, ((Rect) ref rect).height);
    float x = Text.CalcSize(str2).x;
    Rect rect2;
    // ISSUE: explicit constructor call
    ((Rect) ref rect2).\u002Ector(((Rect) ref rect).xMax - x, ((Rect) ref rect).y, x, ((Rect) ref rect).height);
    if (Mouse.IsOver(rect1))
      GUI.color = GenUI.MouseoverColor;
    if (pageCount > 1)
    {
      Widgets.Label(rect1, str1);
      if (Widgets.ButtonInvisible(rect1, true))
      {
        SoundStarter.PlayOneShotOnCamera(SoundDefOf.Click, (Map) null);
        --pageNumber;
        pageNumber = Mathf.Clamp(pageNumber, 1, pageCount);
        flag = true;
      }
    }
    GUI.color = color;
    if (Mouse.IsOver(rect2))
      GUI.color = GenUI.MouseoverColor;
    if (pageCount > 1)
    {
      Widgets.Label(rect2, str2);
      if (Widgets.ButtonInvisible(rect2, true))
      {
        SoundStarter.PlayOneShotOnCamera(SoundDefOf.Click, (Map) null);
        ++pageNumber;
        pageNumber = Mathf.Clamp(pageNumber, 1, pageCount);
        flag = true;
      }
    }
    GUI.color = color;
    float num1 = ((Rect) ref rect).width - ((Rect) ref rect).height * 2f;
    int num2 = Mathf.CeilToInt(num1 / 1.5f / ((Rect) ref rect).width);
    int num3 = Mathf.FloorToInt((float) num2 / 2f);
    float num4 = (float) ((double) ((Rect) ref rect).x + (double) ((Rect) ref rect).height + (double) num1 / 2.0);
    Rect rect3;
    // ISSUE: explicit constructor call
    ((Rect) ref rect3).\u002Ector(num4, ((Rect) ref rect).y, ((Rect) ref rect).height, ((Rect) ref rect).height);
    Widgets.ButtonText(rect3, pageNumber.ToString(), false, true, true, new TextAnchor?());
    Text.Font = (GameFont) 0;
    int num5 = 1;
    int num6 = pageNumber + 1;
    while (num6 <= pageNumber + num3 && num6 <= pageCount)
    {
      ((Rect) ref rect3).x = num4 + num1 / (float) num2 * (float) num5;
      if (Widgets.ButtonText(rect3, num6.ToString(), false, true, true, new TextAnchor?()))
      {
        SoundStarter.PlayOneShotOnCamera(SoundDefOf.Click, (Map) null);
        pageNumber = num6;
        flag = true;
      }
      ++num6;
      ++num5;
    }
    int num7 = 1;
    int num8 = pageNumber - 1;
    while (num8 >= pageNumber - num3 && num8 >= 1)
    {
      ((Rect) ref rect3).x = num4 - num1 / (float) num2 * (float) num7;
      if (Widgets.ButtonText(rect3, num8.ToString(), false, true, true, new TextAnchor?()))
      {
        SoundStarter.PlayOneShotOnCamera(SoundDefOf.Click, (Map) null);
        pageNumber = num8;
        flag = true;
      }
      --num8;
      ++num7;
    }
    Text.Font = font;
    Text.Anchor = anchor;
    --pageNumber;
    return flag;
  }
}
