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

interface ProdutoPublico {
  produtoid: string;
  nome: string;
  valor: number | null;
  unidadeVenda: string | null;
  atributos: AtributoPublico[];
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

interface Slot {
  atributoid: string;
  label: string;
  isFilhoSelector: boolean;
  isCheckbox: boolean;
  checked: boolean;
  valorAdicional: number;
  opcoes: { opcaoid: string; nome: string; valorAdicional: number }[];
  opcaoSelecionada: string;
  filhos: { atributoid: string; nome: string }[];
  filhoSelecionadoId: string;
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
  slots: Slot[] = [];

  private selecoes = new Map<string, string>();

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
    this.selecoes.clear();
    const produto = this.produtoSelecionado;
    this.slots = produto ? this.buildSlots(produto.atributos, '') : [];
  }

  private buildSlots(nodes: AtributoPublico[], parentPath: string): Slot[] {
    const result: Slot[] = [];
    for (const node of nodes) {
      const label = parentPath ? `${parentPath} > ${node.nome}` : node.nome;
      const hasOpcoes = node.opcoes.length > 0;
      const hasFilhos = node.filhos.length > 0;
      const isCheckbox = !hasOpcoes && !hasFilhos && (node.valorAdicional ?? 0) > 0;

      if (isCheckbox) {
        result.push({
          atributoid: node.atributoid,
          label,
          isFilhoSelector: false,
          isCheckbox: true,
          checked: this.selecoes.get(node.atributoid) === 'checked',
          valorAdicional: node.valorAdicional,
          opcoes: [],
          opcaoSelecionada: '',
          filhos: [],
          filhoSelecionadoId: ''
        });
      } else if (hasOpcoes) {
        result.push({
          atributoid: node.atributoid,
          label,
          isFilhoSelector: false,
          isCheckbox: false,
          checked: false,
          valorAdicional: 0,
          opcoes: node.opcoes.map(o => ({ opcaoid: o.opcaoid, nome: o.nome, valorAdicional: o.valorAdicional ?? 0 })),
          opcaoSelecionada: this.selecoes.get(node.atributoid) ?? '',
          filhos: [],
          filhoSelecionadoId: ''
        });
        if (hasFilhos) result.push(...this.buildSlots(node.filhos, label));
      } else if (hasFilhos) {
        if (node.filhos.length === 1) {
          // Nó de caminho único — salta direto para o filho
          result.push(...this.buildSlots(node.filhos, label));
        } else {
          // Múltiplos filhos — selector de navegação (escolha um galho)
          const selectedFilhoId = this.selecoes.get(node.atributoid) ?? '';
          result.push({
            atributoid: node.atributoid,
            label,
            isFilhoSelector: true,
            isCheckbox: false,
            checked: false,
            valorAdicional: 0,
            opcoes: [],
            opcaoSelecionada: '',
            filhos: node.filhos.map(f => ({ atributoid: f.atributoid, nome: f.nome })),
            filhoSelecionadoId: selectedFilhoId
          });
          if (selectedFilhoId) {
            const filho = node.filhos.find(f => f.atributoid === selectedFilhoId);
            if (filho) result.push(...this.buildSlots([filho], label));
          }
        }
      }
    }
    return result;
  }

  onSlotChange(slot: Slot) {
    this.erroSelecao = '';
    if (slot.isCheckbox) {
      this.selecoes.set(slot.atributoid, slot.checked ? 'checked' : '');
    } else {
      const id = slot.isFilhoSelector ? slot.filhoSelecionadoId : slot.opcaoSelecionada;
      this.selecoes.set(slot.atributoid, id);
    }
    const produto = this.produtoSelecionado;
    this.slots = produto ? this.buildSlots(produto.atributos, '') : [];
  }

  adicionarItem() {
    const produto = this.produtoSelecionado;
    if (!produto) return;

    const selecoes: SelecaoComposta[] = this.slots
      .filter(s => !s.isFilhoSelector && !s.isCheckbox && s.opcaoSelecionada)
      .map(s => {
        const opcao = s.opcoes.find(o => o.opcaoid === s.opcaoSelecionada)!;
        return {
          atributoid: s.atributoid,
          atributoNome: s.label,
          opcaoid: opcao.opcaoid,
          opcaoNome: opcao.nome,
          valorAdicional: opcao.valorAdicional
        };
      });

    const checkboxes: CheckboxSelecionado[] = this.slots
      .filter(s => s.isCheckbox && s.checked)
      .map(s => ({ atributoid: s.atributoid, atributoNome: s.label, valorAdicional: s.valorAdicional }));

    const hasSelectSlots = this.slots.some(s => !s.isFilhoSelector && !s.isCheckbox);
    if (hasSelectSlots && selecoes.length === 0) {
      this.erroSelecao = 'Selecione ao menos uma opção.';
      return;
    }

    this.erroSelecao = '';

    const somaAdicionais = selecoes.reduce((s, sel) => s + sel.valorAdicional, 0);
    const somaCheckboxes = checkboxes.reduce((s, cb) => s + cb.valorAdicional, 0);
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
    this.selecoes.clear();
    this.slots = [];
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
    this.selecoes.clear();
    this.slots = [];
  }
}
