namespace ColorShiftKnight.Core
{
    public static class HintSystem
    {
        // Returns null when there is nothing to suggest.
        public static HintResult GetHint(LevelState state)
        {
            int[] path = PathSolver.ShortestPath(state.Knight, state.Obstacles, state.Relic, state.King,
                state.RelicCollected, state.KingCaptured);

            if (path == null || path.Length == 0)
            {
                return null;
            }
            return new HintResult(path[0], path.Length);
        }
    }
}
