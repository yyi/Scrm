using System.Collections.Generic;
using Lombok.NET;

namespace WxClient.commons.command.request
{
    [ToString]
    public class UserLoginDirectlyCommand : Command
    {
        public List<string> LoginUserId { get; set; }
        public int ReserveCount { get; set; }
    }
}