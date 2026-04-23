-- Page Builder v2 — Migration
-- Adiciona coluna PageBuilderVersion na tabela areas (default 'v1')

USE [cmsxDB];
GO

IF NOT EXISTS (
    SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS
    WHERE TABLE_NAME = 'areas' AND COLUMN_NAME = 'PageBuilderVersion'
)
BEGIN
    ALTER TABLE [dbo].[areas]
        ADD [PageBuilderVersion] NVARCHAR(10) NOT NULL CONSTRAINT DF_areas_PageBuilderVersion DEFAULT 'v1';
END
GO
