using Credit_Book_System.Interface.IService;
using Credit_Book_System.Models;
using Microsoft.AspNetCore.Mvc;

namespace Credit_Book_System.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CreditEntriesController : ControllerBase
    {
        private readonly ICreditBookService _creditEntryService;

        public CreditEntriesController(ICreditBookService creditEntryService)
        {
            _creditEntryService = creditEntryService;
        }

        [HttpGet("GetCreditEntries")]
        public async Task<IActionResult> GetCreditEntries()
        {
            try
            {
                var creditEntries = await _creditEntryService.GetCreditEntriesAsync();
                return Ok(creditEntries);
            }
            catch (Exception ex)
            {
                // Log the exception
                Console.WriteLine(ex.Message);
                return StatusCode(500, "An error occurred while fetching data.");
            }
        }

        [HttpPost("AddCreditEntry")]
        public async Task<IActionResult> AddCreditEntry([FromBody] CreditEntry entry)
        {
            try
            {
                if (entry == null || string.IsNullOrEmpty(entry.ItemName) || entry.Quantity <= 0 || entry.Rate <= 0)
                {
                    return BadRequest("Invalid credit entry data provided.");
                }

                await _creditEntryService.AddCreditEntryAsync(entry);
                return Ok(new { message = "Credit entry added successfully." });
            }
            catch (Exception ex)
            {
                // Log the exception
                Console.WriteLine(ex.Message);
                return StatusCode(500, "An error occurred while adding the credit entry.");
            }
        }

        [HttpGet("GetSettlements")]
        public async Task<IActionResult> GetSettlements()
        {
            try
            {
                var settlements = await _creditEntryService.GetSettlementsByCreditEntryIdAsync();
                return Ok(settlements);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return StatusCode(500, "An error occurred while fetching data.");
            }
        }
        [HttpGet("GetOutstandingBalance")]
        public async Task<IActionResult> GetOutstandingBalance()
        {
            try
            {
                var totalExpenses = await _creditEntryService.GetTotalExpensesAsync();
                var totalPaid = await _creditEntryService.GetTotalPaidAmountAsync();
                var remainingBalance = await _creditEntryService.GetRemainingBalanceAsync();

                return Ok(new { totalExpenses, totalPaid, remainingBalance });
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return StatusCode(500, "An error occurred while fetching balance.");
            }
        }

        [HttpPost("AddSettlement")]
        public async Task<IActionResult> AddSettlement([FromBody] Settlement settlement)
        {
            if (settlement == null || settlement.AmountPaid <= 0)
            {
                return BadRequest("Invalid settlement details.");
            }

            try
            {
                await _creditEntryService.AddSettlementAsync(settlement);
                return Ok(new { message = "Partial payment added successfully."});
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                if (ex.Message.Contains("No credit entries exist. Please add a credit entry before recording a settlement."))
                {
                    return StatusCode(400, ex.Message);
                }
                else if (ex.Message.Contains("All credit entries are fully paid. There is no outstanding balance to apply this settlement."))
                {
                    return StatusCode(400, ex.Message);
                }
                else if(ex.Message.Contains("Partial payment exceeds the total outstanding balance."))
                {
                    return StatusCode(400, ex.Message); // Bad Request for specific business logic errors
                }
                else
                {
                    return StatusCode(500, "An error occurred while adding the settlement."); // Internal Server Error for general issues
                }
            }

        }

    }
}
