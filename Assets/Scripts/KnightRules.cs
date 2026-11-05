namespace ColorShiftKnight.Core
{
    public static class KnightRules
    {
        // The eight L-shaped jumps (2 squares + 1 square).
        private static int[] rowJumps = { -2, -2, -1, -1, 1, 1, 2, 2 };
        private static int[] colJumps = { -1, 1, -2, 2, -2, 2, -1, 1 };

        // Returns an array of 36 bools: true = the knight at "from" can jump to that square.
        public static bool[] Destinations(int from, bool[] obstacles)
        {
            bool[] result = new bool[BoardConfig.CellCount];
            int row = Position.RowOf(from);
            int col = Position.ColOf(from);

            for (int i = 0; i < 8; i++)
            {
                int newRow = row + rowJumps[i];
                int newCol = col + colJumps[i];

                if (newRow >= 0 && newRow < BoardConfig.Size && newCol >= 0 && newCol < BoardConfig.Size)
                {
                    int cell = Position.Make(newRow, newCol);
                    if (obstacles[cell] == false)
                    {
                        result[cell] = true;
                    }
                }
            }
            return result;
        }
    }
}
