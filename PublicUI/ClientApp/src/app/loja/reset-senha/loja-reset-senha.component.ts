import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { LojaApiService } from '../services/loja-api.service';

@Component({
  selector: 'app-loja-reset-senha',
  templateUrl: './loja-reset-senha.component.html'
})
export class LojaResetSenhaComponent implements OnInit {
  token = '';
  novaSenha = '';
  confirmarSenha = '';
  carregando = false;
  erro = '';
  sucesso = '';

  constructor(
    private api: LojaApiService,
    private route: ActivatedRoute,
    private router: Router
  ) {}

  ngOnInit() {
    this.token = this.route.snapshot.queryParamMap.get('token') ?? '';
  }

  redefinir() {
    this.erro = '';
    if (this.novaSenha !== this.confirmarSenha) {
      this.erro = 'As senhas não coincidem.';
      return;
    }
    this.carregando = true;
    this.api.resetSenha({ token: this.token, novaSenha: this.novaSenha }).subscribe({
      next: res => {
        this.sucesso = res.message;
        this.carregando = false;
        setTimeout(() => this.router.navigate(['/loja/login']), 2500);
      },
      error: err => {
        this.erro = err?.error?.message ?? 'Token inválido ou expirado.';
        this.carregando = false;
      }
    });
  }
}
