using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Alocacao.Models.Projetos;

namespace Alocacao.Models.Colaboradores
{
    public class AlocacaoProjeto: Projeto
    {
        public DateTime? AlocacaoInicio { get; set; }
        public DateTime? AlocacaoFim { get; set; }
        public int AlocacaoNumeroHoras { get; set; }
    }
}