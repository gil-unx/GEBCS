using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using System.Xml;
using System.Diagnostics;
using DotNetGraph.Core;
using DotNetGraph.Extensions;
using static GECV.Log;
namespace GERDP
{
    public class DGMLWriter
    {

        public static Dictionary<string,DotGraph> dotGraphMap = new Dictionary<string,DotGraph>();

        public struct Graph
        {
            public Node[] Nodes;
            public Link[] Links;
        }

        public struct Node
        {
            [XmlAttribute]
            public string Id;
            [XmlAttribute]
            public string Label;

            public Node(string id, string label)
            {
                this.Id = id;
                this.Label = label;
            }
        }

        public struct Link
        {
            [XmlAttribute]
            public string Source;
            [XmlAttribute]
            public string Target;
            [XmlAttribute]
            public string Label;
            [XmlAttribute]
            public string Category;


            public Link(string source, string target, string label, string category)
            {
                this.Source = source;
                this.Target = target;
                this.Label = label;
                this.Category = category;
            }



        }

        public HashSet<Node> Nodes { get; protected set; }
        public HashSet<Link> Links { get; protected set; }

        public DGMLWriter()
        {
            Nodes = new HashSet<Node>();
            Links = new HashSet<Link>();
        }

        public void AddNode(Node n)
        {
            lock (Nodes)
            {

                this.Nodes.Add(n);
            }
        }

        public void AddLink(Link l)
        {
            lock (Links)
            {
                this.Links.Add(l);
            }
        }

        public void Serialize(string xmlpath)
        {
            Graph g = new Graph();
            g.Nodes = this.Nodes.ToArray();
            g.Links = this.Links.ToArray();

            if (File.Exists(xmlpath))
            {
                File.Delete(xmlpath);
            }

            XmlRootAttribute root = new XmlRootAttribute("DirectedGraph");
            root.Namespace = "http://schemas.microsoft.com/vs/2009/dgml";
            XmlSerializer serializer = new XmlSerializer(typeof(Graph), root);
            XmlWriterSettings settings = new XmlWriterSettings();
            settings.Indent = true;
            using (XmlWriter xmlWriter = XmlWriter.Create(xmlpath, settings))
            {
                serializer.Serialize(xmlWriter, g);
            }
        }



        public void BuildDotGraph(DotGraph dot)
        {

            //foreach(var n in this.Nodes)
            //{

            //    var node = new DotNode().WithIdentifier(n.Id).WithShape(DotNodeShape.Box).WithLabel(n.Label).WithFillColor(DotColor.White).WithFontColor(DotColor.Black).WithStyle(DotNodeStyle.Solid);

            //    dot.Add(node);



            //}

            
            
            

            foreach (var n in this.Links)
            {
                var nodeA = new DotNode().WithIdentifier(n.Source).WithShape(DotNodeShape.Box).WithLabel(n.Source).WithFillColor(DotColor.Pink).WithFontColor(DotColor.Black).WithStyle(DotNodeStyle.Bold).WithWidth(2.5)
    .WithHeight(0.5)
    .WithPenWidth(1.5); ;
                var nodeB = new DotNode().WithIdentifier(n.Target).WithShape(DotNodeShape.Circle).WithLabel(n.Target).WithFillColor(DotColor.Aqua).WithFontColor(DotColor.Black).WithStyle(DotNodeStyle.Solid).WithWidth(3.5)
    .WithHeight(0.5)
    .WithPenWidth(1.5); ;


                

                var edge = new DotEdge()
    .From(nodeA)
    .To(nodeB)
    .WithArrowHead(DotEdgeArrowType.Box)
    .WithArrowTail(DotEdgeArrowType.Diamond)
    .WithColor(DotColor.Red)
    .WithFontColor(DotColor.Black)
    .WithLabel(n.Label)
    .WithStyle(DotEdgeStyle.Dashed).WithPenWidth(1);


                dot.Add(nodeA);
                dot.Add(nodeB);
                dot.Add(edge);
                Info($"对全局DOT集合添加:{nodeA.Identifier.ToString()}与{nodeB.Identifier.ToString()}，他们的联系方式是{edge.Label.ToString()}。");
                if (!dotGraphMap.ContainsKey(n.Label))
                {
                    dotGraphMap[n.Label] = new DotGraph().WithIdentifier(dot.Label +"_"+  n.Label);
                }
                dotGraphMap[n.Label].Add(nodeA);
                dotGraphMap[n.Label].Add(nodeB);
                dotGraphMap[n.Label].Add(edge);

                Info($"对{n.Label}DOT集合添加:{nodeA.Identifier.ToString()}与{nodeB.Identifier.ToString()}，他们的联系方式是{edge.Label.ToString()}。");

            }

        }

    }
}
