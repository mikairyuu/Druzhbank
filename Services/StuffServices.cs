using System;
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


        public async Task<List<InstrumentHistoryItemModel>> GetInstrumentHistory(String? token,
            String? instrument_number,
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
                    return ConvertInstrumentHistory(ans.ToList());
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


        public async Task<List<InstrumentHistoryItemModel>> GetAllInstrumentHistory(String? token, int? operationCount)
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
                        new {@token = token, @operationCount = operationCount});

                    await connection.CloseAsync();
                    return ConvertInstrumentHistory(ans.ToList());
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
                        case Instrument.Card:
                            var is_card_exist = await connection.QueryAsync<InstrumentEntity>(
                                @"select * from ""Cards"" where number = @dest",
                                new {@dest = dest});
                            if (is_card_exist.FirstOrDefault() != null)
                            {
                                var check_cards = await connection.ExecuteAsync
                                (@"update ""Cards"" set count = count-@sum  where number = @source and user_id = (select id from ""User"" where token = @token) and count > @sum",
                                    new {@sum = sum, @source = source, token = @token});
                                if (check_cards > 0)
                                {
                                    var translationCheck = await connection.ExecuteAsync(
                                        @"update ""Cards"" set count = count+@sum where number = @dest",
                                        new {@sum = sum, @dest = dest});
                                    if (translationCheck > 0)
                                    {
                                        await connection.ExecuteAsync(
                                            @"insert into ""OperationHistory"" (type,date,count,user_id,instrument_id,instrument_type,dest) 
                                             values (@type,@date,@sum,(select id from ""User"" where token = @token limit 1),
                                            (select id from ""Cards"" where number = @number limit 1),@instrumentType,
                                               @dest)",
                                            new
                                            {
                                                @token = token, 
                                                @instrumentType = instrument, 
                                                @number = source,
                                                @date = DateTime.Today, 
                                                @sum = "-" + sum.ToString(),
                                                @dest = dest,
                                                @type = PayType.onCard
                                            });
                                        await connection.ExecuteAsync(
                                            @"insert into ""OperationHistory"" (type,date,count,user_id,instrument_id,instrument_type,dest) 
                                             values (@type,@date,@sum,@user_id,@card_id ,@instrumentType,@dest)",
                                            new
                                            {
                                                @dest = source,
                                                @type = PayType.onCard,
                                                @user_id = is_card_exist.First().user_id, 
                                                @instrumentType = instrument,
                                                @card_id = is_card_exist.First().id, 
                                                @date = DateTime.Today, 
                                                @sum = "+" + sum.ToString()
                                            });
                                    }

                                    await connection.CloseAsync();
                                    return Result.Success;
                                }
                            }

                            break;
                        case Instrument.Check:
                            var is_check_exist = await connection.QueryAsync<InstrumentEntity>(
                                @"select * from ""Check"" where number = @dest",
                                new {@dest = dest});
                            if (is_check_exist.FirstOrDefault() != null)
                            {
                                var check_check = await connection.ExecuteAsync
                                (@"update ""Cards"" set count = count-@sum  where number = @source and user_id = (select id from ""User"" where token = @token) and count > @sum",
                                    new {@sum = sum, @source = source, token = @token});
                                if (check_check > 0)
                                {
                                    var translationCheck = await connection.ExecuteAsync(
                                        @"update ""Check"" set count = count+@sum where number = @dest",
                                        new {@sum = sum, @dest = dest});
                                    if (translationCheck > 0)
                                    {
                                        await connection.ExecuteAsync(
                                            @"insert into ""OperationHistory"" (type,date,count,user_id,instrument_id,instrument_type,dest) 
                                             values (@type,@date,@sum,(select id from ""User"" where token = @token limit 1),
                                            (select id from ""Cards"" where number = @number limit 1),@instrumentType,@dest)",
                                            new
                                            {
                                                @type = PayType.onCheck,
                                                @token = token,
                                                @dest =dest,
                                                @instrumentType = Instrument.Card, 
                                                @number = source,
                                                @date = DateTime.Today, 
                                                @sum = "-" + sum.ToString()
                                            });
                                        await connection.ExecuteAsync(
                                            @"insert into ""OperationHistory"" (type,date,count,user_id,instrument_id,instrument_type,dest) 
                                             values (@type,@date,@sum,@user_id,@card_id ,@instrumentType,@dest)",
                                            new
                                            {
                                                @dest = source,
                                                @type = PayType.onCheck,
                                                @token = token,
                                                @user_id = is_check_exist.First().user_id, 
                                                @instrumentType = instrument,
                                                @card_id = is_check_exist.First().id,
                                                @date = DateTime.Today, 
                                                @sum = "+" + sum.ToString()
                                            });
                                    }

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


        public async Task<Result> PayCategory(String? token, String? source, int? dest_id, double? sum)
        {
            NpgsqlConnection connection = null;
            try
            {
                await using (connection = new NpgsqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    var pay_check = await connection.ExecuteAsync
                    (@"update ""Cards"" set count = count-@sum  where number = @source and user_id = (select id from ""User"" where token = @token) and count > @sum",
                        new {@sum = sum, @source = source, token = @token});
                    if (pay_check > 0)
                    {
                        await connection.ExecuteAsync(
                            @"insert into ""OperationHistory"" (type,date,count,user_id,instrument_id,instrument_type,dest) 
                                             values (@type,
                                               @date,
                                               @sum,
                                               (select id from ""User"" where token = @token limit 1),
                                                (select id from ""Cards"" where number = @number limit 1),
                                               @instrumentType,
                                                (select name from ""Category"" where id = @id limit 1))",
                            new
                            {
                                @id = dest_id, @token = token, 
                                @instrumentType = Instrument.Card, 
                                @number = source,
                                @date = DateTime.Today, 
                                @sum = "-" + sum.ToString(),
                                @type = PayType.onCategory
                            });
                        await connection.CloseAsync();
                        return Result.Success;
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


        private List<InstrumentHistoryItemModel> ConvertInstrumentHistory(List<HistotyItemEntity> items)
        {
            var answer = new List<InstrumentHistoryItemModel>();
            foreach (var instrument in items)
            {
                var item = new InstrumentHistoryItemModel();
                item.id = instrument.id;
                item.type = instrument.instrument_type;
                item.count = instrument.count;
                item.dest = instrument.dest;
                item.date = instrument.date.Value.ToUniversalTime().ToString("yyyy-MM-ddTHH\\:mm\\:ss.fffffffzzz");
                answer.Add(item);
            }

            return answer;
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