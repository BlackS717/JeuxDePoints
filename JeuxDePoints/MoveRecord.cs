using System;

namespace JeuxDePoints {
    public class MoveRecord {
        public Guid Id { get; }
        public int SequenceNumber { get; }
        public Move Intent { get; }
        public MoveResolution Resolution { get; }
        public GameStateSnapshot StateBefore { get; }
        public GameStateSnapshot StateAfter { get; }

        public MoveRecord(
            int sequenceNumber,
            Move intent,
            MoveResolution resolution,
            GameStateSnapshot stateBefore,
            GameStateSnapshot stateAfter) {
            Id = Guid.NewGuid();
            SequenceNumber = sequenceNumber;
            Intent = intent;
            Resolution = resolution;
            StateBefore = stateBefore;
            StateAfter = stateAfter;
        }

        public MoveRecord(
            int sequenceNumber,
            Move intent,
            MoveResolution resolution) {
            Id = Guid.NewGuid();
            SequenceNumber = sequenceNumber;
            Intent = intent;
            Resolution = resolution;
            StateBefore = null;
            StateAfter = null;
        }
    }
}
