using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Alocacao.Models.Colaboradores;
using System.Data.SqlClient;
using System.Configuration;

namespace Alocacao.Controllers
{
    public class ColaboradoresController : Controller
    {
        static string connectionString = ConfigurationManager.ConnectionStrings["ConnString"].ConnectionString;

        public ActionResult Index()
        {

            return View(BuscarColaboradores());
        }

        private List<Colaborador> BuscarColaboradores()
        {
            SqlConnection connection = new SqlConnection(connectionString);
            SqlCommand command = new SqlCommand() { Connection = connection };
            List<Colaborador> listaColaboradores = new List<Colaborador>();

            string filtro = "";
            string queryBusca = @"SELECT * FROM Colaboradores c WHERE 1 = 1 {0} ORDER BY C.Nome";

            try
            {
                connection.Open();
                command.CommandText = String.Format(queryBusca, filtro);
                SqlDataReader reader = command.ExecuteReader();

                while (reader.Read())
                {
                    listaColaboradores.Add(
                        new Colaborador() {
                            Id = int.Parse(reader["Id"].ToString()),
                            Nome = reader["Nome"].ToString(),
                            Cargo = reader["Cargo"].ToString(),
                            Departamento = reader["Departamento"].ToString(),
                            DataContratacao = DateTime.Parse(reader["DataContratacao"].ToString()),
                            ProfilePic = ""
                        }
                    );
                }

            }
            catch (Exception ex)
            {

            }
            finally
            {
                connection.Close();
            }

            return listaColaboradores;
        }
    }
}