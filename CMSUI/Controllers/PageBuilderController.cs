using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using CMSUI.Models;
using CMSUI.Services;

namespace CMSUI.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class PageBuilderController : Controller
    {
        private readonly CmsxDbContext _context;
        private readonly IAgentIAFactory _agentFactory;
        private readonly IConfiguration _config;
        private readonly IHttpClientFactory _httpClientFactory;

        public PageBuilderController(CmsxDbContext context, IAgentIAFactory agentFactory, IConfiguration config, IHttpClientFactory httpClientFactory)
        {
            _context = context;
            _agentFactory = agentFactory;
            _config = config;
            _httpClientFactory = httpClientFactory;
        }

        private (bool acessoTotal, string? aplicacaoid) UserContext() =>
            (User.FindFirstValue("acessoTotal") == "True", User.FindFirstValue("aplicacaoid"));

        // ── Catálogo de blocos ───────────────────────────────────────────────

        [AllowAnonymous]
        [HttpGet("blocos")]
        public IActionResult GetBlocos() =>
            Ok(_context.DictBlocos.OrderBy(b => b.Nome).ToArray());

        // ── Resumo de layouts salvos ─────────────────────────────────────────

        [Authorize]
        [HttpGet("layouts-resumo")]
        public IActionResult GetLayoutsResumo()
        {
            var (acessoTotal, claimAppId) = UserContext();
            var areas = _context.Areas
                .Where(a => acessoTotal || a.Aplicacaoid == claimAppId)
                .Where(a => a.Layout != null && a.Layout != "{\"blocos\":[]}")
                .Select(a => new { a.Areaid, a.Nome, a.Layout })
                .ToList()
                .Select(a => {
                    int qtd = 0;
                    try { qtd = JsonDocument.Parse(a.Layout!).RootElement.GetProperty("blocos").GetArrayLength(); } catch { }
                    return new { a.Areaid, a.Nome, QtdBlocos = qtd };
                });
            return Ok(areas);
        }

        // ── Layout de uma área ───────────────────────────────────────────────

        [Authorize]
        [HttpGet("layout/{areaid}")]
        public IActionResult GetLayout(string areaid)
        {
            var (acessoTotal, claimAppId) = UserContext();
            var area = _context.Areas.FirstOrDefault(a => a.Areaid == areaid);
            if (area == null) return NotFound();
            if (!acessoTotal && area.Aplicacaoid != claimAppId) return Forbid();

            return Ok(new { area.Areaid, area.Nome, layout = area.Layout ?? "{\"blocos\":[]}" });
        }

        [Authorize]
        [HttpPut("layout/{areaid}")]
        public IActionResult SaveLayout(string areaid, [FromBody] JsonElement payload)
        {
            var (acessoTotal, claimAppId) = UserContext();
            var area = _context.Areas.FirstOrDefault(a => a.Areaid == areaid);
            if (area == null) return NotFound();
            if (!acessoTotal && area.Aplicacaoid != claimAppId) return Forbid();

            area.Layout = payload.GetRawText();
            _context.SaveChanges();
            return Ok();
        }

        // ── Contexto para o agente IA ────────────────────────────────────────

        [Authorize]
        [HttpGet("contexto-ia")]
        public IActionResult GetContextoIA()
        {
            var (acessoTotal, claimAppId) = UserContext();

            var blocos = _context.DictBlocos
                .OrderBy(b => b.Nome)
                .Select(b => new { b.Tipobloco, b.Nome, b.Descricao, b.SchemaConfig })
                .ToList();

            var areas = _context.Areas
                .Where(a => acessoTotal || a.Aplicacaoid == claimAppId)
                .Select(a => new { a.Areaid, a.Nome })
                .ToList();

            var formularios = _context.Formularios
                .Where(f => _context.Areas
                    .Where(a => acessoTotal || a.Aplicacaoid == claimAppId)
                    .Select(a => a.Areaid)
                    .Contains(f.Areaid))
                .Select(f => new { f.Formularioid, f.Nome })
                .ToList();

            var categorias = _context.Cateria
                .Where(c => acessoTotal || c.Aplicacaoid == claimAppId)
                .Select(c => new { c.Cateriaid, c.Nome, c.Cateriaidpai })
                .ToList();

            return Ok(new { blocos, areas, formularios, categorias });
        }

        // ── Configuração de IA do tenant ─────────────────────────────────────

        [Authorize]
        [HttpGet("ia-config")]
        public IActionResult GetIaConfig()
        {
            var (_, claimAppId) = UserContext();
            if (claimAppId == null) return BadRequest();

            var config = _context.IaConfigs.FirstOrDefault(c => c.Aplicacaoid == claimAppId);
            var limitePadrao = _config.GetValue<int>("AgentIA:LimiteDiarioPadrao", 20);

            // Uso de hoje
            var hoje = DateOnly.FromDateTime(DateTime.Today);
            var uso = _context.IaUsos.FirstOrDefault(u => u.Aplicacaoid == claimAppId && u.Data == hoje);

            return Ok(new
            {
                provedor      = config?.Provedor,
                temChavePropria = !string.IsNullOrWhiteSpace(config?.Apikey),
                modelo        = config?.Modelo,
                limiteDiario  = config?.LimiteDiario ?? limitePadrao,
                usoHoje       = uso?.Contador ?? 0
            });
        }

        public class IaConfigDto
        {
            public string? Provedor { get; set; }
            public string? Apikey { get; set; }
            public string? Modelo { get; set; }
            public int? LimiteDiario { get; set; }
        }

        [Authorize]
        [HttpPut("ia-config")]
        public IActionResult SaveIaConfig([FromBody] IaConfigDto dto)
        {
            var (_, claimAppId) = UserContext();
            if (claimAppId == null) return BadRequest();

            var config = _context.IaConfigs.FirstOrDefault(c => c.Aplicacaoid == claimAppId);
            if (config == null)
            {
                config = new IaConfig { Aplicacaoid = claimAppId };
                _context.IaConfigs.Add(config);
            }

            config.Provedor     = dto.Provedor;
            config.Modelo       = dto.Modelo;
            config.LimiteDiario = dto.LimiteDiario;

            // Só atualiza a key se foi enviada (string não vazia)
            if (!string.IsNullOrWhiteSpace(dto.Apikey))
                config.Apikey = dto.Apikey;

            _context.SaveChanges();
            return Ok();
        }

        [Authorize]
        [HttpGet("unsplash-status")]
        public IActionResult GetUnsplashStatus() =>
            Ok(new { ativo = !string.IsNullOrWhiteSpace(_config["Unsplash:AccessKey"]) });

        [Authorize]
        [HttpDelete("ia-config/apikey")]
        public IActionResult RemoverApiKey()
        {
            var (_, claimAppId) = UserContext();
            var config = _context.IaConfigs.FirstOrDefault(c => c.Aplicacaoid == claimAppId);
            if (config != null) { config.Apikey = null; _context.SaveChanges(); }
            return Ok();
        }

        // ── Geração de layout via IA ─────────────────────────────────────────

        [Authorize]
        [HttpPost("gerar-layout")]
        public async Task<IActionResult> GerarLayout([FromBody] GerarLayoutDto dto)
        {
            var (acessoTotal, claimAppId) = UserContext();

            // Configuração de IA do tenant
            var iaConfig = claimAppId != null
                ? _context.IaConfigs.FirstOrDefault(c => c.Aplicacaoid == claimAppId)
                : null;
            var temChavePropria = !string.IsNullOrWhiteSpace(iaConfig?.Apikey);

            // Verifica limite diário (só se usar chave do sistema e não for admin)
            if (!acessoTotal && !temChavePropria && claimAppId != null)
            {
                var limitePadrao = _config.GetValue<int>("AgentIA:LimiteDiarioPadrao", 20);
                var limite = iaConfig?.LimiteDiario ?? limitePadrao;
                var hoje = DateOnly.FromDateTime(DateTime.Today);
                var uso = _context.IaUsos.FirstOrDefault(u => u.Aplicacaoid == claimAppId && u.Data == hoje);
                if (uso != null && uso.Contador >= limite)
                    return StatusCode(429, $"Limite diário de {limite} gerações atingido. Configure sua própria chave de IA nas configurações.");
            }

            // Monta o contexto do tenant
            var blocos = _context.DictBlocos
                .Select(b => new { b.Tipobloco, b.Nome, b.Descricao, b.SchemaConfig })
                .ToList();

            var areas = _context.Areas
                .Where(a => acessoTotal || a.Aplicacaoid == claimAppId)
                .Select(a => new { a.Areaid, a.Nome })
                .ToList();

            var formularios = _context.Formularios
                .Where(f => _context.Areas
                    .Where(a => acessoTotal || a.Aplicacaoid == claimAppId)
                    .Select(a => a.Areaid)
                    .Contains(f.Areaid))
                .Select(f => new { f.Formularioid, f.Nome })
                .ToList();

            var categorias = _context.Cateria
                .Where(c => acessoTotal || c.Aplicacaoid == claimAppId)
                .Select(c => new { c.Cateriaid, c.Nome })
                .ToList();

            var contexto = new
            {
                blocos_disponiveis = blocos,
                areas_do_tenant = areas,
                formularios_do_tenant = formularios,
                categorias_do_tenant = categorias
            };

            var areaNome = dto.Areaid != null
                ? _context.Areas.FirstOrDefault(a => a.Areaid == dto.Areaid)?.Nome
                : null;
            var contextoArea = areaNome != null ? $" para a área \"{areaNome}\"" : "";

            var prompt = $@"Você é um assistente de criação de páginas web.
O usuário quer criar uma página{contextoArea} com a seguinte descrição: ""{dto.Descricao}""

Você tem acesso aos seguintes blocos disponíveis e dados do tenant:
{JsonSerializer.Serialize(contexto, new JsonSerializerOptions { WriteIndented = false })}

Retorne APENAS um JSON válido no seguinte formato, sem texto adicional, sem markdown, sem explicações:
{{
  ""blocos"": [
    {{
      ""tipo"": ""<tipobloco>"",
      ""config"": {{ <propriedades do schema_config preenchidas com valores adequados> }}
    }}
  ]
}}

Regras importantes:
- Use no máximo 6 blocos
- Use os IDs reais dos dados do tenant quando necessário (areaid, formularioid, cateriaid)
- Ordene os blocos de forma que faça sentido visual para a página descrita
- O JSON deve ser completo e válido — não truncar";

            // Verifica cache
            var provedorEfetivo = dto.Provedor ?? iaConfig?.Provedor;
            var hashKey = ComputeHash($"{provedorEfetivo}:{prompt}");
            var cached = _context.IaCaches.FirstOrDefault(c => c.Hash == hashKey && c.Datavencimento > DateTime.UtcNow);
            if (cached != null)
                return Ok(new { layout = cached.Resultado, provedor = "cache" });

            try
            {
                var agente = _agentFactory.Criar(provedorEfetivo, iaConfig?.Apikey, iaConfig?.Modelo);
                var layoutJson = LimparMarkdown(await agente.GerarAsync(prompt));
                JsonDocument.Parse(layoutJson);

                // Resolve imagens via Unsplash se houver AccessKey configurada
                layoutJson = await ResolverImagensAsync(layoutJson);

                // Salva no cache
                var ttl = _config.GetValue<int>("AgentIA:CacheTTLHoras", 24);
                _context.IaCaches.Add(new IaCache
                {
                    Cacheid        = Guid.NewGuid().ToString(),
                    Hash           = hashKey,
                    Resultado      = layoutJson,
                    Datainclusao   = DateTime.UtcNow,
                    Datavencimento = DateTime.UtcNow.AddHours(ttl)
                });

                // Incrementa uso (só se usar chave do sistema)
                if (!acessoTotal && !temChavePropria && claimAppId != null)
                {
                    var hoje = DateOnly.FromDateTime(DateTime.Today);
                    var uso = _context.IaUsos.FirstOrDefault(u => u.Aplicacaoid == claimAppId && u.Data == hoje);
                    if (uso == null)
                        _context.IaUsos.Add(new IaUso { Usoid = Guid.NewGuid().ToString(), Aplicacaoid = claimAppId, Data = hoje, Contador = 1 });
                    else
                        uso.Contador++;
                }

                _context.SaveChanges();
                return Ok(new { layout = layoutJson, provedor = agente.Provedor });
            }
            catch (JsonException ex)
            {
                return UnprocessableEntity(new { erro = "JSON inválido retornado pela IA.", detalhe = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (HttpRequestException ex)
            {
                return StatusCode(502, $"Erro ao chamar o agente IA ({dto.Provedor ?? "default"}): {ex.Message}");
            }
        }

        // Campos de imagem a resolver por tipo de bloco: (tipobloco, campo)
        private static readonly (string tipo, string campo)[] _camposImagem = new (string tipo, string campo)[]
        {
            ("hero",          "imagemFundo"),
            ("banner-imagem", "url"),
            ("hero-cta",      "imagem_fundo")
        };

        private async Task<string> ResolverImagensAsync(string layoutJson)
        {
            var accessKey = _config["Unsplash:AccessKey"];
            if (string.IsNullOrWhiteSpace(accessKey)) return layoutJson;

            try
            {
                using var doc = JsonDocument.Parse(layoutJson);
                var root = doc.RootElement.Clone();
                var blocos = root.GetProperty("blocos");

                var resultado = new System.Text.StringBuilder();
                resultado.Append("{\"blocos\":[");

                bool primeiro = true;
                foreach (var bloco in blocos.EnumerateArray())
                {
                    if (!primeiro) resultado.Append(',');
                    primeiro = false;

                    var tipo = bloco.GetProperty("tipo").GetString() ?? "";
                    var campoAlvo = _camposImagem.FirstOrDefault(c => c.tipo == tipo).campo;

                    if (campoAlvo == null || !bloco.TryGetProperty("config", out var config))
                    {
                        resultado.Append(bloco.GetRawText());
                        continue;
                    }

                    if (!config.TryGetProperty(campoAlvo, out var valorEl))
                    {
                        resultado.Append(bloco.GetRawText());
                        continue;
                    }

                    var valor = valorEl.GetString() ?? "";
                    // Só resolve se não for já uma URL
                    if (valor.StartsWith("http", StringComparison.OrdinalIgnoreCase) || string.IsNullOrWhiteSpace(valor))
                    {
                        resultado.Append(bloco.GetRawText());
                        continue;
                    }

                    var imageUrl = await BuscarUnsplashAsync(valor, accessKey);
                    if (imageUrl == null)
                    {
                        resultado.Append(bloco.GetRawText());
                        continue;
                    }

                    // Reconstrói o bloco com o campo de imagem substituído
                    var blocoRaw = bloco.GetRawText();
                    var configRaw = config.GetRawText();
                    var valorEscapado = JsonSerializer.Serialize(valor);
                    var urlEscapada = JsonSerializer.Serialize(imageUrl);
                    var configNova = configRaw.Replace($"\"{campoAlvo}\":{valorEscapado}", $"\"{campoAlvo}\":{urlEscapada}");
                    resultado.Append(blocoRaw.Replace(configRaw, configNova));
                }

                resultado.Append("]}");
                // Valida antes de retornar
                JsonDocument.Parse(resultado.ToString());
                return resultado.ToString();
            }
            catch
            {
                return layoutJson; // se falhar, retorna o original
            }
        }

        private async Task<string?> BuscarUnsplashAsync(string keyword, string accessKey)
        {
            try
            {
                var client = _httpClientFactory.CreateClient();
                var url = $"https://api.unsplash.com/photos/random?query={Uri.EscapeDataString(keyword)}&orientation=landscape&client_id={accessKey}";
                var response = await client.GetAsync(url);
                if (!response.IsSuccessStatusCode) return null;
                var json = await response.Content.ReadAsStringAsync();
                using var doc = JsonDocument.Parse(json);
                return doc.RootElement.GetProperty("urls").GetProperty("regular").GetString();
            }
            catch
            {
                return null;
            }
        }

        private static string ComputeHash(string input)
        {
            var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(input));
            return Convert.ToHexString(bytes).ToLowerInvariant();
        }

        private static string LimparMarkdown(string texto)
        {
            // Remove code fences: ```json ... ``` ou ``` ... ```
            texto = Regex.Replace(texto, @"```(?:json)?\s*", "").Trim();

            // Remove backticks avulsos
            texto = texto.Trim('`').Trim();

            // Extrai o objeto JSON (primeiro { até o último })
            var inicio = texto.IndexOf('{');
            if (inicio < 0) return texto.Trim();

            var fim = texto.LastIndexOf('}');
            var json = fim > inicio ? texto[inicio..(fim + 1)] : texto[inicio..];

            // Tenta reparar JSON truncado fechando colchetes/chaves abertas
            try { JsonDocument.Parse(json); return json; }
            catch { return RepararJson(json); }
        }

        private static string RepararJson(string json)
        {
            var stack = new Stack<char>();
            bool inString = false;
            char prev = '\0';

            foreach (var c in json)
            {
                if (c == '"' && prev != '\\') inString = !inString;
                if (!inString)
                {
                    if (c == '{' || c == '[') stack.Push(c);
                    else if (c == '}' && stack.Count > 0 && stack.Peek() == '{') stack.Pop();
                    else if (c == ']' && stack.Count > 0 && stack.Peek() == '[') stack.Pop();
                }
                prev = c;
            }

            var sb = new System.Text.StringBuilder(json.TrimEnd().TrimEnd(','));
            while (stack.Count > 0)
                sb.Append(stack.Pop() == '{' ? '}' : ']');

            return sb.ToString();
        }

        public class GerarLayoutDto
        {
            public string Descricao { get; set; } = "";
            public string? Areaid { get; set; }
            public string? Provedor { get; set; }
        }
    }
}
