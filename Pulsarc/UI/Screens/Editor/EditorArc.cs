using Microsoft.Xna.Framework.Graphics;
using Pulsarc.Skinning;
using Pulsarc.UI.Screens.Gameplay;
using Pulsarc.Utils;

namespace Pulsarc.UI.Screens.Editor
{
    public class EditorArc : HitObject, IEditorHitObject
    {
        private int lastFrameTime = 0;
        private double lastFrameScale = 0;
        private double lastFrameCrosshairZLoc = 0;

        private static Texture2D defaultTexture = Skin.Assets["arcs"];
        private static Texture2D selected = PulsarcDrawing.CreateSelectTexture(defaultTexture);

        public EditorArc(int time, double angle, int keys)
            : base(time, angle, keys, 0, false)
        {
            Hittable = false;
        }

        public void Select()
        {
            Texture = selected;
        }

        public void Deselect()
        {
            Texture = defaultTexture;
        }

        public override void RecalcPos(int currentTime, double currentScale, double crosshairZLoc)
        {
            if (SameAsLastFrame(currentTime, currentScale, crosshairZLoc))
                return;

            SetZLocation(currentTime, currentScale, crosshairZLoc);

            Resize(FindArcRadius());
        }

        private bool SameAsLastFrame(int time, double scale, double crosshairZLoc)
        {
            // The conditionals we're looking at
            bool timeSame = time == lastFrameTime;
            bool scaleSame = scale == lastFrameScale;
            bool crosshairZLocSame = crosshairZLoc == lastFrameCrosshairZLoc;

            // If any of the current values are different,
            // change the lastFrame values to the current value.
            lastFrameTime = !timeSame ? time : lastFrameTime;
            lastFrameScale = !scaleSame ? scale : lastFrameScale;
            lastFrameCrosshairZLoc = !crosshairZLocSame ? crosshairZLoc : lastFrameCrosshairZLoc;

            // If all our conditionals were true, then this frame is the same.
            // If at least one of our conditionals were false, this frame is different.
            return timeSame && scaleSame && crosshairZLocSame;
        }
    }
}
