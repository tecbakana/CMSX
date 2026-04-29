import { Injectable, Inject } from '@angular/core';
import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { LojaApiService, ClienteLoja } from '../services/loja-api.service';

@Component({ templateUrl: './lista-clientes.component.html' })

@Injectable()
export class ListaClientesComponent implements OnInit {
  clientes: ClienteLoja[] = [];
  carregando = true;
  busca = '';

  constructor(
    private api: LojaApiService,
    private router: Router,
    @Inject('BASE_URL') private baseUrl: string
  ) {}

  ngOnInit() {
    this.api.getClientes().subscribe({
      next: r => { this.clientes = r; this.carregando = false; },
      error: () => { this.carregando = false; }
    });
  }

  get clientesFiltrados(): ClienteLoja[] {
    if (!this.busca.trim()) return this.clientes;
    const termo = this.busca.toLowerCase();
    return this.clientes.filter(c =>
      c.nome?.toLowerCase().includes(termo) ||
      c.email?.toLowerCase().includes(termo) ||
      c.documento?.toLowerCase().includes(termo)
    );
  }

  novoCliente() {
    this.router.navigate([this.baseUrl + 'api/loja/clientes/novo']);
  }

  selecionarParaCheckout(cliente: ClienteLoja) {
    this.router.navigate([this.baseUrl + 'api/loja/checkout'], { state: { clienteSelecionado: cliente } });
  }
}
