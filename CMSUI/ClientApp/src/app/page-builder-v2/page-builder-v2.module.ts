import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterModule } from '@angular/router';
import { DragDropModule } from '@angular/cdk/drag-drop';
import { PageBuilderV2Component } from './page-builder-v2.component';

@NgModule({
  declarations: [PageBuilderV2Component],
  imports: [
    CommonModule,
    FormsModule,
    DragDropModule,
    RouterModule.forChild([
      { path: '', component: PageBuilderV2Component },
      { path: ':areaid', component: PageBuilderV2Component }
    ])
  ]
})
export class PageBuilderV2Module { }
