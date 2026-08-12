using DriveOS.SharedKernel.Domain;
using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.Modules.CRM.Domain.Offers;

public sealed class CommercialOfferLine : Entity<CommercialOfferLineId>
{
    private CommercialOfferLine() { }

    internal CommercialOfferLine(CommercialOfferLineId id, CommercialOfferId offerId, OfferLineType type,
        ServiceId? serviceId, string description, decimal quantity, string unit,
        decimal unitPrice, decimal discountAmount, decimal taxRate,
        bool mandatory, OfferPriceSource priceSource, string? manualOverrideReason) : base(id)
    {
        OfferId = offerId;
        Type = type;
        ServiceId = serviceId;
        Description = description;
        Quantity = quantity;
        Unit = unit;
        UnitPrice = unitPrice;
        DiscountAmount = discountAmount;
        TaxRate = taxRate;
        Mandatory = mandatory;
        PriceSource = priceSource;
        ManualOverrideReason = manualOverrideReason;
        NetAmount = decimal.Round(quantity * unitPrice - discountAmount, 2);
        TaxAmount = decimal.Round(NetAmount * taxRate / 100m, 2);
        TotalAmount = NetAmount + TaxAmount;
    }

    public CommercialOfferId OfferId { get; private set; }
    public OfferLineType Type { get; private set; }
    public ServiceId? ServiceId { get; private set; }
    public string Description { get; private set; } = string.Empty;
    public decimal Quantity { get; private set; }
    public string Unit { get; private set; } = string.Empty;
    public decimal UnitPrice { get; private set; }
    public decimal DiscountAmount { get; private set; }
    public decimal TaxRate { get; private set; }
    public decimal NetAmount { get; private set; }
    public decimal TaxAmount { get; private set; }
    public decimal TotalAmount { get; private set; }
    public bool Mandatory { get; private set; }
    public OfferPriceSource PriceSource { get; private set; }
    public string? ManualOverrideReason { get; private set; }
}

public enum OfferLineType { RegistrationFee, TheoryTraining, PracticalLesson, SimulatorLesson, InitialAssessment, PedagogicalReview, ExamSupport, VehicleExamRental, DigitalAccess, AdministrativeService, PartnerTraining, Other }
public enum OfferPriceSource { StandardCatalog, BranchCatalog, NegotiatedPrice, Campaign, PartnerAgreement, ManualOverride }
