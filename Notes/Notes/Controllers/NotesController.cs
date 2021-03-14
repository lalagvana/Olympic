using AutoMapper;
using Database.DbModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Notes.Database;
using Notes.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Notes.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class NotesController : ControllerBase
    {
        private readonly ILogger<NotesController> _logger;
        private readonly NotesDbContext _dbContext;
        private readonly Config _config;

        public NotesController(IOptions<Config> config,
            ILogger<NotesController> logger,
            NotesDbContext dbContext)
        {
            _logger = logger;
            _dbContext = dbContext;
            _config = config.Value;
        }

        [HttpPost]
        public NoteResponse CreateNote(
            [FromBody] NoteRequest request)
        {
            if (string.IsNullOrEmpty(request.Content))
            {
                throw new Exception("Content is required");
            }

            var dbNote = new DbNote
            {
                Id = Guid.NewGuid(),
                Title = request.Title,
                Content = request.Content
            };
            _dbContext.Add(dbNote);
            _dbContext.SaveChanges();

            var mapperConfig = new MapperConfiguration(cfg => cfg.CreateMap<DbNote, NoteResponse>());
            var mapper = new Mapper(mapperConfig);
            return mapper.Map<NoteResponse>(dbNote);
        }


        [HttpGet]
        public List<NoteResponse> GetNotesList()
        {
            var mapperConfig = new MapperConfiguration(cfg => cfg.CreateMap<DbNote, NoteResponse>());
            var mapper = new Mapper(mapperConfig);
            var dbNotesList = _dbContext.Notes;
            var notesList = new List<NoteResponse>();
            dbNotesList.ToList().ForEach(n => notesList.Add(mapper.Map<NoteResponse>(n)));

            foreach (var note in notesList)
            {
                note.Title = string.IsNullOrEmpty(note.Title) ? note.Content.Substring(0, _config.NumberOfSymbols - 1) : note.Title;
            }

            return notesList;
        }


        [HttpGet("{id}")]
        public NoteResponse GetNoteById([FromRoute] Guid id)
        {
            var dbNote = _dbContext.Notes.FirstOrDefault(n => n.Id == id);
            if (dbNote == null)
            {
                throw new Exception("There is no note with such Id");
            }
            var note = new NoteResponse()
            {
                Id = id,
                Title = string.IsNullOrEmpty(dbNote.Title) ? dbNote.Content.Substring(0, _config.NumberOfSymbols) : dbNote.Title,
                Content = dbNote.Content
            };

            return note;
        }

        [HttpPut("{id}")]
        public void EditNote([FromRoute] Guid id,
            [FromBody] NoteRequest request)
        {
            var dbNote = _dbContext.Notes.FirstOrDefault(n => n.Id == id);
            if (dbNote == null)
            {
                throw new Exception("There is no note with such Id");
            }
            if (!string.IsNullOrEmpty(request.Title))
            {
                dbNote.Title = request.Title;
            }
            if (!string.IsNullOrEmpty(request.Content))
            {
                dbNote.Content = request.Content;
            }

            _dbContext.Entry(dbNote).State = EntityState.Modified;
            _dbContext.SaveChanges();
        }

        [HttpGet]
        public List<NoteResponse> GetNotesByQuery([FromQuery] string query)
        {
            var dbNotes = _dbContext.Notes.Where(n => n.Title.Contains(query) || n.Content.Contains(query));
            
            var notes = new List<NoteResponse>();
            var mapperConfig = new MapperConfiguration(cfg => cfg.CreateMap<DbNote, NoteResponse>());
            var mapper = new Mapper(mapperConfig);
            dbNotes.ToList().ForEach(n => notes.Add(mapper.Map<NoteResponse>(n)));
            
            foreach (var note in notes)
            {
                note.Title = string.IsNullOrEmpty(note.Title) ? note.Content.Substring(0, _config.NumberOfSymbols) : note.Title;
            }
            return notes;
        }

        [HttpDelete("{id}")]
        public void DeleteNote([FromRoute] Guid id)
        {
            var dbNote = _dbContext.Notes.FirstOrDefault(n => n.Id == id);
            if (dbNote == null)
            {
                throw new Exception("There is no note with such Id");
            }
            _dbContext.Notes.Remove(dbNote);
            _dbContext.SaveChanges();
        }
    }
}
