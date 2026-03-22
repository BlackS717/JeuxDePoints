using System;
using System.Collections.Generic;
using System.Linq;

namespace JeuxDePoints {
    public class GameState {
        private readonly int rows;
        private readonly int cols;

        private int[] points;
        private List<Line> lines;
        private LineEngine lineEngine;
        private ShotResolver shotResolver;

        private Dictionary<int, List<Line>> pointLines; // maps a point index to the lines it is part of, used for efficient line detection and validation

        private Cannon[] cannons;
        private int currentPlayerId;
        private int currentTurn;
        private bool isGameOver;

        private List<Move> moves;
        private int[] playerScores;

        public GameState(int rows, int cols) {
            this.rows = rows;
            this.cols = cols;
            points = new int[rows * cols];
            cannons = new Cannon[GameRule.NUMBER_OF_PLAYERS];
            moves = new List<Move>();
            lines = new List<Line>();
            playerScores = new int[GameRule.NUMBER_OF_PLAYERS];
            currentPlayerId = 0;
            currentTurn = 0;
            isGameOver = false;
            pointLines = new Dictionary<int, List<Line>>();
            lineEngine = new LineEngine(rows, cols, points, lines, pointLines);
            shotResolver = new ShotResolver(this);

            InitializeCannons();
        }

        private void InitializeCannons() {
            int startY = (rows - 1) / 2;
            for (int i = 0; i < GameRule.NUMBER_OF_PLAYERS; i++) {
                cannons[i] = new Cannon(i, startY);
            }
        }

        public bool MovePlayerCannonToRow(int playerId, int targetRow) {
            if (playerId < 0 || playerId >= GameRule.NUMBER_OF_PLAYERS) {
                return false;
            }

            Cannon cannon = cannons[playerId];
            if (cannon == null) {
                return false;
            }

            int clampedRow = Math.Max(0, Math.Min(rows - 1, targetRow));
            int deltaY = clampedRow - cannon.GetYPosition();

            if (deltaY == 0) {
                return true;
            }

            return cannon.MoveVertically(deltaY, rows);
        }

        
        // 1) Destroy and clean all affected line state.
        // 2) Rebuild possible lines from the cleaned state.
        // Rebuild must never run before destruction completes.
        internal void UpdateLinesAfterShot(int targetRow, int targetCol) {
            int index = GetPointIndex(targetRow, targetCol);

            HashSet<int> affectedPointIndices = DestroyLineAndCleanupState(index);

            if (affectedPointIndices.Count == 0) {
                return;
            }

            // rebuild only after the destruction phase has fully completed
            RebuildLinesAroundRemovedLinePoints(affectedPointIndices);
        }

        private HashSet<int> DestroyLineAndCleanupState(int destroyedPointIndex) {
            HashSet<int> affectedPointIndices = new HashSet<int>();
            points[destroyedPointIndex] = 0;

            if (!pointLines.ContainsKey(destroyedPointIndex)) {
                return affectedPointIndices; // destroyed cell was not part of any line
            }

            List<Line> affectedLines = new List<Line>(pointLines[destroyedPointIndex]);

            foreach (Line line in affectedLines) {
                lines.Remove(line);
                playerScores[line.playerId] -= GameRule.TOTAL_POINTS_PER_LINE;

                foreach (int linePointIndex in GetLinePointIndices(line)) {
                    affectedPointIndices.Add(linePointIndex);

                    if (!pointLines.ContainsKey(linePointIndex)) {
                        continue;
                    }

                    pointLines[linePointIndex].Remove(line);
                    if (pointLines[linePointIndex].Count == 0) {
                        pointLines.Remove(linePointIndex);
                    }
                }
            }

            pointLines.Remove(destroyedPointIndex);
            affectedPointIndices.Add(destroyedPointIndex);

            return affectedPointIndices;
        }


        public bool PlacePoint(int row, int col) {

            if (!IsValidMove(row, col)) {
                Console.WriteLine($"Invalid move by Player {currentPlayerId + 1} at ({row}, {col})");
                return false;
            }
            // print the board state after placing the point for debugging purposes
            //Console.WriteLine($"Player {currentPlayerId + 1} places a point at (row {row}, col {col})");

            int index = GetPointIndex(row, col);

            int value = currentPlayerId == 0 ? (int)CellState.Player1 : (int)CellState.Player2;

            points[index] = value;

            playerScores[currentPlayerId] += lineEngine.DetectLines(row, col, currentPlayerId);

            // PrintBoardState();

            RecordMove(currentPlayerId, ActionType.PlacePoint, index, null, true);

            UpdatePlayerTurn();

            return true;
        }

        public bool ShootCannon(int targetRow, int targetCol) {
            return shotResolver.ShootCannon(targetRow, targetCol);
        }

        public bool CanShootCannon(int playerId) {
            // Implement logic to check if the player can shoot the cannon based on game rules (e.g., ammo count, cooldown, etc.)
            return cannons[playerId].CanShoot(); // Placeholder return value
        }

        public void ReloadCannon(int playerId) {
            if (!GameRule.CAN_RELOAD_AMMO) {
                return;
            }

            cannons[playerId].Reload();
            if(GameRule.RELOADING_USE_TURN) {
                UpdatePlayerTurn();
            }

        }

        private List<int> GetLinePointIndices(Line line) {
            List<int> linePointIndices = new List<int>();

            int deltaRow = Math.Sign(line.end[0] - line.start[0]);
            int deltaCol = Math.Sign(line.end[1] - line.start[1]);

            int currentRow = line.start[0];
            int currentCol = line.start[1];

            for (int i = 0; i < GameRule.TOTAL_POINTS_IN_LINE; i++) {
                if (!IsWithinBounds(currentRow, currentCol)) {
                    break;
                }

                linePointIndices.Add(GetPointIndex(currentRow, currentCol));
                currentRow += deltaRow;
                currentCol += deltaCol;
            }

            return linePointIndices;
        }

        private void RebuildLinesAroundRemovedLinePoints(HashSet<int> removedLinePointIndices) {
            int previousPlayerId = currentPlayerId;

            int[][] axes = {
                new int[]{ 0, 1 },   // horizontal
                new int[]{ 1, 0 },   // vertical
                new int[]{ 1, 1 },   // diagonal \
                new int[]{ -1, 1 },  // diagonal /
            };

            HashSet<int> candidatePointIndices = new HashSet<int>();

            foreach (int removedPointIndex in removedLinePointIndices) {
                (int row, int col) = GetPointCoordinates(removedPointIndex);

                foreach (int[] axis in axes) {
                    int deltaRow = axis[0];
                    int deltaCol = axis[1];

                    CollectCandidatePointsInDirection(row, col, deltaRow, deltaCol, candidatePointIndices);
                    CollectCandidatePointsInDirection(row, col, -deltaRow, -deltaCol, candidatePointIndices);
                }
            }

            foreach (int pointIndex in candidatePointIndices) {
                (int pointRow, int pointCol) = GetPointCoordinates(pointIndex);
                int cellValue = points[pointIndex];

                if (cellValue == (int)CellState.Player1) {
                    currentPlayerId = 0;
                    playerScores[0] += lineEngine.DetectLines(pointRow, pointCol, 0);
                } else if (cellValue == (int)CellState.Player2) {
                    currentPlayerId = 1;
                    playerScores[1] += lineEngine.DetectLines(pointRow, pointCol, 1);
                }
            }

            currentPlayerId = previousPlayerId;
        }

        private void CollectCandidatePointsInDirection(int row, int col, int deltaRow, int deltaCol, HashSet<int> candidates) {
            int r = row + deltaRow;
            int c = col + deltaCol;

            while (IsWithinBounds(r, c) && !IsCellEmpty(r, c)) {
                candidates.Add(GetPointIndex(r, c));
                r += deltaRow;
                c += deltaCol;
            }
        }
        private bool IsPartOfExistingLine(int pointIndex) {
            if (!pointLines.ContainsKey(pointIndex)) {
                return false;
            }

            List<Line> mappedLines = pointLines[pointIndex];
            mappedLines.RemoveAll(line => !lines.Contains(line));

            if (mappedLines.Count == 0) {
                pointLines.Remove(pointIndex);
                return false;
            }

            return true;
        }

        private bool IsValidMove(int row, int col) {
            // check if the move is within bounds
            if (!IsWithinBounds(row, col)) {
                return false;
            }
            if (!IsCellEmpty(row, col)) {
                return false;
            }

            // additional game-specific rules can be added here
            return true;
        }

        private bool IsCellEmpty(int row, int col) {

            // check if the point is already occupied
            return IsWithinBounds(row, col) && points[GetPointIndex(row, col)] == 0;
        }

        private bool IsWithinBounds(int row, int col) {
            return row >= 0 && row < rows && col >= 0 && col < cols;
        }

        internal void RecordMove(int playerId, ActionType actionType, int? pointIndex = null, int? targetIndex = null, bool isSuccessful = false) {
            moves.Add(new Move(playerId, actionType, pointIndex, targetIndex, isSuccessful));
        }

        public void EndGame() {
            isGameOver = true;
        }
        public bool IsGameOver() {
            return isGameOver;
        }

        public void ResetGame() {
            points = new int[rows * cols];
            cannons = new Cannon[GameRule.NUMBER_OF_PLAYERS];
            InitializeCannons();

            currentPlayerId = 0;
            currentTurn = 0;
            isGameOver = false;
            moves.Clear();
            lines.Clear();
            playerScores = new int[GameRule.NUMBER_OF_PLAYERS];
            pointLines.Clear();
            lineEngine = new LineEngine(rows, cols, points, lines, pointLines);
        }

        public int GetPointValue(int row, int col) {
            return points[GetPointIndex(row, col)];
        }

        public int GetPointValue(int index) {
            return points[index];
        }

        public int GetPointIndex(int row, int col) {
            return row * cols + col;
        }

        public (int row, int col) GetPointCoordinates(int index) {
            return (index / cols, index % cols);
        }

        public int GetCurrentPlayerId() {
            return currentPlayerId;
        }

        public int GetPlayerScore(int playerId) {
            return playerScores[playerId];
        }

        public List<Move> GetMoveHistory() {
            return new List<Move>(moves);
        }

        public int GetCurrentTurn() {
            return currentTurn;
        }

        internal void UpdatePlayerTurn() {
            if (GameRule.IS_TURN_BASE) {
                currentPlayerId = (currentPlayerId + 1) % GameRule.NUMBER_OF_PLAYERS;
            }
            currentTurn++;
        }

        internal int CurrentPlayerId {
            get { return currentPlayerId; }
        }

        internal Cannon[] Cannons {
            get { return cannons; }
        }

        internal int[] Points {
            get { return points; }
        }

        public bool IsCurrentPlayerPoint(int row, int col) {
            int pointValue = GetPointValue(row, col);
            return (currentPlayerId == 0 && pointValue == (int)CellState.Player1) ||
                   (currentPlayerId == 1 && pointValue == (int)CellState.Player2);
        }

        public bool IsCurrentPlayerLine(int row, int col) {
            if (!IsCurrentPlayerPoint(row, col)) {
                return false;
            }

            return IsPartOfExistingLine(GetPointIndex(row, col));
        }

        public bool IsPoint(int row, int col) {
            int pointValue = GetPointValue(row, col);
            return pointValue == (int)CellState.Player1 || pointValue == (int)CellState.Player2;
        }

        public bool IsLinePoint(int row, int col) {
            if (!IsPoint(row, col)) {
                return false;
            }

            return IsPartOfExistingLine(GetPointIndex(row, col));
        }

        public List<Line> GetLines() {
            return new List<Line>(lines);
        }

        public int GetRows() {
            return rows;
        }

        public int GetCols() {
            return cols;
        }

        public int GetPlayerCannonY(int playerId) {
            return cannons[playerId] != null ? cannons[playerId].GetYPosition() : -1; // return -1 if the player doesn't have a cannon
        }

        public Cannon GetPlayerCannon(int playerId) {
            return cannons[playerId];
        }

        public int GetShotTargetCol(int power) {
            // clamp power to valid range
            int maxPower = GameRule.MAX_CANNON_POWER;
            int minPower = GameRule.MIN_CANNON_POWER;

            bool isPlayer1 = currentPlayerId == 0;

            power = Math.Max(minPower, Math.Min(maxPower, power));

            int minCol = 0;
            int maxCol = cols - 1;

            // map power to column index (float division for accuracy)
            int col = (int)Math.Floor(
                minCol + (power - minPower) * (maxCol - minCol) / (double)(maxPower - minPower)
            );


            // reverse for player 2 (shooting right-to-left)
            if (!isPlayer1) {
                col = maxCol - col + minCol; // mirrors the column
            }

            return col;
        }

        public void PrintBoardState() {
            for (int r = 0; r < rows; r++) {
                for (int c = 0; c < cols; c++) {
                    int pointValue = GetPointValue(r, c);
                    int symbol = pointValue;
                    Console.Write(symbol + " ");
                }
                Console.WriteLine();
            }
        }
    }
}
