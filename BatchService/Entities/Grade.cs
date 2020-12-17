using System;
using System.Collections.Generic;

namespace BatchService.Entities
{
    public class Grade
    {
        public long Id { get; set; }
        public DateTime CreateDateTime { get; set; }
        public DateTime ModifyDateTime { get; set; }
        public string Name { get; set; }
        public string Section { get; set; }
        public ICollection<Student> Students { get; set; }
    }
}
