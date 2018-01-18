using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _OLC1_Proyecto2_201403877.Analizador
{
    class Ambito
    {
        // PARA MANEJAR AMBITOS
        public String ambito;
        public String padre; 
        public List<Tupla> tabla = new List<Tupla>();

        //PARA MANEJAR SUB AMBITOS
        public int actual = 0;
        public bool hay = false;
        public List<Ambito> pila = new List<Ambito>();

        //RETORNO DE FUNCION
        public object retorno = null;
        public String tipo = "";

        public void setear() {
            tabla.Clear();
            actual = 0;
            hay = false;
            pila.Clear();
            retorno = null;
            tipo = "";
        }
    }
}
