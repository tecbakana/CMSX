import { Component } from '@angular/core';
import { LojaAuthService } from '../services/loja-auth.service';

@Component({ templateUrl: './loja-perfil.component.html' })
export class LojaPerfilComponent {
  auth$ = this.auth.auth$;

  constructor(private auth: LojaAuthService) {}
}
