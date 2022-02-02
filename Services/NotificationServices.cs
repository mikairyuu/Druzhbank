﻿using FirebaseAdmin;
using FirebaseAdmin.Messaging;
using Google.Apis.Auth.OAuth2;

namespace Druzhbank.Services;

public class NotificationServices
{
    public NotificationServices()
    {
        FirebaseApp.Create(new AppOptions()
        {
            Credential = GoogleCredential.GetApplicationDefault()
        });
    }

    public static async Task ToNotificate(List<String> tokens, String type, String sum)
    {
        var ans = type + " на сумму " + sum;
        var message = new MulticastMessage()
        {
            Tokens = tokens,
            Notification = new Notification()
            {
                Title = "Druzhbank Translation",
                Body = ans,
            }
        };

        var response = await FirebaseMessaging.DefaultInstance.SendMulticastAsync(message);
    }
}