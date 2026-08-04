namespace DriveOS.Modules.Organizations.Application.OrganizationConfigurations.Effective;

public interface IJsonConfigurationMerger
{
    string Merge(string baseJson, string overrideJson);
}
