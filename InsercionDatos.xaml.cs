using System;
using System.Collections.Generic;
using System.Data;
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
    /// Lógica de interacción para InsercionDatos.xaml
    /// </summary>
    public partial class InsercionDatos : UserControl
    {
        List<TextBox> textBoxes = new List<TextBox>();
        string Tabla_seleccionada;
        SqlConnection ConexionSQL; 
        int numero_columnas=0;
        string query;

        #region Lógica para generar una ventana dinámica para la inserción de datos.
        public InsercionDatos(DataTable Tabla, string Nombre_Tabla, SqlConnection ConexionSQL)
        {
            InitializeComponent();
            Label_Titulo.Content = $"Inserción de Datos para la Tabla: {Nombre_Tabla}";
            Tabla_seleccionada = Nombre_Tabla; 
            foreach(DataRow Fila in Tabla.Rows)
            {
                if (numero_columnas == 0)
                {
                    numero_columnas++; 
                    continue; 
                }
                Label Label_Contenido = new Label();
                Label_Contenido.Content = $"{Fila[0]}";
                Label_Contenido.HorizontalAlignment= HorizontalAlignment.Center;
                TextBox TextBox_Contenido = new TextBox(); 
                TextBox_Contenido.HorizontalAlignment= HorizontalAlignment.Center;
                TextBox_Contenido.VerticalAlignment= VerticalAlignment.Top;
                TextBox_Contenido.Width = 200;
                TextBox_Contenido.Height = 20;
                StackPanel_principal.Children.Add(Label_Contenido);
                StackPanel_principal.Children.Add(TextBox_Contenido);
                textBoxes.Add(TextBox_Contenido);
                numero_columnas++;
                this.ConexionSQL = ConexionSQL; 
            }
        }
        #endregion

        #region Retornar el Control de Usuario
        public UserControl getInsercionDatos()
        {
            return this;
        }
        #endregion

        #region Generar la Query
        public void Generar_Query_Button_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Generando Query"); 
            int index = 1; 
            query = $"INSERT INTO {Tabla_seleccionada} VALUES(";
            foreach (TextBox a in textBoxes)
            {
                try
                {
                    if (numero_columnas - 1 != index) query += int.Parse(a.Text.ToString()) + ",";
                    else query += int.Parse(a.Text.ToString()) + ");"; 
                }
                catch
                {
                    try
                    {
                        if (numero_columnas - 1 != index) query += double.Parse(a.Text.ToString()) + ",";
                        else query += double.Parse(a.Text.ToString()) + ");";
                    }
                    catch
                    {
                        if (numero_columnas - 1 != index) query += "'"+a.Text.ToString() + "'" + ",";
                        else query += "'" + a.Text.ToString() + "'" + ");";
                    }
                } 
                index++; 
            }
            TextBox_Query.Text= query;
            TextBox_Query.IsEnabled = true; 
            MessageBox.Show(query); 
        }
        #endregion

        #region Hacer la consulta a la Base de Datos
        private void Insertar_Datos_Query_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                SqlCommand query_command = new SqlCommand(query, ConexionSQL);
                query_command.ExecuteNonQuery();
                MessageBox.Show("Se ha actualizado la Base de Datos"); 
            } catch(Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
            
        }
        #endregion
    }
}
