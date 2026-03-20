namespace JeuxDePoints {
    /// <summary>
    /// Holds all the game rules and configurations for "Jeux de Points".
    /// This class is used to centralize all the constants that define how the game behaves,
    /// making it easier to modify the game rules in one place without having to search through the entire codebase.
    /// </summary>

    internal static class GameRule {
        // Game configuration
        public const int TOTAL_POINTS_IN_LINE = 5; // ok
        public const int TOTAL_POINTS_PER_LINE = 1; // ok
        public const int NUMBER_OF_PLAYERS = 2; // ok

        // Gameplay behavior
        // general behavior
        public const bool IS_TURN_BASE = false; // ok

        // Line behavior
        public const bool CAN_USE_POINTS_IN_LINES = true; // ok
        public const bool CAN_CUT_THROUGH_OPPONENT_POINTS = false; // ok
        public const bool CAN_CUT_THROUGH_OWN_POINTS = true; // ok

        // if true
        // start the chain from the longest chain of points and only take the remaining points from the shorter chain
        // else, start from the shorter chain and take the remaining points from the longest chain
        public const bool CONNECT_TO_LONGEST_CHAIN = false;

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
