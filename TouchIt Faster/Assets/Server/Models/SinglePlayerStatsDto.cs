using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.Server.Models
{
    public class OnLoginDto
    {
        public int SinglePlayerHighestScore { get; set; }
        public int MaxHitsInRowSinglePlayer { get; set; }
        public string Email { get; set; }
        public string Username { get; set; }
    }
}
