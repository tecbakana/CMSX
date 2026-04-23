import { Component, OnInit } from '@angular/core';
import { Router, ActivatedRoute } from '@angular/router';
import { HttpClient } from '@angular/common/http';
import { LojaApiService, RegistrarRequest } from '../services/loja-api.service';
import { LojaAuthService } from '../services/loja-auth.service';
import { LojaContextService } from '../services/loja-context.service';

@Component({ templateUrl: './loja-login.component.html' })
export class LojaLoginComponent implements OnInit {
  modo: 'login' | 'cadastro' = 'login';
  carregando = false;
  erro = '';
  returnUrl = '/loja/checkout';

  login = { email: '', senha: '' };

  cadastro: RegistrarRequest = {
    aplicacaoid: '',
    nome: '', documento: '', email: '', senha: '',
    telefone: '', cep: '', logradouro: '', numero: '',
    complemento: '', bairro: '', cidade: '', estado: ''
  };

  buscandoCep = false;

  constructor(
    private api: LojaApiService,
    private auth: LojaAuthService,
    private ctx: LojaContextService,
    private router: Router,
    private route: ActivatedRoute,
    private http: HttpClient
  ) {}

  ngOnInit() {
    if (this.auth.isAutenticado) {
      this.router.navigateByUrl(this.returnUrl);
      return;
    }
    this.returnUrl = this.route.snapshot.queryParamMap.get('returnUrl') ?? '/loja/checkout';
    this.cadastro.aplicacaoid = this.ctx.aplicacaoid;
  }

  alternarModo() {
    this.modo = this.modo === 'login' ? 'cadastro' : 'login';
    this.erro = '';
  }

  entrar() {
    
    const base = this.ctx.slug ? `/s/${this.ctx.slug}/loja` : '/loja'
    this.carregando = true;
    this.erro = '';
    this.api.login(this.login).subscribe({
      next: auth => {
        this.auth.salvarAuth(auth);
        this.router.navigate([`${base}`]);
      },
      error: () => {
        this.erro = 'E-mail ou senha inválidos.';
        this.carregando = false;
      }
    });
  }

  registrar() {
    const base = this.ctx.slug ? `/s/${this.ctx.slug}/loja` : '/loja'
    this.carregando = true;
    this.erro = '';
    this.cadastro.aplicacaoid = this.ctx.aplicacaoid;
    this.api.registrar(this.cadastro).subscribe({
      next: auth => {
        this.auth.salvarAuth(auth);
        this.router.navigate([`${base}`]);
      },
      error: (e) => {
        this.erro = e?.error?.message ?? 'Erro ao criar conta. Verifique os dados.';
        this.carregando = false;
      }
    });
  }

  buscarCep() {
    const cep = this.cadastro.cep.replace(/\D/g, '');
    if (cep.length !== 8) return;
    this.buscandoCep = true;
    this.http.get<any>(`https://viacep.com.br/ws/${cep}/json/`).subscribe({
      next: d => {
        if (!d.erro) {
          this.cadastro.logradouro = d.logradouro;
          this.cadastro.bairro = d.bairro;
          this.cadastro.cidade = d.localidade;
          this.cadastro.estado = d.uf;
        }
        this.buscandoCep = false;
      },
      error: () => { this.buscandoCep = false; }
    });
  }
}
