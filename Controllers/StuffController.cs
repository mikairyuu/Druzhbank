using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Druzhbank.Entity;
using Druzhbank.Enums;
using Druzhbank.Models;
using Druzhbank.Servises;
using Microsoft.AspNetCore.Mvc;

namespace Druzhbank.Controllers
{
    [ApiController]
    public class StuffController : ControllerBase
    {
        [HttpGet("/bankomats")]
        public async Task<UserModel> GetBankomatsInfo()
        {
            var answer = await StuffServises.GetAllBancomats();
            return new UserModel();
        }
        
        [HttpGet("/quotes")]// todo разобраться с передаваемой датой
        public async Task<List<String>> GetQuotes([FromQuery(Name = "date")] DateTime date)
        {
            var ans = await StuffServises.GetQuotes(date);
            return ans;
        }

        [HttpGet("/valute")]
        public async Task<List<String>> GetCoefficients()
        {
            var ans = await StuffServises.GetCoefficient();
            return ans;
        }

        [HttpPost("/getcards")]
        public async Task<List<InstrumentEntity>> GetCards([Bind("User")] UserModel user)
        {
            var answer = await StuffServises.GetInstrumnent(user.token, Instrument.Card);
            return answer;
        }

        [HttpPost("/getcheck")]
        public async Task<List<InstrumentEntity>> GetCheck([Bind("User")] UserModel user)
        {
            var answer = await StuffServises.GetInstrumnent(user.token, Instrument.Check);
            return answer;
        }

        [HttpPost("/getcredits")]
        public async Task<List<InstrumentEntity>> GetCRedit([Bind("User")] UserModel user)
        {
            var answer = await StuffServises.GetInstrumnent(user.token, Instrument.Credit);
            return answer;
        }

        [HttpPost("/history/card")]
        public async Task<List<HistotyItemEntity>> GetHistoryCard([Bind("User")] UserInstrumentModel user)
        {
            var answer = await StuffServises.GetInstrumentHistory(user.token, user.card_number, Instrument.Credit);
            return answer;
        }

        [HttpPost("/history/check")]
        public async Task<List<HistotyItemEntity>> GetHistoryCheck([Bind("User")] UserInstrumentModel user)
        {
            var answer = await StuffServises.GetInstrumentHistory(user.token, user.check_number, Instrument.Credit);
            return answer;
        }

        [HttpPost("/block")]
        public async Task<Result> BlockCard([Bind("User")] UserInstrumentModel user)
        {
            var answer = await StuffServises.BlockCard(user.token, user.card_number);
            return answer;
        }
        
        
        [HttpPost("/refill")]
        public async Task<Result> Refill([Bind("User")] TranslationModel refill)
        {
            var answer = await StuffServises.Translation(refill.token, refill.sourse,refill.dest,refill.sum,Instrument.Card);
            return answer;
        }
        
        
        [HttpPost("/pay")]
        public async Task<Result> Pay([Bind("User")] TranslationModel refill)
        {
            var answer = await StuffServises.Translation(refill.token, refill.sourse,refill.dest,refill.sum,Instrument.Check);
            return answer;
        }
        
        
        [HttpPost("/category")]
        public async Task<List<CategoryEntity>> GetAllCategory()
        {
            var answer = await StuffServises.GetAllCategory();
            return answer;
        }
    }
}