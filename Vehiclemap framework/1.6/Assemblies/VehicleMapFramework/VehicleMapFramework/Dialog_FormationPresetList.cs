// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.Dialog_FormationPresetList
// Assembly: VehicleMapFramework, Version=1.6.562.0, Culture=neutral, PublicKeyToken=null
// MVID: 10A61882-945F-4CFC-9B06-CA8EEF5ADB36
// Assembly location: D:\Programas\steamapps\workshop\content\294100\3426502333\1.6\Assemblies\VehicleMapFramework.dll

using RimWorld;
using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;

#nullable disable
namespace VehicleMapFramework;

public sealed class Dialog_FormationPresetList : Window
{
  private readonly List<VehicleFormationManager.FormationPreset> formationPresets;
  private readonly Action<int> interactAction;
  private readonly Action<string> newSaveAction;
  private readonly string interactButLabel;
  private readonly QuickSearchWidget search = new QuickSearchWidget();
  private string typingName = "";
  private bool focusedSearch;
  private bool focusedNameArea;
  private Vector2 scrollPosition;
  private const float EntryHeight = 40f;
  private const float NameLeftMargin = 8f;
  private const float NameRightMargin = 4f;
  private const float InteractButWidth = 100f;
  private const float InteractButHeight = 36f;
  private const float ButtonHeight = 36f;
  private const float BottomAreaHeight = 55f;
  private const float NameTextFieldWidth = 400f;
  private const float NameTextFieldHeight = 35f;
  private const float NameTextFieldButtonSpace = 20f;

  public virtual Vector2 InitialSize => new Vector2(620f, 700f);

  private static bool FocusSearchField => false;

  public Dialog_FormationPresetList(
    List<VehicleFormationManager.FormationPreset> formationPresets,
    string interactButLabel,
    Action<int> interactAction,
    Action<string> newSaveAction = null)
    : base((IWindowDrawing) null)
  {
    this.doCloseButton = true;
    this.doCloseX = true;
    this.forcePause = true;
    this.absorbInputAroundWindow = true;
    this.closeOnAccept = false;
    this.formationPresets = formationPresets;
    this.interactAction = interactAction;
    this.interactButLabel = interactButLabel;
    this.newSaveAction = newSaveAction;
  }

  public virtual void DoWindowContents(Rect inRect)
  {
    Vector2 vector2;
    // ISSUE: explicit constructor call
    ((Vector2) ref vector2).\u002Ector(((Rect) ref inRect).width - 16f, 40f);
    float y = vector2.y;
    float num1 = (float) this.FilesMatchingFilter() * y;
    Rect rect1;
    // ISSUE: explicit constructor call
    ((Rect) ref rect1).\u002Ector(0.0f, 0.0f, ((Rect) ref inRect).width - 16f, num1);
    Rect rect2 = GenUI.LeftHalf(inRect);
    ((Rect) ref rect2).height = Text.LineHeight;
    this.search.OnGUI(rect2, (Action) null, (Action) null);
    if (!this.focusedSearch && Dialog_FormationPresetList.FocusSearchField)
    {
      this.focusedSearch = true;
      this.search.Focus();
    }
    Rect rect3 = inRect;
    ((Rect) ref rect3).yMin = ((Rect) ref rect2).yMax + 10f;
    ref Rect local1 = ref rect3;
    ((Rect) ref local1).yMax = ((Rect) ref local1).yMax - (float) ((double) Window.CloseButSize.y + 55.0 + 10.0);
    bool flag = this.newSaveAction != null;
    if (flag)
    {
      ref Rect local2 = ref rect3;
      ((Rect) ref local2).yMax = ((Rect) ref local2).yMax - 53f;
    }
    Widgets.BeginScrollView(rect3, ref this.scrollPosition, rect1, true);
    float num2 = 0.0f;
    for (int index = 0; index < this.formationPresets.Count; ++index)
    {
      VehicleFormationManager.FormationPreset formationPreset = this.formationPresets[index];
      if (this.search.filter.Matches(formationPreset.InspectLabel))
      {
        if ((double) num2 + (double) vector2.y >= (double) this.scrollPosition.y && (double) num2 <= (double) this.scrollPosition.y + (double) ((Rect) ref rect3).height)
        {
          Rect rect4;
          // ISSUE: explicit constructor call
          ((Rect) ref rect4).\u002Ector(0.0f, num2, vector2.x, vector2.y);
          if (index % 2 == 1)
            Widgets.DrawAltRect(rect4);
          Widgets.BeginGroup(rect4);
          Rect rect5;
          // ISSUE: explicit constructor call
          ((Rect) ref rect5).\u002Ector(((Rect) ref rect4).width - 36f, (float) (((double) ((Rect) ref rect4).height - 36.0) / 2.0), 36f, 36f);
          if (Widgets.ButtonImage(rect5, TexButton.Delete, Color.white, GenUI.SubtleMouseoverColor, true, (string) null))
          {
            int num = index;
            Find.WindowStack.Add((Window) Dialog_MessageBox.CreateConfirmation(TranslatorFormattedStringExtensions.Translate("ConfirmDelete", NamedArgument.op_Implicit(formationPreset.RenamableLabel)), (Action) (() => this.formationPresets.RemoveAt(num)), true, (string) null, (WindowLayer) 1));
          }
          Rect rect6;
          // ISSUE: explicit constructor call
          ((Rect) ref rect6).\u002Ector(((Rect) ref rect5).x - 36f, (float) (((double) ((Rect) ref rect4).height - 36.0) / 2.0), 36f, 36f);
          if (Widgets.ButtonImage(rect6, TexButton.Rename, Color.white, GenUI.SubtleMouseoverColor, true, (string) null))
            Find.WindowStack.Add((Window) new VehicleFormationManager.FormationPreset.Dialog_RenameFormationPreset(this.formationPresets[index]));
          Text.Font = (GameFont) 1;
          Rect rect7;
          // ISSUE: explicit constructor call
          ((Rect) ref rect7).\u002Ector(((Rect) ref rect6).x - 100f, (float) (((double) ((Rect) ref rect4).height - 36.0) / 2.0), 100f, 36f);
          if (Widgets.ButtonText(rect7, this.interactButLabel, true, true, true, new TextAnchor?()))
            this.interactAction(index);
          GUI.color = Color.white;
          Text.Anchor = (TextAnchor) 0;
          GUI.color = new Color(1f, 1f, 0.6f);
          Rect rect8;
          // ISSUE: explicit constructor call
          ((Rect) ref rect8).\u002Ector(8f, 0.0f, (float) ((double) ((Rect) ref rect7).x - 8.0 - 4.0), ((Rect) ref rect4).height);
          Text.Anchor = (TextAnchor) 3;
          Text.Font = (GameFont) 1;
          Widgets.Label(rect8, GenText.Truncate(formationPreset.InspectLabel, ((Rect) ref rect8).width * 1.8f, (Dictionary<string, string>) null));
          GUI.color = Color.white;
          Text.Anchor = (TextAnchor) 0;
          Widgets.EndGroup();
        }
        num2 += vector2.y;
      }
    }
    Widgets.EndScrollView();
    if (!flag)
      return;
    this.DoTypeInField(GenUI.TopPartPixels(inRect, (float) ((double) ((Rect) ref inRect).height - (double) Window.CloseButSize.y - 18.0)));
  }

  private void DoTypeInField(Rect rect)
  {
    Widgets.BeginGroup(rect);
    bool flag = Event.current.type == 4 && Event.current.keyCode == 13;
    float num = ((Rect) ref rect).height - 35f;
    Text.Font = (GameFont) 1;
    Text.Anchor = (TextAnchor) 3;
    GUI.SetNextControlName("FormationNameField");
    string str = Widgets.TextField(new Rect(5f, num, 400f, 35f), this.typingName);
    if (GenText.IsValidFilename(str))
      this.typingName = str;
    if (!this.focusedNameArea)
    {
      UI.FocusControl("FormationNameField", (Window) this);
      this.focusedNameArea = true;
    }
    if (Widgets.ButtonText(new Rect(420f, num, (float) ((double) ((Rect) ref rect).width - 400.0 - 20.0), 35f), TaggedString.op_Implicit(Translator.Translate("Save")), true, true, true, new TextAnchor?()) | flag)
    {
      if (GenText.NullOrEmpty(this.typingName))
        Messages.Message(TaggedString.op_Implicit(Translator.Translate("NeedAName")), MessageTypeDefOf.RejectInput, false);
      else
        this.newSaveAction(this.typingName?.Trim());
    }
    Text.Anchor = (TextAnchor) 0;
    Widgets.EndGroup();
  }

  private int FilesMatchingFilter()
  {
    return GenCollection.Count<VehicleFormationManager.FormationPreset>(this.formationPresets, (Predicate<VehicleFormationManager.FormationPreset>) (p => this.search.filter.Matches(p.InspectLabel)));
  }
}
