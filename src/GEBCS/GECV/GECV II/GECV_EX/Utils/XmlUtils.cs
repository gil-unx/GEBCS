using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;

namespace GECV_EX.Utils
{
    public static class XmlUtils
    {

        public static string Save<T>(T obj)
        {

            using(StringWriter  sw = new StringWriter())
            {

                XmlSerializer xmlSerializer = new XmlSerializer(typeof(T));

                XmlWriterSettings xmlWriterSettings = new XmlWriterSettings() { Encoding = Encoding.Unicode, CheckCharacters = false };



                var writer = XmlWriter.Create(sw,xmlWriterSettings);
                

                xmlSerializer.Serialize(writer,obj);
                return sw.ToString();
            }

        }

        public static T Load<T>(StreamReader sr)
        {

            XmlSerializer xmlSerializer = new XmlSerializer (typeof(T));

            XmlReaderSettings xmlReaderSettings = new XmlReaderSettings() { CheckCharacters = false };

            var reader = XmlReader.Create(sr, xmlReaderSettings);

             return (T)xmlSerializer.Deserialize(reader);


        }

        public static T Load<T>(string path)
        {
            using (FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read))
            {
                using (StreamReader sr = new StreamReader(fs))
                {
                    return  XmlUtils.Load<T>(sr);
                }

            }
        }

    }
}
