﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pulsarc.Gameplay
{
    static class Judgement
    {
        static public List<JudgementValue> judgements = new List<JudgementValue>()
        {
            //////////////// Judge equal to Stepmania J4 /////////////
            new JudgementValue(1,       22,     320),       // MAX
            new JudgementValue(1,       45,     300),       // Perfect
            new JudgementValue(2/3,     90,     200),       // Great
            new JudgementValue(1/3,     135,    100),      // Good
            new JudgementValue(1/6,     180,    50),        // Bad
            new JudgementValue(0,       200,    0),         // Miss
        };
    

        static public KeyValuePair<double, int> getErrorJudgement(int error)
        {
            KeyValuePair<double, int> result = new KeyValuePair<double, int>(-1, -1);
            if (error < judgements.Last().judge)
            {
                bool notJudged = true;

                for (int i = 0; i < judgements.Count && notJudged; i++)
                {
                    JudgementValue judgement = judgements[i];

                    if (error < judgement.judge)
                    {
                        result = new KeyValuePair<double, int>(judgement.acc, judgement.score);
                        notJudged = false;
                    }
                }

                if(notJudged)
                {
                    result = new KeyValuePair<double, int>(0, 0);
                }
            }

            return result;
        }

        static public JudgementValue getMiss()
        {
            return judgements.Last();
        }
    }

    class JudgementValue {

        public double acc;
        public int judge;
        public int score;

        public JudgementValue(double acc, int judge, int score)
        {
            this.acc = acc;
            this.judge = judge;
            this.score = score;
        }
    }
}
