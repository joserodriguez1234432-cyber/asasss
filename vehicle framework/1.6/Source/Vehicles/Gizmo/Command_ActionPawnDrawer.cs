// Decompiled with JetBrains decompiler
// Type: Vehicles.Command_ActionPawnDrawer
// Assembly: Vehicles, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 81BFDC99-D8AD-42E1-8470-2F73E7836B4A
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\Vehicles.dll

using RimWorld;
using System.Collections.Generic;
using UnityEngine;
using Verse;

#nullable disable
namespace Vehicles;

public class Command_ActionPawnDrawer : Command_Action
{
  public Pawn pawn;

  public virtual void DrawIcon(Rect rect, Material buttonMat, GizmoRenderParms parms)
  {
    if (this.pawn.Dead)
      return;
    Rect rect1;
    // ISSUE: explicit constructor call
    ((Rect) ref rect1).\u002Ector(rect);
    Vector2 vector2 = Vector2.op_Multiply(ColonistBarColonistDrawer.PawnTextureSize, ((Command) this).iconDrawScale);
    rect = GenUI.ContractedBy(new Rect((float) ((double) ((Rect) ref rect).x + (double) ((Rect) ref rect).width / 2.0 - (double) vector2.x / 2.0), (float) ((double) ((Rect) ref rect).y + (double) ((Rect) ref rect).height / 2.0 - (double) vector2.y / 1.75), vector2.x, vector2.y), 1f);
    GUI.DrawTexture(rect, (Texture) PortraitsCache.Get(this.pawn, ColonistBarColonistDrawer.PawnTextureSize, Rot4.South, ColonistBarColonistDrawer.PawnTextureCameraOffset, 1.28205f, true, true, true, true, (IReadOnlyDictionary<Apparel, Color>) null, new Color?(), false, new PawnHealthState?()));
    Widgets.DrawTextureFitted(rect1, (Texture) VehicleTex.UnloadIcon, 1f, 1f);
  }
}
