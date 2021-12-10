﻿using System;
using System.Collections.Generic;
using Druzhbank.Models;
using Npgsql;
using Dapper;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Security.Cryptography;
using Druzhbank.Entity;
using Druzhbank.Enums;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Microsoft.Extensions.Configuration;

namespace Druzhbank.Services
{
    public class UserService
    {
        private string _connectionString;

        public UserService(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("MainDB");
            if (_connectionString == null) throw new Exception("Connection string not specified");
        }


        public async Task<UserModel> Login(String? username, String? password)
        {
            NpgsqlConnection connection = null;
            try
            {
                await using (connection = new NpgsqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    var ans = await connection.QueryAsync<UserEntity>(@"select * from ""User"" where username = @username",
                        new {@username = username});
                    var user = ans.FirstOrDefault();
                    if (GenerateHashFromSalt(password, user.salt) != user.hash)
                        user = null;
                    await connection.CloseAsync();
                    return UserConventer(user);
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


        public async Task<UserModel> GetUser(String? token)
        {
            NpgsqlConnection connection = null;
            try
            {
                await using (connection = new NpgsqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    var ans = await connection.QueryAsync<UserEntity>(@"select * from ""User"" where token = @token",
                        new {@token = token});
                    await connection.CloseAsync();
                    return UserConventer(ans.FirstOrDefault());
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

        public async Task<Result> Logout(String? token)
        {
            NpgsqlConnection connection = null;
            try
            {
                await using (connection = new NpgsqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    var ans = await connection.ExecuteScalarAsync<UserModel>(
                        @"select * from ""User"" where token = @token",
                        new {@token = token});
                    await connection.CloseAsync();
                    return ans != null ? Result.Success : Result.Failure;
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


        public async Task<Result> ChangePassword(String? token, String? password)
        {
            NpgsqlConnection connection = null;
            try
            {
                await using (connection = new NpgsqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    var answer = await connection.QueryAsync<UserModel>(
                        // todo 
                        @"select * from ""User"" where token = @token",
                        new {@token = token});
                    var user = answer.FirstOrDefault();
                    password = GenerateHashFromSalt(password, user.salt);
                    if (password != user.password)
                        await connection.ExecuteScalarAsync(
                            "Update [User] set hash = @password where token = @token",
                            new {@password = password, @token = token});
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


        public async Task<Result> ChangeUsername(String? token, String? username)
        {
            NpgsqlConnection connection = null;
            try
            {
                await using (connection = new NpgsqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    var ans = await connection.ExecuteScalarAsync<UserModel>(
                        // todo 
                        @"select * from ""User"" where token = @token",
                        new {@token = token});
                    await connection.CloseAsync();
                    return ans != null ? Result.Success : Result.Failure;
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
        
        
        public async Task<List<HistoryLoginEntity>> GetLoginHistory(String? token)
        {
            NpgsqlConnection connection = null;
            try
            {
                await using (connection = new NpgsqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    var ans = await connection.QueryAsync<HistoryLoginEntity>(
                        // todo 
                        @"select * from ""VisitHistory"" where user_id = (select id from ""User"" where token = @token)",
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


        private KeyValuePair<string, string> GenerateHash(string s)
        {
            var salt = new byte[128 / 8];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(salt);
            }

            var stringSalt = Convert.ToBase64String(salt);
            var hash = Convert.ToBase64String(KeyDerivation.Pbkdf2(s, salt, KeyDerivationPrf.HMACSHA1, 1000, 256 / 8));
            return new KeyValuePair<string, string>(hash, stringSalt);
        }

        private static string GenerateHashFromSalt(string s, string strSalt) => Convert.ToBase64String(
            KeyDerivation.Pbkdf2(s, Convert.FromBase64String(strSalt), KeyDerivationPrf.HMACSHA1, 1000, 256 / 8));



        private static UserModel UserConventer(UserEntity? user)
        {
            if (user == null) return null;
            var ans = new UserModel();
            ans.name = user.name;
            ans.username = user.username;
            ans.token = user.token;
            return ans;
        }
    }
}