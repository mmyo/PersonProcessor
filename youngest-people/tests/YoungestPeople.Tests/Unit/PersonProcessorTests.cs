using System;
using System.Collections.Generic;
using System.Linq;
using YoungestPeople.Models;
using YoungestPeople.Services;
using Xunit;

namespace YoungestPeople.Tests.Unit
{
    public class PersonProcessorTests
    {
        [Fact]
        public void GetYoungest_ReturnsTopNYoungest()
        {
            var people = new List<Person>
            {
                new Person("A","A", new DateTime(1980,1,1)),
                new Person("B","B", new DateTime(2000,1,1)),
                new Person("C","C", new DateTime(1990,1,1)),
                new Person("D","D", new DateTime(2010,1,1)),
                new Person("E","E", new DateTime(1995,1,1)),
            };

            var proc = new PersonProcessor();
            var top2 = proc.GetYoungest(people, 2);

            Assert.Equal(2, top2.Count);
            // youngest first
            Assert.Equal(new DateTime(2010,1,1), top2[0].DateOfBirth);
            Assert.Equal(new DateTime(2000,1,1), top2[1].DateOfBirth);
        }

        [Fact]
        public void GetYoungest_WithLessThanRequested_ReturnsAllSorted()
        {
            var people = new List<Person>
            {
                new Person("A","A", new DateTime(1990,1,1)),
                new Person("B","B", new DateTime(1992,1,1))
            };
            var proc = new PersonProcessor();
            var top5 = proc.GetYoungest(people, 5);
            Assert.Equal(2, top5.Count);
            Assert.Equal(new DateTime(1992,1,1), top5[0].DateOfBirth);
            Assert.Equal(new DateTime(1990,1,1), top5[1].DateOfBirth);
        }
    }
}
