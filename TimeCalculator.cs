// Scheduling algorithms
// The field "active" indicates whether the process is currently competing for the CPU. The value becomes 1 at the time of process arrival and 0 at the time of process termination. Initially, the value is set to 1 for all processes with arrival time Aᵢ = 0.
// Each Aᵢ is an integer chosen randomly from a uniform distribution between 0 and some value k, where k is a simulation parameter. Example: any integer between 0 and 1000.
// Each Tᵢ is an integer chosen randomly from a normal (Gaussian) distribution with an average d and a standard deviation v, where d and v are simulation parameters.
/* When d is much smaller than k/n, then most processes run in isolation, without having to compete with other processes for the CPU. As a result, all scheduling algorithms should produce similar results. On the other hand, when d is much larger than k/n, 
   then many processes will overlap in time and compete for the CPU. Consequently, different scheduling algorithms should yield different results. */
// Each Rᵢ is initialized to Tᵢ, since prior to execution, the remaining time is equal to the total CPU time required.

using MathNet.Numerics.Distributions;
using System.Data;

namespace Scheduling_Algorithm
{
    class TimeCalculator
    {
        public struct Process
        {
            public string ProcessID;
            public int Active;
            public int A;
            public int T;
            public int R;
            public int TT;

            public Process(string processID, int active, int arrivalTime, int totalCPUTime, int remainingCPUTime, int TurnAroundTime)
            {
                ProcessID = processID;
                Active = active;
                A = arrivalTime;
                T = totalCPUTime;
                R = remainingCPUTime;
                TT = TurnAroundTime;
            }
        }

        private const int n = 5; // Number of processes.
        private const int k = 100; // Upper limit of arrival times.
        private const double d = 500; // Mean of normal distribution.
        private const double v = 10; //Deviation.
        public static int t = 0; // The simulation maintains the current time, t, which is initialized to 0 and is incremented after each simulation step. If no process is ready to run, just advance t.

        static void Main(string[] args)
        {
            Console.WriteLine($"When d {d} is much smaller than k/n {k / n}, then most processes run in isolation.");

            List<Process> processes = new List<Process>(n);

            // Create processes
            for (int i = 0; i < n; i++)
            {
                int totalCPUTime = GetTotalCPUtime();
                processes.Add(new Process { ProcessID = "p" + (i + 1), Active = 1, A = GetArrivalTime(), T = totalCPUTime, R = totalCPUTime });
            }

            // Order process list by arrival time to start First in First out scheduling.
            List<Process> ArrivalTimeOrderedProcesses = processes.OrderBy(o => o.A).ToList();

            Console.WriteLine("<--------------------------------Parameters of each Process--------------------------------->");
            foreach (Process process in ArrivalTimeOrderedProcesses)
            {
                Console.WriteLine(process.ProcessID);
                Console.WriteLine($"Active: {process.Active}");
                Console.WriteLine($"Arrival time: {process.A}");
                Console.WriteLine($"Remaining time /Total CPU Time: {process.R}");
                Console.WriteLine("--------------------------------------------------------");
            }

            Console.WriteLine("<------------Turn around times by process and ATT for each Scheduling Algorithm------------>");

            // FIFO:
            int FIFOAvgTT = GetAvgTTforFIFO(ArrivalTimeOrderedProcesses);
            Console.WriteLine($"The ATT for FIFO is: {FIFOAvgTT}. d/ATT: {d / FIFOAvgTT}");
            Console.WriteLine("--------------------------------------------------------");

            // SJF:
            int SJFAvgTT = GetAvgTTforSJF(ArrivalTimeOrderedProcesses);
            Console.WriteLine($"The ATT for SJF is: {SJFAvgTT}. d/ATT: {d / SJFAvgTT}");
            Console.WriteLine("--------------------------------------------------------");

            // SRT:
            int SRTAvgTT = GetAvgTTforSRT(ArrivalTimeOrderedProcesses);
            Console.WriteLine($"The ATT for SRT is: {SRTAvgTT}. d/ATT: {d / SRTAvgTT}");
            Console.WriteLine("--------------------------------------------------------");
        }

        static int GetAvgTTforFIFO(List<Process> FIFOOrderedProcesses)
        {
            // Adding time to currentTime until the first process starts.
            t += FIFOOrderedProcesses[0].A;

            // Collecting all turnaround times. 
            int allTT = 0;

            for (int i = 0; i < n; i++)
            {
                Process currentProcess = FIFOOrderedProcesses[i];
                int TT = GetTurnAroundTimeForProcess(FIFOOrderedProcesses, i, currentProcess.R, currentProcess.A, t, currentProcess.Active);
                currentProcess.TT = TT;
                allTT += TT;
            }

            int avgTTforFIFO = allTT / n;

            return avgTTforFIFO;
        }

        static int GetAvgTTforSJF(List<Process> processes)
        {
            // Set time to 0.
            t = 0;

            // Adding time to currentTime until the first process starts.
            t += processes[0].A;

            // Collecting all turnaround times. 
            int allTT = 0;

            for (int i = 0; i < n; i++)
            {
                Process currentProcess = processes[i];
                int TT = GetTurnAroundTimeForProcess(processes, i, currentProcess.R, currentProcess.A, t, currentProcess.Active);
                currentProcess.TT = TT;
                allTT += TT;

                // Set next job to next arrival time, then search for ready jobs and select shortest job to run next. No check for last process.
                if (i < n-1)
                {
                    Process nextShortestjob = processes[i+1];

                    for (int g = i + 1; g < n - 1; g++)
                    {
                        if ((processes[g].A <= t) && (processes[g].R < nextShortestjob.R))
                        {
                            nextShortestjob = processes[g];
                            processes.RemoveAt(g);
                            processes.Insert(i + 1, nextShortestjob);
                        }
                    }
                }
            }

            int avgTTforSJF = allTT / n;

            return avgTTforSJF;
        }

        static int GetAvgTTforSRT(List<Process> processes)
        {
            // Set time to 0 and initialize turnaround time.
            t = 0;

            // Adding time to currentTime until the first process starts.
            t += processes[0].A;

            int allTT = GetTurnAroundTimeForSRT(processes, t);


            int avgTTforSRT = allTT / n;

            return avgTTforSRT;
        }

        static int GetTurnAroundTimeForSRT(List<Process> processes, int currentTime)
        {
            int TT = 0;
            int allTT = 0;

            for (int i = 0; i < n; i++)
            {
                int remainingTime = processes[i].R;

                while (remainingTime != 0) /* repeat until all processes have terminated */
                {
                    Process currentProcess = processes[i];

                    currentTime += 1;
                    remainingTime -= 1;

                    // If shorter process exists, swap the processes and continue with shorter process.
                    if ((i < n - 1) && (processes[i + 1].Active == 1) && (processes[i + 1].A <= currentTime) && ((processes[i + 1].R + currentTime) < (remainingTime + currentTime)))
                    {
                        Process longerProcess = new Process(currentProcess.ProcessID, remainingTime, currentProcess.A, currentProcess.T, currentProcess.R, currentProcess.TT);
                        Process shorterProcess = processes[i + 1];

                        processes.RemoveAt(i);
                        processes.Insert(i, shorterProcess);
                        processes.RemoveAt(i + 1);
                        processes.Insert(i + 1, longerProcess);

                        //Set remaining Time to new i process remaining time.
                        remainingTime = processes[i].R;

                    }
                }

                // If R==0 the process has finished.
                if (remainingTime == 0)
                {
                    Process currentProcess = processes[i];

                    currentProcess.Active = 0;
                    TT = currentTime - currentProcess.A;

                    Console.WriteLine($"TT for {currentProcess.ProcessID} : {TT}");

                    // If the currentTime is smaller then the next processes Arrival time, there will be no process running. We'll still need to add this time to currentTime. Step not necessary for last process. 
                    if (i < n - 1 && currentTime < processes[i + 1].A)
                    {
                        int diff = processes[i + 1].A - currentTime;
                        currentTime += diff;
                    }
                    else if (i < n - 1 && currentTime > processes[i + 1].A)
                    {
                        //Console.WriteLine($"There is a delay for {processes[i+1].ProcessID}.");
                    }

                    allTT += TT;
                }
            }

            return allTT;
        }

        static int GetTurnAroundTimeForProcess(List<Process> processes, int processCount, int remainingCPUTime, int arrivalTime, int currentTime, int isActive)
        {
            int TT = 0;

            while (remainingCPUTime != 0) /* repeat until all processes have terminated */
            {
                currentTime += 1;
                remainingCPUTime -= 1;
            }

            // If R==0 the process has finished.
            if (remainingCPUTime == 0)
            {
                isActive = 0;
                TT = currentTime - arrivalTime;

                Console.WriteLine($"TT for {processes[processCount].ProcessID} : {TT}");

                // If the currentTime is smaller then the next processes Arrival time, there will be no process running. We'll still need to add this time to currentTime. Step not necessary for last process. 
                if (processCount < n - 1 && currentTime < processes[processCount + 1].A)
                {
                    int diff = processes[processCount + 1].A - currentTime;
                    currentTime += diff;
                }
                else if (processCount < n - 1 && currentTime > processes[processCount + 1].A)
                {
                    //Console.WriteLine("There is a delay.");
                }

            }

            t = currentTime;

            return TT;
        }

        static int GetTotalCPUtime()
        {
            //chosen randomly from a normal (Gaussian) distribution with an average d and a standard deviation v, where d and v are simulation parameters.
            Normal normalDist = new Normal(d, v);
            double totalCPUTime = normalDist.Sample();
            int totalCpuInt = Convert.ToInt32(totalCPUTime);

            return totalCpuInt;
        }

        static int GetArrivalTime()
        {
            // Arrival time. Generate this for each process.
            Random r = new Random();
            int arrivalTime = r.Next(0, k);

            return arrivalTime;
        }
    }
}








