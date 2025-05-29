using Azure.Core;
using BOs.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using School_TV_Show.DTO;
using School_TV_Show.Helpers;
using Services;

namespace School_TV_Show.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProgramFollowController : ControllerBase
    {
        private readonly IProgramFollowService _programFollowService;
        private readonly IProgramService _programService;

        public ProgramFollowController(IProgramFollowService programFollowService, IProgramService programService)
        {
            _programFollowService = programFollowService;
            _programService = programService;
        }

        [HttpGet]
        public async Task<ActionResult<List<ProgramFollow>>> GetAll()
        {
            return Ok(await _programFollowService.GetAllAsync());
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ProgramFollow>> GetById(int id)
        {
            var follow = await _programFollowService.GetByIdAsync(id);
            return follow != null ? Ok(follow) : NotFound();
        }

        [HttpGet("account/{accountId}")]
        public async Task<ActionResult<List<ProgramFollow>>> GetByAccountId(int accountId)
        {
            return Ok(await _programFollowService.GetByAccountIdAsync(accountId));
        }

        [HttpPost("follow")]
        public async Task<IActionResult> Follow([FromBody] CreateProgramFollowRequest request)
        {
            if(await _programService.IsOwner(request.AccountID, request.ProgramID))
            {
                return BadRequest("You cannot follow yourself");
            }

            var result = await _programFollowService.CreateOrRefollowAsync(request.AccountID, request.ProgramID);
            return Ok(result);
        }

        [HttpPut("status")]
        public async Task<IActionResult> UpdateFollowStatus([FromBody] UpdateProgramFollowStatusRequest request)
        {
            var result = await _programFollowService.UpdateFollowStatusAsync(request.ProgramFollowID, request.Status);
            return Ok(result);
        }

        [HttpPost("add")]
        public async Task<ActionResult> Add([FromBody] ProgramFollow programFollow)
        {
            var result = await _programFollowService.AddAsync(programFollow);
            return result ? Ok() : BadRequest();
        }

        [HttpPut("update")]
        public async Task<ActionResult> Update([FromBody] ProgramFollow programFollow)
        {
            var result = await _programFollowService.UpdateAsync(programFollow);
            return result ? Ok() : NotFound();
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(int id)
        {
            var result = await _programFollowService.DeleteAsync(id);
            return result ? Ok() : NotFound();
        }

        [HttpGet("count/{programId}")]
        public async Task<ActionResult<int>> CountByProgram(int programId)
        {
            return Ok(await _programFollowService.CountByProgramAsync(programId));
        }
    }
}
