using System.Collections.Generic;
using System.Linq;

namespace JeuxDePoints {
    public class GameStateSnapshot {
        public int Rows { get; }
        public int Cols { get; }
        public int[] Points { get; }
        public List<LineState> Lines { get; }
        public Dictionary<int, List<LineState>> PointLines { get; }
        public List<CannonStateSnapshot> Cannons { get; }
        public int CurrentPlayerId { get; }
        public int CurrentTurn { get; }
        public bool IsGameOver { get; }
        public int[] PlayerScores { get; }

        public GameStateSnapshot(
            int rows,
            int cols,
            int[] points,
            IEnumerable<LineState> lines,
            Dictionary<int, List<LineState>> pointLines,
            IEnumerable<CannonStateSnapshot> cannons,
            int currentPlayerId,
            int currentTurn,
            bool isGameOver,
            int[] playerScores) {
            Rows = rows;
            Cols = cols;
            Points = (int[])points.Clone();
            Lines = lines.Select(CloneLineState).ToList();
            PointLines = ClonePointLines(pointLines);
            Cannons = cannons.Select(CloneCannonState).ToList();
            CurrentPlayerId = currentPlayerId;
            CurrentTurn = currentTurn;
            IsGameOver = isGameOver;
            PlayerScores = (int[])playerScores.Clone();
        }

        public GameStateSnapshot Clone() {
            return new GameStateSnapshot(
                Rows,
                Cols,
                Points,
                Lines,
                PointLines,
                Cannons,
                CurrentPlayerId,
                CurrentTurn,
                IsGameOver,
                PlayerScores
            );
        }

        private static Dictionary<int, List<LineState>> ClonePointLines(Dictionary<int, List<LineState>> source) {
            Dictionary<int, List<LineState>> clone = new Dictionary<int, List<LineState>>();
            foreach (KeyValuePair<int, List<LineState>> entry in source) {
                clone[entry.Key] = entry.Value.Select(CloneLineState).ToList();
            }
            return clone;
        }

        private static LineState CloneLineState(LineState line) {
            return new LineState(line.StartRow, line.StartCol, line.EndRow, line.EndCol, line.PlayerId);
        }

        private static CannonStateSnapshot CloneCannonState(CannonStateSnapshot cannon) {
            return new CannonStateSnapshot(cannon.PlayerId, cannon.YPosition, cannon.CurrentAmmo, cannon.MaxAmmo);
        }
    }
}
