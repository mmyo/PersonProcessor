using System;
using System.Collections.Generic;
using System.Linq;
using YoungestPeople.Models;

namespace YoungestPeople.Services
{
    /// <summary>
    /// Processes collections of Person records to find the youngest individuals.
    /// Uses a priority queue for O(n log k) complexity where n is the number of records
    /// and k is the number of youngest people to return.
    /// </summary>
    public sealed class PersonProcessor
    {
        /// <summary>
        /// Finds the N youngest people from a collection, returning them in order of youngest to oldest.
        /// </summary>
        /// <param name="source">The collection of people to process</param>
        /// <param name="take">Number of youngest people to return</param>
        /// <returns>A list of the youngest people, ordered by age (youngest first)</returns>
        /// <remarks>
        /// Implementation uses a min-heap to maintain the top N youngest people while streaming through
        /// the input. This ensures O(n log k) time complexity and O(k) space complexity where:
        /// n = number of input records
        /// k = number of records to return
        /// </remarks>
        public IReadOnlyList<Person> GetYoungest(IEnumerable<Person> source, int take = 10)
        {
            if (take <= 0) return Array.Empty<Person>();

            // Use min-heap priority queue, keeping youngest N people by comparing birth dates
            var pq = new PriorityQueue<Person, long>();

            foreach (var person in source)
            {
                // Convert birth date to ticks for comparison
                // Higher tick count = more recent date = younger person
                var priority = person.DateOfBirth.Ticks;
                
                if (pq.Count < take)
                {
                    // Still building initial set
                    pq.Enqueue(person, priority);
                }
                else if (priority > pq.Peek().DateOfBirth.Ticks)
                {
                    // Current person is younger than oldest in our set
                    pq.Dequeue(); // Remove oldest
                    pq.Enqueue(person, priority); // Add current
                }
            }

            // Convert priority queue to ordered list (youngest first)
            var result = new List<Person>(pq.Count);
            while (pq.Count > 0)
            {
                result.Add(pq.Dequeue());
            }

            // Dequeued from smallest -> largest priority (oldest to youngest). Reverse to return youngest first.
            result.Reverse();
            return result;
        }
    }
}
