using DomainRelay.Abstractions;
using DriveOS.SharedKernel.Results;

namespace DriveOS.Application.Abstractions.Messaging;

public interface IQueryHandler<in TQuery, TResponse>
    : IRequestHandler<TQuery, Result<TResponse>>
    where TQuery : IQuery<TResponse>;