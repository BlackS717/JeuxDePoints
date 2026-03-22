using System;

namespace JeuxDePoints {
    internal class ShotResolver {
        private readonly GameState state;

        public ShotResolver(GameState state) {
            this.state = state;
        }

        public bool ShootCannon(int targetRow, int targetCol) {
            int actingPlayerId = state.CurrentPlayerId;
            int targetIndex = state.GetPointIndex(targetRow, targetCol);

            if (!state.CanShootCannon(actingPlayerId)) {
                Console.WriteLine($"Player {actingPlayerId + 1} can't shoot - no ammo or cannon is on cooldown");

                Cannon cannon = state.Cannons[actingPlayerId];
                if (cannon.GetCurrentAmmo() == 0) {
                    state.ReloadCannon(actingPlayerId);
                }

                state.RecordMove(actingPlayerId, ActionType.ShootCannon, null, targetIndex, false);
                return false;
            }

            state.Cannons[actingPlayerId].Shoot();

            bool hitAnyPoint = state.IsPoint(targetRow, targetCol);
            if (!hitAnyPoint) {
                HandleMissedShot();
                Console.WriteLine(" - no point was hit");
                state.RecordMove(actingPlayerId, ActionType.ShootCannon, null, targetIndex, false);
                return false;
            }

            if (!GameRule.CAN_SHOOT_OPPONENT_POINTS && !GameRule.CAN_SHOOT_OWN_POINTS) {
                HandleMissedShot();
                Console.WriteLine(" - can't shoot points");
                state.RecordMove(actingPlayerId, ActionType.ShootCannon, null, targetIndex, false);
                return false;
            }

            bool success;
            if (state.IsLinePoint(targetRow, targetCol)) {
                success = HandleShotAtLine(targetRow, targetCol);
            } else {
                success = HandleShotAtPoint(targetRow, targetCol);
            }

            state.RecordMove(actingPlayerId, ActionType.ShootCannon, null, targetIndex, success);

            return success;
        }

        private bool HandleShotAtPoint(int targetRow, int targetCol) {
            bool hitOwnPoint = state.IsCurrentPlayerPoint(targetRow, targetCol);

            if (hitOwnPoint) {
                if (!GameRule.CAN_SHOOT_OWN_POINTS) {
                    HandleMissedShot();
                    Console.WriteLine(" missed the shot - can't shoot own points");
                    return false;
                }
            } else {
                if (!GameRule.CAN_SHOOT_OPPONENT_POINTS) {
                    HandleMissedShot();
                    Console.WriteLine(" missed the shot - can't shoot opponent points");
                    return false;
                }
            }

            HandleSuccessfulShot(targetRow, targetCol, true);
            return true;
        }

        private bool HandleShotAtLine(int targetRow, int targetCol) {
            bool hitOwnLine = state.IsCurrentPlayerLine(targetRow, targetCol);

            if (hitOwnLine) {
                if (GameRule.LINE_POINTS_ARE_IMMUNE_TO_OWN_CANNON) {
                    HandleMissedShot();
                    Console.WriteLine(" - can't destroy own line");
                    return false;
                }

                if (!GameRule.CAN_SHOOT_OWN_POINTS) {
                    HandleMissedShot();
                    Console.WriteLine(" - can't shoot own points");
                    return false;
                }
            } else {
                if (GameRule.LINE_POINTS_ARE_IMMUNE_TO_OPPONENT_CANNON) {
                    HandleMissedShot();
                    Console.WriteLine(" - can't destroy opponent line");
                    return false;
                }

                if (!GameRule.CAN_SHOOT_OPPONENT_POINTS) {
                    HandleMissedShot();
                    Console.WriteLine("  - can't shoot opponent points");
                    return false;
                }
            }

            HandleSuccessfulShot(targetRow, targetCol, false);
            return true;
        }

        private void HandleSuccessfulShot(int targetRow, int targetCol, bool isPoint) {
            int currentPlayerId = state.CurrentPlayerId;
            Console.WriteLine($"Player {currentPlayerId + 1} successfully hit a point at ({targetRow}, {targetCol})");

            if (GameRule.SUCCESSFUL_SHOT_REFUND_AMMO) {
                Cannon cannon = state.Cannons[currentPlayerId];
                if (cannon != null) {
                    cannon.GiveAmmo(1);
                }
            }

            if (isPoint) {
                int index = state.GetPointIndex(targetRow, targetCol);
                state.Points[index] = 0;
            } else {
                state.UpdateLinesAfterShot(targetRow, targetCol);
            }

            if (GameRule.SUCCESSFUL_SHOT_CONSUME_TURN) {
                state.UpdatePlayerTurn();
            }
        }

        private void HandleMissedShot() {
            if (GameRule.MISSED_SHOT_CONSUME_TURN) {
                state.UpdatePlayerTurn();
            }
        }
    }
}
