using Microsoft.VisualStudio.TestTools.UnitTesting;
using SpecialtyManagement;
using System;

namespace SpecialtyManagementTest
{
    [TestClass]
    public class UnitTest
    {
        [TestMethod]
        public void GetStudentsFromFile_EqualResultZeroWithFileNotFound()
        {
            Assert.AreEqual(0, Students.GetStudentsFromFile("testFile.test").Count);
        }

        [TestMethod]
        public void GetYearAndSemester_TrueResultForCurrentSemester()
        {
            bool result = false;
            Arrears.GetYearAndSemester(out int year, out int semester, true);
            if (year == DateTime.Now.Year - 1 && semester == 2)
            {
                result = true;
            }

            Assert.IsTrue(result);
        }

        [TestMethod]
        public void GetYearAndSemester_TrueResultForLastSemester()
        {
            bool result = false;
            Arrears.GetYearAndSemester(out int year, out int semester, false);
            if (year == DateTime.Now.Year - 1 && semester == 1)
            {
                result = true;
            }

            Assert.IsTrue(result);
        }
    }
}