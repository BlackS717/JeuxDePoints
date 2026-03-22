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

        public int GetCurrentTurn() => state.GetCurrentTurn();

        public int GetCurrentPlayerId() => state.GetCurrentPlayerId();

        public List<Line> GetLines() => state.GetLines();

        public int GetPointValue(int index) => state.GetPointValue(index);

        public int GetPointValue(int row, int col) => state.GetPointValue(row, col);

        public bool IsLinePoint(int row, int col) => state.IsLinePoint(row, col);

        public (int, int) GetPointCoordinates(int index) => state.GetPointCoordinates(index);

        public int GetPointsIndex(int row, int col) => state.GetPointIndex(row, col);

        public bool IsGameOver() => state.IsGameOver();

        public bool IsUsingDatabasePersistence() => HasDatabaseBackend();

        public string GetPersistenceModeLabel() {
            if (HasDatabaseBackend()) {
                return "Persistence: Database (PostgreSQL configured)";
            }

            return "Persistence: Memory only";
        }

        public void StartNewGame() {
            state = new GameState(state.GetRows(), state.GetCols());
            timelineMoves.Clear();
            historyCursor = 0;

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

            for (int i = 0; i < historyCursor; i++) {
                Move move = timelineMoves[i];
                int? index = move.PointIndex ?? move.TargetIndex;

                if (!index.HasValue) {
                    continue;
                }

                (int row, int col) = rebuiltState.GetPointCoordinates(index.Value);
                switch (move.ActionType) {
                    case ActionType.PlacePoint:
                        rebuiltState.PlacePoint(row, col);
                        break;
                    case ActionType.ShootCannon:
                        rebuiltState.ShootCannon(row, col);
                        break;
                }
            }

            state = rebuiltState;
            ActionPerformedEvent?.Invoke();
            return true;
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
                                string rulesJson = BuildRulesJson();
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

                            historySaver.TruncateBranch(connection, saveSlotId, historyCursor, transaction);

                            for (int i = 0; i < historyCursor; i++) {
                                historySaver.SaveAction(connection, saveSlotId, i + 1, timelineMoves[i], transaction);
                            }

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

                    List<SaveSlotRow> slots = historyLoader.LoadSaveSlots(connection, activeSessionId.Value);
                    SaveSlotRow slot = slots.FirstOrDefault(s => string.Equals(s.SlotName, slotName, StringComparison.OrdinalIgnoreCase));
                    if (slot == null) {
                        return false;
                    }

                    List<Move> moves = historyLoader.LoadActions(connection, slot.Id, 1, slot.CurrentActionSeq);
                    timelineMoves.Clear();
                    timelineMoves.AddRange(CopyTimeline(moves));
                    historyCursor = timelineMoves.Count;
                    return RebuildStateFromCursor();
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

        private static string BuildRulesJson() {
            var fields = typeof(GameRule)
                .GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static)
                .Where(f => f.IsLiteral && !f.IsInitOnly)
                .OrderBy(f => f.Name)
                .ToList();

            StringBuilder builder = new StringBuilder();
            builder.Append("{");

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
