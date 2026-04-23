-- ==============================================================
-- CMSX Database - PostgreSQL
-- Script unificado: estrutura + grupos + page builder + IA + seed
-- Compatível com EF Core Npgsql (nomes de coluna preservados)
-- ==============================================================

-- ==============================================================
-- DROP (ordem inversa de dependência)
-- ==============================================================

DROP TABLE IF EXISTS ia_uso;
DROP TABLE IF EXISTS ia_cache;
DROP TABLE IF EXISTS ia_config;
DROP TABLE IF EXISTS faq;
DROP TABLE IF EXISTS dict_blocos;
DROP TABLE IF EXISTS relusuariogrupo;
DROP TABLE IF EXISTS grupo;
DROP TABLE IF EXISTS relmoduloconfaplicacao;
DROP TABLE IF EXISTS relmoduloaplicacao;
DROP TABLE IF EXISTS relmodulousuario;
DROP TABLE IF EXISTS relusuarioaplicacao;
DROP TABLE IF EXISTS relimagemconteudo;
DROP TABLE IF EXISTS refatributoxopcao;
DROP TABLE IF EXISTS relatributoxproduto;
DROP TABLE IF EXISTS conteudovalor;
DROP TABLE IF EXISTS formularionew;
DROP TABLE IF EXISTS formulario;
DROP TABLE IF EXISTS imagem;
DROP TABLE IF EXISTS arquivo;
DROP TABLE IF EXISTS opcao;
DROP TABLE IF EXISTS atributo;
DROP TABLE IF EXISTS conteudo;
DROP TABLE IF EXISTS produto;
DROP TABLE IF EXISTS informativo;
DROP TABLE IF EXISTS infofoto;
DROP TABLE IF EXISTS newsletter;
DROP TABLE IF EXISTS socialmedia;
DROP TABLE IF EXISTS dict_socialmedia;
DROP TABLE IF EXISTS ciaaerea;
DROP TABLE IF EXISTS cambio;
DROP TABLE IF EXISTS unidades;
DROP TABLE IF EXISTS tipocotacao;
DROP TABLE IF EXISTS tipoenvio;
DROP TABLE IF EXISTS moeda;
DROP TABLE IF EXISTS moduloconf;
DROP TABLE IF EXISTS modulo;
DROP TABLE IF EXISTS categoria;
DROP TABLE IF EXISTS areas;
DROP TABLE IF EXISTS aplicacao;
DROP TABLE IF EXISTS dictareas;
DROP TABLE IF EXISTS dict_templates;
DROP TABLE IF EXISTS templates;

-- ==============================================================
-- TABELAS
-- Nomes de coluna usam exatamente os HasColumnName() do DbContext
-- para compatibilidade com EF Core + Npgsql (identificadores quoted)
-- ==============================================================

CREATE TABLE arquivo (
    "ArquivoId"  VARCHAR(64) NOT NULL,
    "AreaId"     VARCHAR(64) NULL,
    "ConteudoId" VARCHAR(64) NULL,
    "Nome"       VARCHAR(64) NULL,
    "TipoId"     VARCHAR(64) NULL,
    CONSTRAINT PK_arquivo PRIMARY KEY ("ArquivoId")
);

CREATE TABLE atributo (
    "AtributoId" UUID        NOT NULL,
    "Nome"       VARCHAR(45) NOT NULL,
    "Descricao"  VARCHAR(45) NOT NULL,
    "ProdutoId"  VARCHAR(64) NULL,
    CONSTRAINT PK_atributo PRIMARY KEY ("AtributoId")
);

CREATE TABLE conteudo (
    "ConteudoId"  VARCHAR(64) NOT NULL,
    "AreaId"      VARCHAR(64) NULL,
    "Autor"       VARCHAR(80) NULL,
    "Titulo"      VARCHAR(80) NULL,
    "Texto"       TEXT        NULL,
    "DataInclusao" TIMESTAMP  NULL,
    "DataFinal"   TIMESTAMP   NULL,
    "CategoriaId" VARCHAR(64) NULL,
    CONSTRAINT PK_conteudo PRIMARY KEY ("ConteudoId")
);

CREATE TABLE conteudovalor (
    "ConteudoId" VARCHAR(64)   NOT NULL,
    "UnidadeId"  UUID          NULL,
    "Valor"      DECIMAL(10,2) NULL,
    CONSTRAINT PK_conteudovalor PRIMARY KEY ("ConteudoId")
);

CREATE TABLE dict_templates (
    "idTemplate"      VARCHAR(100) NOT NULL,
    "Nome"            VARCHAR(255) NULL,
    "Descricao"       VARCHAR(255) NULL,
    "viewRelacionada" VARCHAR(45)  NULL,
    CONSTRAINT PK_dict_templates PRIMARY KEY ("idTemplate")
);

CREATE TABLE dictareas (
    "id"   INT         NOT NULL,
    "nome" VARCHAR(12) NULL,
    "tipo" INT         NOT NULL,
    CONSTRAINT PK_dictareas PRIMARY KEY ("id")
);

CREATE TABLE formulario (
    "formularioid" VARCHAR(64)   NOT NULL,
    "nome"         VARCHAR(255)  NOT NULL,
    "valor"        VARCHAR(8000) NULL,
    "ativo"        BOOLEAN       NULL,
    "datainclusao" TIMESTAMP     NULL,
    "areaid"       VARCHAR(64)   NULL,
    "categoriaid"  VARCHAR(64)   NULL,
    CONSTRAINT PK_formulario PRIMARY KEY ("formularioid", "nome")
);

CREATE TABLE formularionew (
    "IdForm"       INT          GENERATED ALWAYS AS IDENTITY,
    "Nome"         VARCHAR(100) NULL,
    "Tipo"         INT          NULL,
    "Email"        VARCHAR(155) NULL,
    "Telefone"     VARCHAR(15)  NULL,
    "Texto"        VARCHAR(255) NULL,
    "Ativo"        INT          NULL,
    "formularioid" VARCHAR(64)  NULL,
    CONSTRAINT PK_formularionew PRIMARY KEY ("IdForm")
);

CREATE TABLE imagem (
    "ImagemId"   VARCHAR(64)  NOT NULL,
    "Url"        VARCHAR(300) NULL,
    "Largura"    INT          NULL,
    "Altura"     INT          NULL,
    "AreaId"     VARCHAR(64)  NULL,
    "ConteudoId" VARCHAR(64)  NULL,
    "Descricao"  VARCHAR(255) NULL,
    "ParentId"   VARCHAR(64)  NULL,
    "TipoId"     VARCHAR(64)  NULL,
    CONSTRAINT PK_imagem PRIMARY KEY ("ImagemId")
);

CREATE TABLE modulo (
    "ModuloId" VARCHAR(64)  NOT NULL,
    "Nome"     VARCHAR(400) NULL,
    "Url"      VARCHAR(400) NULL,
    "Posicao"  INT          NULL,
    CONSTRAINT PK_modulo PRIMARY KEY ("ModuloId")
);

CREATE TABLE opcao (
    "OpcaoId"    VARCHAR(64)  NOT NULL,
    "AtributoId" UUID         NOT NULL,
    "Qtd"        INT          NOT NULL,
    "Nome"       VARCHAR(500) NULL,
    "Descricao"  VARCHAR(500) NULL,
    "Estoque"    INT          NULL,
    CONSTRAINT PK_opcao PRIMARY KEY ("OpcaoId")
);

CREATE TABLE produto (
    "ProdutoId"      VARCHAR(64)   NOT NULL,
    "Nome"           VARCHAR(130)  NULL,
    "Descricao"      TEXT          NULL,
    "Valor"          DECIMAL(18,2) NULL,
    "Tipo"           INT           NULL,
    "Destaque"       INT           NULL,
    "DataInicio"     TIMESTAMP     NULL,
    "DataFinal"      TIMESTAMP     NULL,
    "CategoriaId"    VARCHAR(64)   NULL,
    "AplicacaoId"    VARCHAR(64)   NULL,
    "sku"            VARCHAR(45)   NOT NULL,
    "PagSeguroKey"   VARCHAR(800)  NULL,
    "DetalheTecnico" TEXT          NULL,
    "DescricaCurta"  VARCHAR(150)  NULL,
    "produtocol"     VARCHAR(45)   NULL,
    CONSTRAINT PK_produto PRIMARY KEY ("ProdutoId", "sku")
);

CREATE TABLE refatributoxopcao (
    "relacaoid"  UUID NOT NULL,
    "atributoid" UUID NOT NULL,
    "opcaoid"    UUID NOT NULL,
    CONSTRAINT PK_refatributoxopcao PRIMARY KEY ("relacaoid")
);

CREATE TABLE relatributoxproduto (
    "Relacaoid"  UUID NOT NULL,
    "Atributoid" UUID NOT NULL,
    "ProdutoId"  UUID NOT NULL,
    CONSTRAINT PK_relatributoxproduto PRIMARY KEY ("Relacaoid")
);

CREATE TABLE unidades (
    "UnidadeId" UUID        NOT NULL,
    "Nome"      VARCHAR(45) NULL,
    "Sigla"     VARCHAR(45) NULL,
    CONSTRAINT PK_unidades PRIMARY KEY ("UnidadeId")
);

CREATE TABLE usuario (
    "UserId"       VARCHAR(64)  NOT NULL,
    "Nome"         VARCHAR(300) NOT NULL,
    "Sobrenome"    VARCHAR(300) NOT NULL,
    "Apelido"      VARCHAR(6)   NULL,
    "Senha"        VARCHAR(12)  NULL,
    "Ativo"        SMALLINT     NULL,
    "DataInclusao" TIMESTAMP    NULL,
    CONSTRAINT PK_usuario PRIMARY KEY ("UserId")
);

CREATE TABLE cambio (
    "CambioGroupId" VARCHAR(64)   NOT NULL,
    "DataCotacao"   TIMESTAMP     NULL,
    "MoedasXml"     VARCHAR(1000) NULL,
    "Tipo"          SMALLINT      NULL,
    CONSTRAINT PK_cambio PRIMARY KEY ("CambioGroupId")
);

CREATE TABLE categoria (
    "CategoriaId"    VARCHAR(64)   NOT NULL,
    "Nome"           VARCHAR(200)  NULL,
    "Descricao"      VARCHAR(1000) NULL,
    "TipoCategoria"  INT           NULL,
    "CategoriaIdPai" VARCHAR(64)   NULL,
    "AplicacaoId"    VARCHAR(64)   NULL,
    CONSTRAINT PK_categoria PRIMARY KEY ("CategoriaId")
);

CREATE TABLE ciaaerea (
    "CiaAereaId"     VARCHAR(64)   NOT NULL,
    "Descricao"      VARCHAR(4000) NULL,
    "Logotipo"       VARCHAR(300)  NULL,
    "Descricao_Longa" TEXT         NULL,
    "Ativo"          SMALLINT      NULL,
    "TipoNac"        SMALLINT      NULL,
    "TipoInt"        SMALLINT      NULL,
    "webticket_str"  VARCHAR(450)  NULL,
    CONSTRAINT PK_ciaaerea PRIMARY KEY ("CiaAereaId")
);

CREATE TABLE infofoto (
    "FotoId"      VARCHAR(64)   NOT NULL,
    "FotoUrl"     VARCHAR(300)  NULL,
    "Descricao"   VARCHAR(1000) NULL,
    "CategoriaId" VARCHAR(64)   NOT NULL,
    CONSTRAINT PK_infofoto PRIMARY KEY ("FotoId", "CategoriaId")
);

CREATE TABLE informativo (
    "InfoId"    VARCHAR(64)  NOT NULL,
    "Titulo"    VARCHAR(300) NULL,
    "Data"      TIMESTAMP    NULL,
    "Texto"     TEXT         NULL,
    "Foto"      VARCHAR(64)  NULL,
    "Ativo"     SMALLINT     NULL,
    "TipoEnvio" VARCHAR(64)  NULL,
    CONSTRAINT PK_informativo PRIMARY KEY ("InfoId")
);

CREATE TABLE moduloconf (
    "ModuloConfId" VARCHAR(64)  NOT NULL,
    "Descricao"    VARCHAR(800) NULL,
    "Nome"         VARCHAR(200) NULL,
    CONSTRAINT PK_moduloconf PRIMARY KEY ("ModuloConfId")
);

CREATE TABLE moeda (
    "MoedaId" VARCHAR(64) NOT NULL,
    "Nome"    VARCHAR(80) NOT NULL,
    "Sigla"   CHAR(34)    NULL,
    CONSTRAINT PK_moeda PRIMARY KEY ("MoedaId", "Nome")
);

CREATE TABLE newsletter (
    "NewsId"      VARCHAR(64)   NOT NULL,
    "Titulo"      VARCHAR(3000) NULL,
    "Autor"       VARCHAR(1000) NULL,
    "Data"        TIMESTAMP     NULL,
    "Frente"      SMALLINT      NULL,
    "Texto"       TEXT          NULL,
    "Foto"        VARCHAR(4000) NULL,
    "CategoriaId" VARCHAR(64)   NULL,
    "Ativo"       SMALLINT      NULL,
    CONSTRAINT PK_newsletter PRIMARY KEY ("NewsId")
);

CREATE TABLE relmoduloaplicacao (
    "RelacaoId"   VARCHAR(64) NOT NULL,
    "AplicacaoId" VARCHAR(64) NOT NULL,
    "ModuloId"    VARCHAR(64) NOT NULL,
    CONSTRAINT PK_relmoduloaplicacao PRIMARY KEY ("RelacaoId", "AplicacaoId", "ModuloId")
);

CREATE TABLE relmoduloconfaplicacao (
    "RelacaoId"       VARCHAR(64) NOT NULL,
    "ModuloConfId"    VARCHAR(64) NOT NULL,
    "AplicacaoId"     VARCHAR(64) NOT NULL,
    "DataInclusao"    TIMESTAMP   NOT NULL,
    "DataFinalizacao" TIMESTAMP   NOT NULL,
    CONSTRAINT PK_relmoduloconfaplicacao
        PRIMARY KEY ("RelacaoId", "ModuloConfId", "AplicacaoId", "DataInclusao", "DataFinalizacao")
);

CREATE TABLE relmodulousuario (
    "RelacaoId" VARCHAR(64) NOT NULL,
    "ModuloId"  VARCHAR(64) NOT NULL,
    "UsuarioId" VARCHAR(64) NOT NULL,
    CONSTRAINT PK_relmodulousuario PRIMARY KEY ("RelacaoId", "ModuloId", "UsuarioId")
);

CREATE TABLE relusuarioaplicacao (
    "RelacaoId"   VARCHAR(64) NOT NULL,
    "AplicacaoId" VARCHAR(64) NOT NULL,
    "UsuarioId"   VARCHAR(64) NOT NULL,
    CONSTRAINT PK_relusuarioaplicacao PRIMARY KEY ("RelacaoId", "AplicacaoId", "UsuarioId")
);

CREATE TABLE tipocotacao (
    "TipoCotacaoId" VARCHAR(64)  NOT NULL,
    "Nome"          VARCHAR(200) NULL,
    "Descricao"     VARCHAR(200) NULL,
    CONSTRAINT PK_tipocotacao PRIMARY KEY ("TipoCotacaoId")
);

CREATE TABLE tipoenvio (
    "TipoEnvioId"   VARCHAR(64)  NOT NULL,
    "TipoEnvioDesc" VARCHAR(300) NULL,
    CONSTRAINT PK_tipoenvio PRIMARY KEY ("TipoEnvioId")
);

CREATE TABLE socialmedia (
    "SocialMediaId"     VARCHAR(45)  NOT NULL,
    "AplicacaoId"       VARCHAR(45)  NULL,
    "SocialMediaTypeId" INT          NULL,
    "SocialMediaLink"   VARCHAR(255) NULL,
    CONSTRAINT PK_socialmedia PRIMARY KEY ("SocialMediaId")
);

CREATE TABLE dict_socialmedia (
    "SocialMediaId"   INT         NOT NULL,
    "SocialMediaName" VARCHAR(45) NULL,
    "SocialMediaUrl"  VARCHAR(45) NULL,
    CONSTRAINT PK_dict_socialmedia PRIMARY KEY ("SocialMediaId")
);

CREATE TABLE aplicacao (
    "AplicacaoId"     VARCHAR(64)  NOT NULL,
    "Nome"            VARCHAR(400) NULL,
    "Url"             VARCHAR(20)  NULL,
    "DataInicio"      TIMESTAMP    NULL,
    "DataFinal"       TIMESTAMP    NULL,
    "IdUsuarioInicio" VARCHAR(36)  NULL,
    "IdUsuarioFim"    VARCHAR(36)  NULL,
    "PagSeguroToken"  VARCHAR(120) NULL,
    "LayoutChoose"    VARCHAR(150) NULL,
    "Posicao"         INT          NULL,
    "mailUser"        VARCHAR(150) NULL,
    "mailPassword"    VARCHAR(45)  NULL,
    "mailServer"      VARCHAR(80)  NULL,
    "mailPort"        INT          NULL,
    "isSecure"        SMALLINT     NULL,
    "pageFacebook"    VARCHAR(255) NULL,
    "pageLinkedin"    VARCHAR(255) NULL,
    "pageInstagram"   VARCHAR(255) NULL,
    "pageTwitter"     VARCHAR(255) NULL,
    "pagePinterest"   VARCHAR(255) NULL,
    "pageFlicker"     VARCHAR(255) NULL,
    "logotipo"        BYTEA        NULL,
    "googleAdSense"   VARCHAR(500) NULL,
    "isActive"        BOOLEAN      NULL,
    "header"          VARCHAR(245) NULL,
    CONSTRAINT PK_aplicacao PRIMARY KEY ("AplicacaoId")
);

CREATE TABLE areas (
    "AreaId"      VARCHAR(64)  NOT NULL,
    "AplicacaoId" VARCHAR(64)  NULL,
    "Nome"        VARCHAR(80)  NULL,
    "Url"         VARCHAR(300) NULL,
    "Descricao"   VARCHAR(255) NULL,
    "AreaIdPai"   VARCHAR(64)  NULL,
    "DataInicial" TIMESTAMP    NULL,
    "DataFinal"   TIMESTAMP    NULL,
    "Imagem"      SMALLINT     NULL,
    "MenuLateral" SMALLINT     NULL,
    "MenuSplash"  SMALLINT     NULL,
    "MenuCentral" SMALLINT     NULL,
    "posicao"     INT          NULL,
    "MenuFixo"    SMALLINT     NULL,
    "ListaSimples" SMALLINT    NULL,
    "ListaSplash" SMALLINT     NULL,
    "ListaBanner" SMALLINT     NULL,
    "TipoArea"    INT          NULL,
    "layout"      TEXT         NULL,
    CONSTRAINT PK_areas PRIMARY KEY ("AreaId")
);

CREATE TABLE relimagemconteudo (
    "relid"    INT         GENERATED ALWAYS AS IDENTITY,
    "imagemid" VARCHAR(45) NULL,
    "parentid" VARCHAR(45) NULL,
    CONSTRAINT PK_relimagemconteudo PRIMARY KEY ("relid")
);

CREATE TABLE templates (
    "id"       INT          GENERATED ALWAYS AS IDENTITY,
    "Nome"     VARCHAR(155) NULL,
    "Descricao" VARCHAR(255) NULL,
    "Url"      VARCHAR(80)  NULL,
    "Ativo"    BOOLEAN      NULL,
    CONSTRAINT PK_templates PRIMARY KEY ("id")
);

-- ==============================================================
-- GRUPOS DE ACESSO
-- ==============================================================

CREATE TABLE grupo (
    "GrupoId"     VARCHAR(36)  NOT NULL,
    "Nome"        VARCHAR(100) NOT NULL,
    "Descricao"   VARCHAR(255) NULL,
    "AcessoTotal" BOOLEAN      NOT NULL DEFAULT FALSE,
    CONSTRAINT PK_grupo PRIMARY KEY ("GrupoId")
);

CREATE TABLE relusuariogrupo (
    "RelacaoId" VARCHAR(36) NOT NULL,
    "UsuarioId" VARCHAR(64) NOT NULL,
    "GrupoId"   VARCHAR(36) NOT NULL,
    CONSTRAINT PK_relusuariogrupo PRIMARY KEY ("RelacaoId"),
    CONSTRAINT FK_relug_usuario FOREIGN KEY ("UsuarioId") REFERENCES usuario("UserId"),
    CONSTRAINT FK_relug_grupo   FOREIGN KEY ("GrupoId")   REFERENCES grupo("GrupoId")
);

-- ==============================================================
-- PAGE BUILDER
-- ==============================================================

CREATE TABLE dict_blocos (
    "tipobloco"    VARCHAR(50)  NOT NULL,
    "nome"         VARCHAR(100) NOT NULL,
    "descricao"    VARCHAR(500) NULL,
    "icone"        VARCHAR(50)  NULL,
    "schema_config" TEXT        NOT NULL DEFAULT '{}',
    CONSTRAINT PK_dict_blocos PRIMARY KEY ("tipobloco")
);

-- ==============================================================
-- IA: CONFIG, CACHE, USO
-- ==============================================================

CREATE TABLE ia_config (
    aplicacaoid  VARCHAR(50)  NOT NULL,
    provedor     VARCHAR(20)  NULL,
    apikey       VARCHAR(500) NULL,
    modelo       VARCHAR(100) NULL,
    limite_diario INT         NULL,
    CONSTRAINT PK_ia_config PRIMARY KEY (aplicacaoid)
);

CREATE TABLE ia_cache (
    cacheid        VARCHAR(36) NOT NULL,
    hash           VARCHAR(64) NOT NULL,
    resultado      TEXT        NOT NULL,
    datainclusao   TIMESTAMP   NOT NULL DEFAULT NOW(),
    datavencimento TIMESTAMP   NOT NULL,
    CONSTRAINT PK_ia_cache PRIMARY KEY (cacheid),
    CONSTRAINT UQ_ia_cache_hash UNIQUE (hash)
);
CREATE INDEX IX_ia_cache_hash ON ia_cache (hash);

CREATE TABLE ia_uso (
    usoid       VARCHAR(36) NOT NULL,
    aplicacaoid VARCHAR(50) NOT NULL,
    data        DATE        NOT NULL,
    contador    INT         NOT NULL DEFAULT 0,
    CONSTRAINT PK_ia_uso PRIMARY KEY (usoid),
    CONSTRAINT UQ_ia_uso UNIQUE (aplicacaoid, data)
);
CREATE INDEX IX_ia_uso_appdata ON ia_uso (aplicacaoid, data);

-- ==============================================================
-- FAQ
-- ==============================================================

CREATE TABLE faq (
    faqid        VARCHAR(64)  NOT NULL,
    formularioid VARCHAR(64)  NOT NULL,
    pergunta     VARCHAR(500) NOT NULL,
    resposta     TEXT         NOT NULL,
    ordem        INT          NOT NULL DEFAULT 0,
    ativo        BOOLEAN      NOT NULL DEFAULT TRUE,
    datainclusao TIMESTAMP    NOT NULL DEFAULT NOW(),
    CONSTRAINT PK_faq PRIMARY KEY (faqid)
);

-- ==============================================================
-- SEED DATA
-- ==============================================================

-- Aplicação
INSERT INTO aplicacao ("AplicacaoId", "Nome", "Url", "DataInicio", "isActive")
VALUES ('APP-001', 'LimpMax', 'limpmax.com.br', NOW(), TRUE);

-- Usuário admin
INSERT INTO usuario ("UserId", "Nome", "Sobrenome", "Apelido", "Senha", "Ativo", "DataInclusao")
VALUES ('USR-001', 'Administrador', 'Sistema', 'admin', 'admin123', 1, NOW());

-- Módulos do menu
INSERT INTO modulo ("ModuloId", "Nome", "Url", "Posicao") VALUES
('MOD-001', 'Dashboard',    '/dashboard',    1),
('MOD-002', 'Conteudo',     '/conteudo',     2),
('MOD-003', 'Categorias',   '/categorias',   3),
('MOD-004', 'Areas',        '/areas',        4),
('MOD-005', 'Imagens',      '/imagens',      5),
('MOD-006', 'Usuarios',     '/usuarios',     6),
('MOD-007', 'Formularios',  '/formularios',  7),
('MOD-008', 'Produtos',     '/produtos',     8),
('MOD-009', 'Page Builder',    '/page-builder',    9),
('MOD-010', 'Page Builder v2', '/page-builder-v2', 10);

-- Categorias de conteúdo
INSERT INTO categoria ("CategoriaId", "Nome", "Descricao", "TipoCategoria", "AplicacaoId") VALUES
('CAT-001', 'Noticias',      'Noticias gerais',           1, 'APP-001'),
('CAT-002', 'Servicos',      'Servicos oferecidos',       2, 'APP-001'),
('CAT-003', 'Produtos',      'Produtos em destaque',      3, 'APP-001'),
('CAT-004', 'Institucional', 'Informacoes da empresa',    4, 'APP-001');

-- Áreas do site
INSERT INTO areas ("AreaId", "AplicacaoId", "Nome", "Url", "Descricao", "DataInicial", "MenuLateral", "MenuCentral", "posicao", "TipoArea") VALUES
('ARE-001', 'APP-001', 'Home',     '/home',     'Pagina inicial',     NOW(), 0, 1, 1, 1),
('ARE-002', 'APP-001', 'Sobre',    '/sobre',    'Quem somos',         NOW(), 1, 1, 2, 1),
('ARE-003', 'APP-001', 'Servicos', '/servicos', 'Nossos servicos',    NOW(), 1, 1, 3, 2),
('ARE-004', 'APP-001', 'Contato',  '/contato',  'Fale conosco',       NOW(), 1, 1, 4, 1),
('ARE-005', 'APP-001', 'Blog',     '/blog',     'Noticias e artigos', NOW(), 1, 1, 5, 1);

-- Templates
INSERT INTO templates ("Nome", "Descricao", "Url", "Ativo") VALUES
('Template Padrao',  'Layout basico do site', '/templates/padrao',  TRUE),
('Template Blog',    'Layout para blog',      '/templates/blog',    TRUE),
('Template Landing', 'Landing page',          '/templates/landing', TRUE);

-- Dicionário de áreas
INSERT INTO dictareas ("id", "nome", "tipo") VALUES
(1, 'Principal', 1),
(2, 'Lateral',   2),
(3, 'Rodape',    3);

-- Relações
INSERT INTO relusuarioaplicacao ("RelacaoId", "AplicacaoId", "UsuarioId")
VALUES ('REL-UA-001', 'APP-001', 'USR-001');

INSERT INTO relmoduloaplicacao ("RelacaoId", "AplicacaoId", "ModuloId") VALUES
('REL-MA-001', 'APP-001', 'MOD-001'),
('REL-MA-002', 'APP-001', 'MOD-002'),
('REL-MA-003', 'APP-001', 'MOD-003'),
('REL-MA-004', 'APP-001', 'MOD-004'),
('REL-MA-005', 'APP-001', 'MOD-005'),
('REL-MA-006', 'APP-001', 'MOD-006'),
('REL-MA-007', 'APP-001', 'MOD-007'),
('REL-MA-008', 'APP-001', 'MOD-008'),
('REL-MA-009', 'APP-001', 'MOD-009'),
('REL-MA-010', 'APP-001', 'MOD-010');

INSERT INTO relmodulousuario ("RelacaoId", "ModuloId", "UsuarioId") VALUES
('REL-MU-001', 'MOD-001', 'USR-001'),
('REL-MU-002', 'MOD-002', 'USR-001'),
('REL-MU-003', 'MOD-003', 'USR-001'),
('REL-MU-004', 'MOD-004', 'USR-001'),
('REL-MU-005', 'MOD-005', 'USR-001'),
('REL-MU-006', 'MOD-006', 'USR-001'),
('REL-MU-007', 'MOD-007', 'USR-001'),
('REL-MU-008', 'MOD-008', 'USR-001'),
('REL-MU-009', 'MOD-009', 'USR-001'),
('REL-MU-010', 'MOD-010', 'USR-001');

-- Conteúdo de exemplo
INSERT INTO conteudo ("ConteudoId", "AreaId", "Autor", "Titulo", "Texto", "DataInclusao", "CategoriaId") VALUES
('CON-001', 'ARE-001', 'admin', 'Bem-vindo ao LimpMax',
 'Somos especializados em servicos de limpeza profissional para residencias e empresas.',
 NOW(), 'CAT-004'),
('CON-002', 'ARE-002', 'admin', 'Quem Somos',
 'A LimpMax atua ha mais de 10 anos no mercado, oferecendo qualidade e pontualidade.',
 NOW(), 'CAT-004'),
('CON-003', 'ARE-003', 'admin', 'Limpeza Residencial',
 'Servico completo de limpeza para sua casa. Agendamento flexivel.',
 NOW(), 'CAT-002'),
('CON-004', 'ARE-003', 'admin', 'Limpeza Comercial',
 'Terceirizacao de servicos de limpeza para empresas de todos os portes.',
 NOW(), 'CAT-002'),
('CON-005', 'ARE-005', 'admin', 'Dicas de Organizacao',
 'Confira nossas dicas para manter sua casa organizada no dia a dia.',
 NOW(), 'CAT-001');

-- Grupos de acesso
INSERT INTO grupo ("GrupoId", "Nome", "Descricao", "AcessoTotal") VALUES
('GRP-001', 'Administradores', 'Acesso total a todos os tenants e configuracoes', TRUE),
('GRP-002', 'Suporte',         'Visualiza todos os tenants, sem permissao de exclusao', TRUE),
('GRP-003', 'Tenant',          'Acessa apenas a propria aplicacao', FALSE);

INSERT INTO relusuariogrupo ("RelacaoId", "UsuarioId", "GrupoId")
VALUES ('REL-UG-001', 'USR-001', 'GRP-001');

-- ==============================================================
-- DICT_BLOCOS - Catálogo de tipos de bloco para o Page Builder
-- ==============================================================

INSERT INTO dict_blocos (tipobloco, nome, descricao, icone, schema_config) VALUES
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

-- ==============================================================
-- VERIFICAÇÃO
-- ==============================================================

SELECT 'aplicacao'           AS tabela, COUNT(*) AS registros FROM aplicacao
UNION ALL SELECT 'usuario',              COUNT(*) FROM usuario
UNION ALL SELECT 'modulo',               COUNT(*) FROM modulo
UNION ALL SELECT 'categoria',            COUNT(*) FROM categoria
UNION ALL SELECT 'areas',                COUNT(*) FROM areas
UNION ALL SELECT 'conteudo',             COUNT(*) FROM conteudo
UNION ALL SELECT 'grupo',                COUNT(*) FROM grupo
UNION ALL SELECT 'relusuariogrupo',      COUNT(*) FROM relusuariogrupo
UNION ALL SELECT 'relusuarioaplicacao',  COUNT(*) FROM relusuarioaplicacao
UNION ALL SELECT 'relmoduloaplicacao',   COUNT(*) FROM relmoduloaplicacao
UNION ALL SELECT 'relmodulousuario',     COUNT(*) FROM relmodulousuario
UNION ALL SELECT 'dict_blocos',          COUNT(*) FROM dict_blocos;
