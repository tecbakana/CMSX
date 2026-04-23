import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { HttpClientModule } from '@angular/common/http';
import { RouterModule, Routes } from '@angular/router';

import { LojaHomeComponent } from './home/loja-home.component';
import { CatalogoComponent } from './catalogo/catalogo.component';
import { CarrinhoComponent } from './carrinho/carrinho.component';
import { CheckoutComponent } from './checkout/checkout.component';
import { ListaClientesComponent } from './clientes/lista-clientes.component';
import { CadastroClienteComponent } from './clientes/cadastro-cliente.component';
import { HistoricoPedidosComponent } from './historico/historico-pedidos.component';
import { LojaLoginComponent } from './login/loja-login.component';
import { LojaNavComponent } from './nav/loja-nav.component';
import { LojaPerfilComponent } from './perfil/loja-perfil.component';

import { CarrinhoService } from './services/carrinho.service';
import { LojaApiService } from './services/loja-api.service';
import { LojaAuthService } from './services/loja-auth.service';
import { LojaAuthGuard } from './guards/loja-auth.guard';
import { LojaShellComponent } from './loja-shell.component';

import { HTTP_INTERCEPTORS } from '@angular/common/http';
import { LojaAuthInterceptor } from './interceptors/loja-auth.interceptor';

const routes: Routes = [
  {
    path: '',
    component: LojaShellComponent,
    children: [
      { path: '', component: LojaHomeComponent },
      { path: 'catalogo', component: CatalogoComponent },
      { path: 'carrinho', component: CarrinhoComponent, canActivate: [LojaAuthGuard]  },
      { path: 'checkout', component: CheckoutComponent, canActivate: [LojaAuthGuard] },
      { path: 'login', component: LojaLoginComponent },
      { path: 'clientes', component: ListaClientesComponent },
      { path: 'clientes/novo', component: CadastroClienteComponent },
      { path: 'historico', component: HistoricoPedidosComponent, canActivate: [LojaAuthGuard] },
      { path: 'perfil', component: LojaPerfilComponent, canActivate: [LojaAuthGuard] }
    ]
  }
];

@NgModule({
  declarations: [
    LojaHomeComponent,
    CatalogoComponent,
    CarrinhoComponent,
    CheckoutComponent,
    ListaClientesComponent,
    CadastroClienteComponent,
    HistoricoPedidosComponent,
    LojaLoginComponent,
    LojaShellComponent,
    LojaNavComponent,
    LojaPerfilComponent
  ],
  imports: [
    CommonModule,
    FormsModule,
    HttpClientModule,
    RouterModule.forChild(routes)
  ],
  providers: [
    CarrinhoService,
    LojaApiService,
    LojaAuthService,
    LojaAuthGuard,
    {provide: HTTP_INTERCEPTORS, 
    useClass: LojaAuthInterceptor, multi: true}
  ]
})
export class LojaModule { }
