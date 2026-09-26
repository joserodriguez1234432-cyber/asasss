using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;

namespace VehicleRaidFramework
{
    public class Dialog_VRF_RaidStrategies : Window
    {
        private readonly List<string> _allowedStrategies;
        private readonly Action _onChanged;
        private readonly string _vehicleLabel;

        private readonly List<(string modName, List<(string defName, string label)> defs)> _groups;

        private Vector2 _scrollPos;

        private const float RowH  = 28f;
        private const float Pad   = 8f;

        public override Vector2 InitialSize => new Vector2(500f, 560f);

        public Dialog_VRF_RaidStrategies(VRF_NaturalRaidVehicleEntry entry, string vehicleLabel)
            : this(entry.allowedRaidStrategies, vehicleLabel, () => VRF_Mod.Instance.WriteSettings())
        {
        }

        public Dialog_VRF_RaidStrategies(VRF_GravshipRaidEntry gravEntry, string label)
            : this(gravEntry.allowedRaidStrategies, label, () => VRF_Mod.Instance.WriteSettings())
        {
        }

        public Dialog_VRF_RaidStrategies(List<string> allowedStrategies, string vehicleLabel, Action onChanged)
        {
            _allowedStrategies = allowedStrategies ?? new List<string>();
            _onChanged         = onChanged;
            _vehicleLabel      = vehicleLabel;
            doCloseButton = true;
            doCloseX      = true;
            absorbInputAroundWindow = false;
            forcePause    = false;

            var items = DefDatabase<RaidStrategyDef>.AllDefsListForReading
                .Where(d => d.defName != null)
                .Select(d => (mod: d.modContentPack?.Name ?? "Core", defName: d.defName, label: !d.label.NullOrEmpty() ? d.label : d.defName))
                .ToList();

            items.Add((mod: "Core", defName: "DropPod", label: "Drop Pod"));

            _groups = items
                .GroupBy(d => d.mod)
                .OrderBy(g => g.Key)
                .Select(g => (g.Key, g.OrderBy(d => d.label).Select(d => (d.defName, d.label)).ToList()))
                .ToList();

            const float modH = 26f;
            const float sepH = 6f;
            _totalContentHeight = 0f;
            for (int i = 0; i < _groups.Count; i++)
            {
                _totalContentHeight += modH + 4f + _groups[i].defs.Count * RowH + sepH;
            }
        }

        private readonly float _totalContentHeight;

        public override void DoWindowContents(Rect inRect)
        {
            float y = inRect.y;

            Text.Font = GameFont.Medium;
            Widgets.Label(new Rect(inRect.x, y, inRect.width, 30f),
                "VRF_StrategyDialog_Title".Translate(_vehicleLabel));
            y += 34f;
            Text.Font = GameFont.Small;

            GUI.color = new Color(0.75f, 0.75f, 0.75f);
            Widgets.Label(new Rect(inRect.x, y, inRect.width, 20f),
                "VRF_StrategyDialog_Hint".Translate());
            GUI.color = Color.white;
            y += 24f;

            float btnW = (inRect.width - Pad) * 0.5f;
            if (Widgets.ButtonText(new Rect(inRect.x, y, btnW, 26f), "VRF_StrategyDialog_AllOn".Translate()))
            {
                _allowedStrategies.Clear();
                _onChanged?.Invoke();
            }
            if (Widgets.ButtonText(new Rect(inRect.x + btnW + Pad, y, btnW, 26f), "VRF_StrategyDialog_AllOff".Translate()))
            {
                _allowedStrategies.Clear();
                foreach (var (_, defs) in _groups)
                    foreach (var (defName, _) in defs)
                        _allowedStrategies.Add(defName);
                _onChanged?.Invoke();
            }
            y += 32f;

            Widgets.DrawLineHorizontal(inRect.x, y, inRect.width);
            y += 6f;

            const float ModHeaderH = 26f;
            const float SepH       = 6f;
            float totalH = _totalContentHeight;
            Rect scrollArea = new Rect(inRect.x, y, inRect.width, inRect.yMax - y - 36f);
            Rect viewRect   = new Rect(0f, 0f, scrollArea.width - 20f, totalH);
            Widgets.BeginScrollView(scrollArea, ref _scrollPos, viewRect);

            float vy = 0f;
            foreach (var (modName, defs) in _groups)
            {
                Widgets.DrawBoxSolid(new Rect(0f, vy, viewRect.width, ModHeaderH),
                    new Color(0.12f, 0.12f, 0.12f, 0.85f));
                GUI.color   = new Color(0.9f, 0.82f, 0.5f);
                Text.Anchor = TextAnchor.MiddleLeft;
                Widgets.Label(new Rect(Pad, vy, viewRect.width - Pad, ModHeaderH), modName);
                Text.Anchor = TextAnchor.UpperLeft;
                GUI.color   = Color.white;
                vy += ModHeaderH + 4f;

                foreach (var (defName, label) in defs)
                {
                    bool isAllowed = _allowedStrategies.Count == 0
                                  || !_allowedStrategies.Contains(defName);

                    Rect row = new Rect(0f, vy, viewRect.width, RowH - 2f);
                    Widgets.DrawHighlightIfMouseover(row);

                    bool prev = isAllowed;
                    Widgets.CheckboxLabeled(
                        new Rect(Pad, vy + (RowH - 24f) * 0.5f, viewRect.width - Pad * 2f, 24f),
                        label, ref isAllowed);

                    if (Mouse.IsOver(row))
                        TooltipHandler.TipRegion(row, defName);

                    if (isAllowed != prev)
                    {
                        if (isAllowed)
                        {
                            _allowedStrategies.Remove(defName);
                        }
                        else
                        {
                            if (!_allowedStrategies.Contains(defName))
                                _allowedStrategies.Add(defName);
                        }
                        _onChanged?.Invoke();
                    }

                    vy += RowH;
                }

                GUI.color = new Color(0.35f, 0.35f, 0.35f, 0.8f);
                Widgets.DrawLineHorizontal(0f, vy + SepH * 0.5f - 1f, viewRect.width);
                GUI.color = Color.white;
                vy += SepH;
            }

            Widgets.EndScrollView();
        }
    }
}
