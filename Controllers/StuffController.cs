using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Druzhbank.Entity;
using Druzhbank.Enums;
using Druzhbank.Models;
using Druzhbank.Responses;
using Druzhbank.Services;
using Microsoft.AspNetCore.Mvc;

namespace Druzhbank.Controllers
{
    
    
    [ApiController]
    public class StuffController : ControllerBase
    {
        
        private StuffService _stuffService;

        public StuffController(StuffService stuffService)
        {
            _stuffService = stuffService;
        }
        
        [HttpGet("/bankomats")]
        public async Task<List<BankomatModel>> GetBankomatsInfo()
        {
            var answer = await _stuffService.GetAllBancomats();
            return answer;
        }
        
        [HttpGet("/quotes")]// todo разобраться с передаваемой датой
        public async Task<List<String>> GetQuotes([FromQuery(Name = "date")] DateTime date)
        {
            var ans = await _stuffService.GetQuotes(date);
            return ans;
        }

        [HttpGet("/valute")]
        public async Task<String> GetCoefficients()
        {
            var ans = await _stuffService.GetCoefficient();
            return ans;
        }

        [HttpPost("/getcards")]
        public async Task<List<CardModel>> GetCards([Bind("User")] TokenResponse response)
        {
            var answer = await _stuffService.GetCard(response.token);
            return answer;
        }

        [HttpPost("/getcheck")]
        public async Task<List<CheckModel>> GetCheck([Bind("User")] TokenResponse response)
        {
            var answer = await _stuffService.GetCheck(response.token);
            return answer;
        }

        [HttpPost("/getcredits")]
        public async Task<List<CreditModel>> GetCredit([Bind("User")] TokenResponse response)
        {
            var answer = await _stuffService.GetCredit(response.token);
            return answer;
        }

        [HttpPost("/history/card")]
        public async Task<List<HistotyItemEntity>> GetHistoryCard([Bind("User")] UserInstrumentModel user)
        {
            var answer = await _stuffService.GetInstrumentHistory(user.token, user.card_number, Instrument.Credit);
            return answer;
        }

        [HttpPost("/history/check")]
        public async Task<List<HistotyItemEntity>> GetHistoryCheck([Bind("User")] UserInstrumentModel user)
        {
            var answer = await _stuffService.GetInstrumentHistory(user.token, user.check_number, Instrument.Credit);
            return answer;
        }

        [HttpPost("/block")]
        public async Task<Result> BlockCard([Bind("User")] UserInstrumentModel user)
        {
            var answer = await _stuffService.BlockCard(user.token, user.card_number);
            return answer;
        }
        
        
        [HttpPost("/refill")]
        public async Task<Result> Refill([Bind("User")] TranslationModel refill)
        {
            var answer = await _stuffService.Translation(refill.token, refill.sourse,refill.dest,refill.sum,Instrument.Card);
            return answer;
        }
        
        
        [HttpPost("/pay")]
        public async Task<Result> Pay([Bind("User")] TranslationModel refill)
        {
            var answer = await _stuffService.Translation(refill.token, refill.sourse,refill.dest,refill.sum,Instrument.Check);
            return answer;
        }
        
        
        [HttpPost("/category")]
        public async Task<List<CategoryEntity>> GetAllCategory()
        {
            var answer = await _stuffService.GetAllCategory();
            return answer;
        }
    }
}