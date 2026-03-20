namespace JeuxDePoints {
    /// <summary>
    /// Holds all the game rules and configurations for "Jeux de Points".
    /// This class is used to centralize all the constants that define how the game behaves,
    /// making it easier to modify the game rules in one place without having to search through the entire codebase.
    /// </summary>

    internal static class GameRule {
        // Game configuration
        public const int TOTAL_POINTS_IN_LINE = 5;
        public const int TOTAL_POINTS_PER_LINE = 1;
        public const int NUMBER_OF_PLAYERS = 2;

        // Gameplay behavior
        // general behavior
        public const bool IS_TURN_BASE = false; // ok

        // Line behavior
        public const bool CAN_USE_POINTS_IN_LINES = true; // ok
        public const bool CAN_CUT_THROUGH_OPPONENT_POINTS = false; // ok
        public const bool CAN_CUT_THROUGH_OWN_POINTS = true; // ok

        // Cannon behavior
        public const bool LINE_POINTS_ARE_IMMUNE_TO_OPPONENT_CANNON = true;
        public const bool LINE_POINTS_ARE_IMMUNE_TO_OWN_CANNON = true;

        public const bool CANNON_USE_TURN = true;
        public const bool CAN_SHOOT_OWN_POINTS = false;
        public const bool CAN_SHOOT_OPPONENT_POINTS = true;

        public const int MIN_CANNON_POWER = 1;
        public const int MAX_CANNON_POWER = 9;

    }
}
