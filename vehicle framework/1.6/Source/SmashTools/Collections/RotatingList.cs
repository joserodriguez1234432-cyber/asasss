// Decompiled with JetBrains decompiler
// Type: SmashTools.RotatingList`1
// Assembly: SmashTools, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 3B8999A7-6EA8-49BB-AA7F-3E2E898B5249
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\SmashTools.dll

using System.Collections.Generic;

#nullable disable
namespace SmashTools;

public class RotatingList<T> : List<T>
{
  private int currentIndex;

  public RotatingList() => this.currentIndex = 0;

  public RotatingList(int capacity)
    : base(capacity)
  {
    this.currentIndex = 0;
  }

  public RotatingList(IEnumerable<T> enumerable)
    : base(enumerable)
  {
    this.currentIndex = 0;
  }

  public int Index => this.currentIndex;

  public T Next
  {
    get
    {
      T next = this[this.currentIndex++];
      if (this.currentIndex >= this.Count)
        this.currentIndex = 0;
      return next;
    }
  }

  public T Previous
  {
    get
    {
      T previous = this[this.currentIndex--];
      if (this.currentIndex < 0)
        this.currentIndex = this.Count - 1;
      return previous;
    }
  }

  public T Current => this[this.currentIndex];

  public void PostItemRemove()
  {
    if (this.Count == 0)
      this.currentIndex = 0;
    else
      this.currentIndex %= this.Count;
  }

  public void SetIndex(int index) => this.currentIndex = index % this.Count;
}
