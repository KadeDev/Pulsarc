﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Pulsarc.Skinning;
using Pulsarc.UI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Pulsarc.Utils.Graphics
{
    class GraphicsUtils
    {
        public static Texture2D CreateFillTexture(int width, int height, Color paint)
        {
            Texture2D texture = new Texture2D(Pulsarc.graphics.GraphicsDevice, width, height);

            Color[] data = new Color[width * height];
            for (int pixel = 0; pixel < data.Count(); pixel++)
            {
                data[pixel] = paint;
            }

            texture.SetData(data);

            return texture;
        }

        public static Texture2D LoadFileTexture(string path)
        {
            Texture2D texture;
            try
            {
                texture = Texture2D.FromStream(Pulsarc.graphics.GraphicsDevice, File.Open(path, FileMode.Open));
            }
            catch
            {
                Console.WriteLine("Failed to load " + path);
                texture = new Texture2D(Pulsarc.graphics.GraphicsDevice, 1, 1);
            }

            return texture;
        }
    }
}
