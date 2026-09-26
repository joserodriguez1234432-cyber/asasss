using System;
using UnityEngine;
using Verse;

namespace VehicleRaidFramework
{
    public class Dialog_VRF_ColorPicker : Window
    {
        private readonly Action<Color> _onApply;
        private float _h, _s, _v;
        private string _hexBuf;

        public Dialog_VRF_ColorPicker(Color initial, Action<Color> onApply)
        {
            _onApply = onApply;
            Color.RGBToHSV(initial, out _h, out _s, out _v);
            _hexBuf = ColorUtility.ToHtmlStringRGB(initial);
            doCloseX         = true;
            forcePause       = true;
            absorbInputAroundWindow = true;
        }

        public override Vector2 InitialSize => new Vector2(320f, 260f);

        private Color Current => Color.HSVToRGB(_h, _s, _v);

        public override void DoWindowContents(Rect inRect)
        {
            float y = inRect.y;
            Text.Font = GameFont.Small;

            Rect swatchR = new Rect(inRect.xMax - 54f, y, 50f, 50f);
            Widgets.DrawBoxSolid(swatchR, Current);
            Widgets.DrawBox(swatchR, 1);

            float sliderW = inRect.width - 60f;

            Widgets.Label(new Rect(inRect.x, y, 20f, 20f), "H");
            _h = Widgets.HorizontalSlider(new Rect(inRect.x + 22f, y + 2f, sliderW, 18f), _h, 0f, 1f);
            y += 24f;

            Widgets.Label(new Rect(inRect.x, y, 20f, 20f), "S");
            _s = Widgets.HorizontalSlider(new Rect(inRect.x + 22f, y + 2f, sliderW, 18f), _s, 0f, 1f);
            y += 24f;

            Widgets.Label(new Rect(inRect.x, y, 20f, 20f), "V");
            _v = Widgets.HorizontalSlider(new Rect(inRect.x + 22f, y + 2f, sliderW, 18f), _v, 0f, 1f);
            y += 32f;

            Widgets.Label(new Rect(inRect.x, y, 30f, 24f), "#");
            string newHex = Widgets.TextField(new Rect(inRect.x + 22f, y, 100f, 24f), _hexBuf);
            if (newHex != _hexBuf)
            {
                _hexBuf = newHex;
                Color parsed;
                if (ColorUtility.TryParseHtmlString("#" + newHex, out parsed))
                    Color.RGBToHSV(parsed, out _h, out _s, out _v);
            }
            else
                _hexBuf = ColorUtility.ToHtmlStringRGB(Current);
            y += 32f;

            if (Widgets.ButtonText(new Rect(inRect.x, y, 90f, 28f), "Accept".Translate()))
            {
                _onApply(Current);
                Close();
            }
            if (Widgets.ButtonText(new Rect(inRect.x + 96f, y, 90f, 28f), "CancelButton".Translate()))
                Close();
        }
    }
}
