using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NiRAProject.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using NiRAProject.Tools;
using NiRAProject.Dtos;
using NiRAProject.models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;

namespace NiRAProject.controllers
{
    [ApiController]
    [Route("/client")]
    public class ClientController: ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<RRRModel> _userManager;
        private readonly SendEmails _emailSender;

        public ClientController(ApplicationDbContext context, UserManager<RRRModel> userManager, SendEmails emailSender)
        {
            _context = context;
            _userManager = userManager;
            _emailSender = emailSender;
        }

        [HttpGet]
        [Route("/all-domains")]
        [Authorize(Roles = "Registry")]
        public async Task<IActionResult> GetDomains()
        {
            var domains = await _context.Domains.ToListAsync();
            return Ok(domains);
        }
        [HttpPost]
        [Route("/register-application")]
        public async Task<IActionResult> ApplyToBeARegistrar([FromBody] RegApplyDto regApplyDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            await _context.RegistrarApplications.AddAsync(new RegistrarApplicationModel
            {
                UserName = regApplyDto.UserName,
                Email = regApplyDto.Email,
                Password = regApplyDto.Password
            });
            _context.SaveChanges();
            return Ok("Application submitted successfully.");
        }

        [HttpPost]
        [Route("/approve-registrar/{applicationId:int}")]
        [Authorize(Roles = "Registry")]
        public async Task<IActionResult> ApproveRegistrar([FromRoute] int applicationId)
        {
            var application = await _context.RegistrarApplications.FindAsync(applicationId);
            if (application == null)
            {
                return NotFound("Application not found.");  
            }

            // Create RRRModel user with Registrar role
            var registrarUser = new RRRModel
            {
                UserName = application.UserName,
                Email = application.Email,
                Managers = RRRTypes.Registrar
            };
            _context.Remove(application);
            await _context.SaveChangesAsync();
            var newAdmin = await _userManager.CreateAsync(registrarUser, application.Password);
            if (newAdmin.Succeeded) 
            {
                return Ok("Registrar account created successfully.");
            }
            return BadRequest(newAdmin.Errors);
        }
    }
}