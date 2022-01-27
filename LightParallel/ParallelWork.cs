namespace LightParallel
{
    public static class ParallelWork
    {
        /// <summary>
        /// Takes care of triggering every Task in Parallel.
        /// </summary>
        /// <typeparam name="T">Object to use as Input.</typeparam>
        /// <param name="Inputs">List of Inputs that have to be processed by the Action.</param>
        /// <param name="Action">Work to do for each Input.</param>
        /// <returns>A task that waits for all the triggered Tasks.</returns>
        /// <exception cref="ArgumentNullException"></exception>
        public static Task ForEach<T>(this IEnumerable<T> Inputs, Action<T> Action)
        {
            if (Inputs is null) throw new ArgumentNullException(nameof(Inputs));
            if (Action is null) throw new ArgumentNullException(nameof(Action));

            List<Task> workInProgress = new List<Task>();

            foreach (var input in Inputs.ToActionEnumerable(Action))
            {
                workInProgress.Add(input.StartAsTask());
            }

            return Task.WhenAll(workInProgress);
        }

        /// <summary>
        /// Takes care of triggering every Task in Parallel and keeps the number of threads under control.
        /// </summary>
        /// <typeparam name="T">Object to use as Input.</typeparam>
        /// <param name="Inputs">List of Inputs that have to be processed by the Action.</param>
        /// <param name="Action">Work to do for each Input.</param>
        /// <param name="ThreadCount">Max number of threads.</param>
        /// <returns>A Task waiting for the work to complete.</returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public static async Task ForEach<T>(this IEnumerable<T> Inputs, Action<T> Action, int ThreadCount)
        {
            if(Inputs is null) throw new ArgumentNullException(nameof(Inputs));
            if(Action is null) throw new ArgumentNullException(nameof(Action));
            if(ThreadCount < 1) throw new ArgumentOutOfRangeException(nameof(ThreadCount), "ThreadCount should be at least 1.");

            int nextItem = ValidateThreadCount(Inputs.Count(), ThreadCount);
            var pendingWork = Inputs.ToActionEnumerable(Action);
            var workInProgress = pendingWork.Initialize(nextItem).ToList();

            while (workInProgress.Any())
            {
                var finishedTable = await Task.WhenAny(workInProgress).ConfigureAwait(false);

                // Trigger new task
                if (nextItem < pendingWork.Count())
                {
                    workInProgress.Add(pendingWork.ElementAt(nextItem).StartAsTask());
                    nextItem++;
                }

                // Remove completed task
                workInProgress.Remove(finishedTable);
                finishedTable.Dispose();
            }
        }

        private static int ValidateThreadCount(int inputsCount, int ThreadCount)
            => (inputsCount < ThreadCount) ? inputsCount : ThreadCount;
        
        /// <summary>
        /// Set-Up all the actions
        /// </summary>
        private static IEnumerable<Action> ToActionEnumerable<T>(this IEnumerable<T> inputs, Action<T> action)
        {
            if (inputs is not null)
            {
                foreach (var input in inputs)
                {
                    yield return () => action(input);
                }
            }
        }

        /// <summary>
        /// Start the firt X number of tasks based on the desider thread count.
        /// </summary>
        private static IEnumerable<Task> Initialize(this IEnumerable<Action> actions, int threadCount)
        {
            foreach (var action in actions.Take(threadCount))
            {
                yield return action.StartAsTask();
            }
        }

        private static Task StartAsTask(this Action action) => Task.Run(() => action());
    }

    
}