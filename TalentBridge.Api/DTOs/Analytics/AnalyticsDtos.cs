namespace TalentBridge.Api.DTOs.Analytics;

public record PlatformOverviewDto(
    int TotalJobs,
    int ActiveJobs,
    int TotalCandidates,
    int TotalRecruiters,
    int TotalApplications
);

public record JobAnalyticsDto(
    int JobId,
    string JobTitle,
    int TotalApplications,
    int Shortlisted,
    int Rejected,
    int Offered,
    double AverageMatchScore
);

public record RecruiterAnalyticsDto(
    int TotalJobsPosted,
    int ActiveJobs,
    int TotalApplicationsReceived,
    int ShortlistedCandidates,
    int OfferedCandidates
);
