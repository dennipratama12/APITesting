-- ============================================================
-- USER MANAGEMENT — Main DB
-- ============================================================
-- Jalankan di database: test.main
-- ============================================================


-- ============================================================
-- Table: public.users
-- ============================================================
CREATE TABLE IF NOT EXISTS public.users
(
    id           UUID         NOT NULL DEFAULT gen_random_uuid(),
    username     VARCHAR(100) NOT NULL,
    email        VARCHAR(255) NOT NULL,
    full_name    VARCHAR(200) NOT NULL,
    phone_number BIGINT,
    role         VARCHAR(50)  NOT NULL DEFAULT 'user',
    is_active    BOOLEAN      NOT NULL DEFAULT true,
    created_at   TIMESTAMPTZ  NOT NULL DEFAULT NOW(),
    updated_at   TIMESTAMPTZ,
    created_by   UUID,
    updated_by   UUID,
    CONSTRAINT pk_users          PRIMARY KEY (id),
    CONSTRAINT uq_users_username UNIQUE (username),
    CONSTRAINT uq_users_email    UNIQUE (email)
);

CREATE INDEX IF NOT EXISTS idx_users_role       ON public.users (role);
CREATE INDEX IF NOT EXISTS idx_users_is_active  ON public.users (is_active);
CREATE INDEX IF NOT EXISTS idx_users_created_at ON public.users (created_at DESC);


-- ============================================================
-- Function: public.fn_user_get_by_id
-- ============================================================
CREATE OR REPLACE FUNCTION public.fn_user_get_by_id(p_id UUID)
RETURNS TABLE
(
    id           UUID,
    username     VARCHAR,
    email        VARCHAR,
    full_name    VARCHAR,
    phone_number BIGINT,
    role         VARCHAR,
    is_active    BOOLEAN,
    created_at   TIMESTAMPTZ,
    updated_at   TIMESTAMPTZ
)
LANGUAGE plpgsql AS
$$
BEGIN
    RETURN QUERY
        SELECT u.id, u.username, u.email, u.full_name,
               u.phone_number, u.role, u.is_active,
               u.created_at, u.updated_at
        FROM public.users u
        WHERE u.id = p_id;
END;
$$;


-- ============================================================
-- Function: public.fn_user_get_list
-- ============================================================
CREATE OR REPLACE FUNCTION public.fn_user_get_list(
    p_keyword   TEXT,
    p_role      VARCHAR,
    p_is_active BOOLEAN,
    p_limit     INT,
    p_offset    INT
)
RETURNS TABLE
(
    id           UUID,
    username     VARCHAR,
    email        VARCHAR,
    full_name    VARCHAR,
    phone_number BIGINT,
    role         VARCHAR,
    is_active    BOOLEAN,
    created_at   TIMESTAMPTZ,
    updated_at   TIMESTAMPTZ
)
LANGUAGE plpgsql AS
$$
BEGIN
    RETURN QUERY
        SELECT u.id, u.username, u.email, u.full_name,
               u.phone_number, u.role, u.is_active,
               u.created_at, u.updated_at
        FROM public.users u
        WHERE (p_keyword   IS NULL OR (u.full_name ILIKE '%' || p_keyword || '%'
                                    OR u.username  ILIKE '%' || p_keyword || '%'
                                    OR u.email     ILIKE '%' || p_keyword || '%'))
          AND (p_role      IS NULL OR u.role      = p_role)
          AND (p_is_active IS NULL OR u.is_active = p_is_active)
        ORDER BY u.created_at DESC
        LIMIT p_limit OFFSET p_offset;
END;
$$;


-- ============================================================
-- Function: public.fn_user_count
-- ============================================================
CREATE OR REPLACE FUNCTION public.fn_user_count(
    p_keyword   TEXT,
    p_role      VARCHAR,
    p_is_active BOOLEAN
)
RETURNS INT
LANGUAGE plpgsql AS
$$
DECLARE
    v_count INT;
BEGIN
    SELECT COUNT(*)
    INTO v_count
    FROM public.users u
    WHERE (p_keyword   IS NULL OR (u.full_name ILIKE '%' || p_keyword || '%'
                                OR u.username  ILIKE '%' || p_keyword || '%'
                                OR u.email     ILIKE '%' || p_keyword || '%'))
      AND (p_role      IS NULL OR u.role      = p_role)
      AND (p_is_active IS NULL OR u.is_active = p_is_active);

    RETURN v_count;
END;
$$;


-- ============================================================
-- Function: public.fn_user_create
-- ============================================================
CREATE OR REPLACE FUNCTION public.fn_user_create(
    p_username     VARCHAR,
    p_email        VARCHAR,
    p_full_name    VARCHAR,
    p_phone_number BIGINT,
    p_role         VARCHAR
)
RETURNS TABLE
(
    id           UUID,
    username     VARCHAR,
    email        VARCHAR,
    full_name    VARCHAR,
    phone_number BIGINT,
    role         VARCHAR,
    is_active    BOOLEAN,
    created_at   TIMESTAMPTZ,
    updated_at   TIMESTAMPTZ
)
LANGUAGE plpgsql AS
$$
DECLARE
    v_id UUID := gen_random_uuid();
BEGIN
    INSERT INTO public.users (id, username, email, full_name, phone_number, role)
    VALUES (v_id, p_username, p_email, p_full_name, p_phone_number, p_role);

    RETURN QUERY
        SELECT u.id, u.username, u.email, u.full_name,
               u.phone_number, u.role, u.is_active,
               u.created_at, u.updated_at
        FROM public.users u
        WHERE u.id = v_id;
END;
$$;


-- ============================================================
-- Function: public.fn_user_update
-- ============================================================
CREATE OR REPLACE FUNCTION public.fn_user_update(
    p_id           UUID,
    p_email        VARCHAR,
    p_full_name    VARCHAR,
    p_phone_number BIGINT,
    p_role         VARCHAR,
    p_is_active    BOOLEAN
)
RETURNS TABLE
(
    id           UUID,
    username     VARCHAR,
    email        VARCHAR,
    full_name    VARCHAR,
    phone_number BIGINT,
    role         VARCHAR,
    is_active    BOOLEAN,
    created_at   TIMESTAMPTZ,
    updated_at   TIMESTAMPTZ
)
LANGUAGE plpgsql AS
$$
BEGIN
    UPDATE public.users AS u
    SET email        = p_email,
        full_name    = p_full_name,
        phone_number = p_phone_number,
        role         = p_role,
        is_active    = p_is_active,
        updated_at   = NOW()
    WHERE u.id = p_id;

    IF NOT FOUND THEN
        RAISE EXCEPTION 'User dengan id % tidak ditemukan.', p_id;
    END IF;

    RETURN QUERY
        SELECT u.id, u.username, u.email, u.full_name,
               u.phone_number, u.role, u.is_active,
               u.created_at, u.updated_at
        FROM public.users u
        WHERE u.id = p_id;
END;
$$;


-- ============================================================
-- Function: public.fn_user_delete  (soft delete)
-- ============================================================
CREATE OR REPLACE FUNCTION public.fn_user_delete(p_id UUID)
RETURNS BOOLEAN
LANGUAGE plpgsql AS
$$
DECLARE
    v_affected INT;
BEGIN
    UPDATE public.users
    SET is_active  = false,
        updated_at = NOW()
    WHERE id = p_id AND is_active = true;

    GET DIAGNOSTICS v_affected = ROW_COUNT;
    RETURN v_affected > 0;
END;
$$;
