using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.UI;

namespace Online_Ordering
{
    class Logger
    {

        private static string _filename;

        public static void SetFilename(string filename)
        {
            _filename = filename;
        }


        public static void WriteLine(string text)
        {
            try
            {
                using (StreamWriter sw = File.AppendText(_filename))
                {
                    LogMessageWithDate(text, sw);
                    sw.Close();
                }
            }
            catch (Exception ex)
            {
                
            }
           
        }

        public static void Write(string text)
        {
            try
            {
                using (StreamWriter sw = File.AppendText(_filename))
                {
                    LogMessage(text, sw);
                    sw.Close();
                }
            }
            catch (Exception ex)
            {
                
            }
          
        }

        private static void LogMessageWithDate(string text, TextWriter tw)
        {
            tw.WriteLine("{0} :: {1}", DateTime.Now.ToLongDateString(), DateTime.Now.ToLongTimeString());
            tw.WriteLine("\t::{0}", text);
            tw.Flush();
        }

        private static void LogMessage(string text, TextWriter tw)
        {
            tw.WriteLine("\t::{0}", text);
            tw.Flush();
        }
    }
}