using CoursePublishing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Script
{
    class Program
    {
        static void Main(string[] args)
        {
            foreach(var e in new[] { "OOP", "LHPS", "Hackerdom" })
            {
                var s = Publishing.Courses[e].Load<Structure>();
                Publishing.SaveCourseStructure(s, e);
            }
        }
    }
}
