using System;
using System.Collections.Generic;

namespace JeuxDePoints {
    public class Controller {
        private GameState state;

        public event Action StartNewGameEvent;
        public event Action ActionPerformedEvent;

        public Controller(GameState state) {
            this.state = state;
        }

        public bool HandleAction(ActionType actionType, int playerId, int row, int col) {
            bool result = false;
            switch (actionType) {
                case ActionType.PlacePoint:
                    result = state.PlacePoint(row, col);
                    break;
                case ActionType.ShootCannon:
                    result = state.ShootCannon(row, col);
                    break;
            }

            ActionPerformedEvent?.Invoke();

            return result;
        }

        public bool CanShootCannon(int playerId) {
            return state.CanShootCannon(playerId);
        }

        public bool MoveCurrentPlayerCannonToRow(int targetRow) {
            return state.MovePlayerCannonToRow(state.GetCurrentPlayerId(), targetRow);
        }

        public bool MovePlayerCannonToRow(int playerId, int targetRow) {
            return state.MovePlayerCannonToRow(playerId, targetRow);
        }

        public int GetCannonY(int playerId) => state.GetPlayerCannonY(playerId);

        public int GetCannonCurrentAmmo(int playerId) {
            Cannon cannon = state.GetPlayerCannon(playerId);
            return cannon != null ? cannon.GetCurrentAmmo() : 0;
        }

        public int GetCannonMaxAmmo(int playerId) {
            Cannon cannon = state.GetPlayerCannon(playerId);
            return cannon != null ? cannon.GetMaxAmmo() : 0;
        }

        public int GetRows() => state.GetRows();
        public int GetCols() => state.GetCols();

        public List<Move> GetMoveHistory() => state.GetMoveHistory();

        public int GetCurrentTurn() => state.GetCurrentTurn();

        public int GetCurrentPlayerId() => state.GetCurrentPlayerId();

        public List<Line> GetLines() => state.GetLines();

        public int GetPointValue(int index) => state.GetPointValue(index);

        public int GetPointValue(int row, int col) => state.GetPointValue(row, col);

        public (int, int) GetPointCoordinates(int index) => state.GetPointCoordinates(index);

        public int GetPointsIndex(int row, int col) => state.GetPointIndex(row, col);

        public bool IsGameOver() => state.IsGameOver();

        public void StartNewGame() {
            state = new GameState(state.GetRows(), state.GetCols());

            StartNewGameEvent?.Invoke();
        }

        public int GetShotTargetCol(int power) => state.GetShotTargetCol(power);
    }
}
