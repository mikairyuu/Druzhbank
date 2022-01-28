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


    [HttpPost("/get/templates")]
    public async Task<ResponseModel<List<TemplateEntity>>> GetTemplate([Bind("User")] GetTemplateResponse response)
    {
        var ans = await _userService.GetTemplate(response.token, response.number);
        var answer = new ResponseModel<List<TemplateEntity>>();
        answer.success = false;
        if (ans != null)
        {
            answer.data = ans;
            answer.success = true;
        }

        return answer;
    }

    [HttpPost("/set/templates")]
    public async Task<Result> SetTemplate([Bind("User")] SetTemplateResponse response)
    {
        var ans = await _userService.SetTemplate(response.token, response.source, response.dest, response.name,
            response.sum, response.source_type, response.dest_type);
        return ans;
    }

    [HttpDelete("/delete/templates")]
    public async Task<Result> DeleteTemplate([Bind("User")] DeleteTemplateResponse response)
    {
        var ans = await _userService.DeleteTemplate(response.token, response.id);
        return ans;
    }


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