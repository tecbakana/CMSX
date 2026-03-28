import { Component, Inject, OnInit } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { AdminContextService } from '../admin-context.service';

interface DictBloco {
  tipobloco: string;
  nome: string;
  descricao: string;
  icone: string;
  schemaConfig: string;
}

interface BlocoLayout {
  tipo: string;
  config: any;
  coluna?: string;
  _nome?: string;
  _icone?: string;
}

@Component({ templateUrl: './page-builder.component.html' })
export class PageBuilderComponent implements OnInit {
  areas: any[] = [];
  blocos: DictBloco[] = [];

  areaid: string = '';
  areaNome: string = '';
  descricao: string = '';
  provedor: string = '';
  gerando = false;
  salvando = false;

  layoutAtual: BlocoLayout[] = [];
  layoutsResumo: { areaid: string; nome: string; qtdBlocos: number }[] = [];
  erro: string = '';
  sucesso: string = '';

  // Galeria de mídias
  galeriaAberta = false;
  galeriaImagens: any[] = [];
  uploadando = false;
  galeriaPickerAberto = false;
  galeriaPickerCampo: string | null = null;

  // Templates
  usuario: any = {};
  isAdmin = false;
  templates: any[] = [];
  tplAberto = false;
  tplNome = '';
  tplDescricao = '';
  tplTipo = 'home';
  tplPadrao = false;
  tplSalvando = false;

  // Configurações de IA
  configAberta = false;
  cfgProvedor = '';
  cfgApikey = '';
  cfgModelo = '';
  cfgLimiteDiario: number | null = null;
  cfgTemChavePropria = false;
  cfgUsoHoje = 0;
  cfgLimiteEfetivo = 20;
  cfgSalvando = false;
  cfgSucesso = '';
  cfgErro = '';
  unsplashAtivo = false;

  constructor(private http: HttpClient, @Inject('BASE_URL') private baseUrl: string, private adminCtx: AdminContextService) {}

  ngOnInit() {
    const u = JSON.parse(sessionStorage.getItem('usuario') || '{}');
    this.usuario = u;
    this.isAdmin = !!u.acessoTotal;
    this.adminCtx.tenant$.subscribe(() => { this.carregarAreas(); this.carregarLayoutsResumo(); });
    this.carregarAreas();
    this.carregarBlocos();
    this.carregarIaConfig();
    this.carregarLayoutsResumo();
    this.carregarUnsplashStatus();
    if (this.isAdmin) this.carregarTemplates();
  }

  carregarAreas() {
    this.http.get<any[]>(`${this.baseUrl}areas`, { params: this.appParams() }).subscribe({
      next: r => this.areas = r,
      error: () => {}
    });
  }

  private appParams(): HttpParams {
    let p = new HttpParams();
    if (!this.usuario.acessoTotal && this.usuario.aplicacaoid)
      p = p.set('aplicacaoid', this.usuario.aplicacaoid);
    else if (this.usuario.acessoTotal && this.adminCtx.tenantId)
      p = p.set('aplicacaoid', this.adminCtx.tenantId);
    return p;
  }

  carregarLayoutsResumo() {
    this.http.get<any[]>(`${this.baseUrl}pagebuilder/layouts-resumo`, { params: this.appParams() }).subscribe({
      next: r => this.layoutsResumo = r,
      error: () => {}
    });
  }

  carregarBlocos() {
    this.http.get<DictBloco[]>(`${this.baseUrl}pagebuilder/blocos`).subscribe({
      next: r => this.blocos = r,
      error: () => {}
    });
  }

  carregarUnsplashStatus() {
    this.http.get<any>(`${this.baseUrl}pagebuilder/unsplash-status`).subscribe({
      next: r => this.unsplashAtivo = r.ativo,
      error: () => {}
    });
  }

  carregarIaConfig() {
    this.http.get<any>(`${this.baseUrl}pagebuilder/ia-config`).subscribe({
      next: r => {
        this.cfgProvedor       = r.provedor ?? '';
        this.cfgModelo         = r.modelo ?? '';
        this.cfgLimiteDiario   = r.limiteDiario;
        this.cfgTemChavePropria = r.temChavePropria;
        this.cfgUsoHoje        = r.usoHoje;
        this.cfgLimiteEfetivo  = r.limiteDiario;
      },
      error: () => {}
    });
  }

  salvarIaConfig() {
    this.cfgSalvando = true;
    this.cfgErro = '';
    this.cfgSucesso = '';
    this.http.put(`${this.baseUrl}pagebuilder/ia-config`, {
      provedor:     this.cfgProvedor || null,
      apikey:       this.cfgApikey || null,
      modelo:       this.cfgModelo || null,
      limiteDiario: this.cfgLimiteDiario
    }).subscribe({
      next: () => {
        this.cfgSalvando = false;
        this.cfgSucesso = 'Configurações salvas!';
        this.cfgApikey = '';
        this.carregarIaConfig();
      },
      error: () => { this.cfgSalvando = false; this.cfgErro = 'Erro ao salvar.'; }
    });
  }

  removerApiKey() {
    if (!confirm('Remover sua chave? O sistema usará a chave padrão com limite diário.')) return;
    this.http.delete(`${this.baseUrl}pagebuilder/ia-config/apikey`).subscribe({
      next: () => { this.cfgTemChavePropria = false; this.carregarIaConfig(); },
      error: () => {}
    });
  }

  onAreaChange() {
    const area = this.areas.find(a => a.areaid === this.areaid);
    this.areaNome = area?.nome ?? '';
    this.layoutAtual = [];
    this.erro = '';
    this.sucesso = '';

    if (this.areaid) {
      this.http.get<any>(`${this.baseUrl}pagebuilder/layout/${this.areaid}`).subscribe({
        next: r => {
          try {
            const parsed = JSON.parse(r.layout ?? '{"blocos":[]}');
            this.layoutAtual = (parsed.blocos ?? []).map((b: any) => this.enriquecerBloco(b));
          } catch {
            this.layoutAtual = [];
          }
        },
        error: () => {}
      });
    }
  }

  gerarLayout() {
    if (!this.descricao.trim()) {
      this.erro = 'Descreva o layout desejado antes de gerar.';
      return;
    }
    this.gerando = true;
    this.erro = '';
    this.sucesso = '';

    const colunasPorIndice = this.layoutAtual.map(b => b.coluna);

    this.http.post<any>(`${this.baseUrl}pagebuilder/gerar-layout`, {
      descricao: this.descricao,
      areaid: this.areaid || null,
      provedor: this.provedor || null,
      blocos: this.layoutAtual.length > 0 ? this.layoutAtual.map(b => ({ tipo: b.tipo })) : null
    }).subscribe({
      next: r => {
        this.gerando = false;
        const deCacheStr = r.provedor === 'cache' ? ' (cache)' : '';
        try {
          const parsed = JSON.parse(r.layout ?? '{"blocos":[]}');
          this.layoutAtual = (parsed.blocos ?? []).map((b: any, i: number) => {
            const bloco = this.enriquecerBloco(b);
            if (colunasPorIndice[i]) bloco.coluna = colunasPorIndice[i];
            return bloco;
          });
          if (r.provedor !== 'cache') this.carregarIaConfig(); // atualiza contador
          this.sucesso = `Layout gerado${deCacheStr}!`;
        } catch {
          this.erro = 'A IA retornou um JSON inválido. Tente reformular a descrição.';
        }
      },
      error: e => {
        this.gerando = false;
        const body = e?.error;
        this.erro = body?.detalhe ?? body?.erro ?? (typeof body === 'string' ? body : null) ?? 'Erro ao chamar o agente IA.';
      }
    });
  }

  salvarLayout() {
    if (!this.areaid) {
      this.erro = 'Selecione uma área antes de salvar.';
      return;
    }
    this.salvando = true;
    this.erro = '';
    this.sucesso = '';

    const payload = {
      blocos: this.layoutAtual.map(b => ({ tipo: b.tipo, config: b.config, coluna: b.coluna }))
    };

    this.http.put(`${this.baseUrl}pagebuilder/layout/${this.areaid}`, payload).subscribe({
      next: () => {
        this.salvando = false;
        this.sucesso = 'Layout salvo com sucesso!';
        this.carregarLayoutsResumo();
      },
      error: () => { this.salvando = false; this.erro = 'Erro ao salvar o layout.'; }
    });
  }

  selecionarLayoutSalvo(areaid: string) {
    this.areaid = areaid;
    this.onAreaChange();
  }

  abrirPreview() {
    const appId = this.usuario.aplicacaoid ?? this.adminCtx.tenantId;
    const area = this.areas.find((a: any) => a.areaid === this.areaid);
    const areaUrl = area?.url;
    const url = areaUrl ? `/preview/${appId}/${areaUrl}` : `/preview/${appId}`;
    window.open(url, '_blank');
  }

  limparLayout() {
    this.layoutAtual = [];
    this.descricao = '';
    this.areaid = '';
    this.erro = '';
    this.sucesso = '';
  }

  moverBloco(index: number, direcao: 'up' | 'down') {
    const novo = [...this.layoutAtual];
    const alvo = direcao === 'up' ? index - 1 : index + 1;
    if (alvo < 0 || alvo >= novo.length) return;
    [novo[index], novo[alvo]] = [novo[alvo], novo[index]];
    this.layoutAtual = novo;
  }

  removerBloco(index: number) {
    this.layoutAtual.splice(index, 1);
  }

  adicionarBloco(b: DictBloco) {
    this.layoutAtual = [...this.layoutAtual, this.enriquecerBloco({ tipo: b.tipobloco, config: {}, coluna: 'full' })];
  }

  setColunaBloco(index: number, coluna: string) {
    this.layoutAtual[index] = { ...this.layoutAtual[index], coluna };
  }

  private enriquecerBloco(b: any): BlocoLayout {
    const dict = this.blocos.find(d => d.tipobloco === b.tipo);
    return { ...b, _nome: dict?.nome ?? b.tipo, _icone: dict?.icone ?? 'bi-box' };
  }

  carregarTemplates() {
    this.http.get<any[]>(`${this.baseUrl}layouttemplates`).subscribe({
      next: r => this.templates = r,
      error: () => {}
    });
  }

  salvarComoTemplate() {
    if (!this.tplNome.trim()) { this.erro = 'Informe um nome para o template.'; return; }
    if (this.layoutAtual.length === 0) { this.erro = 'Nenhum bloco para salvar.'; return; }
    this.tplSalvando = true;
    const payload = {
      nome: this.tplNome,
      descricao: this.tplDescricao || null,
      tipo: this.tplTipo,
      padrao: this.tplPadrao,
      layout: JSON.stringify({ blocos: this.layoutAtual.map(b => ({ tipo: b.tipo, config: b.config })) })
    };
    this.http.post(`${this.baseUrl}layouttemplates`, payload).subscribe({
      next: () => {
        this.tplSalvando = false;
        this.sucesso = `Template "${this.tplNome}" salvo!`;
        this.tplAberto = false;
        this.tplNome = '';
        this.tplDescricao = '';
        this.tplPadrao = false;
        this.carregarTemplates();
      },
      error: () => { this.tplSalvando = false; this.erro = 'Erro ao salvar template.'; }
    });
  }

  carregarTemplate(t: any) {
    if (!confirm(`Carregar o template "${t.nome}"? O layout atual será substituído.`)) return;
    try {
      const parsed = JSON.parse(t.layout ?? '{"blocos":[]}');
      this.layoutAtual = (parsed.blocos ?? []).map((b: any) => this.enriquecerBloco(b));
      this.sucesso = `Template "${t.nome}" carregado.`;
    } catch {
      this.erro = 'Erro ao carregar o template.';
    }
  }

  excluirTemplate(id: string, nome: string) {
    if (!confirm(`Excluir o template "${nome}"?`)) return;
    this.http.delete(`${this.baseUrl}layouttemplates/${id}`).subscribe({
      next: () => { this.carregarTemplates(); this.sucesso = 'Template excluído.'; },
      error: () => {}
    });
  }

  // ── Galeria de mídias ────────────────────────────────────────────────

  toggleGaleria() {
    this.galeriaAberta = !this.galeriaAberta;
    if (this.galeriaAberta) this.carregarGaleria();
  }

  carregarGaleria() {
    this.http.get<any[]>(`${this.baseUrl}media`, { params: this.appParams() }).subscribe({
      next: r => this.galeriaImagens = r,
      error: () => {}
    });
  }

  uploadImagem(event: Event) {
    const input = event.target as HTMLInputElement;
    if (!input.files?.length) return;
    const form = new FormData();
    form.append('arquivo', input.files[0]);
    this.uploadando = true;
    this.http.post<any>(`${this.baseUrl}media/upload`, form, { params: this.appParams() }).subscribe({
      next: () => { this.uploadando = false; this.carregarGaleria(); },
      error: e => { this.uploadando = false; this.erro = e?.error?.erro ?? 'Erro ao fazer upload.'; }
    });
    input.value = '';
  }

  deletarImagem(blobName: string, nome: string) {
    if (!confirm(`Deletar "${nome}"?`)) return;
    this.http.delete(`${this.baseUrl}media`, { params: new HttpParams().set('blobName', blobName) }).subscribe({
      next: () => this.carregarGaleria(),
      error: () => {}
    });
  }

  copiarUrl(url: string) {
    navigator.clipboard.writeText(url).then(() => {
      this.sucesso = 'URL copiada!';
      setTimeout(() => { if (this.sucesso === 'URL copiada!') this.sucesso = ''; }, 2000);
    });
  }

  abrirGaleriaPicker(campo: string) {
    this.galeriaPickerCampo = campo;
    this.galeriaPickerAberto = true;
    this.carregarGaleria();
  }

  selecionarImagemParaCampo(url: string) {
    if (this.galeriaPickerCampo) this.editandoConfig[this.galeriaPickerCampo] = url;
    this.galeriaPickerAberto = false;
    this.galeriaPickerCampo = null;
  }

  // ── Editor de bloco (modal) ───────────────────────────────────────────
  editandoIndex: number | null = null;
  editandoConfig: any = {};
  editandoSchema: any = {};
  editandoNome = '';
  editandoCampos: { key: string; label: string; type: string; placeholder: string }[] = [];

  abrirEditorBloco(i: number) {
    const bloco = this.layoutAtual[i];
    const dict  = this.blocos.find(d => d.tipobloco === bloco.tipo);
    try { this.editandoSchema = JSON.parse(dict?.schemaConfig ?? '{}'); } catch { this.editandoSchema = {}; }
    this.editandoCampos = Object.entries(this.editandoSchema).map(([key, def]: [string, any]) => ({
      key,
      label:       def?.label       ?? key,
      type:        def?.type        ?? 'string',
      placeholder: def?.placeholder ?? def?.default ?? ''
    }));
    this.editandoConfig = JSON.parse(JSON.stringify(bloco.config ?? {}));
    this.editandoNome   = bloco._nome ?? bloco.tipo;
    this.editandoIndex  = i;
  }

  fecharEditorBloco() {
    this.editandoIndex = null;
    this.editandoConfig = {};
    this.editandoSchema = {};
    this.editandoCampos = [];
  }

  confirmarEdicaoBloco() {
    if (this.editandoIndex === null) return;
    this.layoutAtual[this.editandoIndex].config = { ...this.editandoConfig };
    this.fecharEditorBloco();
  }

  camposSchema(): { key: string; label: string; type: string; placeholder: string }[] {
    return Object.entries(this.editandoSchema).map(([key, def]: [string, any]) => ({
      key,
      label:       def?.label       ?? key,
      type:        def?.type        ?? 'string',
      placeholder: def?.placeholder ?? def?.default ?? ''
    }));
  }

  tipoInput(type: string): string {
    if (type === 'color')   return 'color';
    if (type === 'number')  return 'number';
    if (type === 'boolean') return 'checkbox';
    if (type === 'url')     return 'url';
    return 'text';
  }

  configVisual(bloco: BlocoLayout): { label: string; valor: string }[] {
    const dict = this.blocos.find(d => d.tipobloco === bloco.tipo);
    let schema: any = {};
    try { schema = JSON.parse(dict?.schemaConfig ?? '{}'); } catch {}
    return Object.entries(bloco.config ?? {})
      .filter(([, v]) => v !== '' && v !== null && v !== undefined)
      .map(([k, v]) => ({
        label: schema[k]?.label ?? k,
        valor: Array.isArray(v) ? `${(v as any[]).length} item(s)` : String(v).substring(0, 100)
      }));
  }
}
