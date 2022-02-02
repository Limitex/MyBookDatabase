using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace MyBookDatabase
{
    public class NationalDietLibrarySearchAPI
    {
        private XmlDocument xmlDocument;
        private XmlNamespaceManager xmlNsManager;

        public NationalDietLibrarySearchAPI()
        {
            xmlDocument = new();
            xmlNsManager = new(xmlDocument.NameTable);

            xmlNsManager.AddNamespace("root", "http://www.loc.gov/zing/srw/");
            xmlNsManager.AddNamespace("rdf", "http://www.w3.org/1999/02/22-rdf-syntax-ns#");
            xmlNsManager.AddNamespace("rdfs", "http://www.w3.org/2000/01/rdf-schema#");
            xmlNsManager.AddNamespace("dc", "http://purl.org/dc/elements/1.1/");
            xmlNsManager.AddNamespace("dcterms", "http://purl.org/dc/terms/");
            xmlNsManager.AddNamespace("dcndl", "http://ndl.go.jp/dcndl/terms/");
            xmlNsManager.AddNamespace("foaf", "http://xmlns.com/foaf/0.1/");
            xmlNsManager.AddNamespace("owl", "http://www.w3.org/2002/07/owl#");
        }

        private string ApiUri(long ISBN) => $"https://iss.ndl.go.jp/api/sru?operation=searchRetrieve&version=1.2&recordSchema=dcndl&onlyBib=true&recordPacking=xml&query=isbn=\"{ISBN}\" AND dpid=iss-ndl-opac";

        public BookDataFormat? GetData(string ISBN)
        {
            if (!long.TryParse(ISBN, out long isbm) || ISBN.Length != 13) return null;
            XmlNode? nodeBuffer;
            xmlDocument.Load(ApiUri(isbm));

            if (xmlDocument.SelectSingleNode(
                "/root:searchRetrieveResponse/root:numberOfRecords", 
                xmlNsManager)?.InnerText == "0") return null;
            
            var recordData = xmlDocument.SelectSingleNode(
                "/root:searchRetrieveResponse/root:records/root:record" +
                "/root:recordData/rdf:RDF/dcndl:BibResource", xmlNsManager);

            var r_ISBN = recordData?.SelectNodes("dcterms:identifier", xmlNsManager)?.Where(p =>
                p.SelectSingleNode("@rdf:datatype", xmlNsManager)?.InnerText.Contains("ISBN") ?? false).First().InnerText;

            var r_Title = recordData?.SelectSingleNode("dcterms:title", xmlNsManager)?.InnerText;

            nodeBuffer = recordData?.SelectSingleNode("dc:title/rdf:Description", xmlNsManager);
            var r_dc_Title = nodeBuffer?.SelectSingleNode("rdf:value", xmlNsManager)?.InnerText;
            var r_dc_Title_Trans = nodeBuffer?.SelectSingleNode("dcndl:transcription", xmlNsManager)?.InnerText;

            nodeBuffer = recordData?.SelectSingleNode("dcndl:volume/rdf:Description", xmlNsManager);
            var r_Volume = nodeBuffer?.SelectSingleNode("rdf:value", xmlNsManager)?.InnerText;
            var r_Volume_Trans = nodeBuffer?.SelectSingleNode("dcndl:transcription", xmlNsManager)?.InnerText;

            nodeBuffer = recordData?.SelectSingleNode("dcndl:seriesTitle/rdf:Description", xmlNsManager);
            var r_Series_Title = nodeBuffer?.SelectSingleNode("rdf:value", xmlNsManager)?.InnerText;
            var r_Series_Title_Trans = nodeBuffer?.SelectSingleNode("dcndl:transcription", xmlNsManager)?.InnerText;

            var r_dcterms_Creator = new List<string>();
            var r_dcterms_Creator_Trans = new List<string>();
            recordData?.SelectNodes("dcterms:creator/foaf:Agent", xmlNsManager)?.Select(p => {
                var n = p?.SelectSingleNode("foaf:name", xmlNsManager)?.InnerText;
                var t = p?.SelectSingleNode("dcndl:transcription", xmlNsManager)?.InnerText;
                if (n != null) r_dcterms_Creator.Add(n);
                if (t != null) r_dcterms_Creator_Trans.Add(t); });

            var r_dc_Creator = new List<string>();
            recordData?.SelectNodes("dc:creator", xmlNsManager)?.Select(p => {
                var c = p?.InnerText;
                if (c != null) r_dc_Creator.Add(c); });

            var r_Publisher = new List<string>();
            var r_Publisher_Trans = new List<string>();
            recordData?.SelectNodes("dcterms:publisher/foaf:Agent", xmlNsManager)?.Select(p => {
                var n = p?.SelectSingleNode("foaf:name", xmlNsManager)?.InnerText;
                var t = p?.SelectSingleNode("dcndl:transcription", xmlNsManager)?.InnerText;
                if (n != null) r_Publisher.Add(n);
                if (t != null) r_Publisher_Trans.Add(t); });

            var r_Date = recordData?.SelectSingleNode("dcterms:date", xmlNsManager)?.InnerText;

            var r_Genre = recordData?.SelectSingleNode("dcndl:genre/rdf:Description/rdf:value", xmlNsManager)?.InnerText;

            var r_Extent = recordData?.SelectSingleNode("dcterms:extent", xmlNsManager)?.InnerText;

            var r_Price = recordData?.SelectSingleNode("dcndl:price", xmlNsManager)?.InnerText;

            return new BookDataFormat()
            {
                ISBN = r_ISBN,
                Title = r_Title,
                Title_Meta = r_dc_Title,
                Title_Trans_Meta = r_dc_Title_Trans,
                Volume = r_Volume,
                Volume_Trans = r_Volume_Trans,
                Series_Title = r_Series_Title,
                Series_Title_Trans = r_Series_Title_Trans,
                Creators = r_dcterms_Creator.ToArray(),
                Creators_Trans = r_dcterms_Creator_Trans.ToArray(),
                Creators_Meta = r_dc_Creator.ToArray(),
                Publishers = r_Publisher.ToArray(),
                Publishers_Trans = r_Publisher_Trans.ToArray(),
                Date = r_Date,
                Genre = r_Genre,
                Extent = r_Extent,
                Price = r_Price,
            };
        }

        public static void Show(BookDataFormat? boolData)
        {
            if (boolData == null) return;
            Console.WriteLine("ISBN                 : " + boolData?.ISBN);
            Console.WriteLine("Title                : " + boolData?.Title ?? "NULL");
            Console.WriteLine("Title (Meta)         : " + boolData?.Title_Meta ?? "NULL");
            Console.WriteLine("Title [Trans] (Meta) : " + boolData?.Title_Trans_Meta ?? "NULL");
            Console.WriteLine("Volume               : " + boolData?.Volume ?? "NULL");
            Console.WriteLine("Volume [Trans]       : " + boolData?.Volume_Trans ?? "NULL");
            Console.WriteLine("Series Title         : " + boolData?.Series_Title ?? "NULL");
            Console.WriteLine("Series Title [Trans] : " + boolData?.Series_Title_Trans ?? "NULL");
            Console.WriteLine("Creators             : " + string.Join(" / ", boolData?.Creators ?? new string[0]));
            Console.WriteLine("Creators [Trans]     : " + string.Join(" / ", boolData?.Creators_Trans ?? new string[0]));
            Console.WriteLine("Creators (Meta)      : " + string.Join(" / ", boolData?.Creators_Meta ?? new string[0]));
            Console.WriteLine("Publishers           : " + string.Join(" / ", boolData?.Publishers ?? new string[0]));
            Console.WriteLine("Publishers [Trans]   : " + string.Join(" / ", boolData?.Publishers_Trans ?? new string[0]));
            Console.WriteLine("Date                 : " + boolData?.Date ?? "NULL");
            Console.WriteLine("Genre                : " + boolData?.Genre ?? "NULL");
            Console.WriteLine("Extent               : " + boolData?.Extent ?? "NULL");
            Console.WriteLine("Price                : " + boolData?.Price ?? "NULL");
        }
    }

    public struct BookDataFormat
    {
        public string? ISBN;
        public string? Title;
        public string? Title_Meta;
        public string? Title_Trans_Meta;
        public string? Volume;
        public string? Volume_Trans;
        public string? Series_Title;
        public string? Series_Title_Trans;
        public string[]? Creators;
        public string[]? Creators_Trans;
        public string[]? Creators_Meta;
        public string[]? Publishers;
        public string[]? Publishers_Trans;
        public string? Date;
        public string? Genre;
        public string? Extent;
        public string? Price;
    }

    public static class Expansion
    {
        public static List<XmlNode> Where(this XmlNodeList xmlNodeList, Func<XmlNode, bool> func)
        {
            var nodes = new List<XmlNode>();
            for (var i = 0; i < xmlNodeList.Count; i++)
            {
                var node = xmlNodeList[i];
                if (node != null && func(node)) nodes.Add(node);
            }
            return nodes;
        }

        public static void Select(this XmlNodeList xmlNodeList, Action<XmlNode> func)
        {
            for (var i = 0; i < xmlNodeList.Count; i++)
            {
                var node = xmlNodeList[i];
                if(node != null) func(node);
            }
        }
    }
}
