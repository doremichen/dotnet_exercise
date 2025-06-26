/**
 * Description: This class represents AppDbContext for the Student Grade Manager application
 * Author: Adam chen
 * Date: 2025-06-26
 */
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudentGradeManager.Data
{
    public class AppDbContext : DbContext
    {
        // students table
        public DbSet<Models.Student> Students { get; set; }
        // grades table
        public DbSet<Models.Grade> Grades { get; set; }

        // onConfiguring the database connection
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            // Use SQLite database
            optionsBuilder.UseSqlite("Data Source=student_grades.db");
        }
    }
}
