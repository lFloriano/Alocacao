using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Alocacao.Models.Colaboradores;
using Alocacao.Models.Projetos;
using System.Data.SqlClient;
using System.Configuration;

namespace Alocacao.Controllers
{
    public class ColaboradoresController : Controller
    {
        static string connectionString = ConfigurationManager.ConnectionStrings["ConnString"].ConnectionString;

        public ActionResult Index()
        {
            return BuscarColaboradores(new FiltroColaborador());
        }

        /*
         nome
         numeroproj
         cargo
         dpto
         dtcont ini
         dtcont ate
             */

        
        public ActionResult Filtrar(FiltroColaborador filtro)
        {
            ViewBag.Nome = filtro.Nome ?? "";
            ViewBag.NumeroProjetos = (filtro.NumeroProjetos != null) ? filtro.NumeroProjetos.ToString() : "";
            ViewBag.Cargo = filtro.Cargo?? "";
            ViewBag.Departamento = filtro.Departamento ?? "";
            ViewBag.ContratacaoDe = filtro.ContratadoDe != null ? filtro.ContratadoDe.Value.ToString("yyyy-MM-dd") : "";
            ViewBag.ContratacaoAte = filtro.ContratadoAte != null ? filtro.ContratadoAte.Value.ToString("yyyy-MM-dd") : "";

            return BuscarColaboradores(filtro);
        }

        private ActionResult BuscarColaboradores(FiltroColaborador filtro)
        {
            SqlConnection connection = new SqlConnection(connectionString);
            SqlCommand command = new SqlCommand() { Connection = connection };
            List<Colaborador> listaColaboradores = new List<Colaborador>();

            string clauses = "";
            string queryBusca = @"SELECT * FROM Colaboradores c WHERE 1 = 1 {0} ORDER BY C.Nome";            

            #region Filtro
            if (!String.IsNullOrWhiteSpace(filtro.Nome))
            {
                clauses += String.Format(" AND c.Nome LIKE '%{0}%' ", filtro.Nome.Trim());
            }

            if (!String.IsNullOrWhiteSpace(filtro.Cargo))
            {
                clauses += String.Format(" AND c.Cargo LIKE '%{0}%' ", filtro.Cargo.Trim());
            }

            if (!String.IsNullOrWhiteSpace(filtro.Departamento))
            {
                clauses += String.Format(" AND c.Departamento LIKE '%{0}%' ", filtro.Departamento.Trim());
            }

            //Validacao de data
            if(filtro.ContratadoDe != null)
            {
                if (!ValidarDate(filtro.ContratadoDe.Value))
                {
                    ViewBag.AddMessageError = "Data inicial inválida!";
                    return View("Index", "Colaboradores", new List<Colaborador>());
                }
                else
                {
                    clauses += String.Format(" AND CAST(c.DataContratacao AS DATE) >= '{0}' ", filtro.ContratadoDe.Value.ToString("yyyy-MM-dd"));
                }
            }
            
            if (filtro.ContratadoAte != null)
            {
                if (!ValidarDate(filtro.ContratadoAte.Value))
                {
                    ViewBag.AddMessageError = "Data Final inválida!";
                    return View("Index", "Colaboradores", new List<Colaborador>());
                }
                else
                {
                    clauses += String.Format(" AND CAST(c.DataContratacao AS DATE) <= '{0}' ", filtro.ContratadoAte.Value.ToString("yyyy-MM-dd"));
                }
            }

            if ((filtro.ContratadoDe != null && filtro.ContratadoAte != null) && (filtro.ContratadoAte.Value < filtro.ContratadoDe))
            {
                ViewBag.AddMessageError = "Intervalo de datas inválido!";
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

            return View("Index", listaColaboradores);
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

        public ActionResult Colaborador(int id)
        {
            SqlConnection connection = new SqlConnection(connectionString);
            SqlCommand command = new SqlCommand() { Connection = connection };
            ColaboradorProjetos colaborador = new ColaboradorProjetos();
            colaborador.ProjetosDisponiveis = new List<Projeto>();
            colaborador.ProjetosAlocados = new List<Projeto>();

            #region queryColaboradro
            string queryColaborador = @"
SELECT TOP 1
	c.*,
	(SELECT COUNT(*) FROM Alocacao WHERE IdColaborador = @IdColaborador) AS NumeroProjetos
FROM 
	Colaboradores c 
WHERE 
	c.Id = @IdColaborador";
            #endregion

            #region QueryDisponiveis
            string queryProjDisp = @"
SELECT DISTINCT
    p.*,
	NumColab.ColaboradoresAlocados
FROM 
    Projetos p
    LEFT JOIN(
        SELECT DISTINCT IdProjeto
		FROM alocacao
		WHERE IdColaborador = @IdColaborador		
	)a ON a.IdProjeto = p.Id

	LEFT JOIN (
		SELECT IdProjeto, COUNT(*) AS ColaboradoresAlocados
		FROM Alocacao
		GROUP BY IdProjeto
	)NumColab ON  NumColab.IdProjeto = p.Id
WHERE
	a.IdProjeto IS NULL";
            #endregion

            #region QueryAlocados
            string queryProjAloca = @"
SELECT DISTINCT
    p.*,
	NumColab.ColaboradoresAlocados
FROM 
    Projetos p
    LEFT JOIN(
        SELECT DISTINCT IdProjeto
		FROM alocacao
		WHERE IdColaborador = @IdColaborador		
	)a ON a.IdProjeto = p.Id

	LEFT JOIN (
		SELECT IdProjeto, COUNT(*) AS ColaboradoresAlocados
		FROM Alocacao
		GROUP BY IdProjeto
	)NumColab ON  NumColab.IdProjeto = p.Id
WHERE
	a.IdProjeto IS NOT NULL";
            #endregion

            try
            {
                connection.Open();

                #region colaborador
                command.CommandText = queryColaborador;
                command.Parameters.AddWithValue("IdColaborador", id);
                SqlDataReader reader = command.ExecuteReader();

                while (reader.Read())
                {
                    colaborador.Colaborador = new Colaborador()
                    {
                        Id = int.Parse(reader["Id"].ToString()),
                        Nome = reader["Nome"].ToString(),
                        Cargo = reader["Cargo"].ToString(),
                        Departamento = reader["Departamento"].ToString(),
                        DataContratacao = DateTime.Parse(reader["DataContratacao"].ToString()),
                        NumeroProjetos = int.Parse(reader["NumeroProjetos"].ToString()),
                        ProfilePic = "" //TODO: ADD IMAGENS
                    };
                }

                command.Parameters.Clear();
                reader.Close();
                #endregion

                #region projetosDisponiveis
                command.CommandText = queryProjDisp;
                command.Parameters.AddWithValue("IdColaborador", id);
                reader = command.ExecuteReader();
                while (reader.Read())
                {
                    colaborador.ProjetosDisponiveis.Add(
                        new Projeto()
                        {
                            Id = int.Parse(reader["Id"].ToString()),
                            Nome = reader["Nome"].ToString(),
                            Descricao = reader["Descricao"].ToString(),
                            IdResponsavel = int.Parse(reader["IdResponsavel"].ToString()),
                            DataInicio = DateTime.Parse(reader["DataInicio"].ToString()),
                            DataFim = DateTime.Parse(reader["DataFim"].ToString()),
                            IdCliente = int.Parse(reader["IdCliente"].ToString()),
                            NomeCliente = reader["NomeCliente"].ToString(),
                            StatusProjeto = reader["StatusProjeto"].ToString(),
                            ColaboradoresAlocados = int.Parse(reader["ColaboradoresAlocados"].ToString())
                        }    
                    );
                }
                command.Parameters.Clear();
                reader.Close();

                #endregion

                #region projetosAlocados
                command.CommandText = queryProjAloca;
                command.Parameters.AddWithValue("IdColaborador", id);
                reader = command.ExecuteReader();
                while (reader.Read())
                {
                    colaborador.ProjetosAlocados.Add(
                        new Projeto()
                        {
                            Id = int.Parse(reader["Id"].ToString()),
                            Nome = reader["Nome"].ToString(),
                            Descricao = reader["Descricao"].ToString(),
                            IdResponsavel = int.Parse(reader["IdResponsavel"].ToString()),
                            DataInicio = DateTime.Parse(reader["DataInicio"].ToString()),
                            DataFim = DateTime.Parse(reader["DataFim"].ToString()),
                            IdCliente = int.Parse(reader["IdCliente"].ToString()),
                            NomeCliente = reader["NomeCliente"].ToString(),
                            StatusProjeto = reader["StatusProjeto"].ToString(),
                            ColaboradoresAlocados = int.Parse(reader["ColaboradoresAlocados"].ToString())
                        }
                    );
                }
                command.Parameters.Clear();
                reader.Close();

                #endregion
            }
            catch (Exception ex)
            {
                colaborador.Colaborador = new Colaborador();
                colaborador.ProjetosDisponiveis = new List<Projeto>();
                colaborador.ProjetosAlocados = new List<Projeto>();
            }
            finally
            {
                connection.Close();
            }

            return View(colaborador);
        }
    }

    public class FiltroColaborador
    {
        public string Nome { get; set; }
        public int? NumeroProjetos { get; set; }
        public string Cargo { get; set; }
        public string Departamento { get; set; }
        public DateTime? ContratadoDe { get; set; }
        public DateTime? ContratadoAte { get; set; }
    }
}