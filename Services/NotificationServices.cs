using FirebaseAdmin;
using FirebaseAdmin.Messaging;
using Google.Apis.Auth.OAuth2;

namespace Druzhbank.Services;

public class NotificationServices
{
    public static async Task Notify(List<string> tokens, string type, string sum)
    {
        try
        {
            var ans =  $"{type} на сумму {sum}р.";
            var message = new MulticastMessage
            {
                Tokens = tokens,
                Notification = new Notification
                {
                    Title = "Новый перевод!",
                    Body = ans,
                }
            };

            await FirebaseMessaging.DefaultInstance.SendMulticastAsync(message);
        }

        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }
}