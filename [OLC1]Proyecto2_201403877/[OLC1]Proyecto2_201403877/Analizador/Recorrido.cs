using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Irony.Ast;
using Irony.Parsing;
using System.Windows.Forms;

namespace _OLC1_Proyecto2_201403877.Analizador
{
    class Recorrido
    {

        public List<Tupla> tablaSimbolos = new List<Tupla>();
        public List<Object> lista = new List<Object>();
        public string arch = "";        

        public Tupla pasada_1(ParseTreeNode root) {
            String temp = ""; //temporal para nombre de nodo
            String[] valor;   //temporal split
            Tupla aux;//auxiliar retorno
            Tupla aux2;//auxiliar retorno
            List<String> nwnError = new List<String>();  //error

            switch (root.Term.Name.ToString()){
                case "S":
                    /*
                        S.Rule = CLS;
                     */
                    pasada_1(root.ChildNodes.ElementAt(0));
                    break;
                case "CLS":
                    /*
                        CLS.Rule = PRG + BGN + FIN;
                     */
                    aux = pasada_1(root.ChildNodes.ElementAt(0));
                    pasada_1(root.ChildNodes.ElementAt(1));
                    pasada_1(root.ChildNodes.ElementAt(2));

                    int n = tablaSimbolos.Count;
                    for (int i = 0; i < n; i++)                    
                        tablaSimbolos.ElementAt(i).ambito = aux.nombrre;                    
                    break;
                case "PRG":
                    /*
                        PRG.Rule = tProgram + tId + tPtoComa + PRG_CL;
                     */
                    pasada_1(root.ChildNodes.ElementAt(3));
                    valor = root.ChildNodes.ElementAt(1).ToString().Split(' ');                    
                    Tupla nuevo3 = new Tupla();
                    nuevo3.nombrre = valor[0];
                    return nuevo3;
                case "BGN":
                    /*
                        BGN.Rule = tBegin + CUERPO_PROC;
                     */
                    Tupla nuevaMain = new Tupla();
                    nuevaMain.rol = "main";
                    nuevaMain.root = root.ChildNodes.ElementAt(1);
                    tablaSimbolos.Add(nuevaMain);
                    break;
                case "FIN":
                    /*
                        FIN.Rule = tEnd + FMA;
                     */
                    pasada_1(root.ChildNodes.ElementAt(1));
                    break;
                case "FMA":
                    /*
                        FMA.Rule = FMB
                            | EPS
                            ;
                     */
                    temp = root.ChildNodes.ElementAt(0).ToString().Trim();
                    if (temp.Equals("FMB"))                    
                        pasada_1(root.ChildNodes.ElementAt(0));                    
                    break;
                case "FMB":
                    /*
                        FMB.Rule = FMB + FMC
                            | FMC
                            ;
                     */
                    if (root.ChildNodes.Count == 2){
                        pasada_1(root.ChildNodes.ElementAt(0));
                        pasada_1(root.ChildNodes.ElementAt(1));
                    } else
                        pasada_1(root.ChildNodes.ElementAt(0));                    
                    break;
                case "FMC":
                    /*
                        FMC.Rule = MET
                            | FUNC
                            ;
                     */
                    pasada_1(root.ChildNodes.ElementAt(0));                    
                    break;
                case "MET":
                    /*
                        MET.Rule = tProcedure + tId + tParA + PRE_LSTP + tParC + tPtoComa + tBegin + CUERPO_PROC + tEnd;
                     */
                    aux = pasada_1(root.ChildNodes.ElementAt(3));
                    if (aux.error != true){
                        aux.tipo = "VOID";
                        valor = root.ChildNodes.ElementAt(1).ToString().Split(' ');
                        aux.nombrre = valor[0];
                        aux.rol = "proc";
                        for (int i = 0; i < tablaSimbolos.Count; i++){                            
                            if (tablaSimbolos.ElementAt(i).nombrre == aux.nombrre){//tiene el mismo nombre
                                if (tablaSimbolos.ElementAt(i).rol == aux.rol){//es procedimiento                                    
                                    if (tablaSimbolos.ElementAt(i).parametros.Count == aux.parametros.Count){//mismo numero de parametros
                                        int noCoincide = 0;
                                        for (int j = 0; j < aux.parametros.Count; j++) {
                                            if (tablaSimbolos.ElementAt(i).parametros.ElementAt(j).tipo != aux.parametros.ElementAt(j).tipo)
                                                noCoincide++;
                                        }
                                        if (noCoincide == 0) {
                                            //ERRROR
                                            nwnError.Add("semantico");
                                            nwnError.Add("Ya se definio un metodo con el mismo nombre y tipos de parametros.");
                                            nwnError.Add(root.Span.Location.Line.ToString());
                                            nwnError.Add(root.Span.Location.Column.ToString());
                                            nwnError.Add(arch);
                                            lista.Add(nwnError);
                                            return null;
                                        }
                                    }
                                } else {
                                    nwnError.Add("semantico");
                                    nwnError.Add("Nombre de Metodo.Ya existe una variable definida con el mismo nombre.");
                                    nwnError.Add(root.Span.Location.Line.ToString());
                                    nwnError.Add(root.Span.Location.Column.ToString());
                                    nwnError.Add(arch);
                                    lista.Add(nwnError);
                                    return null;
                                }
                            }
                        }
                        aux.root = root.ChildNodes.ElementAt(7);
                        //Add ambito de parametros
                        for (int i = 0; i < aux.parametros.Count; i++)                        
                            aux.parametros.ElementAt(i).ambito = aux.nombrre;                        
                        tablaSimbolos.Add(aux);
                        return aux;
                    } else {
                    //ERROR
                        nwnError.Add("semantico");
                        nwnError.Add("Existen nombres duplicados en los parametros del metodo.");
                        nwnError.Add(root.Span.Location.Line.ToString());
                        nwnError.Add(root.Span.Location.Column.ToString());
                        nwnError.Add(arch);
                        lista.Add(nwnError);
                        return null;
                    }                    
                case "FUNC":
                    /*
                        FUNC.Rule = tProcedure + TIPO + tDosPtos + tId + tParA + PRE_LSTP + tParC + tPtoComa + tBegin + CUERPO_PROC + tEnd;
                     */
                    aux = pasada_1(root.ChildNodes.ElementAt(1));
                    aux2 = pasada_1(root.ChildNodes.ElementAt(5));
                    if(aux2.error!=true){
                        aux2.tipo = aux.tipo;
                        valor = root.ChildNodes.ElementAt(3).ToString().Split(' ');
                        aux2.nombrre = valor[0];
                        aux2.rol = "proc";
                        for (int i = 0; i < tablaSimbolos.Count; i++) {
                            if (tablaSimbolos.ElementAt(i).nombrre == aux2.nombrre) {//tiene el mismo nombre
                                if (tablaSimbolos.ElementAt(i).rol == aux2.rol){//es procedimiento
                                    if (tablaSimbolos.ElementAt(i).tipo == aux2.tipo){ //mismo tipo
                                        if (tablaSimbolos.ElementAt(i).parametros.Count == aux2.parametros.Count){//mismo numero de parametros
                                            int noCoincide = 0;
                                            for (int j = 0; j < aux2.parametros.Count; j++) {
                                                if (tablaSimbolos.ElementAt(i).parametros.ElementAt(j).tipo != aux2.parametros.ElementAt(j).tipo)                                                
                                                    noCoincide++;                                                
                                            }
                                            if (noCoincide == 0) {
                                                //ERRROR
                                                nwnError.Add("semantico");
                                                nwnError.Add("Ya se definio una funcion con el mismo nombre y tipos de parametros.");
                                                nwnError.Add(root.Span.Location.Line.ToString());
                                                nwnError.Add(root.Span.Location.Column.ToString());
                                                nwnError.Add(arch);
                                                lista.Add(nwnError);
                                                return null;
                                            }
                                        }
                                    }
                                } else {
                                    nwnError.Add("semantico");
                                    nwnError.Add("Nombre de funcion.Ya existe una variable definida con el mismo nombre.");
                                    nwnError.Add(root.Span.Location.Line.ToString());
                                    nwnError.Add(root.Span.Location.Column.ToString());
                                    nwnError.Add(arch);
                                    lista.Add(nwnError);
                                    return null;
                                }
                            }
                        }
                        aux2.root = root.ChildNodes.ElementAt(9);
                        //Add ambito de parametros
                        for (int i = 0; i < aux2.parametros.Count; i++)                        
                            aux2.parametros.ElementAt(i).ambito = aux2.nombrre;                        
                        tablaSimbolos.Add(aux2);
                        return aux2;
                    } else {
                        //ERROR
                        nwnError.Add("semantico");
                        nwnError.Add("Existen nombres duplicados en los parametros del metodo.");
                        nwnError.Add(root.Span.Location.Line.ToString());
                        nwnError.Add(root.Span.Location.Column.ToString());
                        nwnError.Add(arch);
                        lista.Add(nwnError);
                        return null;
                    }                    
                case "PRE_LSTP":
                    /*
                        PRE_LSTP.Rule = LST_PAR
                            | EPS
                            ;
                     */
                    temp = root.ChildNodes.ElementAt(0).ToString().Trim();
                    if (temp.Equals("LST_PAR"))                    
                        return pasada_1(root.ChildNodes.ElementAt(0));                    
                    else {
                        Tupla nuv1 = new Tupla();
                        return nuv1;
                    }
                case "LST_PAR":
                    /*
                        LST_PAR.Rule = LST_PAR + tComa + TIPO + tDosPtos + tId
                            | TIPO + tDosPtos + tId
                            ;
                     */
                    Tupla nwnn = new Tupla();
                    if(root.ChildNodes.Count==5){
                        aux = pasada_1(root.ChildNodes.ElementAt(0));                        
                        aux2 = pasada_1(root.ChildNodes.ElementAt(2));
                        valor = root.ChildNodes.ElementAt(4).ToString().Split(' ');
                        aux2.nombrre = valor[0];
                        aux2.rol = "var";
                        //VERIFICACION DE NOMBRE, SI YA EXISTIERA EL NOMBRE DENTRO DE LOS PARAMETROS
                        for (int i = 0; i < aux.parametros.Count; i++) {
                            if(aux.parametros.ElementAt(i).nombrre==valor[0]){
                                aux.error = true;                                
                                break;
                            }
                        }
                        aux.parametros.Add(aux2);
                        return aux;
                    }else{
                        valor = root.ChildNodes.ElementAt(2).ToString().Split(' ');
                        aux = pasada_1(root.ChildNodes.ElementAt(0));
                        aux.nombrre = valor[0];
                        aux.rol = "var";                        
                        nwnn.parametros.Add(aux);
                        return nwnn;
                    }      
             

                case "PRG_CL":
                    /*
                        PRG_CL.Rule = PRG_S
                            | EPS
                            ;
                     */
                    temp = root.ChildNodes.ElementAt(0).ToString().Trim();
                    if (temp.Equals("PRG_S"))                    
                        pasada_1(root.ChildNodes.ElementAt(0));                    
                    break;
                case "PRG_S":
                    /*
                        PRG_S.Rule = PRG_S + PCLS
                            | PCLS
                            ;
                     */
                    if (root.ChildNodes.Count() == 2) {
                        pasada_1(root.ChildNodes.ElementAt(0));
                        pasada_1(root.ChildNodes.ElementAt(1));
                    }
                    else
                        pasada_1(root.ChildNodes.ElementAt(0));                    
                    break;
                case "PCLS":
                    /*
                        PCLS.Rule = IMPORT
                            | tVar + DECL + tPtoComa
                            | MTRX
                            ;
                     */
                    if (root.ChildNodes.Count() == 3)                    
                        pasada_1(root.ChildNodes.ElementAt(1));                    
                    else
                        pasada_1(root.ChildNodes.ElementAt(0));                    
                    break;
                case "IMPORT":
                    /*
                        IMPORT.Rule = tUses + tId + tPtoComa;
                     */                    
                    Tupla nuevo2 = new Tupla();
                    valor = root.ChildNodes.ElementAt(1).ToString().Split(' ');                   
                    nuevo2.nombrre = valor[0];
                    nuevo2.rol = "import";
                    tablaSimbolos.Add(nuevo2);
                    break;
                case "DECL":
                    /*
                        DECL.Rule = DECL + tComa + tId
                            | TIPO + tDosPtos + tId
                            ;
                     */
                    aux = pasada_1(root.ChildNodes.ElementAt(0));                            
                    //agregar valor por defecto                                                        
                    switch(aux.tipo) {
                        case "INT":
                            aux.valor = 0;
                            break;
                        case "DOUBLE":
                            aux.valor = 0.0;
                            break;
                        case "STRING":
                            aux.valor = "";
                            break;
                        case "CHAR":
                            aux.valor = ' ';
                            break;
                        case "BOOL":
                            aux.valor = false;
                            break;
                    }
                    //agregar id y rol
                    valor = root.ChildNodes.ElementAt(2).ToString().Split(' ');
                    aux.nombrre = valor[0];
                    aux.rol = "var";
                    //buscar id en tabla de simbolos
                    for (int i  = 0; i  < tablaSimbolos.Count; i ++) {
                        if (tablaSimbolos.ElementAt(i).nombrre == valor[0]) { 
                                //ERROR
                                nwnError.Add("semantico");
                                nwnError.Add("Variable. Ya existe un elemento definido con el mismo nombre.");
                                nwnError.Add(root.Span.Location.Line.ToString());
                                nwnError.Add(root.Span.Location.Column.ToString());
                                nwnError.Add(arch);
                                lista.Add(nwnError);
                                return null;
                            }
                    }                    
                    //agregar a tabla de simbolos
                    tablaSimbolos.Add(aux);
                    //retornar el tipo, para que este disponible
                    Tupla nuevo1 = new Tupla();
                    nuevo1.tipo = aux.tipo;
                    return nuevo1;       
             
                case "TIPO":
                    /*
                        TIPO.Rule = tInt
                            | tDouble
                            | tString
                            | tChar
                            | tBool
                            ;
                     */
                    Tupla nuevo = new Tupla();                    
                    valor = root.ChildNodes.ElementAt(0).ToString().Split(' ');                   
                    nuevo.tipo = valor[0];
                    return nuevo; 
                   
                case "MTRX":
                    /*
                        MTRX.Rule = tVar + tId + tOf + TIPO + LST_DM + tPtoComa;
                     */
                    aux = pasada_1(root.ChildNodes.ElementAt(3));//tipo
                    aux2 = pasada_1(root.ChildNodes.ElementAt(4));//dimensiones
                    if (aux2.error == false) {
                        valor = root.ChildNodes.ElementAt(1).ToString().Split(' ');                        
                        for (int i = 0; i < tablaSimbolos.Count; i++) {
                            if (tablaSimbolos.ElementAt(i).nombrre == valor[0]) { 
                                //ERROR
                                nwnError.Add("semantico");
                                nwnError.Add("Arreglo. Ya existe un elemento definido con el mismo nombre.");
                                nwnError.Add(root.Span.Location.Line.ToString());
                                nwnError.Add(root.Span.Location.Column.ToString());
                                nwnError.Add(arch);
                                lista.Add(nwnError);
                                return null;
                            }
                        }
                        Tupla nwn = new Tupla();
                        nwn.nombrre = valor[0];
                        nwn.rol = "var";
                        nwn.dimension = aux2.dimension;
                        nwn.tipo = aux.tipo;
                        ////INICIALIZAR
                        //Mtrx valIni = null;
                        //switch (aux.tipo) {
                        //    case "INT":
                        //        aux.valor = 0;
                        //        break;
                        //    case "DOUBLE":
                        //        aux.valor = 0.0;
                        //        break;
                        //    case "STRING":
                        //        aux.valor = "";
                        //        break;
                        //    case "CHAR":
                        //        aux.valor = ' ';
                        //        break;
                        //    case "BOOL":
                        //        aux.valor = false;
                        //        break;
                        //}                        
                        //if (nwn.dimension.Count == 1)                                                    
                        //    valIni = arr(aux.valor, nwn.dimension.ElementAt(0), 0);
                        //else {
                        //    object auxiliar = aux.valor;
                        //    for (int i = nwn.dimension.Count-1; i >= 0; i--) {                                
                        //        /*if (i==nwn.dimension.Count-1)                                
                        //            valIni = arr(aux.valor, nwn.dimension.ElementAt(i), nwn.dimension.ElementAt(i - 1));    
                        //        else
                        //            valIni = arr(valIni, nwn.dimension.ElementAt(i), nwn.dimension.ElementAt(i - 1)); */
                        //        Mtrx nx = new Mtrx();
                        //        for (int j = 0; j < nwn.dimension.ElementAt(i); j++)
                        //        {
                        //            nx.elem.Add(auxiliar);
                        //        }
                        //        auxiliar = nx;
                        //    }
                        //    valIni = (Mtrx)auxiliar;
                        //}
                        //nwn.valor = valIni;                                                

                        if (nwn.dimension.Count == 1)
                        {
                            switch (nwn.tipo)
                            {
                                case "STRING":
                                    string[] arreglo = new string[nwn.dimension.ElementAt(0)];
                                    nwn.valor = arreglo;
                                    break;
                                case "INT":
                                    int[] arreglo1 = new int[nwn.dimension.ElementAt(0)];
                                    nwn.valor = arreglo1;
                                    break;
                                case "DOUBLE":
                                    double[] arreglo2 = new double[nwn.dimension.ElementAt(0)];
                                    nwn.valor = arreglo2;
                                    break;
                                case "CHAR":
                                    char[] arreglo3 = new char[nwn.dimension.ElementAt(0)];
                                    nwn.valor = arreglo3;
                                    break;
                                case "BOOL":
                                    bool[] arreglo4 = new bool[nwn.dimension.ElementAt(0)];
                                    nwn.valor = arreglo4;
                                    break;
                            }
                        }
                        else if (nwn.dimension.Count == 2)
                        {
                            switch (nwn.tipo)
                            {
                                case "STRING":
                                    string[,] arreglo = new string[nwn.dimension.ElementAt(0), nwn.dimension.ElementAt(1)];
                                    nwn.valor = arreglo;
                                    break;
                                case "INT":
                                    int[,] arreglo1 = new int[nwn.dimension.ElementAt(0), nwn.dimension.ElementAt(1)];
                                    nwn.valor = arreglo1;
                                    break;
                                case "DOUBLE":
                                    double[,] arreglo2 = new double[nwn.dimension.ElementAt(0), nwn.dimension.ElementAt(1)];
                                    nwn.valor = arreglo2;
                                    break;
                                case "CHAR":
                                    char[,] arreglo3 = new char[nwn.dimension.ElementAt(0), nwn.dimension.ElementAt(1)];
                                    nwn.valor = arreglo3;
                                    break;
                                case "BOOL":
                                    bool[,] arreglo4 = new bool[nwn.dimension.ElementAt(0), nwn.dimension.ElementAt(1)];
                                    nwn.valor = arreglo4;
                                    break;
                            }
                        }
                        else if (nwn.dimension.Count == 3)
                        {
                            switch (nwn.tipo)
                            {
                                case "STRING":
                                    string[, ,] arreglo = new string[nwn.dimension.ElementAt(0), nwn.dimension.ElementAt(1), nwn.dimension.ElementAt(2)];
                                    nwn.valor = arreglo;
                                    break;
                                case "INT":
                                    int[, ,] arreglo1 = new int[nwn.dimension.ElementAt(0), nwn.dimension.ElementAt(1), nwn.dimension.ElementAt(2)];
                                    nwn.valor = arreglo1;
                                    break;
                                case "DOUBLE":
                                    double[, ,] arreglo2 = new double[nwn.dimension.ElementAt(0), nwn.dimension.ElementAt(1), nwn.dimension.ElementAt(2)];
                                    nwn.valor = arreglo2;
                                    break;
                                case "CHAR":
                                    char[, ,] arreglo3 = new char[nwn.dimension.ElementAt(0), nwn.dimension.ElementAt(1), nwn.dimension.ElementAt(2)];
                                    nwn.valor = arreglo3;
                                    break;
                                case "BOOL":
                                    bool[, ,] arreglo4 = new bool[nwn.dimension.ElementAt(0), nwn.dimension.ElementAt(1), nwn.dimension.ElementAt(2)];
                                    nwn.valor = arreglo4;
                                    break;
                            }
                        }
                        else if (nwn.dimension.Count == 4)
                        {
                            switch (nwn.tipo)
                            {
                                case "STRING":
                                    string[, , ,] arreglo = new string[nwn.dimension.ElementAt(0), nwn.dimension.ElementAt(1), nwn.dimension.ElementAt(2), nwn.dimension.ElementAt(3)];
                                    nwn.valor = arreglo;
                                    break;
                                case "INT":
                                    int[, , ,] arreglo1 = new int[nwn.dimension.ElementAt(0), nwn.dimension.ElementAt(1), nwn.dimension.ElementAt(2), nwn.dimension.ElementAt(3)];
                                    nwn.valor = arreglo1;
                                    break;
                                case "DOUBLE":
                                    double[, , ,] arreglo2 = new double[nwn.dimension.ElementAt(0), nwn.dimension.ElementAt(1), nwn.dimension.ElementAt(2), nwn.dimension.ElementAt(3)];
                                    nwn.valor = arreglo2;
                                    break;
                                case "CHAR":
                                    char[, , ,] arreglo3 = new char[nwn.dimension.ElementAt(0), nwn.dimension.ElementAt(1), nwn.dimension.ElementAt(2), nwn.dimension.ElementAt(3)];
                                    nwn.valor = arreglo3;
                                    break;
                                case "BOOL":
                                    bool[, , ,] arreglo4 = new bool[nwn.dimension.ElementAt(0), nwn.dimension.ElementAt(1), nwn.dimension.ElementAt(2), nwn.dimension.ElementAt(3)];
                                    nwn.valor = arreglo4;
                                    break;
                            }
                        }


                        tablaSimbolos.Add(nwn);
                        return nwn;
                    }
                    break;

                case "LST_DM":
                    /*
                        LST_DM.Rule = MakePlusRule(LST_DM,EXP);
                     */
                    //crear tupla y add dimension en cada iteracion de for y retornar
                    Tupla nuevaP = new Tupla();
                    int nn = root.ChildNodes.Count;
                    for (int i = 0; i < nn; i++) {
                        aux = pasada_1(root.ChildNodes.ElementAt(i));
                        if (aux.error != true) {
                            if (aux.tipo == "INT") {
                                if (Convert.ToInt32(aux.valor) > 0)
                                    nuevaP.dimension.Add(Convert.ToInt32(aux.valor));
                                else {
                                    nuevaP.error = true;
                                    nwnError.Add("semantico");
                                    nwnError.Add("Arreglo. Se intento definir un tamaño de dimension inferior a 1.");
                                    nwnError.Add(root.Span.Location.Line.ToString());
                                    nwnError.Add(root.Span.Location.Column.ToString());
                                    nwnError.Add(arch);
                                    lista.Add(nwnError);
                                }
                            } else {
                                //ERROR
                                nuevaP.error = true;
                                nwnError.Add("semantico");
                                nwnError.Add("Arreglo. Tipo de dato invalido para dimension.");
                                nwnError.Add(root.Span.Location.Line.ToString());
                                nwnError.Add(root.Span.Location.Column.ToString());
                                nwnError.Add(arch);
                                lista.Add(nwnError);
                            }
                        } else
                            nuevaP.error = true;                                                
                    }                    
                    return nuevaP;
                case "EXP":                    
                    if (root.ChildNodes.Count() == 1) {
                        /*
                            | ART
                         */
                        return pasada_1(root.ChildNodes.ElementAt(0));
                    }
                    else if (root.ChildNodes.Count() == 2) {
                        /*
                            | tNot + EXP
                         */
                        aux = pasada_1(root.ChildNodes.ElementAt(1));                        
                        if (aux.tipo == "BOOL") {
                            if (Convert.ToBoolean(aux.valor) == true)                                
                                aux.valor = false;                                
                            else                                
                                aux.valor = true;                                
                        } else if (aux.tipo == "INT") {
                            if (Convert.ToInt32(aux.valor) == 0)                                
                                aux.valor = true;                                
                            else if (Convert.ToInt32(aux.valor) == 1)                                
                                aux.valor = false;                                
                            else {
                                //ERROR
                                if (aux.error != true) {
                                    nwnError.Add("semantico");
                                    nwnError.Add("No se puede negar un valor de tipo INT excepto 0 y 1");
                                    nwnError.Add(root.Span.Location.Line.ToString());
                                    nwnError.Add(root.Span.Location.Column.ToString());
                                    nwnError.Add(arch);
                                    lista.Add(nwnError);
                                    aux.error = true;
                                }                                
                                aux.valor = false;
                            }
                            aux.tipo = "BOOL";
                        } else if (aux.tipo == "DOUBLE") {
                            double auxD = Convert.ToDouble(aux.valor);
                            if (auxD % 1 == 0) {
                                if (auxD == 0)                                    
                                    aux.valor = true;                                    
                                else if (auxD == 1)                                    
                                    aux.valor = false;                                    
                                else {
                                    //ERROR
                                    if (aux.error != true) {
                                        nwnError.Add("semantico");
                                        nwnError.Add("No se puede negar un valor de tipo DOUBLE excepto 0.0 y 1.0.");
                                        nwnError.Add(root.Span.Location.Line.ToString());
                                        nwnError.Add(root.Span.Location.Column.ToString());
                                        nwnError.Add(arch);
                                        lista.Add(nwnError);
                                        aux.error = true;
                                    }                                    
                                    aux.valor = false;
                                }
                            } else {
                                //ERROR
                                if (aux.error != true){
                                    nwnError.Add("semantico");
                                    nwnError.Add("No se puede negar un valor de tipo DOUBLE excepto 0.0 y 1.0.");
                                    nwnError.Add(root.Span.Location.Line.ToString());
                                    nwnError.Add(root.Span.Location.Column.ToString());
                                    nwnError.Add(arch);
                                    lista.Add(nwnError);
                                    aux.error = true;
                                }                                
                                aux.valor = false;
                            }
                            aux.tipo = "BOOL";
                        } else if (aux.tipo == "CHAR") {
                            if ((Int32)Convert.ToChar(aux.valor) == 0)
                                aux.valor = true;
                            else if ((Int32)Convert.ToChar(aux.valor) == 1)
                                aux.valor = false;
                            else {
                                //ERROR
                                if (aux.error != true) {
                                    nwnError.Add("semantico");
                                    nwnError.Add("No se puede negar un valor de tipo CHAR excepto el equivalente ascii de 0 y 1.");
                                    nwnError.Add(root.Span.Location.Line.ToString());
                                    nwnError.Add(root.Span.Location.Column.ToString());
                                    nwnError.Add(arch);
                                    lista.Add(nwnError);
                                    aux.error = true;
                                }                                
                                aux.valor = false;
                            }
                            aux.tipo = "BOOL";
                        } else {
                            //ERROR
                            if (aux.error != true) {
                                nwnError.Add("semantico");
                                nwnError.Add("No se puede negar elementos de tipo STRING");
                                nwnError.Add(root.Span.Location.Line.ToString());
                                nwnError.Add(root.Span.Location.Column.ToString());
                                nwnError.Add(arch);
                                lista.Add(nwnError);
                                aux.error = true;
                            }
                            aux.tipo = "BOOL";
                            aux.valor = false;
                        }                                                   
                        return aux;
                    } else {
                        /*
                                |  EXP + tOr + EXP
                                | EXP + tAnd + EXP
                                | ART + REL + ART                                                                                                                            
                         */
                        temp = root.ChildNodes.ElementAt(1).ToString().Trim();
                        if (temp.Equals("REL")) {
                            aux = pasada_1(root.ChildNodes.ElementAt(0));
                            String dato = Convert.ToString(pasada_1(root.ChildNodes.ElementAt(1)).valor);
                            aux2 = pasada_1(root.ChildNodes.ElementAt(2));
                            return logicoRel(aux, dato, aux2, root.Span.Location.Line, root.Span.Location.Column);
                        } else {
                            aux = pasada_1(root.ChildNodes.ElementAt(0));
                            aux2 = pasada_1(root.ChildNodes.ElementAt(2));
                            valor = root.ChildNodes.ElementAt(1).ToString().Split(' ');
                            return logicoRel(aux, valor[0], aux2, root.Span.Location.Line, root.Span.Location.Column);
                        }
                    }                    
                case "REL":
                    /*
                        REL.Rule = tMayorQ
                            | tMayorIQ
                            | tMenorQ
                            | tMenorIQ
                            | tIgualQ
                            | tDifQ
                            ;
                     */
                    valor = root.ChildNodes.ElementAt(0).ToString().Split(' ');
                    Tupla nn1 = new Tupla();
                    nn1.valor = valor[0];
                    return nn1;                    
                case "ART":                    
                    if (root.ChildNodes.Count() == 1) {
                        /*                                            
                            | tnumero                            
                            | tId
                            | tCadena
                            | tCaracter
                            | tVbool
                         */
                        valor = root.ChildNodes.ElementAt(0).ToString().Split(' ');
                        Tupla nuevo4 = new Tupla();
                        if (valor.Count() >= 2) {                                                        
                            switch (valor[valor.Count()-1]) {
                                case "(tNumero)":
                                    if (valor[0].Contains(".")) {
                                        nuevo4.tipo = "DOUBLE";
                                        nuevo4.valor = Convert.ToDouble(valor[0]);
                                    } else {
                                        nuevo4.tipo = "INT";
                                        nuevo4.valor = Convert.ToInt32(valor[0]);
                                    }                                    
                                    break;
                                case "(tId)":                                     
                                    for (int i = 0; i < tablaSimbolos.Count; i++) {
                                        if (tablaSimbolos.ElementAt(i).nombrre == valor[0] && tablaSimbolos.ElementAt(i).rol=="var") {
                                            nuevo4.valor = tablaSimbolos.ElementAt(i).valor;
                                            nuevo4.tipo = tablaSimbolos.ElementAt(i).tipo;
                                            return nuevo4;
                                        }   
                                    }
                                    //ERROR
                                    nwnError.Add("semantico");
                                    nwnError.Add("No existe ningun elemento con el nombre especificado.");
                                    nwnError.Add(root.Span.Location.Line.ToString());
                                    nwnError.Add(root.Span.Location.Column.ToString());
                                    nwnError.Add(arch);
                                    lista.Add(nwnError);
                                    //RECUPERACION PARA SEGUIR ANALIZANDO
                                    nuevo4.tipo = "INT";
                                    nuevo4.valor = 0;
                                    nuevo4.error = true;
                                    break;
                                case "(tCadena)":
                                    String txt = "";
                                    for (int i = 0; i < valor.Count()-1; i++) {
                                        if (i != valor.Count() - 2)
                                            txt += valor[i] + " ";
                                        else
                                            txt += valor[i];
                                    }
                                    nuevo4.tipo = "STRING";
                                    nuevo4.valor = txt;
                                    break;
                                case "(tCaracter)":
                                    Char[] arr = valor[0].ToCharArray();                                                                        
                                    Char caracter = arr[0];
                                    nuevo4.tipo = "CHAR";
                                    nuevo4.valor = caracter;
                                    break;
                                case "(tVbool)":
                                    if (valor[0]=="false")                                                                            
                                        nuevo4.valor = false;
                                    else                                        
                                        nuevo4.valor = true;                                    
                                    nuevo4.tipo = "BOOL";
                                    break;                                
                            }
                            return nuevo4;                          
                        } else {
                            /*
                                | ACC_MTRX
                             */
                            // si es acceso a matriz o llamada a metodo
                            return pasada_1(root.ChildNodes.ElementAt(0));    
                        }                        
                    }else if(root.ChildNodes.Count==2){
                        /*
                            | tMenos + ART
                         */
                        aux = pasada_1(root.ChildNodes.ElementAt(1));                        
                        switch (aux.tipo) {
                            case "INT":
                                aux.valor = -1 * Convert.ToInt32(aux.valor);
                                break;
                            case "DOUBLE":
                                aux.valor = -1 * Convert.ToDouble(aux.valor);
                                break;
                            case "CHAR":
                                aux.valor = -1 * Convert.ToInt32(aux.valor);
                                aux.tipo = "INT";                                    
                                break;
                            case "BOOL":
                                if (Convert.ToBoolean(aux.valor) == true)                                
                                    aux.valor = -1;                                                                        
                                else
                                    aux.valor = 0;                                                                        
                                aux.tipo = "INT";
                                break;
                            case "STRING":
                                //ERROR
                                nwnError.Add("semantico");
                                nwnError.Add("A un valor de tipo STRING no se le puede aplicar signo negativo.");
                                nwnError.Add(root.Span.Location.Line.ToString());
                                nwnError.Add(root.Span.Location.Column.ToString());
                                nwnError.Add(arch);
                                lista.Add(nwnError);
                                //RECUPERACION PARA SEGUIR ANALIZANDO
                                aux.valor = 0;
                                aux.tipo = "INT";
                                aux.error = true;
                                break;
                        }                                                    
                        return aux;                    
                    }else{                        
                        temp = root.ChildNodes.ElementAt(1).ToString().Trim();
                        if (temp.Equals("EXP")) {
                            /*
                                | tParA + EXP + tParC  
                             */
                           return pasada_1(root.ChildNodes.ElementAt(1));                            
                        } else {
                            /*
                                | ART + tMas + ART
                                | ART + tMenos + ART
                                | ART + tPor + ART
                                | ART + tDiv + ART                            
                                | ART + tPot + ART
                            */
                            aux = pasada_1(root.ChildNodes.ElementAt(0));
                            aux2 = pasada_1(root.ChildNodes.ElementAt(2));
                            valor = root.ChildNodes.ElementAt(1).ToString().Split(' ');

                            return aritmetico(aux, valor[0], aux2, root.Span.Location.Line, root.Span.Location.Column);                            
                        }                        
                    }                    
            }
            return null;
        }

        public Tupla aritmetico(Tupla val1, String op, Tupla val2,int fila,int columna) {
            Tupla nuevo = new Tupla();
            List<String> nwnError = new List<String>();  //error
            string detError = "";
            switch (op) {
                case "+":
                    switch (val1.tipo) {
		                case "INT":
                            switch (val2.tipo) {
                                case "INT":                                    
                                    nuevo.valor = Convert.ToInt32(val1.valor) + Convert.ToInt32(val2.valor);
                                    nuevo.tipo = "INT";
                                    break;
                                case "DOUBLE":
                                    nuevo.valor = Convert.ToInt32(val1.valor) + Convert.ToDouble(val2.valor);
                                    nuevo.tipo = "DOUBLE";
                                    break;
                                case "CHAR":
                                    nuevo.valor = Convert.ToInt32(val1.valor) + (Int32)Convert.ToChar(val2.valor);
                                    nuevo.tipo = "INT";
                                    break;
                                case "STRING":
                                    nuevo.valor = Convert.ToInt32(val1.valor).ToString() + Convert.ToString(val2.valor);
                                    nuevo.tipo = "STRING";
                                    break;
                                case "BOOL":
                                    bool auxf = Convert.ToBoolean(val2.valor);
                                    if (auxf==true)                                    
                                        nuevo.valor = Convert.ToInt32(val1.valor) + 1;                                                                            
                                    else 
                                        nuevo.valor = Convert.ToInt32(val1.valor) + 0;                                    
                                    nuevo.tipo = "INT";
                                    break;                               
	                        }
                            break;
                        case "DOUBLE":
                            switch (val2.tipo) {
                                case "INT":
                                    nuevo.valor = Convert.ToDouble(val1.valor) + Convert.ToInt32(val2.valor);
                                    nuevo.tipo = "DOUBLE";
                                    break;
                                case "DOUBLE":
                                    nuevo.valor = Convert.ToDouble(val1.valor) + Convert.ToDouble(val2.valor);
                                    nuevo.tipo = "DOUBLE";
                                    break;
                                case "CHAR":
                                    nuevo.valor = Convert.ToDouble(val1.valor) + (Int32)Convert.ToChar(val2.valor);
                                    nuevo.tipo = "DOUBLE";
                                    break;
                                case "STRING":
                                    nuevo.valor = Convert.ToDouble(val1.valor).ToString() + Convert.ToString(val2.valor);
                                    nuevo.tipo = "STRING";
                                    break;
                                case "BOOL":
                                    bool auxf = Convert.ToBoolean(val2.valor);
                                    if (auxf==true)                                    
                                        nuevo.valor = Convert.ToDouble(val1.valor) + 1;                                                                            
                                    else 
                                        nuevo.valor = Convert.ToDouble(val1.valor) + 0;                                    
                                    nuevo.tipo = "DOUBLE";
                                    break;
                            }
                            break;
                        case "CHAR":
                            switch (val2.tipo) {
                                case "INT":
                                    nuevo.valor = (Int32)Convert.ToChar(val1.valor) + Convert.ToInt32(val2.valor);
                                    nuevo.tipo = "INT";
                                    break;
                                case "DOUBLE":
                                    nuevo.valor = (Int32)Convert.ToChar(val1.valor) + Convert.ToDouble(val2.valor);
                                    nuevo.tipo = "DOUBLE";
                                    break;                                    
                                case "CHAR":
                                    //ERROR
                                    detError = "No es posible sumar dos valores tipo CHAR";                                    
                                    nuevo.valor = ' ';
                                    nuevo.tipo = "CHAR";
                                    break;
                                case "STRING":                                                                            
                                    nuevo.valor = Convert.ToString(val1.valor) + Convert.ToString(val2.valor);
                                    nuevo.tipo = "STRING";
                                    break;                                    
                                case "BOOL":
                                    //ERROR
                                    detError = "No es posible sumar un valor tipo CHAR con uno tipo BOOL";                                    
                                    nuevo.valor = ' ';
                                    nuevo.tipo = "CHAR";                                    
                                    break;
                            }
                            break;
                        case "STRING":
                            switch (val2.tipo)
                            {
                                case "INT":
                                    nuevo.valor = Convert.ToString(val1.valor) + Convert.ToInt32(val2.valor).ToString();
                                    nuevo.tipo = "STRING";                                    
                                    break;
                                case "DOUBLE":
                                    nuevo.valor = Convert.ToString(val1.valor) + Convert.ToDouble(val2.valor).ToString();
                                    nuevo.tipo = "STRING";                                    
                                    break;
                                case "CHAR":
                                    nuevo.valor = Convert.ToString(val1.valor) + Convert.ToString(val2.valor);
                                    nuevo.tipo = "STRING";
                                    break;                                        
                                case "STRING":
                                    nuevo.valor = Convert.ToString(val1.valor) + Convert.ToString(val2.valor);
                                    nuevo.tipo = "STRING";
                                    break;                                        
                                case "BOOL":
                                    //ERROR
                                    detError = "No es posible sumar un valor tipo STRING con uno tipo BOOL";                                    
                                    nuevo.valor = " ";
                                    nuevo.tipo = "STRING";
                                    break;
                            }
                            break;
                        case "BOOL":
                            switch (val2.tipo) {
                                case "INT":
                                    bool auxf = Convert.ToBoolean(val1.valor);
                                    if (auxf==true)                                    
                                        nuevo.valor = 1 + Convert.ToInt32(val2.valor);                                                                            
                                    else 
                                        nuevo.valor = 0 + Convert.ToInt32(val2.valor);                                    
                                    nuevo.tipo = "INT";
                                    break;
                                case "DOUBLE":
                                    bool auxf1 = Convert.ToBoolean(val1.valor);
                                    if (auxf1==true)                                    
                                        nuevo.valor = 1 + Convert.ToDouble(val2.valor);                                                                            
                                    else 
                                        nuevo.valor = 0 + Convert.ToDouble(val2.valor);                                    
                                    nuevo.tipo = "DOUBLE";
                                    break;
                                case "CHAR":
                                    //ERROR
                                    detError = "No es posible sumar un valor tipo BOOL con uno tipo CHAR";                                    
                                    nuevo.valor = false;
                                    nuevo.tipo = "BOOL";
                                    break;
                                case "STRING":
                                    //ERROR
                                    detError = "No es posible sumar un valor tipo BOOL con uno tipo STRING";                                    
                                    nuevo.valor = false;
                                    nuevo.tipo = "BOOL";
                                    break;
                                case "BOOL":
                                    bool auxfn = Convert.ToBoolean(val1.valor);
                                    bool auxfm = Convert.ToBoolean(val2.valor);
                                    if (auxfn == true)                                    
                                        nuevo.valor = true;                                    
                                    else {
                                        if (auxfm == true)                                        
                                            nuevo.valor = true;                                        
                                        else 
                                            nuevo.valor = false;                                                                            
                                    }
                                    nuevo.tipo = "BOOL";
                                    break;
                            }
                            break;
	                }
                    break;

                case "*":
                    switch (val1.tipo) {
                        case "INT":
                            switch (val2.tipo) {
                                case "INT":
                                    nuevo.valor = Convert.ToInt32(val1.valor) * Convert.ToInt32(val2.valor);
                                    nuevo.tipo = "INT";
                                    break;                                    
                                case "DOUBLE":
                                    nuevo.valor = Convert.ToInt32(val1.valor) * Convert.ToDouble(val2.valor);
                                    nuevo.tipo = "DOUBLE";
                                    break;
                                case "CHAR":
                                    nuevo.valor = Convert.ToInt32(val1.valor) * (Int32)Convert.ToChar(val2.valor);
                                    nuevo.tipo = "INT";
                                    break;
                                case "STRING":
                                    //ERROR
                                    detError = "No es posible multiplicar un valor tipo INT con uno tipo STRING";
                                    nuevo.valor = 0;
                                    nuevo.tipo = "INT";
                                    break;
                                case "BOOL":
                                    bool auxf = Convert.ToBoolean(val2.valor);
                                    if (auxf==true)                                    
                                        nuevo.valor = Convert.ToInt32(val1.valor) * 1;                                                                            
                                    else 
                                        nuevo.valor = Convert.ToInt32(val1.valor) * 0;                                    
                                    nuevo.tipo = "INT";
                                    break; 
                            }
                            break;
                        case "DOUBLE":
                            switch (val2.tipo) {
                                case "INT":
                                    nuevo.valor = Convert.ToDouble(val1.valor) * Convert.ToInt32(val2.valor);
                                    nuevo.tipo = "DOUBLE";
                                    break;
                                case "DOUBLE":
                                    nuevo.valor = Convert.ToDouble(val1.valor) * Convert.ToDouble(val2.valor);
                                    nuevo.tipo = "DOUBLE";
                                    break;
                                case "CHAR":
                                    nuevo.valor = Convert.ToDouble(val1.valor) * (Int32)Convert.ToChar(val2.valor);
                                    nuevo.tipo = "DOUBLE";
                                    break;
                                case "STRING":
                                    //ERROR
                                    detError = "No es posible multiplicar un valor tipo DOUBLE con uno tipo STRING";
                                    nuevo.valor = 0.0;
                                    nuevo.tipo = "DOUBLE";
                                    break;
                                case "BOOL":
                                    bool auxf = Convert.ToBoolean(val2.valor);
                                    if (auxf==true)                                    
                                        nuevo.valor = Convert.ToDouble(val1.valor) * 1;                                                                            
                                    else 
                                        nuevo.valor = Convert.ToDouble(val1.valor) * 0;                                    
                                    nuevo.tipo = "DOUBLE";
                                    break;
                            }
                            break;
                        case "CHAR":
                            switch (val2.tipo) {
                                case "INT":
                                    nuevo.valor = (Int32)Convert.ToChar(val1.valor) * Convert.ToInt32(val2.valor);
                                    nuevo.tipo = "INT";
                                    break;
                                case "DOUBLE":
                                    nuevo.valor = (Int32)Convert.ToChar(val1.valor) * Convert.ToDouble(val2.valor);
                                    nuevo.tipo = "DOUBLE";
                                    break;
                                case "CHAR":
                                    //ERROR
                                    detError = "No es posible multiplicar dos valores tipo CHAR";
                                    nuevo.valor = ' ';
                                    nuevo.tipo = "CHAR";
                                    break;
                                case "STRING":
                                    //ERROR
                                    detError = "No es posible multiplicar un valor tipo CHAR con uno tipo STRING";
                                    nuevo.valor = ' ';
                                    nuevo.tipo = "CHAR";
                                    break;
                                case "BOOL":
                                    //ERROR
                                    detError = "No es posible multiplicar un valor tipo CHAR con uno tipo BOOL";
                                    nuevo.valor = ' ';
                                    nuevo.tipo = "CHAR";
                                    break;
                            }
                            break;
                        case "STRING":                            
                            //ERROR                            
                            switch (val2.tipo) {
                                case "INT":
                                    if (val1.error != true && val2.error!=true)                                         
                                        detError = "No es posible multiplicar un valor tipo STRING con uno tipo INT";                                                                        
                                    break;
                                case "DOUBLE":
                                    if (val1.error != true && val2.error != true)
                                        detError = "No es posible multiplicar un valor tipo STRING con uno tipo DOUBLE";
                                    break;
                                case "CHAR":
                                    if (val1.error != true && val2.error != true)
                                        detError = "No es posible multiplicar un valor tipo STRING con uno tipo CHAR";
                                    break;
                                case "STRING":
                                    if (val1.error != true && val2.error != true)
                                        detError = "No es posible multiplicar dos valores tipo STRING";
                                    break;
                                case "BOOL":
                                    if (val1.error != true && val2.error != true)
                                        detError = "No es posible multiplicar un valor tipo STRING con uno tipo BOOL";
                                    break;
                            }                            
                            nuevo.valor = "";
                            nuevo.tipo = "STRING";
                            break;
                        case "BOOL":
                            switch (val2.tipo) {
                                case "INT":
                                    bool auxf = Convert.ToBoolean(val1.valor);
                                    if (auxf==true)                                    
                                        nuevo.valor = 1 * Convert.ToInt32(val2.valor);                                                                            
                                    else 
                                        nuevo.valor = 0 * Convert.ToInt32(val2.valor);                                    
                                    nuevo.tipo = "INT";
                                    break;                                    
                                case "DOUBLE":
                                    bool auxf1 = Convert.ToBoolean(val1.valor);
                                    if (auxf1==true)                                    
                                        nuevo.valor = 1 * Convert.ToDouble(val2.valor);                                                                            
                                    else 
                                        nuevo.valor = 0 * Convert.ToDouble(val2.valor);                                    
                                    nuevo.tipo = "DOUBLE";
                                    break;
                                case "CHAR":
                                    //ERROR
                                    detError = "No es posible multiplicar un valor tipo BOOL con uno tipo CHAR";
                                    nuevo.valor = false;
                                    nuevo.tipo = "BOOL";
                                    break;
                                case "STRING":
                                    //ERROR
                                    detError = "No es posible multiplicar un valor tipo BOOL con uno tipo STRING";
                                    nuevo.valor = false;
                                    nuevo.tipo = "BOOL";
                                    break;
                                case "BOOL":
                                    bool auxfn = Convert.ToBoolean(val1.valor);
                                    bool auxfm = Convert.ToBoolean(val2.valor);
                                    if (auxfn == true)
                                    {
                                        if (auxfm == true)                                        
                                            nuevo.valor = true;                                        
                                        else                                        
                                            nuevo.valor = false;                                                                             
                                    }
                                    else 
                                        nuevo.valor = false;                                                                           
                                    nuevo.tipo = "BOOL";
                                    break;
                            }
                            break;
                    }
                    break;

                case "-":
                    switch (val1.tipo) {
                        case "INT":
                            switch (val2.tipo) {
                                case "INT":
                                    nuevo.valor = Convert.ToInt32(val1.valor) - Convert.ToInt32(val2.valor);
                                    nuevo.tipo = "INT";
                                    break;
                                case "DOUBLE":
                                    nuevo.valor = Convert.ToInt32(val1.valor) - Convert.ToDouble(val2.valor);
                                    nuevo.tipo = "DOUBLE";
                                    break;
                                case "CHAR":
                                    nuevo.valor = Convert.ToInt32(val1.valor) - (Int32)Convert.ToChar(val2.valor);
                                    nuevo.tipo = "INT";
                                    break;
                                case "STRING":
                                    //ERROR
                                    detError = "No es posible restar un valor tipo INT con uno tipo STRING";
                                    nuevo.valor = 0;
                                    nuevo.tipo = "INT";
                                    break;
                                case "BOOL":
                                    bool auxf = Convert.ToBoolean(val2.valor);
                                    if (auxf==true)                                    
                                        nuevo.valor = Convert.ToInt32(val1.valor) - 1;                                                                            
                                    else 
                                        nuevo.valor = Convert.ToInt32(val1.valor) - 0;                                    
                                    nuevo.tipo = "INT";
                                    break;
                            }
                            break;
                        case "DOUBLE":
                            switch (val2.tipo) {
                                case "INT":
                                    nuevo.valor = Convert.ToDouble(val1.valor) - Convert.ToInt32(val2.valor) ;
                                    nuevo.tipo = "DOUBLE";
                                    break;
                                case "DOUBLE":
                                    nuevo.valor = Convert.ToDouble(val1.valor) - Convert.ToDouble(val2.valor);
                                    nuevo.tipo = "DOUBLE";
                                    break;
                                case "CHAR":
                                    nuevo.valor = Convert.ToDouble(val1.valor) - (Int32)Convert.ToChar(val2.valor);
                                    nuevo.tipo = "DOUBLE";
                                    break;
                                case "STRING":
                                    //ERROR
                                    detError = "No es posible restar un valor tipo DOUBLE con uno tipo STRING";
                                    nuevo.valor = 0.0;
                                    nuevo.tipo = "DOUBLE";
                                    break;
                                case "BOOL":
                                    bool auxf = Convert.ToBoolean(val2.valor);
                                    if (auxf==true)                                    
                                        nuevo.valor = Convert.ToDouble(val1.valor) - 1;                                                                            
                                    else 
                                        nuevo.valor = Convert.ToDouble(val1.valor) - 0;                                    
                                    nuevo.tipo = "DOUBLE";
                                    break;
                            }
                            break;
                        case "CHAR":
                            switch (val2.tipo) {
                                case "INT":
                                    nuevo.valor = (Int32)Convert.ToChar(val1.valor) - Convert.ToInt32(val2.valor);
                                    nuevo.tipo = "INT";
                                    break;
                                case "DOUBLE":
                                    nuevo.valor = (Int32)Convert.ToChar(val1.valor) - Convert.ToDouble(val2.valor);
                                    nuevo.tipo = "DOUBLE";
                                    break;
                                case "CHAR":
                                    //ERROR
                                    detError = "No es posible restar dos valores tipo CHAR";
                                    nuevo.valor = ' ';
                                    nuevo.tipo = "CHAR";
                                    break;
                                case "STRING":
                                    //ERROR
                                    detError = "No es posible restar un valor tipo CHAR con uno tipo STRING";
                                    nuevo.valor = ' ';
                                    nuevo.tipo = "CHAR";                                    
                                    break;
                                case "BOOL":
                                    //ERROR
                                    detError = "No es posible restar un valor tipo CHAR con uno tipo BOOL";
                                    nuevo.valor = ' ';
                                    nuevo.tipo = "CHAR";
                                    break;
                            }
                            break;
                        case "STRING":
                            //ERROR
                            switch (val2.tipo) {
                                case "INT":
                                    detError = "No es posible restar un valor tipo STRING con uno tipo INT";
                                    break;
                                case "DOUBLE":
                                    detError = "No es posible restar un valor tipo STRING con uno tipo DOUBLE";
                                    break;
                                case "CHAR":
                                    detError = "No es posible restar un valor tipo STRING con uno tipo CHAR";
                                    break;
                                case "STRING":
                                    detError = "No es posible restar dos valores tipo STRING";
                                    break;
                                case "BOOL":
                                    detError = "No es posible restar un valor tipo STRING con uno tipo BOOL";
                                    break;
                            }
                            nuevo.valor = "";
                            nuevo.tipo = "STRING";
                            break;
                        case "BOOL":
                            switch (val2.tipo) {
                                case "INT":
                                    bool auxf = Convert.ToBoolean(val1.valor);
                                    if (auxf==true)                                    
                                        nuevo.valor = 1 - Convert.ToInt32(val2.valor);                                                                            
                                    else 
                                        nuevo.valor = 0 - Convert.ToInt32(val2.valor);                                    
                                    nuevo.tipo = "INT";
                                    break;                                    
                                case "DOUBLE":
                                    bool auxf1 = Convert.ToBoolean(val1.valor);
                                    if (auxf1==true)                                    
                                        nuevo.valor = 1 - Convert.ToDouble(val2.valor);                                                                            
                                    else 
                                        nuevo.valor = 0 - Convert.ToDouble(val2.valor);                                    
                                    nuevo.tipo = "DOUBLE";
                                    break;
                                case "CHAR":
                                    //ERROR
                                    detError = "No es posible restar un valor tipo BOOL con uno tipo CHAR";
                                    nuevo.valor = false;
                                    nuevo.tipo = "BOOL";
                                    break;
                                case "STRING":
                                    //ERROR
                                    detError = "No es posible restar un valor tipo BOOL con uno tipo STRING";
                                    nuevo.valor = false;
                                    nuevo.tipo = "BOOL";
                                    break;
                                case "BOOL":
                                    //ERROR
                                    detError = "No es posible restar dos valores tipo BOOL";
                                    nuevo.valor = false;
                                    nuevo.tipo = "BOOL";
                                    break;
                            }
                            break;
                    }
                    break;

                case "/":
                    switch (val1.tipo) {
                        case "INT":
                            switch (val2.tipo) {
                                case "INT":
                                    if (Convert.ToInt32(val2.valor) != 0) {
                                        nuevo.valor = Convert.ToInt32(val1.valor) / Convert.ToInt32(val2.valor);
                                        nuevo.tipo = "DOUBLE";
                                    } else { 
                                        //ERROR
                                        detError = "No es posible dividir un numero entre 0";
                                        nuevo.valor = 0;
                                        nuevo.tipo = "INT";
                                    }                                    
                                    break;
                                case "DOUBLE":
                                    if (Convert.ToDouble(val2.valor) != 0.0) {
                                        nuevo.valor = Convert.ToInt32(val1.valor) / Convert.ToDouble(val2.valor);
                                        nuevo.tipo = "DOUBLE";
                                    } else {
                                        //ERROR
                                        detError = "No es posible dividir un numero entre 0";
                                        nuevo.valor = 0;
                                        nuevo.tipo = "INT";
                                    }                                    
                                    break;
                                case "CHAR":
                                    if ((Int32)Convert.ToChar(val2.valor) != 0) {
                                        nuevo.valor = Convert.ToInt32(val1.valor) / (Int32)Convert.ToChar(val2.valor);
                                        nuevo.tipo = "DOUBLE";
                                    } else {
                                        //ERROR
                                        detError = "No es posible dividir un numero entre 0";
                                        nuevo.valor = 0;
                                        nuevo.tipo = "INT";
                                    }                                    
                                    break;
                                case "STRING":
                                    //ERROR
                                    detError = "No es posible dividir un valor tipo INT con uno tipo STRING";
                                    nuevo.valor = 0;
                                    nuevo.tipo = "INT";
                                    break;
                                case "BOOL":
                                    bool auxf = Convert.ToBoolean(val2.valor);
                                    if (auxf==true) {
                                        nuevo.valor = Convert.ToInt32(val1.valor) / 1;
                                        nuevo.tipo = "DOUBLE";
                                    } else {
                                        //ERROR
                                        detError = "No es posible dividir un numero entre 0";
                                        nuevo.valor = 0;
                                        nuevo.tipo = "INT";
                                    }                                    
                                    break;
                            }
                            break;
                        case "DOUBLE":
                            switch (val2.tipo) {
                                case "INT":
                                    if (Convert.ToInt32(val2.valor) != 0) {
                                        nuevo.valor = Convert.ToDouble(val1.valor) / Convert.ToInt32(val2.valor);
                                        nuevo.tipo = "DOUBLE";
                                    } else {
                                        //ERROR
                                        detError = "No es posible dividir un numero entre 0";
                                        nuevo.valor = 0.0;
                                        nuevo.tipo = "DOUBLE";
                                    }                                    
                                    break;                                    
                                case "DOUBLE":
                                    if (Convert.ToDouble(val2.valor) != 0.0) {
                                        nuevo.valor = Convert.ToDouble(val1.valor) / Convert.ToDouble(val2.valor);
                                        nuevo.tipo = "DOUBLE";
                                    } else {
                                        //ERROR
                                        detError = "No es posible dividir un numero entre 0";
                                        nuevo.valor = 0.0;
                                        nuevo.tipo = "DOUBLE";
                                    }                                    
                                    break;                                    
                                case "CHAR":
                                    if ((Int32)Convert.ToChar(val2.valor) != 0) {
                                        nuevo.valor = Convert.ToDouble(val1.valor) / (Int32)Convert.ToChar(val2.valor);
                                        nuevo.tipo = "DOUBLE";
                                    } else {
                                        //ERROR
                                        detError = "No es posible dividir un numero entre 0";
                                        nuevo.valor = 0.0;
                                        nuevo.tipo = "DOUBLE";
                                    }                                    
                                    break;
                                case "STRING":
                                    //ERROR
                                    detError = "No es posible dividir un valor tipo DOUBLE con uno tipo STRING";
                                    nuevo.valor = 0.0;
                                    nuevo.tipo = "DOUBLE";
                                    break;
                                case "BOOL":
                                    bool auxf = Convert.ToBoolean(val2.valor);
                                    if (auxf==true) {
                                        nuevo.valor = Convert.ToDouble(val1.valor) / 1;
                                        nuevo.tipo = "DOUBLE";
                                    }
                                    else {
                                        //ERROR
                                        detError = "No es posible dividir un numero entre 0";
                                        nuevo.valor = 0.0;
                                        nuevo.tipo = "DOUBLE";
                                    }                                    
                                    break;
                            }
                            break;
                        case "CHAR":
                            switch (val2.tipo)
                            {
                                case "INT":
                                    if (Convert.ToInt32(val2.valor) != 0) {
                                        nuevo.valor = (Int32)Convert.ToChar(val1.valor) / Convert.ToInt32(val2.valor);
                                        nuevo.tipo = "DOUBLE";
                                    } else {
                                        //ERROR
                                        detError = "No es posible dividir un numero entre 0";
                                        nuevo.valor = ' ';
                                        nuevo.tipo = "CHAR";
                                    }                                    
                                    break;
                                case "DOUBLE":
                                    if (Convert.ToDouble(val2.valor) != 0.0) {
                                        nuevo.valor = (Int32)Convert.ToChar(val1.valor) / Convert.ToDouble(val2.valor);
                                        nuevo.tipo = "DOUBLE";
                                    } else {
                                        //ERROR
                                        detError = "No es posible dividir un numero entre 0";
                                        nuevo.valor = ' ';
                                        nuevo.tipo = "CHAR";
                                    }                                    
                                    break;
                                case "CHAR":
                                    //ERROR
                                    detError = "No es posible dividir dos valores tipo CHAR";
                                    nuevo.valor = ' ';
                                    nuevo.tipo = "CHAR";
                                    break;
                                case "STRING":
                                    //ERROR
                                    detError = "No es posible dividir un valor tipo CHAR con uno tipo STRING";
                                    nuevo.valor = ' ';
                                    nuevo.tipo = "CHAR";
                                    break;
                                case "BOOL":
                                    //ERROR
                                    detError = "No es posible dividir un valor tipo CHAR con uno tipo BOOL";
                                    nuevo.valor = ' ';
                                    nuevo.tipo = "CHAR";
                                    break;
                            }
                            break;
                        case "STRING":
                            //ERROR
                            switch (val2.tipo)
                            {
                                case "INT":
                                    detError = "No es posible dividir un valor tipo STRING con uno tipo INT";
                                    break;
                                case "DOUBLE":
                                    detError = "No es posible dividir un valor tipo STRING con uno tipo DOUBLE";
                                    break;
                                case "CHAR":
                                    detError = "No es posible dividir un valor tipo STRING con uno tipo CHAR";
                                    break;
                                case "STRING":
                                    detError = "No es posible dividir dos valores tipo STRING";
                                    break;
                                case "BOOL":
                                    detError = "No es posible dividir un valor tipo STRING con uno tipo BOOL";
                                    break;
                            }                            
                            nuevo.valor = "";
                            nuevo.tipo = "STRING";
                            break;
                        case "BOOL":
                            switch (val2.tipo) {
                                case "INT":
                                    if (Convert.ToInt32(val2.valor) != 0) {
                                        bool auxf = Convert.ToBoolean(val1.valor);
                                        if (auxf == true)
                                            nuevo.valor = 1 / Convert.ToInt32(val2.valor);
                                        else
                                            nuevo.valor = 0 / Convert.ToInt32(val2.valor);
                                        nuevo.tipo = "DOUBLE";
                                    } else {
                                        //ERROR
                                        detError = "No es posible dividir un numero entre 0";
                                        nuevo.valor = false;
                                        nuevo.tipo = "BOOL";
                                    }                                    
                                    break;
                                case "DOUBLE":
                                    if (Convert.ToInt32(val2.valor) != 0) {
                                        bool auxf1 = Convert.ToBoolean(val1.valor);
                                        if (auxf1 == true)
                                            nuevo.valor = 1 / Convert.ToDouble(val2.valor);
                                        else
                                            nuevo.valor = 0 / Convert.ToDouble(val2.valor);
                                        nuevo.tipo = "DOUBLE";
                                    } else {
                                        //ERROR
                                        detError = "No es posible dividir un numero entre 0";
                                        nuevo.valor = false;
                                        nuevo.tipo = "BOOL";
                                    }                                    
                                    break;
                                case "CHAR":
                                    //ERROR
                                    detError = "No es posible dividir un valor tipo BOOL con uno tipo CHAR";
                                    nuevo.valor = false;
                                    nuevo.tipo = "BOOL";
                                    break;
                                case "STRING":
                                    //ERROR
                                    detError = "No es posible dividir un valor tipo BOOL con uno tipo STRING";
                                    nuevo.valor = false;
                                    nuevo.tipo = "BOOL";
                                    break;
                                case "BOOL":
                                    //ERROR
                                    detError = "No es posible dividir dos valores tipo BOOL";
                                    nuevo.valor = false;
                                    nuevo.tipo = "BOOL";
                                    break;
                            }
                            break;
                    }
                    break;

                case "^":
                    switch (val1.tipo) {
                        case "INT":
                            switch (val2.tipo) {
                                case "INT":
                                    nuevo.valor = System.Math.Pow( Convert.ToInt32(val1.valor), Convert.ToInt32(val2.valor));
                                    nuevo.tipo = "INT";
                                    break;
                                case "DOUBLE":
                                    nuevo.valor = System.Math.Pow( Convert.ToInt32(val1.valor) , Convert.ToDouble(val2.valor));
                                    nuevo.tipo = "DOUBLE";
                                    break;
                                case "CHAR":
                                    nuevo.valor = System.Math.Pow( Convert.ToInt32(val1.valor) , (Int32)Convert.ToChar(val2.valor));
                                    nuevo.tipo = "INT";
                                    break;
                                case "STRING":
                                    //ERROR
                                    detError = "No es posible realizar una potencia de un valor tipo INT con uno tipo STRING";
                                    nuevo.valor = 0;
                                    nuevo.tipo = "INT";
                                    break;
                                case "BOOL":
                                    bool auxf = Convert.ToBoolean(val2.valor);
                                    if (auxf == true)
                                        nuevo.valor = System.Math.Pow(Convert.ToInt32(val1.valor), 1);
                                    else                                    
                                        nuevo.valor = System.Math.Pow(Convert.ToInt32(val1.valor), 0);                                    
                                    nuevo.tipo = "INT";
                                    break;
                            }
                            break;
                        case "DOUBLE":
                            switch (val2.tipo) {
                                case "INT":
                                    nuevo.valor = System.Math.Pow( Convert.ToDouble(val1.valor) , Convert.ToInt32(val2.valor) );
                                    nuevo.tipo = "DOUBLE";
                                    break;
                                case "DOUBLE":
                                    nuevo.valor = System.Math.Pow( Convert.ToDouble(val1.valor) , Convert.ToDouble(val2.valor));
                                    nuevo.tipo = "DOUBLE";
                                    break;   
                                case "CHAR":
                                    nuevo.valor = System.Math.Pow( Convert.ToDouble(val1.valor) , (Int32)Convert.ToChar(val2.valor));
                                    nuevo.tipo = "DOUBLE";
                                    break;
                                case "STRING":
                                    //ERROR
                                    detError = "No es posible realizar una potencia de un valor tipo DOUBLE con uno tipo STRING";
                                    nuevo.valor = 0.0;
                                    nuevo.tipo = "DOUBLE";
                                    break;
                                case "BOOL":
                                    bool auxf = Convert.ToBoolean(val2.valor);
                                    if (auxf==true)                                    
                                        nuevo.valor = System.Math.Pow( Convert.ToDouble(val1.valor) , 1);                                     
                                    else 
                                        nuevo.valor = System.Math.Pow( Convert.ToDouble(val1.valor) , 0);                                    
                                    nuevo.tipo = "DOUBLE";                                    
                                    break;
                            }
                            break;
                        case "CHAR":
                            switch (val2.tipo) {
                                case "INT":
                                    nuevo.valor = System.Math.Pow( (Int32)Convert.ToChar(val1.valor) , Convert.ToInt32(val2.valor));
                                    nuevo.tipo = "INT";
                                    break;
                                case "DOUBLE":
                                    nuevo.valor = System.Math.Pow( (Int32)Convert.ToChar(val1.valor) , Convert.ToDouble(val2.valor));
                                    nuevo.tipo = "DOUBLE";
                                    break;
                                case "CHAR":
                                    //ERROR
                                    detError = "No es posible realizar una potencia de dos valores tipo CHAR";
                                    nuevo.valor = ' ';
                                    nuevo.tipo = "CHAR";
                                    break;
                                case "STRING":
                                    //ERROR
                                    detError = "No es posible realizar una potencia de un valor tipo CHAR con uno tipo STRING";
                                    nuevo.valor = ' ';
                                    nuevo.tipo = "CHAR";
                                    break;
                                case "BOOL":
                                    //ERROR
                                    detError = "No es posible realizar una potencia de un valor tipo CHAR con uno tipo BOOL";
                                    nuevo.valor = ' ';
                                    nuevo.tipo = "CHAR";
                                    break;
                            }
                            break;
                        case "STRING":
                            //ERROR
                            switch (val2.tipo) {
                                case "INT":
                                    detError = "No es posible realizar una potencia de un valor tipo STRING con uno tipo INT";
                                    break;
                                case "DOUBLE":
                                    detError = "No es posible realizar una potencia de un valor tipo STRING con uno tipo DOUBLE";
                                    break;
                                case "CHAR":
                                    detError = "No es posible realizar una potencia de un valor tipo STRING con uno tipo CHAR";
                                    break;
                                case "STRING":
                                    detError = "No es posible realizar una potencia de dos valores tipo STRING";
                                    break;
                                case "BOOL":
                                    detError = "No es posible realizar una potencia de un valor tipo STRING con uno tipo BOOL";
                                    break;
                            }                            
                            nuevo.valor = "";
                            nuevo.tipo = "STRING";
                            break;
                        case "BOOL":
                            switch (val2.tipo) {
                                case "INT":
                                    bool auxf = Convert.ToBoolean(val1.valor);
                                    if (auxf==true)                                    
                                        nuevo.valor = System.Math.Pow(1 , Convert.ToInt32(val2.valor));                                     
                                    else 
                                        nuevo.valor = System.Math.Pow(0 , Convert.ToInt32(val2.valor));                                    
                                    nuevo.tipo = "INT";
                                    break;
                                case "DOUBLE":
                                    bool auxf1 = Convert.ToBoolean(val1.valor);
                                    if (auxf1==true)                                    
                                        nuevo.valor = System.Math.Pow(1 , Convert.ToDouble(val2.valor));                                    
                                    else 
                                        nuevo.valor = System.Math.Pow(0 , Convert.ToDouble(val2.valor));                                    
                                    nuevo.tipo = "DOUBLE";
                                    break;
                                case "CHAR":
                                    //ERROR
                                    detError = "No es posible realizar una potencia de un valor tipo BOOL con uno tipo CHAR";
                                    nuevo.valor = false;
                                    nuevo.tipo = "BOOL";
                                    break;
                                case "STRING":
                                    //ERROR
                                    detError = "No es posible realizar una potencia de un valor tipo BOOL con uno tipo STRING";
                                    nuevo.valor = false;
                                    nuevo.tipo = "BOOL";
                                    break;
                                case "BOOL":
                                    //ERROR
                                    detError = "No es posible realizar una potencia de dos valores tipo BOOL";
                                    nuevo.valor = false;
                                    nuevo.tipo = "BOOL";
                                    break;
                            }
                            break;
                    }
                    break;
            }
            if (detError != "") {
                if (val1.error != true && val2.error != true) {
                    nwnError.Add("semantico");
                    nwnError.Add(detError);
                    nwnError.Add(fila.ToString());
                    nwnError.Add(columna.ToString());
                    nwnError.Add(arch);
                    lista.Add(nwnError);
                    nuevo.error = true;
                }
            }            
            if (val1.error==true || val2.error==true)            
                nuevo.error = true;            
            return nuevo;
        }

        public Tupla logicoRel(Tupla val1, String op, Tupla val2,int fila,int columna) {
            Tupla nuevo = new Tupla();
            int a = 0;
            int b = 0;
            bool a1 = false;
            bool a2 = false;
            List<String> nwnError = new List<String>();  //error
            string detError = ""; //detalle error
            switch (op) {
                case "&&":
                    switch (val1.tipo) {
                        case "INT":
                            if (Convert.ToInt32(val1.valor) == 0 || Convert.ToInt32(val1.tipo)==1) {
                                switch (val2.tipo) {
                                    case "INT":
                                        if (Convert.ToInt32(val2.valor) == 0 || Convert.ToInt32(val2.valor) == 1) {
                                            a1 = unoCero(Convert.ToInt32(val1.valor));
                                            a2 = unoCero(Convert.ToInt32(val2.valor));
                                            nuevo.valor = a1 && a2;                                            
                                        } else { 
                                            //ERROR
                                            detError = "&& no puede operar valores de tipo INT distintos de 0 y 1";                                                                                    
                                        }
                                        break;
                                    case "DOUBLE":
                                        if (Convert.ToDouble(val2.valor) % 1 == 0) {
                                            if (Convert.ToInt32(val2.valor) == 0 || Convert.ToInt32(val2.valor) == 1) {
                                                a1 = unoCero(Convert.ToInt32(val1.valor));
                                                a2 = unoCero(Convert.ToInt32(val2.valor));
                                                nuevo.valor = a1 && a2;
                                            } else {
                                                //ERROR
                                                detError = "&& no puede operar un valor tipo INT con uno DOUBLE distinto de 0.0 y 1.0";     
                                            }
                                        } else { 
                                            //ERROR
                                            detError = "&& no puede operar un valor tipo INT con uno DOUBLE distinto de 0.0 y 1.0";
                                        }
                                        break;
                                    case "CHAR":
                                        if ((Int32)Convert.ToChar(val2.valor) == 0 || (Int32)Convert.ToChar(val2.valor) == 1) {
                                            a1 = unoCero(Convert.ToInt32(val1.valor));
                                            a2 = unoCero((Int32)Convert.ToChar(val2.valor));
                                            nuevo.valor = a1 && a2;
                                        } else {
                                            //ERROR
                                            detError = "&& no puede operar un valor de tipo INT con CHAR, excepto si su equivalente ascii es 0 o 1";
                                        }
                                        break;
                                    case "STRING":
                                        //ERROR
                                        detError = "&& no puede operar valores de tipo INT con STRING";
                                        break;
                                    case "BOOL":
                                        a1 = unoCero(Convert.ToInt32(val1.valor));
                                        nuevo.valor = a1 && Convert.ToBoolean(val2.valor);
                                        break;
                                }
                            } else {
                                //ERRROR
                                detError = "&& no puede operar valores de tipo INT distintos de 0 y 1";                              
                            }
                            break;
                        case "DOUBLE":
                            if (Convert.ToDouble(val1.valor) % 1 == 0) {
                                if (Convert.ToInt32(val1.valor) == 0 || Convert.ToInt32(val1.valor) == 1) {
                                    switch (val2.tipo) {
                                        case "INT":
                                            if (Convert.ToInt32(val2.valor) == 0 || Convert.ToInt32(val2.valor) == 1) {
                                                a1 = unoCero(Convert.ToInt32(val1.valor));
                                                a2 = unoCero(Convert.ToInt32(val2.valor));
                                                nuevo.valor = a1 && a2;
                                            } else { 
                                                //ERROR
                                                detError = "&& no puede operar un valor tipo DOUBLE con uno INT distinto de 0 y 1";
                                            }                                          
                                            break;
                                        case "DOUBLE":
                                            if (Convert.ToDouble(val2.valor) % 1 == 0) {
                                                if (Convert.ToInt32(val2.valor) == 0 || Convert.ToInt32(val2.valor) == 1) {
                                                    a1 = unoCero(Convert.ToInt32(val1.valor));
                                                    a2 = unoCero(Convert.ToInt32(val2.valor));
                                                    nuevo.valor = a1 && a2;
                                                } else { 
                                                    //ERROR
                                                    detError = "&& no puede operar valores de tipo DOUBLE distintos de 0.0 y 1.0";
                                                }
                                            } else { 
                                                //ERROR
                                                detError = "&& no puede operar valores de tipo DOUBLE distintos de 0.0 y 1.0";
                                            }
                                            break;
                                        case "CHAR":
                                            if ((Int32)Convert.ToChar(val2.valor) == 0 || (Int32)Convert.ToChar(val2.valor) == 1) {
                                                a1 = unoCero(Convert.ToInt32(val1.valor));
                                                a2 = unoCero((Int32)Convert.ToChar(val2.valor));
                                                nuevo.valor = a1 && a2;
                                            } else { 
                                                //ERROR
                                                detError = "&& no puede operar un valor de tipo DOUBLE con CHAR, excepto si su equivalente ascii es 0 o 1";
                                            }
                                            break;
                                        case "STRING":
                                            //ERROR
                                            detError = "&& no puede operar valores de tipo DOUBLE con STRING";
                                            break;
                                        case "BOOL":
                                            a1 = unoCero(Convert.ToInt32(val1.valor));
                                            nuevo.valor = a1 && Convert.ToBoolean(val2.valor);
                                            break;
                                    }
                                } else {
                                    //ERROR
                                    detError = "&& no puede operar valores de tipo DOUBLE distintos de 0.0 y 1.0";
                                }
                            } else { 
                                //ERROR
                                detError = "&& no puede operar valores de tipo DOUBLE distintos de 0.0 y 1.0";
                            }
                            break;
                        case "CHAR":
                            if ((Int32)Convert.ToChar(val2.valor) == 0 || (Int32)Convert.ToChar(val2.valor) == 1) {
                                switch (val2.tipo) {
                                    case "INT":
                                        if (Convert.ToInt32(val2.valor) == 0 || Convert.ToInt32(val2.valor) == 1) {
                                            a1 = unoCero((Int32)Convert.ToChar(val1.valor));
                                            a2 = unoCero(Convert.ToInt32(val2.valor));
                                            nuevo.valor = a1 && a2;
                                        } else { 
                                            //ERRROR
                                            detError = "&& no puede operar un valor tipo CHAR con uno INT distinto de 0 y 1";
                                        }
                                        break;
                                    case "DOUBLE":
                                        if(Convert.ToDouble(val2.valor)%1==0){
                                            if (Convert.ToInt32(val2.valor) == 0 || Convert.ToInt32(val2.valor) == 1) {
                                                a1 = unoCero((Int32)Convert.ToChar(val1.valor));
                                                a2 = unoCero(Convert.ToInt32(val2.valor));
                                                nuevo.valor = a1 && a2;
                                            } else {
                                                //ERROR
                                                detError = "&& no puede operar un valor tipo CHAR con uno DOUBLE distinto de 0.0 y 1.0";
                                            }
                                        } else{
                                            //ERROR
                                            detError = "&& no puede operar un valor tipo CHAR con uno DOUBLE distinto de 0.0 y 1.0";
                                        }
                                        break;
                                    case "CHAR":
                                        if ((Int32)Convert.ToChar(val2.valor) == 0 || (Int32)Convert.ToChar(val2.valor) == 1) {
                                            a1 = unoCero((Int32)Convert.ToChar(val1.valor));
                                            a2 = unoCero((Int32)Convert.ToChar(val2.valor));
                                            nuevo.valor = a1 && a2;
                                        } else {
                                            //ERROR
                                            detError = "&& no puede operar valores tipo CHAR excepto si su equivalente ascii es 0 o 1";
                                        }
                                        break;
                                    case "STRING":
                                        //ERROR
                                        detError = "&& no puede operar valores de tipo CHAR con STRING";
                                        break;
                                    case "BOOL":
                                        a1 = unoCero((Int32)Convert.ToChar(val1.valor));
                                        nuevo.valor = a1 && Convert.ToBoolean(val2.valor);
                                        break;
                                }
                            } else { 
                                //ERROR
                                detError = "&& no puede operar valores tipo CHAR excepto si su equivalente ascii es 0 o 1";
                            }
                            break;
                        case "STRING":
                            //ERRROR
                            detError = "&& no puede operar valores tipo STRING con ningun otro tipo";
                            break;
                        case "BOOL":
                            switch (val2.tipo) {
                                case "INT":
                                    if (Convert.ToInt32(val2.valor) == 0 || Convert.ToInt32(val2.valor) == 1) {
                                        a2 = unoCero(Convert.ToInt32(val2.valor));
                                        nuevo.valor = Convert.ToBoolean(val1.valor) && a2;
                                    } else {
                                        //ERROR
                                        detError = "&& no puede operar un valor tipo BOOL con uno INT distinto de 0 y 1";
                                    }
                                    break;
                                case "DOUBLE":
                                    if(Convert.ToDouble(val2.valor)%1==0){
                                        if (Convert.ToInt32(val2.valor) == 0 || Convert.ToInt32(val2.valor) == 1) {
                                            a2 = unoCero(Convert.ToInt32(val2.valor));
                                            nuevo.valor = Convert.ToBoolean(val1.valor) && a2;
                                        } else {
                                            //ERROR
                                            detError = "&& no puede operar un valor tipo BOOL con uno DOUBLE distinto de 0.0 y 1.0";
                                        }
                                    } else {
                                        //ERROR
                                        detError = "&& no puede operar un valor tipo BOOL con uno DOUBLE distinto de 0.0 y 1.0";
                                    }
                                    break;                                
                                case "CHAR":
                                    if ((Int32)Convert.ToChar(val2.valor) == 0 || (Int32)Convert.ToChar(val2.valor) == 1) {
                                        a2 = unoCero((Int32)Convert.ToChar(val2.valor));
                                        nuevo.valor = Convert.ToBoolean(val1.valor) && a2;
                                    } else {
                                        //ERROR
                                        detError = "&& no puede operar un valor de tipo BOOL con CHAR, excepto si su equivalente ascii es 0 o 1";
                                    }
                                    break;
                                case "STRING":
                                    //ERROR
                                    detError = "&& no puede operar valores de tipo BOOL con STRING";
                                    break;
                                case "BOOL":
                                    nuevo.valor = Convert.ToBoolean(val1.valor) && Convert.ToBoolean(val2.valor);
                                    break;
                            }
                            break;
                    }
                    break;

                case "||":
                    switch (val1.tipo) {
                        case "INT":
                            if (Convert.ToInt32(val1.valor) == 0 || Convert.ToInt32(val1.tipo)==1) {
                                switch (val2.tipo) {
                                    case "INT":
                                        if (Convert.ToInt32(val2.valor) == 0 || Convert.ToInt32(val2.valor) == 1) {
                                            a1 = unoCero(Convert.ToInt32(val1.valor));
                                            a2 = unoCero(Convert.ToInt32(val2.valor));
                                            nuevo.valor = a1 || a2;                                            
                                        } else { 
                                            //ERROR
                                            detError = "|| no puede operar valores de tipo INT distintos de 0 y 1";
                                        }
                                        break;
                                    case "DOUBLE":
                                        if (Convert.ToDouble(val2.valor) % 1 == 0) {
                                            if (Convert.ToInt32(val2.valor) == 0 || Convert.ToInt32(val2.valor) == 1) {
                                                a1 = unoCero(Convert.ToInt32(val1.valor));
                                                a2 = unoCero(Convert.ToInt32(val2.valor));
                                                nuevo.valor = a1 || a2;
                                            } else {
                                                //ERROR
                                                detError = "|| no puede operar un valor tipo INT con uno DOUBLE distinto de 0.0 y 1.0";
                                            }
                                        } else { 
                                            //ERROR
                                            detError = "|| no puede operar un valor tipo INT con uno DOUBLE distinto de 0.0 y 1.0";
                                        }
                                        break;
                                    case "CHAR":
                                        if ((Int32)Convert.ToChar(val2.valor) == 0 || (Int32)Convert.ToChar(val2.valor) == 1) {
                                            a1 = unoCero(Convert.ToInt32(val1.valor));
                                            a2 = unoCero((Int32)Convert.ToChar(val2.valor));
                                            nuevo.valor = a1 || a2;
                                        } else {
                                            //ERROR
                                            detError = "|| no puede operar un valor de tipo INT con CHAR, excepto si su equivalente ascii es 0 o 1";
                                        }
                                        break;
                                    case "STRING":
                                        //ERROR
                                        detError = "|| no puede operar valores de tipo INT con STRING";
                                        break;
                                    case "BOOL":
                                        a1 = unoCero(Convert.ToInt32(val1.valor));
                                        nuevo.valor = a1 || Convert.ToBoolean(val2.valor);
                                        break;
                                }
                            } else {
                                //ERRROR
                                detError = "|| no puede operar valores de tipo INT distintos de 0 y 1";  
                            }
                            break;
                        case "DOUBLE":
                            if (Convert.ToDouble(val1.valor) % 1 == 0) {
                                if (Convert.ToInt32(val1.valor) == 0 || Convert.ToInt32(val1.valor) == 1){
                                    switch (val2.tipo) {
                                        case "INT":
                                            if (Convert.ToInt32(val2.valor) == 0 || Convert.ToInt32(val2.valor) == 1) {
                                                a1 = unoCero(Convert.ToInt32(val1.valor));
                                                a2 = unoCero(Convert.ToInt32(val2.valor));
                                                nuevo.valor = a1 || a2;
                                            } else { 
                                                //ERROR
                                                detError = "|| no puede operar un valor tipo DOUBLE con uno INT distinto de 0 y 1";
                                            }                                          
                                            break;
                                        case "DOUBLE":
                                            if (Convert.ToDouble(val2.valor) % 1 == 0) {
                                                if (Convert.ToInt32(val2.valor) == 0 || Convert.ToInt32(val2.valor) == 1) {
                                                    a1 = unoCero(Convert.ToInt32(val1.valor));
                                                    a2 = unoCero(Convert.ToInt32(val2.valor));
                                                    nuevo.valor = a1 || a2;
                                                } else { 
                                                    //ERROR
                                                    detError = "|| no puede operar valores de tipo DOUBLE distintos de 0.0 y 1.0";
                                                }
                                            } else { 
                                                //ERROR
                                                detError = "|| no puede operar valores de tipo DOUBLE distintos de 0.0 y 1.0";
                                            }
                                            break;
                                        case "CHAR":
                                            if ((Int32)Convert.ToChar(val2.valor) == 0 || (Int32)Convert.ToChar(val2.valor) == 1) {
                                                a1 = unoCero(Convert.ToInt32(val1.valor));
                                                a2 = unoCero((Int32)Convert.ToChar(val2.valor));
                                                nuevo.valor = a1 || a2;
                                            } else { 
                                                //ERROR
                                                detError = "|| no puede operar un valor de tipo DOUBLE con CHAR, excepto si su equivalente ascii es 0 o 1";
                                            }
                                            break;
                                        case "STRING":
                                            //ERROR
                                            detError = "|| no puede operar valores de tipo DOUBLE con STRING";
                                            break;
                                        case "BOOL":
                                            a1 = unoCero(Convert.ToInt32(val1.valor));
                                            nuevo.valor = a1 || Convert.ToBoolean(val2.valor);
                                            break;
                                    }
                                } else {
                                    //ERROR
                                    detError = "|| no puede operar valores de tipo DOUBLE distintos de 0.0 y 1.0";
                                }
                            } else { 
                                //ERROR
                                detError = "|| no puede operar valores de tipo DOUBLE distintos de 0.0 y 1.0";
                            }
                            break;
                        case "CHAR":
                            if ((Int32)Convert.ToChar(val2.valor) == 0 || (Int32)Convert.ToChar(val2.valor) == 1) {
                                switch (val2.tipo) {
                                    case "INT":
                                        if (Convert.ToInt32(val2.valor) == 0 || Convert.ToInt32(val2.valor) == 1) {
                                            a1 = unoCero((Int32)Convert.ToChar(val1.valor));
                                            a2 = unoCero(Convert.ToInt32(val2.valor));
                                            nuevo.valor = a1 || a2;
                                        } else { 
                                            //ERRROR
                                            detError = "|| no puede operar un valor tipo CHAR con uno INT distinto de 0 y 1";
                                        }
                                        break;
                                    case "DOUBLE":
                                        if(Convert.ToDouble(val2.valor)%1==0){
                                            if (Convert.ToInt32(val2.valor) == 0 || Convert.ToInt32(val2.valor) == 1) {
                                                a1 = unoCero((Int32)Convert.ToChar(val1.valor));
                                                a2 = unoCero(Convert.ToInt32(val2.valor));
                                                nuevo.valor = a1 || a2;
                                            } else {
                                                //ERROR
                                                detError = "|| no puede operar un valor tipo CHAR con uno DOUBLE distinto de 0.0 y 1.0";
                                            }
                                        } else{
                                            //ERROR
                                            detError = "|| no puede operar un valor tipo CHAR con uno DOUBLE distinto de 0.0 y 1.0";
                                        }
                                        break;
                                    case "CHAR":
                                        if ((Int32)Convert.ToChar(val2.valor) == 0 || (Int32)Convert.ToChar(val2.valor) == 1) {
                                            a1 = unoCero((Int32)Convert.ToChar(val1.valor));
                                            a2 = unoCero((Int32)Convert.ToChar(val2.valor));
                                            nuevo.valor = a1 || a2;
                                        } else {
                                            //ERROR
                                            detError = "|| no puede operar valores tipo CHAR excepto si su equivalente ascii es 0 o 1";
                                        }
                                        break;
                                    case "STRING":
                                        //ERROR
                                        detError = "|| no puede operar valores de tipo CHAR con STRING";
                                        break;
                                    case "BOOL":
                                        a1 = unoCero((Int32)Convert.ToChar(val1.valor));
                                        nuevo.valor = a1 || Convert.ToBoolean(val2.valor);
                                        break;
                                }
                            } else { 
                                //ERROR
                                detError = "|| no puede operar valores tipo CHAR excepto si su equivalente ascii es 0 o 1";
                            }
                            break;
                        case "STRING":
                            //ERRROR
                            detError = "|| no puede operar valores tipo STRING con ningun otro tipo";
                            break;
                        case "BOOL":
                            switch (val2.tipo) {
                                case "INT":
                                    if (Convert.ToInt32(val2.valor) == 0 || Convert.ToInt32(val2.valor) == 1) {
                                        a2 = unoCero(Convert.ToInt32(val2.valor));
                                        nuevo.valor = Convert.ToBoolean(val1.valor) || a2;
                                    } else {
                                        //ERROR
                                        detError = "|| no puede operar un valor tipo BOOL con uno INT distinto de 0 y 1";
                                    }
                                    break;
                                case "DOUBLE":
                                    if(Convert.ToDouble(val2.valor)%1==0){
                                        if (Convert.ToInt32(val2.valor) == 0 || Convert.ToInt32(val2.valor) == 1) {
                                            a2 = unoCero(Convert.ToInt32(val2.valor));
                                            nuevo.valor = Convert.ToBoolean(val1.valor) || a2;
                                        } else {
                                            //ERROR
                                            detError = "|| no puede operar un valor tipo BOOL con uno DOUBLE distinto de 0.0 y 1.0";
                                        }
                                    } else {
                                        //ERROR
                                        detError = "|| no puede operar un valor tipo BOOL con uno DOUBLE distinto de 0.0 y 1.0";
                                    }
                                    break;                                
                                case "CHAR":
                                    if ((Int32)Convert.ToChar(val2.valor) == 0 || (Int32)Convert.ToChar(val2.valor) == 1) {
                                        a2 = unoCero((Int32)Convert.ToChar(val2.valor));
                                        nuevo.valor = Convert.ToBoolean(val1.valor) || a2;
                                    } else {
                                        //ERROR
                                        detError = "|| no puede operar un valor de tipo BOOL con CHAR, excepto si su equivalente ascii es 0 o 1";
                                    }
                                    break;
                                case "STRING":
                                    //ERROR
                                    detError = "|| no puede operar valores de tipo BOOL con STRING";
                                    break;
                                case "BOOL":
                                    nuevo.valor = Convert.ToBoolean(val1.valor) || Convert.ToBoolean(val2.valor);
                                    break;
                            }
                            break;
                    }
                    break;                

                case ">":
                    switch(val1.tipo){
                        case "INT":
                            switch (val2.tipo) {
                                case "INT":
                                    if (Convert.ToInt32(val1.valor) > Convert.ToInt32(val2.valor))                                    
                                        nuevo.valor = true;                                    
                                    else
                                        nuevo.valor = false;                                    
                                    break;
                                case "DOUBLE":
                                    if (Convert.ToInt32(val1.valor) > Convert.ToDouble(val2.valor))                                    
                                        nuevo.valor = true;                                    
                                    else                                    
                                        nuevo.valor = false;                                    
                                    break;
                                case "CHAR":
                                    if (Convert.ToInt32(val1.valor) > (Int32)Convert.ToChar(val2.valor))                                    
                                        nuevo.valor = true;                                    
                                    else                                    
                                        nuevo.valor = false;                                    
                                    break;
                                case "STRING":
                                    //ERROR
                                	detError = "> No puede comparar un valor tipo INT con uno STRING";
                                    break;
                                case "BOOL":
                                    //ERROR
                                	detError = "> No puede comparar un valor tipo INT con uno BOOL";
                                    break;
                            }
                            break;
                        case "DOUBLE":
                            switch (val2.tipo) {
                                case "INT":
                                    if (Convert.ToDouble(val1.valor) > Convert.ToInt32(val2.valor))
                                        nuevo.valor = true;
                                    else
                                        nuevo.valor = false;
                                    break;
                                case "DOUBLE":
                                    if (Convert.ToDouble(val1.valor) > Convert.ToDouble(val2.valor))
                                        nuevo.valor = true;
                                    else
                                        nuevo.valor = false;
                                    break;
                                case "CHAR":
                                    if (Convert.ToDouble(val1.valor) > (Int32)Convert.ToChar(val2.valor))
                                        nuevo.valor = true;
                                    else
                                        nuevo.valor = false;
                                    break;
                                case "STRING":
                                    //ERRROR
                                	detError = "> No pede comparar un valor tipo DOUBLE con uno STRING";
                                    break;
                                case "BOOL":
                                    //ERROR
                                	detError = "> No puede comparar un valor tipo DOUBLE con uno BOOL";
                                    break;
                            }
                            break;
                        case "CHAR":
                            switch (val2.tipo) {
                                case "INT":
                                    if ((Int32)Convert.ToChar(val1.valor) > Convert.ToInt32(val2.valor))
                                        nuevo.valor = true;
                                    else
                                        nuevo.valor = false;   
                                    break;
                                case "DOUBLE":
                                    if ((Int32)Convert.ToChar(val1.valor) > Convert.ToDouble(val2.valor))
                                        nuevo.valor = true;
                                    else
                                        nuevo.valor = false;   
                                    break;
                                case "CHAR":
                                    if ((Int32)Convert.ToChar(val1.valor) > (Int32)Convert.ToChar(val2.valor))
                                        nuevo.valor = true;
                                    else
                                        nuevo.valor = false;   
                                    break;
                                case "STRING":
                                    a = (Int32)Convert.ToChar(val1.valor);
                                    string auxx = Convert.ToString(val2.valor);
                                    for (int i = 0; i < auxx.Count(); i++)                                    
                                        b+= (Int32)Convert.ToChar(auxx[i]);                                    
                                    if (a > b)
                                        nuevo.valor = true;
                                    else
                                        nuevo.valor = false;
                                    break;
                                case "BOOL":
                                    //ERROR
                                	detError = "> No puede comparar un valor tipo CHAR con uno tipo BOOL";
                                    break;
                            }
                            break;
                        case "STRING":                            
                            switch (val2.tipo) {
                                case "INT":
                                    //ERROR
                                	detError = "> No puede comparar un valor tipo STRING con uno tipo INT";
                                    break;
                                case "DOUBLE":
                                    //ERROR
                                	detError = "> No puede comparar un valor tipo STRING con uno tipo DOUBLE";
                                    break;
                                case "CHAR":
                                    b = (Int32)Convert.ToChar(val2.valor);
                                    string auxx = Convert.ToString(val1.valor);
                                    for (int i = 0; i < auxx.Count(); i++)                                    
                                        a+= (Int32)Convert.ToChar(auxx[i]);                                    
                                    if (a > b)
                                        nuevo.valor = true;
                                    else
                                        nuevo.valor = false;
                                    break;
                                case "STRING":
                                    string auxx2 = Convert.ToString(val1.valor);
                                    for (int i = 0; i < auxx2.Count(); i++)                                    
                                        a+= (Int32)Convert.ToChar(auxx2[i]);                                    
                                    string auxx3 = Convert.ToString(val2.valor);
                                    for (int i = 0; i < auxx3.Count(); i++)                                    
                                        b+= (Int32)Convert.ToChar(auxx3[i]);                                    
                                    if (a > b)
                                        nuevo.valor = true;
                                    else
                                        nuevo.valor = false;
                                    break;
                                case "BOOL":
                                    //ERROR
                                	detError = "> No puede comparar un valor tipo STRING con uno tipo BOOL";
                                    break;
                            }
                            break;
                        case "BOOL":
                            //ERROR
                            switch (val2.tipo) {
                                case "INT":
                                	detError = "> No puede comparar un valor tipo BOOL con uno tipo INT";
                                    break;
                                case "DOUBLE":
                                	detError = "> No puede comparar un valor tipo BOOL con uno tipo DOUBLE";
                                    break;
                                case "CHAR":
                                	detError = "> No puede comparar un valor tipo BOOL con uno tipo CHAR";
                                    break;
                                case "STRING":
                                	detError = "> No puede comparar un valor tipo BOOL con uno tipo STRING";
                                    break;
                                case "BOOL":
                                	detError = "> No puede comparar un valor tipo BOOL con uno tipo BOOL";
                                    break;
                            }
                            break;
                    }
                    break;

                case ">=":
                    switch (val1.tipo) {
                        case "INT":
                            switch (val2.tipo) {
                                case "INT":
                                    if (Convert.ToInt32(val1.valor) >= Convert.ToInt32(val2.valor))
                                        nuevo.valor = true;
                                    else
                                        nuevo.valor = false;
                                    break;
                                case "DOUBLE":
                                    if (Convert.ToInt32(val1.valor) >= Convert.ToDouble(val2.valor))
                                        nuevo.valor = true;
                                    else
                                        nuevo.valor = false;
                                    break;
                                case "CHAR":
                                    if (Convert.ToInt32(val1.valor) >= (Int32)Convert.ToChar(val2.valor))
                                        nuevo.valor = true;
                                    else
                                        nuevo.valor = false;
                                    break;
                                case "STRING":
                                    //ERROR
                                	detError = ">= No puede comparar un valor tipo INT con uno STRING";
                                    break;
                                case "BOOL":
                                    //ERROR
                                	detError = ">= No puede comparar un valor tipo INT con uno BOOL";
                                    break;
                            }
                            break;
                        case "DOUBLE":
                            switch (val2.tipo) {
                                case "INT":
                                    if (Convert.ToDouble(val1.valor) >= Convert.ToInt32(val2.valor))
                                        nuevo.valor = true;
                                    else
                                        nuevo.valor = false;
                                    break;
                                case "DOUBLE":
                                    if (Convert.ToDouble(val1.valor) >= Convert.ToDouble(val2.valor))
                                        nuevo.valor = true;
                                    else
                                        nuevo.valor = false;
                                    break;
                                case "CHAR":
                                    if (Convert.ToDouble(val1.valor) >= (Int32)Convert.ToChar(val2.valor))
                                        nuevo.valor = true;
                                    else
                                        nuevo.valor = false;
                                    break;
                                case "STRING":
                                    //ERRROR
                                	detError = ">= No pede comparar un valor tipo DOUBLE con uno STRING";
                                    break;
                                case "BOOL":
                                    //ERROR
                                	detError = ">= No puede comparar un valor tipo DOUBLE con uno BOOL";
                                    break;
                            }
                            break;
                        case "CHAR":
                            switch (val2.tipo) {
                                case "INT":
                                    if ((Int32)Convert.ToChar(val1.valor) >= Convert.ToInt32(val2.valor))
                                        nuevo.valor = true;
                                    else
                                        nuevo.valor = false;
                                    break;
                                case "DOUBLE":
                                    if ((Int32)Convert.ToChar(val1.valor) >= Convert.ToDouble(val2.valor))
                                        nuevo.valor = true;
                                    else
                                        nuevo.valor = false;
                                    break;
                                case "CHAR":
                                    if ((Int32)Convert.ToChar(val1.valor) >= (Int32)Convert.ToChar(val2.valor))
                                        nuevo.valor = true;
                                    else
                                        nuevo.valor = false;
                                    break;
                                case "STRING":
                                    a = (Int32)Convert.ToChar(val1.valor);
                                    string auxx = Convert.ToString(val2.valor);
                                    for (int i = 0; i < auxx.Count(); i++)                                    
                                        b += (Int32)Convert.ToChar(auxx[i]);                                    
                                    if (a >= b)
                                        nuevo.valor = true;
                                    else
                                        nuevo.valor = false;
                                    break;
                                case "BOOL":
                                    //ERROR
                                	detError = ">= No puede comparar un valor tipo CHAR con uno tipo BOOL";
                                    break;
                            }
                            break;
                        case "STRING":
                            switch (val2.tipo) {
                                case "INT":
                                    //ERROR
                                	detError = ">= No puede comparar un valor tipo STRING con uno tipo INT";
                                    break;
                                case "DOUBLE":
                                    //ERROR
                                	detError = ">= No puede comparar un valor tipo STRING con uno tipo DOUBLE";
                                    break;
                                case "CHAR":
                                    b = (Int32)Convert.ToChar(val2.valor);
                                    string auxx = Convert.ToString(val1.valor);
                                    for (int i = 0; i < auxx.Count(); i++)                                    
                                        a += (Int32)Convert.ToChar(auxx[i]);                                    
                                    if (a >= b)
                                        nuevo.valor = true;
                                    else
                                        nuevo.valor = false;
                                    break;
                                case "STRING":
                                    string auxx2 = Convert.ToString(val1.valor);
                                    for (int i = 0; i < auxx2.Count(); i++)                                    
                                        a += (Int32)Convert.ToChar(auxx2[i]);                                    
                                    string auxx3 = Convert.ToString(val2.valor);
                                    for (int i = 0; i < auxx3.Count(); i++)                                    
                                        b += (Int32)Convert.ToChar(auxx3[i]);                                    
                                    if (a >= b)
                                        nuevo.valor = true;
                                    else
                                        nuevo.valor = false;
                                    break;
                                case "BOOL":
                                    //ERROR
                                	detError = ">= No puede comparar un valor tipo STRING con uno tipo BOOL";
                                    break;
                            }
                            break;
                        case "BOOL":
                            //ERROR
                            switch (val2.tipo) {
                                case "INT":
                                	detError = ">= No puede comparar un valor tipo BOOL con uno tipo INT";
                                    break;
                                case "DOUBLE":
                                	detError = ">= No puede comparar un valor tipo BOOL con uno tipo DOUBLE";
                                    break;
                                case "CHAR":
                                	detError = ">= No puede comparar un valor tipo BOOL con uno tipo CHAR";
                                    break;
                                case "STRING":
                                	detError = ">= No puede comparar un valor tipo BOOL con uno tipo STRING";
                                    break;
                                case "BOOL":
                                	detError = ">= No puede comparar un valor tipo BOOL con uno tipo BOOL";
                                    break;
                            }
                            break;
                    }
                    break;

                case "<":
                    switch (val1.tipo) {
                        case "INT":
                            switch (val2.tipo) {
                                case "INT":
                                    if (Convert.ToInt32(val1.valor) < Convert.ToInt32(val2.valor))
                                        nuevo.valor = true;
                                    else
                                        nuevo.valor = false;
                                    break;
                                case "DOUBLE":
                                    if (Convert.ToInt32(val1.valor) < Convert.ToDouble(val2.valor))
                                        nuevo.valor = true;
                                    else
                                        nuevo.valor = false;
                                    break;
                                case "CHAR":
                                    if (Convert.ToInt32(val1.valor) < (Int32)Convert.ToChar(val2.valor))
                                        nuevo.valor = true;
                                    else
                                        nuevo.valor = false;
                                    break;
                                case "STRING":
                                    //ERROR
                                	detError = "< No puede comparar un valor tipo INT con uno STRING";
                                    break;
                                case "BOOL":
                                    //ERROR
                                	detError = "< No puede comparar un valor tipo INT con uno BOOL";
                                    break;
                            }
                            break;
                        case "DOUBLE":
                            switch (val2.tipo) {
                                case "INT":
                                    if (Convert.ToDouble(val1.valor) < Convert.ToInt32(val2.valor))
                                        nuevo.valor = true;
                                    else
                                        nuevo.valor = false;
                                    break;
                                case "DOUBLE":
                                    if (Convert.ToDouble(val1.valor) < Convert.ToDouble(val2.valor))
                                        nuevo.valor = true;
                                    else
                                        nuevo.valor = false;
                                    break;
                                case "CHAR":
                                    if (Convert.ToDouble(val1.valor) < (Int32)Convert.ToChar(val2.valor))
                                        nuevo.valor = true;
                                    else
                                        nuevo.valor = false;
                                    break;
                                case "STRING":
                                    //ERRROR
                                	detError = "< No pede comparar un valor tipo DOUBLE con uno STRING";
                                    break;
                                case "BOOL":
                                    //ERROR
                                	detError = "< No puede comparar un valor tipo DOUBLE con uno BOOL";
                                    break;
                            }
                            break;
                        case "CHAR":
                            switch (val2.tipo) {
                                case "INT":
                                    if ((Int32)Convert.ToChar(val1.valor) < Convert.ToInt32(val2.valor))
                                        nuevo.valor = true;
                                    else
                                        nuevo.valor = false;
                                    break;
                                case "DOUBLE":
                                    if ((Int32)Convert.ToChar(val1.valor) < Convert.ToDouble(val2.valor))
                                        nuevo.valor = true;
                                    else
                                        nuevo.valor = false;
                                    break;
                                case "CHAR":
                                    if ((Int32)Convert.ToChar(val1.valor) < (Int32)Convert.ToChar(val2.valor))
                                        nuevo.valor = true;
                                    else
                                        nuevo.valor = false;
                                    break;
                                case "STRING":
                                    a = (Int32)Convert.ToChar(val1.valor);
                                    string auxx = Convert.ToString(val2.valor);
                                    for (int i = 0; i < auxx.Count(); i++)                                    
                                        b += (Int32)Convert.ToChar(auxx[i]);                                    
                                    if (a < b)
                                        nuevo.valor = true;
                                    else
                                        nuevo.valor = false;
                                    break;
                                case "BOOL":
                                    //ERROR
                                	detError = "< No puede comparar un valor tipo CHAR con uno tipo BOOL";
                                    break;
                            }
                            break;
                        case "STRING":
                            switch (val2.tipo) {
                                case "INT":
                                    //ERROR
                                	detError = "< No puede comparar un valor tipo STRING con uno tipo INT";
                                    break;
                                case "DOUBLE":
                                    //ERROR
                                	detError = "< No puede comparar un valor tipo STRING con uno tipo DOUBLE";
                                    break;
                                case "CHAR":
                                    b = (Int32)Convert.ToChar(val2.valor);
                                    string auxx = Convert.ToString(val1.valor);
                                    for (int i = 0; i < auxx.Count(); i++)                                    
                                        a += (Int32)Convert.ToChar(auxx[i]);                                    
                                    if (a < b)
                                        nuevo.valor = true;
                                    else
                                        nuevo.valor = false;
                                    break;
                                case "STRING":
                                    string auxx2 = Convert.ToString(val1.valor);
                                    for (int i = 0; i < auxx2.Count(); i++)                                    
                                        a += (Int32)Convert.ToChar(auxx2[i]);                                    
                                    string auxx3 = Convert.ToString(val2.valor);
                                    for (int i = 0; i < auxx3.Count(); i++)                                    
                                        b += (Int32)Convert.ToChar(auxx3[i]);                                    
                                    if (a < b)
                                        nuevo.valor = true;
                                    else
                                        nuevo.valor = false;
                                    break;
                                case "BOOL":
                                    //ERROR
                                	detError = "< No puede comparar un valor tipo STRING con uno tipo BOOL";
                                    break;
                            }
                            break;
                        case "BOOL":
                            //ERROR
                            switch (val2.tipo) {
                                case "INT":
                                	detError = "< No puede comparar un valor tipo BOOL con uno tipo INT";
                                    break;
                                case "DOUBLE":
                                	detError = "< No puede comparar un valor tipo BOOL con uno tipo DOUBLE";
                                    break;
                                case "CHAR":
                                	detError = "< No puede comparar un valor tipo BOOL con uno tipo CHAR";
                                    break;
                                case "STRING":
                                	detError = "< No puede comparar un valor tipo BOOL con uno tipo STRING";
                                    break;
                                case "BOOL":
                                	detError = "< No puede comparar un valor tipo BOOL con uno tipo BOOL";
                                    break;
                            }
                            break;
                    }
                    break;

                case "<=":
                    switch (val1.tipo) {
                        case "INT":
                            switch (val2.tipo) {
                                case "INT":
                                    if (Convert.ToInt32(val1.valor) <= Convert.ToInt32(val2.valor))
                                        nuevo.valor = true;
                                    else
                                        nuevo.valor = false;
                                    break;
                                case "DOUBLE":
                                    if (Convert.ToInt32(val1.valor) <= Convert.ToDouble(val2.valor))
                                        nuevo.valor = true;
                                    else
                                        nuevo.valor = false;
                                    break;
                                case "CHAR":
                                    if (Convert.ToInt32(val1.valor) <= (Int32)Convert.ToChar(val2.valor))
                                        nuevo.valor = true;
                                    else
                                        nuevo.valor = false;
                                    break;
                                case "STRING":
                                    //ERROR
                                	detError = "<= No puede comparar un valor tipo INT con uno STRING";
                                    break;
                                case "BOOL":
                                    //ERROR
                                	detError = "<= No puede comparar un valor tipo INT con uno BOOL";
                                    break;
                            }
                            break;
                        case "DOUBLE":
                            switch (val2.tipo) {
                                case "INT":
                                    if (Convert.ToDouble(val1.valor) <= Convert.ToInt32(val2.valor))
                                        nuevo.valor = true;
                                    else
                                        nuevo.valor = false;
                                    break;
                                case "DOUBLE":
                                    if (Convert.ToDouble(val1.valor) <= Convert.ToDouble(val2.valor))
                                        nuevo.valor = true;
                                    else
                                        nuevo.valor = false;
                                    break;
                                case "CHAR":
                                    if (Convert.ToDouble(val1.valor) <= (Int32)Convert.ToChar(val2.valor))
                                        nuevo.valor = true;
                                    else
                                        nuevo.valor = false;
                                    break;
                                case "STRING":
                                    //ERRROR
                                	detError = "<= No pede comparar un valor tipo DOUBLE con uno STRING";
                                    break;
                                case "BOOL":
                                    //ERROR
                                	detError = "<= No puede comparar un valor tipo DOUBLE con uno BOOL";
                                    break;
                            }
                            break;
                        case "CHAR":
                            switch (val2.tipo) {
                                case "INT":
                                    if ((Int32)Convert.ToChar(val1.valor) <= Convert.ToInt32(val2.valor))
                                        nuevo.valor = true;
                                    else
                                        nuevo.valor = false;
                                    break;
                                case "DOUBLE":
                                    if ((Int32)Convert.ToChar(val1.valor) <= Convert.ToDouble(val2.valor))
                                        nuevo.valor = true;
                                    else
                                        nuevo.valor = false;
                                    break;
                                case "CHAR":
                                    if ((Int32)Convert.ToChar(val1.valor) <= (Int32)Convert.ToChar(val2.valor))
                                        nuevo.valor = true;
                                    else
                                        nuevo.valor = false;
                                    break;
                                case "STRING":
                                    a = (Int32)Convert.ToChar(val1.valor);
                                    string auxx = Convert.ToString(val2.valor);
                                    for (int i = 0; i < auxx.Count(); i++)                                    
                                        b += (Int32)Convert.ToChar(auxx[i]);                                    
                                    if (a <= b)
                                        nuevo.valor = true;
                                    else
                                        nuevo.valor = false;
                                    break;
                                case "BOOL":
                                    //ERROR
                                	detError = "<= No puede comparar un valor tipo CHAR con uno tipo BOOL";
                                    break;
                            }
                            break;
                        case "STRING":
                            switch (val2.tipo) {
                                case "INT":
                                    //ERROR
                                	detError = "<= No puede comparar un valor tipo STRING con uno tipo INT";
                                    break;
                                case "DOUBLE":
                                    //ERROR
                                	detError = "<= No puede comparar un valor tipo STRING con uno tipo DOUBLE";
                                    break;
                                case "CHAR":
                                    b = (Int32)Convert.ToChar(val2.valor);
                                    string auxx = Convert.ToString(val1.valor);
                                    for (int i = 0; i < auxx.Count(); i++)                                    
                                        a += (Int32)Convert.ToChar(auxx[i]);                                    
                                    if (a <= b)
                                        nuevo.valor = true;
                                    else
                                        nuevo.valor = false;
                                    break;
                                case "STRING":
                                    string auxx2 = Convert.ToString(val1.valor);
                                    for (int i = 0; i < auxx2.Count(); i++)                                    
                                        a += (Int32)Convert.ToChar(auxx2[i]);                                    
                                    string auxx3 = Convert.ToString(val2.valor);
                                    for (int i = 0; i < auxx3.Count(); i++)                                    
                                        b += (Int32)Convert.ToChar(auxx3[i]);                                    
                                    if (a <= b)
                                        nuevo.valor = true;
                                    else
                                        nuevo.valor = false;
                                    break;
                                case "BOOL":
                                    //ERROR
                                	detError = "<= No puede comparar un valor tipo STRING con uno tipo BOOL";
                                    break;
                            }
                            break;
                        case "BOOL":
                            //ERROR
                            switch (val2.tipo) {
                                case "INT":
                                	detError = "<= No puede comparar un valor tipo BOOL con uno tipo INT";
                                    break;
                                case "DOUBLE":
                                	detError = "<= No puede comparar un valor tipo BOOL con uno tipo DOUBLE";
                                    break;
                                case "CHAR":
                                	detError = "<= No puede comparar un valor tipo BOOL con uno tipo CHAR";
                                    break;
                                case "STRING":
                                	detError = "<= No puede comparar un valor tipo BOOL con uno tipo STRING";
                                    break;
                                case "BOOL":
                                	detError = "<= No puede comparar un valor tipo BOOL con uno tipo BOOL";
                                    break;
                            }
                            break;
                    }
                    break;

                case "==":
                    switch (val1.tipo) {
                        case "INT":
                            switch (val2.tipo) {
                                case "INT":
                                    if (Convert.ToInt32(val1.valor) == Convert.ToInt32(val2.valor))
                                        nuevo.valor = true;
                                    else
                                        nuevo.valor = false;
                                    break;
                                case "DOUBLE":
                                    if (Convert.ToInt32(val1.valor) == Convert.ToDouble(val2.valor))
                                        nuevo.valor = true;
                                    else
                                        nuevo.valor = false;
                                    break;
                                case "CHAR":
                                    if (Convert.ToInt32(val1.valor) == (Int32)Convert.ToChar(val2.valor))
                                        nuevo.valor = true;
                                    else
                                        nuevo.valor = false;
                                    break;
                                case "STRING":
                                    //ERROR
                                	detError = "== No puede comparar un valor tipo INT con uno STRING";
                                    break;
                                case "BOOL":
                                    //ERROR
                                	detError = "== No puede comparar un valor tipo INT con uno BOOL";
                                    break;
                            }
                            break;
                        case "DOUBLE":
                            switch (val2.tipo) {
                                case "INT":
                                    if (Convert.ToDouble(val1.valor) == Convert.ToInt32(val2.valor))
                                        nuevo.valor = true;
                                    else
                                        nuevo.valor = false;
                                    break;
                                case "DOUBLE":
                                    if (Convert.ToDouble(val1.valor) == Convert.ToDouble(val2.valor))
                                        nuevo.valor = true;
                                    else
                                        nuevo.valor = false;
                                    break;
                                case "CHAR":
                                    if (Convert.ToDouble(val1.valor) == (Int32)Convert.ToChar(val2.valor))
                                        nuevo.valor = true;
                                    else
                                        nuevo.valor = false;
                                    break;
                                case "STRING":
                                    //ERRROR
                                	detError = "== No pede comparar un valor tipo DOUBLE con uno STRING";
                                    break;
                                case "BOOL":
                                    //ERROR
                                	detError = "== No puede comparar un valor tipo DOUBLE con uno BOOL";
                                    break;
                            }
                            break;
                        case "CHAR":
                            switch (val2.tipo) {
                                case "INT":
                                    if ((Int32)Convert.ToChar(val1.valor) == Convert.ToInt32(val2.valor))
                                        nuevo.valor = true;
                                    else
                                        nuevo.valor = false;
                                    break;
                                case "DOUBLE":
                                    if ((Int32)Convert.ToChar(val1.valor) == Convert.ToDouble(val2.valor))
                                        nuevo.valor = true;
                                    else
                                        nuevo.valor = false;
                                    break;
                                case "CHAR":
                                    if ((Int32)Convert.ToChar(val1.valor) == (Int32)Convert.ToChar(val2.valor))
                                        nuevo.valor = true;
                                    else
                                        nuevo.valor = false;
                                    break;
                                case "STRING":
                                    a = (Int32)Convert.ToChar(val1.valor);
                                    string auxx = Convert.ToString(val2.valor);
                                    for (int i = 0; i < auxx.Count(); i++)                                    
                                        b += (Int32)Convert.ToChar(auxx[i]);                                    
                                    if (a == b)
                                        nuevo.valor = true;
                                    else
                                        nuevo.valor = false;
                                    break;
                                case "BOOL":
                                    //ERROR
                                	detError = "== No puede comparar un valor tipo CHAR con uno tipo BOOL";
                                    break;
                            }
                            break;
                        case "STRING":
                            switch (val2.tipo) {
                                case "INT":
                                    //ERROR
                                	detError = "== No puede comparar un valor tipo STRING con uno tipo INT";
                                    break;
                                case "DOUBLE":
                                    //ERROR
                                	detError = "== No puede comparar un valor tipo STRING con uno tipo DOUBLE";
                                    break;
                                case "CHAR":
                                    b = (Int32)Convert.ToChar(val2.valor);
                                    string auxx = Convert.ToString(val1.valor);
                                    for (int i = 0; i < auxx.Count(); i++)                                    
                                        a += (Int32)Convert.ToChar(auxx[i]);                                    
                                    if (a == b)
                                        nuevo.valor = true;
                                    else
                                        nuevo.valor = false;
                                    break;
                                case "STRING":
                                    string auxx2 = Convert.ToString(val1.valor);
                                    for (int i = 0; i < auxx2.Count(); i++)                                    
                                        a += (Int32)Convert.ToChar(auxx2[i]);                                    
                                    string auxx3 = Convert.ToString(val2.valor);
                                    for (int i = 0; i < auxx3.Count(); i++)                                    
                                        b += (Int32)Convert.ToChar(auxx3[i]);                                    
                                    if (a == b)
                                        nuevo.valor = true;
                                    else
                                        nuevo.valor = false;
                                    break;
                                case "BOOL":
                                    //ERROR
                                	detError = "== No puede comparar un valor tipo STRING con uno tipo BOOL";
                                    break;
                            }
                            break;
                        case "BOOL":
                            //ERROR
                            switch (val2.tipo) {
                                case "INT":
                                	detError = "== No puede comparar un valor tipo BOOL con uno tipo INT";
                                    break;
                                case "DOUBLE":
                                	detError = "== No puede comparar un valor tipo BOOL con uno tipo DOUBLE";
                                    break;
                                case "CHAR":
                                	detError = "== No puede comparar un valor tipo BOOL con uno tipo CHAR";
                                    break;
                                case "STRING":
                                	detError = "== No puede comparar un valor tipo BOOL con uno tipo STRING";
                                    break;
                                case "BOOL":
                                	detError = "== No puede comparar un valor tipo BOOL con uno tipo BOOL";
                                    break;
                            }
                            break;
                    }
                    break;

                case "!=":
                    switch (val1.tipo) {
                        case "INT":
                            switch (val2.tipo) {
                                case "INT":
                                    if (Convert.ToInt32(val1.valor) != Convert.ToInt32(val2.valor))
                                        nuevo.valor = true;
                                    else
                                        nuevo.valor = false;
                                    break;
                                case "DOUBLE":
                                    if (Convert.ToInt32(val1.valor) != Convert.ToDouble(val2.valor))
                                        nuevo.valor = true;
                                    else
                                        nuevo.valor = false;
                                    break;
                                case "CHAR":
                                    if (Convert.ToInt32(val1.valor) != (Int32)Convert.ToChar(val2.valor))
                                        nuevo.valor = true;
                                    else
                                        nuevo.valor = false;
                                    break;
                                case "STRING":
                                    //ERROR
                                	detError = "!= No puede comparar un valor tipo INT con uno STRING";
                                    break;
                                case "BOOL":
                                    //ERROR
                                	detError = "!= No puede comparar un valor tipo INT con uno BOOL";
                                    break;
                            }
                            break;
                        case "DOUBLE":
                            switch (val2.tipo) {
                                case "INT":
                                    if (Convert.ToDouble(val1.valor) != Convert.ToInt32(val2.valor))
                                        nuevo.valor = true;
                                    else
                                        nuevo.valor = false;
                                    break;
                                case "DOUBLE":
                                    if (Convert.ToDouble(val1.valor) != Convert.ToDouble(val2.valor))
                                        nuevo.valor = true;
                                    else
                                        nuevo.valor = false;
                                    break;
                                case "CHAR":
                                    if (Convert.ToDouble(val1.valor) != (Int32)Convert.ToChar(val2.valor))
                                        nuevo.valor = true;
                                    else
                                        nuevo.valor = false;
                                    break;
                                case "STRING":
                                    //ERRROR
                                	detError = "!= No pede comparar un valor tipo DOUBLE con uno STRING";
                                    break;
                                case "BOOL":
                                    //ERROR
                                	detError = "!= No puede comparar un valor tipo DOUBLE con uno BOOL";
                                    break;
                            }
                            break;
                        case "CHAR":
                            switch (val2.tipo) {
                                case "INT":
                                    if ((Int32)Convert.ToChar(val1.valor) != Convert.ToInt32(val2.valor))
                                        nuevo.valor = true;
                                    else
                                        nuevo.valor = false;
                                    break;
                                case "DOUBLE":
                                    if ((Int32)Convert.ToChar(val1.valor) != Convert.ToDouble(val2.valor))
                                        nuevo.valor = true;
                                    else
                                        nuevo.valor = false;
                                    break;
                                case "CHAR":
                                    if ((Int32)Convert.ToChar(val1.valor) != (Int32)Convert.ToChar(val2.valor))
                                        nuevo.valor = true;
                                    else
                                        nuevo.valor = false;
                                    break;
                                case "STRING":
                                    a = (Int32)Convert.ToChar(val1.valor);
                                    string auxx = Convert.ToString(val2.valor);
                                    for (int i = 0; i < auxx.Count(); i++)                                    
                                        b += (Int32)Convert.ToChar(auxx[i]);                                    
                                    if (a != b)
                                        nuevo.valor = true;
                                    else
                                        nuevo.valor = false;
                                    break;
                                case "BOOL":
                                    //ERROR
                                	detError = "!= No puede comparar un valor tipo CHAR con uno tipo BOOL";
                                    break;
                            }
                            break;
                        case "STRING":
                            switch (val2.tipo) {
                                case "INT":
                                    //ERROR
                                	detError = "!= No puede comparar un valor tipo STRING con uno tipo INT";
                                    break;
                                case "DOUBLE":
                                    //ERROR
                                	detError = "!= No puede comparar un valor tipo STRING con uno tipo DOUBLE";
                                    break;
                                case "CHAR":
                                    b = (Int32)Convert.ToChar(val2.valor);
                                    string auxx = Convert.ToString(val1.valor);
                                    for (int i = 0; i < auxx.Count(); i++)                                    
                                        a += (Int32)Convert.ToChar(auxx[i]);                                    
                                    if (a != b)
                                        nuevo.valor = true;
                                    else
                                        nuevo.valor = false;
                                    break;
                                case "STRING":
                                    string auxx2 = Convert.ToString(val1.valor);
                                    for (int i = 0; i < auxx2.Count(); i++)                                    
                                        a += (Int32)Convert.ToChar(auxx2[i]);                                    
                                    string auxx3 = Convert.ToString(val2.valor);
                                    for (int i = 0; i < auxx3.Count(); i++)                                    
                                        b += (Int32)Convert.ToChar(auxx3[i]);                                    
                                    if (a != b)
                                        nuevo.valor = true;
                                    else
                                        nuevo.valor = false;
                                    break;
                                case "BOOL":
                                    //ERROR
                                	detError = "!= No puede comparar un valor tipo STRING con uno tipo BOOL";
                                    break;
                            }
                            break;
                        case "BOOL":
                            //ERROR
                            switch (val2.tipo) {
                                case "INT":
                                	detError = "!= No puede comparar un valor tipo BOOL con uno tipo INT";
                                    break;
                                case "DOUBLE":
                                	detError = "!= No puede comparar un valor tipo BOOL con uno tipo DOUBLE";
                                    break;
                                case "CHAR":
                                	detError = "!= No puede comparar un valor tipo BOOL con uno tipo CHAR";
                                    break;
                                case "STRING":
                                	detError = "!= No puede comparar un valor tipo BOOL con uno tipo STRING";
                                    break;
                                case "BOOL":
                                	detError = "!= No puede comparar un valor tipo BOOL con uno tipo BOOL";
                                    break;
                            }
                            break;
                    }
                    break;
            }
            if (detError != "") {
                if (val1.error != true && val2.error != true) {
                    nwnError.Add("semantico");
                    nwnError.Add(detError);
                    nwnError.Add(fila.ToString());
                    nwnError.Add(columna.ToString());
                    nwnError.Add(arch);
                    lista.Add(nwnError);
                    nuevo.error = true;
                }                
                nuevo.valor = false;
            }
            if (val1.error == true || val2.error == true) {
                nuevo.error = true;
            }
            nuevo.tipo = "BOOL";
            return nuevo;
        }

        public bool unoCero(int dato) {
            if (dato==1)            
                return true;            
            return false;
        }

        public Mtrx arr(object addHijo,int cantHijo,int cantPadre) {
            Mtrx padre = new Mtrx();            
            if (cantPadre != 0){ //dos o mas dimensiones            
                padre.sizeT = cantPadre;
                for (int i = 0; i < cantPadre; i++) {
                    Mtrx hijo = new Mtrx();
                    hijo.sizeT = cantHijo;
                    for (int j = 0; j < cantHijo; j++)                    
                        hijo.elem.Add(addHijo);                   
                    padre.elem.Add(hijo);
                }
            } else { //una sola dimension
                padre.sizeT = cantHijo;
                for (int j = 0; j < cantHijo; j++)                
                    padre.elem.Add(addHijo);                
            }            
            return padre;
        }
        
    }
}
