using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SchoolDemo.Data;
using SchoolDemo.Models;
using SchoolDemo.Models.Interfaces;
using Microsoft.EntityFrameworkCore;
using SchoolDemo.Models.Api;

namespace SchoolDemo.Services
{
  public class StudentRepository : IStudent
  {
    private SchoolDbContext _context;

    public StudentRepository(SchoolDbContext context)
    {
      _context = context;
    }
    public async Task<Student> Create(NewStudentDto inboundData)
    {

      // Add the student
      Student student = new Student()
      {
        FirstName = inboundData.Name.Split(" ").First<string>(),
        LastName = inboundData.Name.Split(" ").Last<string>(),
        DateOfBirth = inboundData.Dob
      };

      _context.Entry(student).State = Microsoft.EntityFrameworkCore.EntityState.Added;
      await _context.SaveChangesAsync();


      Course course = await _context.Courses.FirstOrDefaultAsync(c => c.CourseCode == inboundData.CourseCode);

      // Add student to course
      Enrollment enrollment = new Enrollment()
      {
        CourseId = course.Id,
        StudentId = student.Id
      };

      _context.Entry(enrollment).State = EntityState.Added;
      await _context.SaveChangesAsync();


      return student;
    }

    public async Task<StudentDto> GetOne(int id)
    {

      return await _context.Students
          .Select(student => new StudentDto
          {
            Id = student.Id,
            FirstName = student.FirstName,
            LastName = student.LastName,
            Grades = student.Transcripts
              .Select(t => new StudentGradeDto
              {
                CourseCode = t.Course.CourseCode,
                Grade = t.Grade.ToString(),
              }).ToList()
          }).FirstOrDefaultAsync(s => s.Id == id);

    }

    public async Task<List<Student>> GetAll()
    {

      return await _context.Students
                     .Include(s => s.Enrollments)
                     .ThenInclude(e => e.Course)
                     .Include(s => s.Transcripts)
                     .ThenInclude(t => t.Course)
                     .ToListAsync();
    }

    public async Task<Student> Update(int id, Student student)
    {
      _context.Entry(student).State = Microsoft.EntityFrameworkCore.EntityState.Modified;
      await _context.SaveChangesAsync();
      return student;
    }

    public async Task Delete(int id)
    {
      // Student student = await GetOne(id);
      // _context.Entry(student).State = Microsoft.EntityFrameworkCore.EntityState.Deleted;
      // await _context.SaveChangesAsync();
    }


    public async Task AddGradeToTranscript(int studentId, Transcript grade)
    {
      var transcript = new Transcript
      {
        StudentId = studentId,
        CourseId = grade.CourseId,
        Grade = grade.Grade,
        Passed = (int)grade.Grade >= 3
      };

      _context.Transcripts.Add(transcript);
      await _context.SaveChangesAsync();
    }

  }
}
