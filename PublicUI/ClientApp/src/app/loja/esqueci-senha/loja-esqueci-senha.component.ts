import { Component } from '@angular/core';
import { Router } from '@angular/router';
import { LojaApiService } from '../services/loja-api.service';

@Component({
  selector: 'app-loja-esqueci-senha',
  templateUrl: './loja-esqueci-senha.component.html'
})
export class LojaEsqueciSenhaComponent {
  email = '';
  carregando = false;
  erro = '';
  sucesso = '';

  constructor(private api: LojaApiService, private router: Router) {}

  enviar() {
    this.erro = '';
    this.sucesso = '';
    this.carregando = true;
    this.api.esqueciSenha({ email: this.email }).subscribe({
      next: res => {
        this.sucesso = res.message;
        this.carregando = false;
      },
      error: () => {
        this.erro = 'Não foi possível processar a solicitação. Tente novamente.';
        this.carregando = false;
      }
    });
  }
}
