using System.Collections.Generic;

namespace ColorShiftKnight.Core
{
    // A Stack of snapshots taken before each move. Undo takes the latest one back.
    public class GameHistory
    {
        private Stack<LevelState> snapshots = new Stack<LevelState>();

        public bool CanUndo()
        {
            return snapshots.Count > 0;
        }

        public void Push(LevelState snapshot)
        {
            snapshots.Push(snapshot);
        }

        public LevelState Pop()
        {
            return snapshots.Pop();
        }

        public void Clear()
        {
            snapshots.Clear();
        }
    }
}
