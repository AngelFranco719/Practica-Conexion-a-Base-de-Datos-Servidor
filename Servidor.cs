using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace ConexionABD_2
{
    /// <summary>
    /// LÓGICA PARA CREAR UN SERVIDOR Y CONECTARLO A UN CLIENTE MEDIANTE SOCKETS Y EL PROTOCOLO TCP-IP.
    /// </summary>
    public class Servidor
    {
        /// DEFINICIÓN DE VARIABLES GLOBALES.
        Socket Conexion = new Socket(AddressFamily.InterNetwork,SocketType.Stream, ProtocolType.Tcp); /// Socket principal.
        IPAddress IP_Servidor = IPAddress.Parse("192.168.137.1"); // Dirección IP del servidor.
        int Puerto_Servidor = 3000; // Puerto que estará disponible para recibir información.
        IPEndPoint Servidor_EP; // Creación de un EndPoint (Servidor)
        Socket Conexion_Establecida; // Socket que recibirá y mandará datos por la red.
        int cantidad_columnas;

        #region Constructor de la Clase Servidor: Inicia la escucha a una nueva conexión y la establece si es posible.
        public Servidor() 
        {
            Servidor_EP = new IPEndPoint(IP_Servidor, Puerto_Servidor);
            try
            {
                Conexion.Bind(Servidor_EP);
                Conexion.Listen(5);
                MessageBox.Show("Esperando una conexión.");

                Conexion_Establecida = Conexion.Accept();
                MessageBox.Show("Conexión Establecida"); 

            } catch(Exception e)
            {
                MessageBox.Show(e.ToString()); 
            }
        }
        #endregion

        #region Método que informa al cliente a qué base de datos está conectada.
        public void BDActual(string BD)
        {
            byte [] Mensaje=Encoding.UTF8.GetBytes(BD);
            Conexion_Establecida.Send(Mensaje);          
        }
        #endregion

        #region Método que se encarga de Mandar las Tablas disponibles al cliente
        public void Mandar_Tablas(List<string>Tablas)
        {
            int cantidad_tablas = Tablas.Count;
            MessageBox.Show($"Mandar {cantidad_tablas} tablas"); 
            byte[] Buffer = BitConverter.GetBytes(cantidad_tablas);
            Conexion_Establecida.Send(Buffer);
            foreach(string s in Tablas)
            {
                byte[] Buffer_Tabla=Encoding.UTF8.GetBytes(s);
                Conexion_Establecida.Send(Buffer_Tabla);
                MessageBox.Show($"Se ha enviado la tabla {s}"); 
                byte[] Buffer_confirmacion = new byte[1];
                int bytes_confirmacion = Conexion_Establecida.Receive(Buffer_confirmacion);
                bool confirmacion_recibida = (Buffer_confirmacion[0] == 1);
            }
        }
        #endregion

        #region Método que se encarga de recibir la Sentencia SQL para insertar Datos.
        public string RecibirQuery()
        {
            byte[] buffer = new byte[1024];
            int bytes_cantidad = Conexion_Establecida.Receive(buffer);
            string query = Encoding.UTF8.GetString(buffer, 0, bytes_cantidad);
            return query; 
        }
        #endregion

        #region Método que se encarga de Recibir la Tabla que el Cliente ha seleccionado para insertar Datos.
        public string RecibirTablaSeleccionada()
        {
            byte[] buffer = new byte[1024];
            int bytes_cantidad = Conexion_Establecida.Receive(buffer);
            string Tabla_seleccionada = Encoding.UTF8.GetString(buffer, 0, bytes_cantidad);
            MessageBox.Show($"El cliente ha seleccionado la tabla {Tabla_seleccionada}");
            byte[] buffer_confirmacion = { 1 };
            Conexion_Establecida.Send(buffer_confirmacion);
            return Tabla_seleccionada; 
        }
        #endregion

        #region Método que se encarga de Mandar las columnas de la tabla seleccionada al cliente.
        public void MandarColumnas(DataTable Columnas)
        {
            MessageBox.Show(Columnas.Rows.Count.ToString()); 
            foreach (DataRow Row in Columnas.Rows)
            {
                string columna = Row[0].ToString();  
                byte[] buffer = Encoding.UTF8.GetBytes(columna);
                Conexion_Establecida.Send(buffer);
                MessageBox.Show($"Se ha mandado la columna {columna}"); 
                byte[] Buffer_confirmacion = new byte[1];
                int bytes_confirmacion = Conexion_Establecida.Receive(Buffer_confirmacion);
                bool confirmacion_recibida = (Buffer_confirmacion[0] == 1);
            }
        }
        #endregion

        #region Método que se encarga de mandar al Cliente la cantidad de columnas disponibles en una tabla.
        public void MandarCantidadColumnas(int Cantidad)
        {
            string cantidad_columnas = Cantidad.ToString();
            MessageBox.Show("Cantidad columnas 1: " + cantidad_columnas.ToString());
            byte[]buffer=Encoding.UTF8.GetBytes(cantidad_columnas);
            Conexion_Establecida.Send(buffer);
        }
        #endregion

    }
}
