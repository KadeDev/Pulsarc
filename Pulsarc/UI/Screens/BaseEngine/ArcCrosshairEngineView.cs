﻿using Microsoft.Xna.Framework;
using Pulsarc.UI.Common;
using Pulsarc.UI.Screens.Gameplay;
using Pulsarc.Utils;
using System.Collections.Generic;
using Wobble.Screens;

namespace Pulsarc.UI.Screens.BaseEngine
{
    public abstract class ArcCrosshairEngineView : ScreenView
    {
        protected Crosshair Crosshair => GetEngine().GetCrosshair();
        protected Background Background => GetEngine().Background;
        protected double Time => GetEngine().GetCurrentTime();

        protected ArcCrosshairEngine GetEngine() => (ArcCrosshairEngine)Screen;

        public bool Initialized { get; protected set; } = false;

        public ArcCrosshairEngineView(Screen screen) : base(screen) { }

        public virtual void Init()
        {
            if (AlreadyInitialized()) { return; }

            Initialized = true;
        }

        protected bool AlreadyInitialized()
        {
            if (Initialized)
            {
                PulsarcLogger.Warning("This engine was already initialized, " +
                    "yet initialization was attmepted a second time!");
            }

            return Initialized;
        }

        public override void Draw(GameTime gameTime)
        {
            // Don't bother drawing the background if dim is 100%
            if ((Background.Dimmed && Background.DimTexture.Opacity != 0f) || !Background.Dimmed)
            {
                Background.Draw();
            }

            Crosshair.Draw();
            DrawArcs();
        }

        protected virtual void DrawArcs()
        {
            // Go through each column
            for (int i = 0; i < GetEngine().KeyCount; i++)
            {
                // Go through the arcs in the column
                for (int k = 0; k < GetEngine().Columns[i].UpdateHitObjects.Count; k++)
                {
                    HitObject hitObject = GetEngine().Columns[i].UpdateHitObjects[k];

                    // If the arc is on screen, draw it.
                    if (hitObject.IsSeen())
                    {
                        hitObject.Draw();
                    }

                    // If the arc is inside the "IgnoreTime" window, stop bothering to
                    // look at the rest of the arcs in this column, we are looking at offscreen
                    // HitObjects at this point.
                    double speed = GetEngine().CurrentSpeedMultiplier;
                    float zLocation = GetEngine().GetCrosshair().GetZLocation();

                    if (hitObject.IsSeenAt(speed, zLocation) - GetEngine().IgnoreTime
                        > GetEngine().GetCurrentTime())
                    {
                        break;
                    }
                }
            }
        }
    }
}
