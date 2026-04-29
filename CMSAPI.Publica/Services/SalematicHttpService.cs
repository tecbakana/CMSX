using System.Text;
using System.Text.Json;

namespace CMSAPIPublica.Services;

public class SalematicHttpService(HttpClient http, ILogger<SalematicHttpService> logger)
{
    private static readonly JsonSerializerOptions _jsonOptions = new() { PropertyNameCaseInsensitive = true };

    public async Task<SalematicAuthResponse?> RegistrarAsync(RegistrarLojaRequest req)
    {
        try
        {
            var payload = new
            {
                AplicacaoId = req.Aplicacaoid,
                req.Nome, req.Documento, req.Email, req.Senha, req.Telefone,
                req.Cep, req.Logradouro, req.Numero, req.Complemento,
                req.Bairro, req.Cidade, req.Estado
            };
            var content = new StringContent(JsonSerializer.Serialize(payload, _jsonOptions), Encoding.UTF8, "application/json");
            var response = await http.PostAsync("api/auth/register", content);
            response.EnsureSuccessStatusCode();
            var body = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<SalematicAuthResponse>(body, _jsonOptions);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Erro ao registrar cliente na Salematic");
            return null;
        }
    }

    public async Task<SalematicAuthResponse?> LoginAsync(LoginLojaRequest req)
    {
        try
        {
            var payload = new { req.Email, req.Senha };
            var content = new StringContent(JsonSerializer.Serialize(payload, _jsonOptions), Encoding.UTF8, "application/json");
            var response = await http.PostAsync("api/auth/login", content);
            response.EnsureSuccessStatusCode();
            var body = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<SalematicAuthResponse>(body, _jsonOptions);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Erro ao fazer login na Salematic");
            return null;
        }
    }

    public async Task<bool> EsqueciSenhaAsync(EsqueciSenhaRequest req)
    {
        try
        {
            var payload = new { req.Email };
            var content = new StringContent(JsonSerializer.Serialize(payload, _jsonOptions), Encoding.UTF8, "application/json");
            var response = await http.PostAsync("api/auth/forgot-password", content);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Erro ao solicitar reset de senha na Salematic");
            return false;
        }
    }

    public async Task<bool> ResetSenhaAsync(ResetSenhaRequest req)
    {
        try
        {
            var payload = new { req.Token, NewPassword = req.NovaSenha };
            var content = new StringContent(JsonSerializer.Serialize(payload, _jsonOptions), Encoding.UTF8, "application/json");
            var response = await http.PostAsync("api/auth/reset-password", content);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Erro ao redefinir senha na Salematic");
            return false;
        }
    }
}
