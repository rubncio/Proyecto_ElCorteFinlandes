using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Drawing.Imaging;
using System.Data.SqlClient;
using System.Reflection.Emit;
using System.Windows.Forms.VisualStyles;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using System.Runtime.InteropServices;

namespace ElCorteFinlandes
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }
        public string departamento = "";
        public String Id ="";
        public String Descripcion = "";
        public String Precio = "";
        public String stock = "";
        public SqlConnection conexion = new SqlConnection("Server = localhost\\SQLEXPRESS; Database = ElCorteFinlandes; integrated security = true");



        [DllImport("user32.DLL", EntryPoint = "ReleaseCapture")]
        private extern static void ReleaseCapture();
        [DllImport("user32.DLL", EntryPoint = "SendMessage")]
        private extern static void SendMessage(System.IntPtr hWnd, int wMsg, int wParam, int lParam);
        //////////////////////FUNCION PARA ARRASTRAR////////////////
        private void panel1_MouseDown(object sender, MouseEventArgs e)
        {
            ReleaseCapture();
            SendMessage(this.Handle, 0x112, 0xf012, 0);
        }

        //////////////////////boton SUBIR Imagen////////////////
        private void button2_Click(object sender, EventArgs e)
        {
            openFileDialog1.Filter = "Imagenes JPG|*.JPG|Imagenes GIF|*.gif|Imagenes Bitmaps|*.bmp|Imagenes PNG|*.bmp";
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                pictureBox2.Image = Image.FromFile(openFileDialog1.FileName);
            }
            
        }

        //////////////////////boton QUITAR Imagen////////////////
        private void button3_Click(object sender, EventArgs e)//boton quitar Imagen.
        {
            pictureBox2.Image = null;
        }

        //////////////////////FUNCIONES////////////////
        public Boolean comprobacionRegistros()
        {
            SqlConnection conexion1 = new SqlConnection("Server = localhost\\SQLEXPRESS; Database = ElCorteFinlandes; integrated security = true");
            conexion1.Open();
            SqlCommand comandoRev = new SqlCommand("select Id from inventario where Id=" + textBox1.Text, conexion1);
            SqlDataReader sDT = comandoRev.ExecuteReader();
            Boolean hayRegistros = sDT.Read();

            return hayRegistros;
        }
        public bool obtenerCampos()
        {
            departamento = obtenerDepartamento();
            Id =textBox1.Text;
            Descripcion = "'" + textBox2.Text + "'";
            Precio = textBox3.Text.Replace(',', '.');
            stock = textBox4.Text;
            int a;
            double b;
            return (int.TryParse(Id, out a) && double.TryParse(Precio, out b)&& int.TryParse(stock, out a));
            
        }
        public String obtenerDepartamento()
        {
            int departamentoIndex = comboBox1.SelectedIndex;
            String departamento = "";
            switch (departamentoIndex)
            {
                case 0:
                    departamento = "'Ropa'";
                    break;
                case 1:
                    departamento = "'Alimentacion'";
                    break;
                case 2:
                    departamento = "'Electronica'";
                    break;
                default:
                    departamento = null;
                    break;
            }
            return departamento;
        }
        public void InsertarDepartamento(String cadenaDepartamento)
        {
            switch (cadenaDepartamento)
            {
                case "Ropa":
                    comboBox1.SelectedIndex = 0;
                    break;
                case "Alimentacion":

                    comboBox1.SelectedIndex = 1;
                    break;
                case "Electronica":
                    comboBox1.SelectedIndex = 2;
                    break;
                default:
                    comboBox1.SelectedIndex = -1;
                    break;
            }
        }
        ///////////////////////////OPERACIONES CRUD////////////////////////////
        /////////////////AGREGAR/////////////////
        private void iconButton1_Click(object sender, EventArgs e)
        {
            //se crea un capturador de errores para que en cualquier error no cierre la aplicacion.
            try
            {
                //se abre la conexion
                conexion.Open();
                //evalua que todos los campos tengan contenido y si no lo tiene lo indica.
                if (obtenerDepartamento() != null && !textBox1.Text.Equals("") && !textBox2.Text.Equals("") && !textBox3.Text.Equals("") && !textBox4.Text.Equals(""))
                {
                    if (obtenerCampos())
                    {
                        //se cogen los valores, el contenido introducido en dichos campos.
                       

                        //Se guarda en una cadena la query que va a insertar los datos en la tabla.
                        string cadena = "insert into Inventario (Id, Precio, Descripcion, Departamento, NumeroDeExistencias) values (" + Id + ", " + Precio + ", " + Descripcion + ", " + departamento + ", " + stock + " )";
                        //creo un comando con la cadena que hemos guardado antes y la conexion a la base de datos.
                        SqlCommand comando = new SqlCommand(cadena, conexion);

                        /*comprueba si ya existe un registro con el id proporcionado, 
                        en el caso de que no lo haya ejecuta el comando anteriormente creado
                        y limpia todos los campos menos el de imagen puesto que todavia no ha terminado la insercion.
                        en el caso de que ya halla un registro lo indica mediante un messageBox
                         */
                        if (!comprobacionRegistros())
                        {
                            MessageBox.Show("Registro agregado con exito", "Registro Agregado", MessageBoxButtons.OK, MessageBoxIcon.Information);

                            textBox1.Text = "";
                            textBox2.Text = "";
                            textBox3.Text = "";
                            textBox4.Text = "";
                            comboBox1.SelectedIndex = -1;

                            comando.ExecuteNonQuery();//inserta los valores.
                            if (pictureBox2.Image != null)
                            {/*
                     * una vez insertados los datos se va a evaluar si se va a introducir la imagen o no,
                     * evaluando el contenido del picturebox
                     */
                                SqlCommand ComandoMeterImg = new SqlCommand();
                                ComandoMeterImg.Connection = conexion;
                                /*una vez creado el comando y agregada su conexion
                                 * introduciremos la query que va a insertar la imagen 
                                 * en el registro anteriormente creado mediante parametros.
                                 */
                                ComandoMeterImg.CommandText = "Update Inventario Set Imagen=@Foto where Id=" + Id;
                                ComandoMeterImg.Parameters.Add("@Foto", System.Data.SqlDbType.Image);//se agrega el comando y se indica de que tipo va a ser.
                                /*creamos el objeto de flujo que va recoger el dato de la imagen mediante el metodo .Save .
                                 * posteriormente le indicamos al comando que el parametro tiene como valor el buffer del flujo.
                                 * executamos la query y vaciamos el pictureBox.
                                 */
                                MemoryStream stream = new MemoryStream();

                                pictureBox2.Image.Save(stream, System.Drawing.Imaging.ImageFormat.Png);

                                ComandoMeterImg.Parameters["@Foto"].Value = stream.GetBuffer();
                                ComandoMeterImg.ExecuteNonQuery();
                                pictureBox2.Image = null;
                            }
                        }
                        else
                        {
                            MessageBox.Show("El registro ya ha sido creado anteriormente", "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                    else
                    {
                        MessageBox.Show("tipos de Datos incorrectos en campo Precio o Id o stock", "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    

                }
                else
                {
                    MessageBox.Show("introduzca todos los valores.", "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                conexion.Close();
            }
            catch (Exception ex)
            {
                conexion.Close();
                MessageBox.Show(ex.ToString());
            }
        }
        /////////////////ELIMINAR/////////////////
        private void iconButton4_Click(object sender, EventArgs e)
        {

            try
            {
                conexion.Open();//se abre la conexion
                //se evalua si se ha ingresado algun valor en el campo del Id, si no se ha hecho lo indica.
                if (!textBox1.Text.Equals(""))
                {   //se obtienen los valores de los campos.
                    obtenerCampos();
                    int x;
                    if (int.TryParse(Id,out x ))
                    {
                        
                        /*se indica mediante una advertencia si esta seguro de que quiere borrar el producto
                         * si elige que no, se indica que no se ha borrado, y si elige que si se borrará.
                         */

                        DialogResult mensaje = MessageBox.Show("¿Esta Seguro de que quiere borrar el producto de Id " + Id + "?", "Seleccione una de las opciones, Si o No.", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                        if (mensaje == DialogResult.No)
                        {
                            MessageBox.Show("La operacion ha sido cancelada", "Operacion Abortada", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                        else
                        {
                            string cadena = "delete from Inventario where Id=" + Id;
                            SqlCommand comando = new SqlCommand(cadena, conexion);//Crea el comando que va a ejecutar la query cadena sobre la conexion(BBDD).
                            if (comando.ExecuteNonQuery() == 1)
                            {//si la ejecucion ha afectado a al menos una fila se indicará que el producto ha sido borrado y si no se indicará que no existe.
                                textBox1.Text = "";
                                textBox2.Text = "";
                                textBox3.Text = "";
                                textBox4.Text = "";
                                comboBox1.SelectedIndex = -1;
                                pictureBox2.Image = null;
                                MessageBox.Show("Articulo Borrado", "El articulo ha sido borrado", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            }
                            else
                            {
                                MessageBox.Show(" No existe un artículo con el código ingresado ", "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);

                            }

                        }
                    }
                    else
                    {
                        MessageBox.Show("tipo de dato de Id incorrecto", "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);

                    }
                }
                else
                {
                    MessageBox.Show("introduzca todos los valores.", "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                conexion.Close();
            }
            catch (Exception ex)
            {
                conexion.Close();
                MessageBox.Show(ex.ToString());
            }
        }
        /////////////////CONSULTAR/////////////////
        
         public void iconButton6_Click(object sender, EventArgs e)
        {
            try {
                conexion.Open();
                if (!textBox1.Text.Equals(""))
                {//se evalua si hay un Id, si lo hay lo buscara y si no indicará que se debe introducir un Id.
                    obtenerCampos();
                    int x;
                    if (int.TryParse(Id, out x))
                    { 

                        string cadena = "select Descripcion, Precio, Departamento, NumeroDeExistencias from Inventario where Id=" + textBox1.Text;
                        //se crea la cadena que buscara el producto por el Id.
                        SqlCommand comando = new SqlCommand(cadena, conexion);
                        //se crea un datareader que reciba las filas devueltas poe el query.
                        SqlDataReader registros = comando.ExecuteReader();
                        if (registros.Read())
                        {//si se el metodo .Read devuelve true indica que hay al menos un registro y nos hemos desplazado a el 
                         //ingresamos el valor de cada columna en su correspondiente textBox.
                            textBox2.Text = registros["Descripcion"].ToString();
                            textBox3.Text = registros["Precio"].ToString();
                            textBox4.Text = registros["NumeroDeExistencias"].ToString();
                            InsertarDepartamento(registros["Departamento"].ToString());

                            //Insercion de imagen.
                            SqlConnection conexionImagen = new SqlConnection("Server = localhost\\SQLEXPRESS; Database = ElCorteFinlandes; integrated security = true");
                            string cadenaImagen = "select Imagen from Inventario where Id=" + textBox1.Text;
                            SqlCommand comandoImagen = new SqlCommand(cadenaImagen, conexionImagen);
                            conexionImagen.Open();
                            SqlDataAdapter da = new SqlDataAdapter(comandoImagen);
                            DataSet ds = new DataSet();
                            da.Fill(ds, "Inventario");//mete la tabla inventario en el dataset ds

                            int c = ds.Tables["Inventario"].Rows.Count;//mira el numero de filas que ha recibido para la tabla inventario
                            if (!Convert.IsDBNull(ds.Tables["Inventario"].Rows[c - 1]["Imagen"]))
                            {//se evalua si se ha obtenido imagen.
                                Byte[] byteImagen = new Byte[0];
                                byteImagen = (Byte[])(ds.Tables["Inventario"].Rows[c - 1]["Imagen"]);
                                MemoryStream stmBLOBData = new MemoryStream(byteImagen);
                                pictureBox2.Image = Image.FromStream(stmBLOBData);
                            }
                            else
                            {
                                pictureBox2.Image = null;
                            }
                        }
                        else
                        {
                            MessageBox.Show("No existe un artículo con el código introducido", "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                    else
                    {
                        MessageBox.Show("Tipo de dato de Id introducido incorrecto", "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);

                    }

                }
                else
                {
                    MessageBox.Show("introduzca el Id.", "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                conexion.Close();
            }
            catch (Exception ex)
            {
                conexion.Close();
                MessageBox.Show(ex.ToString());
            }
        }
    
        /////////////////MODIFICAR/////////////////
        
        private void iconButton5_Click(object sender, EventArgs e)
        {
            try
            {
                conexion.Open();
                if (!textBox1.Text.Equals(""))
                {
                    if (obtenerCampos())
                    {
                        string cadenamodificar = " UPDATE Inventario set Descripcion =  " + Descripcion + " , Precio = " + Precio + " , Departamento = " + departamento + ", NumeroDeExistencias= " + stock + ", Imagen= null where Id = " + Id;
                        SqlCommand comandoModificar = new SqlCommand(cadenamodificar, conexion);
                        DialogResult mensaje = MessageBox.Show("¿Esta Seguro de que quiere modificar el producto de Id " + Id + "?", "Seleccione una de las opciones, Si o No.", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                        if (mensaje == DialogResult.No)
                        {
                            MessageBox.Show("La operacion ha sido abortada","Operacion abortada", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                        else
                        {
                            if (comandoModificar.ExecuteNonQuery() == 1)
                            {
                                MessageBox.Show("Operacion realizada", " Se modificaron los datos del artículo ", MessageBoxButtons.OK, MessageBoxIcon.Information);
                                textBox1.Text = "";
                                textBox2.Text = "";
                                textBox3.Text = "";
                                textBox4.Text = "";
                                comboBox1.SelectedIndex = -1;
                                if (pictureBox2.Image != null)
                                {//evalua si hay imagen en para añadirla y en el caso de que no la hubiera tambien se actualica el campo de la foto a null.
                                    SqlCommand ComandoMeterImg = new SqlCommand();
                                    ComandoMeterImg.Connection = conexion;
                                    ComandoMeterImg.CommandText = "Update Inventario Set Imagen=@Foto where Id=" + Id;
                                    ComandoMeterImg.Parameters.Add("@Foto", System.Data.SqlDbType.Image);
                                    //Imagen
                                    MemoryStream stream = new MemoryStream();
                                    pictureBox2.Image.Save(stream, System.Drawing.Imaging.ImageFormat.Png);
                                    ComandoMeterImg.Parameters["@Foto"].Value = stream.GetBuffer();
                                    ComandoMeterImg.ExecuteNonQuery();
                                }


                                pictureBox2.Image = null;
                            }
                            else
                            {
                                MessageBox.Show(" No existe un artículo con el código introducido ", "Error", MessageBoxButtons.YesNo, MessageBoxIcon.Error);
                            }
                        }
                    }
                    else
                    {
                        MessageBox.Show("tipos de Datos incorrectos en campo Precio o Id", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);

                    }

                }
                else
                {
                    MessageBox.Show("introduzca todos los valores.", "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error  );
                }
                conexion.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Asegurese de no dejar los campos vacios, a continuacion se mostrará el error.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                conexion.Close();
                MessageBox.Show(ex.ToString());
            }
        }


        private void iconButton3_Click(object sender, EventArgs e)//boton de minimizar.
        {
            this.WindowState = FormWindowState.Minimized;
        }

        private void iconButton2_Click(object sender, EventArgs e)//boton de salir.
        {
            DialogResult mensaje = MessageBox.Show("¿Esta Seguro de que quiere Salir ", "Seleccione una de las opciones, Si o No.", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (mensaje != DialogResult.No)
            {
                Application.Exit();
            }
        }
       
        private void iconButton9_Click(object sender, EventArgs e)//boton para enseñar todos los registros.
        {
            Form Form2= new Form2();
            this.Hide();
            Form2.ShowDialog();
            

        }
        private void iconButton7_Click(object sender, EventArgs e)//boton para limpiar.
        {
            textBox1.Text = "";
            textBox2.Text = "";
            textBox3.Text = "";
            textBox4.Text = "";
            comboBox1.SelectedIndex = -1;
            pictureBox2.Image = null;
        }
    }
}

