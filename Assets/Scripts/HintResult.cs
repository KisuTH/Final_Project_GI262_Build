namespace ColorShiftKnight.Core
{
    public class HintResult
    {
        public int NextSquare;
        public int RemainingMinMoves;

        public HintResult(int nextSquare, int remainingMinMoves)
        {
            NextSquare = nextSquare;
            RemainingMinMoves = remainingMinMoves;
        }
    }
}
