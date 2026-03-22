using System;

namespace JeuxDePoints {
    public class Move {
        public int PlayerId { get; }
        public int? PointIndex { get; }
        public ActionType ActionType { get; }
        public int? TargetIndex { get; }
        public bool IsSuccessful { get; }
        public int? Row { get; }
        public int? Col { get; }
        public int? Power { get; }
        public DateTime TimestampUtc { get; }
        public string Notes { get; }

        public Move(int playerId, ActionType actionType, int? pointIndex = null, int? targetIndex = null, bool isSuccessful = false) {
            PlayerId = playerId;
            ActionType = actionType;
            PointIndex = pointIndex;
            TargetIndex = targetIndex;
            IsSuccessful = isSuccessful;
            Row = null;
            Col = null;
            Power = null;
            TimestampUtc = DateTime.UtcNow;
            Notes = string.Empty;
        }

        public Move(
            int playerId,
            ActionType actionType,
            int? pointIndex,
            int? targetIndex,
            bool isSuccessful,
            int? row,
            int? col,
            int? power,
            DateTime timestampUtc,
            string notes = "") {
            PlayerId = playerId;
            ActionType = actionType;
            PointIndex = pointIndex;
            TargetIndex = targetIndex;
            IsSuccessful = isSuccessful;
            Row = row;
            Col = col;
            Power = power;
            TimestampUtc = timestampUtc;
            Notes = notes ?? string.Empty;
        }

        public bool IsPointPlacement() => ActionType == ActionType.PlacePoint;
        public bool IsCannonShot() => ActionType == ActionType.ShootCannon;

    }
}
