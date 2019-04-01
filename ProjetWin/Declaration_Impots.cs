using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjetWin
{
    class Declaration_Impots : Document
    {
        //constructor
        public Declaration_Impots() : base() { }
        public Declaration_Impots(string id, DateTime dateDuDocument, string path) : base(id, dateDuDocument, path) { }
    }
}
