using System.Collections.Generic;

namespace GEBCS
{
   
    public class Record
    {
        public List<bool> ?UpdatePointer {get;set;}
        public string ?Location {get;set;}
        public int Offset {get;set;}
        public int ShiftOffset {get;set;}
        public int Size {get;set;}
        public int RealSize {get;set;}
        public int MaxSize { get; set; }
        public int OffsetName {get;set;}
        public int ChunkName {get;set;}
        public List<string> ?ElementName {get;set;}
        public string ?FileName {get;set;}
        public bool Compression {get;set;}
    }

    public class Pres 
    {
        public string ?Filename {get;set;}
        public uint Magic {get;set;}
        public int GrupOffset {get;set;}
        public int GrupCount {get;set;}
        public uint CeckSum {get;set;}
        public int TocSize {get;set;}
        public int TotalFile {get;set;}
        public List<int> ?Grups {get;set;}
        public List<Record> ?Files {get;set;}
        
        
    }
}