﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Pulsarc.Beatmaps;
using Pulsarc.Skinning;
using Pulsarc.UI.Screens.Gameplay;
using Pulsarc.Utils;
using Pulsarc.Utils.Maths;
using System;
using Wobble.Logging;
using Wobble.Screens;

namespace Pulsarc.UI.Screens.SongSelect.UI
{
    public class BeatmapCard : Card
    {
        public Beatmap Beatmap { get; set; }
        
        public static Texture2D DefaultTexture => Skin.Assets["beatmap_card"];

        // Stats
        public static readonly Anchor DefaultAnchor = Skin.GetConfigAnchor("song_select", "Properties", "BeatmapCardAnchor");

        private static readonly int OffsetX = Skin.GetConfigInt("song_select", "Properties", "BeatmapCardX");
        private static readonly int OffsetY = Skin.GetConfigInt("song_select", "Properties", "BeatmapCardY");
        public static readonly Vector2 StartPosition = Skin.GetConfigStartPosition("song_select", "Properties", "BeatmapCardStartPos") + new Vector2(OffsetX, OffsetY);

        public static readonly int Margin = Skin.GetConfigInt("song_select", "Properties", "BeatmapCardMargin");
        public static readonly int TotalHeight = DefaultTexture.Height + Margin;
        public static readonly int TotalWidth = DefaultTexture.Width + Margin;

        public int Index { get; private set; }

        public Vector2 PersonalStartPosition => StartPosition + personalStartPosOffset;
        private Vector2 personalStartPosOffset;

        // How far this card will extend when selected
        private float clickedDistance;
        // What direction this card will extend in
        private string clickedDirection;

        private float currentClickDistance = 0f;
        private float lastClickDistance = 0f;

        // The difficulty of the map represented as a bar
        private BeatmapCardDifficulty diffBar;
        private Vector2 diffBarOffset;
        private Anchor diffBarStartAnchor;

        /// <summary>
        /// A card displayed on the Song Select Screen. When clicked it loads the beatmap associated with this card.
        /// TODO: Cleanup
        /// </summary>
        /// <param name="beatmap">The beatmap associated with this card.</param>
        /// <param name="truePosition">The position of the card.</param>
        public BeatmapCard(Beatmap beatmap, int index)
            : base(DefaultTexture, StartPosition, DefaultAnchor)
        {
            // set clicked distance and direction
            clickedDistance = Skin.GetConfigFloat(Config, "Properties", "BeatmapCardSelectOffset");
            clickedDirection = Skin.GetConfigString(Config, "Properties", "BeatmapCardSelectDirection");

            Index = index;

            personalStartPosOffset = new Vector2(0, TotalHeight * Pulsarc.HeightScale * Index);

            // set beatmap
            Beatmap = beatmap;

            // set diffbar
            SetDiffBar();

            // set metadata
            SetMetadata();

            ChangePosition(PersonalStartPosition);
        }

        private void SetDiffBar()
        {
            float percent = (float)(Beatmap.Difficulty / 10f);

            Anchor startAnchor;
            Vector2 startPos = Skin.GetConfigStartPosition(Config, Section, "DiffBarStartPos", out startAnchor, this);

            Anchor diffAnchor = GetSkinnableAnchor("DiffBarAnchor");

            diffBar = new BeatmapCardDifficulty
            (
                startPos,
                // diffbar displayed percentage is 0 if less than 0, and 10 if greater than 10
                percent <= 10 ? percent >= 0 ? percent : 0 : 10,
                diffAnchor
            );

            int diffBarXOffset = GetSkinnableInt("DiffBarX");
            int diffBarYOffset = GetSkinnableInt("DiffBarY");
            diffBar.Move(diffBarXOffset, diffBarYOffset);
            diffBarOffset = diffBar.anchorPosition - AnchorUtil.FindDrawablePosition(startAnchor, this);
            diffBarStartAnchor = startAnchor;
        }

        private void SetMetadata()
        {
            AddTextDisplayElement("Title");
            TextElements[0].Update(Beatmap.Title);

            AddTextDisplayElement("Artist");
            TextElements[1].Update(Beatmap.Artist);

            AddTextDisplayElement("Version");
            TextElements[2].Update(Beatmap.Version);

            AddTextDisplayElement("Mapper");
            TextElements[3].Update(Beatmap.Mapper);

            AddTextDisplayElement("Difficulty");
            TextElements[4].Update(string.Format("{0:0.00}", Beatmap.Difficulty));
        }

        protected override void SetConfigAndSection()
        {
            Config = "song_select";
            Section = "Metadata";
        }

        public override void Draw()
        {
            if (!OnScreen())
                return;

            base.Draw();
            diffBar.Draw();
        }

        /// <summary>
        /// The card moving in and out depending on its selected state.
        /// </summary>
        public void AdjustClickDistance()
        {
            // If clicked, smoothly move to the clicked distance
            if (IsClicked && Math.Round(currentClickDistance, 3) < Math.Round(clickedDistance, 3))
                currentClickDistance = PulsarcMath.Lerp(currentClickDistance, clickedDistance, (float)PulsarcTime.DeltaTime / 100f);

            // Else if not clicked and currentClickDistacne is greater than 0, smoothly move to 0
            else if (!IsClicked && Math.Round(currentClickDistance, 3) > 0)
                currentClickDistance = PulsarcMath.Lerp(currentClickDistance, 0, (float)PulsarcTime.DeltaTime / 100f);

            // Else, end the method.
            else
                return;

            float diff = lastClickDistance - currentClickDistance;
            lastClickDistance = currentClickDistance;

            switch (clickedDirection)
            {
                case "Left":
                    Move(new Vector2(diff, 0));
                    break;
                case "Right":
                    Move(new Vector2(-diff, 0));
                    break;
                case "Up":
                    Move(new Vector2(0, diff));
                    break;
                case "Down":
                    Move(new Vector2(0, -diff));
                    break;
                default:
                    goto case "Left";
            }
        }

        /// <summary>
        /// When clicked, start playing the beatmap.
        /// </summary>
        public void OnClick()
        {
            if (IsClicked)
            {
                GameplayEngine gameplay = new GameplayEngine();
                ScreenManager.AddScreen(gameplay);
                gameplay.Init(Beatmap);
            }
            else
            {
                string path = Beatmap.GetFullAudioPath();

                if (AudioManager.songPath != path)
                {
                    AudioManager.songPath = Beatmap.GetFullAudioPath();
                    AudioManager.audioRate = Utils.Config.GetFloat("Gameplay", "SongRate");
                    AudioManager.StartLazyPlayer();

                    if (Beatmap.PreviewTime != 0)
                        AudioManager.DeltaTime(Beatmap.PreviewTime);

                    PulsarcLogger.Important($"Now Playing: {AudioManager.songPath}", LogType.Runtime);
                }
            }
        }

        public void SetClicked(bool set)
        {
            IsClicked = set;
        }

        /// <summary>
        /// Adds diffBar to the elements to be updated.
        /// </summary>
        protected override void UpdateElements()
        {
            // Don't bother updating if we aren't on screen.
            if (!OnScreen())
                return;

            base.UpdateElements();

            diffBar?.ChangePosition(AnchorUtil.FindDrawablePosition(diffBarStartAnchor, this) + diffBarOffset);
        }
    }
}
