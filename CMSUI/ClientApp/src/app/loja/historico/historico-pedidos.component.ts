import { Component, OnInit } from '@angular/core';
import { LojaApiService } from '../services/loja-api.service';
import { LojaAuthService } from '../services/loja-auth.service';
import { AdminContextService } from '../../admin-context.service';

@Component({ templateUrl: './historico-pedidos.component.html' })
export class HistoricoPedidosComponent implements OnInit {
  lista: any[] = [];
  selecionado: any = null;
  timeline: any[] = [];
  carregandoTimeline = false;
  filtroStatus = '';
  private usuario: any;

  readonly statusLabels: Record<string, { label: string; cls: string }> = {
    aguardando_envio: { label: 'Aguardando envio', cls: 'bg-secondary' },
    criado:           { label: 'Criado',            cls: 'bg-primary'   },
    erro_envio:       { label: 'Erro de envio',     cls: 'bg-danger'    },
    confirmado:       { label: 'Confirmado',         cls: 'bg-success'   },
    pagamento:        { label: 'Pagamento',          cls: 'bg-info'      }
  };

  readonly statusOpcoes = Object.entries(this.statusLabels).map(([k, v]) => ({ value: k, label: v.label }));

  constructor(
    private api: LojaApiService,
    private lojaAuth: LojaAuthService,
    private adminCtx: AdminContextService
  ) {}

  ngOnInit() {
    this.usuario = JSON.parse(sessionStorage.getItem('usuario') || '{}');
    if (this.lojaAuth.isAutenticado) {
      this.carregar();
    } else {
      this.adminCtx.tenant$.subscribe(() => this.carregar());
    }
  }

  carregar() {
    this.selecionado = null;
    this.timeline = [];

    // Cliente logado na loja — vê apenas seus próprios pedidos
    if (this.lojaAuth.isAutenticado) {
      this.api.getMeusPedidos(this.lojaAuth.auth!.email)
        .subscribe(r => this.lista = r);
      return;
    }

    // Admin — filtra por tenant
    const params: any = {};
    if (!this.usuario.acessoTotal && this.usuario.aplicacaoid)
      params.aplicacaoid = this.usuario.aplicacaoid;
    else if (this.usuario.acessoTotal && this.adminCtx.tenantId)
      params.aplicacaoid = this.adminCtx.tenantId;
    if (this.filtroStatus) params.status = this.filtroStatus;

    this.api.getPedidos(params).subscribe(r => this.lista = r);
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
    this.api.getTimeline(pedido.pedidoid).subscribe({
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
