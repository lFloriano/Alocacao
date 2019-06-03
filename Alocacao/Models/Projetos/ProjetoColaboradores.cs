using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Alocacao.Models.Colaboradores;

namespace Alocacao.Models.Projetos
{
    public class ProjetoColaboradores
    {
        public Projeto Projeto { get; set; }
        public List<Colaborador> ColaboradoresDisponiveis { get; set; }
        public List<AlocacaoColaborador> ColaboradoresAlocados { get; set; }
    }
}