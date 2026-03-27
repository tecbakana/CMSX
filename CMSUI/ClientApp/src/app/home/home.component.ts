import { Component, Inject, OnInit } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { AdminContextService } from '../admin-context.service';

@Component({
  templateUrl: './home.component.html'
})
export class HomeComponent implements OnInit {
  stats: any = null;
  private usuario: any = {};

  constructor(
    private http: HttpClient,
    @Inject('BASE_URL') private baseUrl: string,
    private adminCtx: AdminContextService
  ) {}

  ngOnInit() {
    this.usuario = JSON.parse(sessionStorage.getItem('usuario') || '{}');
    this.adminCtx.tenant$.subscribe(() => this.carregarStats());
    this.carregarStats();
  }

  private carregarStats() {
    let p = new HttpParams();
    if (this.usuario.acessoTotal && this.adminCtx.tenantId)
      p = p.set('aplicacaoid', this.adminCtx.tenantId);
    this.http.get<any>(this.baseUrl + 'dashboard', { params: p }).subscribe(s => this.stats = s);
  }
}
