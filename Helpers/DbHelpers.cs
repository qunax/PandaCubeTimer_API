using PandaCubeTimer_API.Data;
using PandaCubeTimer_API.Models;

namespace PandaCubeTimer_API.Helpers;

public static class DbHelpers
{
    public static void SeedDisciplines(ApiDbContext db)
    {
        if (!db.Disciplines.Any())
        {
            var disciplines = new List<Discipline>
            {
                new() { Id = WcaDisciplines.Cube3x3, Name = "3x3x3 Cube" },
                new() { Id = WcaDisciplines.Cube2x2, Name = "2x2x2 Cube" },
                new() { Id = WcaDisciplines.Cube4x4, Name = "4x4x4 Cube" },
                new() { Id = WcaDisciplines.Pyraminx, Name = "Pyraminx" },
                new() { Id = WcaDisciplines.Megaminx, Name = "Megaminx" },
                new() { Id = WcaDisciplines.Square1, Name = "Square-1" },
                new() { Id = WcaDisciplines.Clock, Name = "Rubik's Clock" },
                new() { Id = WcaDisciplines.OneHanded, Name = "3x3x3 One-Handed" },
                new() { Id = WcaDisciplines.Custom, Name = "Custom discipline" }
            };

            db.Disciplines.AddRange(disciplines);
            db.SaveChanges();
        }
    }
}