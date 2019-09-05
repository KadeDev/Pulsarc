﻿using Microsoft.Xna.Framework;
using Pulsarc.Skinning;
using Pulsarc.UI.Common;
using Pulsarc.UI.Screens.Gameplay.UI;
using Wobble.Screens;

namespace Pulsarc.UI.Screens.Gameplay
{
    public class GameplayEngineView : ScreenView
    {

        // UI Elements

        Accuracy accuracyDisplay;
        Score scoreDisplay;
        Combo comboDisplay;
        JudgeBox judgeBox;
        AccuracyMeter accMeter;
        public Crosshair crosshair;

        Background background;

        private GameplayEngine GetGameplayEngine() { return (GameplayEngine)Screen; }

        /// <summary>
        /// Create the GameplayEngineView, in otherwords the UI/HUD elements during Gameplay.
        /// </summary>
        /// <param name="screen">The screen this GameplayEngineView is working with.</param>
        public GameplayEngineView(Screen screen) : base(screen) { }

        /// <summary>
        /// Initialize this GameplayEngineView with new UI elements.
        /// </summary>
        public void Init()
        {
            // Initialize UI depending on skin config
            crosshair = GetGameplayEngine().crosshair;

            scoreDisplay = new Score(new Vector2(getSkinnablePosition("ScoreX"), getSkinnablePosition("ScoreY")), centered: true);
            accuracyDisplay = new Accuracy(new Vector2(getSkinnablePosition("AccuracyX"), getSkinnablePosition("AccuracyY")), centered: true);
            comboDisplay = new Combo(new Vector2(getSkinnablePosition("ComboX"), getSkinnablePosition("ComboY")), centered: true);

            judgeBox = new JudgeBox(new Vector2(getSkinnablePosition("JudgesX"), getSkinnablePosition("JudgesY")));
            accMeter = new AccuracyMeter(new Vector2(getSkinnablePosition("AccMeterX"), getSkinnablePosition("AccMeterY"))
                                       , new Vector2(getSkinnablePosition("AccMeterWidth"), getSkinnablePosition("AccMeterHeight")));

            background = GetGameplayEngine().background;
        }

        /// <summary>
        /// Add a hit and its judgement.
        /// </summary>
        /// <param name="time">The time of the hit.</param>
        /// <param name="error">The error of the hit (deltaTime / audio rate)</param>
        /// <param name="judge">The base score of the judgement.</param>
        public void addHit(double time, int error, int judge)
        {
            accMeter.addError(time, error);
            addJudge(time, judge);
        }

        /// <summary>
        /// Add a judgement.
        /// </summary>
        /// <param name="time">The time of the judgement.</param>
        /// <param name="judge">The base score of the judgement.</param>
        public void addJudge(double time, int judge)
        {
            judgeBox.Add(time, judge);
        }

        public override void Update(GameTime gameTime)
        {
            double accuracyTotal = 0;
            foreach (JudgementValue judge in GetGameplayEngine().judgements)
            {
                accuracyTotal += judge.acc;
            }

            accuracyDisplay.Update(GetGameplayEngine().judgements.Count > 0 ? accuracyTotal / GetGameplayEngine().judgements.Count : 1);
            scoreDisplay.Update(GetGameplayEngine().score_display);
            comboDisplay.Update(GetGameplayEngine().combo);
            judgeBox.Update(GetGameplayEngine().time);
            accMeter.Update(GetGameplayEngine().time);
        }

        public override void Draw(GameTime gameTime)
        {
            if (!GameplayEngine.active) return;

            // Draw everything
            background.Draw();
            crosshair.Draw();
            drawArcs();
            accuracyDisplay.Draw();
            scoreDisplay.Draw();
            comboDisplay.Draw();
            judgeBox.Draw();
            accMeter.Draw();
        }

        private void drawArcs()
        {
            bool skip;
            for (int i = 0; i < GetGameplayEngine().keys; i++)
            {
                skip = false;
                for (int k = 0; k < GetGameplayEngine().columns[i].updateHitObjects.Count && !skip; k++)
                {

                    if (GetGameplayEngine().columns[i].updateHitObjects[k].Value.IsSeen())
                    {
                        GetGameplayEngine().columns[i].updateHitObjects[k].Value.Draw();
                    }
                    if (GetGameplayEngine().columns[i].updateHitObjects[k].Key - GetGameplayEngine().msIgnore > GetGameplayEngine().time)
                    {
                        skip = true;
                    }
                }
            }
        }

        private float getSkinnablePosition(string key)
        {
            return Skin.getConfigFloat("gameplay", "Positions", key);
        }

        public bool isActive()
        {
            return GameplayEngine.active;
        }

        public override void Destroy()
        {
            GetGameplayEngine().Reset();
        }
    }
}
