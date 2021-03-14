using System;

namespace Database.DbModels
{
    public class DbNote
    {
        public Guid Id { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
    }
}
