using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace GECV_EX_TR2_Editor_GUI
{
    [Serializable]
    [XmlRoot]
    public class TR2Excel
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public string Type { get; set; }

        public int Index { get; set; }

        public int Column {  get; set; }

        public string Value {  get; set; }

        public string Hex { get; set; }

        public int Import {  get; set; }




        public string GetImportLog()
        {
            StringBuilder sb = new StringBuilder();

            sb.Append($"{Id},{Name},{Type},{Index},{Column},{Value},{Hex},{Import}|");
            //sb.Append('\n');

            switch (Import)
            {
                case 0:
                    sb.Append($"No Import.");
                    break;
                case 1:
                    sb.Append($"Import Value:{Value}");
                    break;
                case 2:
                    sb.Append($"Import Hex:{Hex}");
                    break;
                default:
                    sb.Append($"Error Import Type:{Import}, Data Should be 1,2,3. 1=Import Text,2=Import Value,3=Import Hex.");
                    break;
            }


            return sb.ToString();


        }


    }
}
