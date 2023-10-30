#include "GridFunctions.h"
#include <stdbool.h>
#include <stdlib.h>

extern unsigned int Solver7_ScoreForGrid(unsigned char *grid, unsigned char *current, unsigned char* cache, int depth);
unsigned int FinalScoreForGrid(unsigned char *grid);

extern unsigned int Solver7_ScoreForGrid(unsigned char *grid, unsigned char *current, unsigned char* cache, int depth)
{
    if (GridFunctions_HasEmptySquares(grid) == false)
    {
        return GridFunctions_SumValuesInGrid(grid);
    }

    unsigned int permutations = GridFunctions_CountEmptySquares(grid);

    unsigned int scores[16] = {0};
    current += 16;
    
    for (unsigned int subDirection = 0; subDirection < 4; subDirection++)
    {
        int startIndex = 0;

        int cacheCount = 0;
        unsigned int cachedScores[16] = {0};

        for (int i = 0; i < permutations; i++)
        {
            GridFunctions_CloneGrid(grid, current);
            GridFunctions_AddPermutation(current, &startIndex);

            if (GridFunctions_CollapseGridInPlace(current, subDirection) == false)
            {
                unsigned int finalScore = FinalScoreForGrid(current);
                if (finalScore > scores[i])
                {
                    scores[i] = finalScore;
                }

                startIndex++;
                continue;
            }

            unsigned int score;

            unsigned int index = 0;
            if (GridFunctions_IndexOf(cache, cacheCount, current, &index))
            {
                score = cachedScores[index];
            }
            else
            {
                if (depth == 1)
                {
                    score = FinalScoreForGrid(current);
                }
                else
                {
                    cache += cacheCount << 4;
                    score = Solver7_ScoreForGrid(current, current, cache, depth - 1);
                    cache -= cacheCount << 4;
                }

                GridFunctions_CloneGrid(current, cache + 16 * cacheCount);
                cachedScores[cacheCount] = score;
                cacheCount++;
            }

            if (score > scores[i])
            {
                scores[i] = score;
            }

            startIndex++;
        }
    }

    current -= 16;

    unsigned long total = 0;
    for (unsigned int i = 0; i < permutations; i++)
    {
        total += scores[i];
    }

    return total / permutations;
}

unsigned int FinalScoreForGrid(unsigned char *grid)
{
    return GridFunctions_SquareSum(grid);
}