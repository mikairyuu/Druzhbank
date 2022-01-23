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


        [HttpGet("/valute")]
        public async Task<String> GetCoefficients()
        {
            var ans = await _stuffService.GetCoefficient();
            return ans;
        }

        [HttpPost("/getcards")]
        public async Task<ResponseModel<List<CardModel>>> GetCards([Bind("User")] TokenResponse response)
        {
            var ans = await _stuffService.GetCard(response.token);
            var answer = new ResponseModel<List<CardModel>>();
            answer.success = false;
            if (ans != null)
            {
                answer.data = ans;
                answer.success = true;
            }

            return answer;
        }

        [HttpPost("/getcheck")]
        public async Task<ResponseModel<List<CheckModel>>> GetCheck([Bind("User")] TokenResponse response)
        {
            var ans = await _stuffService.GetCheck(response.token);
            var answer = new ResponseModel<List<CheckModel>>();
            answer.success = false;
            if (ans != null)
            {
                answer.data = ans;
                answer.success = true;
            }

            return answer;
        }

        [HttpPost("/getcredits")]
        public async Task<ResponseModel<List<CreditModel>>> GetCredit([Bind("User")] TokenResponse response)
        {
            var ans = await _stuffService.GetCredit(response.token);
            var answer = new ResponseModel<List<CreditModel>>();
            answer.success = false;
            if (ans != null)
            {
                answer.data = ans;
                answer.success = true;
            }

            return answer;
        }

        [HttpPost("/getallinstruments")]
        public async Task<ResponseModel<List<ShortInstrumentEntity>>> GetAllInstruments(
            [Bind("User")] TokenResponse response)
        {
            var ans = await _stuffService.GetAllInstruments(response.token);
            var answer = new ResponseModel<List<ShortInstrumentEntity>>();
            answer.success = false;
            if (ans != null)
            {
                answer.data = ans;
                answer.success = true;
            }

            return answer;
        }

        [HttpPost("/history/card")]
        public async Task<ResponseModel<PaginatedListModel<InstrumentHistoryItemModel>>> GetHistoryCard(
            [Bind("User")] TokenNumberResponse response)
        {
            var ans = await _stuffService.GetInstrumentHistory(response,Instrument.Card);
            var answer = new ResponseModel<PaginatedListModel<InstrumentHistoryItemModel>>();
            
            answer.success = false;
            if (ans != null)
            {
                answer.data = ans;
                answer.success = true;
            }

            return answer;
        }

        [HttpPost("/history/check")]
        public async Task<ResponseModel<PaginatedListModel<InstrumentHistoryItemModel>>> GetHistoryCheck(
            [Bind("User")] TokenNumberResponse response)
        {
            var ans = await _stuffService.GetInstrumentHistory(response, Instrument.Check);
            var answer = new ResponseModel<PaginatedListModel<InstrumentHistoryItemModel>>();
            answer.success = false;
            if (ans != null)
            {
                answer.data = ans;
                answer.success = true;
            }

            return answer;
        }

        [HttpPost("/history/all")]
        public async Task<ResponseModel<PaginatedListModel<InstrumentHistoryItemModel>>> GetHistoryAll(
            [Bind("User")] OperationResponce response)
        {
            var ans = await _stuffService.GetAllInstrumentHistory(response);
            var answer = new ResponseModel<PaginatedListModel<InstrumentHistoryItemModel>>();
            answer.success = false;
            if (ans != null)
            {
                answer.data = ans;
                answer.success = true;
            }

            return answer;
        }

        [HttpPost("/block")]
        public async Task<Result> BlockCard([Bind("User")] BlockCardResponse response)
        {
            var answer = await _stuffService.BlockCard(response.token, response.number, true);
            return answer;
        }

        [HttpPost("/unblock")]
        public async Task<Result> UnBlockCard([Bind("User")] BlockCardResponse response)
        {
            var answer = await _stuffService.BlockCard(response.token, response.number, false);
            return answer;
        }


        [HttpPost("/refill")]
        public async Task<Result> Refill([Bind("User")] TranslationModel refill)
        {
            switch (refill.payType)
            {
                case PayType.onCard:
                    return await _stuffService.PayByCard(refill.token, refill.source, refill.dest, refill.sum,
                        PayType.onCard);
                case PayType.onCheck:
                    return await _stuffService.PayByCard(refill.token, refill.source, refill.dest, refill.sum,
                        PayType.onCheck);
                case PayType.onCategory:
                    return await _stuffService.PayByCard(refill.token, refill.source, refill.dest, refill.sum,
                        PayType.onCategory);
                default:
                    return Result.Failure;
            }
        }


        [HttpPost("/pay")]
        public async Task<Result> Pay([Bind("User")] TranslationModel refill)
        {
            switch (refill.payType)
            {
                case PayType.onCard:
                    return await _stuffService.PayByCheck(refill.token, refill.source, refill.dest, refill.sum,
                        PayType.onCard);
                case PayType.onCheck:
                    return await _stuffService.PayByCheck(refill.token, refill.source, refill.dest, refill.sum,
                        PayType.onCheck);
                case PayType.onCategory:
                    return await _stuffService.PayByCheck(refill.token, refill.source, refill.dest, refill.sum,
                        PayType.onCategory);
                default:
                    return Result.Failure;
            }
        }


        [HttpPost("/edit/instrument/name")]
        public async Task<Result> PayCategory([Bind("User")] EditeInstrumentNameResponse response)
        {
            var answer = await _stuffService.ChangeInstrumentName(response.token, response.name, response.number,
                response.instrument);
            return answer;
        }


        [HttpPost("/category")]
        public async Task<ResponseModel<List<CategoryEntity>>> GetAllCategory()
        {
            var ans = await _stuffService.GetAllCategory();
            var answer = new ResponseModel<List<CategoryEntity>>();
            answer.success = false;
            if (ans != null)
            {
                answer.data = ans;
                answer.success = true;
            }

            return answer;
        }
    }
}