using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Irony.Ast;
using Irony.Parsing;

namespace _OLC1_Proyecto2_201403877.Analizador
{
    class Grafica
    {
        String general = "";

        private String pRec(ParseTreeNode root) {
            String temp = ""; //temporal para nombre de nodo

            switch (root.Term.Name.ToString())
            {
                case "CUERPO_PROC":
                    /*
                        CUERPO_PROC.Rule = CLS_PROC
                            | EPS
                     */
                    temp = root.ChildNodes.ElementAt(0).ToString().Trim();
                    if (temp.Equals("CLS_PROC")) 
                        pRec(root.ChildNodes.ElementAt(0));
                    break;
                case "CLS_PROC":
                    /*
                        CLS_PROC.Rule = CLS_PROC + CLS_P
                            | CLS_P
                     */
                    if (root.ChildNodes.Count > 1){
                        pRec(root.ChildNodes.ElementAt(0));
                        pRec(root.ChildNodes.ElementAt(1));
                    }else 
                        pRec(root.ChildNodes.ElementAt(0));                    
                    break;
                case "CLS_P":
                    /*
                        CLS_P.Rule = SWT
                            | LL_PROC + tPtoComa    2
                            | IF
                            | WHL
                            | DO_WHL
                            | RPT_UNT
                            | FOR
                            | tVar + DECL + tPtoComa    3
                            | ASIG + tPtoComa       2
                            | GRAF
                            | OUT
                            | RETURN
                            | BRK
                            | CNT                
                            | MTRX
                     */
                    if (root.ChildNodes.Count > 2){
                        return pRec(root.ChildNodes.ElementAt(1));
                    }else
                        return pRec(root.ChildNodes.ElementAt(0));
                                        
            }
            return "";
        }
    }
}
