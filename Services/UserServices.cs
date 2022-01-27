using System;
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


        public async Task<String> SignIn(String? name, String? username, String? password)
        {
            NpgsqlConnection connection = null;
            try
            {
                await using (connection = new NpgsqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    var hash_and_salt = GenerateHash(password);
                    var token = Guid.NewGuid();
                    var ans = await connection.ExecuteAsync(@"insert into ""User"" (name,username,hash,salt,token) " +
                                                            "values (@name,@username,@hash,@salt,@token)",
                        new
                        {
                            @name = name, @username = username, @hash = hash_and_salt.Key, @salt = hash_and_salt.Value,
                            @token = token.ToString()
                        });
                    await connection.CloseAsync();
                    return ans > 0 ? token.ToString() : null;
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


        public async Task<UserModel> Login(String? username, String? password) //todo check changes
        {
            NpgsqlConnection connection = null;
            try
            {
                await using (connection = new NpgsqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    var ans = await connection.QueryAsync<UserEntity>(
                        @"select * from ""User"" where username = @username",
                        new {@username = username});
                    var user = ans.FirstOrDefault();
                    if (user == null || GenerateHashFromSalt(password, user.salt) != user.hash)
                        user = null;

                    if (user != null)
                        await connection.ExecuteAsync(
                            @"insert into ""VisitHistory"" (date_visit,user_id) values (@date,@id)",
                            new {@date = DateTime.Now, @id = user.id});
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

        public async Task<List<TemplateEntity>> GetTemplate(String? token, String? number)
        {
            NpgsqlConnection connection = null;
            try
            {
                await using (connection = new NpgsqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    IEnumerable<TemplateEntity> ans = null;
                    ans = number == null
                        ? await connection.QueryAsync<TemplateEntity>(
                            @"select * from ""Templates"" where user_id = (select id from ""User"" where token = @token)",
                            new {@token = token})
                        : await connection.QueryAsync<TemplateEntity>(
                            @"select * from ""Templates"" where user_id = (select id from ""User"" where token = @token) and source = @number",
                            new {@token = token, @number = number});
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


        public async Task<Result> SetTemplate(String? token, String? source, String? dest, String? name, int? sum)
        {
            NpgsqlConnection connection = null;
            try
            {
                await using (connection = new NpgsqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    var ans = await connection.ExecuteAsync(
                        @"insert into ""Templates"" (source ,dest,sum,user_id,name) values (@source ,@dest,@sum,(select id from ""User"" where token = @token ),@name)",
                        new {@token = token, @source = source, @dest = dest, @sum = sum, @name = name});
                    await connection.CloseAsync();
                    return ans > 0 ? Result.Success : Result.Failure;
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


    


        public async Task<String?>
            ChangePassword(String? token, String? old_password, String? new_password) // check changes
        {
            NpgsqlConnection connection = null;
            try
            {
                await using (connection = new NpgsqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    var ans = await connection.QueryAsync<UserEntity>(@"select * from ""User"" where token = @token",
                        new {@token = token});
                    var user = ans.FirstOrDefault();
                    if (user == null)
                    {
                        await connection.CloseAsync();
                        return null;
                    }

                    /*if (GenerateHashFromSalt(old_password, user.salt) != user.hash ||
                        GenerateHashFromSalt(new_password, user.salt) == user.hash)
                    {
                        await connection.CloseAsync();
                        return null;
                    }*/

                    var hash = GenerateHash(new_password);
                    var new_token = token; // Guid.NewGuid();
                    await connection.ExecuteScalarAsync(
                        @"Update ""User"" set hash = @password, salt = @salt, token = @new_token where token = @token",
                        new
                        {
                            @password = hash.Key, @salt = hash.Value, @new_token = new_token.ToString(), @token = token
                        });
                    await connection.CloseAsync();
                    return new_token.ToString();
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


        public async Task<Result> ChangeUsername(String? token, String? name)
        {
            NpgsqlConnection connection = null;
            try
            {
                await using (connection = new NpgsqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    var ans = await connection.ExecuteAsync(
                        @"Update ""User"" set name = @name where token = @token and name != @name ",
                        new {@name = name, @token = token});
                    await connection.CloseAsync();
                    return ans > 0 ? Result.Success : Result.Failure;
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


        public async Task<List<HistoryLoginModel>> GetLoginHistory(String? token)
        {
            NpgsqlConnection connection = null;
            try
            {
                await using (connection = new NpgsqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    var ans = await connection.QueryAsync<HistoryLoginEntity>(
                        @"select * from ""VisitHistory"" where user_id = (select id from ""User"" where token = @token) order by id desc ",
                        new {@token = token});
                    await connection.CloseAsync();
                    return HistoryLoginConverter((ans.ToList()));
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
            ans.login = user.username;
            ans.token = user.token;
            return ans;
        }


        private static List<HistoryLoginModel> HistoryLoginConverter(List<HistoryLoginEntity> list)
        {
            var answer = new List<HistoryLoginModel>();
            if (list != null)
            {
                foreach (var loginItem in list)
                {
                    var item = new HistoryLoginModel();
                    item.date_visit = loginItem.date_visit.Value.ToUniversalTime()
                        .ToString("yyyy-MM-ddTHH\\:mm\\:ss.fffffffzzz");
                    item.id = loginItem.id;
                    answer.Add(item);
                }

                return answer;
            }

            return null;
        }
    }
}