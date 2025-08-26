using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using NiRAProject.models;
using NiRAProject.Dtos;
using NiRAProject.Tools;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using NiRAProject.Data;
using Microsoft.EntityFrameworkCore;

namespace NiRAProject.controllers
{
    [ApiController]
    [Route("managers")]
    public class AccountsController : ControllerBase
    {
        private readonly KeyOperations KeyOperations;
        private readonly SignInManager<RRRModel> _signInManager;
        private readonly UserManager<RRRModel> _userManager;
        private readonly TokenHelper _tokenHelper;
        private readonly ApplicationDbContext _context;
        public AccountsController(KeyOperations keyOperations, SignInManager<RRRModel> signInManager, UserManager<RRRModel> userManager, TokenHelper tokenHelper, ApplicationDbContext context)
        {
            KeyOperations = keyOperations;
            _signInManager = signInManager;
            _userManager = userManager;
            _tokenHelper = tokenHelper;
            _context = context;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] ManagerDto managerDto)
        {
            // Data Validation
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Enum validation
            if (!Enum.IsDefined(typeof(RRRTypes), managerDto.Role))
            {
                return BadRequest("Invalid role value.");
            }

            bool isReg = KeyOperations.ValidateKey(managerDto.Key);
            if (managerDto.Role == RRRTypes.Registry)
            {
                if (!isReg) return Unauthorized("You are not authorized to create a registry account, please provide a valid key.");
                var manager = new RRRModel
                {
                    UserName = managerDto.UserName,
                    Email = managerDto.Email,
                    Managers = RRRTypes.Registry
                };
                var newAdmin = await _userManager.CreateAsync(manager, managerDto.Password);
                if (newAdmin.Succeeded) return Ok("Registry account created successfully.");
                return BadRequest(newAdmin.Errors);
            }
            else if (managerDto.Role == RRRTypes.Registrant)
            {
                var manager = new RRRModel
                {
                    UserName = managerDto.UserName,
                    Email = managerDto.Email,
                    Managers = RRRTypes.Registrant
                };
                var newAdmin = await _userManager.CreateAsync(manager, managerDto.Password);
                if (newAdmin.Succeeded) return Ok("Registrant account created successfully.");
                return BadRequest(newAdmin.Errors);
            }
            else return BadRequest("You are not authorized to create a registrar account, apply through the client endpoint.");
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var user = await _userManager.FindByEmailAsync(loginDto.Email);
            if (user == null)
            {
                return Unauthorized("Invalid username or password.");
            }

            var result = await _signInManager.CheckPasswordSignInAsync(user, loginDto.Password, false);
            if (result.Succeeded)
            {
                var token = _tokenHelper.GenerateJwtToken(user);
                Response.Cookies.Append("AuthToken", token, new Microsoft.AspNetCore.Http.CookieOptions
                {
                    HttpOnly = true,
                    Secure = true,
                    SameSite = Microsoft.AspNetCore.Http.SameSiteMode.Strict,
                    Expires = DateTimeOffset.UtcNow.AddMinutes(60)
                });
                return Ok(new { message = "Login successful.", token });
            }
            return Unauthorized("Invalid username or password.");
        }
        [HttpGet("get-all-users")]
        [Authorize(Roles = "Registrar, Registrant")]
        public async Task<IActionResult> GetAllYourRegistrants([FromQuery] QueryParams queryParams)
        {
            var email = (User.FindFirst(ClaimTypes.Email).Value == null) ? string.Empty : User.FindFirst(ClaimTypes.Email).Value;
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                return Unauthorized("User not found.");
            }
            if (user.Managers == RRRTypes.Registrar && queryParams.role == 3)
            {
                // Get all your domain keys
                var domainKeys = await _context.DomainKeys.Where(dk => dk.IssuerId == user.Id && dk.Active).ToListAsync();
                List<RRRModel> registrants = new List<RRRModel>();
                foreach (var key in domainKeys)
                {
                    var registrant = await _userManager.Users.FirstOrDefaultAsync(u => u.Id == key.OwnerId && u.Managers == RRRTypes.Registrant);
                    if (registrant != null)
                        registrants.Add(registrant);
                }
                return Ok(registrants);
            }
            else if (user.Managers == RRRTypes.Registrant && queryParams.role == 2)
            {
                var registrars = await _userManager.Users.Where(u => u.Managers == RRRTypes.Registrar).ToListAsync();
                return Ok(registrars);
            }
            else
            {
                return BadRequest("Invalid role query parameter or insufficient permissions.");
            }
        }
        [HttpGet("logout")]
        public IActionResult Logout()
        {
            try
            {
                Response.Cookies.Delete("AuthToken");
                return Ok("Logout successful.");
            }
            catch (Exception)
            {
                return BadRequest("An error occurred during logout.");
            }
        }
    }
}