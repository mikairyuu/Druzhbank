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
    public async Task<ResponseModel<String>> SignIn([Bind("User")] UserModelResponse response)
    {
        var ans = await _userService.SignIn(response.name, response.login, response.password);
        var answer = new ResponseModel<String>();
        answer.success = false;
        if (ans != null)
        {
            answer.data = ans;
            answer.success = true;
        }

        return answer;
    }

    [HttpPost("/login")]
    public async Task<ResponseModel<UserModel>> Login([Bind("User")] LoginResponse response)
    {
        var ans = await _userService.Login(response.login, response.password);
        var answer = new ResponseModel<UserModel>();
        answer.success = false;
        if (ans != null)
        {
            answer.data = ans;
            answer.success = true;
        }

        return answer;
    }


    [HttpPost("/getuser")]
    public async Task<ResponseModel<UserModel>> GetUser([Bind("User")] TokenResponse response)
    {
        var ans = await _userService.GetUser(response.token);
        var answer = new ResponseModel<UserModel>();
        answer.success = false;
        if (ans != null)
        {
            answer.data = ans;
            answer.success = true;
        }

        return answer;
    }


    /*
    [HttpDelete("/logout")]// todo придумать нафиг это воообще надо (в этом запросе смысла как-то немного чот тип 0)
    public async Task<Result> Logout([Bind("User")] TokenResponse response)
    {
        return await _userService.Logout(response.token);
    }*/


    [HttpPut("/editepassword")]
    public async Task<ResponseModel<String>?> EditePassword([Bind("User")] EditPasswordResponse response)
    {
        var ans = await _userService.ChangePassword(response.token, response.old_password, response.new_password);
        var answer = new ResponseModel<String>();
        answer.success = false;
        if (ans != null)
        {
            answer.data = ans;
            answer.success = true;
        }

        return answer;
    }


    [HttpPut("/editeusername")]
    public async Task<Result> EditeUsername([Bind("User")] EditUsernameResponse response)
    {
        return await _userService.ChangeUsername(response.token, response.name);
    }

    [HttpPost("/lastlogins")]
    public async Task<ResponseModel<List<HistoryLoginModel>>> GetUserLoginHistory([Bind("User")] TokenResponse response)
    {
        var ans = await _userService.GetLoginHistory(response.token);
        var answer = new ResponseModel<List<HistoryLoginModel>>();
        answer.success = false;
        if (ans.Count > 0)
        {
            answer.data = ans;
            answer.success = true;
        }

        return answer;
    }
}