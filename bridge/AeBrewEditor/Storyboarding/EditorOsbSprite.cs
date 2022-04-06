﻿using BrewLib.Graphics;
using BrewLib.Graphics.Cameras;
using BrewLib.Graphics.Renderers;
using BrewLib.Graphics.Textures;
using BrewLib.Util;
using OpenTK;
using OpenTK.Graphics;
using AeBrewCommon.Mapset;
using AeBrewCommon.Storyboarding;
using AeBrewCommon.Util;
using System;
using System.IO;

namespace AeBrewEditor.Storyboarding
{
    public class EditorOsbSprite : OsbSprite, DisplayableObject, HasPostProcess
    {
        public readonly static RenderStates AlphaBlendStates = new RenderStates();
        public readonly static RenderStates AdditiveStates = new RenderStates() { BlendingFactor = new BlendingFactorState(BlendingMode.Additive), };

        public void Draw(DrawContext drawContext, Camera camera, Box2 bounds, float opacity, Project project, FrameStats frameStats)
            => Draw(drawContext, camera, bounds, opacity, project, frameStats, this);

        public void PostProcess()
        {
            if (InGroup)
                EndGroup();
        }

        public static void Draw(DrawContext drawContext, Camera camera, Box2 bounds, float opacity, Project project, FrameStats frameStats, OsbSprite sprite)
        {
            var time = project.DisplayTime * 1000;
            if (sprite.TexturePath == null || !sprite.IsActive(time)) return;

            if (frameStats != null)
            {
                frameStats.SpriteCount++;
                frameStats.CommandCount += sprite.CommandCost;
                frameStats.IncompatibleCommands |= sprite.HasIncompatibleCommands;
                frameStats.OverlappedCommands |= sprite.HasOverlappedCommands;
            }

            var fade = sprite.OpacityAt(time);
            if (fade < 0.00001f) return;

            var scale = (Vector2)sprite.ScaleAt(time);
            if (scale.X == 0 || scale.Y == 0) return;
            if (sprite.FlipHAt(time)) scale.X = -scale.X;
            if (sprite.FlipVAt(time)) scale.Y = -scale.Y;

            Texture2dRegion texture = null;
            var fullPath = Path.Combine(project.MapsetPath, sprite.GetTexturePathAt(time));
            try
            {
                texture = project.TextureContainer.Get(fullPath);
                if (texture == null)
                {
                    fullPath = Path.Combine(project.ProjectAssetFolderPath, sprite.GetTexturePathAt(time));
                    texture = project.TextureContainer.Get(fullPath);
                }
            }
            catch (IOException)
            {
                // Happens when another process is writing to the file, will try again later.
                return;
            }
            if (texture == null) return;

            var position = sprite.PositionAt(time);
            var rotation = sprite.RotationAt(time);
            var color = sprite.ColorAt(time);
            var finalColor = ((Color4)color)
                .LerpColor(Color4.Black, project.DimFactor)
                .WithOpacity(opacity * fade);
            var additive = sprite.AdditiveAt(time);

            var origin = GetOriginVector(sprite.Origin, texture.Width, texture.Height);

            if (frameStats != null)
            {
                var size = texture.Size * scale;

                var spriteObb = new OrientedBoundingBox(position, origin * scale, size.X, size.Y, rotation);
                if (spriteObb.Intersects(OsuHitObject.WidescreenStoryboardBounds))
                {
                    frameStats.EffectiveCommandCount += sprite.CommandCost;

                    // Approximate how much of the sprite is on screen
                    var spriteAabb = spriteObb.GetAABB();
                    var intersection = spriteAabb.IntersectWith(OsuHitObject.WidescreenStoryboardBounds);
                    var aabbIntersectionFactor = (intersection.Width * intersection.Height) / (spriteAabb.Width * spriteAabb.Height);

                    var intersectionArea = size.X * size.Y * aabbIntersectionFactor;
                    frameStats.ScreenFill += Math.Min(OsuHitObject.WidescreenStoryboardArea, intersectionArea) / OsuHitObject.WidescreenStoryboardArea;
                }
            }

            var boundsScaling = bounds.Height / 480;
            DrawState.Prepare(drawContext.Get<QuadRenderer>(), camera, additive ? AdditiveStates : AlphaBlendStates)
                .Draw(texture, bounds.Left + bounds.Width * 0.5f + (position.X - 320) * boundsScaling, bounds.Top + position.Y * boundsScaling,
                    origin.X, origin.Y, scale.X * boundsScaling, scale.Y * boundsScaling, rotation, finalColor);
        }
    }
}
