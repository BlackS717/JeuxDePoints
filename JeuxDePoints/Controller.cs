using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;

namespace JeuxDePoints {
    public class Controller {
        private GameState state;
        private readonly List<Move> timelineMoves;
        private readonly Dictionary<string, List<Move>> saveSlots;
        private readonly Func<IDbConnection> dbConnectionFactory;
        private readonly PostgresHistorySaver historySaver;
        private readonly PostgresHistoryLoader historyLoader;
        private long? activeSessionId;
        private int historyCursor;

        public event Action StartNewGameEvent;
        public event Action ActionPerformedEvent;

        public Controller(GameState state, Func<IDbConnection> dbConnectionFactory = null) {
            this.state = state;
            timelineMoves = new List<Move>();
            saveSlots = new Dictionary<string, List<Move>>(StringComparer.OrdinalIgnoreCase);
            this.dbConnectionFactory = dbConnectionFactory;
            historySaver = new PostgresHistorySaver();
            historyLoader = new PostgresHistoryLoader();
            activeSessionId = null;
            historyCursor = 0;
        }

        public bool HandleAction(ActionType actionType, int row, int col) {
            bool result = false;
            switch (actionType) {
                case ActionType.PlacePoint:
                    result = state.PlacePoint(row, col);
                    break;
                case ActionType.ShootCannon:
                    result = state.ShootCannon(row, col);
                    break;
            }

            SyncTimelineWithStateHistory();

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

        public List<Move> GetTimelineMoveHistory() => new List<Move>(timelineMoves);

        public int GetHistoryCursor() => historyCursor;

        public bool CanUndoMove() => historyCursor > 0;

        public bool CanRedoMove() => historyCursor < timelineMoves.Count;

        public bool UndoMove() {
            if (!CanUndoMove()) {
                return false;
            }

            historyCursor--;
            return RebuildStateFromCursor();
        }

        public bool RedoMove() {
            if (!CanRedoMove()) {
                return false;
            }

            historyCursor++;
            return RebuildStateFromCursor();
        }

        public bool GoToMoveIndex(int moveIndex) {
            if (moveIndex < 0 || moveIndex >= timelineMoves.Count) {
                return false;
            }

            historyCursor = moveIndex + 1;
            return RebuildStateFromCursor();
        }

        public List<string> GetSaveSlotNames() {
            if (HasDatabaseBackend()) {
                List<string> databaseSlots = LoadSaveSlotNamesFromDatabase();
                if (databaseSlots.Count > 0) {
                    return databaseSlots;
                }
            }

            List<string> names = new List<string>(saveSlots.Keys);
            names.Sort(StringComparer.OrdinalIgnoreCase);
            return names;
        }

        public bool SaveCurrentToSlot(string slotName) {
            if (string.IsNullOrWhiteSpace(slotName)) {
                return false;
            }

            if (HasDatabaseBackend() && SaveCurrentToDatabaseSlot(slotName.Trim())) {
                return true;
            }

            string normalizedName = slotName.Trim();
            List<Move> copiedTimeline = CopyTimeline(timelineMoves);
            saveSlots[normalizedName] = copiedTimeline;
            return true;
        }

        public bool LoadFromSlot(string slotName) {
            if (string.IsNullOrWhiteSpace(slotName)) {
                return false;
            }

            if (HasDatabaseBackend() && LoadFromDatabaseSlot(slotName.Trim())) {
                return true;
            }

            string normalizedName = slotName.Trim();
            if (!saveSlots.ContainsKey(normalizedName)) {
                return false;
            }

            timelineMoves.Clear();
            timelineMoves.AddRange(CopyTimeline(saveSlots[normalizedName]));
            historyCursor = timelineMoves.Count;
            return RebuildStateFromCursor();
        }

        public bool DeleteSaveSlot(string slotName) {
            if (string.IsNullOrWhiteSpace(slotName)) {
                return false;
            }

            string normalizedName = slotName.Trim();

            if (HasDatabaseBackend()) {
                bool deletedFromDb = DeleteDatabaseSlot(normalizedName);
                if (deletedFromDb) {
                    return true;
                }
            }

            return saveSlots.Remove(normalizedName);
        }

        public int GetCurrentTurn() => state.GetCurrentTurn();

        public int GetCurrentPlayerId() => state.GetCurrentPlayerId();

        public int GetPlayerScore(int playerId) => state.GetPlayerScore(playerId);

        public List<Line> GetLines() => state.GetLines();

        public int GetPointValue(int index) => state.GetPointValue(index);

        public int GetPointValue(int row, int col) => state.GetPointValue(row, col);

        public bool IsLinePoint(int row, int col) => state.IsLinePoint(row, col);

        public (int, int) GetPointCoordinates(int index) => state.GetPointCoordinates(index);

        public int GetPointsIndex(int row, int col) => state.GetPointIndex(row, col);

        public bool IsGameOver() => state.IsGameOver();

        public void EndGame() {
            if (state.IsGameOver()) {
                return;
            }

            state.EndGame();
            ActionPerformedEvent?.Invoke();
        }

        public bool IsUsingDatabasePersistence() => HasDatabaseBackend();

        public string GetPersistenceModeLabel() {
            if (HasDatabaseBackend()) {
                return "Persistence: Database (PostgreSQL configured)";
            }

            return "Persistence: Memory only";
        }

        public void StartNewGame() {
            StartNewGame(state.GetRows(), state.GetCols());
        }

        public void StartNewGame(int rows, int cols) {
            state = new GameState(rows, cols);
            timelineMoves.Clear();
            historyCursor = 0;
            activeSessionId = null;  // Reset session for new game with potentially different dimensions

            StartNewGameEvent?.Invoke();
            ActionPerformedEvent?.Invoke();
        }

        public int GetShotTargetCol(int power) => state.GetShotTargetCol(power);

        private void SyncTimelineWithStateHistory() {
            List<Move> stateHistory = state.GetMoveHistory();

            if (stateHistory.Count < historyCursor) {
                timelineMoves.Clear();
                timelineMoves.AddRange(stateHistory);
                historyCursor = stateHistory.Count;
                return;
            }

            if (historyCursor < timelineMoves.Count) {
                timelineMoves.RemoveRange(historyCursor, timelineMoves.Count - historyCursor);
            }

            if (stateHistory.Count > historyCursor) {
                for (int i = historyCursor; i < stateHistory.Count; i++) {
                    timelineMoves.Add(stateHistory[i]);
                }
                historyCursor = stateHistory.Count;
            }
        }

        private bool RebuildStateFromCursor() {
            GameState rebuiltState = new GameState(state.GetRows(), state.GetCols());

            int moveCount = Math.Min(historyCursor, timelineMoves.Count);
            List<Move> movesToReplay = timelineMoves.Take(moveCount).ToList();
            List<Move> appliedMoves = ReplayMoves(rebuiltState, movesToReplay, true);

            // Keep cursor aligned with what was actually applied.
            historyCursor = appliedMoves.Count;

            state = rebuiltState;
            ActionPerformedEvent?.Invoke();
            return true;
        }

        private List<Move> ReplayMoves(GameState targetState, IEnumerable<Move> moves, bool validateBounds) {
            List<Move> appliedMoves = new List<Move>();

            foreach (Move move in moves) {
                int? index = move.PointIndex ?? move.TargetIndex;
                if (!index.HasValue) {
                    continue;
                }

                if (validateBounds && (index.Value < 0 || index.Value >= (targetState.GetRows() * targetState.GetCols()))) {
                    Console.WriteLine($"Warning: Move index {index.Value} out of bounds for grid {targetState.GetRows()}x{targetState.GetCols()}. Skipping move.");
                    continue;
                }

                (int row, int col) = targetState.GetPointCoordinates(index.Value);
                bool applied = false;

                switch (move.ActionType) {
                    case ActionType.PlacePoint:
                        applied = targetState.PlacePoint(row, col);
                        break;
                    case ActionType.ShootCannon:
                        applied = targetState.ShootCannon(row, col);
                        break;
                }

                if (applied) {
                    appliedMoves.Add(move);
                }
            }

            return appliedMoves;
        }

        private static List<Move> CopyTimeline(List<Move> source) {
            List<Move> copy = new List<Move>(source.Count);

            for (int i = 0; i < source.Count; i++) {
                Move move = source[i];
                copy.Add(new Move(
                    move.PlayerId,
                    move.ActionType,
                    move.PointIndex,
                    move.TargetIndex,
                    move.IsSuccessful,
                    move.Row,
                    move.Col,
                    move.Power,
                    move.TimestampUtc,
                    move.Notes
                ));
            }

            return copy;
        }

        private bool HasDatabaseBackend() {
            return dbConnectionFactory != null;
        }

        private List<string> LoadSaveSlotNamesFromDatabase() {
            try {
                using (IDbConnection connection = dbConnectionFactory()) {
                    connection.Open();

                    if (!EnsureActiveSessionId(connection)) {
                        return new List<string>();
                    }

                    List<SaveSlotRow> slots = historyLoader.LoadSaveSlots(connection, activeSessionId.Value);
                    return slots.Select(s => s.SlotName).OrderBy(s => s, StringComparer.OrdinalIgnoreCase).ToList();
                }
            } catch {
                return new List<string>();
            }
        }

        private bool SaveCurrentToDatabaseSlot(string slotName) {
            try {
                using (IDbConnection connection = dbConnectionFactory()) {
                    connection.Open();
                    using (IDbTransaction transaction = connection.BeginTransaction()) {
                        try {
                            if (!activeSessionId.HasValue) {
                                string rulesJson = BuildRulesJson(state.GetRows(), state.GetCols());
                                string rulesHash = ComputeSha256Hex(rulesJson);
                                activeSessionId = historySaver.CreateSession(connection, "JeuxDePoints Session", rulesHash, rulesJson, transaction);
                            }

                            long saveSlotId = historySaver.UpsertSaveSlot(
                                connection,
                                activeSessionId.Value,
                                slotName,
                                25,
                                historyCursor,
                                transaction
                            );

                            // Replace slot content on each save so existing seq values can be rewritten safely.
                            historySaver.TruncateBranch(connection, saveSlotId, 0, transaction);

                            // Save all moves and create checkpoints at intervals
                            for (int i = 0; i < historyCursor; i++) {
                                historySaver.SaveAction(connection, saveSlotId, i + 1, timelineMoves[i], transaction);
                            }

                            // Save checkpoint for the final state
                            GameStateSnapshot finalSnapshot = state.GetSnapshot();
                            string finalStateJson = finalSnapshot.ToJson();
                            historySaver.SaveCheckpoint(connection, saveSlotId, historyCursor, finalStateJson, transaction);

                            historySaver.UpdateCurrentActionSeq(connection, saveSlotId, historyCursor, transaction);
                            transaction.Commit();
                            return true;
                        } catch {
                            transaction.Rollback();
                            return false;
                        }
                    }
                }
            } catch {
                return false;
            }
        }

        private bool LoadFromDatabaseSlot(string slotName) {
            try {
                using (IDbConnection connection = dbConnectionFactory()) {
                    connection.Open();

                    if (!EnsureActiveSessionId(connection)) {
                        return false;
                    }

                    SlotLoadPlan loadPlan = historyLoader.BuildSlotLoadPlan(
                        connection,
                        activeSessionId.Value,
                        slotName,
                        state.GetRows(),
                        state.GetCols());

                    if (loadPlan == null) {
                        return false;
                    }

                    // Create new GameState with correct grid dimensions IMMEDIATELY to avoid index out of bounds
                    GameState rebuiltState = new GameState(loadPlan.Rows, loadPlan.Cols);
                    state = rebuiltState;

                    if (!string.IsNullOrEmpty(loadPlan.CheckpointStateJson)) {
                        try {
                            // Deserialize the saved state snapshot to restore board state at checkpoint
                            GameStateSnapshot savedSnapshot = GameStateSnapshot.FromJson(loadPlan.CheckpointStateJson);
                            // Restore the state from checkpoint (board layout, cannons, etc.)
                            rebuiltState = RestoreStateFromSnapshot(savedSnapshot);
                            state = rebuiltState;
                        } catch (Exception ex) {
                            // If deserialization fails, log and continue with empty board.
                            Console.WriteLine($"Failed to restore checkpoint: {ex.Message}");
                        }
                    }

                    timelineMoves.Clear();
                    timelineMoves.AddRange(CopyTimeline(loadPlan.Moves));

                    List<Move> appliedMoves = ReplayMoves(state, timelineMoves, true);
                    timelineMoves.Clear();
                    timelineMoves.AddRange(CopyTimeline(appliedMoves));

                    historyCursor = timelineMoves.Count;
                    ActionPerformedEvent?.Invoke();
                    return true;
                }
            } catch {
                return false;
            }
        }

        private bool DeleteDatabaseSlot(string slotName) {
            try {
                using (IDbConnection connection = dbConnectionFactory()) {
                    connection.Open();

                    if (!EnsureActiveSessionId(connection)) {
                        return false;
                    }

                    List<SaveSlotRow> slots = historyLoader.LoadSaveSlots(connection, activeSessionId.Value);
                    SaveSlotRow slot = slots.FirstOrDefault(s => string.Equals(s.SlotName, slotName, StringComparison.OrdinalIgnoreCase));
                    if (slot == null) {
                        return false;
                    }

                    using (IDbTransaction transaction = connection.BeginTransaction()) {
                        try {
                            bool deleted = historySaver.DeleteSaveSlot(connection, slot.Id, transaction);
                            transaction.Commit();
                            return deleted;
                        } catch {
                            transaction.Rollback();
                            return false;
                        }
                    }
                }
            } catch {
                return false;
            }
        }

        private bool EnsureActiveSessionId(IDbConnection connection) {
            if (activeSessionId.HasValue) {
                return true;
            }

            List<MatchSessionRow> sessions = historyLoader.LoadSessions(connection);
            if (sessions.Count == 0) {
                return false;
            }

            activeSessionId = sessions[0].Id;
            return true;
        }

        private static string BuildRulesJson(int rows = -1, int cols = -1) {
            var fields = typeof(GameRule)
                .GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static)
                .Where(f => f.IsLiteral && !f.IsInitOnly)
                .OrderBy(f => f.Name)
                .ToList();

            StringBuilder builder = new StringBuilder();
            builder.Append("{");
            
            // Include grid dimensions if provided
            if (rows > 0 && cols > 0) {
                builder.Append("\"GridRows\":").Append(rows).Append(",");
                builder.Append("\"GridCols\":").Append(cols).Append(",");
            }

            for (int i = 0; i < fields.Count; i++) {
                FieldInfo field = fields[i];
                object value = field.GetRawConstantValue();

                builder.Append("\"").Append(field.Name).Append("\":");
                switch (value) {
                    case string stringValue:
                        builder.Append("\"").Append(stringValue.Replace("\\", "\\\\").Replace("\"", "\\\"")).Append("\"");
                        break;
                    case bool boolValue:
                        builder.Append(boolValue ? "true" : "false");
                        break;
                    default:
                        builder.Append(Convert.ToString(value, System.Globalization.CultureInfo.InvariantCulture));
                        break;
                }

                if (i < fields.Count - 1) {
                    builder.Append(",");
                }
            }

            builder.Append("}");
            return builder.ToString();
        }

        private static GameState RestoreStateFromSnapshot(GameStateSnapshot snapshot) {
            // Create a new GameState with the snapshot's dimensions
            GameState restoredState = new GameState(snapshot.Rows, snapshot.Cols);
            
            // Restore points: iterate through the snapshot and place points where they exist
            for (int i = 0; i < snapshot.Points.Length; i++) {
                if (snapshot.Points[i] != 0) {
                    (int row, int col) = restoredState.GetPointCoordinates(i);
                    int pointValue = snapshot.Points[i];
                    // Directly set the point value in the state
                    restoredState.RestorePoint(row, col, pointValue);
                }
            }
            
            // Restore lines
            foreach (var lineState in snapshot.Lines) {
                restoredState.RestoreLine(lineState);
            }
            
            // Restore cannons
            int cannonIndex = 0;
            foreach (var cannonSnapshot in snapshot.Cannons) {
                restoredState.RestoreCannon(cannonSnapshot, cannonIndex);
                cannonIndex++;
            }
            
            // Restore game state properties
            restoredState.RestoreGameState(snapshot.CurrentPlayerId, snapshot.CurrentTurn, snapshot.IsGameOver, snapshot.PlayerScores);
            
            return restoredState;
        }

        private static string ComputeSha256Hex(string text) {
            byte[] input = Encoding.UTF8.GetBytes(text ?? string.Empty);
            using (SHA256 sha = SHA256.Create()) {
                byte[] hash = sha.ComputeHash(input);
                StringBuilder builder = new StringBuilder(hash.Length * 2);
                for (int i = 0; i < hash.Length; i++) {
                    builder.Append(hash[i].ToString("x2"));
                }
                return builder.ToString();
            }
        }
    }
}
