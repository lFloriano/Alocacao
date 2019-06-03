using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Alocacao.Models.Colaboradores;

namespace Alocacao.Models.Projetos
{
    public class AlocacaoColaborador: Colaborador
    {
        public int AlocacaoId { get; set; }
        public DateTime? AlocacaoInicio { get; set; }
        public DateTime? AlocacaoFim { get; set; }
        public int AlocacaoNumeroHoras { get; set; }
    }
}