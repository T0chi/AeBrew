﻿using BrewLib.Util;
using OpenTK;
using OpenTK.Graphics;
using System.Drawing;
using Tiny;

namespace AeBrewCommon.Subtitles
{
    public class FontShadow : FontEffect
    {
        public int Thickness = 1;
        public Color4 Color = new Color4(0, 0, 0, 100);

        public bool Overlay => false;
        public Vector2 Measure() => new Vector2(Thickness * 2);

        public void Draw(Bitmap bitmap, Graphics textGraphics, Font font, StringFormat stringFormat, string text, float x, float y)
        {
            if (Thickness < 1)
                return;

            using (var brush = new SolidBrush(System.Drawing.Color.FromArgb(Color.ToArgb())))
                for (var i = 1; i <= Thickness; i++)
                    textGraphics.DrawString(text, font, brush, x + i, y + i, stringFormat);
        }

        public bool Matches(TinyToken cachedEffectRoot)
            => cachedEffectRoot.Value<string>("Type") == GetType().FullName &&
                cachedEffectRoot.Value<int>("Thickness") == Thickness &&
                MathUtil.FloatEquals(cachedEffectRoot.Value<float>("ColorR"), Color.R, 0.00001f) &&
                MathUtil.FloatEquals(cachedEffectRoot.Value<float>("ColorG"), Color.G, 0.00001f) &&
                MathUtil.FloatEquals(cachedEffectRoot.Value<float>("ColorB"), Color.B, 0.00001f) &&
                MathUtil.FloatEquals(cachedEffectRoot.Value<float>("ColorA"), Color.A, 0.00001f);

        public TinyObject ToTinyObject() => new TinyObject
        {
            { "Type", GetType().FullName },
            { "Thickness", Thickness },
            { "ColorR", Color.R },
            { "ColorG", Color.G },
            { "ColorB", Color.B },
            { "ColorA", Color.A },
        };
    }
}
