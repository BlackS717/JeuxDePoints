namespace JeuxDePoints {
    public class ReplayCheckpoint {
        public int AppliedMoveCount { get; }
        public GameStateSnapshot Snapshot { get; }

        public ReplayCheckpoint(int appliedMoveCount, GameStateSnapshot snapshot) {
            AppliedMoveCount = appliedMoveCount;
            Snapshot = snapshot;
        }
    }
}
