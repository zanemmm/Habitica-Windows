using System;
using System.Collections.Generic;
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
        public string[] NextDue { get; set; }

        public static string TypeToString(TaskType type)
        {
            switch (type)
            {
                case TaskType.Habits:
                    return "habits";
                case TaskType.Dailys:
                    return "dailys";
                case TaskType.Todos:
                    return "todos";
                case TaskType.Rewards:
                    return "rewards";
                case TaskType.CompletedTodos:
                    return "completedTodos";
            }
            return string.Empty;
        }
    }

    enum TaskType
    {
        Habits,
        Dailys,
        Todos,
        Rewards,
        CompletedTodos,
    }
}
