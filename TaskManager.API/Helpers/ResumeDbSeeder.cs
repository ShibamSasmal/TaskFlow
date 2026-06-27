using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using TaskManager.API.Data;
using TaskManager.API.Models;
using TaskManager.API.Services.ML;

namespace TaskManager.API.Helpers
{
    public static class ResumeDbSeeder
    {
        public static async Task SeedAndTrainAsync(IApplicationBuilder app, string modelPath, string csvPath)
        {
            using var scope = app.ApplicationServices.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            // 1. Seed Database Tables
            await SeedTaxonomyAsync(context);

            // 2. Prepare training data and train ML.NET Model
            await PrepareAndTrainMlModelAsync(csvPath, modelPath);
        }

        private static async Task SeedTaxonomyAsync(AppDbContext context)
        {
            // Seed Skill Categories
            if (!await context.SkillCategories.AnyAsync())
            {
                var categories = new List<SkillCategory>
                {
                    new() { Name = "Programming Languages", Description = "Core language competencies" },
                    new() { Name = "Frameworks & Libraries", Description = "Development platforms and libraries" },
                    new() { Name = "Databases & Storage", Description = "Relational and NoSQL storage systems" },
                    new() { Name = "Cloud & Tools", Description = "Deployment platforms, version control, and environments" },
                    new() { Name = "Soft Skills & Methods", Description = "Methodologies and interpersonal competencies" }
                };

                context.SkillCategories.AddRange(categories);
                await context.SaveChangesAsync();
            }

            var catLanguages = await context.SkillCategories.FirstAsync(c => c.Name == "Programming Languages");
            var catFrameworks = await context.SkillCategories.FirstAsync(c => c.Name == "Frameworks & Libraries");
            var catDatabases = await context.SkillCategories.FirstAsync(c => c.Name == "Databases & Storage");
            var catCloudTools = await context.SkillCategories.FirstAsync(c => c.Name == "Cloud & Tools");
            var catSoftSkills = await context.SkillCategories.FirstAsync(c => c.Name == "Soft Skills & Methods");

            // Seed Skills
            if (!await context.Skills.AnyAsync())
            {
                var skills = new List<Skill>
                {
                    new() { Name = "C#", CategoryId = catLanguages.Id, Aliases = "c#,csharp,c sharp,.net" },
                    new() { Name = "Java", CategoryId = catLanguages.Id, Aliases = "java,spring" },
                    new() { Name = "Python", CategoryId = catLanguages.Id, Aliases = "python,py" },
                    new() { Name = "JavaScript", CategoryId = catLanguages.Id, Aliases = "javascript,js,es6" },
                    new() { Name = "TypeScript", CategoryId = catLanguages.Id, Aliases = "typescript,ts" },
                    new() { Name = "HTML", CategoryId = catLanguages.Id, Aliases = "html,html5" },
                    new() { Name = "CSS", CategoryId = catLanguages.Id, Aliases = "css,css3,scss,sass" },

                    new() { Name = "ASP.NET Core", CategoryId = catFrameworks.Id, Aliases = "asp.net core,asp.net,dotnet core" },
                    new() { Name = "Angular", CategoryId = catFrameworks.Id, Aliases = "angular,ng" },
                    new() { Name = "React", CategoryId = catFrameworks.Id, Aliases = "react,reactjs" },
                    new() { Name = "Spring Boot", CategoryId = catFrameworks.Id, Aliases = "spring boot,springboot" },
                    new() { Name = "RxJS", CategoryId = catFrameworks.Id, Aliases = "rxjs" },

                    new() { Name = "SQL", CategoryId = catDatabases.Id, Aliases = "sql,mysql,sqlite,query" },
                    new() { Name = "PostgreSQL", CategoryId = catDatabases.Id, Aliases = "postgresql,postgres" },
                    new() { Name = "SQL Server", CategoryId = catDatabases.Id, Aliases = "sql server,mssql" },
                    new() { Name = "MongoDB", CategoryId = catDatabases.Id, Aliases = "mongodb,mongo" },
                    new() { Name = "Redis", CategoryId = catDatabases.Id, Aliases = "redis" },

                    new() { Name = "Docker", CategoryId = catCloudTools.Id, Aliases = "docker,containers" },
                    new() { Name = "Kubernetes", CategoryId = catCloudTools.Id, Aliases = "kubernetes,k8s,k8" },
                    new() { Name = "Azure", CategoryId = catCloudTools.Id, Aliases = "azure" },
                    new() { Name = "AWS", CategoryId = catCloudTools.Id, Aliases = "aws,amazon web services" },
                    new() { Name = "Git", CategoryId = catCloudTools.Id, Aliases = "git,github,version control" },
                    new() { Name = "CI/CD", CategoryId = catCloudTools.Id, Aliases = "ci/cd,jenkins,github actions,pipelines" },
                    new() { Name = "Microservices", CategoryId = catCloudTools.Id, Aliases = "microservices,microservice" },

                    new() { Name = "Communication", CategoryId = catSoftSkills.Id, Aliases = "communication,presentation,written" },
                    new() { Name = "Leadership", CategoryId = catSoftSkills.Id, Aliases = "leadership,team lead,manager" },
                    new() { Name = "Problem-solving", CategoryId = catSoftSkills.Id, Aliases = "problem-solving,problem solving,analytical" },
                    new() { Name = "Agile", CategoryId = catSoftSkills.Id, Aliases = "agile,scrum,kanban" }
                };

                context.Skills.AddRange(skills);
                await context.SaveChangesAsync();
            }

            // Seed Role Templates and RoleSkills
            if (!await context.RoleTemplates.AnyAsync())
            {
                var backendRole = new RoleTemplate { RoleName = "Backend Developer", Description = "Core backend focus" };
                var frontendRole = new RoleTemplate { RoleName = "Frontend Developer", Description = "Core frontend SPA focus" };
                var fullstackRole = new RoleTemplate { RoleName = "Full Stack Developer", Description = "Unified fullstack development" };

                context.RoleTemplates.AddRange(backendRole, frontendRole, fullstackRole);
                await context.SaveChangesAsync();

                var allSkills = await context.Skills.ToListAsync();
                var skillMap = allSkills.ToDictionary(s => s.Name, StringComparer.OrdinalIgnoreCase);

                // Helper to add RoleSkill
                void AddRoleSkill(RoleTemplate role, string skillName, bool isRequired)
                {
                    if (skillMap.TryGetValue(skillName, out var s))
                    {
                        context.RoleSkills.Add(new RoleSkill
                        {
                            RoleTemplateId = role.Id,
                            SkillId = s.Id,
                            IsRequired = isRequired
                        });
                    }
                }

                // Backend Developer mapping
                AddRoleSkill(backendRole, "C#", true);
                AddRoleSkill(backendRole, "ASP.NET Core", true);
                AddRoleSkill(backendRole, "SQL", true);
                AddRoleSkill(backendRole, "Git", true);
                AddRoleSkill(backendRole, "Docker", false);
                AddRoleSkill(backendRole, "Azure", false);
                AddRoleSkill(backendRole, "Redis", false);
                AddRoleSkill(backendRole, "Microservices", false);
                AddRoleSkill(backendRole, "CI/CD", false);

                // Frontend Developer mapping
                AddRoleSkill(frontendRole, "Angular", true);
                AddRoleSkill(frontendRole, "TypeScript", true);
                AddRoleSkill(frontendRole, "JavaScript", true);
                AddRoleSkill(frontendRole, "Git", true);
                AddRoleSkill(frontendRole, "HTML", true);
                AddRoleSkill(frontendRole, "CSS", true);
                AddRoleSkill(frontendRole, "React", false);
                AddRoleSkill(frontendRole, "RxJS", false);

                // Full Stack Developer mapping
                AddRoleSkill(fullstackRole, "C#", true);
                AddRoleSkill(fullstackRole, "Angular", true);
                AddRoleSkill(fullstackRole, "SQL", true);
                AddRoleSkill(fullstackRole, "TypeScript", true);
                AddRoleSkill(fullstackRole, "Git", true);
                AddRoleSkill(fullstackRole, "ASP.NET Core", false);
                AddRoleSkill(fullstackRole, "Docker", false);
                AddRoleSkill(fullstackRole, "Azure", false);
                AddRoleSkill(fullstackRole, "Redis", false);

                await context.SaveChangesAsync();
            }
        }

        private static async Task PrepareAndTrainMlModelAsync(string csvPath, string modelPath)
        {
            var dir = Path.GetDirectoryName(csvPath);
            if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }

            // Create Seed training data CSV if missing
            if (!File.Exists(csvPath))
            {
                var trainingSamples = new List<(string Text, string Label)>
                {
                    ("Experienced C# and dotnet developer.", "C#"),
                    ("Hands on experience in C# syntax and generic programming.", "C#"),
                    ("Writing clean C# asynchronous code with async await.", "C#"),
                    ("Developed enterprise REST APIs with ASP.NET Core.", "ASP.NET Core"),
                    ("Designed middleware and dependency injection in ASP.NET Core.", "ASP.NET Core"),
                    ("Built backend services utilizing ASP.NET Core Web API.", "ASP.NET Core"),
                    ("Proficient in writing complex SQL queries and databases.", "SQL"),
                    ("Relational database schema modeling and SQL performance tuning.", "SQL"),
                    ("Created indexes, views, and stored procedures in SQL Server.", "SQL"),
                    ("Designed responsive interfaces with Angular 17.", "Angular"),
                    ("Created Angular components, Directives, and custom Pipes.", "Angular"),
                    ("Managed application states using RxJS and Services in Angular.", "Angular"),
                    ("Implemented modern TypeScript interfaces and classes.", "TypeScript"),
                    ("Type-safe frontend development utilizing TypeScript structures.", "TypeScript"),
                    ("Strong TypeScript programmer with solid design patterns.", "TypeScript"),
                    ("Deployed applications inside lightweight Docker containers.", "Docker"),
                    ("Configured multi-stage Dockerfiles and container registries.", "Docker"),
                    ("Containerized dotnet application services using Docker.", "Docker"),
                    ("Highly collaborative with excellent communication skills.", "Communication"),
                    ("Delivered technical presentations with clear communication.", "Communication"),
                    ("Facilitated sprint planning and client communication.", "Communication"),
                    ("Experienced leader directing team milestones and projects.", "Leadership"),
                    ("Served as scrum master and technical project lead.", "Leadership"),
                    ("Mentored junior developers and led team sprints.", "Leadership"),
                    ("Version control commits and repository merging in Git.", "Git"),
                    ("Resolving branch conflicts and rebasing code inside Git.", "Git"),
                    ("Extensive usage of Git, GitHub, and Pull Request reviews.", "Git"),
                    ("Configured CI/CD automated deployment pipelines.", "CI/CD"),
                    ("Managed GitLab CI, GitHub Actions and Jenkins pipelines.", "CI/CD"),
                    ("Automated unit testing checks inside CI/CD processes.", "CI/CD"),
                    ("Designed microservices using Docker, Kubernetes, and Azure.", "Microservices"),
                    ("Refactored monolith systems into scalable distributed microservices.", "Microservices"),
                    ("Implemented API Gateway patterns for backend microservices.", "Microservices"),
                    ("Deploying apps on Microsoft Azure app services.", "Azure"),
                    ("Configured CosmosDB and Azure Functions cloud services.", "Azure"),
                    ("Deploying scalable cloud compute instances in AWS.", "AWS"),
                    ("Configured AWS S3 buckets and RDS database servers.", "AWS"),
                    ("Experienced Python developer using Django and Pandas.", "Python"),
                    ("Data analysis scripting utilizing Python and numpy.", "Python"),
                    ("Created clean user interfaces with HTML and CSS.", "HTML"),
                    ("Developed beautiful layouts using HTML5 and SCSS styles.", "CSS"),
                    ("Designed responsive flexbox layout grids using custom CSS.", "CSS")
                };

                using var writer = new StreamWriter(csvPath);
                await writer.WriteLineAsync("ResumeText,SkillLabel");
                foreach (var sample in trainingSamples)
                {
                    var escapedText = $"\"{sample.Text.Replace("\"", "\"\"")}\"";
                    await writer.WriteLineAsync($"{escapedText},{sample.Label}");
                }

                Console.WriteLine($"[ML.NET Setup] Created training data CSV at: {csvPath}");
            }

            // Train model if zip file is missing
            if (!File.Exists(modelPath))
            {
                Console.WriteLine("[ML.NET Setup] Model file not found. Starting automatic training pipeline...");
                try
                {
                    ModelTrainer.TrainAndSaveModel(csvPath, modelPath);
                    Console.WriteLine("[ML.NET Setup] Model successfully trained and saved.");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[ML.NET Training Error] Failed to train model: {ex.Message}");
                }
            }
            else
            {
                Console.WriteLine($"[ML.NET Setup] Found existing trained model file at: {modelPath}");
            }
        }
    }
}
