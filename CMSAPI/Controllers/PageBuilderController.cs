using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using CMSXData.Models;
using CMSAPI.Services;
using ICMSX;

namespace CMSAPI.Controllers
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
        public IActionResult GetLayoutsResumo([FromQuery] string? aplicacaoid = null)
        {
            var (acessoTotal, claimAppId) = UserContext();
            var query = _context.Areas.AsQueryable();

            if (!acessoTotal)
                query = query.Where(a => a.Aplicacaoid == claimAppId);
            else if (!string.IsNullOrEmpty(aplicacaoid))
                query = query.Where(a => a.Aplicacaoid == aplicacaoid);

            var areas = query
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

            return Ok(new { area.Areaid, area.Nome, layout = area.Layout ?? "{\"blocos\":[]}", version = area.PageBuilderVersion ?? "v1" });
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

        // ── Extração de paleta de cores via IA ──────────────────────────────

        public class ExtrairPaletaDto
        {
            public string ImagemBase64 { get; set; } = "";
            public string MimeType { get; set; } = "image/jpeg";
            public string? Provedor { get; set; }
        }

        [Authorize]
        [HttpPost("extrair-paleta")]
        public async Task<IActionResult> ExtrairPaleta([FromBody] ExtrairPaletaDto dto)
        {
            var (_, claimAppId) = UserContext();
            var iaConfig = claimAppId != null
                ? _context.IaConfigs.FirstOrDefault(c => c.Aplicacaoid == claimAppId)
                : null;

            var prompt = @"Analise esta imagem e extraia uma paleta de 5 cores que representem bem o estilo visual.
Retorne APENAS um JSON válido, sem texto adicional:
{""primaria"":""#XXXXXX"",""secundaria"":""#XXXXXX"",""fundo"":""#XXXXXX"",""texto"":""#XXXXXX"",""destaque"":""#XXXXXX""}
Regras:
- primaria: cor principal da marca/imagem
- secundaria: cor complementar
- fundo: cor adequada para fundo de página (clara se possível)
- texto: cor adequada para texto sobre o fundo
- destaque: cor vibrante para botões/CTAs";

            try
            {
                var imageBytes = Convert.FromBase64String(dto.ImagemBase64);
                var provedorEfetivo = dto.Provedor ?? iaConfig?.Provedor;
                var agente = _agentFactory.Criar(provedorEfetivo, iaConfig?.Apikey, iaConfig?.Modelo);
                var resposta = LimparMarkdown(await agente.GerarComImagemAsync(imageBytes, dto.MimeType, prompt));
                JsonDocument.Parse(resposta); // valida JSON
                return Ok(new { paleta = resposta });
            }
            catch (JsonException ex)
            {
                return UnprocessableEntity(new { erro = "IA retornou JSON inválido.", detalhe = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (HttpRequestException ex)
            {
                return StatusCode(502, $"Erro ao chamar a IA: {ex.Message}");
            }
        }

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
                .ToList()
                .Select(b => new {
                    b.Tipobloco, b.Nome, b.Descricao,
                    campos = b.SchemaConfig != null
                        ? JsonSerializer.Deserialize<Dictionary<string, object>>(b.SchemaConfig)!.Keys.ToArray()
                        : Array.Empty<string>()
                })
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

            // Perfil do tenant para personalizar o conteúdo gerado
            var tenantApp = _context.Aplicacaos.FirstOrDefault(a => a.Aplicacaoid.ToString() == claimAppId);
            var tenantPerfil = new
            {
                nome_empresa  = tenantApp?.Nome ?? "aguardando informação",
                descricao     = string.IsNullOrWhiteSpace(tenantApp?.Descricao)   ? "aguardando informação" : tenantApp.Descricao,
                telefone      = string.IsNullOrWhiteSpace(tenantApp?.Telefone)    ? "aguardando informação" : tenantApp.Telefone,
                endereco      = string.IsNullOrWhiteSpace(tenantApp?.Endereco)    ? "aguardando informação" : tenantApp.Endereco,
                email_contato = string.IsNullOrWhiteSpace(tenantApp?.Mailuser)    ? "aguardando informação" : tenantApp.Mailuser,
                instagram     = string.IsNullOrWhiteSpace(tenantApp?.Pageinstagram) ? "aguardando informação" : tenantApp.Pageinstagram,
                facebook      = string.IsNullOrWhiteSpace(tenantApp?.Pagefacebook)  ? "aguardando informação" : tenantApp.Pagefacebook,
                linkedin      = string.IsNullOrWhiteSpace(tenantApp?.Pagelinkedin)  ? "aguardando informação" : tenantApp.Pagelinkedin
            };

            var areaNome = dto.Areaid != null
                ? _context.Areas.FirstOrDefault(a => a.Areaid == dto.Areaid)?.Nome
                : null;
            var contextoArea = areaNome != null ? $" para a área \"{areaNome}\"" : "";

            var tenantPerfilJson = JsonSerializer.Serialize(tenantPerfil, new JsonSerializerOptions { WriteIndented = false });
            var blocosJson       = JsonSerializer.Serialize(blocos,       new JsonSerializerOptions { WriteIndented = false });

            string prompt;

            if (dto.Blocos != null && dto.Blocos.Count > 0)
            {
                // Modo híbrido: AI preenche conteúdo da estrutura pré-definida pelo usuário
                var estrutura = JsonSerializer.Serialize(
                    dto.Blocos.Select(b => new { tipo = b.Tipo }),
                    new JsonSerializerOptions { WriteIndented = false });

                prompt = $@"Você é um especialista em conteúdo web.
O usuário montou a seguinte estrutura de blocos para a página{contextoArea}:
{estrutura}

Preencha o conteúdo de cada bloco com base nesta descrição: ""{dto.Descricao}""

Perfil do tenant (use os dados reais para o conteúdo; ""aguardando informação"" = use como placeholder):
{tenantPerfilJson}

Blocos disponíveis e seus campos:
{blocosJson}

Retorne APENAS um JSON válido mantendo EXATAMENTE os tipos e a ordem dos blocos recebidos:
{{""blocos"":[{{""tipo"":""<tipobloco>"",""config"":{{<campos preenchidos>}}}}]}}

Regras:
- Mantenha EXATAMENTE a mesma ordem e tipos de blocos
- Preencha os configs com conteúdo adequado à descrição
- Use IDs reais do tenant quando necessário (areaid, formularioid, cateriaid)
- JSON completo e não truncado";
            }
            else
            {
                // Modo livre: AI decide estrutura e conteúdo
                var contexto = new
                {
                    perfil_tenant         = tenantPerfil,
                    blocos_disponiveis    = blocos,
                    areas_do_tenant       = areas,
                    formularios_do_tenant = formularios,
                    categorias_do_tenant  = categorias
                };

                prompt = $@"Você é um especialista em design e criação de páginas web.
O usuário quer criar uma página{contextoArea} com a seguinte descrição: ""{dto.Descricao}""

Dados do tenant e blocos disponíveis:
{JsonSerializer.Serialize(contexto, new JsonSerializerOptions { WriteIndented = false })}

IMPORTANTE sobre o perfil_tenant: use os dados reais do tenant para preencher o conteúdo dos blocos (nome da empresa, telefone, endereço etc.). Onde o valor for ""aguardando informação"", use esse texto literalmente como placeholder no conteúdo gerado.

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
- Use no máximo 4 blocos
- Sua resposta deve ser completa e não truncada — gere apenas o que couber em resposta completa
- Use os IDs reais dos dados do tenant quando necessário (areaid, formularioid, cateriaid)
- Ordene os blocos de forma que faça sentido visual para a página descrita
- O JSON deve ser completo e válido — não truncar";
            }

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
                var root = JsonNode.Parse(layoutJson)!;
                var blocos = root["blocos"]?.AsArray();
                if (blocos == null) return layoutJson;

                foreach (var blocoNode in blocos)
                {
                    if (blocoNode == null) continue;
                    var tipo = blocoNode["tipo"]?.GetValue<string>() ?? "";
                    var campoAlvo = _camposImagem.FirstOrDefault(c => c.tipo == tipo).campo;
                    if (campoAlvo == null) continue;

                    var config = blocoNode["config"]?.AsObject();
                    if (config == null || !config.TryGetPropertyValue(campoAlvo, out var valorNode)) continue;

                    var valor = valorNode?.GetValue<string>() ?? "";
                    if (valor.StartsWith("http", StringComparison.OrdinalIgnoreCase) || string.IsNullOrWhiteSpace(valor))
                        continue;

                    var imageUrl = await BuscarUnsplashAsync(valor, accessKey);
                    if (imageUrl != null)
                        config[campoAlvo] = JsonValue.Create(imageUrl);
                }

                return root.ToJsonString();
            }
            catch
            {
                return layoutJson;
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
            if (inString) sb.Append('"'); // fecha string aberta (JSON truncado no meio de um valor)
            while (stack.Count > 0)
                sb.Append(stack.Pop() == '{' ? '}' : ']');

            return sb.ToString();
        }

        // ── Versão do Page Builder da área ──────────────────────────────────

        public class AreaVersionDto
        {
            public string Version { get; set; } = "v1";
        }

        [Authorize]
        [HttpPut("area-version/{areaid}")]
        public IActionResult AtualizarAreaVersion(string areaid, [FromBody] AreaVersionDto dto)
        {
            var (acessoTotal, claimAppId) = UserContext();
            var area = _context.Areas.FirstOrDefault(a => a.Areaid == areaid);
            if (area == null) return NotFound();
            if (!acessoTotal && area.Aplicacaoid != claimAppId) return Forbid();

            if (dto.Version != "v1" && dto.Version != "v2")
                return BadRequest("Versão inválida. Use 'v1' ou 'v2'.");

            area.PageBuilderVersion = dto.Version;
            _context.SaveChanges();
            return Ok();
        }

        // ── Interpretação de rascunho via IA ────────────────────────────────

        public class InterpretarRascunhoDto
        {
            public IFormFile Arquivo { get; set; } = null!;
        }

        [Authorize]
        [HttpPost("interpretar-rascunho")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> InterpretarRascunho([FromForm] InterpretarRascunhoDto dto)
        {
            var arquivo = dto.Arquivo;
            if (arquivo == null || arquivo.Length == 0)
                return BadRequest("Arquivo de imagem obrigatório.");

            var (_, claimAppId) = UserContext();
            var iaConfig = claimAppId != null
                ? _context.IaConfigs.FirstOrDefault(c => c.Aplicacaoid == claimAppId)
                : null;

            var tiposDisponiveis = _context.DictBlocos
                .Select(b => b.Tipobloco)
                .ToHashSet();

            var tiposJson = string.Join(", ", tiposDisponiveis.OrderBy(t => t));

            var prompt = $@"Analise este rascunho/wireframe de página web e identifique os blocos de conteúdo e suas posições em um grid de 12 colunas.

Para cada bloco identificado, retorne:
- tipo: o tipo de bloco mais adequado. Escolha APENAS entre os tipos disponíveis: [{tiposJson}]
- row: linha no grid (começando em 1)
- col: coluna inicial no grid (1 a 12)
- rowSpan: quantidade de linhas que o bloco ocupa (mínimo 1)
- colSpan: quantidade de colunas que o bloco ocupa (1 a 12, total das colunas = 12 por linha)
- descricao: descrição breve do conteúdo identificado no rascunho

Retorne APENAS um JSON válido, sem texto adicional:
{{""blocos"":[{{""tipo"":""..."",""row"":1,""col"":1,""rowSpan"":1,""colSpan"":12,""descricao"":""...""}}]}}";

            try
            {
                byte[] imageBytes;
                using (var ms = new MemoryStream())
                {
                    await arquivo.CopyToAsync(ms);
                    imageBytes = ms.ToArray();
                }
                var mimeType = arquivo.ContentType ?? "image/jpeg";

                var agente = _agentFactory.Criar(iaConfig?.Provedor, iaConfig?.Apikey, iaConfig?.Modelo);
                var resposta = LimparMarkdown(await agente.GerarComImagemAsync(imageBytes, mimeType, prompt));

                using var doc = JsonDocument.Parse(resposta);
                var blocosEl = doc.RootElement.GetProperty("blocos");

                var blocosMapeados = blocosEl.EnumerateArray().Select(b =>
                {
                    var tipo = b.TryGetProperty("tipo", out var t) ? t.GetString() ?? "" : "";
                    if (!tiposDisponiveis.Contains(tipo))
                        tipo = "bloco-generico";

                    return new
                    {
                        tipo,
                        row      = b.TryGetProperty("row",      out var r)  ? r.GetInt32()  : 1,
                        col      = b.TryGetProperty("col",      out var c)  ? c.GetInt32()  : 1,
                        rowSpan  = b.TryGetProperty("rowSpan",  out var rs) ? rs.GetInt32() : 1,
                        colSpan  = b.TryGetProperty("colSpan",  out var cs) ? cs.GetInt32() : 12,
                        descricao = b.TryGetProperty("descricao", out var d) ? d.GetString() ?? "" : ""
                    };
                }).ToList();

                return Ok(new { blocos = blocosMapeados });
            }
            catch (JsonException ex)
            {
                return UnprocessableEntity(new { erro = "IA retornou JSON inválido.", detalhe = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (HttpRequestException ex)
            {
                return StatusCode(502, $"Erro ao chamar a IA: {ex.Message}");
            }
        }

        public class GerarLayoutDto
        {
            public string Descricao { get; set; } = "";
            public string? Areaid { get; set; }
            public string? Provedor { get; set; }
            public List<BlocoPreDefinidoDto>? Blocos { get; set; }
        }

        public class BlocoPreDefinidoDto
        {
            public string Tipo { get; set; } = "";
        }

        // ── Importar wireframe + imagem de fundo via IA ──────────────────────

        public class ImportarComFundoDto
        {
            public IFormFile Arquivo { get; set; } = null!;
            public IFormFile? ImagemFundo { get; set; }
        }

        private static readonly string[] _tiposImagemPermitidos = ["image/jpeg", "image/png", "image/webp"];
        private const long _maxBytesImagem = 5 * 1024 * 1024;

        [Authorize]
        [HttpPost("importar-com-fundo")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> ImportarComFundo([FromForm] ImportarComFundoDto dto)
        {
            if (dto.Arquivo == null || dto.Arquivo.Length == 0)
                return BadRequest("Arquivo de wireframe obrigatório.");

            var (_, claimAppId) = UserContext();
            var iaConfig = claimAppId != null
                ? _context.IaConfigs.FirstOrDefault(c => c.Aplicacaoid == claimAppId)
                : null;

            // 1. Faz upload da imagem de fundo para o Blob Storage (se fornecida)
            string? urlFundo = null;
            if (dto.ImagemFundo != null && dto.ImagemFundo.Length > 0)
            {
                if (!_tiposImagemPermitidos.Contains(dto.ImagemFundo.ContentType))
                    return BadRequest("Tipo de imagem de fundo não permitido. Use JPG, PNG ou WebP.");
                if (dto.ImagemFundo.Length > _maxBytesImagem)
                    return BadRequest("Imagem de fundo muito grande. Limite: 5 MB.");

                var connStr = _config["AzureStorage:ConnectionString"];
                if (!string.IsNullOrWhiteSpace(connStr))
                {
                    var containerName = _config["AzureStorage:Container"] ?? "cms-imagens";
                    var container = new BlobContainerClient(connStr, containerName);
                    var ext = Path.GetExtension(dto.ImagemFundo.FileName);
                    var blobName = $"{claimAppId ?? "global"}/{Guid.NewGuid()}{ext}";
                    var blob = container.GetBlobClient(blobName);
                    using var stream = dto.ImagemFundo.OpenReadStream();
                    await blob.UploadAsync(stream, new BlobUploadOptions
                    {
                        HttpHeaders = new BlobHttpHeaders { ContentType = dto.ImagemFundo.ContentType }
                    });
                    urlFundo = blob.Uri.ToString();
                }
            }

            // 2. Interpreta o wireframe com IA
            var tiposDisponiveis = _context.DictBlocos.Select(b => b.Tipobloco).ToHashSet();
            var tiposJson = string.Join(", ", tiposDisponiveis.OrderBy(t => t));

            var prompt = $@"Analise este rascunho/wireframe de página web e identifique os blocos de conteúdo e suas posições em um grid de 12 colunas.

Para cada bloco identificado, retorne:
- tipo: o tipo de bloco mais adequado. Escolha APENAS entre os tipos disponíveis: [{tiposJson}]
- row: linha no grid (começando em 1)
- col: coluna inicial no grid (1 a 12)
- rowSpan: quantidade de linhas que o bloco ocupa (mínimo 1)
- colSpan: quantidade de colunas que o bloco ocupa (1 a 12, total das colunas = 12 por linha)
- descricao: descrição breve do conteúdo identificado no rascunho

Retorne APENAS um JSON válido, sem texto adicional:
{{""blocos"":[{{""tipo"":""..."",""row"":1,""col"":1,""rowSpan"":1,""colSpan"":12,""descricao"":""...""}}]}}";

            try
            {
                byte[] imageBytes;
                using (var ms = new MemoryStream())
                {
                    await dto.Arquivo.CopyToAsync(ms);
                    imageBytes = ms.ToArray();
                }
                var mimeType = dto.Arquivo.ContentType ?? "image/jpeg";

                var agente = _agentFactory.Criar(iaConfig?.Provedor, iaConfig?.Apikey, iaConfig?.Modelo);
                var resposta = LimparMarkdown(await agente.GerarComImagemAsync(imageBytes, mimeType, prompt));

                using var doc = JsonDocument.Parse(resposta);
                var blocosEl = doc.RootElement.GetProperty("blocos");

                var blocosMapeados = blocosEl.EnumerateArray().Select(b =>
                {
                    var tipo = b.TryGetProperty("tipo", out var t) ? t.GetString() ?? "" : "";
                    if (!tiposDisponiveis.Contains(tipo)) tipo = "bloco-generico";

                    // 3. Injeta URL do fundo nos campos de imagem de cada bloco que os suporta
                    var config = new Dictionary<string, object?>();
                    if (urlFundo != null)
                    {
                        var campoFundo = _camposImagem.FirstOrDefault(c => c.tipo == tipo).campo;
                        if (campoFundo != null)
                            config[campoFundo] = urlFundo;
                    }

                    return new
                    {
                        tipo,
                        config,
                        row      = b.TryGetProperty("row",      out var r)  ? r.GetInt32()  : 1,
                        col      = b.TryGetProperty("col",      out var c)  ? c.GetInt32()  : 1,
                        rowSpan  = b.TryGetProperty("rowSpan",  out var rs) ? rs.GetInt32() : 1,
                        colSpan  = b.TryGetProperty("colSpan",  out var cs) ? cs.GetInt32() : 12,
                        descricao = b.TryGetProperty("descricao", out var d) ? d.GetString() ?? "" : ""
                    };
                }).ToList();

                return Ok(new { blocos = blocosMapeados, urlFundo });
            }
            catch (JsonException ex)
            {
                return UnprocessableEntity(new { erro = "IA retornou JSON inválido.", detalhe = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (HttpRequestException ex)
            {
                return StatusCode(502, $"Erro ao chamar a IA: {ex.Message}");
            }
        }
    }
}