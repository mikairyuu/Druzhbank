using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using Druzhbank.Entity;
using Druzhbank.Enums;
using Druzhbank.Models;
using Microsoft.Extensions.Configuration;
using Npgsql;

namespace Druzhbank.Servises
{
    public class StuffServises
    {
        private static string _connectionString;

        public StuffServises(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("MainDB");
            if (_connectionString == null) throw new Exception("Connection string not specified");
        }

        public static async Task<List<BankomatModel>> GetAllBancomats()
        {
            NpgsqlConnection connection = null;
            try
            {
                await using (connection = new NpgsqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    var ans = await connection.QueryAsync<BankomatModel>("select * from [ATM]");
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
        
        
        public static async Task<List<String>> GetCoefficient()// todo неплохо бы узнать как это работает
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
        
        
        public static async Task<List<String>> GetQuotes(DateTime date)// todo неплохо бы узнать как это работает
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



        public static async Task<List<InstrumentEntity>> GetInstrumnent(String? token, Instrument instrument)
        {
            NpgsqlConnection connection = null;
            try
            {
                await using (connection = new NpgsqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    IEnumerable<InstrumentEntity> ans = null;
                    switch (instrument)
                    {
                        // todo
                        case Instrument.Card:  ans = await connection.QueryAsync<InstrumentEntity>("select * from [ATM]");
                            break;
                        case Instrument.Check:  ans = await connection.QueryAsync<InstrumentEntity>("select * from [ATM]");
                            break;
                        case Instrument.Credit:  ans = await connection.QueryAsync<InstrumentEntity>("select * from [ATM]");
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
        
        
        public static async Task<List<HistotyItemEntity>> GetInstrumentHistory(String? token,String? instrument_number, Instrument instrument)
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
                        // todo
                        case Instrument.Card:  ans = await connection.QueryAsync<HistotyItemEntity>("select * from [ATM]");
                            break;
                        case Instrument.Check:  ans = await connection.QueryAsync<HistotyItemEntity>("select * from [ATM]");
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
        
        
        
        public static async Task<Result> BlockCard(String? token,String? instrument_number)
        {
            NpgsqlConnection connection = null;
            try
            {
                await using (connection = new NpgsqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                     var ans = await connection.QueryAsync<InstrumentEntity>("select * from [ATM]");
                    
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
        
        
        public static async Task<Result> Translation(String? token,String? source,String? dest,double? sum,Instrument instrument)
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
                        case Instrument.Card:  await connection.QueryAsync<HistotyItemEntity>("select * from [ATM]");
                            break;
                        case Instrument.Check:  await connection.QueryAsync<HistotyItemEntity>("select * from [ATM]");
                            break;
                    }
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
        
        
        
        public static async Task<List<CategoryEntity>> GetAllCategory()
        {
            NpgsqlConnection connection = null;
            try
            {
                await using (connection = new NpgsqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    var ans = await connection.QueryAsync<CategoryEntity>("select * from [ATM]");
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
    }
}