using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;

namespace BGLogPlugin
{
    public static class FileHelper
    {
        private static string CheckFilename(string name = "FileHelper.txt")
        {
            int i = 1;
            string dir = Path.GetDirectoryName(name);
            string file = Path.GetFileNameWithoutExtension(name) + "{0}";
            string extension = Path.GetExtension(name);

            while (File.Exists(name))
                name = Path.Combine(dir, string.Format(file, "(" + i++ + ")") + extension);

            return name;
        }


        #region Write
        public static void Write<T>(string strPath, T value) where T : class, new()
        {
            try
            {
                if (File.Exists(strPath))
                    File.Delete(strPath);

                using (StreamWriter writer = new StreamWriter(strPath))
                    new XmlSerializer(typeof(T)).Serialize(writer, value);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception occured while writing to file '{strPath}': {ex.Message}");
            }
        }

        public static void Write(string strPath, List<string> values)
        {
            try
            {
                string fullpath = CheckFilename(strPath);
                File.WriteAllLines(fullpath, values);
            }
            catch(Exception ex)
            {
                Console.WriteLine($"Exception occured while writing to file '{strPath}': {ex.Message}");
            }
        }
        #endregion

        #region Read
        public static T Read<T>(string strPath) where T : class, new()
        {
            T retVal = null;

            try
            {
                if (File.Exists(strPath))
                {
                    using (StreamReader reader = new StreamReader(strPath))
                        retVal = (T)new XmlSerializer(typeof(T)).Deserialize(reader);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception occured while reading from file '{strPath}': {ex.Message}");
            }

            if (retVal == null)
                retVal = new T();

            return retVal;
        }
        #endregion
    }
}
