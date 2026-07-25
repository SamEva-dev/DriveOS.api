using DomainRelay;
using DomainRelay.Abstractions;
using DriveOS.SharedKernel.Results;

namespace DriveOS.Application.Abstractions.Messaging;

public interface IQuery<TResponse>
    : IRequest<Result<TResponse>>;