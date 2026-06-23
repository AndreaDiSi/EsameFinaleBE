-- AutoConfig API – PostgreSQL schema
-- Table and column names match the EF Core entity model exactly (PascalCase).
-- The migration history entry at the bottom tells EF Core that InitialCreate
-- is already applied, so db.Database.MigrateAsync() becomes a no-op.
--
-- Run order: schema.sql → seed.sql
-- Or just let docker-compose mount both as init scripts (01/02 prefix).

\set ON_ERROR_STOP on

-- ── Extensions ────────────────────────────────────────────────────────────────
CREATE EXTENSION IF NOT EXISTS "pgcrypto";  -- needed by seed.sql for crypt()

-- ── Users ─────────────────────────────────────────────────────────────────────
CREATE TABLE "Users" (
    "Id"           UUID        NOT NULL DEFAULT gen_random_uuid(),
    "Email"        TEXT        NOT NULL,
    "Name"         TEXT        NOT NULL,
    "PasswordHash" TEXT        NOT NULL,
    "Role"         TEXT        NOT NULL DEFAULT 'User',
    "CreatedAt"    TIMESTAMPTZ NOT NULL DEFAULT now(),
    CONSTRAINT "PK_Users" PRIMARY KEY ("Id")
);
CREATE UNIQUE INDEX "IX_Users_Email" ON "Users" ("Email");

-- ── CarModels ─────────────────────────────────────────────────────────────────
CREATE TABLE "CarModels" (
    "Id"          UUID    NOT NULL DEFAULT gen_random_uuid(),
    "Name"        TEXT    NOT NULL,
    "Brand"       TEXT    NOT NULL,
    "Category"    TEXT    NOT NULL,   -- CarCategory enum: Sedan | Suv | Coupe | Hatchback | Wagon
    "BasePrice"   NUMERIC NOT NULL,
    "Description" TEXT    NOT NULL,
    "ImageColor"  TEXT    NOT NULL,
    CONSTRAINT "PK_CarModels" PRIMARY KEY ("Id")
);

-- ── Motorizations ─────────────────────────────────────────────────────────────
CREATE TABLE "Motorizations" (
    "Id"           UUID    NOT NULL DEFAULT gen_random_uuid(),
    "ModelId"      UUID    NOT NULL,
    "Name"         TEXT    NOT NULL,
    "FuelType"     TEXT    NOT NULL,  -- FuelType enum: Petrol | Diesel | Electric | Hybrid
    "Power"        INTEGER NOT NULL,
    "Torque"       INTEGER NOT NULL,
    "Acceleration" NUMERIC NOT NULL,
    "Consumption"  TEXT    NOT NULL,
    "Price"        NUMERIC NOT NULL,
    CONSTRAINT "PK_Motorizations" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_Motorizations_CarModels_ModelId"
        FOREIGN KEY ("ModelId") REFERENCES "CarModels" ("Id") ON DELETE CASCADE
);
CREATE INDEX "IX_Motorizations_ModelId" ON "Motorizations" ("ModelId");

-- ── CarOptions ────────────────────────────────────────────────────────────────
CREATE TABLE "CarOptions" (
    "Id"          UUID    NOT NULL DEFAULT gen_random_uuid(),
    "Name"        TEXT    NOT NULL,
    "Description" TEXT    NOT NULL,
    "Category"    TEXT    NOT NULL,  -- OptionCategory enum: Color | Interior | Technology | Safety | Comfort
    "Price"       NUMERIC NOT NULL,
    "Color"       TEXT,              -- nullable, only set for Color category
    CONSTRAINT "PK_CarOptions" PRIMARY KEY ("Id")
);

-- ── OptionIncompatibilities (self-referencing M2M on CarOptions) ──────────────
CREATE TABLE "OptionIncompatibilities" (
    "IncompatibleWithId"   UUID NOT NULL,
    "IncompatibleWithMeId" UUID NOT NULL,
    CONSTRAINT "PK_OptionIncompatibilities"
        PRIMARY KEY ("IncompatibleWithId", "IncompatibleWithMeId"),
    CONSTRAINT "FK_OptionIncomp_WithId"
        FOREIGN KEY ("IncompatibleWithId")   REFERENCES "CarOptions" ("Id") ON DELETE CASCADE,
    CONSTRAINT "FK_OptionIncomp_WithMeId"
        FOREIGN KEY ("IncompatibleWithMeId") REFERENCES "CarOptions" ("Id") ON DELETE CASCADE
);
CREATE INDEX "IX_OptionIncompatibilities_IncompatibleWithMeId"
    ON "OptionIncompatibilities" ("IncompatibleWithMeId");

-- ── OptionMotorizationRequirements (M2M: CarOptions ↔ Motorizations) ──────────
CREATE TABLE "OptionMotorizationRequirements" (
    "RequiredByOptionsId"     UUID NOT NULL,
    "RequiredMotorizationsId" UUID NOT NULL,
    CONSTRAINT "PK_OptionMotorizationRequirements"
        PRIMARY KEY ("RequiredByOptionsId", "RequiredMotorizationsId"),
    CONSTRAINT "FK_OptMotReq_Option"
        FOREIGN KEY ("RequiredByOptionsId")     REFERENCES "CarOptions"    ("Id") ON DELETE CASCADE,
    CONSTRAINT "FK_OptMotReq_Motorization"
        FOREIGN KEY ("RequiredMotorizationsId") REFERENCES "Motorizations" ("Id") ON DELETE CASCADE
);
CREATE INDEX "IX_OptionMotorizationRequirements_RequiredMotorizationsId"
    ON "OptionMotorizationRequirements" ("RequiredMotorizationsId");

-- ── Configurations ────────────────────────────────────────────────────────────
CREATE TABLE "Configurations" (
    "Id"             UUID        NOT NULL DEFAULT gen_random_uuid(),
    "UserId"         UUID        NOT NULL,
    "Name"           TEXT        NOT NULL,
    "ModelId"        UUID        NOT NULL,
    "MotorizationId" UUID        NOT NULL,
    "TotalPrice"     NUMERIC     NOT NULL,
    "CreatedAt"      TIMESTAMPTZ NOT NULL DEFAULT now(),
    "UpdatedAt"      TIMESTAMPTZ NOT NULL DEFAULT now(),
    CONSTRAINT "PK_Configurations" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_Configurations_Users_UserId"
        FOREIGN KEY ("UserId")         REFERENCES "Users"         ("Id") ON DELETE CASCADE,
    CONSTRAINT "FK_Configurations_CarModels_ModelId"
        FOREIGN KEY ("ModelId")        REFERENCES "CarModels"     ("Id") ON DELETE RESTRICT,
    CONSTRAINT "FK_Configurations_Motorizations_MotorizationId"
        FOREIGN KEY ("MotorizationId") REFERENCES "Motorizations" ("Id") ON DELETE RESTRICT
);
CREATE INDEX "IX_Configurations_UserId"         ON "Configurations" ("UserId");
CREATE INDEX "IX_Configurations_ModelId"        ON "Configurations" ("ModelId");
CREATE INDEX "IX_Configurations_MotorizationId" ON "Configurations" ("MotorizationId");

-- ── ConfigurationOptions (M2M: Configurations ↔ CarOptions) ──────────────────
CREATE TABLE "ConfigurationOptions" (
    "ConfigurationsId" UUID NOT NULL,
    "OptionsId"        UUID NOT NULL,
    CONSTRAINT "PK_ConfigurationOptions" PRIMARY KEY ("ConfigurationsId", "OptionsId"),
    CONSTRAINT "FK_ConfigurationOptions_Configurations_ConfigurationsId"
        FOREIGN KEY ("ConfigurationsId") REFERENCES "Configurations" ("Id") ON DELETE CASCADE,
    CONSTRAINT "FK_ConfigurationOptions_CarOptions_OptionsId"
        FOREIGN KEY ("OptionsId")        REFERENCES "CarOptions"     ("Id") ON DELETE CASCADE
);
CREATE INDEX "IX_ConfigurationOptions_OptionsId" ON "ConfigurationOptions" ("OptionsId");

-- ── Quotes ────────────────────────────────────────────────────────────────────
CREATE TABLE "Quotes" (
    "Id"              UUID        NOT NULL DEFAULT gen_random_uuid(),
    "ConfigurationId" UUID        NOT NULL,
    "UserId"          UUID        NOT NULL,
    "TotalPrice"      NUMERIC     NOT NULL,
    "Discount"        NUMERIC     NOT NULL,
    "FinalPrice"      NUMERIC     NOT NULL,
    "Status"          TEXT        NOT NULL DEFAULT 'Pending',  -- QuoteStatus enum: Pending | Approved | Rejected | Expired
    "Notes"           TEXT        NOT NULL DEFAULT '',
    "AdminNotes"      TEXT        NOT NULL DEFAULT '',
    "CreatedAt"       TIMESTAMPTZ NOT NULL DEFAULT now(),
    "UpdatedAt"       TIMESTAMPTZ NOT NULL DEFAULT now(),
    "ExpiresAt"       TIMESTAMPTZ NOT NULL,
    CONSTRAINT "PK_Quotes" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_Quotes_Configurations_ConfigurationId"
        FOREIGN KEY ("ConfigurationId") REFERENCES "Configurations" ("Id") ON DELETE CASCADE,
    CONSTRAINT "FK_Quotes_Users_UserId"
        FOREIGN KEY ("UserId")          REFERENCES "Users"          ("Id") ON DELETE RESTRICT
);
CREATE INDEX "IX_Quotes_ConfigurationId" ON "Quotes" ("ConfigurationId");
CREATE INDEX "IX_Quotes_UserId"          ON "Quotes" ("UserId");

-- ── EF Core migrations history ────────────────────────────────────────────────
-- Marks the SQLite InitialCreate migration as already applied so that
-- db.Database.MigrateAsync() in Program.cs does not attempt to recreate tables.
CREATE TABLE IF NOT EXISTS "__EFMigrationsHistory" (
    "MigrationId"    VARCHAR(150) NOT NULL,
    "ProductVersion" VARCHAR(32)  NOT NULL,
    CONSTRAINT "PK___EFMigrationsHistory" PRIMARY KEY ("MigrationId")
);
INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20260529082709_InitialCreate', '8.0.11');
