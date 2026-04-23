export interface BlocoLayoutV2 {
  tipo: string;
  config: Record<string, unknown>;
  row: number;
  col: number;
  rowSpan: number;
  colSpan: number;
  _nome?: string;
  _icone?: string;
}

export interface LayoutV2 {
  version: 'v2';
  blocos: BlocoLayoutV2[];
}
