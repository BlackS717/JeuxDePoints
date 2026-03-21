using System;
using System.Collections.Generic;
using System.Linq;

namespace JeuxDePoints {
    public class GameState {
        private readonly int rows;
        private readonly int cols;

        private int[] points;
        private List<Line> lines;

        private Dictionary<int, List<Line>> pointLines; // maps a point index to the lines it is part of, used for efficient line detection and validation

        private int[] cannons;
        private int currentPlayerId;
        private int currentTurn;
        private bool isGameOver;

        private List<Move> moves;
        private int[] playerScores;

        public GameState(int rows, int cols) {
            this.rows = rows;
            this.cols = cols;
            points = new int[rows * cols];
            cannons = new int[GameRule.NUMBER_OF_PLAYERS];
            moves = new List<Move>();
            lines = new List<Line>();
            playerScores = new int[GameRule.NUMBER_OF_PLAYERS];
            currentPlayerId = 0;
            currentTurn = 0;
            isGameOver = false;
            pointLines = new Dictionary<int, List<Line>>();
        }

        public bool PlacePoint(int row, int col) {

            if (!IsValidMove(row, col)) {
                Console.WriteLine($"Invalid move by Player {currentPlayerId + 1} at ({row}, {col})");
                return false;
            }

            int index = GetPointIndex(row, col);

            int value = currentPlayerId == 0 ? (int) CellState.Player1Point : (int) CellState.Player2Point;

            points[index] = value;

            playerScores[currentPlayerId] += DetectLines(row, col);

            RecordMove(currentPlayerId, ActionType.PlacePoint, index);

            UpdatePlayerTurn();

            return true;
        }

        public bool ShootCannon(int targetRow, int targetCol) {
            // Implement cannon shooting logic here based on game rules
            // This method should check if the player has cannons available, validate the target, and update the game state accordingly
            return false; // Placeholder return value
        }

        private int DetectLines(int row, int col) {
            int linesFormed = 0;
            int[][] axes = {
                new int[]{ 0, 1 },   // horizontal
                new int[]{ 1, 0 },   // vertical
                new int[]{ 1, 1 },   // diagonal \
                new int[]{ -1, 1 },   // diagonal /
            };
            
            for (int i = 0; i < axes.Length; i++) {
                linesFormed += ScanAxes(axes[i], row, col);
                
                if(!GameRule.CAN_USE_POINTS_IN_LINES && linesFormed > 0) {
                    break;
                }
            }

            return linesFormed;
        }

        private int ScanAxes(int[] axes, int row, int col) {
            int playerPointValue = currentPlayerId == 0 ? (int)CellState.Player1Point : (int)CellState.Player2Point;
            int playerLineValue = currentPlayerId == 0 ? (int)CellState.Player1Line : (int)CellState.Player2Line;

            int useCount = 0; // the initial point can only be used once to form a line if CAN_USE_POINTS_IN_LINES is false, so we track the number of time it's used in line formation for validation purposes


            int deltaRow = axes[0];
            int deltaCol = axes[1];

            // scan first direction
            List<int> consecutivePoint = ScanDirection(deltaRow, deltaCol, row, col, playerPointValue, playerLineValue, ref useCount);

            // scan opposite direction
            List<int> consecutivePointOpp = ScanDirection(-deltaRow, -deltaCol, row, col, playerPointValue, playerLineValue, ref useCount);

            int totalPointInAxis = consecutivePoint.Count + consecutivePointOpp.Count - 1; // -1 because the placed point is counted in both directions

            List<Line> allLineInAxe = GetValidLine(consecutivePoint, consecutivePointOpp);


            lines.AddRange(allLineInAxe);

            return allLineInAxe.Count;
        }

        private void ConvertPointsToLine(List<int> pointsInLine) {
            for (int i = 0; i < GameRule.TOTAL_POINTS_IN_LINE; i++) {
                int index = pointsInLine[i];
                points[index] = currentPlayerId == 0 ? (int)CellState.Player1Line : (int)CellState.Player2Line;
            }
        }

        private List<Line> GetValidLine(List<int> dir1, List<int> dir2) {
            List<Line> validLines = new List<Line>();

            // prioritize the longest direction for line formation if the rule CONNECT_TO_LONGEST_CHAIN is true
            if(GameRule.CONNECT_TO_LONGEST_CHAIN) {
                // Ensure dir1 is the longer list
                if (dir2.Count > dir1.Count) {
                    var temp = dir1;
                    dir1 = dir2;
                    dir2 = temp;
                }
            } else {
                // start with the shortest direction unless one direction is already long enough to form a line on its own
                // in which case we prioritize that direction for line formation
                // Ensure dir1 is the shorter list
                if (dir1.Count > dir2.Count) {
                    var temp = dir1;
                    dir1 = dir2; 
                    dir2 = temp;
                }
            }

            // case 1: if the longest direction has enough points to form a line on its own, we draw a line from those points
            Line primaryLine = CreateLineFromPoints(dir1);

            // case 2: if the shortest direction have enough points to form a line on its own, we draw a line from those points
            Line secondaryLine = CreateLineFromPoints(dir2);

            // case 3: if neither direction has enough points to form a line on its own, but together they have enough points to form a line
            // we combine the points from both directions to form a line
            // (taking care to not double count the placed point which is included in both directions)

            Line combinedLine = null;

            if (primaryLine == null && secondaryLine == null) {
                // invert dir1 so that the points are in the correct order from one end of the line to the other
                List<int> reversed = new List<int>(dir1);
                reversed.Reverse();

                List<int> combined = reversed.Concat(dir2.Skip(1)).Distinct().Take(GameRule.TOTAL_POINTS_IN_LINE).ToList();
                int combinedCount = combined.Count;
                combinedLine = CreateLineFromPoints(combined);
            }
            
            if (primaryLine != null) {
                validLines.Add(primaryLine);
            }
            if (secondaryLine != null) {
                validLines.Add(secondaryLine);
            }
            if (combinedLine != null) {
                validLines.Add(combinedLine);
            }

            return validLines;
        }

        private Line CreateLineFromPoints(List<int> pointsInLine) {
            if (pointsInLine.Count < GameRule.TOTAL_POINTS_IN_LINE) {
                return null; // not enough points to form a line
            }

            (int, int) startCoordsTuple = GetPointCoordinates(pointsInLine.First());
            (int, int) endCoordsTuple = GetPointCoordinates(pointsInLine[GameRule.TOTAL_POINTS_IN_LINE - 1]);

            int[] startCoords = new int[] { startCoordsTuple.Item1, startCoordsTuple.Item2 };
            int[] endCoords = new int[] { endCoordsTuple.Item1, endCoordsTuple.Item2 };

            Line newLine = new Line(startCoords, endCoords, currentPlayerId);
            if (!CanFormLine(newLine)) {
                return null; // doesn't meet the criteria to form a line
            }

            ConvertPointsToLine(pointsInLine);

            // map the points in the valid lines to those lines for efficient lookup when validating future lines
            foreach (int pointIndex in pointsInLine) {
                if (!pointLines.ContainsKey(pointIndex)) {
                    pointLines[pointIndex] = new List<Line>();
                }
                pointLines[pointIndex].Add(newLine);
            }

            return newLine;
        }

        private bool CanFormLine(Line line) {
            // check if the new line intersects with any existing line of the same player
            bool intersectsOwnLine = lines.Any(existingLine => existingLine.playerId == currentPlayerId && existingLine.Intersects(line));

            //  check if the new line intersects with any existing line of the opposing player
            bool intersectsOpponentLine = lines.Any(existingLine => existingLine.playerId != currentPlayerId && existingLine.Intersects(line));

            if (intersectsOwnLine && !GameRule.CAN_CUT_THROUGH_OWN_LINE) {
                return false; // can't cut through own points
            }

            if (intersectsOpponentLine && !GameRule.CAN_CUT_THROUGH_OPPONENT_LINE) {
                return false; // can't cut through opponent points
            }

            return true;
        }


        private List<int> ScanDirection(int deltaRow, int deltaCol, int row, int col, int playerPointValue, int playerLineValue, ref int useCount) {

            List<int> pointsInThisDirection = new List<int> {
                GetPointIndex(row, col) // include the placed point itself
            }; 

            if(!GameRule.CAN_USE_POINTS_IN_LINES) {
                // if a point in line can't be reused to form another line
                // we remove the initial point if use count is already 1
                // (meaning it's already being used in another line formation) to prevent it from being counted again in this line formation
                if (useCount >= 1) {
                    pointsInThisDirection.Clear();
                }
            }

            int r = row + deltaRow;
            int c = col + deltaCol;

            while (IsWithinBounds(r, c)) {
                int index = GetPointIndex(r, c);
                int value = points[index];
                if (value != playerPointValue && value != playerLineValue) {
                    break;
                }
                if (value == playerLineValue) {
                    if (GameRule.CAN_USE_POINTS_IN_LINES) {
                        // if it's part of a line, check if the previous point is not part of the same line
                        // if so, we can use this point to form a line
                        if(!IsPartOfSameLine(pointsInThisDirection.Last(), index)) {
                            pointsInThisDirection.Add(index);
                        } else {
                            break; // if it's part of the same line as the last point we added, we can't use it
                        }
                    } else {
                        break; // if we already used a line point in this line, we can't use another one
                    }
                } else {
                    pointsInThisDirection.Add(index);
                }

                r += deltaRow;
                c += deltaCol;
            }

            if (pointsInThisDirection.Count >= GameRule.TOTAL_POINTS_IN_LINE) {
                useCount++;
            }

            return pointsInThisDirection;
        }

        private bool IsPartOfExistingLine(int pointIndex) {
            return pointLines.ContainsKey(pointIndex) && pointLines[pointIndex].Count > 0;
        }

        private bool IsPartOfSameLine(int pointIndex1, int pointIndex2) {
            if (!pointLines.ContainsKey(pointIndex1) || !pointLines.ContainsKey(pointIndex2)) {
                return false;
            }
            List<Line> lines1 = pointLines[pointIndex1];
            List<Line> lines2 = pointLines[pointIndex2];
            return lines1.Any(line => lines2.Contains(line));
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

        private void RecordMove(int playerId, ActionType actionType, int? pointIndex = null, int? targetIndex = null, bool isSuccessful = false) {
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
            cannons = new int[GameRule.NUMBER_OF_PLAYERS];
            currentPlayerId = 0;
            currentTurn = 0;
            isGameOver = false;
            moves.Clear();
            lines.Clear();
            playerScores = new int[GameRule.NUMBER_OF_PLAYERS];
            pointLines.Clear();
        }

        public int GetPointValue(int row, int col) {
            return points[GetPointIndex(row, col)];
        }

        public int GetPointValue(int index) {
            return points[index];
        }

        private int GetPointIndex(int row, int col) {
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

        private void UpdatePlayerTurn() {
            if (GameRule.IS_TURN_BASE) {
                currentPlayerId = (currentPlayerId + 1) % GameRule.NUMBER_OF_PLAYERS;
            }
            currentTurn++;
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
    }
}
