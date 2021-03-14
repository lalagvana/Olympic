using System;

namespace Notes.Models
{
    public class NoteResponse
    {
        public Guid Id { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
    }
}
