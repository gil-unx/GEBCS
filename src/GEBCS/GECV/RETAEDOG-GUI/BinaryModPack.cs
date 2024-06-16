using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace RETAEDOG_GUI
{
    [Serializable]
    [XmlRoot]
    public class BinaryModPack
    {

        [XmlElement]
        public string name;
        [XmlElement]
        public string description;
        [XmlElement]
        public string author;
        [XmlElement]
        public string website;
        [XmlElement]
        public int version;

        [XmlElement]
        public string game;

        [XmlElement]
        public DateTime buildtime;



        [XmlIgnore]
        public Dictionary<string, Dictionary<string, byte[]>> qpckmap;

        [XmlIgnore]
        public Dictionary<string, byte[]> filemap;



        private BinaryModPack() { }

        


        public static BinaryModPack GetModInfo(string xmlpath)
        {

            using(FileStream fs = new FileStream(xmlpath, FileMode.Open, FileAccess.Read))
            {
                XmlSerializer serializer = new XmlSerializer(typeof(BinaryModPack));
                return serializer.Deserialize(fs) as BinaryModPack;
            }

        }


        public static void CreateEmptyModInfo(string xmlpath)
        {

            BinaryModPack binaryModPack = new BinaryModPack();

            binaryModPack.name = "Your Mod Name";
            binaryModPack.description = "Your Description";
            binaryModPack.author = "Modder Name";
            binaryModPack.website = "If you want or blank (https:).";
            binaryModPack.version = 0;
            binaryModPack.game = "GE2RB.exe(Input Game EXE File Name)";
            binaryModPack.buildtime = DateTime.Now;


            using (FileStream ms = new FileStream(xmlpath, FileMode.Create))
            {


                using (XmlWriter xw = XmlWriter.Create(ms))
                {
                    xw.Settings.Encoding = Encoding.UTF8;
                    XmlSerializer serializer = new XmlSerializer(typeof(BinaryModPack));
                    serializer.Serialize(ms, binaryModPack);
                }


            }


        }



        

        public string GetModDataInformation()
        {
            StringBuilder sb = new StringBuilder();


            
            foreach(var i in qpckmap)
            {
                sb.Append($"[");
                sb.Append(i.Key);
                sb.Append("::");
                sb.Append(i.Value.Count);
                sb.Append("]\n");
            }

            foreach(var i in filemap)
            {
                sb.Append(i.Key);
                sb.Append('\n');
            }


            return sb.ToString();
        }

    }
}
