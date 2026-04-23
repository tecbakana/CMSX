-- --------------------------------------------------
-- CMSX Page Builder - Novas tabelas e seed
-- Execute no banco cmsxDB após criar a estrutura principal
-- --------------------------------------------------

USE [cmsxDB];
GO

-- --------------------------------------------------
-- 1. Tabela dict_blocos (catálogo de tipos de bloco)
-- --------------------------------------------------

IF OBJECT_ID(N'[dbo].[dict_blocos]', 'U') IS NOT NULL DROP TABLE [dbo].[dict_blocos];
GO

CREATE TABLE [dbo].[dict_blocos] (
    [tipobloco]    NVARCHAR(50)   NOT NULL PRIMARY KEY,
    [nome]         NVARCHAR(100)  NOT NULL,
    [descricao]    NVARCHAR(500)  NULL,
    [icone]        NVARCHAR(50)   NULL,
    [schema_config] NVARCHAR(MAX) NOT NULL DEFAULT '{}'
);
GO

-- --------------------------------------------------
-- 2. Campo Layout na tabela areas (se não existir)
-- --------------------------------------------------

IF NOT EXISTS (
    SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS
    WHERE TABLE_NAME = 'areas' AND COLUMN_NAME = 'layout'
)
BEGIN
    ALTER TABLE [dbo].[areas] ADD [layout] NVARCHAR(MAX) NULL;
END
GO

-- --------------------------------------------------
-- 3. Seed: dict_blocos
-- --------------------------------------------------

DELETE FROM [dbo].[dict_blocos];
GO

INSERT INTO [dbo].[dict_blocos] ([tipobloco], [nome], [descricao], [icone], [schema_config]) VALUES
(
    'hero',
    'Hero Banner',
    'Bloco de destaque principal com título, subtítulo e chamada para ação.',
    'bi-image',
    '{
        "titulo": {"type":"string","label":"Título","default":"Bem-vindo"},
        "subtitulo": {"type":"string","label":"Subtítulo","default":""},
        "textoBotao": {"type":"string","label":"Texto do botão","default":"Saiba mais"},
        "linkBotao": {"type":"string","label":"Link do botão","default":"/"},
        "imagemFundo": {"type":"string","label":"URL da imagem de fundo","default":""}
    }'
),
(
    'texto',
    'Bloco de Texto',
    'Parágrafo ou seção de texto rico com título opcional.',
    'bi-text-paragraph',
    '{
        "titulo": {"type":"string","label":"Título","default":""},
        "conteudo": {"type":"text","label":"Conteúdo (HTML permitido)","default":""}
    }'
),
(
    'lista-conteudos',
    'Lista de Conteúdos',
    'Exibe uma lista de conteúdos vinculados a uma área específica.',
    'bi-list-ul',
    '{
        "areaid": {"type":"string","label":"ID da Área","default":""},
        "titulo": {"type":"string","label":"Título da seção","default":"Últimas Notícias"},
        "limite": {"type":"number","label":"Quantidade máxima","default":6}
    }'
),
(
    'lista-produtos',
    'Lista de Produtos',
    'Exibe produtos em grade com imagem, nome e preço.',
    'bi-grid',
    '{
        "cateriaid": {"type":"string","label":"ID da Categoria (filtro)","default":""},
        "titulo": {"type":"string","label":"Título da seção","default":"Nossos Produtos"},
        "limite": {"type":"number","label":"Quantidade máxima","default":8}
    }'
),
(
    'banner-imagem',
    'Banner de Imagem',
    'Imagem larga com link opcional.',
    'bi-card-image',
    '{
        "imagemUrl": {"type":"string","label":"URL da imagem","default":""},
        "altText": {"type":"string","label":"Texto alternativo","default":""},
        "linkUrl": {"type":"string","label":"Link ao clicar","default":""},
        "largura": {"type":"string","label":"Largura (full, container)","default":"full"}
    }'
),
(
    'formulario',
    'Formulário',
    'Incorpora um formulário dinâmico cadastrado no sistema.',
    'bi-ui-checks',
    '{
        "formularioid": {"type":"string","label":"ID do Formulário","default":""},
        "titulo": {"type":"string","label":"Título exibido","default":"Fale Conosco"}
    }'
),
(
    'faq',
    'FAQ / Wiki',
    'Exibe perguntas e respostas de uma categoria específica.',
    'bi-question-circle',
    '{
        "cateriaid": {"type":"string","label":"ID da Categoria","default":""},
        "titulo": {"type":"string","label":"Título da seção","default":"Perguntas Frequentes"},
        "exibirAcordeon": {"type":"boolean","label":"Exibir como acordeão","default":true}
    }'
),
(
    'categorias',
    'Grade de Categorias',
    'Exibe categorias em formato de cards para navegação.',
    'bi-folder2-open',
    '{
        "cateriaidpai": {"type":"string","label":"ID da Categoria pai (deixe vazio para raiz)","default":""},
        "titulo": {"type":"string","label":"Título da seção","default":"Categorias"},
        "colunas": {"type":"number","label":"Colunas","default":3}
    }'
),
(
    'destaques',
    'Bloco de Destaques',
    'Cards de destaque com ícone, título e descrição curta (ex: benefícios, diferenciais).',
    'bi-star',
    '{
        "titulo": {"type":"string","label":"Título da seção","default":"Por que nos escolher?"},
        "itens": {"type":"array","label":"Lista de itens","default":[
            {"icone":"bi-check-circle","titulo":"Item 1","descricao":"Descrição do item"},
            {"icone":"bi-check-circle","titulo":"Item 2","descricao":"Descrição do item"}
        ]}
    }'
),
(
    'separador',
    'Separador',
    'Linha horizontal ou espaço em branco para separar seções.',
    'bi-dash-lg',
    '{
        "tipo": {"type":"string","label":"Tipo (linha, espaco)","default":"linha"},
        "altura": {"type":"number","label":"Altura em pixels (para espaco)","default":40}
    }'
);
GO

-- --------------------------------------------------
-- 4. Módulo Page Builder no menu
-- --------------------------------------------------

IF NOT EXISTS (SELECT 1 FROM [dbo].[modulo] WHERE ModuloId = 'MOD-009')
BEGIN
    INSERT INTO [dbo].[modulo] (ModuloId, Nome, Url, Posicao)
    VALUES ('MOD-009', 'Page Builder', '/page-builder', 9);
END
GO

IF NOT EXISTS (SELECT 1 FROM [dbo].[modulo] WHERE ModuloId = 'MOD-010')
BEGIN
    INSERT INTO [dbo].[modulo] (ModuloId, Nome, Url, Posicao)
    VALUES ('MOD-010', 'Page Builder v2', '/page-builder-v2', 10);
END
GO

-- --------------------------------------------------
-- Verificação
-- --------------------------------------------------

SELECT tipobloco, nome FROM [dbo].[dict_blocos] ORDER BY tipobloco;
GO
