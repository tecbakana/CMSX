-- Adiciona coluna UnidadeVenda na tabela produto
-- Executar no banco do CMSX (SQL Server)

ALTER TABLE produto
    ADD UnidadeVenda NVARCHAR(45) NULL;
