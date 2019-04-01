using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjetWin
{
    class Document
    {
        protected string id;
        protected string path;
        protected DateTime dateDuDocument;    

        //constructors
        public Document()
        {
            id = null;
            path = null;
            dateDuDocument = new DateTime(894, 11, 5);
        }
        public Document(string id, DateTime dateDuDocument, string path)
        {
            this.id = id;
            this.path = path;
            this.dateDuDocument = dateDuDocument;
        }

        //properties
        public string Id
        {
            get { return id; }
            set { id = value; }
        }
        public DateTime DateDuDocument
        {
            get { return dateDuDocument; }
            set { dateDuDocument = value; }
        }
        public string Path
        {
            get { return path; }
            set { path = value; }
        }
    }
}
