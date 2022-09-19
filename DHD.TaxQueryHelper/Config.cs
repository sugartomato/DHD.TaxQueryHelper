using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;


namespace DHD.TaxQueryHelper
{
    internal class Config
    {
        private static XDocument? _doc = null;
        private static XElement? _root = null;
        private static String _configFilePath = ConfigFolderPath + "Config.xml";
        public static void Load()
        {
            try
            {
                if (System.IO.File.Exists(_configFilePath))
                {
                    _doc = XDocument.Load(_configFilePath);
                    if (_doc != null && _doc.Root != null)
                    {
                        _root = _doc.Root;
                    }
                }
                else
                {
                    _doc = new XDocument();
                    _root = new XElement("TaxQuery");
                    _root.Add(new XElement("FPDM"));
                    _doc.Add(_root);
                }
            }
            catch (Exception)
            {
            }
        }

        private static void Save()
        { 
            if(_doc != null)
                _doc.Save(_configFilePath);
        }

        /// <summary>
        /// 添加到书签
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public static Boolean AddPathBookmark(PathBookmark model)
        {
            if (_root == null) return false;
            XElement? pbs = _root.Element("PathBookmarks");
            if (pbs == null)
                pbs = new XElement("PathBookmarks");
            
            XElement newmark = new XElement("PathBookmark");
            newmark.Value = model.Path;
            newmark.SetAttributeValue("name", model.Name);

            pbs.Add(newmark);
            Save();
            return true;
        }

        /// <summary>
        /// 配置文件存储目录
        /// </summary>
        public static String ConfigFolderPath
        {
            get
            {
                String dir = $"{Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)}\\DHD\\DHD.TaxQueryHelper\\";
                if (!System.IO.Directory.Exists(dir))
                { 
                    System.IO.Directory.CreateDirectory(dir);
                }
                return dir;
            }
        }


        public static List<PathBookmark>? PathBookmarks {
            get
            {
                if (_root == null) return null;
                XElement? pbs = _root.Element("PathBookmarks");
                if (pbs != null && pbs.Elements().Count() > 0)
                {
                    List<PathBookmark> result = new List<PathBookmark>();
                    foreach (XElement e in pbs.Elements())
                    {
                        PathBookmark pb = new PathBookmark();
                        XAttribute? nameAtt = e.Attribute("name");
                        if (nameAtt != null) pb.Name = nameAtt.Value;
                        pb.Path = e.Value;
                        result.Add(pb);
                    }
                    return result;
                }
                return null;
            }
        }


        /// <summary>
        /// 添加到书签
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public static Boolean AddFPDM(String FPDM)
        {
            if (_root == null) return false;
            XElement? pbs = _root.Element("FPDM");
            if (pbs == null)
                pbs = new XElement("FPDM");

            XElement newmark = new XElement("DM");
            newmark.Value = FPDM;
            pbs.Add(newmark);
            Save();
            return true;
        }

        public static List<String>? FPDM
        {
            get
            {
                if (_root == null) return null;
                XElement? pbs = _root.Element("FPDM");
                if (pbs != null && pbs.Elements().Count() > 0)
                {
                    List<String> result = new List<String>();
                    foreach (XElement e in pbs.Elements())
                    {
                        result.Add(e.Value);
                    }
                    if (result != null)
                    {
                        result.Sort();
                    }
                    return result;
                }
                return null;
            }
        }


    }
}
