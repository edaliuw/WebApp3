using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace tourneybracket.Models
{
    
    public class Match
    {
        
        [Required]
        public int MatchID { get; set; }
        public int MatchNumber { get; set; }
        public int TeamAID { get; set; }
        public int TeamBID { get; set; }
        public Team TeamA { get; set; }
        public Team TeamB { get; set; }
        public string TeamAScore { get; set; }
        public string TeamBScore { get; set; }
        public Bracket Bracket { get; set; }
        public Round Round { get; set; }
        public string RoundType { get; set; }
    }
}
