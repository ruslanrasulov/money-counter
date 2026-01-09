using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace MoneyCounter
{
    internal sealed class Program
    {
        private static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine("Path is not entered");

                return;
            }

            try
            {
                var path = args[0];
                var str = File.ReadAllText(args[0]);

                var monthExpenses = ParseMonth(str);
                var folder = Path.GetDirectoryName(path);
                var fileName = Path.GetFileNameWithoutExtension(path);
                var updatedFilePath = Path.Combine(folder, $"{fileName} (updated).txt");

                File.WriteAllText(updatedFilePath, monthExpenses.ToString());

                Console.WriteLine($"Saved updated file: {updatedFilePath}");
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        private static MonthExpenses ParseMonth(string text)
        {
            var lines = text.Split('\n', StringSplitOptions.RemoveEmptyEntries);

            var monthExpenses = new MonthExpenses { Title = lines[0] };
            var dailyExpenses = new List<DayExpenses>();

            for (int i = 1; i < lines.Length; i++)
            {
                if (lines[i].StartsWith("##"))
                {
                    var day = int.Parse(lines[i].Substring(2).Trim());
                    var dayExpenses = new DayExpenses { Day = day };
                    var values = new List<(string, int)>();

                    for (int j = i + 1;  j < lines.Length; j++)
                    {
                        if (lines[j].StartsWith("##"))
                        {
                            i = j;
                            break;
                        }

                        var expense = lines[j].Split(':', StringSplitOptions.RemoveEmptyEntries);
                        if (expense.Length != 2)
                        {
                            throw new InvalidOperationException($"Invalid day {day}");
                        }

                        var expenseName = expense[0].Trim();
                        var expenseValueString = expense[1].Trim();

                        if (string.IsNullOrEmpty(expenseName) || string.IsNullOrEmpty(expenseValueString))
                        {
                            throw new InvalidOperationException($"Invalid values {expenseName} {expenseValueString}");
                        }

                        values.Add((expenseName, int.Parse(expenseValueString[..^1])));
                    }

                    dayExpenses.Values = values;
                    dailyExpenses.Add(dayExpenses);
                }
            }

            monthExpenses.Expenses = dailyExpenses;

            return monthExpenses;
        }
    }

    internal class MonthExpenses
    {
        public string Title { get; set; }

        public List<DayExpenses> Expenses { get; set; }

        public override string ToString()
        {
            var fullSum = Expenses.Sum(e => e.TotalAmount);

            return $"{Title.Trim()} ({fullSum}р)\n\n{string.Join("\n\n", Expenses.Select(e => e.ToString()))}";
        }
    }

    internal class DayExpenses
    {
        public int Day { get; set; }
        public List<(string Expense, int Amount)> Values { get; set; }
        public int TotalAmount => Values.Sum(v => v.Amount);

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.AppendFormat("##{0}\n", Day);

            foreach (var expense in Values)
            {
                sb.AppendFormat("{0}: {1}р\n", expense.Expense, expense.Amount);
            }

            sb.AppendFormat("##{0} ({1}р)", Day, TotalAmount);

            return sb.ToString();
        }
    }
}