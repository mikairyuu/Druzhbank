﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Dapper;
using Druzhbank.Entity;
using Druzhbank.Enums;
using Druzhbank.Models;
using Microsoft.Extensions.Configuration;
using Npgsql;

namespace Druzhbank.Services
{
    public class StuffService
    {
        private string _connectionString;

        public StuffService(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("MainDB");
            if (_connectionString == null) throw new Exception("Connection string not specified");
        }

        public async Task<List<BankomatModel>> GetAllBancomats()
        {
            NpgsqlConnection connection = null;
            try
            {
                await using (connection = new NpgsqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    var ans = await connection.QueryAsync<BankomatModel>(@"select * from ""ATM""");
                    await connection.CloseAsync();
                    return ans.ToList();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return null;
            }
            finally
            {
                connection?.CloseAsync();
            }
        }


        public async Task<String> GetCoefficient()
        {
            try
            {
                var url = "https://www.cbr-xml-daily.ru/latest.js";
                var answer = await GetValute(url);
                return answer;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return null;
            }
        }


        public async Task<List<String>> GetQuotes(DateTime date) // todo неплохо бы узнать как это работает
        {
            NpgsqlConnection connection = null;
            try
            {
                await using (connection = new NpgsqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    //var ans = await connection.QueryAsync<BankomatModel>("select * from [ATM]");
                    await connection.CloseAsync();
                    return null;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return null;
            }
            finally
            {
                connection?.CloseAsync();
            }
        }


        public async Task<List<CreditModel>> GetCredit(String? token)
        {
            NpgsqlConnection connection = null;
            try
            {
                await using (connection = new NpgsqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    IEnumerable<InstrumentEntity> ans = null;
                    ans = await connection.QueryAsync<InstrumentEntity>(
                        @"select * from ""Credit"" where user_id = (select id from ""User"" where token = @token)",
                        new {@token = token});
                    await connection.CloseAsync();
                    return ConvertCredit(ans.ToList());
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return null;
            }
            finally
            {
                connection?.CloseAsync();
            }
        }

        public async Task<List<CheckModel>> GetCheck(String? token)
        {
            NpgsqlConnection connection = null;
            try
            {
                await using (connection = new NpgsqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    IEnumerable<InstrumentEntity> ans = null;
                    ans = await connection.QueryAsync<InstrumentEntity>(
                        @"select * from ""Check"" where user_id = (select id from ""User"" where token = @token)",
                        new {@token = token});
                    await connection.CloseAsync();
                    return ConvertCheck(ans.ToList());
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return null;
            }
            finally
            {
                connection?.CloseAsync();
            }
        }


        public async Task<List<CardModel>> GetCard(String? token)
        {
            NpgsqlConnection connection = null;
            try
            {
                await using (connection = new NpgsqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    IEnumerable<InstrumentEntity> ans = null;
                    ans = await connection.QueryAsync<InstrumentEntity>(
                        @"select * from ""Cards"" where user_id = (select id from ""User"" where token = @token)",
                        new {@token = token});
                    await connection.CloseAsync();
                    return ConvertCard(ans.ToList());
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return null;
            }
            finally
            {
                connection?.CloseAsync();
            }
        }


        public async Task<List<HistotyItemEntity>> GetInstrumentHistory(String? token, String? instrument_number,
            Instrument instrument)
        {
            NpgsqlConnection connection = null;
            try
            {
                await using (connection = new NpgsqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    IEnumerable<HistotyItemEntity> ans = null;
                    switch (instrument)
                    {
                        case Instrument.Card:
                            ans = await connection.QueryAsync<HistotyItemEntity>
                            (@"select * from ""OperationHistory"" where user_id = (select id from ""User"" where token = @token) and instrument_id = (select id from ""Cards"" where number = @number) and instrument_type = @type",
                                new {@token = token, @number = instrument_number, @type = instrument});
                            break;
                        case Instrument.Check:
                            ans = await connection.QueryAsync<HistotyItemEntity>
                            (@"select * from ""OperationHistory"" where user_id = (select id from ""User"" where token = @token) and instrument_id = (select id from ""Check"" where number = @number) and instrument_type = @type",
                                new {@token = token, @number = instrument_number, @type = instrument});
                            break;
                    }

                    await connection.CloseAsync();
                    return ans.ToList();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return null;
            }
            finally
            {
                connection?.CloseAsync();
            }
        }


        public async Task<List<HistotyItemEntity>> GetAllInstrumentHistory(String? token, String? instrument_number,
            int? operationCount)
        {
            NpgsqlConnection connection = null;
            try
            {
                await using (connection = new NpgsqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    IEnumerable<HistotyItemEntity> ans = null;
                    ans = await connection.QueryAsync<HistotyItemEntity>
                    (@"select * from (select * from ""OperationHistory"" order by date desc) as a where user_id = (select id from ""User"" where token = @token) limit @operationCount",
                        new {@token = token, @number = instrument_number, @operationCount = operationCount});
                    await connection.CloseAsync();
                    return ans.ToList();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return null;
            }
            finally
            {
                connection?.CloseAsync();
            }
        }

        public async Task<Result> BlockCard(String? token, String? instrument_number)
        {
            NpgsqlConnection connection = null;
            try
            {
                await using (connection = new NpgsqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    await connection.ExecuteAsync
                    (@"update ""Cards"" set is_blocked = true where number = @number and user_id = (select id from ""User"" where token = @token)",
                        new {@token = token, @number = instrument_number});
                    await connection.CloseAsync();
                    return Result.Success;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return Result.Failure;
            }
            finally
            {
                connection?.CloseAsync();
            }
        }


        public async Task<Result> Translation(String? token, String? source, String? dest, double? sum,
            Instrument instrument)
        {
            NpgsqlConnection connection = null;
            try
            {
                await using (connection = new NpgsqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    switch (instrument)
                    {
                        // todo
                        case Instrument.Card:
                            var is_card_exist = await connection.QueryAsync<InstrumentEntity>(
                                @"select id from ""Cards"" where number = @dest",
                                new {@dest = dest});
                            if (is_card_exist.FirstOrDefault() != null)
                            {
                                var check_cards = await connection.ExecuteAsync
                                (@"update ""Cards"" set count = count-@sum  where number = @source and user_id = (select id from ""User"" where token = @token)",
                                    new {@sum = sum, @source = source, token = @token});
                                if (check_cards > 0)
                                {
                                    await connection.ExecuteAsync(
                                        @"update ""Cards"" set count = count+@sum where number = @dest",
                                        new {@sum = sum, @dest = dest});
                                    await connection.CloseAsync();
                                    return Result.Success;
                                }
                            }

                            break;
                        case Instrument.Check:
                            var is_check_exist = await connection.QueryAsync<InstrumentEntity>(
                                @"select id from ""Cards"" where number = @dest",
                                new {@dest = dest});
                            if (is_check_exist.FirstOrDefault() != null)
                            {
                                var check_check = await connection.ExecuteAsync
                                (@"update ""Cards"" set count = count-@sum  where number = @source and user_id = (select id from ""User"" where token = @token)",
                                    new {@sum = sum, @source = source, token = @token});
                                if (check_check > 0)
                                {
                                    await connection.ExecuteAsync(
                                        @"update ""Check"" set count = count+@sum where number = @dest",
                                        new {@sum = sum, @dest = dest});
                                    await connection.CloseAsync();
                                    return Result.Success;
                                }
                            }

                            break;
                    }

                    await connection.CloseAsync();
                    return Result.Failure;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return Result.Failure;
            }
            finally
            {
                connection?.CloseAsync();
            }
        }


        public async Task<List<CategoryEntity>> GetAllCategory()
        {
            NpgsqlConnection connection = null;
            try
            {
                await using (connection = new NpgsqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    var ans = await connection.QueryAsync<CategoryEntity>(@"select * from ""Category""");
                    await connection.CloseAsync();
                    return ans.ToList();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return null;
            }
            finally
            {
                connection?.CloseAsync();
            }
        }


        private List<CardModel> ConvertCard(List<InstrumentEntity> instruments)
        {
            var answer = new List<CardModel>();
            foreach (var instrument in instruments)
            {
                var card = new CardModel();
                card.id = instrument.id;
                card.expairy_date = instrument.expairy_date;
                card.hash_cvv = instrument.hash_cvv;
                card.is_blocked = instrument.is_blocked;
                card.count = instrument.count;
                card.name = instrument.name;
                card.number = instrument.number;
                answer.Add(card);
            }

            return answer;
        }

        private List<CheckModel> ConvertCheck(List<InstrumentEntity> instruments)
        {
            var answer = new List<CheckModel>();
            foreach (var instrument in instruments)
            {
                var check = new CheckModel();
                check.id = instrument.id;
                check.is_blocked = instrument.is_blocked;
                check.count = instrument.count;
                check.name = instrument.name;
                check.number = instrument.number;
                answer.Add(check);
            }

            return answer;
        }

        private List<CreditModel> ConvertCredit(List<InstrumentEntity> instruments)
        {
            var answer = new List<CreditModel>();
            foreach (var instrument in instruments)
            {
                var credit = new CreditModel();
                credit.id = instrument.id;
                credit.payment_date = instrument.payment_date;
                credit.count = instrument.count;
                credit.name = instrument.name;
                credit.number = instrument.number;
                answer.Add(credit);
            }

            return answer;
        }


        private async Task<string> GetValute(string uri)
        {
            HttpWebRequest request = (HttpWebRequest) WebRequest.Create(uri);
            request.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;

            using (HttpWebResponse response = (HttpWebResponse) await request.GetResponseAsync())
            using (Stream stream = response.GetResponseStream())
            using (StreamReader reader = new StreamReader(stream))
            {
                return await reader.ReadToEndAsync();
            }
        }
    }
}