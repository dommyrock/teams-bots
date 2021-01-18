using System;

namespace Teams_Bots.Models
{
    public class CardObject
    {
        public DateTime DueDate { get; set; }
        public string Comment { get; set; }
        public string Card_Id { get; set; }
        public Guid ProcesOdobravanja_Id { get; set; }
    }
}