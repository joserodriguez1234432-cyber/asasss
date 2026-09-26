// Decompiled with JetBrains decompiler
// Type: SmashTools.Performance.AsyncLongOperationAction
// Assembly: SmashTools, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 3B8999A7-6EA8-49BB-AA7F-3E2E898B5249
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\SmashTools.dll

using JetBrains.Annotations;
using System;

#nullable disable
namespace SmashTools.Performance;

[UsedImplicitly]
public class AsyncLongOperationAction : AsyncAction
{
  public event Action OnInvoke;

  public event Func<bool> OnValidate;

  public override bool LongOperation => true;

  public override bool IsValid
  {
    get
    {
      if (this.OnInvoke == null)
        return false;
      return this.OnValidate == null || this.OnValidate();
    }
  }

  public override void Invoke() => this.OnInvoke();

  public override void ReturnToPool()
  {
    this.OnInvoke = (Action) null;
    this.OnValidate = (Func<bool>) null;
    AsyncPool<AsyncLongOperationAction>.Return(this);
  }
}
