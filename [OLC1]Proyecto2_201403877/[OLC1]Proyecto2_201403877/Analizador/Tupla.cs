using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Irony.Ast;
using Irony.Parsing;

namespace _OLC1_Proyecto2_201403877.Analizador
{
    class Tupla
    {
        //PARA TABLA DE SIMBOLOS
        public String nombrre,tipo,rol,ambito;
        public ParseTreeNode root = null;
        public List<Tupla> parametros = new List<Tupla>();
        public List<int> dimension = new List<int>();
        public Object valor;
        public bool error = false;

        public bool aumento = false;//para el ++ y --    
    

    }
}
