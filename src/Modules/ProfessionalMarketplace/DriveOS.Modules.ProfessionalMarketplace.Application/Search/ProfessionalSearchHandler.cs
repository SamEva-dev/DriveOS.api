using DriveOS.Application.Abstractions.Messaging;
using DriveOS.SharedKernel.Results;

namespace DriveOS.Modules.ProfessionalMarketplace.Application.Search;

public sealed class SearchProfessionalsQueryHandler(IProfessionalSearchReadService search)
    :IQueryHandler<SearchProfessionalsQuery,ProfessionalSearchPage>
{
    public async Task<Result<ProfessionalSearchPage>> Handle(SearchProfessionalsQuery query,CancellationToken ct)
        =>Result.Success(await search.SearchAsync(query,ct));
}
