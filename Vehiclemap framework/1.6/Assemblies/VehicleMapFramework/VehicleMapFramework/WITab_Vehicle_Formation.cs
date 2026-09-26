// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.WITab_Vehicle_Formation
// Assembly: VehicleMapFramework, Version=1.6.562.0, Culture=neutral, PublicKeyToken=null
// MVID: 10A61882-945F-4CFC-9B06-CA8EEF5ADB36
// Assembly location: D:\Programas\steamapps\workshop\content\294100\3426502333\1.6\Assemblies\VehicleMapFramework.dll

using RimWorld;
using RimWorld.Planet;
using SmashTools;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using VehicleMapFramework.VMF_HarmonyPatches;
using Vehicles;
using Vehicles.Rendering;
using Verse;

#nullable disable
namespace VehicleMapFramework;

public class WITab_Vehicle_Formation : WITab
{
  private static readonly Vector2 WinSize = new Vector2(430f, 440f);
  private static readonly Vector2 ViewportSize = new Vector2(400f, 400f);
  private const float Padding = 10f;
  private const float SaveLoadButtonWidth = 180f;
  private const float SaveLoadButtonHeight = 22f;
  public Dictionary<VehiclePawn, VehiclePortrait> portraits;
  private Vector2 scrollPosition;
  private float zoom;
  private VehiclePawn lastDraggedVehicle;
  private Vector2 dragOffset;
  private VehiclePawn curDraggedVehicle;
  private CellRect curDraggedCellRect;
  private int dragAndDropGroup;
  private bool collision;

  public virtual bool IsVisible
  {
    get
    {
      return VehicleMapUtility.VFUnstableRelease.Available && VehicleCaravanHelper.get_Vehicles(this.SelObject).Any<VehiclePawn>((Func<VehiclePawn, bool>) (v => v is VehiclePawnWithMap));
    }
  }

  protected VehicleFormationComp FormationComp
  {
    get => this.SelObject.GetComponent<VehicleFormationComp>();
  }

  public WITab_Vehicle_Formation()
  {
    ((InspectTabBase) this).size = WITab_Vehicle_Formation.WinSize;
    ((InspectTabBase) this).labelKey = "VMF_VehicleFormation";
  }

  public virtual void OnOpen()
  {
    ((InspectTabBase) this).OnOpen();
    Dictionary<VehiclePawn, VehicleFormationComp.DrawData> drawPositions = this.FormationComp?.DrawPositions;
    if (drawPositions == null)
      return;
    CellRect cellRect = CellRect.Empty;
    foreach (VehicleFormationComp.DrawData drawData in drawPositions.Values)
      cellRect = ((CellRect) ref cellRect).Encapsulate(drawData.cellRect);
    Rect rect1;
    // ISSUE: explicit constructor call
    ((Rect) ref rect1).\u002Ector(Vector2.zero, WITab_Vehicle_Formation.WinSize);
    Rect rect2 = GenUI.CenteredOnXIn(GenUI.BottomPartPixels(new Rect(Vector2.zero, new Vector2(WITab_Vehicle_Formation.ViewportSize.x, WITab_Vehicle_Formation.WinSize.y)), WITab_Vehicle_Formation.ViewportSize.y), rect1);
    ref Rect local = ref rect2;
    ((Rect) ref local).y = ((Rect) ref local).y - 10f;
    this.zoom = (float) ((double) Mathf.Min(((Rect) ref rect2).width, ((Rect) ref rect2).height) / (double) Mathf.Max(((CellRect) ref cellRect).Width, ((CellRect) ref cellRect).Height) * 0.800000011920929);
    this.scrollPosition = Vector2.op_Subtraction(Vector2.op_Addition(Vector2Utility.ToVector2(Ext_Unity.MirrorVertical(((CellRect) ref cellRect).CenterVector3)), Vector2.op_Division(Patch_Map_MapUpdate.MeshSize, 2f)), Vector2.op_Division(Vector2.op_Subtraction(Vector2.op_Division(WITab_Vehicle_Formation.ViewportSize, 2f), Vector2.op_Division(((Rect) ref rect2).position, 2f)), this.zoom));
    if (this.portraits == null)
      this.portraits = new Dictionary<VehiclePawn, VehiclePortrait>();
    foreach (VehiclePawn key in drawPositions.Keys)
      this.portraits[key] = new VehiclePortrait();
  }

  protected virtual void FillTab()
  {
    Dictionary<VehiclePawn, VehicleFormationComp.DrawData> drawPositions = this.FormationComp?.DrawPositions;
    if (drawPositions == null)
      return;
    List<VehicleFormationManager.FormationPreset> formationPresets = Find.World.GetComponent<VehicleFormationManager>()?.FormationPresets;
    if (formationPresets != null)
    {
      if (Widgets.ButtonText(new Rect((float) ((double) WITab_Vehicle_Formation.WinSize.x / 2.0 - 2.5 - 180.0), 5f, 180f, 22f), TaggedString.op_Implicit(Translator.Translate("Save")), true, true, true, new TextAnchor?()))
        Find.WindowStack.Add((Window) new Dialog_FormationPresetList(formationPresets, TaggedString.op_Implicit(Translator.Translate("Save")), (Action<int>) (i =>
        {
          formationPresets[i].drawPositions = drawPositions.ToDictionary<KeyValuePair<VehiclePawn, VehicleFormationComp.DrawData>, VehiclePawn, VehicleFormationComp.DrawData>((Func<KeyValuePair<VehiclePawn, VehicleFormationComp.DrawData>, VehiclePawn>) (p => p.Key), (Func<KeyValuePair<VehiclePawn, VehicleFormationComp.DrawData>, VehicleFormationComp.DrawData>) (p => p.Value));
          Messages.Message(TaggedString.op_Implicit(TranslatorFormattedStringExtensions.Translate("SavedAs", NamedArgument.op_Implicit(formationPresets[i].RenamableLabel))), MessageTypeDefOf.NeutralEvent, false);
        }), (Action<string>) (name =>
        {
          formationPresets.Add(new VehicleFormationManager.FormationPreset()
          {
            drawPositions = drawPositions.ToDictionary<KeyValuePair<VehiclePawn, VehicleFormationComp.DrawData>, VehiclePawn, VehicleFormationComp.DrawData>((Func<KeyValuePair<VehiclePawn, VehicleFormationComp.DrawData>, VehiclePawn>) (p => p.Key), (Func<KeyValuePair<VehiclePawn, VehicleFormationComp.DrawData>, VehicleFormationComp.DrawData>) (p => p.Value)),
            RenamableLabel = name
          });
          Messages.Message(TaggedString.op_Implicit(TranslatorFormattedStringExtensions.Translate("SavedAs", NamedArgument.op_Implicit(name))), MessageTypeDefOf.NeutralEvent, false);
        })));
      if (Widgets.ButtonText(new Rect((float) ((double) WITab_Vehicle_Formation.WinSize.x / 2.0 + 2.5), 5f, 180f, 22f), TaggedString.op_Implicit(Translator.Translate("Load")), true, true, true, new TextAnchor?()))
        Find.WindowStack.Add((Window) new Dialog_FormationPresetList(formationPresets, TaggedString.op_Implicit(Translator.Translate("Load")), (Action<int>) (i =>
        {
          drawPositions.Clear();
          List<VehiclePawn> list = VehicleCaravanHelper.get_Vehicles(this.SelObject).ToList<VehiclePawn>();
          foreach (KeyValuePair<VehiclePawn, VehicleFormationComp.DrawData> drawPosition in formationPresets[i].drawPositions)
          {
            VehiclePawn vehiclePawn;
            VehicleFormationComp.DrawData drawData1;
            drawPosition.Deconstruct(ref vehiclePawn, ref drawData1);
            VehiclePawn key = vehiclePawn;
            VehicleFormationComp.DrawData drawData2 = drawData1;
            if (list.Contains(key))
              drawPositions[key] = drawData2;
          }
          this.FormationComp.CenteredDrawPositions();
        })));
    }
    Action action = (Action) null;
    Rect outRect = GenUI.CenteredOnXIn(GenUI.BottomPartPixels(new Rect(Vector2.zero, new Vector2(WITab_Vehicle_Formation.ViewportSize.x, WITab_Vehicle_Formation.WinSize.y)), WITab_Vehicle_Formation.ViewportSize.y), new Rect(Vector2.zero, WITab_Vehicle_Formation.WinSize));
    ref Rect local1 = ref outRect;
    ((Rect) ref local1).y = ((Rect) ref local1).y - 10f;
    Widgets.DrawWindowBackground(outRect, new Color(0.4f, 0.8f, 0.4f));
    Rect viewRect;
    // ISSUE: explicit constructor call
    ((Rect) ref viewRect).\u002Ector(Vector2.zero, Patch_Map_MapUpdate.MeshSize);
    int num = DragAndDropWidget.NewGroup((Action<object, Vector2>) null);
    this.dragAndDropGroup = num == -1 ? this.dragAndDropGroup : num;
    VMF_Widgets.BeginZoomPanArea(outRect, ref this.scrollPosition, ref this.zoom, viewRect, ((Rect) ref outRect).width / ((Rect) ref viewRect).width, 50f, ignoreDragGroup: this.dragAndDropGroup);
    DragAndDropWidget.DropArea(this.dragAndDropGroup, GenUI.AtZero(GenUI.ContractedBy(outRect, 5f)), new Action<object>(this.OnDrop), (object) null);
    Vector2 center = Vector2.op_Division(Patch_Map_MapUpdate.MeshSize, 2f);
    foreach (KeyValuePair<VehiclePawn, VehicleFormationComp.DrawData> keyValuePair1 in drawPositions)
    {
      VehiclePawn vehiclePawn;
      VehicleFormationComp.DrawData drawData3;
      keyValuePair1.Deconstruct(ref vehiclePawn, ref drawData3);
      VehiclePawn key = vehiclePawn;
      VehicleFormationComp.DrawData drawData4 = drawData3;
      VehiclePortrait vehiclePortrait;
      if (!this.portraits.TryGetValue(key, out vehiclePortrait))
        vehiclePortrait = this.portraits[key] = new VehiclePortrait();
      CellRect cellRect1 = drawData4.cellRect;
      Rect contentRect;
      ref Rect local2 = ref contentRect;
      Vector2 vector2_1 = Vector2.op_Addition(center, new Vector2((float) cellRect1.minX, (float) -cellRect1.maxZ));
      IntVec2 size = ((CellRect) ref cellRect1).Size;
      Vector2 vector2_2 = ((IntVec2) ref size).ToVector2();
      // ISSUE: explicit constructor call
      ((Rect) ref local2).\u002Ector(vector2_1, vector2_2);
      Rect gui = VMF_Widgets.ContentToGUI(contentRect);
      if (DragAndDropWidget.Draggable(this.dragAndDropGroup, gui, (object) key, (Action) null, (Action) null))
      {
        Vector2 mousePosition = Event.current.mousePosition;
        if (this.lastDraggedVehicle != key)
        {
          this.lastDraggedVehicle = key;
          this.dragOffset = Vector2.op_Subtraction(mousePosition, ((Rect) ref gui).position);
        }
        Vector2 content = VMF_Widgets.GUIToContent(Vector2.op_Subtraction(mousePosition, this.dragOffset));
        content.x = Mathf.Round(content.x);
        content.y = Mathf.Round(content.y);
        gui = VMF_Widgets.ContentToGUI(new Rect(content, ((Rect) ref contentRect).size));
        this.curDraggedVehicle = key;
        this.curDraggedCellRect = new CellRect(Mathf.RoundToInt(content.x - center.x), Mathf.RoundToInt(center.y - content.y) - ((CellRect) ref cellRect1).Height + 1, ((CellRect) ref cellRect1).Width, ((CellRect) ref cellRect1).Height);
        Rect activeContentRect = new Rect(content, ((Rect) ref contentRect).size);
        action = (Action) (() =>
        {
          WITab_Vehicle_Formation.DrawFadingGrid(activeContentRect);
          this.collision = false;
          foreach (KeyValuePair<VehiclePawn, VehicleFormationComp.DrawData> keyValuePair2 in drawPositions)
          {
            if (keyValuePair2.Key != this.curDraggedVehicle)
            {
              CellRect cellRect2 = keyValuePair2.Value.cellRect;
              if (((CellRect) ref cellRect2).Overlaps(this.curDraggedCellRect))
              {
                CellRect cellRect3 = ((CellRect) ref cellRect2).ClipInsideRect(this.curDraggedCellRect);
                foreach (IntVec3 intVec3 in cellRect3)
                {
                  this.collision = true;
                  Widgets.DrawBoxSolidWithOutline(VMF_Widgets.ContentToGUI(new Rect(Vector2.op_Addition(center, new Vector2((float) intVec3.x, (float) (-intVec3.z + 1))), Vector2.one)), GenColor.WithAlpha(Color.red, 0.2f), GenColor.WithAlpha(Color.red, 0.4f), 1);
                }
              }
            }
          }
        });
      }
      else if (this.lastDraggedVehicle == key && !DragAndDropWidget.Dragging)
        this.lastDraggedVehicle = (VehiclePawn) null;
      BlitRequest request = BlitRequest.For(key);
      request.rot = Rot8.North;
      Rect rect1 = gui;
      ((Rect) ref rect1).size = Vector2.op_Multiply(Vector2.op_Division(((GraphicData) key.VehicleDef.graphicData).drawSize, ((ThingDef) key.VehicleDef).uiIconScale), this.zoom);
      ref Rect local3 = ref rect1;
      Vector2 center1 = ((Rect) ref gui).center;
      Vector2 vector2_3;
      if (key is VehiclePawnWithMap vehiclePawnWithMap)
      {
        CompVehicleDrawOffset vehicleDrawOffset = vehiclePawnWithMap.CompVehicleDrawOffset;
        if (vehicleDrawOffset != null)
        {
          vector2_3 = Vector2.op_Multiply(Vector2Utility.ToVector2(Ext_Unity.MirrorVertical(vehicleDrawOffset.DrawOffsetFull(Rot8.North))), this.zoom);
          goto label_20;
        }
      }
      vector2_3 = Vector2.zero;
label_20:
      Vector2 vector2_4 = Vector2.op_Addition(center1, vector2_3);
      ((Rect) ref local3).center = vector2_4;
      Rect rect2 = rect1;
      vehiclePortrait.Draw(rect2, in request);
    }
    if (action != null)
      action();
    VMF_Widgets.EndZoomPanArea();
  }

  private void OnDrop(object obj)
  {
    if (obj is VehiclePawn key && this.curDraggedVehicle == key && !this.collision)
    {
      this.FormationComp.DrawPositions[key] = new VehicleFormationComp.DrawData(this.curDraggedCellRect, Vector3Utility.SetToAltitude(((CellRect) ref this.curDraggedCellRect).CenterVector3, (AltitudeLayer) 20));
      this.FormationComp.CenteredDrawPositions();
    }
    this.curDraggedVehicle = (VehiclePawn) null;
  }

  protected virtual void CloseTab()
  {
    base.CloseTab();
    this.FormationComp?.CenteredDrawPositions();
    foreach (VehiclePortrait vehiclePortrait in this.portraits.Values)
      vehiclePortrait.Dispose();
    this.portraits.Clear();
  }

  public static void DrawFadingGrid(Rect contentRect, int radius = 4, float maxAlpha = 0.2f)
  {
    if (Event.current.type != 7)
      return;
    int num1 = Mathf.FloorToInt(((Rect) ref contentRect).xMin) - radius;
    int num2 = Mathf.CeilToInt(((Rect) ref contentRect).xMax) + radius;
    int num3 = Mathf.FloorToInt(((Rect) ref contentRect).yMin) - radius;
    int num4 = Mathf.CeilToInt(((Rect) ref contentRect).yMax) + radius;
    for (int index1 = num1; index1 < num2; ++index1)
    {
      for (int index2 = num3; index2 < num4; ++index2)
      {
        Rect contentRect1;
        // ISSUE: explicit constructor call
        ((Rect) ref contentRect1).\u002Ector((float) index1, (float) index2, 1f, 1f);
        if (!((Rect) ref contentRect).Contains(((Rect) ref contentRect1).position))
        {
          Rect gui = VMF_Widgets.ContentToGUI(contentRect1);
          float num5 = 0.0f;
          if ((double) (index1 + 1) < (double) ((Rect) ref contentRect).xMin)
            num5 = ((Rect) ref contentRect).xMin - (float) (index1 + 1);
          else if ((double) index1 > (double) ((Rect) ref contentRect).xMax)
            num5 = (float) index1 - ((Rect) ref contentRect).xMax;
          float num6 = 0.0f;
          if ((double) (index2 + 1) < (double) ((Rect) ref contentRect).yMin)
            num6 = ((Rect) ref contentRect).yMin - (float) (index2 + 1);
          else if ((double) index2 > (double) ((Rect) ref contentRect).yMax)
            num6 = (float) index2 - ((Rect) ref contentRect).yMax;
          Vector2 vector2 = new Vector2(num5, num6);
          float magnitude = ((Vector2) ref vector2).magnitude;
          if ((double) magnitude <= (double) radius)
          {
            float num7 = Mathf.Lerp(maxAlpha, 0.0f, magnitude / (float) radius);
            if ((double) num7 > 1.0 / 1000.0)
            {
              Color color = GUI.color;
              GUI.color = new Color(1f, 1f, 1f, num7);
              Widgets.DrawBox(gui, 1, (Texture2D) null);
              GUI.color = color;
            }
          }
        }
      }
    }
  }
}
