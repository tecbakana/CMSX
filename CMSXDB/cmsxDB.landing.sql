-- ---------------------------------------------------------------
-- Multiplai — Aplicacao plataforma (landing page pública)
-- Execute no banco cmsx-db após cmsxDB.tenant_demo.sql
-- ---------------------------------------------------------------


IF NOT EXISTS (SELECT 1 FROM [dbo].[aplicacao] WHERE [url] = 'multiplai')
BEGIN
    DECLARE @appId NVARCHAR(36) = NEWID();

    INSERT INTO [dbo].[aplicacao]
        ([aplicacaoid],[nome],[url],[datainicio],[isactive],[layoutchoose],[isdemo])
    VALUES
        (@appId, 'Multiplai', 'multiplai', GETDATE(), 1, '_Layout.cshtml', 0);

    INSERT INTO [dbo].[areas]
        ([areaid],[aplicacaoid],[nome],[url],[posicao],[layout])
    VALUES
        (NEWID(), @appId, 'Home', 'home', 1, '{"blocos":[]}');
END
GO
