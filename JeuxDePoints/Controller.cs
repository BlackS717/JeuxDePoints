using System;
using System.Collections.Generic;

namespace JeuxDePoints {
    public class Controller {
        private GameState state;

        public event Action StartNewGameEvent;
        

        public Controller(GameState state) {
            this.state = state;
        }

        public bool HandleAction(ActionType actionType, int playerId, int x, int y) {
            bool result = false;
            switch (actionType) {
                case ActionType.PlacePoint:
                    result = state.PlacePoint(x, y);
                    break;
                case ActionType.ShootCannon:
                    result = state.ShootCannon(x, y);
                    break;
            }

            return result;
        }

        public int GetRows() => state.GetRows();
        public int GetCols() => state.GetCols();

        public List<Move> GetMoveHistory() => state.GetMoveHistory();

        public int GetCurrentTurn() => state.GetCurrentTurn();

        public int GetCurrentPlayerId() => state.GetCurrentPlayerId();

        public List<Line> GetLines() => state.GetLines();

        public int GetPointValue(int index) => state.GetPointValue(index);

        public int GetPointValue(int row, int col) => state.GetPointValue(row, col);

        public bool IsGameOver() => state.IsGameOver();

        public void StartNewGame() {
            state = new GameState(state.GetRows(), state.GetCols());

            StartNewGameEvent?.Invoke();
        }



    }
}
