using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NiRAProject.Data;
using NiRAProject.Dtos;
using NiRAProject.models;
using Microsoft.AspNetCore.Identity;

namespace NiRAProject.controllers
{

    [ApiController]
    [Route("/domain-mgt")]
    public class DomainControllers : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<RRRModel> _userManager;
        public DomainControllers(ApplicationDbContext context, UserManager<RRRModel> userManager)
        {
            _context = context;
            _userManager = userManager;
        }
        [HttpPost]
        [Route("create-domain-types")]
        [Authorize(Roles = "Registry")]
        public async Task<IActionResult> CreateDomainTypes([FromBody] DomainTypeDto domainTypeDto)
        {
            // Create domain type logic here
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            // Check if SuperDomain exists
            if (domainTypeDto.SuperDomain != 0)
            {
                var superDomain = await _context.Domains.FindAsync(domainTypeDto.SuperDomain);
                if (superDomain == null)
                {
                    return BadRequest("SuperDomain does not exist.");
                }
                var newDomainType = new domainType
                {
                    _domainSuffix = domainTypeDto._domainSuffix,
                    SuperDomain = domainTypeDto.SuperDomain
                };
                await _context.DomainTypes.AddAsync(newDomainType);
                await _context.SaveChangesAsync();
                return CreatedAtAction(nameof(GetDomainTypeById), new { id = newDomainType.Id }, newDomainType);
            }
            var rootDomainType = new domainType
            {
                _domainSuffix = domainTypeDto._domainSuffix,
                SuperDomain = 0
            };
            await _context.DomainTypes.AddAsync(rootDomainType);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetDomainTypeById), new { id = rootDomainType.Id }, rootDomainType);
        }

        // Get Domain type by ID
        [HttpGet]
        [Route("get-domain-types/{id:int}")]
        [Authorize(Roles = "Registry")]
        public async Task<IActionResult> GetDomainTypeById([FromRoute] int id)
        {
            var domainType = await _context.DomainTypes.FirstOrDefaultAsync(dt => dt.Id == id);
            if (domainType == null)
            {
                return NotFound("Domain type not found.");
            }
            return Ok(domainType);
        }

        [HttpDelete]
        [Route("delete-domain-type/{id:int}")]
        [Authorize(Roles = "Registry")]
        public async Task<IActionResult> DeleteDomainType([FromRoute] int id)
        {
            var domainType = await _context.DomainTypes.FindAsync(id);
            var hasChildDomains = await _context.Domains.AnyAsync(d => d.domainTypeId == id);
            if (hasChildDomains)
            {
                return Unauthorized("Cannot delete domain type with associated domains directly.\nTo delete this, delete all child domains first");
            }
            if (domainType == null)
            {
                return NotFound("Domain type not found.");
            }
            _context.DomainTypes.Remove(domainType);
            await _context.SaveChangesAsync();
            return Ok("Domain type deleted successfully.");
        }
        [HttpDelete]
        [Route("delete-domain/{id:int}")]
        [Authorize(Roles = "Registry")]
        public async Task<IActionResult> DeleteDomain([FromRoute] int id)
        {
            var domain = await _context.Domains.FindAsync(id);
            if (domain == null)
            {
                return NotFound("Domain not found.");
            }
            _context.Domains.Remove(domain);
            await _context.SaveChangesAsync();
            return Ok("Domain deleted successfully.");
        }
        [HttpPost]
        [Route("domain-key-request/{issuerId}")]
        [Authorize(Roles = "Registrant")]
        public async Task<IActionResult> RequestDomainKey([FromBody] DomainKeyRequestDto domainKeyRequestDto, [FromRoute] string issuerId)
        {
            var email = User.FindFirst(ClaimTypes.Email)?.Value;
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
            if (user == null)
            {
                return Unauthorized("User not found.");
            }
            var domain = await _context.DomainKeys.FirstOrDefaultAsync(d => d.OwnerId == user.Id);
            if (domain != null)
            {
                if (domain.Active)
                {
                    return StatusCode(200, "You already have an active domain key.");
                }
                // The person already has a domain so he just needs to request for reactivation
                domain.TTL = domain.TTL.AddDays(domainKeyRequestDto.TTL);
                await _context.SaveChangesAsync();
                return Ok("Domain key reactivation request successful.");
            }
            var newDomainKey = new DomainKey
            {
                IssuerId = issuerId,
                TTL = DateTime.UtcNow.AddDays(domainKeyRequestDto.TTL),
                OwnerId = user.Id,
                Active = false
            };
            await _context.DomainKeys.AddAsync(newDomainKey);
            await _context.SaveChangesAsync();
            return Ok(newDomainKey);
        }
        [HttpPost]
        [Route("domain-name-request/{typeId:int}")]
        [Authorize(Roles = "Registrant")]
        public async Task<IActionResult> RequestDomain([FromBody] DomainReqDto domainReq, [FromRoute] int typeId)
        {
            var email = User.FindFirst(ClaimTypes.Email)?.Value;
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
            if (user == null)
            {
                return Unauthorized("User not found.");
            }
            var domainType = await _context.DomainTypes.FindAsync(typeId);
            if (domainType == null) return NotFound("Domain type not found.");
            var domain = await _context.Domains.FirstOrDefaultAsync(d => d.domainName == domainReq.domainName);
            if (domain != null)
            {
                return StatusCode(200, "Domain name already exists.");
            }
            var domainKey = await _context.DomainKeys.FirstOrDefaultAsync(dk => dk.OwnerId == user.Id && dk.Active);
            if (domainKey == null) return Unauthorized("You do not have an active domain key, please request for one from your registrar.");
            var newDomain = new Domain
            {
                domainName = domainReq.domainName,
                domainTypeId = typeId,
                OwnerId = user.Id,
                Active = false
            };
            await _context.Domains.AddAsync(newDomain);
            await _context.SaveChangesAsync();
            return Ok(newDomain);
        }
        [HttpPut]
        [Route("activate-domain/{id:int}")]
        [Authorize(Roles = "Registry")]
        public async Task<IActionResult> ActivateDomain([FromRoute] int id)
        {
            var domain = await _context.Domains.FindAsync(id);
            if (domain == null)
            {
                return NotFound("Domain not found.");
            }
            if (domain.Active)
            {
                return BadRequest("Domain is already active.");
            }
            domain.Active = true;
            await _context.SaveChangesAsync();
            return Ok("Domain activated successfully.");
        }
        [HttpGet]
        [Route("your-domain-keys")]
        [Authorize(Roles = "Registrar")]
        public async Task<IActionResult> GetYourDomainKeys()
        {
            var domainKeys = _context.DomainKeys.AsQueryable();
            var email = User.FindFirst(ClaimTypes.Email)?.Value;
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                return Unauthorized("User not found.");
            }
            domainKeys = domainKeys.Where(dk => dk.IssuerId == user.Id);
            domainKeys = domainKeys.OrderByDescending(dk => dk.TTL);
            await domainKeys.ToListAsync();
            return Ok(domainKeys);
        }
        [HttpPut]
        [Route("all-domain-keys/{id:int}")]
        [Authorize(Roles = "Registrar")]
        public async Task<IActionResult> ApproveDomainKeys([FromRoute] int id)
        {
            var domainKey = await _context.DomainKeys.FindAsync(id);
            if (domainKey == null)
            {
                return NotFound("Domain key request not found.");
            }
            if (domainKey.Active)
            {
                return BadRequest("Domain key is already active.");
            }
            domainKey.Active = true;
            await _context.SaveChangesAsync();
            return Ok("Domain key approved and activated successfully.");
        }
        [HttpPost]
        [Route("domain-whois")]
        public async Task<IActionResult> WhoIsDomain ([FromBody] string URL)
        {
            var domains = URL.Split('.');
            var domainName = domains[0];
            var actualDomain = await _context.Domains.FirstOrDefaultAsync(d => d.domainName == domainName);
            var domainKey = await _context.DomainKeys.FirstOrDefaultAsync(dk => dk.OwnerId == actualDomain.OwnerId);
            if (domainKey.TTL < DateTime.UtcNow)
            {
                domainKey.Active = false;
                await _context.SaveChangesAsync();
                return BadRequest("Domain key has expired.");
            }
            bool isActive = (actualDomain != null && actualDomain.Active && domainKey.Active) ? true : false;
            if (isActive) 
            {
                var owner = await _context.Users.FirstOrDefaultAsync(u => u.Id == actualDomain.OwnerId);
                var domainType = await _context.DomainTypes.FirstOrDefaultAsync(dt => dt.Id == actualDomain.domainTypeId);
                if (domainType == null) return NotFound("Domain type is invalid.");
                return Ok(new {
                    DomainName = actualDomain.domainName,
                    DomainType = domainType._domainSuffix,
                    Owner = owner
                });
            }
            return BadRequest("Domain is not active or non existent.");
        }
    }
}