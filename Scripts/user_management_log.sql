-- ============================================================
-- USER MANAGEMENT — Log DB
-- ============================================================
-- Jalankan di database: test.log
-- ============================================================


-- ============================================================
-- Schema: log
-- ============================================================
CREATE SCHEMA IF NOT EXISTS log;


-- ============================================================
-- Table: log.user_action_logs
-- ============================================================
CREATE TABLE IF NOT EXISTS log.user_action_logs
(
    id           UUID         NOT NULL DEFAULT gen_random_uuid(),
    user_id      UUID,
    action       VARCHAR(50)  NOT NULL,
    request_data JSONB,
    ip_address   VARCHAR(50),
    trace_id     VARCHAR(100),
    created_at   TIMESTAMPTZ  NOT NULL DEFAULT NOW(),
    CONSTRAINT pk_user_action_logs PRIMARY KEY (id)
);

CREATE INDEX IF NOT EXISTS idx_ual_user_id    ON log.user_action_logs (user_id);
CREATE INDEX IF NOT EXISTS idx_ual_action     ON log.user_action_logs (action);
CREATE INDEX IF NOT EXISTS idx_ual_created_at ON log.user_action_logs (created_at DESC);


-- ============================================================
-- Procedure: log.sp_user_action_log_insert
-- ============================================================
CREATE OR REPLACE PROCEDURE log.sp_user_action_log_insert(
    p_user_id      UUID,
    p_action       VARCHAR,
    p_request_data JSONB,
    p_ip_address   VARCHAR,
    p_trace_id     VARCHAR
)
LANGUAGE plpgsql AS
$$
BEGIN
    INSERT INTO log.user_action_logs (user_id, action, request_data, ip_address, trace_id)
    VALUES (p_user_id, p_action, p_request_data, p_ip_address, p_trace_id);
END;
$$;
