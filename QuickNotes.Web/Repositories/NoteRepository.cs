using System.Collections.Generic;
using System.Linq;
using QuickNotes.Web.Data;
using QuickNotes.Web.Models;

namespace QuickNotes.Web.Repositories
{
    public interface INoteRepository
    {
        IEnumerable<Note> GetByUserId(int userId);
        void Add(Note note);
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

        public void Add(Note note)
        {
            _context.Notes.Add(note);
            _context.SaveChanges();
        }
    }
}
