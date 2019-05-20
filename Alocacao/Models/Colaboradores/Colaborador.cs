using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Alocacao.Models.Colaboradores
{
    public class Colaborador
    {
        public int Id { get; set; }
        public string Nome { get; set; }
        public string Cargo { get; set; }
        public string Departamento { get; set; }
        public DateTime DataContratacao { get; set; }
        public int NumeroProjetos { get; set; }

        //TODO: SEPARAR EM TABELAS
        public int IdCargo { get; set; }
        public int IdDepartamento { get; set; }

        //Caminho da imagem
        public string ProfilePic { get; set; }

    }
}