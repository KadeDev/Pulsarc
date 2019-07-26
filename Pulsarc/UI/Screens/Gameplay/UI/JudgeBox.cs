﻿using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Pulsarc.Skinning;

namespace Pulsarc.UI.Screens.Gameplay.UI
{
    class JudgeBox : Drawable
    {
        List<KeyValuePair<long, int>> toDraw;
        Dictionary<int, Judge> judges;

        public JudgeBox(Vector2 position) : base(Skin.defaultTexture)
        {
            texture = null;

            this.position = position;

            judges = new Dictionary<int, Judge>();
            foreach (JudgementValue judge in Judgement.judgements)
            {
                judges.Add(judge.score,new Judge(judge.score, new Vector2(position.X, position.Y)));
            }

            toDraw = new List<KeyValuePair<long, int>>();
        }

        public void Add(long time, int judgeKey)
        {
            toDraw.Add(new KeyValuePair<long, int>(time, judgeKey));
        }

        public void Update(long time)
        {
            int judgeDisplayTimeMs = 100;

            for(int i = 0; i < toDraw.Count; i++)
            {
                if(toDraw[i].Key + judgeDisplayTimeMs < time)
                {
                    toDraw.RemoveAt(i);
                    i--;
                }
            }
        }

        public override void Draw()
        {
            foreach(KeyValuePair<long,int> judge in toDraw)
            {
                judges[judge.Value].Draw();
            }
        }
    }
}
