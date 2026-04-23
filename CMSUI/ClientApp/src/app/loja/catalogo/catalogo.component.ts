import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { LojaApiService, Produto } from '../services/loja-api.service';
import { CarrinhoService } from '../services/carrinho.service';
import { LojaContextService } from '../services/loja-context.service';

@Component({ templateUrl: './catalogo.component.html' })
export class CatalogoComponent implements OnInit {
  produtos: Produto[] = [];
  carregando = true;
  busca = '';

  constructor(
    private api: LojaApiService,
    private carrinho: CarrinhoService,
    private router: Router,
    private ctx: LojaContextService
  ) {}

  ngOnInit() {
    this.api.getProdutos(this.ctx.aplicacaoid).subscribe({
      next: r => { this.produtos = r; this.carregando = false; },
      error: () => { this.carregando = false; }
    });
  }

  get produtosFiltrados(): Produto[] {
    if (!this.busca.trim()) return this.produtos;
    const termo = this.busca.toLowerCase();
    return this.produtos.filter(p =>
      p.nome?.toLowerCase().includes(termo) ||
      p.sku?.toLowerCase().includes(termo)
    );
  }

  adicionarAoCarrinho(produto: Produto) {
    this.carrinho.adicionar({
      produtoid: produto.produtoid,
      sku: produto.sku,
      nome: produto.nome,
      valor: produto.valor
    });
  }

  irParaCarrinho() {
    const base = this.ctx.slug ? `/s/${this.ctx.slug}/loja` : '/loja'
    this.router.navigate([`${base}/carrinho`]);
  }

  formatarValor(v: number): string {
    return v?.toLocaleString('pt-BR', { style: 'currency', currency: 'BRL' }) ?? '—';
  }

  totalItensCarrinho(): number {
    return this.carrinho.totalItens();
  }
}

