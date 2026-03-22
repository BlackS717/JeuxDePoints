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
        public const bool IS_TURN_BASE = true; // ok

        // Line behavior
        public const bool CAN_USE_POINTS_IN_LINES = true; // ok
        public const bool CAN_CUT_THROUGH_OPPONENT_LINE = false; // ok
        public const bool CAN_CUT_THROUGH_OWN_LINE = true; // ok

        // if true
        // start the chain from the longest chain of points and only take the remaining points from the shorter chain
        // else, start from the shorter chain and take the remaining points from the longest chain
        public const bool CONNECT_TO_LONGEST_CHAIN = true;

        // Cannon behavior
        // cannon always shoot a single point and can't be blocked
        // cannon can't rotate, it can only hit points in the same row and power determine how far it can shoot
        // 1 target the first intersection and 9 target the last intersection in the same row
        // player have to calculate the power of the cannon to hit the target point
        // some points can't be targeted depending on grid size
        // e.g in a 18x18 grid, 1 hit 0 and 9 hit 17
        public const bool LINE_POINTS_ARE_IMMUNE_TO_OPPONENT_CANNON = true;
        public const bool LINE_POINTS_ARE_IMMUNE_TO_OWN_CANNON = true;

        public const bool SUCCESSFUL_SHOT_CONSUME_TURN = true;
        public const bool MISSED_SHOT_CONSUME_TURN = true;

        public const bool CAN_SHOOT_OWN_POINTS = false;
        public const bool CAN_SHOOT_OPPONENT_POINTS = true;

        public const bool INFINITE_AMMO = true; // if true, player have limited ammo
        public const bool CAN_RELOAD_AMMO = true; // if true, player can reload ammo during their turn
        public const bool RELOADING_USE_TURN = true; // if true, reloading consume the player's turn
        public const bool SUCCESSFUL_SHOT_REFUND_AMMO = false; // if true, player get ammo back if they successfully shoot a point

        public const bool SHOT_ANIMATION_ENABLED = true;

        public const int MAX_AMMO = 5; // max ammo a player can have at the start of the game
        public const int AMMO_RELOAD_AMOUNT = 3; // amount of ammo reloaded when player choose to reload

        public const int MIN_CANNON_POWER = 1; // shoot at the first intersection
        public const int MAX_CANNON_POWER = 9; // shoot at the last intersection 



    }
}
