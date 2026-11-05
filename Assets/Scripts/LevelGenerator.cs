using System;

namespace ColorShiftKnight.Core
{
    public static class LevelGenerator
    {
        public const int MaxGenerateAttempts = 2000;
        public const int MaxReshuffleAttempts = 5000;

        // Builds a new level that can be won (checked with PathSolver). Returns null if it fails.
        public static LevelState CreateInitial(Random rng)
        {
            for (int attempt = 0; attempt < MaxGenerateAttempts; attempt++)
            {
                // Every square except Start and Goal.
                int[] cells = new int[BoardConfig.CellCount - 2];
                int count = 0;
                for (int cell = 0; cell < BoardConfig.CellCount; cell++)
                {
                    if (cell != BoardConfig.Start && cell != BoardConfig.Goal)
                    {
                        cells[count] = cell;
                        count++;
                    }
                }
                Shuffle(cells, count, rng);

                LevelState state = new LevelState();
                state.Knight = BoardConfig.Start;
                state.Moves = 0;
                for (int i = 0; i < BoardConfig.ObstacleCount; i++)
                {
                    state.Obstacles[cells[i]] = true;
                }
                state.Relic = cells[BoardConfig.ObstacleCount];
                state.King = cells[BoardConfig.ObstacleCount + 1];

                int[] path = PathSolver.ShortestPath(state.Knight, state.Obstacles, state.Relic, state.King, false, false);
                if (path != null)
                {
                    state.MoveLimit = path.Length + 1;
                    return state;
                }
            }
            return null;
        }

        // Called right after the knight lands on the Relic. Random obstacles are tried again
        // and again until a winning path exists. Returns false (and changes nothing) if none is found.
        public static bool ReshuffleAfterRelic(LevelState state, Random rng)
        {
            int[] pool = new int[BoardConfig.CellCount];
            int count = 0;
            for (int cell = 0; cell < BoardConfig.CellCount; cell++)
            {
                if (cell == BoardConfig.Start || cell == BoardConfig.Goal || cell == state.Knight || cell == state.King)
                {
                    continue;
                }
                pool[count] = cell;
                count++;
            }

            for (int attempt = 0; attempt < MaxReshuffleAttempts; attempt++)
            {
                Shuffle(pool, count, rng);
                bool[] obstacles = new bool[BoardConfig.CellCount];
                for (int i = 0; i < BoardConfig.ObstacleCount; i++)
                {
                    obstacles[pool[i]] = true;
                }

                int[] path = PathSolver.ShortestPath(state.Knight, obstacles, state.Relic, state.King, true, state.KingCaptured);
                if (path != null)
                {
                    state.Obstacles = obstacles;
                    state.MoveLimit = state.Moves + path.Length + 1;
                    return true;
                }
            }
            return false;
        }

        // Shuffles the first "count" items of the array.
        private static void Shuffle(int[] items, int count, Random rng)
        {
            for (int i = count - 1; i > 0; i--)
            {
                int j = rng.Next(i + 1);
                int temp = items[i];
                items[i] = items[j];
                items[j] = temp;
            }
        }
    }
}
