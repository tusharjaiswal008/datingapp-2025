using System;
using System.Security.Cryptography;
using System.Text;
using API.Data;
using API.DTOs;
using API.Entities;
using API.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers;

public class AccountController:BaseApiController
{
private readonly AppDbContext _context;
private readonly ITokenService _tokenService;
public AccountController(AppDbContext context,ITokenService tokenService)
{
    _context = context;
    _tokenService = tokenService;
}

[HttpPost("register")]
public async Task<ActionResult<UserDto>> Register([FromBody]RegisterDto registerDto)
{
    if(await EmailExists(registerDto.Email))
    {
        return BadRequest("Email already exists");
    }
    using var hmac = new HMACSHA512();

    var user = new AppUser
    {
        Email = registerDto.Email,
        DisplayName = registerDto.DisplayName,
        PasswordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(registerDto.Password)),
        PasswordSalt = hmac.Key
    };

    _context.Users.Add(user);
    await _context.SaveChangesAsync(); 

     return new UserDto
    {
        Id = user.Id,
        DisplayName = user.DisplayName,
        Email = user.Email,
        Token = _tokenService.CreateToken(user)
    };     
}

[HttpPost("login")]
public async Task<ActionResult<UserDto>> Login(LoginDto loginDto)
{
    var user = await _context.Users.SingleOrDefaultAsync(x=>x.Email == loginDto.Email);

    if(user == null)
    {
        return Unauthorized("Invalid Email Address");
    }

    using var hmac = new HMACSHA512(user.PasswordSalt);
    var computedhash = hmac.ComputeHash(Encoding.UTF8.GetBytes(loginDto.Password));

    for(var i = 0; i<computedhash.Length;i++)
    {
        if(computedhash[i] != user.PasswordHash[i])
        {
             return Unauthorized("Invalid Password");   
        }    
    }

    return new UserDto
    {
        Id = user.Id,
        DisplayName = user.DisplayName,
        Email = user.Email,
        Token = _tokenService.CreateToken(user)
    };    
}

private async Task<bool> EmailExists(string email)
    {
        return await _context.Users.AnyAsync(x => x.Email.ToLower() == email.ToLower());
    }
}


