using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Alocacao.Models.Projetos
{
    public class Projeto
    {
        public int Id { get; set; }
        public string Nome { get; set; }
        public string Descricao { get; set; }
        public int IdResponsavel { get; set; }
        public string NomeResponsavel { get; set; }
        public DateTime? DataInicio { get; set; }
        public DateTime? DataFim { get; set; }
        public int IdCliente { get; set; }
        public string NomeCliente { get; set; }
        public string StatusProjeto { get; set; }
        public int ColaboradoresAlocados { get; set; }
    }
}