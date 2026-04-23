-- Migration: ClienteLoja enxuta
-- Salematic é o dono dos dados do cliente.
-- CMSX guarda apenas o vínculo: SalematicClienteId + AplicacaoId.

-- Remover colunas redundantes (dados pessoais pertencem ao Salematic)
ALTER TABLE clienteloja DROP COLUMN IF EXISTS Nome;
ALTER TABLE clienteloja DROP COLUMN IF EXISTS Documento;
ALTER TABLE clienteloja DROP COLUMN IF EXISTS Email;
ALTER TABLE clienteloja DROP COLUMN IF EXISTS Telefone;

-- Garantir NOT NULL nas colunas essenciais (caso já existam com NULL)
ALTER TABLE clienteloja ALTER COLUMN AplicacaoId NVARCHAR(64) NOT NULL;
ALTER TABLE clienteloja ALTER COLUMN SalematicClienteId INT NOT NULL;

-- Garantir DataInclusao com default
ALTER TABLE clienteloja ADD CONSTRAINT DF_clienteloja_DataInclusao
    DEFAULT GETDATE() FOR DataInclusao;

-- Índice único para evitar vínculos duplicados
IF NOT EXISTS (
    SELECT 1 FROM sys.indexes
    WHERE name = 'UX_clienteloja_Salematic_Aplicacao'
)
CREATE UNIQUE INDEX UX_clienteloja_Salematic_Aplicacao
    ON clienteloja(SalematicClienteId, AplicacaoId);

DROP INDEX IF EXISTS IX_clienteloja_Documento ON clienteloja