using DriveOS.Modules.CRM.Domain.Leads;
using DriveOS.Modules.CRM.Domain.Tasks;
using DriveOS.SharedKernel.Identifiers;
using FluentAssertions;

namespace DriveOS.UnitTests.Modules.CRM.Tasks;

public sealed class CrmTaskTests
{
    [Fact]
    public void Create_ShouldNormalizeContentAndKeepTaskPending()
    {
        var result = CrmTask.Create(
            CrmTaskId.New(),
            OrganizationId.New(),
            LeadId.New(),
            CrmTaskType.Call,
            "  Rappeler le prospect  ",
            "  Après 18 h  ",
            DateTimeOffset.UtcNow.AddDays(1),
            null
        );

        result.IsSuccess.Should().BeTrue();
        result.Value.Title.Should().Be("Rappeler le prospect");
        result.Value.Notes.Should().Be("Après 18 h");
        result.Value.Status.Should().Be(CrmTaskStatus.Pending);
    }

    [Fact]
    public void Complete_ShouldRejectAlreadyClosedTask()
    {
        CrmTask task = CrmTask
            .Create(
                CrmTaskId.New(),
                OrganizationId.New(),
                LeadId.New(),
                CrmTaskType.Email,
                "Envoyer le dossier",
                null,
                DateTimeOffset.UtcNow.AddDays(1),
                null
            )
            .Value;

        task.Complete(DateTimeOffset.UtcNow).IsSuccess.Should().BeTrue();
        var second = task.Cancel(DateTimeOffset.UtcNow);

        second.IsFailure.Should().BeTrue();
        second.Error.Code.Should().Be("Crm.Tasks.AlreadyClosed");
    }
}
