const { env } = require('process');

// CMSAPI roda em http://localhost:5124 — separado do host Angular (CMSUI)
const target = env.CMSAPI_URL || 'http://localhost:5124';

const PROXY_CONFIG = [
  {
    context: [
      "/usuarios",
      "/aplicacaos",
      "/dashboard",
      "/modulos",
      "/auth",
      "/conteudos",
      "/areas",
      "/categorias",
      "/produtos",
      "/formularios",
      "/grupos",
      "/vinculos",
      "/vinculosmodulo",
      "/pagebuilder",
      "/wiki",
      "/faq",
      "/site",
      "/layouttemplates",
      "/pedidos",
      "/api/loja",
      "/swagger"
    ],
    proxyTimeout: 60000,
    timeout: 60000,
    target: target,
    secure: false,
    headers: {
      Connection: 'Keep-Alive'
    }
  }
]

module.exports = PROXY_CONFIG;
