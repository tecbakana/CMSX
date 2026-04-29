import { Component, OnInit } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { ActivatedRoute } from '@angular/router';

interface OpcaoPublica {
  opcaoid: string;
  nome: string;
  valorAdicional: number;
}

interface AtributoPublico {
  atributoid: string;
  nome: string;
  ordem: number;
  valorAdicional: number;
  opcoes: OpcaoPublica[];
  filhos: AtributoPublico[];
}

interface CheckboxAtributo {
  atributoid: string;
  label: string;
  valorAdicional: number;
  selecionado: boolean;
}

interface ProdutoPublico {
  produtoid: string;
  nome: string;
  valor: number | null;
  unidadeVenda: string | null;
  atributos: AtributoPublico[];
}

interface OpcaoSelecionavel {
  atributoid: string;
  sectionLabel: string;
  opcaoid: string;
  nome: string;
  valorAdicional: number;
  selecionada: boolean;
}

interface GrupoOpcao {
  label: string;
  opcoes: OpcaoSelecionavel[];
}

interface SelecaoComposta {
  atributoid: string;
  atributoNome: string;
  opcaoid: string;
  opcaoNome: string;
  valorAdicional: number;
}

interface CheckboxSelecionado {
  atributoid: string;
  atributoNome: string;
  valorAdicional: number;
}

interface ItemComposto {
  produtoid: string;
  produtoNome: string;
  quantidade: number;
  valorBase: number;
  selecoes: SelecaoComposta[];
  checkboxes: CheckboxSelecionado[];
  valorUnitario: number;
  valorTotal: number;
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
  erroSelecao = '';

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
  itensCompostos: ItemComposto[] = [];

  produtos: ProdutoPublico[] = [];
  produtoSelecionadoId = '';
  grupos: GrupoOpcao[] = [];
  checkboxAtribs: CheckboxAtributo[] = [];

  constructor(private http: HttpClient, private route: ActivatedRoute) {}

  ngOnInit() {
    this.token = this.route.snapshot.queryParamMap.get('ref') ?? '';
    if (this.token) this.carregarProdutos();
  }

  carregarProdutos() {
    this.http.get<ProdutoPublico[]>(`/api/publico/orcamento/produtos?ref=${this.token}`)
      .subscribe({ next: p => this.produtos = p });
  }

  get produtoSelecionado(): ProdutoPublico | null {
    return this.produtos.find(p => p.produtoid === this.produtoSelecionadoId) ?? null;
  }

  onProdutoChange() {
    this.erroSelecao = '';
    const produto = this.produtoSelecionado;
    if (!produto) { this.grupos = []; return; }

    const flat = new Map<string, OpcaoSelecionavel[]>();
    const checkboxes: CheckboxAtributo[] = [];
    const traverse = (node: AtributoPublico, path: string) => {
      const label = path ? `${path} > ${node.nome}` : node.nome;
      if (node.opcoes.length === 0 && node.filhos.length === 0 && (node.valorAdicional ?? 0) > 0) {
        checkboxes.push({ atributoid: node.atributoid, label, valorAdicional: node.valorAdicional, selecionado: false });
      } else if (node.opcoes.length > 0) {
        const lista = flat.get(label) ?? [];
        node.opcoes.forEach(o => lista.push({
          atributoid: node.atributoid,
          sectionLabel: label,
          opcaoid: o.opcaoid,
          nome: o.nome,
          valorAdicional: o.valorAdicional ?? 0,
          selecionada: false
        }));
        flat.set(label, lista);
      }
      node.filhos.forEach(f => traverse(f, label));
    };

    produto.atributos.forEach(a => traverse(a, ''));
    this.grupos = [...flat.entries()].map(([label, opcoes]) => ({ label, opcoes }));
    this.checkboxAtribs = checkboxes;
  }

  trackByIndex(index: number) { return index; }

  adicionarItem() {
    const produto = this.produtoSelecionado;
    if (!produto) return;

    const selecionadas = this.grupos.reduce(
      (acc: OpcaoSelecionavel[], g: GrupoOpcao) => acc.concat(g.opcoes.filter((o: OpcaoSelecionavel) => o.selecionada)),
      []
    );
    if (selecionadas.length === 0 && this.grupos.length > 0) {
      this.erroSelecao = 'Selecione ao menos uma opção.';
      return;
    }

    this.erroSelecao = '';

    const selecoes: SelecaoComposta[] = selecionadas.map((o: OpcaoSelecionavel) => ({
      atributoid: o.atributoid,
      atributoNome: o.sectionLabel,
      opcaoid: o.opcaoid,
      opcaoNome: o.nome,
      valorAdicional: o.valorAdicional
    }));

    const checkboxes: CheckboxSelecionado[] = this.checkboxAtribs
      .filter(c => c.selecionado)
      .map(c => ({ atributoid: c.atributoid, atributoNome: c.label, valorAdicional: c.valorAdicional }));

    const somaAdicionais = selecoes.reduce((s, sel) => s + sel.valorAdicional, 0);
    const somaCheckboxes = checkboxes.reduce((s, c) => s + c.valorAdicional, 0);
    const valorBase = produto.valor ?? 0;
    const valorUnitario = valorBase + somaAdicionais + somaCheckboxes;

    this.itensCompostos.push({
      produtoid: produto.produtoid,
      produtoNome: produto.nome,
      quantidade: 1,
      valorBase,
      selecoes,
      checkboxes,
      valorUnitario,
      valorTotal: valorUnitario
    });

    this.produtoSelecionadoId = '';
    this.grupos = [];
    this.checkboxAtribs = [];
    this.atualizarTotal();
  }

  removeItemComposto(index: number) {
    this.itensCompostos.splice(index, 1);
    this.atualizarTotal();
  }

  onQuantidadeCompostoChange(item: ItemComposto) {
    item.valorTotal = item.valorUnitario * item.quantidade;
    this.atualizarTotal();
  }

  calcularValorLinha(item: ItemOrcamento): number {
    return (item.valor ?? 0) * item.quantidade;
  }

  atualizarTotal() {
    const totalCompostos = this.itensCompostos.reduce((s, i) => s + i.valorUnitario * i.quantidade, 0);
    const totalSimples = this.itens.reduce((s, i) => s + this.calcularValorLinha(i), 0);
    const total = totalCompostos + totalSimples;
    this.form.valorestimado = total > 0 ? total : null;
  }

  salvar() {
    this.erro = '';
    if (!this.token) { this.erro = 'Link inválido. Solicite um novo link ao administrador.'; return; }
    if (!this.form.nome.trim()) { this.erro = 'Nome do cliente é obrigatório.'; return; }
    if (this.itensCompostos.length === 0 && this.itens.filter(i => i.descricao.trim()).length === 0) {
      this.erro = 'Adicione pelo menos um item ao orçamento.'; return;
    }

    this.enviando = true;
    const payload = {
      token: this.token,
      ...this.form,
      itens: this.itens.filter(i => i.descricao.trim()),
      itensCompostos: this.itensCompostos.map(ic => ({
        produtoid: ic.produtoid,
        quantidade: ic.quantidade,
        selecoes: ic.selecoes.map(s => ({ atributoid: s.atributoid, opcaoid: s.opcaoid })),
        checkboxAtributos: ic.checkboxes.map(c => c.atributoid)
      }))
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
    this.itensCompostos = [];
    this.erro = '';
    this.erroSelecao = '';
    this.produtoSelecionadoId = '';
    this.grupos = [];
    this.checkboxAtribs = [];
  }
}
