using Microsoft.Bot.Schema;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Teams_Bots.Models
{
    public class ConversationRefModel
    {
        public ConversationReference ConversationReference { get; set; }
        public string Data { get; set; } //TODO extebd this Data iinto custom model to fit all possible card fields
    }
}