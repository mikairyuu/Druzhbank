using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Xml;
using Dapper;
using Druzhbank.Entity;
using Druzhbank.Enums;
using Druzhbank.Models;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
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
                var url = "https://www.cbr.ru/scripts/XML_daily.asp";
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

        public async Task<List<ShortInstrumentEntity>> GetAllInstruments(String? token)
        {
            NpgsqlConnection connection = null;
            try
            {
                await using (connection = new NpgsqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    IEnumerable<ShortInstrumentEntity> ans = null;
                    ans = await connection.QueryAsync<ShortInstrumentEntity>(
                        @"with my_check as(
                        select * from ""Check"" where user_id = (select id from ""User"" where token = @token)
                        ),
                    check_ as (
                        select my_check.id,name,number,instrument_type from my_check
                        join ""OperationHistory"" on instrument_type =1
                    group by my_check.id,instrument_type,name,number),

                    my_cards as(
                        select * from ""Cards"" where user_id = (select id from ""User"" where token = @token) and is_blocked = 'false'
                        ),
                    cards as(
                        select my_cards.id,name,number,instrument_type from my_cards
                        join ""OperationHistory"" on instrument_type =0
                    group by my_cards.id,instrument_type,name,number)

                    select * from check_ union select * from cards",
                        new {@token = token});
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
                        @"select * from ""Cards"" where user_id = (select id from ""User"" where token = @token) and is_blocked = 'false'",
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
            String? instrument_number, int operationCount,
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
                            (@"(select * from ""OperationHistory"" where user_id = (select id from ""User"" where token = @token) 
                                   and instrument_id = (select id from ""Cards"" where number = @number) and instrument_type = @type)  order by id desc  limit @count",
                                new
                                {
                                    @token = token, @number = instrument_number, @type = instrument,
                                    @count = operationCount
                                });
                            break;
                        case Instrument.Check:
                            ans = await connection.QueryAsync<HistotyItemEntity>
                            (@"(select * from ""OperationHistory"" where user_id = (select id from ""User"" where token = @token) 
                                   and instrument_id = (select id from ""Check"" where number = @number) and instrument_type = @type) order by id desc limit @count",
                                new
                                {
                                    @token = token, @number = instrument_number, @type = instrument,
                                    @count = operationCount
                                });
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
                    (@"select * from (select * from ""OperationHistory"" order by id desc) as a where user_id = (select id from ""User"" where token = @token) limit @operationCount",
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


        public async Task<Result> ChangeInstrumentName(String? token, String? name, String? number, Instrument instrument)
        {
            NpgsqlConnection connection = null;
            try
            {
                await using (connection = new NpgsqlConnection(_connectionString))
                {
                    await connection.OpenAsync();

                    var ans = -1;
                    switch (instrument)
                    {
                        case Instrument.Card:
                            ans = await connection.ExecuteAsync
                            (@"update ""Cards"" set name = @name where number = @number and user_id = (select id from ""User"" where token = @token)",
                                new {@token = token, @number = number, @name = name});
                            break;
                        case Instrument.Check:
                            ans = await connection.ExecuteAsync
                            (@"update ""Check"" set name = @name where number = @number and user_id = (select id from ""User"" where token = @token)",
                                new {@token = token, @number = number, @name = name});
                            break;
                        case Instrument.Credit:
                            ans = await connection.ExecuteAsync
                            (@"update ""Credit"" set name = @name where number = @number and user_id = (select id from ""User"" where token = @token)",
                                new {@token = token, @number = number, @name = name});
                            break;
                    }
                    await connection.CloseAsync();
                    if (ans > 0)
                        return Result.Success;
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


        public async Task<Result> PayByCard(String? token, String? source, String? dest, double? sum,
            PayType instrument)
        {
            NpgsqlConnection connection = null;
            try
            {
                await using (connection = new NpgsqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    switch (instrument)
                    {
                        case PayType.onCard:
                            return await PayCard(token, source, dest, sum, true, connection);
                        case PayType.onCheck:
                            return await PayCheck(token, source, dest, sum, true, connection);
                        case PayType.onCategory:
                            return await PayCategory(token, source, dest, sum, true, connection);
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

        public async Task<Result> PayByCheck(String? token, String? source, String? dest, double? sum,
            PayType instrument)
        {
            NpgsqlConnection connection = null;
            try
            {
                await using (connection = new NpgsqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    switch (instrument)
                    {
                        case PayType.onCard:
                            return await PayCard(token, source, dest, sum, false, connection);
                        case PayType.onCheck:
                            return await PayCheck(token, source, dest, sum, false, connection);
                        case PayType.onCategory:
                            return await PayCategory(token, source, dest, sum, false, connection);
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

        public async Task<Result> PayCard(String? token, String? source, String? dest, double? sum,
            Boolean byCard, NpgsqlConnection connection)
        {
            try
            {
                var is_card_exist = await connection.QueryAsync<InstrumentEntity>(
                    @"select * from ""Cards"" where number = @dest and is_blocked = 'false'",
                    new {@dest = dest});
                if (is_card_exist.FirstOrDefault() != null)
                {
                    var check_cards = byCard
                        ? await connection.ExecuteAsync
                        (@"update ""Cards"" set count = count-@sum  where number = @source and user_id = (select id from ""User"" where token = @token) and count > @sum and is_blocked = 'false'",
                            new {@sum = sum, @source = source, token = @token})
                        : await connection.ExecuteAsync(
                            @"update ""Check"" set count = count-@sum  where number = @source and user_id = (select id from ""User"" where token = @token) and count > @sum and is_blocked = 'false'",
                            new {@sum = sum, @source = source, token = @token});
                    if (check_cards > 0)
                    {
                        var translationCheck = await connection.ExecuteAsync(
                            @"update ""Cards"" set count = count+@sum where number = @dest",
                            new {@sum = sum, @dest = dest});
                        if (translationCheck > 0)
                        {
                            if (byCard)
                                await connection.ExecuteAsync(
                                    @"insert into ""OperationHistory"" (type,date,count,user_id,instrument_id,instrument_type,dest,source) 
                                                 values (@type,@date,@sum,(select id from ""User"" where token = @token limit 1),
                                                (select id from ""Cards"" where number = @number limit 1),@instrumentType,
                                                   @dest,@source)",
                                    new
                                    {
                                        @token = token,
                                        @instrumentType = Instrument.Card,
                                        @number = source,
                                        @date = DateTime.Now,
                                        @sum = "-" + sum.ToString(),
                                        @dest = dest,
                                        @type = PayType.onCard,
                                        @source = source
                                    });
                            else
                                await connection.ExecuteAsync(
                                    @"insert into ""OperationHistory"" (type,date,count,user_id,instrument_id,instrument_type,dest,source) 
                                                 values (@type,@date,@sum,(select id from ""User"" where token = @token limit 1),
                                                (select id from ""Check"" where number = @number limit 1),@instrumentType,
                                                   @dest,@source)",
                                    new
                                    {
                                        @token = token,
                                        @instrumentType = Instrument.Check,
                                        @number = source,
                                        @date = DateTime.Now,
                                        @sum = "-" + sum.ToString(),
                                        @dest = dest,
                                        @type = PayType.onCard,
                                        @source = source
                                    });
                            await connection.ExecuteAsync(
                                @"insert into ""OperationHistory"" (type,date,count,user_id,instrument_id,instrument_type,dest,source) 
                                             values (@type,@date,@sum,@user_id,@card_id ,@instrumentType,@dest,@source)",
                                new
                                {
                                    @dest = source,
                                    @source = dest,
                                    @type = PayType.onCard,
                                    @user_id = is_card_exist.First().user_id,
                                    @instrumentType = byCard ? Instrument.Card : Instrument.Check,
                                    @card_id = is_card_exist.First().id,
                                    @date = DateTime.Now,
                                    @sum = "+" + sum.ToString()
                                });
                        }

                        return Result.Success;
                    }
                }

                return Result.Failure;
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

        public async Task<Result> PayCategory(String? token, String? source, String? dest_name, double? sum,
            Boolean byCard, NpgsqlConnection connection)
        {
            try
            {
                var pay_check = byCard
                    ? await connection.ExecuteAsync
                    (@"update ""Cards"" set count = count-@sum  where number = @source and user_id = (select id from ""User"" where token = @token) and count > @sum and is_blocked = 'false'",
                        new {@sum = sum, @source = source, token = @token})
                    : await connection.ExecuteAsync
                    (@"update ""Check"" set count = count-@sum  where number = @source and user_id = (select id from ""User"" where token = @token) and count > @sum and is_blocked = 'false'",
                        new {@sum = sum, @source = source, token = @token});
                if (pay_check > 0)
                {
                    if (byCard)
                        await connection.ExecuteAsync(
                            @"insert into ""OperationHistory"" (type,date,count,user_id,instrument_id,instrument_type,dest,source ) 
                                                 values (@type,
                                                   @date,
                                                   @sum,
                                                   (select id from ""User"" where token = @token limit 1),
                                                    (select id from ""Cards"" where number = @number limit 1),
                                                   @instrumentType,
                                                    @dest,@source)",
                            new
                            {
                                @dest = dest_name, @token = token,
                                @source = source,
                                @instrumentType = Instrument.Card,
                                @number = source,
                                @date = DateTime.Now,
                                @sum = "-" + sum.ToString(),
                                @type = PayType.onCategory
                            });
                    else
                        await connection.ExecuteAsync(
                            @"insert into ""OperationHistory"" (type,date,count,user_id,instrument_id,instrument_type,dest,source ) 
                                                 values (@type,
                                                   @date,
                                                   @sum,
                                                   (select id from ""User"" where token = @token limit 1),
                                                    (select id from ""Check"" where number = @number limit 1),
                                                   @instrumentType,
                                                    @dest,@source)",
                            new
                            {
                                @dest = dest_name, @token = token,
                                @source = source,
                                @instrumentType = Instrument.Check,
                                @number = source,
                                @date = DateTime.Now,
                                @sum = "-" + sum.ToString(),
                                @type = PayType.onCategory
                            });

                    return Result.Success;
                }

                return Result.Failure;
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

        public async Task<Result> PayCheck(String? token, String? source, String? dest, double? sum,
            Boolean byCard, NpgsqlConnection connection)
        {
            try
            {
                var is_check_exist = await connection.QueryAsync<InstrumentEntity>(
                    @"select * from ""Check"" where number = @dest",
                    new {@dest = dest});
                if (is_check_exist.FirstOrDefault() != null)
                {
                    var check_check = byCard
                        ? await connection.ExecuteAsync
                        (@"update ""Cards"" set count = count-@sum  where number = @source and user_id = (select id from ""User"" where token = @token) and count > @sum and is_blocked = 'false'",
                            new {@sum = sum, @source = source, token = @token})
                        : await connection.ExecuteAsync(
                            @"update ""Check"" set count = count-@sum  where number = @source and user_id = (select id from ""User"" where token = @token) and count > @sum and is_blocked = 'false'",
                            new {@sum = sum, @source = source, token = @token});
                    if (check_check > 0)
                    {
                        var translationCheck = await connection.ExecuteAsync(
                            @"update ""Check"" set count = count+@sum where number = @dest",
                            new {@sum = sum, @dest = dest});
                        if (translationCheck > 0)
                        {
                            if (byCard)
                                await connection.ExecuteAsync(
                                    @"insert into ""OperationHistory"" (type,date,count,user_id,instrument_id,instrument_type,dest,source ) 
                                                 values (@type,@date,@sum,(select id from ""User"" where token = @token limit 1),
                                                (select id from ""Cards"" where number = @number limit 1),@instrumentType,@dest,@source )",
                                    new
                                    {
                                        @type = PayType.onCheck,
                                        @token = token,
                                        @dest = dest,
                                        @source = source,
                                        @instrumentType = Instrument.Card,
                                        @number = source,
                                        @date = DateTime.Now,
                                        @sum = "-" + sum.ToString()
                                    });
                            else
                                await connection.ExecuteAsync(
                                    @"insert into ""OperationHistory"" (type,date,count,user_id,instrument_id,instrument_type,dest,source ) 
                                                 values (@type,@date,@sum,(select id from ""User"" where token = @token limit 1),
                                                (select id from ""Check"" where number = @number limit 1),@instrumentType,@dest,@source )",
                                    new
                                    {
                                        @type = PayType.onCheck,
                                        @token = token,
                                        @dest = dest,
                                        @source = source,
                                        @instrumentType = Instrument.Check,
                                        @number = source,
                                        @date = DateTime.Now,
                                        @sum = "-" + sum.ToString()
                                    });
                            await connection.ExecuteAsync(
                                @"insert into ""OperationHistory"" (type,date,count,user_id,instrument_id,instrument_type,dest,source) 
                                             values (@type,@date,@sum,@user_id,@card_id ,@instrumentType,@dest,@source)",
                                new
                                {
                                    @dest = source,
                                    @source = dest,
                                    @type = PayType.onCheck,
                                    @token = token,
                                    @user_id = is_check_exist.First().user_id,
                                    @instrumentType = byCard ? Instrument.Card : Instrument.Check,
                                    @card_id = is_check_exist.First().id,
                                    @date = DateTime.Now,
                                    @sum = "+" + sum.ToString()
                                });
                        }

                        return Result.Success;
                    }
                }

                return Result.Failure;
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
                item.type = instrument.type;
                item.instrument_type = instrument.instrument_type;
                item.count = instrument.count;
                item.dest = instrument.dest;
                item.source = instrument.source;
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
                var res =  await reader.ReadToEndAsync();
                var doc = new XmlDocument();
                doc.LoadXml(res);
                return JsonConvert.SerializeXmlNode(doc);
            }
        }
    }
}