import { Component, OnInit } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { ActivatedRoute } from '@angular/router';

interface OpcaoPublica {
  opcaoid: string;
  nome: string;
}

interface AtributoPublico {
  atributoid: string;
  nome: string;
  opcoes: OpcaoPublica[];
}

interface ProdutoPublico {
  produtoid: string;
  nome: string;
  valor: number | null;
  unidadeVenda: string | null;
  atributos: AtributoPublico[];
}

interface ItemOrcamento {
  descricao: string;
  quantidade: number;
  valor: number | null;
  ativo: boolean;
}

@Component({
  templateUrl: './novo-orcamento.component.html'
})
export class NovoOrcamentoComponent implements OnInit {
  token = '';
  enviando = false;
  sucesso = false;
  erro = '';

  form = {
    nome: '',
    email: '',
    telefone: '',
    descricaoservico: '',
    valorestimado: null as number | null,
    prazo: '',
    nomevendedor: ''
  };

  itens: ItemOrcamento[] = [];

  produtos: ProdutoPublico[] = [];
  produtoSelecionadoId = '';
  opcoesSelecionadas: { [atributoid: string]: string } = {};

  constructor(
    private http: HttpClient,
    private route: ActivatedRoute
  ) {}

  ngOnInit() {
    this.token = this.route.snapshot.queryParamMap.get('ref') ?? '';
    if (this.token) {
      this.carregarProdutos();
    }
  }

  carregarProdutos() {
    this.http.get<ProdutoPublico[]>(`/api/publico/orcamento/produtos?ref=${this.token}`)
      .subscribe({ next: p => this.produtos = p });
  }

  get produtoSelecionado(): ProdutoPublico | null {
    return this.produtos.find(p => p.produtoid === this.produtoSelecionadoId) ?? null;
  }

  onProdutoChange() {
    this.opcoesSelecionadas = {};
  }

  adicionarItem() {
    const produto = this.produtoSelecionado;
    if (!produto) return;

    const partes = [produto.nome];
    if (produto.unidadeVenda) partes.push(`Un.: ${produto.unidadeVenda}`);
    for (const atrib of produto.atributos) {
      const opcaoid = this.opcoesSelecionadas[atrib.atributoid];
      if (opcaoid) {
        const opcao = atrib.opcoes.find(o => o.opcaoid === opcaoid);
        if (opcao) partes.push(`${atrib.nome}: ${opcao.nome}`);
      }
    }

    this.itens.push({
      descricao: partes.join(' | '),
      quantidade: 1,
      valor: produto.valor,
      ativo: true
    });

    this.produtoSelecionadoId = '';
    this.opcoesSelecionadas = {};
    this.atualizarTotal();
  }

  removeItem(index: number) {
    this.itens.splice(index, 1);
    this.atualizarTotal();
  }

  calcularValorLinha(item: ItemOrcamento): number {
    return (item.valor ?? 0) * item.quantidade;
  }

  atualizarTotal() {
    this.form.valorestimado = this.itens.length > 0
      ? this.itens.reduce((sum, i) => sum + this.calcularValorLinha(i), 0)
      : null;
  }

  salvar() {
    this.erro = '';

    if (!this.token) {
      this.erro = 'Link inválido. Solicite um novo link ao administrador.';
      return;
    }
    if (!this.form.nome.trim()) {
      this.erro = 'Nome do cliente é obrigatório.';
      return;
    }

    this.enviando = true;

    const payload = {
      token: this.token,
      ...this.form,
      itens: this.itens.filter(i => i.descricao.trim())
    };

    this.http.post('/api/publico/orcamento', payload).subscribe({
      next: () => { this.sucesso = true; this.enviando = false; },
      error: () => { this.erro = 'Erro ao salvar orçamento. Tente novamente.'; this.enviando = false; }
    });
  }

  novoOrcamento() {
    this.sucesso = false;
    this.form = { nome: '', email: '', telefone: '', descricaoservico: '', valorestimado: null, prazo: '', nomevendedor: '' };
    this.itens = [];
    this.erro = '';
    this.produtoSelecionadoId = '';
    this.opcoesSelecionadas = {};
  }
}
