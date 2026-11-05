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

        // Name of a square, e.g. A1 (top-left) and F6 (bottom-right). Letters go left to right, numbers go top to bottom.
        public static string NameOf(int cell)
        {
            string letters = "ABCDEF";
            string letter = letters.Substring(ColOf(cell), 1);
            int number = RowOf(cell) + 1;
            return letter + number;
        }
    }
}
