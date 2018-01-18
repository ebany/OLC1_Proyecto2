using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Irony.Ast;
using Irony.Parsing;
using _OLC1_Proyecto2_201403877.Analizador;
using System.Diagnostics;

namespace _OLC1_Proyecto2_201403877
{
    public partial class Form1 : Form
    {
        //EDITOR DE TEXTO
        ArrayList listaPestaña = new ArrayList();
        private List<Tupla> tsG = new List<Tupla>();
        int contarPestaña = 0;
        int carcater = 0;

        //DIRECTORIO
        private String rutaActual = "";
        private String seleccion = "";
        List<String> path1 = new List<String>();

        public Form1()
        {
            InitializeComponent();
            timer1.Interval = 10;
            timer1.Tick += new EventHandler(timer1_Tick);
            timer1.Enabled = true;

            TabPage nuevaPestaña = new TabPage("NP");

            RichTextBox pagina = new RichTextBox();
            pagina.Location = new System.Drawing.Point(60, 20);
            pagina.Size = new System.Drawing.Size(tabControl1.Size.Width - 90, tabControl1.Size.Height - 70);
            PictureBox barra = new PictureBox();
            barra.Location = new System.Drawing.Point(30, 20);
            barra.Size = new System.Drawing.Size(30, tabControl1.Size.Height - 70);
            barra.Paint += new System.Windows.Forms.PaintEventHandler(this.pictureBox1_Paint);
            listaPestaña.Add(nuevaPestaña);
            tabControl1.TabPages.Add(nuevaPestaña);
            contarPestaña++;
            tabControl1.SelectedTab = nuevaPestaña;
            tabControl1.SelectedTab.Controls.Add(barra);
            tabControl1.SelectedTab.Controls.Add(pagina); 
        }

        //REFRESCAR NUMERACION DE FILA Y COLUMNA
        private void timer1_Tick(object sender, EventArgs e)
        {
            PictureBox temp2 = (PictureBox)tabControl1.SelectedTab.Controls[0];
            RichTextBox temp = (RichTextBox)tabControl1.SelectedTab.Controls[1];
            temp2.Refresh();
            int posicion = temp.SelectionStart;
            int linea = temp.GetLineFromCharIndex(posicion);
            int columna = posicion - temp.GetFirstCharIndexOfCurrentLine();
            label1.Text = "Posicion Cursor --> Linea:" + linea + "  Columna:" + columna;
        }

        //NUMERACION FILA Y COLUMNA, EDITOR DE TEXTO.
        private void pictureBox1_Paint(object sender, PaintEventArgs e)
        {
            carcater = 0;
            //System.Threading.Thread.Sleep(1000);
            PictureBox temp2 = (PictureBox)tabControl1.SelectedTab.Controls[0];
            RichTextBox temp = (RichTextBox)tabControl1.SelectedTab.Controls[1];
            float altura = temp.GetPositionFromCharIndex(0).Y;

            if (temp.Lines.Length > 0)
            {
                for (int i = 0; i < temp.Lines.Length; i++)
                {
                    int temp1 = i;
                    String a = Convert.ToString(temp1);
                    SolidBrush drawBrush = new SolidBrush(Color.Blue);
                    PointF drawPoint = new PointF(5, altura);

                    e.Graphics.DrawString(a, temp.Font, Brushes.Blue, temp2.Width - (e.Graphics.MeasureString(a, temp.Font).Width + 10), altura);
                    carcater += temp.Lines[i].Length + 1;
                    altura = temp.GetPositionFromCharIndex(carcater).Y;
                }
            }
            else
            {
                int b = 0;
                String c = Convert.ToString(b);
                e.Graphics.DrawString(c, temp.Font, Brushes.Blue, temp2.Width - (e.Graphics.MeasureString(c, temp.Font).Width + 10), altura);
            }
        }

        // AGREGAR PESTAÑA
        private void button1_Click(object sender, EventArgs e)
        {
            TabPage nuevaPestaña = new TabPage("NP");

            RichTextBox pagina = new RichTextBox();
            pagina.Location = new System.Drawing.Point(60, 20);
            pagina.Size = new System.Drawing.Size(tabControl1.Size.Width - 90, tabControl1.Size.Height - 70);
            PictureBox barra = new PictureBox();
            barra.Location = new System.Drawing.Point(30, 20);
            barra.Size = new System.Drawing.Size(30, tabControl1.Size.Height - 70);
            barra.Paint += new System.Windows.Forms.PaintEventHandler(this.pictureBox1_Paint);
            listaPestaña.Add(nuevaPestaña);
            tabControl1.TabPages.Add(nuevaPestaña);
            contarPestaña++;
            tabControl1.SelectedTab = nuevaPestaña;
            tabControl1.SelectedTab.Controls.Add(barra);
            tabControl1.SelectedTab.Controls.Add(pagina);    
        }

        //ELIMINAR PESTAÑA
        private void button2_Click(object sender, EventArgs e)//eliminar pestaña
        {
            if (contarPestaña > 1)
            {
                TabPage curret_tab = tabControl1.SelectedTab;
                listaPestaña.Remove(curret_tab);
                tabControl1.TabPages.Remove(curret_tab);
                contarPestaña--;
            }
            else {
                MessageBox.Show("No se puede eliminar.");
            }
        }

        //ESTABLECER RUTA DE TREE VIEW
        private void button3_Click(object sender, EventArgs e)
        {
            if (folderBrowserDialog1.ShowDialog() == DialogResult.OK)
            {
                String folderPath = folderBrowserDialog1.SelectedPath;
                label2.Text = "Ruta Actual: " + folderPath;
                rutaActual = folderPath;
                treeView1.Nodes.Clear();
                DirectoryInfo directoryInfo = new DirectoryInfo(folderPath);
                path1.Clear();
                treeView1.Nodes.Add(CrearArbol(directoryInfo));
                treeView1.ExpandAll();
            }
        }

        private TreeNode CrearArbol(DirectoryInfo directoryInfo)
        {
            TreeNode treeNode = new TreeNode(directoryInfo.Name);
            foreach (var item in directoryInfo.GetDirectories())            
                treeNode.Nodes.Add(CrearArbol(item));
            
            foreach (var item in directoryInfo.GetFiles()) {                
                if (item.ToString().Contains(".olc")) {
                    treeNode.Nodes.Add(new TreeNode(item.Name));
                    //MessageBox.Show(item.DirectoryName + "\\" + item.ToString());                    
                    String ruta = item.DirectoryName + "\\" + item.ToString();
                    path1.Add(ruta);
                }
            }
            return treeNode;
        }

        //ACTUALIZAR TREE VIEW
        private void button4_Click(object sender, EventArgs e)
        {
            if (rutaActual != "")
            {
                treeView1.Nodes.Clear();
                DirectoryInfo directoryInfo = new DirectoryInfo(rutaActual);
                path1.Clear();
                treeView1.Nodes.Add(CrearArbol(directoryInfo));
                treeView1.ExpandAll();
            }
            else
            {
                MessageBox.Show("Seleccione un directorio! >:v");
            }
        }

        //SELECCION EN DIRECTORIO
        private void treeView1_AfterSelect(object sender, TreeViewEventArgs e)
        {         
            int fin = e.Node.FullPath.IndexOf("\\");
            if (fin != -1)
            {
                String pathSelected = e.Node.FullPath.Remove(0, fin);
                label3.Text = "Seleccion: " + pathSelected;
                seleccion = rutaActual + pathSelected;
            }
            else {
                label3.Text = "Seleccion: " + e.Node.FullPath;
                seleccion = rutaActual + "\\";
            }            
        }

        //MODIFICAR
        private void button6_Click(object sender, EventArgs e)
        {
            RichTextBox temp = (RichTextBox)tabControl1.SelectedTab.Controls[1];
            if (seleccion != "" && seleccion.Contains(".olc"))
            {
                String reemplazo = temp.Text;
                System.IO.File.WriteAllText(seleccion, reemplazo);
                MessageBox.Show("Datos de archivo, Modificados.");
            }            
        }

        //CARGAR TEXTO DE ARCHIVO AL EDITOR
        private void treeView1_DoubleClick(object sender, EventArgs e)
        {            
            RichTextBox temp = (RichTextBox)tabControl1.SelectedTab.Controls[1];
            if (seleccion != "" && seleccion.Contains(".olc")) 
            {                
                String[] ruta = seleccion.Split('\\');
                tabControl1.SelectedTab.Text = ruta[ruta.Length-1];
                temp.Text = "";
                System.IO.StreamReader sr = new System.IO.StreamReader(seleccion);
                String texto = sr.ReadToEnd();
                temp.AppendText(texto);
                sr.Close();
            }
            
        }

        //GUARDAR CONTENIDO EN ARCHIVO OLC Y ACTUALIZAR TREE VIEW
        private void button5_Click(object sender, EventArgs e)
        {
            saveFileDialog1.Filter = "OLC class(*.olc)|*.olc";
            if (saveFileDialog1.ShowDialog() == DialogResult.OK)
            {
                System.IO.StreamWriter sr = new System.IO.StreamWriter(saveFileDialog1.FileName);
                RichTextBox temp = (RichTextBox)tabControl1.SelectedTab.Controls[1];
                sr.Write(temp.Text);
                sr.Close();

                if (rutaActual != "") 
                {
                    treeView1.Nodes.Clear();
                    DirectoryInfo directoryInfo = new DirectoryInfo(rutaActual);
                    path1.Clear();
                    treeView1.Nodes.Add(CrearArbol(directoryInfo));
                    treeView1.ExpandAll();
                }                
            }
        }

        //analizar
        private void button7_Click(object sender, EventArgs e)
        {
            RichTextBox temp = (RichTextBox)tabControl1.SelectedTab.Controls[1];
            Sintactico.lista.Clear();
            ParseTreeNode raiz = Sintactico.analizar(temp.Text);            

            if (raiz == null || Sintactico.lista.Count()!=0) {
                MessageBox.Show("Existen errores");
                crearHTML(Sintactico.lista);
            } else {
                MessageBox.Show("cadena aceptada");
                Recorrido nuevo = new Recorrido();
                
                //LLENAR TABLA DE SIMBOLOS GENERAL
                nuevo.pasada_1(raiz);
                tablaSimbolos(nuevo.tablaSimbolos);

                if (nuevo.lista.Count == 0){ //SIN ERRORES SEMANTICOS                
                    
                } else { 
                    
                }
            }
        }

        //CREACION DE ARCHIVO HTML. REPORTE DE ERRORES
        private void crearHTML(List<Object> lista)
        {
            System.IO.File.Delete("Errores.html");
            System.IO.File.AppendAllText("Errores.html", "<html>");
            System.IO.StreamWriter escribir = new System.IO.StreamWriter("Errores.html", true);
            escribir.Write("<head><meta http-equiv=\"content-type\" charset=\"utf-8\"> <title>Errores</title></head>");
            DateTime thisDay = DateTime.Now;
            escribir.Write("Dia de ejecucion: " + thisDay.ToString("D") + "<br>");
            escribir.Write("Hora de ejecucion: " + thisDay.ToString("h:m tt") + "<br>");
            escribir.Write("<center><h1><font color=\"navy\">" + "Descripcion de Errores" + "</font></h1></center>");
            escribir.Write("<TABLE border=\"1\" width=\"300\" align=\"center\"> <th>Linea</th> <th>Columna</th> <th>Tipo de Error</th> <th>Descripcion</th> <th>Ubicacion</th> ");
            for (int i = 0; i < lista.Count(); i++)
            {
                List<String> temp = (List<String>)lista.ElementAt(i);

                escribir.Write("<TR>" +
                                "<TD align=\"center\">" + temp.ElementAt(2) + " </TD>" +
                                "<TD align=\"center\">" + temp.ElementAt(3) + " </TD>" +
                                "<TD align=\"center\">" + temp.ElementAt(0) + " </TD>"
                                );
                if (temp.ElementAt(0) == "Lexico")
                {
                    escribir.Write("<TD align=\"center\">" + "Simbolo no esperado: " + temp.ElementAt(1) + " </TD>");
                }
                else if (temp.ElementAt(0) == "control")
                {
                    escribir.Write("<TD align=\"center\">" + "Error: " + temp.ElementAt(1) + " </TD>");
                }
                else if(temp.ElementAt(0)=="semantico"){
                    escribir.Write("<TD align=\"center\">" + temp.ElementAt(1) + " </TD>");
                }
                else
                {
                    escribir.Write("<TD align=\"center\">" + "Se Esperaba: " + temp.ElementAt(1) + " </TD>");
                }
                escribir.Write("<TD align=\"center\">" + temp.ElementAt(4) + " </TD>" + "</TR>");
            }
            escribir.Write("</TABLE>");
            escribir.Write("</html>");
            escribir.Close();
        }

        //HTML TABLA DE SIMBOLOS
        private void tablaSimbolos(List<Tupla> tabla) {
            System.IO.File.Delete("tabla.html");
            System.IO.File.AppendAllText("tabla.html", "<html>");
            System.IO.StreamWriter escribir = new System.IO.StreamWriter("tabla.html", true);
            escribir.Write("<head><meta http-equiv=\"content-type\" charset=\"utf-8\"> <title>tabla de simbolos</title></head>");
            DateTime thisDay = DateTime.Now;
            escribir.Write("Dia de ejecucion: " + thisDay.ToString("D") + "<br>");
            escribir.Write("Hora de ejecucion: " + thisDay.ToString("h:m tt") + "<br>");
            escribir.Write("<center><h1><font color=\"navy\">" + "Contenido de tabla" + "</font></h1></center>");
            escribir.Write("<TABLE border=\"1\" width=\"300\" align=\"center\"> <thead> <tr> ");
            escribir.Write("<th>Nombre</th> <th>Tipo</th> <th>Rol</th> <th>Ambito</th> <th>Nodo</th> <th>Parametros</th> <th>Dim</th> <th>Valor</th>");
            escribir.Write("</tr> </thead> <tbody>");

            for (int i = 0; i < tabla.Count; i++)
            {
                escribir.Write("<tr>");

                escribir.Write("<td>" + tabla.ElementAt(i).nombrre + "</td>");

                escribir.Write("<td>"+ tabla.ElementAt(i).tipo +"</td>");
                escribir.Write("<td>" + tabla.ElementAt(i).rol + "</td>");
                escribir.Write("<td>" + tabla.ElementAt(i).ambito + "</td>");
                if (tabla.ElementAt(i).root!=null)                
                    escribir.Write("<td> SI</td>");
                else
                    escribir.Write("<td> </td>");
                escribir.Write("<td>");
                for (int j = 0; j < tabla.ElementAt(i).parametros.Count; j++)
                {
                    escribir.Write(tabla.ElementAt(i).parametros.ElementAt(j).tipo+",");
                }
                escribir.Write("</td>");
                escribir.Write("<td>");
                for (int j = 0; j < tabla.ElementAt(i).dimension.Count; j++)
                {
                    escribir.Write(tabla.ElementAt(i).dimension.ElementAt(j).ToString() + ",");
                }
                escribir.Write("</td>");
                escribir.Write("<td>");
                if (tabla.ElementAt(i).dimension.Count == 0)
                {
                    if (tabla.ElementAt(i).rol!="proc")
                    {
                        switch (tabla.ElementAt(i).tipo)
                        {
                            case "INT":
                                escribir.Write(Convert.ToInt32(tabla.ElementAt(i).valor).ToString());
                                break;
                            case "DOUBLE":
                                escribir.Write(Convert.ToDouble(tabla.ElementAt(i).valor).ToString());
                                break;
                            case "STRING":
                                escribir.Write(Convert.ToString(tabla.ElementAt(i).valor));
                                break;
                            case "CHAR":
                                escribir.Write(Convert.ToChar(tabla.ElementAt(i).valor).ToString());
                                break;
                            case "BOOL":
                                escribir.Write(Convert.ToBoolean(tabla.ElementAt(i).valor).ToString());
                                break;
                        }   
                    }                    
                }
                else
                    escribir.Write("es arreglo");
                escribir.Write("</td>");

                escribir.Write("</tr>");
            }

            escribir.Write("</tbody> </TABLE>");
            escribir.Write("</html>");
            escribir.Close();
        }

        //ABRIR REPORTE DE ERRORES
        private void button8_Click(object sender, EventArgs e)
        {
            Process pr = new Process();
            pr.StartInfo.FileName = "Errores.html";
            pr.Start();
        }

        //ABRIR TABLA DE SIMBOLOS
        private void button9_Click(object sender, EventArgs e)
        {
            Process pr = new Process();
            pr.StartInfo.FileName = "tabla.html";
            pr.Start();
        }

        //ANALIZAR CARPETA
        private void button10_Click(object sender, EventArgs e)
        {
            if (seleccion.Contains(".olc") && path1.Contains(seleccion)) {
                //MessageBox.Show(seleccion);
                List<Object> erroresLS = new List<Object>();
                List<Object> erroreSMT = new List<Object>();
                bool error = false;
                tsG.Clear();
                for (int i = 0; i < path1.Count; i++) {
                    System.IO.StreamReader sr = new System.IO.StreamReader(path1.ElementAt(i));
                    String texto = sr.ReadToEnd();
                    Sintactico.lista.Clear();
                    String[] ruta = path1.ElementAt(i).Split('\\');
                    Sintactico.archivo = ruta[ruta.Length - 1];
                    ParseTreeNode raiz = Sintactico.analizar(texto);
                    sr.Close();

                    if (raiz == null || Sintactico.lista.Count() != 0) {
                        erroresLS.AddRange(Sintactico.lista);
                        error = true;
                    } else {
                        Recorrido nuevo = new Recorrido();
                        nuevo.arch = ruta[ruta.Length - 1];
                        //LLENAR TABLA DE SIMBOLOS GENERAL
                        nuevo.pasada_1(raiz);
                        tsG.AddRange(nuevo.tablaSimbolos);
                        erroreSMT.AddRange(nuevo.lista);
                    }
                }
                //ANALISIS LEXICO Y SINTACTICO SATISFACTORIO
                if (error != true && erroresLS.Count == 0) {                    
                    tablaSimbolos(tsG);
                    //crearHTML(erroreSMT);
                    pasada2 ps2 = new pasada2();
                    ps2.erroreSMT.AddRange(erroreSMT);
                    ps2.tsG.AddRange(tsG);
                    String[] prin = seleccion.Split('\\');
                    String[] prin2 = prin[prin.Count() - 1].Split('.');                     
                    ps2.clsPrin = prin2[0];

                    //INICIAR ANALISIS SEMANTICO
                    ps2.iniciar();
                    richTextBox1.Clear();
                    richTextBox1.AppendText(ps2.consola);
                    crearHTML(ps2.erroreSMT);
                    if (ps2.erroreSMT.Count > 0){
                        MessageBox.Show("Existen Errores Semanticos.");
                    }else {
                        MessageBox.Show("Analisis Satisfactorio");
                    }                                            
                } else {
                    MessageBox.Show("Existen errores\nlexicos o sintacticos");
                    crearHTML(erroresLS);
                }
            } else
                MessageBox.Show("seleccione el archivo principal");            
        }

    }
}
