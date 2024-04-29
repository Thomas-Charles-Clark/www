using Microsoft.AspNet.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace wwwDesignersGuild.SignalR
{
    public class MoodboardHub : Hub
    {
        public void SendTwitterAuth(string notifyUserId, string oauth_token, string oauth_token_secret)
        {
            Clients.User(notifyUserId).receiveTwitterOAuth(new
            {
                oauth_token,
                oauth_token_secret
            });
        }
    }
}