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
        private void UpdateLinesAfterShot(int targetRow, int targetCol) {
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
            RevertStandaloneLinePointsToRegularPoints(affectedPointIndices, destroyedPointIndex);

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

            int value = currentPlayerId == 0 ? (int)CellState.Player1Point : (int)CellState.Player2Point;

            points[index] = value;

            playerScores[currentPlayerId] += DetectLines(row, col);

            // PrintBoardState();

            RecordMove(currentPlayerId, ActionType.PlacePoint, index);

            UpdatePlayerTurn();

            return true;
        }

        public bool ShootCannon(int targetRow, int targetCol) {
            // return true if the shot is considered a hit according to the game rules, false if it's a miss
            // update the game state accordingly (e.g., remove the hit point, update scores, etc.) based on the game rules

            if(!CanShootCannon(currentPlayerId)) {
                Console.WriteLine($"Player {currentPlayerId + 1} can't shoot - no ammo or cannon is on cooldown");

                Cannon cannon = cannons[currentPlayerId];
                if (cannon.GetCurrentAmmo() == 0) {
                    ReloadCannon(currentPlayerId);
                }

                return false;
            }

            cannons[currentPlayerId].Shoot();

            // detect if the shot hit anything
            bool hitAnyPoint = !IsCellEmpty(targetRow, targetCol);
            int pointValue = GetPointValue(targetRow, targetCol);


            //Console.WriteLine("Player {0} shoots at (row {1}, col {2} = value {3})", currentPlayerId + 1, targetRow, targetCol, pointValue);

            // if it didn't hit any point, it's a miss
            if (!hitAnyPoint) {
                // TODO: record the missed shot
                HandleMissedShot(targetRow, targetCol);
                Console.WriteLine($" - no point was hit");
                return false;
            }

            // rule based validation ---

            if (!GameRule.CAN_SHOOT_OPPONENT_POINTS && !GameRule.CAN_SHOOT_OWN_POINTS) {
                HandleMissedShot(targetRow, targetCol);
                Console.WriteLine($" - can't shoot points");
                return false;
            }

            bool successfulShot = false;
            bool hitPoint = IsPoint(targetRow, targetCol);

            if (hitPoint) {
                successfulShot = HandleShotAtPoint(targetRow, targetCol);
            } else {
                successfulShot = HandleShotAtLine(targetRow, targetCol);
            }

            return successfulShot;
        }

        private bool HandleShotAtPoint(int targetRow, int targetCol) {

            bool hitOwnPoint = IsCurrentPlayerPoint(targetRow, targetCol);

            if (hitOwnPoint) {
                // return early if shooting own points is not allowed, without updating the game state
                if (!GameRule.CAN_SHOOT_OWN_POINTS) {
                    // point part of a line is still considered a point for validation purposes    
                    HandleMissedShot(targetRow, targetCol);
                    Console.WriteLine($" missed the shot - can't shoot own points");
                    return false;
                }
            } else {
                // return early if shooting opponent points is not allowed, without updating the game state
                if (!GameRule.CAN_SHOOT_OPPONENT_POINTS) {
                    HandleMissedShot(targetRow, targetCol);
                    Console.WriteLine($" missed the shot - can't shoot opponent points");
                    return false;
                }
            }

            // destroy the point
            HandleSuccessfulShot(targetRow, targetCol, true);
            return true;
        }

        private bool HandleShotAtLine(int targetRow, int targetCol) {
            bool hitOwnLine = IsCurrentPlayerLine(targetRow, targetCol);

            if (hitOwnLine) {
                
                if(GameRule.LINE_POINTS_ARE_IMMUNE_TO_OWN_CANNON) {
                    HandleMissedShot(targetRow, targetCol);
                    Console.WriteLine($" - can't destroy own line");
                    return false;
                }
                
                if (!GameRule.CAN_SHOOT_OWN_POINTS) {
                    // point part of a line is still considered a point for validation purposes    
                    HandleMissedShot(targetRow, targetCol);
                    Console.WriteLine($" - can't shoot own points");
                    return false;
                }


            } else {
                if(GameRule.LINE_POINTS_ARE_IMMUNE_TO_OPPONENT_CANNON) {
                    HandleMissedShot(targetRow, targetCol);
                    Console.WriteLine($" - can't destroy opponent line");
                    return false;
                }

                if (!GameRule.CAN_SHOOT_OPPONENT_POINTS) {
                    HandleMissedShot(targetRow, targetCol);
                    Console.WriteLine($"  - can't shoot opponent points");
                    return false;
                }
            }

            // destroy the point
            HandleSuccessfulShot(targetRow, targetCol, false);
            return true;

        }

        private void HandleSuccessfulShot(int targetRow, int targetCol, bool isPoint) {
            // Implement logic to handle a successful shot (e.g., remove the hit point, update scores, check for end of game, etc.)
            Console.WriteLine($"Player {currentPlayerId + 1} successfully hit a point at ({targetRow}, {targetCol})");

            if (GameRule.SUCCESSFUL_SHOT_REFUND_AMMO) {
                Cannon cannon = cannons[currentPlayerId];
                if (cannon != null) {
                    cannon.GiveAmmo(1);
                }
            }

            if (isPoint) {
                int index = GetPointIndex(targetRow, targetCol);
                points[index] = 0; // remove the hit point from the board
            } else {
                // if it's a line we have to update the board accordingly
                UpdateLinesAfterShot(targetRow, targetCol);


            }

            if (GameRule.SUCCESSFUL_SHOT_CONSUME_TURN) {
                UpdatePlayerTurn();
            }
        }

        private void HandleMissedShot(int targetRow, int targetCol) {
            // Implement logic to handle a missed shot (e.g., update scores, check for end of game, etc.)
            if (GameRule.MISSED_SHOT_CONSUME_TURN) {
                UpdatePlayerTurn();
            }
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

            Console.WriteLine($"Player {playerId + 1} reloads their cannon");
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

                if (!GameRule.CAN_USE_POINTS_IN_LINES && linesFormed > 0) {
                    break;
                }
            }

            return linesFormed;
        }

        private int ScanAxes(int[] axes, int row, int col) {
            int useCount = 0; // the initial point can only be used once to form a line if CAN_USE_POINTS_IN_LINES is false, so we track the number of time it's used in line formation for validation purposes


            int deltaRow = axes[0];
            int deltaCol = axes[1];

            // scan first direction
            List<int> consecutivePoint = ScanDirection(deltaRow, deltaCol, row, col, ref useCount);

            // scan opposite direction
            List<int> consecutivePointOpp = ScanDirection(-deltaRow, -deltaCol, row, col, ref useCount);

            //    int totalPointInAxis = consecutivePoint.Count + consecutivePointOpp.Count - 1; // -1 because the placed point is counted in both directions

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
            if (GameRule.CONNECT_TO_LONGEST_CHAIN) {
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
            if (IsSameAsExistingLine(newLine)) {
                return null;
            }

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

        private bool IsSameAsExistingLine(Line line) {
            return lines.Any(existingLine =>
                existingLine.playerId == line.playerId &&
                (
                    (existingLine.start[0] == line.start[0] && existingLine.start[1] == line.start[1] &&
                     existingLine.end[0] == line.end[0] && existingLine.end[1] == line.end[1])
                    ||
                    (existingLine.start[0] == line.end[0] && existingLine.start[1] == line.end[1] &&
                     existingLine.end[0] == line.start[0] && existingLine.end[1] == line.start[1])
                )
            );
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

        private void RevertStandaloneLinePointsToRegularPoints(HashSet<int> affectedPointIndices, int destroyedPointIndex) {
            foreach (int pointIndex in affectedPointIndices) {
                if (pointIndex == destroyedPointIndex) {
                    continue;
                }

                if (pointLines.ContainsKey(pointIndex)) {
                    continue;
                }

                int pointValue = points[pointIndex];
                if (pointValue == (int)CellState.Player1Line) {
                    points[pointIndex] = (int)CellState.Player1Point;
                } else if (pointValue == (int)CellState.Player2Line) {
                    points[pointIndex] = (int)CellState.Player2Point;
                }
            }
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

                if (cellValue == (int)CellState.Player1Point || cellValue == (int)CellState.Player1Line) {
                    currentPlayerId = 0;
                    playerScores[0] += DetectLines(pointRow, pointCol);
                } else if (cellValue == (int)CellState.Player2Point || cellValue == (int)CellState.Player2Line) {
                    currentPlayerId = 1;
                    playerScores[1] += DetectLines(pointRow, pointCol);
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


        private List<int> ScanDirection(int deltaRow, int deltaCol, int row, int col, ref int useCount) {

            List<int> pointsInThisDirection = new List<int> {
                GetPointIndex(row, col) // include the placed point itself
            };

            if (!GameRule.CAN_USE_POINTS_IN_LINES) {
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
                if (!IsCurrentPlayerPoint(r, c) && !IsCurrentPlayerLine(r, c)) {
                    break;
                }
                if (IsCurrentPlayerLine(r, c)) {
                    if (GameRule.CAN_USE_POINTS_IN_LINES) {
                        // if it's part of a line, check if the previous point is not part of the same line
                        // if so, we can use this point to form a line
                        if (!IsPartOfSameLine(pointsInThisDirection.Last(), index)) {
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
            cannons = new Cannon[GameRule.NUMBER_OF_PLAYERS];
            InitializeCannons();

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

        private void UpdatePlayerTurn() {
            if (GameRule.IS_TURN_BASE) {
                currentPlayerId = (currentPlayerId + 1) % GameRule.NUMBER_OF_PLAYERS;
            }
            currentTurn++;
        }

        public bool IsCurrentPlayerPoint(int row, int col) {
            int pointValue = GetPointValue(row, col);
            return (currentPlayerId == 0 && pointValue == (int)CellState.Player1Point) ||
                   (currentPlayerId == 1 && pointValue == (int)CellState.Player2Point);
        }

        public bool IsCurrentPlayerLine(int row, int col) {
            int pointValue = GetPointValue(row, col);
            return (currentPlayerId == 0 && pointValue == (int)CellState.Player1Line) ||
                   (currentPlayerId == 1 && pointValue == (int)CellState.Player2Line);
        }

        public bool IsPoint(int row, int col) {
            int pointValue = GetPointValue(row, col);
            return pointValue == (int)CellState.Player1Point || pointValue == (int)CellState.Player2Point;
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
