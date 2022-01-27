using LightParallelTests.Mocks;
using NUnit.Framework;
using LightParallel;
using System.Linq;
using System.Threading.Tasks;
using System;

namespace LightParallelTests
{
    public class ParallelWork
    {
        [Test]
        public async Task CanProcessItemsWithoutThreadLimits()
        {
            var testSubject = new MockInputList().Inputs;

            await testSubject.ForEach(SomeWork).ConfigureAwait(false);

            Assert.True(testSubject.Where(x => x.WasProcessed == true).Count() == testSubject.Count());
        }

        [Test]
        public async Task CanLimitThreadsToX_WhenXIsSmallerThanInputs()
        {
            var testSubject = new MockInputList().Inputs;

            await testSubject.ForEach(SomeMoreWork, 2).ConfigureAwait(false);

            Assert.True(testSubject.Where(x => x.WasProcessed == true).Count() == testSubject.Count());
        }

        [Test]
        public async Task CanLimitThreadsToX_WhenXIsLargerThanInputs()
        {
            var testSubject = new MockInputList().Inputs;

            await testSubject.ForEach(SomeMoreWork, 400).ConfigureAwait(false);

            Assert.True(testSubject.Where(x => x.WasProcessed == true).Count() == testSubject.Count());
        }

        [Test]
        public void ValidatesInput()
        {
            // With Valid List
            var testSubject = new MockInputList().Inputs;
            Assert.ThrowsAsync<ArgumentNullException>(async () => await testSubject.ForEach(null)); // Invalid Task
            Assert.ThrowsAsync<ArgumentNullException>(async () => await testSubject.ForEach(null, 1)); // Invalid Task
            Assert.ThrowsAsync<ArgumentOutOfRangeException>(async () => await testSubject.ForEach(SomeMoreWork, 0)); // Invalid Thread Count

            // With Invalid List
            testSubject = null;
            Assert.ThrowsAsync<ArgumentNullException>(async () => await testSubject.ForEach(SomeMoreWork));
            Assert.ThrowsAsync<ArgumentNullException>(async () => await testSubject.ForEach(SomeMoreWork, 1));
        }

       /// <summary>
       /// Sample Task.
       /// </summary>
        private Action<MockInput> SomeWork = input => input.WasProcessed = true;

        /// <summary>
        /// Sample task.
        /// </summary>
        private void SomeMoreWork(MockInput input)
        {
            input.WasProcessed = true;
        }
        
    }
}