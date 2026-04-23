using Microsoft.Extensions.DependencyInjection;
using ICMSX;

namespace CMSXDAO
{
    public static class DependencyInjectionExtensions
    {
        public static IServiceCollection AddCMSXDAL(this IServiceCollection services)
        {
            services.AddScoped<IDataFactory, DataFactory>();
            services.AddScoped<IAcessoDAL, AcessoDAL>();
            services.AddScoped<IAplicacaoDAL, AplicacaoDAL>();
            services.AddScoped<IAreasDAL, AreasDAL>();
            services.AddScoped<IArquivoDAL, ArquivoDAL>();
            services.AddScoped<IAtributoDAL, AtributoDAL>();
            services.AddScoped<ICategoriaDAL, CategoriaDAL>();
            services.AddScoped<IConteudoDAL, ConteudoDAL>();
            services.AddScoped<IFormularioDAL, FormularioDAL>();
            services.AddScoped<IGaleriaDAL, GaleriaDAL>();
            services.AddScoped<IImagem, ImagemDAL>();
            services.AddScoped<IMenuDAL, MenuDAL>();
            services.AddScoped<IModuloDAL, ModuloDAL>();
            services.AddScoped<IOpcaoDAL, OpcaoDAL>();
            services.AddScoped<IProdutoDAL, ProdutoDAL>();
            services.AddScoped<IRelacaoDAL, RelacaoDAL>();
            services.AddScoped<IResortDAL, ResortDAL>();
            services.AddScoped<IRoteiroDAL, RoteiroDAL>();
            services.AddScoped<IUsuarioDAL, UsuarioDAL>();
            services.AddScoped<IClienteLojaDAL, ClienteLojaDAL>();
            return services;
        }
    }
}
