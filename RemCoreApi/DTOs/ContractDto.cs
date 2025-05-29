namespace REM.Core.Api.DTOs;

public class ContractDto
{
    public int Id { get; set; }
    public int? Contracttypeid { get; set; }
    public string? Description { get; set; }
    public int? Vendorid { get; set; }
    public int? Contractedpartyid { get; set; }
    public int? Currencyid { get; set; }
    public bool? Isreceivable { get; set; }
    public bool? Isarchived { get; set; }
    public bool? Isinholdover { get; set; }
    public Guid? Entityid { get; set; }
    public string? Discriminator { get; set; }
    public bool? Isbroken { get; set; }
    public decimal? Netequivalentfactor { get; set; }
    public decimal? LeaseaccountingOriginalpurchaseprice { get; set; }
    public bool? LeaseaccountingEoltakeownership { get; set; }
    public decimal? LeaseaccountingInitialprepayment { get; set; }
    public int? LeaseaccountingUsefullife { get; set; }
    public decimal? LeaseaccountingCalculatedrestoringrate { get; set; }
    public string? LeaseaccountingLeasetype { get; set; }
    public string? LeaseaccountingAssetcategorytype { get; set; }
    public string? LeaseaccountingLedgersystem { get; set; }
    public DateTime? Makegooddateofobligation { get; set; }
    public DateTime? LeaseaccountingStartdate { get; set; }
    public int? LeaseaccountingManualoverride { get; set; }
    public DateTime? Archiveddate { get; set; }
    public DateTime? Holdoverstartdate { get; set; }
    public bool? LeaseaccountingForcereview { get; set; }
    public int? Treasuryapproverid { get; set; }
    public bool? Ispartialbuilding { get; set; }
    public string? LifecycleState { get; set; }
    public int? Clonedfromcontractid { get; set; }
    public string? LeaseaccountingAccountingcode { get; set; }
    public string? Notes { get; set; }
    public string? Referenceno { get; set; }
    public string? Status { get; set; }
    public decimal? Terminationcost { get; set; }
    public DateTime? Terminationdate { get; set; }
}

public class CreateContractDto
{
    public int? Contracttypeid { get; set; }
    public string? Description { get; set; }
    public int? Vendorid { get; set; }
    public int? Contractedpartyid { get; set; }
    public int? Currencyid { get; set; }
    public bool? Isreceivable { get; set; }
    public string? Referenceno { get; set; }
    public string? Status { get; set; }
    public string? Notes { get; set; }
}

public class UpdateContractDto
{
    public int? Contracttypeid { get; set; }
    public string? Description { get; set; }
    public int? Vendorid { get; set; }
    public int? Contractedpartyid { get; set; }
    public int? Currencyid { get; set; }
    public bool? Isreceivable { get; set; }
    public bool? Isarchived { get; set; }
    public string? Referenceno { get; set; }
    public string? Status { get; set; }
    public string? Notes { get; set; }
}
