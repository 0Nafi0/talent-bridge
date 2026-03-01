using Microsoft.AspNetCore.Identity;
using TalentBridge.Api.Models;

namespace TalentBridge.Api.Data;

public static class DatabaseSeeder
{
    public static async Task SeedAsync(IServiceProvider serviceProvider)
    {
        var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var db = serviceProvider.GetRequiredService<ApplicationDbContext>();

        // ── 1. Roles ───────────────────────────────────────────────────
        var roles = new[] { "Admin", "Recruiter", "Candidate" };
        foreach (var role in roles)
            if (!await roleManager.RoleExistsAsync(role))
                await roleManager.CreateAsync(new IdentityRole(role));

        // ── 2. Skills ──────────────────────────────────────────────────
        if (!db.Skills.Any())
        {
            var skills = new List<Skill>
            {
                new() { Name = "C#",            Category = "Programming" },
                new() { Name = "ASP.NET Core",  Category = "Programming" },
                new() { Name = "JavaScript",    Category = "Programming" },
                new() { Name = "TypeScript",    Category = "Programming" },
                new() { Name = "React",         Category = "Frontend" },
                new() { Name = "Angular",       Category = "Frontend" },
                new() { Name = "Python",        Category = "Programming" },
                new() { Name = "SQL",           Category = "Database" },
                new() { Name = "PostgreSQL",    Category = "Database" },
                new() { Name = "MongoDB",       Category = "Database" },
                new() { Name = "Docker",        Category = "DevOps" },
                new() { Name = "Kubernetes",    Category = "DevOps" },
                new() { Name = "Git",           Category = "Tools" },
                new() { Name = "UI/UX Design",  Category = "Design" },
                new() { Name = "Project Management", Category = "Management" }
            };
            db.Skills.AddRange(skills);
            await db.SaveChangesAsync();
        }

        // ── 3. Admin user ──────────────────────────────────────────────
        await CreateUserIfNotExists(
            userManager, db,
            email: "admin@talentbridge.com",
            password: "Admin@123",
            fullName: "TalentBridge Admin",
            role: "Admin"
        );

        // ── 4. Recruiters ──────────────────────────────────────────────
        var recruiter1Id = await CreateUserIfNotExists(
            userManager, db,
            email: "recruiter1@acmecorp.com",
            password: "Recruiter@123",
            fullName: "Alice Johnson",
            role: "Recruiter"
        );
        var recruiter2Id = await CreateUserIfNotExists(
            userManager, db,
            email: "recruiter2@techwave.io",
            password: "Recruiter@123",
            fullName: "Bob Williams",
            role: "Recruiter"
        );

        // Recruiter profiles
        if (!db.RecruiterProfiles.Any())
        {
            db.RecruiterProfiles.AddRange(
                new RecruiterProfile
                {
                    UserId = recruiter1Id,
                    FullName = "Alice Johnson",
                    CompanyName = "Acme Corp",
                    CompanyDescription = "A global leader in software solutions.",
                    CompanyWebsite = "https://acmecorp.com",
                    Location = "New York, USA"
                },
                new RecruiterProfile
                {
                    UserId = recruiter2Id,
                    FullName = "Bob Williams",
                    CompanyName = "TechWave",
                    CompanyDescription = "Innovative startup building next-gen web apps.",
                    CompanyWebsite = "https://techwave.io",
                    Location = "San Francisco, USA"
                }
            );
            await db.SaveChangesAsync();
        }

        // ── 5. Candidates ──────────────────────────────────────────────
        var candidate1Id = await CreateUserIfNotExists(
            userManager, db,
            email: "candidate1@mail.com",
            password: "Candidate@123",
            fullName: "Charlie Brown",
            role: "Candidate"
        );
        var candidate2Id = await CreateUserIfNotExists(
            userManager, db,
            email: "candidate2@mail.com",
            password: "Candidate@123",
            fullName: "Diana Prince",
            role: "Candidate"
        );
        var candidate3Id = await CreateUserIfNotExists(
            userManager, db,
            email: "candidate3@mail.com",
            password: "Candidate@123",
            fullName: "Ethan Hunt",
            role: "Candidate"
        );

        // Candidate profiles + skills
        if (!db.CandidateProfiles.Any())
        {
            var cs_skill    = db.Skills.First(s => s.Name == "C#");
            var asp_skill   = db.Skills.First(s => s.Name == "ASP.NET Core");
            var react_skill = db.Skills.First(s => s.Name == "React");
            var js_skill    = db.Skills.First(s => s.Name == "JavaScript");
            var ts_skill    = db.Skills.First(s => s.Name == "TypeScript");
            var sql_skill   = db.Skills.First(s => s.Name == "SQL");
            var py_skill    = db.Skills.First(s => s.Name == "Python");
            var docker_skill = db.Skills.First(s => s.Name == "Docker");

            var profile1 = new CandidateProfile
            {
                UserId = candidate1Id,
                FullName = "Charlie Brown",
                Headline = "Full Stack .NET Developer",
                Bio = "5 years of experience in backend and frontend development.",
                Location = "Austin, TX",
                YearsOfExperience = 5,
                CandidateSkills =
                [
                    new CandidateSkill { SkillId = cs_skill.Id,     YearsOfExperience = 5 },
                    new CandidateSkill { SkillId = asp_skill.Id,    YearsOfExperience = 3 },
                    new CandidateSkill { SkillId = sql_skill.Id,    YearsOfExperience = 4 },
                    new CandidateSkill { SkillId = react_skill.Id,  YearsOfExperience = 2 }
                ]
            };

            var profile2 = new CandidateProfile
            {
                UserId = candidate2Id,
                FullName = "Diana Prince",
                Headline = "Frontend React Developer",
                Bio = "Passionate about UI/UX and accessible web design.",
                Location = "London, UK",
                YearsOfExperience = 3,
                CandidateSkills =
                [
                    new CandidateSkill { SkillId = js_skill.Id,    YearsOfExperience = 3 },
                    new CandidateSkill { SkillId = ts_skill.Id,    YearsOfExperience = 2 },
                    new CandidateSkill { SkillId = react_skill.Id, YearsOfExperience = 3 }
                ]
            };

            var profile3 = new CandidateProfile
            {
                UserId = candidate3Id,
                FullName = "Ethan Hunt",
                Headline = "DevOps & Backend Engineer",
                Bio = "Expert in containerization and cloud deployments.",
                Location = "Berlin, Germany",
                YearsOfExperience = 7,
                CandidateSkills =
                [
                    new CandidateSkill { SkillId = py_skill.Id,     YearsOfExperience = 6 },
                    new CandidateSkill { SkillId = docker_skill.Id, YearsOfExperience = 4 },
                    new CandidateSkill { SkillId = sql_skill.Id,    YearsOfExperience = 5 }
                ]
            };

            db.CandidateProfiles.AddRange(profile1, profile2, profile3);
            await db.SaveChangesAsync();
        }

        // ── 6. Jobs ────────────────────────────────────────────────────
        if (!db.Jobs.Any())
        {
            var rp1 = db.RecruiterProfiles.First(r => r.CompanyName == "Acme Corp");
            var rp2 = db.RecruiterProfiles.First(r => r.CompanyName == "TechWave");

            var cs_id    = db.Skills.First(s => s.Name == "C#").Id;
            var asp_id   = db.Skills.First(s => s.Name == "ASP.NET Core").Id;
            var react_id = db.Skills.First(s => s.Name == "React").Id;
            var ts_id    = db.Skills.First(s => s.Name == "TypeScript").Id;
            var js_id    = db.Skills.First(s => s.Name == "JavaScript").Id;
            var py_id    = db.Skills.First(s => s.Name == "Python").Id;
            var docker_id = db.Skills.First(s => s.Name == "Docker").Id;
            var sql_id   = db.Skills.First(s => s.Name == "SQL").Id;

            var jobs = new List<Job>
            {
                new()
                {
                    RecruiterProfileId = rp1.Id,
                    Title = "Senior .NET Backend Developer",
                    Description = "We are looking for a senior .NET developer to build scalable REST APIs.",
                    Location = "New York, USA",
                    JobType = JobType.FullTime,
                    Status = JobStatus.Open,
                    SalaryMin = 90000,
                    SalaryMax = 130000,
                    ExperienceLevel = "Senior",
                    PostedAt = DateTime.UtcNow.AddDays(-10),
                    ExpiresAt = DateTime.UtcNow.AddDays(20),
                    JobSkills =
                    [
                        new JobSkill { SkillId = cs_id,  IsRequired = true },
                        new JobSkill { SkillId = asp_id, IsRequired = true },
                        new JobSkill { SkillId = sql_id, IsRequired = false }
                    ]
                },
                new()
                {
                    RecruiterProfileId = rp1.Id,
                    Title = "React Frontend Developer",
                    Description = "Join our team to build beautiful, performant UIs using React and TypeScript.",
                    Location = "Remote",
                    JobType = JobType.Remote,
                    Status = JobStatus.Open,
                    SalaryMin = 70000,
                    SalaryMax = 100000,
                    ExperienceLevel = "Mid",
                    PostedAt = DateTime.UtcNow.AddDays(-5),
                    ExpiresAt = DateTime.UtcNow.AddDays(25),
                    JobSkills =
                    [
                        new JobSkill { SkillId = react_id, IsRequired = true },
                        new JobSkill { SkillId = ts_id,    IsRequired = true },
                        new JobSkill { SkillId = js_id,    IsRequired = false }
                    ]
                },
                new()
                {
                    RecruiterProfileId = rp2.Id,
                    Title = "DevOps Engineer",
                    Description = "Looking for a DevOps engineer proficient in Docker, Kubernetes, and CI/CD pipelines.",
                    Location = "San Francisco, USA",
                    JobType = JobType.FullTime,
                    Status = JobStatus.Open,
                    SalaryMin = 110000,
                    SalaryMax = 150000,
                    ExperienceLevel = "Senior",
                    PostedAt = DateTime.UtcNow.AddDays(-3),
                    ExpiresAt = DateTime.UtcNow.AddDays(27),
                    JobSkills =
                    [
                        new JobSkill { SkillId = docker_id, IsRequired = true },
                        new JobSkill { SkillId = py_id,     IsRequired = false }
                    ]
                },
                new()
                {
                    RecruiterProfileId = rp2.Id,
                    Title = "Junior JavaScript Developer",
                    Description = "Exciting opportunity for a junior developer to grow with a dynamic startup.",
                    Location = "Remote",
                    JobType = JobType.Contract,
                    Status = JobStatus.Open,
                    SalaryMin = 40000,
                    SalaryMax = 60000,
                    ExperienceLevel = "Junior",
                    PostedAt = DateTime.UtcNow.AddDays(-7),
                    ExpiresAt = DateTime.UtcNow.AddDays(23),
                    JobSkills =
                    [
                        new JobSkill { SkillId = js_id, IsRequired = true },
                        new JobSkill { SkillId = react_id, IsRequired = false }
                    ]
                },
                new()
                {
                    RecruiterProfileId = rp1.Id,
                    Title = "Data Engineer (Python/SQL)",
                    Description = "Build and maintain data pipelines using Python and SQL.",
                    Location = "New York, USA",
                    JobType = JobType.FullTime,
                    Status = JobStatus.Open,
                    SalaryMin = 85000,
                    SalaryMax = 120000,
                    ExperienceLevel = "Mid",
                    PostedAt = DateTime.UtcNow.AddDays(-2),
                    ExpiresAt = DateTime.UtcNow.AddDays(28),
                    JobSkills =
                    [
                        new JobSkill { SkillId = py_id,  IsRequired = true },
                        new JobSkill { SkillId = sql_id, IsRequired = true }
                    ]
                }
            };

            db.Jobs.AddRange(jobs);
            await db.SaveChangesAsync();
        }

        // ── 7. Applications + Timelines ────────────────────────────────
        if (!db.JobApplications.Any())
        {
            var cp1 = db.CandidateProfiles.First(c => c.FullName == "Charlie Brown");
            var cp2 = db.CandidateProfiles.First(c => c.FullName == "Diana Prince");
            var cp3 = db.CandidateProfiles.First(c => c.FullName == "Ethan Hunt");

            var job1 = db.Jobs.First(j => j.Title == "Senior .NET Backend Developer");
            var job2 = db.Jobs.First(j => j.Title == "React Frontend Developer");
            var job3 = db.Jobs.First(j => j.Title == "DevOps Engineer");
            var job4 = db.Jobs.First(j => j.Title == "Junior JavaScript Developer");

            var adminUser = await userManager.FindByEmailAsync("admin@talentbridge.com");
            var adminId = adminUser!.Id;

            var applications = new List<JobApplication>
            {
                new()
                {
                    CandidateProfileId = cp1.Id,
                    JobId = job1.Id,
                    Status = ApplicationStatus.Shortlisted,
                    CoverLetter = "I am very excited to apply for this position. My 5 years of .NET experience make me a great fit.",
                    MatchScore = 85.0,
                    AppliedAt = DateTime.UtcNow.AddDays(-8),
                    Timelines =
                    [
                        new ApplicationTimeline { Status = ApplicationStatus.Applied,     Notes = "Application received.",    ChangedAt = DateTime.UtcNow.AddDays(-8), ChangedByUserId = adminId },
                        new ApplicationTimeline { Status = ApplicationStatus.Shortlisted, Notes = "Strong profile, shortlisted for review.", ChangedAt = DateTime.UtcNow.AddDays(-5), ChangedByUserId = adminId }
                    ]
                },
                new()
                {
                    CandidateProfileId = cp2.Id,
                    JobId = job2.Id,
                    Status = ApplicationStatus.Interview,
                    CoverLetter = "React and TypeScript are my strongest skills and I'd love to contribute to your team.",
                    MatchScore = 92.0,
                    AppliedAt = DateTime.UtcNow.AddDays(-4),
                    Timelines =
                    [
                        new ApplicationTimeline { Status = ApplicationStatus.Applied,     Notes = "Application received.",                ChangedAt = DateTime.UtcNow.AddDays(-4), ChangedByUserId = adminId },
                        new ApplicationTimeline { Status = ApplicationStatus.Shortlisted, Notes = "Excellent React experience.",           ChangedAt = DateTime.UtcNow.AddDays(-3), ChangedByUserId = adminId },
                        new ApplicationTimeline { Status = ApplicationStatus.Interview,   Notes = "Scheduled for technical interview.",    ChangedAt = DateTime.UtcNow.AddDays(-1), ChangedByUserId = adminId }
                    ]
                },
                new()
                {
                    CandidateProfileId = cp3.Id,
                    JobId = job3.Id,
                    Status = ApplicationStatus.Applied,
                    CoverLetter = "My Docker and DevOps expertise aligns perfectly with this role.",
                    MatchScore = 78.0,
                    AppliedAt = DateTime.UtcNow.AddDays(-2),
                    Timelines =
                    [
                        new ApplicationTimeline { Status = ApplicationStatus.Applied, Notes = "Application received.", ChangedAt = DateTime.UtcNow.AddDays(-2), ChangedByUserId = adminId }
                    ]
                },
                new()
                {
                    CandidateProfileId = cp2.Id,
                    JobId = job4.Id,
                    Status = ApplicationStatus.Rejected,
                    CoverLetter = "Eager to grow as a developer in a startup environment.",
                    MatchScore = 55.0,
                    AppliedAt = DateTime.UtcNow.AddDays(-6),
                    Timelines =
                    [
                        new ApplicationTimeline { Status = ApplicationStatus.Applied,  Notes = "Application received.",              ChangedAt = DateTime.UtcNow.AddDays(-6), ChangedByUserId = adminId },
                        new ApplicationTimeline { Status = ApplicationStatus.Rejected, Notes = "Looking for more experienced candidates.", ChangedAt = DateTime.UtcNow.AddDays(-4), ChangedByUserId = adminId }
                    ]
                }
            };

            db.JobApplications.AddRange(applications);
            await db.SaveChangesAsync();
        }

        // ── 8. Notifications ───────────────────────────────────────────
        if (!db.Notifications.Any())
        {
            var candidate1 = await userManager.FindByEmailAsync("candidate1@mail.com");
            var candidate2 = await userManager.FindByEmailAsync("candidate2@mail.com");

            db.Notifications.AddRange(
                new Notification
                {
                    UserId = candidate1!.Id,
                    Title = "Application Shortlisted",
                    Message = "Your application for 'Senior .NET Backend Developer' has been shortlisted!",
                    Type = NotificationType.StatusUpdate,
                    IsRead = false,
                    CreatedAt = DateTime.UtcNow.AddDays(-5)
                },
                new Notification
                {
                    UserId = candidate2!.Id,
                    Title = "Interview Scheduled",
                    Message = "Congratulations! You have been invited for a technical interview for 'React Frontend Developer'.",
                    Type = NotificationType.StatusUpdate,
                    IsRead = false,
                    CreatedAt = DateTime.UtcNow.AddDays(-1)
                },
                new Notification
                {
                    UserId = candidate2.Id,
                    Title = "Application Rejected",
                    Message = "Unfortunately, your application for 'Junior JavaScript Developer' was not successful.",
                    Type = NotificationType.StatusUpdate,
                    IsRead = true,
                    CreatedAt = DateTime.UtcNow.AddDays(-4)
                }
            );
            await db.SaveChangesAsync();
        }
    }

    // ── Helper ─────────────────────────────────────────────────────────
    private static async Task<string> CreateUserIfNotExists(
        UserManager<ApplicationUser> userManager,
        ApplicationDbContext db,
        string email, string password, string fullName, string role)
    {
        var existing = await userManager.FindByEmailAsync(email);
        if (existing is not null) return existing.Id;

        var user = new ApplicationUser
        {
            FullName = fullName,
            Email = email,
            UserName = email,
            Role = role,
            EmailConfirmed = true
        };

        var result = await userManager.CreateAsync(user, password);
        if (!result.Succeeded)
            throw new Exception($"Failed to create user {email}: {string.Join(", ", result.Errors.Select(e => e.Description))}");

        await userManager.AddToRoleAsync(user, role);
        return user.Id;
    }
}
