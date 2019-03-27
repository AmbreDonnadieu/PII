using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjetWin
{
    class Feuille_de_Soins : Document
    {
        private string delivrePar; //personne physique ou morale ayant délivré la feuille de soins
        private string soinsConcernes; //facultatif

        //constructors
        public Feuille_de_Soins():base()
        {
            delivrePar = null;
            soinsConcernes = null;
        }
        public Feuille_de_Soins(string id, DateTime dateDuDocument, string delivrePar, string soinsConcernes) : base(id, dateDuDocument)
        {
            this.delivrePar = delivrePar;
            this.soinsConcernes = soinsConcernes;
        }
        public Feuille_de_Soins(string id, DateTime dateDuDocument, string delivrePar) : base(id, dateDuDocument)
        {
            this.delivrePar = delivrePar;
            soinsConcernes = null;
        }

        //properties
        public string DelivrePar
        {
            get { return delivrePar; }
            set { delivrePar = value; }
        }
        public string SoinsConcernes
        {
            get { return soinsConcernes; }
            set { soinsConcernes = value; }
        }
    }
}