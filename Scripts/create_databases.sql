-- ============================================================
-- CREATE DATABASES
-- ============================================================
-- Jalankan script ini dari koneksi ke database 'postgres'
-- sebagai superuser (postgres) SEBELUM menjalankan
-- user_management.sql
--
-- Jika database sudah ada, PostgreSQL akan throw error
-- "database already exists" — bisa diabaikan.
-- ============================================================

CREATE DATABASE "test.main"
    OWNER = postgres
    ENCODING = 'UTF8'
    LC_COLLATE = 'en_US.UTF-8'
    LC_CTYPE = 'en_US.UTF-8'
    TEMPLATE = template0;

CREATE DATABASE "test.log"
    OWNER = postgres
    ENCODING = 'UTF8'
    LC_COLLATE = 'en_US.UTF-8'
    LC_CTYPE = 'en_US.UTF-8'
    TEMPLATE = template0;
