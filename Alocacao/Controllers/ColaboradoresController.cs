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

        public ActionResult Colaborador(int id, string tempType = null, string tempText = null)
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
	NumColab.DataFim AS AlocacaoFim,
    NumColab.AlocacaoId
FROM 
    Projetos p
    LEFT JOIN(
        SELECT DISTINCT IdProjeto
		FROM alocacao
		WHERE IdColaborador = @IdColaborador		
	)a ON a.IdProjeto = p.Id

	LEFT JOIN (
		SELECT
            Id AS AlocacaoId,
			IdProjeto,
			COUNT(*) AS ColaboradoresAlocados,
			NumeroHoras,
			DataInicio,
			DataFim
		FROM 
			Alocacao
        WHERE 
            IdColaborador = @IdColaborador	
		GROUP BY
            Id,
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
                            AlocacaoNumeroHoras = int.Parse(reader["NumeroHoras"].ToString()),
                            AlocacaoId = int.Parse(reader["AlocacaoId"].ToString())
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

            //creative technical solution for tempData
            if(tempType != null)
            {
                if(tempType.Equals("success"))
                {
                    TempData["AddMessageSuccess"] = "Alteração efetuada!";
                }
                else
                {
                    TempData["AddMessageError"] = "Erro ao efetuar alteração!";
                }
            }

            return View(colaborador);
        }

        public ActionResult Alocar(int idColaborador, string[] projetos, DateTime dataInicio, DateTime dataFim, int numeroHoras,
           int idGestor = 1)
        {
            #region Validacoes

            int horasIntervalo = 0;
            //horasIntervalo = (dataFim - dataInicio).Hours;
            horasIntervalo = Convert.ToInt32((dataFim - dataInicio).TotalHours);


            if (projetos == null || projetos.Length <= 0)
            {
                TempData["AddMessageError"] = "Informe os projetos para alocação!";
                return RedirectToAction("Colaborador", "Colaboradores", new { id = idColaborador });
            }

            if (horasIntervalo < numeroHoras)
            {
                TempData["AddMessageError"] = "O período selecionado não comporta o número de horas!";
                return RedirectToAction("Colaborador", "Colaboradores", new { id = idColaborador });
            }

            if (numeroHoras <= 0)
            {
                TempData["AddMessageError"] = "Informe um número de horas positivo!";
                return RedirectToAction("Colaborador", "Colaboradores", new { id = idColaborador });
            }

            if (ValidarDate(dataInicio) == false || ValidarDate(dataFim) == false)
            {
                TempData["AddMessageError"] = "As datas devem estar num intervalo válido (1900 - 2100)!";
                return RedirectToAction("Colaborador", "Colaboradores", new { id = idColaborador });
            }

            if (dataFim < dataInicio)
            {
                TempData["AddMessageError"] = "Data final deve suceder data inicial!";
                return RedirectToAction("Colaborador", "Colaboradores", new { id = idColaborador });
            }
            #endregion

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
            #region Validacoes

            int horasIntervalo = 0;
            horasIntervalo = Convert.ToInt32((dataFim - dataInicio).TotalHours);

            if (projetos == null || projetos.Length <= 0)
            {
                TempData["AddMessageError"] = "Informe os projetos para alocação!";
                return RedirectToAction("Colaborador", "Colaboradores", new { id = idColaborador });
            }

            if (horasIntervalo < numeroHoras)
            {
                TempData["AddMessageError"] = "O período selecionado não comporta o número de horas!";
                return RedirectToAction("Colaborador", "Colaboradores", new { id = idColaborador });
            }

            if (numeroHoras <= 0)
            {
                TempData["AddMessageError"] = "Informe um número de horas positivo!";
                return RedirectToAction("Colaborador", "Colaboradores", new { id = idColaborador });
            }

            if (ValidarDate(dataInicio) == false || ValidarDate(dataFim) == false)
            {
                TempData["AddMessageError"] = "As datas devem estar num intervalo válido (1900 - 2100)!";
                return RedirectToAction("Colaborador", "Colaboradores", new { id = idColaborador });
            }

            if (dataFim < dataInicio)
            {
                TempData["AddMessageError"] = "Data final deve suceder data inicial!";
                return RedirectToAction("Colaborador", "Colaboradores", new { id = idColaborador });
            }
            #endregion

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
    AND Id IN ({0})  
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

        public ActionResult ExcluirAlocacao(int idColaborador, string[] alocacoes)
        {
            SqlConnection connection = new SqlConnection(connectionString);
            SqlCommand command = new SqlCommand() { Connection = connection };
            SqlTransaction transaction = null;
            Alert resultadoTempData = new Alert();

            #region queryHistorico
            string queryHistorico = @"
INSERT INTO Alocacao_Historico(
	IdAlocacao,
	CriadoAlocacao,
	IdColaborador,
	IdProjeto,
	IdGestor,
	NumeroHoras,
	DataInicio,
	DataFim
)
(
SELECT
	Id, Criado, Idcolaborador, IdProjeto, IdGestor, NumeroHoras, DataInicio, DataFim
FROM
	Alocacao
WHERE 
    Id IN({0})
)";
            #endregion

            string queryDelete = String.Format("DELETE FROM Alocacao WHERE Id IN({0})", String.Join(",", alocacoes));

            try
            {
                connection.Open();
                command.Transaction = connection.BeginTransaction();

                #region AddHistorico
                command.CommandText = String.Format(queryHistorico, String.Join(",", alocacoes));
                command.ExecuteNonQuery();
                #endregion

                #region excluir
                command.CommandText = queryDelete;
                command.ExecuteNonQuery();
                #endregion


                command.Transaction.Commit();

                resultadoTempData.Text = "Alocação excluida!";
                resultadoTempData.Type = "success";
            }
            catch (Exception ex)
            {
                resultadoTempData.Text = "Erro ao excluir alocação!";
                resultadoTempData.Type = "warning";

                command.Transaction.Rollback();
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

    public class Alert
    {
        public string Type { get; set; }
        public string Text { get; set; }
    }
}