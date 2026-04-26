import { Component, OnInit } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { AdminContextService } from '../admin-context.service';

@Component({
  templateUrl: './orcamento.component.html'
})
export class OrcamentoComponent implements OnInit {
  lista: any[] = [];
  selecionado: any = null;
  carregandoDetalhe = false;
  private usuario: any;
  private refToken = '';

  constructor(
    private http: HttpClient,
    private adminCtx: AdminContextService
  ) {}

  ngOnInit() {
    this.usuario = JSON.parse(sessionStorage.getItem('usuario') || '{}');
    this.adminCtx.tenant$.subscribe(() => this.carregar());
  }

  private appParams(): HttpParams {
    let p = new HttpParams();
    if (this.usuario?.acessoTotal && this.adminCtx.tenantId)
      p = p.set('aplicacaoid', this.adminCtx.tenantId);
    return p;
  }

  carregar() {
    this.selecionado = null;
    this.http.get<any[]>('/orcamentos', { params: this.appParams() })
      .subscribe(r => this.lista = r);

    const appId = this.usuario?.acessoTotal
      ? (this.adminCtx.tenantId ?? '')
      : (this.usuario?.aplicacaoid ?? '');
    if (appId) {
      this.http.get<any[]>(`/publicTokens?aplicacaoid=${appId}`)
        .subscribe(tokens => {
          const ativo = tokens.find(t => t.ativo);
          this.refToken = ativo?.token ?? '';
        });
    }
  }

  selecionar(item: any) {
    if (this.selecionado?.orcamentoid === item.orcamentoid) {
      this.selecionado = null;
      return;
    }
    this.carregandoDetalhe = true;
    this.http.get<any>(`/orcamentos/${item.orcamentoid}`)
      .subscribe({
        next: d => { this.selecionado = d; this.carregandoDetalhe = false; },
        error: () => { this.carregandoDetalhe = false; }
      });
  }

  toggleAprovar(item: any, event: Event) {
    event.stopPropagation();
    this.http.put(`/orcamentos/${item.orcamentoid}/aprovar`, {})
      .subscribe((r: any) => {
        item.aprovado = r.aprovado;
        if (this.selecionado?.orcamentoid === item.orcamentoid)
          this.selecionado.aprovado = r.aprovado;
      });
  }

  excluir(item: any, event: Event) {
    event.stopPropagation();
    if (!confirm(`Excluir orçamento de ${item.nome}?`)) return;
    this.http.delete(`/orcamentos/${item.orcamentoid}`)
      .subscribe(() => {
        this.lista = this.lista.filter(o => o.orcamentoid !== item.orcamentoid);
        if (this.selecionado?.orcamentoid === item.orcamentoid)
          this.selecionado = null;
      });
  }

  linkPublico(): string {
    if (this.refToken)
      return `${window.location.origin}/orcamento/novo?ref=${this.refToken}`;
    return '';
  }

  copiarLink() {
    navigator.clipboard.writeText(this.linkPublico());
  }

  formatarValor(v: number | null): string {
    if (v == null) return '—';
    return v.toLocaleString('pt-BR', { style: 'currency', currency: 'BRL' });
  }

  formatarData(d: string | null): string {
    if (!d) return '—';
    return new Date(d).toLocaleDateString('pt-BR');
  }
}
