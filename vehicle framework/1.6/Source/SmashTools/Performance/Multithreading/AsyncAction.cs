// Decompiled with JetBrains decompiler
// Type: SmashTools.Performance.AsyncAction
// Assembly: SmashTools, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 3B8999A7-6EA8-49BB-AA7F-3E2E898B5249
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\SmashTools.dll

using System;

#nullable disable
namespace SmashTools.Performance;

public abstract class AsyncAction
{
  public virtual bool LongOperation => false;

  public virtual bool IsValid => true;

  public abstract void Invoke();

  public abstract void ReturnToPool();

  public virtual void ExceptionThrown(Exception ex)
  {
  }
}
