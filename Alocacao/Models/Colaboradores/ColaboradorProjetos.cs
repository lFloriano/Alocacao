using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Alocacao.Models.Projetos;

namespace Alocacao.Models.Colaboradores
{
    public class ColaboradorProjetos
    {
        public Colaborador Colaborador { get; set; }
        public List<Projeto> ProjetosDisponiveis { get; set; }
        public List<Projeto> ProjetosAlocados { get; set; }
    }
}