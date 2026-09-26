// Decompiled with JetBrains decompiler
// Type: SmashTools.Algorithms.KuhnMunkres
// Assembly: SmashTools, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 3B8999A7-6EA8-49BB-AA7F-3E2E898B5249
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\SmashTools.dll

using UnityEngine;

#nullable disable
namespace SmashTools.Algorithms;

public sealed class KuhnMunkres
{
  private readonly int n;
  private readonly float[] labelByWorker;
  private readonly float[] labelByJob;
  private readonly int[] minSlackWorkerByJob;
  private readonly float[] minSlackValueByJob;
  private readonly int[] matchWorkerByJob;
  private readonly int[] parentWorkerByCommittedJob;
  private readonly bool[] committedWorkers;

  public KuhnMunkres(int n)
  {
    this.n = n;
    this.labelByWorker = new float[n];
    this.labelByJob = new float[n];
    this.minSlackWorkerByJob = new int[n];
    this.minSlackValueByJob = new float[n];
    this.matchWorkerByJob = new int[n];
    this.parentWorkerByCommittedJob = new int[n];
    this.committedWorkers = new bool[n];
  }

  public int[] Compute(float[,] costMatrix)
  {
    int[] numArray = new int[this.n];
    numArray.Populate<int>(-1);
    this.matchWorkerByJob.Populate<int>(-1);
    this.InitializeLabels(costMatrix);
    for (int i = 0; i < this.n; ++i)
    {
      if (numArray[i] < 0)
      {
        this.StartPhase(i, costMatrix);
        this.ExecutePhase(costMatrix, numArray);
      }
    }
    return numArray;
  }

  private void InitializeLabels(float[,] costMatrix)
  {
    for (int index1 = 0; index1 < this.n; ++index1)
    {
      this.labelByWorker[index1] = float.MaxValue;
      for (int index2 = 0; index2 < this.n; ++index2)
        this.labelByWorker[index1] = Mathf.Min(this.labelByWorker[index1], costMatrix[index1, index2]);
    }
    this.labelByJob.Populate<float>(0.0f);
  }

  private void StartPhase(int i, float[,] costMatrix)
  {
    this.committedWorkers.Populate<bool>(false);
    this.parentWorkerByCommittedJob.Populate<int>(-1);
    this.committedWorkers[i] = true;
    for (int index = 0; index < this.n; ++index)
    {
      this.minSlackValueByJob[index] = costMatrix[i, index] - this.labelByWorker[i] - this.labelByJob[index];
      this.minSlackWorkerByJob[index] = i;
    }
  }

  private void ExecutePhase(float[,] costMatrix, int[] result)
  {
label_0:
    float maxValue = float.MaxValue;
    int num1 = -1;
    int index1 = -1;
    for (int index2 = 0; index2 < this.n; ++index2)
    {
      if (this.parentWorkerByCommittedJob[index2] == -1 && (double) this.minSlackValueByJob[index2] < (double) maxValue)
      {
        maxValue = this.minSlackValueByJob[index2];
        num1 = this.minSlackWorkerByJob[index2];
        index1 = index2;
      }
    }
    for (int index3 = 0; index3 < this.n; ++index3)
    {
      if (this.committedWorkers[index3])
        this.labelByWorker[index3] += maxValue;
    }
    for (int index4 = 0; index4 < this.n; ++index4)
    {
      if (this.parentWorkerByCommittedJob[index4] != -1)
        this.labelByJob[index4] -= maxValue;
      else
        this.minSlackValueByJob[index4] -= maxValue;
    }
    this.parentWorkerByCommittedJob[index1] = num1;
    if (this.matchWorkerByJob[index1] == -1)
    {
      int index5 = this.parentWorkerByCommittedJob[index1];
      while (true)
      {
        int num2 = result[index5];
        result[index5] = index1;
        this.matchWorkerByJob[index1] = index5;
        index1 = num2;
        if (index1 != -1)
          index5 = this.parentWorkerByCommittedJob[index1];
        else
          break;
      }
    }
    else
    {
      int index6 = this.matchWorkerByJob[index1];
      this.committedWorkers[index6] = true;
      for (int index7 = 0; index7 < this.n; ++index7)
      {
        if (this.parentWorkerByCommittedJob[index7] == -1)
        {
          float num3 = costMatrix[index6, index7] - this.labelByWorker[index6] - this.labelByJob[index7];
          if ((double) this.minSlackValueByJob[index7] > (double) num3)
          {
            this.minSlackValueByJob[index7] = num3;
            this.minSlackWorkerByJob[index7] = index6;
          }
        }
      }
      goto label_0;
    }
  }
}
