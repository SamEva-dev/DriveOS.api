using DriveOS.Application.Abstractions.Persistence;

namespace DriveOS.Modules.CRM.Application.Abstractions.Persistence;

/// <summary>
/// CRM-specific unit of work boundary.
/// This prevents the CRM DbContext from replacing another module's IUnitOfWork
/// registration in the application service provider.
/// </summary>
public interface ICrmUnitOfWork : IUnitOfWork { }
