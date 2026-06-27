import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { ResumeAnalyzerService } from '../core/services/resume-analyzer.service';
import { AnalysisSummary } from '../core/models/resume-analysis.model';

@Component({
  selector: 'app-resume-analyzer',
  templateUrl: './resume-analyzer.component.html',
  styleUrls: ['./resume-analyzer.component.css'],
  standalone: false
})
export class ResumeAnalyzerComponent implements OnInit {
  roles: string[] = [];
  selectedRole: string = '';
  selectedFile: File | null = null;
  history: AnalysisSummary[] = [];
  loading = false;
  uploading = false;
  error: string | null = null;
  isDragOver = false;

  constructor(
    private resumeService: ResumeAnalyzerService,
    private router: Router
  ) {}

  ngOnInit(): void {
    this.loadRoles();
    this.loadHistory();
  }

  loadRoles(): void {
    this.resumeService.getRoles().subscribe({
      next: (res) => {
        this.roles = res.roles;
        if (this.roles.length > 0) {
          this.selectedRole = this.roles[0];
        }
      },
      error: (err) => {
        console.error('Error loading roles', err);
      }
    });
  }

  loadHistory(): void {
    this.loading = true;
    this.resumeService.getHistory().subscribe({
      next: (res) => {
        this.history = res;
        this.loading = false;
      },
      error: (err) => {
        console.error('Error loading history', err);
        this.loading = false;
      }
    });
  }

  onFileSelected(event: any): void {
    const file = event.target.files[0];
    if (file) {
      this.validateAndSetFile(file);
    }
  }

  onDragOver(event: DragEvent): void {
    event.preventDefault();
    event.stopPropagation();
    this.isDragOver = true;
  }

  onDragLeave(event: DragEvent): void {
    event.preventDefault();
    event.stopPropagation();
    this.isDragOver = false;
  }

  onDrop(event: DragEvent): void {
    event.preventDefault();
    event.stopPropagation();
    this.isDragOver = false;
    const file = event.dataTransfer?.files[0];
    if (file) {
      this.validateAndSetFile(file);
    }
  }

  validateAndSetFile(file: File): void {
    this.error = null;
    const ext = file.name.substring(file.name.lastIndexOf('.')).toLowerCase();
    if (ext !== '.pdf' && ext !== '.docx') {
      this.error = 'Invalid file type. Only PDF and DOCX files are allowed.';
      this.selectedFile = null;
      return;
    }
    if (file.size > 5 * 1024 * 1024) {
      this.error = 'File size exceeds the 5 MB limit.';
      this.selectedFile = null;
      return;
    }
    this.selectedFile = file;
  }

  onSubmit(): void {
    if (!this.selectedFile || !this.selectedRole) {
      this.error = 'Please select a file and a target role.';
      return;
    }

    this.uploading = true;
    this.error = null;

    this.resumeService.uploadAndAnalyze(this.selectedFile, this.selectedRole).subscribe({
      next: (res) => {
        this.uploading = false;
        this.router.navigate(['/resume-analyzer/analysis', res.analysisId]);
      },
      error: (err) => {
        this.uploading = false;
        this.error = err.error || 'An error occurred while uploading and analyzing your resume.';
      }
    });
  }

  removeFile(): void {
    this.selectedFile = null;
    this.error = null;
  }
}
