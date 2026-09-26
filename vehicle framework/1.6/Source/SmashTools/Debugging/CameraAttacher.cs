// Decompiled with JetBrains decompiler
// Type: SmashTools.CameraAttacher
// Assembly: SmashTools, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 3B8999A7-6EA8-49BB-AA7F-3E2E898B5249
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\SmashTools.dll

using System;
using UnityEngine;
using Verse;

#nullable disable
namespace SmashTools;

[StaticConstructorOnStartup]
public class CameraAttacher : MonoBehaviour
{
  private Thing thing;
  private static GameObject currentAttacher;

  private void Update()
  {
    if (!this.thing.Spawned || Input.GetKeyDown((KeyCode) 27) || Input.GetMouseButtonDown(1))
    {
      CameraController.Close();
      Object.Destroy((Object) ((Component) this).gameObject);
    }
    else
      CameraController.Update(this.thing.DrawPos);
  }

  public static CameraAttacher Create(Thing thing)
  {
    if (Object.op_Implicit((Object) CameraAttacher.currentAttacher))
    {
      Object.Destroy((Object) CameraAttacher.currentAttacher);
      CameraController.Close();
    }
    CameraController.Start(Find.Camera);
    GameObject gameObject = new GameObject(nameof (CameraAttacher), new Type[1]
    {
      typeof (CameraAttacher)
    });
    CameraAttacher component = gameObject.GetComponent<CameraAttacher>();
    component.thing = thing;
    CameraAttacher.currentAttacher = gameObject;
    return component;
  }
}
