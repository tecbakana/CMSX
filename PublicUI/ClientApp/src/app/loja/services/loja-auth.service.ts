import { Injectable } from '@angular/core';
import { BehaviorSubject, Observable, map } from 'rxjs';

export interface LojaAuthResponse {
  token: string;
  clienteId: number;
  nome: string;
  email: string;
}

const STORAGE_KEY = 'loja_auth';

@Injectable()
export class LojaAuthService {
  private _auth = new BehaviorSubject<LojaAuthResponse | null>(this.carregar());

  readonly auth$ = this._auth.asObservable();
  readonly isAutenticado$: Observable<boolean> = this._auth.pipe(map(a => !!a));

  get auth(): LojaAuthResponse | null {
    return this._auth.value;
  }

  get isAutenticado(): boolean {
    return !!this._auth.value;
  }

  getToken(): string | null {
    return this._auth.value?.token ?? null;
  }

  salvarAuth(auth: LojaAuthResponse): void {
    localStorage.setItem(STORAGE_KEY, JSON.stringify(auth));
    this._auth.next(auth);
  }

  logout(): void {
    localStorage.removeItem(STORAGE_KEY);
    this._auth.next(null);
  }

  private carregar(): LojaAuthResponse | null {
    try {
      const raw = localStorage.getItem(STORAGE_KEY);
      return raw ? JSON.parse(raw) : null;
    } catch {
      return null;
    }
  }
}
