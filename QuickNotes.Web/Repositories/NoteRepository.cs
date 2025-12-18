using System.Collections.Generic;
using System.Linq;
using QuickNotes.Web.Data;
using QuickNotes.Web.Models;

namespace QuickNotes.Web.Repositories
{
    public interface INoteRepository
    {
        IEnumerable<Note> GetByUserId(int userId);
        Note GetById(int id);
        void Add(Note note);
        void Update(Note note);
        void Delete(int id);
    }

    public class NoteRepository : INoteRepository
    {
        private readonly ApplicationDbContext _context;

        public NoteRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public IEnumerable<Note> GetByUserId(int userId)
        {
            return _context.Notes.Where(n => n.UserId == userId).ToList();
        }

        public Note GetById(int id)
        {
            return _context.Notes.FirstOrDefault(n => n.Id == id);
        }

        public void Add(Note note)
        {
            _context.Notes.Add(note);
            _context.SaveChanges();
        }

        public void Update(Note note)
        {
            _context.Notes.Update(note);
            _context.SaveChanges();
        }

        public void Delete(int id)
        {
            var note = _context.Notes.Find(id);
            if (note != null)
            {
                _context.Notes.Remove(note);
                _context.SaveChanges();
            }
        }
    }
}
