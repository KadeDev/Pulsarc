﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Pulsarc.Utils;
using System;
using System.Collections.Generic;
using System.Text;

namespace Pulsarc.UI.Screens.Settings.UI
{
    public abstract class Setting : Drawable
    {
        public dynamic value;
        public string type;
        public string text;
        public TextDisplayElement title;
        public TextDisplayElement more;

        // Whether this input needs constant input (true) or single click (false)
        public bool hold = false;

        // Wheter this input is able to handle a Key event
        public bool keyListen = false;

        public Setting(string title, string more, Vector2 position, Texture2D texture, float aspect, Anchor anchor, dynamic baseValue, string type) : base(texture, aspect, anchor)
        {
            value = baseValue;
            this.type = type;
            Console.WriteLine(type.ToString() + " set for " + title);
            this.text = title;
            this.title = new TextDisplayElement(title, new Vector2(position.X - 50, position.Y), anchor: Anchor.CenterRight);
            changePosition(position);
        }
        public abstract void onClick(Point mousePosition);

        public override void Draw()
        {
            base.Draw();
            title.Draw();
        }

        public override void move(Vector2 position, bool truePositioning = false)
        {
            base.move(position, truePositioning);
            title.move(position, truePositioning);
        }

        public virtual void Save(string category, string key)
        {
            switch(type)
            {
                case "float":
                    Config.setFloat(category, key,(float) getSaveValue());
                    break;
                case "int":
                    Config.setInt(category, key,(int) getSaveValue());
                    break;
                case "double":
                    Config.setDouble(category, key,(double) getSaveValue());
                    break;
                case "bool":
                    Config.setBool(category, key,(bool) getSaveValue());
                    break;
                default:
                    Console.WriteLine("Cannot save type " + type.ToString() + " in category "+category+" for setting "+key);
                    break;
            }
        }

        public virtual void handleKeyEvent(Keys key) { }

        public abstract dynamic getSaveValue();
    }
}
