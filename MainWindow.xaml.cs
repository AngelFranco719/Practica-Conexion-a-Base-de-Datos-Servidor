using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ConexionABD_2
{
    /// <summary>
    /// LÓGICA PARA LA CONEXIÓN A LA BASE DE DATOS Y MODIFICACIÓN DINÁMICA DE LA INTERFAZ.
    /// </summary>
    public partial class MainWindow : Window
    {
        //DEFINICIÓN DE VARIABLES GLOBALES. 
        SqlConnection ConexionSQL_Master; // Conexión a la Base de Datos MASTER del servidor.
        SqlConnection ConexionSQL; // Conexión a la Base de Datos seleccionada por el usuario.
        string nombre_BD; // Variable que guarda la Base de Datos Seleccionada.
        List<string> tablas_BD=new List<string>();  // Lista que guarda el nombre de todas las tablas de la Base de Datos.

        public MainWindow()
        {
            InitializeComponent();
            /// Haciendo conexión al Master del Servidor.
            string MiConexion = ConfigurationManager.ConnectionStrings["ConexionABD_2.Properties.Settings.masterConnectionString"].ConnectionString;
            ConexionSQL_Master = new SqlConnection(MiConexion);
            ConexionSQL_Master.Open();
            ObtenerBD();
        }
        #region Obtener las Bases de Datos Disponibles
        public void ObtenerBD()
        {
            SqlCommand Comando = new SqlCommand("SELECT name FROM sys.databases WHERE database_id>=5;",ConexionSQL_Master);
            SqlDataReader resultado = Comando.ExecuteReader();
            int index = 0; 
            while (resultado.Read())
            {
                Label Base_Datos = new Label();
                Base_Datos.Content = resultado.GetString(0);
                Base_Datos.Cursor = Cursors.Hand;
                Base_Datos.MouseDown += Label_MouseDown_BD; 
                Contenido_Expander_BD.Children.Add(Base_Datos); 
                index++; 
            }
        }
        #endregion

        #region Obtener las Tablas Disponibles de la Base de Datos.
        public void ObtenerTablas()
        {
            tablas_BD.Clear(); 
            string query = $"SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_TYPE='BASE TABLE' AND TABLE_CATALOG='{nombre_BD}';";
            SqlCommand query_command = new SqlCommand(query,ConexionSQL); 
            SqlDataReader resultado=query_command.ExecuteReader();
            while (resultado.Read())
            {
                GenerarLabel(resultado.GetString(0));
                tablas_BD.Add(resultado.GetString(0)); 
            }
            resultado.Close();
            /// Obtener las Vistas.
            string query1 = "SELECT TABLE_NAME FROM INFORMATION_SCHEMA.VIEWS;";
            SqlCommand sqlCommand = new SqlCommand(query1, ConexionSQL);
            SqlDataAdapter sqlDataAdapter = new SqlDataAdapter(sqlCommand);
            DataTable resultado1 = new DataTable();
            sqlDataAdapter.Fill(resultado1);
            GenerarLabelVistas(resultado1); 
        }
        #endregion

        #region Logica para hacer Expanders Dinamicos
        public void GenerarLabelVistas(DataTable resultado)
        {
            foreach (DataRow fila in resultado.Rows)
            {
                Label Vista_label = new Label();
                Vista_label.Cursor = Cursors.Hand; 
                Vista_label.Content = fila[0].ToString();
                Vista_label.MouseDown += Vista_label_MouseDown;
                StackPanel_OpcionesVistas.Children.Add(Vista_label);
            }
        }

        public void GenerarLabel(string Nombre_Tabla)
        {
            Label Opcion = new Label(); 
            Opcion.Content=Nombre_Tabla;
            StackPanel_Opciones.Children.Add(Opcion);
            Opcion.MouseDown += Label_MouseDown;
            Opcion.Cursor = Cursors.Hand;
        }

        public void Vista_label_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Label Seleccionado = (Label)sender;
            Expander_Vistas.IsExpanded = false;
            Expander_Vistas.Header = Seleccionado.Content;
            Grid.GetRow(Grid_Operacional);
            Grid_Fila1.Children.Clear();
        }

        public void Label_MouseDown(Object sender, MouseButtonEventArgs e)
        {
            Label Seleccionado = (Label)sender;
            Expander_Tablas.IsExpanded = false;
            Expander_Tablas.Header=Seleccionado.Content;
            Grid.GetRow(Grid_Operacional);
            Grid_Fila1.Children.Clear();
            Eliminar_TextBox.Text = $"ID_{Seleccionado.Content.ToString()}";
        }

        public void Label_MouseDown_BD(Object sender, MouseButtonEventArgs e)
        {
            Label Seleccionado=(Label)sender;
            Expander_BD.IsExpanded = false;
            Expander_BD.Header = Seleccionado.Content.ToString(); 
        }

        public void Label_MouseDown_Vista(Object sender, MouseButtonEventArgs e)
        {
            Label Seleccionado = (Label)sender;
            Expander_Tablas.IsExpanded = false;
            Expander_Tablas.Header = Seleccionado.Content;
            Grid.GetRow(Grid_Operacional);
            Grid_Fila1.Children.Clear();
            Eliminar_TextBox.Text = $"ID_{Seleccionado.Content.ToString()}";
        }
        #endregion 

        #region Mostrar los Datos de una Tabla
        private void Datos_Button_Click(object sender, RoutedEventArgs e)
        {
            Grid_Fila1.Children.Clear();
            if (Expander_Tablas.Header.ToString()!= "Selecciona una Tabla.")
            {
                string Tabla_seleccionada=Expander_Tablas.Header.ToString();
                string query = $"SELECT * FROM {Tabla_seleccionada};";
                SqlCommand query_command = new SqlCommand(query,ConexionSQL);
                DataTable Tabla = new DataTable();
                SqlDataAdapter resultados = new SqlDataAdapter(query_command);
                resultados.Fill(Tabla);
                DataView dataView = new DataView(Tabla);
                DataGrid Tabla_DataGrid = new DataGrid();
                Tabla_DataGrid.AutoGenerateColumns = true;
                Tabla_DataGrid.ItemsSource= dataView;
                Grid_Fila1.Children.Add(Tabla_DataGrid); 
            }
        }
        #endregion

        #region Mostrar la Descripción de una Tabla
        private void Descripcion_Button_Click(object sender, RoutedEventArgs e)
        {
            Grid_Fila1.Children.Clear();
            if (Expander_Tablas.Header.ToString()!="Selecciona una Tabla.")
            {
                string Tabla_seleccionada=Expander_Tablas.Header.ToString();
                string query = $"SELECT COLUMN_NAME, DATA_TYPE, IS_NULLABLE " +
                    $"FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME='{Tabla_seleccionada}';";
                SqlCommand query_command = new SqlCommand(query,ConexionSQL); 
                SqlDataAdapter adapter= new SqlDataAdapter(query_command);
                DataTable Tabla=new DataTable();
                adapter.Fill(Tabla);
                DataView dataView=new DataView(Tabla);
                DataGrid Tabla_Descripcion = new DataGrid();
                Tabla_Descripcion.AutoGenerateColumns = true;
                Tabla_Descripcion.ItemsSource= dataView;
                Grid_Fila1.Children.Add(Tabla_Descripcion);
            }
        }
        #endregion

        #region Insertar Datos en una Tabla
        private void Insertar_Datos_Button_Click(object sender, RoutedEventArgs e)
        {
            Grid_Fila1.Children.Clear();
            if (Expander_Tablas.Header.ToString() != "Selecciona una Tabla.")
            {
                string Tabla_seleccionada = Expander_Tablas.Header.ToString();
                string query = $"SELECT COLUMN_NAME, DATA_TYPE FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME='{Tabla_seleccionada}';";
                SqlCommand Comando = new SqlCommand(query,ConexionSQL); 
                SqlDataAdapter adapter= new SqlDataAdapter(Comando);
                DataTable Tabla_resultado = new DataTable();
                adapter.Fill(Tabla_resultado);
                /// Instancia de una Clase tipo UserControl para la inserción de Datos.
                InsercionDatos Ventana_insercion = new InsercionDatos(Tabla_resultado, Tabla_seleccionada, ConexionSQL);  
                Grid_Fila1.Children.Add(Ventana_insercion);
            }
        }
        #endregion

        #region Eliminar una Tupla de una Tabla
        private void Eliminar_Button_Click(object sender, RoutedEventArgs e)
        {
            if(Expander_Tablas.Header.ToString()!= "Selecciona una Tabla.")
            {
                string query = $"DELETE FROM {Expander_Tablas.Header.ToString()} WHERE ID_{Expander_Tablas.Header}={int.Parse(Eliminar_TextBox.Text)}";
                SqlCommand sqlCommand = new SqlCommand(query, ConexionSQL);
                try
                {
                    sqlCommand.ExecuteNonQuery();
                    MessageBox.Show("Base de Datos Actualizada.");
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.ToString());
                }
            }            
        }
        #endregion

        #region Mostrar Tuplas de una Vista
        private void Vistas_Datos_Button_Click(object sender, RoutedEventArgs e)
        {
            if(Expander_Vistas.Header.ToString()!= "Vistas Disponibles.")
            {
                string query = $"SELECT *FROM {Expander_Vistas.Header.ToString()};";
                SqlCommand sqlCommand = new SqlCommand(query,ConexionSQL);
                SqlDataAdapter sqlDataAdapter=new SqlDataAdapter(sqlCommand);
                DataTable Tabla = new DataTable();
                sqlDataAdapter.Fill(Tabla); 
                DataView dataView = new DataView(Tabla);
                DataGrid Tabla_Descripcion = new DataGrid();
                Tabla_Descripcion.AutoGenerateColumns = true;
                Tabla_Descripcion.ItemsSource = dataView;
                Grid_Fila1.Children.Add(Tabla_Descripcion);
            }
        }
        #endregion

        #region Conectar con la Base de Datos Seleccionada
        private void Conectar_Button_Click(object sender, RoutedEventArgs e)
        {
            if(Expander_BD.Header.ToString()!= "Bases de Datos Disponibles.")
            {
                try
                {
                    StackPanel_Opciones.Children.Clear();
                    StackPanel_OpcionesVistas.Children.Clear(); 
                    nombre_BD = Expander_BD.Header.ToString();
                    string connectionString = $"Data Source=LAPTOP-47THKE4K;Initial Catalog={nombre_BD};User ID=sa;Password=root;";
                    ConexionSQL = new SqlConnection(connectionString);
                    ConexionSQL.Open();
                    MessageBox.Show("Se ha conectado correctamente con la Base de Datos.");
                    ObtenerTablas(); 
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.ToString()); 
                }
            } 
        }
        #endregion

        #region Lógica para Conectar el Servidor con el Cliente.

        private void Servidor_Button_Click(object sender, RoutedEventArgs e)
        {
            Servidor Nueva_Conexion = new Servidor();
            Nueva_Conexion.BDActual(Expander_BD.Header.ToString());
            Nueva_Conexion.Mandar_Tablas(tablas_BD);
            string tabla_Seleccionada = Nueva_Conexion.RecibirTablaSeleccionada(); 
            string query = $"SELECT COUNT(COLUMN_NAME) FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME='{tabla_Seleccionada}';";
            SqlCommand command = new SqlCommand(query,ConexionSQL);
            int cantidad_columnas = (int)command.ExecuteScalar();
            Nueva_Conexion.MandarCantidadColumnas(cantidad_columnas);

            string query_1 = $"SELECT COLUMN_NAME FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME='{tabla_Seleccionada}';";
            SqlCommand command_1 = new SqlCommand(query_1,ConexionSQL);
            SqlDataAdapter adapter = new SqlDataAdapter(command_1);
            DataTable columnas = new DataTable();
            adapter.Fill(columnas);
            Nueva_Conexion.MandarColumnas(columnas);
            string insert_query = Nueva_Conexion.RecibirQuery();
            MessageBox.Show(insert_query); 
            try
            {
                SqlCommand insert = new SqlCommand(insert_query, ConexionSQL);
                insert.ExecuteNonQuery();
                MessageBox.Show("Se ha agregado la tupla del cliente");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        #endregion
    }
}
