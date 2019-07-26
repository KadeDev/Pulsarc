﻿using Microsoft.Xna.Framework;
using System;

namespace Pulsarc.UI.Screens.Result.UI
{
    class Accuracy : TextDisplayElement
    {
        public Accuracy(Vector2 position, int fontSize = 20, bool centered = false) : base("", position, fontSize, centered)
        {
        }

        public void Update(double value)
        {
            Update(Math.Round(value * 100,2) + "%");
        }
    }
}
