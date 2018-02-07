using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.Server.Models
{
    public enum RequestType
    {
        Friends
    }

    public class Request
    {
        public RequestType ReqType;
    }
}
