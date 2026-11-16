using System;

namespace ColorShiftKnight.Core
{
    // Game logic only (no Unity code).
    public class GameCore
    {
        private Random rng;
        private GameHistory history = new GameHistory();

        public LevelState State;
        public bool IsWon;
        public bool IsLost;
        public int UndosLeft;
        private LevelState initial;

        public GameCore(int seed)
        {
            rng = new Random(seed);
        }

        public bool IsOver()
        {
            return IsWon || IsLost;
        }

        public bool StartNewLevel()
        {
            LevelState state = LevelGenerator.CreateInitial(rng);
            if (state == null)
            {
                return false;
            }

            State = state;
            initial = state.Clone();
            history.Clear();
            IsWon = false;
            IsLost = false;
            UndosLeft = BoardConfig.MaxUndos;
            return true;
        }

        // 36 bools: true = the knight may jump to that square now.
        public bool[] LegalMoves()
        {
            if (State == null || IsOver())
            {
                return new bool[BoardConfig.CellCount];
            }
            return KnightRules.Destinations(State.Knight, State.Obstacles);
        }

        public int TryMove(int target)
        {
            if (State == null || IsOver())
            {
                return MoveResult.Invalid;
            }
            if (target < 0 || target >= BoardConfig.CellCount || LegalMoves()[target] == false)
            {
                return MoveResult.Invalid;
            }

            LevelState next = State.Clone();
            next.Knight = target;
            next.Moves++;

            int result = MoveResult.Moved;

            if (next.RelicCollected == false && target == next.Relic)
            {
                next.RelicCollected = true;
                next.BoardShifted = !next.BoardShifted;
                if (LevelGenerator.ReshuffleAfterRelic(next, rng) == false)
                {
                    return MoveResult.Rejected; // "next" is thrown away, State stays the same
                }
                result = MoveResult.RelicCollected;
            }
            else if (next.KingCaptured == false && target == next.King)
            {
                next.KingCaptured = true;
                result = MoveResult.KingCaptured;
            }

            history.Push(State.Clone());
            State = next;

            if (next.Knight == BoardConfig.Goal && next.HasAllObjectives())
            {
                IsWon = true;
                return MoveResult.Won;
            }

            if (next.Moves >= next.MoveLimit)
            {
                IsLost = true;
                return MoveResult.Lost;
            }

            if (next.Knight == BoardConfig.Goal)
            {
                return MoveResult.GoalLocked;
            }
            return result;
        }

        // Allowed after a loss, not after a win. Each level allows only BoardConfig.MaxUndos undos.
        public bool Undo()
        {
            if (State == null || IsWon || UndosLeft <= 0 || history.CanUndo() == false)
            {
                return false;
            }

            State = history.Pop();
            IsLost = false;
            UndosLeft--;
            return true;
        }

        // Puts the current level back to its starting position (same board, same Move Limit).
        public void Restart()
        {
            if (initial == null)
            {
                return;
            }
            State = initial.Clone();
            history.Clear();
            IsWon = false;
            IsLost = false;
            UndosLeft = BoardConfig.MaxUndos;
        }

        // Returns null when there is no hint.
        public HintResult GetHint()
        {
            if (State == null || IsOver())
            {
                return null;
            }
            return HintSystem.GetHint(State);
        }
    }
}
