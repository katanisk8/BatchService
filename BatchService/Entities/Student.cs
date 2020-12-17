using System;

namespace BatchService.Entities
{
    public class Student
    {
        public long Id { get; set; }
        public DateTime CreateDateTime { get; set; }
        public DateTime ModifyDateTime { get; set; }
        public string StudentName { get; set; }
        public DateTime DateOfBirth { get; set; }
        public string CityOfBirth { get; set; }
        public decimal Height { get; set; }
        public float Weight { get; set; }
        public string Firstname { get; set; }
        public string Surname { get; set; }
        public long GradeId { get; set; }
        public Grade Grade { get; set; }
    }
}
