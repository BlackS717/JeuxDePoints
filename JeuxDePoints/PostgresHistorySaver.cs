using System;
using System.Collections.Generic;
using System.Data;

namespace JeuxDePoints {
    internal class PostgresHistorySaver {
        public long CreateSession(IDbConnection connection, string name, string rulesHash, string rulesJson, IDbTransaction transaction = null) {
            ValidateConnection(connection);

            using (IDbCommand command = connection.CreateCommand()) {
                command.Transaction = transaction;
                command.CommandText = @"
                    INSERT INTO match_session (name, rules_hash, rules_json)
                    VALUES (@name, @rules_hash, CAST(@rules_json AS jsonb))
                    RETURNING id;";

                AddParameter(command, "@name", name);
                AddParameter(command, "@rules_hash", rulesHash);
                AddParameter(command, "@rules_json", rulesJson);

                return Convert.ToInt64(command.ExecuteScalar());
            }
        }

        public long UpsertSaveSlot(
            IDbConnection connection,
            long sessionId,
            string slotName,
            int checkpointInterval,
            int currentActionSeq,
            IDbTransaction transaction = null) {
            ValidateConnection(connection);

            using (IDbCommand command = connection.CreateCommand()) {
                command.Transaction = transaction;
                command.CommandText = @"
                    INSERT INTO save_slot (session_id, slot_name, checkpoint_interval, current_action_seq)
                    VALUES (@session_id, @slot_name, @checkpoint_interval, @current_action_seq)
                    ON CONFLICT (session_id, slot_name)
                    DO UPDATE SET
                        checkpoint_interval = EXCLUDED.checkpoint_interval,
                        current_action_seq = EXCLUDED.current_action_seq
                    RETURNING id;";

                AddParameter(command, "@session_id", sessionId);
                AddParameter(command, "@slot_name", slotName);
                AddParameter(command, "@checkpoint_interval", checkpointInterval);
                AddParameter(command, "@current_action_seq", currentActionSeq);

                return Convert.ToInt64(command.ExecuteScalar());
            }
        }

        public void SaveAction(IDbConnection connection, long saveSlotId, int seq, Move move, IDbTransaction transaction = null) {
            if (move == null) {
                throw new ArgumentNullException(nameof(move));
            }

            ValidateConnection(connection);

            using (IDbCommand command = connection.CreateCommand()) {
                command.Transaction = transaction;
                command.CommandText = @"
                    INSERT INTO game_action (
                        save_slot_id,
                        seq,
                        player_id,
                        action_type,
                        row_index,
                        col_index,
                        point_index,
                        target_index,
                        power,
                        is_successful,
                        notes,
                        timestamp_utc
                    )
                    VALUES (
                        @save_slot_id,
                        @seq,
                        @player_id,
                        @action_type,
                        @row_index,
                        @col_index,
                        @point_index,
                        @target_index,
                        @power,
                        @is_successful,
                        @notes,
                        @timestamp_utc
                    );";

                AddParameter(command, "@save_slot_id", saveSlotId);
                AddParameter(command, "@seq", seq);
                AddParameter(command, "@player_id", move.PlayerId);
                AddParameter(command, "@action_type", move.ActionType.ToString());
                AddParameter(command, "@row_index", move.Row.HasValue ? (object)move.Row.Value : DBNull.Value);
                AddParameter(command, "@col_index", move.Col.HasValue ? (object)move.Col.Value : DBNull.Value);
                AddParameter(command, "@point_index", move.PointIndex.HasValue ? (object)move.PointIndex.Value : DBNull.Value);
                AddParameter(command, "@target_index", move.TargetIndex.HasValue ? (object)move.TargetIndex.Value : DBNull.Value);
                AddParameter(command, "@power", move.Power.HasValue ? (object)move.Power.Value : DBNull.Value);
                AddParameter(command, "@is_successful", move.IsSuccessful);
                AddParameter(command, "@notes", string.IsNullOrEmpty(move.Notes) ? (object)DBNull.Value : move.Notes);
                AddParameter(command, "@timestamp_utc", move.TimestampUtc);

                command.ExecuteNonQuery();
            }
        }

        public void SaveCheckpoint(IDbConnection connection, long saveSlotId, int actionSeq, string stateJson, IDbTransaction transaction = null) {
            ValidateConnection(connection);

            using (IDbCommand command = connection.CreateCommand()) {
                command.Transaction = transaction;
                command.CommandText = @"
                    INSERT INTO replay_checkpoint (save_slot_id, action_seq, state_json)
                    VALUES (@save_slot_id, @action_seq, CAST(@state_json AS jsonb))
                    ON CONFLICT (save_slot_id, action_seq)
                    DO UPDATE SET state_json = EXCLUDED.state_json;";

                AddParameter(command, "@save_slot_id", saveSlotId);
                AddParameter(command, "@action_seq", actionSeq);
                AddParameter(command, "@state_json", stateJson);

                command.ExecuteNonQuery();
            }
        }

        public void UpdateCurrentActionSeq(IDbConnection connection, long saveSlotId, int currentActionSeq, IDbTransaction transaction = null) {
            ValidateConnection(connection);

            using (IDbCommand command = connection.CreateCommand()) {
                command.Transaction = transaction;
                command.CommandText = @"
                    UPDATE save_slot
                    SET current_action_seq = @current_action_seq
                    WHERE id = @save_slot_id;";

                AddParameter(command, "@current_action_seq", currentActionSeq);
                AddParameter(command, "@save_slot_id", saveSlotId);

                command.ExecuteNonQuery();
            }
        }

        public void TruncateBranch(IDbConnection connection, long saveSlotId, int currentActionSeq, IDbTransaction transaction = null) {
            ValidateConnection(connection);

            using (IDbCommand command = connection.CreateCommand()) {
                command.Transaction = transaction;
                command.CommandText = @"
                    DELETE FROM replay_checkpoint
                    WHERE save_slot_id = @save_slot_id
                      AND action_seq > @current_action_seq;";

                AddParameter(command, "@save_slot_id", saveSlotId);
                AddParameter(command, "@current_action_seq", currentActionSeq);
                command.ExecuteNonQuery();
            }

            using (IDbCommand command = connection.CreateCommand()) {
                command.Transaction = transaction;
                command.CommandText = @"
                    DELETE FROM game_action
                    WHERE save_slot_id = @save_slot_id
                      AND seq > @current_action_seq;";

                AddParameter(command, "@save_slot_id", saveSlotId);
                AddParameter(command, "@current_action_seq", currentActionSeq);
                command.ExecuteNonQuery();
            }
        }

        public bool DeleteSaveSlot(IDbConnection connection, long saveSlotId, IDbTransaction transaction = null) {
            ValidateConnection(connection);

            int affectedRows;
            using (IDbCommand command = connection.CreateCommand()) {
                command.Transaction = transaction;
                command.CommandText = @"
                    DELETE FROM replay_checkpoint
                    WHERE save_slot_id = @save_slot_id;";

                AddParameter(command, "@save_slot_id", saveSlotId);
                command.ExecuteNonQuery();
            }

            using (IDbCommand command = connection.CreateCommand()) {
                command.Transaction = transaction;
                command.CommandText = @"
                    DELETE FROM game_action
                    WHERE save_slot_id = @save_slot_id;";

                AddParameter(command, "@save_slot_id", saveSlotId);
                command.ExecuteNonQuery();
            }

            using (IDbCommand command = connection.CreateCommand()) {
                command.Transaction = transaction;
                command.CommandText = @"
                    DELETE FROM save_slot
                    WHERE id = @save_slot_id;";

                AddParameter(command, "@save_slot_id", saveSlotId);
                affectedRows = command.ExecuteNonQuery();
            }

            return affectedRows > 0;
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
    }
}
