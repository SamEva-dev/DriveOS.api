using DomainRelay.Abstractions;
using DriveOS.SharedKernel.Results;

namespace DriveOS.Application.Abstractions.Messaging;

public interface ICommand : IRequest<Result>;

public interface ICommand<TResponse> : IRequest<Result<TResponse>>;
