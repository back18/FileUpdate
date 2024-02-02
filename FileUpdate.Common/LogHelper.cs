using QuanLib.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace FileUpdate.Common
{
    public static class LogHelper
    {
        public const string XML_CONFIG_FILE = "log4net.xml";
        public const string LOG_FILE = "update.log";

        public static void Load()
        {
            using FileStream xmlConfigStream = GetXmlConfigStream();
            LogManager.LoadInstance(new("[%date{HH:mm:ss}] [%t/%p] [%c]: %m%n", LOG_FILE, Encoding.UTF8, xmlConfigStream, false));
        }

        private static FileStream GetXmlConfigStream()
        {
            if (File.Exists(XML_CONFIG_FILE))
                return File.OpenRead(XML_CONFIG_FILE);

            XmlDocument xmlDocument = new();
            XmlDeclaration xmlDeclaration = xmlDocument.CreateXmlDeclaration("1.0", "utf-8", null);
            xmlDocument.AppendChild(xmlDeclaration);
            XmlElement xmlElement = xmlDocument.CreateElement("log4net");
            xmlDocument.AppendChild(xmlElement);

            FileStream fileStream = File.Create(XML_CONFIG_FILE);
            xmlDocument.Save(fileStream);
            return fileStream;
        }
    }
}
