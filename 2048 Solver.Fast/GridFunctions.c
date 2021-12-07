#include <stdio.h>
#include <stdbool.h>

unsigned char Log2(unsigned int value)
{
    unsigned char i = 0;
    here:

    value >>= 1;
    if(value == 0)
    {
        return i;
    }
    i++;
    
    goto here;
}

bool HasEmptySquares(const unsigned char* grid)
{
    const unsigned char* const end = grid + 16;
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

int main()
{
    const unsigned char grid = 2;
    printf("%d", !grid);
    return 0;
}
