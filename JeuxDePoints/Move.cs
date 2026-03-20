namespace JeuxDePoints {
    public class Move {
        public int PlayerId { get; }
        public int? PointIndex { get; }
        public ActionType ActionType { get; }
        public int? TargetIndex { get; }
        public bool IsSuccessful { get; }

        public Move(int playerId, ActionType actionType, int? pointIndex = null, int? targetIndex = null, bool isSuccessful = false) {
            PlayerId = playerId;
            ActionType = actionType;
            PointIndex = pointIndex;
            TargetIndex = targetIndex;
            IsSuccessful = isSuccessful;
        }

        public bool IsPointPlacement() => ActionType == ActionType.PlacePoint;
        public bool IsCannonShot() => ActionType == ActionType.ShootCannon;

    }
}
