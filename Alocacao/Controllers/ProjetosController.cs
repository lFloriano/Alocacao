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

        public ActionResult Projeto(int id, string tempType = null, string tempText = null)
        {
            SqlConnection connection = new SqlConnection(connectionString);
            SqlCommand command = new SqlCommand() { Connection = connection };
            ProjetoColaboradores projeto = new ProjetoColaboradores();
            projeto.ColaboradoresDisponiveis = new List<Colaborador>();
            projeto.ColaboradoresAlocados= new List<AlocacaoColaborador>();

            #region queryProjeto
            string queryProjeto = @"
SELECT TOP 1
	p.*,
	g.Nome AS NomeResponsavel,
	c.nome as NomeCliente,
	(SELECT COUNT(*) FROM Alocacao WHERE IdProjeto = @IdProjeto) AS ColaboradoresAlocados
FROM 
	Projetos p
	LEFT JOIN gestores g ON g.id = p.responsavel
	LEFT JOIN Clientes c ON c.id = p.IdCliente
WHERE 
	p.Id = @Idprojeto";
            #endregion

            #region QueryDisponiveis
            string queryDisp = @"
SELECT DISTINCT
    c.*,
	COALESCE(NumColab.numero, 0) AS NumeroProjetos
FROM 
    Colaboradores c
    LEFT JOIN(
        SELECT DISTINCT IdColaborador
		FROM alocacao
		WHERE IdProjeto = @IdProjeto
	)a ON a.IdColaborador = c.Id

	LEFT JOIN (
		SELECT IdColaborador, COUNT(*) AS Numero
		FROM Alocacao
		GROUP BY IdColaborador
	)NumColab ON  NumColab.IdColaborador = c.Id
WHERE
	a.IdColaborador IS NULL";
            #endregion

            #region QueryAlocados
            string queryAloca = @"
SELECT DISTINCT
    p.*,
	COALESCE(NumColab.Numero, 0) AS NumeroProjetos,
	CAST(NumColab.NumeroHoras AS INT) AS NumeroHoras,
	NumColab.DataInicio AS AlocacaoInicio,
	NumColab.DataFim AS AlocacaoFim,
    NumColab.AlocacaoId
FROM 
    Colaboradores p
    LEFT JOIN(
        SELECT DISTINCT IdColaborador
		FROM alocacao
		WHERE IdProjeto = @IdProjeto
	)a ON a.IdColaborador = p.Id

	LEFT JOIN (
		SELECT
            Id AS AlocacaoId,
			IdColaborador,
			COUNT(*) AS Numero,
			NumeroHoras,
			DataInicio,
			DataFim
		FROM 
			Alocacao
        WHERE 
            IdProjeto = @IdProjeto
		GROUP BY
            Id,
			IdColaborador,
			NumeroHoras,
			DataInicio,
			DataFim
	)NumColab ON  NumColab.IdColaborador = p.Id

WHERE
	a.IdColaborador IS NOT NULL";
            #endregion

            try
            {
                connection.Open();

                #region colaborador
                command.CommandText = queryProjeto;
                command.Parameters.AddWithValue("IdProjeto", id);
                SqlDataReader reader = command.ExecuteReader();
                
                while (reader.Read())
                {
                    projeto.Projeto = new Projeto()
                    {
                        Id = int.Parse(reader["Id"].ToString()),
                        Nome = reader["Nome"].ToString(),
                        Descricao = reader["Descricao"].ToString(),
                        NomeResponsavel = reader["NomeResponsavel"].ToString(),
                        DataInicio = DateTime.Parse(reader["DataInicio"].ToString()),
                        DataFim = DateTime.Parse(reader["DataFim"].ToString()),
                        NomeCliente = reader["NomeCliente"].ToString(),
                        StatusProjeto = reader["StatusProjeto"].ToString(),
                        ColaboradoresAlocados = int.Parse(reader["ColaboradoresAlocados"].ToString())
                    };
                }

                command.Parameters.Clear();
                reader.Close();
                #endregion

                #region ColabordoresDisponiveis
                command.CommandText = queryDisp;
                command.Parameters.AddWithValue("IdProjeto", id);
                reader = command.ExecuteReader();
                while (reader.Read())
                {
                    projeto.ColaboradoresDisponiveis.Add(
                        new Colaborador()
                        {
                            Id = int.Parse(reader["Id"].ToString()),
                            Nome = reader["Nome"].ToString(),
                            Cargo = reader["Cargo"].ToString(),
                            Departamento = reader["Departamento"].ToString(),
                            DataContratacao = DateTime.Parse(reader["DataContratacao"].ToString()),
                            NumeroProjetos = int.Parse(reader["NumeroProjetos"].ToString())
                        }
                    );
                }
                command.Parameters.Clear();
                reader.Close();

                #endregion

                #region projetosAlocados
                command.CommandText = queryAloca;
                command.Parameters.AddWithValue("IdProjeto", id);
                reader = command.ExecuteReader();
                while (reader.Read())
                {
                    projeto.ColaboradoresAlocados.Add(
                        new AlocacaoColaborador()
                        {
                            Id = int.Parse(reader["Id"].ToString()),
                            Nome = reader["Nome"].ToString(),
                            Cargo = reader["Cargo"].ToString(),
                            Departamento = reader["Departamento"].ToString(),
                            DataContratacao = DateTime.Parse(reader["DataContratacao"].ToString()),
                            NumeroProjetos = int.Parse(reader["NumeroProjetos"].ToString()),
                            AlocacaoId = int.Parse(reader["AlocacaoId"].ToString()),
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
                projeto.Projeto = new Projeto();
                projeto.ColaboradoresDisponiveis = new List<Colaborador>();
                projeto.ColaboradoresAlocados = new List<AlocacaoColaborador>();
            }
            finally
            {
                connection.Close();
            }

            //creative technical solution for tempData
            if (tempType != null)
            {
                if (tempType.Equals("success"))
                {
                    TempData["AddMessageSuccess"] = "Alteração efetuada!";
                }
                else
                {
                    TempData["AddMessageError"] = "Erro ao efetuar alteração!";
                }
            }

            return View(projeto);
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