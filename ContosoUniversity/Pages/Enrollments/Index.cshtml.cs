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

namespace ContosoUniversity.Pages.Enrollments
{
    public class IndexModel : PageModel
    {
        private readonly ContosoUniversity.Data.ContosoUniversityContext _context;
        private readonly IConfiguration _configuration;

        public IndexModel(ContosoUniversity.Data.ContosoUniversityContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        public string StudentSort { get; set; }
        public string CourseSort { get; set; }
        public string GradeSort { get; set; }
        public string CurrentFilter { get; set; }
        public string CurrentSort { get; set; }
        public PaginatedList<Enrollment> PLEnrollments { get; set; } = default!;
        public int? PageSize { get; set; }

        public async Task OnGetAsync(string sortOrder, string searchString, int? pageIndex, int? pageSize)
        {
            CurrentSort = sortOrder;
            CurrentFilter = searchString;

            // Инициализация сортировок
            StudentSort = String.IsNullOrEmpty(sortOrder) ? "student_desc" : "";
            CourseSort = sortOrder == "course" ? "course_desc" : "course";
            GradeSort = sortOrder == "grade" ? "grade_desc" : "grade";

            // Сброс страницы при новом поиске
            if (!string.IsNullOrEmpty(searchString))
                pageIndex = 1;
            else
                searchString = CurrentFilter;

            CurrentFilter = searchString;

            // Начинаем с Include — сохраняем в IQueryable
            IQueryable<Enrollment> enrollments = _context.Enrollments
                .Include(e => e.Student)
                .Include(e => e.Course);

            // Фильтрация по поисковой строке
            if (!string.IsNullOrEmpty(searchString))
            {
                enrollments = enrollments.Where(e =>
                    e.Student.LastName.Contains(searchString) ||
                    e.Student.FirstName.Contains(searchString) ||
                    e.Course.Title.Contains(searchString));
            }

            // Сортировка
            switch (sortOrder)
            {
                case "student_desc":
                    enrollments = enrollments.OrderByDescending(e => e.Student.LastName).ThenByDescending(e => e.Student.FirstName);
                    break;
                case "course":
                    enrollments = enrollments.OrderBy(e => e.Course.Title);
                    break;
                case "course_desc":
                    enrollments = enrollments.OrderByDescending(e => e.Course.Title);
                    break;
                case "grade":
                    enrollments = enrollments.OrderBy(e => e.Grade);
                    break;
                case "grade_desc":
                    enrollments = enrollments.OrderByDescending(e => e.Grade);
                    break;
                default:
                    enrollments = enrollments.OrderBy(e => e.Student.LastName).ThenBy(e => e.Student.FirstName);
                    break;
            }

            // ИСПОЛЬЗУЕМ переданный pageSize, если есть, иначе значение из конфигурации
            // ПРИСВАИВАЕМ его в свойство модели для использования в Razor
            PageSize = pageSize;
            int actualPageSize = pageSize ?? _configuration.GetValue<int>("PageSize", 5);

            PLEnrollments = await PaginatedList<Enrollment>.CreateAsync(
                enrollments.AsNoTracking(),
                pageIndex ?? 1,
                actualPageSize);
        }
    }
}
