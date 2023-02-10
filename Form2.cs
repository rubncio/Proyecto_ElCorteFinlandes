using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing.Imaging;
using System.Reflection.Emit;
using System.Windows.Forms.VisualStyles;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace ElCorteFinlandes
{
    public partial class Form2 : Form
    {
        public Form2()
        {
            InitializeComponent();
        }

        private void Form2_Load(object sender, EventArgs e)
        {
            // TODO: esta línea de código carga datos en la tabla 'elCorteFinlandesDataSet.Inventario' Puede moverla o quitarla según sea necesario.
            this.inventarioTableAdapter.Fill(this.elCorteFinlandesDataSet.Inventario);

        }
        [DllImport("user32.DLL", EntryPoint = "ReleaseCapture")]
        private extern static void ReleaseCapture();
        [DllImport("user32.DLL", EntryPoint = "SendMessage")]
        private extern static void SendMessage(System.IntPtr hWnd, int wMsg, int wParam, int lParam);
        private void panel1_MouseDown(object sender, MouseEventArgs e)
        {
            ReleaseCapture();
            SendMessage(this.Handle, 0x112, 0xf012, 0);
        }

        private void iconButton2_Click(object sender, EventArgs e)
        {
            System.Windows.Forms.MessageBoxButtons
               opciones = MessageBoxButtons.YesNo;
            DialogResult mensaje = MessageBox.Show("¿Esta Seguro de que quiere Salir ", "Seleccione una de las opciones, Si o No.", opciones, MessageBoxIcon.Question);
            if (mensaje != DialogResult.No)
            {
                
               Application.Exit();
            }
        }

        private void iconButton3_Click(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Minimized;
        }

        private void panel5_Paint(object sender, PaintEventArgs e)
        {

        }

        private void dataGridView2_CellContentClick_1(object sender, DataGridViewCellEventArgs e)
        {


            Form1 form1 = new Form1();
            /*IContainerControl a=form1.GetContainerControl();
            MessageBox.Show(a.ToString());*/




            string id = Convert.ToString(dataGridView2.Rows[dataGridView2.CurrentCellAddress.Y].Cells[0].Value);
            if (id != "") { 
            form1.textBox1.Text = id.ToString();

            SqlConnection conexion = new SqlConnection("Server = localhost\\SQLEXPRESS; Database = ElCorteFinlandes; integrated security = true");

            conexion.Open();


            string cadena = "select Descripcion, Precio, Departamento, NumeroDeExistencias from Inventario where Id=" + id;

            SqlCommand comando = new SqlCommand(cadena, conexion);

            SqlDataReader registros = comando.ExecuteReader();
            registros.Read();

            form1.textBox2.Text = registros["Descripcion"].ToString();
            form1.textBox3.Text = registros["Precio"].ToString().Replace(',', '.');
            form1.textBox4.Text = registros["NumeroDeExistencias"].ToString();
            form1.InsertarDepartamento(registros["Departamento"].ToString());

            //Insercion de imagen.
            SqlConnection conexionImagen = new SqlConnection("Server = localhost\\SQLEXPRESS; Database = ElCorteFinlandes; integrated security = true");
            string cadenaImagen = "select Imagen from Inventario where Id=" + form1.textBox1.Text;
            SqlCommand comandoImagen = new SqlCommand(cadenaImagen, conexionImagen);
            conexionImagen.Open();
            SqlDataAdapter da = new SqlDataAdapter(comandoImagen);
            DataSet ds = new DataSet();

            da.Fill(ds, "Inventario");
            int c = ds.Tables["Inventario"].Rows.Count;
            if (!Convert.IsDBNull(ds.Tables["Inventario"].Rows[c - 1]["Imagen"]))
            {
                Byte[] byteImagen = new Byte[0];
                byteImagen = (Byte[])(ds.Tables["Inventario"].Rows[c - 1]["Imagen"]);
                MemoryStream stmBLOBData = new MemoryStream(byteImagen);
                form1.pictureBox2.Image = Image.FromStream(stmBLOBData);
            }
            else
            {
                form1.pictureBox2.Image = null;
            }
            this.Hide();
            form1.Show();
            }
            else
            {
                MessageBox.Show("seleccione un registro o fila", "Incorrecto", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void iconButton1_Click(object sender, EventArgs e)
        {

            Form Form1 = new Form1();
            this.Hide();
            Form1.ShowDialog();

        }
    }
}
