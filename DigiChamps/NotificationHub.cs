﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.AspNet.SignalR;
using System.Threading.Tasks;

namespace DigiChamps
{
    public class NotificationHub : Hub
    {
        public override Task OnConnected()
        {
            signalConnectionId(this.Context.ConnectionId);
            return base.OnConnected();
        }

        private void signalConnectionId(string signalConnectionId)
        {
            Clients.Client(signalConnectionId).signalConnectionId(signalConnectionId);
        }
    }
}