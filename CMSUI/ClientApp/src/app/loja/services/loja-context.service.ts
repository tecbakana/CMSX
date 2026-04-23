import { Injectable, Inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { BehaviorSubject, Observable, of } from 'rxjs';
import { tap } from 'rxjs/operators';
import { ActivatedRoute } from '@angular/router';

export interface LojaCtx {
  aplicacaoid: string;
  nomeLoja: string;
  slug: string;
}

@Injectable({ providedIn: 'root' })
export class LojaContextService {
  private _ctx = new BehaviorSubject<LojaCtx | null>(null);
  ctx$ = this._ctx.asObservable();
  private _slug = '';

  constructor(
    private http: HttpClient,
    @Inject('BASE_URL') private baseUrl: string,
    private route: ActivatedRoute
  ) {}

  get aplicacaoid(): string {
    return this._ctx.value?.aplicacaoid ?? this.fromSession();
  }

  get slug(): string {
    return this._ctx.value?.slug ?? this._slug;
  }

  get nomeLoja(): string {
    return this._ctx.value?.nomeLoja ?? 'Loja';
  }

  initFromSlug(slug: string): Observable<LojaCtx> {
    this._slug = slug; // disponível imediatamente, sem depender da API
    return this.http.get<LojaCtx>(`${this.baseUrl}api/loja/resolve?slug=${slug}`).pipe(
      tap(ctx => this._ctx.next({...ctx,slug}))
    );
  }

  initFromSession(): void {
    const usuario = JSON.parse(sessionStorage.getItem('usuario') || '{}');
    const aplicacaoid = usuario.aplicacaoid ?? '';
    if (aplicacaoid) {
      this._ctx.next({
        aplicacaoid, nomeLoja: 'Loja',
        slug: this.slug
      });
    }
  }

  private fromSession(): string {
    const usuario = JSON.parse(sessionStorage.getItem('usuario') || '{}');
    return usuario.aplicacaoid ?? '';
  }
}
