-- CMSX — Setup completo do banco
-- Executar no banco SQL Server do CMSX
-- Ordem: 1) UnidadeVenda  2) Orçamento  3) PublicToken

-- ============================================================
-- 1. Coluna UnidadeVenda na tabela produto
-- ============================================================
IF NOT EXISTS (
    SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS
    WHERE TABLE_NAME = 'produto' AND COLUMN_NAME = 'UnidadeVenda'
)
BEGIN
    ALTER TABLE produto ADD UnidadeVenda NVARCHAR(45) NULL;
END

-- ============================================================
-- 2. Módulo de Orçamentos
-- ============================================================
IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'orcamentocabecalho')
BEGIN
    CREATE TABLE orcamentocabecalho (
        orcamentoid      UNIQUEIDENTIFIER NOT NULL DEFAULT NEWID()  CONSTRAINT PK_orcamentocabecalho PRIMARY KEY,
        aplicacaoid      NVARCHAR(64)     NOT NULL,
        nome             NVARCHAR(200)    NOT NULL,
        email            NVARCHAR(200)    NULL,
        telefone         NVARCHAR(50)     NULL,
        descricaoservico NVARCHAR(MAX)    NULL,
        valorestimado    DECIMAL(12,2)    NULL,
        prazo            NVARCHAR(200)    NULL,
        nomevendedor     NVARCHAR(200)    NULL,
        aprovado         BIT              NOT NULL DEFAULT 0,
        datainclusao     DATETIME2        NOT NULL DEFAULT GETUTCDATE()
    );

    CREATE INDEX IX_orcamentocabecalho_aplicacaoid ON orcamentocabecalho (aplicacaoid);
END

IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'orcamentodetalhe')
BEGIN
    CREATE TABLE orcamentodetalhe (
        orcamentodetalheid UNIQUEIDENTIFIER NOT NULL DEFAULT NEWID() CONSTRAINT PK_orcamentodetalhe PRIMARY KEY,
        orcamentoid        UNIQUEIDENTIFIER NOT NULL,
        descricao          NVARCHAR(500)    NOT NULL,
        quantidade         DECIMAL(10,2)    NOT NULL DEFAULT 1,
        valor              DECIMAL(12,2)    NULL,
        ativo              BIT              NOT NULL DEFAULT 1,
        CONSTRAINT FK_orcamentodetalhe_orcamentoid
            FOREIGN KEY (orcamentoid) REFERENCES orcamentocabecalho(orcamentoid)
    );

    CREATE INDEX IX_orcamentodetalhe_orcamentoid ON orcamentodetalhe (orcamentoid);
END

-- ============================================================
-- 3. Token opaco para links públicos
-- ============================================================
IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'publictoken')
BEGIN
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
END
