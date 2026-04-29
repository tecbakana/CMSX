import { Component, OnInit } from '@angular/core'; // Adicionado OnInit
import { Router } from '@angular/router';
import { Subject, of, Observable } from 'rxjs'; // Imports essenciais
import { debounceTime, distinctUntilChanged, switchMap, catchError } from 'rxjs/operators';
import { LojaApiService } from '../services/loja-api.service';

@Component({ templateUrl: './cadastro-cliente.component.html' })
export class CadastroClienteComponent implements OnInit { // Boa prática: usar ngOnInit
  cepSubject = new Subject<string>();

  form = {
    nome: '',
    documento: '',
    email: '',
    telefone: '',
    cep: '',
    logradouro: '',
    numero:'',
    complemento:'',
    bairro: '',
    cidade: '',
    estado: ''
  };
  enviando = false;
  erro = '';

  constructor(
    private api: LojaApiService,
    private router: Router
  ) {}

  ngOnInit() {
    this.cepSubject.pipe(
      debounceTime(600),
      distinctUntilChanged(),
      switchMap(cep => {
        if (cep.replace(/\D/g, '').length < 8) return of(null);
        this.enviando = true;
        return this.api.consultarCep(cep).pipe(
          catchError(() => {
            this.enviando = false;
            return of(null);
          })
        );
      })
    ).subscribe((res: any) => {
      this.enviando = false;
      if (res && !res.erro) {
        this.form.logradouro = res.logradouro || '';
        this.form.bairro = res.bairro || '';
        this.form.cidade = res.localidade || '';
        this.form.estado = res.uf || '';
      }
    });
  }

  podeSalvar(): boolean {
    return !!this.form.nome.trim() && !!this.form.email.trim() && !this.enviando;
  }

  salvar() {
    if (!this.podeSalvar()) return;
    this.enviando = true;
    this.erro = '';
    this.api.cadastrarCliente(this.form).subscribe({
      next: () => this.router.navigate(['/loja/clientes']),
      error: () => {
        this.erro = 'Falha ao cadastrar cliente.';
        this.enviando = false;
      }
    });
  }

  cancelar() {
    this.router.navigate(['/loja/clientes']);
  }
}