-- Módulo de Orçamentos — criação das tabelas
-- Executar no banco do CMSX (SQL Server)

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

CREATE INDEX IX_orcamentocabecalho_aplicacaoid ON orcamentocabecalho (aplicacaoid);
CREATE INDEX IX_orcamentodetalhe_orcamentoid   ON orcamentodetalhe   (orcamentoid);
