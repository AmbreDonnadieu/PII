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
        protected DateTime dateDuDocument;

        //constructors
        public Document()
        {
            id = null;
            dateDuDocument = new DateTime(0, 0, 0);
        }
        public Document(string id, DateTime dateDuDocument)
        {
            this.id = id;
            this.dateDuDocument = dateDuDocument;
        }

        //properties
        public string Id
        {
            get { return id; }
        }
        public DateTime DateDuDocument
        {
            get { return dateDuDocument; }
        }
    }
}
