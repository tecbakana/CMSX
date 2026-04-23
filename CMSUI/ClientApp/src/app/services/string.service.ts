import { Injectable } from '@angular/core';

@Injectable({ providedIn: 'root' })
export class StringService {
  clean(value: string): string {
    return value?.trim().replace(/\s+/g, ' ') ?? '';
  }

  decode(value: string): string {
    try {
      return decodeURIComponent(value ?? '');
    } catch {
      return value;
    }
  }
}