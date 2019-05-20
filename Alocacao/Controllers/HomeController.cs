using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data.SqlClient;
using Alocacao.Models.Colaboradores;

namespace Alocacao.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Colaboradores()
        {
            SqlConnection connection = new SqlConnection();
            SqlCommand command = new SqlCommand() { Connection = connection };
            List<Colaborador> listaColaboradores = new List<Colaborador>();

            #region queryBusca
            string queryBusca = @"";
            #endregion

            try
            {
                connection.Open();
                command.CommandText = queryBusca;
                SqlDataReader reader = command.ExecuteReader();

                while (reader.Read())
                {
                    listaColaboradores.Add(
                        new Colaborador() { }
                    );
                }

            }
            catch(Exception ex)
            {

            }
            finally
            {
                connection.Close();
            }

            return View(listaColaboradores);
        }
    }
}