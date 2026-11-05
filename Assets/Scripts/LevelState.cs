namespace ColorShiftKnight.Core
{
    public class LevelState
    {
        public int Knight;
        public int Relic;
        public int King;
        public bool[] Obstacles = new bool[BoardConfig.CellCount];

        public bool RelicCollected;
        public bool KingCaptured;
        public bool BoardShifted;

        public int Moves;
        public int MoveLimit;

        public bool HasAllObjectives()
        {
            return RelicCollected && KingCaptured;
        }

        public LevelState Clone()
        {
            LevelState copy = new LevelState();
            copy.Knight = Knight;
            copy.Relic = Relic;
            copy.King = King;
            for (int i = 0; i < BoardConfig.CellCount; i++)
            {
                copy.Obstacles[i] = Obstacles[i];
            }
            copy.RelicCollected = RelicCollected;
            copy.KingCaptured = KingCaptured;
            copy.BoardShifted = BoardShifted;
            copy.Moves = Moves;
            copy.MoveLimit = MoveLimit;
            return copy;
        }
    }
}
