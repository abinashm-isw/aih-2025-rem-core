using Microsoft.EntityFrameworkCore;
using REM.Core.Api.Data;
using REM.Core.Api.DTOs;
using REM.Core.Api.Models;

namespace REM.Core.Api.Services;

public class ContractService : IContractService
{
    private readonly OracleDbContext _context;
    private readonly ILogger<ContractService> _logger;

    public ContractService(OracleDbContext context, ILogger<ContractService> logger)
    {
        _context = context;
        _logger = logger;
    }    public async Task<IEnumerable<ContractDto>> GetAllContractsAsync()
    {
        try
        {
            // Use raw SQL to avoid Oracle boolean type mapping issues
            var sql = @"
                SELECT * FROM ""DEV_RAY2__REM"".""CONTRACTS_CONTRACT"" 
                WHERE (""ISARCHIVED"" IS NULL OR ""ISARCHIVED"" = 0)
                ORDER BY ""ID"" DESC
                FETCH FIRST 1000 ROWS ONLY";
            
            var contracts = await _context.Contracts
                .FromSqlRaw(sql)
                .AsNoTracking()
                .ToListAsync();

            return contracts.Select(MapToDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving all contracts");
            throw;
        }
    }public async Task<ContractDto?> GetContractByIdAsync(int id)
    {
        try
        {
            var contract = await _context.Contracts
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.Id == id);

            return contract != null ? MapToDto(contract) : null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving contract with ID {ContractId}", id);
            throw;
        }
    }

    public async Task<ContractDto> CreateContractAsync(CreateContractDto createContractDto)
    {
        try
        {            var contract = new Contract
            {
                Contracttypeid = createContractDto.Contracttypeid,
                Description = createContractDto.Description,
                Vendorid = createContractDto.Vendorid,
                Contractedpartyid = createContractDto.Contractedpartyid,
                Currencyid = createContractDto.Currencyid,
                Isreceivable = createContractDto.Isreceivable.HasValue ? (createContractDto.Isreceivable.Value ? 1 : 0) : (int?)null,
                Referenceno = createContractDto.Referenceno,
                Status = createContractDto.Status ?? "Active",
                Notes = createContractDto.Notes,                Entityid = Guid.NewGuid(),                Isarchived = 0,
                Isbroken = 0
            };

            _context.Contracts.Add(contract);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Created new contract with ID {ContractId}", contract.Id);
            return MapToDto(contract);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating new contract");
            throw;
        }
    }

    public async Task<ContractDto?> UpdateContractAsync(int id, UpdateContractDto updateContractDto)
    {
        try
        {
            var contract = await _context.Contracts
                .FirstOrDefaultAsync(c => c.Id == id);

            if (contract == null)
                return null;            // Update properties
            contract.Contracttypeid = updateContractDto.Contracttypeid;
            contract.Description = updateContractDto.Description;
            contract.Vendorid = updateContractDto.Vendorid;
            contract.Contractedpartyid = updateContractDto.Contractedpartyid;
            contract.Currencyid = updateContractDto.Currencyid;
            contract.Isreceivable = updateContractDto.Isreceivable.HasValue ? (updateContractDto.Isreceivable.Value ? 1 : 0) : (int?)null;
            contract.Isarchived = updateContractDto.Isarchived.HasValue ? (updateContractDto.Isarchived.Value ? 1 : 0) : (int?)null;
            contract.Referenceno = updateContractDto.Referenceno;
            contract.Status = updateContractDto.Status;
            contract.Notes = updateContractDto.Notes;

            await _context.SaveChangesAsync();

            _logger.LogInformation("Updated contract with ID {ContractId}", id);
            return MapToDto(contract);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating contract with ID {ContractId}", id);
            throw;
        }
    }

    public async Task<bool> DeleteContractAsync(int id)
    {
        try
        {
            var contract = await _context.Contracts
                .FirstOrDefaultAsync(c => c.Id == id);

            if (contract == null)
                return false;            // Soft delete by archiving
            contract.Isarchived = 1;
            contract.Archiveddate = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            _logger.LogInformation("Archived contract with ID {ContractId}", id);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error archiving contract with ID {ContractId}", id);
            throw;
        }
    }    public async Task<IEnumerable<ContractDto>> SearchContractsAsync(string? description, string? status, int? vendorId, int? contractTypeId)
    {        try
        {
            // Build dynamic SQL to avoid Oracle boolean type mapping issues
            var whereConditions = new List<string> { "(\"ISARCHIVED\" IS NULL OR \"ISARCHIVED\" = 0)" };
            var parameters = new List<object>();
            var paramIndex = 0;

            if (!string.IsNullOrEmpty(description))
            {
                whereConditions.Add($"\"DESCRIPTION\" LIKE {{{paramIndex}}}");
                parameters.Add($"%{description}%");
                paramIndex++;
            }

            if (!string.IsNullOrEmpty(status))
            {
                whereConditions.Add($"\"STATUS\" = {{{paramIndex}}}");
                parameters.Add(status);
                paramIndex++;
            }

            if (vendorId.HasValue)
            {
                whereConditions.Add($"\"VENDORID\" = {{{paramIndex}}}");
                parameters.Add(vendorId.Value);
                paramIndex++;
            }

            if (contractTypeId.HasValue)
            {
                whereConditions.Add($"\"CONTRACTTYPEID\" = {{{paramIndex}}}");
                parameters.Add(contractTypeId.Value);
                paramIndex++;
            }

            var sql = $@"
                SELECT * FROM ""DEV_RAY2__REM"".""CONTRACTS_CONTRACT"" 
                WHERE {string.Join(" AND ", whereConditions)}
                ORDER BY ""ID"" DESC
                FETCH FIRST 500 ROWS ONLY";

            var contracts = await _context.Contracts
                .FromSqlRaw(sql, parameters.ToArray())
                .AsNoTracking()
                .ToListAsync();

            return contracts.Select(MapToDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching contracts");
            throw;
        }
    }private static ContractDto MapToDto(Contract contract)
    {
        return new ContractDto
        {
            Id = contract.Id,
            Contracttypeid = contract.Contracttypeid,
            Description = contract.Description,
            Vendorid = contract.Vendorid,
            Contractedpartyid = contract.Contractedpartyid,
            Currencyid = contract.Currencyid,
            Isreceivable = contract.Isreceivable.HasValue ? contract.Isreceivable.Value == 1 : (bool?)null,
            Isarchived = contract.Isarchived.HasValue ? contract.Isarchived.Value == 1 : (bool?)null,
            Isinholdover = contract.Isinholdover.HasValue ? contract.Isinholdover.Value == 1 : (bool?)null,
            Entityid = contract.Entityid,
            Discriminator = contract.Discriminator,
            Isbroken = contract.Isbroken.HasValue ? contract.Isbroken.Value == 1 : (bool?)null,
            Netequivalentfactor = contract.Netequivalentfactor,
            LeaseaccountingOriginalpurchaseprice = contract.LeaseaccountingOriginalpurchaseprice,
            LeaseaccountingEoltakeownership = contract.LeaseaccountingEoltakeownership.HasValue ? contract.LeaseaccountingEoltakeownership.Value == 1 : (bool?)null,
            LeaseaccountingInitialprepayment = contract.LeaseaccountingInitialprepayment,
            LeaseaccountingUsefullife = contract.LeaseaccountingUsefullife,
            LeaseaccountingCalculatedrestoringrate = contract.LeaseaccountingCalculatedrestoringrate,
            LeaseaccountingLeasetype = contract.LeaseaccountingLeasetype,
            LeaseaccountingAssetcategorytype = contract.LeaseaccountingAssetcategorytype,
            LeaseaccountingLedgersystem = contract.LeaseaccountingLedgersystem,
            Makegooddateofobligation = contract.Makegooddateofobligation,
            LeaseaccountingStartdate = contract.LeaseaccountingStartdate,
            LeaseaccountingManualoverride = contract.LeaseaccountingManualoverride,
            Archiveddate = contract.Archiveddate,
            Holdoverstartdate = contract.Holdoverstartdate,
            LeaseaccountingForcereview = contract.LeaseaccountingForcereview.HasValue ? contract.LeaseaccountingForcereview.Value == 1 : (bool?)null,
            Treasuryapproverid = contract.Treasuryapproverid,
            Ispartialbuilding = contract.Ispartialbuilding.HasValue ? contract.Ispartialbuilding.Value == 1 : (bool?)null,
            LifecycleState = contract.LifecycleState,
            Clonedfromcontractid = contract.Clonedfromcontractid,
            LeaseaccountingAccountingcode = contract.LeaseaccountingAccountingcode,
            Notes = contract.Notes,
            Referenceno = contract.Referenceno,
            Status = contract.Status,
            Terminationcost = contract.Terminationcost,
            Terminationdate = contract.Terminationdate
        };
    }
}
