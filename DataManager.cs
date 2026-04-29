using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using static PharmaCare.Reminder;

namespace PharmaCare
{
    //  File Handling — Atomic Write
    public static class DataManager
    {
        private static readonly string BaseDir =
            AppDomain.CurrentDomain.BaseDirectory;

        public static readonly string PatientsFile =
            Path.Combine(BaseDir, "patients.json");

        public static List<Patient> LoadPatients()
        {
            var list = new List<Patient>();
            if (!File.Exists(PatientsFile)) return list;

            try
            {
                string json = File.ReadAllText(PatientsFile, System.Text.Encoding.UTF8);
                var arr = JArray.Parse(json);
                foreach (JObject d in arr)
                {
                    if (d["type"]?.ToString() == "pediatric")
                        list.Add(PediatricPatient.FromDict(d));
                    else
                        list.Add(Patient.FromDict(d));
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Load error: {ex.Message}");
            }
            return list;
        }

        public static void SavePatients(List<Patient> patients)
        {
            var arr = new JArray();
            foreach (var p in patients)
                arr.Add(p.ToDict());

            string tempFile = PatientsFile + ".tmp";
            try
            {
                File.WriteAllText(tempFile,
                    arr.ToString(Formatting.Indented),
                    System.Text.Encoding.UTF8);

                // Atomic move
                if (File.Exists(PatientsFile))
                    File.Delete(PatientsFile);
                File.Move(tempFile, PatientsFile);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Save error: {ex.Message}");
                if (File.Exists(tempFile))
                    File.Delete(tempFile);
            }
        }
    }
}