-- --------------------------------------------------
-- CMSX Seed Data - LimpMaxDB
-- Execute no banco LimpMaxDB apos criar a estrutura
-- --------------------------------------------------

USE [cmsxDB];
GO

-- --------------------------------------------------
-- 1. Tabelas independentes
-- --------------------------------------------------

-- Aplicacao (cliente / site gerenciado pelo CMS)
INSERT INTO [dbo].[aplicacao] (AplicacaoId, Nome, Url, DataInicio, isActive)
VALUES ('APP-001', 'LimpMax', 'limpmax.com.br', GETDATE(), 1);
GO

-- Usuario admin
INSERT INTO [dbo].[usuario] (UserId, Nome, Sobrenome, Apelido, Senha, Ativo, DataInclusao)
VALUES ('USR-001', 'Administrador', 'Sistema', 'admin', 'admin123', 1, GETDATE());
GO

-- Modulos (itens de menu do CMS)
INSERT INTO [dbo].[modulo] (ModuloId, Nome, Url, Posicao) VALUES
('MOD-001', 'Dashboard',    '/dashboard',    1),
('MOD-002', 'Conteudo',     '/conteudo',     2),
('MOD-003', 'Categorias',   '/categorias',   3),
('MOD-004', 'Areas',        '/areas',        4),
('MOD-005', 'Imagens',      '/imagens',      5),
('MOD-006', 'Usuarios',     '/usuarios',     6),
('MOD-007', 'Formularios',  '/formularios',    7),
('MOD-008', 'Produtos',     '/produtos',       8),
('MOD-009', 'Page Builder', '/page-builder',   9),
('MOD-010', 'Page Builder v2', '/page-builder-v2', 10);
GO

-- Categorias de conteudo
INSERT INTO [dbo].[categoria] (CategoriaId, Nome, Descricao, TipoCategoria, AplicacaoId) VALUES
('CAT-001', 'Noticias',    'Noticias gerais',            1, 'APP-001'),
('CAT-002', 'Servicos',    'Servicos oferecidos',        2, 'APP-001'),
('CAT-003', 'Produtos',    'Produtos em destaque',       3, 'APP-001'),
('CAT-004', 'Institucional','Informacoes da empresa',    4, 'APP-001');
GO

-- Areas (secoes do site)
INSERT INTO [dbo].[areas] (AreaId, AplicacaoId, Nome, Url, Descricao, DataInicial, MenuLateral, MenuCentral, posicao, TipoArea) VALUES
('ARE-001', 'APP-001', 'Home',         '/home',         'Pagina inicial',         GETDATE(), 0, 1, 1, 1),
('ARE-002', 'APP-001', 'Sobre',        '/sobre',        'Quem somos',             GETDATE(), 1, 1, 2, 1),
('ARE-003', 'APP-001', 'Servicos',     '/servicos',     'Nossos servicos',        GETDATE(), 1, 1, 3, 2),
('ARE-004', 'APP-001', 'Contato',      '/contato',      'Fale conosco',           GETDATE(), 1, 1, 4, 1),
('ARE-005', 'APP-001', 'Blog',         '/blog',         'Noticias e artigos',     GETDATE(), 1, 1, 5, 1);
GO

-- Templates disponíveis
INSERT INTO [dbo].[templates] (Nome, Descricao, Url, Ativo) VALUES
('Template Padrao',    'Layout basico do site',   '/templates/padrao',   1),
('Template Blog',      'Layout para blog',        '/templates/blog',     1),
('Template Landing',   'Landing page',            '/templates/landing',  1);
GO

-- Dicionario de areas
INSERT INTO [dbo].[dictareas] (id, nome, tipo) VALUES
(1, 'Principal', 1),
(2, 'Lateral',   2),
(3, 'Rodape',    3);
GO

-- --------------------------------------------------
-- 2. Relacoes (tabelas com FK)
-- --------------------------------------------------

-- Vincula usuario à aplicacao
INSERT INTO [dbo].[relusuarioaplicacao] (RelacaoId, AplicacaoId, UsuarioId)
VALUES ('REL-UA-001', 'APP-001', 'USR-001');
GO

-- Vincula modulos à aplicacao
INSERT INTO [dbo].[relmoduloaplicacao] (RelacaoId, AplicacaoId, ModuloId) VALUES
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
GO

-- Vincula modulos ao usuario
INSERT INTO [dbo].[relmodulousuario] (RelacaoId, ModuloId, UsuarioId) VALUES
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
GO

-- --------------------------------------------------
-- 3. Conteudo de exemplo
-- --------------------------------------------------

INSERT INTO [dbo].[conteudo] (ConteudoId, AreaId, Autor, Titulo, Texto, DataInclusao, CategoriaId) VALUES
('CON-001', 'ARE-001', 'admin', 'Bem-vindo ao LimpMax',
 'Somos especializados em servicos de limpeza profissional para residencias e empresas.',
 GETDATE(), 'CAT-004'),
('CON-002', 'ARE-002', 'admin', 'Quem Somos',
 'A LimpMax atua ha mais de 10 anos no mercado, oferecendo qualidade e pontualidade.',
 GETDATE(), 'CAT-004'),
('CON-003', 'ARE-003', 'admin', 'Limpeza Residencial',
 'Servico completo de limpeza para sua casa. Agendamento flexivel.',
 GETDATE(), 'CAT-002'),
('CON-004', 'ARE-003', 'admin', 'Limpeza Comercial',
 'Terceirizacao de servicos de limpeza para empresas de todos os portes.',
 GETDATE(), 'CAT-002'),
('CON-005', 'ARE-005', 'admin', 'Dicas de Organizacao',
 'Confira nossas dicas para manter sua casa organizada no dia a dia.',
 GETDATE(), 'CAT-001');
GO

-- --------------------------------------------------
-- Verificacao
-- --------------------------------------------------
SELECT 'aplicacao'        AS Tabela, COUNT(*) AS Registros FROM aplicacao       UNION ALL
SELECT 'usuario',                    COUNT(*)               FROM usuario          UNION ALL
SELECT 'modulo',                     COUNT(*)               FROM modulo           UNION ALL
SELECT 'categoria',                  COUNT(*)               FROM categoria        UNION ALL
SELECT 'areas',                      COUNT(*)               FROM areas            UNION ALL
SELECT 'conteudo',                   COUNT(*)               FROM conteudo         UNION ALL
SELECT 'relusuarioaplicacao',        COUNT(*)               FROM relusuarioaplicacao UNION ALL
SELECT 'relmoduloaplicacao',         COUNT(*)               FROM relmoduloaplicacao  UNION ALL
SELECT 'relmodulousuario',           COUNT(*)               FROM relmodulousuario;
GO
