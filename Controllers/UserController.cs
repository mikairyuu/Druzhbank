using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Druzhbank.Entity;
using Druzhbank.Enums;
using Druzhbank.Models;
using Druzhbank.Responses;
using Microsoft.AspNetCore.Mvc;
using Druzhbank.Services;

[ApiController]
public class UserController : ControllerBase
{
    private UserService _userService;

    public UserController(UserService userService)
    {
        _userService = userService;
    }
    
    [HttpPost("/login")]
    public async Task<UserModel> Login([Bind("User")] LoginResponse response)
    {
        var answer = await _userService.Login(response.username, response.password);
        return answer;
    }
    
    
    [HttpPost("/getuser")]
    public async Task<UserModel> GetUser([Bind("User")] TokenResponse response)
    {
        var answer = await _userService.GetUser(response.token);
        return answer;
    }

    
    [HttpDelete("/logout")]
    public async Task<Result> Logout([Bind("User")] TokenResponse response)
    {
        return await _userService.Logout(response.token);
    }
    
    
    [HttpPut("/editepassword")] // todo  решить надо ли менять токен!!!!!
    public async Task<Result> EditePassword([Bind("User")] UserModel user)
    {
        return await _userService.ChangePassword(user.token,user.password);
    }
    
    
    [HttpPut("/editeusername")]
    public async Task<Result> EditeUsername([Bind("User")] UserModel user)
    {
        return await _userService.ChangePassword(user.token,user.username);
    }
    
    [HttpPost("/lastlogins")]
    public async Task<List<HistoryLoginEntity>> GetUserLoginHistory([Bind("User")] TokenResponse response)
    {
        var answer = await _userService.GetLoginHistory(response.token);
        return answer;
    }
}