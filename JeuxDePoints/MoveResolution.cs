using System.Collections.Generic;
using System.Linq;

namespace JeuxDePoints {
    public class MoveResolution {
        public bool IsSuccessful { get; }
        public MoveFailureReason FailureReason { get; }
        public string Message { get; }
        public int[] ScoreBefore { get; }
        public int[] ScoreAfter { get; }
        public int AmmoBefore { get; }
        public int AmmoAfter { get; }
        public List<int> DestroyedPointIndices { get; }
        public List<LineState> AddedLines { get; }
        public List<LineState> RemovedLines { get; }

        public MoveResolution(
            bool isSuccessful,
            MoveFailureReason failureReason,
            string message,
            int[] scoreBefore,
            int[] scoreAfter,
            int ammoBefore,
            int ammoAfter,
            IEnumerable<int> destroyedPointIndices,
            IEnumerable<LineState> addedLines,
            IEnumerable<LineState> removedLines) {
            IsSuccessful = isSuccessful;
            FailureReason = failureReason;
            Message = message ?? string.Empty;
            ScoreBefore = (int[])scoreBefore.Clone();
            ScoreAfter = (int[])scoreAfter.Clone();
            AmmoBefore = ammoBefore;
            AmmoAfter = ammoAfter;
            DestroyedPointIndices = new List<int>(destroyedPointIndices ?? Enumerable.Empty<int>());
            AddedLines = (addedLines ?? Enumerable.Empty<LineState>())
                .Select(line => new LineState(line.StartRow, line.StartCol, line.EndRow, line.EndCol, line.PlayerId))
                .ToList();
            RemovedLines = (removedLines ?? Enumerable.Empty<LineState>())
                .Select(line => new LineState(line.StartRow, line.StartCol, line.EndRow, line.EndCol, line.PlayerId))
                .ToList();
        }
    }
}
