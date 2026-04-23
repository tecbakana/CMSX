ALTER TABLE produto ADD SalematicProdutoId INT NULL;

CREATE TABLE clienteloja (
    ClienteLojaId      UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    AplicacaoId        NVARCHAR(64) NOT NULL,
    SalematicClienteId INT NOT NULL,
    Nome               NVARCHAR(200),
    Documento          NVARCHAR(20),
    Email              NVARCHAR(200),
    Telefone           NVARCHAR(20),
    DataInclusao       DATETIME2
);

CREATE INDEX IX_clienteloja_AplicacaoId ON clienteloja(AplicacaoId);
CREATE INDEX IX_clienteloja_Documento   ON clienteloja(AplicacaoId, Documento);