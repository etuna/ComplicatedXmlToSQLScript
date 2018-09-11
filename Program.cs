using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace ComplicatedXmlToSql
{
    class Program
    {

        //Variables
        static XmlDocument doc;
        static ArrayList items;
        static XmlElement root;
        static ArrayList[] parametersAndValues; //Array of arraylists
        static string path = "C:/Users/etuna/source/repos/XmlToSql/settings.xml";
        static int numOfParameter = 0;
        static Hashtable hashtable;
        static ArrayList itemValArrayList;
        static ArrayList parameters;
        static ArrayList nodes;
        //----------------------------------------------------------------------
        static void Main(string[] args)
        {
            Init(path);
            if (getItems())
            {
                Console.WriteLine("It got items");
            }

            Process();
            GetDistinctParameters();
            generateDBCommands();
        }



        public static void Init(string mpath)
        {
            doc = new XmlDocument();
            items = new ArrayList();
            nodes = new ArrayList();
            if (File.Exists(mpath))
            {
                doc.Load(mpath);
            }
            else
            {
                throw new FileNotFoundException();
            }
        }

        public static bool getItems()
        {
            root = doc.DocumentElement; // Root Node

            if (root == null) // Empty file
            {
                return false;
            }


            foreach (XmlNode child in root.ChildNodes)
            {
                if (child.NodeType == XmlNodeType.Comment)
                {
                    continue;
                }

                items.Add(child);
            }

            return true;
        }

        public static void Process()
        {
            foreach (XmlNode item in items)
            {
                string itemName = item.Name;
                Node m = new Node(itemName);
                if (item.Attributes != null)
                {
                    foreach (XmlAttribute a in item.Attributes)
                    {
                        string attName = a.Name;
                        string attVal = a.Value.ToString();

                        KeyValuePair<string, string> pair = new KeyValuePair<string, string>(attName, attVal);
                        m.attributes.Add(pair);
                    }
                }


                if (item.ChildNodes != null)
                {
                    foreach (XmlNode child in item.ChildNodes)
                    {
                        if (child.NodeType != XmlNodeType.Comment)
                        {
                            string childName = child.Name;
                            string childVal = child.InnerText.ToString();

                            KeyValuePair<string, string> pair = new KeyValuePair<string, string>(childName, childVal);
                            m.children.Add(pair);
                        }
                    }
                }
                nodes.Add(m);
            }
        }


        public static void GetDistinctParameters()
        {

            parameters = new ArrayList();
            foreach (Node item in nodes)
            {
                if (item.attributes.Count != 0)
                {
                    foreach (KeyValuePair<string, string> pair in item.attributes)
                    {
                        string par = pair.Key;
                        if (!parameters.Contains(par))
                        {
                            parameters.Add(par);
                        }
                    }
                }

                if (item.children.Count != 0)
                {
                    foreach (KeyValuePair<string, string> pair in item.children)
                    {
                        string par = pair.Key;
                        if (!parameters.Contains(par))
                        {
                            parameters.Add(par);
                        }
                    }
                }
            }
        }


        public static void generateDBCommands()
        {
            

            string sqlCommand = "";

            sqlCommand += "CREATE TABLE " + root.Name + " (";
            foreach (string par in parameters)
            {
                sqlCommand += par + " VARCHAR(50),";
            }
            sqlCommand = sqlCommand.Substring(0, sqlCommand.Length - 2);
            sqlCommand += ");$";



            foreach (Node n in nodes)
            {
                syncParameters(n);
                int index = 0;
                sqlCommand += "INSERT INTO " + root.Name + " VALUES(";
                
                foreach(KeyValuePair<string, string> pair in n.allParameters)
                {
                    string val = pair.Value;
                    sqlCommand += "\"" + val + "\", ";
                }


                sqlCommand = sqlCommand.Substring(0, sqlCommand.Length - 3);
                sqlCommand += "\");$";




            }
            sqlCommand = sqlCommand.Replace("$", "" + System.Environment.NewLine);
            using (System.IO.StreamWriter file = new System.IO.StreamWriter("C:/Users/etuna/Desktop/scriptOutput.txt"))
            {
                file.WriteLine(sqlCommand);
               // Console.WriteLine(sqlCommand);
                //Console.WriteLine("Complete! sqlOutput.txt has been generated.");
                Console.ReadKey();
            }


        }


        public static bool ItHasParameter(Node n, string parameter)
        {
            
            foreach(KeyValuePair<string, string > pair in n.attributes)
            {
                string key = pair.Key;
                if (parameter.Equals(key))
                {
                    return true;
                }
            }
            foreach(KeyValuePair<string, string> pair in n.children)
            {
                string key = pair.Key;
                if(parameter.Equals(key))
                {
                    return true;
                }
            }
            return false;

        }


        public static void syncParameters(Node node)
        {
            foreach(string par in parameters)
            {
                bool added = false;
                if (ItHasParameter(node, par))
                {
                    foreach(KeyValuePair<string, string> pair in node.attributes)
                    {
                        string key = pair.Key;
                        if (key.Equals(par))
                        {
                            KeyValuePair<string, string> mpair = new KeyValuePair<string, string>(par, pair.Value);
                            node.allParameters.Add(mpair);
                            added = true;
                        }
                    }

                    foreach(KeyValuePair<string, string> pair in node.children)
                    {
                        string key = pair.Key;
                        if (key.Equals(par))

                        {
                            KeyValuePair<string, string> mpair = new KeyValuePair<string, string>(par, pair.Value);
                            node.allParameters.Add(mpair);
                            added = true;
                        }
                    }

                   
                }else
                {
                    if (!added)
                    {
                        KeyValuePair<string, string> mpair = new KeyValuePair<string, string>(par, "null");
                        node.allParameters.Add(mpair);
                    }
                }
            }



        }

    }
}
