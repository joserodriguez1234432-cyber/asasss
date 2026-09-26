// Decompiled with JetBrains decompiler
// Type: Vehicles.ITab_Airdrop_Container
// Assembly: Vehicles, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 81BFDC99-D8AD-42E1-8470-2F73E7836B4A
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\Vehicles.dll

using RimWorld;
using RimWorld.Planet;
using System.Collections.Generic;
using UnityEngine;
using Verse;
using Verse.Sound;

#nullable disable
namespace Vehicles;

public class ITab_Airdrop_Container : ITab
{
  protected const float TopPadding = 20f;
  protected const float ThingIconSize = 28f;
  protected const float ThingRowHeight = 28f;
  protected const float ThingDropButtonSize = 24f;
  protected const float ThingLeftX = 36f;
  protected const float StandardLineHeight = 22f;
  protected Vector2 scrollPosition = Vector2.zero;
  protected float scrollViewHeight;
  public static readonly Color ThingLabelColor = new Color(0.9f, 0.9f, 0.9f, 1f);
  public static readonly Color HighlightColor = new Color(0.5f, 0.5f, 0.5f, 1f);
  public static readonly Color MissingItemColor = new Color(1f, 0.0f, 0.1f, 0.75f);
  protected static List<Thing> workingInvList = new List<Thing>();

  public ITab_Airdrop_Container()
  {
    ((InspectTabBase) this).size = new Vector2(300f, 480f);
    ((InspectTabBase) this).labelKey = "TabStorage";
    ((InspectTabBase) this).tutorTag = "Storage";
  }

  protected virtual string InventoryLabelKey => "TabStorage";

  public virtual bool IsVisible => this.Inventory != null;

  protected virtual bool AllowDropping => false;

  protected virtual ThingOwner Inventory
  {
    get
    {
      if (this.SelThing is Pawn selThing1)
        return (ThingOwner) selThing1.inventory.innerContainer;
      return this.SelThing is IThingHolder selThing2 ? selThing2.GetDirectlyHeldThings() : (ThingOwner) null;
    }
  }

  protected virtual void FillTab()
  {
    TextBlock textBlock;
    // ISSUE: explicit constructor call
    ((TextBlock) ref textBlock).\u002Ector((GameFont) 1);
    try
    {
      Text.Font = (GameFont) 1;
      Rect rect1 = GenUI.ContractedBy(new Rect(0.0f, 20f, ((InspectTabBase) this).size.x, ((InspectTabBase) this).size.y - 20f), 10f);
      Rect rect2 = new Rect(((Rect) ref rect1).x, ((Rect) ref rect1).y, ((Rect) ref rect1).width, ((Rect) ref rect1).height);
      GUI.color = Color.white;
      Rect rect3 = new Rect(0.0f, 0.0f, ((Rect) ref rect2).width, ((Rect) ref rect2).height);
      Rect rect4 = new Rect(0.0f, 0.0f, ((Rect) ref rect2).width - 16f, this.scrollViewHeight);
      Widgets.BeginGroup(rect2);
      Widgets.BeginScrollView(rect3, ref this.scrollPosition, rect4, true);
      float num = 0.0f;
      this.DrawHeader(ref num, ((Rect) ref rect4).width);
      if (((InspectTabBase) this).IsVisible)
      {
        Widgets.ListSeparator(ref num, ((Rect) ref rect4).width, TaggedString.op_Implicit(Translator.Translate(this.InventoryLabelKey)));
        ITab_Airdrop_Container.workingInvList.Clear();
        ITab_Airdrop_Container.workingInvList.AddRange((IEnumerable<Thing>) this.Inventory);
        foreach (Thing workingInv in ITab_Airdrop_Container.workingInvList)
          this.DrawThingRow(ref num, ((Rect) ref rect4).width, workingInv, inventory: true);
        ITab_Airdrop_Container.workingInvList.Clear();
      }
      if (((InspectTabBase) this).IsVisible)
        this.DrawAdditionalRows(ref num, rect4);
      if (Event.current.type == 8)
        this.scrollViewHeight = num + 30f;
      Widgets.EndScrollView();
      Widgets.EndGroup();
    }
    finally
    {
      textBlock.Dispose();
    }
  }

  protected virtual void DrawThingRow(
    ref float y,
    float width,
    Thing thing,
    int? transferStackCount = null,
    bool inventory = false,
    bool missingFromInventory = false)
  {
    Rect rect1;
    // ISSUE: explicit constructor call
    ((Rect) ref rect1).\u002Ector(0.0f, y, width, 28f);
    TextBlock textBlock1;
    // ISSUE: explicit constructor call
    ((TextBlock) ref textBlock1).\u002Ector(Color.white);
    try
    {
      if (missingFromInventory)
        GUI.color = ITab_Airdrop_Container.MissingItemColor;
      Widgets.InfoCardButton(((Rect) ref rect1).width - 24f, y, thing);
      ref Rect local1 = ref rect1;
      ((Rect) ref local1).width = ((Rect) ref local1).width - 24f;
      if (inventory && this.AllowDropping && this.SelThing.Spawned)
      {
        Rect rect2;
        // ISSUE: explicit constructor call
        ((Rect) ref rect2).\u002Ector(((Rect) ref rect1).xMax - 24f, y, 24f, 24f);
        TooltipHandler.TipRegion(rect2, TipSignal.op_Implicit(Translator.Translate("DropThing")));
        if (Widgets.ButtonImage(rect2, TexData.Drop, true, (string) null))
        {
          SoundStarter.PlayOneShotOnCamera(SoundDefOf.Tick_High, (Map) null);
          this.InterfaceDrop(thing);
        }
        ref Rect local2 = ref rect1;
        ((Rect) ref local2).width = ((Rect) ref local2).width - 24f;
      }
      Rect rect3 = rect1;
      ((Rect) ref rect3).xMin = ((Rect) ref rect3).xMax - 60f;
      CaravanThingsTabUtility.DrawMass(thing, rect3);
      ref Rect local3 = ref rect1;
      ((Rect) ref local3).width = ((Rect) ref local3).width - 60f;
    }
    finally
    {
      textBlock1.Dispose();
    }
    TextBlock textBlock2;
    // ISSUE: explicit constructor call
    ((TextBlock) ref textBlock2).\u002Ector(Color.white);
    try
    {
      if (Mouse.IsOver(rect1))
      {
        GUI.color = ITab_Airdrop_Container.HighlightColor;
        GUI.DrawTexture(rect1, (Texture) TexUI.HighlightTex);
      }
      if (((BuildableDef) thing.def).DrawMatSingle != null && ((BuildableDef) thing.def).DrawMatSingle.mainTexture != null)
        Widgets.ThingIcon(new Rect(4f, y, 28f, 28f), thing, 1f, new Rot4?(), false, 1f, false);
      Text.Anchor = (TextAnchor) 3;
      GUI.color = missingFromInventory ? ITab_Airdrop_Container.MissingItemColor : ITab_Airdrop_Container.ThingLabelColor;
      Rect rect4;
      // ISSUE: explicit constructor call
      ((Rect) ref rect4).\u002Ector(36f, y, ((Rect) ref rect1).width - 36f, ((Rect) ref rect1).height);
      string empty = string.Empty;
      string str1 = !transferStackCount.HasValue ? ((Entity) thing).LabelCap : $"{thing.LabelCapNoCount} x{GenString.ToStringCached(transferStackCount.Value)}";
      Text.WordWrap = false;
      Widgets.Label(rect4, GenText.Truncate(str1, ((Rect) ref rect4).width, (Dictionary<string, string>) null));
      Text.WordWrap = true;
      string str2 = thing.DescriptionDetailed;
      if (thing.def.useHitPoints)
        str2 = $"{str2}\n{(object) thing.HitPoints} / {(object) thing.MaxHitPoints}";
      TooltipHandler.TipRegion(rect1, TipSignal.op_Implicit(str2));
      y += 28f;
    }
    finally
    {
      textBlock2.Dispose();
    }
  }

  protected virtual void DrawAdditionalRows(ref float y, Rect rect)
  {
  }

  protected virtual void DrawHeader(ref float curY, float width)
  {
  }

  protected virtual bool InterfaceDrop(Thing thing)
  {
    return this.Inventory.TryDropOutsideVehicle(thing, this.SelThing.Map, GenAdj.OccupiedRect(this.SelThing), (DestroyMode) 7);
  }

  protected virtual bool InterfaceDropAll()
  {
    return this.Inventory.TryDropAllOutsideVehicle(this.SelThing.Map, GenAdj.OccupiedRect(this.SelThing), (DestroyMode) 7);
  }
}
