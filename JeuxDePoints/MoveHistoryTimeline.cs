using System;
using System.Collections.Generic;
using System.Linq;

namespace JeuxDePoints {
    public class MoveHistoryTimeline {
        private readonly List<MoveRecord> records;
        private readonly List<ReplayCheckpoint> checkpoints;
        private GameStateSnapshot initialSnapshot;
        private readonly int checkpointInterval;
        private int cursor;

        public MoveHistoryTimeline(GameStateSnapshot initialSnapshot, int checkpointInterval = 25) {
            this.initialSnapshot = initialSnapshot ?? throw new ArgumentNullException(nameof(initialSnapshot));
            if (checkpointInterval <= 0) {
                throw new ArgumentOutOfRangeException(nameof(checkpointInterval));
            }

            this.checkpointInterval = checkpointInterval;
            records = new List<MoveRecord>();
            checkpoints = new List<ReplayCheckpoint>();
            cursor = 0;
        }

        public IReadOnlyList<MoveRecord> Records {
            get { return records.AsReadOnly(); }
        }

        public int AppliedMoveCount {
            get { return cursor; }
        }

        public int CheckpointInterval {
            get { return checkpointInterval; }
        }

        public IReadOnlyList<ReplayCheckpoint> Checkpoints {
            get { return checkpoints.AsReadOnly(); }
        }

        public bool CanUndo {
            get { return cursor > 0; }
        }

        public bool CanRedo {
            get { return cursor < records.Count; }
        }

        public void Reset(GameStateSnapshot snapshot) {
            initialSnapshot = snapshot ?? throw new ArgumentNullException(nameof(snapshot));
            records.Clear();
            checkpoints.Clear();
            cursor = 0;
        }

        public void Append(MoveRecord record, GameStateSnapshot stateAfterRecord) {
            if (record == null) {
                throw new ArgumentNullException(nameof(record));
            }
            if (stateAfterRecord == null) {
                throw new ArgumentNullException(nameof(stateAfterRecord));
            }

            if (cursor < records.Count) {
                records.RemoveRange(cursor, records.Count - cursor);
                checkpoints.RemoveAll(cp => cp.AppliedMoveCount > cursor);
            }

            records.Add(record);
            cursor = records.Count;

            if (cursor % checkpointInterval == 0) {
                checkpoints.Add(new ReplayCheckpoint(cursor, stateAfterRecord.Clone()));
            }
        }

        public GameStateSnapshot GetCurrentSnapshot(Func<GameStateSnapshot, MoveRecord, GameStateSnapshot> reducer) {
            return ReplayTo(cursor, reducer);
        }

        public GameStateSnapshot Undo(Func<GameStateSnapshot, MoveRecord, GameStateSnapshot> reducer) {
            if (!CanUndo) {
                throw new InvalidOperationException("No move available to undo.");
            }

            return ReplayTo(cursor - 1, reducer);
        }

        public GameStateSnapshot Redo(Func<GameStateSnapshot, MoveRecord, GameStateSnapshot> reducer) {
            if (!CanRedo) {
                throw new InvalidOperationException("No move available to redo.");
            }

            return ReplayTo(cursor + 1, reducer);
        }

        public GameStateSnapshot ReplayTo(int appliedMoveCount, Func<GameStateSnapshot, MoveRecord, GameStateSnapshot> reducer) {
            if (appliedMoveCount < 0 || appliedMoveCount > records.Count) {
                throw new ArgumentOutOfRangeException(nameof(appliedMoveCount));
            }
            if (reducer == null) {
                throw new ArgumentNullException(nameof(reducer));
            }

            int replayStart = 0;
            GameStateSnapshot state = initialSnapshot.Clone();

            ReplayCheckpoint nearestCheckpoint = checkpoints
                .Where(cp => cp.AppliedMoveCount <= appliedMoveCount)
                .OrderByDescending(cp => cp.AppliedMoveCount)
                .FirstOrDefault();

            if (nearestCheckpoint != null) {
                replayStart = nearestCheckpoint.AppliedMoveCount;
                state = nearestCheckpoint.Snapshot.Clone();
            }

            for (int i = replayStart; i < appliedMoveCount; i++) {
                state = reducer(state, records[i]);
            }

            cursor = appliedMoveCount;
            return state;
        }

        public IReadOnlyList<MoveRecord> GetAppliedMoves() {
            return records.Take(cursor).ToList().AsReadOnly();
        }
    }
}
