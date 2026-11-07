namespace ColorShiftKnight.Core
{
    // Fixed numbers of the game. A square is a number 0..35 (row * 6 + column).
    // Row 0 is the top row and column 0 is the left column.
    public static class BoardConfig
    {
        public const int Size = 6;
        public const int CellCount = 36;
        public const int ObstacleCount = 8;
        public const int Start = 30; // A6 (bottom-left)
        public const int Goal = 5;   // F1 (top-right)
        public const int MaxUndos = 3; // Undo can be used this many times in one level
    }
}
