﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace CMSXDB
{
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Infrastructure;
    
    public partial class cmsxDBEntities : DbContext
    {
        public cmsxDBEntities()
            : base("name=cmsxDBEntities")
        {
        }
    
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            throw new UnintentionalCodeFirstException();
        }
    
        public virtual DbSet<arquivo> arquivo { get; set; }
        public virtual DbSet<atributo> atributo { get; set; }
        public virtual DbSet<conteudo> conteudo { get; set; }
        public virtual DbSet<conteudovalor> conteudovalor { get; set; }
        public virtual DbSet<dict_templates> dict_templates { get; set; }
        public virtual DbSet<dictareas> dictareas { get; set; }
        public virtual DbSet<formulario> formulario { get; set; }
        public virtual DbSet<formularionew> formularionew { get; set; }
        public virtual DbSet<imagem> imagem { get; set; }
        public virtual DbSet<modulo> modulo { get; set; }
        public virtual DbSet<opcao> opcao { get; set; }
        public virtual DbSet<produto> produto { get; set; }
        public virtual DbSet<refatributoxopcao> refatributoxopcao { get; set; }
        public virtual DbSet<relatributoxproduto> relatributoxproduto { get; set; }
        public virtual DbSet<unidades> unidades { get; set; }
        public virtual DbSet<usuario> usuario { get; set; }
        public virtual DbSet<cambio> cambio { get; set; }
        public virtual DbSet<categoria> categoria { get; set; }
        public virtual DbSet<ciaaerea> ciaaerea { get; set; }
        public virtual DbSet<infofoto> infofoto { get; set; }
        public virtual DbSet<informativo> informativo { get; set; }
        public virtual DbSet<moduloconf> moduloconf { get; set; }
        public virtual DbSet<moeda> moeda { get; set; }
        public virtual DbSet<newsletter> newsletter { get; set; }
        public virtual DbSet<relmoduloaplicacao> relmoduloaplicacao { get; set; }
        public virtual DbSet<relmoduloconfaplicacao> relmoduloconfaplicacao { get; set; }
        public virtual DbSet<relusuarioaplicacao> relusuarioaplicacao { get; set; }
        public virtual DbSet<tipocotacao> tipocotacao { get; set; }
        public virtual DbSet<tipoenvio> tipoenvio { get; set; }
        public virtual DbSet<socialmedia> socialmedia { get; set; }
        public virtual DbSet<dict_socialmedia> dict_socialmedia { get; set; }
        public virtual DbSet<aplicacao> aplicacao { get; set; }
        public virtual DbSet<areas> areas { get; set; }
        public virtual DbSet<relimagemconteudo> relimagemconteudo { get; set; }
        public virtual DbSet<templates> templates { get; set; }
    }
}
