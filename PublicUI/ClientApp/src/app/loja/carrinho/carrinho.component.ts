import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { CarrinhoService, ItemCarrinho } from '../services/carrinho.service';
import { LojaContextService } from '../services/loja-context.service';

@Component({ templateUrl: './carrinho.component.html' })
export class CarrinhoComponent implements OnInit {
  itens: ItemCarrinho[] = [];

  constructor(
    private carrinho: CarrinhoService,
    private router: Router,
    private ctx: LojaContextService
  ) {}

  ngOnInit() {
    this.itens = this.carrinho.getItens();
    console.log('[slug: a]',this.ctx.token);
  }

  remover(produtoid: string) {
    this.carrinho.remover(produtoid);
    this.itens = this.carrinho.getItens();
  }

  alterarQuantidade(produtoid: string, quantidade: number) {
    this.carrinho.alterarQuantidade(produtoid, quantidade);
    this.itens = this.carrinho.getItens();
  }

  limpar() {
    this.carrinho.limpar();
    this.itens = [];
  }

  total(): number {
    return this.carrinho.total();
  }

  finalizarCompra() {
    const base = this.ctx.slug ? `/s/${this.ctx.slug}/loja` : '/loja'
    this.router.navigate([`${base}/checkout`]);
  }

  continuarComprando() {
    const base = this.ctx.slug ? `/s/${this.ctx.slug}/loja` : '/loja'
    this.router.navigate([`${base}/catalogo`]);
  }

  formatarValor(v: number): string {
    return v?.toLocaleString('pt-BR', { style: 'currency', currency: 'BRL' }) ?? '—';
  }
}

