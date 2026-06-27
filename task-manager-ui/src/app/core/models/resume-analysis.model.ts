export interface AnalysisResponse {
  analysisId: string;
  fileName: string;
  targetRole: string;
  overallScore: number;
  scoreGrade: string;
  detectedSkills: DetectedSkill[];
  missingSkills: MissingSkill[];
  scoreBreakdown: ScoreBreakdown;
  suggestions: string[];
  processedAt: string;
}

export interface DetectedSkill {
  name: string;
  category: string;
  confidence: number;
}

export interface MissingSkill {
  name: string;
  priority: 'Required' | 'Preferred';
  reason: string;
  resourceUrl: string;
}

export interface ScoreBreakdown {
  skillCoverage: number;
  preferredSkills: number;
  resumeStructure: number;
  contentQuality: number;
  formatCompliance: number;
}

export interface AnalysisSummary {
  analysisId: string;
  fileName: string;
  targetRole: string;
  overallScore: number;
  scoreGrade: string;
  processedAt: string;
}
