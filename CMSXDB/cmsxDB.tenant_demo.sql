-- ---------------------------------------------------------------
-- CMSX — Perfil do tenant + Demo tenant
-- Execute no banco cmsx-db após cmsxDB.pagebuilder.sql
-- ---------------------------------------------------------------


-- 1. Novos campos de perfil em aplicacao
IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME='aplicacao' AND COLUMN_NAME='telefone')
    ALTER TABLE [dbo].[aplicacao] ADD [telefone] NVARCHAR(50) NULL;
GO
IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME='aplicacao' AND COLUMN_NAME='endereco')
    ALTER TABLE [dbo].[aplicacao] ADD [endereco] NVARCHAR(500) NULL;
GO
IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME='aplicacao' AND COLUMN_NAME='descricao')
    ALTER TABLE [dbo].[aplicacao] ADD [descricao] NVARCHAR(MAX) NULL;
GO
IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME='aplicacao' AND COLUMN_NAME='isdemo')
    ALTER TABLE [dbo].[aplicacao] ADD [isdemo] BIT NOT NULL DEFAULT 0;
GO

-- 2. Usuário demo (login: demo / senha: demo)
IF NOT EXISTS (SELECT 1 FROM [dbo].[usuario] WHERE [apelido] = 'demo')
BEGIN
    DECLARE @userId   NVARCHAR(36) = NEWID();
    DECLARE @appId    NVARCHAR(36) = NEWID();
    DECLARE @relId    NVARCHAR(36) = NEWID();
    DECLARE @areaId   NVARCHAR(36) = NEWID();

    INSERT INTO [dbo].[usuario]
        ([userid],[nome],[sobrenome],[apelido],[senha],[ativo],[datainclusao])
    VALUES
        (@userId, 'Demo', 'CMSX', 'demo', 'demo', 1, GETDATE());

    INSERT INTO [dbo].[aplicacao]
        ([aplicacaoid],[nome],[url],[idusuarioinicio],[datainicio],[isactive],[layoutchoose],[isdemo])
    VALUES
        (@appId, 'Demo CMSX', 'demo', @userId, GETDATE(), 1, '_Layout.cshtml', 1);

    INSERT INTO [dbo].[relusuarioaplicacao]
        ([relacaoid],[usuarioid],[aplicacaoid])
    VALUES
        (@relId, @userId, @appId);

    INSERT INTO [dbo].[areas]
        ([areaid],[aplicacaoid],[nome],[url],[posicao],[layout])
    VALUES
        (@areaId, @appId, 'Home', 'home', 1, '{"blocos":[]}');
END
GO
