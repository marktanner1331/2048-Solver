#include <stdbool.h>

extern unsigned char GridFunctions_Log2(unsigned int value);

extern unsigned int GridFunctions_SquareSum(const unsigned char *grid);

extern unsigned int GridFunctions_SumValuesInGrid(const unsigned char *grid);

extern bool GridFunctions_HasEmptySquares(const unsigned char *grid);

extern unsigned int GridFunctions_CountEmptySquares(const unsigned char *grid);

extern void GridFunctions_CloneGrid(unsigned char *source, unsigned char *dest);

extern void GridFunctions_AddPermutation(unsigned char *grid, unsigned int *const startIndex);

extern bool GridFunctions_GridsAreEqual(unsigned char *a, unsigned char *b);

extern bool GridFunctions_IndexOf(unsigned char *cache, int cacheCount, unsigned char *current, unsigned int *index);

extern bool GridFunctions_collapseRow3(unsigned char *a, int delta);

extern bool GridFunctions_CollapseGridInPlace(unsigned char *grid, unsigned int direction);