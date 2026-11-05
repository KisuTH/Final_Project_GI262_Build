namespace ColorShiftKnight.Core
{
    // Result codes of TryMove.
    public static class MoveResult
    {
        public const int Invalid = 0;        // not a legal knight move, or the game is over
        public const int Moved = 1;
        public const int RelicCollected = 2;
        public const int KingCaptured = 3;
        public const int GoalLocked = 4;     // stood on Goal without both objectives
        public const int Won = 5;
        public const int Lost = 6;           // move limit reached
        public const int Rejected = 7;       // no solvable layout after the Relic; move not applied
    }
}
