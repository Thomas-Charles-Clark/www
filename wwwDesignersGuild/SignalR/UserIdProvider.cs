using Microsoft.AspNet.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace wwwDesignersGuild.SignalR
{
    public class UserIdProvider : IUserIdProvider
    {
        string IUserIdProvider.GetUserId(IRequest request)
        {
            return request.Cookies["_dgVisitIdent"].Value;
        }
    }
}