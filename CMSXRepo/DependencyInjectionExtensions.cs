using CMSXData.Models;
using ICMSX;
using Microsoft.Extensions.DependencyInjection;

namespace CMSXRepo;

public static class DependencyInjectionExtensions
{
    public static IServiceCollection AddCMSXRepo(this IServiceCollection services)
    {
        services.AddScoped<IAcessoRepositorio, AcessoRepositorio>();
        services.AddScoped<IAplicacaoRepositorio, AplicacaoRepositorio>();
        services.AddScoped<IAreasRepositorio, AreasRepositorio>();
        services.AddScoped<IArquivoRepositorio, ArquivoRepositorio>();
        services.AddScoped<IAtributoRepositorio, AtributoRepositorio>();
        services.AddScoped<ICategoriaRepositorio, CategoriaRepositorio>();
        services.AddScoped<IConteudoRepositorio, ConteudoRepositorio>();
        services.AddScoped<IFormularioRepositorio, FormularioRepositorio>();
        services.AddScoped<IImagemRepositorio, ImagemRepositorio>();
        services.AddScoped<IMenuRepositorio, MenuRepositorio>();
        services.AddScoped<IModuloRepositorio, ModuloRepositorio>();
        services.AddScoped<IOpcaoRepositorio, OpcaoRepositorio>();
        services.AddScoped<IProdutoRepositorio, ProdutoRepositorio>();
        services.AddScoped<IRelacaoRepositorio, RelacaoRepositorio>();
        services.AddScoped<IUsuarioRepositorio, UsuarioRepositorio>();
        services.AddScoped<IClienteLojaRepositorio, ClienteLojaRepositorio>();
        services.AddScoped<IOrcamentoRepositorio, OrcamentoRepositorio>();
        services.AddScoped<ILojaRepositorio, LojaRepositorio>();

        return services;
    }
}
