// Decompiled with JetBrains decompiler
// Type: SmashTools.Targeting.WorldTargeter`1
// Assembly: SmashTools, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 3B8999A7-6EA8-49BB-AA7F-3E2E898B5249
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\SmashTools.dll

using RimWorld;
using RimWorld.Planet;
using System.Collections.Generic;
using UnityEngine;
using Verse;
using Verse.Sound;

#nullable disable
namespace SmashTools.Targeting;

public class WorldTargeter<TPayload> : Targeter<GlobalTargetInfo> where TPayload : ITargetOption
{
  private readonly ITargeterSource<GlobalTargetInfo, TPayload> source;
  private GlobalTargetInfo curTarget;
  private TargetValidation curResult;
  private bool closeWorldTabWhenFinished;

  public WorldTargeter(ITargeterSource<GlobalTargetInfo, TPayload> source)
    : base((ITargeterUpdate<GlobalTargetInfo>) null)
  {
    this.source = source;
  }

  public WorldTargeter(
    ITargeterSource<GlobalTargetInfo, TPayload> source,
    ITargeterUpdate<GlobalTargetInfo> updater)
    : base(updater)
  {
    this.source = source;
  }

  public Texture2D TargetTexture { get; init; }

  public override void OnStart()
  {
    this.closeWorldTabWhenFinished = !WorldRendererUtility.WorldRendered;
  }

  public override void OnStop()
  {
    if (!this.closeWorldTabWhenFinished)
      return;
    CameraJumper.TryHideWorld();
  }

  protected override TargeterResult PrimaryClick()
  {
    if (!this.curResult.isValid)
      return TargeterResult.Reject;
    TargeterResult targeterResult = this.source.Select(this.curTarget);
    if (this.targetData.targets.Count > 0)
    {
      GlobalTargetInfo curTarget = this.curTarget;
      List<GlobalTargetInfo> targets = this.targetData.targets;
      GlobalTargetInfo globalTargetInfo = targets[targets.Count - 1];
      if (GlobalTargetInfo.op_Equality(curTarget, globalTargetInfo))
        return TargeterResult.Submit with
        {
          options = targeterResult.options
        };
    }
    bool flag;
    switch (targeterResult.action)
    {
      case TargeterAction.Accept:
      case TargeterAction.Submit:
        flag = true;
        break;
      default:
        flag = false;
        break;
    }
    if (flag)
      this.targetData.targets.Add(this.curTarget);
    return targeterResult;
  }

  protected override TargeterResult SecondaryClick()
  {
    if (this.targetData.targets.NullOrEmpty<GlobalTargetInfo>())
      return base.SecondaryClick();
    GenCollection.Pop<GlobalTargetInfo>(this.targetData.targets);
    return TargeterResult.None;
  }

  protected override void Submit(ITargetOption option)
  {
    SoundStarter.PlayOneShotOnCamera(SoundDefOf.Tick_High, (Map) null);
    this.source.OnTargetingFinished(this.targetData, (TPayload) option);
  }

  public override void OnGUI()
  {
    base.OnGUI();
    string enumerable = TaggedString.op_Implicit(this.curResult.Tooltip);
    if (enumerable.NullOrEmpty<char>())
      return;
    Vector2 mousePosition = Event.current.mousePosition;
    Rect rect1;
    // ISSUE: explicit constructor call
    ((Rect) ref rect1).\u002Ector(mousePosition.x + 8f, mousePosition.y + 8f, 32f, 32f);
    if (Object.op_Implicit((Object) this.TargetTexture))
      GUI.DrawTexture(rect1, (Texture) this.TargetTexture);
    Vector2 vector2 = Text.CalcSize(enumerable);
    Rect rect2;
    // ISSUE: explicit constructor call
    ((Rect) ref rect2).\u002Ector(((Rect) ref rect1).xMax, ((Rect) ref rect1).y, 9999f, 100f);
    Rect rect3;
    // ISSUE: explicit constructor call
    ((Rect) ref rect3).\u002Ector(((Rect) ref rect2).x - vector2.x * 0.1f, ((Rect) ref rect2).y, vector2.x * 1.2f, vector2.y);
    GUI.DrawTexture(rect3, (Texture) TexUI.GrayTextBG);
    Widgets.Label(rect2, enumerable);
  }

  public override void Update()
  {
    if (!this.source.TargeterValid || !WorldRendererUtility.WorldRendered)
    {
      this.Stop();
    }
    else
    {
      base.Update();
      this.UpdateTargetUnderMouse();
    }
  }

  private void UpdateTargetUnderMouse()
  {
    this.curTarget = GlobalTargetInfo.Invalid;
    this.curResult = TargetValidation.Failed;
    List<WorldObject> worldObjectList = GenWorldUI.WorldObjectsUnderMouse(UI.MousePositionOnUI);
    if (worldObjectList.Count > 0)
    {
      foreach (WorldObject worldObject in worldObjectList)
      {
        TargetValidation targetValidation = this.source.CanTarget(GlobalTargetInfo.op_Implicit(worldObject));
        this.curTarget = GlobalTargetInfo.op_Implicit(worldObject);
        this.curResult = targetValidation;
        if (targetValidation.isValid)
          break;
      }
    }
    else
    {
      PlanetTile planetTile = GenWorld.MouseTile(false);
      if (!((PlanetTile) ref planetTile).Valid)
        return;
      this.curTarget = new GlobalTargetInfo(planetTile);
      this.curResult = this.source.CanTarget(this.curTarget);
    }
  }
}
