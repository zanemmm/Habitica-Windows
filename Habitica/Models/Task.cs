using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Habitica.Models
{
    class Task
    {
        public string Id { get; set; }
        public string Type { get; set; }
        public string Text { get; set; }
        public string[] Tags { get; set; }
        public string Date { get; set; }

        public SimpleTaskCard ToSimpleTaskCard(bool isShowDeadline)
        {
            DateTime deadline = DateTime.MinValue;
            if (Date != null && Date != string.Empty)
            {
                deadline = DateTime.Parse(Date);
            }

            SimpleTaskCard card = new SimpleTaskCard
            {
                Id = Id,
                Title = Text,
                Deadline = deadline,
                IsShowDeadline = isShowDeadline,
            };
            return card;
        }

        public static string TypeToString(TaskType type)
        {
            switch (type)
            {
                case TaskType.Habit:
                    return "habit";
                case TaskType.Daily:
                    return "daily";
                case TaskType.Todo:
                    return "todo";
                case TaskType.Reward:
                    return "reward";
                case TaskType.CompletedTodo:
                    return "completedTodo";
            }
            return string.Empty;
        }
    }

    enum TaskType
    {
        Habit,
        Daily,
        Todo,
        Reward,
        CompletedTodo,
    }
}
