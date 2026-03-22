using System;
using System.Collections.Generic;
using System.Data;

namespace JeuxDePoints {
    internal class PostgresHistoryLoader {
        public List<MatchSessionRow> LoadSessions(IDbConnection connection) {
            ValidateConnection(connection);

            List<MatchSessionRow> sessions = new List<MatchSessionRow>();
            using (IDbCommand command = connection.CreateCommand()) {
                command.CommandText = @"
                    SELECT id, name, rules_hash, rules_json::text, created_at_utc, updated_at_utc
                    FROM match_session
                    ORDER BY updated_at_utc DESC;";

                using (IDataReader reader = command.ExecuteReader()) {
                    while (reader.Read()) {
                        sessions.Add(new MatchSessionRow {
                            Id = ReadInt64(reader, 0),
                            Name = ReadString(reader, 1),
                            RulesHash = ReadString(reader, 2),
                            RulesJson = ReadString(reader, 3),
                            CreatedAtUtc = ReadDateTime(reader, 4),
                            UpdatedAtUtc = ReadDateTime(reader, 5)
                        });
                    }
                }
            }

            return sessions;
        }

        public List<SaveSlotRow> LoadSaveSlots(IDbConnection connection, long sessionId) {
            ValidateConnection(connection);

            List<SaveSlotRow> slots = new List<SaveSlotRow>();
            using (IDbCommand command = connection.CreateCommand()) {
                command.CommandText = @"
                    SELECT id, session_id, slot_name, checkpoint_interval, current_action_seq, created_at_utc, updated_at_utc
                    FROM save_slot
                    WHERE session_id = @session_id
                    ORDER BY updated_at_utc DESC;";

                AddParameter(command, "@session_id", sessionId);

                using (IDataReader reader = command.ExecuteReader()) {
                    while (reader.Read()) {
                        slots.Add(new SaveSlotRow {
                            Id = ReadInt64(reader, 0),
                            SessionId = ReadInt64(reader, 1),
                            SlotName = ReadString(reader, 2),
                            CheckpointInterval = ReadInt32(reader, 3),
                            CurrentActionSeq = ReadInt32(reader, 4),
                            CreatedAtUtc = ReadDateTime(reader, 5),
                            UpdatedAtUtc = ReadDateTime(reader, 6)
                        });
                    }
                }
            }

            return slots;
        }

        public List<Move> LoadActions(IDbConnection connection, long saveSlotId, int fromSeqInclusive = 1, int? toSeqInclusive = null) {
            ValidateConnection(connection);

            string sequenceFilter = toSeqInclusive.HasValue
                ? "seq >= @from_seq AND seq <= @to_seq"
                : "seq >= @from_seq";

            List<Move> actions = new List<Move>();
            using (IDbCommand command = connection.CreateCommand()) {
                command.CommandText = $@"
                    SELECT
                        player_id,
                        action_type,
                        row_index,
                        col_index,
                        point_index,
                        target_index,
                        power,
                        is_successful,
                        timestamp_utc,
                        notes
                    FROM game_action
                    WHERE save_slot_id = @save_slot_id
                      AND {sequenceFilter}
                    ORDER BY seq ASC;";

                AddParameter(command, "@save_slot_id", saveSlotId);
                AddParameter(command, "@from_seq", fromSeqInclusive);
                if (toSeqInclusive.HasValue) {
                    AddParameter(command, "@to_seq", toSeqInclusive.Value);
                }

                using (IDataReader reader = command.ExecuteReader()) {
                    while (reader.Read()) {
                        ActionType? actionType = TryMapActionType(ReadString(reader, 1));
                        if (!actionType.HasValue) {
                            continue;
                        }

                        actions.Add(new Move(
                            ReadInt32(reader, 0),
                            actionType.Value,
                            ReadNullableInt32(reader, 4),
                            ReadNullableInt32(reader, 5),
                            ReadBoolean(reader, 7),
                            ReadNullableInt32(reader, 2),
                            ReadNullableInt32(reader, 3),
                            ReadNullableInt32(reader, 6),
                            ReadDateTime(reader, 8),
                            ReadNullableString(reader, 9) ?? string.Empty
                        ));
                    }
                }
            }

            return actions;
        }

        public ReplayCheckpointRow LoadNearestCheckpoint(IDbConnection connection, long saveSlotId, int targetActionSeq) {
            ValidateConnection(connection);

            using (IDbCommand command = connection.CreateCommand()) {
                command.CommandText = @"
                    SELECT action_seq, state_json::text, created_at_utc
                    FROM replay_checkpoint
                    WHERE save_slot_id = @save_slot_id
                      AND action_seq <= @target_action_seq
                    ORDER BY action_seq DESC
                    LIMIT 1;";

                AddParameter(command, "@save_slot_id", saveSlotId);
                AddParameter(command, "@target_action_seq", targetActionSeq);

                using (IDataReader reader = command.ExecuteReader()) {
                    if (!reader.Read()) {
                        return null;
                    }

                    return new ReplayCheckpointRow {
                        ActionSeq = ReadInt32(reader, 0),
                        StateJson = ReadString(reader, 1),
                        CreatedAtUtc = ReadDateTime(reader, 2)
                    };
                }
            }
        }

        private static ActionType? TryMapActionType(string actionTypeText) {
            if (string.Equals(actionTypeText, "PlacePoint", StringComparison.OrdinalIgnoreCase)) {
                return ActionType.PlacePoint;
            }

            if (string.Equals(actionTypeText, "ShootCannon", StringComparison.OrdinalIgnoreCase)) {
                return ActionType.ShootCannon;
            }

            return null;
        }

        private static void ValidateConnection(IDbConnection connection) {
            if (connection == null) {
                throw new ArgumentNullException(nameof(connection));
            }

            if (connection.State != ConnectionState.Open) {
                throw new InvalidOperationException("Database connection must be open.");
            }
        }

        private static void AddParameter(IDbCommand command, string name, object value) {
            IDbDataParameter parameter = command.CreateParameter();
            parameter.ParameterName = name;
            parameter.Value = value ?? DBNull.Value;
            command.Parameters.Add(parameter);
        }

        private static int ReadInt32(IDataRecord reader, int ordinal) {
            return Convert.ToInt32(reader.GetValue(ordinal));
        }

        private static long ReadInt64(IDataRecord reader, int ordinal) {
            return Convert.ToInt64(reader.GetValue(ordinal));
        }

        private static bool ReadBoolean(IDataRecord reader, int ordinal) {
            return Convert.ToBoolean(reader.GetValue(ordinal));
        }

        private static DateTime ReadDateTime(IDataRecord reader, int ordinal) {
            return Convert.ToDateTime(reader.GetValue(ordinal));
        }

        private static string ReadString(IDataRecord reader, int ordinal) {
            return Convert.ToString(reader.GetValue(ordinal));
        }

        private static int? ReadNullableInt32(IDataRecord reader, int ordinal) {
            return reader.IsDBNull(ordinal) ? (int?)null : ReadInt32(reader, ordinal);
        }

        private static string ReadNullableString(IDataRecord reader, int ordinal) {
            return reader.IsDBNull(ordinal) ? null : ReadString(reader, ordinal);
        }
    }

    internal class MatchSessionRow {
        public long Id { get; set; }
        public string Name { get; set; }
        public string RulesHash { get; set; }
        public string RulesJson { get; set; }
        public DateTime CreatedAtUtc { get; set; }
        public DateTime UpdatedAtUtc { get; set; }
    }

    internal class SaveSlotRow {
        public long Id { get; set; }
        public long SessionId { get; set; }
        public string SlotName { get; set; }
        public int CheckpointInterval { get; set; }
        public int CurrentActionSeq { get; set; }
        public DateTime CreatedAtUtc { get; set; }
        public DateTime UpdatedAtUtc { get; set; }
    }

    internal class ReplayCheckpointRow {
        public int ActionSeq { get; set; }
        public string StateJson { get; set; }
        public DateTime CreatedAtUtc { get; set; }
    }
}
