using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using Verse;
using RimWorld;
using SmashTools;
using Vehicles;
using Vehicles.Rendering;
using VehicleRaid;

namespace VehicleRaidFramework
{
    public enum VRF_MainTab { Raids, Settlements, Outposts, Ambush, Hover, Config }
    public enum VRF_SettingsPage { FactionList, VehicleList, VehicleDetail }

    public static class VRF_SettingsUI
    {
        private static VRF_MainTab    _tab  = VRF_MainTab.Raids;
        private static VRF_SettingsPage _page = VRF_SettingsPage.FactionList;
        private static FactionDef     _selectedFaction;
        private static PawnKindDef    _selectedVehicleKind;

        private static Vector2 _factionScrollPos;
        private static Vector2 _vehicleScrollPos;
        private static Vector2 _detailScrollPos;
        private class CachedPresetInfo
        {
            public DateTime lastWrite;
            public VehicleMapFramework.VRF_GravshipPresetData data;
        }
        private static readonly Dictionary<string, CachedPresetInfo> _cachedPresetData = new Dictionary<string, CachedPresetInfo>();
        private static readonly Dictionary<string, Texture2D> _cachedPresetTextures = new Dictionary<string, Texture2D>();
        private static Vector2 _gravshipListScrollPos = Vector2.zero;

        private static Vector2 _presetScrollPos;
        private static Vector2 _hoverScrollPos;
        private static Vector2 _configScrollPos;

        private static bool   _showPresetPanel  = false;
        private static string _exportFileName   = "MyPreset";

        private static readonly Dictionary<string, string> _cpBuffers  = new Dictionary<string, string>();
        private static readonly Dictionary<string, string> _mrpBuffers = new Dictionary<string, string>();
        private static readonly Dictionary<string, string> _mxpBuffers = new Dictionary<string, string>();
        private static readonly Dictionary<string, string> _hmsBuffers = new Dictionary<string, string>();
        private static readonly Dictionary<string, string> _budgetBufs = new Dictionary<string, string>();
        private static readonly Dictionary<string, Dictionary<string, string>> _hoverBufs =
            new Dictionary<string, Dictionary<string, string>>();
        private static readonly Dictionary<string, string> _configBufs = new Dictionary<string, string>();

        private static List<VRF_NaturalRaidVehicleEntry> _vehicleListClipboard;
        private static string _clipboardSourceFaction;
        private static string _vehicleSearchFilter = "";

        private const float RowHeight = 50f;
        private const float Pad       = 8f;
        private const float BtnW      = 110f;
        private const float TabH      = 32f;

        public static void Draw(Rect inRect)
        {
            float y = inRect.y;
            Text.Font = GameFont.Medium;
            Widgets.Label(new Rect(inRect.x, y, inRect.width, 32f), "VRF_Settings_Title".Translate());
            Text.Font = GameFont.Small;
            y += 36f;

            DrawMainTabs(new Rect(inRect.x, y, inRect.width, TabH));
            y += TabH + 2f;

            if (_tab != VRF_MainTab.Hover && _tab != VRF_MainTab.Config && _page != VRF_SettingsPage.FactionList)
            {
                if (Widgets.ButtonText(new Rect(inRect.x, y, BtnW, 26f), "VRF_Settings_Back".Translate()))
                {
                    if (_page == VRF_SettingsPage.VehicleDetail)
                        _page = VRF_SettingsPage.VehicleList;
                    else
                    {
                        _page = VRF_SettingsPage.FactionList;
                        _selectedFaction = null;
                        _vehicleSearchFilter = "";
                    }
                }
                y += 32f;
            }

            Rect content = new Rect(inRect.x, y, inRect.width, inRect.yMax - y);
            switch (_tab)
            {
                case VRF_MainTab.Raids:       DrawFactionPage(content, VRF_SpawnContext.Raid);       break;
                case VRF_MainTab.Settlements: DrawFactionPage(content, VRF_SpawnContext.Settlement); break;
                case VRF_MainTab.Outposts:    DrawFactionPage(content, VRF_SpawnContext.Outpost);    break;
                case VRF_MainTab.Ambush:      DrawFactionPage(content, VRF_SpawnContext.Ambush);     break;
                case VRF_MainTab.Hover:       DrawHoverTab(content);                                 break;
                case VRF_MainTab.Config:      DrawConfigTab(content);                                break;
            }
        }

        private static void DrawMainTabs(Rect rect)
        {
            float w = rect.width / 6f;
            var tabs = new[]
            {
                (VRF_MainTab.Raids,       "VRF_Tab_Raids"),
                (VRF_MainTab.Settlements, "VRF_Tab_Settlements"),
                (VRF_MainTab.Outposts,    "VRF_Tab_Outposts"),
                (VRF_MainTab.Ambush,      "VRF_Tab_Ambush"),
                (VRF_MainTab.Hover,       "VRF_Tab_Hover"),
                (VRF_MainTab.Config,      "VRF_Tab_Config"),
            };
            for (int i = 0; i < tabs.Length; i++)
            {
                var (tab, key) = tabs[i];
                Rect r = new Rect(rect.x + i * w, rect.y, w - 2f, rect.height);
                bool active = _tab == tab;
                if (active) Widgets.DrawBoxSolid(r, new Color(0.25f, 0.25f, 0.25f));
                else         Widgets.DrawBoxSolid(r, new Color(0.15f, 0.15f, 0.15f));
                Widgets.DrawBox(r, 1);
                Text.Anchor = TextAnchor.MiddleCenter;
                GUI.color = active ? Color.white : new Color(0.75f, 0.75f, 0.75f);
                Widgets.Label(r, key.Translate());
                GUI.color = Color.white;
                Text.Anchor = TextAnchor.UpperLeft;
                if (!active && Widgets.ButtonInvisible(r))
                {
                    _tab  = tab;
                    _page = VRF_SettingsPage.FactionList;
                    _selectedFaction = null;
                    _vehicleSearchFilter = "";
                }
            }
        }

        private static void DrawFactionPage(Rect rect, VRF_SpawnContext ctx)
        {
            switch (_page)
            {
                case VRF_SettingsPage.FactionList:   DrawFactionList(rect, ctx);   break;
                case VRF_SettingsPage.VehicleList:   DrawVehicleList(rect, ctx);   break;
                case VRF_SettingsPage.VehicleDetail: DrawVehicleDetail(rect, ctx); break;
            }
        }

        private static string BudgetKey(FactionDef f, VRF_SpawnContext ctx) =>
            f.defName + "_budget_" + ctx.ToString();

        private static float GetContextBudget(VRF_NaturalRaidFactionConfig cfg, VRF_SpawnContext ctx)
        {
            switch (ctx)
            {
                case VRF_SpawnContext.Settlement: return cfg.settlementBudget;
                case VRF_SpawnContext.Outpost:    return cfg.outpostBudget;
                case VRF_SpawnContext.Ambush:     return cfg.ambushBudget;
                default: return 0f;
            }
        }

        private static void SetContextBudget(VRF_NaturalRaidFactionConfig cfg, VRF_SpawnContext ctx, float val)
        {
            switch (ctx)
            {
                case VRF_SpawnContext.Settlement: cfg.settlementBudget = val; break;
                case VRF_SpawnContext.Outpost:    cfg.outpostBudget    = val; break;
                case VRF_SpawnContext.Ambush:     cfg.ambushBudget     = val; break;
            }
        }

        private static void DrawFactionList(Rect rect, VRF_SpawnContext ctx)
        {
            var factions = VRF_VehicleKindCache.RaidableFactions;

            float topBarH   = 30f;
            float checkH    = 26f;
            float topTotalH = topBarH + Pad + checkH + Pad;

            Rect topBar = new Rect(rect.x, rect.y, rect.width, topBarH);

            string descKey = ctx == VRF_SpawnContext.Raid ? "VRF_Settings_FactionListDesc"
                           : ctx == VRF_SpawnContext.Settlement ? "VRF_Settings_FactionListDesc_Settlement"
                           : ctx == VRF_SpawnContext.Outpost    ? "VRF_Settings_FactionListDesc_Outpost"
                           : "VRF_Settings_FactionListDesc_Ambush";
            Widgets.Label(new Rect(topBar.x, topBar.y, topBar.width - BtnW * 2f - Pad * 2f, topBarH), descKey.Translate());

            if (Widgets.ButtonText(new Rect(topBar.xMax - BtnW * 2f - Pad, topBar.y + 2f, BtnW, 26f), "VRF_Settings_Clear".Translate()))
            {
                Find.WindowStack.Add(Dialog_MessageBox.CreateConfirmation(
                    "VRF_Settings_ClearConfirm".Translate(),
                    () => { VRF_Mod.Settings.ResetAllSettings(); ResetBuffers(); VRF_Mod.Instance.WriteSettings(); }));
            }
            if (Widgets.ButtonText(new Rect(topBar.xMax - BtnW, topBar.y + 2f, BtnW, 26f), "VRF_Settings_Presets".Translate()))
                _showPresetPanel = !_showPresetPanel;

            float checkY = rect.y + topBarH + Pad;
            bool prevAuto = VRF_Mod.Settings.autoLoadPresets;
            Widgets.CheckboxLabeled(new Rect(rect.x, checkY, rect.width * 0.6f, checkH), "VRF_Settings_AutoLoadPresets".Translate(), ref VRF_Mod.Settings.autoLoadPresets);
            if (VRF_Mod.Settings.autoLoadPresets != prevAuto) VRF_Mod.Instance.WriteSettings();

            if (factions.NullOrEmpty())
            {
                Widgets.Label(new Rect(rect.x, rect.y + topTotalH, rect.width, 24f), "VRF_Settings_NoFactions".Translate());
                return;
            }

            if (_showPresetPanel)
            {
                float panelW   = Mathf.Min(rect.width * 0.45f, 340f);
                Rect panelRect = new Rect(rect.xMax - panelW, rect.y + topTotalH, panelW, rect.height - topTotalH);
                DrawPresetPanel(panelRect);
                Rect scrollArea = new Rect(rect.x, rect.y + topTotalH, rect.width - panelW - Pad, rect.height - topTotalH);
                Rect viewRect   = new Rect(0f, 0f, scrollArea.width - 20f, factions.Count * RowHeight);
                Widgets.BeginScrollView(scrollArea, ref _factionScrollPos, viewRect);
                DrawFactionRows(factions, viewRect, ctx);
                Widgets.EndScrollView();
            }
            else
            {
                Rect scrollArea = new Rect(rect.x, rect.y + topTotalH, rect.width, rect.height - topTotalH);
                Rect viewRect   = new Rect(0f, 0f, scrollArea.width - 20f, factions.Count * RowHeight);
                Widgets.BeginScrollView(scrollArea, ref _factionScrollPos, viewRect);
                DrawFactionRows(factions, viewRect, ctx);
                Widgets.EndScrollView();
            }
        }

        private static void DrawFactionRows(List<FactionDef> factions, Rect viewRect, VRF_SpawnContext ctx)
        {
            for (int i = 0; i < factions.Count; i++)
            {
                FactionDef fDef = factions[i];
                Rect row = new Rect(0f, i * RowHeight, viewRect.width, RowHeight - 2f);
                if (i % 2 == 0) Widgets.DrawAltRect(row);
                Widgets.DrawHighlightIfMouseover(row);

                Rect iconR = new Rect(row.x + Pad, row.y + (row.height - 32f) * 0.5f, 32f, 32f);
                if (fDef.FactionIcon != null)
                {
                    if (!fDef.colorSpectrum.NullOrEmpty()) GUI.color = fDef.colorSpectrum[0];
                    GUI.DrawTexture(iconR, fDef.FactionIcon, ScaleMode.ScaleToFit);
                    GUI.color = Color.white;
                }

                Rect labelR = new Rect(iconR.xMax + Pad, row.y, row.width - iconR.width - Pad * 3f - BtnW, row.height);
                Text.Anchor = TextAnchor.MiddleLeft;
                Widgets.Label(labelR, fDef.label ?? fDef.defName);
                Text.Anchor = TextAnchor.UpperLeft;

                Rect btnR = new Rect(row.xMax - BtnW - Pad, row.y + (row.height - 26f) * 0.5f, BtnW, 26f);
                if (Widgets.ButtonText(btnR, "VRF_Settings_Configure".Translate()))
                {
                    _selectedFaction = fDef;
                    _page = VRF_SettingsPage.VehicleList;
                    _vehicleScrollPos = Vector2.zero;
                }
            }
        }

        private static void DrawPresetPanel(Rect rect)
        {
            Widgets.DrawBoxSolid(rect, new Color(0.1f, 0.1f, 0.1f, 0.85f));
            Widgets.DrawBox(rect, 1);
            Rect inner = rect.ContractedBy(Pad);
            float y = inner.y;

            GUI.color = new Color(0.9f, 0.85f, 0.6f);
            Widgets.Label(new Rect(inner.x, y, inner.width, 22f), "VRF_Settings_PresetExport".Translate());
            GUI.color = Color.white;
            y += 24f;
            Widgets.Label(new Rect(inner.x, y, inner.width, 20f), "VRF_Settings_PresetFileName".Translate());
            y += 22f;
            _exportFileName = Widgets.TextField(new Rect(inner.x, y, inner.width - BtnW - Pad, 24f), _exportFileName);
            if (Widgets.ButtonText(new Rect(inner.xMax - BtnW, y, BtnW, 24f), "VRF_Settings_Export".Translate()))
            {
                string safeName = string.IsNullOrWhiteSpace(_exportFileName) ? "MyPreset" : _exportFileName;
                safeName = string.Concat(safeName.Split(Path.GetInvalidFileNameChars()));
                VRF_PresetIO.Export(VRF_Mod.Settings, safeName);
            }
            y += 30f;

            GUI.color = new Color(0.9f, 0.85f, 0.6f);
            Widgets.Label(new Rect(inner.x, y, inner.width, 22f), "VRF_Settings_PresetImport".Translate());
            GUI.color = Color.white;
            y += 24f;

            var presetFiles = VRF_PresetIO.FindAllPresetFiles();
            if (presetFiles.Count == 0)
            {
                GUI.color = new Color(0.6f, 0.6f, 0.6f);
                Widgets.Label(new Rect(inner.x, y, inner.width, 22f), "VRF_Settings_NoPresets".Translate());
                GUI.color = Color.white;
            }
            else
            {
                float listH    = inner.yMax - y;
                Rect listArea  = new Rect(inner.x, y, inner.width, listH);
                Rect listView  = new Rect(0f, 0f, listArea.width - 20f, presetFiles.Count * 30f);
                Widgets.BeginScrollView(listArea, ref _presetScrollPos, listView);
                for (int i = 0; i < presetFiles.Count; i++)
                {
                    string file = presetFiles[i];
                    Rect row    = new Rect(0f, i * 30f, listView.width, 28f);
                    if (i % 2 == 0) Widgets.DrawAltRect(row);
                    Widgets.DrawHighlightIfMouseover(row);
                    Text.Anchor = TextAnchor.MiddleLeft;
                    Widgets.Label(new Rect(row.x + Pad, row.y, row.width - BtnW - Pad * 2f, row.height),
                        VRF_PresetIO.GetPresetDisplayName(file));
                    Text.Anchor = TextAnchor.UpperLeft;
                    if (Widgets.ButtonText(new Rect(row.xMax - BtnW, row.y + 1f, BtnW, 26f), "VRF_Settings_Load".Translate()))
                        VRF_PresetIO.Import(file, VRF_Mod.Settings);
                }
                Widgets.EndScrollView();
            }
        }

        private static void DrawVehicleList(Rect rect, VRF_SpawnContext ctx)
        {
            if (_selectedFaction == null) { _page = VRF_SettingsPage.FactionList; return; }
            var allKinds     = VRF_VehicleKindCache.AllVehicleKinds;
            var factionConfig = VRF_Mod.Settings.GetOrCreateFactionConfig(_selectedFaction.defName);

            Text.Font = GameFont.Small;
            string header   = "VRF_Settings_VehicleListDesc".Translate(_selectedFaction.label ?? _selectedFaction.defName);
            float headerH   = Text.CalcHeight(header, rect.width - BtnW * 4f - Pad * 4f);
            Widgets.Label(new Rect(rect.x, rect.y, rect.width - BtnW * 4f - Pad * 4f, headerH), header);
            float btnY      = rect.y + (headerH - 26f) * 0.5f;
            bool hasClipboard = _vehicleListClipboard != null && _vehicleListClipboard.Count > 0;

            if (Widgets.ButtonText(new Rect(rect.xMax - BtnW * 4f - Pad * 3f, btnY, BtnW, 26f), "VRF_Settings_AutoFill".Translate()))
                DoAutoFill(_selectedFaction, factionConfig, ctx);

            if (Widgets.ButtonText(new Rect(rect.xMax - BtnW * 3f - Pad * 2f, btnY, BtnW, 26f), "VRF_Settings_Clear".Translate()))
            {
                string fn = _selectedFaction.label ?? _selectedFaction.defName;
                string fd = _selectedFaction.defName;
                Find.WindowStack.Add(Dialog_MessageBox.CreateConfirmation(
                    "VRF_Settings_ClearFactionConfirm".Translate(fn),
                    () => { VRF_Mod.Settings.GetOrCreateFactionConfig(fd).GetEntriesForContext(ctx).Clear(); VRF_Mod.Instance.WriteSettings(); }));
            }

            if (Widgets.ButtonText(new Rect(rect.xMax - BtnW * 2f - Pad, btnY, BtnW, 26f), "VRF_Settings_CopyList".Translate()))
            {
                _vehicleListClipboard     = factionConfig.GetEntriesForContext(ctx).Select(CloneVehicleEntry).ToList();
                _clipboardSourceFaction   = _selectedFaction.label ?? _selectedFaction.defName;
                Messages.Message("VRF_Settings_CopiedList".Translate(_clipboardSourceFaction), MessageTypeDefOf.NeutralEvent, false);
            }

            GUI.color = hasClipboard ? Color.white : new Color(0.5f, 0.5f, 0.5f);
            if (Widgets.ButtonText(new Rect(rect.xMax - BtnW, btnY, BtnW, 26f), "VRF_Settings_PasteList".Translate()) && hasClipboard)
            {
                string src = _clipboardSourceFaction ?? "?";
                string tgt = _selectedFaction.label ?? _selectedFaction.defName;
                Find.WindowStack.Add(Dialog_MessageBox.CreateConfirmation(
                    "VRF_Settings_PasteConfirm".Translate(src, tgt),
                    () => { foreach (var s in _vehicleListClipboard) PasteIntoEntry(s, factionConfig.GetOrCreateForContext(s.vehicleKindDefName, ctx)); VRF_Mod.Instance.WriteSettings(); }));
            }
            GUI.color = Color.white;

            float budgetY = rect.y + headerH + 4f;
            float budgetH = 0f;
            if (ctx != VRF_SpawnContext.Raid)
            {
                budgetH = 26f;
                string bKey = BudgetKey(_selectedFaction, ctx);
                float curBudget = GetContextBudget(factionConfig, ctx);
                if (!_budgetBufs.TryGetValue(bKey, out string bBuf) || bBuf == null)
                {
                    bBuf = ((int)curBudget).ToString();
                    _budgetBufs[bKey] = bBuf;
                }
                Widgets.Label(new Rect(rect.x, budgetY + 2f, 160f, 22f), "VRF_Settings_Budget".Translate());
                float prev = curBudget;
                Widgets.TextFieldNumeric(new Rect(rect.x + 164f, budgetY, 100f, 24f), ref curBudget, ref bBuf, 0f, 999999f);
                _budgetBufs[bKey] = bBuf;
                if (Mathf.Abs(curBudget - prev) > 0.01f)
                {
                    SetContextBudget(factionConfig, ctx, curBudget);
                    VRF_Mod.Instance.WriteSettings();
                }
                Rect budgetTip = new Rect(rect.x + 164f + 100f + 4f, budgetY + 3f, 18f, 18f);
                GUI.color = Color.yellow;
                Widgets.Label(budgetTip, "?");
                GUI.color = Color.white;
                if (Mouse.IsOver(budgetTip))
                    TooltipHandler.TipRegion(budgetTip, "VRF_Settings_Tip_Budget".Translate());
                budgetH += 4f;
            }

            float searchY = rect.y + headerH + 4f + budgetH;
            float searchH = 26f;
            _vehicleSearchFilter = Widgets.TextField(new Rect(rect.x, searchY, rect.width - 20f, searchH), _vehicleSearchFilter);

            if (allKinds.NullOrEmpty())
            {
                Widgets.Label(new Rect(rect.x, searchY + searchH + 4f, rect.width, 24f), "VRF_Settings_NoVehicles".Translate());
                return;
            }

            string filter = _vehicleSearchFilter?.Trim().ToLowerInvariant() ?? "";
            var filtered  = string.IsNullOrEmpty(filter) ? allKinds
                : allKinds.Where(k => (k.label ?? k.defName).ToLowerInvariant().Contains(filter)).ToList();
            var groups    = filtered.GroupBy(k => k.modContentPack?.Name ?? "Unknown").OrderBy(g => g.Key).ToList();

            const float ModHeaderH = 28f;
            const float SepH       = 10f;
            float totalH = groups.Sum(g => ModHeaderH + 4f + g.Count() * RowHeight + SepH);

            float listTop   = searchY + searchH + 4f;
            Rect scrollArea = new Rect(rect.x, listTop, rect.width, rect.height - listTop + rect.y);
            Rect viewRect   = new Rect(0f, 0f, scrollArea.width - 20f, totalH);
            Widgets.BeginScrollView(scrollArea, ref _vehicleScrollPos, viewRect);

            float vy = 0f; int altIdx = 0;
            foreach (var group in groups)
            {
                GUI.color   = new Color(0.9f, 0.82f, 0.5f);
                Text.Font   = GameFont.Small;
                Widgets.DrawBoxSolid(new Rect(0f, vy, viewRect.width, ModHeaderH), new Color(0.12f, 0.12f, 0.12f, 0.85f));
                Text.Anchor = TextAnchor.MiddleLeft;
                Widgets.Label(new Rect(Pad, vy, viewRect.width - Pad, ModHeaderH), group.Key);
                Text.Anchor = TextAnchor.UpperLeft;
                GUI.color   = Color.white;
                vy += ModHeaderH + 4f;

                foreach (PawnKindDef kind in group)
                {
                    bool isGravship = IsGravship(kind.race as VehicleDef, kind);
                    // Gravships are configured only as VMF structures/presets.  Do not create a
                    // normal vehicle entry merely because the general list was drawn.
                    var entry = isGravship ? null : factionConfig.GetOrCreateForContext(kind.defName, ctx);
                    Rect row  = new Rect(0f, vy, viewRect.width, RowHeight - 2f);
                    if (altIdx % 2 == 0) Widgets.DrawAltRect(row);
                    altIdx++;
                    Widgets.DrawHighlightIfMouseover(row);

                    float cx = row.x + Pad;
                    if (!isGravship)
                    {
                        bool wasEnabled = entry.enabled;
                        Widgets.Checkbox(cx, row.y + (row.height - 24f) * 0.5f, ref entry.enabled, 24f);
                        if (entry.enabled != wasEnabled) VRF_Mod.Instance.WriteSettings();
                        cx += 24f + Pad;
                    }

                    float thumbSz = row.height - 4f;
                    DrawVehicleThumb(new Rect(cx, row.y + 2f, thumbSz, thumbSz), kind, entry);
                    cx += thumbSz + Pad;

                    float labelW = row.width - cx - BtnW - Pad * 2f;
                    Text.Anchor  = TextAnchor.MiddleLeft;
                    float dcp    = entry != null && entry.combatPowerOverride > 0f ? entry.combatPowerOverride : kind.combatPower;
                    string cpStr = isGravship
                        ? "Configuración exclusiva: presets de Gravship"
                        : "VRF_Settings_CombatPower".Translate() + ": " + dcp.ToString("F0");
                    Rect labelR  = new Rect(cx, row.y, labelW, row.height);
                    Widgets.Label(labelR, $"{kind.label ?? kind.defName}\n<color=#aaaaaa>{cpStr}</color>");
                    if (Mouse.IsOver(labelR)) TooltipHandler.TipRegion(labelR, VRF_CombatPowerEstimator.BuildBreakdown(kind));
                    Text.Anchor = TextAnchor.UpperLeft;

                    if (Widgets.ButtonText(new Rect(row.xMax - BtnW - Pad, row.y + (row.height - 26f) * 0.5f, BtnW, 26f), "VRF_Settings_Details".Translate()))
                    {
                        _selectedVehicleKind = kind;
                        _page = VRF_SettingsPage.VehicleDetail;
                        _detailScrollPos = Vector2.zero;
                    }
                    vy += RowHeight;
                }
                GUI.color = new Color(0.4f, 0.4f, 0.4f, 0.8f);
                Widgets.DrawLineHorizontal(0f, vy + SepH * 0.5f - 1f, viewRect.width);
                GUI.color = Color.white;
                vy += SepH;
            }
            Widgets.EndScrollView();
        }

                public static bool IsGravship(VehicleDef vDef, PawnKindDef kind = null)
        {
            if (vDef != null)
            {
                if (vDef.defName.IndexOf("grav", System.StringComparison.OrdinalIgnoreCase) >= 0)
                    return true;
                if (vDef.thingClass != null && vDef.thingClass.FullName != null && vDef.thingClass.FullName.Contains("VehiclePawnWithMap"))
                    return true;
            }
            if (kind != null)
            {
                if (kind.defName.IndexOf("grav", System.StringComparison.OrdinalIgnoreCase) >= 0)
                    return true;
                if (kind.race is VehicleDef vd && vd.thingClass != null && vd.thingClass.FullName != null && vd.thingClass.FullName.Contains("VehiclePawnWithMap"))
                    return true;
            }
            return false;
        }

        private static void DrawVehicleDetail(Rect rect, VRF_SpawnContext ctx)
        {
            if (_selectedVehicleKind == null) { _page = VRF_SettingsPage.VehicleList; return; }
            if (_selectedFaction == null)     { _page = VRF_SettingsPage.FactionList; return; }

            PawnKindDef kind = _selectedVehicleKind;
            VehicleDef  vDef = kind.race as VehicleDef;
            var factionConfig = VRF_Mod.Settings.GetOrCreateFactionConfig(_selectedFaction.defName);
            bool isGrav = IsGravship(vDef, kind);
            if (isGrav)
            {
                DrawGravshipDetail(rect, kind, vDef, factionConfig);
                return;
            }

            var entry = factionConfig.GetOrCreateForContext(kind.defName, ctx);

            if (entry.combatPowerOverride <= 0f) entry.combatPowerOverride = kind.combatPower;
            if (entry.minRaidPoints       <= 0f) entry.minRaidPoints       = entry.combatPowerOverride;

            string bufKey = _selectedFaction.defName + "_" + ctx.ToString() + "_" + kind.defName;
            if (!_cpBuffers.TryGetValue(bufKey,  out string cpBuf)  || cpBuf  == null) { cpBuf  = ((int)entry.combatPowerOverride).ToString(); _cpBuffers[bufKey]  = cpBuf; }
            if (!_mrpBuffers.TryGetValue(bufKey, out string mrpBuf) || mrpBuf == null) { mrpBuf = ((int)entry.minRaidPoints).ToString();       _mrpBuffers[bufKey] = mrpBuf; }
            if (!_mxpBuffers.TryGetValue(bufKey, out string mxpBuf) || mxpBuf == null) { mxpBuf = ((int)entry.maxRaidPoints).ToString();       _mxpBuffers[bufKey] = mxpBuf; }

            float previewSize     = Mathf.Min(rect.width * 0.38f, 200f);
            float totalContentH   = CalcDetailContentHeight(kind, vDef, entry, previewSize, ctx);
            bool  needsScroll     = totalContentH > rect.height;
            float viewW           = needsScroll ? rect.width - 20f : rect.width;
            Rect  viewRect        = new Rect(0f, 0f, viewW, Mathf.Max(totalContentH, rect.height));

            Widgets.BeginScrollView(rect, ref _detailScrollPos, viewRect);

            float localInfoX = previewSize + Pad * 2f;
            float localInfoW = viewW - localInfoX;

            Rect previewBox = new Rect(0f, 0f, previewSize, previewSize);
            Widgets.DrawBoxSolid(previewBox, new Color(0.08f, 0.08f, 0.08f, 0.9f));
            Widgets.DrawBox(previewBox, 1);
            if (vDef != null) DrawVehicleWithTurrets(previewBox.ContractedBy(6f), vDef, entry);
            else              DrawVehicleThumb(previewBox.ContractedBy(6f), kind);

            float iy = 0f;
            Text.Font = GameFont.Medium;
            Widgets.Label(new Rect(localInfoX, iy, localInfoW, 32f), kind.label ?? kind.defName);
            iy += 36f;
            Text.Font = GameFont.Small;

            if (vDef != null)
            {
                Widgets.Label(new Rect(localInfoX, iy, localInfoW, 22f), "VRF_Settings_TechLevel".Translate(vDef.techLevel.ToStringHuman())); iy += 24f;
                Widgets.Label(new Rect(localInfoX, iy, localInfoW, 22f), "VRF_Settings_VehicleType".Translate(vDef.type.ToString()));         iy += 24f;
            }
            iy += 4f;

            bool wasEnabled = entry.enabled;
            Widgets.CheckboxLabeled(new Rect(localInfoX, iy, localInfoW, 26f), "VRF_Settings_EnableForRaids".Translate(), ref entry.enabled);
            if (entry.enabled != wasEnabled) VRF_Mod.Instance.WriteSettings();
            iy += 28f;

            if (entry.enabled)
            {
                GUI.color = new Color(0.55f, 1f, 0.55f);
                Widgets.Label(new Rect(localInfoX, iy, localInfoW, 22f), "VRF_Settings_VehicleActive".Translate());
            }
            else
            {
                GUI.color = new Color(0.65f, 0.65f, 0.65f);
                Widgets.Label(new Rect(localInfoX, iy, localInfoW, 22f), "VRF_Settings_VehicleInactive".Translate());
            }
            GUI.color = Color.white;
            iy += 26f;

            if (ctx == VRF_SpawnContext.Raid)
            {
                iy += 4f;
                int allowedCount = entry.allowedRaidStrategies.Count == 0
                    ? DefDatabase<RaidStrategyDef>.DefCount
                    : DefDatabase<RaidStrategyDef>.DefCount - entry.allowedRaidStrategies.Count;
                string btnLabel = "VRF_Settings_RaidStrategies".Translate() +
                    $" ({allowedCount}/{DefDatabase<RaidStrategyDef>.DefCount})";
                if (Widgets.ButtonText(new Rect(localInfoX, iy, localInfoW, 26f), btnLabel))
                    Find.WindowStack.Add(new Dialog_VRF_RaidStrategies(entry, kind.label ?? kind.defName));
                iy += 30f;
            }

            if (ctx != VRF_SpawnContext.Raid)
            {
                iy += 4f;
                bool wasForce = entry.forceSpawn;
                Widgets.CheckboxLabeled(new Rect(localInfoX, iy, localInfoW, 26f), "VRF_Settings_ForceSpawn".Translate(), ref entry.forceSpawn);
                if (entry.forceSpawn != wasForce) VRF_Mod.Instance.WriteSettings();
                if (Mouse.IsOver(new Rect(localInfoX, iy, localInfoW, 26f)))
                    TooltipHandler.TipRegion(new Rect(localInfoX, iy, localInfoW, 26f), "VRF_Settings_ForceSpawnDesc".Translate());
                iy += 28f;
            }

            // If NOT gravship, draw combat power and min/max points fields
            if (!isGrav)
            {
                iy += 6f;
                Widgets.Label(new Rect(localInfoX, iy, localInfoW, 22f), "VRF_Settings_CombatPower".Translate());
                iy += 22f;
                {
                    float cpFieldW = Mathf.Min(localInfoW, 120f);
                    float prevCp = entry.combatPowerOverride;
                    Widgets.TextFieldNumeric(new Rect(localInfoX, iy, cpFieldW, 24f), ref entry.combatPowerOverride, ref cpBuf, 1f, 999999f);
                    _cpBuffers[bufKey] = cpBuf;
                    if (entry.combatPowerOverride != prevCp) VRF_Mod.Instance.WriteSettings();
                    Rect cpTipRect = new Rect(localInfoX + cpFieldW + 6f, iy + 3f, 18f, 18f);
                    GUI.color = Color.yellow;
                    Widgets.Label(cpTipRect, "?");
                    GUI.color = Color.white;
                    if (Mouse.IsOver(cpTipRect))
                        TooltipHandler.TipRegion(cpTipRect, "VRF_Settings_Tip_CombatPower".Translate());
                }
                iy += 30f;

                const float QMarkW  = 24f;
                const float ColGap  = 20f;
                float colW  = (localInfoW - ColGap) * 0.5f;
                float fieldW = Mathf.Min(colW - QMarkW - 4f, 100f);
                float mxpX   = localInfoX + colW + ColGap;

                Widgets.Label(new Rect(localInfoX, iy, colW, 22f), "VRF_Settings_MinCombatPoints".Translate());
                Widgets.Label(new Rect(mxpX,       iy, colW, 22f), "VRF_Settings_MaxCombatPoints".Translate());
                iy += 22f;

                {
                    float prevMrp = entry.minRaidPoints;
                    Widgets.TextFieldNumeric(new Rect(localInfoX, iy, fieldW, 24f), ref entry.minRaidPoints, ref mrpBuf, 0f, 999999f);
                    _mrpBuffers[bufKey] = mrpBuf;
                    if (entry.minRaidPoints != prevMrp) VRF_Mod.Instance.WriteSettings();
                    Rect minTipRect = new Rect(localInfoX + fieldW + 4f, iy + 3f, 18f, 18f);
                    GUI.color = Color.yellow;
                    Widgets.Label(minTipRect, "?");
                    GUI.color = Color.white;
                    if (Mouse.IsOver(minTipRect))
                        TooltipHandler.TipRegion(minTipRect, "VRF_Settings_Tip_MinPoints".Translate());
                }
                {
                    float prevMxp = entry.maxRaidPoints;
                    Widgets.TextFieldNumeric(new Rect(mxpX, iy, fieldW, 24f), ref entry.maxRaidPoints, ref mxpBuf, 0f, 999999f);
                    _mxpBuffers[bufKey] = mxpBuf;
                    if (entry.maxRaidPoints != prevMxp) VRF_Mod.Instance.WriteSettings();
                    Rect maxTipRect = new Rect(mxpX + fieldW + 4f, iy + 3f, 18f, 18f);
                    GUI.color = Color.yellow;
                    Widgets.Label(maxTipRect, "?");
                    GUI.color = Color.white;
                    if (Mouse.IsOver(maxTipRect))
                        TooltipHandler.TipRegion(maxTipRect, "VRF_Settings_Tip_MaxPoints".Translate());
                }
                iy += 28f;
            }

            float topSectionBottom = Mathf.Max(previewBox.yMax, iy) + Pad;

            if (isGrav)
            {
                // Flight mode section for gravship (only Desactivado / Gravship)
                float flightH = CalcFlightModeHeight(entry);
                DrawFlightModeSection(new Rect(0f, topSectionBottom, viewW, flightH), vDef, entry, bufKey);
                float listY = topSectionBottom + flightH + Pad;

                // Gravship structures list
                float listH = Mathf.Max(viewRect.height - listY - Pad, 260f);
                DrawGravshipStructuresList(new Rect(0f, listY, viewW, listH), vDef, factionConfig, kind);
            }
            else if (vDef != null)
            {
                float sliderH  = CalcSliderAreaHeight(vDef, entry);
                float flightH  = (vDef.type == VehicleType.Air) ? CalcFlightModeHeight(entry) + Pad : 0f;
                float paintH   = CalcPaintSectionHeight(vDef, entry);
                float upgradeH = CalcUpgradeSectionHeight(vDef, entry);

                DrawResourceSliders(new Rect(0f, topSectionBottom, viewW, sliderH), vDef, entry);
                if (vDef.type == VehicleType.Air)
                    DrawFlightModeSection(new Rect(0f, topSectionBottom + sliderH + Pad, viewW, CalcFlightModeHeight(entry)), vDef, entry, bufKey);
                float paintY   = topSectionBottom + sliderH + flightH + Pad;
                DrawPaintSection(new Rect(0f, paintY, viewW, paintH), vDef, entry);
                float upgradeY = paintY + paintH + Pad;
                DrawUpgradeSection(new Rect(0f, upgradeY, viewW, upgradeH), vDef, entry);
            }

            Widgets.EndScrollView();
        }

        private static void DrawGravshipDetail(Rect rect, PawnKindDef kind, VehicleDef vDef, VRF_NaturalRaidFactionConfig factionConfig)
        {
            Text.Font = GameFont.Medium;
            Widgets.Label(new Rect(rect.x, rect.y, rect.width, 30f), kind.label ?? kind.defName);
            Text.Font = GameFont.Small;

            GUI.color = new Color(0.75f, 0.85f, 1f);
            Widgets.Label(new Rect(rect.x, rect.y + 34f, rect.width, 42f),
                VehicleMapFramework.VRF_GravshipPresetUtility.IsGravshipVehicleDef(vDef)
                    ? "Las gravships solo aparecen mediante presets habilitados. Cada preset controla sus propios puntos, rango y estrategias de raid."
                    : "Los vehículos con mapa de Vehicle Map Framework solo aparecen mediante presets habilitados. Cada preset controla sus propios puntos, rango y estrategias de raid.");
            GUI.color = Color.white;

            DrawGravshipStructuresList(new Rect(rect.x, rect.y + 82f, rect.width, rect.height - 82f), vDef, factionConfig, kind);
        }

        private static System.Reflection.MethodInfo _loadImageMethod;
        private static bool _loadImageMethodSearched;

        private static bool TryLoadImageToTexture(Texture2D tex, byte[] bytes)
        {
            if (!_loadImageMethodSearched)
            {
                _loadImageMethodSearched = true;
                foreach (var asm in System.AppDomain.CurrentDomain.GetAssemblies())
                {
                    var type = asm.GetType("UnityEngine.ImageConversion");
                    if (type != null)
                    {
                        _loadImageMethod = type.GetMethod("LoadImage", new System.Type[] { typeof(Texture2D), typeof(byte[]) });
                        if (_loadImageMethod != null) break;
                    }
                }
            }
            if (_loadImageMethod != null)
            {
                try
                {
                    return (bool)_loadImageMethod.Invoke(null, new object[] { tex, bytes });
                }
                catch { }
            }
            return false;
        }

        public static void DrawGravshipPresetStructureThumbnail(Rect thumbRect, string filePath, VehicleDef vDef, PawnKindDef kind = null)
        {
            // 1. Check if a pre-rendered or saved PNG thumbnail exists alongside the JSON preset
            string pngPath = System.IO.Path.ChangeExtension(filePath, ".png");
            if (System.IO.File.Exists(pngPath))
            {
                if (!_cachedPresetTextures.TryGetValue(pngPath, out Texture2D pngTex) || pngTex == null)
                {
                    try
                    {
                        pngTex = new Texture2D(2, 2);
                        byte[] bytes = System.IO.File.ReadAllBytes(pngPath);
                        if (TryLoadImageToTexture(pngTex, bytes))
                        {
                            _cachedPresetTextures[pngPath] = pngTex;
                        }
                    }
                    catch
                    {
                        pngTex = null;
                    }
                }

                if (pngTex != null)
                {
                    GUI.DrawTexture(thumbRect.ContractedBy(2f), pngTex, ScaleMode.ScaleToFit);
                    return;
                }
            }

            // 2. Load or retrieve cached preset data
            VehicleMapFramework.VRF_GravshipPresetData data = null;
            try
            {
                var fileInfo = new System.IO.FileInfo(filePath);
                if (fileInfo.Exists)
                {
                    if (_cachedPresetData.TryGetValue(filePath, out CachedPresetInfo cached) && cached != null && cached.lastWrite == fileInfo.LastWriteTimeUtc)
                    {
                        data = cached.data;
                    }
                    else
                    {
                        data = VehicleMapFramework.VRF_GravshipPresetUtility.LoadGravshipPresetFromFile(filePath);
                        _cachedPresetData[filePath] = new CachedPresetInfo { lastWrite = fileInfo.LastWriteTimeUtc, data = data };
                    }
                }
            }
            catch { }

            // Fallback if data is missing or empty
            if (data == null || ((data.cells == null || data.cells.Count == 0) && (data.buildings == null || data.buildings.Count == 0)))
            {
                if (vDef != null) DrawVehicleWithTurrets(thumbRect.ContractedBy(4f), vDef);
                else if (kind != null) DrawVehicleThumb(thumbRect.ContractedBy(4f), kind);
                return;
            }

            // 3. Procedural blueprint rendering of the structure
            // Calculate bounding coordinates using GenAdj.OccupiedRect for exact multi-cell building bounds
            int minX = int.MaxValue, maxX = int.MinValue;
            int minZ = int.MaxValue, maxZ = int.MinValue;

            if (data.cells != null)
            {
                for (int i = 0; i < data.cells.Count; i++)
                {
                    var c = data.cells[i];
                    if (c.offsetX < minX) minX = c.offsetX;
                    if (c.offsetX > maxX) maxX = c.offsetX;
                    if (c.offsetZ < minZ) minZ = c.offsetZ;
                    if (c.offsetZ > maxZ) maxZ = c.offsetZ;
                }
            }

            if (data.buildings != null)
            {
                for (int i = 0; i < data.buildings.Count; i++)
                {
                    var b = data.buildings[i];
                    ThingDef bDef = DefDatabase<ThingDef>.GetNamedSilentFail(b.defName);
                    Rot4 r = new Rot4(b.rotation);
                    CellRect occ = (bDef != null)
                        ? GenAdj.OccupiedRect(new IntVec3(b.offsetX, 0, b.offsetZ), r, bDef.size)
                        : new CellRect(b.offsetX, b.offsetZ, 1, 1);

                    if (occ.minX < minX) minX = occ.minX;
                    if (occ.maxX > maxX) maxX = occ.maxX;
                    if (occ.minZ < minZ) minZ = occ.minZ;
                    if (occ.maxZ > maxZ) maxZ = occ.maxZ;
                }
            }

            if (minX > maxX || minZ > maxZ)
            {
                if (vDef != null) DrawVehicleWithTurrets(thumbRect.ContractedBy(4f), vDef);
                return;
            }

            Rect drawBox = thumbRect.ContractedBy(4f);
            int gridW = maxX - minX + 1;
            int gridH = maxZ - minZ + 1;

            float cellPx = Mathf.Min(drawBox.width / (gridW + 0.5f), drawBox.height / (gridH + 0.5f));
            float renderW = gridW * cellPx;
            float renderH = gridH * cellPx;
            float startX = drawBox.x + (drawBox.width - renderW) * 0.5f;
            float startY = drawBox.y + (drawBox.height - renderH) * 0.5f;

            // A. Draw floor / foundation cells
            Color floorColor = new Color(0.24f, 0.32f, 0.38f, 0.90f);
            if (data.cells != null)
            {
                for (int i = 0; i < data.cells.Count; i++)
                {
                    var c = data.cells[i];
                    float cx = startX + (c.offsetX - minX) * cellPx;
                    float cy = startY + (maxZ - c.offsetZ) * cellPx;
                    Rect cRect = new Rect(cx, cy, cellPx, cellPx);

                    Widgets.DrawBoxSolid(cRect, floorColor);
                    if (cellPx >= 3.5f)
                    {
                        Widgets.DrawBox(cRect, 1);
                    }
                }
            }

            // B. Draw buildings
            if (data.buildings != null)
            {
                for (int i = 0; i < data.buildings.Count; i++)
                {
                    var b = data.buildings[i];
                    ThingDef bDef = DefDatabase<ThingDef>.GetNamedSilentFail(b.defName);
                    if (bDef == null) continue;

                    Rot4 r = new Rot4(b.rotation);
                    CellRect occ = GenAdj.OccupiedRect(new IntVec3(b.offsetX, 0, b.offsetZ), r, bDef.size);

                    float bx = startX + (occ.minX - minX) * cellPx;
                    float by = startY + (maxZ - occ.maxZ) * cellPx;
                    float bw = occ.Width * cellPx;
                    float bh = occ.Height * cellPx;
                    Rect bRect = new Rect(bx, by, bw, bh);

                    bool isEngine = bDef == ThingDefOf.GravEngine || (bDef.defName != null && (bDef.defName.Contains("GravEngine") || bDef.defName.Contains("Gravcore")));

                    Color bColor = bDef.uiIconColor;
                    if (!string.IsNullOrEmpty(b.stuffDef))
                    {
                        ThingDef stuffDef = DefDatabase<ThingDef>.GetNamedSilentFail(b.stuffDef);
                        if (stuffDef?.stuffProps != null)
                        {
                            bColor = stuffDef.stuffProps.color;
                        }
                    }

                    // 1. Draw solid cell footprint for every building cell
                    // Walls / impassable objects get opaque filling, other structures get tinted base
                    if (isEngine)
                    {
                        Widgets.DrawBoxSolid(bRect.ExpandedBy(1f), new Color(0f, 0.85f, 1f, 0.65f));
                    }
                    else if (bDef.IsWall || bDef.passability == Traversability.Impassable)
                    {
                        Widgets.DrawBoxSolid(bRect, bColor);
                    }
                    else
                    {
                        Color footColor = new Color(bColor.r * 0.35f + 0.1f, bColor.g * 0.35f + 0.1f, bColor.b * 0.35f + 0.1f, 0.75f);
                        Widgets.DrawBoxSolid(bRect, footColor);
                    }

                    // 2. Draw building icon stretching across all occupied cells
                    Texture2D icon = bDef.uiIcon;
                    if (icon != null && !bDef.IsWall)
                    {
                        GUI.color = isEngine ? new Color(0.35f, 1f, 1f, 1f) : bColor;
                        if (r != Rot4.North && r.AsAngle != 0f)
                        {
                            Matrix4x4 m = GUI.matrix;
                            GUIUtility.RotateAroundPivot(r.AsAngle, bRect.center);
                            GUI.DrawTexture(bRect, icon, ScaleMode.StretchToFill, true);
                            GUI.matrix = m;
                        }
                        else
                        {
                            GUI.DrawTexture(bRect, icon, ScaleMode.StretchToFill, true);
                        }
                        GUI.color = Color.white;
                    }

                    if (cellPx >= 3f)
                    {
                        Widgets.DrawBox(bRect, 1);
                    }
                }
            }
        }

        private static void DrawGravshipStructuresList(Rect rect, VehicleDef vDef, VRF_NaturalRaidFactionConfig factionConfig, PawnKindDef kind)
        {
            bool isActualGravship = VehicleMapFramework.VRF_GravshipPresetUtility.IsGravshipVehicleDef(vDef);
            var presetFiles = isActualGravship
                ? VehicleMapFramework.VRF_GravshipPresetUtility.FindAllGravshipPresetFiles()
                : VehicleMapFramework.VRF_GravshipPresetUtility.FindAllVehicleStructurePresetFiles(vDef);
            float y = rect.y;

            GUI.color = new Color(0.7f, 0.85f, 1f);
            Text.Font = GameFont.Medium;
            Widgets.Label(new Rect(rect.x, y, rect.width, 30f), isActualGravship ? "Estructuras Gravship Disponibles" : "Estructuras VMF Disponibles");
            GUI.color = Color.white;
            y += 34f;
            Text.Font = GameFont.Small;

            if (presetFiles == null || presetFiles.Count == 0)
            {
                GUI.color = new Color(0.75f, 0.75f, 0.75f);
                Widgets.Label(new Rect(rect.x, y, rect.width, 26f), "No se encontraron estructuras/presets guardados en las carpetas de Mods o Config.");
                GUI.color = Color.white;
                return;
            }

            const float cardH = 76f;
            const float cardPad = 8f;

            float listVisibleH = Mathf.Max(rect.height - 34f, 220f);
            float totalH = presetFiles.Count * (cardH + cardPad);
            bool needsScroll = totalH > listVisibleH;

            Rect outRect = new Rect(rect.x, y, rect.width, listVisibleH);
            Rect viewRect = new Rect(0f, 0f, outRect.width - (needsScroll ? 16f : 0f), Mathf.Max(totalH, listVisibleH));

            Widgets.BeginScrollView(outRect, ref _gravshipListScrollPos, viewRect);

            float curY = 0f;
            for (int i = 0; i < presetFiles.Count; i++)
            {
                string filePath = presetFiles[i];
                string pName = System.IO.Path.GetFileNameWithoutExtension(filePath);
                var gEntry = factionConfig.GetOrCreateGravshipEntry(pName);

                Rect cardRect = new Rect(0f, curY, viewRect.width, cardH);
                Widgets.DrawBoxSolid(cardRect, new Color(0.12f, 0.12f, 0.12f, 0.75f));
                Widgets.DrawBox(cardRect, 1);
                Widgets.DrawHighlightIfMouseover(cardRect);

                // Invisible click area on the left/center of the card to open editor
                Rect clickArea = new Rect(cardRect.x, cardRect.y, cardRect.width - 220f, cardRect.height);
                if (Widgets.ButtonInvisible(clickArea))
                {
                    Find.WindowStack.Add(new Dialog_VRF_GravshipStructureEditor(gEntry, vDef, filePath));
                }

                // Rendered vehicle / structure thumbnail
                Rect thumbRect = new Rect(cardRect.x + 6f, cardRect.y + 6f, 64f, 64f);
                Widgets.DrawBoxSolid(thumbRect, new Color(0.04f, 0.05f, 0.07f, 0.95f));
                Widgets.DrawBox(thumbRect, 1);
                DrawGravshipPresetStructureThumbnail(thumbRect, filePath, vDef, kind);

                // Stats and labels
                float infoX = thumbRect.xMax + 10f;
                float infoW = cardRect.width - infoX - 225f;

                Text.Font = GameFont.Medium;
                Widgets.Label(new Rect(infoX, cardRect.y + 5f, infoW, 26f), pName);
                Text.Font = GameFont.Small;

                string maxPtsStr = gEntry.maxRaidPoints > 0f ? gEntry.maxRaidPoints.ToString("F0") : "Sin límite";
                string statsText = $"Poder: {gEntry.combatPower:F0}   |   Min: {gEntry.minRaidPoints:F0}   |   Max: {maxPtsStr}";
                GUI.color = new Color(0.85f, 0.85f, 0.85f);
                Widgets.Label(new Rect(infoX, cardRect.y + 30f, infoW, 20f), statsText);

                if (gEntry.enabled)
                {
                    GUI.color = new Color(0.55f, 1f, 0.55f);
                    Widgets.Label(new Rect(infoX, cardRect.y + 50f, infoW, 20f), "Activo en raids naturales");
                }
                else
                {
                    GUI.color = new Color(0.6f, 0.6f, 0.6f);
                    Widgets.Label(new Rect(infoX, cardRect.y + 50f, infoW, 20f), "Inactivo");
                }
                GUI.color = Color.white;

                // Right side: Checkbox, Estrategias, Editar
                float rightX = cardRect.xMax - 215f;

                bool wasEn = gEntry.enabled;
                Widgets.CheckboxLabeled(new Rect(rightX, cardRect.y + 6f, 110f, 24f), "Habilitar", ref gEntry.enabled);
                if (gEntry.enabled != wasEn) VRF_Mod.Instance.WriteSettings();

                int totalStrats = DefDatabase<RaidStrategyDef>.DefCount;
                int allowedCount = gEntry.allowedRaidStrategies.Count == 0
                    ? totalStrats
                    : totalStrats - gEntry.allowedRaidStrategies.Count;
                string stratLabel = $"Estrategias ({allowedCount}/{totalStrats})";
                if (Widgets.ButtonText(new Rect(rightX, cardRect.y + 38f, 125f, 26f), stratLabel))
                {
                    Find.WindowStack.Add(new Dialog_VRF_RaidStrategies(gEntry, pName));
                }

                if (Widgets.ButtonText(new Rect(cardRect.xMax - 80f, cardRect.y + 38f, 74f, 26f), "Editar"))
                {
                    Find.WindowStack.Add(new Dialog_VRF_GravshipStructureEditor(gEntry, vDef, filePath));
                }

                curY += cardH + cardPad;
            }

            Widgets.EndScrollView();
        }

        private static float CalcDetailContentHeight(PawnKindDef kind, VehicleDef vDef, VRF_NaturalRaidVehicleEntry entry, float previewSize, VRF_SpawnContext ctx)
        {
            bool isGrav = IsGravship(vDef, kind);

            float infoH = 36f;
            if (vDef != null) infoH += 48f;
            infoH += 4f + 28f + 26f;
            if (ctx == VRF_SpawnContext.Raid) infoH += 4f + 30f; 
            if (ctx != VRF_SpawnContext.Raid) infoH += 4f + 28f;

            if (!isGrav)
            {
                infoH += 6f + 22f + 30f + 22f + 28f + 28f;
            }

            float topSectionBottom = Mathf.Max(previewSize, infoH) + Pad;

            if (isGrav)
            {
                float flightH = CalcFlightModeHeight(entry) + Pad;
                bool isActualGravship = VehicleMapFramework.VRF_GravshipPresetUtility.IsGravshipVehicleDef(vDef);
                var presetFiles = isActualGravship
                    ? VehicleMapFramework.VRF_GravshipPresetUtility.FindAllGravshipPresetFiles()
                    : VehicleMapFramework.VRF_GravshipPresetUtility.FindAllVehicleStructurePresetFiles(vDef);
                int count = presetFiles != null ? presetFiles.Count : 0;
                float listH = 34f + Mathf.Clamp(count * 84f, 180f, 380f);
                return topSectionBottom + flightH + listH + Pad;
            }

            float sliderH  = vDef != null ? CalcSliderAreaHeight(vDef, entry) : 0f;
            float flightH2 = (vDef != null && vDef.type == VehicleType.Air) ? CalcFlightModeHeight(entry) + Pad : 0f;
            float paintH   = vDef != null ? CalcPaintSectionHeight(vDef, entry) + Pad : 0f;
            float upgradeH = vDef != null ? CalcUpgradeSectionHeight(vDef, entry) + Pad : 0f;
            return topSectionBottom + sliderH + flightH2 + paintH + upgradeH + Pad;
        }

private static VehicleDef _selectedHoverVehicle;
        private static Vector2   _hoverDetailScrollPos;
        private static string    _hoverVehicleSearchFilter = "";

        private static void DrawHoverTab(Rect rect)
        {
            if (_selectedHoverVehicle == null)
                DrawHoverVehicleList(rect);
            else
                DrawHoverVehicleDetail(rect);
        }

        private static void DrawHoverVehicleList(Rect rect)
        {
            var allVehicles = DefDatabase<VehicleDef>.AllDefsListForReading
                .OrderBy(v => v.label ?? v.defName)
                .ToList();

            float topH = 30f;
            Rect topBar = new Rect(rect.x, rect.y, rect.width, topH);

            if (Widgets.ButtonText(new Rect(topBar.xMax - BtnW * 2f - Pad, topBar.y + 2f, BtnW, 26f), "VRF_Settings_Presets".Translate()))
                _showPresetPanel = !_showPresetPanel;

            float searchY = rect.y + topH + 4f;
            _hoverVehicleSearchFilter = Widgets.TextField(new Rect(rect.x, searchY, rect.width - 20f, 26f), _hoverVehicleSearchFilter);

            string filter = _hoverVehicleSearchFilter?.Trim().ToLowerInvariant() ?? "";
            var filtered = string.IsNullOrEmpty(filter)
                ? allVehicles
                : allVehicles.Where(v => (v.label ?? v.defName).ToLowerInvariant().Contains(filter)).ToList();

            var groups = filtered.GroupBy(v => v.modContentPack?.Name ?? "Unknown").OrderBy(g => g.Key).ToList();

            const float ModHeaderH = 28f;
            const float SepH       = 10f;
            float totalH = groups.Sum(g => ModHeaderH + 4f + g.Count() * RowHeight + SepH);

            if (_showPresetPanel)
            {
                float panelW  = Mathf.Min(rect.width * 0.4f, 320f);
                Rect  panelR  = new Rect(rect.xMax - panelW, searchY + 30f, panelW, rect.height - topH - 34f);
                DrawPresetPanel(panelR);
                rect = new Rect(rect.x, rect.y, rect.width - panelW - Pad, rect.height);
            }

            float listTop   = searchY + 30f;
            Rect scrollArea = new Rect(rect.x, listTop, rect.width, rect.height - listTop + rect.y);
            Rect viewRect   = new Rect(0f, 0f, scrollArea.width - 20f, totalH);
            Widgets.BeginScrollView(scrollArea, ref _hoverScrollPos, viewRect);

            float vy = 0f; int altIdx = 0;
            foreach (var group in groups)
            {
                GUI.color   = new Color(0.9f, 0.82f, 0.5f);
                Widgets.DrawBoxSolid(new Rect(0f, vy, viewRect.width, ModHeaderH), new Color(0.12f, 0.12f, 0.12f, 0.85f));
                Text.Anchor = TextAnchor.MiddleLeft;
                Widgets.Label(new Rect(Pad, vy, viewRect.width - Pad, ModHeaderH), group.Key);
                Text.Anchor = TextAnchor.UpperLeft;
                GUI.color   = Color.white;
                vy += ModHeaderH + 4f;

                foreach (VehicleDef vDef in group)
                {
                    Rect row = new Rect(0f, vy, viewRect.width, RowHeight - 2f);
                    if (altIdx % 2 == 0) Widgets.DrawAltRect(row);
                    altIdx++;
                    Widgets.DrawHighlightIfMouseover(row);

                    bool hasHover = vDef.comps != null && vDef.comps.OfType<CompProperties_VehicleHover>().Any();
                    bool hasConfig = VRF_Mod.Settings.GetHoverConfig(vDef.defName) != null;

                    float thumbSz = row.height - 4f;
                    Rect thumbR = new Rect(row.x + Pad, row.y + 2f, thumbSz, thumbSz);
                    if (Event.current.type == EventType.Repaint)
                    {
                        PatternData pd = BuildPatternDataSafe(vDef);
                        VehicleGui.DrawVehicleDefOnGUI(thumbR, vDef, pd, Rot8.East);
                    }

                    float labelX = thumbR.xMax + Pad;
                    float labelW = row.width - labelX - BtnW - Pad * 2f;
                    Text.Anchor  = TextAnchor.MiddleLeft;
                    string status = hasHover
                        ? "<color=#88ff88>Hover</color>"
                        : (hasConfig ? "<color=#ffdd66>Config</color>" : "<color=#888888>No hover</color>");
                    Widgets.Label(new Rect(labelX, row.y, labelW, row.height), $"{vDef.label ?? vDef.defName}\n<color=#aaaaaa>{status}</color>");
                    Text.Anchor = TextAnchor.UpperLeft;

                    if (Widgets.ButtonText(new Rect(row.xMax - BtnW - Pad, row.y + (row.height - 26f) * 0.5f, BtnW, 26f), "VRF_Settings_Details".Translate()))
                    {
                        _selectedHoverVehicle = vDef;
                        _hoverDetailScrollPos = Vector2.zero;
                        SyncHoverBufsFromConfig(vDef);
                    }
                    vy += RowHeight;
                }

                GUI.color = new Color(0.4f, 0.4f, 0.4f, 0.8f);
                Widgets.DrawLineHorizontal(0f, vy + SepH * 0.5f - 1f, viewRect.width);
                GUI.color = Color.white;
                vy += SepH;
            }
            Widgets.EndScrollView();
        }

        private static void SyncHoverBufsFromConfig(VehicleDef vDef)
        {
            if (!_hoverBufs.TryGetValue(vDef.defName, out var bufs))
            {
                bufs = new Dictionary<string, string>();
                _hoverBufs[vDef.defName] = bufs;
            }
            bufs.Clear();

            var cfg = VRF_Mod.Settings.GetOrCreateHoverConfig(vDef.defName);
            var props = vDef.comps?.OfType<CompProperties_VehicleHover>().FirstOrDefault();

            if (props != null && VRF_Mod.Settings.GetHoverConfig(vDef.defName) == null)
            {
                cfg.maxTicks                 = props.maxTicks;
                cfg.maxTicksVertical         = props.maxTicksVertical;
                cfg.maxTicksPropeller        = props.maxTicksPropeller;
                cfg.hoverAltitude            = props.hoverAltitude;
                cfg.hoverShadowOffset        = props.hoverShadowOffset;
                cfg.hoverBobAmount           = props.hoverBobAmount;
                cfg.hoverBobSpeed            = props.hoverBobSpeed;
                cfg.hoverMoveSpeed           = props.hoverMoveSpeed;
            }
        }

        private static void DrawHoverVehicleDetail(Rect rect)
        {
            VehicleDef vDef = _selectedHoverVehicle;
            if (vDef == null) return;

            if (Widgets.ButtonText(new Rect(rect.x, rect.y, BtnW, 26f), "VRF_Settings_Back".Translate()))
            {
                _selectedHoverVehicle = null;
                return;
            }

            var hoverCfg = VRF_Mod.Settings.GetOrCreateHoverConfig(vDef.defName);
            if (!_hoverBufs.TryGetValue(vDef.defName, out var bufs))
            {
                bufs = new Dictionary<string, string>();
                _hoverBufs[vDef.defName] = bufs;
            }

            bool hasNativeHover = vDef.comps != null && vDef.comps.OfType<CompProperties_VehicleHover>().Any();

            if (Widgets.ButtonText(new Rect(rect.x + BtnW + Pad, rect.y, BtnW * 1.2f, 26f), "VRF_Settings_HoverReset".Translate()))
            {
                VRF_Mod.Settings.hoverConfigs.RemoveAll(h => h.vehicleDefName == vDef.defName);
                bufs.Clear();
                _hoverBufs.Remove(vDef.defName);
                VRF_Mod.Instance.WriteSettings();
                return;
            }

            if (Widgets.ButtonText(new Rect(rect.x + BtnW * 2.2f + Pad * 2f, rect.y, BtnW * 1.4f, 26f), "VRF_Hover_ExportPatch".Translate()))
            {
                var cfg = VRF_Mod.Settings.GetOrCreateHoverConfig(vDef.defName);
                VRF_PresetIO.ExportHoverAsPatch(cfg, vDef.label ?? vDef.defName);
            }

            bool isAirplane   = hoverCfg.flightType == "Airplane";
            bool isHelicopter = !isAirplane;

            float contentH = 30f + 130f + 26f + 30f + 26f + 10f
                + (isHelicopter ? 26f * 6f + 10f : 0f)
                + 26f * 5f + 30f;
            contentH = Mathf.Max(contentH, 360f);

            float detailTop = rect.y + 34f;
            Rect scrollA = new Rect(rect.x, detailTop, rect.width, rect.height - 34f);
            Rect viewR   = new Rect(0f, 0f, scrollA.width - 20f, contentH);
            Widgets.BeginScrollView(scrollA, ref _hoverDetailScrollPos, viewR);

            const float LabelW  = 200f;
            const float FieldW  = 120f;
            const float RowH    = 26f;
            const float SecGap  = 10f;
            float fullW = viewR.width;

            float previewSz = Mathf.Min(fullW * 0.28f, 120f);
            float rightX    = previewSz + Pad * 2f;
            float rightW    = fullW - rightX;

            Rect previewBox = new Rect(0f, 0f, previewSz, previewSz);
            Widgets.DrawBoxSolid(previewBox, new Color(0.08f, 0.08f, 0.08f, 0.9f));
            Widgets.DrawBox(previewBox, 1);
            if (Event.current.type == EventType.Repaint)
                VehicleGui.DrawVehicleDefOnGUI(previewBox.ContractedBy(4f), vDef, BuildPatternDataSafe(vDef), Rot8.East);

            float ry = 0f;
            Text.Font = GameFont.Medium;
            Widgets.Label(new Rect(rightX, ry, rightW, 30f), vDef.label ?? vDef.defName);
            ry += 32f;
            Text.Font = GameFont.Small;
            GUI.color = hasNativeHover ? new Color(0.55f, 1f, 0.55f) : new Color(0.8f, 0.8f, 0.4f);
            Widgets.Label(new Rect(rightX, ry, rightW, 20f),
                hasNativeHover ? "VRF_Settings_HoverNative".Translate() : "VRF_Settings_HoverOverride".Translate());
            GUI.color = Color.white;
            ry += 24f;

            bool wasEnabled = hoverCfg.enabled;
            Widgets.CheckboxLabeled(new Rect(rightX, ry, rightW, RowH), "VRF_Settings_HoverEnable".Translate(), ref hoverCfg.enabled);
            if (hoverCfg.enabled != wasEnabled) { ApplyHoverConfig(vDef, hoverCfg); VRF_Mod.Instance.WriteSettings(); }
            ry += RowH + 4f;

            float by = Mathf.Max(previewSz + Pad, ry);

            Widgets.DrawLineHorizontal(0f, by, fullW * 0.6f);
            by += SecGap;

            GUI.color = new Color(0.7f, 0.85f, 1f);
            Widgets.Label(new Rect(0f, by, fullW, 22f), "VRF_Hover_FlightType".Translate());
            GUI.color = Color.white;
            by += 24f;

            string flightLabel = isAirplane ? "VRF_Hover_Type_Airplane".Translate() : "VRF_Hover_Type_Helicopter".Translate();
            if (Widgets.ButtonText(new Rect(0f, by, 180f, RowH), flightLabel))
            {
                var opts = new List<FloatMenuOption>
                {
                    new FloatMenuOption("VRF_Hover_Type_Helicopter".Translate(), () =>
                    {
                        hoverCfg.flightType = "Hover";
                        bufs.Clear();
                        if (!bufs.ContainsKey("hoverBobAmount")) { bufs["hoverBobAmount"] = hoverCfg.hoverBobAmount.ToString("F2"); }
                        ApplyHoverConfig(vDef, hoverCfg);
                        VRF_Mod.Instance.WriteSettings();
                    }),
                    new FloatMenuOption("VRF_Hover_Type_Airplane".Translate(), () =>
                    {
                        hoverCfg.flightType        = "Airplane";
                        hoverCfg.hoverMoveSpeed    = 15f;
                        hoverCfg.hoverRotationSpeed = 60f;
                        hoverCfg.hoverAltitude     = 0.5f;
                        bufs.Clear();
                        ApplyHoverConfig(vDef, hoverCfg);
                        VRF_Mod.Instance.WriteSettings();
                    })
                };
                Find.WindowStack.Add(new FloatMenu(opts));
            }
            by += RowH + SecGap;

            Widgets.DrawLineHorizontal(0f, by, fullW * 0.6f);
            by += SecGap;

            GUI.color = new Color(0.9f, 0.85f, 0.6f);
            Widgets.Label(new Rect(0f, by, fullW, 22f), "VRF_Hover_SectionCommon".Translate());
            GUI.color = Color.white;
            by += 24f;

            DrawLabelField(ref by, fullW, LabelW, FieldW, vDef, hoverCfg, bufs, "maxTicks",         "VRF_Hover_MaxTicks",      ref hoverCfg.maxTicks,          1,    99999);
            DrawLabelField(ref by, fullW, LabelW, FieldW, vDef, hoverCfg, bufs, "baseMoveSpeed",    "VRF_Hover_BaseMoveSpeed", ref hoverCfg.baseMoveSpeed,     0f,   999f);
            DrawLabelField(ref by, fullW, LabelW, FieldW, vDef, hoverCfg, bufs, "hoverMoveSpeed",   "VRF_Hover_MoveSpeed",     ref hoverCfg.hoverMoveSpeed,    0.1f, 50f);
            DrawLabelField(ref by, fullW, LabelW, FieldW, vDef, hoverCfg, bufs, "hoverAltitude",    "VRF_Hover_Altitude",      ref hoverCfg.hoverAltitude,     0f,   20f);
            DrawLabelField(ref by, fullW, LabelW, FieldW, vDef, hoverCfg, bufs, "hoverShadowOffset","VRF_Hover_ShadowOffset",  ref hoverCfg.hoverShadowOffset, 0f,   10f);

            if (isHelicopter)
            {
                by += SecGap;
                GUI.color = new Color(0.7f, 0.9f, 0.7f);
                Widgets.Label(new Rect(0f, by, fullW, 22f), "VRF_Hover_SectionHelicopter".Translate());
                GUI.color = Color.white;
                by += 24f;
                DrawLabelField(ref by, fullW, LabelW, FieldW, vDef, hoverCfg, bufs, "maxTicksVertical",  "VRF_Hover_MaxTicksVertical",  ref hoverCfg.maxTicksVertical,         1,    99999);
                DrawLabelField(ref by, fullW, LabelW, FieldW, vDef, hoverCfg, bufs, "maxTicksPropeller", "VRF_Hover_MaxTicksPropeller",  ref hoverCfg.maxTicksPropeller,        1,    99999);
                DrawLabelField(ref by, fullW, LabelW, FieldW, vDef, hoverCfg, bufs, "hoverBobAmount",    "VRF_Hover_BobAmount",          ref hoverCfg.hoverBobAmount,           0f,   5f);
                DrawLabelField(ref by, fullW, LabelW, FieldW, vDef, hoverCfg, bufs, "hoverBobSpeed",     "VRF_Hover_BobSpeed",           ref hoverCfg.hoverBobSpeed,            0f,   10f);
                DrawLabelField(ref by, fullW, LabelW, FieldW, vDef, hoverCfg, bufs, "angVelPropeller",   "VRF_Hover_AngVelPropeller",    ref hoverCfg.angularVelocityPropeller, 0f,   360f);
            }
            else
            {
                by += SecGap;
                GUI.color = new Color(0.7f, 0.85f, 1f);
                Widgets.Label(new Rect(0f, by, fullW, 22f), "VRF_Hover_SectionAirplane".Translate());
                GUI.color = Color.white;
                by += 24f;
                DrawLabelField(ref by, fullW, LabelW, FieldW, vDef, hoverCfg, bufs, "hoverRotationSpeed","VRF_Hover_RotationSpeed", ref hoverCfg.hoverRotationSpeed, 1f, 720f);
            }

            Widgets.EndScrollView();
        }

        private static void DrawLabelField(ref float y, float rowW, float labelW, float fieldW,
            VehicleDef vDef, VRF_HoverConfig cfg, Dictionary<string, string> bufs,
            string key, string labelKey, ref int val, int min, int max)
        {
            if (!bufs.TryGetValue(key, out string buf) || buf == null) { buf = val.ToString(); bufs[key] = buf; }
            Text.Anchor = TextAnchor.MiddleLeft;
            Widgets.Label(new Rect(0f, y, labelW, 24f), labelKey.Translate());
            Text.Anchor = TextAnchor.UpperLeft;
            int prev = val;
            Widgets.TextFieldNumeric(new Rect(labelW + Pad, y, fieldW, 24f), ref val, ref buf, min, max);
            bufs[key] = buf;
            if (val != prev) { ApplyHoverConfig(vDef, cfg); VRF_Mod.Instance.WriteSettings(); }
            y += 26f;
        }

        private static void DrawLabelField(ref float y, float rowW, float labelW, float fieldW,
            VehicleDef vDef, VRF_HoverConfig cfg, Dictionary<string, string> bufs,
            string key, string labelKey, ref float val, float min, float max)
        {
            if (!bufs.TryGetValue(key, out string buf) || buf == null) { buf = val.ToString("F2"); bufs[key] = buf; }
            Text.Anchor = TextAnchor.MiddleLeft;
            Widgets.Label(new Rect(0f, y, labelW, 24f), labelKey.Translate());
            Text.Anchor = TextAnchor.UpperLeft;
            float prev = val;
            Widgets.TextFieldNumeric(new Rect(labelW + Pad, y, fieldW, 24f), ref val, ref buf, min, max);
            bufs[key] = buf;
            if (Mathf.Abs(val - prev) > 0.0001f) { ApplyHoverConfig(vDef, cfg); VRF_Mod.Instance.WriteSettings(); }
            y += 26f;
        }

        private static void ApplyHoverConfig(VehicleDef vDef, VRF_HoverConfig cfg)
        {
            var props = vDef.comps?.OfType<CompProperties_VehicleHover>().FirstOrDefault();
            if (props == null) return;
            if (System.Enum.TryParse<FlightType>(cfg.flightType, out var ft))
                props.flightType    = ft;
            props.maxTicks          = cfg.maxTicks;
            props.maxTicksVertical  = cfg.maxTicksVertical;
            props.maxTicksPropeller = cfg.maxTicksPropeller;
            props.hoverAltitude     = cfg.hoverAltitude;
            props.hoverShadowOffset = cfg.hoverShadowOffset;
            props.hoverBobAmount    = cfg.hoverBobAmount;
            props.hoverBobSpeed     = cfg.hoverBobSpeed;
            props.hoverMoveSpeed    = cfg.hoverMoveSpeed;
            props.hoverRotationSpeed = cfg.hoverRotationSpeed;

            if (cfg.baseMoveSpeed > 0f)
            {
                if (!VehicleMod.settings.vehicles.vehicleStats.ContainsKey(vDef.defName))
                    VehicleMod.settings.vehicles.vehicleStats[vDef.defName] = new System.Collections.Generic.Dictionary<string, float>();
                VehicleMod.settings.vehicles.vehicleStats[vDef.defName][VehicleStatDefOf.MoveSpeed.defName] = cfg.baseMoveSpeed;

                VehicleStatDefOf.MoveSpeed.Worker.ClearCachedBaseValues(vDef);
                vDef.RecacheMovementPermissions();
            }
        }

        private static void DoAutoFill(FactionDef fDef, VRF_NaturalRaidFactionConfig cfg, VRF_SpawnContext ctx)
        {
            var allKinds = VRF_VehicleKindCache.AllVehicleKinds;
            if (allKinds.NullOrEmpty()) return;
            string fn = fDef.label ?? fDef.defName;
            string fd = fDef.defName;
            Find.WindowStack.Add(Dialog_MessageBox.CreateConfirmation(
                "VRF_Settings_AutoFillConfirm".Translate(fn),
                () =>
                {
                    var c = VRF_Mod.Settings.GetOrCreateFactionConfig(fd);
                    foreach (var k in allKinds)
                    {
                        VehicleDef vd = k.race as VehicleDef;
                        if (IsGravship(vd, k)) continue;
                        if (vd != null && vd.type == VehicleType.Sea) continue;

                        if (vd != null && vd.type == VehicleType.Air
                            && VRF_AerialVehicleClassifier.IsSiegePod(vd)
                            && !VRF_Mod.Settings.AutoFillSiegeDropVehicles)
                            continue;

                        if (vd != null && !VRF_Mod.Settings.AutoFillTransportVehicles)
                        {
                            bool hasTurrets = vd.CompPropsVehicleTurrets != null && !vd.CompPropsVehicleTurrets.turrets.NullOrEmpty();
                            if (!hasTurrets) continue;
                        }

                        var e           = c.GetOrCreateForContext(k.defName, ctx);
                        e.enabled       = true;
                        float est       = VRF_CombatPowerEstimator.Estimate(k);
                        e.combatPowerOverride = est;
                        e.minRaidPoints = est;
                        e.maxRaidPoints = 0f;
                        e.fuelPercent   = 100f;

                        if (vd != null && vd.type == VehicleType.Air)
                        {
                            if (VRF_AerialVehicleClassifier.IsSiegePod(vd))
                            {
                                e.helicopterMode = false;
                                e.isSiegeDrop    = true;
                            }
                            else if (VRF_AerialVehicleClassifier.IsHelicopter(vd))
                            {
                                var speeds = VRF_HoverSpeedDeriver.ForHelicopter(vd);
                                e.helicopterMode      = true;
                                e.airVehicleType      = "Helicopter";
                                e.helicopterMoveSpeed = speeds.moveSpeed;

                                var hcfg = VRF_Mod.Settings.GetOrCreateHoverConfig(vd.defName);
                                hcfg.flightType              = "Hover";
                                hcfg.baseMoveSpeed           = speeds.moveSpeed;
                                hcfg.maxTicks                = 600;
                                hcfg.maxTicksVertical        = 400;
                                hcfg.maxTicksPropeller       = 800;
                                hcfg.hoverAltitude           = 4f;
                                hcfg.hoverShadowOffset       = 1.5f;
                                hcfg.hoverBobAmount          = 0.22f;
                                hcfg.hoverBobSpeed           = 2.0f;
                                hcfg.hoverMoveSpeed          = speeds.moveSpeed;
                                hcfg.hoverRotationSpeed      = speeds.rotationSpeed;
                                hcfg.angularVelocityPropeller = 59f;
                            }
                            else if (VRF_AerialVehicleClassifier.IsAirplane(vd))
                            {
                                var speeds = VRF_HoverSpeedDeriver.ForAirplane(vd);
                                e.helicopterMode      = true;
                                e.airVehicleType      = "Airplane";
                                e.helicopterMoveSpeed = speeds.moveSpeed;

                                var hcfg = VRF_Mod.Settings.GetOrCreateHoverConfig(vd.defName);
                                hcfg.flightType              = "Airplane";
                                hcfg.baseMoveSpeed           = speeds.moveSpeed;
                                hcfg.maxTicks                = 300;
                                hcfg.maxTicksVertical        = 300;
                                hcfg.maxTicksPropeller       = 300;
                                hcfg.hoverAltitude           = 0.5f;
                                hcfg.hoverShadowOffset       = 1.5f;
                                hcfg.hoverBobAmount          = 0f;
                                hcfg.hoverBobSpeed           = 0f;
                                hcfg.hoverMoveSpeed          = speeds.moveSpeed;
                                hcfg.hoverRotationSpeed      = speeds.rotationSpeed;
                                hcfg.angularVelocityPropeller = 0f;
                            }
                        }

                        string bk = fd + "_" + ctx.ToString() + "_" + k.defName;
                        _cpBuffers.Remove(bk); _mrpBuffers.Remove(bk); _mxpBuffers.Remove(bk);
                        if (vd?.CompPropsUpgradeTree != null)
                        {
                            e.upgradeLoadouts.Clear();
                            var lo = VRF_CombatPowerEstimator.BuildAutoLoadout(vd);
                            if (lo != null) 
                            {
                                e.upgradeLoadouts.Add(lo);
                                
                                var upgTurrets = GetUpgradeTurretsForLoadout(vd, lo);
                                var validUpgTurrets = upgTurrets.Where(t => t?.def?.ammunition?.AllowedThingDefs.FirstOrDefault() != null).ToList();
                                if (validUpgTurrets.Count > 0)
                                {
                                    var grouped = validUpgTurrets.GroupBy(t => t.def.ammunition.AllowedThingDefs.First()).ToList();
                                    float ammoFrac    = Mathf.Clamp01(VRF_Mod.Settings?.AutoFillAmmoFraction ?? 0.5f);
                                    float pctPerGroup = ammoFrac * 100f / grouped.Count;
                                    foreach (var group in grouped)
                                    {
                                        float pctPerTurret = pctPerGroup / group.Count();
                                        foreach (var t in group)
                                        {
                                            lo.GetOrCreateUpgradeAmmo(t.def.defName).ammoPercent = pctPerTurret;
                                        }
                                    }
                                }
                            }
                        }

                        var baseTurrets = GetBaseTurretsForMainSliders(vd, e);
                        var validBaseTurrets = baseTurrets.Where(t => t?.def?.ammunition?.AllowedThingDefs.FirstOrDefault() != null).ToList();
                        if (validBaseTurrets.Count > 0)
                        {
                            var grouped = validBaseTurrets.GroupBy(t => t.def.ammunition.AllowedThingDefs.First()).ToList();
                            float ammoFrac   = Mathf.Clamp01(VRF_Mod.Settings?.AutoFillAmmoFraction ?? 0.5f);
                            float pctPerGroup = ammoFrac * 100f / grouped.Count;
                            foreach (var group in grouped)
                            {
                                float pctPerTurret = pctPerGroup / group.Count();
                                foreach (var t in group)
                                {
                                    e.GetOrCreateTurretAmmo(t.def.defName).ammoPercent = pctPerTurret;
                                }
                            }
                        }
                    }
                    VRF_Mod.Instance.WriteSettings();
                }));
        }

        private static float CalcFlightModeHeight(VRF_NaturalRaidVehicleEntry entry)
        {
            float h = 30f + 28f; 
            if (entry.helicopterMode) h += 22f + 28f; 
            h += 8f + 26f;
            return h + Pad;
        }

        private static float CalcSliderAreaHeight(VehicleDef vDef, VRF_NaturalRaidVehicleEntry entry = null)
        {
            const float titleH = 24f; const float barH = 16f; const float rowH = 54f;
            float h = titleH + barH;
            var fuel = vDef.GetSortedCompProperties<CompProperties_FueledTravel>();
            if (fuel != null && !fuel.ElectricPowered) h += 24f;
            foreach (VehicleTurret t in GetBaseTurretsForMainSliders(vDef, entry))
            {
                if (t?.def?.ammunition == null) continue;
                if (t.def.ammunition.AllowedThingDefs.FirstOrDefault() == null) continue;
                h += rowH;
            }
            return h + Pad;
        }

        private static float CalcPaintSectionHeight(VehicleDef vDef, VRF_NaturalRaidVehicleEntry entry)
        {
            float h = 32f + 4f + 22f + 4f + 2f + 6f + 34f + 8f;
            if (entry.paintConfig != null && entry.paintConfig.mode == VRF_PaintMode.Fixed)
                h += 22f + 24f + (22f + 32f + 10f) + 28f;
            return h + Pad;
        }

        private static float CalcUpgradeSectionHeight(VehicleDef vDef, VRF_NaturalRaidVehicleEntry entry)
        {
            var nodes = GetAllNodesForVehicle(vDef);
            if (nodes.Count == 0) return 0f;
            float h = 26f + 10f + 2f + 10f + 22f + 32f;
            int totalNodeRows = Mathf.CeilToInt(nodes.Count / 2f);
            const float previewSz = 90f; const float ammoRowH = 44f;
            foreach (var loadout in entry.upgradeLoadouts)
            {
                var uta = GetUpgradeTurretsForLoadout(vDef, loadout)
                    .Where(t => t?.def?.ammunition != null && t.def.ammunition.AllowedThingDefs.FirstOrDefault() != null).ToList();
                float ammoH = uta.Count > 0 ? 22f + 6f + uta.Count * ammoRowH : 0f;
                h += 26f + totalNodeRows * 22f + 12f + ammoH + previewSz + 8f;
            }
            return h + Pad;
        }

        private static List<VehicleTurret> GetBaseTurretsForMainSliders(VehicleDef vDef, VRF_NaturalRaidVehicleEntry entry)
        {
            var result   = new List<VehicleTurret>(vDef.CompPropsVehicleTurrets?.turrets ?? new List<VehicleTurret>());
            if (entry?.upgradeLoadouts == null || entry.upgradeLoadouts.Count == 0) return result;
            var allNodes = GetAllNodesForVehicle(vDef);
            if (allNodes.Count == 0) return result;
            var allKeys = new HashSet<string>();
            foreach (var lo in entry.upgradeLoadouts) foreach (var k in lo.nodeKeys) allKeys.Add(k);
            var toRemove = new HashSet<string>();
            foreach (var node in allNodes)
            {
                if (!allKeys.Contains(node.key) || node.upgrades == null) continue;
                foreach (var upg in node.upgrades)
                {
                    if (!(upg is TurretUpgrade tu)) continue;
                    if (!tu.removeTurrets.NullOrEmpty()) foreach (string rk in tu.removeTurrets) toRemove.Add(rk);
                }
            }
            result.RemoveAll(t => toRemove.Contains(t.key));
            return result;
        }

        private static List<VehicleTurret> GetUpgradeTurretsForLoadout(VehicleDef vDef, VRF_UpgradeLoadout loadout)
        {
            var result   = new List<VehicleTurret>();
            var allNodes = GetAllNodesForVehicle(vDef);
            var removed  = new HashSet<string>();
            foreach (var node in allNodes)
            {
                if (!loadout.nodeKeys.Contains(node.key) || node.upgrades == null) continue;
                foreach (var upg in node.upgrades)
                {
                    if (!(upg is TurretUpgrade tu)) continue;
                    if (!tu.removeTurrets.NullOrEmpty()) foreach (string rk in tu.removeTurrets) removed.Add(rk);
                }
            }
            var baseTurretKeys = new HashSet<string>(
                vDef.CompPropsVehicleTurrets?.turrets?.Select(t => t.key).Where(k => k != null) ?? Enumerable.Empty<string>());
            var survivingBase  = new HashSet<string>(baseTurretKeys.Where(k => !removed.Contains(k)));
            foreach (var node in allNodes)
            {
                if (!loadout.nodeKeys.Contains(node.key) || node.upgrades == null) continue;
                foreach (var upg in node.upgrades)
                {
                    if (!(upg is TurretUpgrade tu) || tu.turrets.NullOrEmpty()) continue;
                    foreach (var t in tu.turrets) if (!survivingBase.Contains(t.key)) result.Add(t);
                }
            }
            result.RemoveAll(t => removed.Contains(t.key) && !removed.Contains(t.key));
            return result;
        }

        private static List<UpgradeNode> GetAllNodesForVehicle(VehicleDef vDef)
        {
            var comp = vDef.comps?.OfType<CompProperties_UpgradeTree>().FirstOrDefault();
            if (comp?.def?.nodes == null) return new List<UpgradeNode>();
            return comp.def.nodes;
        }

        private static float GetUpgradedCargoCapacity(VehicleDef vDef, VRF_UpgradeLoadout loadout, List<UpgradeNode> allNodes)
        {
            float cargo = vDef.GetStatValueAbstract(VehicleStatDefOf.CargoCapacity);
            if (loadout == null || allNodes == null) return cargo;

            foreach (var node in allNodes)
            {
                if (!loadout.nodeKeys.Contains(node.key) || node.upgrades == null) continue;
                foreach (var upg in node.upgrades)
                {
                    if (upg is Vehicles.StatUpgrade su && su.vehicleStats != null)
                    {
                        foreach (var mod in su.vehicleStats)
                        {
                            if (mod.def == VehicleStatDefOf.CargoCapacity)
                            {
                                if (mod.type == Vehicles.UpgradeType.Add) cargo += mod.value;
                                else if (mod.type == Vehicles.UpgradeType.Set) cargo = mod.value;
                            }
                        }
                    }
                }
            }
            return Mathf.Max(0f, cargo);
        }

        private static void DrawResourceSliders(Rect rect, VehicleDef vDef, VRF_NaturalRaidVehicleEntry entry)
        {
            float cargoCapacity = vDef.GetStatValueAbstract(VehicleStatDefOf.CargoCapacity);
            var fuelProps       = vDef.GetSortedCompProperties<CompProperties_FueledTravel>();
            var baseTurrets     = GetBaseTurretsForMainSliders(vDef, entry);
            float maxFuelKg     = 0f;
            bool hasFuel        = fuelProps != null && !fuelProps.ElectricPowered;
            if (hasFuel) { float m = fuelProps.fuelType != null ? fuelProps.fuelType.GetStatValueAbstract(StatDefOf.Mass) : 1f; maxFuelKg = fuelProps.fuelCapacity * m; }

            var ammoDefs = new List<(VehicleTurret turret, ThingDef ammoDef, float ammoMass)>();
            foreach (VehicleTurret t in baseTurrets)
            {
                if (t?.def?.ammunition == null) continue;
                ThingDef ad = t.def.ammunition.AllowedThingDefs.FirstOrDefault();
                if (ad == null) continue;
                float m = ad.GetStatValueAbstract(StatDefOf.Mass); if (m <= 0f) m = 0.1f;
                ammoDefs.Add((t, ad, m));
            }

            float currentFuelKg = hasFuel ? maxFuelKg : 0f;
            float currentAmmoKg = ammoDefs.Sum(a => (cargoCapacity > 0f ? cargoCapacity : 999f) * (entry.GetOrCreateTurretAmmo(a.turret.def.defName).ammoPercent / 100f));
            float totalUsedKg   = currentFuelKg + currentAmmoKg;
            float y = rect.y;

            GUI.color   = new Color(0.9f, 0.85f, 0.6f);
            Text.Font   = GameFont.Small;
            Widgets.Label(new Rect(rect.x, y, rect.width, 24f), "VRF_Settings_CargoSection".Translate(cargoCapacity.ToString("F0")));
            GUI.color = Color.white; y += 24f;

            if (cargoCapacity > 0f)
            {
                float fillPct = Mathf.Clamp01(totalUsedKg / cargoCapacity);
                Color barCol  = fillPct > 0.76f ? new Color(0.9f, 0.2f, 0.2f) : new Color(0.3f, 0.7f, 0.3f);
                Widgets.DrawBoxSolid(new Rect(rect.x, y, rect.width, 10f), new Color(0.2f, 0.2f, 0.2f));
                Widgets.DrawBoxSolid(new Rect(rect.x, y, rect.width * fillPct, 10f), barCol);
                Widgets.DrawBox(new Rect(rect.x, y, rect.width, 10f), 1);
                Text.Anchor = TextAnchor.MiddleRight;
                Widgets.Label(new Rect(rect.x, y, rect.width, 10f), $"{totalUsedKg:F1} / {cargoCapacity:F0} kg");
                Text.Anchor = TextAnchor.UpperLeft;
                y += 16f;
            }

            if (hasFuel)
            {
                string fn      = fuelProps.fuelType?.label ?? "fuel";
                Widgets.Label(new Rect(rect.x, y, rect.width, 20f), "VRF_Settings_Fuel".Translate(fn, "100", maxFuelKg.ToString("F1"), maxFuelKg.ToString("F1"))); 
                y += 24f;
            }

            foreach (var (turret, ammoDef, ammoMass) in ammoDefs)
            {
                var tEntry       = entry.GetOrCreateTurretAmmo(turret.def.defName);
                float maxAmmoKg  = cargoCapacity > 0f ? cargoCapacity : 999f;
                float otherAmmoKg = currentAmmoKg - maxAmmoKg * (tEntry.ammoPercent / 100f);
                float maxAllPct   = cargoCapacity > 0f ? Mathf.Clamp01((cargoCapacity * 0.75f - otherAmmoKg) / maxAmmoKg) * 100f : 100f;
                float clamped     = Mathf.Min(tEntry.ammoPercent, maxAllPct);
                float ammoKg      = maxAmmoKg * (clamped / 100f);
                int   count       = Mathf.FloorToInt(ammoKg / ammoMass);
                string tLabel     = turret.def.label ?? turret.def.defName;
                Widgets.Label(new Rect(rect.x, y, rect.width, 20f), "VRF_Settings_Ammo".Translate(tLabel, ammoDef.label ?? ammoDef.defName, clamped.ToString("F0"), count.ToString(), ammoKg.ToString("F1"))); y += 22f;
                float newPct      = Widgets.HorizontalSlider(new Rect(rect.x, y, rect.width, 22f), clamped, 0f, 100f, roundTo: 1f);
                newPct = Mathf.Min(newPct, maxAllPct);
                if (Mathf.Abs(newPct - tEntry.ammoPercent) > 0.01f) { tEntry.ammoPercent = newPct; VRF_Mod.Instance.WriteSettings(); }
                y += 32f;
            }
        }

        private static void DrawFlightModeSection(Rect rect, VehicleDef vDef, VRF_NaturalRaidVehicleEntry entry, string bufKey)
        {
            float y = rect.y;
            GUI.color = new Color(0.7f, 0.85f, 1f);
            Widgets.Label(new Rect(rect.x, y, rect.width, 22f), "VRF_Settings_FlightMode".Translate()); GUI.color = Color.white; y += 26f;

            string currentTypeLabel = entry.helicopterMode
                ? (entry.airVehicleType == "Airplane"
                    ? "VRF_Settings_AirplaneType".Translate().ToString()
                    : (entry.airVehicleType == "Gravship"
                        ? "VRF_Settings_GravshipType".Translate().ToString()
                        : "VRF_Settings_HelicopterType".Translate().ToString()))
                : "VRF_Settings_FlightModeDisabled".Translate().ToString();

            if (Widgets.ButtonText(new Rect(rect.x, y, Mathf.Min(rect.width, 220f), 24f), currentTypeLabel))
            {
                bool isGravVehicle = IsGravship(vDef);
                List<FloatMenuOption> flightOptions = new List<FloatMenuOption>();
                flightOptions.Add(new FloatMenuOption("VRF_Settings_FlightModeDisabled".Translate(), () =>
                {
                    entry.helicopterMode = false;
                    VRF_Mod.Instance.WriteSettings();
                }));
                if (!isGravVehicle)
                {
                    flightOptions.Add(new FloatMenuOption("VRF_Settings_HelicopterType".Translate(), () =>
                    {
                        entry.helicopterMode  = true;
                        entry.airVehicleType  = "Helicopter";
                        entry.helicopterMoveSpeed = 4.5f;
                        _hmsBuffers.Remove(bufKey);
                        VRF_Mod.Instance.WriteSettings();
                    }));
                    flightOptions.Add(new FloatMenuOption("VRF_Settings_AirplaneType".Translate(), () =>
                    {
                        entry.helicopterMode  = true;
                        entry.airVehicleType  = "Airplane";
                        entry.helicopterMoveSpeed = 15f;
                        _hmsBuffers.Remove(bufKey);
                        VRF_Mod.Instance.WriteSettings();
                    }));
                }
                flightOptions.Add(new FloatMenuOption("VRF_Settings_GravshipType".Translate(), () =>
                {
                    entry.helicopterMode  = true;
                    entry.airVehicleType  = "Gravship";
                    _hmsBuffers.Remove(bufKey);
                    VRF_Mod.Instance.WriteSettings();
                }));
                Find.WindowStack.Add(new FloatMenu(flightOptions));
            }
            y += 28f;

            if (entry.helicopterMode)
            {
                if (entry.airVehicleType == "Gravship")
                {
                    Rect btnRect = new Rect(rect.x, y, Mathf.Min(rect.width, 260f), 26f);
                    if (Widgets.ButtonText(btnRect, "VRF_ThrusterConfig_OpenButton".Translate()))
                    {
                        Find.WindowStack.Add(new VehicleMapFramework.Dialog_VRF_ThrusterConfig());
                    }
                    y += 30f;

                    if (!string.IsNullOrEmpty(entry.gravshipPresetName))
                    {
                        float calcSpeed = VehicleMapFramework.VRF_GravshipSpeedUtility.CalculatePresetSpeed(entry.gravshipPresetName);
                        Widgets.Label(new Rect(rect.x, y, rect.width, 22f), $"Velocidad estimada: {calcSpeed:F2} c/s");
                        y += 24f;
                    }
                }
                else
                {
                    string speedLabel = entry.airVehicleType == "Airplane"
                        ? "VRF_Settings_AirplaneMoveSpeed".Translate().ToString()
                        : "VRF_Settings_HoverMoveSpeed".Translate().ToString();
                    Widgets.Label(new Rect(rect.x, y, rect.width, 22f), speedLabel); y += 22f;
                    if (!_hmsBuffers.TryGetValue(bufKey, out string hmsBuf) || hmsBuf == null) { hmsBuf = entry.helicopterMoveSpeed.ToString("F1"); _hmsBuffers[bufKey] = hmsBuf; }
                    float prev = entry.helicopterMoveSpeed;
                    Widgets.TextFieldNumeric(new Rect(rect.x, y, Mathf.Min(rect.width, 120f), 24f), ref entry.helicopterMoveSpeed, ref hmsBuf, 0.1f, 99f);
                    _hmsBuffers[bufKey] = hmsBuf;
                    if (Mathf.Abs(entry.helicopterMoveSpeed - prev) > 0.001f) VRF_Mod.Instance.WriteSettings();
                }
            }

            y += 8f;
            bool wasSiegeDrop = entry.isSiegeDrop;
            Rect siegeDropRect = new Rect(rect.x, y, rect.width, 26f);
            Widgets.CheckboxLabeled(siegeDropRect, "VRF_Settings_IsSiegeDrop".Translate(), ref entry.isSiegeDrop);
            if (entry.isSiegeDrop != wasSiegeDrop) VRF_Mod.Instance.WriteSettings();
            if (Mouse.IsOver(siegeDropRect))
                TooltipHandler.TipRegion(siegeDropRect, "VRF_Settings_IsSiegeDropDesc".Translate());
        }

        private static void DrawPaintSection(Rect rect, VehicleDef vDef, VRF_NaturalRaidVehicleEntry entry)
        {
            float y = rect.y;
            GUI.color = new Color(1f, 0.92f, 0.6f); Text.Font = GameFont.Medium;
            Widgets.Label(new Rect(rect.x, y, rect.width, 28f), "VRF_Settings_Optional".Translate()); GUI.color = Color.white; Text.Font = GameFont.Small; y += 32f;
            GUI.color = new Color(0.75f, 0.88f, 1f);
            Widgets.Label(new Rect(rect.x, y, rect.width, 22f), "VRF_Settings_PaintTitle".Translate()); GUI.color = Color.white; y += 26f;
            Widgets.DrawLineHorizontal(rect.x, y, rect.width * 0.5f); y += 10f;

            bool hasPaint = entry.paintConfig != null;
            string modeLabel = hasPaint ? (entry.paintConfig.mode == VRF_PaintMode.Fixed ? "VRF_Settings_Paint_Fixed".Translate().ToString() : "VRF_Settings_Paint_Faction".Translate().ToString()) : "VRF_Settings_Paint_Default".Translate().ToString();
            Widgets.Label(new Rect(rect.x, y + 4f, 60f, 22f), "VRF_Settings_Paint_Mode".Translate());
            if (Widgets.ButtonText(new Rect(rect.x + 64f, y, BtnW * 1.4f, 26f), modeLabel))
            {
                var opts = new List<FloatMenuOption>
                {
                    new FloatMenuOption("VRF_Settings_Paint_Default".Translate(), () => { entry.paintConfig = null; VRF_Mod.Instance.WriteSettings(); }),
                    new FloatMenuOption("VRF_Settings_Paint_Faction".Translate(), () => { entry.paintConfig = new VRF_PaintConfig { mode = VRF_PaintMode.Faction }; VRF_Mod.Instance.WriteSettings(); }),
                    new FloatMenuOption("VRF_Settings_Paint_Fixed".Translate(),   () =>
                    {
                        if (entry.paintConfig?.mode != VRF_PaintMode.Fixed)
                        {
                            var rgb2 = vDef.graphicData as GraphicDataRGB;
                            entry.paintConfig = new VRF_PaintConfig { mode = VRF_PaintMode.Fixed, colorOne = rgb2?.color ?? Color.white, colorTwo = rgb2?.colorTwo ?? Color.white, colorThree = rgb2?.colorThree ?? Color.white, patternDefName = rgb2?.pattern?.defName };
                        }
                        VRF_Mod.Instance.WriteSettings();
                    })
                };
                Find.WindowStack.Add(new FloatMenu(opts));
            }
            y += 34f;

            if (!hasPaint || entry.paintConfig.mode != VRF_PaintMode.Fixed) return;
            var cfg = entry.paintConfig;
            Widgets.Label(new Rect(rect.x, y, rect.width, 22f), "VRF_Settings_Paint_Colors".Translate()); y += 24f;
            const float swatchSize = 32f; const float colSlot = swatchSize + 60f;
            float cx = rect.x;
            DrawColorButton(cx, y, swatchSize, "VRF_Settings_Paint_Color1".Translate(), cfg.colorOne,   c => { cfg.colorOne   = c; VRF_Mod.Instance.WriteSettings(); }); cx += colSlot;
            DrawColorButton(cx, y, swatchSize, "VRF_Settings_Paint_Color2".Translate(), cfg.colorTwo,   c => { cfg.colorTwo   = c; VRF_Mod.Instance.WriteSettings(); }); cx += colSlot;
            DrawColorButton(cx, y, swatchSize, "VRF_Settings_Paint_Color3".Translate(), cfg.colorThree, c => { cfg.colorThree = c; VRF_Mod.Instance.WriteSettings(); });
            y += 22f + swatchSize + 10f;
            Widgets.Label(new Rect(rect.x, y, 120f, 22f), "VRF_Settings_Paint_Pattern".Translate());
            PatternDef currentPat = cfg.ResolvedPattern ?? PatternDefOf.Default;
            string skinTag = currentPat is SkinDef ? " [Skin]" : (currentPat == PatternDefOf.Default ? " [Default]" : "");
            if (Widgets.ButtonText(new Rect(rect.x + 128f, y, BtnW * 1.6f, 26f), currentPat.LabelCap + skinTag))
            {
                var patOpts = new List<FloatMenuOption>();
                var all     = new List<PatternDef> { PatternDefOf.Default };
                all.AddRange(DefDatabase<PatternDef>.AllDefsListForReading.Where(p => !(p is SkinDef) && p.ValidFor(vDef)));
                all.AddRange(DefDatabase<SkinDef>.AllDefsListForReading.Where(p => p.ValidFor(vDef)).Cast<PatternDef>());
                foreach (PatternDef pat in all)
                {
                    PatternDef captured = pat;
                    string tag = captured is SkinDef ? " [Skin]" : (captured == PatternDefOf.Default ? " [Default]" : "");
                    patOpts.Add(new FloatMenuOption(captured.LabelCap + tag, () => { cfg.patternDefName = captured == PatternDefOf.Default ? null : captured.defName; VRF_Mod.Instance.WriteSettings(); }));
                }
                Find.WindowStack.Add(new FloatMenu(patOpts));
            }
        }

        private static void DrawUpgradeSection(Rect rect, VehicleDef vDef, VRF_NaturalRaidVehicleEntry entry)
        {
            var allNodes = GetAllNodesForVehicle(vDef);
            if (allNodes.Count == 0) return;
            float y = rect.y;
            GUI.color = new Color(0.6f, 1f, 0.7f);
            Widgets.Label(new Rect(rect.x, y, rect.width, 22f), "VRF_Settings_Upgrades".Translate()); GUI.color = Color.white; y += 26f;
            Widgets.DrawLineHorizontal(rect.x, y, rect.width * 0.5f); y += 10f;
            GUI.color = new Color(0.7f, 0.7f, 0.7f); Text.Font = GameFont.Tiny;
            Widgets.Label(new Rect(rect.x, y, rect.width, 20f), "VRF_Settings_UpgradesDesc".Translate()); GUI.color = Color.white; Text.Font = GameFont.Small; y += 22f;
            if (Widgets.ButtonText(new Rect(rect.x, y, BtnW * 1.3f, 26f), "VRF_Settings_Upgrades_AddOption".Translate()))
            { entry.upgradeLoadouts.Add(new VRF_UpgradeLoadout()); VRF_Mod.Instance.WriteSettings(); }
            y += 32f;

            for (int i = 0; i < entry.upgradeLoadouts.Count; i++)
            {
                var loadout          = entry.upgradeLoadouts[i];
                var upgTurretAmmo    = GetUpgradeTurretsForLoadout(vDef, loadout)
                    .Where(t => t?.def?.ammunition != null && t.def.ammunition.AllowedThingDefs.FirstOrDefault() != null).ToList();
                const float previewSz = 90f; const float ammoRowH = 44f;
                float boxH = 26f + Mathf.CeilToInt(allNodes.Count / 2f) * 22f + 12f
                    + (upgTurretAmmo.Count > 0 ? upgTurretAmmo.Count * ammoRowH + 60f + 6f : 0f) + previewSz + 8f;
                Widgets.DrawBoxSolid(new Rect(rect.x, y, rect.width, boxH), new Color(0.1f, 0.1f, 0.1f, 0.5f));

                GUI.color = new Color(0.9f, 0.9f, 0.5f); Text.Anchor = TextAnchor.MiddleLeft;
                Widgets.Label(new Rect(rect.x + Pad, y, rect.width * 0.6f, 22f), "VRF_Settings_Upgrades_Option".Translate(i + 1));
                Text.Anchor = TextAnchor.UpperLeft; GUI.color = Color.white;
                if (Widgets.ButtonText(new Rect(rect.xMax - 70f, y + 2f, 66f, 18f), "VRF_Settings_Upgrades_Remove".Translate()))
                { entry.upgradeLoadouts.RemoveAt(i); VRF_Mod.Instance.WriteSettings(); break; }
                y += 26f;

                float colW = (rect.width - Pad * 2f) * 0.5f; float rowY = y; int col = 0;
                foreach (UpgradeNode node in allNodes)
                {
                    bool selected  = loadout.nodeKeys.Contains(node.key);
                    bool prereqMet = node.prerequisiteNodes.NullOrEmpty() || node.prerequisiteNodes.All(k => loadout.nodeKeys.Contains(k));
                    bool disabled  = (!node.disableIfUpgradeNodeEnabled.NullOrEmpty() && loadout.nodeKeys.Contains(node.disableIfUpgradeNodeEnabled))
                                   || (!node.disableIfUpgradeNodesEnabled.NullOrEmpty() && node.disableIfUpgradeNodesEnabled.Any(k => loadout.nodeKeys.Contains(k)));
                    bool canSelect = prereqMet && !disabled;
                    string lbl     = node.label ?? node.key; if (lbl.Length > 22) lbl = lbl.Substring(0, 22) + "…";
                    Rect cellR     = new Rect(rect.x + Pad + col * colW, rowY, colW - 4f, 20f);
                    GUI.color = !canSelect && !selected ? new Color(0.45f, 0.45f, 0.45f) : selected ? new Color(0.5f, 1f, 0.5f) : Color.white;
                    bool ns = selected;
                    Widgets.CheckboxLabeled(cellR, lbl, ref ns, !canSelect && !selected);
                    GUI.color = Color.white;
                    if (ns != selected)
                    {
                        if (ns && canSelect) loadout.nodeKeys.Add(node.key);
                        else if (!ns) { loadout.nodeKeys.Remove(node.key); loadout.nodeKeys.RemoveAll(k => { UpgradeNode d = allNodes.Find(n => n.key == k); return d != null && !d.prerequisiteNodes.NullOrEmpty() && d.prerequisiteNodes.Contains(node.key); }); }
                        VRF_Mod.Instance.WriteSettings();
                    }
                    col++;
                    if (col >= 2) { col = 0; rowY += 22f; }
                }
                if (col > 0) rowY += 22f;
                y = rowY + 12f;

                if (upgTurretAmmo.Count > 0)
                {
                    float cargo = GetUpgradedCargoCapacity(vDef, loadout, allNodes);

                    var fuelProps = vDef.GetSortedCompProperties<CompProperties_FueledTravel>();
                    float maxFuelKg = 0f;
                    if (fuelProps != null && !fuelProps.ElectricPowered)
                    {
                        float m = fuelProps.fuelType != null ? fuelProps.fuelType.GetStatValueAbstract(StatDefOf.Mass) : 1f;
                        maxFuelKg = fuelProps.fuelCapacity * m;
                    }

                    var survivingBase = new HashSet<string>(vDef.CompPropsVehicleTurrets?.turrets?.Select(t => t.key).Where(k => k != null) ?? Enumerable.Empty<string>());
                    foreach (var node in allNodes)
                    {
                        if (!loadout.nodeKeys.Contains(node.key) || node.upgrades == null) continue;
                        foreach (var upg in node.upgrades)
                            if (upg is TurretUpgrade tu && !tu.removeTurrets.NullOrEmpty())
                                foreach (var rt in tu.removeTurrets) survivingBase.Remove(rt);
                    }

                    var allBaseTurrets = GetBaseTurretsForMainSliders(vDef, entry);
                    float baseAmmoKg = 0f;
                    foreach (var t in allBaseTurrets)
                    {
                        if (t?.def?.ammunition == null || !survivingBase.Contains(t.key)) continue;
                        baseAmmoKg += cargo * (entry.GetOrCreateTurretAmmo(t.def.defName).ammoPercent / 100f);
                    }

                    float upgAmmoKg = 0f;
                    foreach (var t in upgTurretAmmo)
                    {
                        upgAmmoKg += cargo * (loadout.GetOrCreateUpgradeAmmo(t.def.defName).ammoPercent / 100f);
                    }
                    
                    float totalAmmoKg = baseAmmoKg + upgAmmoKg;
                    float totalUsedKg = maxFuelKg + totalAmmoKg;

                    GUI.color = new Color(0.9f, 0.85f, 0.6f);
                    Widgets.Label(new Rect(rect.x + Pad, y, rect.width - Pad * 2f, 20f), "VRF_Settings_CargoSection".Translate(cargo.ToString("F0"))); GUI.color = Color.white; y += 22f;

                    if (cargo > 0f)
                    {
                        float fillPct = Mathf.Clamp01(totalUsedKg / cargo);
                        Color barCol  = fillPct > 0.76f ? new Color(0.9f, 0.2f, 0.2f) : new Color(0.3f, 0.7f, 0.3f);
                        Widgets.DrawBoxSolid(new Rect(rect.x + Pad, y, rect.width - Pad * 2f, 10f), new Color(0.2f, 0.2f, 0.2f));
                        Widgets.DrawBoxSolid(new Rect(rect.x + Pad, y, (rect.width - Pad * 2f) * fillPct, 10f), barCol);
                        Widgets.DrawBox(new Rect(rect.x + Pad, y, rect.width - Pad * 2f, 10f), 1);
                        Text.Anchor = TextAnchor.MiddleRight;
                        Widgets.Label(new Rect(rect.x + Pad, y, rect.width - Pad * 2f, 10f), $"{totalUsedKg:F1} / {cargo:F0} kg");
                        Text.Anchor = TextAnchor.UpperLeft;
                        y += 16f;
                    }

                    foreach (VehicleTurret turret in upgTurretAmmo)
                    {
                        ThingDef ad   = turret.def.ammunition.AllowedThingDefs.First();
                        var tAmmo     = loadout.GetOrCreateUpgradeAmmo(turret.def.defName);
                        float mass    = ad.GetStatValueAbstract(StatDefOf.Mass); if (mass <= 0f) mass = 0.1f;
                        float maxKg   = cargo > 0f ? cargo : 999f;
                        
                        float otherAmmoKg = totalAmmoKg - maxKg * (tAmmo.ammoPercent / 100f);
                        float maxAllPct   = cargo > 0f ? Mathf.Clamp01((cargo * 0.75f - otherAmmoKg) / maxKg) * 100f : 100f;
                        float clamped     = Mathf.Min(tAmmo.ammoPercent, maxAllPct);

                        float ammoKg  = maxKg * (clamped / 100f);
                        int   count   = Mathf.FloorToInt(ammoKg / mass);
                        Widgets.Label(new Rect(rect.x + Pad, y, rect.width - Pad * 2f, 20f), "VRF_Settings_Ammo".Translate(turret.def.label ?? turret.def.defName, ad.label ?? ad.defName, clamped.ToString("F0"), count.ToString(), ammoKg.ToString("F1"))); y += 22f;
                        
                        float newP = Widgets.HorizontalSlider(new Rect(rect.x + Pad, y, rect.width - Pad * 2f, 22f), clamped, 0f, 100f, roundTo: 1f);
                        newP = Mathf.Min(newP, maxAllPct);
                        if (Mathf.Abs(newP - tAmmo.ammoPercent) > 0.01f) { tAmmo.ammoPercent = newP; VRF_Mod.Instance.WriteSettings(); }
                        y += 26f;
                    }
                    y += 6f;
                }

                float prevW = Mathf.Min(rect.width - Pad * 2f, previewSz * 2.5f);
                Rect prevBox = new Rect(rect.x + Pad, y, prevW, previewSz);
                Widgets.DrawBoxSolid(prevBox, new Color(0.08f, 0.08f, 0.08f, 0.9f)); Widgets.DrawBox(prevBox, 1);
                if (Event.current.type == EventType.Repaint)
                {
                    PatternData pd = BuildPatternData(vDef, entry);
                    BlitRequest req = BlitRequest.For(vDef); req.patternData = pd; req.rot = Rot8.East;
                    foreach (UpgradeNode node in allNodes)
                    {
                        if (!loadout.nodeKeys.Contains(node.key)) continue;
                        if (!node.graphicOverlays.NullOrEmpty()) foreach (var gdo in node.graphicOverlays) { var ov = GraphicOverlay.Create(gdo, vDef); if (ov != null) req.blitTargets.Add(ov); }
                        if (node.upgrades == null) continue;
                        foreach (Upgrade upg in node.upgrades) { if (!(upg is TurretUpgrade tu) || tu.turrets.NullOrEmpty()) continue; foreach (VehicleTurret t in tu.turrets) if (!t.NoGraphic) req.blitTargets.Add(t); }
                    }
                    VehicleGui.DrawVehicleOnGUI(prevBox.ContractedBy(4f), in req);
                }
                y += previewSz + 8f;
            }
        }

        private static void DrawVehicleWithTurrets(Rect rect, VehicleDef vDef, VRF_NaturalRaidVehicleEntry entry = null)
        {
            if (Event.current.type != EventType.Repaint) return;
            PatternData pd = BuildPatternData(vDef, entry);
            VehicleGui.DrawVehicleDefOnGUI(rect, vDef, pd, Rot8.East);
        }

        private static PatternData BuildPatternData(VehicleDef vDef, VRF_NaturalRaidVehicleEntry entry)
        {
            if (entry?.paintConfig != null && entry.paintConfig.mode == VRF_PaintMode.Fixed)
            {
                PatternDef pat = entry.paintConfig.ResolvedPattern ?? (vDef.graphicData is GraphicDataRGB r ? r.pattern : null) ?? PatternDefOf.Default;
                return new PatternData(entry.paintConfig.colorOne, entry.paintConfig.colorTwo, entry.paintConfig.colorThree, pat, Vector2.zero, 1f);
            }
            if (vDef.graphicData is GraphicDataRGB rgb) return new PatternData(rgb);
            return new PatternData();
        }

        private static PatternData BuildPatternDataSafe(VehicleDef vDef)
        {
            if (vDef.graphicData is GraphicDataRGB rgb) return new PatternData(rgb);
            return new PatternData(Color.white, Color.white, Color.white, PatternDefOf.Default, Vector2.zero, 1f);
        }

        private static void DrawColorButton(float x, float y, float size, string label, Color current, Action<Color> onSet)
        {
            Text.Anchor = TextAnchor.MiddleLeft;
            Widgets.Label(new Rect(x, y, size + 56f, 20f), label);
            Text.Anchor = TextAnchor.UpperLeft;
            Rect swatch = new Rect(x, y + 22f, size, size);
            Widgets.DrawBoxSolid(swatch, current); Widgets.DrawBox(swatch, 1);
            Widgets.DrawHighlightIfMouseover(swatch);
            if (Widgets.ButtonInvisible(swatch)) Find.WindowStack.Add(new Dialog_VRF_ColorPicker(current, onSet));
        }

        private static void DrawVehicleThumb(Rect rect, PawnKindDef kind, VRF_NaturalRaidVehicleEntry entry = null)
        {
            VehicleDef vDef = kind.race as VehicleDef;
            if (vDef != null)
            {
                if (Event.current.type != EventType.Repaint) return;
                VehicleGui.DrawVehicleDefOnGUI(rect, vDef, BuildPatternData(vDef, entry), Rot8.East);
                return;
            }
            if (kind?.lifeStages == null || kind.lifeStages.Count == 0) return;
            var stage = kind.lifeStages[kind.lifeStages.Count - 1];
            if (stage?.bodyGraphicData?.Graphic == null) return;
            Texture2D tex = stage.bodyGraphicData.Graphic.MatEast?.mainTexture as Texture2D;
            if (tex != null) { GUI.color = stage.bodyGraphicData.color != default ? stage.bodyGraphicData.color : Color.white; GUI.DrawTexture(rect, tex, ScaleMode.ScaleToFit); GUI.color = Color.white; }
        }

        public static void ResetBuffers()
        {
            _cpBuffers.Clear(); _mrpBuffers.Clear(); _mxpBuffers.Clear();
            _hmsBuffers.Clear(); _budgetBufs.Clear(); _hoverBufs.Clear(); _configBufs.Clear();
            _vehicleSearchFilter = ""; _hoverVehicleSearchFilter = ""; _configScrollPos = Vector2.zero;
            _page = VRF_SettingsPage.FactionList;
            _selectedFaction = null; _selectedVehicleKind = null; _selectedHoverVehicle = null;
        }

        private static void DrawConfigTab(Rect rect)
        {
            // This tab has more controls than the settings window can display.  Keep all
            // controls, including Save/Reset, inside one scrollable content area.
            Rect scrollArea = rect;
            Rect viewRect = new Rect(0f, 0f, rect.width - 16f, Mathf.Max(rect.height, 1000f));
            Widgets.BeginScrollView(scrollArea, ref _configScrollPos, viewRect);
            rect = viewRect;
            float y = rect.y;

            Text.Font = GameFont.Medium;
            {
                string title  = "VRF_Settings_GlobalStrategyTitle".Translate();
                float  titleW = Text.CalcSize(title).x + 4f;
                Widgets.Label(new Rect(rect.x, y, titleW, 32f), title);
                Rect titleTip = new Rect(rect.x + titleW + 2f, y + 7f, 22f, 22f);
                GUI.color = Color.yellow;
                Text.Font = GameFont.Small;
                Widgets.Label(titleTip, "?");
                Text.Font = GameFont.Medium;
                GUI.color = Color.white;
                if (Mouse.IsOver(titleTip))
                    TooltipHandler.TipRegion(titleTip, "VRF_Settings_Tip_GlobalStrategy".Translate());
            }
            y += 36f;
            Text.Font = GameFont.Small;

            int totalStrats   = DefDatabase<RaidStrategyDef>.DefCount;
            int excludedCount = VRF_Mod.Settings.globalExcludedRaidStrategies.Count;
            int allowedCount  = totalStrats - excludedCount;
            string btnLabel   = "VRF_Settings_GlobalStrategyBtn".Translate() +
                                $" ({allowedCount}/{totalStrats})";
            if (Widgets.ButtonText(new Rect(rect.x, y, 260f, 28f), btnLabel))
                Find.WindowStack.Add(new Dialog_VRF_GlobalRaidStrategies());
            y += 36f;

            // Configuración de Propulsores de Gravship
            y += 8f;
            GUI.color = new Color(0.7f, 0.9f, 1f);
            Widgets.Label(new Rect(rect.x, y, rect.width, 22f), "VRF_ThrusterConfig_Title".Translate());
            GUI.color = Color.white;
            y += 24f;
            if (Widgets.ButtonText(new Rect(rect.x, y, 280f, 28f), "VRF_ThrusterConfig_OpenButton".Translate()))
            {
                Find.WindowStack.Add(new VehicleMapFramework.Dialog_VRF_ThrusterConfig());
            }
            y += 34f;

            Widgets.DrawLineHorizontal(rect.x, y, rect.width * 0.6f);
            y += 14f;

            Text.Font = GameFont.Medium;
            {
                string title = "VRF_Settings_VehicleMapLimitTitle".Translate();
                float titleW = Text.CalcSize(title).x + 4f;
                Widgets.Label(new Rect(rect.x, y, titleW, 30f), title);
                Rect tipRect = new Rect(rect.x + titleW + 2f, y + 5f, 22f, 22f);
                GUI.color = Color.yellow;
                Text.Font = GameFont.Small;
                Widgets.Label(tipRect, "?");
                Text.Font = GameFont.Medium;
                GUI.color = Color.white;
                if (Mouse.IsOver(tipRect))
                    TooltipHandler.TipRegion(tipRect, "VRF_Settings_Tip_VehicleMapLimit".Translate());
            }
            y += 34f;
            Text.Font = GameFont.Small;

            {
                const string limitKey = "MaxVehicleMapSpawnsPerRaid";
                if (!_configBufs.TryGetValue(limitKey, out string limitBuffer) || limitBuffer == null)
                {
                    limitBuffer = VRF_Mod.Settings.MaxVehicleMapSpawnsPerRaid.ToString();
                    _configBufs[limitKey] = limitBuffer;
                }

                int previousLimit = VRF_Mod.Settings.MaxVehicleMapSpawnsPerRaid;
                Widgets.TextFieldNumeric(new Rect(rect.x, y, 90f, 24f),
                    ref VRF_Mod.Settings.MaxVehicleMapSpawnsPerRaid, ref limitBuffer, 0, 100);
                _configBufs[limitKey] = limitBuffer;
                if (VRF_Mod.Settings.MaxVehicleMapSpawnsPerRaid != previousLimit)
                    VRF_Mod.Instance.WriteSettings();

                GUI.color = new Color(1f, 0.72f, 0.3f);
                Widgets.Label(new Rect(rect.x + 104f, y + 2f, rect.width - 104f, 24f),
                    "VRF_Settings_VehicleMapLimitWarning".Translate());
                GUI.color = Color.white;
            }
            y += 38f;

            Widgets.DrawLineHorizontal(rect.x, y, rect.width * 0.6f);
            y += 14f;

            Text.Font = GameFont.Medium;
            Widgets.Label(new Rect(rect.x, y, rect.width, 32f), "VRF_Settings_VehiclePointsFractionTitle".Translate());
            y += 36f;
            Text.Font = GameFont.Small;

            {
                float sliderW = Mathf.Min(rect.width * 0.5f, 360f);
                float prevFrac = VRF_Mod.Settings.VehiclePointsFraction;
                float newFrac = Widgets.HorizontalSlider(
                    new Rect(rect.x, y + 4f, sliderW, 20f),
                    prevFrac, 0f, 1f, true);
                newFrac = Mathf.Round(newFrac * 100f) / 100f;
                string pctLabel = (newFrac * 100f).ToString("F0") + " %";
                Text.Anchor = TextAnchor.MiddleLeft;
                Widgets.Label(new Rect(rect.x + sliderW + 10f, y, 60f, 28f), pctLabel);
                Text.Anchor = TextAnchor.UpperLeft;
                if (Mathf.Abs(newFrac - prevFrac) > 0.001f)
                {
                    VRF_Mod.Settings.VehiclePointsFraction = newFrac;
                    VRF_Mod.Instance.WriteSettings();
                }
            }
            y += 34f;

            Widgets.DrawLineHorizontal(rect.x, y, rect.width * 0.6f);
            y += 14f;

            Text.Font = GameFont.Medium;
            Widgets.Label(new Rect(rect.x, y, rect.width, 32f), "VRF_Settings_AutoFillAmmoFractionTitle".Translate());
            y += 36f;
            Text.Font = GameFont.Small;

            {
                float sliderW = Mathf.Min(rect.width * 0.5f, 360f);
                float prevFrac = VRF_Mod.Settings.AutoFillAmmoFraction;
                float newFrac = Widgets.HorizontalSlider(
                    new Rect(rect.x, y + 4f, sliderW, 20f),
                    prevFrac, 0f, 1f, true);
                newFrac = Mathf.Round(newFrac * 100f) / 100f;
                string pctLabel = (newFrac * 100f).ToString("F0") + " %";
                Text.Anchor = TextAnchor.MiddleLeft;
                Widgets.Label(new Rect(rect.x + sliderW + 10f, y, 60f, 28f), pctLabel);
                Text.Anchor = TextAnchor.UpperLeft;
                if (Mathf.Abs(newFrac - prevFrac) > 0.001f)
                {
                    VRF_Mod.Settings.AutoFillAmmoFraction = newFrac;
                    VRF_Mod.Instance.WriteSettings();
                }
            }
            y += 34f;

            Widgets.DrawLineHorizontal(rect.x, y, rect.width * 0.6f);
            y += 14f;

            Text.Font = GameFont.Medium;
            {
                string title  = "VRF_Settings_EstimatorTitle".Translate();
                float  titleW = Text.CalcSize(title).x + 4f;
                Widgets.Label(new Rect(rect.x, y, titleW, 32f), title);
                Rect titleTip = new Rect(rect.x + titleW + 2f, y + 7f, 22f, 22f);
                GUI.color = Color.yellow;
                Text.Font = GameFont.Small;
                Widgets.Label(titleTip, "?");
                Text.Font = GameFont.Medium;
                GUI.color = Color.white;
                if (Mouse.IsOver(titleTip))
                    TooltipHandler.TipRegion(titleTip, "VRF_Settings_Tip_Estimator".Translate());
            }
            y += 40f;
            Text.Font = GameFont.Small;

            float labelW = 200f;
            float fieldW = 100f;
            float rowH = 28f;

            DrawConfigField(rect.x, ref y, labelW, fieldW, rowH, "VRF_Settings_WeightHP".Translate(), ref VRF_Mod.Settings.WeightHP, "WeightHP");
            DrawConfigField(rect.x, ref y, labelW, fieldW, rowH, "VRF_Settings_WeightArmor".Translate(), ref VRF_Mod.Settings.WeightArmor, "WeightArmor");
            DrawConfigField(rect.x, ref y, labelW, fieldW, rowH, "VRF_Settings_WeightDPS".Translate(), ref VRF_Mod.Settings.WeightDPS, "WeightDPS");
            DrawConfigField(rect.x, ref y, labelW, fieldW, rowH, "VRF_Settings_WeightRange".Translate(), ref VRF_Mod.Settings.WeightRange, "WeightRange");
            DrawConfigField(rect.x, ref y, labelW, fieldW, rowH, "VRF_Settings_WeightSpeed".Translate(), ref VRF_Mod.Settings.WeightSpeed, "WeightSpeed");
            DrawConfigField(rect.x, ref y, labelW, fieldW, rowH, "VRF_Settings_WeightDrivers".Translate(), ref VRF_Mod.Settings.WeightDrivers, "WeightDrivers");
            DrawConfigField(rect.x, ref y, labelW, fieldW, rowH, "VRF_Settings_WeightGunners".Translate(), ref VRF_Mod.Settings.WeightGunners, "WeightGunners");
            DrawConfigField(rect.x, ref y, labelW, fieldW, rowH, "VRF_Settings_WeightPassengers".Translate(), ref VRF_Mod.Settings.WeightPassengers, "WeightPassengers");
            DrawConfigField(rect.x, ref y, labelW, fieldW, rowH, "VRF_Settings_WeightSize".Translate(), ref VRF_Mod.Settings.WeightSize, "WeightSize");
            DrawConfigField(rect.x, ref y, labelW, fieldW, rowH, "VRF_Settings_AirMultiplier".Translate(), ref VRF_Mod.Settings.AirMultiplier, "AirMultiplier");
            {
                Rect airTip = new Rect(rect.x + labelW + fieldW + 6f, y - rowH + (rowH - 18f) * 0.5f, 18f, 18f);
                GUI.color = Color.yellow;
                Widgets.Label(airTip, "?");
                GUI.color = Color.white;
                if (Mouse.IsOver(airTip))
                    TooltipHandler.TipRegion(airTip, "VRF_Settings_Tip_AirMultiplier".Translate());
            }

            y += 10f;
            bool prevTransport = VRF_Mod.Settings.AutoFillTransportVehicles;
            Widgets.CheckboxLabeled(new Rect(rect.x, y, 400f, 24f), "VRF_Settings_AutoFillTransport".Translate(), ref VRF_Mod.Settings.AutoFillTransportVehicles);
            if (prevTransport != VRF_Mod.Settings.AutoFillTransportVehicles) VRF_Mod.Instance.WriteSettings();
            y += 30f;

            bool prevSiegeDrop = VRF_Mod.Settings.AutoFillSiegeDropVehicles;
            Widgets.CheckboxLabeled(new Rect(rect.x, y, 400f, 24f), "VRF_Settings_AutoFillSiegeDrop".Translate(), ref VRF_Mod.Settings.AutoFillSiegeDropVehicles);
            if (prevSiegeDrop != VRF_Mod.Settings.AutoFillSiegeDropVehicles) VRF_Mod.Instance.WriteSettings();
            if (Mouse.IsOver(new Rect(rect.x, y, 400f, 24f)))
                TooltipHandler.TipRegion(new Rect(rect.x, y, 400f, 24f), "VRF_Settings_AutoFillSiegeDropDesc".Translate());
            y += 30f;

            bool prevDropOnly = VRF_Mod.Settings.SiegeDropOnlyOnDropRaids;
            Widgets.CheckboxLabeled(new Rect(rect.x, y, 400f, 24f), "VRF_Settings_SiegeDropOnly".Translate(), ref VRF_Mod.Settings.SiegeDropOnlyOnDropRaids);
            if (prevDropOnly != VRF_Mod.Settings.SiegeDropOnlyOnDropRaids) VRF_Mod.Instance.WriteSettings();
            if (Mouse.IsOver(new Rect(rect.x, y, 400f, 24f)))
                TooltipHandler.TipRegion(new Rect(rect.x, y, 400f, 24f), "VRF_Settings_SiegeDropOnlyDesc".Translate());
            y += 30f;

            Widgets.DrawLineHorizontal(rect.x, y, rect.width * 0.6f);
            y += 14f;

            Text.Font = GameFont.Medium;
            Widgets.Label(new Rect(rect.x, y, rect.width, 30f), "Developer / Debug");
            y += 34f;
            Text.Font = GameFont.Small;

            bool prevVerbose = VRF_Mod.Settings.VerboseAILogging;
            Widgets.CheckboxLabeled(new Rect(rect.x, y, 500f, 24f),
                "Verbose NPC vehicle AI logging",
                ref VRF_Mod.Settings.VerboseAILogging);
            if (Mouse.IsOver(new Rect(rect.x, y, 500f, 24f)))
                TooltipHandler.TipRegion(new Rect(rect.x, y, 500f, 24f),
                    "When enabled, detailed diagnostics are written to the log for every VRF NPC vehicle event: " +
                    "job given, job started/ended, duty change, lord transition, power-state change, " +
                    "enemy-target acquisition, periodic full state dump, and more. " +
                    "Disable in normal play — produces heavy log output.");
            if (prevVerbose != VRF_Mod.Settings.VerboseAILogging)
                VRF_Mod.Instance.WriteSettings();
            y += 30f;

            if (Widgets.ButtonText(new Rect(rect.x, y, 120f, 30f), "VRF_Settings_Save".Translate()))
            {
                VRF_Mod.Instance.WriteSettings();
                Messages.Message("VRF_Settings_Saved".Translate(), MessageTypeDefOf.PositiveEvent, false);
            }

            if (Widgets.ButtonText(new Rect(rect.x + 130f, y, 120f, 30f), "VRF_Settings_Reset".Translate()))
            {
                VRF_Mod.Settings.ResetEstimatorSettings();
                _configBufs.Clear();
                VRF_Mod.Instance.WriteSettings();
                Messages.Message("VRF_Settings_ResetMsg".Translate(), MessageTypeDefOf.NeutralEvent, false);
            }

            Widgets.EndScrollView();
        }

        private static void DrawConfigField(float x, ref float y, float labelW, float fieldW, float rowH, string label, ref float val, string key)
        {
            Widgets.Label(new Rect(x, y, labelW, rowH), label);
            
            if (!_configBufs.TryGetValue(key, out string buf) || buf == null)
            {
                buf = val.ToString(System.Globalization.CultureInfo.InvariantCulture);
                _configBufs[key] = buf;
            }
            
            string text = Widgets.TextField(new Rect(x + labelW, y, fieldW, 24f), buf);
            if (text != buf)
            {
                string filtered = "";
                foreach (char c in text)
                {
                    if (char.IsDigit(c) || c == '.' || c == ',')
                        filtered += c;
                }
                
                _configBufs[key] = filtered;

                string parseStr = filtered.Replace(',', '.');
                if (float.TryParse(parseStr, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out float result))
                {
                    val = Mathf.Clamp(result, 0f, 1000f);
                }
            }
            
            y += rowH;
        }

        private static VRF_NaturalRaidVehicleEntry CloneVehicleEntry(VRF_NaturalRaidVehicleEntry src)
        {
            var c = new VRF_NaturalRaidVehicleEntry(src.vehicleKindDefName)
            {
                enabled = src.enabled, combatPowerOverride = src.combatPowerOverride,
                minRaidPoints = src.minRaidPoints, maxRaidPoints = src.maxRaidPoints,
                fuelPercent = src.fuelPercent, helicopterMode = src.helicopterMode,
                helicopterMoveSpeed = src.helicopterMoveSpeed, airVehicleType = src.airVehicleType, forceSpawn = src.forceSpawn,
                isSiegeDrop = src.isSiegeDrop, allowDropPod = src.allowDropPod,
                gravshipPresetName = src.gravshipPresetName
            };
            foreach (var t in src.turretAmmo) c.turretAmmo.Add(new VRF_TurretAmmoEntry(t.turretKey) { ammoPercent = t.ammoPercent });
            if (src.paintConfig != null) c.paintConfig = new VRF_PaintConfig { mode = src.paintConfig.mode, colorOne = src.paintConfig.colorOne, colorTwo = src.paintConfig.colorTwo, colorThree = src.paintConfig.colorThree, patternDefName = src.paintConfig.patternDefName };
            foreach (var lo in src.upgradeLoadouts) { var cl = new VRF_UpgradeLoadout(); cl.nodeKeys.AddRange(lo.nodeKeys); foreach (var ua in lo.upgradeAmmo) cl.upgradeAmmo.Add(new VRF_TurretAmmoEntry(ua.turretKey) { ammoPercent = ua.ammoPercent }); c.upgradeLoadouts.Add(cl); }
            return c;
        }

        private static void PasteIntoEntry(VRF_NaturalRaidVehicleEntry src, VRF_NaturalRaidVehicleEntry dest)
        {
            dest.enabled = src.enabled; dest.combatPowerOverride = src.combatPowerOverride;
            dest.minRaidPoints = src.minRaidPoints; dest.maxRaidPoints = src.maxRaidPoints;
            dest.fuelPercent = src.fuelPercent; dest.helicopterMode = src.helicopterMode;
            dest.helicopterMoveSpeed = src.helicopterMoveSpeed; dest.airVehicleType = src.airVehicleType; dest.forceSpawn = src.forceSpawn;
            dest.isSiegeDrop = src.isSiegeDrop; dest.allowDropPod = src.allowDropPod;
            dest.gravshipPresetName = src.gravshipPresetName;
            dest.turretAmmo.Clear();
            foreach (var t in src.turretAmmo) dest.turretAmmo.Add(new VRF_TurretAmmoEntry(t.turretKey) { ammoPercent = t.ammoPercent });
            dest.paintConfig = null;
            if (src.paintConfig != null) dest.paintConfig = new VRF_PaintConfig { mode = src.paintConfig.mode, colorOne = src.paintConfig.colorOne, colorTwo = src.paintConfig.colorTwo, colorThree = src.paintConfig.colorThree, patternDefName = src.paintConfig.patternDefName };
            dest.upgradeLoadouts.Clear();
            foreach (var lo in src.upgradeLoadouts) { var cl = new VRF_UpgradeLoadout(); cl.nodeKeys.AddRange(lo.nodeKeys); foreach (var ua in lo.upgradeAmmo) cl.upgradeAmmo.Add(new VRF_TurretAmmoEntry(ua.turretKey) { ammoPercent = ua.ammoPercent }); dest.upgradeLoadouts.Add(cl); }
        }
    }
}
