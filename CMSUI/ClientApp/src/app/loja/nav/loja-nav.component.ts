import { Component, OnInit } from '@angular/core';
import { Observable, of } from 'rxjs';
import { Router } from '@angular/router';
import { LojaAuthService } from '../services/loja-auth.service';
import { LojaContextService } from '../services/loja-context.service';

@Component({
  selector: 'loja-nav',
  templateUrl: './loja-nav.component.html'
})
export class LojaNavComponent  implements OnInit{

  constructor(
    private auth: LojaAuthService,
    private ctx: LojaContextService,
    private router: Router
  ) {}

  auth$ = this.auth.auth$;

  isLoaded = false;

  ngOnInit(): void {

      this.auth$.subscribe({
        next: () => {
          this.isLoaded = true;
        },
        error: () => {
          this.isLoaded = true; // Mesmo com erro, o carregamento "terminou"
        }
      });
  }

  private get base(): string {
    return this.ctx.slug ? `/s/${this.ctx.slug}/loja` : '/loja';
  }

  verPedidos() {
    this.router.navigate([`${this.base}/historico`]);
  }

  irPerfil() {
    this.router.navigate([`${this.base}/perfil`]);
  }

  sair() {
    this.auth.logout();
    this.router.navigate([`${this.base}/login`]);
  }

  cadastrar() {
    this.router.navigate([`${this.base}/clientes/novo`]);
  }

  entrar() {
    this.router.navigate([`${this.base}/login`]);
  }
}
