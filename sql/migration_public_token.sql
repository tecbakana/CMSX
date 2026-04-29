-- Fase 1 — Token opaco para links públicos de tenant
-- Executar no banco do CMSX (SQL Server)

CREATE TABLE publictoken (
    publictokenid  UNIQUEIDENTIFIER NOT NULL DEFAULT NEWID() CONSTRAINT PK_publictoken PRIMARY KEY,
    token          NVARCHAR(100)    NOT NULL,
    aplicacaoid    NVARCHAR(64)     NOT NULL,
    ativo          BIT              NOT NULL DEFAULT 1,
    datainclusao   DATETIME2        NOT NULL DEFAULT GETUTCDATE(),
    datavencimento DATETIME2        NULL,
    CONSTRAINT UQ_publictoken_token UNIQUE (token)
);

CREATE INDEX IX_publictoken_aplicacaoid ON publictoken (aplicacaoid);
