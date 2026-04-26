import { Component, Inject, OnInit } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { ActivatedRoute } from '@angular/router';

@Component({ templateUrl: './aplicacao.component.html' })
export class AplicacaoComponent implements OnInit {
  lista: any[] = [];
  selecionado: any = null;
  modoEdicao = false;
  abaAtiva = 'geral';

  readonly layouts = [
    { valor: '_Layout.cshtml',      label: 'Básico I' },
    { valor: '_LayoutBasic.cshtml', label: 'Básico II' },
    { valor: '_LayoutFlame.cshtml', label: 'OnePage' },
    { valor: '_LayoutLoja.cshtml',  label: 'Loja' }
  ];

  minhApp = false; // modo tenant: exibe só a própria app
  tokens: any[] = [];
  novoTokenVencimento = '';

  constructor(
    private http: HttpClient,
    @Inject('BASE_URL') private baseUrl: string,
    private route: ActivatedRoute
  ) {}

  ngOnInit() {
    this.modoEdicao = false;
    this.selecionado = null;
    this.minhApp = this.route.snapshot.routeConfig?.path === 'minha-aplicacao';
    this.carregar();
  }

  carregar() {
    this.http.get<any[]>(this.baseUrl + 'aplicacaos').subscribe(r => {
      if (this.minhApp) {
        const usuario = JSON.parse(sessionStorage.getItem('usuario') || '{}');
        this.lista = r.filter(a => a.aplicacaoid === usuario.aplicacaoid);
        // se só tem uma, já abre em modo edição diretamente
        if (this.lista.length === 1) this.editar(this.lista[0]);
      } else {
        this.lista = r;
      }
    });
  }

  novo() {
    this.selecionado = {
      nome: '', url: '', layoutchoose: '_Layout.cshtml', isactive: true,
      mailuser: '', mailpassword: '', mailserver: '', mailport: 587, issecure: false,
      pagsegurotoken: '', ogleadsense: '', header: '',
      pagefacebook: '', pageinstagram: '', pagetwitter: '',
      pagelinkedin: '', pagepinterest: '', pageflicker: '',
      telefone: '', endereco: '', descricao: ''
    };
    this.abaAtiva = 'geral';
    this.modoEdicao = true;
  }

  editar(item: any) {
    this.selecionado = { ...item, issecure: item.issecure === 1 || item.issecure === true };
    this.abaAtiva = 'geral';
    this.modoEdicao = true;
    if (item.aplicacaoid) this.carregarTokens(item.aplicacaoid);
  }

  carregarTokens(aplicacaoid: string) {
    this.http.get<any[]>(this.baseUrl + `publicTokens?aplicacaoid=${aplicacaoid}`)
      .subscribe(r => this.tokens = r);
  }

  gerarToken() {
    const payload: any = { aplicacaoid: this.selecionado.aplicacaoid };
    if (this.novoTokenVencimento) payload.datavencimento = this.novoTokenVencimento;
    this.http.post(this.baseUrl + 'publicTokens', payload)
      .subscribe(() => { this.novoTokenVencimento = ''; this.carregarTokens(this.selecionado.aplicacaoid); });
  }

  revogarToken(id: string) {
    if (confirm('Revogar este token? Links públicos que usam este token deixarão de funcionar.')) {
      this.http.delete(this.baseUrl + `PublicTokens/${id}`)
        .subscribe(() => this.carregarTokens(this.selecionado.aplicacaoid));
    }
  }

  copiarToken(token: string) {
    navigator.clipboard.writeText(token);
  }

  salvar() {
    const usuario = JSON.parse(sessionStorage.getItem('usuario') || '{}');
    const payload = { ...this.selecionado, idusuarioinicio: usuario.userid };

    if (this.selecionado.aplicacaoid) {
      this.http.put(this.baseUrl + 'aplicacaos/' + this.selecionado.aplicacaoid, payload)
        .subscribe(() => { this.modoEdicao = false; this.carregar(); });
    } else {
      this.http.post(this.baseUrl + 'aplicacaos', payload)
        .subscribe(() => { this.modoEdicao = false; this.carregar(); });
    }
  }

  alternarStatus(item: any) {
    const novoStatus = !item.isactive;
    const msg = novoStatus ? 'Ativar esta aplicação?' : 'Inativar esta aplicação?';
    if (confirm(msg)) {
      this.http.patch(this.baseUrl + 'aplicacaos/' + item.aplicacaoid + '/status', novoStatus)
        .subscribe(() => this.carregar());
    }
  }

  excluir(id: string) {
    if (confirm('Excluir esta aplicação? Esta ação não pode ser desfeita.')) {
      this.http.delete(this.baseUrl + 'aplicacaos/' + id).subscribe(() => this.carregar());
    }
  }

  cancelar() { this.modoEdicao = false; this.selecionado = null; }
}
