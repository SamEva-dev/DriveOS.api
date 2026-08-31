using DriveOS.Application.Abstractions.Time;
using DriveOS.Modules.ProfessionalMarketplace.Application.Notifications;
using DriveOS.Modules.ProfessionalMarketplace.Application.Persistence;
using DriveOS.Modules.ProfessionalMarketplace.Domain.Compliance;
using DriveOS.Modules.ProfessionalMarketplace.Domain.Engagements;
using DriveOS.Modules.ProfessionalMarketplace.Domain.Missions;
using DriveOS.Modules.ProfessionalMarketplace.Domain.ProfessionalProfiles;
using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.Modules.ProfessionalMarketplace.Application.Compliance;

public sealed record ProfessionalComplianceExpirationRunResult(
    int DocumentsMarkedExpiringSoon,
    int DocumentsExpired,
    int CredentialsExpired,
    int ProfilesReevaluated,
    int NotificationsQueued);

public interface IProfessionalComplianceExpirationAutomation
{
    Task<ProfessionalComplianceExpirationRunResult> RunAsync(
        int warningDays=30,
        CancellationToken cancellationToken=default);
}

public sealed class ProfessionalComplianceExpirationAutomation(
    IProfessionalDocumentRepository documents,
    IProfessionalCredentialRepository credentials,
    IProfessionalProfileRepository profiles,
    IProfessionalComplianceRequirementRepository requirements,
    IProfessionalCompliancePolicyRepository policies,
    IProfessionalComplianceWaiverRepository waivers,
    IProfessionalMissionRepository missions,
    IProfessionalEngagementRepository engagements,
    IProfessionalComplianceOperationalGateway operational,
    IProfessionalMarketplaceUnitOfWork uow,
    IMarketplaceNotificationGateway notifications,
    IClock clock):IProfessionalComplianceExpirationAutomation
{
    public async Task<ProfessionalComplianceExpirationRunResult> RunAsync(
        int warningDays=30,
        CancellationToken cancellationToken=default)
    {
        int safeWarning=Math.Clamp(warningDays,1,180);
        DateOnly today=DateOnly.FromDateTime(clock.UtcNow.UtcDateTime);
        DateOnly warningDate=today.AddDays(safeWarning);

        var docs=await documents.ListExpirationCandidatesAsync(today,warningDate,true,cancellationToken);
        var creds=await credentials.ListExpirationCandidatesAsync(today,warningDate,true,cancellationToken);
        var expiredWaivers=await waivers.ListExpiredActiveAsync(today,true,cancellationToken);

        int expiring=0,expiredDocs=0,expiredCreds=0,queued=0;
        var affectedProfiles=new HashSet<ProfessionalProfileId>();

        foreach(var waiver in expiredWaivers)
        {
            if(waiver.Expire(today,clock.UtcNow).IsSuccess)
                affectedProfiles.Add(waiver.ProfessionalProfileId);
        }

        foreach(var document in docs)
        {
            if(document.ExpirationDate is not DateOnly expiry)continue;

            if(expiry<today)
            {
                if(document.MarkExpired(clock.UtcNow).IsSuccess)
                {
                    expiredDocs++;
                    affectedProfiles.Add(document.ProfessionalProfileId);
                }
            }
            else if(document.Status==ProfessionalDocumentStatus.Valid)
            {
                if(document.MarkExpiringSoon(clock.UtcNow).IsSuccess)
                {
                    expiring++;
                    affectedProfiles.Add(document.ProfessionalProfileId);
                }
            }
        }

        foreach(var credential in creds)
        {
            if(credential.ValidUntil is not DateOnly expiry)continue;

            if(expiry<today&&credential.MarkExpired(clock.UtcNow).IsSuccess)
            {
                expiredCreds++;
                affectedProfiles.Add(credential.ProfessionalProfileId);
            }
            else
            {
                affectedProfiles.Add(credential.ProfessionalProfileId);
            }
        }

        // Persist evidence/waiver state first. Retried runs are idempotent.
        await uow.CommitAsync(cancellationToken);

        int reevaluated=0;
        foreach(ProfessionalProfileId profileId in affectedProfiles)
        {
            ProfessionalProfile? profile=await profiles.GetByIdForUpdateAsync(profileId,cancellationToken);
            if(profile is null||string.IsNullOrWhiteSpace(profile.BillingCountryCode))continue;

            IReadOnlyList<ProfessionalComplianceRequirement> applicable=
                await requirements.ListApplicableAsync(
                    profile.BillingCountryCode,
                    profile.ProfessionalType,
                    profile.TeachingCategoryCodes,
                    today,
                    cancellationToken);

            IReadOnlyList<ProfessionalDocument> profileDocs=
                await documents.ListByProfileAsync(profile.Id,cancellationToken);

            IReadOnlyList<ProfessionalCredential> profileCreds=
                await credentials.ListByProfileAsync(profile.Id,cancellationToken);

            var missing=new List<string>();
            var invalid=new List<string>();
            var pending=new List<string>();

            foreach(ProfessionalComplianceRequirement requirement in applicable.Where(x=>x.Mandatory))
            {
                if(requirement.EvidenceKind==ProfessionalEvidenceKind.Document)
                {
                    ProfessionalDocument[] matches=profileDocs.Where(x=>
                        x.DocumentTypeCode==requirement.EvidenceTypeCode&&
                        x.CountryCode==requirement.CountryCode).ToArray();

                    if(matches.Length==0){missing.Add(requirement.RequirementCode);continue;}
                    if(matches.Any(x=>x.IsValidOn(today)))continue;

                    if(matches.Any(x=>x.Status is
                        ProfessionalDocumentStatus.Uploaded or
                        ProfessionalDocumentStatus.PendingReview or
                        ProfessionalDocumentStatus.ExpiringSoon))
                    {
                        pending.Add(requirement.RequirementCode);
                        continue;
                    }

                    invalid.Add(requirement.RequirementCode);
                    continue;
                }

                ProfessionalCredential[] matchesCred=profileCreds.Where(x=>
                    x.CredentialTypeCode==requirement.EvidenceTypeCode&&
                    x.CountryCode==requirement.CountryCode).ToArray();

                if(requirement.ApplicableCategoryCodes.Length>0)
                {
                    matchesCred=matchesCred.Where(x=>
                        requirement.ApplicableCategoryCodes.All(cat=>
                            x.CategoryCodes.Contains(cat,StringComparer.Ordinal))).ToArray();
                }

                if(matchesCred.Length==0){missing.Add(requirement.RequirementCode);continue;}
                if(matchesCred.Any(x=>x.IsValidOn(today)))continue;

                if(matchesCred.Any(x=>x.Status==ProfessionalCredentialStatus.PendingVerification))
                {
                    pending.Add(requirement.RequirementCode);
                    continue;
                }

                invalid.Add(requirement.RequirementCode);
            }

            ProfessionalComplianceStatus status;
            if(!profile.IsProfileComplete)
                status=ProfessionalComplianceStatus.Incomplete;
            else if(invalid.Count>0&&applicable.Where(x=>x.Blocking)
                .Any(x=>invalid.Contains(x.RequirementCode,StringComparer.Ordinal)))
                status=ProfessionalComplianceStatus.NonCompliant;
            else if(missing.Count>0)
                status=ProfessionalComplianceStatus.Incomplete;
            else if(pending.Count>0)
                status=ProfessionalComplianceStatus.PendingReview;
            else if(invalid.Count>0)
                status=ProfessionalComplianceStatus.PartiallyCompliant;
            else
                status=ProfessionalComplianceStatus.Compliant;

            profile.MarkComplianceAutomated(status,clock.UtcNow);

            string[] violations=missing.Concat(invalid)
                .Distinct(StringComparer.Ordinal)
                .ToArray();

            ProfessionalComplianceEnforcementAction? selectedAction=null;
            DateOnly? selectedGraceUntil=null;
            var selectedRequirementCodes=new List<string>();

            foreach(string requirementCode in violations)
            {
                ProfessionalComplianceRequirement? requirement=
                    applicable.FirstOrDefault(x=>x.RequirementCode==requirementCode);

                if(requirement is null)continue;

                ProfessionalComplianceWaiver? waiver=await waivers.GetEffectiveAsync(
                    profile.Id,
                    profile.BillingCountryCode,
                    requirementCode,
                    today,
                    cancellationToken);

                if(waiver is not null)
                    continue;

                ProfessionalComplianceCriticalityPolicy? policy=await policies.GetApplicableAsync(
                    profile.BillingCountryCode,
                    requirementCode,
                    today,
                    cancellationToken);

                if(policy is null)
                    continue; // No policy means compliance reporting only, never an invented operational sanction.

                DateOnly violationDate=GetViolationDate(
                    requirement,
                    profileDocs,
                    profileCreds,
                    today);

                DateOnly graceUntil=violationDate.AddDays(policy.GracePeriodDays);

                ProfessionalComplianceEnforcementAction effectiveAction=
                    policy.GracePeriodDays>0&&today<=graceUntil
                        ?ProfessionalComplianceEnforcementAction.AlertOnly
                        :policy.Action;

                if(selectedAction is null||(int)effectiveAction>(int)selectedAction.Value)
                {
                    selectedAction=effectiveAction;
                    selectedGraceUntil=policy.GracePeriodDays>0?graceUntil:null;
                    selectedRequirementCodes.Clear();
                    selectedRequirementCodes.Add(requirementCode);
                }
                else if(effectiveAction==selectedAction.Value)
                {
                    selectedRequirementCodes.Add(requirementCode);
                    if(selectedGraceUntil is null||graceUntil>selectedGraceUntil)
                        selectedGraceUntil=policy.GracePeriodDays>0?graceUntil:selectedGraceUntil;
                }
            }

            IReadOnlyList<ProfessionalEngagement> profileEngagements=
                await engagements.ListByProfileAsync(profile.Id,cancellationToken);

            OrganizationId[] activeOrganizations=profileEngagements
                .Where(x=>x.Status is ProfessionalEngagementStatus.Active or ProfessionalEngagementStatus.Suspended)
                .Select(x=>x.OrganizationId)
                .Distinct()
                .ToArray();

            if(selectedAction is ProfessionalComplianceEnforcementAction action)
            {
                string reason=$"Compliance policy {action}: {string.Join(", ",selectedRequirementCodes)}";

                profile.ApplyComplianceEnforcement(action,reason,selectedGraceUntil,clock.UtcNow);

                if(action is ProfessionalComplianceEnforcementAction.PauseMissions or
                    ProfessionalComplianceEnforcementAction.SuspendProfessional)
                {
                    IReadOnlyList<ProfessionalMission> profileMissions=
                        await missions.ListByProfileAsync(profile.Id,cancellationToken);

                    foreach(ProfessionalMission snapshot in profileMissions.Where(x=>x.Status==ProfessionalMissionStatus.Active))
                    {
                        ProfessionalMission? tracked=await missions.GetAsync(snapshot.Id,true,cancellationToken);
                        if(tracked is not null&&tracked.Status==ProfessionalMissionStatus.Active)
                            tracked.PauseByCompliancePolicy(reason,clock.UtcNow);
                    }
                }

                if(profile.UserId is UserId operationalUser&&!operationalUser.IsEmpty)
                {
                    await operational.ApplyAsync(new(
                        operationalUser,
                        activeOrganizations,
                        action is ProfessionalComplianceEnforcementAction.BlockNewSessions or
                            ProfessionalComplianceEnforcementAction.PauseMissions or
                            ProfessionalComplianceEnforcementAction.SuspendProfessional,
                        reason),cancellationToken);
                }
            }
            else
            {
                profile.ClearComplianceEnforcement(clock.UtcNow);

                if(profile.UserId is UserId operationalUser&&!operationalUser.IsEmpty)
                {
                    await operational.ApplyAsync(new(
                        operationalUser,
                        activeOrganizations,
                        false,
                        "Compliance restriction cleared"),cancellationToken);
                }
            }

            reevaluated++;

            if(profile.UserId is UserId userId&&!userId.IsEmpty)
            {
                ProfessionalCredential? nearest=profileCreds
                    .Where(x=>x.ValidUntil!=null&&x.ValidUntil>=today&&x.ValidUntil<=warningDate)
                    .OrderBy(x=>x.ValidUntil)
                    .FirstOrDefault();

                if(nearest is not null)
                {
                    await notifications.TryEnqueueAsync(new(
                        "User",userId.Value,null,"COMPLIANCE",
                        "professionalMarketplace.notifications.credentialExpiring",
                        $"credential-expiring:{nearest.Id.Value}:{nearest.ValidUntil:yyyyMMdd}",
                        new Dictionary<string,string?>
                        {
                            ["credentialId"]=nearest.Id.Value.ToString(),
                            ["credentialType"]=nearest.CredentialTypeCode,
                            ["expiresOn"]=nearest.ValidUntil?.ToString("yyyy-MM-dd")
                        },
                        "PROFESSIONAL_CREDENTIAL",nearest.Id.Value,
                        profile.ProfessionalEmail,
                        profile.Languages.FirstOrDefault(x=>x.StartsWith("fr",StringComparison.OrdinalIgnoreCase))??"en",
                        null),cancellationToken);
                    queued++;
                }

                ProfessionalDocument? nearestDocument=profileDocs
                    .Where(x=>x.ExpirationDate!=null&&x.ExpirationDate>=today&&x.ExpirationDate<=warningDate)
                    .OrderBy(x=>x.ExpirationDate)
                    .FirstOrDefault();

                if(nearestDocument is not null)
                {
                    await notifications.TryEnqueueAsync(new(
                        "User",userId.Value,null,"COMPLIANCE",
                        "professionalMarketplace.notifications.credentialExpiring",
                        $"document-expiring:{nearestDocument.Id.Value}:{nearestDocument.ExpirationDate:yyyyMMdd}",
                        new Dictionary<string,string?>
                        {
                            ["documentId"]=nearestDocument.Id.Value.ToString(),
                            ["documentType"]=nearestDocument.DocumentTypeCode,
                            ["expiresOn"]=nearestDocument.ExpirationDate?.ToString("yyyy-MM-dd")
                        },
                        "PROFESSIONAL_DOCUMENT",nearestDocument.Id.Value,
                        profile.ProfessionalEmail,
                        profile.Languages.FirstOrDefault(x=>x.StartsWith("fr",StringComparison.OrdinalIgnoreCase))??"en",
                        null),cancellationToken);
                    queued++;
                }

                if(selectedAction is ProfessionalComplianceEnforcementAction enforcedAction)
                {
                    await notifications.TryEnqueueAsync(new(
                        "User",userId.Value,null,"COMPLIANCE",
                        "professionalMarketplace.notifications.compliancePolicyApplied",
                        $"compliance-policy:{profile.Id.Value}:{enforcedAction}:{today:yyyyMMdd}",
                        new Dictionary<string,string?>
                        {
                            ["profileId"]=profile.Id.Value.ToString(),
                            ["status"]=status.ToString(),
                            ["action"]=enforcedAction.ToString(),
                            ["requirements"]=string.Join(",",selectedRequirementCodes),
                            ["graceUntil"]=selectedGraceUntil?.ToString("yyyy-MM-dd")
                        },
                        "PROFESSIONAL_PROFILE",profile.Id.Value,
                        profile.ProfessionalEmail,
                        profile.Languages.FirstOrDefault(x=>x.StartsWith("fr",StringComparison.OrdinalIgnoreCase))??"en",
                        null),cancellationToken);
                    queued++;
                }
            }
        }

        await uow.CommitAsync(cancellationToken);
        return new(expiring,expiredDocs,expiredCreds,reevaluated,queued);
    }

    private static DateOnly GetViolationDate(
        ProfessionalComplianceRequirement requirement,
        IReadOnlyList<ProfessionalDocument> documents,
        IReadOnlyList<ProfessionalCredential> credentials,
        DateOnly today)
    {
        if(requirement.EvidenceKind==ProfessionalEvidenceKind.Document)
        {
            DateOnly[] expirations=documents
                .Where(x=>x.DocumentTypeCode==requirement.EvidenceTypeCode&&
                          x.CountryCode==requirement.CountryCode&&
                          x.ExpirationDate!=null)
                .Select(x=>x.ExpirationDate!.Value)
                .Where(x=>x<today)
                .ToArray();

            return expirations.Length==0?today:expirations.Max().AddDays(1);
        }

        DateOnly[] credentialExpirations=credentials
            .Where(x=>x.CredentialTypeCode==requirement.EvidenceTypeCode&&
                      x.CountryCode==requirement.CountryCode&&
                      x.ValidUntil!=null)
            .Select(x=>x.ValidUntil!.Value)
            .Where(x=>x<today)
            .ToArray();

        return credentialExpirations.Length==0?today:credentialExpirations.Max().AddDays(1);
    }
}
