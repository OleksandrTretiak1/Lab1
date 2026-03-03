using System;
using System.Collections.Generic;
using System.Threading;
using System.Numerics;

namespace ParallelPLab1
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            Console.Title = "Лабораторна робота №1 - Багатопотоковість";

            while (true)
            {
                int stepValue;
                while (true)
                {
                    Console.WriteLine("\n=== Новий розрахунок ===");
                    Console.Write("Вкажіть крок послідовності (ціле число > 0): ");
                    if (int.TryParse(Console.ReadLine(), out stepValue) && stepValue > 0) break;
                    Console.WriteLine("Помилка: потрібно ввести додатне ціле число.");
                }

                int[] timeLimits;
                while (true)
                {
                    Console.WriteLine("Вкажіть час роботи для кожного потоку в секундах (через пробіл):");
                    string input = Console.ReadLine();

                    if (string.IsNullOrWhiteSpace(input)) continue;

                    string[] parts = input.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    timeLimits = new int[parts.Length];
                    bool success = true;

                    for (int i = 0; i < parts.Length; i++)
                    {
                        if (!int.TryParse(parts[i], out timeLimits[i]) || timeLimits[i] <= 0)
                        {
                            success = false;
                            break;
                        }
                    }

                    if (success) break;
                    Console.WriteLine("Помилка: всі значення часу мають бути цілими числами більше нуля.");
                }

                int totalThreads = timeLimits.Length;
                SequenceCalculator[] calculators = new SequenceCalculator[totalThreads];
                Thread[] workers = new Thread[totalThreads];
                Thread[] timers = new Thread[totalThreads];

                for (int i = 0; i < totalThreads; i++)
                {
                    int currentLimit = timeLimits[i];
                    calculators[i] = new SequenceCalculator(i + 1, stepValue, currentLimit);

                    workers[i] = new Thread(calculators[i].ExecuteMath);

                    int index = i;
                    timers[i] = new Thread(() =>
                    {
                        Thread.Sleep(currentLimit * 1000);
                        calculators[index].TriggerStop();
                    });
                }

                for (int i = 0; i < totalThreads; i++)
                {
                    workers[i].Start();
                    timers[i].Start();
                }

                for (int i = 0; i < totalThreads; i++)
                {
                    workers[i].Join();
                }

                Console.WriteLine("\n[OK] Усі потоки завершили обчислення. Очікування наступного вводу...");
            }
        }
    }

    class SequenceCalculator
    {
        private readonly int _id;
        private readonly int _step;
        private readonly int _timeLimit;
        private volatile bool _isRunning = true;

        public SequenceCalculator(int id, int step, int limit)
        {
            _id = id;
            _step = step;
            _timeLimit = limit;
        }

        public void TriggerStop() => _isRunning = false;

        public void ExecuteMath()
        {
            BigInteger totalSum = 0;
            BigInteger iterations = 0;

            while (_isRunning)
            {
                totalSum += _step;
                iterations++;
            }

            Console.WriteLine($"[Потік №{_id}] Сума: {totalSum} | Крок: {_step} | Кількість доданків: {iterations} | Час: {_timeLimit} сек.");
        }
    }
}