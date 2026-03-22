BEGIN;

CREATE TABLE IF NOT EXISTS match_session (
    id                  BIGSERIAL PRIMARY KEY,
    name                TEXT NOT NULL,
    rules_hash          TEXT NOT NULL,
    rules_json          JSONB NOT NULL,
    created_at_utc      TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    updated_at_utc      TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

CREATE TABLE IF NOT EXISTS save_slot (
    id                      BIGSERIAL PRIMARY KEY,
    session_id              BIGINT NOT NULL REFERENCES match_session(id) ON DELETE CASCADE,
    slot_name               TEXT NOT NULL,
    checkpoint_interval     INTEGER NOT NULL DEFAULT 25 CHECK (checkpoint_interval > 0),
    current_action_seq      INTEGER NOT NULL DEFAULT 0 CHECK (current_action_seq >= 0),
    created_at_utc          TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    updated_at_utc          TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    UNIQUE (session_id, slot_name)
);

CREATE TABLE IF NOT EXISTS game_action (
    id                  BIGSERIAL PRIMARY KEY,
    save_slot_id        BIGINT NOT NULL REFERENCES save_slot(id) ON DELETE CASCADE,
    seq                 INTEGER NOT NULL CHECK (seq > 0),

    player_id           INTEGER NOT NULL CHECK (player_id >= 0),
    action_type         TEXT NOT NULL CHECK (action_type IN ('PlacePoint', 'ShootCannon', 'MoveCannon', 'ReloadCannon')),

    row_index           INTEGER,
    col_index           INTEGER,
    point_index         INTEGER,
    target_index        INTEGER,
    power               INTEGER,

    is_successful       BOOLEAN NOT NULL,
    failure_reason      TEXT,
    notes               TEXT,

    timestamp_utc       TIMESTAMPTZ NOT NULL DEFAULT NOW(),

    UNIQUE (save_slot_id, seq)
);

CREATE TABLE IF NOT EXISTS replay_checkpoint (
    id                  BIGSERIAL PRIMARY KEY,
    save_slot_id        BIGINT NOT NULL REFERENCES save_slot(id) ON DELETE CASCADE,
    action_seq          INTEGER NOT NULL CHECK (action_seq >= 0),
    state_json          JSONB NOT NULL,
    created_at_utc      TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    UNIQUE (save_slot_id, action_seq)
);

-- Performance indexes for timeline/replay operations.
CREATE INDEX IF NOT EXISTS idx_save_slot_session
    ON save_slot (session_id);

CREATE INDEX IF NOT EXISTS idx_game_action_slot_seq
    ON game_action (save_slot_id, seq);

CREATE INDEX IF NOT EXISTS idx_game_action_slot_time
    ON game_action (save_slot_id, timestamp_utc);

CREATE INDEX IF NOT EXISTS idx_replay_checkpoint_slot_seq
    ON replay_checkpoint (save_slot_id, action_seq);

-- Helper function to update updated_at_utc automatically.
CREATE OR REPLACE FUNCTION set_updated_at_utc()
RETURNS TRIGGER AS $$
BEGIN
    NEW.updated_at_utc = NOW();
    RETURN NEW;
END;
$$ LANGUAGE plpgsql;

DROP TRIGGER IF EXISTS trg_match_session_updated_at ON match_session;
CREATE TRIGGER trg_match_session_updated_at
BEFORE UPDATE ON match_session
FOR EACH ROW
EXECUTE FUNCTION set_updated_at_utc();

DROP TRIGGER IF EXISTS trg_save_slot_updated_at ON save_slot;
CREATE TRIGGER trg_save_slot_updated_at
BEFORE UPDATE ON save_slot
FOR EACH ROW
EXECUTE FUNCTION set_updated_at_utc();

COMMIT;
