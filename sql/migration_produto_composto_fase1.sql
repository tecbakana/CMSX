-- Fase 1: Produto Composto

-- 0. produto(ProdutoId) precisa de constraint única para ser referenciado por FKs
--    (PK da tabela é composta com sku, por isso o UNIQUE separado é necessário)
IF NOT EXISTS (
    SELECT 1 FROM sys.indexes
    WHERE name = 'UQ_produto_ProdutoId' AND object_id = OBJECT_ID('produto')
)
    ALTER TABLE produto ADD CONSTRAINT UQ_produto_ProdutoId UNIQUE (ProdutoId);

-- 1. Atributo: hierarquia recursiva
ALTER TABLE atributo ADD ParentAtributoId uniqueidentifier NULL;
ALTER TABLE atributo ADD Ordem int NULL;
ALTER TABLE atributo ADD CONSTRAINT FK_Atributo_Parent
  FOREIGN KEY (ParentAtributoId) REFERENCES atributo(AtributoId);

-- 2. Opcao: modificador de valor por unidade
ALTER TABLE opcao ADD ValorAdicional decimal(18,2) NULL;

-- 3. OrcamentoDetalheComposto
CREATE TABLE orcamentodetalhecomposto (
    OrcamentoDetalheCompostoId uniqueidentifier NOT NULL DEFAULT NEWID(),
    Orcamentoid                uniqueidentifier NOT NULL,
    Produtoid                  varchar(64) NOT NULL,
    Quantidade                 decimal(10,2) NOT NULL DEFAULT 1,
    ValorBase                  decimal(18,2) NOT NULL,
    ValorTotal                 decimal(18,2) NOT NULL,
    ConfiguracaoJson           nvarchar(MAX) NOT NULL,
    Versao                     int NOT NULL DEFAULT 1,
    Atual                      bit NOT NULL DEFAULT 1,
    Datainclusao               datetime2 NOT NULL DEFAULT GETUTCDATE(),
    CONSTRAINT PK_orcamentodetalhecomposto PRIMARY KEY (OrcamentoDetalheCompostoId),
    CONSTRAINT FK_OrcDetalheComposto_Cabecalho FOREIGN KEY (Orcamentoid)
      REFERENCES orcamentocabecalho(Orcamentoid),
    CONSTRAINT FK_OrcDetalheComposto_Produto FOREIGN KEY (Produtoid)
      REFERENCES produto(Produtoid)
);
CREATE INDEX IX_orcamentodetalhecomposto_orcamentoid
  ON orcamentodetalhecomposto(Orcamentoid);
CREATE INDEX IX_orcamentodetalhecomposto_atual
  ON orcamentodetalhecomposto(Orcamentoid, Atual);

-- 4. Selecao
CREATE TABLE selecao (
    SelecaoId                  uniqueidentifier NOT NULL DEFAULT NEWID(),
    OrcamentoDetalheCompostoId uniqueidentifier NOT NULL,
    Atributoid                 uniqueidentifier NOT NULL,
    Opcaoid                    varchar(64) NOT NULL,
    ValorAdicional             decimal(18,2) NOT NULL DEFAULT 0,
    CONSTRAINT PK_selecao PRIMARY KEY (SelecaoId),
    CONSTRAINT FK_Selecao_Detalhe FOREIGN KEY (OrcamentoDetalheCompostoId)
      REFERENCES orcamentodetalhecomposto(OrcamentoDetalheCompostoId),
    CONSTRAINT FK_Selecao_Atributo FOREIGN KEY (Atributoid)
      REFERENCES atributo(AtributoId),
    CONSTRAINT FK_Selecao_Opcao FOREIGN KEY (Opcaoid)
      REFERENCES opcao(OpcaoId)
);
CREATE INDEX IX_selecao_detalhecomposto ON selecao(OrcamentoDetalheCompostoId);

-- 5. ModeloComposto
CREATE TABLE modelocomposto (
    ModeloCompostoId   uniqueidentifier NOT NULL DEFAULT NEWID(),
    Aplicacaoid        varchar(64) NOT NULL,
    Produtoid          varchar(64) NOT NULL,
    Nome               varchar(500) NOT NULL,
    ValorUnitario      decimal(18,2) NOT NULL,
    ConfiguracaoHash   varchar(64) NOT NULL,
    Usos               int NOT NULL DEFAULT 1,
    Datacriacao        datetime2 NOT NULL DEFAULT GETUTCDATE(),
    CONSTRAINT PK_modelocomposto PRIMARY KEY (ModeloCompostoId),
    CONSTRAINT FK_ModeloComposto_Produto FOREIGN KEY (Produtoid)
      REFERENCES produto(Produtoid)
);
CREATE UNIQUE INDEX IX_modelocomposto_hash
  ON modelocomposto(ConfiguracaoHash, Aplicacaoid, Produtoid);
CREATE INDEX IX_modelocomposto_aplicacao ON modelocomposto(Aplicacaoid, Produtoid);

-- 6. ModeloSelecao
CREATE TABLE modeloselecao (
    ModeloSelecaoId  uniqueidentifier NOT NULL DEFAULT NEWID(),
    ModeloCompostoId uniqueidentifier NOT NULL,
    Atributoid       uniqueidentifier NOT NULL,
    Opcaoid          varchar(64) NOT NULL,
    CONSTRAINT PK_modeloselecao PRIMARY KEY (ModeloSelecaoId),
    CONSTRAINT FK_ModeloSelecao_Modelo FOREIGN KEY (ModeloCompostoId)
      REFERENCES modelocomposto(ModeloCompostoId),
    CONSTRAINT FK_ModeloSelecao_Atributo FOREIGN KEY (Atributoid)
      REFERENCES atributo(AtributoId),
    CONSTRAINT FK_ModeloSelecao_Opcao FOREIGN KEY (Opcaoid)
      REFERENCES opcao(OpcaoId)
);
CREATE INDEX IX_modeloselecao_modelo ON modeloselecao(ModeloCompostoId);
