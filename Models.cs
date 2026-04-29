using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace PharmaCare
{
    //  Enum — MealTime
    public enum MealTime
    {
        BeforeMeal,
        AfterMeal,
        WithMeal,
        AnyTime
    }

    public static class MealTimeExtensions
    {
        public static string ToDisplayString(this MealTime mt)
        {
            switch (mt)
            {
                case MealTime.BeforeMeal: return "Before Meal";
                case MealTime.AfterMeal: return "After Meal";
                case MealTime.WithMeal: return "With Meal";
                default: return "Any Time";
            }
        }

        public static MealTime FromDisplayString(string s)
        {
            switch (s)
            {
                case "Before Meal": return MealTime.BeforeMeal;
                case "After Meal": return MealTime.AfterMeal;
                case "With Meal": return MealTime.WithMeal;
                default: return MealTime.AnyTime;
            }
        }
    }

    //  Class — Reminder
    public class Reminder
    {
        private static int _nextId = 0;

        public string Id { get; set; }
        public string Medicine { get; set; }
        public string TimeStr { get; set; }
        public MealTime MealTime { get; set; }

        public Reminder(string medicine, string timeStr, MealTime mealTime, string id = null)
        {
            Medicine = medicine;
            TimeStr = timeStr;
            MealTime = mealTime;
            Id = id ?? $"rem_{_nextId++}";
        }

        public bool IsDueNow()
        {
            try
            {
                var parts = TimeStr.Split(':');
                int h = int.Parse(parts[0]);
                int m = int.Parse(parts[1]);
                var now = DateTime.Now;
                return now.Hour == h && now.Minute == m && now.Second < 10;
            }
            catch { return false; }
        }

        public JObject ToDict()
        {
            return new JObject
            {
                ["id"] = Id,
                ["medicine"] = Medicine,
                ["time_str"] = TimeStr,
                ["meal_time"] = MealTime.ToDisplayString()
            };
        }

        public static Reminder FromDict(JObject d)
        {
            var meal = MealTimeExtensions.FromDisplayString(d["meal_time"]?.ToString() ?? "Any Time");
            return new Reminder(
                d["medicine"]?.ToString() ?? "",
                d["time_str"]?.ToString() ?? "",
                meal,
                d["id"]?.ToString()
            );
        }
    }

    //  Class — Patient
    public class Patient
    {
        private static int _nextId = 0;

        public string Id { get; set; }
        public string Name { get; set; }
        public int Age { get; set; }
        public List<Reminder> Reminders { get; set; } = new List<Reminder>();

        public Patient(string name, int age, string id = null)
        {
            Name = name;
            Age = age;
            Id = id ?? $"pat_{_nextId++}";
        }

        public void AddReminder(Reminder r) => Reminders.Add(r);

        public virtual JObject ToDict()
        {
            var remArr = new JArray();
            foreach (var r in Reminders)
                remArr.Add(r.ToDict());

            return new JObject
            {
                ["type"] = "adult",
                ["id"] = Id,
                ["name"] = Name,
                ["age"] = Age,
                ["reminders"] = remArr
            };
        }

        public static Patient FromDict(JObject d)
        {
            var p = new Patient(
                d["name"]?.ToString() ?? "",
                d["age"] != null ? (int)d["age"] : 0,
                d["id"]?.ToString()
            );
            foreach (JObject r in d["reminders"] ?? new JArray())
                p.Reminders.Add(Reminder.FromDict(r));
            return p;
        }
    }

    //  Class — PediatricPatient (Inheritance)
    public class PediatricPatient : Patient
    {
        public string Guardian { get; set; }

        public PediatricPatient(string name, int age, string guardian, string id = null)
            : base(name, age, id)
        {
            Guardian = guardian;
        }

        public override JObject ToDict()
        {
            var d = base.ToDict();
            d["type"] = "pediatric";
            d["guardian"] = Guardian;
            return d;
        }

        public static new PediatricPatient FromDict(JObject d)
        {
            var p = new PediatricPatient(
                d["name"]?.ToString() ?? "",
                d["age"] != null ? (int)d["age"] : 0,
                d["guardian"]?.ToString() ?? "",
                d["id"]?.ToString()
            );
            foreach (JObject r in d["reminders"] ?? new JArray())
                p.Reminders.Add(Reminder.FromDict(r));
            return p;
        }
    }
}