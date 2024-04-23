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
    public class Servidor
    {
        Socket Conexion = new Socket(AddressFamily.InterNetwork,SocketType.Stream, ProtocolType.Tcp);
        IPAddress IP_Servidor = IPAddress.Parse("192.168.137.1");
        int Puerto_Servidor = 3000;
        IPEndPoint Servidor_EP;
        Socket Conexion_Establecida;
        int cantidad_columnas; 
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
        public void BDActual(string BD)
        {
            byte [] Mensaje=Encoding.UTF8.GetBytes(BD);
            Conexion_Establecida.Send(Mensaje);          
        }
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

        public string RecibirQuery()
        {
            byte[] buffer = new byte[1024];
            int bytes_cantidad = Conexion_Establecida.Receive(buffer);
            string query = Encoding.UTF8.GetString(buffer, 0, bytes_cantidad);
            return query; 
        }

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

        public void MandarCantidadColumnas(int Cantidad)
        {
            
            string cantidad_columnas = Cantidad.ToString();
            MessageBox.Show("Cantidad columnas 1: " + cantidad_columnas.ToString());
            byte[]buffer=Encoding.UTF8.GetBytes(cantidad_columnas);
            Conexion_Establecida.Send(buffer);
        }

    }
}
