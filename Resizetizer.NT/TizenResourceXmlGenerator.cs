using System.Collections.Generic;
using System.IO;
using System.Xml;

namespace Resizetizer
{
    internal class TizenResourceXmlGenerator
    {
        private static readonly IDictionary<string, string> resolutionMap = new Dictionary<string, string>
        {
            { "LDPI", "from 0 to 240" },
            { "MDPI", "from 241 to 300" },
            { "HDPI", "from 301 to 380" },
            { "XHDPI", "from 381 to 480" },
            { "XXHDPI", "from 481 to 600" },
        };

        public TizenResourceXmlGenerator(string intermediateOutputPath, ILogger logger)
        {
            Logger = logger;
            IntermediateOutputPath = intermediateOutputPath;
        }

        public string IntermediateOutputPath { get; private set; }

        public ILogger Logger { get; private set; }

        public void Generate()
        {
            XmlDocument doc = new XmlDocument();
            XmlNode docNode = doc.CreateXmlDeclaration("1.0", "UTF-8", "yes");
            doc.AppendChild(docNode);

            XmlNode rootNode = doc.CreateElement("res", "http://tizen.org/ns/rm");
            doc.AppendChild(rootNode);

            XmlElement groupImageNode = doc.CreateElement("group-image", "http://tizen.org/ns/rm");
            groupImageNode.SetAttribute("folder", "contents");
            rootNode.AppendChild(groupImageNode);

            XmlElement groupLayoutNode = doc.CreateElement("group-layout", "http://tizen.org/ns/rm");
            groupLayoutNode.SetAttribute("folder", "contents");
            rootNode.AppendChild(groupLayoutNode);

            XmlElement groupSoundNode = doc.CreateElement("group-sound", "http://tizen.org/ns/rm");
            groupSoundNode.SetAttribute("folder", "contents");
            rootNode.AppendChild(groupSoundNode);

            XmlElement groupBinNode = doc.CreateElement("group-bin", "http://tizen.org/ns/rm");
            groupBinNode.SetAttribute("folder", "contents");
            rootNode.AppendChild(groupBinNode);

            string outputResourceDir = Path.Combine(IntermediateOutputPath, "res");
            string outputContentsDir = Path.Combine(outputResourceDir, "contents");

            var contentsDirInfo = new DirectoryInfo(outputContentsDir);
            if (!contentsDirInfo.Exists)
            {
                Logger.Log("No 'res/contents/' directory to generate res.xml.");
                return;
            }
            foreach (DirectoryInfo subDir in contentsDirInfo.GetDirectories())
            {
                if (subDir.Name.Contains("-"))
                {
                    var resolution = subDir.Name.Split('-')[1];
                    if (resolutionMap.TryGetValue(resolution, out string dpiRange))
                    {
                        foreach (XmlNode groupNode in rootNode)
                        {
                            XmlElement node = doc.CreateElement("node", "http://tizen.org/ns/rm");
                            node.SetAttribute("folder", $"contents/{ subDir.Name }");
                            node.SetAttribute("screen-dpi-range", dpiRange);
                            groupNode.AppendChild(node);

                            Logger.Log($"Add { subDir.Name } to { groupNode.Name }");
                        }
                    }
                }
            }
            doc.Save(Path.Combine(outputResourceDir, "res.xml"));
            Logger.Log($"res.xml file has been saved in { outputResourceDir }");
        }
    }
}
