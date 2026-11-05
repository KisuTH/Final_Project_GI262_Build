namespace ColorShiftKnight.Core
{
    // Helper methods for square numbers (0..35).
    public static class Position
    {
        public static int RowOf(int cell)
        {
            return cell / BoardConfig.Size;
        }

        public static int ColOf(int cell)
        {
            return cell % BoardConfig.Size;
        }

        public static int Make(int row, int col)
        {
            return row * BoardConfig.Size + col;
        }

        // Chess-style name, e.g. A1 (bottom-left) and F6 (top-right).
        public static string NameOf(int cell)
        {
            string letters = "ABCDEF";
            string letter = letters.Substring(ColOf(cell), 1);
            int number = BoardConfig.Size - RowOf(cell);
            return letter + number;
        }
    }
}
