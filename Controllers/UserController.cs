using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Druzhbank.Entity;
using Druzhbank.Enums;
using Druzhbank.Models;
using Microsoft.AspNetCore.Mvc;
using Druzhbank.Servises;

[ApiController]
public class UserController : ControllerBase
{
    [HttpPost("/login")]
    public async Task<UserModel> Login([Bind("User")] UserModel user)
    {
        var answer = await UserServises.Login(user.username, user.password);
        return answer;
    }
    
    
    [HttpPost("/getuser")]
    public async Task<UserModel> GetUser([Bind("User")] UserModel user)
    {
        var answer = await UserServises.GetUser(user.token);
        return answer;
    }

    
    [HttpDelete("/logout")]
    public async Task<Result> Logout([Bind("User")] UserModel user)
    {
        return await UserServises.Logout(user.token);
    }
    
    
    [HttpPut("/editepassword")] // todo  решить надо ли менять токен!!!!!
    public async Task<Result> EditePassword([Bind("User")] UserModel user)
    {
        return await UserServises.ChangePassword(user.token,user.password);
    }
    
    
    [HttpPut("/editeusername")]
    public async Task<Result> EditeUsername([Bind("User")] UserModel user)
    {
        return await UserServises.ChangePassword(user.token,user.username);
    }
    
    [HttpPost("/lastlogins")]
    public async Task<List<HistoryLoginEntity>> GetUserLoginHistory([Bind("User")] UserModel user)
    {
        var answer = await UserServises.GetLoginHistory(user.token);
        return answer;
    }
}