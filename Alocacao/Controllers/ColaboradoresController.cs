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

            #region queryBusca
            string queryBusca = @"
SELECT
	c.*,
	COALESCE(a.Numero, 0) AS NumeroProjetos 
FROM 
	colaboradores c
	LEFT JOIN(
		SELECT
			IdColaborador,
			COUNT(*) as Numero
		FROM
			Alocacao
		GROUP BY
			IdColaborador
	)a ON a.IdColaborador = c.Id
WHERE
    1 = 1
    {0}
ORDER BY C.Nome";
            #endregion

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
                    TempData["AddMessageError"]= "Data inicial inválida!";
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
                    TempData["AddMessageError"]= "Data Final inválida!";
                    return View("Index", "Colaboradores", new List<Colaborador>());
                }
                else
                {
                    clauses += String.Format(" AND CAST(c.DataContratacao AS DATE) <= '{0}' ", filtro.ContratadoAte.Value.ToString("yyyy-MM-dd"));
                }
            }

            if ((filtro.ContratadoDe != null && filtro.ContratadoAte != null) && (filtro.ContratadoAte.Value < filtro.ContratadoDe))
            {
                TempData["AddMessageError"]= "Intervalo de datas inválido!";
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
                            ProfilePic = "",
                            NumeroProjetos = int.Parse(reader["NumeroProjetos"].ToString())
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
            colaborador.ProjetosAlocados = new List<AlocacaoProjeto>();

            #region queryColaborador
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
	COALESCE(NumColab.ColaboradoresAlocados, 0) AS ColaboradoresAlocados,
	c.Nome AS NomeCliente,
    g.Nome AS NomeResponsavel
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

	LEFT JOIN Clientes c on c.Id = p.IdCliente
    LEFT JOIN Gestores g on g.Id = p.Responsavel
WHERE
	a.IdProjeto IS NULL";
            #endregion

            #region QueryAlocados
            string queryProjAloca = @"
SELECT DISTINCT
    p.*,
	COALESCE(NumColab.ColaboradoresAlocados, 0) AS ColaboradoresAlocados,
	c.Nome AS NomeCliente,
    g.Nome AS NomeResponsavel,
	CAST(NumColab.NumeroHoras AS INT) AS NumeroHoras,
	NumColab.DataInicio AS AlocacaoInicio,
	NumColab.DataFim AS AlocacaoFim
FROM 
    Projetos p
    LEFT JOIN(
        SELECT DISTINCT IdProjeto
		FROM alocacao
		WHERE IdColaborador = @IdColaborador		
	)a ON a.IdProjeto = p.Id

	LEFT JOIN (
		SELECT
			IdProjeto,
			COUNT(*) AS ColaboradoresAlocados,
			NumeroHoras,
			DataInicio,
			DataFim
		FROM 
			Alocacao
		GROUP BY
			IdProjeto,
			NumeroHoras,
			DataInicio,
			DataFim
	)NumColab ON  NumColab.IdProjeto = p.Id

	LEFT JOIN Clientes c on c.Id = p.IdCliente
    LEFT JOIN Gestores g on g.Id = p.Responsavel
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
                        new AlocacaoProjeto()
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
                            ColaboradoresAlocados = int.Parse(reader["ColaboradoresAlocados"].ToString()),
                            AlocacaoInicio = DateTime.Parse(reader["AlocacaoInicio"].ToString()),
                            AlocacaoFim = DateTime.Parse(reader["AlocacaoFim"].ToString()),
                            AlocacaoNumeroHoras = int.Parse(reader["NumeroHoras"].ToString())
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
                colaborador.ProjetosAlocados = new List<AlocacaoProjeto>();
            }
            finally
            {
                connection.Close();
            }

            return View(colaborador);
        }

        public ActionResult Alocar(int idColaborador, string[] projetos, DateTime dataInicio, DateTime dataFim, int numeroHoras,
           int idGestor = 1)
        {
            /*TODO add verificacoes
             Inicio <= fim
             intervalo de dias contém o numero de horas marcadas
             numeroHoras > 0
             colaborador possui horas disponiveis no periodo              
             */

            SqlConnection connection = new SqlConnection(connectionString);
            SqlCommand command = new SqlCommand() { Connection = connection };
            SqlTransaction transaction = null;

            #region queryInsert
            string queryInsert = @"
INSERT INTO Alocacao(
    Criado,
    DataInicio,
    DataFim,
    NumeroHoras,
    idColaborador,
    idGestor,
    idProjeto
)
VALUES(
    GETDATE(),
    @DataInicio,
    @DataFim,
    @NumeroHoras,
    @IdColaborador,
    @IdGestor,
    @IdProjeto
)";
            #endregion

            try
            {
                connection.Open();
                command.Transaction = connection.BeginTransaction();
                command.CommandText = queryInsert;

                foreach (string p in projetos)
                {
                    command.Parameters.Clear();

                    command.Parameters.AddWithValue("DataInicio", dataInicio);
                    command.Parameters.AddWithValue("DataFim", dataInicio);
                    command.Parameters.AddWithValue("NumeroHoras", numeroHoras);
                    command.Parameters.AddWithValue("IdColaborador", idColaborador);
                    command.Parameters.AddWithValue("IdGestor", idGestor);
                    command.Parameters.AddWithValue("IdProjeto", p);

                    command.ExecuteNonQuery();
                }

                command.Transaction.Commit();
                TempData["AddMessageSuccess"] = "Alocação efetuada!";
            }
            catch (Exception ex)
            {
                command.Transaction.Rollback();
                TempData["AddMessageError"] = "Erro ao realizar alocação!";
            }
            finally
            {
                connection.Close();
            }

            return RedirectToAction("Colaborador", new { id = idColaborador });
        }

        public ActionResult EditarAlocacao(int idColaborador, string[] projetos, DateTime dataInicio, DateTime dataFim, int numeroHoras,
   int idGestor = 1)
        {
            /*TODO add verificacoes
             Inicio <= fim
             intervalo de dias contém o numero de horas marcadas
             numeroHoras > 0
             colaborador possui horas disponiveis no periodo              
             */

            SqlConnection connection = new SqlConnection(connectionString);
            SqlCommand command = new SqlCommand() { Connection = connection };
            SqlTransaction transaction = null;

            #region queryUpdate
            string queryUpdate = @"
UPDATE 
    Alocacao
SET
    DataInicio  = @DataInicio,
    DataFim     = @DataFim,
    NumeroHoras = @NumeroHoras
WHERE
    IdColaborador = @IdColaborador
    AND IdProjeto IN({0})  
";
            #endregion

            try
            {
                connection.Open();
                command.Transaction = connection.BeginTransaction();
                command.CommandText = String.Format(queryUpdate, string.Join(",", projetos));

                command.Parameters.Clear();
                command.Parameters.AddWithValue("DataInicio", dataInicio);
                command.Parameters.AddWithValue("DataFim", dataInicio);
                command.Parameters.AddWithValue("NumeroHoras", numeroHoras);
                command.Parameters.AddWithValue("IdColaborador", idColaborador);
                command.ExecuteNonQuery();

                command.Transaction.Commit();
                TempData["AddMessageSuccess"] = "Alocação alterada!";
            }
            catch (Exception ex)
            {
                command.Transaction.Rollback();
                TempData["AddMessageError"] = "Erro ao editar alocação!";
            }
            finally
            {
                connection.Close();
            }

            return RedirectToAction("Colaborador", new { id = idColaborador });
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