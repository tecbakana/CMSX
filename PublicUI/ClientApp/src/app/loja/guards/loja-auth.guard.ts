import { Injectable } from '@angular/core';
import { CanActivate, Router, ActivatedRouteSnapshot, RouterStateSnapshot } from '@angular/router';
import { LojaAuthService } from '../services/loja-auth.service';
import { LojaContextService } from '../services/loja-context.service';

@Injectable()
export class LojaAuthGuard implements CanActivate {
  constructor(private auth: LojaAuthService, private router: Router,private ctx: LojaContextService) {}

  canActivate(_route: ActivatedRouteSnapshot, state: RouterStateSnapshot): boolean {
    if (this.auth.isAutenticado) return true;
    const base = this.ctx.slug ? `/s/${this.ctx.slug}/loja` : '/loja'
    this.router.navigate([`${base}/login`], { queryParams: { returnUrl: state.url } });
    return false;
  }
}
