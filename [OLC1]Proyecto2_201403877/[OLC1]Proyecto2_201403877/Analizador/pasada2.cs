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
    class pasada2
    {
        //INICIALIZADOS AL CREAR CLASE
        public List<Tupla> tsG = new List<Tupla>();
        public List<Object> erroreSMT = new List<Object>();
        public string clsPrin = "";

        //CONTENIDO DE EJECUCION
        public string output = "";
        public string diagrama = "";
        
        //CONTROL DE AMBITOS
        int actual = 0;
        List<Ambito> pila = new List<Ambito>();

        //PROCEDIMIENTOS
        bool llamada = false;
        //CICLOS Y CONDICIONES
        bool interrupt = false;
        //SWITCH
        List<Tupla> expCase = new List<Tupla>();
        Tupla swt = null;
        bool sigue = false;

        //NATIVO
        public String consola = "";
        public void iniciar() {
            Ambito principal = new Ambito();            
            for (int i = 0; i < tsG.Count; i++) {
                if (tsG.ElementAt(i).ambito == clsPrin && tsG.ElementAt(i).rol == "main") {
                    //añadir primer ambito a pila
                    principal.ambito = "main";
                    principal.padre = tsG.ElementAt(i).ambito;
                    principal.tipo = "VOID";
                    pila.Add(principal);
                    //iniciar recorrido de main
                    pRec(tsG.ElementAt(i).root);
                    break;
                }
            }
        }

        private Tupla pRec(ParseTreeNode root) {
            Tupla nuevo = new Tupla();//nuevo creado para devolver valores
            String temp = ""; //temporal para nombre de nodo
            String[] valor;   //temporal split
            Tupla aux;//auxiliar retorno
            Tupla aux2;//auxiliar retorno                        
            bool flag = false;// hay o no error
            bool subFlag = false;//validar ambito tabla simbolos
            Ambito principal = new Ambito();
            Ambito acc;

            switch (root.Term.Name.ToString()) {
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
                    if (root.ChildNodes.Count == 2) {
                        pRec(root.ChildNodes.ElementAt(0));
                        pRec(root.ChildNodes.ElementAt(1));
                    }
                    else
                        pRec(root.ChildNodes.ElementAt(0));
                    break;
                case "CLS_P":
                    /*                        
                            | tVar + DECL + tPtoComa
                            | MTRX                                   
                            | ASIG + tPtoComa
                            | OUT                     
                            | LL_PROC + tPtoComa                            
                            | RETURN
                            
                            | IF
                            | WHL                      
                            | FOR  
                                                 
                            | SWT                                                                                    
                            | DO_WHL
                            | RPT_UNT
                                                      
                            *| GRAF                                                        
                            | BRK
                            | CNT                
                            
                     */
                    if (llamada!=true && interrupt!=true){

                        if (root.ChildNodes.Count == 3) 
                            pRec(root.ChildNodes.ElementAt(1));
                        else if(root.ChildNodes.Count==2){
                            temp = root.ChildNodes.ElementAt(0).ToString().Trim();
                            if (temp.Equals("LL_PROC")){//LLAMADA A PROCEDIMIENTO
                                aux = pRec(root.ChildNodes.ElementAt(0));
                                //MessageBox.Show(aux.nombrre);
                                if (aux.dimension.Count == 0){                            	
                                    //BUSCAR EN ORIGEN
                                    List<String> imp = new List<string>();//IMPORTACION DE ORIGEN
                                    bool tipPar = false;                                    
                                    for (int i = 0; i < tsG.Count; i++){
                                        if (tsG.ElementAt(i).ambito == pila.ElementAt(actual).padre) { 
                                            if(tsG.ElementAt(i).rol == "proc"){
                                                if (tsG.ElementAt(i).nombrre == aux.nombrre && tsG.ElementAt(i).parametros.Count == aux.parametros.Count) {
                                                    for (int j = 0; j < tsG.ElementAt(i).parametros.Count; j++){
                                                        if (tsG.ElementAt(i).parametros.ElementAt(j).tipo != aux.parametros.ElementAt(j).tipo) { 
                                                            tipPar = true;
                                                            break;
                                                        }
                                                    }
                                                
                                                    if (tipPar != true){ //AÑADIR AMBITO DE PROCEDIMIENTO   
                                                        //MessageBox.Show("llamada");                                                    
                                                        principal.ambito = tsG.ElementAt(i).nombrre;
                                                        principal.padre = tsG.ElementAt(i).ambito;
                                                        principal.tipo = tsG.ElementAt(i).tipo;
                                                        pila.Add(principal);
                                                        actual++;
                                                        //AGREGAR PARAMETROS A AMBITO ACTUAL
                                                        for (int k = 0; k < tsG.ElementAt(i).parametros.Count; k++){
                                                            Tupla auxM = new Tupla();
                                                            auxM.tipo = tsG.ElementAt(i).parametros.ElementAt(k).tipo;
                                                            auxM.nombrre = tsG.ElementAt(i).parametros.ElementAt(k).nombrre;
                                                            auxM.valor = aux.parametros.ElementAt(k).valor;
                                                            auxM.rol = "var";
                                                            auxM.ambito = tsG.ElementAt(i).nombrre;
                                                            pila.ElementAt(actual).tabla.Add(auxM);                                                                
                                                        }
                                                        //iniciar recorrido de PROCEDIMIENTO
                                                        pRec(tsG.ElementAt(i).root);   
                                                        //TERMINA
                                                        pila.RemoveAt(actual);
                                                        actual--;
                                                        llamada = false;
                                                    }else{
                                                        //ERROR
                                                        addError("Se realizo la llamada a un procedimiento pero el tipo de los valores enviados como parametros no coinciden", root.Span.Location.Line, root.Span.Location.Column);                                                    
                                                    }                                                                                                  
                                                    flag = true;
                                                    break;
                                                }
                                            }else if (tsG.ElementAt(i).rol == "import") {
                                                imp.Add(tsG.ElementAt(i).nombrre);
                                            }
                                        }
                                    }
                                    //SI NO EXISTE EN EL ORIGEN
                                    if (flag == false) {
                                        bool salir = true;
                                        int salAux = 0;
                                        while (salir) {
                                            //RECORRER LOS IMPORT
                                            for (int i  = 0; i  < imp.Count; i ++){
                                                for (int j = 0; j < tsG.Count; j++){
                                                    if (tsG.ElementAt(j).ambito == imp.ElementAt(i)) { //AMBITO DE IMPORT
                                                        if (tsG.ElementAt(j).rol == "proc") { //ES PROCEDIMIENTO
                                                            if (tsG.ElementAt(j).nombrre == aux.nombrre && tsG.ElementAt(j).parametros.Count == aux.parametros.Count) { //MISMO NOMBRE Y NUM DE PARAMETROS
                                                                bool exx = false;
                                                                for (int k = 0; k < tsG.ElementAt(i).parametros.Count; k++){
                                                                    if (tsG.ElementAt(j).parametros.ElementAt(k).tipo != aux.parametros.ElementAt(k).tipo){
                                                                        exx = true;
                                                                        break;
                                                                    }
                                                                }                                                            
                                                                if (exx != true){//TODO COINCIDE
                                                                    principal.ambito = tsG.ElementAt(j).nombrre;
                                                                    principal.padre = tsG.ElementAt(j).ambito;
                                                                    principal.tipo = tsG.ElementAt(j).tipo;
                                                                    pila.Add(principal);
                                                                    actual++;
                                                                    //AGREGAR PARAMETROS A AMBITO ACTUAL
                                                                    for (int k = 0; k < tsG.ElementAt(j).parametros.Count; k++)
                                                                    {
                                                                        Tupla auxM = new Tupla();
                                                                        auxM.tipo = tsG.ElementAt(j).parametros.ElementAt(k).tipo;
                                                                        auxM.nombrre = tsG.ElementAt(j).parametros.ElementAt(k).nombrre;
                                                                        auxM.valor = aux.parametros.ElementAt(k).valor;
                                                                        auxM.rol = "var";
                                                                        auxM.ambito = tsG.ElementAt(j).nombrre;
                                                                        pila.ElementAt(actual).tabla.Add(auxM);
                                                                    }
                                                                    //iniciar recorrido de PROCEDIMIENTO
                                                                    pRec(tsG.ElementAt(j).root);
                                                                    //TERMINA                                                                
                                                                    pila.RemoveAt(actual);
                                                                    actual--;
                                                                    llamada = false;
                                                                }else {
                                                                    //ERROR
                                                                    addError("Se realizo la llamada a un procedimiento pero el tipo de los valores enviados como parametros no coinciden", root.Span.Location.Line, root.Span.Location.Column);                                                                
                                                                }                                                            
                                                                flag = true;
                                                                salir = false;
                                                                break;
                                                            }
                                                        }
                                                    }
                                                }
                                            }
                                            //AGREGAR MAS IMPORT
                                            if (salir != false) {
                                                List<String> tempImp = new List<string>();
                                                for (int i = 0; i < imp.Count; i++){
                                                    for (int j = 0; j < tsG.Count; j++){
                                                        if (tsG.ElementAt(j).ambito == imp.ElementAt(i) && tsG.ElementAt(j).rol=="import") {
                                                            tempImp.Add(tsG.ElementAt(j).nombrre);
                                                        }
                                                    }
                                                }
                                                imp.Clear();
                                                imp.AddRange(tempImp);
                                            }
                                            if (imp.Count==0 || salAux > 9)
                                                salir = false;
                                            salAux++;
                                        }
                                        if (flag != true) {
                                            //ERROR
                                            addError("No existe ningun procedimiento que coincida con los datos proporcionados en la llamada", root.Span.Location.Line, root.Span.Location.Column);                                        
                                        }
                                    }
                            	
                                }else { 
                                    //ERROR
                                    addError("Se intenta acceder a una posicion de arreglo pero no se utiliza ese valor", root.Span.Location.Line, root.Span.Location.Column);                                        
                                }

                            }else{
                                pRec(root.ChildNodes.ElementAt(0));
                            }
                        }else
                            pRec(root.ChildNodes.ElementAt(0));

                    }
                    break;
                case "TIPO":
                    /*
                        TIPO.Rule = tInt
                            | tDouble
                            | tString
                            | tChar
                            | tBool
                            ;
                     */                    
                    valor = root.ChildNodes.ElementAt(0).ToString().Split(' ');
                    nuevo.tipo = valor[0];
                    return nuevo;
                case "DECL":
                    /*
                        DECL.Rule = DECL + tComa + tId
                            | TIPO + tDosPtos + tId
                     */
                    aux = pRec(root.ChildNodes.ElementAt(0));
                    //agregar valor por defecto   
                    aux.valor = valDefault(aux.tipo).valor;                                                     
                    //agregar id y rol
                    valor = root.ChildNodes.ElementAt(2).ToString().Split(' ');
                    aux.nombrre = valor[0];
                    aux.rol = "var";
                    //EN AMBITO GENERAL, PILA
                    if (pila.ElementAt(actual).hay == false) {                        
                        for (int i = 0; i < pila.ElementAt(actual).tabla.Count; i++) {
                            if (pila.ElementAt(actual).tabla.ElementAt(i).nombrre == aux.nombrre) { 
                                //ERROR
                                addError("Variable. Ya existe un elemento definido con el mismo nombre en el ambito actual.", root.Span.Location.Line, root.Span.Location.Column);                                
                                flag = true;
                                break;
                            }
                        }
                        if (flag == false) //AGREGAR A LA TABLA DEL AMBITO ACTUAL
                            pila.ElementAt(actual).tabla.Add(aux);                                                    
                    } else { //EN SUB AMBITO 
                        acc = pila.ElementAt(actual);
                        for (int i = 0; i < acc.pila.ElementAt(acc.actual).tabla.Count; i++) {
                            if (acc.pila.ElementAt(acc.actual).tabla.ElementAt(i).nombrre == aux.nombrre) {
                                //ERROR                                
                                addError("Variable. Ya existe un elemento definido con el mismo nombre en el ambito actual.", root.Span.Location.Line, root.Span.Location.Column);
                                flag = true;
                                break;
                            }
                        }
                        if (flag == false) //AGREGAR A LA TABLA DE SUB AMBITO ACTUAL
                            acc.pila.ElementAt(acc.actual).tabla.Add(aux);                        
                    }                    
                    //SIMULAR RETORNO DE 'TIPO', POR SI ES UNA LISTA
                    nuevo.tipo = aux.tipo;
                    return nuevo;
                case "MTRX":
                    /*
                        MTRX.Rule = tVar + tId + tOf + TIPO + LST_DM + tPtoComa
                     */
                    aux = pRec(root.ChildNodes.ElementAt(3));//tipo
                    aux2 = pRec(root.ChildNodes.ElementAt(4));//dimensiones
                    if (aux2.error == false){
                        valor = root.ChildNodes.ElementAt(1).ToString().Split(' ');
                        nuevo.nombrre = valor[0];
                        nuevo.rol = "var";
                        nuevo.dimension = aux2.dimension;
                        nuevo.tipo = aux.tipo;
                        //INICIALIZAR
                        //Mtrx valIni = null;
                        aux.valor = valDefault(aux.tipo).valor;

                        //MessageBox.Show(aux.valor.ToString());

                        //if (nuevo.dimension.Count == 1)
                        //    valIni = arr(aux.valor, nuevo.dimension.ElementAt(0), 0);
                        //else
                        //{
                        //    //MessageBox.Show(nuevo.dimension.Count.ToString());
                        //    object auxiliar = aux.valor;
                        //    for (int i = nuevo.dimension.Count - 1; i >= 0; i--)
                        //    {
                        //        /*if (i == nuevo.dimension.Count - 1)
                        //            valIni = arr(aux.valor, nuevo.dimension.ElementAt(i), nuevo.dimension.ElementAt(i - 1));
                        //        else
                        //            valIni = arr(valIni, nuevo.dimension.ElementAt(i), nuevo.dimension.ElementAt(i - 1));*/
                        //        Mtrx nx = new Mtrx();
                        //        for (int j = 0; j < nuevo.dimension.ElementAt(i); j++)
                        //        {
                        //            object extt = new object();
                        //            extt = auxiliar;
                        //            nx.elem.Add(extt);
                        //        }
                        //        auxiliar = nx;
                        //    }
                        //    valIni = (Mtrx)auxiliar;
                        //}
                        //nuevo.valor = valIni;

                        if (nuevo.dimension.Count == 1) {
                            switch (nuevo.tipo)
                            {
                                case "STRING":
                                    string [] arreglo = new string[nuevo.dimension.ElementAt(0)];
                                    nuevo.valor = arreglo;
                                    break;
                                case "INT":
                                    int [] arreglo1 = new int[nuevo.dimension.ElementAt(0)];
                                    nuevo.valor = arreglo1;
                                    break;
                                case "DOUBLE":
                                    double [] arreglo2 = new double[nuevo.dimension.ElementAt(0)];
                                    nuevo.valor = arreglo2;
                                    break;
                                case "CHAR":
                                    char [] arreglo3 = new char[nuevo.dimension.ElementAt(0)];
                                    nuevo.valor = arreglo3;
                                    break;
                                case "BOOL":
                                    bool [] arreglo4 = new bool[nuevo.dimension.ElementAt(0)];
                                    nuevo.valor = arreglo4;
                                    break;
                            }
                        }
                        else if (nuevo.dimension.Count == 2) {
                            switch (nuevo.tipo)
                            {
                                case "STRING":
                                    string[,] arreglo = new string[nuevo.dimension.ElementAt(0), nuevo.dimension.ElementAt(1)];
                                    nuevo.valor = arreglo;
                                    break;
                                case "INT":
                                    int[,] arreglo1 = new int[nuevo.dimension.ElementAt(0), nuevo.dimension.ElementAt(1)];
                                    nuevo.valor = arreglo1;
                                    break;
                                case "DOUBLE":
                                    double[,] arreglo2 = new double[nuevo.dimension.ElementAt(0),nuevo.dimension.ElementAt(1)];
                                    nuevo.valor = arreglo2;
                                    break;
                                case "CHAR":
                                    char[,] arreglo3 = new char[nuevo.dimension.ElementAt(0), nuevo.dimension.ElementAt(1)];
                                    nuevo.valor = arreglo3;
                                    break;
                                case "BOOL":
                                    bool[,] arreglo4 = new bool[nuevo.dimension.ElementAt(0), nuevo.dimension.ElementAt(1)];
                                    nuevo.valor = arreglo4;
                                    break;
                            }
                        }else  if (nuevo.dimension.Count == 3) {
                            switch (nuevo.tipo)
                            {
                                case "STRING":
                                    string[,,] arreglo = new string[nuevo.dimension.ElementAt(0), nuevo.dimension.ElementAt(1), nuevo.dimension.ElementAt(2)];
                                    nuevo.valor = arreglo;
                                    break;
                                case "INT":
                                    int[, ,] arreglo1 = new int[nuevo.dimension.ElementAt(0), nuevo.dimension.ElementAt(1), nuevo.dimension.ElementAt(2)];
                                    nuevo.valor = arreglo1;
                                    break;
                                case "DOUBLE":
                                    double[,,] arreglo2 = new double[nuevo.dimension.ElementAt(0), nuevo.dimension.ElementAt(1), nuevo.dimension.ElementAt(2)];
                                    nuevo.valor = arreglo2;
                                    break;
                                case "CHAR":
                                    char[,,] arreglo3 = new char[nuevo.dimension.ElementAt(0), nuevo.dimension.ElementAt(1), nuevo.dimension.ElementAt(2)];
                                    nuevo.valor = arreglo3;
                                    break;
                                case "BOOL":
                                    bool[,,] arreglo4 = new bool[nuevo.dimension.ElementAt(0), nuevo.dimension.ElementAt(1), nuevo.dimension.ElementAt(2)];
                                    nuevo.valor = arreglo4;
                                    break;
                            }
                        }
                        else if (nuevo.dimension.Count == 4) {
                            switch (nuevo.tipo)
                            {
                                case "STRING":
                                    string[,, ,] arreglo = new string[nuevo.dimension.ElementAt(0), nuevo.dimension.ElementAt(1), nuevo.dimension.ElementAt(2), nuevo.dimension.ElementAt(3)];
                                    nuevo.valor = arreglo;
                                    break;
                                case "INT":
                                    int[, ,,] arreglo1 = new int[nuevo.dimension.ElementAt(0), nuevo.dimension.ElementAt(1), nuevo.dimension.ElementAt(2), nuevo.dimension.ElementAt(3)];
                                    nuevo.valor = arreglo1;
                                    break;
                                case "DOUBLE":
                                    double[, , ,] arreglo2 = new double[nuevo.dimension.ElementAt(0), nuevo.dimension.ElementAt(1), nuevo.dimension.ElementAt(2), nuevo.dimension.ElementAt(3)];
                                    nuevo.valor = arreglo2;
                                    break;
                                case "CHAR":
                                    char[, , ,] arreglo3 = new char[nuevo.dimension.ElementAt(0), nuevo.dimension.ElementAt(1), nuevo.dimension.ElementAt(2), nuevo.dimension.ElementAt(3)];
                                    nuevo.valor = arreglo3;
                                    break;
                                case "BOOL":
                                    bool[, , ,] arreglo4 = new bool[nuevo.dimension.ElementAt(0), nuevo.dimension.ElementAt(1), nuevo.dimension.ElementAt(2), nuevo.dimension.ElementAt(3)];
                                    nuevo.valor = arreglo4;
                                    break;
                            }
                        }

                        /*Mtrx b = (Mtrx)valIni.elem.ElementAt(0);
                        Mtrx c = (Mtrx)b.elem.ElementAt(0);
                        Mtrx d = (Mtrx)c.elem.ElementAt(0);
                        Mtrx e = (Mtrx)d.elem.ElementAt(0);
                        Mtrx f = (Mtrx)e.elem.ElementAt(0);
                        Mtrx g = (Mtrx)f.elem.ElementAt(0);                        
                        String a = Convert.ToString(g.elem.ElementAt(0));
                        MessageBox.Show(a);*/
                        //EN AMBITO GENERAL, PILA
                        if (pila.ElementAt(actual).hay == false){
                            for (int i = 0; i < pila.ElementAt(actual).tabla.Count; i++){
                                if (pila.ElementAt(actual).tabla.ElementAt(i).nombrre == nuevo.nombrre){
                                    //ERROR
                                    addError("Arreglo. Ya existe un elemento definido con el mismo nombre en el ambito actual.", root.Span.Location.Line, root.Span.Location.Column);
                                    flag = true;
                                    break;
                                }
                            }
                            if (flag == false) //AGREGAR A LA TABLA DEL AMBITO ACTUAL
                                pila.ElementAt(actual).tabla.Add(nuevo);
                        } else { //EN SUB AMBITO 
                            acc = pila.ElementAt(actual);
                            for (int i = 0; i < acc.pila.ElementAt(acc.actual).tabla.Count; i++){
                                if (acc.pila.ElementAt(acc.actual).tabla.ElementAt(i).nombrre == nuevo.nombrre){
                                    //ERROR                                
                                    addError("Variable. Ya existe un elemento definido con el mismo nombre en el ambito actual.", root.Span.Location.Line, root.Span.Location.Column);
                                    flag = true;
                                    break;
                                }
                            }
                            if (flag == false) //AGREGAR A LA TABLA DE SUB AMBITO ACTUAL
                                acc.pila.ElementAt(acc.actual).tabla.Add(nuevo);
                        }                                                                                                                                                                                                
                    }
                    break;
                case "LST_DM":
                    /*
                        LST_DM.Rule = MakePlusRule(LST_DM,EXP) 
                     */
                    int nn = root.ChildNodes.Count;
                    for (int i = 0; i < nn; i++) {
                        aux = exp(root.ChildNodes.ElementAt(i));
                        if (aux.error != true) {
                            if (aux.tipo == "INT") {
                                if (Convert.ToInt32(aux.valor) > 0)
                                    nuevo.dimension.Add(Convert.ToInt32(aux.valor));
                                else {
                                    //ERROR
                                    nuevo.error = true;                                                 
                                    addError("Arreglo. Se intento definir un tamaño de dimension inferior a 1.", root.Span.Location.Line, root.Span.Location.Column);                                    
                                }
                            } else {
                                //ERROR
                                nuevo.error = true;                                                                
                                addError("Arreglo. Tipo de dato invalido para definir dimension.", root.Span.Location.Line, root.Span.Location.Column);
                            }
                        }
                        else
                            nuevo.error = true; 
                    }
                    return nuevo;
                case "EXP":
                    return exp(root);
                case "ASIG":
                    /*
                        ASIG.Rule = ACC_MTRX + OPC_ASIG
                            | tId + OPC_ASIG
                     */
                    

                    temp = root.ChildNodes.ElementAt(0).ToString().Trim();
                    if (temp.Equals("ACC_MTRX")) {
                        aux = pRec(root.ChildNodes.ElementAt(0));
                        aux2 = pRec(root.ChildNodes.ElementAt(1));
                        bool dimVer = false;
                        if (aux.error != true && aux2.error != true) { //NINGUNO TRAE ERRORES
                            if (aux.parametros.Count == 0){
                                if (aux.dimension.Count >= 1){//ES UN ARREGLO                                    
                                    if (pila.ElementAt(actual).hay == false) { //AMBITO ACTUAL-------------------------------------------

                                        for (int i = 0; i < pila.ElementAt(actual).tabla.Count; i++){
                                            if (pila.ElementAt(actual).tabla.ElementAt(i).nombrre == aux.nombrre) {
                                                if (pila.ElementAt(actual).tabla.ElementAt(i).dimension.Count > 0){ //SI ES ARREGLO
                                                    if (pila.ElementAt(actual).tabla.ElementAt(i).dimension.Count == aux.dimension.Count){//MISMO NUMERO DE DIMENSIONES
                                                        for (int j = 0; j < pila.ElementAt(actual).tabla.ElementAt(i).dimension.Count; j++){
                                                            if (pila.ElementAt(actual).tabla.ElementAt(i).dimension.ElementAt(j)-1 < aux.dimension.ElementAt(j)) {
                                                                dimVer = true;
                                                                break;
                                                            }
                                                        }
                                                        if (dimVer == false){                                                            
                                                                //Mtrx mt = (Mtrx)pila.ElementAt(actual).tabla.ElementAt(i).valor;                                                                
                                                            if (aux2.aumento == false){//ASIGNACION NORMAL
	                                                            if (aux2.tipo == pila.ElementAt(actual).tabla.ElementAt(i).tipo){
                                                                    //Mtrx mt = (Mtrx)pila.ElementAt(actual).tabla.ElementAt(i).valor;
                                                                    //if (aux.dimension.Count == 1){//UNA DIMENSION
                                                                    //    mt.elem[aux.dimension.ElementAt(0)] = aux2.valor;
                                                                    //    return aux;
                                                                    //}else{ // N DIMENSIONES
                                                                    //    for (int k = 0; k < aux.dimension.Count; k++){
                                                                    //        if (k != aux.dimension.Count - 1){ //NO ES ULTIMO ELEMNTO
                                                                    //            mt = (Mtrx)mt.elem[aux.dimension.ElementAt(k)];
                                                                    //        }else{ //ULTIMO ELEMENTO	                                                                            
                                                                    //            mt.elem[aux.dimension.ElementAt(k)] = aux2.valor;
                                                                                
                                                                    //            return aux;
                                                                    //        }
                                                                    //    }
                                                                    //}
                                                                    switch(pila.ElementAt(actual).tabla.ElementAt(i).tipo)
                                                                    {
                                                                        case "INT":
                                                                            if (aux.dimension.Count == 1) {
                                                                                int[] a = (int[])pila.ElementAt(actual).tabla.ElementAt(i).valor;
                                                                                a[aux.dimension.ElementAt(0)] = Convert.ToInt32(aux2.valor);
                                                                            }
                                                                            else if (aux.dimension.Count == 2) {
                                                                                int[,] a = (int[,])pila.ElementAt(actual).tabla.ElementAt(i).valor;
                                                                                a[aux.dimension.ElementAt(0), aux.dimension.ElementAt(1)] = Convert.ToInt32(aux2.valor);
                                                                            }
                                                                            else if (aux.dimension.Count == 3)
                                                                            {
                                                                                int[,,] a = (int[,,])pila.ElementAt(actual).tabla.ElementAt(i).valor;
                                                                                a[aux.dimension.ElementAt(0), aux.dimension.ElementAt(1), aux.dimension.ElementAt(2)] = Convert.ToInt32(aux2.valor);
                                                                            }
                                                                            else if (aux.dimension.Count == 4)
                                                                            {
                                                                                int[, ,,] a = (int[, ,,])pila.ElementAt(actual).tabla.ElementAt(i).valor;
                                                                                a[aux.dimension.ElementAt(0), aux.dimension.ElementAt(1), aux.dimension.ElementAt(2), aux.dimension.ElementAt(3)] = Convert.ToInt32(aux2.valor);
                                                                            }
                                                                            break;
                                                                        case "DOUBLE":
                                                                            if (aux.dimension.Count == 1)
                                                                            {
                                                                                double[] a = (double[])pila.ElementAt(actual).tabla.ElementAt(i).valor;
                                                                                a[aux.dimension.ElementAt(0)] = Convert.ToDouble(aux2.valor);
                                                                            }
                                                                            else if (aux.dimension.Count == 2)
                                                                            {
                                                                                double[,] a = (double[,])pila.ElementAt(actual).tabla.ElementAt(i).valor;
                                                                                a[aux.dimension.ElementAt(0), aux.dimension.ElementAt(1)] = Convert.ToDouble(aux2.valor);
                                                                            }
                                                                            else if (aux.dimension.Count == 3)
                                                                            {
                                                                                double[, ,] a = (double[, ,])pila.ElementAt(actual).tabla.ElementAt(i).valor;
                                                                                a[aux.dimension.ElementAt(0), aux.dimension.ElementAt(1), aux.dimension.ElementAt(2)] = Convert.ToDouble(aux2.valor);
                                                                            }
                                                                            else if (aux.dimension.Count == 4)
                                                                            {
                                                                                double[, , ,] a = (double[, , ,])pila.ElementAt(actual).tabla.ElementAt(i).valor;
                                                                                a[aux.dimension.ElementAt(0), aux.dimension.ElementAt(1), aux.dimension.ElementAt(2), aux.dimension.ElementAt(3)] = Convert.ToDouble(aux2.valor);
                                                                            }
                                                                            break;
                                                                        case "CHAR":
                                                                            if (aux.dimension.Count == 1)
                                                                            {
                                                                                char[] a = (char[])pila.ElementAt(actual).tabla.ElementAt(i).valor;
                                                                                a[aux.dimension.ElementAt(0)] = Convert.ToChar(aux2.valor);
                                                                            }
                                                                            else if (aux.dimension.Count == 2)
                                                                            {
                                                                                char[,] a = (char[,])pila.ElementAt(actual).tabla.ElementAt(i).valor;
                                                                                a[aux.dimension.ElementAt(0), aux.dimension.ElementAt(1)] = Convert.ToChar(aux2.valor);
                                                                            }
                                                                            else if (aux.dimension.Count == 3)
                                                                            {
                                                                                char[, ,] a = (char[, ,])pila.ElementAt(actual).tabla.ElementAt(i).valor;
                                                                                a[aux.dimension.ElementAt(0), aux.dimension.ElementAt(1), aux.dimension.ElementAt(2)] = Convert.ToChar(aux2.valor);
                                                                            }
                                                                            else if (aux.dimension.Count == 4)
                                                                            {
                                                                                char[, , ,] a = (char[, , ,])pila.ElementAt(actual).tabla.ElementAt(i).valor;
                                                                                a[aux.dimension.ElementAt(0), aux.dimension.ElementAt(1), aux.dimension.ElementAt(2), aux.dimension.ElementAt(3)] = Convert.ToChar(aux2.valor);
                                                                            }
                                                                            break;
                                                                        case "STRING":
                                                                            if (aux.dimension.Count == 1)
                                                                            {
                                                                                string[] a = (string[])pila.ElementAt(actual).tabla.ElementAt(i).valor;
                                                                                a[aux.dimension.ElementAt(0)] = Convert.ToString(aux2.valor);
                                                                            }
                                                                            else if (aux.dimension.Count == 2)
                                                                            {
                                                                                string[,] a = (string[,])pila.ElementAt(actual).tabla.ElementAt(i).valor;
                                                                                a[aux.dimension.ElementAt(0), aux.dimension.ElementAt(1)] = Convert.ToString(aux2.valor);
                                                                            }
                                                                            else if (aux.dimension.Count == 3)
                                                                            {
                                                                                string[, ,] a = (string[, ,])pila.ElementAt(actual).tabla.ElementAt(i).valor;
                                                                                a[aux.dimension.ElementAt(0), aux.dimension.ElementAt(1), aux.dimension.ElementAt(2)] = Convert.ToString(aux2.valor);
                                                                            }
                                                                            else if (aux.dimension.Count == 4)
                                                                            {
                                                                                string[, , ,] a = (string[, , ,])pila.ElementAt(actual).tabla.ElementAt(i).valor;
                                                                                a[aux.dimension.ElementAt(0), aux.dimension.ElementAt(1), aux.dimension.ElementAt(2), aux.dimension.ElementAt(3)] = Convert.ToString(aux2.valor);
                                                                            }
                                                                            break;
                                                                        case "BOOL":
                                                                            if (aux.dimension.Count == 1)
                                                                            {
                                                                                bool[] a = (bool[])pila.ElementAt(actual).tabla.ElementAt(i).valor;
                                                                                a[aux.dimension.ElementAt(0)] = Convert.ToBoolean(aux2.valor);
                                                                            }
                                                                            else if (aux.dimension.Count == 2)
                                                                            {
                                                                                bool[,] a = (bool[,])pila.ElementAt(actual).tabla.ElementAt(i).valor;
                                                                                a[aux.dimension.ElementAt(0), aux.dimension.ElementAt(1)] = Convert.ToBoolean(aux2.valor);
                                                                            }
                                                                            else if (aux.dimension.Count == 3)
                                                                            {
                                                                                bool[, ,] a = (bool[, ,])pila.ElementAt(actual).tabla.ElementAt(i).valor;
                                                                                a[aux.dimension.ElementAt(0), aux.dimension.ElementAt(1), aux.dimension.ElementAt(2)] = Convert.ToBoolean(aux2.valor);
                                                                            }
                                                                            else if (aux.dimension.Count == 4)
                                                                            {
                                                                                bool[, , ,] a = (bool[, , ,])pila.ElementAt(actual).tabla.ElementAt(i).valor;
                                                                                a[aux.dimension.ElementAt(0), aux.dimension.ElementAt(1), aux.dimension.ElementAt(2), aux.dimension.ElementAt(3)] = Convert.ToBoolean(aux2.valor);
                                                                            }
                                                                            break;
                                                                    }
                                                                    return aux;
	                                                            }else { 
	                                                                //ERROR
	                                                                addError("GLOBAL. Se intenta asignar un valor que no coincide con el tipo del arreglo", root.Span.Location.Line, root.Span.Location.Column);
                                                                    aux.error = true;
                                                                }
	                                                        }else { //++ O --
	                                                            //Mtrx mt = (Mtrx)pila.ElementAt(actual).tabla.ElementAt(i).valor;
	                                                            switch (pila.ElementAt(actual).tabla.ElementAt(i).tipo){                                                                    
	                                                                case "INT":                                                                    
                                                                        //if (aux.dimension.Count == 1){//UNA DIMENSION
                                                                        //    int a1 = Convert.ToInt32(mt.elem[aux.dimension.ElementAt(0)]);
                                                                        //    if (Convert.ToInt32(aux2.valor) > 0)
                                                                        //        a1++;
                                                                        //    else
                                                                        //        a1--;
                                                                        //    mt.elem[aux.dimension.ElementAt(0)] = a1;
                                                                        //    return aux;
                                                                        //}else{ // N DIMENSIONES
                                                                        //    for (int k = 0; k < aux.dimension.Count; k++){
                                                                        //        if (k != aux.dimension.Count - 1){ //NO ES ULTIMO ELEMNTO
                                                                        //            mt = (Mtrx)mt.elem[aux.dimension.ElementAt(k)];
                                                                        //        }else{ //ULTIMO ELEMENTO
                                                                        //            int a1 = Convert.ToInt32(mt.elem[aux.dimension.ElementAt(k)]);
                                                                        //            if (Convert.ToInt32(aux2.valor) > 0)
                                                                        //                a1++;
                                                                        //            else
                                                                        //                a1--;
                                                                        //            mt.elem[aux.dimension.ElementAt(k)] = a1;
                                                                        //            return aux;
                                                                        //        }
                                                                        //    }
                                                                        //}
                                                                        if (aux.dimension.Count == 1)
                                                                        {
                                                                            int[] a = (int[])pila.ElementAt(actual).tabla.ElementAt(i).valor;
                                                                            a[aux.dimension.ElementAt(0)] += Convert.ToInt32(aux2.valor);
                                                                        }
                                                                        else if (aux.dimension.Count == 2)
                                                                        {
                                                                            int[,] a = (int[,])pila.ElementAt(actual).tabla.ElementAt(i).valor;
                                                                            a[aux.dimension.ElementAt(0), aux.dimension.ElementAt(1)] += Convert.ToInt32(aux2.valor);
                                                                        }
                                                                        else if (aux.dimension.Count == 3)
                                                                        {
                                                                            int[, ,] a = (int[, ,])pila.ElementAt(actual).tabla.ElementAt(i).valor;
                                                                            a[aux.dimension.ElementAt(0), aux.dimension.ElementAt(1), aux.dimension.ElementAt(2)] += Convert.ToInt32(aux2.valor);
                                                                        }
                                                                        else if (aux.dimension.Count == 4)
                                                                        {
                                                                            int[, , ,] a = (int[, , ,])pila.ElementAt(actual).tabla.ElementAt(i).valor;
                                                                            a[aux.dimension.ElementAt(0), aux.dimension.ElementAt(1), aux.dimension.ElementAt(2), aux.dimension.ElementAt(3)] += Convert.ToInt32(aux2.valor);
                                                                        }
                                                                        return aux;
	                                                                    //break;
	                                                                case "DOUBLE":                                                                    
                                                                        //if (aux.dimension.Count == 1){//UNA DIMENSION
                                                                        //    double a1 = Convert.ToDouble(mt.elem[aux.dimension.ElementAt(0)]);
                                                                        //    if (Convert.ToInt32(aux2.valor) > 0)
                                                                        //        a1++;
                                                                        //    else
                                                                        //        a1--;
                                                                        //    mt.elem[aux.dimension.ElementAt(0)] = a1;
                                                                        //    return aux;
                                                                        //}else{ // N DIMENSIONES
                                                                        //    for (int k = 0; k < aux.dimension.Count; k++){
                                                                        //        if (k != aux.dimension.Count - 1){ //NO ES ULTIMO ELEMNTO
                                                                        //            mt = (Mtrx)mt.elem[aux.dimension.ElementAt(k)];
                                                                        //        }else{ //ULTIMO ELEMENTO
                                                                        //            double a1 = Convert.ToDouble(mt.elem[aux.dimension.ElementAt(k)]);
                                                                        //            if (Convert.ToInt32(aux2.valor) > 0)
                                                                        //                a1++;
                                                                        //            else
                                                                        //                a1--;
                                                                        //            mt.elem[aux.dimension.ElementAt(k)] = a1;
                                                                        //            return aux;
                                                                        //        }
                                                                        //    }
                                                                        //}
                                                                        if (aux.dimension.Count == 1)
                                                                        {
                                                                            double[] a = (double[])pila.ElementAt(actual).tabla.ElementAt(i).valor;
                                                                            a[aux.dimension.ElementAt(0)] += Convert.ToDouble(aux2.valor);
                                                                        }
                                                                        else if (aux.dimension.Count == 2)
                                                                        {
                                                                            double[,] a = (double[,])pila.ElementAt(actual).tabla.ElementAt(i).valor;
                                                                            a[aux.dimension.ElementAt(0), aux.dimension.ElementAt(1)] += Convert.ToDouble(aux2.valor);
                                                                        }
                                                                        else if (aux.dimension.Count == 3)
                                                                        {
                                                                            double[, ,] a = (double[, ,])pila.ElementAt(actual).tabla.ElementAt(i).valor;
                                                                            a[aux.dimension.ElementAt(0), aux.dimension.ElementAt(1), aux.dimension.ElementAt(2)] += Convert.ToDouble(aux2.valor);
                                                                        }
                                                                        else if (aux.dimension.Count == 4)
                                                                        {
                                                                            double[, , ,] a = (double[, , ,])pila.ElementAt(actual).tabla.ElementAt(i).valor;
                                                                            a[aux.dimension.ElementAt(0), aux.dimension.ElementAt(1), aux.dimension.ElementAt(2), aux.dimension.ElementAt(3)] += Convert.ToDouble(aux2.valor);
                                                                        }
                                                                        return aux;
	                                                                    //break;
	                                                                case "CHAR":
                                                                        //if (aux.dimension.Count == 1){//UNA DIMENSION
                                                                        //    int a1 = (Int32)Convert.ToChar(mt.elem[aux.dimension.ElementAt(0)]);
                                                                        //    if (Convert.ToInt32(aux2.valor) > 0)
                                                                        //        a1++;
                                                                        //    else
                                                                        //        a1--;
                                                                        //    mt.elem[aux.dimension.ElementAt(0)] = Convert.ToChar( a1);
                                                                        //    return aux;
                                                                        //}else{ // N DIMENSIONES
                                                                        //    for (int k = 0; k < aux.dimension.Count; k++){
                                                                        //        if (k != aux.dimension.Count - 1){ //NO ES ULTIMO ELEMNTO
                                                                        //            mt = (Mtrx)mt.elem[aux.dimension.ElementAt(k)];
                                                                        //        }else{ //ULTIMO ELEMENTO
                                                                        //            int a1 = (Int32)Convert.ToChar(mt.elem[aux.dimension.ElementAt(k)]);
                                                                        //            if (Convert.ToInt32(aux2.valor) > 0)
                                                                        //                a1++;
                                                                        //            else
                                                                        //                a1--;
                                                                        //            mt.elem[aux.dimension.ElementAt(k)] = Convert.ToChar( a1);
                                                                        //            return aux;
                                                                        //        }
                                                                        //    }
                                                                        //}
                                                                        if (aux.dimension.Count == 1)
                                                                        {
                                                                            char[] a = (char[])pila.ElementAt(actual).tabla.ElementAt(i).valor;
                                                                            a[aux.dimension.ElementAt(0)] += Convert.ToChar(aux2.valor);
                                                                        }
                                                                        else if (aux.dimension.Count == 2)
                                                                        {
                                                                            char[,] a = (char[,])pila.ElementAt(actual).tabla.ElementAt(i).valor;
                                                                            a[aux.dimension.ElementAt(0), aux.dimension.ElementAt(1)] += Convert.ToChar(aux2.valor);
                                                                        }
                                                                        else if (aux.dimension.Count == 3)
                                                                        {
                                                                            char[, ,] a = (char[, ,])pila.ElementAt(actual).tabla.ElementAt(i).valor;
                                                                            a[aux.dimension.ElementAt(0), aux.dimension.ElementAt(1), aux.dimension.ElementAt(2)] += Convert.ToChar(aux2.valor);
                                                                        }
                                                                        else if (aux.dimension.Count == 4)
                                                                        {
                                                                            char[, , ,] a = (char[, , ,])pila.ElementAt(actual).tabla.ElementAt(i).valor;
                                                                            a[aux.dimension.ElementAt(0), aux.dimension.ElementAt(1), aux.dimension.ElementAt(2), aux.dimension.ElementAt(3)] += Convert.ToChar(aux2.valor);
                                                                        }
                                                                        return aux;
	                                                                    //break;
	                                                                default:
	                                                                    //ERROR
	                                                                    addError("GLOBAL. Se intenta aumentar o decrementr el valor de un tipo de dato no valido, STRING o BOOL", root.Span.Location.Line, root.Span.Location.Column);
                                                                        aux.error = true;
                                                                        break;
	                                                            }
	                                                        }                                                            
                                                        }else { 
                                                            //ERROR
                                                            addError("Se intenta asignar un valor a una posicion de un arreglo, pero se excede el valor de las dimensiones establecidas", root.Span.Location.Line, root.Span.Location.Column);
                                                            dimVer = false;
                                                            aux.error = true;
                                                        }
                                                    }else { 
                                                        //  ERROR
                                                        addError("Se intenta asignar un valor a una posicion de un arreglo, pero el numero de dimensiones no coincide", root.Span.Location.Line, root.Span.Location.Column);
                                                        aux.error = true;
                                                    }
                                                } else { //NO ES ARREGLO
                                                    //ERRROR
                                                    addError("Se intenta asignar un valor a una posicion de un arreglo, pero la variable no es un arreglo", root.Span.Location.Line, root.Span.Location.Column);
                                                    aux.error = true;
                                                }
                                                flag = true;
                                                break;
                                            }
                                        }
                                    } else {//SUB AMBITO-----------------------------------------------------
                                        acc = pila.ElementAt(actual);
                                		for (int i = 0; i < acc.pila.ElementAt(acc.actual).tabla.Count; i++){
                                            if (acc.pila.ElementAt(acc.actual).tabla.ElementAt(i).nombrre == aux.nombrre) {
                                                if (acc.pila.ElementAt(acc.actual).tabla.ElementAt(i).dimension.Count > 0){ //SI ES ARREGLO
                                                    if (acc.pila.ElementAt(acc.actual).tabla.ElementAt(i).dimension.Count == aux.dimension.Count){//MISMO NUMERO DE DIMENSIONES
                                                        for (int j = 0; j < acc.pila.ElementAt(acc.actual).tabla.ElementAt(i).dimension.Count; j++){
                                                            if (acc.pila.ElementAt(acc.actual).tabla.ElementAt(i).dimension.ElementAt(j)-1 < aux.dimension.ElementAt(j)) {
                                                                dimVer = true;
                                                                break;
                                                            }
                                                        }
                                                        if (dimVer == false){                                                            
                                                                //Mtrx mt = (Mtrx)acc.pila.ElementAt(acc.actual).tabla.ElementAt(i).valor;                                                               
                                                            if (aux2.aumento == false){//ASIGNACION NORMAL
	                                                            if (aux2.tipo == acc.pila.ElementAt(acc.actual).tabla.ElementAt(i).tipo){
                                                                    //Mtrx mt = (Mtrx)acc.pila.ElementAt(acc.actual).tabla.ElementAt(i).valor;
                                                                    //if (aux.dimension.Count == 1){//UNA DIMENSION
                                                                    //    mt.elem[aux.dimension.ElementAt(0)] = aux2.valor;
                                                                    //    return aux;
                                                                    //}else{ // N DIMENSIONES                                                                                                                                                
                                                                    //    for (int k = 0; k < aux.dimension.Count; k++){
                                                                    //        //MessageBox.Show(aux.dimension.ElementAt(k).ToString());
                                                                    //        if (k != aux.dimension.Count - 1){ //NO ES ULTIMO ELEMNTO
                                                                    //            mt = (Mtrx)mt.elem[aux.dimension.ElementAt(k)];
                                                                    //        }else{ //ULTIMO ELEMENTO
                                                                    //            //MessageBox.Show(Convert.ToString(mt.elem[aux.dimension.ElementAt(k)]));
                                                                    //            //mt.elem.RemoveAt(aux.dimension.ElementAt(k));
                                                                    //            //mt.elem.Insert(aux.dimension.ElementAt(k),aux2.valor);
                                                                    //            mt.elem[aux.dimension.ElementAt(k)] = aux2.valor;
                                                                    //            return aux;
                                                                    //        }
                                                                    //    }                                                                        	                                                                    
                                                                    //}
                                                                    switch (acc.pila.ElementAt(acc.actual).tabla.ElementAt(i).tipo)
                                                                    {
                                                                        case "INT":
                                                                            if (aux.dimension.Count == 1)
                                                                            {
                                                                                int[] a = (int[])acc.pila.ElementAt(acc.actual).tabla.ElementAt(i).valor;
                                                                                a[aux.dimension.ElementAt(0)] = Convert.ToInt32(aux2.valor);
                                                                            }
                                                                            else if (aux.dimension.Count == 2)
                                                                            {
                                                                                int[,] a = (int[,])acc.pila.ElementAt(acc.actual).tabla.ElementAt(i).valor;
                                                                                a[aux.dimension.ElementAt(0), aux.dimension.ElementAt(1)] = Convert.ToInt32(aux2.valor);
                                                                            }
                                                                            else if (aux.dimension.Count == 3)
                                                                            {
                                                                                int[, ,] a = (int[, ,])acc.pila.ElementAt(acc.actual).tabla.ElementAt(i).valor;
                                                                                a[aux.dimension.ElementAt(0), aux.dimension.ElementAt(1), aux.dimension.ElementAt(2)] = Convert.ToInt32(aux2.valor);
                                                                            }
                                                                            else if (aux.dimension.Count == 4)
                                                                            {
                                                                                int[, , ,] a = (int[, , ,])acc.pila.ElementAt(acc.actual).tabla.ElementAt(i).valor;
                                                                                a[aux.dimension.ElementAt(0), aux.dimension.ElementAt(1), aux.dimension.ElementAt(2), aux.dimension.ElementAt(3)] = Convert.ToInt32(aux2.valor);
                                                                            }
                                                                            break;
                                                                        case "DOUBLE":
                                                                            if (aux.dimension.Count == 1)
                                                                            {
                                                                                double[] a = (double[])acc.pila.ElementAt(acc.actual).tabla.ElementAt(i).valor;
                                                                                a[aux.dimension.ElementAt(0)] = Convert.ToDouble(aux2.valor);
                                                                            }
                                                                            else if (aux.dimension.Count == 2)
                                                                            {
                                                                                double[,] a = (double[,])acc.pila.ElementAt(acc.actual).tabla.ElementAt(i).valor;
                                                                                a[aux.dimension.ElementAt(0), aux.dimension.ElementAt(1)] = Convert.ToDouble(aux2.valor);
                                                                            }
                                                                            else if (aux.dimension.Count == 3)
                                                                            {
                                                                                double[, ,] a = (double[, ,])acc.pila.ElementAt(acc.actual).tabla.ElementAt(i).valor;
                                                                                a[aux.dimension.ElementAt(0), aux.dimension.ElementAt(1), aux.dimension.ElementAt(2)] = Convert.ToDouble(aux2.valor);
                                                                            }
                                                                            else if (aux.dimension.Count == 4)
                                                                            {
                                                                                double[, , ,] a = (double[, , ,])acc.pila.ElementAt(acc.actual).tabla.ElementAt(i).valor;
                                                                                a[aux.dimension.ElementAt(0), aux.dimension.ElementAt(1), aux.dimension.ElementAt(2), aux.dimension.ElementAt(3)] = Convert.ToDouble(aux2.valor);
                                                                            }
                                                                            break;
                                                                        case "CHAR":
                                                                            if (aux.dimension.Count == 1)
                                                                            {
                                                                                char[] a = (char[])acc.pila.ElementAt(acc.actual).tabla.ElementAt(i).valor;
                                                                                a[aux.dimension.ElementAt(0)] = Convert.ToChar(aux2.valor);
                                                                            }
                                                                            else if (aux.dimension.Count == 2)
                                                                            {
                                                                                char[,] a = (char[,])acc.pila.ElementAt(acc.actual).tabla.ElementAt(i).valor;
                                                                                a[aux.dimension.ElementAt(0), aux.dimension.ElementAt(1)] = Convert.ToChar(aux2.valor);
                                                                            }
                                                                            else if (aux.dimension.Count == 3)
                                                                            {
                                                                                char[, ,] a = (char[, ,])acc.pila.ElementAt(acc.actual).tabla.ElementAt(i).valor;
                                                                                a[aux.dimension.ElementAt(0), aux.dimension.ElementAt(1), aux.dimension.ElementAt(2)] = Convert.ToChar(aux2.valor);
                                                                            }
                                                                            else if (aux.dimension.Count == 4)
                                                                            {
                                                                                char[, , ,] a = (char[, , ,])acc.pila.ElementAt(acc.actual).tabla.ElementAt(i).valor;
                                                                                a[aux.dimension.ElementAt(0), aux.dimension.ElementAt(1), aux.dimension.ElementAt(2), aux.dimension.ElementAt(3)] = Convert.ToChar(aux2.valor);
                                                                            }
                                                                            break;
                                                                        case "STRING":
                                                                            if (aux.dimension.Count == 1)
                                                                            {
                                                                                string[] a = (string[])acc.pila.ElementAt(acc.actual).tabla.ElementAt(i).valor;
                                                                                a[aux.dimension.ElementAt(0)] = Convert.ToString(aux2.valor);
                                                                            }
                                                                            else if (aux.dimension.Count == 2)
                                                                            {
                                                                                string[,] a = (string[,])acc.pila.ElementAt(acc.actual).tabla.ElementAt(i).valor;
                                                                                a[aux.dimension.ElementAt(0), aux.dimension.ElementAt(1)] = Convert.ToString(aux2.valor);
                                                                            }
                                                                            else if (aux.dimension.Count == 3)
                                                                            {
                                                                                string[, ,] a = (string[, ,])acc.pila.ElementAt(acc.actual).tabla.ElementAt(i).valor;
                                                                                a[aux.dimension.ElementAt(0), aux.dimension.ElementAt(1), aux.dimension.ElementAt(2)] = Convert.ToString(aux2.valor);
                                                                            }
                                                                            else if (aux.dimension.Count == 4)
                                                                            {
                                                                                string[, , ,] a = (string[, , ,])acc.pila.ElementAt(acc.actual).tabla.ElementAt(i).valor;
                                                                                a[aux.dimension.ElementAt(0), aux.dimension.ElementAt(1), aux.dimension.ElementAt(2), aux.dimension.ElementAt(3)] = Convert.ToString(aux2.valor);
                                                                            }
                                                                            break;
                                                                        case "BOOL":
                                                                            if (aux.dimension.Count == 1)
                                                                            {
                                                                                bool[] a = (bool[])acc.pila.ElementAt(acc.actual).tabla.ElementAt(i).valor;
                                                                                a[aux.dimension.ElementAt(0)] = Convert.ToBoolean(aux2.valor);
                                                                            }
                                                                            else if (aux.dimension.Count == 2)
                                                                            {
                                                                                bool[,] a = (bool[,])acc.pila.ElementAt(acc.actual).tabla.ElementAt(i).valor;
                                                                                a[aux.dimension.ElementAt(0), aux.dimension.ElementAt(1)] = Convert.ToBoolean(aux2.valor);
                                                                            }
                                                                            else if (aux.dimension.Count == 3)
                                                                            {
                                                                                bool[, ,] a = (bool[, ,])acc.pila.ElementAt(acc.actual).tabla.ElementAt(i).valor;
                                                                                a[aux.dimension.ElementAt(0), aux.dimension.ElementAt(1), aux.dimension.ElementAt(2)] = Convert.ToBoolean(aux2.valor);
                                                                            }
                                                                            else if (aux.dimension.Count == 4)
                                                                            {
                                                                                bool[, , ,] a = (bool[, , ,])acc.pila.ElementAt(acc.actual).tabla.ElementAt(i).valor;
                                                                                a[aux.dimension.ElementAt(0), aux.dimension.ElementAt(1), aux.dimension.ElementAt(2), aux.dimension.ElementAt(3)] = Convert.ToBoolean(aux2.valor);
                                                                            }
                                                                            break;
                                                                    }
                                                                    return aux;
	                                                            }else { 
	                                                                //ERROR
	                                                                addError("GLOBAL. Se intenta asignar un valor que no coincide con el tipo del arreglo", root.Span.Location.Line, root.Span.Location.Column);
                                                                    aux.error = true;
                                                                }
	                                                        }else { //++ O --
	                                                            Mtrx mt = (Mtrx)acc.pila.ElementAt(acc.actual).tabla.ElementAt(i).valor;
	                                                            switch (acc.pila.ElementAt(acc.actual).tabla.ElementAt(i).tipo){                                                                    
	                                                                case "INT":                                                                    
                                                                        //if (aux.dimension.Count == 1){//UNA DIMENSION
                                                                        //    int a1 = Convert.ToInt32(mt.elem[aux.dimension.ElementAt(0)]);
                                                                        //    if (Convert.ToInt32(aux2.valor) > 0)
                                                                        //        a1++;
                                                                        //    else
                                                                        //        a1--;
                                                                        //    mt.elem[aux.dimension.ElementAt(0)] = a1;
                                                                        //    return aux;
                                                                        //}else{ // N DIMENSIONES
                                                                        //    for (int k = 0; k < aux.dimension.Count; k++){
                                                                        //        if (k != aux.dimension.Count - 1){ //NO ES ULTIMO ELEMNTO
                                                                        //            mt = (Mtrx)mt.elem[aux.dimension.ElementAt(k)];
                                                                        //        }else{ //ULTIMO ELEMENTO
                                                                        //            int a1 = Convert.ToInt32(mt.elem[aux.dimension.ElementAt(k)]);
                                                                        //            if (Convert.ToInt32(aux2.valor) > 0)
                                                                        //                a1++;
                                                                        //            else
                                                                        //                a1--;
                                                                        //            mt.elem[aux.dimension.ElementAt(k)] = a1;
                                                                        //            return aux;
                                                                        //        }
                                                                        //    }
                                                                        //}
                                                                        if (aux.dimension.Count == 1)
                                                                        {
                                                                            int[] a = (int[])acc.pila.ElementAt(acc.actual).tabla.ElementAt(i).valor;
                                                                            a[aux.dimension.ElementAt(0)] += Convert.ToInt32(aux2.valor);
                                                                        }
                                                                        else if (aux.dimension.Count == 2)
                                                                        {
                                                                            int[,] a = (int[,])acc.pila.ElementAt(acc.actual).tabla.ElementAt(i).valor;
                                                                            a[aux.dimension.ElementAt(0), aux.dimension.ElementAt(1)] += Convert.ToInt32(aux2.valor);
                                                                        }
                                                                        else if (aux.dimension.Count == 3)
                                                                        {
                                                                            int[, ,] a = (int[, ,])acc.pila.ElementAt(acc.actual).tabla.ElementAt(i).valor;
                                                                            a[aux.dimension.ElementAt(0), aux.dimension.ElementAt(1), aux.dimension.ElementAt(2)] += Convert.ToInt32(aux2.valor);
                                                                        }
                                                                        else if (aux.dimension.Count == 4)
                                                                        {
                                                                            int[, , ,] a = (int[, , ,])acc.pila.ElementAt(acc.actual).tabla.ElementAt(i).valor;
                                                                            a[aux.dimension.ElementAt(0), aux.dimension.ElementAt(1), aux.dimension.ElementAt(2), aux.dimension.ElementAt(3)] += Convert.ToInt32(aux2.valor);
                                                                        }
                                                                        return aux;
	                                                                    //break;
	                                                                case "DOUBLE":                                                                    
                                                                        //if (aux.dimension.Count == 1){//UNA DIMENSION
                                                                        //    double a1 = Convert.ToDouble(mt.elem[aux.dimension.ElementAt(0)]);
                                                                        //    if (Convert.ToInt32(aux2.valor) > 0)
                                                                        //        a1++;
                                                                        //    else
                                                                        //        a1--;
                                                                        //    mt.elem[aux.dimension.ElementAt(0)] = a1;
                                                                        //    return aux;
                                                                        //}else{ // N DIMENSIONES
                                                                        //    for (int k = 0; k < aux.dimension.Count; k++){
                                                                        //        if (k != aux.dimension.Count - 1){ //NO ES ULTIMO ELEMNTO
                                                                        //            mt = (Mtrx)mt.elem[aux.dimension.ElementAt(k)];
                                                                        //        }else{ //ULTIMO ELEMENTO
                                                                        //            double a1 = Convert.ToDouble(mt.elem[aux.dimension.ElementAt(k)]);
                                                                        //            if (Convert.ToInt32(aux2.valor) > 0)
                                                                        //                a1++;
                                                                        //            else
                                                                        //                a1--;
                                                                        //            mt.elem[aux.dimension.ElementAt(k)] = a1;
                                                                        //            return aux;
                                                                        //        }
                                                                        //    }
                                                                        //}
                                                                        if (aux.dimension.Count == 1)
                                                                        {
                                                                            double[] a = (double[])acc.pila.ElementAt(acc.actual).tabla.ElementAt(i).valor;
                                                                            a[aux.dimension.ElementAt(0)] += Convert.ToDouble(aux2.valor);
                                                                        }
                                                                        else if (aux.dimension.Count == 2)
                                                                        {
                                                                            double[,] a = (double[,])acc.pila.ElementAt(acc.actual).tabla.ElementAt(i).valor;
                                                                            a[aux.dimension.ElementAt(0), aux.dimension.ElementAt(1)] += Convert.ToDouble(aux2.valor);
                                                                        }
                                                                        else if (aux.dimension.Count == 3)
                                                                        {
                                                                            double[, ,] a = (double[, ,])acc.pila.ElementAt(acc.actual).tabla.ElementAt(i).valor;
                                                                            a[aux.dimension.ElementAt(0), aux.dimension.ElementAt(1), aux.dimension.ElementAt(2)] += Convert.ToDouble(aux2.valor);
                                                                        }
                                                                        else if (aux.dimension.Count == 4)
                                                                        {
                                                                            double[, , ,] a = (double[, , ,])acc.pila.ElementAt(acc.actual).tabla.ElementAt(i).valor;
                                                                            a[aux.dimension.ElementAt(0), aux.dimension.ElementAt(1), aux.dimension.ElementAt(2), aux.dimension.ElementAt(3)] += Convert.ToDouble(aux2.valor);
                                                                        }
                                                                        return aux;
	                                                                    //break;
	                                                                case "CHAR":
                                                                        //if (aux.dimension.Count == 1){//UNA DIMENSION
                                                                        //    int a1 = (Int32)Convert.ToChar(mt.elem[aux.dimension.ElementAt(0)]);
                                                                        //    if (Convert.ToInt32(aux2.valor) > 0)
                                                                        //        a1++;
                                                                        //    else
                                                                        //        a1--;
                                                                        //    mt.elem[aux.dimension.ElementAt(0)] = Convert.ToChar( a1);
                                                                        //    return aux;
                                                                        //}else{ // N DIMENSIONES
                                                                        //    for (int k = 0; k < aux.dimension.Count; k++){
                                                                        //        if (k != aux.dimension.Count - 1){ //NO ES ULTIMO ELEMNTO
                                                                        //            mt = (Mtrx)mt.elem[aux.dimension.ElementAt(k)];
                                                                        //        }else{ //ULTIMO ELEMENTO
                                                                        //            int a1 = (Int32)Convert.ToChar(mt.elem[aux.dimension.ElementAt(k)]);
                                                                        //            if (Convert.ToInt32(aux2.valor) > 0)
                                                                        //                a1++;
                                                                        //            else
                                                                        //                a1--;
                                                                        //            mt.elem[aux.dimension.ElementAt(k)] = Convert.ToChar( a1);
                                                                        //            return aux;
                                                                        //        }
                                                                        //    }
                                                                        //}
                                                                        if (aux.dimension.Count == 1)
                                                                        {
                                                                            char[] a = (char[])acc.pila.ElementAt(acc.actual).tabla.ElementAt(i).valor;
                                                                            a[aux.dimension.ElementAt(0)] += Convert.ToChar(aux2.valor);
                                                                        }
                                                                        else if (aux.dimension.Count == 2)
                                                                        {
                                                                            char[,] a = (char[,])acc.pila.ElementAt(acc.actual).tabla.ElementAt(i).valor;
                                                                            a[aux.dimension.ElementAt(0), aux.dimension.ElementAt(1)] += Convert.ToChar(aux2.valor);
                                                                        }
                                                                        else if (aux.dimension.Count == 3)
                                                                        {
                                                                            char[, ,] a = (char[, ,])acc.pila.ElementAt(acc.actual).tabla.ElementAt(i).valor;
                                                                            a[aux.dimension.ElementAt(0), aux.dimension.ElementAt(1), aux.dimension.ElementAt(2)] += Convert.ToChar(aux2.valor);
                                                                        }
                                                                        else if (aux.dimension.Count == 4)
                                                                        {
                                                                            char[, , ,] a = (char[, , ,])acc.pila.ElementAt(acc.actual).tabla.ElementAt(i).valor;
                                                                            a[aux.dimension.ElementAt(0), aux.dimension.ElementAt(1), aux.dimension.ElementAt(2), aux.dimension.ElementAt(3)] += Convert.ToChar(aux2.valor);
                                                                        }
                                                                        return aux;
	                                                                    //break;
	                                                                default:
	                                                                    //ERROR
	                                                                    addError("GLOBAL. Se intenta aumentar o decrementr el valor de un tipo de dato no valido, STRING o BOOL", root.Span.Location.Line, root.Span.Location.Column);
                                                                        aux.error = true;
                                                                        break;
	                                                            }
	                                                        }                                                            
                                                        }else { 
                                                            //ERROR
                                                            addError("Se intenta asignar un valor a una posicion de un arreglo, pero se excede el valor de las dimensiones establecidas", root.Span.Location.Line, root.Span.Location.Column);
                                                            dimVer = false;
                                                            aux.error = true;
                                                        }
                                                    }else { 
                                                        //  ERROR
                                                        addError("Se intenta asignar un valor a una posicion de un arreglo, pero el numero de dimensiones no coincide", root.Span.Location.Line, root.Span.Location.Column);
                                                        aux.error = true;
                                                    }
                                                } else { //NO ES ARREGLO
                                                    //ERRROR
                                                    addError("Se intenta asignar un valor a una posicion de un arreglo, pero la variable no es un arreglo", root.Span.Location.Line, root.Span.Location.Column);
                                                    aux.error = true;
                                                }
                                                flag = true;
                                                break;
                                            }
                                        }

                                    }
                                    //TABLA GENERAL---------------------------------------------------------------
                                    if (flag == false) {
                                        for (int i = 0; i < tsG.Count; i++){
                                            if (tsG.ElementAt(i).ambito == pila.ElementAt(actual).padre) {
                                                if (tsG.ElementAt(i).nombrre == aux.nombrre && tsG.ElementAt(i).rol == "var") {
                                                    if (tsG.ElementAt(i).dimension.Count > 0){//SI ES ARREGLO
                                                        if (tsG.ElementAt(i).dimension.Count == aux.dimension.Count){// MISMO NUMERO DE DIMENSIONES
                                                            for (int j = 0; j < tsG.ElementAt(i).dimension.Count; j++){
                                                                if (tsG.ElementAt(i).dimension.ElementAt(j) - 1 < aux.dimension.ElementAt(j)){
                                                                    dimVer = true;
                                                                    break;
                                                                }
                                                            }
                                                            if (dimVer == false){
                                                            	if (aux2.aumento == false){//ASIGNACION NORMAL
                                                                    if (aux2.tipo == tsG.ElementAt(i).tipo){
                                                                    //    Mtrx mt = (Mtrx)tsG.ElementAt(i).valor;
                                                                    //    if (aux.dimension.Count == 1){//UNA DIMENSION
                                                                    //        mt.elem[aux.dimension.ElementAt(0)] = aux2.valor;
                                                                    //        return aux;
                                                                    //    }else{ // N DIMENSIONES
                                                                    //        for (int k = 0; k < aux.dimension.Count; k++){
                                                                    //            if (k != aux.dimension.Count - 1){ //NO ES ULTIMO ELEMNTO
                                                                    //                mt = (Mtrx)mt.elem[aux.dimension.ElementAt(k)];
                                                                    //            }else{ //ULTIMO ELEMENTO
                                                                    //                mt.elem[aux.dimension.ElementAt(k)] = aux2.valor;
                                                                    //                return aux;
                                                                    //            }
                                                                    //        }
                                                                    //    }
                                                                        switch (tsG.ElementAt(i).tipo)
                                                                        {
                                                                            case "INT":
                                                                                if (aux.dimension.Count == 1)
                                                                                {
                                                                                    int[] a = (int[])tsG.ElementAt(i).valor;
                                                                                    a[aux.dimension.ElementAt(0)] = Convert.ToInt32(aux2.valor);
                                                                                }
                                                                                else if (aux.dimension.Count == 2)
                                                                                {
                                                                                    int[,] a = (int[,])tsG.ElementAt(i).valor;
                                                                                    a[aux.dimension.ElementAt(0), aux.dimension.ElementAt(1)] = Convert.ToInt32(aux2.valor);
                                                                                }
                                                                                else if (aux.dimension.Count == 3)
                                                                                {
                                                                                    int[, ,] a = (int[, ,])tsG.ElementAt(i).valor;
                                                                                    a[aux.dimension.ElementAt(0), aux.dimension.ElementAt(1), aux.dimension.ElementAt(2)] = Convert.ToInt32(aux2.valor);
                                                                                }
                                                                                else if (aux.dimension.Count == 4)
                                                                                {
                                                                                    int[, , ,] a = (int[, , ,])tsG.ElementAt(i).valor;
                                                                                    a[aux.dimension.ElementAt(0), aux.dimension.ElementAt(1), aux.dimension.ElementAt(2), aux.dimension.ElementAt(3)] = Convert.ToInt32(aux2.valor);
                                                                                }
                                                                                break;
                                                                            case "DOUBLE":
                                                                                if (aux.dimension.Count == 1)
                                                                                {
                                                                                    double[] a = (double[])tsG.ElementAt(i).valor;
                                                                                    a[aux.dimension.ElementAt(0)] = Convert.ToDouble(aux2.valor);
                                                                                }
                                                                                else if (aux.dimension.Count == 2)
                                                                                {
                                                                                    double[,] a = (double[,])tsG.ElementAt(i).valor;
                                                                                    a[aux.dimension.ElementAt(0), aux.dimension.ElementAt(1)] = Convert.ToDouble(aux2.valor);
                                                                                }
                                                                                else if (aux.dimension.Count == 3)
                                                                                {
                                                                                    double[, ,] a = (double[, ,])tsG.ElementAt(i).valor;
                                                                                    a[aux.dimension.ElementAt(0), aux.dimension.ElementAt(1), aux.dimension.ElementAt(2)] = Convert.ToDouble(aux2.valor);
                                                                                }
                                                                                else if (aux.dimension.Count == 4)
                                                                                {
                                                                                    double[, , ,] a = (double[, , ,])tsG.ElementAt(i).valor;
                                                                                    a[aux.dimension.ElementAt(0), aux.dimension.ElementAt(1), aux.dimension.ElementAt(2), aux.dimension.ElementAt(3)] = Convert.ToDouble(aux2.valor);
                                                                                }
                                                                                break;
                                                                            case "CHAR":
                                                                                if (aux.dimension.Count == 1)
                                                                                {
                                                                                    char[] a = (char[])tsG.ElementAt(i).valor;
                                                                                    a[aux.dimension.ElementAt(0)] = Convert.ToChar(aux2.valor);
                                                                                }
                                                                                else if (aux.dimension.Count == 2)
                                                                                {
                                                                                    char[,] a = (char[,])tsG.ElementAt(i).valor;
                                                                                    a[aux.dimension.ElementAt(0), aux.dimension.ElementAt(1)] = Convert.ToChar(aux2.valor);
                                                                                }
                                                                                else if (aux.dimension.Count == 3)
                                                                                {
                                                                                    char[, ,] a = (char[, ,])tsG.ElementAt(i).valor;
                                                                                    a[aux.dimension.ElementAt(0), aux.dimension.ElementAt(1), aux.dimension.ElementAt(2)] = Convert.ToChar(aux2.valor);
                                                                                }
                                                                                else if (aux.dimension.Count == 4)
                                                                                {
                                                                                    char[, , ,] a = (char[, , ,])tsG.ElementAt(i).valor;
                                                                                    a[aux.dimension.ElementAt(0), aux.dimension.ElementAt(1), aux.dimension.ElementAt(2), aux.dimension.ElementAt(3)] = Convert.ToChar(aux2.valor);
                                                                                }
                                                                                break;
                                                                            case "STRING":
                                                                                if (aux.dimension.Count == 1)
                                                                                {
                                                                                    string[] a = (string[])tsG.ElementAt(i).valor;
                                                                                    a[aux.dimension.ElementAt(0)] = Convert.ToString(aux2.valor);
                                                                                }
                                                                                else if (aux.dimension.Count == 2)
                                                                                {
                                                                                    string[,] a = (string[,])tsG.ElementAt(i).valor;
                                                                                    a[aux.dimension.ElementAt(0), aux.dimension.ElementAt(1)] = Convert.ToString(aux2.valor);
                                                                                }
                                                                                else if (aux.dimension.Count == 3)
                                                                                {
                                                                                    string[, ,] a = (string[, ,])tsG.ElementAt(i).valor;
                                                                                    a[aux.dimension.ElementAt(0), aux.dimension.ElementAt(1), aux.dimension.ElementAt(2)] = Convert.ToString(aux2.valor);
                                                                                }
                                                                                else if (aux.dimension.Count == 4)
                                                                                {
                                                                                    string[, , ,] a = (string[, , ,])tsG.ElementAt(i).valor;
                                                                                    a[aux.dimension.ElementAt(0), aux.dimension.ElementAt(1), aux.dimension.ElementAt(2), aux.dimension.ElementAt(3)] = Convert.ToString(aux2.valor);
                                                                                }
                                                                                break;
                                                                            case "BOOL":
                                                                                if (aux.dimension.Count == 1)
                                                                                {
                                                                                    bool[] a = (bool[])tsG.ElementAt(i).valor;
                                                                                    a[aux.dimension.ElementAt(0)] = Convert.ToBoolean(aux2.valor);
                                                                                }
                                                                                else if (aux.dimension.Count == 2)
                                                                                {
                                                                                    bool[,] a = (bool[,])tsG.ElementAt(i).valor;
                                                                                    a[aux.dimension.ElementAt(0), aux.dimension.ElementAt(1)] = Convert.ToBoolean(aux2.valor);
                                                                                }
                                                                                else if (aux.dimension.Count == 3)
                                                                                {
                                                                                    bool[, ,] a = (bool[, ,])tsG.ElementAt(i).valor;
                                                                                    a[aux.dimension.ElementAt(0), aux.dimension.ElementAt(1), aux.dimension.ElementAt(2)] = Convert.ToBoolean(aux2.valor);
                                                                                }
                                                                                else if (aux.dimension.Count == 4)
                                                                                {
                                                                                    bool[, , ,] a = (bool[, , ,])tsG.ElementAt(i).valor;
                                                                                    a[aux.dimension.ElementAt(0), aux.dimension.ElementAt(1), aux.dimension.ElementAt(2), aux.dimension.ElementAt(3)] = Convert.ToBoolean(aux2.valor);
                                                                                }
                                                                                break;
                                                                        }
                                                                        return aux;
		                                                            }else { 
		                                                                //ERROR
		                                                                addError("GLOBAL. Se intenta asignar un valor que no coincide con el tipo del arreglo", root.Span.Location.Line, root.Span.Location.Column);
                                                                        aux.error = true;
                                                                    }
		                                                        }else { //++ O --
		                                                            Mtrx mt = (Mtrx)tsG.ElementAt(i).valor;
		                                                            switch (tsG.ElementAt(i).tipo){                                                                    
		                                                                case "INT":                                                                    
                                                                            //if (aux.dimension.Count == 1){//UNA DIMENSION
                                                                            //    int a1 = Convert.ToInt32(mt.elem[aux.dimension.ElementAt(0)]);
                                                                            //    if (Convert.ToInt32(aux2.valor) > 0)
                                                                            //        a1++;
                                                                            //    else
                                                                            //        a1--;
                                                                            //    mt.elem[aux.dimension.ElementAt(0)] = a1;
                                                                            //    return aux;
                                                                            //}else{ // N DIMENSIONES
                                                                            //    for (int k = 0; k < aux.dimension.Count; k++){
                                                                            //        if (k != aux.dimension.Count - 1){ //NO ES ULTIMO ELEMNTO
                                                                            //            mt = (Mtrx)mt.elem[aux.dimension.ElementAt(k)];
                                                                            //        }else{ //ULTIMO ELEMENTO
                                                                            //            int a1 = Convert.ToInt32(mt.elem[aux.dimension.ElementAt(k)]);
                                                                            //            if (Convert.ToInt32(aux2.valor) > 0)
                                                                            //                a1++;
                                                                            //            else
                                                                            //                a1--;
                                                                            //            mt.elem[aux.dimension.ElementAt(k)] = a1;
                                                                            //            return aux;
                                                                            //        }
                                                                            //    }
                                                                            //}
                                                                            if (aux.dimension.Count == 1)
                                                                            {
                                                                                int[] a = (int[])tsG.ElementAt(i).valor;
                                                                                a[aux.dimension.ElementAt(0)] += Convert.ToInt32(aux2.valor);
                                                                            }
                                                                            else if (aux.dimension.Count == 2)
                                                                            {
                                                                                int[,] a = (int[,])tsG.ElementAt(i).valor;
                                                                                a[aux.dimension.ElementAt(0), aux.dimension.ElementAt(1)] += Convert.ToInt32(aux2.valor);
                                                                            }
                                                                            else if (aux.dimension.Count == 3)
                                                                            {
                                                                                int[, ,] a = (int[, ,])tsG.ElementAt(i).valor;
                                                                                a[aux.dimension.ElementAt(0), aux.dimension.ElementAt(1), aux.dimension.ElementAt(2)] += Convert.ToInt32(aux2.valor);
                                                                            }
                                                                            else if (aux.dimension.Count == 4)
                                                                            {
                                                                                int[, , ,] a = (int[, , ,])tsG.ElementAt(i).valor;
                                                                                a[aux.dimension.ElementAt(0), aux.dimension.ElementAt(1), aux.dimension.ElementAt(2), aux.dimension.ElementAt(3)] += Convert.ToInt32(aux2.valor);
                                                                            }
                                                                            return aux;
		                                                                    //break;
		                                                                case "DOUBLE":                                                                    
                                                                            //if (aux.dimension.Count == 1){//UNA DIMENSION
                                                                            //    double a1 = Convert.ToDouble(mt.elem[aux.dimension.ElementAt(0)]);
                                                                            //    if (Convert.ToInt32(aux2.valor) > 0)
                                                                            //        a1++;
                                                                            //    else
                                                                            //        a1--;
                                                                            //    mt.elem[aux.dimension.ElementAt(0)] = a1;
                                                                            //    return aux;
                                                                            //}else{ // N DIMENSIONES
                                                                            //    for (int k = 0; k < aux.dimension.Count; k++){
                                                                            //        if (k != aux.dimension.Count - 1){ //NO ES ULTIMO ELEMNTO
                                                                            //            mt = (Mtrx)mt.elem[aux.dimension.ElementAt(k)];
                                                                            //        }else{ //ULTIMO ELEMENTO
                                                                            //            double a1 = Convert.ToDouble(mt.elem[aux.dimension.ElementAt(k)]);
                                                                            //            if (Convert.ToInt32(aux2.valor) > 0)
                                                                            //                a1++;
                                                                            //            else
                                                                            //                a1--;
                                                                            //            mt.elem[aux.dimension.ElementAt(k)] = a1;
                                                                            //            return aux;
                                                                            //        }
                                                                            //    }
                                                                            //}
                                                                            if (aux.dimension.Count == 1)
                                                                            {
                                                                                double[] a = (double[])tsG.ElementAt(i).valor;
                                                                                a[aux.dimension.ElementAt(0)] += Convert.ToDouble(aux2.valor);
                                                                            }
                                                                            else if (aux.dimension.Count == 2)
                                                                            {
                                                                                double[,] a = (double[,])tsG.ElementAt(i).valor;
                                                                                a[aux.dimension.ElementAt(0), aux.dimension.ElementAt(1)] += Convert.ToDouble(aux2.valor);
                                                                            }
                                                                            else if (aux.dimension.Count == 3)
                                                                            {
                                                                                double[, ,] a = (double[, ,])tsG.ElementAt(i).valor;
                                                                                a[aux.dimension.ElementAt(0), aux.dimension.ElementAt(1), aux.dimension.ElementAt(2)] += Convert.ToDouble(aux2.valor);
                                                                            }
                                                                            else if (aux.dimension.Count == 4)
                                                                            {
                                                                                double[, , ,] a = (double[, , ,])tsG.ElementAt(i).valor;
                                                                                a[aux.dimension.ElementAt(0), aux.dimension.ElementAt(1), aux.dimension.ElementAt(2), aux.dimension.ElementAt(3)] += Convert.ToDouble(aux2.valor);
                                                                            }
                                                                            return aux;
		                                                                    //break;
		                                                                case "CHAR":
                                                                            //if (aux.dimension.Count == 1){//UNA DIMENSION
                                                                            //    int a1 = (Int32)Convert.ToChar(mt.elem[aux.dimension.ElementAt(0)]);
                                                                            //    if (Convert.ToInt32(aux2.valor) > 0)
                                                                            //        a1++;
                                                                            //    else
                                                                            //        a1--;
                                                                            //    mt.elem[aux.dimension.ElementAt(0)] = Convert.ToChar( a1);
                                                                            //    return aux;
                                                                            //}else{ // N DIMENSIONES
                                                                            //    for (int k = 0; k < aux.dimension.Count; k++){
                                                                            //        if (k != aux.dimension.Count - 1){ //NO ES ULTIMO ELEMNTO
                                                                            //            mt = (Mtrx)mt.elem[aux.dimension.ElementAt(k)];
                                                                            //        }else{ //ULTIMO ELEMENTO
                                                                            //            int a1 = (Int32)Convert.ToChar(mt.elem[aux.dimension.ElementAt(k)]);
                                                                            //            if (Convert.ToInt32(aux2.valor) > 0)
                                                                            //                a1++;
                                                                            //            else
                                                                            //                a1--;
                                                                            //            mt.elem[aux.dimension.ElementAt(k)] = Convert.ToChar( a1);
                                                                            //            return aux;
                                                                            //        }
                                                                            //    }
                                                                            //}
                                                                            if (aux.dimension.Count == 1)
                                                                            {
                                                                                char[] a = (char[])tsG.ElementAt(i).valor;
                                                                                a[aux.dimension.ElementAt(0)] += Convert.ToChar(aux2.valor);
                                                                            }
                                                                            else if (aux.dimension.Count == 2)
                                                                            {
                                                                                char[,] a = (char[,])tsG.ElementAt(i).valor;
                                                                                a[aux.dimension.ElementAt(0), aux.dimension.ElementAt(1)] += Convert.ToChar(aux2.valor);
                                                                            }
                                                                            else if (aux.dimension.Count == 3)
                                                                            {
                                                                                char[, ,] a = (char[, ,])tsG.ElementAt(i).valor;
                                                                                a[aux.dimension.ElementAt(0), aux.dimension.ElementAt(1), aux.dimension.ElementAt(2)] += Convert.ToChar(aux2.valor);
                                                                            }
                                                                            else if (aux.dimension.Count == 4)
                                                                            {
                                                                                char[, , ,] a = (char[, , ,])tsG.ElementAt(i).valor;
                                                                                a[aux.dimension.ElementAt(0), aux.dimension.ElementAt(1), aux.dimension.ElementAt(2), aux.dimension.ElementAt(3)] += Convert.ToChar(aux2.valor);
                                                                            }
                                                                            return aux;
		                                                                    //break;
		                                                                default:
		                                                                    //ERROR
		                                                                    addError("GLOBAL. Se intenta aumentar o decrementr el valor de un tipo de dato no valido, STRING o BOOL", root.Span.Location.Line, root.Span.Location.Column);
                                                                            aux.error = true;
                                                                            break;
		                                                            }
		                                                        }
                                                            }else {
                                                                //ERROR
                                                                addError("GLOBAL. Se intenta asignar un valor a una posicion de un arreglo, pero se excede el valor de las dimensiones establecidas", root.Span.Location.Line, root.Span.Location.Column);
                                                                dimVer = false;
                                                                aux.error = true;
                                                            }
                                                        }else { 
                                                            //ERROR
                                                            addError("GLOBAL. Se intenta asignar un valor a una posicion de un arreglo, pero el numero de dimensiones no coincide", root.Span.Location.Line, root.Span.Location.Column);
                                                            aux.error = true;
                                                        }                                                        
                                                    }else { //ELSE
                                                        //ERROR
                                                        addError("Asignacion. Se definieron dimensiones, pero la variable a la que se hace referencia no es un arreglo", root.Span.Location.Line, root.Span.Location.Column);
                                                        aux.error = true;
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }else { 
                                    //ERROR
                                    addError("No se puede asignar un valor a una llamada de procedimiento", root.Span.Location.Line, root.Span.Location.Column);
                                    aux.error = true;
                                }
                            }else { 
                                //ERROR
                                addError("No se puede asignar un valor a una llamada de procedimiento", root.Span.Location.Line, root.Span.Location.Column);
                                aux.error = true;
                            }
                        }
                        return aux;
                    }else {
                        valor = root.ChildNodes.ElementAt(0).ToString().Split(' ');
                        aux = pRec(root.ChildNodes.ElementAt(1));
                        aux.nombrre = valor[0];
                        if (aux.error != true) { //NO TRAE ERRORES                            
                            //EN AMBITO GENERAL, PILA --------------------------------------------------
                            if (pila.ElementAt(actual).hay == false){
                                for (int i = 0; i < pila.ElementAt(actual).tabla.Count; i++){
                                    if (pila.ElementAt(actual).tabla.ElementAt(i).nombrre == valor[0] && pila.ElementAt(actual).tabla.ElementAt(i).rol == "var")
                                    {//SE ENCONTRO LA VARIABLE
                                        if (aux.aumento == false){//ASIGNACION NORMAL
                                            if (pila.ElementAt(actual).tabla.ElementAt(i).tipo == aux.tipo){//COINCIDEN LOS TIPOS
                                                if (pila.ElementAt(actual).tabla.ElementAt(i).dimension.Count == 0){//ES UN ID
                                                    pila.ElementAt(actual).tabla.ElementAt(i).valor = aux.valor;
                                                }else{//ES UN ARREGLO
                                                    //ERROR
                                                    addError("LOCAL. Se intenta asignar un valor a una matriz, no a un elemento de la matriz", root.Span.Location.Line, root.Span.Location.Column);
                                                    aux.error = true;
                                                }
                                            }else{ // TIPOS DISTINTOS
                                                //ERROR
                                                addError("LOCAL. Se intenta asignar un valor que no coincide con el tipo de la variable", root.Span.Location.Line, root.Span.Location.Column);
                                                aux.error = true;
                                            }
                                        }else { //AUMENTO O DECREMENTO DE VARIABLE
                                            if (pila.ElementAt(actual).tabla.ElementAt(i).dimension.Count == 0){//ES UN ID
                                                switch (pila.ElementAt(actual).tabla.ElementAt(i).tipo) {
                                                    case "INT":
                                                        int cc = Convert.ToInt32(pila.ElementAt(actual).tabla.ElementAt(i).valor);
                                                        if (Convert.ToInt32(aux.valor) > 0)
                                                            cc++;
                                                        else
                                                            cc--;
                                                        pila.ElementAt(actual).tabla.ElementAt(i).valor = cc;
                                                        break;
                                                    case "DOUBLE":
                                                        double bb = Convert.ToDouble( pila.ElementAt(actual).tabla.ElementAt(i).valor);
                                                        if (Convert.ToInt32(aux.valor) > 0)
                                                            bb++;
                                                        else
                                                            bb--;
                                                        pila.ElementAt(actual).tabla.ElementAt(i).valor = bb;
                                                        break;
                                                    case "CHAR":
                                                        int aa = (Int32)Convert.ToChar(pila.ElementAt(actual).tabla.ElementAt(i).valor);
                                                        if (Convert.ToInt32(aux.valor) > 0)
                                                            aa++;                                                                
                                                        else 
                                                            aa--;                                                            
                                                        pila.ElementAt(actual).tabla.ElementAt(i).valor = Convert.ToChar(aa);
                                                        break;
                                                    default:
                                                        //ERROR
                                                        addError("LOCAL. Se intenta aumentar o decrementr el valor de un tipo de dato no valido, STRING o BOOL", root.Span.Location.Line, root.Span.Location.Column);
                                                        aux.error = true;
                                                        break;
                                                }
                                            }else {//ES UN ARREGLO
                                                //ERROR
                                                addError("LOCAL. Se intenta aumentar o decrementar el valor de una matriz, no a un elemento de la matriz", root.Span.Location.Line, root.Span.Location.Column);
                                                aux.error = true;
                                            }                                                
                                        }                                            
                                        flag = true;
                                        break;
                                    }
                                }                                    
                            }else{ //EN SUB AMBITO -----------------------------------------------------
                                acc = pila.ElementAt(actual);
                                for (int i = 0; i < acc.pila.ElementAt(acc.actual).tabla.Count; i++){
                                    if (acc.pila.ElementAt(acc.actual).tabla.ElementAt(i).nombrre == valor[0] && acc.pila.ElementAt(acc.actual).tabla.ElementAt(i).rol == "var")
                                    {//SE ENCONTRO LA VARIABLE
                                        if (aux.aumento == false){//ASIGNACION NORMAL
                                            if (acc.pila.ElementAt(acc.actual).tabla.ElementAt(i).tipo == aux.tipo){//COINCIDEN LOS TIPOS
                                                if (acc.pila.ElementAt(acc.actual).tabla.ElementAt(i).dimension.Count == 0){//ES UN ID
                                                    acc.pila.ElementAt(acc.actual).tabla.ElementAt(i).valor = aux.valor;
                                                }else{//ES UN ARREGLO
                                                    //ERROR
                                                    addError("LOCAL. Se intenta asignar un valor a una matriz, no a un elemento de la matriz", root.Span.Location.Line, root.Span.Location.Column);
                                                    aux.error = true;
                                                }
                                            }else{ // TIPOS DISTINTOS
                                                //ERROR
                                                addError("LOCAL. Se intenta asignar un valor que no coincide con el tipo de la variable", root.Span.Location.Line, root.Span.Location.Column);
                                                aux.error = true;
                                            }
                                        }else { //AUMENTO O DECREMENTO DE VARIABLE
                                            if (acc.pila.ElementAt(acc.actual).tabla.ElementAt(i).dimension.Count == 0){//ES UN ID
                                                switch (acc.pila.ElementAt(acc.actual).tabla.ElementAt(i).tipo) {
                                                    case "INT":
                                                        int cc = Convert.ToInt32(acc.pila.ElementAt(acc.actual).tabla.ElementAt(i).valor);
                                                        if (Convert.ToInt32(aux.valor) > 0)
                                                            cc++;
                                                        else
                                                            cc--;
                                                        acc.pila.ElementAt(acc.actual).tabla.ElementAt(i).valor = cc;
                                                        break;
                                                    case "DOUBLE":
                                                        double bb = Convert.ToDouble( acc.pila.ElementAt(acc.actual).tabla.ElementAt(i).valor);
                                                        if (Convert.ToInt32(aux.valor) > 0)
                                                            bb++;
                                                        else
                                                            bb--;
                                                        acc.pila.ElementAt(acc.actual).tabla.ElementAt(i).valor = bb;
                                                        break;
                                                    case "CHAR":
                                                        int aa = (Int32)Convert.ToChar(acc.pila.ElementAt(acc.actual).tabla.ElementAt(i).valor);
                                                        if (Convert.ToInt32(aux.valor) > 0)
                                                            aa++;                                                                
                                                        else 
                                                            aa--;                                                            
                                                        acc.pila.ElementAt(acc.actual).tabla.ElementAt(i).valor = Convert.ToChar(aa);
                                                        break;
                                                    default:
                                                        //ERROR
                                                        addError("LOCAL. Se intenta aumentar o decrementr el valor de un tipo de dato no valido, STRING o BOOL", root.Span.Location.Line, root.Span.Location.Column);
                                                        aux.error = true;
                                                        break;
                                                }
                                            }else {//ES UN ARREGLO
                                                //ERROR
                                                addError("LOCAL. Se intenta aumentar o decrementar el valor de una matriz, no a un elemento de la matriz", root.Span.Location.Line, root.Span.Location.Column);
                                                aux.error = true;
                                            }                                                
                                        }                                            
                                        flag = true;
                                        break;
                                    }
                                }                                    
                            }     
                        	//NO SE ENCONTRO EN AMBITO NI SUB AMBITO---------------------------------------------
                            if (flag == false) { //REVISAR TABLA GENERAL                                
                                for (int i = 0; i < tsG.Count; i++) {
                                    if (tsG.ElementAt(i).nombrre == valor[0] && tsG.ElementAt(i).rol=="var") {
                                        if (tsG.ElementAt(i).ambito == pila.ElementAt(actual).padre) { //MISMA CLASE
                        					if (aux.aumento == false){//ASIGNACION NORMAL                                                
	                                            if (tsG.ElementAt(i).tipo == aux.tipo){//COINCIDEN LOS TIPOS
	                                                if (tsG.ElementAt(i).dimension.Count == 0){//ES UN ID
	                                                    tsG.ElementAt(i).valor = aux.valor;
	                                                }else{//ES UN ARREGLO
	                                                    //ERROR
	                                                    addError("GLOBAL. Se intenta asignar un valor a una matriz, no a un elemento de la matriz", root.Span.Location.Line, root.Span.Location.Column);
                                                        aux.error = true;
                                                    }
	                                            }else{ // TIPOS DISTINTOS
	                                                //ERROR
	                                                addError("GLOBAL. Se intenta asignar un valor que no coincide con el tipo de la variable", root.Span.Location.Line, root.Span.Location.Column);
                                                    aux.error = true;
                                                }
	                                        }else { //AUMENTO O DECREMENTO DE VARIABLE
	                                            if (tsG.ElementAt(i).dimension.Count == 0){//ES UN ID
	                                                switch (tsG.ElementAt(i).tipo) {
	                                                    case "INT":
	                                                        int cc = Convert.ToInt32(tsG.ElementAt(i).valor);
	                                                        if (Convert.ToInt32(aux.valor) > 0)
	                                                            cc++;
	                                                        else
	                                                            cc--;
	                                                        tsG.ElementAt(i).valor = cc;
	                                                        break;
	                                                    case "DOUBLE":
	                                                        double bb = Convert.ToDouble( tsG.ElementAt(i).valor);
	                                                        if (Convert.ToInt32(aux.valor) > 0)
	                                                            bb++;
	                                                        else
	                                                            bb--;
	                                                        tsG.ElementAt(i).valor = bb;
	                                                        break;
	                                                    case "CHAR":
	                                                        int aa = (Int32)Convert.ToChar(tsG.ElementAt(i).valor);
	                                                        if (Convert.ToInt32(aux.valor) > 0)
	                                                            aa++;                                                                
	                                                        else 
	                                                            aa--;                                                            
	                                                        tsG.ElementAt(i).valor = Convert.ToChar(aa);
	                                                        break;
	                                                    default:
	                                                        //ERROR
	                                                        addError("GLOBAL. Se intenta aumentar o decrementr el valor de un tipo de dato no valido, STRING o BOOL", root.Span.Location.Line, root.Span.Location.Column);
                                                            aux.error = true;
                                                            break;
	                                                }
	                                            }else {//ES UN ARREGLO
	                                                //ERROR
	                                                addError("GLOBAL. Se intenta aumentar o decrementar el valor de una matriz, no a un elemento de la matriz", root.Span.Location.Line, root.Span.Location.Column);
                                                    aux.error = true;
                                                }                                                
	                                        }
                                            flag = true;
                                            break;
                                        } else { //DISTINTA CLASE
                                            //ERROR                                            
                                            subFlag = true;
                                        }                                                                                                                                                             
                                    }
                                }
                                if (flag == false && subFlag == true) {
                                    //ERROR
                                    addError("GLOBAL. aaNo se puede acceder a una variable que no sea del mimsmo origen (clase)", root.Span.Location.Line, root.Span.Location.Column);
                                    aux.error = true;
                                } else if (flag == false) {
                                    addError("Asignacion. No existe la variable en ninguno de los ambitos actuales.", root.Span.Location.Line, root.Span.Location.Column);
                                    aux.error = true;
                                }                                
                            }
                        }
                        return aux;
                    }                    
                case "OPC_ASIG":
                    /*
                        OPC_ASIG.Rule = tIgual + EXP
                            | tMM
                            | tMiMi
                     */
                    temp = root.ChildNodes.ElementAt(0).ToString().Trim();
                    if (root.ChildNodes.Count > 1)
                    {
                        aux = pRec(root.ChildNodes.ElementAt(1));
                        return aux;
                    }else {
                        if (temp.Contains('+')){
                            nuevo.valor = 1;
                            nuevo.aumento = true;
                        }else{
                            nuevo.valor = -1;
                            nuevo.aumento = true;
                        }   
                    }                    
                    return nuevo;
                case "ACC_MTRX":
                    /*
                        ACC_MTRX.Rule = LL_PROC + AUX_2
                            | tId + REL_M + AUX_2
                     */
                    //  VALIDAR SI PARAMETROS Y DIMENSION TRAE AL MISMO TIEMPO, EN SUBIDA DE VALORES
                            if(root.ChildNodes.Count==2){
                                aux = pRec(root.ChildNodes.ElementAt(0));
                                aux2 = pRec(root.ChildNodes.ElementAt(1));
                                if (aux.error != true && aux2.error != true){
                                    if (aux.parametros.Count > 0 && aux2.dimension.Count > 0){
                                        //ERROR
                                        addError("Procedimiento. Se intenta llamar a un procedimiento pero se definieron dimensiones para un arreglo A(b) c d", root.Span.Location.Line, root.Span.Location.Column);
                                        aux.error = true;
                                    }else {
                                        aux.dimension.AddRange(aux2.dimension);
                                    }
                                }else 
                                    aux.error = true;                                
                                return aux;
                            }else{
                                valor = root.ChildNodes.ElementAt(0).ToString().Split(' ');
                                aux = pRec(root.ChildNodes.ElementAt(1));
                                aux2 = pRec(root.ChildNodes.ElementAt(2));                                
                                if (aux.error != true){
                                    if (aux.tipo == "INT"){
                                        if (Convert.ToInt32(aux.valor) >= 0)
                                            nuevo.dimension.Add(Convert.ToInt32(aux.valor));
                                        else{
                                            //ERROR
                                            addError("Dimension. El valor debe ser mayor o igual a 0", root.Span.Location.Line, root.Span.Location.Column);
                                            nuevo.error = true;
                                        }
                                    }else{
                                        //ERROR
                                        addError("Dimension. Tipo incorrecto para acceder a posicion de arreglo", root.Span.Location.Line, root.Span.Location.Column);
                                        nuevo.error = true;
                                    }
                                }
                                else
                                    nuevo.error = true;
                                if (aux2.error != true){
                                    nuevo.dimension.AddRange(aux2.dimension);
                                }
                                else
                                    nuevo.error = true;
                                nuevo.nombrre = valor[0];
                                return nuevo;
                            }                    
                case "LL_PROC":
                    /*
                        LL_PROC.Rule = tId + tParA + PRE_LST2
                     */
                            valor = root.ChildNodes.ElementAt(0).ToString().Split(' ');
                            aux = pRec(root.ChildNodes.ElementAt(2));
                            aux.nombrre = valor[0];
                            if (aux.error != true){
                                if (aux.parametros.Count > 1 || aux.parametros.Count==0){ //PROCEDIMIENTO
                                    return aux;
                                }else { //PUEDE SER PROCEDIMIENTO A(PAR) O ARREGLO A (DIM)
                                    //BUSCAR EN AMBITO ACTUAL
                                    nuevo.nombrre = valor[0];
                                    if (pila.ElementAt(actual).hay != true){
                                        for (int i = 0; i < pila.ElementAt(actual).tabla.Count; i++){
                                            if (pila.ElementAt(actual).tabla.ElementAt(i).nombrre == valor[0]){
                                                if (aux.parametros.ElementAt(0).tipo == "INT"){
                                                    if(Convert.ToInt32(aux.parametros.ElementAt(0).valor)>=0) {
                                                        nuevo.dimension.Add(Convert.ToInt32(aux.parametros.ElementAt(0).valor));
                                                    } else {
                                                        //ERROR
                                                        addError("Dimension. El valor debe ser mayor o igual a 0", root.Span.Location.Line, root.Span.Location.Column);
                                                        nuevo.error = true;
                                                    }
                                                } else {
                                                    //ERROR
                                                    addError("Dimension. Tipo incorrecto para acceder a posicion de arreglo", root.Span.Location.Line, root.Span.Location.Column);
                                                    nuevo.error = true;
                                                }
                                                flag = true;
                                                break;
                                            }
                                        }
                                    }else { //BUSCAR EN SUB AMBITO
                                        acc = pila.ElementAt(actual);
                                        for (int i = 0; i < acc.pila.ElementAt(acc.actual).tabla.Count; i++){
                                            if (acc.pila.ElementAt(acc.actual).tabla.ElementAt(i).nombrre == valor[0]){
                                                if (aux.parametros.ElementAt(0).tipo == "INT"){
                                                    if (Convert.ToInt32(aux.parametros.ElementAt(0).valor) >= 0){
                                                        nuevo.dimension.Add(Convert.ToInt32(aux.parametros.ElementAt(0).valor));
                                                    }else{
                                                        //ERROR
                                                        addError("Dimension. El valor debe ser mayor o igual a 0", root.Span.Location.Line, root.Span.Location.Column);
                                                        nuevo.error = true;
                                                    }
                                                }else{
                                                    //ERROR
                                                    addError("Dimension. Tipo incorrecto para acceder a posicion de arreglo", root.Span.Location.Line, root.Span.Location.Column);
                                                    nuevo.error = true;
                                                }

                                                flag = true;
                                                break;
                                            }
                                        }
                                    }                 
                                    //BUSCAR EN TABLA DE SIMBOLOS GENERAL
                                    if (flag == false) {
                                        //String ambActual = pila.ElementAt(actual).padre;
                                        List<String> importacion = new List<string>();
                                        importacion.Add(pila.ElementAt(actual).padre);
                                        bool entrar = true;
                                        int auxiliar = 0;
                                        while (entrar) {
                                            for (int j = 0; j < importacion.Count; j++){
                                                for (int i = 0; i < tsG.Count; i++){
                                                    if (tsG.ElementAt(i).nombrre == valor[0] && tsG.ElementAt(i).ambito == importacion.ElementAt(j)){ //MISMO NOMBRE Y CLASE
                                                        if (tsG.ElementAt(i).dimension.Count == 0){ //ES PROCEDIMIENTO
                                                            return aux;
                                                            //entrar = false;
                                                        }else{ //ES ARREGLO
                                                            if (tsG.ElementAt(i).ambito == pila.ElementAt(actual).padre){
                                                                if (aux.parametros.ElementAt(0).tipo == "INT"){
                                                                    if (Convert.ToInt32(aux.parametros.ElementAt(0).valor) >= 0) {
                                                                        nuevo.dimension.Add(Convert.ToInt32(aux.parametros.ElementAt(0).valor));
                                                                    } else {
                                                                        //ERROR
                                                                        addError("Dimension. El valor debe ser mayor o igual a 0", root.Span.Location.Line, root.Span.Location.Column);
                                                                        nuevo.error = true;
                                                                    }
                                                                }else { 
                                                                    //ERROR
                                                                    addError("Dimension. Tipo incorrecto para acceder a posicion de arreglo", root.Span.Location.Line, root.Span.Location.Column);
                                                                    nuevo.error = true;                                                                    
                                                                }
                                                            }else {
                                                                //ERROR
                                                                addError("Arreglo. No se puede acceder a variables globales de otro origen(clase)", root.Span.Location.Line, root.Span.Location.Column);
                                                                nuevo.error = true;                                                                 
                                                            }                                                            
                                                        }
                                                        entrar = false;
                                                        break;
                                                    }
                                                }   
                                            }
                                            if (entrar == true){
                                                List<String> tempImp = new List<string>();
                                                //VERIFICAR SI TIENE IMPORT
                                                for (int j = 0; j < importacion.Count; j++){
                                                    for (int i = 0; i < tsG.Count; i++){
                                                        if (tsG.ElementAt(i).ambito == importacion.ElementAt(j) && tsG.ElementAt(i).rol == "import"){
                                                            tempImp.Add(tsG.ElementAt(i).nombrre);
                                                        }
                                                    }
                                                }
                                                importacion.Clear();
                                                importacion.AddRange(tempImp);
                                            }
                                            if (importacion.Count == 0 || auxiliar > 7)
                                                entrar = false;
                                            auxiliar++;
                                        }
                                    }

                                }
                            } else
                                nuevo.error = true;

                    return nuevo;
                case "PRE_LST2":
                    /*
                        PRE_LST2.Rule = LST_PRT2 + tParC
                            | tParC
                     */
                    if (root.ChildNodes.Count > 1){
                        return pRec(root.ChildNodes.ElementAt(0));
                    }else {
                        return nuevo;
                    }                    
                case "LST_PRT2":
                    /*
                        LST_PRT2.Rule = LST_PRT2 + tComa + EXP
                            | EXP
                     */
                    if (root.ChildNodes.Count > 1){
                        aux = pRec(root.ChildNodes.ElementAt(0));
                        aux2 = pRec(root.ChildNodes.ElementAt(2));
                        if (aux2.error != true)
                            aux.parametros.Add(aux2);
                        else
                            aux.error = true;
                        return aux;
                    }else {
                        aux = pRec(root.ChildNodes.ElementAt(0));
                        if (aux.error != true)
                            nuevo.parametros.Add(aux);
                        else
                            nuevo.error = true;
                        return nuevo;
                    }                    
                case "AUX_2":
                    /*
                        AUX_2.Rule = AUX_3
                            | EPS
                     */
                    temp = root.ChildNodes.ElementAt(0).ToString().Trim();
                    if (temp.Equals("AUX_3")){
                        return pRec(root.ChildNodes.ElementAt(0));
                    }
                    return nuevo;
                case "AUX_3":
                    /*
                        AUX_3.Rule = AUX_3 + EXP
                            | EXP
                     */
                    if (root.ChildNodes.Count == 2){
                        aux = pRec(root.ChildNodes.ElementAt(0));
                        aux2 = pRec(root.ChildNodes.ElementAt(1));
                        if (aux2.error != true){
                            if(aux2.tipo=="INT"){
                                if (Convert.ToInt32(aux2.valor) >= 0)
                                    aux.dimension.Add(Convert.ToInt32(aux2.valor));
                                else { 
                                    //ERROR
                                    addError("Dimension. El valor debe ser mayor o igual a 0", root.Span.Location.Line, root.Span.Location.Column);
                                    aux.error = true;
                                }
                            }else{
                                //ERROR
                                addError("Dimension. Tipo incorrecto para acceder a posicion de arreglo", root.Span.Location.Line, root.Span.Location.Column);
                                aux.error = true;
                            }
                        }
                        else
                            aux.error = true;                      
                        return aux;
                    }else {
                        aux = pRec(root.ChildNodes.ElementAt(0));
                        if (aux.error != true){
                            if (aux.tipo == "INT"){
                                if (Convert.ToInt32(aux.valor) >= 0)
                                    nuevo.dimension.Add(Convert.ToInt32(aux.valor));
                                else {
                                    //ERROR
                                    addError("Dimension. El valor debe ser mayor o igual a 0", root.Span.Location.Line, root.Span.Location.Column);
                                    nuevo.error = true;    
                                }
                            }else{
                                //ERROR
                                addError("Dimension. Tipo incorrecto para acceder a posicion de arreglo", root.Span.Location.Line, root.Span.Location.Column);
                                nuevo.error = true;
                            }
                        }else
                            nuevo.error = true;
                        return nuevo;
                    }                    
                case "REL_M":
                    return chapus(root);
                case "OUT":
                    /*
                        OUT.Rule = tOut + tParA + EXP + tParC + tPtoComa                     
                     */
                    aux = pRec(root.ChildNodes.ElementAt(2));
                    if (aux.error != true) {
                        consola += Convert.ToString(aux.valor) + "\n";
                    }
                    break;
                case "RETURN":
                    /*
                        RETURN.Rule = tReturn + OPC_RET;
                     */
                    aux = pRec(root.ChildNodes.ElementAt(1));
                    if (aux.error != true) {
                        if (pila.ElementAt(actual).tipo!="VOID"){
                            if (aux.tipo != "VOID"){
                                if (aux.tipo == pila.ElementAt(actual).tipo){
                                    pila.ElementAt(actual).retorno = aux.valor;
                                    llamada = true;
                                }else{
                                    //ERROR
                                    addError("Una funcion debe retornar un valor de acuerdo a su tipo de dato", root.Span.Location.Line, root.Span.Location.Column);
                                    aux2 = valDefault(pila.ElementAt(actual).tipo);
                                    pila.ElementAt(actual).retorno = aux2.valor;
                                }
                            }else { 
                                //ERROR
                                addError("Una funcion es una subrutina que debe retornar un valor", root.Span.Location.Line, root.Span.Location.Column);
                            }                            
                        }else { //NO DEBE RETORNAR NADA
                            if (aux.tipo == "VOID"){
                                llamada = true;
                            }else {
                                //ERROR
                                addError("Un metodo es una subrutina con instrucciones y no debe retornar ningun valor.", root.Span.Location.Line, root.Span.Location.Column);
                            }
                        }
                    }
                    //llamada = true;
                    break;
                case "OPC_RET":
                    /*
                        OPC_RET.Rule = tPtoComa
                            | EXP + tPtoComa
                     */
                    if (root.ChildNodes.Count > 1) {
                        return pRec(root.ChildNodes.ElementAt(0));
                    }
                    nuevo.tipo = "VOID";
                    return nuevo;
                case "IF":
                    /*
                        IF.Rule = tIf + tParA + EXP + tParC + tThen + tBegin + CUERPO_PROC + tEnd + FIN_IF
                     */
                    aux = pRec(root.ChildNodes.ElementAt(2));
                    if (aux.error != true) {
                        aux2 = establecerLogico(aux, root.Span.Location.Line, root.Span.Location.Column);
                        if (aux2.error != true) {
                            if (Convert.ToBoolean(aux2.valor) == true){ //IF, CREAR SUB AMBITO
                                //DEFAULT
                                principal.setear();
                                principal.ambito = "IF";
                                principal.padre = pila.ElementAt(actual).ambito;
                                pila.ElementAt(actual).pila.Add(principal);
                                if (pila.ElementAt(actual).hay == false)
                                    pila.ElementAt(actual).hay = true;
                                else
                                    pila.ElementAt(actual).actual++;
                                //AGREGAR VARIABLES A AMBITO ACTUAL
                                copiarIda();
                                //INICIAR RECORRIDO IF
                                pRec(root.ChildNodes.ElementAt(6));
                                //COPIAR VARIABLES
                                copiarVuelta();
                                //TERMINAR RECORRIDO         
                                pila.ElementAt(actual).pila.RemoveAt(pila.ElementAt(actual).actual);
                                if (pila.ElementAt(actual).actual == 0)
                                    pila.ElementAt(actual).hay = false;
                                else
                                    pila.ElementAt(actual).actual--;
                            }else { //ELSE IF o ELSE o NADA
                                pRec(root.ChildNodes.ElementAt(8));
                            }
                        }
                    }
                    break;
                case "FIN_IF":
                    /*
                        FIN_IF.Rule = tElse + tBegin + CUERPO_PROC + tEnd
                            | tElIf + tParA + EXP + tParC + tThen + tBegin + CUERPO_PROC + tEnd + FIN_IF
                            | EPS
                     */
                    if (root.ChildNodes.Count == 4) { //ELSE, SE DEBE CREAR SUB AMBITO
                        //DEFAULT
                        principal.setear();
                        principal.ambito = "ELSE";
                        principal.padre = pila.ElementAt(actual).ambito;
                        pila.ElementAt(actual).pila.Add(principal);
                        if (pila.ElementAt(actual).hay == false)
                            pila.ElementAt(actual).hay = true;
                        else
                            pila.ElementAt(actual).actual++;
                        //AGREGAR VARIABLES A AMBITO ACTUAL
                        copiarIda();
                        //INICIAR RECORRIDO IF
                        pRec(root.ChildNodes.ElementAt(2));
                        //COPIAR VARIABLES
                        copiarVuelta();
                        //TERMINAR RECORRIDO         
                        pila.ElementAt(actual).pila.RemoveAt(pila.ElementAt(actual).actual);
                        if (pila.ElementAt(actual).actual == 0)
                            pila.ElementAt(actual).hay = false;
                        else
                            pila.ElementAt(actual).actual--;
                    } else if (root.ChildNodes.Count > 4) { //VERIFICAR SI SE CUMPLE IF-ELSE
                        aux = pRec(root.ChildNodes.ElementAt(2));
                        if (aux.error != true){
                            aux2 = establecerLogico(aux, root.Span.Location.Line, root.Span.Location.Column);
                            if (aux2.error != true){
                                if (Convert.ToBoolean(aux2.valor) == true){ //IF, CREAR SUB AMBITO
                                    //DEFAULT
                                    principal.setear();
                                    principal.ambito = "IF-ELSE";
                                    principal.padre = pila.ElementAt(actual).ambito;
                                    pila.ElementAt(actual).pila.Add(principal);
                                    if (pila.ElementAt(actual).hay == false)
                                        pila.ElementAt(actual).hay = true;
                                    else
                                        pila.ElementAt(actual).actual++;
                                    //AGREGAR VARIABLES A AMBITO ACTUAL
                                    copiarIda();
                                    //INICIAR RECORRIDO IF
                                    pRec(root.ChildNodes.ElementAt(6));
                                    //COPIAR VARIABLES
                                    copiarVuelta();
                                    //TERMINAR RECORRIDO         
                                    pila.ElementAt(actual).pila.RemoveAt(pila.ElementAt(actual).actual);
                                    if (pila.ElementAt(actual).actual == 0)
                                        pila.ElementAt(actual).hay = false;
                                    else
                                        pila.ElementAt(actual).actual--;
                                }
                                else{ //ELSE IF o ELSE o NADA
                                    pRec(root.ChildNodes.ElementAt(8));
                                }
                            }
                        }
                    }
                    break;
                case "WHL":
                    /*
                        WHL.Rule = tWhile + tParA + EXP + tParC + tDo + tBegin + CUERPO_PROC + tEnd;
                     */
                    aux = pRec(root.ChildNodes.ElementAt(2));
                    if (aux.error != true) {//SI VIENE BIEN LA EXPRESION
                        aux2 = establecerLogico(aux, root.Span.Location.Line, root.Span.Location.Column);
                        if (aux2.error != true) { //SI RETORNA UN VALOR LOGICO
                            int contraM = 0;
                            while (Convert.ToBoolean(pRec(root.ChildNodes.ElementAt(2)).valor)==true && interrupt!=true && llamada!=true){
                                //DEFAULT
                                principal.setear();
                                principal.ambito = "WHILE";
                                principal.padre = pila.ElementAt(actual).ambito;
                                pila.ElementAt(actual).pila.Add(principal);                                                                
                                if (pila.ElementAt(actual).hay == false)
                                    pila.ElementAt(actual).hay = true;
                                else
                                    pila.ElementAt(actual).actual++;
                                //AGREGAR VARIABLES A AMBITO ACTUAL
                                copiarIda();
                                //INICIAR RECORRIDO IF
                                pRec(root.ChildNodes.ElementAt(6));
                                //COPIAR VARIABLES
                                copiarVuelta();
                                //TERMINAR RECORRIDO                                      
                                pila.ElementAt(actual).pila.RemoveAt(pila.ElementAt(actual).actual);
                                if (pila.ElementAt(actual).actual == 0)
                                    pila.ElementAt(actual).hay = false;
                                else
                                    pila.ElementAt(actual).actual--;

                                if (contraM > 9000){
                                    addError("While enciclado, supero las 9000 iteraciones", root.Span.Location.Line, root.Span.Location.Column);
                                    break;
                                }
                                contraM++;
                            }
                            interrupt = false;
                        }
                    }
                    break;
                case "BRK":
                    /*
                        BRK.Rule = tBreak + tPtoComa;
                     */
                    if (pila.ElementAt(actual).hay != false){//HAY SUB AMBITOS
                        acc = pila.ElementAt(actual);
                        bool auxFl = false;
                        for (int i = 0; i < acc.pila.Count; i++){
                            if (acc.pila.ElementAt(i).ambito == "WHILE" || acc.pila.ElementAt(i).ambito == "DO-WHILE" || acc.pila.ElementAt(i).ambito == "REPEAT" || acc.pila.ElementAt(i).ambito == "FOR" || acc.pila.ElementAt(i).ambito == "CASE")
                            {
                                interrupt = true;
                                auxFl = true;
                                break;
                            }
                        }
                        if (auxFl != true) { 
                            //ERROR
                            addError("Unicamente puede usarse un break dentro de un ciclo o en sentencias de control anidados dentro de un ciclo", root.Span.Location.Line, root.Span.Location.Column);
                        }
                    }else { //NO HAY SUB AMBITOS
                        //ERROR
                        addError("Procedimiento. No se puede usar un break fuera de un ciclo", root.Span.Location.Line, root.Span.Location.Column);
                    }
                    break;
                case "DO_WHL":
                    /*
                        DO_WHL.Rule = tDo + tBegin + CUERPO_PROC + tEnd + tWhile + tParA + EXP + tParC + tPtoComa
                     */
                    //EJECUTA AL MENOS UNA VEZ
                    //DEFAULT
                    principal.setear();
                    principal.ambito = "DO-WHILE";
                    principal.padre = pila.ElementAt(actual).ambito;
                    pila.ElementAt(actual).pila.Add(principal);
                    if (pila.ElementAt(actual).hay == false)
                        pila.ElementAt(actual).hay = true;
                    else
                        pila.ElementAt(actual).actual++;
                    //AGREGAR VARIABLES A AMBITO ACTUAL
                    copiarIda();
                    //INICIAR RECORRIDO IF
                    pRec(root.ChildNodes.ElementAt(2));
                    //COPIAR VARIABLES
                    copiarVuelta();
                    //TERMINAR RECORRIDO         
                    pila.ElementAt(actual).pila.RemoveAt(pila.ElementAt(actual).actual);
                    if (pila.ElementAt(actual).actual == 0)
                        pila.ElementAt(actual).hay = false;
                    else
                        pila.ElementAt(actual).actual--;

                    //VERIFICAR SI DEBE SEGUIR EJECUTANDO
                    if (interrupt != true) {                        
                        aux = pRec(root.ChildNodes.ElementAt(6));
                        if (aux.error != true){//SI VIENE BIEN LA EXPRESION
                            aux2 = establecerLogico(aux, root.Span.Location.Line, root.Span.Location.Column);
                            if (aux2.error != true){ //SI RETORNA UN VALOR LOGICO
                                int contraM = 0;
                                while (Convert.ToBoolean(pRec(root.ChildNodes.ElementAt(6)).valor) == true && interrupt != true && llamada != true){
                                    //DEFAULT
                                    principal.setear();
                                    principal.ambito = "DO-WHILE";
                                    principal.padre = pila.ElementAt(actual).ambito;
                                    pila.ElementAt(actual).pila.Add(principal);
                                    if (pila.ElementAt(actual).hay == false)
                                        pila.ElementAt(actual).hay = true;
                                    else
                                        pila.ElementAt(actual).actual++;
                                    //AGREGAR VARIABLES A AMBITO ACTUAL
                                    copiarIda();
                                    //INICIAR RECORRIDO IF
                                    pRec(root.ChildNodes.ElementAt(2));
                                    //COPIAR VARIABLES
                                    copiarVuelta();
                                    //TERMINAR RECORRIDO         
                                    pila.ElementAt(actual).pila.RemoveAt(pila.ElementAt(actual).actual);
                                    if (pila.ElementAt(actual).actual == 0)
                                        pila.ElementAt(actual).hay = false;
                                    else
                                        pila.ElementAt(actual).actual--;

                                    if (contraM > 9000){
                                        addError("Do-while enciclado, supero las 9000 iteraciones", root.Span.Location.Line, root.Span.Location.Column);
                                        break;
                                    }
                                    contraM++;
                                }
                                interrupt = false;
                            }
                        }
                    }
                    interrupt = false;
                    break;
                case "RPT_UNT":
                    /*
                        RPT_UNT.Rule = tRepeat + tBegin + CUERPO_PROC + tEnd + tUntil + tParA + EXP + tParC + tPtoComa
                     */
                    //EJECUTA AL MENOS UNA VEZ
                    //DEFAULT
                    principal.setear();
                    principal.ambito = "REPEAT";
                    principal.padre = pila.ElementAt(actual).ambito;
                    pila.ElementAt(actual).pila.Add(principal);
                    if (pila.ElementAt(actual).hay == false)
                        pila.ElementAt(actual).hay = true;
                    else
                        pila.ElementAt(actual).actual++;
                    //AGREGAR VARIABLES A AMBITO ACTUAL
                    copiarIda();
                    //INICIAR RECORRIDO IF
                    pRec(root.ChildNodes.ElementAt(2));
                    //COPIAR VARIABLES
                    copiarVuelta();
                    //TERMINAR RECORRIDO         
                    pila.ElementAt(actual).pila.RemoveAt(pila.ElementAt(actual).actual);
                    if (pila.ElementAt(actual).actual == 0)
                        pila.ElementAt(actual).hay = false;
                    else
                        pila.ElementAt(actual).actual--;

                    //VERIFICAR SI DEBE SEGUIR EJECUTANDO
                    if (interrupt != true) {                        
                        aux = pRec(root.ChildNodes.ElementAt(6));
                        if (aux.error != true){//SI VIENE BIEN LA EXPRESION
                            aux2 = establecerLogico(aux, root.Span.Location.Line, root.Span.Location.Column);
                            if (aux2.error != true){ //SI RETORNA UN VALOR LOGICO
                                int contraM = 0;
                                while (Convert.ToBoolean(pRec(root.ChildNodes.ElementAt(6)).valor) == false && interrupt != true && llamada != true){
                                    //DEFAULT
                                    principal.setear();
                                    principal.ambito = "REPEAT";
                                    principal.padre = pila.ElementAt(actual).ambito;
                                    pila.ElementAt(actual).pila.Add(principal);
                                    if (pila.ElementAt(actual).hay == false)
                                        pila.ElementAt(actual).hay = true;
                                    else
                                        pila.ElementAt(actual).actual++;
                                    //AGREGAR VARIABLES A AMBITO ACTUAL
                                    copiarIda();
                                    //INICIAR RECORRIDO IF
                                    pRec(root.ChildNodes.ElementAt(2));
                                    //COPIAR VARIABLES
                                    copiarVuelta();
                                    //TERMINAR RECORRIDO         
                                    pila.ElementAt(actual).pila.RemoveAt(pila.ElementAt(actual).actual);
                                    if (pila.ElementAt(actual).actual == 0)
                                        pila.ElementAt(actual).hay = false;
                                    else
                                        pila.ElementAt(actual).actual--;

                                    if (contraM > 9000){
                                        addError("Repeat Until enciclado, supero las 9000 iteraciones", root.Span.Location.Line, root.Span.Location.Column);
                                        break;
                                    }
                                    contraM++;
                                }
                                interrupt = false;
                            }
                        }
                    }
                    interrupt = false;
                    break;
                case "FOR":
                    /*
                        FOR.Rule = tFor + EXTRA + tTo + EXP + tDo + tBegin + CUERPO_PROC + tEnd
                     */
                    aux = pRec(root.ChildNodes.ElementAt(1));
                    aux2 = pRec(root.ChildNodes.ElementAt(3));    
                    int fila = root.Span.Location.Line;
                    int columna = root.Span.Location.Column;
                    
                    if (aux.error != true && aux2.error != true) {
                        if (aux2.tipo == "INT"){
                            if (aux.parametros.Count == 0){//SI SE PUEDE ANALIZAR                                                                
                                Tupla exxx = forCon(aux, Convert.ToInt32(aux2.valor), 0, fila, columna);
                                if (exxx.error != true) { //VARIABLE DE CONTROL VALIDO
                                    int aumDec = Convert.ToInt32(exxx.valor);

                                    int contraM = 0;
                                    while (Convert.ToInt32(forCon(aux, Convert.ToInt32(aux2.valor), 0, fila, columna).valor)!=0  && interrupt != true && llamada != true){
                                        //DEFAULT
                                        principal.setear();
                                        principal.ambito = "FOR";
                                        principal.padre = pila.ElementAt(actual).ambito;
                                        pila.ElementAt(actual).pila.Add(principal);
                                        if (pila.ElementAt(actual).hay == false)
                                            pila.ElementAt(actual).hay = true;
                                        else
                                            pila.ElementAt(actual).actual++;
                                        //AGREGAR VARIABLES A AMBITO ACTUAL
                                        copiarIda();
                                        //INICIAR RECORRIDO IF
                                        pRec(root.ChildNodes.ElementAt(6));
                                        //REALIZAR AUM O DECR DE VARIABLE DE CONTROL
                                        forCon(aux, Convert.ToInt32(aux2.valor), aumDec, fila, columna);
                                        //COPIAR VARIABLES
                                        copiarVuelta();
                                        //TERMINAR RECORRIDO                                      
                                        pila.ElementAt(actual).pila.RemoveAt(pila.ElementAt(actual).actual);
                                        if (pila.ElementAt(actual).actual == 0)
                                            pila.ElementAt(actual).hay = false;
                                        else
                                            pila.ElementAt(actual).actual--;

                                        if (contraM > 9000){
                                            addError("FOR enciclado, supero las 9000 iteraciones", root.Span.Location.Line, root.Span.Location.Column);
                                            break;
                                        }
                                        contraM++;
                                    }
                                    interrupt = false;
                                }
                            }else { 
                                //ERROR
                                addError("La variable de control de un ciclo for no puede ser una llamada a metodo", root.Span.Location.Line, root.Span.Location.Column);
                            }
                        }else { 
                            //ERROR
                            addError("El valor final de un ciclo for debe ser tipo INT", root.Span.Location.Line, root.Span.Location.Column);
                        }
                    }
                    break;
                case "EXTRA":
                    /*
                        EXTRA.Rule = ASIG
                            | ACC_MTRX
                            | tId
                     */
                    temp = root.ChildNodes.ElementAt(0).ToString().Trim();
                    if (temp.Equals("ASIG") || temp.Equals("ACC_MTRX")){
                        return pRec(root.ChildNodes.ElementAt(0));
                    }else {
                        valor = root.ChildNodes.ElementAt(0).ToString().Split(' ');
                        nuevo.nombrre = valor[0];                        
                        return nuevo;
                    }
                case "SWT":
                    /*
                        SWT.Rule = tSelect + tCase + tParA + EXP + tParC + tOf + tBegin + CASE + OPC_SWT + tEnd
                     */
                    swt = null;
                    aux = pRec(root.ChildNodes.ElementAt(3));
                    if (aux.error != true) {
                        if (aux.tipo != "BOOL"){
                            //swt = aux;
                            expCase.Add(aux);
                            pRec(root.ChildNodes.ElementAt(7));
                            if (interrupt != true) {
                                pRec(root.ChildNodes.ElementAt(8));
                            }
                            //AL TERMINAR
                            interrupt = false;
                            swt = null;
                            sigue = false;
                            expCase.RemoveAt(expCase.Count-1);
                        }else { 
                            //ERROR
                            addError("Switch. No se pueden comparar valores logicos", root.Span.Location.Line, root.Span.Location.Column);
                        }
                    }                    
                    break;
                case "CASE":
                    /*
                        CASE.Rule = EXP + tDosPtos + tBegin + CUERPO_PROC + tEnd
                     */
                    aux = pRec(root.ChildNodes.ElementAt(0));
                    if (aux.error != true) {
                        if (aux.tipo != "BOOL"){                            
                            if (caseCompar(aux, expCase.ElementAt(expCase.Count-1), root.Span.Location.Line, root.Span.Location.Column)==true || sigue==true){
                                //NUEVO AMBITO                                
                                //DEFAULT
                                principal.setear();
                                principal.ambito = "CASE";
                                principal.padre = pila.ElementAt(actual).ambito;
                                pila.ElementAt(actual).pila.Add(principal);
                                if (pila.ElementAt(actual).hay == false)
                                    pila.ElementAt(actual).hay = true;
                                else
                                    pila.ElementAt(actual).actual++;
                                //AGREGAR VARIABLES A AMBITO ACTUAL
                                copiarIda();
                                //INICIAR RECORRIDO IF
                                pRec(root.ChildNodes.ElementAt(3));
                                //COPIAR VARIABLES
                                copiarVuelta();
                                //TERMINAR RECORRIDO         
                                pila.ElementAt(actual).pila.RemoveAt(pila.ElementAt(actual).actual);
                                if (pila.ElementAt(actual).actual == 0)
                                    pila.ElementAt(actual).hay = false;
                                else
                                    pila.ElementAt(actual).actual--;
                                if (interrupt != true)
                                    sigue = true;
                                else
                                    sigue = false;
                            }
                        }else {
                            //ERROR
                            addError("CASE. No se pueden comparar valores logicos", root.Span.Location.Line, root.Span.Location.Column);
                        }
                    }
                    break;
                case "OPC_SWT":
                    /*
                        OPC_SWT.Rule = CASE + OPC_SWT
                            | tElse + tDosPtos + tBegin + CUERPO_PROC + tEnd
                            | EPS
                     */
                    if (root.ChildNodes.Count == 2) {
                        if (interrupt == false) {
                            pRec(root.ChildNodes.ElementAt(0));
                            if (interrupt == false){
                                pRec(root.ChildNodes.ElementAt(1));
                            }
                        }
                    }else if (root.ChildNodes.Count > 2){
                        if (interrupt == false) {
                            //DEFAULT
                            principal.setear();
                            principal.ambito = "CASE";
                            principal.padre = pila.ElementAt(actual).ambito;
                            pila.ElementAt(actual).pila.Add(principal);
                            if (pila.ElementAt(actual).hay == false)
                                pila.ElementAt(actual).hay = true;
                            else
                                pila.ElementAt(actual).actual++;
                            //AGREGAR VARIABLES A AMBITO ACTUAL
                            copiarIda();
                            //INICIAR RECORRIDO IF
                            pRec(root.ChildNodes.ElementAt(3));
                            //COPIAR VARIABLES
                            copiarVuelta();
                            //TERMINAR RECORRIDO         
                            pila.ElementAt(actual).pila.RemoveAt(pila.ElementAt(actual).actual);
                            if (pila.ElementAt(actual).actual == 0)
                                pila.ElementAt(actual).hay = false;
                            else
                                pila.ElementAt(actual).actual--;
                        }
                    }
                    break;
                case "GRAF":
                    /*
                        GRAF.Rule = tGraf + tParA + TP_GRF + tId + PRE_TP + tParC + tPtoComa
                     */
                    aux = pRec(root.ChildNodes.ElementAt(2));
                    aux2 = pRec(root.ChildNodes.ElementAt(4));
                    //MessageBox.Show(aux2.parametros.Count.ToString());                    
                    if (aux.error != true && aux2.error != true) { 
                        //BUSCAR PROCEDIMIENTO
                        valor = root.ChildNodes.ElementAt(3).ToString().Split(' ');
                        List<String> imp = new List<string>();//IMPORTACION DE ORIGEN
                        //BUSCAR EN AMBITO ACTUAL
                        for (int i = 0; i < tsG.Count; i++){
                            if (tsG.ElementAt(i).ambito == pila.ElementAt(actual).padre && tsG.ElementAt(i).rol=="proc") { //MISMO AMBITO y es PROC                                
                                if (tsG.ElementAt(i).nombrre == valor[0]){//MISMO NOMBRE                                    
                                    if (tsG.ElementAt(i).tipo == aux.tipo) {                                        
                                        if (tsG.ElementAt(i).parametros.Count == aux2.parametros.Count) { //MISMO NUMERO DE PARAMETROS
                                            //MessageBox.Show("mismo");
                                            bool valida = false;
                                            for (int j = 0; j < tsG.ElementAt(i).parametros.Count; j++){//  VALIDAR QUE COINCIDAN LOS TIPOS
                                                if (tsG.ElementAt(i).parametros.ElementAt(j).tipo != aux2.parametros.ElementAt(j).tipo) {
                                                    valida = true;
                                                    break;
                                                }
                                            }
                                            if (valida != true){//TODO COINCIDE
                                                /*principal.setear();
                                                principal.ambito = tsG.ElementAt(i).nombrre;
                                                principal.padre = tsG.ElementAt(i).ambito;
                                                principal.tipo = tsG.ElementAt(i).tipo;
                                                pila.Add(principal);
                                                actual++;
                                                //AGREGAR PARAMETROS A AMBITO ACTUAL
                                                for (int k = 0; k < tsG.ElementAt(i).parametros.Count; k++){
                                                    Tupla auxM = new Tupla();
                                                    auxM.tipo = tsG.ElementAt(i).parametros.ElementAt(k).tipo;
                                                    auxM.nombrre = tsG.ElementAt(i).parametros.ElementAt(k).nombrre;
                                                    auxM.valor = valDefault(auxM.tipo).valor;
                                                    auxM.rol = "var";
                                                    auxM.ambito = tsG.ElementAt(i).nombrre;
                                                    pila.ElementAt(actual).tabla.Add(auxM);                                                                
                                                }
                                                //iniciar recorrido de PROCEDIMIENTO
                                                pRec(tsG.ElementAt(i).root);
                                                //TERMINA
                                                if (tsG.ElementAt(i).tipo != "VOID") {
                                                    if (pila.ElementAt(actual).retorno != null){
                                                        nuevo.tipo = pila.ElementAt(actual).tipo;
                                                        nuevo.valor = pila.ElementAt(actual).retorno;
                                                    }else{
                                                        //ERROR
                                                        addError("Fallo en obtener un valor de retorno del procedimiento", root.Span.Location.Line, root.Span.Location.Column);
                                                        nuevo.error = true;
                                                        nuevo.tipo = "INT";
                                                        nuevo.valor = 0;
                                                    }
                                                }                                                
                                                pila.RemoveAt(actual);
                                                actual--;
                                                llamada = false;*/
                                            }else { 
                                                //ERROR
                                                addError("GRAF. No coinciden los tipos de parametros proporcionados con los establecidos", root.Span.Location.Line, root.Span.Location.Column);
                                            }
                                            flag = true;
                                            break;
                                        }
                                    }
                                }
                            }else if (tsG.ElementAt(i).rol == "import" && tsG.ElementAt(i).ambito == pila.ElementAt(actual).padre){
                                imp.Add(tsG.ElementAt(i).nombrre);
                            }
                        }
                        if (flag != true) { 
                            bool salir = true;
                            int salAux = 0;                            
                            while (salir) {
                                //RECORRER LOS IMPORT
                                for (int i  = 0; i  < imp.Count; i ++){
                                    //MessageBox.Show(imp.ElementAt(i));
                                    for (int j = 0; j < tsG.Count; j++){
                                        if (tsG.ElementAt(j).ambito == imp.ElementAt(i)) { //AMBITO DE IMPORT
                                            if (tsG.ElementAt(j).rol == "proc") { //ES PROCEDIMIENTO
                                                if (tsG.ElementAt(j).nombrre == valor[0] && tsG.ElementAt(j).parametros.Count == aux2.parametros.Count) { //MISMO NOMBRE Y NUM DE PARAMETROS
                                                    //MessageBox.Show("encontrado");
                                                    bool exx = false;
                                                    for (int k = 0; k < tsG.ElementAt(i).parametros.Count; k++){
                                                        if (tsG.ElementAt(j).parametros.ElementAt(k).tipo != aux2.parametros.ElementAt(k).tipo){
                                                            exx = true;
                                                            break;
                                                        }
                                                    }                                                    
                                                    if (exx != true){//TODO COINCIDE
                                                        /*principal.setear();
                                                        principal.ambito = tsG.ElementAt(j).nombrre;
                                                        principal.padre = tsG.ElementAt(j).ambito;
                                                        principal.tipo = tsG.ElementAt(j).tipo;
                                                        pila.Add(principal);
                                                        actual++;
                                                        //AGREGAR PARAMETROS A AMBITO ACTUAL
                                                        for (int k = 0; k < tsG.ElementAt(j).parametros.Count; k++)
                                                        {
                                                            Tupla auxM = new Tupla();
                                                            auxM.tipo = tsG.ElementAt(j).parametros.ElementAt(k).tipo;
                                                            auxM.nombrre = tsG.ElementAt(j).parametros.ElementAt(k).nombrre;
                                                            auxM.valor = valDefault(auxM.tipo).valor;
                                                            auxM.rol = "var";
                                                            auxM.ambito = tsG.ElementAt(j).nombrre;
                                                            pila.ElementAt(actual).tabla.Add(auxM);
                                                        }
                                                        //iniciar recorrido de PROCEDIMIENTO
                                                        pRec(tsG.ElementAt(j).root);
                                                        //TERMINA
                                                        if (tsG.ElementAt(j).tipo != "VOID") { 
                                                            if (pila.ElementAt(actual).retorno != null){
                                                                nuevo.tipo = pila.ElementAt(actual).tipo;
                                                                nuevo.valor = pila.ElementAt(actual).retorno;
                                                            }else{
                                                                //ERROR
                                                                addError("Fallo en obtener un valor de retorno del procedimiento", root.Span.Location.Line, root.Span.Location.Column);
                                                                nuevo.error = true;
                                                                nuevo.tipo = "INT";
                                                                nuevo.valor = 0;
                                                            }
                                                        }                                                        
                                                        pila.RemoveAt(actual);
                                                        actual--;
                                                        llamada = false;*/
                                                    }else {
                                                        //ERROR
                                                        addError("Se realizo la llamada a un procedimiento pero el tipo de los valores enviados como parametros no coinciden", root.Span.Location.Line, root.Span.Location.Column);
                                                        nuevo.error = true;
                                                        nuevo.tipo = "INT";
                                                        nuevo.valor = 0;
                                                    }
                                                    
                                                    flag = true;
                                                    salir = false;
                                                    break;
                                                }
                                            }
                                        }
                                    }
                                }
                                //AGREGAR MAS IMPORT
                                if (salir == true) {
                                    List<String> tempImp = new List<string>();
                                    for (int i = 0; i < imp.Count; i++){
                                        for (int j = 0; j < tsG.Count; j++){
                                            if (tsG.ElementAt(j).ambito == imp.ElementAt(i) && tsG.ElementAt(j).rol=="import") {
                                                tempImp.Add(tsG.ElementAt(j).nombrre);
                                                //MessageBox.Show(tsG.ElementAt(j).nombrre);
                                            }
                                        }
                                    }
                                    imp.Clear();
                                    imp.AddRange(tempImp);
                                }                                
                                if (imp.Count==0 || salAux > 9)
                                    salir = false;
                                salAux++;
                            }
                            if (flag != true) {
                                //ERROR
                                addError("NO ORIGEN. No existe ningun procedimiento que coincida con los datos proporcionados en la llamada", root.Span.Location.Line, root.Span.Location.Column);
                                nuevo.error = true;
                                nuevo.tipo = "INT";
                                nuevo.valor = 0;
                            }
                        }
                    }
                    return nuevo;
                case "TP_GRF":
                    /*
                        TP_GRF.Rule = TIPO
                            | EPS
                     */
                    temp = root.ChildNodes.ElementAt(0).ToString().Trim();
                    if (temp.Equals("TIPO")){
                        return pRec(root.ChildNodes.ElementAt(0));
                    }
                    nuevo.tipo = "VOID";
                    return nuevo;
                case "PRE_TP":
                    /*
                        PRE_TP.Rule = TP_PRM
                            | EPS
                     */
                    temp = root.ChildNodes.ElementAt(0).ToString().Trim();
                    if (temp.Equals("TP_PRM")) {
                        return pRec(root.ChildNodes.ElementAt(0));
                    }                    
                    return nuevo;
                case "TP_PRM":
                    /*
                        TP_PRM.Rule = TP_PRM + TIPO
                            | TIPO
                     */
                    if (root.ChildNodes.Count > 1){
                        aux = pRec(root.ChildNodes.ElementAt(0));
                        aux2 = pRec(root.ChildNodes.ElementAt(1));
                        aux.parametros.Add(aux2);
                        return aux;
                    }else {
                        aux = pRec(root.ChildNodes.ElementAt(0));
                        nuevo.parametros.Add(aux);
                        return nuevo;
                    }
                    
            }            
            return null;
        }

        private Tupla exp(ParseTreeNode root) {
            Tupla nuevo = new Tupla();//nuevo creado para devolver valores
            String temp = ""; //temporal para nombre de nodo
            String[] valor;   //temporal split
            Tupla aux;//auxiliar retorno
            Tupla aux2;//auxiliar retorno                        
            bool flag = false;// hay o no error
            bool subflag = false;//validar ambito
            Ambito acc;
            Ambito principal = new Ambito();//agregar un ambito nuevo

            switch (root.Term.Name.ToString()) { 
                case "EXP":
                    if (root.ChildNodes.Count() == 1) {
                        /*
                            | ART
                         */
                        return exp(root.ChildNodes.ElementAt(0));
                    } else if (root.ChildNodes.Count() == 2) {
                        /*
                            | tNot + EXP
                         */
                        aux = exp(root.ChildNodes.ElementAt(1));
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
                                    addError("No se puede negar un valor de tipo INT excepto 0 y 1", root.Span.Location.Line, root.Span.Location.Column);
                                    aux.error = true;
                                }
                                aux.valor = false;
                            }                            
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
                                        addError("No se puede negar un valor de tipo DOUBLE excepto 0.0 y 1.0.", root.Span.Location.Line, root.Span.Location.Column);
                                        aux.error = true;
                                    }
                                    aux.valor = false;
                                }
                            }else{
                                //ERROR
                                if (aux.error != true) {                                                                        
                                    addError("No se puede negar un valor de tipo DOUBLE excepto 0.0 y 1.0.", root.Span.Location.Line, root.Span.Location.Column);
                                    aux.error = true;
                                }
                                aux.valor = false;
                            }                            
                        }else if (aux.tipo == "CHAR"){
                            if ((Int32)Convert.ToChar(aux.valor) == 0)
                                aux.valor = true;
                            else if ((Int32)Convert.ToChar(aux.valor) == 1)
                                aux.valor = false;
                            else{
                                //ERROR
                                if (aux.error != true){                                                                        
                                    addError("No se puede negar un valor de tipo CHAR excepto el equivalente ascii de 0 y 1.", root.Span.Location.Line, root.Span.Location.Column);
                                    aux.error = true;
                                }
                                aux.valor = false;
                            }                            
                        }else{
                            //ERROR
                            if (aux.error != true){                                                                
                                addError("No se puede negar elementos de tipo STRING", root.Span.Location.Line, root.Span.Location.Column);
                                aux.error = true;
                            }                            
                            aux.valor = false;
                        }
                        aux.tipo = "BOOL";
                        return aux;
                    }else{
                        /*
                                |  EXP + tOr + EXP
                                | EXP + tAnd + EXP
                                | ART + REL + ART                                                                                                                            
                         */
                        temp = root.ChildNodes.ElementAt(1).ToString().Trim();
                        if (temp.Equals("REL")){
                            aux = exp(root.ChildNodes.ElementAt(0));
                            String dato = Convert.ToString(exp(root.ChildNodes.ElementAt(1)).valor);
                            aux2 = exp(root.ChildNodes.ElementAt(2));
                            return logicoRel(aux, dato, aux2, root.Span.Location.Line, root.Span.Location.Column);
                        }else{
                            aux = exp(root.ChildNodes.ElementAt(0));
                            aux2 = exp(root.ChildNodes.ElementAt(2));
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
                     */
                    valor = root.ChildNodes.ElementAt(0).ToString().Split(' ');                    
                    nuevo.valor = valor[0];
                    return nuevo; 
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
                        if (valor.Count() >= 2) {                                                        
                            switch (valor[valor.Count()-1]) {
                                case "(tNumero)":
                                    if (valor[0].Contains(".")) {
                                        nuevo.tipo = "DOUBLE";
                                        nuevo.valor = Convert.ToDouble(valor[0]);
                                    } else {
                                        nuevo.tipo = "INT";
                                        nuevo.valor = Convert.ToInt32(valor[0]);
                                    }                                    
                                    break;
                                case "(tId)":
                                    //EN AMBITO GENERAL, PILA
                                    if (pila.ElementAt(actual).hay == false){
                                        for (int i = 0; i < pila.ElementAt(actual).tabla.Count; i++){
                                            if (pila.ElementAt(actual).tabla.ElementAt(i).nombrre == valor[0] && pila.ElementAt(actual).tabla.ElementAt(i).rol == "var"){
                                                if (pila.ElementAt(actual).tabla.ElementAt(i).dimension.Count == 0){//VARIABLE
                                                    nuevo.valor = pila.ElementAt(actual).tabla.ElementAt(i).valor;
                                                    nuevo.tipo = pila.ElementAt(actual).tabla.ElementAt(i).tipo;
                                                }else{ //ES UN ARREGLO
                                                    //ERROR
                                                    addError("LOCAL,Valor. Se intento acceder al valor de un arreglo, no se definio dimensiones", root.Span.Location.Line, root.Span.Location.Column);
                                                }
                                                flag = true;
                                                break;
                                            }
                                        }
                                    }
                                    else
                                    { //EN SUB AMBITO 
                                        acc = pila.ElementAt(actual);
                                        for (int i = 0; i < acc.pila.ElementAt(acc.actual).tabla.Count; i++){
                                            if (acc.pila.ElementAt(acc.actual).tabla.ElementAt(i).nombrre == valor[0] && acc.pila.ElementAt(acc.actual).tabla.ElementAt(i).rol == "var"){
                                                if (acc.pila.ElementAt(acc.actual).tabla.ElementAt(i).dimension.Count == 0){//VARIABLE
                                                    nuevo.valor = acc.pila.ElementAt(acc.actual).tabla.ElementAt(i).valor;
                                                    nuevo.tipo = acc.pila.ElementAt(acc.actual).tabla.ElementAt(i).tipo;
                                                }else{ //ES UN ARREGLO
                                                    //ERROR
                                                    addError("LOCAL,Valor. Se intento acceder al valor de un arreglo, no se definio dimensiones", root.Span.Location.Line, root.Span.Location.Column);
                                                }
                                                flag = true;
                                                break;
                                            }
                                        }
                                    }
                                    //TABLA DE SIMBOLOS
                                    if (flag == false) {
                                        for (int i = 0; i < tsG.Count; i++){
                                            if (tsG.ElementAt(i).nombrre == valor[0] && tsG.ElementAt(i).rol == "var"){                                                
                                                if (tsG.ElementAt(i).ambito == pila.ElementAt(actual).padre){
                                                    if (tsG.ElementAt(i).dimension.Count == 0){
                                                        nuevo.valor = tsG.ElementAt(i).valor;
                                                        nuevo.tipo = tsG.ElementAt(i).tipo;
                                                    }else{
                                                        //ERROR
                                                        addError("GLOBAL,Valor. Se intento acceder al valor de un arreglo, no se definio dimensiones", root.Span.Location.Line, root.Span.Location.Column);
                                                    }  
                                                    flag = true;
                                                    break;
                                                }else{
                                                    //ERROR
                                                    //addError("Se intento acceder a una variable que no pertence al origen(clase)", root.Span.Location.Line, root.Span.Location.Column);
                                                    subflag = true;
                                                }                                                                                                                                              
                                            }
                                        }
                                        if (subflag == true && flag == false) {
                                            //ERROR
                                            addError("Se intento acceder a una variable que no pertence al origen(clase)", root.Span.Location.Line, root.Span.Location.Column);
                                        }else if (flag == false) {
                                            //ERROR
                                            addError("No existe ningun elemento con el nombre especificado, en ningun ambito", root.Span.Location.Line, root.Span.Location.Column);
                                            nuevo.tipo = "INT";
                                            nuevo.valor = 0;
                                            nuevo.error = true;
                                        }                                        
                                    }                                                                                                                                              
                                    break;
                                case "(tCadena)":
                                    String txt = "";
                                    for (int i = 0; i < valor.Count()-1; i++) {
                                        if (i != valor.Count() - 2)
                                            txt += valor[i] + " ";
                                        else
                                            txt += valor[i];
                                    }
                                    nuevo.tipo = "STRING";
                                    nuevo.valor = txt;
                                    break;
                                case "(tCaracter)":
                                    Char[] arr = valor[0].ToCharArray();                                                                        
                                    Char caracter = arr[0];
                                    nuevo.tipo = "CHAR";
                                    nuevo.valor = caracter;
                                    break;
                                case "(tVbool)":
                                    if (valor[0]=="false")                                                                            
                                        nuevo.valor = false;
                                    else                                        
                                        nuevo.valor = true;                                    
                                    nuevo.tipo = "BOOL";
                                    break;                                
                            }
                            return nuevo;                          
                        } else {
                            /*
                                | ACC_MTRX  <---------------------------------------------------------------------
                             */
                            // si es acceso a matriz o llamada a metodo??
                            aux =  pRec(root.ChildNodes.ElementAt(0));

                            if (aux.error != true){
                                if (aux.dimension.Count > 0){ //ES ARREGLO   

			                        bool dimVer = false;			                                                  
                                    if (pila.ElementAt(actual).hay == false) { //AMBITO ACTUAL-------------------------------------------
                                        for (int i = 0; i < pila.ElementAt(actual).tabla.Count; i++){
                                            if (pila.ElementAt(actual).tabla.ElementAt(i).nombrre == aux.nombrre) {
                                                if (pila.ElementAt(actual).tabla.ElementAt(i).dimension.Count > 0){ //SI ES ARREGLO
                                                    if (pila.ElementAt(actual).tabla.ElementAt(i).dimension.Count == aux.dimension.Count){//MISMO NUMERO DE DIMENSIONES
                                                        for (int j = 0; j < pila.ElementAt(actual).tabla.ElementAt(i).dimension.Count; j++){
                                                            if (pila.ElementAt(actual).tabla.ElementAt(i).dimension.ElementAt(j)-1 < aux.dimension.ElementAt(j)) {
                                                                dimVer = true;
                                                                break;
                                                            }
                                                        }
                                                        if (dimVer == false){                                                            
                                                                //Mtrx mt = (Mtrx)pila.ElementAt(actual).tabla.ElementAt(i).valor;                                                                
                                                            //Mtrx mt = (Mtrx)pila.ElementAt(actual).tabla.ElementAt(i).valor;
                                                            //if (aux.dimension.Count == 1){//UNA DIMENSION
                                                            //    switch (pila.ElementAt(actual).tabla.ElementAt(i).tipo){
                                                            //        case "INT":
                                                            //            nuevo.valor = Convert.ToInt32(mt.elem[aux.dimension.ElementAt(0)]);
                                                            //            break;
                                                            //        case "DOUBLE":
                                                            //            nuevo.valor = Convert.ToDouble(mt.elem[aux.dimension.ElementAt(0)]);
                                                            //            break;
                                                            //        case "CHAR":
                                                            //            nuevo.valor = Convert.ToChar(mt.elem[aux.dimension.ElementAt(0)]);
                                                            //            break;
                                                            //        case "STRING":
                                                            //            nuevo.valor = Convert.ToString(mt.elem[aux.dimension.ElementAt(0)]);
                                                            //            break;
                                                            //        case "BOOL":
                                                            //            nuevo.valor = Convert.ToBoolean(mt.elem[aux.dimension.ElementAt(0)]);
                                                            //            break;
                                                            //    }
                                                            //    nuevo.tipo = pila.ElementAt(actual).tabla.ElementAt(i).tipo;
                                                            //}else{ // N DIMENSIONES
                                                            //    for (int k = 0; k < aux.dimension.Count; k++){                                                                                                                                        
                                                            //        if (k != aux.dimension.Count - 1){ //NO ES ULTIMO ELEMNTO
                                                            //            mt = (Mtrx)mt.elem.ElementAt(aux.dimension.ElementAt(k));
                                                            //        }else{ //ULTIMO ELEMENTO     
                                                                        
                                                            //            switch (pila.ElementAt(actual).tabla.ElementAt(i).tipo)
                                                            //            {
                                                            //                case "INT":
                                                            //                    nuevo.valor = Convert.ToInt32(mt.elem[aux.dimension.ElementAt(k)]);
                                                            //                    break;
                                                            //                case "DOUBLE":
                                                            //                    nuevo.valor = Convert.ToDouble(mt.elem[aux.dimension.ElementAt(k)]);
                                                            //                    break;
                                                            //                case "CHAR":
                                                            //                    nuevo.valor = Convert.ToChar(mt.elem[aux.dimension.ElementAt(k)]);
                                                            //                    break;
                                                            //                case "STRING":
                                                            //                    //MessageBox.Show(Convert.ToString( mt.elem.ElementAt(aux.dimension.ElementAt(k))));
                                                            //                    nuevo.valor = Convert.ToString(mt.elem[aux.dimension.ElementAt(k)]);
                                                            //                    break;
                                                            //                case "BOOL":
                                                            //                    nuevo.valor = Convert.ToBoolean(mt.elem[aux.dimension.ElementAt(k)]);
                                                            //                    break;
                                                            //            }
                                                            //            nuevo.tipo = pila.ElementAt(actual).tabla.ElementAt(i).tipo;
                                                            //        }
                                                            //    }
                                                            //}    
                                                            switch (pila.ElementAt(actual).tabla.ElementAt(i).tipo)
                                                            {
                                                                case "INT":
                                                                    if (aux.dimension.Count == 1)
                                                                    {
                                                                        int[] a = (int[])pila.ElementAt(actual).tabla.ElementAt(i).valor;
                                                                        nuevo.valor = a[aux.dimension.ElementAt(0)];
                                                                    }
                                                                    else if (aux.dimension.Count == 2)
                                                                    {
                                                                        int[,] a = (int[,])pila.ElementAt(actual).tabla.ElementAt(i).valor;
                                                                        nuevo.valor = a[aux.dimension.ElementAt(0), aux.dimension.ElementAt(1)];
                                                                    }
                                                                    else if (aux.dimension.Count == 3)
                                                                    {
                                                                        int[, ,] a = (int[, ,])pila.ElementAt(actual).tabla.ElementAt(i).valor;
                                                                        nuevo.valor = a[aux.dimension.ElementAt(0), aux.dimension.ElementAt(1), aux.dimension.ElementAt(2)];
                                                                    }
                                                                    else if (aux.dimension.Count == 4)
                                                                    {
                                                                        int[, , ,] a = (int[, , ,])pila.ElementAt(actual).tabla.ElementAt(i).valor;
                                                                        nuevo.valor = a[aux.dimension.ElementAt(0), aux.dimension.ElementAt(1), aux.dimension.ElementAt(2), aux.dimension.ElementAt(3)];
                                                                    }
                                                                    break;
                                                                case "DOUBLE":
                                                                    if (aux.dimension.Count == 1)
                                                                    {
                                                                        double[] a = (double[])pila.ElementAt(actual).tabla.ElementAt(i).valor;
                                                                        nuevo.valor = a[aux.dimension.ElementAt(0)];
                                                                    }
                                                                    else if (aux.dimension.Count == 2)
                                                                    {
                                                                        double[,] a = (double[,])pila.ElementAt(actual).tabla.ElementAt(i).valor;
                                                                        nuevo.valor = a[aux.dimension.ElementAt(0), aux.dimension.ElementAt(1)];
                                                                    }
                                                                    else if (aux.dimension.Count == 3)
                                                                    {
                                                                        double[, ,] a = (double[, ,])pila.ElementAt(actual).tabla.ElementAt(i).valor;
                                                                        nuevo.valor = a[aux.dimension.ElementAt(0), aux.dimension.ElementAt(1), aux.dimension.ElementAt(2)];
                                                                    }
                                                                    else if (aux.dimension.Count == 4)
                                                                    {
                                                                        double[, , ,] a = (double[, , ,])pila.ElementAt(actual).tabla.ElementAt(i).valor;
                                                                        nuevo.valor = a[aux.dimension.ElementAt(0), aux.dimension.ElementAt(1), aux.dimension.ElementAt(2), aux.dimension.ElementAt(3)];
                                                                    }
                                                                    break;
                                                                case "CHAR":
                                                                    if (aux.dimension.Count == 1)
                                                                    {
                                                                        char[] a = (char[])pila.ElementAt(actual).tabla.ElementAt(i).valor;
                                                                        nuevo.valor = a[aux.dimension.ElementAt(0)];
                                                                    }
                                                                    else if (aux.dimension.Count == 2)
                                                                    {
                                                                        char[,] a = (char[,])pila.ElementAt(actual).tabla.ElementAt(i).valor;
                                                                        nuevo.valor = a[aux.dimension.ElementAt(0), aux.dimension.ElementAt(1)];
                                                                    }
                                                                    else if (aux.dimension.Count == 3)
                                                                    {
                                                                        char[, ,] a = (char[, ,])pila.ElementAt(actual).tabla.ElementAt(i).valor;
                                                                        nuevo.valor = a[aux.dimension.ElementAt(0), aux.dimension.ElementAt(1), aux.dimension.ElementAt(2)];
                                                                    }
                                                                    else if (aux.dimension.Count == 4)
                                                                    {
                                                                        char[, , ,] a = (char[, , ,])pila.ElementAt(actual).tabla.ElementAt(i).valor;
                                                                        nuevo.valor = a[aux.dimension.ElementAt(0), aux.dimension.ElementAt(1), aux.dimension.ElementAt(2), aux.dimension.ElementAt(3)];
                                                                    }
                                                                    break;
                                                                case "STRING":
                                                                    if (aux.dimension.Count == 1)
                                                                    {
                                                                        string[] a = (string[])pila.ElementAt(actual).tabla.ElementAt(i).valor;
                                                                        nuevo.valor = a[aux.dimension.ElementAt(0)];
                                                                    }
                                                                    else if (aux.dimension.Count == 2)
                                                                    {
                                                                        string[,] a = (string[,])pila.ElementAt(actual).tabla.ElementAt(i).valor;
                                                                        nuevo.valor = a[aux.dimension.ElementAt(0), aux.dimension.ElementAt(1)];
                                                                    }
                                                                    else if (aux.dimension.Count == 3)
                                                                    {
                                                                        string[, ,] a = (string[, ,])pila.ElementAt(actual).tabla.ElementAt(i).valor;
                                                                        nuevo.valor = a[aux.dimension.ElementAt(0), aux.dimension.ElementAt(1), aux.dimension.ElementAt(2)];
                                                                    }
                                                                    else if (aux.dimension.Count == 4)
                                                                    {
                                                                        string[, , ,] a = (string[, , ,])pila.ElementAt(actual).tabla.ElementAt(i).valor;
                                                                        nuevo.valor = a[aux.dimension.ElementAt(0), aux.dimension.ElementAt(1), aux.dimension.ElementAt(2), aux.dimension.ElementAt(3)];
                                                                    }
                                                                    break;
                                                                case "BOOL":
                                                                    if (aux.dimension.Count == 1)
                                                                    {
                                                                        bool[] a = (bool[])pila.ElementAt(actual).tabla.ElementAt(i).valor;
                                                                        nuevo.valor = a[aux.dimension.ElementAt(0)];
                                                                    }
                                                                    else if (aux.dimension.Count == 2)
                                                                    {
                                                                        bool[,] a = (bool[,])pila.ElementAt(actual).tabla.ElementAt(i).valor;
                                                                        nuevo.valor = a[aux.dimension.ElementAt(0), aux.dimension.ElementAt(1)];
                                                                    }
                                                                    else if (aux.dimension.Count == 3)
                                                                    {
                                                                        bool[, ,] a = (bool[, ,])pila.ElementAt(actual).tabla.ElementAt(i).valor;
                                                                        nuevo.valor = a[aux.dimension.ElementAt(0), aux.dimension.ElementAt(1), aux.dimension.ElementAt(2)];
                                                                    }
                                                                    else if (aux.dimension.Count == 4)
                                                                    {
                                                                        bool[, , ,] a = (bool[, , ,])pila.ElementAt(actual).tabla.ElementAt(i).valor;
                                                                        nuevo.valor = a[aux.dimension.ElementAt(0), aux.dimension.ElementAt(1), aux.dimension.ElementAt(2), aux.dimension.ElementAt(3)];
                                                                    }
                                                                    break;
                                                            }
                                                            nuevo.tipo = pila.ElementAt(actual).tabla.ElementAt(i).tipo;
                                                        }else { 
                                                            //ERROR
                                                            addError("Se intenta asignar un valor a una posicion de un arreglo, pero se excede el valor de las dimensiones establecidas", root.Span.Location.Line, root.Span.Location.Column);
                                                            dimVer = false;
                                                            nuevo.error = true;
                                                            nuevo.tipo = "INT";
                                                            nuevo.valor = 0;
                                                        }
                                                    }else { 
                                                        //  ERROR
                                                        addError("Se intenta asignar un valor a una posicion de un arreglo, pero el numero de dimensiones no coincide", root.Span.Location.Line, root.Span.Location.Column);
                                                        nuevo.error = true;
                                                        nuevo.tipo = "INT";
                                                        nuevo.valor = 0;
                                                    }
                                                } else { //NO ES ARREGLO
                                                    //ERRROR
                                                    addError("Se intenta asignar un valor a una posicion de un arreglo, pero la variable no es un arreglo", root.Span.Location.Line, root.Span.Location.Column);
                                                    nuevo.error = true;
                                                    nuevo.tipo = "INT";
                                                    nuevo.valor = 0;
                                                }
                                                flag = true;
                                                break;
                                            }
                                        }
                                    } else {//SUB AMBITO-----------------------------------------------------
                                        acc = pila.ElementAt(actual);
                                		for (int i = 0; i < acc.pila.ElementAt(acc.actual).tabla.Count; i++){
                                            if (acc.pila.ElementAt(acc.actual).tabla.ElementAt(i).nombrre == aux.nombrre) {
                                                if (acc.pila.ElementAt(acc.actual).tabla.ElementAt(i).dimension.Count > 0){ //SI ES ARREGLO
                                                    if (acc.pila.ElementAt(acc.actual).tabla.ElementAt(i).dimension.Count == aux.dimension.Count){//MISMO NUMERO DE DIMENSIONES
                                                        for (int j = 0; j < acc.pila.ElementAt(acc.actual).tabla.ElementAt(i).dimension.Count; j++){
                                                            if (acc.pila.ElementAt(acc.actual).tabla.ElementAt(i).dimension.ElementAt(j)-1 < aux.dimension.ElementAt(j)) {
                                                                dimVer = true;
                                                                break;
                                                            }
                                                        }
                                                        if (dimVer == false){                                                            
                                                                //Mtrx mt = (Mtrx)acc.pila.ElementAt(acc.actual).tabla.ElementAt(i).valor;                                                               
                                                            //Mtrx mt = (Mtrx)acc.pila.ElementAt(acc.actual).tabla.ElementAt(i).valor;
                                                            //if (aux.dimension.Count == 1){//UNA DIMENSION
                                                            //    switch (acc.pila.ElementAt(acc.actual).tabla.ElementAt(i).tipo){
                                                            //        case "INT":
                                                            //            nuevo.valor = Convert.ToInt32(mt.elem[aux.dimension.ElementAt(0)]);
                                                            //            break;
                                                            //        case "DOUBLE":
                                                            //            nuevo.valor = Convert.ToDouble(mt.elem[aux.dimension.ElementAt(0)]);
                                                            //            break;
                                                            //        case "CHAR":
                                                            //            nuevo.valor = Convert.ToChar(mt.elem[aux.dimension.ElementAt(0)]);
                                                            //            break;
                                                            //        case "STRING":
                                                            //            nuevo.valor = Convert.ToString(mt.elem[aux.dimension.ElementAt(0)]);
                                                            //            break;
                                                            //        case "BOOL":
                                                            //            nuevo.valor = Convert.ToBoolean(mt.elem[aux.dimension.ElementAt(0)]);
                                                            //            break;
                                                            //    }
                                                            //    nuevo.tipo = acc.pila.ElementAt(acc.actual).tabla.ElementAt(i).tipo;
                                                            //}else{ // N DIMENSIONES
                                                            //    for (int k = 0; k < aux.dimension.Count; k++){
                                                            //        if (k != aux.dimension.Count - 1){ //NO ES ULTIMO ELEMNTO
                                                            //            mt = (Mtrx)mt.elem.ElementAt(aux.dimension.ElementAt(k));
                                                            //        }else{ //ULTIMO ELEMENTO                                                                        
                                                            //            switch (acc.pila.ElementAt(acc.actual).tabla.ElementAt(i).tipo){
                                                            //                case "INT":
                                                            //                    nuevo.valor = Convert.ToInt32(mt.elem[aux.dimension.ElementAt(k)]);
                                                            //                    break;
                                                            //                case "DOUBLE":
                                                            //                    nuevo.valor = Convert.ToDouble(mt.elem[aux.dimension.ElementAt(k)]);
                                                            //                    break;
                                                            //                case "CHAR":
                                                            //                    nuevo.valor = Convert.ToChar(mt.elem[aux.dimension.ElementAt(k)]);
                                                            //                    break;
                                                            //                case "STRING":
                                                            //                    nuevo.valor = Convert.ToString(mt.elem[aux.dimension.ElementAt(k)]);
                                                            //                    break;
                                                            //                case "BOOL":
                                                            //                    nuevo.valor = Convert.ToBoolean(mt.elem[aux.dimension.ElementAt(k)]);
                                                            //                    break;
                                                            //            }
                                                            //            nuevo.tipo = acc.pila.ElementAt(acc.actual).tabla.ElementAt(i).tipo;
                                                            //        }
                                                            //    }
                                                            //}      
                                                            switch (acc.pila.ElementAt(acc.actual).tabla.ElementAt(i).tipo)
                                                            {
                                                                case "INT":
                                                                    if (aux.dimension.Count == 1)
                                                                    {
                                                                        int[] a = (int[])acc.pila.ElementAt(acc.actual).tabla.ElementAt(i).valor;
                                                                        nuevo.valor = a[aux.dimension.ElementAt(0)];
                                                                    }
                                                                    else if (aux.dimension.Count == 2)
                                                                    {
                                                                        int[,] a = (int[,])acc.pila.ElementAt(acc.actual).tabla.ElementAt(i).valor;
                                                                        nuevo.valor = a[aux.dimension.ElementAt(0), aux.dimension.ElementAt(1)];
                                                                    }
                                                                    else if (aux.dimension.Count == 3)
                                                                    {
                                                                        int[, ,] a = (int[, ,])acc.pila.ElementAt(acc.actual).tabla.ElementAt(i).valor;
                                                                        nuevo.valor = a[aux.dimension.ElementAt(0), aux.dimension.ElementAt(1), aux.dimension.ElementAt(2)];
                                                                    }
                                                                    else if (aux.dimension.Count == 4)
                                                                    {
                                                                        int[, , ,] a = (int[, , ,])acc.pila.ElementAt(acc.actual).tabla.ElementAt(i).valor;
                                                                        nuevo.valor = a[aux.dimension.ElementAt(0), aux.dimension.ElementAt(1), aux.dimension.ElementAt(2), aux.dimension.ElementAt(3)];
                                                                    }
                                                                    break;
                                                                case "DOUBLE":
                                                                    if (aux.dimension.Count == 1)
                                                                    {
                                                                        double[] a = (double[])acc.pila.ElementAt(acc.actual).tabla.ElementAt(i).valor;
                                                                        nuevo.valor = a[aux.dimension.ElementAt(0)];
                                                                    }
                                                                    else if (aux.dimension.Count == 2)
                                                                    {
                                                                        double[,] a = (double[,])acc.pila.ElementAt(acc.actual).tabla.ElementAt(i).valor;
                                                                        nuevo.valor = a[aux.dimension.ElementAt(0), aux.dimension.ElementAt(1)];
                                                                    }
                                                                    else if (aux.dimension.Count == 3)
                                                                    {
                                                                        double[, ,] a = (double[, ,])acc.pila.ElementAt(acc.actual).tabla.ElementAt(i).valor;
                                                                        nuevo.valor = a[aux.dimension.ElementAt(0), aux.dimension.ElementAt(1), aux.dimension.ElementAt(2)];
                                                                    }
                                                                    else if (aux.dimension.Count == 4)
                                                                    {
                                                                        double[, , ,] a = (double[, , ,])acc.pila.ElementAt(acc.actual).tabla.ElementAt(i).valor;
                                                                        nuevo.valor = a[aux.dimension.ElementAt(0), aux.dimension.ElementAt(1), aux.dimension.ElementAt(2), aux.dimension.ElementAt(3)];
                                                                    }
                                                                    break;
                                                                case "CHAR":
                                                                    if (aux.dimension.Count == 1)
                                                                    {
                                                                        char[] a = (char[])acc.pila.ElementAt(acc.actual).tabla.ElementAt(i).valor;
                                                                        nuevo.valor = a[aux.dimension.ElementAt(0)];
                                                                    }
                                                                    else if (aux.dimension.Count == 2)
                                                                    {
                                                                        char[,] a = (char[,])acc.pila.ElementAt(acc.actual).tabla.ElementAt(i).valor;
                                                                        nuevo.valor = a[aux.dimension.ElementAt(0), aux.dimension.ElementAt(1)];
                                                                    }
                                                                    else if (aux.dimension.Count == 3)
                                                                    {
                                                                        char[, ,] a = (char[, ,])acc.pila.ElementAt(acc.actual).tabla.ElementAt(i).valor;
                                                                        nuevo.valor = a[aux.dimension.ElementAt(0), aux.dimension.ElementAt(1), aux.dimension.ElementAt(2)];
                                                                    }
                                                                    else if (aux.dimension.Count == 4)
                                                                    {
                                                                        char[, , ,] a = (char[, , ,])acc.pila.ElementAt(acc.actual).tabla.ElementAt(i).valor;
                                                                        nuevo.valor = a[aux.dimension.ElementAt(0), aux.dimension.ElementAt(1), aux.dimension.ElementAt(2), aux.dimension.ElementAt(3)];
                                                                    }
                                                                    break;
                                                                case "STRING":
                                                                    if (aux.dimension.Count == 1)
                                                                    {
                                                                        string[] a = (string[])acc.pila.ElementAt(acc.actual).tabla.ElementAt(i).valor;
                                                                        nuevo.valor = a[aux.dimension.ElementAt(0)];
                                                                    }
                                                                    else if (aux.dimension.Count == 2)
                                                                    {
                                                                        string[,] a = (string[,])acc.pila.ElementAt(acc.actual).tabla.ElementAt(i).valor;
                                                                        nuevo.valor = a[aux.dimension.ElementAt(0), aux.dimension.ElementAt(1)];
                                                                    }
                                                                    else if (aux.dimension.Count == 3)
                                                                    {
                                                                        string[, ,] a = (string[, ,])acc.pila.ElementAt(acc.actual).tabla.ElementAt(i).valor;
                                                                        nuevo.valor = a[aux.dimension.ElementAt(0), aux.dimension.ElementAt(1), aux.dimension.ElementAt(2)];
                                                                    }
                                                                    else if (aux.dimension.Count == 4)
                                                                    {
                                                                        string[, , ,] a = (string[, , ,])acc.pila.ElementAt(acc.actual).tabla.ElementAt(i).valor;
                                                                        nuevo.valor = a[aux.dimension.ElementAt(0), aux.dimension.ElementAt(1), aux.dimension.ElementAt(2), aux.dimension.ElementAt(3)];
                                                                    }
                                                                    break;
                                                                case "BOOL":
                                                                    if (aux.dimension.Count == 1)
                                                                    {
                                                                        bool[] a = (bool[])acc.pila.ElementAt(acc.actual).tabla.ElementAt(i).valor;
                                                                        nuevo.valor = a[aux.dimension.ElementAt(0)];
                                                                    }
                                                                    else if (aux.dimension.Count == 2)
                                                                    {
                                                                        bool[,] a = (bool[,])acc.pila.ElementAt(acc.actual).tabla.ElementAt(i).valor;
                                                                        nuevo.valor = a[aux.dimension.ElementAt(0), aux.dimension.ElementAt(1)];
                                                                    }
                                                                    else if (aux.dimension.Count == 3)
                                                                    {
                                                                        bool[, ,] a = (bool[, ,])acc.pila.ElementAt(acc.actual).tabla.ElementAt(i).valor;
                                                                        nuevo.valor = a[aux.dimension.ElementAt(0), aux.dimension.ElementAt(1), aux.dimension.ElementAt(2)];
                                                                    }
                                                                    else if (aux.dimension.Count == 4)
                                                                    {
                                                                        bool[, , ,] a = (bool[, , ,])acc.pila.ElementAt(acc.actual).tabla.ElementAt(i).valor;
                                                                        nuevo.valor = a[aux.dimension.ElementAt(0), aux.dimension.ElementAt(1), aux.dimension.ElementAt(2), aux.dimension.ElementAt(3)];
                                                                    }
                                                                    break;
                                                            }
                                                            nuevo.tipo = acc.pila.ElementAt(acc.actual).tabla.ElementAt(i).tipo;
                                                        }else { 
                                                            //ERROR
                                                            addError("Se intenta asignar un valor a una posicion de un arreglo, pero se excede el valor de las dimensiones establecidas", root.Span.Location.Line, root.Span.Location.Column);
                                                            dimVer = false;
                                                            nuevo.error = true;
                                                            nuevo.tipo = "INT";
                                                            nuevo.valor = 0;
                                                        }
                                                    }else { 
                                                        //  ERROR
                                                        addError("Se intenta asignar un valor a una posicion de un arreglo, pero el numero de dimensiones no coincide", root.Span.Location.Line, root.Span.Location.Column);
                                                        nuevo.error = true;
                                                        nuevo.tipo = "INT";
                                                        nuevo.valor = 0;
                                                    }
                                                } else { //NO ES ARREGLO
                                                    //ERRROR
                                                    addError("Se intenta asignar un valor a una posicion de un arreglo, pero la variable no es un arreglo", root.Span.Location.Line, root.Span.Location.Column);
                                                    nuevo.error = true;
                                                    nuevo.tipo = "INT";
                                                    nuevo.valor = 0;
                                                }
                                                flag = true;
                                                break;
                                            }
                                        }

                                    }
                                    //TABLA GENERAL---------------------------------------------------------------
                                    if (flag == false) {
                                        for (int i = 0; i < tsG.Count; i++){
                                            if (tsG.ElementAt(i).ambito == pila.ElementAt(actual).padre) {
                                                if (tsG.ElementAt(i).nombrre == aux.nombrre && tsG.ElementAt(i).rol == "var") {
                                                    if (tsG.ElementAt(i).dimension.Count > 0){//SI ES ARREGLO
                                                        if (tsG.ElementAt(i).dimension.Count == aux.dimension.Count){// MISMO NUMERO DE DIMENSIONES
                                                            for (int j = 0; j < tsG.ElementAt(i).dimension.Count; j++){
                                                                if (tsG.ElementAt(i).dimension.ElementAt(j) - 1 < aux.dimension.ElementAt(j)){
                                                                    dimVer = true;
                                                                    break;
                                                                }
                                                            }
                                                            if (dimVer == false){
                                                                //Mtrx mt = (Mtrx)tsG.ElementAt(i).valor;
                                                                //if (aux.dimension.Count == 1){//UNA DIMENSION
                                                                //    switch (tsG.ElementAt(i).tipo){
                                                                //        case "INT":
                                                                //            nuevo.valor = Convert.ToInt32(mt.elem[aux.dimension.ElementAt(0)]);
                                                                //            break;
                                                                //        case "DOUBLE":
                                                                //            nuevo.valor = Convert.ToDouble(mt.elem[aux.dimension.ElementAt(0)]);
                                                                //            break;
                                                                //        case "CHAR":
                                                                //            nuevo.valor = Convert.ToChar(mt.elem[aux.dimension.ElementAt(0)]);
                                                                //            break;
                                                                //        case "STRING":
                                                                //            nuevo.valor = Convert.ToString(mt.elem[aux.dimension.ElementAt(0)]);
                                                                //            break;
                                                                //        case "BOOL":
                                                                //            nuevo.valor = Convert.ToBoolean(mt.elem[aux.dimension.ElementAt(0)]);
                                                                //            break;
                                                                //    }
                                                                //    nuevo.tipo = tsG.ElementAt(i).tipo;
                                                                //}else{ // N DIMENSIONES
                                                                //    for (int k = 0; k < aux.dimension.Count; k++){
                                                                //        if (k != aux.dimension.Count - 1){ //NO ES ULTIMO ELEMNTO
                                                                //            mt = (Mtrx)mt.elem.ElementAt(aux.dimension.ElementAt(k));
                                                                //        }else{ //ULTIMO ELEMENTO                                                                        
                                                                //            switch (tsG.ElementAt(i).tipo)
                                                                //            {
                                                                //                case "INT":
                                                                //                    nuevo.valor = Convert.ToInt32(mt.elem[aux.dimension.ElementAt(k)]);
                                                                //                    break;
                                                                //                case "DOUBLE":
                                                                //                    nuevo.valor = Convert.ToDouble(mt.elem[aux.dimension.ElementAt(k)]);
                                                                //                    break;
                                                                //                case "CHAR":
                                                                //                    nuevo.valor = Convert.ToChar(mt.elem[aux.dimension.ElementAt(k)]);
                                                                //                    break;
                                                                //                case "STRING":
                                                                //                    nuevo.valor = Convert.ToString(mt.elem[aux.dimension.ElementAt(k)]);
                                                                //                    break;
                                                                //                case "BOOL":
                                                                //                    nuevo.valor = Convert.ToBoolean(mt.elem[aux.dimension.ElementAt(k)]);
                                                                //                    break;
                                                                //            }
                                                                //            nuevo.tipo = tsG.ElementAt(i).tipo;
                                                                //        }
                                                                //    }
                                                                //}
                                                                switch (tsG.ElementAt(i).tipo)
                                                                {
                                                                    case "INT":
                                                                        if (aux.dimension.Count == 1)
                                                                        {
                                                                            int[] a = (int[])tsG.ElementAt(i).valor;
                                                                            nuevo.valor = a[aux.dimension.ElementAt(0)];
                                                                        }
                                                                        else if (aux.dimension.Count == 2)
                                                                        {
                                                                            int[,] a = (int[,])tsG.ElementAt(i).valor;
                                                                            nuevo.valor = a[aux.dimension.ElementAt(0), aux.dimension.ElementAt(1)];
                                                                        }
                                                                        else if (aux.dimension.Count == 3)
                                                                        {
                                                                            int[, ,] a = (int[, ,])tsG.ElementAt(i).valor;
                                                                            nuevo.valor = a[aux.dimension.ElementAt(0), aux.dimension.ElementAt(1), aux.dimension.ElementAt(2)];
                                                                        }
                                                                        else if (aux.dimension.Count == 4)
                                                                        {
                                                                            int[, , ,] a = (int[, , ,])tsG.ElementAt(i).valor;
                                                                            nuevo.valor = a[aux.dimension.ElementAt(0), aux.dimension.ElementAt(1), aux.dimension.ElementAt(2), aux.dimension.ElementAt(3)];
                                                                        }
                                                                        break;
                                                                    case "DOUBLE":
                                                                        if (aux.dimension.Count == 1)
                                                                        {
                                                                            double[] a = (double[])tsG.ElementAt(i).valor;
                                                                            nuevo.valor = a[aux.dimension.ElementAt(0)];
                                                                        }
                                                                        else if (aux.dimension.Count == 2)
                                                                        {
                                                                            double[,] a = (double[,])tsG.ElementAt(i).valor;
                                                                            nuevo.valor = a[aux.dimension.ElementAt(0), aux.dimension.ElementAt(1)];
                                                                        }
                                                                        else if (aux.dimension.Count == 3)
                                                                        {
                                                                            double[, ,] a = (double[, ,])tsG.ElementAt(i).valor;
                                                                            nuevo.valor = a[aux.dimension.ElementAt(0), aux.dimension.ElementAt(1), aux.dimension.ElementAt(2)];
                                                                        }
                                                                        else if (aux.dimension.Count == 4)
                                                                        {
                                                                            double[, , ,] a = (double[, , ,])tsG.ElementAt(i).valor;
                                                                            nuevo.valor = a[aux.dimension.ElementAt(0), aux.dimension.ElementAt(1), aux.dimension.ElementAt(2), aux.dimension.ElementAt(3)];
                                                                        }
                                                                        break;
                                                                    case "CHAR":
                                                                        if (aux.dimension.Count == 1)
                                                                        {
                                                                            char[] a = (char[])tsG.ElementAt(i).valor;
                                                                            nuevo.valor = a[aux.dimension.ElementAt(0)];
                                                                        }
                                                                        else if (aux.dimension.Count == 2)
                                                                        {
                                                                            char[,] a = (char[,])tsG.ElementAt(i).valor;
                                                                            nuevo.valor = a[aux.dimension.ElementAt(0), aux.dimension.ElementAt(1)];
                                                                        }
                                                                        else if (aux.dimension.Count == 3)
                                                                        {
                                                                            char[, ,] a = (char[, ,])tsG.ElementAt(i).valor;
                                                                            nuevo.valor = a[aux.dimension.ElementAt(0), aux.dimension.ElementAt(1), aux.dimension.ElementAt(2)];
                                                                        }
                                                                        else if (aux.dimension.Count == 4)
                                                                        {
                                                                            char[, , ,] a = (char[, , ,])tsG.ElementAt(i).valor;
                                                                            nuevo.valor = a[aux.dimension.ElementAt(0), aux.dimension.ElementAt(1), aux.dimension.ElementAt(2), aux.dimension.ElementAt(3)];
                                                                        }
                                                                        break;
                                                                    case "STRING":
                                                                        if (aux.dimension.Count == 1)
                                                                        {
                                                                            string[] a = (string[])tsG.ElementAt(i).valor;
                                                                            nuevo.valor = a[aux.dimension.ElementAt(0)];
                                                                        }
                                                                        else if (aux.dimension.Count == 2)
                                                                        {
                                                                            string[,] a = (string[,])tsG.ElementAt(i).valor;
                                                                            nuevo.valor = a[aux.dimension.ElementAt(0), aux.dimension.ElementAt(1)];
                                                                        }
                                                                        else if (aux.dimension.Count == 3)
                                                                        {
                                                                            string[, ,] a = (string[, ,])tsG.ElementAt(i).valor;
                                                                            nuevo.valor = a[aux.dimension.ElementAt(0), aux.dimension.ElementAt(1), aux.dimension.ElementAt(2)];
                                                                        }
                                                                        else if (aux.dimension.Count == 4)
                                                                        {
                                                                            string[, , ,] a = (string[, , ,])tsG.ElementAt(i).valor;
                                                                            nuevo.valor = a[aux.dimension.ElementAt(0), aux.dimension.ElementAt(1), aux.dimension.ElementAt(2), aux.dimension.ElementAt(3)];
                                                                        }
                                                                        break;
                                                                    case "BOOL":
                                                                        if (aux.dimension.Count == 1)
                                                                        {
                                                                            bool[] a = (bool[])tsG.ElementAt(i).valor;
                                                                            nuevo.valor = a[aux.dimension.ElementAt(0)];
                                                                        }
                                                                        else if (aux.dimension.Count == 2)
                                                                        {
                                                                            bool[,] a = (bool[,])tsG.ElementAt(i).valor;
                                                                            nuevo.valor = a[aux.dimension.ElementAt(0), aux.dimension.ElementAt(1)];
                                                                        }
                                                                        else if (aux.dimension.Count == 3)
                                                                        {
                                                                            bool[, ,] a = (bool[, ,])tsG.ElementAt(i).valor;
                                                                            nuevo.valor = a[aux.dimension.ElementAt(0), aux.dimension.ElementAt(1), aux.dimension.ElementAt(2)];
                                                                        }
                                                                        else if (aux.dimension.Count == 4)
                                                                        {
                                                                            bool[, , ,] a = (bool[, , ,])tsG.ElementAt(i).valor;
                                                                            nuevo.valor = a[aux.dimension.ElementAt(0), aux.dimension.ElementAt(1), aux.dimension.ElementAt(2), aux.dimension.ElementAt(3)];
                                                                        }
                                                                        break;
                                                                }
                                                                nuevo.tipo = tsG.ElementAt(i).tipo;
                                                            }else {
                                                                //ERROR
                                                                addError("GLOBAL. Se intenta asignar un valor a una posicion de un arreglo, pero se excede el valor de las dimensiones establecidas", root.Span.Location.Line, root.Span.Location.Column);
                                                                dimVer = false;
                                                                nuevo.error = true;
                                                                nuevo.tipo = "INT";
                                                                nuevo.valor = 0;
                                                            }
                                                        }else { 
                                                            //ERROR
                                                            addError("GLOBAL. Se intenta asignar un valor a una posicion de un arreglo, pero el numero de dimensiones no coincide", root.Span.Location.Line, root.Span.Location.Column);
                                                            nuevo.error = true;
                                                            nuevo.tipo = "INT";
                                                            nuevo.valor = 0;
                                                        }                                                        
                                                    }else { //ELSE
                                                        //ERROR
                                                        addError("Asignacion. Se definieron dimensiones, pero la variable a la que se hace referencia no es un arreglo", root.Span.Location.Line, root.Span.Location.Column);
                                                        nuevo.error = true;
                                                        nuevo.tipo = "INT";
                                                        nuevo.valor = 0;
                                                    }
                                                }
                                            }
                                        }
                                    }	
                                    		                                			                           
                                }else { //ES PROCEDIMIENTO
                                    //MessageBox.Show("llamada a metodo " + aux.nombrre);
                                    flag = false; //SETTER VAR
                                    //BUSCAR EN ORIGEN
                                    List<String> imp = new List<string>();//IMPORTACION DE ORIGEN
                                    bool tipPar = false;                                    
                                    for (int i = 0; i < tsG.Count; i++){
                                        if (tsG.ElementAt(i).ambito == pila.ElementAt(actual).padre) { 
                                            if(tsG.ElementAt(i).rol == "proc"){
                                                if (tsG.ElementAt(i).nombrre == aux.nombrre && tsG.ElementAt(i).parametros.Count == aux.parametros.Count) {
                                                    for (int j = 0; j < tsG.ElementAt(i).parametros.Count; j++){
                                                        if (tsG.ElementAt(i).parametros.ElementAt(j).tipo != aux.parametros.ElementAt(j).tipo) { 
                                                            tipPar = true;
                                                            break;
                                                        }
                                                    }
                                                    if (tsG.ElementAt(i).tipo != "VOID"){
                                                        if (tipPar != true){ //AÑADIR AMBITO DE PROCEDIMIENTO                                                                                                               
                                                            principal.ambito = tsG.ElementAt(i).nombrre;
                                                            principal.padre = tsG.ElementAt(i).ambito;
                                                            principal.tipo = tsG.ElementAt(i).tipo;
                                                            pila.Add(principal);
                                                            actual++;
                                                            //AGREGAR PARAMETROS A AMBITO ACTUAL
                                                            for (int k = 0; k < tsG.ElementAt(i).parametros.Count; k++){
                                                                Tupla auxM = new Tupla();
                                                                auxM.tipo = tsG.ElementAt(i).parametros.ElementAt(k).tipo;
                                                                auxM.nombrre = tsG.ElementAt(i).parametros.ElementAt(k).nombrre;
                                                                auxM.valor = aux.parametros.ElementAt(k).valor;
                                                                auxM.rol = "var";
                                                                auxM.ambito = tsG.ElementAt(i).nombrre;
                                                                pila.ElementAt(actual).tabla.Add(auxM);                                                                
                                                            }
                                                            //iniciar recorrido de PROCEDIMIENTO
                                                            pRec(tsG.ElementAt(i).root);
                                                            //TERMINA
                                                            if (pila.ElementAt(actual).retorno != null){
                                                                nuevo.tipo = pila.ElementAt(actual).tipo;
                                                                nuevo.valor = pila.ElementAt(actual).retorno;
                                                            }else {
                                                                //ERROR
                                                                addError("Fallo en obtener un valor de retorno del procedimiento", root.Span.Location.Line, root.Span.Location.Column);
                                                                nuevo.error = true;
                                                                nuevo.tipo = "INT";
                                                                nuevo.valor = 0;
                                                            }
                                                            pila.RemoveAt(actual);
                                                            actual--;
                                                            llamada = false;
                                                        }else{
                                                            //ERROR
                                                            addError("Se realizo la llamada a un procedimiento pero el tipo de los valores enviados como parametros no coinciden", root.Span.Location.Line, root.Span.Location.Column);
                                                            nuevo.error = true;
                                                            nuevo.tipo = "INT";
                                                            nuevo.valor = 0;
                                                        }
                                                    }else {
                                                        //ERROR
                                                        addError("Una expresion debe operar valores. Se llamo a un procedimiento que no retorna ningun valor", root.Span.Location.Line, root.Span.Location.Column);
                                                        nuevo.error = true;
                                                        nuevo.tipo = "INT";
                                                        nuevo.valor = 0;
                                                    }                                                    
                                                    flag = true;
                                                    break;
                                                }
                                            }else if (tsG.ElementAt(i).rol == "import") {
                                                imp.Add(tsG.ElementAt(i).nombrre);
                                            }
                                        }
                                    }
                                    //SI NO EXISTE EN EL ORIGEN
                                    if (flag == false) {

                                        bool salir = true;
                                        int salAux = 0;
                                        while (salir) {
                                            //RECORRER LOS IMPORT
                                            for (int i  = 0; i  < imp.Count; i ++){
                                                for (int j = 0; j < tsG.Count; j++){
                                                    if (tsG.ElementAt(j).ambito == imp.ElementAt(i)) { //AMBITO DE IMPORT
                                                        if (tsG.ElementAt(j).rol == "proc") { //ES PROCEDIMIENTO
                                                            if (tsG.ElementAt(j).nombrre == aux.nombrre && tsG.ElementAt(j).parametros.Count == aux.parametros.Count) { //MISMO NOMBRE Y NUM DE PARAMETROS
                                                                bool exx = false;
                                                                for (int k = 0; k < tsG.ElementAt(i).parametros.Count; k++){
                                                                    if (tsG.ElementAt(j).parametros.ElementAt(k).tipo != aux.parametros.ElementAt(k).tipo){
                                                                        exx = true;
                                                                        break;
                                                                    }
                                                                }
                                                                if (tsG.ElementAt(j).tipo != "VOID"){//ES UNA FUNCION
                                                                    if (exx != true){//TODO COINCIDE
                                                                        principal.ambito = tsG.ElementAt(j).nombrre;
                                                                        principal.padre = tsG.ElementAt(j).ambito;
                                                                        principal.tipo = tsG.ElementAt(j).tipo;
                                                                        pila.Add(principal);
                                                                        actual++;
                                                                        //AGREGAR PARAMETROS A AMBITO ACTUAL
                                                                        for (int k = 0; k < tsG.ElementAt(j).parametros.Count; k++)
                                                                        {
                                                                            Tupla auxM = new Tupla();
                                                                            auxM.tipo = tsG.ElementAt(j).parametros.ElementAt(k).tipo;
                                                                            auxM.nombrre = tsG.ElementAt(j).parametros.ElementAt(k).nombrre;
                                                                            auxM.valor = aux.parametros.ElementAt(k).valor;
                                                                            auxM.rol = "var";
                                                                            auxM.ambito = tsG.ElementAt(j).nombrre;
                                                                            pila.ElementAt(actual).tabla.Add(auxM);
                                                                        }
                                                                        //iniciar recorrido de PROCEDIMIENTO
                                                                        pRec(tsG.ElementAt(j).root);
                                                                        //TERMINA
                                                                        if (pila.ElementAt(actual).retorno != null){
                                                                            nuevo.tipo = pila.ElementAt(actual).tipo;
                                                                            nuevo.valor = pila.ElementAt(actual).retorno;
                                                                        }else{
                                                                            //ERROR
                                                                            addError("Fallo en obtener un valor de retorno del procedimiento", root.Span.Location.Line, root.Span.Location.Column);
                                                                            nuevo.error = true;
                                                                            nuevo.tipo = "INT";
                                                                            nuevo.valor = 0;
                                                                        }
                                                                        pila.RemoveAt(actual);
                                                                        actual--;
                                                                        llamada = false;
                                                                    }else {
                                                                        //ERROR
                                                                        addError("Se realizo la llamada a un procedimiento pero el tipo de los valores enviados como parametros no coinciden", root.Span.Location.Line, root.Span.Location.Column);
                                                                        nuevo.error = true;
                                                                        nuevo.tipo = "INT";
                                                                        nuevo.valor = 0;
                                                                    }
                                                                }else {
                                                                    //ERROR
                                                                    addError("Una expresion debe operar valores. Se llamo a un procedimiento que no retorna ningun valor", root.Span.Location.Line, root.Span.Location.Column);
                                                                    nuevo.error = true;
                                                                    nuevo.tipo = "INT";
                                                                    nuevo.valor = 0;
                                                                }
                                                                flag = true;
                                                                salir = false;
                                                                break;
                                                            }
                                                        }
                                                    }
                                                }
                                            }
                                            //AGREGAR MAS IMPORT
                                            if (salir == true) {
                                                List<String> tempImp = new List<string>();
                                                for (int i = 0; i < imp.Count; i++){
                                                    for (int j = 0; j < tsG.Count; j++){
                                                        if (tsG.ElementAt(j).ambito == imp.ElementAt(i) && tsG.ElementAt(j).rol=="import") {
                                                            tempImp.Add(tsG.ElementAt(j).nombrre);
                                                        }
                                                    }
                                                }
                                                imp.Clear();
                                                imp.AddRange(tempImp);
                                            }
                                            if (imp.Count==0 || salAux > 9)
                                                salir = false;
                                            salAux++;
                                        }
                                        if (flag != true) {
                                            //ERROR
                                            addError("No existe ningun procedimiento que coincida con los datos proporcionados en la llamada", root.Span.Location.Line, root.Span.Location.Column);
                                            nuevo.error = true;
                                            nuevo.tipo = "INT";
                                            nuevo.valor = 0;
                                        }
                                    }
                                }
                            }else {
                                nuevo.error = true;
                                nuevo.tipo = "INT";
                                nuevo.valor = 0;
                            }
                            return nuevo;
                        }                        
                    }else if(root.ChildNodes.Count==2){
                        /*
                            | tMenos + ART
                         */
                        aux = exp(root.ChildNodes.ElementAt(1));                        
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
                                addError("A un valor de tipo STRING no se le puede aplicar signo negativo",root.Span.Location.Line,root.Span.Location.Column);                                
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
                           return exp(root.ChildNodes.ElementAt(1));                            
                        } else {
                            /*
                                | ART + tMas + ART
                                | ART + tMenos + ART
                                | ART + tPor + ART
                                | ART + tDiv + ART                            
                                | ART + tPot + ART
                            */
                            aux = exp(root.ChildNodes.ElementAt(0));
                            aux2 = exp(root.ChildNodes.ElementAt(2));
                            valor = root.ChildNodes.ElementAt(1).ToString().Split(' ');
                            return aritmetico(aux, valor[0], aux2, root.Span.Location.Line, root.Span.Location.Column);                            
                        }                        
                    }
            }            
            return null;
        }

        private Tupla chapus(ParseTreeNode root) {
			
            Tupla nuevo = new Tupla();//nuevo creado para devolver valores
            String temp = ""; //temporal para nombre de nodo
            String[] valor;   //temporal split
            Tupla aux;//auxiliar retorno
            Tupla aux2;//auxiliar retorno                        
            bool flag = false;// hay o no error
            bool subflag = false;//validar ambito tabla simbolos
            Ambito acc;

            switch (root.Term.Name.ToString()) { 
                case "REL_M":
                    if (root.ChildNodes.Count() == 1) {
                        /*
                            | ART_M
                         */
                        return chapus(root.ChildNodes.ElementAt(0));
                    } else if (root.ChildNodes.Count() == 2) {
                        /*
                            | tNot + REL_M
                         */
                        aux = chapus(root.ChildNodes.ElementAt(1));
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
                                    addError("No se puede negar un valor de tipo INT excepto 0 y 1", root.Span.Location.Line, root.Span.Location.Column);
                                    aux.error = true;
                                }
                                aux.valor = false;
                            }                            
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
                                        addError("No se puede negar un valor de tipo DOUBLE excepto 0.0 y 1.0.", root.Span.Location.Line, root.Span.Location.Column);
                                        aux.error = true;
                                    }
                                    aux.valor = false;
                                }
                            }else{
                                //ERROR
                                if (aux.error != true) {                                                                        
                                    addError("No se puede negar un valor de tipo DOUBLE excepto 0.0 y 1.0.", root.Span.Location.Line, root.Span.Location.Column);
                                    aux.error = true;
                                }
                                aux.valor = false;
                            }                            
                        }else if (aux.tipo == "CHAR"){
                            if ((Int32)Convert.ToChar(aux.valor) == 0)
                                aux.valor = true;
                            else if ((Int32)Convert.ToChar(aux.valor) == 1)
                                aux.valor = false;
                            else{
                                //ERROR
                                if (aux.error != true){                                                                        
                                    addError("No se puede negar un valor de tipo CHAR excepto el equivalente ascii de 0 y 1.", root.Span.Location.Line, root.Span.Location.Column);
                                    aux.error = true;
                                }
                                aux.valor = false;
                            }                            
                        }else{
                            //ERROR
                            if (aux.error != true){                                                                
                                addError("No se puede negar elementos de tipo STRING", root.Span.Location.Line, root.Span.Location.Column);
                                aux.error = true;
                            }                            
                            aux.valor = false;
                        }
                        aux.tipo = "BOOL";
                        return aux;
                    }else{
                        /*
                                REL_M + tOr + REL_M
                				| REL_M + tAnd + REL_M   
                				| REL_M + REL + REL_M                                                                                    
                         */                        
            				temp = root.ChildNodes.ElementAt(1).ToString().Trim();
                        if (temp.Equals("REL")){
                            aux = chapus(root.ChildNodes.ElementAt(0));
                            String dato = Convert.ToString(exp(root.ChildNodes.ElementAt(1)).valor);
                            aux2 = chapus(root.ChildNodes.ElementAt(2));
                            return logicoRel(aux, dato, aux2, root.Span.Location.Line, root.Span.Location.Column);
                        }else{
                            aux = chapus(root.ChildNodes.ElementAt(0));
                            aux2 = chapus(root.ChildNodes.ElementAt(2));
                            valor = root.ChildNodes.ElementAt(1).ToString().Split(' ');
                            return logicoRel(aux, valor[0], aux2, root.Span.Location.Line, root.Span.Location.Column);
                        }                                                    
                    }                                    
                case "ART_M":                    
                    if (root.ChildNodes.Count() == 1) {
                        /*                                            
                            | tnumero                            
                            | tId
                            | tCadena
                            | tCaracter
                            | tVbool
                         */
                        valor = root.ChildNodes.ElementAt(0).ToString().Split(' ');                        
                        if (valor.Count() >= 2) {                                                        
                            switch (valor[valor.Count()-1]) {
                                case "(tNumero)":
                                    if (valor[0].Contains(".")) {
                                        nuevo.tipo = "DOUBLE";
                                        nuevo.valor = Convert.ToDouble(valor[0]);
                                    } else {
                                        nuevo.tipo = "INT";
                                        nuevo.valor = Convert.ToInt32(valor[0]);
                                    }                                    
                                    break;
                                case "(tId)":
                                    //EN AMBITO GENERAL, PILA
                                    if (pila.ElementAt(actual).hay == false){
                                        for (int i = 0; i < pila.ElementAt(actual).tabla.Count; i++){
                                            if (pila.ElementAt(actual).tabla.ElementAt(i).nombrre == valor[0] && pila.ElementAt(actual).tabla.ElementAt(i).rol == "var"){
                                                if (pila.ElementAt(actual).tabla.ElementAt(i).dimension.Count == 0){//VARIABLE
                                                    nuevo.valor = pila.ElementAt(actual).tabla.ElementAt(i).valor;
                                                    nuevo.tipo = pila.ElementAt(actual).tabla.ElementAt(i).tipo;
                                                }else { //ES UN ARREGLO
                                                    //ERROR
                                                    addError("LOCAL,Valor. Se intento acceder al valor de un arreglo, no se definio dimensiones", root.Span.Location.Line, root.Span.Location.Column);
                                                }                                                                                                
                                                flag = true;
                                                break;                                                
                                            }
                                        }                                                                                    
                                    }
                                    else{ //EN SUB AMBITO 
                                        acc = pila.ElementAt(actual);
                                        for (int i = 0; i < acc.pila.ElementAt(acc.actual).tabla.Count; i++){
                                            if (acc.pila.ElementAt(acc.actual).tabla.ElementAt(i).nombrre == valor[0] && acc.pila.ElementAt(acc.actual).tabla.ElementAt(i).rol == "var"){
                                                if (acc.pila.ElementAt(acc.actual).tabla.ElementAt(i).dimension.Count == 0){//VARIABLE
                                                    nuevo.valor = acc.pila.ElementAt(acc.actual).tabla.ElementAt(i).valor;
                                                    nuevo.tipo = acc.pila.ElementAt(acc.actual).tabla.ElementAt(i).tipo;
                                                }else { //ES UN ARREGLO
                                                    //ERROR
                                                    addError("LOCAL,Valor. Se intento acceder al valor de un arreglo, no se definio dimensiones", root.Span.Location.Line, root.Span.Location.Column);
                                                }                                                
                                                flag = true;
                                                break;                                                
                                            }
                                        }                                        
                                    }
                                    //TABLA DE SIMBOLOS
                                    if (flag == false){
                                        for (int i = 0; i < tsG.Count; i++){
                                            if (tsG.ElementAt(i).nombrre == valor[0] && tsG.ElementAt(i).rol == "var"){
                                                if (tsG.ElementAt(i).ambito == pila.ElementAt(actual).padre){
                                                    if (tsG.ElementAt(i).dimension.Count == 0){
                                                        nuevo.valor = tsG.ElementAt(i).valor;
                                                        nuevo.tipo = tsG.ElementAt(i).tipo;
                                                    }else{
                                                        //ERROR
                                                        addError("GLOBAL,Valor. Se intento acceder al valor de un arreglo, no se definio dimensiones", root.Span.Location.Line, root.Span.Location.Column);
                                                    }
                                                    flag = true;
                                                    break;
                                                }else{
                                                    //ERROR
                                                    //addError("Se intento acceder a una variable que no pertence al origen(clase)", root.Span.Location.Line, root.Span.Location.Column);
                                                    subflag = true;
                                                }
                                            }
                                        }
                                        if (subflag == true && flag == false){
                                            //ERROR
                                            addError("Se intento acceder a una variable que no pertence al origen(clase)", root.Span.Location.Line, root.Span.Location.Column);
                                        }else if (flag == false){
                                            //ERROR
                                            addError("No existe ningun elemento con el nombre especificado, en ningun ambito", root.Span.Location.Line, root.Span.Location.Column);
                                            nuevo.tipo = "INT";
                                            nuevo.valor = 0;
                                            nuevo.error = true;
                                        }
                                    }                                                                                                                                              
                                    break;
                                case "(tCadena)":
                                    String txt = "";
                                    for (int i = 0; i < valor.Count()-1; i++) {
                                        if (i != valor.Count() - 2)
                                            txt += valor[i] + " ";
                                        else
                                            txt += valor[i];
                                    }
                                    nuevo.tipo = "STRING";
                                    nuevo.valor = txt;
                                    break;
                                case "(tCaracter)":
                                    Char[] arr = valor[0].ToCharArray();                                                                        
                                    Char caracter = arr[0];
                                    nuevo.tipo = "CHAR";
                                    nuevo.valor = caracter;
                                    break;
                                case "(tVbool)":
                                    if (valor[0]=="false")                                                                            
                                        nuevo.valor = false;
                                    else                                        
                                        nuevo.valor = true;                                    
                                    nuevo.tipo = "BOOL";
                                    break;                                
                            }
                            return nuevo;                          
                        } else {
                            /*
                                | ACC_MTRX  <---------------------------------------------------------------------
                             */
                            // si es acceso a matriz o llamada a metodo??
                            return pRec(root.ChildNodes.ElementAt(0));    
                        }                        
                    }else if(root.ChildNodes.Count==2){
                        /*
                            | tMenos + ART_M  
                         */
                        aux = chapus(root.ChildNodes.ElementAt(1));                        
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
                                addError("A un valor de tipo STRING no se le puede aplicar signo negativo",root.Span.Location.Line,root.Span.Location.Column);                                
                                aux.valor = 0;
                                aux.tipo = "INT";
                                aux.error = true;
                                break;
                        }                                                    
                        return aux;                    
                    }else{                                                
                            /*
                                ART_M + tMas + ART_M
				                | ART_M + tMenos + ART_M
				                | ART_M + tPor + ART_M
				                | ART_M + tDiv + ART_M                            
                                | ART_M + tPot + ART_M
                            */
                            aux = chapus(root.ChildNodes.ElementAt(0));
                            aux2 = chapus(root.ChildNodes.ElementAt(2));
                            valor = root.ChildNodes.ElementAt(1).ToString().Split(' ');
                            return aritmetico(aux, valor[0], aux2, root.Span.Location.Line, root.Span.Location.Column);                                                    
                    }
            }            
            return null;            
        }

        private Tupla valDefault(String tipo) {
            Tupla aux = new Tupla();
            switch (tipo) {
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
            return aux;
        }

        private void addError(String tError, int fila, int columna){
    		List<String> nwnError = new List<String>();  //error
    		nwnError.Add("semantico");
            nwnError.Add(tError);
            nwnError.Add(fila.ToString());
            nwnError.Add(columna.ToString());
            nwnError.Add(pila.ElementAt(actual).padre + ", " + pila.ElementAt(actual).ambito);
            erroreSMT.Add(nwnError);
        }

        private Tupla aritmetico(Tupla val1, String op, Tupla val2, int fila, int columna)
        {
            Tupla nuevo = new Tupla();
            List<String> nwnError = new List<String>();  //error
            string detError = "";
            switch (op)
            {
                case "+":
                    switch (val1.tipo)
                    {
                        case "INT":
                            switch (val2.tipo)
                            {
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
                                    if (auxf == true)
                                        nuevo.valor = Convert.ToInt32(val1.valor) + 1;
                                    else
                                        nuevo.valor = Convert.ToInt32(val1.valor) + 0;
                                    nuevo.tipo = "INT";
                                    break;
                            }
                            break;
                        case "DOUBLE":
                            switch (val2.tipo)
                            {
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
                                    if (auxf == true)
                                        nuevo.valor = Convert.ToDouble(val1.valor) + 1;
                                    else
                                        nuevo.valor = Convert.ToDouble(val1.valor) + 0;
                                    nuevo.tipo = "DOUBLE";
                                    break;
                            }
                            break;
                        case "CHAR":
                            switch (val2.tipo)
                            {
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
                            switch (val2.tipo)
                            {
                                case "INT":
                                    bool auxf = Convert.ToBoolean(val1.valor);
                                    if (auxf == true)
                                        nuevo.valor = 1 + Convert.ToInt32(val2.valor);
                                    else
                                        nuevo.valor = 0 + Convert.ToInt32(val2.valor);
                                    nuevo.tipo = "INT";
                                    break;
                                case "DOUBLE":
                                    bool auxf1 = Convert.ToBoolean(val1.valor);
                                    if (auxf1 == true)
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
                                    else
                                    {
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
                    switch (val1.tipo)
                    {
                        case "INT":
                            switch (val2.tipo)
                            {
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
                                    if (auxf == true)
                                        nuevo.valor = Convert.ToInt32(val1.valor) * 1;
                                    else
                                        nuevo.valor = Convert.ToInt32(val1.valor) * 0;
                                    nuevo.tipo = "INT";
                                    break;
                            }
                            break;
                        case "DOUBLE":
                            switch (val2.tipo)
                            {
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
                                    if (auxf == true)
                                        nuevo.valor = Convert.ToDouble(val1.valor) * 1;
                                    else
                                        nuevo.valor = Convert.ToDouble(val1.valor) * 0;
                                    nuevo.tipo = "DOUBLE";
                                    break;
                            }
                            break;
                        case "CHAR":
                            switch (val2.tipo)
                            {
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
                            switch (val2.tipo)
                            {
                                case "INT":
                                    if (val1.error != true && val2.error != true)
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
                            switch (val2.tipo)
                            {
                                case "INT":
                                    bool auxf = Convert.ToBoolean(val1.valor);
                                    if (auxf == true)
                                        nuevo.valor = 1 * Convert.ToInt32(val2.valor);
                                    else
                                        nuevo.valor = 0 * Convert.ToInt32(val2.valor);
                                    nuevo.tipo = "INT";
                                    break;
                                case "DOUBLE":
                                    bool auxf1 = Convert.ToBoolean(val1.valor);
                                    if (auxf1 == true)
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
                    switch (val1.tipo)
                    {
                        case "INT":
                            switch (val2.tipo)
                            {
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
                                    if (auxf == true)
                                        nuevo.valor = Convert.ToInt32(val1.valor) - 1;
                                    else
                                        nuevo.valor = Convert.ToInt32(val1.valor) - 0;
                                    nuevo.tipo = "INT";
                                    break;
                            }
                            break;
                        case "DOUBLE":
                            switch (val2.tipo)
                            {
                                case "INT":
                                    nuevo.valor = Convert.ToDouble(val1.valor) - Convert.ToInt32(val2.valor);
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
                                    if (auxf == true)
                                        nuevo.valor = Convert.ToDouble(val1.valor) - 1;
                                    else
                                        nuevo.valor = Convert.ToDouble(val1.valor) - 0;
                                    nuevo.tipo = "DOUBLE";
                                    break;
                            }
                            break;
                        case "CHAR":
                            switch (val2.tipo)
                            {
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
                            switch (val2.tipo)
                            {
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
                            switch (val2.tipo)
                            {
                                case "INT":
                                    bool auxf = Convert.ToBoolean(val1.valor);
                                    if (auxf == true)
                                        nuevo.valor = 1 - Convert.ToInt32(val2.valor);
                                    else
                                        nuevo.valor = 0 - Convert.ToInt32(val2.valor);
                                    nuevo.tipo = "INT";
                                    break;
                                case "DOUBLE":
                                    bool auxf1 = Convert.ToBoolean(val1.valor);
                                    if (auxf1 == true)
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
                    switch (val1.tipo)
                    {
                        case "INT":
                            switch (val2.tipo)
                            {
                                case "INT":
                                    if (Convert.ToInt32(val2.valor) != 0)
                                    {
                                        nuevo.valor = Convert.ToInt32(val1.valor) / Convert.ToInt32(val2.valor);
                                        nuevo.tipo = "DOUBLE";
                                    }
                                    else
                                    {
                                        //ERROR
                                        detError = "No es posible dividir un numero entre 0";
                                        nuevo.valor = 0;
                                        nuevo.tipo = "INT";
                                    }
                                    break;
                                case "DOUBLE":
                                    if (Convert.ToDouble(val2.valor) != 0.0)
                                    {
                                        nuevo.valor = Convert.ToInt32(val1.valor) / Convert.ToDouble(val2.valor);
                                        nuevo.tipo = "DOUBLE";
                                    }
                                    else
                                    {
                                        //ERROR
                                        detError = "No es posible dividir un numero entre 0";
                                        nuevo.valor = 0;
                                        nuevo.tipo = "INT";
                                    }
                                    break;
                                case "CHAR":
                                    if ((Int32)Convert.ToChar(val2.valor) != 0)
                                    {
                                        nuevo.valor = Convert.ToInt32(val1.valor) / (Int32)Convert.ToChar(val2.valor);
                                        nuevo.tipo = "DOUBLE";
                                    }
                                    else
                                    {
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
                                    if (auxf == true)
                                    {
                                        nuevo.valor = Convert.ToInt32(val1.valor) / 1;
                                        nuevo.tipo = "DOUBLE";
                                    }
                                    else
                                    {
                                        //ERROR
                                        detError = "No es posible dividir un numero entre 0";
                                        nuevo.valor = 0;
                                        nuevo.tipo = "INT";
                                    }
                                    break;
                            }
                            break;
                        case "DOUBLE":
                            switch (val2.tipo)
                            {
                                case "INT":
                                    if (Convert.ToInt32(val2.valor) != 0)
                                    {
                                        nuevo.valor = Convert.ToDouble(val1.valor) / Convert.ToInt32(val2.valor);
                                        nuevo.tipo = "DOUBLE";
                                    }
                                    else
                                    {
                                        //ERROR
                                        detError = "No es posible dividir un numero entre 0";
                                        nuevo.valor = 0.0;
                                        nuevo.tipo = "DOUBLE";
                                    }
                                    break;
                                case "DOUBLE":
                                    if (Convert.ToDouble(val2.valor) != 0.0)
                                    {
                                        nuevo.valor = Convert.ToDouble(val1.valor) / Convert.ToDouble(val2.valor);
                                        nuevo.tipo = "DOUBLE";
                                    }
                                    else
                                    {
                                        //ERROR
                                        detError = "No es posible dividir un numero entre 0";
                                        nuevo.valor = 0.0;
                                        nuevo.tipo = "DOUBLE";
                                    }
                                    break;
                                case "CHAR":
                                    if ((Int32)Convert.ToChar(val2.valor) != 0)
                                    {
                                        nuevo.valor = Convert.ToDouble(val1.valor) / (Int32)Convert.ToChar(val2.valor);
                                        nuevo.tipo = "DOUBLE";
                                    }
                                    else
                                    {
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
                                    if (auxf == true)
                                    {
                                        nuevo.valor = Convert.ToDouble(val1.valor) / 1;
                                        nuevo.tipo = "DOUBLE";
                                    }
                                    else
                                    {
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
                                    if (Convert.ToInt32(val2.valor) != 0)
                                    {
                                        nuevo.valor = (Int32)Convert.ToChar(val1.valor) / Convert.ToInt32(val2.valor);
                                        nuevo.tipo = "DOUBLE";
                                    }
                                    else
                                    {
                                        //ERROR
                                        detError = "No es posible dividir un numero entre 0";
                                        nuevo.valor = ' ';
                                        nuevo.tipo = "CHAR";
                                    }
                                    break;
                                case "DOUBLE":
                                    if (Convert.ToDouble(val2.valor) != 0.0)
                                    {
                                        nuevo.valor = (Int32)Convert.ToChar(val1.valor) / Convert.ToDouble(val2.valor);
                                        nuevo.tipo = "DOUBLE";
                                    }
                                    else
                                    {
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
                            switch (val2.tipo)
                            {
                                case "INT":
                                    if (Convert.ToInt32(val2.valor) != 0)
                                    {
                                        bool auxf = Convert.ToBoolean(val1.valor);
                                        if (auxf == true)
                                            nuevo.valor = 1 / Convert.ToInt32(val2.valor);
                                        else
                                            nuevo.valor = 0 / Convert.ToInt32(val2.valor);
                                        nuevo.tipo = "DOUBLE";
                                    }
                                    else
                                    {
                                        //ERROR
                                        detError = "No es posible dividir un numero entre 0";
                                        nuevo.valor = false;
                                        nuevo.tipo = "BOOL";
                                    }
                                    break;
                                case "DOUBLE":
                                    if (Convert.ToInt32(val2.valor) != 0)
                                    {
                                        bool auxf1 = Convert.ToBoolean(val1.valor);
                                        if (auxf1 == true)
                                            nuevo.valor = 1 / Convert.ToDouble(val2.valor);
                                        else
                                            nuevo.valor = 0 / Convert.ToDouble(val2.valor);
                                        nuevo.tipo = "DOUBLE";
                                    }
                                    else
                                    {
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
                    switch (val1.tipo)
                    {
                        case "INT":
                            switch (val2.tipo)
                            {
                                case "INT":
                                    nuevo.valor = System.Math.Pow(Convert.ToInt32(val1.valor), Convert.ToInt32(val2.valor));
                                    nuevo.tipo = "INT";
                                    break;
                                case "DOUBLE":
                                    nuevo.valor = System.Math.Pow(Convert.ToInt32(val1.valor), Convert.ToDouble(val2.valor));
                                    nuevo.tipo = "DOUBLE";
                                    break;
                                case "CHAR":
                                    nuevo.valor = System.Math.Pow(Convert.ToInt32(val1.valor), (Int32)Convert.ToChar(val2.valor));
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
                            switch (val2.tipo)
                            {
                                case "INT":
                                    nuevo.valor = System.Math.Pow(Convert.ToDouble(val1.valor), Convert.ToInt32(val2.valor));
                                    nuevo.tipo = "DOUBLE";
                                    break;
                                case "DOUBLE":
                                    nuevo.valor = System.Math.Pow(Convert.ToDouble(val1.valor), Convert.ToDouble(val2.valor));
                                    nuevo.tipo = "DOUBLE";
                                    break;
                                case "CHAR":
                                    nuevo.valor = System.Math.Pow(Convert.ToDouble(val1.valor), (Int32)Convert.ToChar(val2.valor));
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
                                    if (auxf == true)
                                        nuevo.valor = System.Math.Pow(Convert.ToDouble(val1.valor), 1);
                                    else
                                        nuevo.valor = System.Math.Pow(Convert.ToDouble(val1.valor), 0);
                                    nuevo.tipo = "DOUBLE";
                                    break;
                            }
                            break;
                        case "CHAR":
                            switch (val2.tipo)
                            {
                                case "INT":
                                    nuevo.valor = System.Math.Pow((Int32)Convert.ToChar(val1.valor), Convert.ToInt32(val2.valor));
                                    nuevo.tipo = "INT";
                                    break;
                                case "DOUBLE":
                                    nuevo.valor = System.Math.Pow((Int32)Convert.ToChar(val1.valor), Convert.ToDouble(val2.valor));
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
                            switch (val2.tipo)
                            {
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
                            switch (val2.tipo)
                            {
                                case "INT":
                                    bool auxf = Convert.ToBoolean(val1.valor);
                                    if (auxf == true)
                                        nuevo.valor = System.Math.Pow(1, Convert.ToInt32(val2.valor));
                                    else
                                        nuevo.valor = System.Math.Pow(0, Convert.ToInt32(val2.valor));
                                    nuevo.tipo = "INT";
                                    break;
                                case "DOUBLE":
                                    bool auxf1 = Convert.ToBoolean(val1.valor);
                                    if (auxf1 == true)
                                        nuevo.valor = System.Math.Pow(1, Convert.ToDouble(val2.valor));
                                    else
                                        nuevo.valor = System.Math.Pow(0, Convert.ToDouble(val2.valor));
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
            if (detError != "")
            {
                if (val1.error != true && val2.error != true)
                {                    
                    addError(detError,fila,columna);
                    nuevo.error = true;
                }
            }
            if (val1.error == true || val2.error == true)
                nuevo.error = true;
            return nuevo;
        }

        private Tupla logicoRel(Tupla val1, String op, Tupla val2, int fila, int columna)
        {
            Tupla nuevo = new Tupla();
            int a = 0;
            int b = 0;
            bool a1 = false;
            bool a2 = false;
            List<String> nwnError = new List<String>();  //error
            string detError = ""; //detalle error
            switch (op)
            {
                case "&&":
                    switch (val1.tipo)
                    {
                        case "INT":
                            if (Convert.ToInt32(val1.valor) == 0 || Convert.ToInt32(val1.tipo) == 1)
                            {
                                switch (val2.tipo)
                                {
                                    case "INT":
                                        if (Convert.ToInt32(val2.valor) == 0 || Convert.ToInt32(val2.valor) == 1)
                                        {
                                            a1 = unoCero(Convert.ToInt32(val1.valor));
                                            a2 = unoCero(Convert.ToInt32(val2.valor));
                                            nuevo.valor = a1 && a2;
                                        }
                                        else
                                        {
                                            //ERROR
                                            detError = "&& no puede operar valores de tipo INT distintos de 0 y 1";
                                        }
                                        break;
                                    case "DOUBLE":
                                        if (Convert.ToDouble(val2.valor) % 1 == 0)
                                        {
                                            if (Convert.ToInt32(val2.valor) == 0 || Convert.ToInt32(val2.valor) == 1)
                                            {
                                                a1 = unoCero(Convert.ToInt32(val1.valor));
                                                a2 = unoCero(Convert.ToInt32(val2.valor));
                                                nuevo.valor = a1 && a2;
                                            }
                                            else
                                            {
                                                //ERROR
                                                detError = "&& no puede operar un valor tipo INT con uno DOUBLE distinto de 0.0 y 1.0";
                                            }
                                        }
                                        else
                                        {
                                            //ERROR
                                            detError = "&& no puede operar un valor tipo INT con uno DOUBLE distinto de 0.0 y 1.0";
                                        }
                                        break;
                                    case "CHAR":
                                        if ((Int32)Convert.ToChar(val2.valor) == 0 || (Int32)Convert.ToChar(val2.valor) == 1)
                                        {
                                            a1 = unoCero(Convert.ToInt32(val1.valor));
                                            a2 = unoCero((Int32)Convert.ToChar(val2.valor));
                                            nuevo.valor = a1 && a2;
                                        }
                                        else
                                        {
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
                            }
                            else
                            {
                                //ERRROR
                                detError = "&& no puede operar valores de tipo INT distintos de 0 y 1";
                            }
                            break;
                        case "DOUBLE":
                            if (Convert.ToDouble(val1.valor) % 1 == 0)
                            {
                                if (Convert.ToInt32(val1.valor) == 0 || Convert.ToInt32(val1.valor) == 1)
                                {
                                    switch (val2.tipo)
                                    {
                                        case "INT":
                                            if (Convert.ToInt32(val2.valor) == 0 || Convert.ToInt32(val2.valor) == 1)
                                            {
                                                a1 = unoCero(Convert.ToInt32(val1.valor));
                                                a2 = unoCero(Convert.ToInt32(val2.valor));
                                                nuevo.valor = a1 && a2;
                                            }
                                            else
                                            {
                                                //ERROR
                                                detError = "&& no puede operar un valor tipo DOUBLE con uno INT distinto de 0 y 1";
                                            }
                                            break;
                                        case "DOUBLE":
                                            if (Convert.ToDouble(val2.valor) % 1 == 0)
                                            {
                                                if (Convert.ToInt32(val2.valor) == 0 || Convert.ToInt32(val2.valor) == 1)
                                                {
                                                    a1 = unoCero(Convert.ToInt32(val1.valor));
                                                    a2 = unoCero(Convert.ToInt32(val2.valor));
                                                    nuevo.valor = a1 && a2;
                                                }
                                                else
                                                {
                                                    //ERROR
                                                    detError = "&& no puede operar valores de tipo DOUBLE distintos de 0.0 y 1.0";
                                                }
                                            }
                                            else
                                            {
                                                //ERROR
                                                detError = "&& no puede operar valores de tipo DOUBLE distintos de 0.0 y 1.0";
                                            }
                                            break;
                                        case "CHAR":
                                            if ((Int32)Convert.ToChar(val2.valor) == 0 || (Int32)Convert.ToChar(val2.valor) == 1)
                                            {
                                                a1 = unoCero(Convert.ToInt32(val1.valor));
                                                a2 = unoCero((Int32)Convert.ToChar(val2.valor));
                                                nuevo.valor = a1 && a2;
                                            }
                                            else
                                            {
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
                                }
                                else
                                {
                                    //ERROR
                                    detError = "&& no puede operar valores de tipo DOUBLE distintos de 0.0 y 1.0";
                                }
                            }
                            else
                            {
                                //ERROR
                                detError = "&& no puede operar valores de tipo DOUBLE distintos de 0.0 y 1.0";
                            }
                            break;
                        case "CHAR":
                            if ((Int32)Convert.ToChar(val2.valor) == 0 || (Int32)Convert.ToChar(val2.valor) == 1)
                            {
                                switch (val2.tipo)
                                {
                                    case "INT":
                                        if (Convert.ToInt32(val2.valor) == 0 || Convert.ToInt32(val2.valor) == 1)
                                        {
                                            a1 = unoCero((Int32)Convert.ToChar(val1.valor));
                                            a2 = unoCero(Convert.ToInt32(val2.valor));
                                            nuevo.valor = a1 && a2;
                                        }
                                        else
                                        {
                                            //ERRROR
                                            detError = "&& no puede operar un valor tipo CHAR con uno INT distinto de 0 y 1";
                                        }
                                        break;
                                    case "DOUBLE":
                                        if (Convert.ToDouble(val2.valor) % 1 == 0)
                                        {
                                            if (Convert.ToInt32(val2.valor) == 0 || Convert.ToInt32(val2.valor) == 1)
                                            {
                                                a1 = unoCero((Int32)Convert.ToChar(val1.valor));
                                                a2 = unoCero(Convert.ToInt32(val2.valor));
                                                nuevo.valor = a1 && a2;
                                            }
                                            else
                                            {
                                                //ERROR
                                                detError = "&& no puede operar un valor tipo CHAR con uno DOUBLE distinto de 0.0 y 1.0";
                                            }
                                        }
                                        else
                                        {
                                            //ERROR
                                            detError = "&& no puede operar un valor tipo CHAR con uno DOUBLE distinto de 0.0 y 1.0";
                                        }
                                        break;
                                    case "CHAR":
                                        if ((Int32)Convert.ToChar(val2.valor) == 0 || (Int32)Convert.ToChar(val2.valor) == 1)
                                        {
                                            a1 = unoCero((Int32)Convert.ToChar(val1.valor));
                                            a2 = unoCero((Int32)Convert.ToChar(val2.valor));
                                            nuevo.valor = a1 && a2;
                                        }
                                        else
                                        {
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
                            }
                            else
                            {
                                //ERROR
                                detError = "&& no puede operar valores tipo CHAR excepto si su equivalente ascii es 0 o 1";
                            }
                            break;
                        case "STRING":
                            //ERRROR
                            detError = "&& no puede operar valores tipo STRING con ningun otro tipo";
                            break;
                        case "BOOL":
                            switch (val2.tipo)
                            {
                                case "INT":
                                    if (Convert.ToInt32(val2.valor) == 0 || Convert.ToInt32(val2.valor) == 1)
                                    {
                                        a2 = unoCero(Convert.ToInt32(val2.valor));
                                        nuevo.valor = Convert.ToBoolean(val1.valor) && a2;
                                    }
                                    else
                                    {
                                        //ERROR
                                        detError = "&& no puede operar un valor tipo BOOL con uno INT distinto de 0 y 1";
                                    }
                                    break;
                                case "DOUBLE":
                                    if (Convert.ToDouble(val2.valor) % 1 == 0)
                                    {
                                        if (Convert.ToInt32(val2.valor) == 0 || Convert.ToInt32(val2.valor) == 1)
                                        {
                                            a2 = unoCero(Convert.ToInt32(val2.valor));
                                            nuevo.valor = Convert.ToBoolean(val1.valor) && a2;
                                        }
                                        else
                                        {
                                            //ERROR
                                            detError = "&& no puede operar un valor tipo BOOL con uno DOUBLE distinto de 0.0 y 1.0";
                                        }
                                    }
                                    else
                                    {
                                        //ERROR
                                        detError = "&& no puede operar un valor tipo BOOL con uno DOUBLE distinto de 0.0 y 1.0";
                                    }
                                    break;
                                case "CHAR":
                                    if ((Int32)Convert.ToChar(val2.valor) == 0 || (Int32)Convert.ToChar(val2.valor) == 1)
                                    {
                                        a2 = unoCero((Int32)Convert.ToChar(val2.valor));
                                        nuevo.valor = Convert.ToBoolean(val1.valor) && a2;
                                    }
                                    else
                                    {
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
                    switch (val1.tipo)
                    {
                        case "INT":
                            if (Convert.ToInt32(val1.valor) == 0 || Convert.ToInt32(val1.tipo) == 1)
                            {
                                switch (val2.tipo)
                                {
                                    case "INT":
                                        if (Convert.ToInt32(val2.valor) == 0 || Convert.ToInt32(val2.valor) == 1)
                                        {
                                            a1 = unoCero(Convert.ToInt32(val1.valor));
                                            a2 = unoCero(Convert.ToInt32(val2.valor));
                                            nuevo.valor = a1 || a2;
                                        }
                                        else
                                        {
                                            //ERROR
                                            detError = "|| no puede operar valores de tipo INT distintos de 0 y 1";
                                        }
                                        break;
                                    case "DOUBLE":
                                        if (Convert.ToDouble(val2.valor) % 1 == 0)
                                        {
                                            if (Convert.ToInt32(val2.valor) == 0 || Convert.ToInt32(val2.valor) == 1)
                                            {
                                                a1 = unoCero(Convert.ToInt32(val1.valor));
                                                a2 = unoCero(Convert.ToInt32(val2.valor));
                                                nuevo.valor = a1 || a2;
                                            }
                                            else
                                            {
                                                //ERROR
                                                detError = "|| no puede operar un valor tipo INT con uno DOUBLE distinto de 0.0 y 1.0";
                                            }
                                        }
                                        else
                                        {
                                            //ERROR
                                            detError = "|| no puede operar un valor tipo INT con uno DOUBLE distinto de 0.0 y 1.0";
                                        }
                                        break;
                                    case "CHAR":
                                        if ((Int32)Convert.ToChar(val2.valor) == 0 || (Int32)Convert.ToChar(val2.valor) == 1)
                                        {
                                            a1 = unoCero(Convert.ToInt32(val1.valor));
                                            a2 = unoCero((Int32)Convert.ToChar(val2.valor));
                                            nuevo.valor = a1 || a2;
                                        }
                                        else
                                        {
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
                            }
                            else
                            {
                                //ERRROR
                                detError = "|| no puede operar valores de tipo INT distintos de 0 y 1";
                            }
                            break;
                        case "DOUBLE":
                            if (Convert.ToDouble(val1.valor) % 1 == 0)
                            {
                                if (Convert.ToInt32(val1.valor) == 0 || Convert.ToInt32(val1.valor) == 1)
                                {
                                    switch (val2.tipo)
                                    {
                                        case "INT":
                                            if (Convert.ToInt32(val2.valor) == 0 || Convert.ToInt32(val2.valor) == 1)
                                            {
                                                a1 = unoCero(Convert.ToInt32(val1.valor));
                                                a2 = unoCero(Convert.ToInt32(val2.valor));
                                                nuevo.valor = a1 || a2;
                                            }
                                            else
                                            {
                                                //ERROR
                                                detError = "|| no puede operar un valor tipo DOUBLE con uno INT distinto de 0 y 1";
                                            }
                                            break;
                                        case "DOUBLE":
                                            if (Convert.ToDouble(val2.valor) % 1 == 0)
                                            {
                                                if (Convert.ToInt32(val2.valor) == 0 || Convert.ToInt32(val2.valor) == 1)
                                                {
                                                    a1 = unoCero(Convert.ToInt32(val1.valor));
                                                    a2 = unoCero(Convert.ToInt32(val2.valor));
                                                    nuevo.valor = a1 || a2;
                                                }
                                                else
                                                {
                                                    //ERROR
                                                    detError = "|| no puede operar valores de tipo DOUBLE distintos de 0.0 y 1.0";
                                                }
                                            }
                                            else
                                            {
                                                //ERROR
                                                detError = "|| no puede operar valores de tipo DOUBLE distintos de 0.0 y 1.0";
                                            }
                                            break;
                                        case "CHAR":
                                            if ((Int32)Convert.ToChar(val2.valor) == 0 || (Int32)Convert.ToChar(val2.valor) == 1)
                                            {
                                                a1 = unoCero(Convert.ToInt32(val1.valor));
                                                a2 = unoCero((Int32)Convert.ToChar(val2.valor));
                                                nuevo.valor = a1 || a2;
                                            }
                                            else
                                            {
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
                                }
                                else
                                {
                                    //ERROR
                                    detError = "|| no puede operar valores de tipo DOUBLE distintos de 0.0 y 1.0";
                                }
                            }
                            else
                            {
                                //ERROR
                                detError = "|| no puede operar valores de tipo DOUBLE distintos de 0.0 y 1.0";
                            }
                            break;
                        case "CHAR":
                            if ((Int32)Convert.ToChar(val2.valor) == 0 || (Int32)Convert.ToChar(val2.valor) == 1)
                            {
                                switch (val2.tipo)
                                {
                                    case "INT":
                                        if (Convert.ToInt32(val2.valor) == 0 || Convert.ToInt32(val2.valor) == 1)
                                        {
                                            a1 = unoCero((Int32)Convert.ToChar(val1.valor));
                                            a2 = unoCero(Convert.ToInt32(val2.valor));
                                            nuevo.valor = a1 || a2;
                                        }
                                        else
                                        {
                                            //ERRROR
                                            detError = "|| no puede operar un valor tipo CHAR con uno INT distinto de 0 y 1";
                                        }
                                        break;
                                    case "DOUBLE":
                                        if (Convert.ToDouble(val2.valor) % 1 == 0)
                                        {
                                            if (Convert.ToInt32(val2.valor) == 0 || Convert.ToInt32(val2.valor) == 1)
                                            {
                                                a1 = unoCero((Int32)Convert.ToChar(val1.valor));
                                                a2 = unoCero(Convert.ToInt32(val2.valor));
                                                nuevo.valor = a1 || a2;
                                            }
                                            else
                                            {
                                                //ERROR
                                                detError = "|| no puede operar un valor tipo CHAR con uno DOUBLE distinto de 0.0 y 1.0";
                                            }
                                        }
                                        else
                                        {
                                            //ERROR
                                            detError = "|| no puede operar un valor tipo CHAR con uno DOUBLE distinto de 0.0 y 1.0";
                                        }
                                        break;
                                    case "CHAR":
                                        if ((Int32)Convert.ToChar(val2.valor) == 0 || (Int32)Convert.ToChar(val2.valor) == 1)
                                        {
                                            a1 = unoCero((Int32)Convert.ToChar(val1.valor));
                                            a2 = unoCero((Int32)Convert.ToChar(val2.valor));
                                            nuevo.valor = a1 || a2;
                                        }
                                        else
                                        {
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
                            }
                            else
                            {
                                //ERROR
                                detError = "|| no puede operar valores tipo CHAR excepto si su equivalente ascii es 0 o 1";
                            }
                            break;
                        case "STRING":
                            //ERRROR
                            detError = "|| no puede operar valores tipo STRING con ningun otro tipo";
                            break;
                        case "BOOL":
                            switch (val2.tipo)
                            {
                                case "INT":
                                    if (Convert.ToInt32(val2.valor) == 0 || Convert.ToInt32(val2.valor) == 1)
                                    {
                                        a2 = unoCero(Convert.ToInt32(val2.valor));
                                        nuevo.valor = Convert.ToBoolean(val1.valor) || a2;
                                    }
                                    else
                                    {
                                        //ERROR
                                        detError = "|| no puede operar un valor tipo BOOL con uno INT distinto de 0 y 1";
                                    }
                                    break;
                                case "DOUBLE":
                                    if (Convert.ToDouble(val2.valor) % 1 == 0)
                                    {
                                        if (Convert.ToInt32(val2.valor) == 0 || Convert.ToInt32(val2.valor) == 1)
                                        {
                                            a2 = unoCero(Convert.ToInt32(val2.valor));
                                            nuevo.valor = Convert.ToBoolean(val1.valor) || a2;
                                        }
                                        else
                                        {
                                            //ERROR
                                            detError = "|| no puede operar un valor tipo BOOL con uno DOUBLE distinto de 0.0 y 1.0";
                                        }
                                    }
                                    else
                                    {
                                        //ERROR
                                        detError = "|| no puede operar un valor tipo BOOL con uno DOUBLE distinto de 0.0 y 1.0";
                                    }
                                    break;
                                case "CHAR":
                                    if ((Int32)Convert.ToChar(val2.valor) == 0 || (Int32)Convert.ToChar(val2.valor) == 1)
                                    {
                                        a2 = unoCero((Int32)Convert.ToChar(val2.valor));
                                        nuevo.valor = Convert.ToBoolean(val1.valor) || a2;
                                    }
                                    else
                                    {
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
                    switch (val1.tipo)
                    {
                        case "INT":
                            switch (val2.tipo)
                            {
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
                            switch (val2.tipo)
                            {
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
                            switch (val2.tipo)
                            {
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
                                        b += (Int32)Convert.ToChar(auxx[i]);
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
                            switch (val2.tipo)
                            {
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
                                        a += (Int32)Convert.ToChar(auxx[i]);
                                    if (a > b)
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
                            switch (val2.tipo)
                            {
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
                    switch (val1.tipo)
                    {
                        case "INT":
                            switch (val2.tipo)
                            {
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
                            switch (val2.tipo)
                            {
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
                            switch (val2.tipo)
                            {
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
                            switch (val2.tipo)
                            {
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
                            switch (val2.tipo)
                            {
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
                    switch (val1.tipo)
                    {
                        case "INT":
                            switch (val2.tipo)
                            {
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
                            switch (val2.tipo)
                            {
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
                            switch (val2.tipo)
                            {
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
                            switch (val2.tipo)
                            {
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
                            switch (val2.tipo)
                            {
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
                    switch (val1.tipo)
                    {
                        case "INT":
                            switch (val2.tipo)
                            {
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
                            switch (val2.tipo)
                            {
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
                            switch (val2.tipo)
                            {
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
                            switch (val2.tipo)
                            {
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
                            switch (val2.tipo)
                            {
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
                    switch (val1.tipo)
                    {
                        case "INT":
                            switch (val2.tipo)
                            {
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
                            switch (val2.tipo)
                            {
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
                            switch (val2.tipo)
                            {
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
                            switch (val2.tipo)
                            {
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
                            switch (val2.tipo)
                            {
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
                    switch (val1.tipo)
                    {
                        case "INT":
                            switch (val2.tipo)
                            {
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
                            switch (val2.tipo)
                            {
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
                            switch (val2.tipo)
                            {
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
                            switch (val2.tipo)
                            {
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
                            switch (val2.tipo)
                            {
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
            if (detError != "")
            {
                if (val1.error != true && val2.error != true)
                {
                    addError(detError,fila,columna);
                    nuevo.error = true;
                }
                nuevo.valor = false;
            }
            if (val1.error == true || val2.error == true)
            {
                nuevo.error = true;
            }
            nuevo.tipo = "BOOL";
            return nuevo;
        }

        private bool unoCero(int dato)
        {            
            if (dato == 1)
                return true;
            return false;
        }

        private Mtrx arr(object addHijo, int cantHijo, int cantPadre)
        {
            Mtrx extra = new Mtrx();
            Type a = extra.GetType();
            Type b = addHijo.GetType();
            //MessageBox.Show(cantPadre.ToString() + " " + cantHijo.ToString() );

            Mtrx padre = new Mtrx();
            if (cantPadre != 0)
            { //dos o mas dimensiones                            

                if (a.Equals(b)){
                    //MessageBox.Show("padre");
                    padre.sizeT = cantPadre;
                    for (int i = 0; i < cantPadre; i++){
                        Mtrx hijo = new Mtrx();
                        hijo.sizeT = cantHijo;
                        for (int j = 0; j < cantHijo; j++) {
                            object nuevo = new object();
                            nuevo = addHijo;
                            hijo.elem.Add(nuevo);
                        }
                        padre.elem.Add(hijo);
                    }                    
                }
                else{
                    //MessageBox.Show("hijos");                    
                    padre.sizeT = cantHijo;
                    for (int j = 0; j < cantHijo; j++) {
                        object nuevo = new object();
                        nuevo = addHijo;
                        padre.elem.Add(nuevo);
                    }
                        
                }                
            }
            else
            { //una sola dimension
                padre.sizeT = cantHijo;
                for (int j = 0; j < cantHijo; j++)
                    padre.elem.Add(addHijo);
            }
            return padre;
        }

        private Tupla establecerLogico(Tupla original,int fila, int columna) {
            Tupla nueva = new Tupla();
            switch (original.tipo){
                case "INT":
                    if (Convert.ToInt32(original.valor) == 0){
                        nueva.valor = false;
                    }else if (Convert.ToInt32(original.valor) == 1){
                        nueva.valor = true;
                    }else{
                        //ERROR
                        addError("INT. Una condicion requiere un valor BOOL o su equivalente 1 o 0", fila, columna);
                        nueva.error = true;
                    }
                    break;
                case "DOUBLE":
                    if (Convert.ToDouble(original.valor) == 0){
                        nueva.valor = false;
                    }else if (Convert.ToDouble(original.valor) == 1){
                        nueva.valor = true;
                    }else{
                        //ERROR
                        addError("DOUBLE. Una condicion requiere un valor BOOL o su equivalente 1 o 0", fila, columna);
                        nueva.error = true;
                    }
                    break;
                case "CHAR":
                    if ((Int32)Convert.ToChar(original.valor) == 0) {
                        nueva.valor = false;
                    }else if ((Int32)Convert.ToChar(original.valor) == 1){
                        nueva.valor = true;
                    }else { 
                        //ERROR
                        addError("CHAR. Una condicion requiere un valor BOOL o su equivalente 1 o 0", fila, columna);
                        nueva.error = true;
                    }
                    break;
                case "BOOL":
                    return original;                    
                case "STRING":
                    addError("STRING. Una condicion requiere un valor BOOL o su equivalente 1 o 0", fila, columna);
                    nueva.error = true;
                    break;
            }
            nueva.tipo = "BOOL";
            return nueva;
        }

        private void copiarVuelta() {
            Ambito acc = pila.ElementAt(actual);
            if (acc.actual == 0) { //SOLO HAY UN ELEMENTO EN SUB AMBTIO
                for (int i = 0; i < acc.pila.ElementAt(acc.actual).tabla.Count; i++){
                    for (int j = 0; j < pila.ElementAt(actual).tabla.Count; j++){
                        if (acc.pila.ElementAt(acc.actual).tabla.ElementAt(i).nombrre == pila.ElementAt(actual).tabla.ElementAt(j).nombrre) {
                            pila.ElementAt(actual).tabla.ElementAt(j).valor = acc.pila.ElementAt(acc.actual).tabla.ElementAt(i).valor;
                            break;
                        }
                    }
                }
            }else{//HAY VARIOS ELEMENTOS EN SUB AMBITO
                for (int i = 0; i < acc.pila.ElementAt(acc.actual).tabla.Count; i++){
                    for (int j = 0; j < acc.pila.ElementAt(acc.actual-1).tabla.Count; j++){
                        if (acc.pila.ElementAt(acc.actual).tabla.ElementAt(i).nombrre == acc.pila.ElementAt(acc.actual-1).tabla.ElementAt(j).nombrre){
                            acc.pila.ElementAt(acc.actual - 1).tabla.ElementAt(j).valor = acc.pila.ElementAt(acc.actual).tabla.ElementAt(i).valor;
                            break;
                        }
                    }
                }
            }            
        }

        private void copiarIda() {
            Ambito acc = pila.ElementAt(actual);
            if (acc.actual == 0){//SOLO HAY UN ELEMENTO EN SUB AMBITO
                pila.ElementAt(actual).pila.ElementAt(acc.actual).tabla.AddRange(acc.tabla);
            }else { //HAY VARIOS ELEMENTOS EN SUB AMBITO
                pila.ElementAt(actual).pila.ElementAt(acc.actual).tabla.AddRange(pila.ElementAt(actual).pila.ElementAt(acc.actual-1).tabla);
            }
        }
        
        private Tupla forCon(Tupla id, int final,int aumDec,int fila,int columna){
            Tupla nuevo = new Tupla();
            //int auDec = 0; //VARIABLE PARA DETERMINAR AUMENTO O DECREMENTO DE FOR. -1 DECREMENTO / 1 AUMENTO / 0 NO HACER FOR
            bool encontrado = false;
            
            if (id.dimension.Count == 0){//VARIABLE NORMAL
                //BUSCAR EN AMBITO--------------------------------------
                if (pila.ElementAt(actual).hay == false){
                    for (int i = 0; i < pila.ElementAt(actual).tabla.Count; i++){
                        if (pila.ElementAt(actual).tabla.ElementAt(i).nombrre == id.nombrre && pila.ElementAt(actual).tabla.ElementAt(i).rol=="var"){ //NOMBRE Y VAR
                            if (pila.ElementAt(actual).tabla.ElementAt(i).dimension.Count == 0){//SI ES ID
                                if (pila.ElementAt(actual).tabla.ElementAt(i).tipo == "INT"){
                                    //AUMENTO DECREMENTO
                                    if (Convert.ToInt32(pila.ElementAt(actual).tabla.ElementAt(i).valor) > final)
                                        nuevo.valor = -1;
                                    else if (Convert.ToInt32(pila.ElementAt(actual).tabla.ElementAt(i).valor) < final)
                                        nuevo.valor = 1;
                                    else
                                        nuevo.valor = 0;
                                    //AUMENTAR O DECREMENTAR
                                    if (aumDec != 0) {
                                        int val = Convert.ToInt32(pila.ElementAt(actual).tabla.ElementAt(i).valor);
                                        if (aumDec == 1)
                                            val++;
                                        else 
                                            val--;
                                        pila.ElementAt(actual).tabla.ElementAt(i).valor = val;
                                    }
                                }else { 
                                    //ERROR
                                    addError("FOR. La variable de control debe ser un valor de tipo INT", fila, columna);
                                    nuevo.error = true;                                    
                                }
                            }else { //ES ARREGLO :(
                                //ERROR
                                addError("FOR. Variable de control. Se intenta obtener el valor de una variable pero esta dfinida como un arreglo", fila, columna);
                                nuevo.error = true;                                
                            }
                            encontrado = true;
                            break;
                        }
                    }
                }else { //BUSCAR EN SUB AMBITO------------------------------------
                    Ambito acc = pila.ElementAt(actual);
                    for (int i = 0; i < acc.pila.ElementAt(acc.actual).tabla.Count; i++){
                        if (acc.pila.ElementAt(acc.actual).tabla.ElementAt(i).nombrre == id.nombrre && acc.pila.ElementAt(acc.actual).tabla.ElementAt(i).rol=="var"){ //NOMBRE Y VAR
                            if (acc.pila.ElementAt(acc.actual).tabla.ElementAt(i).dimension.Count == 0){//SI ES ID
                                if (acc.pila.ElementAt(acc.actual).tabla.ElementAt(i).tipo == "INT"){
                                    //AUMENTO DECREMENTO
                                    if (Convert.ToInt32(acc.pila.ElementAt(acc.actual).tabla.ElementAt(i).valor) > final)
                                        nuevo.valor = -1;
                                    else if (Convert.ToInt32(acc.pila.ElementAt(acc.actual).tabla.ElementAt(i).valor) < final)
                                        nuevo.valor = 1;
                                    else
                                        nuevo.valor = 0;
                                    //AUMENTAR O DECREMENTAR
                                    if (aumDec != 0) {
                                        int val = Convert.ToInt32(acc.pila.ElementAt(acc.actual).tabla.ElementAt(i).valor);
                                        if (aumDec == 1)
                                            val++;
                                        else 
                                            val--;
                                        pila.ElementAt(actual).pila.ElementAt(acc.actual).tabla.ElementAt(i).valor = val;
                                    }
                                }else { 
                                    //ERROR
                                    addError("FOR. La variable de control debe ser un valor de tipo INT", fila, columna);
                                    nuevo.error = true;                                    
                                }
                            }else { //ES ARREGLO :(
                                //ERROR
                                addError("FOR. Variable de control. Se intenta obtener el valor de una variable pero esta dfinida como un arreglo", fila, columna);
                                nuevo.error = true;                                
                            }
                            encontrado = true;
                            break;
                        }
                    }
                }
                //TABLA GENERAL
                if (encontrado == false) {
                    for (int i = 0; i < tsG.Count; i++){
                        if (tsG.ElementAt(i).nombrre == id.nombrre && tsG.ElementAt(i).rol == "var") {
                            if (tsG.ElementAt(i).dimension.Count == 0){// ES ID
                                if (tsG.ElementAt(i).tipo == "INT"){
                                    //AUMENTO DECREMENTO
                                    if (Convert.ToInt32(tsG.ElementAt(i).valor) > final)
                                        nuevo.valor = -1;
                                    else if (Convert.ToInt32(tsG.ElementAt(i).valor) < final)
                                        nuevo.valor = 1;
                                    else
                                        nuevo.valor = 0;
                                    //AUMENTAR O DECREMENTAR
                                    if (aumDec != 0) {
                                        int val = Convert.ToInt32(tsG.ElementAt(i).valor);
                                        if (aumDec == 1)
                                            val++;
                                        else 
                                            val--;
                                        tsG.ElementAt(i).valor = val;
                                    }
                                }else {
                                    //ERROR
                                    addError("FOR.GLOBAL. La variable de control debe ser un valor de tipo INT", fila, columna);
                                    nuevo.error = true;       
                                }
                            }else { //ES ARREGLO :(
                                addError("FOR.GLOBAL. Variable de control. Se intenta obtener el valor de una variable pero esta dfinida como un arreglo", fila, columna);
                                nuevo.error = true;  
                            }
                            encontrado = true;
                            break;
                        }
                    }
                    if (encontrado == false) {
                        addError("FOR. No existe la variable que se definio como variable de control", fila, columna);
                        nuevo.error = true;
                    }
                }
            }else{ //ARREGLO ................NOOOOOOOOOOOOOOO :(
                //BUSCAR EN AMBITO--------------------------------------
                if (pila.ElementAt(actual).hay == false){
                    for (int i = 0; i < pila.ElementAt(actual).tabla.Count; i++){
                        if (pila.ElementAt(actual).tabla.ElementAt(i).nombrre == id.nombrre && pila.ElementAt(actual).tabla.ElementAt(i).rol=="var"){ //NOMBRE Y VAR
                            if (pila.ElementAt(actual).tabla.ElementAt(i).dimension.Count > 0){//SI ES ARREGLO
                                if (pila.ElementAt(actual).tabla.ElementAt(i).tipo == "INT"){
                                    
                                    if (pila.ElementAt(actual).tabla.ElementAt(i).dimension.Count == id.dimension.Count){//MISMA CANT DE PARAMETROS
                                        //VALIDAR RANGO DE DIMENSION
                                        bool valido = false;
                                        for (int j = 0; j < pila.ElementAt(actual).tabla.ElementAt(i).dimension.Count; j++){
                                            if (pila.ElementAt(actual).tabla.ElementAt(i).dimension.ElementAt(j) <= id.dimension.ElementAt(j)){
                                                valido = true;
                                                break;
                                            }
                                        }
                                        if (valido == false){//ANALIZAR
                                            //Mtrx mt = (Mtrx)pila.ElementAt(actual).tabla.ElementAt(i).valor;
                                            //if (id.dimension.Count == 1){//UNA DIMENSION
                                            //    //mt.elem[id.dimension.ElementAt(0)] = aux2.valor;	
                                            //    //AUMENTO DECREMENTO                                                
                                            //    if (Convert.ToInt32(mt.elem.ElementAt(id.dimension.ElementAt(0))) > final)
                                            //        nuevo.valor = -1;
                                            //    else if (Convert.ToInt32(mt.elem.ElementAt(id.dimension.ElementAt(0))) < final)
                                            //        nuevo.valor = 1;
                                            //    else
                                            //        nuevo.valor = 0;
                                            //    //AUMENTAR O DECREMENTAR
                                            //    if (aumDec != 0) {
                                            //        int val = Convert.ToInt32(mt.elem.ElementAt(id.dimension.ElementAt(0)));
                                            //        if (aumDec == 1)
                                            //            val++;
                                            //        else 
                                            //            val--;
                                            //        mt.elem[id.dimension.ElementAt(0)] = val;
                                            //    }
                                            //}else{ // N DIMENSIONES
                                            //    for (int k = 0; k < id.dimension.Count; k++){
                                            //        if (k != id.dimension.Count - 1){ //NO ES ULTIMO ELEMNTO
                                            //            mt = (Mtrx)mt.elem.ElementAt(id.dimension.ElementAt(k));
                                            //        }else{ //ULTIMO ELEMENTO
                                            //            //mt.elem[id.dimension.ElementAt(k)] = aux2.valor;	
                                            //            //AUMENTO DECREMENTO
                                            //            if (Convert.ToInt32(mt.elem.ElementAt(id.dimension.ElementAt(k))) > final)
                                            //                nuevo.valor = -1;
                                            //            else if (Convert.ToInt32(mt.elem.ElementAt(id.dimension.ElementAt(k))) < final)
                                            //                nuevo.valor = 1;
                                            //            else
                                            //                nuevo.valor = 0;
                                            //            //AUMENTAR O DECREMENTAR
                                            //            if (aumDec != 0) {
                                            //                int val = Convert.ToInt32(mt.elem.ElementAt(id.dimension.ElementAt(k)));
                                            //                if (aumDec == 1)
                                            //                    val++;
                                            //                else 
                                            //                    val--;
                                            //                mt.elem[id.dimension.ElementAt(k)] = val;
                                            //            }
                                            //        }
                                            //    }
                                            //}
                                            if (id.dimension.Count == 1)
                                            {
                                                int[] a = (int[])pila.ElementAt(actual).tabla.ElementAt(i).valor;                                                
                                                //AUMENTO DECREMENTO                                                
                                                if (Convert.ToInt32(a[id.dimension.ElementAt(0)]) > final)
                                                    nuevo.valor = -1;
                                                else if (Convert.ToInt32(a[id.dimension.ElementAt(0)]) < final)
                                                    nuevo.valor = 1;
                                                else
                                                    nuevo.valor = 0;
                                                //AUMENTAR O DECREMENTAR
                                                if (aumDec != 0)
                                                {
                                                    int val = Convert.ToInt32(a[id.dimension.ElementAt(0)]);
                                                    if (aumDec == 1)
                                                        val++;
                                                    else
                                                        val--;
                                                    a[id.dimension.ElementAt(0)] = val;
                                                }
                                            }
                                            else if (id.dimension.Count == 2)
                                            {
                                                int[,] a = (int[,])pila.ElementAt(actual).tabla.ElementAt(i).valor;
                                                //nuevo.valor = a[id.dimension.ElementAt(0), id.dimension.ElementAt(1)];
                                                //AUMENTO DECREMENTO                                                
                                                if (Convert.ToInt32(a[id.dimension.ElementAt(0), id.dimension.ElementAt(1)]) > final)
                                                    nuevo.valor = -1;
                                                else if (Convert.ToInt32(a[id.dimension.ElementAt(0), id.dimension.ElementAt(1)]) < final)
                                                    nuevo.valor = 1;
                                                else
                                                    nuevo.valor = 0;
                                                //AUMENTAR O DECREMENTAR
                                                if (aumDec != 0)
                                                {
                                                    int val = Convert.ToInt32(a[id.dimension.ElementAt(0), id.dimension.ElementAt(1)]);
                                                    if (aumDec == 1)
                                                        val++;
                                                    else
                                                        val--;
                                                    a[id.dimension.ElementAt(0), id.dimension.ElementAt(1)] = val;
                                                }
                                            }
                                            else if (id.dimension.Count == 3)
                                            {
                                                int[, ,] a = (int[, ,])pila.ElementAt(actual).tabla.ElementAt(i).valor;
                                                //nuevo.valor = a[id.dimension.ElementAt(0), id.dimension.ElementAt(1), id.dimension.ElementAt(2)];
                                                //AUMENTO DECREMENTO                                                
                                                if (Convert.ToInt32(a[id.dimension.ElementAt(0), id.dimension.ElementAt(1), id.dimension.ElementAt(2)]) > final)
                                                    nuevo.valor = -1;
                                                else if (Convert.ToInt32(a[id.dimension.ElementAt(0), id.dimension.ElementAt(1), id.dimension.ElementAt(2)]) < final)
                                                    nuevo.valor = 1;
                                                else
                                                    nuevo.valor = 0;
                                                //AUMENTAR O DECREMENTAR
                                                if (aumDec != 0)
                                                {
                                                    int val = Convert.ToInt32(a[id.dimension.ElementAt(0), id.dimension.ElementAt(1), id.dimension.ElementAt(2)]);
                                                    if (aumDec == 1)
                                                        val++;
                                                    else
                                                        val--;
                                                    a[id.dimension.ElementAt(0), id.dimension.ElementAt(1), id.dimension.ElementAt(2)] = val;
                                                }
                                            }
                                            else if (id.dimension.Count == 4)
                                            {
                                                int[, , ,] a = (int[, , ,])pila.ElementAt(actual).tabla.ElementAt(i).valor;
                                                //nuevo.valor = a[id.dimension.ElementAt(0), id.dimension.ElementAt(1), id.dimension.ElementAt(2), id.dimension.ElementAt(3)];
                                                //AUMENTO DECREMENTO                                                
                                                if (Convert.ToInt32(a[id.dimension.ElementAt(0), id.dimension.ElementAt(1), id.dimension.ElementAt(2), id.dimension.ElementAt(3)]) > final)
                                                    nuevo.valor = -1;
                                                else if (Convert.ToInt32(a[id.dimension.ElementAt(0), id.dimension.ElementAt(1), id.dimension.ElementAt(2), id.dimension.ElementAt(3)]) < final)
                                                    nuevo.valor = 1;
                                                else
                                                    nuevo.valor = 0;
                                                //AUMENTAR O DECREMENTAR
                                                if (aumDec != 0)
                                                {
                                                    int val = Convert.ToInt32(a[id.dimension.ElementAt(0), id.dimension.ElementAt(1), id.dimension.ElementAt(2), id.dimension.ElementAt(3)]);
                                                    if (aumDec == 1)
                                                        val++;
                                                    else
                                                        val--;
                                                    a[id.dimension.ElementAt(0), id.dimension.ElementAt(1), id.dimension.ElementAt(2), id.dimension.ElementAt(3)] = val;
                                                }
                                            }

                                        }else { 
                                            //ERROR
                                            addError("FOR. Se definio un arreglo como variable de control, pero una de sus dimensiones excede el rango", fila, columna);
                                            nuevo.error = true; 
                                        }
                                    }else { //NO COINCIDE NUM DE PARAMETROS
                                        //ERROR
                                        addError("FOR. Se definio un arreglo como variable de control, pero se definio una cantidad de dimensiones incorrecta", fila, columna);
                                        nuevo.error = true;  
                                    }
                                    /*//AUMENTO DECREMENTO
                                    if (Convert.ToInt32(pila.ElementAt(actual).tabla.ElementAt(i).valor) > final)
                                        nuevo.valor = -1;
                                    else if (Convert.ToInt32(pila.ElementAt(actual).tabla.ElementAt(i).valor) < final)
                                        nuevo.valor = 1;
                                    else
                                        nuevo.valor = 0;
                                    //AUMENTAR O DECREMENTAR
                                    if (aumDec != 0) {
                                        int val = Convert.ToInt32(pila.ElementAt(actual).tabla.ElementAt(i).valor);
                                        if (aumDec == 1)
                                            val++;
                                        else 
                                            val--;
                                        pila.ElementAt(actual).tabla.ElementAt(i).valor = val;
                                    }*/
                                }else { 
                                    //ERROR
                                    addError("FOR. La variable de control debe ser un valor de tipo INT", fila, columna);
                                    nuevo.error = true;                                    
                                }
                            }else { //ES ID :(
                                //ERROR
                                addError("FOR. Variable de control. Se intenta obtener el valor de un arreglo pero esta dfinida como variable normal", fila, columna);
                                nuevo.error = true;                                
                            }
                            encontrado = true;
                            break;
                        }
                    }
                }else { //BUSCAR EN SUB AMBITO------------------------------------
                    Ambito acc = pila.ElementAt(actual);
                    for (int i = 0; i < acc.pila.ElementAt(acc.actual).tabla.Count; i++){
                        if (acc.pila.ElementAt(acc.actual).tabla.ElementAt(i).nombrre == id.nombrre && acc.pila.ElementAt(acc.actual).tabla.ElementAt(i).rol=="var"){ //NOMBRE Y VAR
                            if (acc.pila.ElementAt(acc.actual).tabla.ElementAt(i).dimension.Count > 0){//SI ES ARREGLO
                                if (acc.pila.ElementAt(acc.actual).tabla.ElementAt(i).tipo == "INT"){

                                    if (acc.pila.ElementAt(acc.actual).tabla.ElementAt(i).dimension.Count == id.dimension.Count){//MISMA CANT DE PARAMETROS
                                        //VALIDAR RANGO DE DIMENSION
                                        bool valido = false;
                                        for (int j = 0; j < acc.pila.ElementAt(acc.actual).tabla.ElementAt(i).dimension.Count; j++){
                                            if (acc.pila.ElementAt(acc.actual).tabla.ElementAt(i).dimension.ElementAt(j) <= id.dimension.ElementAt(j)){
                                                valido = true;
                                                break;
                                            }
                                        }
                                        if (valido == false){//ANALIZAR
                                            //Mtrx mt = (Mtrx)acc.pila.ElementAt(acc.actual).tabla.ElementAt(i).valor;
                                            //if (id.dimension.Count == 1){//UNA DIMENSION
                                            //    //mt.elem[id.dimension.ElementAt(0)] = aux2.valor;	
                                            //    //AUMENTO DECREMENTO
                                            //    if (Convert.ToInt32(mt.elem.ElementAt(id.dimension.ElementAt(0))) > final)
                                            //        nuevo.valor = -1;
                                            //    else if (Convert.ToInt32(mt.elem.ElementAt(id.dimension.ElementAt(0))) < final)
                                            //        nuevo.valor = 1;
                                            //    else
                                            //        nuevo.valor = 0;
                                            //    //AUMENTAR O DECREMENTAR
                                            //    if (aumDec != 0) {
                                            //        int val = Convert.ToInt32(mt.elem.ElementAt(id.dimension.ElementAt(0)));
                                            //        if (aumDec == 1)
                                            //            val++;
                                            //        else 
                                            //            val--;
                                            //        mt.elem[id.dimension.ElementAt(0)] = val;
                                            //    }
                                            //}else{ // N DIMENSIONES
                                            //    for (int k = 0; k < id.dimension.Count; k++){
                                            //        if (k != id.dimension.Count - 1){ //NO ES ULTIMO ELEMNTO
                                            //            mt = (Mtrx)mt.elem.ElementAt(id.dimension.ElementAt(k));
                                            //        }else{ //ULTIMO ELEMENTO
                                            //            //mt.elem[id.dimension.ElementAt(k)] = aux2.valor;	 
                                            //            //AUMENTO DECREMENTO
                                            //            if (Convert.ToInt32(mt.elem.ElementAt(id.dimension.ElementAt(k))) > final)
                                            //                nuevo.valor = -1;
                                            //            else if (Convert.ToInt32(mt.elem.ElementAt(id.dimension.ElementAt(k))) < final)
                                            //                nuevo.valor = 1;
                                            //            else
                                            //                nuevo.valor = 0;
                                            //            //AUMENTAR O DECREMENTAR
                                            //            if (aumDec != 0) {
                                            //                int val = Convert.ToInt32(mt.elem.ElementAt(id.dimension.ElementAt(k)));
                                            //                if (aumDec == 1)
                                            //                    val++;
                                            //                else 
                                            //                    val--;
                                            //                mt.elem[id.dimension.ElementAt(k)] = val;
                                            //            }
                                            //        }
                                            //    }
                                            //}

                                            if (id.dimension.Count == 1)
                                            {
                                                int[] a = (int[])acc.pila.ElementAt(acc.actual).tabla.ElementAt(i).valor;
                                                //AUMENTO DECREMENTO                                                
                                                if (Convert.ToInt32(a[id.dimension.ElementAt(0)]) > final)
                                                    nuevo.valor = -1;
                                                else if (Convert.ToInt32(a[id.dimension.ElementAt(0)]) < final)
                                                    nuevo.valor = 1;
                                                else
                                                    nuevo.valor = 0;
                                                //AUMENTAR O DECREMENTAR
                                                if (aumDec != 0)
                                                {
                                                    int val = Convert.ToInt32(a[id.dimension.ElementAt(0)]);
                                                    if (aumDec == 1)
                                                        val++;
                                                    else
                                                        val--;
                                                    a[id.dimension.ElementAt(0)] = val;
                                                }
                                            }
                                            else if (id.dimension.Count == 2)
                                            {
                                                int[,] a = (int[,])acc.pila.ElementAt(acc.actual).tabla.ElementAt(i).valor;
                                                //nuevo.valor = a[id.dimension.ElementAt(0), id.dimension.ElementAt(1)];
                                                //AUMENTO DECREMENTO                                                
                                                if (Convert.ToInt32(a[id.dimension.ElementAt(0), id.dimension.ElementAt(1)]) > final)
                                                    nuevo.valor = -1;
                                                else if (Convert.ToInt32(a[id.dimension.ElementAt(0), id.dimension.ElementAt(1)]) < final)
                                                    nuevo.valor = 1;
                                                else
                                                    nuevo.valor = 0;
                                                //AUMENTAR O DECREMENTAR
                                                if (aumDec != 0)
                                                {
                                                    int val = Convert.ToInt32(a[id.dimension.ElementAt(0), id.dimension.ElementAt(1)]);
                                                    if (aumDec == 1)
                                                        val++;
                                                    else
                                                        val--;
                                                    a[id.dimension.ElementAt(0), id.dimension.ElementAt(1)] = val;
                                                }
                                            }
                                            else if (id.dimension.Count == 3)
                                            {
                                                int[, ,] a = (int[, ,])acc.pila.ElementAt(acc.actual).tabla.ElementAt(i).valor;
                                                //nuevo.valor = a[id.dimension.ElementAt(0), id.dimension.ElementAt(1), id.dimension.ElementAt(2)];
                                                //AUMENTO DECREMENTO                                                
                                                if (Convert.ToInt32(a[id.dimension.ElementAt(0), id.dimension.ElementAt(1), id.dimension.ElementAt(2)]) > final)
                                                    nuevo.valor = -1;
                                                else if (Convert.ToInt32(a[id.dimension.ElementAt(0), id.dimension.ElementAt(1), id.dimension.ElementAt(2)]) < final)
                                                    nuevo.valor = 1;
                                                else
                                                    nuevo.valor = 0;
                                                //AUMENTAR O DECREMENTAR
                                                if (aumDec != 0)
                                                {
                                                    int val = Convert.ToInt32(a[id.dimension.ElementAt(0), id.dimension.ElementAt(1), id.dimension.ElementAt(2)]);
                                                    if (aumDec == 1)
                                                        val++;
                                                    else
                                                        val--;
                                                    a[id.dimension.ElementAt(0), id.dimension.ElementAt(1), id.dimension.ElementAt(2)] = val;
                                                }
                                            }
                                            else if (id.dimension.Count == 4)
                                            {
                                                int[, , ,] a = (int[, , ,])acc.pila.ElementAt(acc.actual).tabla.ElementAt(i).valor;
                                                //nuevo.valor = a[id.dimension.ElementAt(0), id.dimension.ElementAt(1), id.dimension.ElementAt(2), id.dimension.ElementAt(3)];
                                                //AUMENTO DECREMENTO                                                
                                                if (Convert.ToInt32(a[id.dimension.ElementAt(0), id.dimension.ElementAt(1), id.dimension.ElementAt(2), id.dimension.ElementAt(3)]) > final)
                                                    nuevo.valor = -1;
                                                else if (Convert.ToInt32(a[id.dimension.ElementAt(0), id.dimension.ElementAt(1), id.dimension.ElementAt(2), id.dimension.ElementAt(3)]) < final)
                                                    nuevo.valor = 1;
                                                else
                                                    nuevo.valor = 0;
                                                //AUMENTAR O DECREMENTAR
                                                if (aumDec != 0)
                                                {
                                                    int val = Convert.ToInt32(a[id.dimension.ElementAt(0), id.dimension.ElementAt(1), id.dimension.ElementAt(2), id.dimension.ElementAt(3)]);
                                                    if (aumDec == 1)
                                                        val++;
                                                    else
                                                        val--;
                                                    a[id.dimension.ElementAt(0), id.dimension.ElementAt(1), id.dimension.ElementAt(2), id.dimension.ElementAt(3)] = val;
                                                }
                                            }

                                        }else { 
                                            //ERROR
                                            addError("FOR. Se definio un arreglo como variable de control, pero una de sus dimensiones excede el rango", fila, columna);
                                            nuevo.error = true; 
                                        }
                                    }else { //NO COINCIDE NUM DE PARAMETROS
                                        //ERROR
                                        addError("FOR. Se definio un arreglo como variable de control, pero se definio una cantidad de dimensiones incorrecta", fila, columna);
                                        nuevo.error = true;  
                                    }
                                    /*//AUMENTO DECREMENTO
                                    if (Convert.ToInt32(acc.pila.ElementAt(acc.actual).tabla.ElementAt(i).valor) > final)
                                        nuevo.valor = -1;
                                    else if (Convert.ToInt32(acc.pila.ElementAt(acc.actual).tabla.ElementAt(i).valor) < final)
                                        nuevo.valor = 1;
                                    else
                                        nuevo.valor = 0;
                                    //AUMENTAR O DECREMENTAR
                                    if (aumDec != 0) {
                                        int val = Convert.ToInt32(acc.pila.ElementAt(acc.actual).tabla.ElementAt(i).valor);
                                        if (aumDec == 1)
                                            val++;
                                        else 
                                            val--;
                                        pila.ElementAt(actual).pila.ElementAt(acc.actual).tabla.ElementAt(i).valor = val;
                                    }*/
                                }else { 
                                    //ERROR
                                    addError("FOR. La variable de control debe ser un valor de tipo INT", fila, columna);
                                    nuevo.error = true;                                    
                                }
                            }else { //ES ARREGLO :(
                                //ERROR
                                addError("FOR. Variable de control. Se intenta obtener el valor de un arreglo pero esta dfinida como una variable normal", fila, columna);
                                nuevo.error = true;                                
                            }
                            encontrado = true;
                            break;
                        }
                    }
                }
                //TABLA GENERAL
                if (encontrado == false) {
                    for (int i = 0; i < tsG.Count; i++){
                        if (tsG.ElementAt(i).nombrre == id.nombrre && tsG.ElementAt(i).rol == "var") {
                            if (tsG.ElementAt(i).dimension.Count > 0){// ES ARREGLO
                                if (tsG.ElementAt(i).tipo == "INT"){

                                    if (tsG.ElementAt(i).dimension.Count == id.dimension.Count){//MISMA CANT DE PARAMETROS
                                        //VALIDAR RANGO DE DIMENSION
                                        bool valido = false;
                                        for (int j = 0; j < tsG.ElementAt(i).dimension.Count; j++){
                                            if (tsG.ElementAt(i).dimension.ElementAt(j) <= id.dimension.ElementAt(j)){
                                                valido = true;
                                                break;
                                            }
                                        }
                                        if (valido == false){//ANALIZAR
                                            //Mtrx mt = (Mtrx)tsG.ElementAt(i).valor;
                                            //if (id.dimension.Count == 1){//UNA DIMENSION
                                            //    //mt.elem[id.dimension.ElementAt(0)] = aux2.valor;	
                                            //    //AUMENTO DECREMENTO
                                            //    if (Convert.ToInt32(mt.elem.ElementAt(id.dimension.ElementAt(0))) > final)
                                            //        nuevo.valor = -1;
                                            //    else if (Convert.ToInt32(mt.elem.ElementAt(id.dimension.ElementAt(0))) < final)
                                            //        nuevo.valor = 1;
                                            //    else
                                            //        nuevo.valor = 0;
                                            //    //AUMENTAR O DECREMENTAR
                                            //    if (aumDec != 0) {
                                            //        int val = Convert.ToInt32(mt.elem.ElementAt(id.dimension.ElementAt(0)));
                                            //        if (aumDec == 1)
                                            //            val++;
                                            //        else 
                                            //            val--;
                                            //        mt.elem[id.dimension.ElementAt(0)] = val;
                                            //    }
                                            //}else{ // N DIMENSIONES
                                            //    for (int k = 0; k < id.dimension.Count; k++){
                                            //        if (k != id.dimension.Count - 1){ //NO ES ULTIMO ELEMNTO
                                            //            mt = (Mtrx)mt.elem.ElementAt(id.dimension.ElementAt(k));
                                            //        }else{ //ULTIMO ELEMENTO
                                            //            //mt.elem[id.dimension.ElementAt(k)] = aux2.valor;	
                                            //            //AUMENTO DECREMENTO
                                            //            if (Convert.ToInt32(mt.elem.ElementAt(id.dimension.ElementAt(k))) > final)
                                            //                nuevo.valor = -1;
                                            //            else if (Convert.ToInt32(mt.elem.ElementAt(id.dimension.ElementAt(k))) < final)
                                            //                nuevo.valor = 1;
                                            //            else
                                            //                nuevo.valor = 0;
                                            //            //AUMENTAR O DECREMENTAR
                                            //            if (aumDec != 0) {
                                            //                int val = Convert.ToInt32(mt.elem.ElementAt(id.dimension.ElementAt(k)));
                                            //                if (aumDec == 1)
                                            //                    val++;
                                            //                else 
                                            //                    val--;
                                            //                mt.elem[id.dimension.ElementAt(k)] = val;
                                            //            }
                                            //        }
                                            //    }
                                            //}

                                            if (id.dimension.Count == 1)
                                            {
                                                int[] a = (int[])tsG.ElementAt(i).valor;
                                                //AUMENTO DECREMENTO                                                
                                                if (Convert.ToInt32(a[id.dimension.ElementAt(0)]) > final)
                                                    nuevo.valor = -1;
                                                else if (Convert.ToInt32(a[id.dimension.ElementAt(0)]) < final)
                                                    nuevo.valor = 1;
                                                else
                                                    nuevo.valor = 0;
                                                //AUMENTAR O DECREMENTAR
                                                if (aumDec != 0)
                                                {
                                                    int val = Convert.ToInt32(a[id.dimension.ElementAt(0)]);
                                                    if (aumDec == 1)
                                                        val++;
                                                    else
                                                        val--;
                                                    a[id.dimension.ElementAt(0)] = val;
                                                }
                                            }
                                            else if (id.dimension.Count == 2)
                                            {
                                                int[,] a = (int[,])tsG.ElementAt(i).valor;
                                                //nuevo.valor = a[id.dimension.ElementAt(0), id.dimension.ElementAt(1)];
                                                //AUMENTO DECREMENTO                                                
                                                if (Convert.ToInt32(a[id.dimension.ElementAt(0), id.dimension.ElementAt(1)]) > final)
                                                    nuevo.valor = -1;
                                                else if (Convert.ToInt32(a[id.dimension.ElementAt(0), id.dimension.ElementAt(1)]) < final)
                                                    nuevo.valor = 1;
                                                else
                                                    nuevo.valor = 0;
                                                //AUMENTAR O DECREMENTAR
                                                if (aumDec != 0)
                                                {
                                                    int val = Convert.ToInt32(a[id.dimension.ElementAt(0), id.dimension.ElementAt(1)]);
                                                    if (aumDec == 1)
                                                        val++;
                                                    else
                                                        val--;
                                                    a[id.dimension.ElementAt(0), id.dimension.ElementAt(1)] = val;
                                                }
                                            }
                                            else if (id.dimension.Count == 3)
                                            {
                                                int[, ,] a = (int[, ,])tsG.ElementAt(i).valor;
                                                //nuevo.valor = a[id.dimension.ElementAt(0), id.dimension.ElementAt(1), id.dimension.ElementAt(2)];
                                                //AUMENTO DECREMENTO                                                
                                                if (Convert.ToInt32(a[id.dimension.ElementAt(0), id.dimension.ElementAt(1), id.dimension.ElementAt(2)]) > final)
                                                    nuevo.valor = -1;
                                                else if (Convert.ToInt32(a[id.dimension.ElementAt(0), id.dimension.ElementAt(1), id.dimension.ElementAt(2)]) < final)
                                                    nuevo.valor = 1;
                                                else
                                                    nuevo.valor = 0;
                                                //AUMENTAR O DECREMENTAR
                                                if (aumDec != 0)
                                                {
                                                    int val = Convert.ToInt32(a[id.dimension.ElementAt(0), id.dimension.ElementAt(1), id.dimension.ElementAt(2)]);
                                                    if (aumDec == 1)
                                                        val++;
                                                    else
                                                        val--;
                                                    a[id.dimension.ElementAt(0), id.dimension.ElementAt(1), id.dimension.ElementAt(2)] = val;
                                                }
                                            }
                                            else if (id.dimension.Count == 4)
                                            {
                                                int[, , ,] a = (int[, , ,])tsG.ElementAt(i).valor;
                                                //nuevo.valor = a[id.dimension.ElementAt(0), id.dimension.ElementAt(1), id.dimension.ElementAt(2), id.dimension.ElementAt(3)];
                                                //AUMENTO DECREMENTO                                                
                                                if (Convert.ToInt32(a[id.dimension.ElementAt(0), id.dimension.ElementAt(1), id.dimension.ElementAt(2), id.dimension.ElementAt(3)]) > final)
                                                    nuevo.valor = -1;
                                                else if (Convert.ToInt32(a[id.dimension.ElementAt(0), id.dimension.ElementAt(1), id.dimension.ElementAt(2), id.dimension.ElementAt(3)]) < final)
                                                    nuevo.valor = 1;
                                                else
                                                    nuevo.valor = 0;
                                                //AUMENTAR O DECREMENTAR
                                                if (aumDec != 0)
                                                {
                                                    int val = Convert.ToInt32(a[id.dimension.ElementAt(0), id.dimension.ElementAt(1), id.dimension.ElementAt(2), id.dimension.ElementAt(3)]);
                                                    if (aumDec == 1)
                                                        val++;
                                                    else
                                                        val--;
                                                    a[id.dimension.ElementAt(0), id.dimension.ElementAt(1), id.dimension.ElementAt(2), id.dimension.ElementAt(3)] = val;
                                                }
                                            }

                                        }else { 
                                            //ERROR
                                            addError("FOR.GLOBAL. Se definio un arreglo como variable de control, pero una de sus dimensiones excede el rango", fila, columna);
                                            nuevo.error = true; 
                                        }
                                    }else { //NO COINCIDE NUM DE PARAMETROS
                                        //ERROR
                                        addError("FOR.GLOBAL. Se definio un arreglo como variable de control, pero se definio una cantidad de dimensiones incorrecta", fila, columna);
                                        nuevo.error = true;  
                                    }
                                    /*//AUMENTO DECREMENTO
                                    if (Convert.ToInt32(tsG.ElementAt(i).valor) > final)
                                        nuevo.valor = -1;
                                    else if (Convert.ToInt32(tsG.ElementAt(i).valor) < final)
                                        nuevo.valor = 1;
                                    else
                                        nuevo.valor = 0;
                                    //AUMENTAR O DECREMENTAR
                                    if (aumDec != 0) {
                                        int val = Convert.ToInt32(tsG.ElementAt(i).valor);
                                        if (aumDec == 1)
                                            val++;
                                        else 
                                            val--;
                                        tsG.ElementAt(i).valor = val;
                                    }*/
                                }else {
                                    //ERROR
                                    addError("FOR.GLOBAL. La variable de control debe ser un valor de tipo INT", fila, columna);
                                    nuevo.error = true;       
                                }
                            }else { //ES ID :(
                                addError("FOR.GLOBAL. Variable de control. Se intenta obtener el valor de un arreglo pero esta dfinida como una variable normal", fila, columna);
                                nuevo.error = true;  
                            }
                            encontrado = true;
                            break;
                        }
                    }
                    if (encontrado == false) {
                        addError("FOR. No existe la variable que se definio como variable de control", fila, columna);
                        nuevo.error = true;
                    }
                }
            }

            return nuevo;
        }

        private bool caseCompar(Tupla aux, Tupla aux2,int fila,int columna) {
            bool retorno = false;
            switch (aux.tipo){
                case "INT":
                    switch (aux2.tipo){
                        case "INT":
                            if (Convert.ToInt32(aux.valor) == Convert.ToInt32(aux2.valor))
                                retorno = true;
                            break;
                        case "DOUBLE":
                            if (Convert.ToInt32(aux.valor) == Convert.ToDouble(aux2.valor))
                                retorno = true;
                            break;
                        case "CHAR":
                            if (Convert.ToInt32(aux.valor) == (Int32)Convert.ToChar(aux2.valor))
                                retorno = true;
                            break;
                        case "STRING":
                            //ERROR
                            addError("CASE. No se pueden comparar un INT con un STRING", fila,columna);
                            break;
                    }
                    break;
                case "DOUBLE":
                    switch (aux2.tipo){
                        case "INT":
                            if (Convert.ToDouble(aux.valor) == Convert.ToInt32(aux2.valor))
                                retorno = true;
                            break;
                        case "DOUBLE":
                            if (Convert.ToDouble(aux.valor) == Convert.ToDouble(aux2.valor))
                                retorno = true;
                            break;
                        case "CHAR":
                            if (Convert.ToDouble(aux.valor) == (Int32)Convert.ToChar(aux2.valor))
                                retorno = true;
                            break;
                        case "STRING":
                            //ERROR
                            addError("CASE. No se pueden comparar un DOUBLE con un STRING", fila, columna);
                            break;
                    }
                    break;
                case "CHAR":
                    switch (aux2.tipo){
                        case "INT":
                            if ((Int32)Convert.ToChar(aux.valor) == Convert.ToInt32(aux2.valor))
                                retorno = true;
                            break;
                        case "DOUBLE":
                            if ((Int32)Convert.ToChar(aux.valor) == Convert.ToDouble(aux2.valor))
                                retorno = true;
                            break;
                        case "CHAR":
                            if ((Int32)Convert.ToChar(aux.valor) == (Int32)Convert.ToChar(aux2.valor))
                                retorno = true;
                            break;
                        case "STRING":
                            int b = (Int32)Convert.ToChar(aux.valor);
                            int a = 0;
                            string auxx = Convert.ToString(aux2.valor);
                            for (int i = 0; i < auxx.Count(); i++)
                                a += (Int32)Convert.ToChar(auxx[i]);
                            if (a == b)
                                retorno = true; 
                            break;
                    }
                    break;
                case "STRING":
                    switch (aux2.tipo){
                        case "INT":
                            //ERROR
                            addError("CASE. No se pueden comparar un STRING con un INT", fila, columna);
                            break;
                        case "DOUBLE":
                            //ERROR
                            addError("CASE. No se pueden comparar un STRING con un DOUBLE", fila, columna);
                            break;
                        case "CHAR":
                            int b = (Int32)Convert.ToChar(aux2.valor);
                            int a = 0;
                            string auxx = Convert.ToString(aux.valor);
                            for (int i = 0; i < auxx.Count(); i++)
                                a += (Int32)Convert.ToChar(auxx[i]);
                            if (a == b)
                                retorno = true;                            
                            break;
                        case "STRING":
                            if (Convert.ToString(aux.valor) == Convert.ToString(aux2.valor))
                                retorno = true;
                            break;
                    }
                    break;
            }
            return retorno;
        }
    }
}
