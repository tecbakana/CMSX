import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { CarrinhoService } from '../services/carrinho.service';
import { LojaContextService } from '../services/loja-context.service';

@Component({
  selector: 'app-loja-home',
  templateUrl: './loja-home.component.html'
})
export class LojaHomeComponent implements OnInit {
  constructor(
    private router: Router,
    private route: ActivatedRoute,
    private carrinho: CarrinhoService,
    public ctx: LojaContextService
  ) {}

  ngOnInit() {
    const slug = this.route.snapshot.parent?.paramMap.get('slug');
    if (slug) {
      this.ctx.initFromSlug(slug).subscribe();
    } else {
      this.ctx.initFromSession();
    }
  }

  get totalCarrinho(): number {
    return this.carrinho.totalItens();
  }

  ir(rota: string) {
    const slug = this.route.snapshot.parent?.paramMap.get('slug');
    if (slug) {
      this.router.navigate(['/s', slug, 'loja', rota]);
    } else {
      this.router.navigate(['/loja', rota]);
    }
  }
}
