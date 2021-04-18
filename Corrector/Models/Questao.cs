using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Corrector.Models
{
    public class Questao
    {
        public int NumeroQuetao { get; set; }
        public string Resposta { get; set; }
        public string Gabarito { get; set; }
        public string Disciplina { get; set; }
        public int Ponto { get; set; }
    }

    public class GabaritoQuestao
    {
        public List<Questao> Questoes { get; set; }
        public int Total { get; set; }
    }
}
