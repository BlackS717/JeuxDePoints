using System;
using System.Collections.Generic;
using System.Linq;

namespace JeuxDePoints {
    internal class LineEngine {
        private readonly int rows;
        private readonly int cols;

        private readonly int[] points;
        private readonly List<Line> lines;
        private readonly Dictionary<int, List<Line>> pointLines;

        public LineEngine(int rows, int cols, int[] points, List<Line> lines, Dictionary<int, List<Line>> pointLines) {
            this.rows = rows;
            this.cols = cols;
            this.points = points;
            this.lines = lines;
            this.pointLines = pointLines;
        }

        public int DetectLines(int row, int col, int currentPlayerId) {
            int linesFormed = 0;
            int[][] axes = {
                new int[]{ 0, 1 },   // horizontal
                new int[]{ 1, 0 },   // vertical
                new int[]{ 1, 1 },   // diagonal \
                new int[]{ -1, 1 },  // diagonal /
            };

            for (int i = 0; i < axes.Length; i++) {
                linesFormed += ScanAxes(axes[i], row, col, currentPlayerId);

                if (!GameRule.CAN_USE_POINTS_IN_LINES && linesFormed > 0) {
                    break;
                }
            }

            return linesFormed;
        }

        private int ScanAxes(int[] axes, int row, int col, int currentPlayerId) {
            int useCount = 0;

            int deltaRow = axes[0];
            int deltaCol = axes[1];

            List<int> consecutivePoint = ScanDirection(deltaRow, deltaCol, row, col, currentPlayerId, ref useCount);
            List<int> consecutivePointOpp = ScanDirection(-deltaRow, -deltaCol, row, col, currentPlayerId, ref useCount);

            List<Line> allLineInAxe = GetValidLine(consecutivePoint, consecutivePointOpp, currentPlayerId);
            lines.AddRange(allLineInAxe);

            return allLineInAxe.Count;
        }

        private List<Line> GetValidLine(List<int> dir1, List<int> dir2, int currentPlayerId) {
            List<Line> validLines = new List<Line>();

            if (GameRule.CONNECT_TO_LONGEST_CHAIN) {
                if (dir2.Count > dir1.Count) {
                    List<int> temp = dir1;
                    dir1 = dir2;
                    dir2 = temp;
                }
            } else {
                if (dir1.Count > dir2.Count) {
                    List<int> temp = dir1;
                    dir1 = dir2;
                    dir2 = temp;
                }
            }

            Line primaryLine = CreateLineFromPoints(dir1, currentPlayerId);
            Line secondaryLine = CreateLineFromPoints(dir2, currentPlayerId);

            Line combinedLine = null;

            if (primaryLine == null && secondaryLine == null) {
                List<int> reversed = new List<int>(dir1);
                reversed.Reverse();

                List<int> combined = reversed.Concat(dir2.Skip(1)).Distinct().Take(GameRule.TOTAL_POINTS_IN_LINE).ToList();
                combinedLine = CreateLineFromPoints(combined, currentPlayerId);
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

        private Line CreateLineFromPoints(List<int> pointsInLine, int currentPlayerId) {
            if (pointsInLine.Count < GameRule.TOTAL_POINTS_IN_LINE) {
                return null;
            }

            (int, int) startCoordsTuple = GetPointCoordinates(pointsInLine.First());
            (int, int) endCoordsTuple = GetPointCoordinates(pointsInLine[GameRule.TOTAL_POINTS_IN_LINE - 1]);

            int[] startCoords = new int[] { startCoordsTuple.Item1, startCoordsTuple.Item2 };
            int[] endCoords = new int[] { endCoordsTuple.Item1, endCoordsTuple.Item2 };

            Line newLine = new Line(startCoords, endCoords, currentPlayerId);
            if (IsSameAsExistingLine(newLine)) {
                return null;
            }

            if (!CanFormLine(newLine, currentPlayerId)) {
                return null;
            }

            for (int i = 0; i < GameRule.TOTAL_POINTS_IN_LINE; i++) {
                int pointIndex = pointsInLine[i];
                if (!pointLines.ContainsKey(pointIndex)) {
                    pointLines[pointIndex] = new List<Line>();
                }
                pointLines[pointIndex].Add(newLine);
            }

            return newLine;
        }

        private bool CanFormLine(Line line, int currentPlayerId) {
            bool intersectsOwnLine = lines.Any(existingLine => existingLine.playerId == currentPlayerId && existingLine.Intersects(line));
            bool intersectsOpponentLine = lines.Any(existingLine => existingLine.playerId != currentPlayerId && existingLine.Intersects(line));

            if (intersectsOwnLine && !GameRule.CAN_CUT_THROUGH_OWN_LINE) {
                return false;
            }

            if (intersectsOpponentLine && !GameRule.CAN_CUT_THROUGH_OPPONENT_LINE) {
                return false;
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

        private List<int> ScanDirection(int deltaRow, int deltaCol, int row, int col, int currentPlayerId, ref int useCount) {
            List<int> pointsInThisDirection = new List<int> {
                GetPointIndex(row, col)
            };

            if (!GameRule.CAN_USE_POINTS_IN_LINES) {
                if (useCount >= 1) {
                    pointsInThisDirection.Clear();
                }
            }

            int r = row + deltaRow;
            int c = col + deltaCol;

            while (IsWithinBounds(r, c)) {
                int index = GetPointIndex(r, c);
                if (!IsCurrentPlayerPoint(r, c, currentPlayerId)) {
                    break;
                }

                if (IsCurrentPlayerLine(r, c, currentPlayerId)) {
                    if (GameRule.CAN_USE_POINTS_IN_LINES) {
                        if (!IsPartOfSameLine(pointsInThisDirection.Last(), index)) {
                            pointsInThisDirection.Add(index);
                        } else {
                            break;
                        }
                    } else {
                        break;
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

        private bool IsCurrentPlayerPoint(int row, int col, int currentPlayerId) {
            int pointValue = points[GetPointIndex(row, col)];
            return (currentPlayerId == 0 && pointValue == (int)CellState.Player1) ||
                   (currentPlayerId == 1 && pointValue == (int)CellState.Player2);
        }

        private bool IsCurrentPlayerLine(int row, int col, int currentPlayerId) {
            if (!IsCurrentPlayerPoint(row, col, currentPlayerId)) {
                return false;
            }

            return IsPartOfExistingLine(GetPointIndex(row, col));
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

        private bool IsPartOfSameLine(int pointIndex1, int pointIndex2) {
            if (!pointLines.ContainsKey(pointIndex1) || !pointLines.ContainsKey(pointIndex2)) {
                return false;
            }

            List<Line> lines1 = pointLines[pointIndex1];
            List<Line> lines2 = pointLines[pointIndex2];
            return lines1.Any(line => lines2.Contains(line));
        }

        private int GetPointIndex(int row, int col) {
            return row * cols + col;
        }

        private (int row, int col) GetPointCoordinates(int index) {
            return (index / cols, index % cols);
        }

        private bool IsWithinBounds(int row, int col) {
            return row >= 0 && row < rows && col >= 0 && col < cols;
        }
    }
}