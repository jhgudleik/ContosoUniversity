using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using ContosoUniversity.Data;
using ContosoUniversity.Models;
using Microsoft.Extensions.Configuration;


namespace ContosoUniversity.Pages.Students
{
    public class IndexModel : PageModel
    {
        private readonly ContosoUniversity.Data.ContosoUniversityContext _context;

        private readonly IConfiguration configuration;

        public IndexModel(ContosoUniversity.Data.ContosoUniversityContext context, IConfiguration configuration)
        {
            _context = context;
            this.configuration = configuration;

        }
        public string NameSort { get; set; }
        public string DateSort { get; set; }
        public string CurrentFilter { get; set; }
        public string CurrentSort   { get; set; }
        public PaginatedList<Student> PLStudents { get; set; }

        public IQueryable<Student> Students { get;set; } = default!;

        public async Task OnGetAsync(string sortOrder, string searchString, int? pageIndex)
        {
            CurrentSort = sortOrder;
            NameSort = String.IsNullOrEmpty(sortOrder) ? "name_desc" : "";
            DateSort = sortOrder == "Date" ? "date_desc" : "Date";

            if (searchString != null) pageIndex = 1;
            else searchString = CurrentFilter;

            CurrentFilter = searchString;

            IQueryable<Student> students = from s in _context.Students
                                            select s;

            if (!String.IsNullOrEmpty(searchString))
                students = students
                    .Where(s => s.LastName.Contains(searchString)
                                       || s.FirstName.Contains(searchString));
            switch (sortOrder)
            {
                case "name_desc":
                    students = students.OrderByDescending(s => s.LastName);
                    break;
                case "Date":
                    students = students.OrderBy(s => s.EnrollmentDate);
                    break;
                case "date_desc":
                    students = students.OrderByDescending(s => s.EnrollmentDate);
                    break;
                default:
                    students = students.OrderBy(s => s.LastName);
                    break;
            }
//            this.Students = await students.AsNoTracking(); //.ToListAsync();
            this.Students = students; //.ToListAsync();
            int pageSize = configuration.GetValue("PageSize", 5);
            this.PLStudents = await PaginatedList<Student>.CreateAsync(
                Students.AsNoTracking(), pageIndex ?? 1, pageSize);
        }

        /*
        public async Task OnGetAsync_1()
        {
            Students = await _context.Students.ToListAsync();
        } 
        */
    }
}
