using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ComplicatedXmlToSql
{
    class Node
    {
        public ArrayList attributes;
        public ArrayList children;
        public ArrayList allParameters;
        public string name;


        public Node(string name)
        {
            attributes = new ArrayList(); // stores key-val pairs
            children = new ArrayList(); // stores key-val pairs
            allParameters = new ArrayList();
            this.name = name;
        }
    }
}
