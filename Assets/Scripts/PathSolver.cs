using System.Collections.Generic;

namespace ColorShiftKnight.Core
{
    // Searching (BFS with a Queue): finds the fewest knight moves needed to reach Goal
    // after collecting the Relic and capturing the King.
    // A search state is: square * 4 + (relic done ? 2 : 0) + (king done ? 1 : 0)  -> 0..143
    public static class PathSolver
    {
        // Returns the squares to step on (not including the start square),
        // or null if the level cannot be won from here.
        public static int[] ShortestPath(int from, bool[] obstacles, int relic, int king, bool relicDone, bool kingDone)
        {
            int stateCount = BoardConfig.CellCount * 4;
            bool[] seen = new bool[stateCount];
            int[] parent = new int[stateCount];
            Queue<int> queue = new Queue<int>();

            int startState = Encode(from, relicDone, kingDone);
            seen[startState] = true;
            parent[startState] = -1;
            queue.Enqueue(startState);

            while (queue.Count > 0)
            {
                int current = queue.Dequeue();
                int cell = current / 4;
                int flags = current % 4;
                bool hasRelic = flags >= 2;
                bool hasKing = flags % 2 == 1;

                if (cell == BoardConfig.Goal && hasRelic && hasKing)
                {
                    return Rebuild(current, parent);
                }

                bool[] destinations = KnightRules.Destinations(cell, obstacles);
                for (int next = 0; next < BoardConfig.CellCount; next++)
                {
                    if (destinations[next] == false)
                    {
                        continue;
                    }

                    bool nextRelic = hasRelic;
                    bool nextKing = hasKing;
                    if (next == relic)
                    {
                        nextRelic = true;
                    }
                    if (next == king)
                    {
                        nextKing = true;
                    }

                    int nextState = Encode(next, nextRelic, nextKing);
                    if (seen[nextState] == false)
                    {
                        seen[nextState] = true;
                        parent[nextState] = current;
                        queue.Enqueue(nextState);
                    }
                }
            }
            return null;
        }

        private static int Encode(int cell, bool relic, bool king)
        {
            int flags = 0;
            if (relic)
            {
                flags = flags + 2;
            }
            if (king)
            {
                flags = flags + 1;
            }
            return cell * 4 + flags;
        }

        private static int[] Rebuild(int lastState, int[] parent)
        {
            int length = 0;
            int state = lastState;
            while (parent[state] != -1)
            {
                length++;
                state = parent[state];
            }

            int[] path = new int[length];
            state = lastState;
            for (int i = length - 1; i >= 0; i--)
            {
                path[i] = state / 4;
                state = parent[state];
            }
            return path;
        }
    }
}
