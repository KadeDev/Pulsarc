using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Pulsarc.Beatmaps;
using Pulsarc.Utils;
using Pulsarc.UI.Screens.Result;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using Wobble.Screens;

namespace Pulsarc.UI.Screens.Gameplay
{
    public class GameplayEngine : Screen
    {
        public override ScreenView View { get ; protected set; }
        private GameplayEngineView getGameplayView() { return (GameplayEngineView)View; }

        /// <summary>Whether or not the gameplay engine is currently running</summary>
        public static bool active = false;

        /// <summary>temp: Whether or not the gameplay is automatically run</summary>
        bool autoPlay = false;
        /// <summary>Whether or not autoplay should use randomness.</summary>
        bool autoPlayRandom = false;

        // Used for delaying the gameplay's end
        Stopwatch endWatch;
        public int endDelay = 2000;

        // Beatmap Elements
        /// <summary>The current beatmap being played.</summary>
        public Beatmap currentBeatmap;
        public Column[] columns;
        /// <summary>Used to determine the key-style of the current map (4k, 7k, etc.)</summary>
        public int keys;

        // Events indexes
        public int speedVariationIndex;
        public int eventIndex;

        // Gameplay Elements
        public double timeOffset;

        public Crosshair crosshair;
        /// <summary>User-defined base speed</summary>
        public double userSpeed;
        /// <summary>Current speed modifier defined by the Beatmap</summary>
        public double currentSpeedMultiplier;
        public double currentArcsSpeed;
        public List<KeyValuePair<double, int>> errors;
        public List<JudgementValue> judgements;
        public Dictionary<Keys, int> bindings;

        public long max_score;
        public long score;
        public int score_display;
        /// <summary>The current combo during gameplay.</summary>
        public int combo;
        /// <summary>The highest combo obtained during gameplay thus far.</summary> 
        public int max_combo;
        /// <summary>Hidden value to determine score.</summary>
        public int combo_multiplier;
        /// <summary>How fast the audio (and relevant gameplay) plays at.</summary>
        public float rate;

        public double time => AudioManager.getTime() + timeOffset;

        // Performance
        /// <summary>Time distance from which hitobjects are neither updated not drawn</summary>
        public int msIgnore = 500;

        /// <summary>
        /// The engine that handles the gameplay of Pulsarc.
        /// </summary>
        public GameplayEngine()
        {
            View = new GameplayEngineView(this);
        }

        /// <summary>
        /// Initializes the current GameplayView with the provided beatmap.
        /// </summary>
        /// <param name="beatmap">The beatmap to play through</param>
        public void Init(Beatmap beatmap)
        {
            // Reset in case it wasn't properly handled outside
            Reset();

            // Set the offset for each play before starting audio
            // TODO: add local beatmap offset
            AudioManager.offset = Config.getInt("Audio", "GlobalOffset");

            // temp: These values should be obtained from mods/config/beatmap parsing
            rate = 1f; 
            keys = 4;
            userSpeed = Config.getInt("Gameplay", "ApproachSpeed") / 5f / rate; // "5f" is used to give more choice in config for speed

            crosshair = new Crosshair(300); // 300 = base crosshair diameter in intralism
            timeOffset = 0;

            // Initialize default variables, parse beatmap
            endWatch = new Stopwatch();
            AudioManager.audioRate = rate;

            currentSpeedMultiplier = userSpeed;
            currentArcsSpeed = 1;

            speedVariationIndex = 0;
            eventIndex = 0;

            // Initialize Gameplay variables
            columns = new Column[keys];
            judgements = new List<JudgementValue>();
            errors = new List<KeyValuePair<double, int>>();
            bindings = new Dictionary<Keys, int>();

            combo = 0;
            max_combo = 0;
            combo_multiplier = Scoring.max_combo_multiplier;
            score = 0;

            currentBeatmap = beatmap;

            // Set the path of the song to be played later on
            AudioManager.song_path = Directory.GetParent(currentBeatmap.path).FullName + "\\" + currentBeatmap.Audio;

            // Create columns and their hitobjects
            for (int i = 1; i <= keys; i++)
            {
                columns[i - 1] = new Column(i);
            }

            int objectCount = 0;
            int speedVarInitIndex = 0;
            
            foreach (Arc arc in currentBeatmap.arcs)
            {
                // Go through events to update current arcs speed
                while(currentBeatmap.speedVariations.Count > speedVarInitIndex && currentBeatmap.speedVariations[speedVarInitIndex].time <= arc.time)
                {
                    switch (currentBeatmap.speedVariations[speedVarInitIndex].type)
                    {
                        case 1:
                            // Arcs spawn speed change
                            //currentArcsSpeed = currentBeatmap.speedVariations[speedVarInitIndex].intensity;
                            break;
                    }
                    speedVarInitIndex++;
                }

                // Add arcs to the columns
                for (int i = 0; i < keys; i++)
                {
                    if (BeatmapHelper.isColumn(arc, i))
                    {
                        columns[i].AddHitObject(new HitObject(arc.time, (int)(i / (float)keys * 360), keys, currentArcsSpeed), currentArcsSpeed * currentSpeedMultiplier, crosshair.getZLocation());
                        objectCount++;
                    }
                }
            }

            // Compute the beatmap's highest possible score, for displaying the current display_score later on
            max_score = Scoring.getMaxScore(objectCount);

            // Sort the hitobjects according to their first appearance for optimizing update/draw
            foreach (Column col in columns)
            {
                col.SortUpdateHitObjects();
            }

            // Load user bindings
            bindings.Add(Config.bindings["Left"], 2);
            bindings.Add(Config.bindings["Up"], 3);
            bindings.Add(Config.bindings["Down"], 1);
            bindings.Add(Config.bindings["Right"], 0);

            // Load autoplay by filling the input queue with desired inputs
            if (autoPlay)
            {
                Keys[] presses =
                {
                    Config.bindings["Right"],
                    Config.bindings["Down"],
                    Config.bindings["Left"],
                    Config.bindings["Up"],
                };

                List<KeyValuePair<double, Keys>> inputs = new List<KeyValuePair<double, Keys>>();

                for (int i = 0; i < keys; i++)
                {
                    foreach (HitObject arc in columns[i].hitObjects)
                    {
                        if (autoPlayRandom)
                        {
                            inputs.Add(new KeyValuePair<double, Keys>(arc.time + Math.Pow(new Random().Next(80) - 40, 3) / 1300, presses[i]));
                        }
                        else
                        {
                            inputs.Add(new KeyValuePair<double, Keys>(arc.time, presses[i]));
                        }
                    }
                }

                inputs.Sort((x, y) => x.Key.CompareTo(y.Key));

                foreach (KeyValuePair<double, Keys> input in inputs)
                {
                    InputManager.keyboardPresses.Enqueue(input);
                }
            }

            // Once everything is loaded, initialize the view
            getGameplayView().Init();

            // Start audio and gameplay
            AudioManager.Start();
            GameplayEngine.active = true;
            Pulsarc.display_cursor = false;

            // Collect any excess memory to prevent GC from starting soon, avoiding freezes.
            // TODO: disable GC while in gameplay
            GC.Collect();
        }

        /// <summary>
        /// Initialize this gameplay view by using the folder location and difficulty name to find the beatmap. Legacy.
        /// </summary>
        /// <param name="folder">Beatmap folder name.</param>
        /// <param name="diff">Difficulty name for the beatmap.</param>
        public void Init(string folder, string diff)
        {
            // Legacy
            Init(BeatmapHelper.Load("Songs/" + folder + "/" + diff + ".psc"));
        }

        public override void Update(GameTime gameTime)
        {
            // If not active, don't update.
            if (!active) return;

            // Quit gameplay when nothing is left to play in terms of Audio.
            // Could be improved to respect an EndDelay timer.
            if (AudioManager.active && AudioManager.FinishedPlaying())
            {
                EndGameplay();
                return;
            }

            // Handle user input in priority
            handleInputs();

            // Gameplay commands
            // End the gameplay with the "escape" key TODO: make this key bindable.
            if (Keyboard.GetState().IsKeyDown(Keys.Escape))
            {
                EndGameplay();
                return;
            }

            // Pause Gameplay with bindable "Pause" key.
            if (Keyboard.GetState().IsKeyDown(Config.bindings["Pause"]))
            {
                Pause();
            }
            
            // Resume Gameplay with bindable "Continue" key.
            if (Keyboard.GetState().IsKeyDown(Config.bindings["Continue"]))
            {
                Resume();
            }
            
            // Restart gameplay using bindable "Retry" key.
            if (Keyboard.GetState().IsKeyDown(Config.bindings["Retry"]))
            {
                Retry();
                return;
            }

            // Keep track of whether or not any object is left to play
            bool atLeastOne = false;

            // Handle all events
            // Speed variations
            if (currentBeatmap.speedVariations.Count > speedVariationIndex && currentBeatmap.speedVariations[speedVariationIndex].time <= time)
            {
                switch (currentBeatmap.speedVariations[speedVariationIndex].type)
                {
                    case 0:
                        // Global speed change
                        //currentSpeedMultiplier = userSpeed * (1/currentBeatmap.speedVariations[speedVariationIndex].intensity);
                        break;
                }
                speedVariationIndex++;
            }

            // Events
            if (currentBeatmap.events.Count > eventIndex + 1 && currentBeatmap.events[eventIndex].time <= time)
            {
                currentBeatmap.events[eventIndex].Handle(this);
                eventIndex++;
            }

            // Update UI and objects positions
            for (int i = 0; i < keys; i++)
            {
                bool updatedAll = false;
                for (int k = 0; k < columns[i].updateHitObjects.Count && !updatedAll; k++)
                {
                    // Remove the hitobject if it is marked for removal before updating it
                    if (columns[i].updateHitObjects[k].Value.erase)
                    {
                        columns[i].updateHitObjects.RemoveAt(k);
                        continue;
                    }

                    // Process the new position of this object
                    columns[i].updateHitObjects[k].Value.recalcPos((int)time, currentSpeedMultiplier, crosshair.getZLocation());
                    atLeastOne = true;

                    // Ignore the following objects if we have reached the ignored distance
                    if (columns[i].updateHitObjects[k].Key - msIgnore > time)
                    {
                        updatedAll = true;
                    }

                    // Determine whether or not this note has been missed by the user, and take action if so
                    if (columns[i].updateHitObjects[k].Value.time + Judgement.getMiss().judge * rate < time)
                    {
                        // Remove the hitobject and reset the combo
                        columns[i].hitObjects.Remove(columns[i].updateHitObjects[k].Value);
                        columns[i].updateHitObjects.RemoveAt(k);
                        k--;
                        combo = 0;

                        // Add a miss to the score and obtained judgements, then display it
                        JudgementValue miss = Judgement.getMiss();

                        KeyValuePair<long, int> hitResult = Scoring.processHitResults(miss, score, combo_multiplier);
                        score = hitResult.Key;
                        combo_multiplier = hitResult.Value;
                        getGameplayView().addJudge(time, miss.score);
                        judgements.Add(miss);
                    }
                }
            }

            // Reprocess the displayed score
            updateScoreDisplay();

            // Update other display elements
            View.Update(gameTime);

            // End gameplay with a delay if needed
            if (!atLeastOne)
            {
                if (!endWatch.IsRunning)
                {
                    endWatch.Start();
                }
                else
                {
                    if (endWatch.ElapsedMilliseconds >= endDelay)
                    {
                        EndGameplay();
                    }
                }

            }
        }

        /// <summary>
        /// Update score_display according to the maximum displayed score.
        /// </summary>
        private void updateScoreDisplay()
        {
            score_display = (int) (score / (float) max_score * Scoring.max_score);
        }

        /// <summary>
        /// Move the gameplay backwards or forward in time.
        /// </summary>
        /// <param name="delta">How much time to move.</param>
        public void deltaTime(long delta)
        {
            AudioManager.deltaTime(delta);
        }

        /// <summary>
        /// Pause the gameplay.
        /// </summary>
        public void Pause()
        {
            AudioManager.Pause();
        }

        /// <summary>
        /// Resume gameplay.
        /// </summary>
        public void Resume()
        {
            AudioManager.Resume();
        }

        /// <summary>
        /// Restart gameplay
        /// </summary>
        public void Retry()
        {
            Beatmap retry = currentBeatmap;

            Reset();

            Init(retry);
        }

        /// <summary>
        /// Reset this GameplayEngine, use before retrying or changing to a new map.
        /// </summary>
        public void Reset()
        {
            active = false;

            // Clear Input and Audio
            InputManager.keyboardPresses.Clear();
            AudioManager.Stop();

            // Unset attributes to avoid potential conflict with next gameplays
            currentBeatmap = null;
            columns = null;

            // Reset Attributes
            userSpeed = 1;
            currentSpeedMultiplier = 1;
            currentArcsSpeed = 1;
        }

        /// <summary>
        /// 
        /// </summary>
        public void handleInputs()
        {
            while (InputManager.keyboardPresses.Count > 0 
                && InputManager.keyboardPresses.Peek().Key <= AudioManager.getTime()) // Prevents future input from being handled. Useful for auto. Remove for quick auto result testing
            {
                KeyValuePair<double, Keys> press = InputManager.keyboardPresses.Dequeue();

                // Process a hit if the pressed key corresponds to a bound key
                if(bindings.ContainsKey(press.Value)) { 
                    HitObject pressed = null;
                    var column = bindings[press.Value];

                    // Check the first hitobject of the corresponding column if there is >= one
                    if (columns[column].hitObjects.Count > 0)
                    {
                        pressed = columns[column].hitObjects[0];

                        int error = (int)((pressed.time - press.Key) / rate);

                        // Get the judge for the timing error
                        JudgementValue judge = Judgement.getErrorJudgementValue(Math.Abs(error));

                        // If no judge is obtained, it is a ghost hit and is ignored
                        if (judge != null)
                        {
                            // Otherwise, handle the hit according to the judge

                            getGameplayView().addHit(press.Key, error, judge.score);

                            columns[column].hitObjects[0].erase = true;
                            columns[column].hitObjects.RemoveAt(0);
                            errors.Add(new KeyValuePair<double, int>(press.Key, error));
                            judgements.Add(judge);

                            KeyValuePair<long, int> hitResult = Scoring.processHitResults(judge, score, combo_multiplier);
                            score = hitResult.Key;
                            combo_multiplier = hitResult.Value;

                            if (judge.score > 0)
                            {
                                combo++;
                                if(combo > max_combo)
                                {
                                    max_combo = combo;
                                }
                            }
                            else
                            {
                                combo = 0;
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Stop gameplay and remove this engine from displaying.
        /// </summary>
        public void EndGameplay()
        {
            // Create the result screen before exiting gameplay
            ResultScreen next = new ResultScreen(judgements, errors, score_display, max_combo, currentBeatmap);
            Pulsarc.display_cursor = true;
            Reset();
            // Switch to results screen
            ScreenManager.RemoveScreen(true);
            ScreenManager.AddScreen(next);

            // TODO: restart GC when out of gameplay
            GC.Collect();
        }
    }
}
