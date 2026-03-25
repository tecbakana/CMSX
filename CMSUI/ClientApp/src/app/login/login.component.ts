import { Component, Inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Router } from '@angular/router';

@Component({
  templateUrl: './login.component.html'
})
export class LoginComponent {
  apelido = '';
  senha = '';
  erro = '';
  carregando = false;

  constructor(
    private http: HttpClient,
    @Inject('BASE_URL') private baseUrl: string,
    private router: Router
  ) {
    if (sessionStorage.getItem('usuario')) {
      this.router.navigate(['/']);
    }
  }

  login() {
    this.erro = '';
    this.carregando = true;
    this.http.post<any>(this.baseUrl + 'auth/login', { apelido: this.apelido, senha: this.senha })
      .subscribe({
        next: (user) => {
          sessionStorage.setItem('usuario', JSON.stringify(user));
          window.location.href = '/';
        },
        error: () => {
          this.erro = 'Login ou senha inválidos.';
          this.carregando = false;
        }
      });
  }

  loginDemo() {
    this.erro = '';
    this.carregando = true;
    this.http.post<any>(this.baseUrl + 'auth/demo-login', {})
      .subscribe({
        next: (user) => {
          sessionStorage.setItem('usuario', JSON.stringify(user));
          window.location.href = '/';
        },
        error: () => {
          this.erro = 'Tenant demo não disponível. Execute o script cmsxDB.tenant_demo.sql.';
          this.carregando = false;
        }
      });
  }
}
