using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;

namespace JeuxDePoints {
    public class GameStateSnapshot {
        public int Rows { get; }
        public int Cols { get; }
        public int[] Points { get; }
        public List<LineState> Lines { get; }
        public Dictionary<int, List<LineState>> PointLines { get; }
        public List<CannonStateSnapshot> Cannons { get; }
        public int CurrentPlayerId { get; }
        public int CurrentTurn { get; }
        public bool IsGameOver { get; }
        public int[] PlayerScores { get; }

        public GameStateSnapshot(
            int rows,
            int cols,
            int[] points,
            IEnumerable<LineState> lines,
            Dictionary<int, List<LineState>> pointLines,
            IEnumerable<CannonStateSnapshot> cannons,
            int currentPlayerId,
            int currentTurn,
            bool isGameOver,
            int[] playerScores) {
            Rows = rows;
            Cols = cols;
            Points = (int[])points.Clone();
            Lines = lines.Select(CloneLineState).ToList();
            PointLines = ClonePointLines(pointLines);
            Cannons = cannons.Select(CloneCannonState).ToList();
            CurrentPlayerId = currentPlayerId;
            CurrentTurn = currentTurn;
            IsGameOver = isGameOver;
            PlayerScores = (int[])playerScores.Clone();
        }

        public GameStateSnapshot Clone() {
            return new GameStateSnapshot(
                Rows,
                Cols,
                Points,
                Lines,
                PointLines,
                Cannons,
                CurrentPlayerId,
                CurrentTurn,
                IsGameOver,
                PlayerScores
            );
        }

        public string ToJson() {
            var options = new JsonSerializerOptions { WriteIndented = false };
            
            var cannonsArray = Cannons.Select(c => new {
                c.PlayerId,
                c.YPosition,
                c.CurrentAmmo,
                c.MaxAmmo
            }).ToArray();

            var linesArray = Lines.Select(l => new {
                l.StartRow,
                l.StartCol,
                l.EndRow,
                l.EndCol,
                l.PlayerId
            }).ToArray();

            // Serialize pointLines as an array of objects with key and value
            var pointLinesArray = PointLines.Select(kvp => new {
                Key = kvp.Key,
                Lines = kvp.Value.Select(l => new {
                    l.StartRow,
                    l.StartCol,
                    l.EndRow,
                    l.EndCol,
                    l.PlayerId
                }).ToArray()
            }).ToArray();

            var data = new {
                Rows,
                Cols,
                Points,
                Lines = linesArray,
                PointLines = pointLinesArray,
                Cannons = cannonsArray,
                CurrentPlayerId,
                CurrentTurn,
                IsGameOver,
                PlayerScores
            };

            return JsonSerializer.Serialize(data, options);
        }

        public static GameStateSnapshot FromJson(string json) {
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            using (JsonDocument doc = JsonDocument.Parse(json)) {
                JsonElement root = doc.RootElement;

                int rows = root.GetProperty("Rows").GetInt32();
                int cols = root.GetProperty("Cols").GetInt32();
                
                // Deserialize Points array
                int[] points = root.GetProperty("Points").EnumerateArray()
                    .Select(e => e.GetInt32())
                    .ToArray();

                // Deserialize Cannons
                var cannons = root.GetProperty("Cannons").EnumerateArray()
                    .Select(e => new CannonStateSnapshot(
                        e.GetProperty("PlayerId").GetInt32(),
                        e.GetProperty("YPosition").GetInt32(),
                        e.GetProperty("CurrentAmmo").GetInt32(),
                        e.GetProperty("MaxAmmo").GetInt32()
                    ))
                    .ToList();

                // Deserialize Lines
                var lines = root.GetProperty("Lines").EnumerateArray()
                    .Select(e => new LineState(
                        e.GetProperty("StartRow").GetInt32(),
                        e.GetProperty("StartCol").GetInt32(),
                        e.GetProperty("EndRow").GetInt32(),
                        e.GetProperty("EndCol").GetInt32(),
                        e.GetProperty("PlayerId").GetInt32()
                    ))
                    .ToList();

                // Deserialize PointLines (Dictionary)
                var pointLines = new Dictionary<int, List<LineState>>();
                JsonElement pointLinesElement = root.GetProperty("PointLines");
                foreach (JsonElement item in pointLinesElement.EnumerateArray()) {
                    int key = item.GetProperty("Key").GetInt32();
                    var linesList = item.GetProperty("Lines").EnumerateArray()
                        .Select(e => new LineState(
                            e.GetProperty("StartRow").GetInt32(),
                            e.GetProperty("StartCol").GetInt32(),
                            e.GetProperty("EndRow").GetInt32(),
                            e.GetProperty("EndCol").GetInt32(),
                            e.GetProperty("PlayerId").GetInt32()
                        ))
                        .ToList();
                    pointLines[key] = linesList;
                }

                int currentPlayerId = root.GetProperty("CurrentPlayerId").GetInt32();
                int currentTurn = root.GetProperty("CurrentTurn").GetInt32();
                bool isGameOver = root.GetProperty("IsGameOver").GetBoolean();
                int[] playerScores = root.GetProperty("PlayerScores").EnumerateArray()
                    .Select(e => e.GetInt32())
                    .ToArray();

                return new GameStateSnapshot(rows, cols, points, lines, pointLines, cannons, currentPlayerId, currentTurn, isGameOver, playerScores);
            }
        }

        private static Dictionary<int, List<LineState>> ClonePointLines(Dictionary<int, List<LineState>> source) {
            Dictionary<int, List<LineState>> clone = new Dictionary<int, List<LineState>>();
            foreach (KeyValuePair<int, List<LineState>> entry in source) {
                clone[entry.Key] = entry.Value.Select(CloneLineState).ToList();
            }
            return clone;
        }

        private static LineState CloneLineState(LineState line) {
            return new LineState(line.StartRow, line.StartCol, line.EndRow, line.EndCol, line.PlayerId);
        }

        private static CannonStateSnapshot CloneCannonState(CannonStateSnapshot cannon) {
            return new CannonStateSnapshot(cannon.PlayerId, cannon.YPosition, cannon.CurrentAmmo, cannon.MaxAmmo);
        }
    }
}
