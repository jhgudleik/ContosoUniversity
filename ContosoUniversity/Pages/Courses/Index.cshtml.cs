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

namespace ContosoUniversity.Pages.Courses
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

        public string TitleSort { get; set; }
        public string CreditSort { get; set; }
        public string CurrentFilter { get; set; }
        public string CurrentSort { get; set; }

        public PaginatedList<Course> PLCourses { get; set; } = default!;
        public int? PageSize { get; set; }

        public async Task OnGetAsync(string sortOrder, string searchString, int? pageIndex, int? pageSize)
        {
            CurrentSort = sortOrder;
            CurrentFilter = searchString;

            // Инициализация сортировок
            TitleSort = String.IsNullOrEmpty(sortOrder) ? "title_desc" : "";
            CreditSort = sortOrder == "credits" ? "credits_desc" : "credits";

            // Сброс страницы при новом поиске
            if (!string.IsNullOrEmpty(searchString))
                pageIndex = 1;
            else
                searchString = CurrentFilter;

            CurrentFilter = searchString;

            // Начинаем с запроса
            IQueryable<Course> courses = _context.Courses;

            // Фильтрация по названию курса
            if (!string.IsNullOrEmpty(searchString))
            {
                courses = courses.Where(c => c.Title.Contains(searchString));
            }

            // Сортировка
            switch (sortOrder)
            {
                case "title_desc":
                    courses = courses.OrderByDescending(c => c.Title);
                    break;
                case "credits":
                    courses = courses.OrderBy(c => c.Credits);
                    break;
                case "credits_desc":
                    courses = courses.OrderByDescending(c => c.Credits);
                    break;
                default:
                    courses = courses.OrderBy(c => c.Title); // По умолчанию
                    break;
            }

            // Установка размера страницы
            PageSize = pageSize;
            int actualPageSize = pageSize ?? _configuration.GetValue<int>("PageSize", 5);

            PLCourses = await PaginatedList<Course>.CreateAsync(
                courses.AsNoTracking(),
                pageIndex ?? 1,
                actualPageSize);
        }
    }
}
