using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace CMSXData.Models;

public partial class CmsxDbContext : DbContext
{
    public CmsxDbContext()
    {
    }

    public CmsxDbContext(DbContextOptions<CmsxDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Aplicacao> Aplicacaos { get; set; }

    public virtual DbSet<Area> Areas { get; set; }
    public virtual DbSet<DictBloco> DictBlocos { get; set; }

    public virtual DbSet<Arquivo> Arquivos { get; set; }

    public virtual DbSet<Atributo> Atributos { get; set; }

    public virtual DbSet<Cambio> Cambios { get; set; }

    public virtual DbSet<Caterium> Cateria { get; set; }

    public virtual DbSet<Ciaaerea> Ciaaereas { get; set; }

    public virtual DbSet<Conteudo> Conteudos { get; set; }

    public virtual DbSet<Conteudovalor> Conteudovalors { get; set; }

    public virtual DbSet<DictSocialmedium> DictSocialmedia { get; set; }

    public virtual DbSet<DictTemplate> DictTemplates { get; set; }

    public virtual DbSet<Dictarea> Dictareas { get; set; }

    public virtual DbSet<Faq> Faqs { get; set; }

    public virtual DbSet<Formulario> Formularios { get; set; }

    public virtual DbSet<Formularionew> Formularionews { get; set; }

    public virtual DbSet<Imagem> Imagems { get; set; }

    public virtual DbSet<Infofoto> Infofotos { get; set; }

    public virtual DbSet<Informativo> Informativos { get; set; }

    public virtual DbSet<Modulo> Modulos { get; set; }

    public virtual DbSet<Moduloconf> Moduloconfs { get; set; }

    public virtual DbSet<Moedum> Moeda { get; set; }

    public virtual DbSet<Newsletter> Newsletters { get; set; }

    public virtual DbSet<Opcao> Opcaos { get; set; }

    public virtual DbSet<Produto> Produtos { get; set; }

    public virtual DbSet<Refatributoxopcao> Refatributoxopcaos { get; set; }

    public virtual DbSet<Relatributoxproduto> Relatributoxprodutos { get; set; }

    public virtual DbSet<Relimagemconteudo> Relimagemconteudos { get; set; }

    public virtual DbSet<Relmoduloaplicacao> Relmoduloaplicacaos { get; set; }

    public virtual DbSet<Relmoduloconfaplicacao> Relmoduloconfaplicacaos { get; set; }

    public virtual DbSet<Relmodulousuario> Relmodulousuarios { get; set; }

    public virtual DbSet<Relusuarioaplicacao> Relusuarioaplicacaos { get; set; }

    public virtual DbSet<RelusuarioaplicacaoOld> RelusuarioaplicacaoOlds { get; set; }

    public virtual DbSet<Socialmedium> Socialmedia { get; set; }

    public virtual DbSet<TblCliente> TblClientes { get; set; }

    public virtual DbSet<Template> Templates { get; set; }

    public virtual DbSet<Tipocotacao> Tipocotacaos { get; set; }

    public virtual DbSet<Tipoenvio> Tipoenvios { get; set; }

    public virtual DbSet<Unidade> Unidades { get; set; }

    public virtual DbSet<Usuario> Usuarios { get; set; }

    public virtual DbSet<Pedido> Pedidos { get; set; }

    public virtual DbSet<Statuspedido> Statuspedidos { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see http://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseNpgsql("User ID=postgres;Password=su74;Host=localhost;Port=5432;Database=cmsxDB;");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasPostgresExtension("uuid-ossp");

        modelBuilder.Entity<Aplicacao>(entity =>
        {
            entity.HasKey(e => e.Aplicacaoid).HasName("aplicacaoIdPK");

            entity.ToTable("aplicacao");

            entity.Property(e => e.Aplicacaoid)
                .HasDefaultValueSql("uuid_generate_v4()")
                .HasComment("Id unico da aplicacao")
                .HasColumnName("aplicacaoid");
            entity.Property(e => e.Datafinal)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("datafinal");
            entity.Property(e => e.Datainicio)
                .HasDefaultValueSql("CURRENT_DATE")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("datainicio");
            entity.Property(e => e.Header)
                .HasMaxLength(245)
                .HasColumnName("header");
            entity.Property(e => e.Idusuariofim)
                .HasMaxLength(36)
                .HasColumnName("idusuariofim");
            entity.Property(e => e.Idusuarioinicio)
                .HasMaxLength(36)
                .HasColumnName("idusuarioinicio");
            entity.Property(e => e.Isactive).HasColumnName("isactive");
            entity.Property(e => e.Isactivea)
                .HasColumnType("bit(1)")
                .HasColumnName("isactivea");
            entity.Property(e => e.Issecure).HasColumnName("issecure");
            entity.Property(e => e.Layoutchoose)
                .HasMaxLength(150)
                .HasColumnName("layoutchoose");
            entity.Property(e => e.Lotipo).HasColumnName("lotipo");
            entity.Property(e => e.Mailpassword)
                .HasMaxLength(45)
                .HasColumnName("mailpassword");
            entity.Property(e => e.Mailport).HasColumnName("mailport");
            entity.Property(e => e.Mailserver)
                .HasMaxLength(80)
                .HasColumnName("mailserver");
            entity.Property(e => e.Mailuser)
                .HasMaxLength(150)
                .HasColumnName("mailuser");
            entity.Property(e => e.Nome)
                .HasMaxLength(400)
                .HasColumnName("nome");
            entity.Property(e => e.Ogleadsense)
                .HasMaxLength(500)
                .HasColumnName("ogleadsense");
            entity.Property(e => e.Pagefacebook)
                .HasMaxLength(255)
                .HasColumnName("pagefacebook");
            entity.Property(e => e.Pageflicker)
                .HasMaxLength(255)
                .HasColumnName("pageflicker");
            entity.Property(e => e.Pageinstagram)
                .HasMaxLength(255)
                .HasColumnName("pageinstagram");
            entity.Property(e => e.Pagelinkedin)
                .HasMaxLength(255)
                .HasColumnName("pagelinkedin");
            entity.Property(e => e.Pagepinterest)
                .HasMaxLength(255)
                .HasColumnName("pagepinterest");
            entity.Property(e => e.Pagetwitter)
                .HasMaxLength(255)
                .HasColumnName("pagetwitter");
            entity.Property(e => e.Pagsegurotoken)
                .HasMaxLength(120)
                .HasColumnName("pagsegurotoken");
            entity.Property(e => e.Posicao).HasColumnName("posicao");
            entity.Property(e => e.Url)
                .HasMaxLength(20)
                .HasColumnName("url");
        });

        modelBuilder.Entity<Area>(entity =>
        {
            entity.HasKey(e => e.Areaid).HasName("pk_areas");

            entity.ToTable("areas");

            entity.Property(e => e.Areaid)
                .HasMaxLength(64)
                .HasColumnName("areaid");
            entity.Property(e => e.Aplicacaoid)
                .HasMaxLength(64)
                .HasColumnName("aplicacaoid");
            entity.Property(e => e.Areaidpai)
                .HasMaxLength(64)
                .HasColumnName("areaidpai");
            entity.Property(e => e.Datafinal)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("datafinal");
            entity.Property(e => e.Datainicial)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("datainicial");
            entity.Property(e => e.Descricao)
                .HasMaxLength(255)
                .HasColumnName("descricao");
            entity.Property(e => e.Imagem).HasColumnName("imagem");
            entity.Property(e => e.Listabanner).HasColumnName("listabanner");
            entity.Property(e => e.Listasimples).HasColumnName("listasimples");
            entity.Property(e => e.Listasplash).HasColumnName("listasplash");
            entity.Property(e => e.Menucentral).HasColumnName("menucentral");
            entity.Property(e => e.Menufixo).HasColumnName("menufixo");
            entity.Property(e => e.Menulateral).HasColumnName("menulateral");
            entity.Property(e => e.Menusplash).HasColumnName("menusplash");
            entity.Property(e => e.Nome)
                .HasMaxLength(80)
                .HasColumnName("nome");
            entity.Property(e => e.Posicao).HasColumnName("posicao");
            entity.Property(e => e.Tipoarea).HasColumnName("tipoarea");
            entity.Property(e => e.Url)
                .HasMaxLength(300)
                .HasColumnName("url");
            entity.Property(e => e.Layout).HasColumnName("layout");
        });

        modelBuilder.Entity<DictBloco>(entity =>
        {
            entity.HasKey(e => e.Tipobloco).HasName("pk_dict_blocos");
            entity.ToTable("dict_blocos");
            entity.Property(e => e.Tipobloco).HasMaxLength(50).HasColumnName("tipobloco");
            entity.Property(e => e.Nome).HasMaxLength(100).HasColumnName("nome");
            entity.Property(e => e.Descricao).HasMaxLength(255).HasColumnName("descricao");
            entity.Property(e => e.Icone).HasMaxLength(50).HasColumnName("icone");
            entity.Property(e => e.SchemaConfig).HasColumnName("schema_config");
        });

        modelBuilder.Entity<Arquivo>(entity =>
        {
            entity.HasKey(e => e.Arquivoid).HasName("pk_arquivo");

            entity.ToTable("arquivo");

            entity.Property(e => e.Arquivoid)
                .HasMaxLength(64)
                .HasColumnName("arquivoid");
            entity.Property(e => e.Areaid)
                .HasMaxLength(64)
                .HasColumnName("areaid");
            entity.Property(e => e.Conteudoid)
                .HasMaxLength(64)
                .HasColumnName("conteudoid");
            entity.Property(e => e.Nome)
                .HasMaxLength(64)
                .HasColumnName("nome");
            entity.Property(e => e.Tipoid)
                .HasMaxLength(64)
                .HasColumnName("tipoid");
        });

        modelBuilder.Entity<Atributo>(entity =>
        {
            entity.HasKey(e => e.Atributoid).HasName("pk_atributo");

            entity.ToTable("atributo");

            entity.Property(e => e.Atributoid)
                .ValueGeneratedNever()
                .HasColumnName("atributoid");
            entity.Property(e => e.Descricao)
                .HasMaxLength(45)
                .HasColumnName("descricao");
            entity.Property(e => e.Nome)
                .HasMaxLength(45)
                .HasColumnName("nome");
            entity.Property(e => e.Produtoid)
                .HasMaxLength(64)
                .HasColumnName("produtoid");
        });

        modelBuilder.Entity<Cambio>(entity =>
        {
            entity.HasKey(e => e.Cambiogroupid).HasName("pk_cambio");

            entity.ToTable("cambio");

            entity.Property(e => e.Cambiogroupid)
                .HasMaxLength(64)
                .HasColumnName("cambiogroupid");
            entity.Property(e => e.Datacotacao)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("datacotacao");
            entity.Property(e => e.Moedasxml)
                .HasMaxLength(1000)
                .HasColumnName("moedasxml");
            entity.Property(e => e.Tipo).HasColumnName("tipo");
        });

        modelBuilder.Entity<Caterium>(entity =>
        {
            entity.HasKey(e => e.Cateriaid).HasName("pk_cateria");

            entity.ToTable("cateria");

            entity.Property(e => e.Cateriaid)
                .HasMaxLength(64)
                .HasColumnName("cateriaid");
            entity.Property(e => e.Aplicacaoid)
                .HasMaxLength(64)
                .HasColumnName("aplicacaoid");
            entity.Property(e => e.Cateriaidpai)
                .HasMaxLength(64)
                .HasColumnName("cateriaidpai");
            entity.Property(e => e.Descricao)
                .HasMaxLength(1000)
                .HasColumnName("descricao");
            entity.Property(e => e.Nome)
                .HasMaxLength(200)
                .HasColumnName("nome");
            entity.Property(e => e.Tipocateria).HasColumnName("tipocateria");
        });

        modelBuilder.Entity<Ciaaerea>(entity =>
        {
            entity.HasKey(e => e.Ciaaereaid).HasName("pk_ciaaerea");

            entity.ToTable("ciaaerea");

            entity.Property(e => e.Ciaaereaid)
                .HasMaxLength(64)
                .HasColumnName("ciaaereaid");
            entity.Property(e => e.Ativo).HasColumnName("ativo");
            entity.Property(e => e.Descricao)
                .HasMaxLength(4000)
                .HasColumnName("descricao");
            entity.Property(e => e.DescricaoLonga).HasColumnName("descricao_longa");
            entity.Property(e => e.Lotipo)
                .HasMaxLength(300)
                .HasColumnName("lotipo");
            entity.Property(e => e.Tipoint).HasColumnName("tipoint");
            entity.Property(e => e.Tiponac).HasColumnName("tiponac");
            entity.Property(e => e.WebticketStr)
                .HasMaxLength(450)
                .HasColumnName("webticket_str");
        });

        modelBuilder.Entity<Conteudo>(entity =>
        {
            entity.HasKey(e => e.Conteudoid).HasName("pk_conteudo");

            entity.ToTable("conteudo");

            entity.Property(e => e.Conteudoid)
                .HasMaxLength(64)
                .HasColumnName("conteudoid");
            entity.Property(e => e.Areaid)
                .HasMaxLength(64)
                .HasColumnName("areaid");
            entity.Property(e => e.Autor)
                .HasMaxLength(80)
                .HasColumnName("autor");
            entity.Property(e => e.Cateriaid)
                .HasMaxLength(64)
                .HasColumnName("cateriaid");
            entity.Property(e => e.Datafinal)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("datafinal");
            entity.Property(e => e.Datainclusao)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("datainclusao");
            entity.Property(e => e.Texto).HasColumnName("texto");
            entity.Property(e => e.Titulo)
                .HasMaxLength(80)
                .HasColumnName("titulo");
        });

        modelBuilder.Entity<Conteudovalor>(entity =>
        {
            entity.HasKey(e => e.Conteudoid).HasName("pk_conteudovalor");

            entity.ToTable("conteudovalor");

            entity.Property(e => e.Conteudoid)
                .HasMaxLength(64)
                .HasColumnName("conteudoid");
            entity.Property(e => e.Unidadeid).HasColumnName("unidadeid");
            entity.Property(e => e.Valor)
                .HasPrecision(10, 2)
                .HasColumnName("valor");
        });

        modelBuilder.Entity<DictSocialmedium>(entity =>
        {
            entity.HasKey(e => e.Socialmediaid).HasName("pk_dict_socialmedia");

            entity.ToTable("dict_socialmedia");

            entity.Property(e => e.Socialmediaid)
                .ValueGeneratedNever()
                .HasColumnName("socialmediaid");
            entity.Property(e => e.Socialmedianame)
                .HasMaxLength(45)
                .HasColumnName("socialmedianame");
            entity.Property(e => e.Socialmediaurl)
                .HasMaxLength(45)
                .HasColumnName("socialmediaurl");
        });

        modelBuilder.Entity<DictTemplate>(entity =>
        {
            entity.HasKey(e => e.Idtemplate).HasName("pk_dict_templates");

            entity.ToTable("dict_templates");

            entity.Property(e => e.Idtemplate)
                .HasMaxLength(100)
                .HasColumnName("idtemplate");
            entity.Property(e => e.Descricao)
                .HasMaxLength(255)
                .HasColumnName("descricao");
            entity.Property(e => e.Nome)
                .HasMaxLength(255)
                .HasColumnName("nome");
            entity.Property(e => e.Viewrelacionada)
                .HasMaxLength(45)
                .HasColumnName("viewrelacionada");
        });

        modelBuilder.Entity<Dictarea>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("pk_dictareas");

            entity.ToTable("dictareas");

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("id");
            entity.Property(e => e.Nome)
                .HasMaxLength(12)
                .HasColumnName("nome");
            entity.Property(e => e.Tipo).HasColumnName("tipo");
        });

        modelBuilder.Entity<Formulario>(entity =>
        {
            entity.HasKey(e => new { e.Formularioid, e.Nome }).HasName("pk_formulario");

            entity.ToTable("formulario");

            entity.Property(e => e.Formularioid)
                .HasMaxLength(64)
                .HasColumnName("formularioid");
            entity.Property(e => e.Nome)
                .HasMaxLength(255)
                .HasColumnName("nome");
            entity.Property(e => e.Areaid)
                .HasMaxLength(64)
                .HasColumnName("areaid");
            entity.Property(e => e.Ativo)
                .HasColumnType("bit(1)")
                .HasColumnName("ativo");
            entity.Property(e => e.Datainclusao)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("datainclusao");
            entity.Property(e => e.Valor)
                .HasMaxLength(8000)
                .HasColumnName("valor");
            entity.Property(e => e.Categoriaid)
                .HasMaxLength(64)
                .HasColumnName("categoriaid");
        });

        modelBuilder.Entity<Faq>(entity =>
        {
            entity.HasKey(e => e.Faqid).HasName("pk_faq");
            entity.ToTable("faq");
            entity.Property(e => e.Faqid).HasMaxLength(64).HasColumnName("faqid");
            entity.Property(e => e.Formularioid).HasMaxLength(64).HasColumnName("formularioid");
            entity.Property(e => e.Pergunta).HasMaxLength(500).HasColumnName("pergunta");
            entity.Property(e => e.Resposta).HasColumnName("resposta");
            entity.Property(e => e.Ordem).HasColumnName("ordem");
            entity.Property(e => e.Ativo).HasColumnName("ativo");
            entity.Property(e => e.Datainclusao).HasColumnName("datainclusao");
        });

        modelBuilder.Entity<Formularionew>(entity =>
        {
            entity.HasKey(e => e.Idform).HasName("pk_formularionew");

            entity.ToTable("formularionew");

            entity.Property(e => e.Idform).HasColumnName("idform");
            entity.Property(e => e.Ativo).HasColumnName("ativo");
            entity.Property(e => e.Email)
                .HasMaxLength(155)
                .HasColumnName("email");
            entity.Property(e => e.Nome)
                .HasMaxLength(100)
                .HasColumnName("nome");
            entity.Property(e => e.Telefone)
                .HasMaxLength(15)
                .HasColumnName("telefone");
            entity.Property(e => e.Texto)
                .HasMaxLength(255)
                .HasColumnName("texto");
            entity.Property(e => e.Tipo).HasColumnName("tipo");
            entity.Property(e => e.Formularioid)
                .HasMaxLength(64)
                .HasColumnName("formularioid");
        });

        modelBuilder.Entity<Imagem>(entity =>
        {
            entity.HasKey(e => e.Imagemid).HasName("pk_imagem");

            entity.ToTable("imagem");

            entity.Property(e => e.Imagemid)
                .HasMaxLength(64)
                .HasColumnName("imagemid");
            entity.Property(e => e.Altura).HasColumnName("altura");
            entity.Property(e => e.Areaid)
                .HasMaxLength(64)
                .HasColumnName("areaid");
            entity.Property(e => e.Conteudoid)
                .HasMaxLength(64)
                .HasColumnName("conteudoid");
            entity.Property(e => e.Descricao)
                .HasMaxLength(255)
                .HasColumnName("descricao");
            entity.Property(e => e.Largura).HasColumnName("largura");
            entity.Property(e => e.Parentid)
                .HasMaxLength(64)
                .HasColumnName("parentid");
            entity.Property(e => e.Tipoid)
                .HasMaxLength(64)
                .HasColumnName("tipoid");
            entity.Property(e => e.Url)
                .HasMaxLength(300)
                .HasColumnName("url");
        });

        modelBuilder.Entity<Infofoto>(entity =>
        {
            entity.HasKey(e => new { e.Fotoid, e.Cateriaid }).HasName("pk_infofoto");

            entity.ToTable("infofoto");

            entity.Property(e => e.Fotoid)
                .HasMaxLength(64)
                .HasColumnName("fotoid");
            entity.Property(e => e.Cateriaid)
                .HasMaxLength(64)
                .HasColumnName("cateriaid");
            entity.Property(e => e.Descricao)
                .HasMaxLength(1000)
                .HasColumnName("descricao");
            entity.Property(e => e.Fotourl)
                .HasMaxLength(300)
                .HasColumnName("fotourl");
        });

        modelBuilder.Entity<Informativo>(entity =>
        {
            entity.HasKey(e => e.Infoid).HasName("pk_informativo");

            entity.ToTable("informativo");

            entity.Property(e => e.Infoid)
                .HasMaxLength(64)
                .HasColumnName("infoid");
            entity.Property(e => e.Ativo).HasColumnName("ativo");
            entity.Property(e => e.Data)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("data");
            entity.Property(e => e.Foto)
                .HasMaxLength(64)
                .HasColumnName("foto");
            entity.Property(e => e.Texto).HasColumnName("texto");
            entity.Property(e => e.Tipoenvio)
                .HasMaxLength(64)
                .HasColumnName("tipoenvio");
            entity.Property(e => e.Titulo)
                .HasMaxLength(300)
                .HasColumnName("titulo");
        });

        modelBuilder.Entity<Modulo>(entity =>
        {
            entity
                .HasNoKey()
                .ToTable("modulo");

            entity.Property(e => e.Moduloid)
                .HasDefaultValueSql("uuid_generate_v4()")
                .HasColumnName("moduloid");
            entity.Property(e => e.Nome)
                .HasMaxLength(400)
                .HasColumnName("nome");
            entity.Property(e => e.Posicao).HasColumnName("posicao");
            entity.Property(e => e.Url)
                .HasMaxLength(400)
                .HasColumnName("url");
        });

        modelBuilder.Entity<Moduloconf>(entity =>
        {
            entity.HasKey(e => e.Moduloconfid).HasName("pk_moduloconf");

            entity.ToTable("moduloconf");

            entity.Property(e => e.Moduloconfid)
                .HasMaxLength(64)
                .HasColumnName("moduloconfid");
            entity.Property(e => e.Descricao)
                .HasMaxLength(800)
                .HasColumnName("descricao");
            entity.Property(e => e.Nome)
                .HasMaxLength(200)
                .HasColumnName("nome");
        });

        modelBuilder.Entity<Moedum>(entity =>
        {
            entity.HasKey(e => new { e.Moedaid, e.Nome }).HasName("pk_moeda");

            entity.ToTable("moeda");

            entity.Property(e => e.Moedaid)
                .HasMaxLength(64)
                .HasColumnName("moedaid");
            entity.Property(e => e.Nome)
                .HasMaxLength(80)
                .HasColumnName("nome");
            entity.Property(e => e.Sigla)
                .HasMaxLength(34)
                .IsFixedLength()
                .HasColumnName("sigla");
        });

        modelBuilder.Entity<Newsletter>(entity =>
        {
            entity.HasKey(e => e.Newsid).HasName("pk_newsletter");

            entity.ToTable("newsletter");

            entity.Property(e => e.Newsid)
                .HasMaxLength(64)
                .HasColumnName("newsid");
            entity.Property(e => e.Ativo).HasColumnName("ativo");
            entity.Property(e => e.Autor)
                .HasMaxLength(1000)
                .HasColumnName("autor");
            entity.Property(e => e.Cateriaid)
                .HasMaxLength(64)
                .HasColumnName("cateriaid");
            entity.Property(e => e.Data)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("data");
            entity.Property(e => e.Foto)
                .HasMaxLength(4000)
                .HasColumnName("foto");
            entity.Property(e => e.Frente).HasColumnName("frente");
            entity.Property(e => e.Texto).HasColumnName("texto");
            entity.Property(e => e.Titulo)
                .HasMaxLength(3000)
                .HasColumnName("titulo");
        });

        modelBuilder.Entity<Opcao>(entity =>
        {
            entity.HasKey(e => e.Opcaoid).HasName("pk_opcao");

            entity.ToTable("opcao");

            entity.Property(e => e.Opcaoid)
                .HasMaxLength(64)
                .HasColumnName("opcaoid");
            entity.Property(e => e.Atributoid).HasColumnName("atributoid");
            entity.Property(e => e.Descricao)
                .HasMaxLength(500)
                .HasColumnName("descricao");
            entity.Property(e => e.Estoque).HasColumnName("estoque");
            entity.Property(e => e.Nome)
                .HasMaxLength(500)
                .HasColumnName("nome");
            entity.Property(e => e.Qtd).HasColumnName("qtd");
        });

        modelBuilder.Entity<Produto>(entity =>
        {
            entity.HasKey(e => new { e.Produtoid, e.Sku }).HasName("pk_produto");

            entity.ToTable("produto");

            entity.Property(e => e.Produtoid)
                .HasMaxLength(64)
                .HasColumnName("produtoid");
            entity.Property(e => e.Sku)
                .HasMaxLength(45)
                .HasColumnName("sku");
            entity.Property(e => e.Aplicacaoid)
                .HasMaxLength(64)
                .HasColumnName("aplicacaoid");
            entity.Property(e => e.Cateriaid)
                .HasMaxLength(64)
                .HasColumnName("cateriaid");
            entity.Property(e => e.Datafinal)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("datafinal");
            entity.Property(e => e.Datainicio)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("datainicio");
            entity.Property(e => e.Descricacurta)
                .HasMaxLength(150)
                .HasColumnName("descricacurta");
            entity.Property(e => e.Descricao).HasColumnName("descricao");
            entity.Property(e => e.Destaque).HasColumnName("destaque");
            entity.Property(e => e.Detalhetecnico).HasColumnName("detalhetecnico");
            entity.Property(e => e.Nome)
                .HasMaxLength(130)
                .HasColumnName("nome");
            entity.Property(e => e.Pagsegurokey)
                .HasMaxLength(800)
                .HasColumnName("pagsegurokey");
            entity.Property(e => e.Produtocol)
                .HasMaxLength(45)
                .HasColumnName("produtocol");
            entity.Property(e => e.Tipo).HasColumnName("tipo");
            entity.Property(e => e.Valor)
                .HasPrecision(18, 2)
                .HasColumnName("valor");
        });

        modelBuilder.Entity<Refatributoxopcao>(entity =>
        {
            entity.HasKey(e => e.Relacaoid).HasName("pk_refatributoxopcao");

            entity.ToTable("refatributoxopcao");

            entity.Property(e => e.Relacaoid)
                .ValueGeneratedNever()
                .HasColumnName("relacaoid");
            entity.Property(e => e.Atributoid).HasColumnName("atributoid");
            entity.Property(e => e.Opcaoid).HasColumnName("opcaoid");
        });

        modelBuilder.Entity<Relatributoxproduto>(entity =>
        {
            entity.HasKey(e => e.Relacaoid).HasName("pk_relatributoxproduto");

            entity.ToTable("relatributoxproduto");

            entity.Property(e => e.Relacaoid)
                .ValueGeneratedNever()
                .HasColumnName("relacaoid");
            entity.Property(e => e.Atributoid).HasColumnName("atributoid");
            entity.Property(e => e.Produtoid).HasColumnName("produtoid");
        });

        modelBuilder.Entity<Relimagemconteudo>(entity =>
        {
            entity.HasKey(e => e.Relid).HasName("pk_relimagemconteudo");

            entity.ToTable("relimagemconteudo");

            entity.Property(e => e.Relid).HasColumnName("relid");
            entity.Property(e => e.Imagemid)
                .HasMaxLength(45)
                .HasColumnName("imagemid");
            entity.Property(e => e.Parentid)
                .HasMaxLength(45)
                .HasColumnName("parentid");
        });

        modelBuilder.Entity<Relmoduloaplicacao>(entity =>
        {
            entity.HasKey(e => new { e.Relacaoid, e.Aplicacaoid, e.Moduloid }).HasName("pk_relmoduloaplicacao");

            entity.ToTable("relmoduloaplicacao");

            entity.Property(e => e.Relacaoid)
                .HasMaxLength(64)
                .HasColumnName("relacaoid");
            entity.Property(e => e.Aplicacaoid)
                .HasMaxLength(64)
                .HasColumnName("aplicacaoid");
            entity.Property(e => e.Moduloid)
                .HasMaxLength(64)
                .HasColumnName("moduloid");
        });

        modelBuilder.Entity<Relmoduloconfaplicacao>(entity =>
        {
            entity.HasKey(e => new { e.Relacaoid, e.Moduloconfid, e.Aplicacaoid, e.Datainclusao, e.Datafinalizacao }).HasName("pk_relmoduloconfaplicacao");

            entity.ToTable("relmoduloconfaplicacao");

            entity.Property(e => e.Relacaoid)
                .HasMaxLength(64)
                .HasColumnName("relacaoid");
            entity.Property(e => e.Moduloconfid)
                .HasMaxLength(64)
                .HasColumnName("moduloconfid");
            entity.Property(e => e.Aplicacaoid)
                .HasMaxLength(64)
                .HasColumnName("aplicacaoid");
            entity.Property(e => e.Datainclusao)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("datainclusao");
            entity.Property(e => e.Datafinalizacao)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("datafinalizacao");
        });

        modelBuilder.Entity<Relmodulousuario>(entity =>
        {
            entity
                .HasNoKey()
                .ToTable("relmodulousuario");

            entity.Property(e => e.Moduloid).HasColumnName("moduloid");
            entity.Property(e => e.Relacaoid)
                .HasDefaultValueSql("uuid_generate_v4()")
                .HasColumnName("relacaoid");
            entity.Property(e => e.Usuarioid).HasColumnName("usuarioid");
        });

        modelBuilder.Entity<Relusuarioaplicacao>(entity =>
        {
            entity
                .HasNoKey()
                .ToTable("relusuarioaplicacao");

            entity.Property(e => e.Aplicacaoid).HasColumnName("aplicacaoid");
            entity.Property(e => e.Relacaoid)
                .HasDefaultValueSql("uuid_generate_v4()")
                .HasColumnName("relacaoid");
            entity.Property(e => e.Usuarioid).HasColumnName("usuarioid");
        });

        modelBuilder.Entity<RelusuarioaplicacaoOld>(entity =>
        {
            entity.HasKey(e => new { e.Relacaoid, e.Aplicacaoid, e.Usuarioid }).HasName("pk_relusuarioaplicacao");

            entity.ToTable("relusuarioaplicacaoOld");

            entity.Property(e => e.Relacaoid)
                .HasMaxLength(64)
                .HasDefaultValueSql("uuid_generate_v4()")
                .HasColumnName("relacaoid");
            entity.Property(e => e.Aplicacaoid)
                .HasMaxLength(64)
                .HasColumnName("aplicacaoid");
            entity.Property(e => e.Usuarioid)
                .HasMaxLength(64)
                .HasColumnName("usuarioid");
        });

        modelBuilder.Entity<Socialmedium>(entity =>
        {
            entity.HasKey(e => e.Socialmediaid).HasName("pk_socialmedia");

            entity.ToTable("socialmedia");

            entity.Property(e => e.Socialmediaid)
                .HasMaxLength(45)
                .HasColumnName("socialmediaid");
            entity.Property(e => e.Aplicacaoid)
                .HasMaxLength(45)
                .HasColumnName("aplicacaoid");
            entity.Property(e => e.Socialmedialink)
                .HasMaxLength(255)
                .HasColumnName("socialmedialink");
            entity.Property(e => e.Socialmediatypeid).HasColumnName("socialmediatypeid");
        });

        modelBuilder.Entity<TblCliente>(entity =>
        {
            entity
                .HasNoKey()
                .ToTable("tbl_cliente");

            entity.Property(e => e.Id)
                .ValueGeneratedOnAdd()
                .HasColumnName("id");
            entity.Property(e => e.Idade).HasColumnName("idade");
            entity.Property(e => e.Nome)
                .HasMaxLength(255)
                .HasColumnName("nome");
        });

        modelBuilder.Entity<Template>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("pk_templates");

            entity.ToTable("templates");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Ativo)
                .HasColumnType("bit(1)")
                .HasColumnName("ativo");
            entity.Property(e => e.Descricao)
                .HasMaxLength(255)
                .HasColumnName("descricao");
            entity.Property(e => e.Nome)
                .HasMaxLength(155)
                .HasColumnName("nome");
            entity.Property(e => e.Url)
                .HasMaxLength(80)
                .HasColumnName("url");
        });

        modelBuilder.Entity<Tipocotacao>(entity =>
        {
            entity.HasKey(e => e.Tipocotacaoid).HasName("pk_tipocotacao");

            entity.ToTable("tipocotacao");

            entity.Property(e => e.Tipocotacaoid)
                .HasMaxLength(64)
                .HasColumnName("tipocotacaoid");
            entity.Property(e => e.Descricao)
                .HasMaxLength(200)
                .HasColumnName("descricao");
            entity.Property(e => e.Nome)
                .HasMaxLength(200)
                .HasColumnName("nome");
        });

        modelBuilder.Entity<Tipoenvio>(entity =>
        {
            entity.HasKey(e => e.Tipoenvioid).HasName("pk_tipoenvio");

            entity.ToTable("tipoenvio");

            entity.Property(e => e.Tipoenvioid)
                .HasMaxLength(64)
                .HasColumnName("tipoenvioid");
            entity.Property(e => e.Tipoenviodesc)
                .HasMaxLength(300)
                .HasColumnName("tipoenviodesc");
        });

        modelBuilder.Entity<Unidade>(entity =>
        {
            entity.HasKey(e => e.Unidadeid).HasName("pk_unidades");

            entity.ToTable("unidades");

            entity.Property(e => e.Unidadeid)
                .ValueGeneratedNever()
                .HasColumnName("unidadeid");
            entity.Property(e => e.Nome)
                .HasMaxLength(45)
                .HasColumnName("nome");
            entity.Property(e => e.Sigla)
                .HasMaxLength(45)
                .HasColumnName("sigla");
        });

        modelBuilder.Entity<Usuario>(entity =>
        {
            entity
                .HasNoKey()
                .ToTable("usuario");

            entity.Property(e => e.Apelido)
                .HasMaxLength(6)
                .HasColumnName("apelido");
            entity.Property(e => e.Ativo).HasColumnName("ativo");
            entity.Property(e => e.Datainclusao)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("datainclusao");
            entity.Property(e => e.Nome)
                .HasMaxLength(300)
                .HasColumnName("nome");
            entity.Property(e => e.Senha)
                .HasMaxLength(12)
                .HasColumnName("senha");
            entity.Property(e => e.Sobrenome)
                .HasMaxLength(300)
                .HasColumnName("sobrenome");
            entity.Property(e => e.Userid)
                .HasDefaultValueSql("uuid_generate_v4()")
                .HasColumnName("userid");
        });

        modelBuilder.Entity<Pedido>(entity =>
        {
            entity.HasKey(e => e.Pedidoid).HasName("pedidoPK");
            entity.ToTable("pedido");
            entity.Property(e => e.Pedidoid)
                .HasDefaultValueSql("NEWID()")
                .HasColumnName("pedidoid");
            entity.Property(e => e.Aplicacaoid).HasMaxLength(36).HasColumnName("aplicacaoid");
            entity.Property(e => e.Numeropedido).HasMaxLength(100).HasColumnName("numeropedido");
            entity.Property(e => e.Clientenome).HasMaxLength(200).HasColumnName("clientenome");
            entity.Property(e => e.Clienteemail).HasMaxLength(200).HasColumnName("clienteemail");
            entity.Property(e => e.Valorpedido).HasColumnType("decimal(12,2)").HasColumnName("valorpedido");
            entity.Property(e => e.Statusatual).HasMaxLength(50).HasColumnName("statusatual");
            entity.Property(e => e.Datainclusao)
                .HasDefaultValueSql("GETUTCDATE()")
                .HasColumnType("datetime2")
                .HasColumnName("datainclusao");
        });

        modelBuilder.Entity<Statuspedido>(entity =>
        {
            entity.HasKey(e => e.Statuspedidoid).HasName("statuspedidoPK");
            entity.ToTable("statuspedido");
            entity.Property(e => e.Statuspedidoid)
                .HasDefaultValueSql("NEWID()")
                .HasColumnName("statuspedidoid");
            entity.Property(e => e.Pedidoid).HasColumnName("pedidoid");
            entity.Property(e => e.Status).HasMaxLength(50).HasColumnName("status");
            entity.Property(e => e.Descricao).HasMaxLength(500).HasColumnName("descricao");
            entity.Property(e => e.Datahora)
                .HasDefaultValueSql("GETUTCDATE()")
                .HasColumnType("datetime2")
                .HasColumnName("datahora");
            entity.HasOne(e => e.PedidoNavigation)
                .WithMany(p => p.Statuspedidos)
                .HasForeignKey(e => e.Pedidoid)
                .HasConstraintName("statuspedido_pedidoid_fk");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
