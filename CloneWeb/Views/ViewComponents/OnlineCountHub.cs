using Microsoft.AspNetCore.SignalR;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace CloneWeb.Views.ViewComponents
{
    public class OnlineCountHub : Hub
    {
        private static int _count = 0;

        public override async Task OnConnectedAsync()
        {
            Interlocked.Increment(ref _count);
            await Clients.All.SendAsync("updateCount", _count);
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            Interlocked.Decrement(ref _count);
            await Clients.All.SendAsync("updateCount", _count);
            await base.OnDisconnectedAsync(exception);
        }
    }
}
