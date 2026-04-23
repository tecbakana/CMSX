import { Component, Inject, OnInit } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { AdminContextService } from '../admin-context.service';

@Component({ templateUrl: './pedido.component.html' })
export class PedidoComponent implements OnInit {
  lista: any[] = [];
  selecionado: any = null;
  timeline: any[] = [];
  carregandoTimeline = false;
  filtroStatus = '';
  private usuario: any;

  readonly statusLabels: Record<string, { label: string; cls: string }> = {
    entrada:    { label: 'Entrada',    cls: 'bg-secondary' },
    confirmado: { label: 'Confirmado', cls: 'bg-primary'   },
    pagamento:  { label: 'Pagamento',  cls: 'bg-success'   }
  };

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
    if (!this.usuario.acessoTotal && this.usuario.aplicacaoid)
      p = p.set('aplicacaoid', this.usuario.aplicacaoid);
    else if (this.usuario.acessoTotal && this.adminCtx.tenantId)
      p = p.set('aplicacaoid', this.adminCtx.tenantId);
    if (this.filtroStatus)
      p = p.set('status', this.filtroStatus);
    return p;
  }

  carregar() {
    this.selecionado = null;
    this.timeline = [];
    this.http.get<any[]>('/pedidos', { params: this.appParams() })
      .subscribe(r => this.lista = r);
  }

  selecionar(pedido: any) {
    if (this.selecionado?.pedidoid === pedido.pedidoid) {
      this.selecionado = null;
      this.timeline = [];
      return;
    }
    this.selecionado = pedido;
    this.timeline = [];
    this.carregandoTimeline = true;
    this.http.get<any[]>(`/pedidos/${pedido.pedidoid}/timeline`)
      .subscribe({
        next: t => { this.timeline = t; this.carregandoTimeline = false; },
        error: () => { this.carregandoTimeline = false; }
      });
  }

  badgeInfo(status: string) {
    return this.statusLabels[status] ?? { label: status, cls: 'bg-dark' };
  }

  formatarValor(v: number | null): string {
    if (v == null) return '—';
    return v.toLocaleString('pt-BR', { style: 'currency', currency: 'BRL' });
  }
}
