import { Component, OnInit } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { ResumeAnalyzerService } from '../core/services/resume-analyzer.service';
import { AnalysisResponse, DetectedSkill } from '../core/models/resume-analysis.model';

@Component({
  selector: 'app-analysis-detail',
  templateUrl: './analysis-detail.component.html',
  styleUrls: ['./analysis-detail.component.css'],
  standalone: false
})
export class AnalysisDetailComponent implements OnInit {
  analysisId: string = '';
  analysis: AnalysisResponse | null = null;
  loading = false;
  error: string | null = null;

  constructor(
    private route: ActivatedRoute,
    private resumeService: ResumeAnalyzerService
  ) {}

  ngOnInit(): void {
    this.route.paramMap.subscribe(params => {
      const id = params.get('id');
      if (id) {
        this.analysisId = id;
        this.loadAnalysis();
      }
    });
  }

  loadAnalysis(): void {
    this.loading = true;
    this.error = null;

    this.resumeService.getAnalysis(this.analysisId).subscribe({
      next: (res) => {
        this.analysis = res;
        this.loading = false;
      },
      error: (err) => {
        console.error('Error loading analysis details', err);
        this.error = err.error || 'Failed to retrieve resume analysis report.';
        this.loading = false;
      }
    });
  }

  get detectedSkillsByCategory(): { [key: string]: DetectedSkill[] } {
    if (!this.analysis) return {};
    
    return this.analysis.detectedSkills.reduce((acc, skill) => {
      const category = skill.category || 'General';
      if (!acc[category]) {
        acc[category] = [];
      }
      acc[category].push(skill);
      return acc;
    }, {} as { [key: string]: DetectedSkill[] });
  }

  get strokeDashoffset(): number {
    if (!this.analysis) return 283; // Circumference of circle with r=45 is 2 * PI * 45 = 282.7
    const score = this.analysis.overallScore;
    return 283 - (283 * score) / 100;
  }
}
