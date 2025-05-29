using REM.Core.Api.DTOs;
using REM.Core.Api.Models;

namespace REM.Core.Api.Services;

public interface IContractService
{
    Task<IEnumerable<ContractDto>> GetAllContractsAsync();
    Task<ContractDto?> GetContractByIdAsync(int id);
    Task<ContractDto> CreateContractAsync(CreateContractDto createContractDto);
    Task<ContractDto?> UpdateContractAsync(int id, UpdateContractDto updateContractDto);
    Task<bool> DeleteContractAsync(int id);
    Task<IEnumerable<ContractDto>> SearchContractsAsync(string? description, string? status, int? vendorId, int? contractTypeId);
}
