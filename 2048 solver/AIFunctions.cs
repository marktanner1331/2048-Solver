using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _2048_solver
{
    class AIFunctions
    {
        public static Direction getRandomDirection()
        {
            Random rand = new Random();
            switch (rand.Next(4))
            {
                case 0:
                    return Direction.left;
                case 1:
                    return Direction.right;
                case 2:
                    return Direction.up;
                case 3:
                    return Direction.down;
                default:
                    throw new Exception();
            }
        }


        public static unsafe int getScoreForGrid1(IntPtr gridPtr)
        {
            int score = 0;
            byte* grid = (byte*)gridPtr;

            for (int i = 0; i < 16; i++)
            {
                if (grid[i] == 0)
                {
                    score++;
                }
            }
            
            return score;
        }
    }
}
