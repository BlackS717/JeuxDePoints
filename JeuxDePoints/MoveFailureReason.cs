namespace JeuxDePoints {
    public enum MoveFailureReason {
        None = 0,
        InvalidInput = 1,
        OutOfBounds = 2,
        OccupiedCell = 3,
        NoAmmo = 4,
        CannotShootOwnPoint = 5,
        CannotShootOpponentPoint = 6,
        ImmuneOwnLine = 7,
        ImmuneOpponentLine = 8,
        RuleBlocked = 9,
        Unknown = 10
    }
}
