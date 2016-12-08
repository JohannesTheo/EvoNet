﻿using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace EvoSim.ThreadingHelper
{
    public class TaskManager
    {
        readonly int poolSize;

        private Thread[] workerThreads;
        private Queue<ThreadTask> workerQueue = new Queue<ThreadTask>();
        private List<ThreadTaskGroup> pendingTaskGroups = new List<ThreadTaskGroup>();
        private List<ThreadTaskGroup> completedTaskGroups = new List<ThreadTaskGroup>();
        private List<ThreadTaskGroup> runningTaskGroups = new List<ThreadTaskGroup>();
        private Semaphore workerSemaphore;
        private GameTime currentTime;

        public TaskManager(int threadPoolSize)
        {
            poolSize = threadPoolSize;
            workerSemaphore = new Semaphore(0, 1024);
            workerThreads = new Thread[poolSize];
            for (int threadIndex = 0; threadIndex < workerThreads.Length; threadIndex++)
            {
                workerThreads[threadIndex] = new Thread(new ParameterizedThreadStart(ThreadRunner));
                workerThreads[threadIndex].Start(threadIndex);
            }
        }

        public void AddGroup(ThreadTaskGroup group)
        {
            pendingTaskGroups.Add(group);
        }

        private bool keepRunning = true;

        public void RunTasks(GameTime time)
        {
            currentTime = time;
            while (pendingTaskGroups.Count > 0 || runningTaskGroups.Count > 0)
            {
                for (int groupIndex = pendingTaskGroups.Count-1; groupIndex >= 0; --groupIndex)
                {
                    if (pendingTaskGroups[groupIndex].CanBeQueued())
                    {
                        ThreadTaskGroup taskGroup = pendingTaskGroups[groupIndex];
                        runningTaskGroups.Add(taskGroup);
                        pendingTaskGroups.RemoveAt(groupIndex);
                        lock (workerQueue)
                        {
                            foreach (ThreadTask task in taskGroup.Tasks)
                            {
                                workerQueue.Enqueue(task);
                            }
                            workerSemaphore.Release(taskGroup.Tasks.Count);
                        }
                    }
                }
                for (int runningGroupIndex = runningTaskGroups.Count-1; runningGroupIndex >= 0; --runningGroupIndex)
                {
                    if (runningTaskGroups[runningGroupIndex].IsDone())
                    {
                        completedTaskGroups.Add(runningTaskGroups[runningGroupIndex]);
                        runningTaskGroups.RemoveAt(runningGroupIndex);
                    }
                }
                Thread.Yield();
            }
        }

        public void ResetTaskGroups()
        {
            if (runningTaskGroups.Count > 0)
            {
                throw new InvalidOperationException("Cannot reset task groups while we have running tasks.");
            }
            foreach (ThreadTaskGroup group in completedTaskGroups)
            {
                group.Reset();
            }
            pendingTaskGroups.AddRange(completedTaskGroups);
            completedTaskGroups.Clear();
        }

        public void Shutdown()
        {
            keepRunning = false;
            lock (workerQueue)
            {
                workerQueue.Clear();
            }
            workerSemaphore.Release(poolSize);
        }

        private void ThreadRunner(object state)
        {
            int threadIndex = (int)state;
            while(keepRunning)
            {
                ThreadTask task = null;
                lock (workerQueue)
                {
                    if (workerQueue.Count > 0)
                    {
                        task = workerQueue.Dequeue();
                    }
                }
                if (task!= null)
                {
                    task.DoTask(currentTime);
                }
                else
                {
                    workerSemaphore.WaitOne();
                }
            }
        }
    }
}
