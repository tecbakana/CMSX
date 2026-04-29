import { Injectable } from '@angular/core';

export interface ItemCarrinho {
  produtoid: string;
  sku: string;
  nome: string;
  valor: number;
  quantidade: number;
}

const CHAVE_STORAGE = 'loja_carrinho';

@Injectable()
export class CarrinhoService {
  private itens: ItemCarrinho[] = [];

  constructor() {
    this.carregar();
  }

  private carregar() {
    try {
      const raw = localStorage.getItem(CHAVE_STORAGE);
      this.itens = raw ? JSON.parse(raw) : [];
    } catch {
      this.itens = [];
    }
  }

  private salvar() {
    localStorage.setItem(CHAVE_STORAGE, JSON.stringify(this.itens));
  }

  getItens(): ItemCarrinho[] {
    return [...this.itens];
  }

  adicionar(produto: Omit<ItemCarrinho, 'quantidade'>) {
    const existente = this.itens.find(i => i.produtoid === produto.produtoid);
    if (existente) {
      existente.quantidade++;
    } else {
      this.itens.push({ ...produto, quantidade: 1 });
    }
    this.salvar();
  }

  remover(produtoid: string) {
    this.itens = this.itens.filter(i => i.produtoid !== produtoid);
    this.salvar();
  }

  alterarQuantidade(produtoid: string, quantidade: number) {
    if (quantidade <= 0) {
      this.remover(produtoid);
      return;
    }
    const item = this.itens.find(i => i.produtoid === produtoid);
    if (item) {
      item.quantidade = quantidade;
      this.salvar();
    }
  }

  limpar() {
    this.itens = [];
    this.salvar();
  }

  total(): number {
    return this.itens.reduce((acc, i) => acc + i.valor * i.quantidade, 0);
  }

  totalItens(): number {
    return this.itens.reduce((acc, i) => acc + i.quantidade, 0);
  }
}
