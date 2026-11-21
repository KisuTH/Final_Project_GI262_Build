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
        public const int EarlyHintLevels = 5; // levels 1-5 are the early levels
        public const int EarlyHints = 5;      // Hint uses per level in the early levels
        public const int LateHints = 3;       // Hint uses per level from level 6 on

        // How many times Hint can be used in the given level number.
        public static int HintsForLevel(int levelNumber)
        {
            if (levelNumber <= EarlyHintLevels)
            {
                return EarlyHints;
            }
            return LateHints;
        }
    }
}
