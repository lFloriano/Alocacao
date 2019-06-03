using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data.SqlClient;
using Alocacao.Models.Colaboradores;
using Alocacao.Models.Projetos;
using System.Configuration;

namespace Alocacao.Controllers
{
    public class ProjetosController : Controller
    {
        static string connectionString = ConfigurationManager.ConnectionStrings["ConnString"].ConnectionString;

        public ActionResult Index()
        {
            return BuscarProjetos(new FiltroProjeto());
        }

        private ActionResult BuscarProjetos(FiltroProjeto filtro)
        {
            SqlConnection connection = new SqlConnection(connectionString);
            SqlCommand command = new SqlCommand() { Connection = connection };
            List<Projeto> listaProjetos = new List<Projeto>();

            string clauses = "";

            #region queryBusca
            string queryBusca = @"
SELECT
	p.Id,
	p.Nome,
	p.Descricao,
	p.Responsavel,
	g.Nome AS NomeResponsavel,
	p.DataInicio,
	p.DataFim,
	p.IdCliente,
	c.Nome AS NomeCliente,
	p.StatusProjeto,
	COALESCE(a.numero, 0) AS ColaboradoresAlocados
FROM
	projetos p
	LEFT JOIN Clientes c ON c.Id = p.IdCliente
	LEFT JOIN Gestores g ON g.Id = p.Responsavel
	LEFT JOIN(
		SELECT
			idProjeto,
			COUNT(*) AS Numero
		FROM
			alocacao
		GROUP BY
			idProjeto
	)a ON a.IdProjeto = p.Id
WHERE
    1 = 1
    {0}
ORDER BY 
    p.Nome";
            #endregion

            #region Filtro
            if (!String.IsNullOrWhiteSpace(filtro.Nome))
            {
                clauses += String.Format(" AND p.Nome LIKE '%{0}%' ", filtro.Nome.Trim());
            }

            //Validacao de data
            if (filtro.DataInicio != null)
            {
                if (!ValidarDate(filtro.DataInicio.Value))
                {
                    TempData["AddMessageError"] = "Data inicial inválida!";
                    return View("Index", "Colaboradores", new List<Colaborador>());
                }
                else
                {
                    clauses += String.Format(" AND CAST(p.DataInicio AS DATE) >= '{0}' ", filtro.DataInicio.Value.ToString("yyyy-MM-dd"));
                }
            }

            if (filtro.DataFim != null)
            {
                if (!ValidarDate(filtro.DataFim.Value))
                {
                    TempData["AddMessageError"] = "Data Final inválida!";
                    return View("Index", "Colaboradores", new List<Colaborador>());
                }
                else
                {
                    clauses += String.Format(" AND CAST(p.DataFim AS DATE) <= '{0}' ", filtro.DataFim.Value.ToString("yyyy-MM-dd"));
                }
            }

            if ((filtro.DataInicio != null && filtro.DataFim != null) && (filtro.DataFim.Value < filtro.DataInicio))
            {
                TempData["AddMessageError"] = "Intervalo de datas inválido!";
                return View("Index", new List<Colaborador>());
            }

            //TODO: DE-ATE NumeroProjetos
            #endregion

            try
            {
                connection.Open();
                command.CommandText = String.Format(queryBusca, clauses);
                SqlDataReader reader = command.ExecuteReader();

                while (reader.Read())
                {
                    listaProjetos.Add(
                        new Projeto()
                        {
                            Id = int.Parse(reader["Id"].ToString()),
                            Nome = reader["Nome"].ToString(),
                            Descricao = reader["Descricao"].ToString(),
                            IdResponsavel = int.Parse(reader["Responsavel"].ToString()),
                            NomeResponsavel = reader["NomeResponsavel"].ToString(),
                            DataInicio = DateTime.Parse(reader["DataInicio"].ToString()),
                            DataFim = DateTime.Parse(reader["DataFim"].ToString()),
                            IdCliente = int.Parse(reader["IdCliente"].ToString()),
                            NomeCliente = reader["NomeCliente"].ToString(),
                            StatusProjeto = reader["StatusProjeto"].ToString(),
                            ColaboradoresAlocados = int.Parse(reader["ColaboradoresAlocados"].ToString())
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

            return View("Index", listaProjetos);
        }

        private bool ValidarDate(DateTime date)
        {
            bool retorno = true;

            if (date.Year > 2100 || date.Year < 1900)
            {
                retorno = false;
            }

            return retorno;
        }
    }



    public class FiltroProjeto
    {
        public string Nome { get; set; }
        public string Descricao { get; set; }
        public string NomeResponsavel { get; set; }
        public DateTime? DataInicio { get; set; }
        public DateTime? DataFim { get; set; }
        public int IdCliente { get; set; }
        public string NomeCliente { get; set; }
        public string StatusProjeto { get; set; }
        public int ColaboradoresAlocados { get; set; }
    }
}