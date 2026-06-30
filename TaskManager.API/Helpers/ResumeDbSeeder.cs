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
            bool newlySeeded = await SeedTaxonomyAsync(context);

            if (newlySeeded && File.Exists(modelPath))
            {
                Console.WriteLine("[ML.NET Setup] Taxonomy updated. Deleting existing model to force retrain...");
                try
                {
                    File.Delete(modelPath);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[ML.NET Setup] Failed to delete existing model file: {ex.Message}");
                }
            }

            // 2. Prepare training data and train ML.NET Model
            await PrepareAndTrainMlModelAsync(csvPath, modelPath, newlySeeded);
        }

        private static async Task<bool> SeedTaxonomyAsync(AppDbContext context)
        {
            bool databaseUpdated = false;

            // Seed Skill Categories
            var existingCategories = await context.SkillCategories.ToListAsync();
            var categoriesToSeed = new List<SkillCategory>
            {
                new() { Name = "Programming Languages", Description = "Core language competencies" },
                new() { Name = "Frameworks & Libraries", Description = "Development platforms and libraries" },
                new() { Name = "Databases & Storage", Description = "Relational and NoSQL storage systems" },
                new() { Name = "Cloud & Tools", Description = "Deployment platforms, version control, and environments" },
                new() { Name = "Soft Skills & Methods", Description = "Methodologies and interpersonal competencies" }
            };

            foreach (var cat in categoriesToSeed)
            {
                if (!existingCategories.Any(c => c.Name.Equals(cat.Name, StringComparison.OrdinalIgnoreCase)))
                {
                    context.SkillCategories.Add(cat);
                    databaseUpdated = true;
                }
            }

            if (databaseUpdated)
            {
                await context.SaveChangesAsync();
            }

            var catLanguages = await context.SkillCategories.FirstAsync(c => c.Name == "Programming Languages");
            var catFrameworks = await context.SkillCategories.FirstAsync(c => c.Name == "Frameworks & Libraries");
            var catDatabases = await context.SkillCategories.FirstAsync(c => c.Name == "Databases & Storage");
            var catCloudTools = await context.SkillCategories.FirstAsync(c => c.Name == "Cloud & Tools");
            var catSoftSkills = await context.SkillCategories.FirstAsync(c => c.Name == "Soft Skills & Methods");

            // Seed Skills
            var existingSkills = await context.Skills.ToListAsync();
            var skillsToSeed = new List<Skill>
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
                new() { Name = "Machine Learning", CategoryId = catFrameworks.Id, Aliases = "machine learning,ml,deep learning,artificial intelligence,ai" },
                new() { Name = "PyTorch", CategoryId = catFrameworks.Id, Aliases = "pytorch,torch" },
                new() { Name = "TensorFlow", CategoryId = catFrameworks.Id, Aliases = "tensorflow,keras" },
                new() { Name = "Scikit-learn", CategoryId = catFrameworks.Id, Aliases = "scikit-learn,sklearn,scikit learn" },
                new() { Name = "Pandas", CategoryId = catFrameworks.Id, Aliases = "pandas" },
                new() { Name = "NumPy", CategoryId = catFrameworks.Id, Aliases = "numpy" },

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

            bool skillsUpdated = false;
            foreach (var skill in skillsToSeed)
            {
                if (!existingSkills.Any(s => s.Name.Equals(skill.Name, StringComparison.OrdinalIgnoreCase)))
                {
                    context.Skills.Add(skill);
                    skillsUpdated = true;
                    databaseUpdated = true;
                }
            }

            if (skillsUpdated)
            {
                await context.SaveChangesAsync();
            }

            // Seed Role Templates
            var existingRoles = await context.RoleTemplates.ToListAsync();
            var rolesToSeed = new List<RoleTemplate>
            {
                new() { RoleName = "Backend Developer", Description = "Core backend focus" },
                new() { RoleName = "Frontend Developer", Description = "Core frontend SPA focus" },
                new() { RoleName = "Full Stack Developer", Description = "Unified fullstack development" },
                new() { RoleName = "AI/ML Engineer", Description = "Machine Learning and Artificial Intelligence focus" }
            };

            bool rolesUpdated = false;
            foreach (var role in rolesToSeed)
            {
                if (!existingRoles.Any(r => r.RoleName.Equals(role.RoleName, StringComparison.OrdinalIgnoreCase)))
                {
                    context.RoleTemplates.Add(role);
                    rolesUpdated = true;
                    databaseUpdated = true;
                }
            }

            if (rolesUpdated)
            {
                await context.SaveChangesAsync();
            }

            // Re-fetch all template roles and skills to build dictionary maps
            var dbRoles = await context.RoleTemplates.ToListAsync();
            var dbSkills = await context.Skills.ToListAsync();
            var skillMap = dbSkills.ToDictionary(s => s.Name, StringComparer.OrdinalIgnoreCase);

            var backendRole = dbRoles.First(r => r.RoleName == "Backend Developer");
            var frontendRole = dbRoles.First(r => r.RoleName == "Frontend Developer");
            var fullstackRole = dbRoles.First(r => r.RoleName == "Full Stack Developer");
            var aimlRole = dbRoles.First(r => r.RoleName == "AI/ML Engineer");

            // Seed RoleSkills
            var existingRoleSkills = await context.RoleSkills.ToListAsync();
            bool roleSkillsUpdated = false;

            void AddRoleSkillIfMissing(RoleTemplate role, string skillName, bool isRequired)
            {
                if (skillMap.TryGetValue(skillName, out var skill))
                {
                    bool exists = existingRoleSkills.Any(rs => rs.RoleTemplateId == role.Id && rs.SkillId == skill.Id);
                    if (!exists)
                    {
                        context.RoleSkills.Add(new RoleSkill
                        {
                            RoleTemplateId = role.Id,
                            SkillId = skill.Id,
                            IsRequired = isRequired
                        });
                        roleSkillsUpdated = true;
                        databaseUpdated = true;
                    }
                }
            }

            // Backend Developer mapping
            AddRoleSkillIfMissing(backendRole, "C#", true);
            AddRoleSkillIfMissing(backendRole, "ASP.NET Core", true);
            AddRoleSkillIfMissing(backendRole, "SQL", true);
            AddRoleSkillIfMissing(backendRole, "Git", true);
            AddRoleSkillIfMissing(backendRole, "Docker", false);
            AddRoleSkillIfMissing(backendRole, "Azure", false);
            AddRoleSkillIfMissing(backendRole, "Redis", false);
            AddRoleSkillIfMissing(backendRole, "Microservices", false);
            AddRoleSkillIfMissing(backendRole, "CI/CD", false);

            // Frontend Developer mapping
            AddRoleSkillIfMissing(frontendRole, "Angular", true);
            AddRoleSkillIfMissing(frontendRole, "TypeScript", true);
            AddRoleSkillIfMissing(frontendRole, "JavaScript", true);
            AddRoleSkillIfMissing(frontendRole, "Git", true);
            AddRoleSkillIfMissing(frontendRole, "HTML", true);
            AddRoleSkillIfMissing(frontendRole, "CSS", true);
            AddRoleSkillIfMissing(frontendRole, "React", false);
            AddRoleSkillIfMissing(frontendRole, "RxJS", false);

            // Full Stack Developer mapping
            AddRoleSkillIfMissing(fullstackRole, "C#", true);
            AddRoleSkillIfMissing(fullstackRole, "Angular", true);
            AddRoleSkillIfMissing(fullstackRole, "SQL", true);
            AddRoleSkillIfMissing(fullstackRole, "TypeScript", true);
            AddRoleSkillIfMissing(fullstackRole, "Git", true);
            AddRoleSkillIfMissing(fullstackRole, "ASP.NET Core", false);
            AddRoleSkillIfMissing(fullstackRole, "Docker", false);
            AddRoleSkillIfMissing(fullstackRole, "Azure", false);
            AddRoleSkillIfMissing(fullstackRole, "Redis", false);

            // AI/ML Engineer mapping
            AddRoleSkillIfMissing(aimlRole, "Python", true);
            AddRoleSkillIfMissing(aimlRole, "Machine Learning", true);
            AddRoleSkillIfMissing(aimlRole, "PyTorch", true);
            AddRoleSkillIfMissing(aimlRole, "TensorFlow", true);
            AddRoleSkillIfMissing(aimlRole, "Git", true);
            AddRoleSkillIfMissing(aimlRole, "Scikit-learn", false);
            AddRoleSkillIfMissing(aimlRole, "Pandas", false);
            AddRoleSkillIfMissing(aimlRole, "NumPy", false);
            AddRoleSkillIfMissing(aimlRole, "SQL", false);
            AddRoleSkillIfMissing(aimlRole, "Docker", false);

            if (roleSkillsUpdated)
            {
                await context.SaveChangesAsync();
            }

            return databaseUpdated;
        }

        private static async Task PrepareAndTrainMlModelAsync(string csvPath, string modelPath, bool forceRewrite)
        {
            var dir = Path.GetDirectoryName(csvPath);
            if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }

            // Create Seed training data CSV if missing or forced
            if (!File.Exists(csvPath) || forceRewrite)
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
                    ("Designed responsive flexbox layout grids using custom CSS.", "CSS"),

                    // AI/ML Engineer training samples
                    ("Developed machine learning models and neural networks.", "Machine Learning"),
                    ("Deep learning architecture design and AI solutions.", "Machine Learning"),
                    ("Built and evaluated classification algorithms for predictive analytics.", "Machine Learning"),
                    ("Trained deep learning models using PyTorch frameworks.", "PyTorch"),
                    ("Implemented neural network layers and training loops in PyTorch.", "PyTorch"),
                    ("Optimized tensor computations and GPU execution with PyTorch.", "PyTorch"),
                    ("Built convolutional neural networks using TensorFlow and Keras.", "TensorFlow"),
                    ("Deployed TensorFlow models to production environments.", "TensorFlow"),
                    ("Utilized TensorFlow for natural language processing and computer vision.", "TensorFlow"),
                    ("Engineered feature pipelines using Scikit-learn.", "Scikit-learn"),
                    ("Trained random forests and support vector machines in Scikit-learn.", "Scikit-learn"),
                    ("Model evaluation, cross validation, and hyperparameter tuning with Scikit-learn.", "Scikit-learn"),
                    ("Data manipulation and analysis using Python Pandas library.", "Pandas"),
                    ("Performed data cleaning, aggregation, and merging with Pandas.", "Pandas"),
                    ("Loaded and structured complex datasets into Pandas DataFrames.", "Pandas"),
                    ("Numerical computing and matrix operations using NumPy.", "NumPy"),
                    ("Optimized array processing and vectorized operations with NumPy.", "NumPy"),
                    ("Mathematical functions and linear algebra computing with NumPy.", "NumPy")
                };

                using var writer = new StreamWriter(csvPath);
                await writer.WriteLineAsync("ResumeText,SkillLabel");
                foreach (var sample in trainingSamples)
                {
                    var escapedText = $"\"{sample.Text.Replace("\"", "\"\"")}\"";
                    await writer.WriteLineAsync($"{escapedText},{sample.Label}");
                }

                Console.WriteLine($"[ML.NET Setup] Created/Updated training data CSV at: {csvPath}");
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
