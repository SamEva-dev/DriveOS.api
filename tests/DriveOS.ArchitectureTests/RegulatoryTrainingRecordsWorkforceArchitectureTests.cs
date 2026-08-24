using FluentAssertions;
using Xunit;

namespace DriveOS.ArchitectureTests;

public sealed class RegulatoryTrainingRecordsWorkforceArchitectureTests
{
    [Fact]
    public void Regulatory_projector_should_use_workforce_as_instructor_authorization_source()
    {
        string root = FindRepositoryRoot();
        string projector = File.ReadAllText(Path.Combine(root,
            "src", "DriveOS.Api", "Integrations", "RegulatoryTrainingRecords", "RegulatoryTrainingSessionProjector.cs"));

        projector.Should().Contain("IWorkforceInstructorAuthorizationReadService");
        projector.Should().NotContain("IInstructorRegulatoryCredentialReadService");
        projector.Should().Contain("path.LicenseCategoryCode");
    }

    private static string FindRepositoryRoot()
    {
        DirectoryInfo? directory = new(AppContext.BaseDirectory);
        while (directory is not null)
        {
            if (Directory.Exists(Path.Combine(directory.FullName, "src")))
                return directory.FullName;
            directory = directory.Parent;
        }
        throw new DirectoryNotFoundException("Could not locate repository root.");
    }
}
