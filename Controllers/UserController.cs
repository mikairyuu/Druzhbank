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
    
    [HttpPost("/signin")]
    public async Task<String> SignIn([Bind("User")] UserModel response)
    {
        var answer = await _userService.SignIn(response.name,response.username, response.password);
        return answer;
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

    
    [HttpDelete("/logout")]// todo придумать нафиг это воообще надо (в этом запросе смысла как-то немного чот тип 0)
    public async Task<Result> Logout([Bind("User")] TokenResponse response)
    {
        return await _userService.Logout(response.token);
    }
    
    
    [HttpPut("/editepassword")] // todo  решить надо ли менять токен!!!!!
    public async Task<Result> EditePassword([Bind("User")] EditPasswordResponse response)
    {
        return await _userService.ChangePassword(response.token,response.password);
    }
    
    
    [HttpPut("/editeusername")]
    public async Task<Result> EditeUsername([Bind("User")] EditUsernameResponse response)
    {// todo можно выводить сообщение ошибки 
        return await _userService.ChangeUsername(response.token,response.username);
    }
    
    [HttpPost("/lastlogins")]
    public async Task<List<HistoryLoginEntity>> GetUserLoginHistory([Bind("User")] TokenResponse response)
    {
        var answer = await _userService.GetLoginHistory(response.token);
        return answer;
    }
}