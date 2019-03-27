using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjetWin
{
    class Fiche_de_Paie : Document
    {
        private string employeur;

        //constructors
        public Fiche_de_Paie():base()
        {
            employeur = null;
        }
        public Fiche_de_Paie(string id, DateTime dateDuDocument, string employeur) : base(id, dateDuDocument)
        {
            this.employeur = employeur;
        }

        //property
        public string Employeur
        {
            get { return employeur; }
            set { employeur = value; }
        }
    }
}
