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
    }
}
