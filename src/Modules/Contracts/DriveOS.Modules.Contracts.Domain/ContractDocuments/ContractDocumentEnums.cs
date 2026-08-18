namespace DriveOS.Modules.Contracts.Domain.ContractDocuments;

public enum ContractDocumentType { MainContract = 1, Annex = 2, Amendment = 3, Evidence = 4, Notice = 5, Other = 99 }
public enum ContractDocumentVisibility { InternalOnly = 1, Student = 2, Signatories = 3, AuthorizedParties = 4 }
public enum ContractDocumentStatus { Active = 1, Archived = 2 }
