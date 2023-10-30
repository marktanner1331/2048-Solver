#include <stdbool.h>
#include "GridFunctions.h"

extern unsigned char GridFunctions_Log2(unsigned int value)
{
    unsigned char i = 0;
here:

    value >>= 1;
    if (value == 0)
    {
        return i;
    }
    i++;

    goto here;
}

extern unsigned int GridFunctions_SquareSum(const unsigned char *grid)
{
    unsigned int sum = 0;
    const unsigned char *const end = grid + 16;

    while (grid != end)
    {
        if (*grid != 0)
        {
            sum += 1 << (*grid << 1);
        }

        grid++;
    }

    return sum;
}

extern unsigned int GridFunctions_SumValuesInGrid(const unsigned char *grid)
{
    unsigned int sum = 0;
    const unsigned char *const end = grid + 16;

    while (grid != end)
    {
        if (*grid != 0)
        {
            sum += (1 << *grid);
        }

        grid++;
    }

    return sum;
}

extern bool GridFunctions_HasEmptySquares(const unsigned char *grid)
{
    const unsigned char *const end = grid + 16;
    while (grid != end)
    {
        // maybe use if(!*grid)

        // or completely unroll it like
        //  if(!grid[0] || !grid[1] ...)
        //  that way we can do a 'const unsigned char* const'
        if (*grid == 0)
        {
            return true;
        }

        grid++;
    }

    return false;
}

extern unsigned int GridFunctions_CountEmptySquares(const unsigned char *grid)
{
    unsigned int numEmpty = 0;
    const unsigned char *const end = grid + 16;

    while (grid != end)
    {
        if (*grid == 0)
        {
            numEmpty++;
        }

        grid++;
    }

    return numEmpty;
}

extern void GridFunctions_CloneGrid(unsigned char *source, unsigned char *dest)
{
    for (unsigned char i = 0; i < 16; i++)
    {
        *dest = *source;
        dest++;
        source++;
    }

    //((unsigned long long *)dest)[0] = ((unsigned long *)source)[0];
    //((unsigned long long *)dest)[1] = ((unsigned long *)source)[1];
}

extern void GridFunctions_AddPermutation(unsigned char *grid, unsigned int *const startIndex)
{
    grid += *startIndex;
    while (*startIndex != 16)
    {
        if (*grid == 0)
        {
            *grid = 1;
            return;
        }

        grid++;
        (*startIndex)++;
    }
}

extern bool GridFunctions_GridsAreEqual(unsigned char *a, unsigned char *b)
{
    return ((unsigned long long *)a)[0] == ((unsigned long long *)b)[0] && ((unsigned long long *)a)[1] == ((unsigned long long *)b)[1];
}

extern bool GridFunctions_IndexOf(unsigned char *cache, int cacheCount, unsigned char *current, unsigned int *index)
{
    for (unsigned int i = 0; i < cacheCount; i++)
    {
        if (GridFunctions_GridsAreEqual(cache, current))
        {
            *index = i;
            return true;
        }

        cache += 16;
    }

    *index = 0;
    return false;
}

extern bool GridFunctions_collapseRow3(unsigned char *a, int delta)
{
    unsigned char *b = a + delta;
    unsigned char *c = b + delta;
    unsigned char *d = c + delta;

    if (*a == 0)
    {
        if (*b == 0)
        {
            if (*c == 0)
            {
                if (*d != 0)
                {
                    *a = *d;
                    *d = 0;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                *a = *c;
                *c = 0;

                if (*a == *d)
                {
                    (*a)++;
                    *d = 0;
                }
                else if (*d != 0)
                {
                    *b = *d;
                    *d = 0;
                }
            }
        }
        else
        {
            *a = *b;
            *b = 0;

            if (*c == 0)
            {
                if (*a == *d)
                {
                    (*a)++;
                    *d = 0;
                }
                else if (*d != 0)
                {
                    *b = *d;
                    *d = 0;
                }
            }
            else
            {
                if (*a == *c)
                {
                    (*a)++;
                    *c = 0;
                    if (*d != 0)
                    {
                        *b = *d;
                        *d = 0;
                    }
                }
                else
                {
                    *b = *c;
                    *c = 0;

                    if (*b == *d)
                    {
                        (*b)++;
                        *d = 0;
                    }
                    else if (*d != 0)
                    {
                        *c = *d;
                        *d = 0;
                    }
                }
            }
        }
    }
    else
    {
        if (*b == 0)
        {
            if (*c == 0)
            {
                if (*a == *d)
                {
                    (*a)++;
                    *d = 0;
                }
                else if (*d != 0)
                {
                    *b = *d;
                    *d = 0;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                if (*d == 0)
                {
                    if (*a == *c)
                    {
                        (*a)++;
                    }
                    else
                    {
                        *b = *c;
                    }

                    *c = 0;
                }
                else
                {
                    if (*a == *c)
                    {
                        (*a)++;
                        *c = 0;
                        *b = *d;
                    }
                    else
                    {
                        *b = *c;

                        if (*b == *d)
                        {
                            (*b)++;
                            *c = 0;
                        }
                        else
                        {
                            *c = *d;
                        }
                    }

                    *d = 0;
                }
            }
        }
        else
        {
            if (*c == 0)
            {
                if (*d == 0)
                {
                    if (*a == *b)
                    {
                        (*a)++;
                        *b = 0;
                    }
                    else
                    {
                        return false;
                    }
                }
                else
                {
                    if (*a == *b)
                    {
                        (*a)++;
                        *b = *d;
                    }
                    else if (*b == *d)
                    {
                        (*b)++;
                    }
                    else
                    {
                        *c = *d;
                    }

                    *d = 0;
                }
            }
            else
            {
                if (*d == 0)
                {
                    if (*a == *b)
                    {
                        (*a)++;
                        *b = *c;
                        *c = 0;
                    }
                    else if (*b == *c)
                    {
                        (*b)++;
                        *c = 0;
                    }
                    else
                    {
                        return false;
                    }
                }
                else
                {
                    if (*a == *b)
                    {
                        (*a)++;
                        *b = *c;
                        if (*b == *d)
                        {
                            (*b)++;
                            *c = 0;
                        }
                        else
                        {
                            *c = *d;
                        }

                        *d = 0;
                    }
                    else if (*b == *c)
                    {
                        (*b)++;
                        *c = *d;
                        *d = 0;
                    }
                    else if (*c == *d)
                    {
                        (*c)++;
                        *d = 0;
                    }
                    else
                    {
                        return false;
                    }
                }
            }
        }
    }

    return true;
}

extern bool GridFunctions_CollapseGridInPlace(unsigned char *grid, unsigned int direction)
{
    bool success = false;
    switch (direction)
    {
        case 0: //left
            success |= GridFunctions_collapseRow3(grid, 1);
            success |= GridFunctions_collapseRow3(grid + 4, 1);
            success |= GridFunctions_collapseRow3(grid + 8, 1);
            success |= GridFunctions_collapseRow3(grid + 12, 1);
            return success;
        case 1: //right
            success |= GridFunctions_collapseRow3(grid + 3, -1);
            success |= GridFunctions_collapseRow3(grid + 7, -1);
            success |= GridFunctions_collapseRow3(grid + 11, -1);
            success |= GridFunctions_collapseRow3(grid + 15, -1);
            return success;
        case 2: //up
            success |= GridFunctions_collapseRow3(grid, 4);
            success |= GridFunctions_collapseRow3(grid + 1, 4);
            success |= GridFunctions_collapseRow3(grid + 2, 4);
            success |= GridFunctions_collapseRow3(grid + 3, 4);
            return success;
        case 3: //down
            success |= GridFunctions_collapseRow3(grid + 12, -4);
            success |= GridFunctions_collapseRow3(grid + 13, -4);
            success |= GridFunctions_collapseRow3(grid + 14, -4);
            success |= GridFunctions_collapseRow3(grid + 15, -4);
            return success;
        default:
            return false;
    }
}