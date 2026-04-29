import { Component, OnInit } from '@angular/core';
import { LojaApiService } from '../services/loja-api.service';
import { LojaAuthService } from '../services/loja-auth.service';

@Component({ templateUrl: './historico-pedidos.component.html' })
export class HistoricoPedidosComponent implements OnInit {
  lista: any[] = [];
  selecionado: any = null;
  timeline: any[] = [];
  carregandoTimeline = false;
  filtroStatus = '';

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
    private lojaAuth: LojaAuthService
  ) {}

  ngOnInit() {
    if (this.lojaAuth.isAutenticado) {
      this.carregar();
    }
  }

  carregar() {
    this.selecionado = null;
    this.timeline = [];
    this.api.getMeusPedidos(this.lojaAuth.auth!.email)
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
