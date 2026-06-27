import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterModule } from '@angular/router';
import { SharedModule } from '../shared/shared.module';
import { ResumeAnalyzerComponent } from './resume-analyzer.component';
import { AnalysisDetailComponent } from './analysis-detail.component';

@NgModule({
  declarations: [
    ResumeAnalyzerComponent,
    AnalysisDetailComponent
  ],
  imports: [
    CommonModule,
    FormsModule,
    RouterModule,
    SharedModule
  ],
  exports: [
    ResumeAnalyzerComponent,
    AnalysisDetailComponent
  ]
})
export class ResumeAnalyzerModule { }
