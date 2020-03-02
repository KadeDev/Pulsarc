﻿using Microsoft.Xna.Framework;
using Pulsarc.Skinning;
using System;
using Wobble.Logging;

namespace Pulsarc.UI.Screens.Editor.UI
{
    public enum Beat
    {
        Whole = 1,
        Half = 2,
        Third = 3,
        Fourth = 4,
        Sixth = 6,
        Eighth = 8,
        Twelveth = 12,
        Sixteenth = 16
    }

    public abstract class BeatDisplay : Drawable
    {
        // The color for each kind of beat.
        protected static readonly Color WholeBeatColor = GetColor(Beat.Whole);
        protected static readonly Color HalfBeatColor = GetColor(Beat.Half);
        protected static readonly Color ThirdBeatColor = GetColor(Beat.Third);
        protected static readonly Color FourthBeatColor = GetColor(Beat.Fourth);
        protected static readonly Color SixthBeatColor = GetColor(Beat.Sixth);
        protected static readonly Color EighthBeatColor = GetColor(Beat.Eighth);
        protected static readonly Color TwelvethBeatColor = GetColor(Beat.Twelveth);
        protected static readonly Color SixteenthBeatColor = GetColor(Beat.Sixteenth);

        // The Beat this object represents
        public Beat Beat { get; protected set; }

        // The time that this beat corresponds to in the audio
        public int Time { get; protected set; }
        
        // The Scale of the editor (determines spacing between notes)
        public float Scale { get; protected set; }

        public BeatDisplay(Beat beat, int time, float scale)
        {
            Beat = beat;
            Time = time;
            Scale = scale;
            AspectRatio = -1;

            SetBeatTexture();
        }

        /// <summary>
        /// Set the Texture for this BeatDisplay
        /// </summary>
        protected abstract void SetBeatTexture();

        /// <summary>
        /// Gets the user-defined color for beat provided
        /// </summary>
        /// <param name="beat">The beat to get the color of.</param>
        /// <returns></returns>
        private static Color GetColor(Beat beat)
        {
            string name = Enum.GetName(typeof(Beat), beat) + "BeatColor";

            try
            {
                return Skin.GetConfigColor("editor", "Colors", name);
            }
            catch
            {
                Logger.Debug("Something went wrong with setting beat colors!\n" +
                    $"Please make that \"{name}\"is accounted for and is spelled correctly in editor.ini!\n" +
                    "Setting color to Black.", LogType.Runtime);

                return Color.Black;
            }
        }
    }
}