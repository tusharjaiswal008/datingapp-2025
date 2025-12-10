using Microsoft.AspNetCore.Mvc;
using API.Data;
using API.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;


namespace API.Controllers
{

    public class MembersController : BaseApiController
    {
        private readonly AppDbContext _context;
        public MembersController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]  //localhost:5000/api/members
        public async Task <ActionResult<IReadOnlyList<AppUser>>> GetMembers()
        {
            var members = await _context.Users.ToListAsync();
            return members;
        }

        [Authorize]
        [HttpGet("{id}")] //localhost:5000/api/members/bob-id
        public async Task <ActionResult<AppUser>> GetMember(string id)
        {
            var member = await _context.Users.FindAsync(id);
            if(member == null)
            {
                return NotFound();
            }
            return member;
        }
    }
}
