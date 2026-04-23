import { Component, ElementRef, HostListener, Inject, OnInit, ViewChild } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { ActivatedRoute, Router } from '@angular/router';
import { CdkDragEnd, CdkDragMove } from '@angular/cdk/drag-drop';
import { AdminContextService } from '../admin-context.service';
import { BlocoLayoutV2 } from './bloco-layout-v2.model';

interface DictBloco {
  tipobloco: string;
  nome: string;
  descricao: string;
  icone: string;
  schemaConfig: string;
}

const COLS = 12;
const ROW_HEIGHT = 80;

@Component({
  selector: 'app-page-builder-v2',
  templateUrl: './page-builder-v2.component.html',
  styleUrls: ['./page-builder-v2.component.css']
})
export class PageBuilderV2Component implements OnInit {
  @ViewChild('canvasRef') canvasRef!: ElementRef<HTMLDivElement>;
  @ViewChild('drawCanvasRef') drawCanvasRef!: ElementRef<HTMLCanvasElement>;

  readonly COLS = COLS;
  readonly ROW_HEIGHT = ROW_HEIGHT;

  areas: any[] = [];
  blocos: DictBloco[] = [];
  areaid = '';
  areaNome = '';
  layoutAtual: BlocoLayoutV2[] = [];
  totalRows = 6;
  erro = '';
  sucesso = '';
  salvando = false;
  usuario: any = {};

  // Catalog drag state
  isDraggingFromCatalog = false;
  canvasHighlightCol = 0;
  canvasHighlightRow = 0;
  canvasHighlightColSpan = 4;

  // Resize state
  resizingIndex: number | null = null;
  resizingType: 'col' | 'row' | null = null;
  private resizeStartX = 0;
  private resizeStartY = 0;
  private resizeStartSpan = 0;
  private resizeColWidth = 0;

  // Block editor (same schema as v1)
  editandoIndex: number | null = null;
  editandoConfig: any = {};
  editandoSchema: any = {};
  editandoNome = '';
  editandoCampos: { key: string; label: string; type: string; placeholder: string }[] = [];

  layoutOrigemV1 = false;
  mostrarModalMigracao = false;
  private areaIdFromRoute = '';

  galeriaImagens: any[] = [];
  galeriaPickerAberto = false;
  galeriaPickerCampo: string | null = null;

  // Importar rascunho
  importarRascunhoAberto = false;
  importarModo: 'upload' | 'canvas' = 'upload';
  importandoRascunho = false;
  importarErro = '';
  blocoNovosIndices: Set<number> = new Set();
  rascunhoPreviewUrl = '';
  fundoPreviewUrl = '';
  private rascunhoArquivo: File | null = null;
  private fundoArquivo: File | null = null;

  // Canvas de desenho
  drawingShapes: { x: number; y: number; w: number; h: number }[] = [];
  private isDrawingShape = false;
  private drawStartX = 0;
  private drawStartY = 0;
  private currentShape: { x: number; y: number; w: number; h: number } | null = null;

  private _configVisualCache = new Map<BlocoLayoutV2, { key: string; label: string; valor: string }[]>();

  constructor(
    private http: HttpClient,
    @Inject('BASE_URL') private baseUrl: string,
    private adminCtx: AdminContextService,
    private route: ActivatedRoute,
    private router: Router
  ) {}

  ngOnInit() {
    this.usuario = JSON.parse(sessionStorage.getItem('usuario') || '{}');
    this.route.params.subscribe(params => {
      this.areaIdFromRoute = params['areaid'] || '';
      if (this.areas.length > 0 && this.areaIdFromRoute) {
        this.areaid = this.areaIdFromRoute;
        this.onAreaChange();
      }
    });
    this.adminCtx.tenant$.subscribe(() => this.carregarAreas());
    this.carregarAreas();
    this.carregarBlocos();
  }

  private appParams(): HttpParams {
    let p = new HttpParams();
    if (!this.usuario.acessoTotal && this.usuario.aplicacaoid)
      p = p.set('aplicacaoid', this.usuario.aplicacaoid);
    else if (this.usuario.acessoTotal && this.adminCtx.tenantId)
      p = p.set('aplicacaoid', this.adminCtx.tenantId);
    return p;
  }

  carregarAreas() {
    this.http.get<any[]>(`${this.baseUrl}areas`, { params: this.appParams() }).subscribe({
      next: r => {
        this.areas = r;
        if (this.areaIdFromRoute && !this.areaid) {
          this.areaid = this.areaIdFromRoute;
          this.onAreaChange();
        }
      },
      error: () => {}
    });
  }

  carregarBlocos() {
    this.http.get<DictBloco[]>(`${this.baseUrl}pagebuilder/blocos`).subscribe({
      next: r => this.blocos = r,
      error: () => {}
    });
  }

  onAreaChange() {
    const area = this.areas.find(a => a.areaid === this.areaid);
    this.areaNome = area?.nome ?? '';
    this.setLayoutAtual([]);
    this.layoutOrigemV1 = false;
    this.erro = '';
    this.sucesso = '';
    if (!this.areaid) return;

    this.http.get<any>(`${this.baseUrl}pagebuilder/layout/${this.areaid}`).subscribe({
      next: r => {
        try {
          const parsed = JSON.parse(r.layout ?? '{"blocos":[]}');
          if (parsed.version === 'v2') {
            this.layoutOrigemV1 = false;
            this.setLayoutAtual((parsed.blocos ?? []).map((b: any) => this.enriquecerBloco(b)));
          } else {
            this.layoutOrigemV1 = true;
            this.setLayoutAtual(this.migrarV1ParaV2(parsed.blocos ?? []));
          }
        } catch {
          this.layoutOrigemV1 = true;
          this.setLayoutAtual([]);
        }
      },
      error: () => {}
    });
  }

  private migrarV1ParaV2(blocos: any[]): BlocoLayoutV2[] {
    return blocos.map((b, i) => {
      const colSpan = b.coluna === '1/2' ? 6 : b.coluna === '1/3' ? 4 : 12;
      return this.enriquecerBloco({ ...b, row: i + 1, col: 1, rowSpan: 1, colSpan });
    });
  }

  salvarLayout() {
    if (!this.areaid) { this.erro = 'Selecione uma área antes de salvar.'; return; }
    if (this.layoutOrigemV1) {
      this.mostrarModalMigracao = true;
      return;
    }
    this.executarSalvamento(false);
  }

  confirmarMigracao() {
    this.mostrarModalMigracao = false;
    this.executarSalvamento(true);
  }

  manterBeta() {
    this.mostrarModalMigracao = false;
    this.executarSalvamento(false);
  }

  private executarSalvamento(migrar: boolean) {
    this.salvando = true;
    this.erro = '';
    this.sucesso = '';

    const payload = {
      version: 'v2',
      blocos: this.layoutAtual.map(b => ({
        tipo: b.tipo, config: b.config,
        row: b.row, col: b.col, rowSpan: b.rowSpan, colSpan: b.colSpan
      }))
    };

    this.http.put(`${this.baseUrl}pagebuilder/layout/${this.areaid}`, payload).subscribe({
      next: () => {
        this.salvando = false;
        this.sucesso = 'Layout salvo!';
        this.layoutOrigemV1 = false;
        if (migrar) {
          this.http.put(`${this.baseUrl}pagebuilder/area-version/${this.areaid}`, { Version: 'v2' }).subscribe();
        }
      },
      error: () => { this.salvando = false; this.erro = 'Erro ao salvar o layout.'; }
    });
  }

  irParaV1() {
    this.router.navigate(['/page-builder']);
  }

  removerBloco(index: number) {
    const novo = [...this.layoutAtual];
    novo.splice(index, 1);
    this.setLayoutAtual(novo);
  }

  adicionarBlocoClick(dict: DictBloco) {
    const row = Math.max(1, this.totalRows - 1);
    this.layoutAtual = [...this.layoutAtual, {
      tipo: dict.tipobloco,
      config: {},
      row, col: 1, rowSpan: 1, colSpan: 12,
      _nome: dict.nome, _icone: dict.icone
    }];
    this._configVisualCache.clear();
    this.recalcularLinhas();
  }

  private setLayoutAtual(blocos: BlocoLayoutV2[]) {
    this._configVisualCache.clear();
    this.layoutAtual = blocos;
    this.recalcularLinhas();
  }

  recalcularLinhas() {
    const maxRow = this.layoutAtual.reduce((m, b) => Math.max(m, b.row + b.rowSpan - 1), 0);
    this.totalRows = Math.max(6, maxRow + 2);
  }

  get canvasRowTemplate(): string {
    return `repeat(${this.totalRows}, ${ROW_HEIGHT}px)`;
  }

  private enriquecerBloco(b: any): BlocoLayoutV2 {
    const dict = this.blocos.find(d => d.tipobloco === b.tipo);
    return {
      tipo: b.tipo,
      config: b.config ?? {},
      row: b.row ?? 1,
      col: b.col ?? 1,
      rowSpan: b.rowSpan ?? 1,
      colSpan: b.colSpan ?? 12,
      _nome: dict?.nome ?? b._nome ?? b.tipo,
      _icone: dict?.icone ?? b._icone ?? 'bi-box'
    };
  }

  trackByIndex = (i: number) => i;

  // ── Drag: catalog → canvas ──────────────────────────────────────────

  onCatalogDragMoved(event: CdkDragMove<DictBloco>) {
    if (!this.canvasRef) return;
    const rect = this.canvasRef.nativeElement.getBoundingClientRect();
    const colWidth = rect.width / COLS;
    const relX = event.pointerPosition.x - rect.left;
    const relY = event.pointerPosition.y - rect.top;
    this.isDraggingFromCatalog = relX >= 0 && relX <= rect.width && relY >= 0 && relY <= rect.height;
    if (this.isDraggingFromCatalog) {
      this.canvasHighlightCol = Math.max(1, Math.min(COLS, Math.floor(relX / colWidth) + 1));
      this.canvasHighlightRow = Math.max(1, Math.floor(relY / ROW_HEIGHT) + 1);
      this.canvasHighlightColSpan = Math.min(4, COLS - this.canvasHighlightCol + 1);
    }
  }

  onCatalogDragEnded(event: CdkDragEnd, dict: DictBloco) {
    this.isDraggingFromCatalog = false;
    this.canvasHighlightCol = 0;

    if (!this.canvasRef) { event.source.reset(); return; }
    const rect = this.canvasRef.nativeElement.getBoundingClientRect();
    const insideCanvas =
      event.dropPoint.x >= rect.left && event.dropPoint.x <= rect.right &&
      event.dropPoint.y >= rect.top  && event.dropPoint.y <= rect.bottom;

    event.source.reset();
    if (!insideCanvas) return;

    const colWidth = rect.width / COLS;
    const col = Math.max(1, Math.min(COLS - 3, Math.floor((event.dropPoint.x - rect.left) / colWidth) + 1));
    const row = Math.max(1, Math.floor((event.dropPoint.y - rect.top) / ROW_HEIGHT) + 1);

    this.layoutAtual = [...this.layoutAtual, {
      tipo: dict.tipobloco,
      config: {},
      row, col, rowSpan: 1, colSpan: 4,
      _nome: dict.nome, _icone: dict.icone
    }];
    this._configVisualCache.clear();
    this.recalcularLinhas();
  }

  // ── Drag: canvas block repositioning ───────────────────────────────

  onBlocoDragEnded(event: CdkDragEnd, i: number) {
    if (!this.canvasRef) { event.source.reset(); return; }
    const rect = this.canvasRef.nativeElement.getBoundingClientRect();
    const colWidth = rect.width / COLS;

    event.source.reset();

    const { x, y } = event.dropPoint;
    if (x < rect.left || x > rect.right || y < rect.top || y > rect.bottom) return;

    const newCol = Math.max(1, Math.min(COLS, Math.floor((x - rect.left) / colWidth) + 1));
    const newRow = Math.max(1, Math.floor((y - rect.top) / ROW_HEIGHT) + 1);
    const bloco = this.layoutAtual[i];
    const clampedCol = Math.min(newCol, COLS - bloco.colSpan + 1);

    this._configVisualCache.delete(bloco);
    this.layoutAtual = [...this.layoutAtual];
    this.layoutAtual[i] = { ...bloco, col: clampedCol, row: newRow };
    this.recalcularLinhas();
  }

  // ── Resize ──────────────────────────────────────────────────────────

  iniciarResize(event: MouseEvent, index: number, tipo: 'col' | 'row') {
    event.preventDefault();
    event.stopPropagation();
    this.resizingIndex = index;
    this.resizingType = tipo;
    this.resizeStartX = event.clientX;
    this.resizeStartY = event.clientY;
    const bloco = this.layoutAtual[index];
    this.resizeStartSpan = tipo === 'col' ? bloco.colSpan : bloco.rowSpan;
    if (this.canvasRef)
      this.resizeColWidth = this.canvasRef.nativeElement.getBoundingClientRect().width / COLS;
  }

  @HostListener('window:mousemove', ['$event'])
  onMouseMove(event: MouseEvent) {
    if (this.resizingIndex === null || !this.resizingType) return;
    const bloco = this.layoutAtual[this.resizingIndex];
    this.layoutAtual = [...this.layoutAtual];
    if (this.resizingType === 'col') {
      const delta = event.clientX - this.resizeStartX;
      const newSpan = Math.max(1, Math.min(COLS - bloco.col + 1,
        this.resizeStartSpan + Math.round(delta / this.resizeColWidth)));
      this.layoutAtual[this.resizingIndex] = { ...bloco, colSpan: newSpan };
    } else {
      const delta = event.clientY - this.resizeStartY;
      const newSpan = Math.max(1, this.resizeStartSpan + Math.round(delta / ROW_HEIGHT));
      this.layoutAtual[this.resizingIndex] = { ...bloco, rowSpan: newSpan };
      this.recalcularLinhas();
    }
  }

  @HostListener('window:mouseup')
  onMouseUp() {
    this.resizingIndex = null;
    this.resizingType = null;
  }

  // ── Block editor (mesmo schemaConfig do v1) ──────────────────────────

  abrirEditorBloco(i: number) {
    const bloco = this.layoutAtual[i];
    const dict = this.blocos.find(d => d.tipobloco === bloco.tipo);
    try { this.editandoSchema = JSON.parse(dict?.schemaConfig ?? '{}'); } catch { this.editandoSchema = {}; }
    this.editandoCampos = Object.entries(this.editandoSchema).map(([key, def]: [string, any]) => ({
      key,
      label: def?.label ?? key,
      type: def?.type ?? 'string',
      placeholder: def?.placeholder ?? def?.default ?? ''
    }));
    this.editandoConfig = JSON.parse(JSON.stringify(bloco.config ?? {}));
    this.editandoNome = bloco._nome ?? bloco.tipo;
    this.editandoIndex = i;
  }

  fecharEditorBloco() {
    this.editandoIndex = null;
    this.editandoConfig = {};
    this.editandoSchema = {};
    this.editandoCampos = [];
  }

  confirmarEdicaoBloco() {
    if (this.editandoIndex === null) return;
    this._configVisualCache.delete(this.layoutAtual[this.editandoIndex]);
    this.layoutAtual[this.editandoIndex] = {
      ...this.layoutAtual[this.editandoIndex],
      config: { ...this.editandoConfig }
    };
    this.fecharEditorBloco();
  }

  getItemKeys(campoKey: string): string[] {
    const items = this.editandoConfig[campoKey];
    if (items?.length > 0) return Object.keys(items[0]);
    const def = this.editandoSchema[campoKey]?.default;
    if (def?.length > 0) return Object.keys(def[0]);
    return [];
  }

  adicionarItem(campoKey: string) {
    const novoItem: any = {};
    this.getItemKeys(campoKey).forEach((k: string) => novoItem[k] = '');
    this.editandoConfig[campoKey] = [...(this.editandoConfig[campoKey] ?? []), novoItem];
  }

  removerItem(campoKey: string, index: number) {
    this.editandoConfig[campoKey] = this.editandoConfig[campoKey].filter((_: any, i: number) => i !== index);
  }

  abrirGaleriaPicker(campo: string) {
    this.galeriaPickerCampo = campo;
    this.galeriaPickerAberto = true;
    this.http.get<any[]>(`${this.baseUrl}media`, { params: this.appParams() }).subscribe({
      next: r => this.galeriaImagens = r,
      error: () => {}
    });
  }

  selecionarImagemParaCampo(url: string) {
    if (this.galeriaPickerCampo) this.editandoConfig[this.galeriaPickerCampo] = url;
    this.galeriaPickerAberto = false;
    this.galeriaPickerCampo = null;
  }

  configVisual(bloco: BlocoLayoutV2): { key: string; label: string; valor: string }[] {
    if (this._configVisualCache.has(bloco)) return this._configVisualCache.get(bloco)!;
    const dict = this.blocos.find(d => d.tipobloco === bloco.tipo);
    let schema: any = {};
    try { schema = JSON.parse(dict?.schemaConfig ?? '{}'); } catch {}
    const result = Object.entries(bloco.config ?? {})
      .filter(([, v]) => v !== '' && v !== null && v !== undefined)
      .map(([k, v]) => ({
        key: k,
        label: schema[k]?.label ?? k,
        valor: Array.isArray(v) ? `${(v as any[]).length} item(s)` : String(v).substring(0, 100)
      }));
    this._configVisualCache.set(bloco, result);
    return result;
  }

  abrirPreview() {
    const appId = this.usuario.aplicacaoid ?? this.adminCtx.tenantId;
    const area = this.areas.find((a: any) => a.areaid === this.areaid);
    const url = area?.url ? `/preview/${appId}/${area.url}` : `/preview/${appId}`;
    window.open(url, '_blank');
  }

  // ── Importar rascunho ────────────────────────────────────────────────

  abrirModalRascunho() {
    this.importarRascunhoAberto = true;
    this.importarModo = 'upload';
    this.importarErro = '';
    this.rascunhoArquivo = null;
    this.rascunhoPreviewUrl = '';
    this.fundoArquivo = null;
    this.fundoPreviewUrl = '';
    this.drawingShapes = [];
    this.currentShape = null;
  }

  fecharModalRascunho() {
    this.importarRascunhoAberto = false;
    this.importarErro = '';
    this.rascunhoArquivo = null;
    this.rascunhoPreviewUrl = '';
    this.fundoArquivo = null;
    this.fundoPreviewUrl = '';
    this.drawingShapes = [];
    this.currentShape = null;
  }

  trocarModoRascunho(modo: 'upload' | 'canvas') {
    this.importarModo = modo;
    this.importarErro = '';
    if (modo === 'canvas') {
      setTimeout(() => this.iniciarCanvas(), 0);
    }
  }

  private iniciarCanvas() {
    const canvas = this.drawCanvasRef?.nativeElement;
    if (!canvas) return;
    const ctx = canvas.getContext('2d');
    if (!ctx) return;
    ctx.fillStyle = '#ffffff';
    ctx.fillRect(0, 0, canvas.width, canvas.height);
  }

  onArquivoRascunho(event: Event) {
    const input = event.target as HTMLInputElement;
    const file = input.files?.[0];
    if (!file) return;
    this.rascunhoArquivo = file;
    const reader = new FileReader();
    reader.onload = () => { this.rascunhoPreviewUrl = reader.result as string; };
    reader.readAsDataURL(file);
  }

  onArquivoFundo(event: Event) {
    const input = event.target as HTMLInputElement;
    const file = input.files?.[0];
    if (!file) return;
    this.fundoArquivo = file;
    const reader = new FileReader();
    reader.onload = () => { this.fundoPreviewUrl = reader.result as string; };
    reader.readAsDataURL(file);
  }

  importarRascunho() {
    this.importarErro = '';
    if (this.importarModo === 'upload') {
      if (!this.rascunhoArquivo) { this.importarErro = 'Selecione uma imagem JPG ou PNG.'; return; }
      this.enviarArquivoRascunho(this.rascunhoArquivo);
    } else {
      const canvas = this.drawCanvasRef?.nativeElement;
      if (!canvas) return;
      if (this.drawingShapes.length === 0) { this.importarErro = 'Desenhe ao menos um retângulo no canvas.'; return; }
      canvas.toBlob(blob => {
        if (!blob) return;
        this.enviarArquivoRascunho(new File([blob], 'rascunho.png', { type: 'image/png' }));
      }, 'image/png');
    }
  }

  private enviarArquivoRascunho(file: File) {
    this.importandoRascunho = true;
    const form = new FormData();
    form.append('arquivo', file);

    let endpoint = `${this.baseUrl}pagebuilder/interpretar-rascunho`;
    if (this.fundoArquivo) {
      form.append('imagemFundo', this.fundoArquivo);
      endpoint = `${this.baseUrl}pagebuilder/importar-com-fundo`;
    }

    this.http.post<any>(endpoint, form).subscribe({
      next: r => {
        this.importandoRascunho = false;
        const blocos: any[] = r.blocos ?? [];
        if (blocos.length === 0) {
          this.importarErro = 'Nenhum bloco reconhecido. Use formas mais definidas no rascunho (retângulos nítidos e bem delimitados).';
          return;
        }
        const startIndex = this.layoutAtual.length;
        const novos = blocos.map((b: any) => this.enriquecerBloco(b));
        this.setLayoutAtual([...this.layoutAtual, ...novos]);
        this.blocoNovosIndices = new Set(novos.map((_, i) => startIndex + i));
        setTimeout(() => { this.blocoNovosIndices = new Set(); }, 3000);
        this.fecharModalRascunho();
      },
      error: e => {
        this.importandoRascunho = false;
        this.importarErro = e.error?.erro ?? 'Erro ao interpretar o rascunho.';
      }
    });
  }

  // ── Canvas de desenho ────────────────────────────────────────────────

  onDrawMouseDown(event: MouseEvent) {
    const canvas = this.drawCanvasRef?.nativeElement;
    if (!canvas) return;
    const rect = canvas.getBoundingClientRect();
    this.isDrawingShape = true;
    this.drawStartX = (event.clientX - rect.left) * (canvas.width / rect.width);
    this.drawStartY = (event.clientY - rect.top) * (canvas.height / rect.height);
  }

  onDrawMouseMove(event: MouseEvent) {
    if (!this.isDrawingShape) return;
    const canvas = this.drawCanvasRef?.nativeElement;
    if (!canvas) return;
    const rect = canvas.getBoundingClientRect();
    const x = (event.clientX - rect.left) * (canvas.width / rect.width);
    const y = (event.clientY - rect.top) * (canvas.height / rect.height);
    this.currentShape = {
      x: Math.min(this.drawStartX, x),
      y: Math.min(this.drawStartY, y),
      w: Math.abs(x - this.drawStartX),
      h: Math.abs(y - this.drawStartY)
    };
    this.redrawCanvas();
  }

  onDrawMouseUp() {
    if (!this.isDrawingShape) return;
    this.isDrawingShape = false;
    if (this.currentShape && this.currentShape.w > 5 && this.currentShape.h > 5) {
      this.drawingShapes.push({ ...this.currentShape });
    }
    this.currentShape = null;
    this.redrawCanvas();
  }

  onDrawTouchStart(event: TouchEvent) {
    event.preventDefault();
    const touch = event.touches[0];
    this.onDrawMouseDown({ clientX: touch.clientX, clientY: touch.clientY } as MouseEvent);
  }

  onDrawTouchMove(event: TouchEvent) {
    event.preventDefault();
    const touch = event.touches[0];
    this.onDrawMouseMove({ clientX: touch.clientX, clientY: touch.clientY } as MouseEvent);
  }

  onDrawTouchEnd() {
    this.onDrawMouseUp();
  }

  limparDrawCanvas() {
    this.drawingShapes = [];
    this.currentShape = null;
    this.iniciarCanvas();
  }

  private redrawCanvas() {
    const canvas = this.drawCanvasRef?.nativeElement;
    if (!canvas) return;
    const ctx = canvas.getContext('2d');
    if (!ctx) return;
    ctx.clearRect(0, 0, canvas.width, canvas.height);
    ctx.fillStyle = '#ffffff';
    ctx.fillRect(0, 0, canvas.width, canvas.height);
    ctx.strokeStyle = '#333333';
    ctx.lineWidth = 2;
    ctx.setLineDash([]);
    for (const s of this.drawingShapes) {
      ctx.strokeRect(s.x, s.y, s.w, s.h);
    }
    if (this.currentShape) {
      ctx.strokeStyle = '#0d6efd';
      ctx.lineWidth = 1.5;
      ctx.setLineDash([4, 4]);
      ctx.strokeRect(this.currentShape.x, this.currentShape.y, this.currentShape.w, this.currentShape.h);
      ctx.setLineDash([]);
    }
  }
}
