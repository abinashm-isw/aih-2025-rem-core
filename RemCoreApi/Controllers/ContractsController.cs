using Microsoft.AspNetCore.Mvc;
using RemCoreApi.DTOs;
using RemCoreApi.Services;

namespace RemCoreApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class ContractsController : ControllerBase
{
    private readonly IContractService _contractService;
    private readonly ILogger<ContractsController> _logger;

    public ContractsController(IContractService contractService, ILogger<ContractsController> logger)
    {
        _contractService = contractService;
        _logger = logger;
    }

    /// <summary>
    /// Get all contracts (excluding archived ones by default)
    /// </summary>
    /// <returns>List of contracts</returns>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<ContractDto>), 200)]
    [ProducesResponseType(500)]
    public async Task<ActionResult<IEnumerable<ContractDto>>> GetAllContracts()
    {
        try
        {
            var contracts = await _contractService.GetAllContractsAsync();
            return Ok(contracts);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving all contracts");
            return StatusCode(500, "An error occurred while retrieving contracts");
        }
    }

    /// <summary>
    /// Get a specific contract by ID
    /// </summary>
    /// <param name="id">Contract ID</param>
    /// <returns>Contract details</returns>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ContractDto), 200)]
    [ProducesResponseType(404)]
    [ProducesResponseType(500)]
    public async Task<ActionResult<ContractDto>> GetContract(int id)
    {
        try
        {
            var contract = await _contractService.GetContractByIdAsync(id);
            
            if (contract == null)
            {
                return NotFound($"Contract with ID {id} not found");
            }

            return Ok(contract);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving contract with ID {ContractId}", id);
            return StatusCode(500, "An error occurred while retrieving the contract");
        }
    }

    /// <summary>
    /// Create a new contract
    /// </summary>
    /// <param name="createContractDto">Contract creation data</param>
    /// <returns>Created contract</returns>
    [HttpPost]
    [ProducesResponseType(typeof(ContractDto), 201)]
    [ProducesResponseType(400)]
    [ProducesResponseType(500)]
    public async Task<ActionResult<ContractDto>> CreateContract([FromBody] CreateContractDto createContractDto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var contract = await _contractService.CreateContractAsync(createContractDto);
            
            return CreatedAtAction(
                nameof(GetContract), 
                new { id = contract.Id }, 
                contract);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating new contract");
            return StatusCode(500, "An error occurred while creating the contract");
        }
    }

    /// <summary>
    /// Update an existing contract
    /// </summary>
    /// <param name="id">Contract ID</param>
    /// <param name="updateContractDto">Contract update data</param>
    /// <returns>Updated contract</returns>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(ContractDto), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    [ProducesResponseType(500)]
    public async Task<ActionResult<ContractDto>> UpdateContract(int id, [FromBody] UpdateContractDto updateContractDto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var contract = await _contractService.UpdateContractAsync(id, updateContractDto);
            
            if (contract == null)
            {
                return NotFound($"Contract with ID {id} not found");
            }

            return Ok(contract);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating contract with ID {ContractId}", id);
            return StatusCode(500, "An error occurred while updating the contract");
        }
    }

    /// <summary>
    /// Delete (archive) a contract
    /// </summary>
    /// <param name="id">Contract ID</param>
    /// <returns>Success status</returns>
    [HttpDelete("{id}")]
    [ProducesResponseType(204)]
    [ProducesResponseType(404)]
    [ProducesResponseType(500)]
    public async Task<IActionResult> DeleteContract(int id)
    {
        try
        {
            var result = await _contractService.DeleteContractAsync(id);
            
            if (!result)
            {
                return NotFound($"Contract with ID {id} not found");
            }

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting contract with ID {ContractId}", id);
            return StatusCode(500, "An error occurred while deleting the contract");
        }
    }

    /// <summary>
    /// Search contracts with various filters
    /// </summary>
    /// <param name="description">Filter by description (contains)</param>
    /// <param name="status">Filter by status (exact match)</param>
    /// <param name="vendorId">Filter by vendor ID</param>
    /// <param name="contractTypeId">Filter by contract type ID</param>
    /// <returns>Filtered list of contracts</returns>
    [HttpGet("search")]
    [ProducesResponseType(typeof(IEnumerable<ContractDto>), 200)]
    [ProducesResponseType(500)]
    public async Task<ActionResult<IEnumerable<ContractDto>>> SearchContracts(
        [FromQuery] string? description = null,
        [FromQuery] string? status = null,
        [FromQuery] int? vendorId = null,
        [FromQuery] int? contractTypeId = null)
    {
        try
        {
            var contracts = await _contractService.SearchContractsAsync(
                description, status, vendorId, contractTypeId);
            
            return Ok(contracts);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching contracts");
            return StatusCode(500, "An error occurred while searching contracts");
        }
    }

    /// <summary>
    /// Get contract statistics (health check endpoint)
    /// </summary>
    /// <returns>Basic statistics about contracts</returns>
    [HttpGet("stats")]
    [ProducesResponseType(typeof(object), 200)]
    [ProducesResponseType(500)]
    public async Task<ActionResult> GetContractStats()
    {
        try
        {
            var allContracts = await _contractService.GetAllContractsAsync();
            var contractsList = allContracts.ToList();
            
            var stats = new
            {
                TotalActiveContracts = contractsList.Count,
                ContractsByStatus = contractsList
                    .GroupBy(c => c.Status ?? "Unknown")
                    .ToDictionary(g => g.Key, g => g.Count()),
                RecentContracts = contractsList
                    .OrderByDescending(c => c.Id)
                    .Take(5)
                    .Select(c => new { c.Id, c.Description, c.Status })
                    .ToList()
            };

            return Ok(stats);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving contract statistics");
            return StatusCode(500, "An error occurred while retrieving statistics");
        }
    }
}
