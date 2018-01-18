using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Irony.Ast;
using Irony.Parsing;
using _OLC1_Proyecto2.controlDot;
using System.IO;
using System.Diagnostics;

namespace _OLC1_Proyecto2_201403877.Analizador
{
    class Sintactico:Grammar
    {        
        public static String archivo;
        public static List<Object> lista = new List<Object>();

        public static ParseTreeNode analizar(String cadena) {
            Gramatica gramatica = new Gramatica();
            LanguageData lenguaje = new LanguageData(gramatica);
            Parser parser = new Parser(lenguaje);
            ParseTree arbol = parser.Parse(cadena);
            ParseTreeNode raiz = arbol.Root;

            if (raiz == null)
            {
                for (int i = 0; i < arbol.ParserMessages.Count(); i++)
                {
                    List<String> nuevo = new List<String>();                    
                    if (arbol.ParserMessages.ElementAt(i).Message.Contains("Invalid"))
                    {
                        nuevo.Add("Lexico");
                    }
                    else
                    {
                        nuevo.Add("Sintactico");
                    }
                    String temp = arbol.ParserMessages.ElementAt(i).Message.Replace("Syntax error, expected:", "");
                    nuevo.Add(temp.Replace("Invalid character:", ""));
                    nuevo.Add(arbol.ParserMessages.ElementAt(i).Location.Line.ToString());
                    nuevo.Add(arbol.ParserMessages.ElementAt(i).Location.Column.ToString());
                    nuevo.Add(archivo);
                    lista.Add(nuevo);
                }                
            }
            else
            {
                for (int i = 0; i < arbol.ParserMessages.Count(); i++)
                {
                    List<String> nuevo = new List<String>();                    
                    if (arbol.ParserMessages.ElementAt(i).Message.Contains("Invalid"))
                    {
                        nuevo.Add("Lexico");
                    }
                    else
                    {
                        nuevo.Add("Sintactico");
                    }
                    String temp = arbol.ParserMessages.ElementAt(i).Message.Replace("Syntax error, expected:", "");
                    nuevo.Add(temp.Replace("Invalid character:", ""));
                    nuevo.Add(arbol.ParserMessages.ElementAt(i).Location.Line.ToString());
                    nuevo.Add(arbol.ParserMessages.ElementAt(i).Location.Column.ToString());
                    nuevo.Add(archivo);
                    lista.Add(nuevo);
                }                
            }

            //if (lista.Count()==0)
               //generarImagen(raiz);

            return raiz;            
        }

        private static void generarImagen(ParseTreeNode raiz) {
            String grafoDOT = ControlDot.getDOT(raiz);
            File.Create("Grafica.dot").Dispose();
            TextWriter tw = new StreamWriter("Grafica.dot");
            tw.WriteLine(grafoDOT);
            tw.Close();

            //System.IO.File.Delete("grafo.png");

            //EJECUTAR GRAPHVIZ
            ProcessStartInfo startinfo = new ProcessStartInfo("C:\\Program Files (x86)\\Graphviz2.38\\bin\\dot.exe");
            Process Process;
            startinfo.RedirectStandardOutput = true;
            startinfo.UseShellExecute = false;
            startinfo.CreateNoWindow = true;
            startinfo.Arguments = "-Tsvg Grafica.dot -o grafo.svg";
            //"-Tpng Grafica.dot -o grafo.png";
            Process = Process.Start(startinfo);
            Process.Close();            
            
        }


    }
}
