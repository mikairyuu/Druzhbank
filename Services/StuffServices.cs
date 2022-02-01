using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Dapper;
using Druzhbank.Entity;
using Druzhbank.Enums;
using Druzhbank.Models;
using Druzhbank.Responses;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Npgsql;

namespace Druzhbank.Services
{
    public class StuffService
    {
        private string _connectionString;
        private CacheProviderService _cacheService;

        public StuffService(IConfiguration configuration, CacheProviderService cacheService)
        {
            _connectionString = configuration.GetConnectionString("MainDB");
            if (_connectionString == null) throw new Exception("Connection string not specified");
            _cacheService = cacheService;
        }

        public async Task<List<BankomatModel>> GetAllBancomats()
        {
            NpgsqlConnection connection = null;
            try
            {
                await using (connection = new NpgsqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    var ans = await connection.QueryAsync<BankomatEntity>(@"select * from ""ATM""");
                    await connection.CloseAsync();
                    return BankomatConverter(ans.ToList());
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


        public async Task<List<CreditModel>> GetCredit(String? token, String? number)
        {
            NpgsqlConnection connection = null;
            try
            {
                await using (connection = new NpgsqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    IEnumerable<InstrumentEntity> ans = null;
                    ans = number == null
                        ? await connection.QueryAsync<InstrumentEntity>(
                            @"select * from ""Credit"" where user_id = (select id from ""User"" where token = @token)",
                            new {@token = token})
                        : await connection.QueryAsync<InstrumentEntity>(
                            @"select * from ""Credit"" where user_id = (select id from ""User"" where token = @token) and number = @number",
                            new {@token = token, @number = number});
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

        public async Task<List<CheckModel>> GetCheck(String? token, String? number)
        {
            NpgsqlConnection connection = null;
            try
            {
                await using (connection = new NpgsqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    IEnumerable<InstrumentEntity> ans = null;
                    ans = number == null
                        ? await connection.QueryAsync<InstrumentEntity>(
                            @"select * from ""Check"" where user_id = (select id from ""User"" where token = @token)",
                            new {@token = token})
                        : await connection.QueryAsync<InstrumentEntity>(
                            @"select * from ""Check"" where user_id = (select id from ""User"" where token = @token) and number = @number ",
                            new {@token = token, @number = number});
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


        public async Task<List<CardModel>> GetCard(String? token, String? number)
        {
            NpgsqlConnection connection = null;
            try
            {
                await using (connection = new NpgsqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    IEnumerable<InstrumentEntity> ans = null;
                    ans = number == null
                        ? await connection.QueryAsync<InstrumentEntity>(
                            @"select * from ""Cards"" where user_id = (select id from ""User"" where token = @token)",
                            new {@token = token})
                        : await connection.QueryAsync<InstrumentEntity>(
                            @"select * from ""Cards"" where user_id = (select id from ""User"" where token = @token) and number = @number ",
                            new {@token = token, @number = number});
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


        public async Task<PaginatedListModel<InstrumentHistoryItemModel>> GetInstrumentHistory(
            TokenNumberResponse response,
            Instrument instrument)
        {
            var token = response.token;
            var instrument_number = response.number;
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
                                   and instrument_id = (select id from ""Cards"" where number = @number) and instrument_type = @type)  order by id desc",
                                new
                                {
                                    @token = token, @number = instrument_number, @type = instrument,
                                });
                            break;
                        case Instrument.Check:
                            ans = await connection.QueryAsync<HistotyItemEntity>
                            (@"(select * from ""OperationHistory"" where user_id = (select id from ""User"" where token = @token) 
                                   and instrument_id = (select id from ""Check"" where number = @number) and instrument_type = @type) order by id desc",
                                new
                                {
                                    @token = token, @number = instrument_number, @type = instrument,
                                });
                            break;
                    }

                    await connection.CloseAsync();
                    var answer = PagedList<HistotyItemEntity>.ToPagedList(ans.ToList(), response.PageNumber,
                        response.PageSize);
                    var pagList = new PaginatedListModel<InstrumentHistoryItemModel>();
                    pagList.data = ConvertInstrumentHistory(answer.ToList());
                    pagList.currentPage = answer.CurrentPage;
                    pagList.isNext = answer.HasNext;
                    pagList.countPage = answer.TotalPages;
                    return pagList;
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


        public async Task<PaginatedListModel<InstrumentHistoryItemModel>> GetAllInstrumentHistory(
            OperationResponce responce)
        {
            NpgsqlConnection connection = null;
            try
            {
                var token = responce.token;
                await using (connection = new NpgsqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    IEnumerable<HistotyItemEntity> ans = null;
                    ans = await connection.QueryAsync<HistotyItemEntity>
                    (@"select * from (select * from ""OperationHistory"" order by id desc) as a where user_id = (select id from ""User"" where token = @token)",
                        new {@token = token});

                    await connection.CloseAsync();
                    var answer =
                        PagedList<HistotyItemEntity>.ToPagedList(ans.ToList(), responce.PageNumber, responce.PageSize);
                    var pagList = new PaginatedListModel<InstrumentHistoryItemModel>();
                    pagList.data = ConvertInstrumentHistory(answer.ToList());
                    pagList.currentPage = answer.CurrentPage;
                    pagList.isNext = answer.HasNext;
                    pagList.countPage = answer.TotalPages;
                    return pagList;
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


        public async Task<Result> ChangeInstrumentName(String? token, String? name, String? number,
            Instrument instrument)
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

        public async Task<Result> BlockCard(String? token, String? instrument_number, bool block)
        {
            NpgsqlConnection connection = null;
            try
            {
                await using (connection = new NpgsqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    await connection.ExecuteAsync
                    (@"update ""Cards"" set is_blocked = @isBlocked where number = @number and user_id = (select id from ""User"" where token = @token)",
                        new {@token = token, @number = instrument_number, isBlocked = block});
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


        public async Task<Result> PayByCard(String? token, String? source, String? dest, decimal? sum,
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

        public async Task<Result> PayByCheck(String? token, String? source, String? dest, decimal? sum,
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

        public async Task<Result> PayCard(String? token, String? source, String? dest, decimal? sum,
            Boolean byCard, NpgsqlConnection connection)
        {
            try
            {
                var transaction = await connection.QueryAsync<int>(
                    @"SELECT translationOnCard(@byCard,@source,@dest,@token,@sum) as result;",
                    new {@dest = dest, @sum = sum, @source = source, @token = token, @byCard = byCard});

                if (transaction.FirstOrDefault() == 1)
                {
                    var is_card_exist = await connection.QueryAsync<InstrumentEntity>(
                        @"select * from ""Cards"" where number = @dest and is_blocked = 'false'",
                        new {@dest = dest});
                    var new_sum = SumConverter(sum);
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
                                @sum = "-" + new_sum,
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
                                @sum = "-" + new_sum,
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
                            @instrumentType = Instrument.Card,
                            @card_id = is_card_exist.First().id,
                            @date = DateTime.Now,
                            @sum = "+" + new_sum
                        });

                    //Notificate(token, is_card_exist.First().user_id, connection, sum);

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

        public async Task<Result> PayCategory(String? token, String? source, String? dest_name, decimal? sum,
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
                    var new_sum = SumConverter(sum);
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
                                @sum = "-" + new_sum,
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
                                @sum = "-" + new_sum,
                                @type = PayType.onCategory
                            });

                    //Notificate(token, -1, connection, sum);
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

        public async Task<Result> PayCheck(String? token, String? source, String? dest, decimal? sum,
            Boolean byCard, NpgsqlConnection connection)
        {
            try
            {
                var transaction = await connection.QueryAsync<int>(
                    @"SELECT translationoncheck(@byCard,@source,@dest,@token,@sum) as result;",
                    new {@dest = dest, @sum = sum, @source = source, @token = token, @byCard = byCard});

                if (transaction.FirstOrDefault() == 1)
                {
                    var is_check_exist = await connection.QueryAsync<InstrumentEntity>(
                        @"select * from ""Check"" where number = @dest",
                        new {@dest = dest});
                    var new_sum = SumConverter(sum);
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
                                @sum = "-" + new_sum
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
                                @sum = "-" + new_sum
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
                            @instrumentType = Instrument.Check,
                            @card_id = is_check_exist.First().id,
                            @date = DateTime.Now,
                            @sum = "+" + new_sum
                        });

                    //Notificate(token, is_check_exist.First().user_id, connection, sum);
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


        private List<InstrumentHistoryItemModel> ConvertInstrumentHistory(List<HistotyItemEntity>? items)
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
                card.expairy_date = instrument.expairy_date.Value.ToUniversalTime()
                    .ToString("yyyy-MM-ddTHH\\:mm\\:ss.fffffffzzz");
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
                credit.payment_date = instrument.payment_date.Value.ToUniversalTime()
                    .ToString("yyyy-MM-ddTHH\\:mm\\:ss.fffffffzzz");
                credit.count = instrument.count;
                credit.name = instrument.name;
                credit.number = instrument.number;
                answer.Add(credit);
            }

            return answer;
        }


        private async Task<string> GetValute(string uri)
        {
            if (_cacheService.lastValuteData != null)
                if (_cacheService.lastValuteData.Value.Day == DateTime.Now.AddHours(2).Day) //-1h to update at 1 AM MSK
                    return _cacheService.cachedValute;
            var response = await (await new HttpClient().GetAsync(uri)).Content.ReadAsByteArrayAsync();
            var utf8 = Encoding.GetEncoding("UTF-8");
            var utf8Bytes = utf8.GetString(Encoding.Convert(Encoding.GetEncoding("windows-1251"), utf8, response));
            var doc = new XmlDocument();
            doc.LoadXml(utf8Bytes);
            var res = JsonConvert.SerializeXmlNode(doc);
            _cacheService.lastValuteData = DateTime.Now.AddHours(3); //+3h to be at MSK
            _cacheService.cachedValute = res;
            return res;
        }


        private List<BankomatModel> BankomatConverter(List<BankomatEntity> list)
        {
            var ans = new List<BankomatModel>();
            foreach (var item in list)
            {
                var bankomat = new BankomatModel();
                bankomat.id = item.id;
                bankomat.adress = item.adress;
                bankomat.coordinates = item.coordinates;
                bankomat.is_atm = item.is_atm;
                bankomat.is_working = item.is_working;
                bankomat.time_start = item.time_start.ToString();
                bankomat.time_end = item.time_end.ToString();
                ans.Add(bankomat);
            }

            return ans;
        }

        private String? SumConverter(decimal? sum)
        {
            if (sum != null)
            {
                var new_sum = Math.Floor((decimal) sum);
                var diff = sum - new_sum;
                if (diff > 0)
                {
                    if (diff.ToString().Length == 3)
                        return sum.ToString() + "0";
                    return sum.ToString();
                }

                return new_sum.ToString() + ".00";
            }

            return sum.ToString();
        }


        private async Task Notificate(String token, int? user_id, NpgsqlConnection connection,decimal? sum)
        {
            try
            {
                var tokens = await connection.QueryAsync<NotificationUserEntity>(
                    @"select * from ""NotificationTokens"" where user_id = @user_id or user_id = (select id from ""User"" where token = @token)",
                    new {@user_id = user_id, @token = token});
                var GetterTokens = new List<String>();
                var TranslationTokens = new List<String>();
                foreach (var item in tokens)
                {
                    if(item.user_id == user_id)
                        GetterTokens.Add(item.token);
                    else
                        TranslationTokens.Add(item.token);
                }

                await NotificationServices.ToNotificate(GetterTokens,"Пополнение",sum.ToString());
                await NotificationServices.ToNotificate(TranslationTokens,"Перевод",sum.ToString());
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }
    }
}