// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.VehicleMapUIRenderer
// Assembly: VehicleMapFramework, Version=1.6.562.0, Culture=neutral, PublicKeyToken=null
// MVID: 10A61882-945F-4CFC-9B06-CA8EEF5ADB36
// Assembly location: D:\Programas\steamapps\workshop\content\294100\3426502333\1.6\Assemblies\VehicleMapFramework.dll

using JetBrains.Annotations;
using SmashTools;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using Vehicles;
using Verse;

#nullable disable
namespace VehicleMapFramework;

public class VehicleMapUIRenderer : GameComponent
{
  private const int VEHICLE_MAP_LAYER = 28;
  public static Func<float> TimeProvider = (Func<float>) (() => Time.time);
  private readonly Dictionary<VehicleMapUIRenderer.CacheKey, VehicleMapUIRenderer.CachedMapTexture> cachedTextures;
  private readonly Game game;
  private readonly List<RenderTexture> renderTexturesPool;
  private readonly List<VehicleMapUIRenderer.CacheKey> toRemove;
  private readonly List<VehicleMapUIRenderer.CacheKey> toSetDirty;
  private Camera camera;
  private CommandBuffer commandBuffer;

  public VehicleMapUIRenderer(Game game)
  {
    this.game = game;
    this.renderTexturesPool = new List<RenderTexture>();
    this.toRemove = new List<VehicleMapUIRenderer.CacheKey>();
    this.toSetDirty = new List<VehicleMapUIRenderer.CacheKey>();
    // ISSUE: explicit constructor call
    base.\u002Ector();
  }

  public virtual void FinalizeInit()
  {
    this.CreateCamera();
    GameEvent.OnGameDisposing -= new Action(this.Clear);
    GameEvent.OnGameDisposing += new Action(this.Clear);
  }

  public virtual void GameComponentUpdate()
  {
    if (this.cachedTextures.Count == 0)
      return;
    foreach (KeyValuePair<VehicleMapUIRenderer.CacheKey, VehicleMapUIRenderer.CachedMapTexture> cachedTexture in this.cachedTextures)
    {
      VehicleMapUIRenderer.CachedMapTexture cachedMapTexture = cachedTexture.Value;
      if (cachedMapTexture.Expired)
      {
        this.toRemove.Add(cachedTexture.Key);
        cachedMapTexture = cachedTexture.Value;
        if (Object.op_Inequality((Object) cachedMapTexture.RenderTexture, (Object) null))
        {
          List<RenderTexture> renderTexturesPool = this.renderTexturesPool;
          cachedMapTexture = cachedTexture.Value;
          RenderTexture renderTexture = cachedMapTexture.RenderTexture;
          renderTexturesPool.Add(renderTexture);
        }
      }
    }
    foreach (VehicleMapUIRenderer.CacheKey key in this.toRemove)
      this.cachedTextures.Remove(key);
    this.toRemove.Clear();
  }

  private void CreateCamera()
  {
    GameObject gameObject = new GameObject("VehicleMapCamera", new Type[1]
    {
      typeof (Camera)
    });
    gameObject.SetActive(false);
    Object.DontDestroyOnLoad((Object) gameObject);
    this.camera = gameObject.GetComponent<Camera>();
    ((Component) this.camera).transform.rotation = Quaternion.Euler(90f, 0.0f, 0.0f);
    this.camera.orthographic = true;
    this.camera.cullingMask = 268435456 /*0x10000000*/;
    this.camera.clearFlags = (CameraClearFlags) 2;
    this.camera.backgroundColor = new Color(0.0f, 0.0f, 0.0f, 0.0f);
    this.camera.useOcclusionCulling = false;
    this.camera.renderingPath = (RenderingPath) 1;
    ((Component) this.camera).transform.position = new Vector3(0.0f, 5f, 0.0f);
    this.camera.nearClipPlane = 0.0f;
    this.camera.farClipPlane = 5.5f;
    this.commandBuffer = new CommandBuffer()
    {
      name = "VehicleMapDrawBuffer"
    };
    this.camera.AddCommandBuffer((CameraEvent) 10, this.commandBuffer);
  }

  public static Texture GetVehicleMapTexture(
    VehiclePawnWithMap vehicle,
    Rot4 rot,
    (int width, int height) texSize,
    Vector2? drawSize = null,
    Vector3? drawOffset = null)
  {
    VehicleMapUIRenderer component = Current.Game?.GetComponent<VehicleMapUIRenderer>();
    if (component?.camera == null || component.commandBuffer == null)
      return (Texture) BaseContent.BadTex;
    Camera camera = component.camera;
    VehicleMapUIRenderer.CacheKey key = new VehicleMapUIRenderer.CacheKey(texSize, vehicle, rot);
    VehicleMapUIRenderer.CachedMapTexture cachedMapTexture = component.GetOrCreateCachedMapTexture(key);
    if (!cachedMapTexture.Dirty)
    {
      component.cachedTextures[key] = new VehicleMapUIRenderer.CachedMapTexture(cachedMapTexture.RenderTexture, false, VehicleMapUIRenderer.TimeProvider());
      return (Texture) cachedMapTexture.RenderTexture;
    }
    IntVec3 mapSize = vehicle.MapSize;
    Vector2 vector2_1 = ((IntVec3) ref mapSize).ToVector2();
    Vector3 vector3_1 = Vector3Utility.RotatedBy(new Vector3((float) (-(double) vector2_1.x / 2.0), 0.0f, (float) (-(double) vector2_1.y / 2.0)), rot);
    Vector2 vector2_2 = drawSize ?? vector2_1;
    Vector3 vector3_2 = drawOffset ?? Vector3.zero;
    float num = Mathf.Max(vector2_2.x, vector2_2.y);
    ((Behaviour) camera).enabled = true;
    camera.orthographicSize = num / 2f;
    camera.aspect = (float) texSize.width / (float) texSize.height;
    camera.targetTexture = cachedMapTexture.RenderTexture;
    component.commandBuffer.Clear();
    component.RenderVehicleMap(vehicle.VehicleMap, Vector3.op_Addition(vector3_1, vector3_2), rot);
    camera.Render();
    camera.targetTexture = (RenderTexture) null;
    ((Behaviour) camera).enabled = false;
    component.cachedTextures[key] = new VehicleMapUIRenderer.CachedMapTexture(cachedMapTexture.RenderTexture, false, VehicleMapUIRenderer.TimeProvider());
    return (Texture) cachedMapTexture.RenderTexture;
  }

  public static Texture GetOverlayWithVehicleMapTexture(
    VehiclePawnWithMap vehicle,
    GraphicOverlay overlay,
    Rot4 rot,
    (int width, int height) texSize,
    CellRect mapLimit)
  {
    VehicleMapUIRenderer component = Current.Game?.GetComponent<VehicleMapUIRenderer>();
    if (component?.camera == null || component.commandBuffer == null)
      return (Texture) BaseContent.BadTex;
    VehicleMapUIRenderer.CacheKey key = new VehicleMapUIRenderer.CacheKey(texSize, vehicle, rot, overlay);
    VehicleMapUIRenderer.CachedMapTexture cachedMapTexture = component.GetOrCreateCachedMapTexture(key);
    if (!cachedMapTexture.Dirty)
    {
      component.cachedTextures[key] = new VehicleMapUIRenderer.CachedMapTexture(cachedMapTexture.RenderTexture, false, GenTicks.TicksGame);
      return (Texture) cachedMapTexture.RenderTexture;
    }
    Camera camera = component.camera;
    ((Behaviour) camera).enabled = true;
    Vector2 drawSize = overlay.Graphic.drawSize;
    Vector2 vector2 = ((Rot4) ref rot).IsHorizontal ? Vector2Utility.Rotated(drawSize) : drawSize;
    camera.targetTexture = cachedMapTexture.RenderTexture;
    camera.orthographicSize = vector2.y / 2f;
    camera.aspect = (float) texSize.width / (float) texSize.height;
    component.commandBuffer.Clear();
    Graphic graphic = overlay.Graphic;
    component.commandBuffer.DrawMesh(graphic.MeshAt(rot), Matrix4x4.TRS(Vector3.zero, Quaternion.identity, Vector3.one), graphic.MatAt(rot, (Thing) vehicle));
    Vector3 vector3_1 = Vector3.op_Subtraction(Vector3.op_Addition(VehicleMapUtility.OffsetFor(vehicle, Rot8.op_Implicit(rot)), ((Graphic) vehicle.VehicleGraphic).DrawOffset(rot)), overlay.Graphic.DrawOffset(rot));
    Vector3 drawPos = Vector3.op_Addition(Vector3Utility.RotatedBy(new Vector3((float) -vehicle.MapSize.x / 2f, 0.0f, (float) -vehicle.MapSize.z / 2f), rot), vector3_1);
    Vector3 vector3_2 = Vector3.op_Division(Vector2Utility.ToVector3(vector2), 2f);
    float num1 = (float) texSize.height / vector2.y;
    Vector3 vector3_3 = drawPos;
    IntVec3 min = ((CellRect) ref mapLimit).Min;
    Vector3 vector3_4 = Vector3Utility.RotatedBy(((IntVec3) ref min).ToVector3(), rot);
    Vector3 vector3_5 = Vector3.op_Multiply(Vector3.op_Addition(Vector3.op_Addition(vector3_3, vector3_4), vector3_2), num1);
    Vector3 vector3_6 = drawPos;
    CellRect cellRect = ((CellRect) ref mapLimit).MaxExpandedBy(1);
    IntVec3 max = ((CellRect) ref cellRect).Max;
    Vector3 vector3_7 = Vector3Utility.RotatedBy(((IntVec3) ref max).ToVector3(), rot);
    Vector3 vector3_8 = Vector3.op_Multiply(Vector3.op_Addition(Vector3.op_Addition(vector3_6, vector3_7), vector3_2), num1);
    float num2 = Mathf.Min(vector3_5.x, vector3_8.x);
    float num3 = Mathf.Min(vector3_5.z, vector3_8.z);
    float num4 = Mathf.Max(vector3_5.x, vector3_8.x);
    float num5 = Mathf.Max(vector3_5.z, vector3_8.z);
    component.commandBuffer.EnableScissorRect(Rect.MinMaxRect(num2, num3, num4, num5));
    component.RenderVehicleMap(vehicle.VehicleMap, drawPos, rot);
    camera.Render();
    camera.targetTexture = (RenderTexture) null;
    ((Behaviour) camera).enabled = false;
    component.commandBuffer.DisableScissorRect();
    component.cachedTextures[key] = new VehicleMapUIRenderer.CachedMapTexture(cachedMapTexture.RenderTexture, false, GenTicks.TicksGame);
    return (Texture) cachedMapTexture.RenderTexture;
  }

  private void RenderVehicleMap(Map map, Vector3 drawPos, Rot4 rot)
  {
    MapDrawer mapDrawer = map.mapDrawer;
    VehicleSectionLayerManager cachedMapComponent = ComponentCache.GetCachedMapComponent<VehicleSectionLayerManager>(map);
    for (int index1 = 0; index1 < map.Size.x; index1 += 17)
    {
      for (int index2 = 0; index2 < map.Size.z; index2 += 17)
        this.DrawSection(mapDrawer.SectionAt(new IntVec3(index1, 0, index2)), drawPos, rot, cachedMapComponent);
    }
  }

  private void DrawSection(
    Section section,
    Vector3 drawPos,
    Rot4 rot,
    VehicleSectionLayerManager component)
  {
    Quaternion rotation = Quaternion.AngleAxis(((Rot4) ref rot).AsAngle, Vector3.up);
    DrawLayerNow(section.GetLayer(typeof (SectionLayer_TerrainOnVehicle)));
    DrawLayerNow(component.GetLayer(section, typeof (SectionLayer_ThingsGeneral), Rot8.op_Implicit(rot)));

    void DrawLayerNow(SectionLayer layer)
    {
      if (layer == null)
        return;
      for (int index = 0; index < ((MapDrawLayer) layer).subMeshes.Count; ++index)
      {
        LayerSubMesh subMesh = ((MapDrawLayer) layer).subMeshes[index];
        if (subMesh.finalized && !subMesh.disabled)
          this.commandBuffer.DrawMesh(subMesh.mesh, Matrix4x4.TRS(drawPos, rotation, Vector3.one), subMesh.material);
      }
    }
  }

  public void Clear()
  {
    foreach (VehicleMapUIRenderer.CachedMapTexture cachedMapTexture in this.cachedTextures.Values)
    {
      if (cachedMapTexture.RenderTexture != null)
        Object.Destroy((Object) cachedMapTexture.RenderTexture);
    }
    this.cachedTextures.Clear();
    foreach (RenderTexture renderTexture in this.renderTexturesPool)
    {
      if (renderTexture != null)
        Object.Destroy((Object) renderTexture);
    }
    this.renderTexturesPool.Clear();
    this.toRemove.Clear();
    this.toSetDirty.Clear();
    if (Object.op_Inequality((Object) this.camera, (Object) null))
    {
      this.camera.RemoveAllCommandBuffers();
      Object.Destroy((Object) ((Component) this.camera).gameObject);
    }
    this.commandBuffer?.Release();
    this.camera = (Camera) null;
    this.commandBuffer = (CommandBuffer) null;
    GameEvent.OnGameDisposing -= new Action(this.Clear);
  }

  public static void SetDirty(VehiclePawnWithMap vehicle, VehicleMapUIRenderer.DurationType type = VehicleMapUIRenderer.DurationType.Time)
  {
    VehicleMapUIRenderer component = Current.Game?.GetComponent<VehicleMapUIRenderer>();
    if (component == null)
      return;
    Dictionary<VehicleMapUIRenderer.CacheKey, VehicleMapUIRenderer.CachedMapTexture> cachedTextures = component.cachedTextures;
    foreach (KeyValuePair<VehicleMapUIRenderer.CacheKey, VehicleMapUIRenderer.CachedMapTexture> keyValuePair in cachedTextures)
    {
      if (keyValuePair.Key.vehicle == vehicle && keyValuePair.Value.DurationType == type)
        component.toSetDirty.Add(keyValuePair.Key);
    }
    foreach (VehicleMapUIRenderer.CacheKey key1 in component.toSetDirty)
    {
      Dictionary<VehicleMapUIRenderer.CacheKey, VehicleMapUIRenderer.CachedMapTexture> dictionary = cachedTextures;
      VehicleMapUIRenderer.CacheKey key2 = key1;
      VehicleMapUIRenderer.CachedMapTexture cachedMapTexture;
      if (type != VehicleMapUIRenderer.DurationType.Time)
      {
        if (type != VehicleMapUIRenderer.DurationType.Ticks)
          throw new ArgumentOutOfRangeException(nameof (type), (object) type, (string) null);
        cachedMapTexture = new VehicleMapUIRenderer.CachedMapTexture(cachedTextures[key1].RenderTexture, true, GenTicks.TicksGame);
      }
      else
        cachedMapTexture = new VehicleMapUIRenderer.CachedMapTexture(cachedTextures[key1].RenderTexture, true, VehicleMapUIRenderer.TimeProvider());
      dictionary[key2] = cachedMapTexture;
    }
    component.toSetDirty.Clear();
  }

  private VehicleMapUIRenderer.CachedMapTexture GetOrCreateCachedMapTexture(
    VehicleMapUIRenderer.CacheKey key)
  {
    VehicleMapUIRenderer.CachedMapTexture cachedMapTexture1;
    if (!this.cachedTextures.TryGetValue(key, out cachedMapTexture1))
    {
      Dictionary<VehicleMapUIRenderer.CacheKey, VehicleMapUIRenderer.CachedMapTexture> cachedTextures = this.cachedTextures;
      VehicleMapUIRenderer.CacheKey key1 = key;
      cachedMapTexture1 = new VehicleMapUIRenderer.CachedMapTexture(this.GetRenderTexture(key.size), true, VehicleMapUIRenderer.TimeProvider());
      VehicleMapUIRenderer.CachedMapTexture cachedMapTexture2 = cachedMapTexture1;
      cachedTextures[key1] = cachedMapTexture2;
    }
    return cachedMapTexture1;
  }

  private RenderTexture GetRenderTexture((int width, int height) size)
  {
    for (int index = this.renderTexturesPool.Count - 1; index >= 0; --index)
    {
      RenderTexture renderTexture = this.renderTexturesPool[index];
      if (((Texture) renderTexture).width == size.width && ((Texture) renderTexture).height == size.height)
      {
        this.renderTexturesPool.RemoveAt(index);
        return renderTexture;
      }
    }
    RenderTexture renderTexture1 = new RenderTexture(size.width, size.height, 24);
    ((Object) renderTexture1).name = "VehicleMapTexture";
    renderTexture1.useMipMap = false;
    ((Texture) renderTexture1).filterMode = (FilterMode) 1;
    return renderTexture1;
  }

  public enum DurationType
  {
    Time,
    Ticks,
  }

  private readonly record struct CacheKey(
    (int width, int height) size,
    VehiclePawnWithMap vehicle,
    Rot4 rot,
    [UsedImplicitly] GraphicOverlay overlay = null)
  ;

  public readonly struct CachedMapTexture(RenderTexture renderTexture, bool dirty)
  {
    public const float CacheDurationTime = 1f;
    public const int CacheDurationTicks = 60;

    public CachedMapTexture(RenderTexture renderTexture, bool dirty, float lastUseTime)
      : this(renderTexture, dirty)
    {
      this.LastUseTime = lastUseTime;
      this.DurationType = VehicleMapUIRenderer.DurationType.Time;
    }

    public CachedMapTexture(RenderTexture renderTexture, bool dirty, int lastUseTick)
      : this(renderTexture, dirty)
    {
      this.LastUseTick = lastUseTick;
      this.DurationType = VehicleMapUIRenderer.DurationType.Ticks;
    }

    public RenderTexture RenderTexture { get; } = renderTexture;

    public bool Dirty { get; } = dirty;

    public VehicleMapUIRenderer.DurationType DurationType { get; } = VehicleMapUIRenderer.DurationType.Time;

    public float LastUseTime { get; } = 0.0f;

    public int LastUseTick { get; } = 0;

    public bool Expired
    {
      get
      {
        return this.DurationType != VehicleMapUIRenderer.DurationType.Ticks ? (double) VehicleMapUIRenderer.TimeProvider() - (double) this.LastUseTime > 1.0 : GenTicks.TicksGame - this.LastUseTick > 60;
      }
    }
  }
}
