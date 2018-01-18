using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Irony.Ast;
using Irony.Parsing;

namespace _OLC1_Proyecto2_201403877.Analizador
{
    class Gramatica: Grammar
    {        

        public Gramatica() : base(caseSensitive: true)
        {

            #region Comentario
            CommentTerminal comentarioLinea = new CommentTerminal("tComentL","//","\n");
            CommentTerminal comentarioMLinea = new CommentTerminal("tComentML","{","}");
            #endregion

            #region ER
            //RegexBasedTerminal blanco = new RegexBasedTerminal("blanco","[\w]+");
            NumberLiteral tnumero = new NumberLiteral("tNumero");
            RegexBasedTerminal tVbool = new RegexBasedTerminal("tVbool","false|true");                   
            //RegexBasedTerminal tCaracter = new RegexBasedTerminal("tCaracter", "\'^\'\'|\'[!-~]\'|\'#n\'|\'#t\'");            
            StringLiteral tCaracter = TerminalFactory.CreateCSharpChar("tCaracter");
            StringLiteral tCadena = new StringLiteral("tCadena","\"");
            IdentifierTerminal tId = new IdentifierTerminal("tId");

            #endregion

            #region Terminales                        

            var tInt = ToTerm("INT");
            var tDouble = ToTerm("DOUBLE");
            var tString = ToTerm("STRING");
            var tChar = ToTerm("CHAR");
            var tBool = ToTerm("BOOL");

            var tMM = ToTerm("++");
            var tMiMi = ToTerm("--");

            var tMas = ToTerm("+");
            var tMenos = ToTerm("-");
            var tPor = ToTerm("*");
            var tDiv = ToTerm("/");
            var tPot = ToTerm("^");

            var tIgualQ = ToTerm("==");
            var tDifQ = ToTerm("!=");
            var tMenorQ = ToTerm("<");
            var tMenorIQ = ToTerm("<=");
            var tMayorQ = ToTerm(">");
            var tMayorIQ = ToTerm(">=");

            var tOr = ToTerm("||");
            var tAnd = ToTerm("&&");            
            var tNot = ToTerm("!!");

            var tProgram = ToTerm("PROGRAM");
            var tBegin = ToTerm("BEGIN");
            var tEnd = ToTerm("END");
            var tUses = ToTerm("USES");

            var tVar = ToTerm("VAR");
            var tOf = ToTerm("OF");
            var tProcedure = ToTerm("PROCEDURE");
            var tReturn = ToTerm("RETURN");

            var tPtoComa = ToTerm(";");
            var tDosPtos = ToTerm(":");
            var tComa = ToTerm(",");
            var tParA = ToTerm("(");
            var tParC = ToTerm(")");
            var tIgual = ToTerm(":=");

            var tIf = ToTerm("IF");
            var tThen = ToTerm("THEN");
            var tElse = ToTerm("ELSE");
            var tElIf = ToTerm("ELSEIF");
            var tSelect = ToTerm("SELECT");
            var tCase = ToTerm("CASE");
            var tWhile = ToTerm("WHILE");
            var tDo = ToTerm("DO");
            var tRepeat = ToTerm("REPEAT");
            var tUntil = ToTerm("UNTIL");
            var tFor = ToTerm("FOR");
            var tTo = ToTerm("TO");

            var tBreak = ToTerm("BREAK");
            var tContinue = ToTerm("CONTINUE");

            var tGraf = ToTerm("GRAF");
            var tOut = ToTerm("OUT");

            #endregion

            #region No Terminales
            NonTerminal S = new NonTerminal("S");
            NonTerminal CLS = new NonTerminal("CLS");
            NonTerminal TIPO = new NonTerminal("TIPO");
            
            NonTerminal PRG = new NonTerminal("PRG");
            NonTerminal PRG_CL = new NonTerminal("PRG_CL");
            NonTerminal PRG_S = new NonTerminal("PRG_S");
            NonTerminal PCLS = new NonTerminal("PCLS");

            NonTerminal IMPORT = new NonTerminal("IMPORT");
            NonTerminal DECL = new NonTerminal("DECL");
            NonTerminal MTRX = new NonTerminal("MTRX");
            NonTerminal LST_DM = new NonTerminal("LST_DM");

            NonTerminal BGN = new NonTerminal("BGN");
            NonTerminal FIN = new NonTerminal("FIN");
            
            NonTerminal EPS = new NonTerminal("EPS");

            NonTerminal ART = new NonTerminal("ART");
            NonTerminal MNS = new NonTerminal("MNS");
            NonTerminal POR = new NonTerminal("POR");
            NonTerminal DIV = new NonTerminal("DIV");
            NonTerminal PN = new NonTerminal("PN");
            NonTerminal OPC_ART = new NonTerminal("OPC_ART");

            NonTerminal EXP = new NonTerminal("EXP");
            NonTerminal AND = new NonTerminal("AND");
            NonTerminal NNEG = new NonTerminal("NNEG");
            NonTerminal OPC_EXP = new NonTerminal("OPC_EXP");
            NonTerminal REL = new NonTerminal("REL");

            NonTerminal MET = new NonTerminal("MET");
            NonTerminal LST_PAR = new NonTerminal("LST_PAR");
            NonTerminal PRE_LSTP = new NonTerminal("PRE_LSTP");
            NonTerminal CUERPO_PROC = new NonTerminal("CUERPO_PROC");
            NonTerminal FUNC = new NonTerminal("FUNC");

            NonTerminal CLS_PROC = new NonTerminal("CLS_PROC");
            NonTerminal CLS_P = new NonTerminal("CLS_P");

            NonTerminal LL_PROC = new NonTerminal("LL_PROC");
            NonTerminal LST_PRT2 = new NonTerminal("LST_PRT2");
            NonTerminal PRE_LST2 = new NonTerminal("PRE_LST2");            
            NonTerminal ACC_MTRX = new NonTerminal("ACC_MTRX");
            NonTerminal RETURN = new NonTerminal("RETURN");
            NonTerminal OPC_RET = new NonTerminal("OPC_RET");
            NonTerminal ASIG = new NonTerminal("ASIG");
            NonTerminal OPC_ASIG = new NonTerminal("OPC_ASIG");

            NonTerminal IF = new NonTerminal("IF");
            NonTerminal FIN_IF = new NonTerminal("FIN_IF");
            NonTerminal SWT = new NonTerminal("SWT");
            NonTerminal CASE = new NonTerminal("CASE");
            NonTerminal OPC_SWT = new NonTerminal("OPC_SWT");
            NonTerminal WHL = new NonTerminal("WHL");
            NonTerminal DO_WHL = new NonTerminal("DO_WHL");
            NonTerminal RPT_UNT = new NonTerminal("RPT_UNT");
            NonTerminal FOR = new NonTerminal("FOR");

            NonTerminal GRAF = new NonTerminal("GRAF");
            NonTerminal TP_GRF = new NonTerminal("TP_GRF");
            NonTerminal TP_PRM = new NonTerminal("TP_PRM");
            NonTerminal PRE_TP = new NonTerminal("PRE_TP");
            NonTerminal OUT = new NonTerminal("OUT");

            NonTerminal BRK = new NonTerminal("BRK");
            NonTerminal CNT = new NonTerminal("CNT");

            NonTerminal FMA = new NonTerminal("FMA");
            NonTerminal FMB = new NonTerminal("FMB");
            NonTerminal FMC = new NonTerminal("FMC");

            NonTerminal aux = new NonTerminal("AUX");

            NonTerminal REL_M = new NonTerminal("REL_M");
            NonTerminal ART_M = new NonTerminal("ART_M");
            NonTerminal AUX_2 = new NonTerminal("AUX_2");
            NonTerminal AUX_3 = new NonTerminal("AUX_3");
            NonTerminal EXTRA = new NonTerminal("EXTRA");

            #endregion

            #region Gramatica           

            S.Rule = CLS;

            EPS.Rule = Empty;

            CLS.Rule = PRG + BGN + FIN;

            /*----------------------PRIMER BLOQUE*/
            PRG.Rule = tProgram + tId + tPtoComa + PRG_CL;
            PRG.ErrorRule = SyntaxError + tBegin;

            PRG_CL.Rule = PRG_S
                | EPS
                ;

            PRG_S.Rule = PRG_S + PCLS
                | PCLS
                ;

            PCLS.Rule = IMPORT
                | tVar + DECL + tPtoComa
                | MTRX
                ;
            PCLS.ErrorRule = SyntaxError + tPtoComa;

            IMPORT.Rule = tUses + tId + tPtoComa;
            IMPORT.ErrorRule = SyntaxError + tPtoComa;

            DECL.Rule = DECL + tComa + tId
                | TIPO + tDosPtos + tId
                ;

            TIPO.Rule = tInt
                | tDouble
                | tString
                | tChar
                | tBool
                ;

            MTRX.Rule = tVar + tId + tOf + TIPO + LST_DM + tPtoComa;
            MTRX.ErrorRule = SyntaxError + tPtoComa;

            LST_DM.Rule = MakePlusRule(LST_DM,EXP);            

            /*RELACIONAL Y LOGICA*/            
            EXP.Rule = EXP + tOr + EXP
                | EXP + tAnd + EXP
                | tNot + EXP
                | ART + REL + ART
                | ART
                ;

            REL.Rule = tMayorQ
                | tMayorIQ
                | tMenorQ
                | tMenorIQ
                | tIgualQ
                | tDifQ
                ;            

            // LLAMADAS A PROCEDIMIENTOS EN EXPRESIONES SE TOMAN EN ACC_MTRX
            ART.Rule = ART + tMas + ART
                | ART + tMenos + ART
                | ART + tPor + ART
                | ART + tDiv + ART
                | tParA + EXP + tParC
                | ACC_MTRX                
                | tnumero
                | ART + tPot + ART
                | tId
                | tCadena
                | tCaracter
                | tVbool                                                
                | tMenos + ART                
                ;

            /*----------------------SEGUNDO BLOQUE*/
            BGN.Rule = tBegin + CUERPO_PROC;
            BGN.ErrorRule = SyntaxError + tEnd;

            /*----------------------TERCER BLOQUE*/

            FIN.Rule = tEnd + FMA;

            FMA.Rule = FMB
                | EPS
                ;

            FMB.Rule = FMB + FMC
                | FMC
                ;

            FMC.Rule = MET
                | FUNC
                ;
            FMC.ErrorRule = SyntaxError + tEnd;

            MET.Rule = tProcedure + tId + tParA + PRE_LSTP + tParC + tPtoComa + tBegin + CUERPO_PROC + tEnd;            
            MET.ErrorRule = SyntaxError + tEnd;

            FUNC.Rule = tProcedure + TIPO + tDosPtos + tId + tParA + PRE_LSTP + tParC + tPtoComa + tBegin + CUERPO_PROC + tEnd;            
            FUNC.ErrorRule = SyntaxError + tEnd;

            CUERPO_PROC.Rule = CLS_PROC
                | EPS
                ;

            CLS_PROC.Rule = CLS_PROC + CLS_P
                | CLS_P
                ;

            CLS_P.Rule = SWT
                | LL_PROC + tPtoComa
                | IF
                | WHL
                | DO_WHL
                | RPT_UNT
                | FOR
                | tVar + DECL + tPtoComa
                | ASIG + tPtoComa
                | GRAF
                | OUT
                | RETURN
                | BRK
                | CNT                
                | MTRX
                ;
            CLS_P.ErrorRule = SyntaxError + tEnd;
            CLS_P.ErrorRule = SyntaxError + tPtoComa;

            PRE_LSTP.Rule = LST_PAR
                | EPS
                ;

            LST_PAR.Rule = LST_PAR + tComa + TIPO + tDosPtos + tId
                | TIPO + tDosPtos + tId
                ;

            /*CICLOS Y ESTRUCTURAS DE CONTROL*/

            IF.Rule = tIf + tParA + EXP + tParC + tThen + tBegin + CUERPO_PROC + tEnd + FIN_IF;
            IF.ErrorRule = SyntaxError + tEnd;

            FIN_IF.Rule = tElse + tBegin + CUERPO_PROC + tEnd
                | tElIf + tParA + EXP + tParC + tThen + tBegin + CUERPO_PROC + tEnd + FIN_IF
                | EPS
                ;
            FIN_IF.ErrorRule = SyntaxError + tEnd;

            SWT.Rule = tSelect + tCase + tParA + EXP + tParC + tOf + tBegin + CASE + OPC_SWT + tEnd;
            SWT.ErrorRule = SyntaxError + tEnd;

            CASE.Rule = EXP + tDosPtos + tBegin + CUERPO_PROC + tEnd;
            CASE.ErrorRule = SyntaxError + tEnd;

            OPC_SWT.Rule = CASE + OPC_SWT
                | tElse + tDosPtos + tBegin + CUERPO_PROC + tEnd
                | EPS
                ;
            OPC_SWT.ErrorRule = SyntaxError + tEnd;

            WHL.Rule = tWhile + tParA + EXP + tParC + tDo + tBegin + CUERPO_PROC + tEnd;
            WHL.ErrorRule = SyntaxError + tEnd;

            DO_WHL.Rule = tDo + tBegin + CUERPO_PROC + tEnd + tWhile + tParA + EXP + tParC + tPtoComa;
            DO_WHL.ErrorRule = SyntaxError + tEnd;
            DO_WHL.ErrorRule = SyntaxError + tPtoComa;

            RPT_UNT.Rule = tRepeat + tBegin + CUERPO_PROC + tEnd + tUntil + tParA + EXP + tParC + tPtoComa;
            RPT_UNT.ErrorRule = SyntaxError + tEnd;
            RPT_UNT.ErrorRule = SyntaxError + tPtoComa;

            FOR.Rule = tFor + EXTRA + tTo + EXP + tDo + tBegin + CUERPO_PROC + tEnd;
            FOR.ErrorRule = SyntaxError + tEnd;

            EXTRA.Rule = ASIG
                 | ACC_MTRX
                 | tId;

            /*OTROS*/

            LL_PROC.Rule = tId + tParA + PRE_LST2;

            PRE_LST2.Rule = LST_PRT2 + tParC
                | tParC
                ;

            LST_PRT2.Rule = LST_PRT2 + tComa + EXP
                | EXP
                ;

            ACC_MTRX.Rule = LL_PROC + AUX_2
                | tId + REL_M + AUX_2
                ;

            AUX_2.Rule = AUX_3
                | EPS
                ;

            AUX_3.Rule = AUX_3 + EXP
                | EXP
                ;

            /*----------------------*/

            RETURN.Rule = tReturn + OPC_RET;
            RETURN.ErrorRule = SyntaxError + tPtoComa;

            OPC_RET.Rule = tPtoComa
                | EXP + tPtoComa
                ;
            OPC_RET.ErrorRule = SyntaxError + tPtoComa;

            ASIG.Rule = ACC_MTRX + OPC_ASIG
                | tId + OPC_ASIG
                ;            

            OPC_ASIG.Rule = tIgual + EXP
                    | tMM
                    | tMiMi
                //| tMas + tMas
                //| tMenos + tMenos
                ;            

            BRK.Rule = tBreak + tPtoComa;

            CNT.Rule = tContinue + tPtoComa;

            /*NATIVOS DEL LENGUAJE*/
            GRAF.Rule = tGraf + tParA + TP_GRF + tId + PRE_TP + tParC + tPtoComa;
            GRAF.ErrorRule = SyntaxError + tPtoComa;

            TP_GRF.Rule = TIPO
                | EPS;

            PRE_TP.Rule = TP_PRM
                | EPS
                ;

            TP_PRM.Rule = TP_PRM + TIPO
                | TIPO
                ;

            OUT.Rule = tOut + tParA + EXP + tParC + tPtoComa;
            OUT.ErrorRule = SyntaxError + tPtoComa;

            /*EXTRAS, CHAPUS*/

            REL_M.Rule = REL_M + tOr + REL_M
                | REL_M + tAnd + REL_M
                | tNot + REL_M
                | REL_M + REL + REL_M
                | ART_M
                ;

            /*ARITMETICA*/                        
            ART_M.Rule = ART_M + tMas + ART_M
                | ART_M + tMenos + ART_M
                | ART_M + tPor + ART_M
                | ART_M + tDiv + ART_M                
                | tnumero
                | ART_M + tPot + ART_M
                | tId
                | tCadena
                | tCaracter
                | tVbool
                //| tMenos + ART_M               
                ;

            #endregion

            #region Preferencias                        

            base.NonGrammarTerminals.Add(comentarioLinea);
            base.NonGrammarTerminals.Add(comentarioMLinea);
            
            this.Root = S;
            
            this.RegisterOperators(1, Associativity.Left, tOr);
            this.RegisterOperators(2, Associativity.Left, tAnd);
            this.RegisterOperators(3, Associativity.Right, tNot);

            this.RegisterOperators(4, Associativity.Left, tIgualQ, tDifQ, tMenorQ, tMenorIQ, tMayorQ, tMayorIQ);

            this.RegisterOperators(5, Associativity.Left, tMas, tMenos);
            this.RegisterOperators(6, Associativity.Left, tPor, tDiv);
            this.RegisterOperators(7, Associativity.Right, tPot);
            
            //this.MarkPunctuation(tPtoComa,tDosPtos);
            #endregion
        }
        
    }
}
