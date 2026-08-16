using DriveOS.Application.Abstractions.Messaging;
using DriveOS.SharedKernel.Results;

namespace DriveOS.Modules.Students.Application.Documents;

public sealed class GetStudentDocumentsQueryHandler(IStudentDocumentService s)
    : IQueryHandler<GetStudentDocumentsQuery, StudentDocumentListResponse>
{
    public async Task<Result<StudentDocumentListResponse>> Handle(
        GetStudentDocumentsQuery q,
        CancellationToken ct
    )
    {
        var v = await s.GetAsync(q, ct);
        return v is null
            ? Result.Failure<StudentDocumentListResponse>(
                StudentDocumentApplicationErrors.StudentNotFound
            )
            : Result.Success(v);
    }
}

public sealed class RequestStudentDocumentCommandHandler(IStudentDocumentService s)
    : ICommandHandler<RequestStudentDocumentCommand, Guid>
{
    public Task<Result<Guid>> Handle(RequestStudentDocumentCommand c, CancellationToken ct) =>
        s.RequestAsync(c, ct);
}

public sealed class UploadStudentDocumentCommandHandler(IStudentDocumentService s)
    : ICommandHandler<UploadStudentDocumentCommand, Guid>
{
    public Task<Result<Guid>> Handle(UploadStudentDocumentCommand c, CancellationToken ct) =>
        s.UploadAsync(c, ct);
}

public sealed class ValidateStudentDocumentCommandHandler(IStudentDocumentService s)
    : ICommandHandler<ValidateStudentDocumentCommand>
{
    public Task<Result> Handle(ValidateStudentDocumentCommand c, CancellationToken ct) =>
        s.ValidateAsync(c, ct);
}

public sealed class ShareStudentDocumentCommandHandler(IStudentDocumentService s)
    : ICommandHandler<ShareStudentDocumentCommand>
{
    public Task<Result> Handle(ShareStudentDocumentCommand c, CancellationToken ct) =>
        s.ShareAsync(c, ct);
}

public sealed class ArchiveStudentDocumentCommandHandler(IStudentDocumentService s)
    : ICommandHandler<ArchiveStudentDocumentCommand>
{
    public Task<Result> Handle(ArchiveStudentDocumentCommand c, CancellationToken ct) =>
        s.ArchiveAsync(c, ct);
}

public sealed class DownloadStudentDocumentQueryHandler(IStudentDocumentService s)
    : IQueryHandler<DownloadStudentDocumentQuery, StudentDocumentDownload>
{
    public Task<Result<StudentDocumentDownload>> Handle(
        DownloadStudentDocumentQuery q,
        CancellationToken ct
    ) => s.DownloadAsync(q, ct);
}
